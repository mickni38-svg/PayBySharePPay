namespace DataStorage.PayBySharePay.Entities;

public class FriendRelation
{
    public int Id { get; set; }
    public int InitiatorId { get; set; }
    public Participant Initiator { get; set; } = null!;
    public int ReceiverId { get; set; }
    public Participant Receiver { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
