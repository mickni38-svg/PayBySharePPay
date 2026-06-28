using Api.PayBySharePay.Services;
using DataStorage.PayBySharePay.Context;
using DataStorage.PayBySharePay.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.PayBySharePay.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DevController : ControllerBase
{
    private readonly PayBySharePayDbContext _context;
    private readonly ILastMerchantCallbackStore _callbackStore;

    public DevController(PayBySharePayDbContext context, ILastMerchantCallbackStore callbackStore)
    {
        _context = context;
        _callbackStore = callbackStore;
    }

    /// <summary>
    /// TEST ONLY – sletter alle ordre, ordrelinjer, betalinger og beskeder.
    /// </summary>
    [HttpDelete("reset")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ResetData()
    {
        _context.MerchantOrderLines.RemoveRange(_context.MerchantOrderLines);
        _context.MerchantOrderDrafts.RemoveRange(_context.MerchantOrderDrafts);
        _context.Payments.RemoveRange(_context.Payments);
        _context.OrderParticipants.RemoveRange(_context.OrderParticipants);
        _context.Messages.RemoveRange(_context.Messages);
        _context.Orders.RemoveRange(_context.Orders);

        await _context.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// TEST ONLY – sætter GroupOrderUrl på alle merchants der mangler det.
    /// </summary>
    [HttpPost("seed-merchant-urls")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SeedMerchantUrls([FromQuery] string merchantDemoUrl = "https://brave-flower-0026a7503.7.azurestaticapps.net/" )
    {
        var merchants = await _context.Participants
            .Where(p => p.Type == ParticipantType.Merchant && p.GroupOrderUrl == null)
            .ToListAsync();

        foreach (var m in merchants)
            m.GroupOrderUrl = merchantDemoUrl;

        await _context.SaveChangesAsync();
        return Ok(new { updated = merchants.Count, url = merchantDemoUrl });
    }

    /// <summary>
    /// DEV/TEST ONLY – returnerer den seneste GroupOrderPaid-payload sendt til merchant for en given ordre.
    /// Bruges til at verificere final callback indhold uden et rigtigt merchant-endpoint.
    /// </summary>
    [HttpGet("merchant-callbacks/latest")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetLatestMerchantCallback([FromQuery] int orderId)
    {
        var payload = _callbackStore.Get(orderId);
        if (payload is null)
            return NotFound(new { message = $"Ingen callback fundet for ordre {orderId}" });

        return Ok(payload);
    }
}
