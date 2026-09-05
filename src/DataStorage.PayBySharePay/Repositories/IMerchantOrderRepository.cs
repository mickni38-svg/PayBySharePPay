using DataStorage.PayBySharePay.Entities;

namespace DataStorage.PayBySharePay.Repositories;

public interface IMerchantOrderRepository
{
    Task<MerchantOrder?> GetBySourceOrderIdAsync(int sourceOrderId, CancellationToken cancellationToken = default);
    Task<MerchantOrder?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MerchantOrder>> GetByMerchantAsync(int merchantParticipantId, bool completed, CancellationToken cancellationToken = default);
    Task<MerchantOrder> AddAsync(MerchantOrder merchantOrder, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
