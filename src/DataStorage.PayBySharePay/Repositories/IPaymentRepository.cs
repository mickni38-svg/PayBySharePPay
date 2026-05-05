using DataStorage.PayBySharePay.Entities;

namespace DataStorage.PayBySharePay.Repositories;

public interface IPaymentRepository
{
    Task<IEnumerable<Payment>> GetByOrderIdAsync(int orderId);
    Task<Payment> AddAsync(Payment payment);
    Task SaveChangesAsync();
}
