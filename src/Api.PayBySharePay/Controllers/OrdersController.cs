using Api.PayBySharePay.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.PayBySharePay.DTOs;
using Service.PayBySharePay.Interfaces;
namespace Api.PayBySharePay.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IExternalPaymentService _externalPaymentService;
    private readonly IGroupPaymentOrchestrationService _orchestration;

    public OrdersController(
        IOrderService orderService,
        IExternalPaymentService externalPaymentService,
        IGroupPaymentOrchestrationService orchestration)
    {
        _orderService = orderService;
        _externalPaymentService = externalPaymentService;
        _orchestration = orchestration;
    }

    /// <summary>Henter alle ordrer, eller filtrerer på participantId</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrderSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int? participantId = null)
    {
        if (participantId.HasValue)
        {
            var filtered = await _orderService.GetOrdersByParticipantAsync(participantId.Value);
            return Ok(filtered);
        }

        var results = await _orderService.GetAllOrdersAsync();
        return Ok(results);
    }

    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var dto = new CreateOrderDto
        {
            CreatedByParticipantId = request.CreatedByParticipantId,
            Title = request.Title,
            Category = request.Category,
            Message = request.Message,
            MerchantParticipantId = request.MerchantParticipantId,
            ParticipantIds = request.ParticipantIds
        };

        var result = await _orderService.CreateOrderAsync(dto);
        return CreatedAtAction(nameof(GetOverview), new { id = result.Id }, result);
    }

    [HttpGet("{id}/overview")]
    [ProducesResponseType(typeof(OrderOverviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOverview(int id)
    {
        var result = await _orderService.GetOrderOverviewAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// Returnerer capture-status for alle betalinger på ordren.
    /// </summary>
    [HttpGet("{id}/capture-status")]
    [ProducesResponseType(typeof(CaptureStatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCaptureStatus(int id)
    {
        var result = await _orchestration.GetCaptureStatusAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// Deltager starter betalingsreservation for sin del af gruppebetalingen.
    /// Kalder IPaymentProvider.ReserveAsync og returnerer redirect-url til betalingsudbyder.
    /// </summary>
    [HttpPost("{id}/reserve")]
    [ProducesResponseType(typeof(ReserveParticipantPaymentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReservePayment(int id, [FromBody] Api.PayBySharePay.DTOs.ReservePaymentRequest request)
    {
        var result = await _orchestration.ReserveParticipantPaymentAsync(
            orderId: id,
            participantId: request.ParticipantId,
            merchantId: request.MerchantId,
            amountMinorUnits: request.AmountMinorUnits,
            currency: request.Currency,
            returnUrl: request.ReturnUrl,
            callbackUrl: request.CallbackUrl);

        if (!result.Success)
            return BadRequest(new { result.ErrorCode, result.ErrorMessage });

        return Ok(result);
    }

    /// <summary>Host gennemfører gruppebetaling — sætter status til Completed</summary>
    [HttpPost("{id}/complete")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CompleteOrder(int id, [FromBody] CompleteOrderRequest request)
    {
        var result = await _orderService.CompleteOrderAsync(id, request.RequestingParticipantId);
        return Ok(result);
    }

    /// <summary>
    /// Host annullerer ordren — canceller alle ikke-capturede betalingsreservationer.
    /// Idempotent: allerede annullerede ordrer returnerer success.
    /// </summary>
    [HttpPost("{id}/cancel")]
    [ProducesResponseType(typeof(CancelOrderResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelOrder(int id, [FromBody] CancelOrderRequest request)
    {
        var result = await _orchestration.CancelOrderAsync(id, request.RequestingParticipantId);
        return Ok(result);
    }

    /// <summary>
    /// Host godkender ordren — capture'r alle reserverede betalinger én ad gangen.
    /// Kun host kan kalde dette endpoint. Kræver ordre i status ReadyToPay.
    /// Er idempotent: allerede captured betalinger springes over.
    /// </summary>
    [HttpPost("{id}/approve")]
    [ProducesResponseType(typeof(ApproveAndCaptureResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ApproveOrder(int id, [FromBody] ApproveOrderRequest request)
    {
        var result = await _orchestration.ApproveAndCaptureAllAsync(id, request.RequestingParticipantId);
        return Ok(result);
    }

    /// <summary>
    /// Host initierer betaling via eksternt betalings-API.
    /// Kalder dummy (fremtidigt: rigtigt) betalings-API, og ved success gemmes ordren som Completed.
    /// </summary>
    [HttpPost("{id}/pay")]
    [ProducesResponseType(typeof(PayOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status402PaymentRequired)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PayOrder(int id, [FromBody] PayOrderRequest request)
    {
        // 1. Kald eksternt betalings-API (dummy → altid success)
        var overview = await _orderService.GetOrderOverviewAsync(id );

        var amount = request.Amount > 0 ? request.Amount : overview.TotalAmount;

        var paymentResult = await _externalPaymentService.ChargeAsync(new(
            OrderId: id,
            Amount: amount,
            Currency: request.Currency,
            Description: $"Gruppebetaling #{id}: {overview.Title}"
        ));

        if (!paymentResult.Success)
        {
            return StatusCode(StatusCodes.Status402PaymentRequired,
                new { error = paymentResult.ErrorMessage ?? "Betaling afvist af betalingsudbyderen." });
        }

        // 2. Gem i vores DB og sæt ordre til Completed
        var order = await _orderService.CompleteOrderAsync(id, request.RequestingParticipantId);

        return Ok(new PayOrderResponse
        {
            OrderId = id,
            Status = order.Status,
            PaymentReference = paymentResult.PaymentReference
        });
    }
}
