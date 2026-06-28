using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.PayBySharePay.DTOs;
using Service.PayBySharePay.Interfaces;
using DataStorage.PayBySharePay.Repositories;

namespace Api.PayBySharePay.Controllers;

/// <summary>
/// Modtager payment-status callbacks fra Vipps MobilePay ePayment API.
///
/// Vipps sender POST til denne URL ved hver betalingshændelse:
///   CREATED, AUTHORIZED (= reserved), CAPTURED, CANCELLED, ABORTED, EXPIRED, TERMINATED
///
/// URL registreres som <c>webhookUrl</c> i CreatePayment-kaldet.
/// Se: https://developer.vippsmobilepay.com/api/epayment/#tag/Merchant-Endpoints/operation/cardCallbackUrl
/// </summary>
[ApiController]
[Route("api/payments/vipps")]
public class VippsCallbackController : ControllerBase
{
    private readonly IParticipantPaymentRepository _paymentRepository;
    private readonly IParticipantPaymentStateService _stateService;
    private readonly IOrderService _orderService;
    private readonly ILogger<VippsCallbackController> _logger;

    public VippsCallbackController(
        IParticipantPaymentRepository paymentRepository,
        IParticipantPaymentStateService stateService,
        IOrderService orderService,
        ILogger<VippsCallbackController> logger)
    {
        _paymentRepository = paymentRepository;
        _stateService = stateService;
        _orderService = orderService;
        _logger = logger;
    }

    /// <summary>
    /// Callback endpoint til Vipps MobilePay ePayment.
    /// Vipps sender POST her når betalingsstatus ændres.
    /// Path-parameter <c>{reference}</c> er vores <c>ParticipantPaymentId</c>.
    /// </summary>
    [HttpPost("callbacks/{reference}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> VippsCallback(
        [FromRoute] string reference,
        [FromBody] VippsCallbackPayload payload,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "[VippsCallback] Modtaget status={Status} for reference={Reference} (psp={PspReference})",
            payload.Name, reference, payload.PspReference);

        var payment = await _paymentRepository.GetByProviderPaymentIdAsync(reference);
        if (payment is null)
        {
            _logger.LogWarning("[VippsCallback] Ingen betaling fundet for reference={Reference} — returnerer 200 for at undgå Vipps-retry", reference);
            return Ok();
        }

        var correlationId = $"vipps-callback-{reference}-{payload.Name}";

        switch (payload.Name?.ToUpperInvariant())
        {
            case "AUTHORIZED":
            case "RESERVE":
                await _stateService.SetReservedAsync(payment.Id, correlationId, cancellationToken);

                // Tjek om alle deltagere nu er Reserved — sæt ordre ReadyToPay hvis ja
                try
                {
                    await _orderService.CheckAndSetReadyToPayByReservedAsync(payment.OrderId, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "[VippsCallback] CheckAndSetReadyToPayByReservedAsync fejlede for ordre {OrderId} — betaling er stadig Reserved",
                        payment.OrderId);
                }
                break;

            case "CAPTURED":
                // Capture styres af vores eget approve-flow — ingen state-ændring her
                _logger.LogInformation("[VippsCallback] CAPTURED bekræftet for reference={Reference} — ignoreres (capture håndteres af approve-flow)", reference);
                break;

            case "CANCELLED":
            case "ABORTED":
                await _stateService.SetCancelledAsync(payment.Id, correlationId, cancellationToken);
                break;

            case "TERMINATED":
            case "EXPIRED":
                await _stateService.SetReservationFailedAsync(
                    payment.Id,
                    payload.Name.ToUpperInvariant(),
                    $"Betaling {payload.Name.ToLower()} af Vipps MobilePay.",
                    correlationId,
                    cancellationToken);
                break;

            default:
                _logger.LogInformation("[VippsCallback] Ukendt status={Status} for reference={Reference} — ignoreret", payload.Name, reference);
                break;
        }

        return Ok();
    }
}

/// <summary>
/// Payload som Vipps MobilePay sender til callback-URL ved betalingshændelser.
/// Se: https://developer.vippsmobilepay.com/api/epayment/#tag/Merchant-Endpoints/operation/cardCallbackUrl
/// </summary>
public sealed class VippsCallbackPayload
{
    /// <summary>Vores reference (ParticipantPaymentId).</summary>
    public string Reference { get; init; } = string.Empty;

    /// <summary>Vipps' interne transaktionsreference.</summary>
    public string? PspReference { get; init; }

    /// <summary>Begivenhedens navn: CREATED, AUTHORIZED, CAPTURED, CANCELLED, ABORTED, EXPIRED, TERMINATED.</summary>
    public string Name { get; init; } = string.Empty;

    public VippsCallbackAmount? Amount { get; init; }
    public string? Timestamp { get; init; }
    public string? IdempotencyKey { get; init; }
}

public sealed class VippsCallbackAmount
{
    public long Value { get; init; }
    public string Currency { get; init; } = string.Empty;
}
