namespace DataStorage.PayBySharePay.Entities;

public class OrderParticipant
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public int ParticipantId { get; set; }
    public Participant Participant { get; set; } = null!;
    public string Status { get; set; } = "Pending";
}
