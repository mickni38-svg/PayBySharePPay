using DataStorage.PayBySharePay.Entities;
using DataStorage.PayBySharePay.Repositories;
using Service.PayBySharePay.DTOs;
using Service.PayBySharePay.Interfaces;

namespace Service.PayBySharePay.Services;

public class MessageService : IMessageService
{
    private readonly IMessageRepository _messageRepository;
    private readonly IParticipantRepository _participantRepository;
    private readonly IOrderRepository _orderRepository;

    public MessageService(
        IMessageRepository messageRepository,
        IParticipantRepository participantRepository,
        IOrderRepository orderRepository)
    {
        _messageRepository = messageRepository;
        _participantRepository = participantRepository;
        _orderRepository = orderRepository;
    }

    public async Task<IEnumerable<MessageDto>> GetMessagesByOrderAsync(int orderId)
    {
        var messages = await _messageRepository.GetByOrderIdAsync(orderId);
        return messages.Select(m => new MessageDto
        {
            Id = m.Id,
            ParticipantId = m.ParticipantId,
            ParticipantName = m.Participant.Name,
            Content = m.Content,
            CreatedAt = m.CreatedAt
        });
    }

    public async Task<MessageDto> CreateMessageAsync(CreateMessageDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Content))
            throw new ArgumentException("En besked må ikke være tom.");

        _ = await _orderRepository.GetByIdWithDetailsAsync(dto.OrderId)
            ?? throw new KeyNotFoundException($"Ordre med id {dto.OrderId} findes ikke.");

        var participant = await _participantRepository.GetByIdAsync(dto.ParticipantId)
            ?? throw new KeyNotFoundException($"Deltager med id {dto.ParticipantId} findes ikke.");

        var message = new Message
        {
            OrderId = dto.OrderId,
            ParticipantId = dto.ParticipantId,
            Content = dto.Content.Trim()
        };

        await _messageRepository.AddAsync(message);
        await _messageRepository.SaveChangesAsync();

        return new MessageDto
        {
            Id = message.Id,
            ParticipantId = message.ParticipantId,
            ParticipantName = participant.Name,
            Content = message.Content,
            CreatedAt = message.CreatedAt
        };
    }
}
