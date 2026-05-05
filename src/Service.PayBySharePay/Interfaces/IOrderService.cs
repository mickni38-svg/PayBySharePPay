using Service.PayBySharePay.DTOs;

namespace Service.PayBySharePay.Interfaces;

public interface IOrderService
{
    Task<OrderDto> CreateOrderAsync(CreateOrderDto dto);
    Task<OrderOverviewDto> GetOrderOverviewAsync(int orderId);
}
