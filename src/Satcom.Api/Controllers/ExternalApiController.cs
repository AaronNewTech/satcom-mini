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

    [HttpGet("positions/{noradId}/{observerLat}/{observerLng}/{observerAlt}/{seconds}")]
    public async Task<IActionResult> GetPositions(int noradId, double observerLat, double observerLng, double observerAlt, int seconds)
    {
        if (seconds < 1 || seconds > 300) return BadRequest(new { error = "seconds must be between 1 and 300" });

        var resp = await _externalService.GetSatellitePositionsAsync(noradId, observerLat, observerLng, observerAlt, seconds);
        if (resp == null) return NotFound(new { message = "No positions returned from external API" });

        // Return the wrapper response (info + positions) to the caller unchanged
        return Ok(resp);
    }

    [HttpGet("above/{observerLat}/{observerLng}/{observerAlt}/{searchRadius}/{categoryId}")]
    public async Task<IActionResult> Above(double observerLat, double observerLng, double observerAlt, int searchRadius, int categoryId)
    {
        // Basic validation
        if (searchRadius < 0 || searchRadius > 90) return BadRequest(new { error = "searchRadius must be between 0 and 90" });

        var json = await _externalService.GetSatellitesAboveAsync(observerLat, observerLng, observerAlt, searchRadius, categoryId);
        if (json == null) return NotFound(new { message = "No data returned from external API" });

        // Return the provider's JSON body unchanged
        return Content(json, "application/json");
    }
}
