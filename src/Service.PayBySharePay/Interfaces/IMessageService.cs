using Service.PayBySharePay.DTOs;

namespace Service.PayBySharePay.Interfaces;

public interface IMessageService
{
    Task<IEnumerable<MessageDto>> GetMessagesByOrderAsync(int orderId);
    Task<MessageDto> CreateMessageAsync(CreateMessageDto dto);
    Task<IEnumerable<MessageDto>> GetByParticipantAsync(int participantId);
    Task<int> GetUnreadCountAsync(int participantId);
    Task MarkAllReadAsync(int participantId);
}
