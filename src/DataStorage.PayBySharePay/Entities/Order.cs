namespace DataStorage.PayBySharePay.Entities;

public class Order
{
    public int Id { get; set; }
    public int CreatedByParticipantId { get; set; }
    public Participant CreatedBy { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Message { get; set; }
    public string Status { get; set; } = "Collecting";
    public int? MerchantParticipantId { get; set; }
    public Participant? MerchantParticipant { get; set; }
    public string? JoinToken { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<OrderParticipant> OrderParticipants { get; set; } = new List<OrderParticipant>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<Message> Messages { get; set; } = new List<Message>();
    public ICollection<MerchantOrderDraft> MerchantOrderDrafts { get; set; } = new List<MerchantOrderDraft>();
}
