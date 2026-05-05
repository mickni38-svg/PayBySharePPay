using DataStorage.PayBySharePay.Entities;

namespace DataStorage.PayBySharePay.Repositories;

public interface IFriendRelationRepository
{
    Task<bool> RelationExistsAsync(int initiatorId, int receiverId);
    Task<FriendRelation> AddAsync(FriendRelation relation);
    Task SaveChangesAsync();
}
