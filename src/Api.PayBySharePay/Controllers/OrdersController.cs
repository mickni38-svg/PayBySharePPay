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

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
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
}
