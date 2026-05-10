namespace DataStorage.PayBySharePay.Entities;

public class Participant
{
    public int Id { get; set; }
    public ParticipantType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }

    // Merchant-specific fields
    public string? CompanyName { get; set; }
    public string? CvrNumber { get; set; }
    public string? VatNumber { get; set; }
    public string? ContactPerson { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? CompanyAddress { get; set; }
    public string? PaymentReference { get; set; }
    public string? PayoutAccountInfo { get; set; }
    public string? PaymentProvider { get; set; }
    public string? GroupOrderUrl { get; set; }

    public ICollection<FriendRelation> FriendsInitiated { get; set; } = new List<FriendRelation>();
    public ICollection<FriendRelation> FriendsReceived { get; set; } = new List<FriendRelation>();
    public ICollection<OrderParticipant> OrderParticipants { get; set; } = new List<OrderParticipant>();
}
