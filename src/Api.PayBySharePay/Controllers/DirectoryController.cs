using Microsoft.AspNetCore.Mvc;
using Service.PayBySharePay.DTOs;
using Service.PayBySharePay.Interfaces;

namespace Api.PayBySharePay.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DirectoryController : ControllerBase
{
    private readonly IDirectoryService _directoryService;

    public DirectoryController(IDirectoryService directoryService)
    {
        _directoryService = directoryService;
    }

    /// <summary>Søg i directory – returnerer både brugere og merchants</summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<DirectoryEntryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromQuery] string? query, [FromQuery] int? excludeFriendsOf)
    {
        var results = await _directoryService.SearchAsync(query ?? string.Empty, excludeFriendsOf);
        return Ok(results);
    }
}
