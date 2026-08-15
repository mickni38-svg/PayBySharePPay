using Api.PayBySharePay.DTOs;
using Microsoft.AspNetCore.Mvc;
using Service.PayBySharePay.DTOs;
using Service.PayBySharePay.Interfaces;

namespace Api.PayBySharePay.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ParticipantsController : ControllerBase
{
    private readonly IParticipantService _participantService;

    public ParticipantsController(IParticipantService participantService)
    {
        _participantService = participantService;
    }

    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<ParticipantDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromQuery] string? query, [FromQuery] int? initiatorId = null)
    {
        var results = await _participantService.SearchParticipantsAsync(query ?? string.Empty, initiatorId);
        return Ok(results);
    }

    [HttpPost("person")]
    [ProducesResponseType(typeof(ParticipantDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePerson([FromBody] CreatePersonRequest request)
    {
        var dto = new CreatePersonDto
        {
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone
        };

        var result = await _participantService.CreatePersonAsync(dto);
        return CreatedAtAction(nameof(Search), new { query = result.Name }, result);
    }

    [HttpPost("merchant")]
    [ProducesResponseType(typeof(ParticipantDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateMerchant([FromBody] CreateMerchantRequest request)
    {
        var dto = new CreateMerchantDto
        {
            Name = request.Name,
            CompanyName = request.CompanyName,
            CvrNumber = request.CvrNumber,
            VatNumber = request.VatNumber,
            ContactPerson = request.ContactPerson,
            ContactEmail = request.ContactEmail,
            ContactPhone = request.ContactPhone,
            CompanyAddress = request.CompanyAddress,
            PaymentReference = request.PaymentReference,
            PayoutAccountInfo = request.PayoutAccountInfo,
            PaymentProvider = request.PaymentProvider
        };

        var result = await _participantService.CreateMerchantAsync(dto);
        return CreatedAtAction(nameof(Search), new { query = result.Name }, result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ParticipantDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _participantService.GetByIdAsync(id);
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpPut("{id:int}/profile")]
    [ProducesResponseType(typeof(ParticipantDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProfile(int id, [FromBody] UpdateProfileRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { error = "Navn må ikke være tomt." });

        var dto = new UpdateProfileDto
        {
            Id = id,
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone
        };

        var result = await _participantService.UpdateProfileAsync(dto);
        return Ok(result);
    }

    [HttpGet("vipps-test-users")]
    [ProducesResponseType(typeof(IEnumerable<Service.PayBySharePay.DTOs.VippsTestPersonDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVippsTestUsers()
    {
        var result = await _participantService.GetVippsTestPersonsAsync();
        return Ok(result);
    }

    [HttpPatch("{id:int}/vipps-test-user")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetVippsTestUser(int id, [FromBody] SetVippsTestUserRequest request)
    {
        try
        {
            await _participantService.SetVippsTestUserAsync(id, request.VippsTestUserId);
            return NoContent();
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    [HttpGet("{id:int}/logo")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMerchantLogo(int id)
    {
        var logo = await _participantService.GetMerchantLogoAsync(id);
        if (logo is null)
            return NotFound();

        var etag = logo.UpdatedAtUtc?.Ticks.ToString() ?? "0";
        Response.Headers.ETag = $"\"{etag}\"";
        Response.Headers.CacheControl = "public, max-age=3600";

        return File(logo.ImageData, logo.ContentType, logo.FileName);
    }

    [HttpPut("{id:int}/logo")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMerchantLogo(int id, IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { error = "Fil må ikke være tom." });

        const long maxBytes = 1 * 1024 * 1024;
        if (file.Length > maxBytes)
            return BadRequest(new { error = "Filen må højst være 1 MB." });

        var allowedTypes = new[] { "image/png", "image/jpeg", "image/webp" };
        if (!allowedTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
            return BadRequest(new { error = "Kun PNG, JPEG og WebP er tilladt." });

        using var ms = new System.IO.MemoryStream();
        await file.CopyToAsync(ms);

        var dto = new Service.PayBySharePay.DTOs.UpdateMerchantLogoDto
        {
            ImageData = ms.ToArray(),
            ContentType = file.ContentType,
            FileName = file.FileName
        };

        try
        {
            await _participantService.UpdateMerchantLogoAsync(id, dto);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
