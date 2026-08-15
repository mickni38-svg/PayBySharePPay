namespace DataStorage.PayBySharePay.Entities;

public class Participant
{
    public int Id { get; set; }
    public ParticipantType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? PasswordHash { get; set; }

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

    /// <summary>
    /// Vipps MobilePay Merchant Serial Number (MSN) for dette salgssted.
    /// Bruges til at sende betalinger direkte til det rigtige salgssted.
    /// Null = brug global MSN fra appsettings.
    /// </summary>
    public string? VippsMerchantSerialNumber { get; set; }

    /// <summary>Vipps API ClientId for dette salgssted. Null = brug global fra appsettings.</summary>
    public string? VippsClientId { get; set; }

    /// <summary>Vipps API ClientSecret for dette salgssted. Null = brug global fra appsettings.</summary>
    public string? VippsClientSecret { get; set; }

    /// <summary>Vipps Ocp-Apim-Subscription-Key for dette salgssted. Null = brug global fra appsettings.</summary>
    public string? VippsSubscriptionKey { get; set; }

    // Merchant logo
    public byte[]? LogoImageData { get; set; }
    public string? LogoContentType { get; set; }
    public string? LogoFileName { get; set; }
    public DateTime? LogoUpdatedAtUtc { get; set; }

    /// <summary>
    /// Midlertidig dev-mapping: hvilken testperson i databasen repræsenterer denne bruger i Vipps sandbox.
    /// Null = ingen mapping sat endnu.
    /// </summary>
    public int? VippsTestUserId { get; set; }
    public Participant? VippsTestUser { get; set; }

    public ICollection<FriendRelation> FriendsInitiated { get; set; } = new List<FriendRelation>();
    public ICollection<FriendRelation> FriendsReceived { get; set; } = new List<FriendRelation>();
    public ICollection<OrderParticipant> OrderParticipants { get; set; } = new List<OrderParticipant>();
    public ICollection<ParticipantExternalLogin> ExternalLogins { get; set; } = new List<ParticipantExternalLogin>();
}
