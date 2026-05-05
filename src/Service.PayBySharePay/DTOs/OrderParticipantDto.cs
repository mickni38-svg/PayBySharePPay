namespace Service.PayBySharePay.DTOs;

public class OrderParticipantDto
{
    public int ParticipantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
