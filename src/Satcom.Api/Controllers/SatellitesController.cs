using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Satcom.Api.Dtos;
using Satcom.Api.Domain;
using Satcom.Api.Services;
using Satcom.Api.Domain.Geo;

namespace Satcom.Api.Controllers;

[ApiController]
[Route("v1/satellites")]
public class SatellitesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IExternalSatelliteService? _externalService;

    public SatellitesController(AppDbContext db, IExternalSatelliteService? externalService = null)
    {
        _db = db;
        _externalService = externalService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var sats = await _db.Satellites
            .OrderBy(s => s.Callsign)
            .ToListAsync();
        return Ok(sats);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var sat = await _db.Satellites.FindAsync(id);
        if (sat == null) return NotFound();
        return Ok(sat);
    }

    // The previous "location" endpoint relied on bearing-based triangulation
    // and was not fully wired for this deployment. It has been removed to
    // avoid exposing an incomplete feature. Use the external positions
    // proxy (`/v1/external/positions/...`) to retrieve satellite positions
    // (NORAD-based) from the configured external API instead.

    [HttpGet("{id:guid}/observations")]
    public async Task<IActionResult> GetObservations(Guid id, [FromQuery] int limit = 50)
    {
        limit = Math.Clamp(limit == 0 ? 50 : limit, 1, 500);
        var rows = await _db.Telemetries
            .Where(t => t.SatelliteId == id)
            .OrderByDescending(t => t.ReceivedAtUtc)
            .Take(limit)
            .Select(t => new { t.ReceivedAtUtc, t.RssiDbm, t.BearingDeg, t.StationId })
            .ToListAsync();
        return Ok(rows);
    }

    // Local convenience route that mirrors the external provider's /above endpoint.
    // This uses an absolute route so callers can hit /v1/satellite/above/...
    [HttpGet("/v1/satellite/above/{observerLat}/{observerLng}/{observerAlt}/{searchRadius}/{categoryId}")]
    public async Task<IActionResult> Above(double observerLat, double observerLng, double observerAlt, int searchRadius, int categoryId)
    {
        if (_externalService == null)
            return StatusCode(501, new { error = "External satellite service not configured" });

        if (searchRadius < 0 || searchRadius > 90) return BadRequest(new { error = "searchRadius must be between 0 and 90" });

        var json = await _externalService.GetSatellitesAboveAsync(observerLat, observerLng, observerAlt, searchRadius, categoryId);
        if (json == null) return NotFound(new { message = "No data returned from external API" });

        return Content(json, "application/json");
    }
}
