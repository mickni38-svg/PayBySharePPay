using Service.PayBySharePay.DTOs;

namespace Service.PayBySharePay.Interfaces;

public interface IMessageService
{
    Task<IEnumerable<MessageDto>> GetMessagesByOrderAsync(int orderId);
    Task<MessageDto> CreateMessageAsync(CreateMessageDto dto);
}
