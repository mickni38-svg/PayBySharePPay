using Service.PayBySharePay.DTOs;

namespace Service.PayBySharePay.Interfaces;

public interface ISquareInspiredMerchantOrderAdapter
{
    SquareInspiredMerchantOrderRequest Map(PayNSyncFinalGroupOrderDto order);
}
