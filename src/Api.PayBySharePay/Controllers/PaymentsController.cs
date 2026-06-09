using Api.PayBySharePay.DTOs;
using DataStorage.PayBySharePay.Repositories;
using Microsoft.AspNetCore.Mvc;
using Service.PayBySharePay.DTOs;
using Service.PayBySharePay.Interfaces;

namespace Api.PayBySharePay.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly IParticipantPaymentStateService _stateService;
    private readonly IParticipantPaymentRepository _paymentRepository;

    public PaymentsController(
        IPaymentService paymentService,
        IParticipantPaymentStateService stateService,
        IParticipantPaymentRepository paymentRepository)
    {
        _paymentService = paymentService;
        _stateService = stateService;
        _paymentRepository = paymentRepository;
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

    /// <summary>
    /// Webhook fra betalingsudbyder (MobilePay/Vipps/Fake) ved statusopdatering.
    /// Opdaterer ParticipantPayment til Reserved, Cancelled eller Failed.
    /// AllowAnonymous: provider-signatur valideres ikke endnu (FakeProvider).
    /// </summary>
    [HttpPost("webhooks/provider")]
    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    [ProducesResponseType(typeof(PaymentWebhookResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ProviderWebhook([FromBody] PaymentWebhookRequest request)
        => await HandleWebhook(request);

    /// <summary>
    /// MobilePay-specifikt webhook alias — samme logik som /webhooks/provider.
    /// Bruges af doc 08 API-design og fremtidig MobilePay sandbox integration.
    /// </summary>
    [HttpPost("webhooks/mobilepay")]
    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    [ProducesResponseType(typeof(PaymentWebhookResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MobilePayWebhook([FromBody] PaymentWebhookRequest request)
        => await HandleWebhook(request);

    private async Task<IActionResult> HandleWebhook(PaymentWebhookRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ProviderPaymentId))
            return BadRequest(new { error = "ProviderPaymentId er påkrævet." });

        var payment = await _paymentRepository.GetByProviderPaymentIdAsync(request.ProviderPaymentId);
        if (payment is null)
            return NotFound(new { error = $"Ingen betaling fundet for ProviderPaymentId: {request.ProviderPaymentId}" });

        var correlationId = $"webhook-{request.ProviderPaymentId}";

        switch (request.Status?.ToUpperInvariant())
        {
            case "RESERVED":
            case "AUTHORIZED":
                await _stateService.SetReservedAsync(payment.Id, correlationId);
                break;

            case "CANCELLED":
                await _stateService.SetCancelledAsync(payment.Id, correlationId);
                break;

            case "FAILED":
                await _stateService.SetReservationFailedAsync(payment.Id, "PROVIDER_FAILED", "Betaling afvist af udbyder.", correlationId);
                break;

            default:
                return Ok(new PaymentWebhookResult { Accepted = false, Message = $"Ukendt status: {request.Status} — ignoreret." });
        }

        return Ok(new PaymentWebhookResult { Accepted = true });
    }
}
