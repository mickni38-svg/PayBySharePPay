using System.ComponentModel.DataAnnotations;

namespace Api.PayBySharePay.DTOs;

public class RegisterPaymentRequest
{
    [Required]
    public int OrderId { get; set; }
    [Required]
    public int ParticipantId { get; set; }
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Beløb skal være større end 0.")]
    public decimal Amount { get; set; }
}
