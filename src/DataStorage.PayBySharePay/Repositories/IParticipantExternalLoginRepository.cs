using DataStorage.PayBySharePay.Entities;

namespace DataStorage.PayBySharePay.Repositories;

public interface IParticipantExternalLoginRepository
{
    Task<ParticipantExternalLogin?> GetByProviderAsync(string provider, string providerUserId);
    Task AddAsync(ParticipantExternalLogin login);
    Task SaveChangesAsync();
}
