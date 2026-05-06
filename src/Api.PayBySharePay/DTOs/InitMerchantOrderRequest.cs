using System.ComponentModel.DataAnnotations;

namespace Api.PayBySharePay.DTOs;

public class InitMerchantOrderRequest
{
    [Required]
    [Range(1, int.MaxValue)]
    public int OrderId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int MerchantParticipantId { get; set; }

    [Required]
    public string MerchantDraftReference { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue)]
    public decimal SubtotalAmount { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal TotalAmount { get; set; }

    public string Currency { get; set; } = "DKK";
    public string PaymentMode { get; set; } = "AuthorizeThenCapture";
    public DateTime? ExpiresAtUtc { get; set; }

    public List<OrderLineRequest> Lines { get; set; } = new();
}

public class OrderLineRequest
{
    public string LineId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}
