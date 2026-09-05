namespace DataStorage.PayBySharePay.Entities;

/// <summary>
/// Permanent, paid order released to a merchant after all participant payments are captured.
/// Contains only the customer/contact snapshot needed by the merchant and no participant or
/// payment-provider identity.
/// </summary>
public sealed class MerchantOrder
{
    public int Id { get; set; }

    public int SourceOrderId { get; set; }
    public Order SourceOrder { get; set; } = null!;

    public int MerchantParticipantId { get; set; }
    public Participant MerchantParticipant { get; set; } = null!;

    public string PayNSyncOrderNumber { get; set; } = string.Empty;
    public string HostName { get; set; } = string.Empty;
    public string? HostPhone { get; set; }

    public string? DeliveryAddress { get; set; }
    public string? DeliveryPostalCode { get; set; }
    public string? DeliveryCity { get; set; }
    public string? DeliveryCountry { get; set; }

    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "DKK";
    public string PaymentStatus { get; set; } = "Paid";
    public string OrderHubStatus { get; set; } = "New";
    public string? Note { get; set; }
    public DateTime PaidAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>Ordrenummer returneret af merchantens eksterne ordresystem.</summary>
    public string? ExternalOrderNumber { get; set; }
    /// <summary>Råt svar fra merchantens ordre-API til audit og fejlsøgning.</summary>
    public string? ExternalResponseJson { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<MerchantOrderItem> Items { get; set; } = new List<MerchantOrderItem>();
}
