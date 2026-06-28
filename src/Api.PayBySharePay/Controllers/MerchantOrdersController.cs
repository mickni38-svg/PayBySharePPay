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
    /// Modtager ordredata fra merchant (fx ved klik på "Bekræft ordre og reservér betaling").
    /// Gemmer draft, starter Vipps/MobilePay reservation og returnerer redirectUrl.
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(InitMerchantOrderResultDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> InitOrder([FromBody] InitMerchantOrderRequest request)
    {
        var dto = new InitMerchantOrderDto
        {
            OrderId = request.OrderId,
            MerchantParticipantId = request.MerchantParticipantId,
            ParticipantToken = request.ParticipantToken,
            MerchantDraftReference = request.MerchantDraftReference,
            SubtotalAmount = request.SubtotalAmount,
            TotalAmount = request.TotalAmount,
            Currency = request.Currency,
            PaymentMode = request.PaymentMode,
            ExpiresAtUtc = request.ExpiresAtUtc,
            TestPhoneNumber = request.TestPhoneNumber,
            RawMerchantPayloadJson = request.RawMerchantPayloadJson,
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

        if (result.Status == "Failed")
            return result.Message?.Contains("gennemført") == true
                ? UnprocessableEntity(result)   // ALREADY_CAPTURED
                : BadRequest(result);

        return StatusCode(StatusCodes.Status201Created, result);
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
