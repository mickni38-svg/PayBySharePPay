namespace Service.PayBySharePay.DTOs;

public class CreateMessageDto
{
    public int OrderId { get; set; }
    public int ParticipantId { get; set; }
    public string Content { get; set; } = string.Empty;
}
