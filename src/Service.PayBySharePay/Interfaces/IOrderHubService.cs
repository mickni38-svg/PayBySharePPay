using Service.PayBySharePay.DTOs;

namespace Service.PayBySharePay.Interfaces;

public interface IOrderHubService
{
    Task<OrderHubSettingsDto> GetSettingsAsync(int participantId, CancellationToken cancellationToken = default);
    Task<OrderHubSettingsDto> SetEnabledAsync(int participantId, bool enabled, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderHubOrderDto>> GetActiveOrdersAsync(int participantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderHubOrderDto>> GetHistoryAsync(int participantId, CancellationToken cancellationToken = default);
    Task<OrderHubOrderDto> UpdateStatusAsync(int participantId, int merchantOrderId, string newStatus, CancellationToken cancellationToken = default);
}
