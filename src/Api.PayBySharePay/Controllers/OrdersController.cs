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

    public OrdersController(IOrderService orderService, IExternalPaymentService externalPaymentService)
    {
        _orderService = orderService;
        _externalPaymentService = externalPaymentService;
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
