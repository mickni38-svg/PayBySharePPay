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
}
