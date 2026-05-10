using DataStorage.PayBySharePay.Context;
using DataStorage.PayBySharePay.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataStorage.PayBySharePay.Repositories;

public class ParticipantRepository : IParticipantRepository
{
    private readonly PayBySharePayDbContext _context;

    public ParticipantRepository(PayBySharePayDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Participant>> SearchAsync(string query, int? excludeFriendsOf = null)
    {
        var q = _context.Participants.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            q = q.Where(p => p.Name.Contains(query) ||
                              (p.CompanyName != null && p.CompanyName.Contains(query)) ||
                              (p.Email != null && p.Email.Contains(query)));
        }

        if (excludeFriendsOf.HasValue)
        {
            var friendIds = _context.FriendRelations
                .Where(f => f.InitiatorId == excludeFriendsOf.Value || f.ReceiverId == excludeFriendsOf.Value)
                .Select(f => f.InitiatorId == excludeFriendsOf.Value ? f.ReceiverId : f.InitiatorId);

            // Ekskludér kun Person-typer der allerede er venner – Merchants vises altid
            q = q.Where(p => p.Type == ParticipantType.Merchant ||
                              (p.Id != excludeFriendsOf.Value && !friendIds.Contains(p.Id)));
        }

        return await q.ToListAsync();
    }

    public async Task<Participant?> GetByIdAsync(int id)
    {
        return await _context.Participants.FindAsync(id);
    }

    public async Task<Participant> AddAsync(Participant participant)
    {
        _context.Participants.Add(participant);
        return participant;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
