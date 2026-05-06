using DataStorage.PayBySharePay.Entities;

namespace DataStorage.PayBySharePay.Repositories;

public interface IMerchantOrderDraftRepository
{
    Task<MerchantOrderDraft?> GetByOrderIdAsync(int orderId);
    Task<MerchantOrderDraft> AddAsync(MerchantOrderDraft draft);
    Task SaveChangesAsync();
}
