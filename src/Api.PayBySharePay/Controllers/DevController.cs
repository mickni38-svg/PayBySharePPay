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

    public DevController(PayBySharePayDbContext context)
    {
        _context = context;
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
    public async Task<IActionResult> SeedMerchantUrls([FromQuery] string merchantDemoUrl = "https://ashy-bay-0e753db03.7.azurestaticapps.net")
    {
        var merchants = await _context.Participants
            .Where(p => p.Type == ParticipantType.Merchant && p.GroupOrderUrl == null)
            .ToListAsync();

        foreach (var m in merchants)
            m.GroupOrderUrl = merchantDemoUrl;

        await _context.SaveChangesAsync();
        return Ok(new { updated = merchants.Count, url = merchantDemoUrl });
    }
}
