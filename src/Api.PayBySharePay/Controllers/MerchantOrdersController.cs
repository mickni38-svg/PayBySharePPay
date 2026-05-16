using Api.PayBySharePay.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.PayBySharePay.DTOs;
using Service.PayBySharePay.Interfaces;

namespace Api.PayBySharePay.Controllers;

[Authorize]
[ApiController]
[Route("api/merchant-orders")]
public class MerchantOrdersController : ControllerBase
{
    private readonly IMerchantOrderService _merchantOrderService;

    public MerchantOrdersController(IMerchantOrderService merchantOrderService)
    {
        _merchantOrderService = merchantOrderService;
    }

    /// <summary>
    /// Modtager ordredata fra merchant (fx ved klik på "Betal som gruppe").
    /// Status sættes til Collecting. Ordre frigives ikke endnu.
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(MerchantOrderDraftDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> InitOrder([FromBody] InitMerchantOrderRequest request)
    {
        var dto = new InitMerchantOrderDto
        {
            OrderId = request.OrderId,
            MerchantParticipantId = request.MerchantParticipantId,
            MerchantDraftReference = request.MerchantDraftReference,
            SubtotalAmount = request.SubtotalAmount,
            TotalAmount = request.TotalAmount,
            Currency = request.Currency,
            PaymentMode = request.PaymentMode,
            ExpiresAtUtc = request.ExpiresAtUtc,
            Lines = request.Lines.Select(l => new MerchantOrderLineDto
            {
                LineId = l.LineId,
                Name = l.Name,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice,
                LineTotal = l.LineTotal
            }).ToList()
        };

        var result = await _merchantOrderService.InitOrderAsync(dto);
        return CreatedAtAction(nameof(GetByOrderId), new { orderId = result.OrderId }, result);
    }

    /// <summary>Henter merchant order draft for en given gruppebetaling</summary>
    [HttpGet("by-order/{orderId}")]
    [ProducesResponseType(typeof(MerchantOrderDraftDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByOrderId(int orderId)
    {
        var result = await _merchantOrderService.GetByOrderIdAsync(orderId);
        if (result is null) return NotFound();
        return Ok(result);
    }
}
