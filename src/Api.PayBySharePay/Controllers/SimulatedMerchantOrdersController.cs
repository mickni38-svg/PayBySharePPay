using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;
using Service.PayBySharePay.DTOs;

namespace Api.PayBySharePay.Controllers;

[ApiController]
[Route("api/simulated-merchant/orders")]
public sealed class SimulatedMerchantOrdersController : ControllerBase
{
    private static readonly ConcurrentDictionary<string, SquareInspiredMerchantOrderResponse> Orders = new();

    [HttpPost]
    public ActionResult<SquareInspiredMerchantOrderResponse> Create(SquareInspiredMerchantOrderRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.IdempotencyKey) || string.IsNullOrWhiteSpace(request.ReferenceId))
            return BadRequest();

        var response = Orders.GetOrAdd(request.IdempotencyKey, _ => new SquareInspiredMerchantOrderResponse
        {
            OrderId = $"SIM-{request.ReferenceId}",
            ReferenceId = request.ReferenceId,
            Status = "ACCEPTED"
        });
        return Ok(response);
    }
}
