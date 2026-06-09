namespace DataStorage.PayBySharePay.Entities;

public class PaymentEventLog
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ParticipantPaymentId { get; set; }
    public string? ProviderPaymentId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public ParticipantPaymentStatus? OldStatus { get; set; }
    public ParticipantPaymentStatus? NewStatus { get; set; }
    public string? PayloadJson { get; set; }
    public string? CorrelationId { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
