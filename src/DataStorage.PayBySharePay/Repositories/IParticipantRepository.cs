using DataStorage.PayBySharePay.Entities;

namespace DataStorage.PayBySharePay.Repositories;

public interface IParticipantRepository
{
    Task<IEnumerable<Participant>> SearchAsync(string query, int? excludeFriendsOf = null);
    Task<Participant?> GetByIdAsync(int id);
    Task<Participant?> GetByEmailAsync(string email);
    Task<Participant> AddAsync(Participant participant);
    Task UpdateAsync(Participant participant);
    Task SaveChangesAsync();
}
