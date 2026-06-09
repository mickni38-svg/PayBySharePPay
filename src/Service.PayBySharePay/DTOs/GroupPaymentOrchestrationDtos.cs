namespace Service.PayBySharePay.DTOs;

/// <summary>Resultat af at reservere én deltagers betaling.</summary>
public sealed record ReserveParticipantPaymentResult(
    bool Success,
    int ParticipantPaymentId,
    string? RedirectUrl,
    string? ErrorCode,
    string? ErrorMessage);

/// <summary>Samlet resultat af Host's "Godkend ordre" — capture af alle reserverede betalinger.</summary>
public sealed class ApproveAndCaptureResult
{
    public bool AllCaptured { get; init; }
    public string OrderStatus { get; init; } = string.Empty;
    public List<ParticipantCaptureResult> Results { get; init; } = [];
}

/// <summary>Capture-resultat pr. deltager.</summary>
public sealed class ParticipantCaptureResult
{
    public int ParticipantId { get; init; }
    public string ParticipantName { get; init; } = string.Empty;
    public int ParticipantPaymentId { get; init; }
    public bool Success { get; init; }
    public string? ProviderCaptureId { get; init; }
    public string? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }
}

/// <summary>Resultat af at annullere en ordre.</summary>
public sealed class CancelOrderResult
{
    public bool Success { get; init; }
    public string OrderStatus { get; init; } = string.Empty;
    public int CancelledCount { get; init; }
    public int SkippedCount { get; init; }
    public List<string> Errors { get; init; } = [];
}

/// <summary>Capture-status for alle betalinger på en ordre — bruges af GET capture-status endpoint.</summary>
public sealed class CaptureStatusDto
{
    public int OrderId { get; init; }
    public string OrderStatus { get; init; } = string.Empty;
    public List<ParticipantPaymentStatusDto> Payments { get; init; } = [];
}

/// <summary>Betalingsstatus for én deltager — returneres i CaptureStatusDto.</summary>
public sealed class ParticipantPaymentStatusDto
{
    public int ParticipantId { get; init; }
    public string ParticipantName { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string? ProviderTransactionId { get; init; }
}

/// <summary>Webhook payload fra betalingsudbyder (MobilePay/Fake) ved statusopdatering.</summary>
public sealed class PaymentWebhookRequest
{
    public string ProviderPaymentId { get; init; } = string.Empty;
    /// <summary>fx "Reserved", "Captured", "Cancelled", "Failed"</summary>
    public string Status { get; init; } = string.Empty;
    public string? ProviderReference { get; init; }
}

/// <summary>Webhook response.</summary>
public sealed class PaymentWebhookResult
{
    public bool Accepted { get; init; }
    public string? Message { get; init; }
}
