using DataStorage.PayBySharePay.Entities;

namespace DataStorage.PayBySharePay.Repositories;

public interface IPaymentEventLogRepository
{
    Task AddAsync(PaymentEventLog eventLog);
    Task<IEnumerable<PaymentEventLog>> GetByParticipantPaymentIdAsync(int participantPaymentId);
    Task<IEnumerable<PaymentEventLog>> GetByOrderIdAsync(int orderId);
}
