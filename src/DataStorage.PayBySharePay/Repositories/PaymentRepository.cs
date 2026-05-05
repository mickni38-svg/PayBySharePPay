using DataStorage.PayBySharePay.Context;
using DataStorage.PayBySharePay.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataStorage.PayBySharePay.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly PayBySharePayDbContext _context;

    public PaymentRepository(PayBySharePayDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Payment>> GetByOrderIdAsync(int orderId)
    {
        return await _context.Payments
            .Where(p => p.OrderId == orderId)
            .Include(p => p.Participant)
            .ToListAsync();
    }

    public async Task<Payment> AddAsync(Payment payment)
    {
        _context.Payments.Add(payment);
        return payment;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
