using DataStorage.PayBySharePay.Entities;
using DataStorage.PayBySharePay.Repositories;
using Microsoft.Extensions.Configuration;
using Service.PayBySharePay.DTOs;
using Service.PayBySharePay.Interfaces;

namespace Service.PayBySharePay.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IParticipantRepository _participantRepository;
    private readonly string _apiBaseUrl;

    public OrderService(IOrderRepository orderRepository, IParticipantRepository participantRepository, IConfiguration configuration)
    {
        _orderRepository = orderRepository;
        _participantRepository = participantRepository;
        _apiBaseUrl = configuration["AppSettings:ApiBaseUrl"] ?? "http://localhost:5071";
    }

    public async Task<OrderDto> CreateOrderAsync(CreateOrderDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title) && string.IsNullOrWhiteSpace(dto.Category))
            throw new ArgumentException("En ordre skal have en titel eller kategori.");

        var creator = await _participantRepository.GetByIdAsync(dto.CreatedByParticipantId)
            ?? throw new KeyNotFoundException($"Bruger med id {dto.CreatedByParticipantId} findes ikke.");

        Participant? merchant = null;
        if (dto.MerchantParticipantId.HasValue)
        {
            merchant = await _participantRepository.GetByIdAsync(dto.MerchantParticipantId.Value)
                ?? throw new KeyNotFoundException($"Merchant med id {dto.MerchantParticipantId} findes ikke.");
        }

        var joinToken = Guid.NewGuid().ToString("N");

        var order = new Order
        {
            CreatedByParticipantId = dto.CreatedByParticipantId,
            Title = dto.Title.Trim(),
            Category = dto.Category,
            Message = dto.Message,
            Status = "Collecting",
            MerchantParticipantId = dto.MerchantParticipantId,
            JoinToken = joinToken
        };

        // Opretter selv tilføjes automatisk som deltager
        order.OrderParticipants.Add(new OrderParticipant
        {
            ParticipantId = dto.CreatedByParticipantId,
            Status = "Accepted",
            ParticipantToken = Guid.NewGuid().ToString("N")
        });

        foreach (var participantId in dto.ParticipantIds.Where(id => id != dto.CreatedByParticipantId))
        {
            _ = await _participantRepository.GetByIdAsync(participantId)
                ?? throw new KeyNotFoundException($"Deltager med id {participantId} findes ikke.");

            order.OrderParticipants.Add(new OrderParticipant
            {
                ParticipantId = participantId,
                Status = "Invited",
                ParticipantToken = Guid.NewGuid().ToString("N")
            });
        }

        await _orderRepository.AddAsync(order);
        await _orderRepository.SaveChangesAsync();

        // Send notifikation (besked) til alle deltagere hvis merchant er valgt
        if (merchant?.GroupOrderUrl != null)
        {
            foreach (var op in order.OrderParticipants.ToList())
            {
                var participantLink = $"{merchant.GroupOrderUrl}?orderId={order.Id}&merchantId={merchant.Id}&participantToken={op.ParticipantToken}&api={_apiBaseUrl}";
                order.Messages.Add(new Message
                {
                    OrderId = order.Id,
                    ParticipantId = op.ParticipantId,
                    Content = $"Bestil din mad hos {merchant.CompanyName ?? merchant.Name}: {participantLink}"
                });
            }

            await _orderRepository.SaveChangesAsync();
        }

        return MapToDto(order);
    }

    public async Task<OrderOverviewDto> GetOrderOverviewAsync(int orderId)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId)
            ?? throw new KeyNotFoundException($"Ordre med id {orderId} findes ikke.");

        // Alle drafts (én per deltager der har bestilt)
        var allDrafts = order.MerchantOrderDrafts.ToList();
        var draft = allDrafts.FirstOrDefault(); // bruges kun til totalAmount/status

        // Betalingsstatus pr. deltager
        var paidParticipantIds = order.Payments
            .Where(p => p.Status == "Completed")
            .Select(p => p.ParticipantId)
            .ToHashSet();

        // Byg ordrelinjer pr. deltager
        var participantOrderLines = new List<ParticipantOrderLinesDto>();
        if (allDrafts.Any())
        {
            var nonMerchantParticipants = order.OrderParticipants
                .Where(op => op.Participant.Type != DataStorage.PayBySharePay.Entities.ParticipantType.Merchant)
                .ToList();

            // Saml alle linjer fra alle drafts grupperet på ParticipantId
            var linesByParticipant = allDrafts
                .SelectMany(d => d.Lines)
                .Where(l => l.ParticipantId.HasValue)
                .GroupBy(l => l.ParticipantId!.Value)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Linjer uden ParticipantId (fra drafts uden tildeling)
            var unassignedLines = allDrafts
                .SelectMany(d => d.Lines)
                .Where(l => !l.ParticipantId.HasValue).ToList();

            foreach (var op in nonMerchantParticipants)
            {
                var lines = linesByParticipant.TryGetValue(op.ParticipantId, out var pl) ? pl : new();
                var hasPaid = paidParticipantIds.Contains(op.ParticipantId);
                if (lines.Any() || !unassignedLines.Any())
                {
                    participantOrderLines.Add(new ParticipantOrderLinesDto
                    {
                        ParticipantId = op.ParticipantId,
                        ParticipantName = op.Participant.Name,
                        HasPaid = hasPaid,
                        Lines = lines.Select(l => new MerchantOrderLineDto
                        {
                            ParticipantId = l.ParticipantId,
                            LineId = l.LineId,
                            Name = l.Name,
                            Quantity = l.Quantity,
                            UnitPrice = l.UnitPrice,
                            LineTotal = l.LineTotal
                        }).ToList()
                    });
                }
            }

            // Hvis ingen linjer er tildelt deltager, vis alle under én samlet gruppe
            if (!linesByParticipant.Any() && unassignedLines.Any())
            {
                participantOrderLines.Add(new ParticipantOrderLinesDto
                {
                    ParticipantId = 0,
                    ParticipantName = "Bestilling",
                    HasPaid = false,
                    Lines = unassignedLines.Select(l => new MerchantOrderLineDto
                    {
                        LineId = l.LineId,
                        Name = l.Name,
                        Quantity = l.Quantity,
                        UnitPrice = l.UnitPrice,
                        LineTotal = l.LineTotal
                    }).ToList()
                });
            }
        }

        return new OrderOverviewDto
        {
            OrderId = order.Id,
            CreatedByParticipantId = order.CreatedByParticipantId,
            Title = order.Title,
            Category = order.Category,
            Message = order.Message,
            Status = order.Status,
            CreatedAt = order.CreatedAt,
            MerchantName = order.MerchantParticipant?.CompanyName ?? order.MerchantParticipant?.Name,
            MerchantAddress = order.MerchantParticipant?.CompanyAddress,
            TotalAmount = draft?.TotalAmount ?? 0m,
            Participants = order.OrderParticipants.Select(op => new OrderParticipantDto
            {
                ParticipantId = op.ParticipantId,
                Name = op.Participant.Name,
                Type = op.Participant.Type.ToString(),
                Status = op.Status
            }).ToList(),
            Payments = order.Payments.Select(p => new PaymentDto
            {
                Id = p.Id,
                ParticipantId = p.ParticipantId,
                ParticipantName = p.Participant.Name,
                Amount = p.Amount,
                Status = p.Status,
                CreatedAt = p.CreatedAt
            }).ToList(),
            Messages = order.Messages.Select(m => new MessageDto
            {
                Id = m.Id,
                ParticipantId = m.ParticipantId,
                ParticipantName = m.Participant.Name,
                Content = m.Content,
                CreatedAt = m.CreatedAt
            }).ToList(),
            ParticipantOrderLines = participantOrderLines
        };
    }

    private static OrderDto MapToDto(Order o) => new()
    {
        Id = o.Id,
        CreatedByParticipantId = o.CreatedByParticipantId,
        Title = o.Title,
        Category = o.Category,
        Message = o.Message,
        Status = o.Status,
        CreatedAt = o.CreatedAt
    };

    public async Task<IEnumerable<OrderSummaryDto>> GetAllOrdersAsync()
    {
        var orders = await _orderRepository.GetAllWithDetailsAsync();
        return orders.Select(MapToSummary);
    }

    public async Task<IEnumerable<OrderSummaryDto>> GetOrdersByParticipantAsync(int participantId)
    {
        var orders = await _orderRepository.GetByParticipantIdAsync(participantId);
        return orders.Select(MapToSummary);
    }

    private static OrderSummaryDto MapToSummary(Order o) => new()
    {
        Id = o.Id,
        Title = o.Title,
        Category = o.Category,
        Status = o.Status,
        CreatedAt = o.CreatedAt,
        CreatedByParticipantId = o.CreatedByParticipantId,
        TotalAmount = o.MerchantOrderDrafts.FirstOrDefault()?.TotalAmount ?? 0m,
        MerchantName = o.MerchantParticipant?.CompanyName ?? o.MerchantParticipant?.Name,
        Participants = o.OrderParticipants.Select(op => new OrderParticipantDto
        {
            ParticipantId = op.ParticipantId,
            Name = op.Participant.Name,
            Type = op.Participant.Type.ToString(),
            Status = op.Status
        }).ToList()
    };

    public async Task<OrderDto> CompleteOrderAsync(int orderId, int requestingParticipantId)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId)
            ?? throw new KeyNotFoundException($"Ordre med id {orderId} findes ikke.");

        if (order.CreatedByParticipantId != requestingParticipantId)
            throw new UnauthorizedAccessException("Kun værten kan gennemføre betalingen.");

        if (order.Status != "ReadyToPay")
            throw new InvalidOperationException($"Ordren er ikke klar til betaling. Status: {order.Status}");

        order.Status = "Completed";
        await _orderRepository.SaveChangesAsync();

        return MapToDto(order);
    }

    public async Task CheckAndSetReadyToPayAsync(int orderId)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId)
            ?? throw new KeyNotFoundException($"Ordre med id {orderId} findes ikke.");

        if (order.Status != "Collecting")
            return;

        var nonMerchantParticipants = order.OrderParticipants
            .Where(op => op.Participant.Type != DataStorage.PayBySharePay.Entities.ParticipantType.Merchant)
            .ToList();

        if (nonMerchantParticipants.Count == 0)
            return;

        var allSubmitted = nonMerchantParticipants.All(op => op.Status == "OrderSubmitted");
        if (allSubmitted)
        {
            order.Status = "ReadyToPay";
            await _orderRepository.SaveChangesAsync();
        }
    }
}
