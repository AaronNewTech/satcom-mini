using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Satcom.Api.Dtos;
using Satcom.Api.Domain;
using Satcom.Api.Domain.Geo;

namespace Satcom.Api.Controllers;

[ApiController]
[Route("v1/satellites")]
public class SatellitesController : ControllerBase
{
    private readonly AppDbContext _db;

    public SatellitesController(AppDbContext db)
    {
        _db = db;
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

    [HttpGet("{id:guid}/location")]
    public async Task<IActionResult> GetLocation(Guid id)
    {
        var latest = await _db.Telemetries
            .Where(t => t.SatelliteId == id)
            .OrderByDescending(t => t.ReceivedAtUtc)
            .Take(10)
            .Select(t => new { t.BearingDeg, t.Station!.Lat, t.Station!.Lon })
            .ToListAsync();

        if (latest.Count < 2) return Ok(null);

        var estimate = GeoCalc.EstimateFromBearings(
            latest.Where(x => x.BearingDeg.HasValue)
                  .Select(x => (new GeoPoint(x.Lat, x.Lon), x.BearingDeg!.Value))
        );

        if (estimate is null) return Ok(null);
        var e = estimate.Value;
        return Ok(new LocationOut(e.Point.Lat, e.Point.Lon, e.AccuracyKm, e.ComputedAtUtc));
    }

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
}
