using DataStorage.PayBySharePay.Entities;

namespace DataStorage.PayBySharePay.Repositories;

public interface IFriendRelationRepository
{
    Task<bool> RelationExistsAsync(int initiatorId, int receiverId);
    Task<IEnumerable<Participant>> GetFriendsOfAsync(int participantId);
    Task<FriendRelation> AddAsync(FriendRelation relation);
    Task SaveChangesAsync();
}
