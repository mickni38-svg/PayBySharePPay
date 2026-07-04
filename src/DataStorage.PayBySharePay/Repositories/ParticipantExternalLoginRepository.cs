using DataStorage.PayBySharePay.Context;
using DataStorage.PayBySharePay.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataStorage.PayBySharePay.Repositories;

public class ParticipantExternalLoginRepository : IParticipantExternalLoginRepository
{
    private readonly PayBySharePayDbContext _db;

    public ParticipantExternalLoginRepository(PayBySharePayDbContext db)
    {
        _db = db;
    }

    public Task<ParticipantExternalLogin?> GetByProviderAsync(string provider, string providerUserId)
        => _db.ParticipantExternalLogins
              .FirstOrDefaultAsync(e => e.Provider == provider && e.ProviderUserId == providerUserId);

    public async Task AddAsync(ParticipantExternalLogin login)
        => await _db.ParticipantExternalLogins.AddAsync(login);

    public Task SaveChangesAsync()
        => _db.SaveChangesAsync();
}
