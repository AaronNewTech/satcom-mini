using Microsoft.AspNetCore.Mvc;
using Satcom.Api.Services;

namespace Satcom.Api.Controllers;

[ApiController]
[Route("v1/external")]
public class ExternalApiController : ControllerBase
{
    private readonly IExternalSatelliteService _externalService;

    public ExternalApiController(IExternalSatelliteService externalService)
    {
        _externalService = externalService;
    }

    [HttpGet("satellite/{noradId}")]
    public async Task<IActionResult> GetSatellite(string noradId)
    {
        var data = await _externalService.GetSatelliteDataAsync(noradId);
        if (data == null)
            return NotFound(new { message = "Satellite data not found from external API" });

        return Ok(new
        {
            source = "external_api",
            data = data,
            message = "Data retrieved using correctly formatted 'apiKey' header"
        });
    }
}
