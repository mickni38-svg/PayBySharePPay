using DataStorage.PayBySharePay.Context;
using DataStorage.PayBySharePay.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataStorage.PayBySharePay.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly PayBySharePayDbContext _context;

    public OrderRepository(PayBySharePayDbContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetByIdWithDetailsAsync(int id)
    {
        return await _context.Orders
            .Include(o => o.OrderParticipants).ThenInclude(op => op.Participant)
            .Include(o => o.Payments).ThenInclude(p => p.Participant)
            .Include(o => o.Messages).ThenInclude(m => m.Participant)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<IEnumerable<Order>> GetAllAsync()
    {
        return await _context.Orders.ToListAsync();
    }

    public async Task<Order> AddAsync(Order order)
    {
        _context.Orders.Add(order);
        return order;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
