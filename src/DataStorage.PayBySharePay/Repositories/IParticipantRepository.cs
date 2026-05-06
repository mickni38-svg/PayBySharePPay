using DataStorage.PayBySharePay.Entities;

namespace DataStorage.PayBySharePay.Repositories;

public interface IParticipantRepository
{
    Task<IEnumerable<Participant>> SearchAsync(string query, int? excludeFriendsOf = null);
    Task<Participant?> GetByIdAsync(int id);
    Task<Participant> AddAsync(Participant participant);
    Task SaveChangesAsync();
}
