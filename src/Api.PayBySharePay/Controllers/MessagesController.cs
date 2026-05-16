using Api.PayBySharePay.DTOs;
using Microsoft.AspNetCore.Mvc;
using Service.PayBySharePay.DTOs;
using Service.PayBySharePay.Interfaces;

namespace Api.PayBySharePay.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessagesController : ControllerBase
{
    private readonly IMessageService _messageService;

    public MessagesController(IMessageService messageService)
    {
        _messageService = messageService;
    }

    [HttpGet("order/{orderId}")]
    [ProducesResponseType(typeof(IEnumerable<MessageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByOrder(int orderId)
    {
        var results = await _messageService.GetMessagesByOrderAsync(orderId);
        return Ok(results);
    }

    /// <summary>Henter alle beskeder for en deltager</summary>
    [HttpGet("by-participant/{participantId}")]
    [ProducesResponseType(typeof(IEnumerable<MessageDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByParticipant(int participantId)
    {
        var results = await _messageService.GetByParticipantAsync(participantId);
        return Ok(results);
    }

    /// <summary>Antal ulæste beskeder for en deltager</summary>
    [HttpGet("unread-count")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUnreadCount([FromQuery] int participantId)
    {
        var count = await _messageService.GetUnreadCountAsync(participantId);
        return Ok(count);
    }

    /// <summary>Markér alle beskeder som læst for en deltager</summary>
    [HttpPost("mark-read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarkAllRead([FromQuery] int participantId)
    {
        await _messageService.MarkAllReadAsync(participantId);
        return NoContent();
    }

    [HttpPost]
    [ProducesResponseType(typeof(MessageDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateMessage([FromBody] CreateMessageRequest request)
    {
        var dto = new CreateMessageDto
        {
            OrderId = request.OrderId,
            ParticipantId = request.ParticipantId,
            Content = request.Content
        };

        var result = await _messageService.CreateMessageAsync(dto);
        return StatusCode(StatusCodes.Status201Created, result);
    }
}
