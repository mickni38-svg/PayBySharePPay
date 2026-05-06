namespace Service.PayBySharePay.DTOs;

public class MerchantOrderLineDto
{
    public string LineId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}
