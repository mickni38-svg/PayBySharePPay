using Api.PayBySharePay.DTOs;
using Microsoft.AspNetCore.Mvc;
using Service.PayBySharePay.DTOs;
using Service.PayBySharePay.Interfaces;

namespace Api.PayBySharePay.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RegisterPayment([FromBody] RegisterPaymentRequest request)
    {
        var dto = new RegisterPaymentDto
        {
            OrderId = request.OrderId,
            ParticipantId = request.ParticipantId,
            Amount = request.Amount
        };

        var result = await _paymentService.RegisterPaymentAsync(dto);
        return StatusCode(StatusCodes.Status201Created, result);
    }
}
