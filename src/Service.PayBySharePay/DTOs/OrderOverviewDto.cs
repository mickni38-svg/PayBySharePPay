namespace Service.PayBySharePay.DTOs;

public class OrderOverviewDto
{
    public int OrderId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Message { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<OrderParticipantDto> Participants { get; set; } = new();
    public List<PaymentDto> Payments { get; set; } = new();
    public List<MessageDto> Messages { get; set; } = new();
}
