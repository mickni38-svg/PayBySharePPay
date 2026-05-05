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

    public async Task<IEnumerable<Participant>> SearchAsync(string query)
    {
        return await _context.Participants
            .Where(p => p.Name.Contains(query) ||
                        (p.CompanyName != null && p.CompanyName.Contains(query)) ||
                        (p.Email != null && p.Email.Contains(query)))
            .ToListAsync();
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
