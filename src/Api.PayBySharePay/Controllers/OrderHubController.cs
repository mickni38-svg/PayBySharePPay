using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.PayBySharePay.DTOs;
using Service.PayBySharePay.Interfaces;

namespace Api.PayBySharePay.Controllers;

[Authorize]
[ApiController]
[Route("api/order-hub")]
public sealed class OrderHubController(IOrderHubService orderHubService) : ControllerBase
{
    [HttpGet("settings")]
    public async Task<ActionResult<OrderHubSettingsDto>> GetSettings(CancellationToken cancellationToken)
    {
        if (!TryGetParticipantId(out var participantId))
            return Unauthorized();

        return Ok(await orderHubService.GetSettingsAsync(participantId, cancellationToken));
    }

    [HttpPut("settings")]
    public async Task<ActionResult<OrderHubSettingsDto>> SetSettings(
        [FromBody] OrderHubSettingsDto request,
        CancellationToken cancellationToken)
    {
        if (!TryGetParticipantId(out var participantId))
            return Unauthorized();

        return Ok(await orderHubService.SetEnabledAsync(participantId, request.Enabled, cancellationToken));
    }

    [HttpGet("orders")]
    public async Task<ActionResult<IReadOnlyList<OrderHubOrderDto>>> GetOrders(CancellationToken cancellationToken)
    {
        if (!TryGetParticipantId(out var participantId))
            return Unauthorized();

        return Ok(await orderHubService.GetActiveOrdersAsync(participantId, cancellationToken));
    }

    [HttpGet("history")]
    public async Task<ActionResult<IReadOnlyList<OrderHubOrderDto>>> GetHistory(CancellationToken cancellationToken)
    {
        if (!TryGetParticipantId(out var participantId))
            return Unauthorized();

        return Ok(await orderHubService.GetHistoryAsync(participantId, cancellationToken));
    }

    [HttpPut("orders/{merchantOrderId:int}/status")]
    public async Task<ActionResult<OrderHubOrderDto>> UpdateStatus(
        int merchantOrderId,
        [FromBody] UpdateOrderHubStatusRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetParticipantId(out var participantId))
            return Unauthorized();

        return Ok(await orderHubService.UpdateStatusAsync(
            participantId,
            merchantOrderId,
            request.Status,
            cancellationToken));
    }

    private bool TryGetParticipantId(out int participantId)
    {
        participantId = 0;
        var value = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        return int.TryParse(value, out participantId) && participantId > 0;
    }
}

public sealed class UpdateOrderHubStatusRequest
{
    public string Status { get; init; } = string.Empty;
}
