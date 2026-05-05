using Service.PayBySharePay.DTOs;

namespace Service.PayBySharePay.Interfaces;

public interface IPaymentService
{
    Task<PaymentDto> RegisterPaymentAsync(RegisterPaymentDto dto);
}
