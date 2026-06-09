using DataStorage.PayBySharePay.Context;
using DataStorage.PayBySharePay.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataStorage.PayBySharePay.Repositories;

public class PaymentEventLogRepository(PayBySharePayDbContext context) : IPaymentEventLogRepository
{
    public async Task AddAsync(PaymentEventLog eventLog)
    {
        context.PaymentEventLogs.Add(eventLog);
        await context.SaveChangesAsync();
    }

    public async Task<IEnumerable<PaymentEventLog>> GetByParticipantPaymentIdAsync(int participantPaymentId)
        => await context.PaymentEventLogs
            .Where(e => e.ParticipantPaymentId == participantPaymentId)
            .OrderBy(e => e.CreatedAtUtc)
            .ToListAsync();

    public async Task<IEnumerable<PaymentEventLog>> GetByOrderIdAsync(int orderId)
        => await context.PaymentEventLogs
            .Where(e => e.OrderId == orderId)
            .OrderBy(e => e.CreatedAtUtc)
            .ToListAsync();
}
