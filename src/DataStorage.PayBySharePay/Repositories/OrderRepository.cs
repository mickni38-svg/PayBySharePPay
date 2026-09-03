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
            .Include(o => o.MerchantParticipant)
            .Include(o => o.MerchantOrderDrafts).ThenInclude(d => d.Lines)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<IEnumerable<Order>> GetAllWithDetailsAsync()
    {
        return await _context.Orders
            .Include(o => o.OrderParticipants).ThenInclude(op => op.Participant)
            .Include(o => o.Payments)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetByParticipantIdAsync(int participantId)
    {
        return await _context.Orders
            .Include(o => o.OrderParticipants).ThenInclude(op => op.Participant)
            .Include(o => o.Payments)
            .Include(o => o.MerchantParticipant)
            .Include(o => o.MerchantOrderDrafts)
            .Where(o => o.CreatedByParticipantId == participantId ||
                        o.OrderParticipants.Any(op => op.ParticipantId == participantId))
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetAllAsync()
    {
        return await _context.Orders.ToListAsync();
    }

    public async Task<Order> AddAsync(Order order)
    {
        // UC-18: frys værtens leveringsadresse på selve ordren. En senere
        // profilændring må ikke ændre afleveringsstedet for en eksisterende ordre.
        var creator = await _context.Participants
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == order.CreatedByParticipantId);

        if (creator is not null)
        {
            order.DeliveryAddress = creator.Address;
            order.DeliveryPostalCode = creator.PostalCode;
            order.DeliveryCity = creator.City;
            order.DeliveryCountry = creator.Country;
        }

        _context.Orders.Add(order);
        return order;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
