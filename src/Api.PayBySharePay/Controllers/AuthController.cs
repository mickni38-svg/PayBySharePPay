using Api.PayBySharePay.Auth;
using Api.PayBySharePay.DTOs;
using Microsoft.AspNetCore.Mvc;
using Service.PayBySharePay;
using Service.PayBySharePay.DTOs;
using Service.PayBySharePay.Interfaces;

namespace Api.PayBySharePay.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IParticipantService _participantService;
    private readonly JwtTokenService _tokenService;
    private readonly IExternalAuthService _externalAuthService;
    private readonly IWebHostEnvironment _environment;

    public AuthController(
        IParticipantService participantService,
        JwtTokenService tokenService,
        IExternalAuthService externalAuthService,
        IWebHostEnvironment environment)
    {
        _participantService = participantService;
        _tokenService = tokenService;
        _externalAuthService = externalAuthService;
        _environment = environment;
    }

    /// <summary>
    /// Logger Person eller Merchant ind med email og password.
    /// Passwordløst login er kun tilladt for legacy seed-personer i Development.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var participant = await _participantService.GetByEmailAsync(request.Email.Trim());
        if (participant is null)
            return Unauthorized(new { error = "Email eller adgangskode er forkert." });

        var passwordMissing = string.IsNullOrWhiteSpace(request.Password);
        var isDevelopmentSeedLogin =
            _environment.IsDevelopment() &&
            participant.Type == "Person" &&
            participant.PasswordHash is null;

        if (passwordMissing)
        {
            if (!isDevelopmentSeedLogin)
                return Unauthorized(new { error = "Email eller adgangskode er forkert." });
        }
        else if (participant.PasswordHash is null ||
                 !_participantService.VerifyPassword(request.Password!, participant.PasswordHash))
        {
            return Unauthorized(new { error = "Email eller adgangskode er forkert." });
        }

        var expiresAt = DateTime.UtcNow.AddMinutes(480);
        var token = _tokenService.GenerateToken(participant.Id, participant.Name);

        return Ok(new LoginResponse
        {
            Token = token,
            ParticipantId = participant.Id,
            Name = participant.Name,
            ParticipantType = participant.Type,
            ExpiresAt = expiresAt
        });
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterPersonRequest request)
    {
        var existing = await _participantService.GetByEmailAsync(request.Email.Trim());
        if (existing is not null)
            return Conflict(new { error = "En konto med denne e-mail eksisterer allerede." });

        var person = await _participantService.CreatePersonAsync(new CreatePersonDto
        {
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            Password = request.Password
        });

        var expiresAt = DateTime.UtcNow.AddMinutes(480);
        var token = _tokenService.GenerateToken(person.Id, person.Name);

        return StatusCode(201, new LoginResponse
        {
            Token = token,
            ParticipantId = person.Id,
            Name = person.Name,
            ParticipantType = person.Type,
            ExpiresAt = expiresAt
        });
    }

    [HttpPost("register-merchant")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegisterMerchant([FromBody] RegisterMerchantRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.VippsMerchantSerialNumber))
            return BadRequest(new { error = "MSN-nummer (Vipps Merchant Serial Number) er påkrævet." });

        var existing = await _participantService.GetByEmailAsync(request.Email.Trim());
        if (existing is not null)
            return Conflict(new { error = "En konto med denne e-mail eksisterer allerede." });

        var merchant = await _participantService.CreateMerchantAsync(new CreateMerchantDto
        {
            Name = request.Name,
            CompanyName = request.CompanyName,
            Email = request.Email,
            Password = request.Password,
            VippsMerchantSerialNumber = request.VippsMerchantSerialNumber,
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
            ParticipantType = merchant.Type,
            ExpiresAt = expiresAt
        });
    }

    [HttpPost("google-login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> GoogleLogin([FromBody] ExternalLoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.IdToken))
            return BadRequest(new { error = "IdToken må ikke være tomt." });

        try
        {
            var person = await _externalAuthService.GoogleLoginAsync(request.IdToken);
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
        catch (ExternalLoginEmailConflictException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
