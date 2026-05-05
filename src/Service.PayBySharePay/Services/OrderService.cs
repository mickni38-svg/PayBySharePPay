using DataStorage.PayBySharePay.Entities;
using DataStorage.PayBySharePay.Repositories;
using Service.PayBySharePay.DTOs;
using Service.PayBySharePay.Interfaces;

namespace Service.PayBySharePay.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IParticipantRepository _participantRepository;

    public OrderService(IOrderRepository orderRepository, IParticipantRepository participantRepository)
    {
        _orderRepository = orderRepository;
        _participantRepository = participantRepository;
    }

    public async Task<OrderDto> CreateOrderAsync(CreateOrderDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title) && string.IsNullOrWhiteSpace(dto.Category))
            throw new ArgumentException("En ordre skal have en titel eller kategori.");

        var order = new Order
        {
            Title = dto.Title.Trim(),
            Category = dto.Category,
            Message = dto.Message,
            Status = "Open"
        };

        foreach (var participantId in dto.ParticipantIds)
        {
            var participant = await _participantRepository.GetByIdAsync(participantId)
                ?? throw new KeyNotFoundException($"Deltager med id {participantId} findes ikke.");

            order.OrderParticipants.Add(new OrderParticipant
            {
                ParticipantId = participantId,
                Status = "Pending"
            });
        }

        await _orderRepository.AddAsync(order);
        await _orderRepository.SaveChangesAsync();

        return MapToDto(order);
    }

    public async Task<OrderOverviewDto> GetOrderOverviewAsync(int orderId)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId)
            ?? throw new KeyNotFoundException($"Ordre med id {orderId} findes ikke.");

        return new OrderOverviewDto
        {
            OrderId = order.Id,
            Title = order.Title,
            Category = order.Category,
            Message = order.Message,
            Status = order.Status,
            CreatedAt = order.CreatedAt,
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
            }).ToList()
        };
    }

    private static OrderDto MapToDto(Order o) => new()
    {
        Id = o.Id,
        Title = o.Title,
        Category = o.Category,
        Message = o.Message,
        Status = o.Status,
        CreatedAt = o.CreatedAt
    };
}
