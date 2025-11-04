using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Satcom.Api.Dtos;
using Satcom.Api.Domain;

namespace Satcom.Api.Controllers;

[ApiController]
[Route("v1/telemetry")]
public class TelemetryController : ControllerBase
{
    private readonly AppDbContext _db;

    public TelemetryController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TelemetryIn dto)
    {
        if (dto.ReceivedAtUtc == default) return BadRequest(new { error = "receivedAtUtc required" });

        var telemetry = new Telemetry
        {
            SatelliteId = dto.SatelliteId,
            StationId = dto.StationId,
            ReceivedAtUtc = DateTime.SpecifyKind(dto.ReceivedAtUtc, DateTimeKind.Utc),
            RssiDbm = dto.RssiDbm,
            ToaMs = dto.ToaMs,
            BearingDeg = dto.BearingDeg,
            SnrDb = dto.SnrDb
        };

        _db.Telemetries.Add(telemetry);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = telemetry.Id }, new { id = telemetry.Id });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var t = await _db.Telemetries.Include(x => x.Satellite).Include(x => x.Station)
            .FirstOrDefaultAsync(x => x.Id == id);
        return t is null ? NotFound() : Ok(t);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var telemetry = await _db.Telemetries
            .Include(x => x.Satellite)
            .Include(x => x.Station)
            .OrderByDescending(x => x.ReceivedAtUtc)
            .ToListAsync();
        return Ok(telemetry);
    }

    [HttpGet("satellites")]
    public async Task<IActionResult> GetSatellites([FromQuery] Guid stationId)
    {
        var satellites = await _db.Telemetries
            .Where(t => t.StationId == stationId)
            .Include(t => t.Satellite)
            .Select(t => t.Satellite)
            .Where(s => s != null)
            .Distinct()
            .ToListAsync();
        return Ok(satellites);
    }
}
