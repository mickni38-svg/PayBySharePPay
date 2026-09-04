namespace Service.PayBySharePay.DTOs;

public class OrderOverviewDto
{
    public int OrderId { get; set; }
    public int CreatedByParticipantId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Message { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? MerchantName { get; set; }
    public string? MerchantLogoUrl { get; set; }
    public string? MerchantAddress { get; set; }
    public decimal TotalAmount { get; set; }
    public List<OrderParticipantDto> Participants { get; set; } = new();
    public List<PaymentDto> Payments { get; set; } = new();
    public List<MessageDto> Messages { get; set; } = new();
    public List<ParticipantOrderLinesDto> ParticipantOrderLines { get; set; } = new();
    /// <summary>Betalingsstatus pr. deltager fra ParticipantPayment-tabellen.</summary>
    public List<ParticipantPaymentSummaryDto> ParticipantPayments { get; set; } = new();
}

public class ParticipantOrderLinesDto
{
    public int ParticipantId { get; set; }
    public string ParticipantName { get; set; } = string.Empty;
    public bool HasPaid { get; set; }
    public List<MerchantOrderLineDto> Lines { get; set; } = new();
}

/// <summary>Betalingsstatus pr. deltager til Host-oversigt og test-dashboard.</summary>
public class ParticipantPaymentSummaryDto
{
    public int ParticipantPaymentId { get; set; }
    public int ParticipantId { get; set; }
    public string ParticipantName { get; set; } = string.Empty;
    public long AmountMinorUnits { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? ProviderPaymentId { get; set; }
    public DateTime? ReservedAtUtc { get; set; }
    public DateTime? CapturedAtUtc { get; set; }
    public string? LastErrorCode { get; set; }
    public string? LastErrorMessage { get; set; }
}
