namespace DataStorage.PayBySharePay.Entities;

public class ParticipantPayment
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public int ParticipantId { get; set; }
    public Participant Participant { get; set; } = null!;
    public string? MerchantId { get; set; }
    public long AmountMinorUnits { get; set; }
    public string Currency { get; set; } = "DKK";
    public ParticipantPaymentStatus Status { get; set; } = ParticipantPaymentStatus.Created;
    public string? ProviderName { get; set; }
    public string? ProviderPaymentId { get; set; }
    public string? ProviderReference { get; set; }
    public DateTime? ReservationStartedAtUtc { get; set; }
    public DateTime? ReservedAtUtc { get; set; }
    public DateTime? CaptureStartedAtUtc { get; set; }
    public DateTime? CapturedAtUtc { get; set; }
    public DateTime? CancelledAtUtc { get; set; }
    public string? LastErrorCode { get; set; }
    public string? LastErrorMessage { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public byte[] RowVersion { get; set; } = [];
}
