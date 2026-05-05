using DataStorage.PayBySharePay.Context;
using DataStorage.PayBySharePay.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataStorage.PayBySharePay.Repositories;

public class MessageRepository : IMessageRepository
{
    private readonly PayBySharePayDbContext _context;

    public MessageRepository(PayBySharePayDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Message>> GetByOrderIdAsync(int orderId)
    {
        return await _context.Messages
            .Where(m => m.OrderId == orderId)
            .Include(m => m.Participant)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task<Message> AddAsync(Message message)
    {
        _context.Messages.Add(message);
        return message;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
