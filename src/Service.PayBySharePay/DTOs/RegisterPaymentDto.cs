namespace Service.PayBySharePay.DTOs;

public class RegisterPaymentDto
{
    public int OrderId { get; set; }
    public int ParticipantId { get; set; }
    public decimal Amount { get; set; }
}
