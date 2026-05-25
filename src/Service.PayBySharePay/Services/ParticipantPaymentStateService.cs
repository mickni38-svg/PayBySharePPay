using DataStorage.PayBySharePay.Entities;
using DataStorage.PayBySharePay.Repositories;
using Microsoft.Extensions.Logging;
using Service.PayBySharePay.Interfaces;

namespace Service.PayBySharePay.Services;

public class ParticipantPaymentStateService(
    IParticipantPaymentRepository paymentRepository,
    IPaymentEventLogRepository eventLogRepository,
    ILogger<ParticipantPaymentStateService> logger) : IParticipantPaymentStateService
{
    private static readonly IReadOnlyDictionary<ParticipantPaymentStatus, IReadOnlySet<ParticipantPaymentStatus>> AllowedTransitions =
        new Dictionary<ParticipantPaymentStatus, IReadOnlySet<ParticipantPaymentStatus>>
        {
            [ParticipantPaymentStatus.Created]              = new HashSet<ParticipantPaymentStatus> { ParticipantPaymentStatus.ReservationStarted },
            [ParticipantPaymentStatus.ReservationStarted]  = new HashSet<ParticipantPaymentStatus> { ParticipantPaymentStatus.Reserved, ParticipantPaymentStatus.ReservationFailed, ParticipantPaymentStatus.Cancelled },
            [ParticipantPaymentStatus.Reserved]             = new HashSet<ParticipantPaymentStatus> { ParticipantPaymentStatus.CapturePending, ParticipantPaymentStatus.Cancelled },
            [ParticipantPaymentStatus.CapturePending]       = new HashSet<ParticipantPaymentStatus> { ParticipantPaymentStatus.Captured, ParticipantPaymentStatus.CaptureFailed },
            [ParticipantPaymentStatus.CaptureFailed]        = new HashSet<ParticipantPaymentStatus> { ParticipantPaymentStatus.CapturePending },
            [ParticipantPaymentStatus.Captured]             = new HashSet<ParticipantPaymentStatus> { ParticipantPaymentStatus.Refunded },
            [ParticipantPaymentStatus.ReservationFailed]    = new HashSet<ParticipantPaymentStatus>(),
            [ParticipantPaymentStatus.Cancelled]            = new HashSet<ParticipantPaymentStatus>(),
            [ParticipantPaymentStatus.Expired]              = new HashSet<ParticipantPaymentStatus>(),
            [ParticipantPaymentStatus.Refunded]             = new HashSet<ParticipantPaymentStatus>(),
        };

    public async Task<ParticipantPayment> CreateAsync(
        int orderId, int participantId, string? merchantId, long amountMinorUnits,
        string currency, string providerName, CancellationToken cancellationToken = default)
    {
        var payment = new ParticipantPayment
        {
            OrderId = orderId,
            ParticipantId = participantId,
            MerchantId = merchantId,
            AmountMinorUnits = amountMinorUnits,
            Currency = currency,
            ProviderName = providerName,
            Status = ParticipantPaymentStatus.Created,
            CreatedAtUtc = DateTime.UtcNow
        };

        await paymentRepository.AddAsync(payment);

        await LogEventAsync(payment, "Created", null, ParticipantPaymentStatus.Created, null, null, cancellationToken);

        return payment;
    }

    public async Task SetReservationStartedAsync(
        int participantPaymentId, string providerPaymentId, string? correlationId = null,
        CancellationToken cancellationToken = default)
    {
        var payment = await RequirePaymentAsync(participantPaymentId);

        if (payment.Status == ParticipantPaymentStatus.ReservationStarted)
        {
            logger.LogInformation("Idempotent: payment {Id} already in ReservationStarted", participantPaymentId);
            return;
        }

        var oldStatus = TransitionTo(payment, ParticipantPaymentStatus.ReservationStarted);
        payment.ProviderPaymentId = providerPaymentId;
        payment.ReservationStartedAtUtc = DateTime.UtcNow;

        await paymentRepository.SaveChangesAsync();
        await LogEventAsync(payment, "ReservationStarted", oldStatus, payment.Status, null, correlationId, cancellationToken);
    }

    public async Task SetReservedAsync(
        int participantPaymentId, string? correlationId = null,
        CancellationToken cancellationToken = default)
    {
        var payment = await RequirePaymentAsync(participantPaymentId);

        if (payment.Status == ParticipantPaymentStatus.Reserved)
        {
            logger.LogInformation("Idempotent: payment {Id} already Reserved", participantPaymentId);
            return;
        }

        var oldStatus = TransitionTo(payment, ParticipantPaymentStatus.Reserved);
        payment.ReservedAtUtc = DateTime.UtcNow;

        await paymentRepository.SaveChangesAsync();
        await LogEventAsync(payment, "Reserved", oldStatus, payment.Status, null, correlationId, cancellationToken);
    }

    public async Task SetReservationFailedAsync(
        int participantPaymentId, string? errorCode, string? errorMessage,
        string? correlationId = null, CancellationToken cancellationToken = default)
    {
        var payment = await RequirePaymentAsync(participantPaymentId);

        if (payment.Status == ParticipantPaymentStatus.ReservationFailed)
        {
            logger.LogInformation("Idempotent: payment {Id} already ReservationFailed", participantPaymentId);
            return;
        }

        var oldStatus = TransitionTo(payment, ParticipantPaymentStatus.ReservationFailed);
        payment.LastErrorCode = errorCode;
        payment.LastErrorMessage = errorMessage;

        await paymentRepository.SaveChangesAsync();
        await LogEventAsync(payment, "ReservationFailed", oldStatus, payment.Status, null, correlationId, cancellationToken);
    }

    public async Task SetCapturePendingAsync(
        int participantPaymentId, string? correlationId = null,
        CancellationToken cancellationToken = default)
    {
        var payment = await RequirePaymentAsync(participantPaymentId);

        if (payment.Status == ParticipantPaymentStatus.CapturePending)
        {
            logger.LogInformation("Idempotent: payment {Id} already CapturePending", participantPaymentId);
            return;
        }

        var oldStatus = TransitionTo(payment, ParticipantPaymentStatus.CapturePending);
        payment.CaptureStartedAtUtc = DateTime.UtcNow;

        await paymentRepository.SaveChangesAsync();
        await LogEventAsync(payment, "CapturePending", oldStatus, payment.Status, null, correlationId, cancellationToken);
    }

    public async Task SetCapturedAsync(
        int participantPaymentId, string? correlationId = null,
        CancellationToken cancellationToken = default)
    {
        var payment = await RequirePaymentAsync(participantPaymentId);

        if (payment.Status == ParticipantPaymentStatus.Captured)
        {
            logger.LogInformation("Idempotent: payment {Id} already Captured", participantPaymentId);
            return;
        }

        var oldStatus = TransitionTo(payment, ParticipantPaymentStatus.Captured);
        payment.CapturedAtUtc = DateTime.UtcNow;

        await paymentRepository.SaveChangesAsync();
        await LogEventAsync(payment, "Captured", oldStatus, payment.Status, null, correlationId, cancellationToken);
    }

    public async Task SetCaptureFailedAsync(
        int participantPaymentId, string? errorCode, string? errorMessage,
        string? correlationId = null, CancellationToken cancellationToken = default)
    {
        var payment = await RequirePaymentAsync(participantPaymentId);

        if (payment.Status == ParticipantPaymentStatus.CaptureFailed)
        {
            logger.LogInformation("Idempotent: payment {Id} already CaptureFailed", participantPaymentId);
            return;
        }

        var oldStatus = TransitionTo(payment, ParticipantPaymentStatus.CaptureFailed);
        payment.LastErrorCode = errorCode;
        payment.LastErrorMessage = errorMessage;

        await paymentRepository.SaveChangesAsync();
        await LogEventAsync(payment, "CaptureFailed", oldStatus, payment.Status, null, correlationId, cancellationToken);
    }

    public async Task SetCancelledAsync(
        int participantPaymentId, string? correlationId = null,
        CancellationToken cancellationToken = default)
    {
        var payment = await RequirePaymentAsync(participantPaymentId);

        if (payment.Status == ParticipantPaymentStatus.Cancelled)
        {
            logger.LogInformation("Idempotent: payment {Id} already Cancelled", participantPaymentId);
            return;
        }

        var oldStatus = TransitionTo(payment, ParticipantPaymentStatus.Cancelled);
        payment.CancelledAtUtc = DateTime.UtcNow;

        await paymentRepository.SaveChangesAsync();
        await LogEventAsync(payment, "Cancelled", oldStatus, payment.Status, null, correlationId, cancellationToken);
    }

    // ---- helpers ----

    private async Task<ParticipantPayment> RequirePaymentAsync(int id)
    {
        var payment = await paymentRepository.GetByIdAsync(id)
            ?? throw new InvalidOperationException($"ParticipantPayment {id} not found.");
        return payment;
    }

    private static ParticipantPaymentStatus TransitionTo(ParticipantPayment payment, ParticipantPaymentStatus newStatus)
    {
        var old = payment.Status;

        if (!AllowedTransitions.TryGetValue(old, out var allowed) || !allowed.Contains(newStatus))
            throw new InvalidOperationException(
                $"Invalid state transition for payment {payment.Id}: {old} -> {newStatus}.");

        payment.Status = newStatus;
        return old;
    }

    private async Task LogEventAsync(
        ParticipantPayment payment, string eventType,
        ParticipantPaymentStatus? oldStatus, ParticipantPaymentStatus? newStatus,
        string? payloadJson, string? correlationId,
        CancellationToken cancellationToken)
    {
        _ = cancellationToken;

        var log = new PaymentEventLog
        {
            OrderId = payment.OrderId,
            ParticipantPaymentId = payment.Id,
            ProviderPaymentId = payment.ProviderPaymentId,
            EventType = eventType,
            OldStatus = oldStatus,
            NewStatus = newStatus,
            PayloadJson = payloadJson,
            CorrelationId = correlationId,
            CreatedAtUtc = DateTime.UtcNow
        };

        await eventLogRepository.AddAsync(log);
    }
}
