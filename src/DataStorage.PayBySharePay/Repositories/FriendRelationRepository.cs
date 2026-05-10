using DataStorage.PayBySharePay.Context;
using DataStorage.PayBySharePay.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataStorage.PayBySharePay.Repositories;

public class FriendRelationRepository : IFriendRelationRepository
{
    private readonly PayBySharePayDbContext _context;

    public FriendRelationRepository(PayBySharePayDbContext context)
    {
        _context = context;
    }

    public async Task<bool> RelationExistsAsync(int initiatorId, int receiverId)
    {
        return await _context.FriendRelations
            .AnyAsync(f => (f.InitiatorId == initiatorId && f.ReceiverId == receiverId) ||
                           (f.InitiatorId == receiverId && f.ReceiverId == initiatorId));
    }

    public async Task<IEnumerable<Participant>> GetFriendsOfAsync(int participantId)
    {
        return await _context.FriendRelations
            .Where(f => f.InitiatorId == participantId || f.ReceiverId == participantId)
            .Select(f => f.InitiatorId == participantId ? f.Receiver : f.Initiator)
            .ToListAsync();
    }

    public async Task<FriendRelation> AddAsync(FriendRelation relation)
    {
        _context.FriendRelations.Add(relation);
        return relation;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
