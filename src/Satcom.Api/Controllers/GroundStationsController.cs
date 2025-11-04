using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Satcom.Api.Domain;

namespace Satcom.Api.Controllers;

[ApiController]
[Route("v1/groundstations")]
public class GroundStationsController : ControllerBase
{
    private readonly AppDbContext _db;

    public GroundStationsController(AppDbContext db)
    {
        _db = db;
    }

    // GET /v1/groundstations
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var stations = await _db.GroundStations
            .OrderBy(gs => gs.Name)
            .ToListAsync();
        return Ok(stations);
    }

    // GET /v1/groundstations/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var station = await _db.GroundStations.FindAsync(id);
        if (station == null) return NotFound();
        return Ok(station);
    }
}
