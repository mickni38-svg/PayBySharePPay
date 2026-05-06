using Api.PayBySharePay.Auth;
using Api.PayBySharePay.DTOs;
using Microsoft.AspNetCore.Mvc;
using Service.PayBySharePay.Interfaces;

namespace Api.PayBySharePay.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IParticipantService _participantService;
    private readonly JwtTokenService _tokenService;

    public AuthController(IParticipantService participantService, JwtTokenService tokenService)
    {
        _participantService = participantService;
        _tokenService = tokenService;
    }

    /// <summary>
    /// MVP login: slår email op i Participants og returnerer JWT.
    /// Ingen password endnu — erstattes med rigtig auth i Later Phase.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var participants = await _participantService.SearchParticipantsAsync(request.Email);
        var person = participants.FirstOrDefault(p =>
            p.Type == "Person" &&
            string.Equals(p.Email, request.Email, StringComparison.OrdinalIgnoreCase));

        if (person is null)
            return Unauthorized(new { error = "Ingen bruger fundet med denne email." });

        var expiresAt = DateTime.UtcNow.AddMinutes(480);
        var token = _tokenService.GenerateToken(person.Id, person.Name);

        return Ok(new LoginResponse
        {
            Token = token,
            ParticipantId = person.Id,
            Name = person.Name,
            ExpiresAt = expiresAt
        });
    }
}
