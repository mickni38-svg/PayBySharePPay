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
    private readonly IParticipantPaymentRepository _paymentRepository;
    private readonly string _merchantDemoUrl;
    private readonly string _frontendUrl;

    public OrderService(
        IOrderRepository orderRepository,
        IParticipantRepository participantRepository,
        IParticipantPaymentRepository paymentRepository,
        IConfiguration configuration)
    {
        _orderRepository = orderRepository;
        _participantRepository = participantRepository;
        _paymentRepository = paymentRepository;
        _merchantDemoUrl = configuration["AppSettings:MerchantDemoUrl"] ?? "http://localhost:8081";
        _frontendUrl = configuration["AppSettings:FrontendUrl"] ?? "http://localhost:4200";
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

        // Send notifikation med bestillingslink til ALLE deltagere inkl. host
        if (merchant != null)
        {
            // Brug merchant's GroupOrderUrl – eller konstruér et MerchantDemo-link fra konfiguration
            var baseUrl = merchant.GroupOrderUrl ?? _merchantDemoUrl;
            var ordreNavn = string.IsNullOrWhiteSpace(order.Title) ? order.Category ?? "gruppebetaling" : order.Title;

            foreach (var op in order.OrderParticipants.ToList())
            {
                var participantLink = $"{baseUrl}?orderId={order.Id}&merchantId={merchant.Id}&participantToken={op.ParticipantToken}";
                var isHost = op.ParticipantId == dto.CreatedByParticipantId;
                var msgText = isHost
                    ? $"🍽️ Du har oprettet '{ordreNavn}' hos {merchant.CompanyName ?? merchant.Name}. Bestil din mad her: {participantLink}"
                    : $"🍽️ {creator.Name} har inviteret dig til '{ordreNavn}' hos {merchant.CompanyName ?? merchant.Name}. Bestil din mad her: {participantLink}";

                order.Messages.Add(new Message
                {
                    OrderId = order.Id,
                    ParticipantId = op.ParticipantId,
                    Content = msgText
                });
            }
            await _orderRepository.SaveChangesAsync();
        }
        else
        {
            // Ingen merchant valgt – send generel invitation til inviterede deltagere
            var orderTitle = string.IsNullOrWhiteSpace(dto.Title) ? dto.Category : dto.Title;
            var invitedParticipants = order.OrderParticipants
                .Where(op => op.ParticipantId != dto.CreatedByParticipantId)
                .ToList();
            foreach (var op in invitedParticipants)
            {
                order.Messages.Add(new Message
                {
                    OrderId = order.Id,
                    ParticipantId = op.ParticipantId,
                    Content = $"🍽️ {creator.Name} har inviteret dig til gruppebetaling: \"{orderTitle}\". Åbn appen for at se detaljer."
                });
            }
            if (invitedParticipants.Count > 0)
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

        // Betalingsstatus pr. deltager – tjek både OrderParticipant.Status og Payments tabel
        var paidViaPayments = order.Payments
            .Where(p => p.Status == "Completed")
            .Select(p => p.ParticipantId)
            .ToHashSet();
        var paidParticipantIds = order.OrderParticipants
            .Where(op => op.Status == "Paid")
            .Select(op => op.ParticipantId)
            .ToHashSet();
        // Merge begge kilder
        paidParticipantIds.UnionWith(paidViaPayments);

        // Synkroniser OrderParticipant.Status hvis betaling er registreret men status ikke opdateret
        foreach (var op in order.OrderParticipants.Where(op => paidViaPayments.Contains(op.ParticipantId) && op.Status != "Paid"))
        {
            op.Status = "Paid";
        }
        if (order.OrderParticipants.Any(op => paidViaPayments.Contains(op.ParticipantId) && op.Status == "Paid"))
        {
            await _orderRepository.SaveChangesAsync();
        }

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

        // ParticipantPayments — betalingsstatus pr. deltager
        var participantPayments = (await _paymentRepository.GetByOrderIdAsync(orderId)).ToList();

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
            MerchantLogoUrl = GetMerchantLogoUrl(order.MerchantParticipant),
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
            ParticipantOrderLines = participantOrderLines,
            ParticipantPayments = participantPayments.Select(pp =>
            {
                var opName = order.OrderParticipants
                    .FirstOrDefault(op => op.ParticipantId == pp.ParticipantId)?.Participant.Name ?? "Ukendt";
                return new ParticipantPaymentSummaryDto
                {
                    ParticipantPaymentId = pp.Id,
                    ParticipantId = pp.ParticipantId,
                    ParticipantName = opName,
                    AmountMinorUnits = pp.AmountMinorUnits,
                    Currency = pp.Currency,
                    Status = pp.Status.ToString(),
                    ProviderPaymentId = pp.ProviderPaymentId,
                    ReservedAtUtc = pp.ReservedAtUtc,
                    CapturedAtUtc = pp.CapturedAtUtc,
                    LastErrorCode = pp.LastErrorCode,
                    LastErrorMessage = pp.LastErrorMessage
                };
            }).ToList()
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
        MerchantLogoUrl = GetMerchantLogoUrl(o.MerchantParticipant),
        Participants = o.OrderParticipants.Select(op => new OrderParticipantDto
        {
            ParticipantId = op.ParticipantId,
            Name = op.Participant.Name,
            Type = op.Participant.Type.ToString(),
            Status = op.Status
        }).ToList()
    };

    private static string? GetMerchantLogoUrl(Participant? merchant)
        => merchant?.LogoImageData is not null
            ? $"/api/participants/{merchant.Id}/logo"
            : null;

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

        var nonMerchantParticipants = order.OrderParticipants
            .Where(op => op.Participant.Type != DataStorage.PayBySharePay.Entities.ParticipantType.Merchant)
            .ToList();

        if (nonMerchantParticipants.Count == 0)
            return;

        var allReady = nonMerchantParticipants.All(op => op.Status == "OrderSubmitted");

        if (allReady)
        {
            order.Status = "ReadyToPay";

            // Send notifikation til host med link til Overblik
            var ordreTitle = string.IsNullOrWhiteSpace(order.Title) ? order.Category ?? "ordre" : order.Title;
            var overblikLink = $"{_frontendUrl}/orders";
            order.Messages.Add(new Message
            {
                OrderId = order.Id,
                ParticipantId = order.CreatedByParticipantId,
                Content = $"✅ Alle deltagere har bestilt til '{ordreTitle}'. Du kan nu gennemføre betalingen: {overblikLink}"
            });

            await _orderRepository.SaveChangesAsync();
        }
    }

    public async Task CheckAndSetReadyToPayByReservedAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId)
            ?? throw new KeyNotFoundException($"Ordre med id {orderId} findes ikke.");

        // Kun relevant hvis ordren stadig er i Collecting (eller ReservationStarted-tilstand)
        if (order.Status != "Collecting")
            return;

        var nonMerchantParticipants = order.OrderParticipants
            .Where(op => op.Participant.Type != DataStorage.PayBySharePay.Entities.ParticipantType.Merchant)
            .ToList();

        if (nonMerchantParticipants.Count == 0)
            return;

        // Hent alle betalinger for ordren
        var payments = (await _paymentRepository.GetByOrderIdAsync(orderId)).ToList();

        var allReserved = nonMerchantParticipants.All(op =>
            payments.Any(pp =>
                pp.ParticipantId == op.ParticipantId &&
                pp.Status == ParticipantPaymentStatus.Reserved));

        if (!allReserved)
            return;

        order.Status = "ReadyToPay";

        var ordreTitle = string.IsNullOrWhiteSpace(order.Title) ? order.Category ?? "ordre" : order.Title;
        var overblikLink = $"{_frontendUrl}/orders";
        order.Messages.Add(new Message
        {
            OrderId = order.Id,
            ParticipantId = order.CreatedByParticipantId,
            Content = $"✅ Alle har bestilt og reserveret betaling til '{ordreTitle}'. Du kan nu godkende den samlede ordre: {overblikLink}"
        });

        await _orderRepository.SaveChangesAsync();
    }
}
