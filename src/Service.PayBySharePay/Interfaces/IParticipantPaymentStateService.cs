using DataStorage.PayBySharePay.Entities;

namespace Service.PayBySharePay.Interfaces;

public interface IParticipantPaymentStateService
{
    Task SetReservationStartedAsync(int participantPaymentId, string providerPaymentId, string? correlationId = null, CancellationToken cancellationToken = default);
    Task SetReservedAsync(int participantPaymentId, string? correlationId = null, CancellationToken cancellationToken = default);
    Task SetReservationFailedAsync(int participantPaymentId, string? errorCode, string? errorMessage, string? correlationId = null, CancellationToken cancellationToken = default);
    Task SetCapturePendingAsync(int participantPaymentId, string? correlationId = null, CancellationToken cancellationToken = default);
    Task SetCapturedAsync(int participantPaymentId, string? correlationId = null, CancellationToken cancellationToken = default);
    Task SetCaptureFailedAsync(int participantPaymentId, string? errorCode, string? errorMessage, string? correlationId = null, CancellationToken cancellationToken = default);
    Task SetCancelledAsync(int participantPaymentId, string? correlationId = null, CancellationToken cancellationToken = default);
    Task<ParticipantPayment> CreateAsync(int orderId, int participantId, string? merchantId, long amountMinorUnits, string currency, string providerName, CancellationToken cancellationToken = default);
}
