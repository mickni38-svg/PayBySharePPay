using DataStorage.PayBySharePay.Entities;

namespace DataStorage.PayBySharePay.Repositories;

public interface IParticipantPaymentRepository
{
    Task<ParticipantPayment?> GetByIdAsync(int id);
    Task<ParticipantPayment?> GetByProviderPaymentIdAsync(string providerPaymentId);
    Task<IEnumerable<ParticipantPayment>> GetByOrderIdAsync(int orderId);
    Task<ParticipantPayment> AddAsync(ParticipantPayment participantPayment);
    Task SaveChangesAsync();
}
