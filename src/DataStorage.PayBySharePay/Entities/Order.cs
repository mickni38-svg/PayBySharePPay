namespace DataStorage.PayBySharePay.Entities;

public class Order
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Message { get; set; }
    public string Status { get; set; } = "Open";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<OrderParticipant> OrderParticipants { get; set; } = new List<OrderParticipant>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
