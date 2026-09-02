using System.Collections.Concurrent;
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
    private static readonly ConcurrentDictionary<string, Lazy<Task<OrderDto>>> CreateRequests = new();

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
        var title = request.Title.Trim();
        if (title.Length == 0 || title.Length > 80)
            return BadRequest(new { message = "Titel skal udfyldes og må højst være 80 tegn." });

        if (request.Message?.Length > 500)
            return BadRequest(new { message = "Besked må højst være 500 tegn." });

        if (!request.MerchantParticipantId.HasValue)
            return BadRequest(new { message = "Der skal vælges et spisested." });

        if (request.ParticipantIds.Count == 0)
            return BadRequest(new { message = "Vælg mindst én deltager." });

        if (request.ParticipantIds.Contains(request.CreatedByParticipantId))
            return BadRequest(new { message = "Værten må ikke være med i deltagerlisten." });

        if (request.ParticipantIds.Contains(request.MerchantParticipantId.Value))
            return BadRequest(new { message = "Spisestedet må ikke være med i deltagerlisten." });

        if (request.ParticipantIds.Distinct().Count() != request.ParticipantIds.Count)
            return BadRequest(new { message = "En deltager må kun vælges én gang." });

        var key = request.IdempotencyKey.Trim();
        var lazyCreate = CreateRequests.GetOrAdd(key, _ => new Lazy<Task<OrderDto>>(() =>
        {
            var dto = new CreateOrderDto
            {
                CreatedByParticipantId = request.CreatedByParticipantId,
                Title = title,
                Category = request.Category,
                Message = request.Message,
                MerchantParticipantId = request.MerchantParticipantId,
                ParticipantIds = request.ParticipantIds,
                IdempotencyKey = key
            };

            return _orderService.CreateOrderAsync(dto);
        }, LazyThreadSafetyMode.ExecutionAndPublication));

        try
        {
            var result = await lazyCreate.Value;
            return CreatedAtAction(nameof(GetOverview), new { id = result.Id }, result);
        }
        catch
        {
            CreateRequests.TryRemove(key, out _);
            throw;
        }
    }

    [HttpGet("{id}/overview")]
    [ProducesResponseType(typeof(OrderOverviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOverview(int id)
    {
        var result = await _orderService.GetOrderOverviewAsync(id);
        return Ok(result);
    }

    [HttpGet("{id}/capture-status")]
    [ProducesResponseType(typeof(CaptureStatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCaptureStatus(int id)
    {
        var result = await _orchestration.GetCaptureStatusAsync(id);
        return Ok(result);
    }

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

    [HttpPost("{id}/pay")]
    [ProducesResponseType(typeof(PayOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status402PaymentRequired)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PayOrder(int id, [FromBody] PayOrderRequest request)
    {
        var overview = await _orderService.GetOrderOverviewAsync(id);
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

        var order = await _orderService.CompleteOrderAsync(id, request.RequestingParticipantId);

        return Ok(new PayOrderResponse
        {
            OrderId = id,
            Status = order.Status,
            PaymentReference = paymentResult.PaymentReference
        });
    }
}
