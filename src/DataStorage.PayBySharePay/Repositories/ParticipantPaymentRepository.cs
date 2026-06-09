using DataStorage.PayBySharePay.Context;
using DataStorage.PayBySharePay.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataStorage.PayBySharePay.Repositories;

public class ParticipantPaymentRepository(PayBySharePayDbContext context) : IParticipantPaymentRepository
{
    public Task<ParticipantPayment?> GetByIdAsync(int id)
        => context.ParticipantPayments.FirstOrDefaultAsync(p => p.Id == id);

    public Task<ParticipantPayment?> GetByProviderPaymentIdAsync(string providerPaymentId)
        => context.ParticipantPayments.FirstOrDefaultAsync(p => p.ProviderPaymentId == providerPaymentId);

    public async Task<IEnumerable<ParticipantPayment>> GetByOrderIdAsync(int orderId)
        => await context.ParticipantPayments.Where(p => p.OrderId == orderId).ToListAsync();

    public async Task<ParticipantPayment> AddAsync(ParticipantPayment participantPayment)
    {
        context.ParticipantPayments.Add(participantPayment);
        await context.SaveChangesAsync();
        return participantPayment;
    }

    public Task SaveChangesAsync() => context.SaveChangesAsync();
}
