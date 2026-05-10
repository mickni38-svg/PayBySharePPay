using Api.PayBySharePay.Auth;
using Api.PayBySharePay.DTOs;
using Microsoft.AspNetCore.Mvc;
using Service.PayBySharePay.DTOs;
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

    [HttpPost("register")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterPersonRequest request)
    {
        var existing = await _participantService.SearchParticipantsAsync(request.Email);
        if (existing.Any(p => string.Equals(p.Email, request.Email, StringComparison.OrdinalIgnoreCase)))
            return Conflict(new { error = "En bruger med denne e-mail eksisterer allerede." });

        var person = await _participantService.CreatePersonAsync(new CreatePersonDto
        {
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone
        });

        var expiresAt = DateTime.UtcNow.AddMinutes(480);
        var token = _tokenService.GenerateToken(person.Id, person.Name);

        return StatusCode(201, new LoginResponse
        {
            Token = token,
            ParticipantId = person.Id,
            Name = person.Name,
            ExpiresAt = expiresAt
        });
    }

    [HttpPost("register-merchant")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegisterMerchant([FromBody] RegisterMerchantRequest request)
    {
        var existing = await _participantService.SearchParticipantsAsync(request.ContactEmail ?? request.Name);
        if (!string.IsNullOrEmpty(request.ContactEmail) &&
            existing.Any(p => string.Equals(p.Email, request.ContactEmail, StringComparison.OrdinalIgnoreCase)))
            return Conflict(new { error = "Et spisested med denne e-mail eksisterer allerede." });

        var merchant = await _participantService.CreateMerchantAsync(new CreateMerchantDto
        {
            Name = request.Name,
            CompanyName = request.CompanyName,
            CvrNumber = request.CvrNumber,
            ContactPerson = request.ContactPerson,
            ContactEmail = request.ContactEmail,
            ContactPhone = request.ContactPhone,
            CompanyAddress = request.CompanyAddress
        });

        var expiresAt = DateTime.UtcNow.AddMinutes(480);
        var token = _tokenService.GenerateToken(merchant.Id, merchant.Name);

        return StatusCode(201, new LoginResponse
        {
            Token = token,
            ParticipantId = merchant.Id,
            Name = merchant.Name,
            ExpiresAt = expiresAt
        });
    }
}
