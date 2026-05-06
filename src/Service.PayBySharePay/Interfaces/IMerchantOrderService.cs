using Service.PayBySharePay.DTOs;

namespace Service.PayBySharePay.Interfaces;

public interface IMerchantOrderService
{
    Task<MerchantOrderDraftDto> InitOrderAsync(InitMerchantOrderDto dto);
    Task<MerchantOrderDraftDto?> GetByOrderIdAsync(int orderId);
}
