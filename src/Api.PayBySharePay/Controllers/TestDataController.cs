using DataStorage.PayBySharePay.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.PayBySharePay.Controllers;

/// <summary>
/// Testværktøjer til at nulstille transaktionsdata uden at slette brugere,
/// merchants, venner eller Vipps-testmappinger.
/// </summary>
[Authorize]
[ApiController]
[Route("api/test-data")]
public sealed class TestDataController(PayBySharePayDbContext context) : ControllerBase
{
    /// <summary>
    /// Sletter alle ordrer, ordrelinjer, betalingsdata og beskeder, så et nyt
    /// testforløb kan startes fra en ren transaktionstilstand.
    /// </summary>
    [HttpDelete("reset")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Reset()
    {
        context.MerchantOrderLines.RemoveRange(context.MerchantOrderLines);
        context.MerchantOrderDrafts.RemoveRange(context.MerchantOrderDrafts);
        context.ParticipantPayments.RemoveRange(context.ParticipantPayments);
        context.Payments.RemoveRange(context.Payments);
        context.OrderParticipants.RemoveRange(context.OrderParticipants);
        context.Messages.RemoveRange(context.Messages);
        context.Orders.RemoveRange(context.Orders);

        await context.SaveChangesAsync();
        return NoContent();
    }
}
