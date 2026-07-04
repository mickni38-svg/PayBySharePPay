namespace Service.PayBySharePay.DTOs;

public class VippsTestPersonDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }

    /// <summary>Id på den bruger der allerede har valgt denne testperson. Null = ledig.</summary>
    public int? MappedByParticipantId { get; set; }
}
