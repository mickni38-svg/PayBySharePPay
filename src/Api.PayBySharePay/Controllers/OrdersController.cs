using Api.PayBySharePay.DTOs;
using Microsoft.AspNetCore.Mvc;
using Service.PayBySharePay.DTOs;
using Service.PayBySharePay.Interfaces;

namespace Api.PayBySharePay.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var dto = new CreateOrderDto
        {
            Title = request.Title,
            Category = request.Category,
            Message = request.Message,
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
}
