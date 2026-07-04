namespace DataStorage.PayBySharePay.Entities;

public class ParticipantExternalLogin
{
    public int Id { get; set; }
    public int ParticipantId { get; set; }
    public Participant Participant { get; set; } = null!;

    /// <summary>Udbyderens navn, f.eks. "Google" eller "Apple".</summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>Brugerens unikke ID hos udbyderen (subject claim).</summary>
    public string ProviderUserId { get; set; } = string.Empty;

    /// <summary>E-mail returneret af udbyderen på tidspunktet for tilknytning.</summary>
    public string? Email { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
