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

    public Task<MerchantOrder?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
        => context.MerchantOrders
            .Include(order => order.Items)
            .SingleOrDefaultAsync(order => order.Id == id, cancellationToken);

    public async Task<IReadOnlyList<MerchantOrder>> GetByMerchantAsync(
        int merchantParticipantId,
        bool completed,
        CancellationToken cancellationToken = default)
        => await context.MerchantOrders
            .Include(order => order.Items)
            .Where(order => order.MerchantParticipantId == merchantParticipantId)
            .Where(order => completed
                ? order.OrderHubStatus == "Completed"
                : order.OrderHubStatus != "Completed")
            .OrderByDescending(order => order.CreatedAtUtc)
            .ToListAsync(cancellationToken);

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
