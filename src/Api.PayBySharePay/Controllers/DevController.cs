using Api.PayBySharePay.Services;
using DataStorage.PayBySharePay.Context;
using DataStorage.PayBySharePay.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service.PayBySharePay.Interfaces;

namespace Api.PayBySharePay.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DevController : ControllerBase
{
    private readonly PayBySharePayDbContext _context;
    private readonly ILastMerchantCallbackStore _callbackStore;
    private readonly IConfiguration _configuration;
    private readonly IParticipantPaymentStateService _stateService;
    private readonly IOrderService _orderService;

    public DevController(PayBySharePayDbContext context, ILastMerchantCallbackStore callbackStore, IConfiguration configuration, IParticipantPaymentStateService stateService, IOrderService orderService)
    {
        _context = context;
        _callbackStore = callbackStore;
        _configuration = configuration;
        _stateService = stateService;
        _orderService = orderService;
    }

    /// <summary>
    /// TEST ONLY – sletter alle ordre, ordrelinjer, betalinger og beskeder.
    /// </summary>
    [HttpDelete("reset")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ResetData()
    {
        _context.MerchantOrders.RemoveRange(_context.MerchantOrders);
        _context.MerchantOrderLines.RemoveRange(_context.MerchantOrderLines);
        _context.MerchantOrderDrafts.RemoveRange(_context.MerchantOrderDrafts);
        _context.ParticipantPayments.RemoveRange(_context.ParticipantPayments);
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
    public async Task<IActionResult> SeedMerchantUrls([FromQuery] string? merchantDemoUrl = null, [FromQuery] string? merchantOrderUrl = null, [FromQuery] bool force = false)
    {
        merchantDemoUrl ??= _configuration["AppSettings:MerchantDemoUrl"] ?? "https://merchant.paynsync.dk/";
        var apiBaseUrl = _configuration["AppSettings:ApiBaseUrl"] ?? "https://localhost:7007";
        merchantOrderUrl ??= $"{apiBaseUrl.TrimEnd('/')}/api/simulated-merchant/orders";

        var merchants = await _context.Participants
            .Where(p => p.Type == ParticipantType.Merchant && (force || p.GroupOrderUrl == null || p.MerchantOrderUrl == null))
            .ToListAsync();

        foreach (var m in merchants)
        {
            m.GroupOrderUrl = merchantDemoUrl;
            m.MerchantOrderUrl = merchantOrderUrl;
        }

        await _context.SaveChangesAsync();
        return Ok(new { updated = merchants.Count, menuUrl = merchantDemoUrl, orderUrl = merchantOrderUrl });
    }

    /// <summary>
    /// TEST ONLY – simulerer Vipps AUTHORIZED-callback for en betaling.
    /// Bruges til at godkende en betaling i testmiljøet uden MobilePay-appen.
    /// </summary>
    [HttpPost("simulate-authorized")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SimulateAuthorized([FromQuery] int participantPaymentId, CancellationToken cancellationToken)
    {
        var payment = await _context.ParticipantPayments.FindAsync([participantPaymentId], cancellationToken);
        if (payment is null)
            return NotFound(new { message = $"Ingen betaling fundet med id {participantPaymentId}" });

        var correlationId = $"dev-simulate-authorized-{participantPaymentId}";
        await _stateService.SetReservedAsync(payment.Id, correlationId, cancellationToken);

        await _orderService.CheckAndSetReadyToPayByReservedAsync(payment.OrderId, cancellationToken);

        return Ok(new { participantPaymentId, orderId = payment.OrderId, status = "Reserved" });
    }

    /// <summary>
    /// DEV/TEST ONLY – returnerer den seneste GroupOrderPaid-payload
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
