using Service.PayBySharePay.DTOs;

namespace Service.PayBySharePay.Interfaces;

public interface IOrderService
{
    Task<OrderDto> CreateOrderAsync(CreateOrderDto dto);
    Task<OrderOverviewDto> GetOrderOverviewAsync(int orderId);
    Task<IEnumerable<OrderSummaryDto>> GetAllOrdersAsync();
    Task<IEnumerable<OrderSummaryDto>> GetOrdersByParticipantAsync(int participantId);
    Task<OrderDto> CompleteOrderAsync(int orderId, int requestingParticipantId);
    Task CheckAndSetReadyToPayAsync(int orderId);
}
