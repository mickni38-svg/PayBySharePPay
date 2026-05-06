using DataStorage.PayBySharePay.Context;
using DataStorage.PayBySharePay.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataStorage.PayBySharePay.Repositories;

public class MerchantOrderDraftRepository : IMerchantOrderDraftRepository
{
    private readonly PayBySharePayDbContext _context;

    public MerchantOrderDraftRepository(PayBySharePayDbContext context)
    {
        _context = context;
    }

    public async Task<MerchantOrderDraft?> GetByOrderIdAsync(int orderId)
    {
        return await _context.MerchantOrderDrafts
            .Include(d => d.Lines)
            .FirstOrDefaultAsync(d => d.OrderId == orderId);
    }

    public async Task<MerchantOrderDraft> AddAsync(MerchantOrderDraft draft)
    {
        _context.MerchantOrderDrafts.Add(draft);
        return draft;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
