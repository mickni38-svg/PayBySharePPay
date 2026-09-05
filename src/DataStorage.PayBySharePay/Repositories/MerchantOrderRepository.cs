using DataStorage.PayBySharePay.Context;
using DataStorage.PayBySharePay.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataStorage.PayBySharePay.Repositories;

public sealed class MerchantOrderRepository(PayBySharePayDbContext context) : IMerchantOrderRepository
{
    public Task<MerchantOrder?> GetBySourceOrderIdAsync(
        int sourceOrderId,
        CancellationToken cancellationToken = default)
        => context.MerchantOrders
            .Include(order => order.Items)
            .SingleOrDefaultAsync(order => order.SourceOrderId == sourceOrderId, cancellationToken);

    public Task<MerchantOrder> AddAsync(
        MerchantOrder merchantOrder,
        CancellationToken cancellationToken = default)
    {
        context.MerchantOrders.Add(merchantOrder);
        return Task.FromResult(merchantOrder);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => context.SaveChangesAsync(cancellationToken);
}
