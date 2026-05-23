using System.ComponentModel.DataAnnotations;

namespace Api.PayBySharePay.DTOs;

public class PayOrderRequest
{
    [Required]
    [Range(1, int.MaxValue)]
    public int RequestingParticipantId { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "DKK";
}

public class PayOrderResponse
{
    public int OrderId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentReference { get; set; } = string.Empty;
}
