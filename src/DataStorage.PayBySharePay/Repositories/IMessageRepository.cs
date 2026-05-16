using DataStorage.PayBySharePay.Entities;

namespace DataStorage.PayBySharePay.Repositories;

public interface IMessageRepository
{
    Task<IEnumerable<Message>> GetByOrderIdAsync(int orderId);
    Task<IEnumerable<Message>> GetByParticipantIdAsync(int participantId);
    Task<Message> AddAsync(Message message);
    Task SaveChangesAsync();
}
