namespace Service.PayBySharePay.DTOs;

public class CreateOrderDto
{
    public string Title { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Message { get; set; }
    public List<int> ParticipantIds { get; set; } = new();
}
