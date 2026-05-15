namespace Service.PayBySharePay.DTOs;

public class OrderOverviewDto
{
    public int OrderId { get; set; }
    public int CreatedByParticipantId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Message { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? MerchantName { get; set; }
    public string? MerchantAddress { get; set; }
    public decimal TotalAmount { get; set; }
    public List<OrderParticipantDto> Participants { get; set; } = new();
    public List<PaymentDto> Payments { get; set; } = new();
    public List<MessageDto> Messages { get; set; } = new();
    public List<ParticipantOrderLinesDto> ParticipantOrderLines { get; set; } = new();
}

public class ParticipantOrderLinesDto
{
    public int ParticipantId { get; set; }
    public string ParticipantName { get; set; } = string.Empty;
    public bool HasPaid { get; set; }
    public List<MerchantOrderLineDto> Lines { get; set; } = new();
}
