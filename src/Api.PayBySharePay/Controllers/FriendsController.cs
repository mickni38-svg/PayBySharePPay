using Api.PayBySharePay.DTOs;
using Microsoft.AspNetCore.Mvc;
using Service.PayBySharePay.DTOs;
using Service.PayBySharePay.Interfaces;

namespace Api.PayBySharePay.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FriendsController : ControllerBase
{
    private readonly IParticipantService _participantService;

    public FriendsController(IParticipantService participantService)
    {
        _participantService = participantService;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddFriend([FromBody] AddFriendRequest request)
    {
        var dto = new AddFriendDto
        {
            InitiatorId = request.InitiatorId,
            ReceiverId = request.ReceiverId
        };

        await _participantService.AddFriendAsync(dto);
        return NoContent();
    }
}
