using Microsoft.EntityFrameworkCore;
using Satcom.Api.Domain;
using Microsoft.AspNetCore.Builder;

namespace Satcom.Api.Endpoints;

public static class GroundStationEndpoints
{
    public static RouteGroupBuilder MapGroundStations(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/v1/groundstations");

        // List all ground stations
        group.MapGet("", async (AppDbContext db) =>
        {
            var stations = await db.GroundStations
                .OrderBy(gs => gs.Name)
                .ToListAsync();
            return Results.Ok(stations);
        }).WithOpenApi();

        // Get a single ground station by id
        group.MapGet("/{id:guid}", async (Guid id, AppDbContext db) =>
        {
            var station = await db.GroundStations.FindAsync(id);
            if (station == null) return Results.NotFound();
            return Results.Ok(station);
        }).WithOpenApi();

        return group;
    }
}


// change to controller-based implementation for all endpoints then change dir to controllers and Correct. Once you refactor all your API logic into controllers (using classes with [ApiController] and [Route] attributes), you no longer need to use the minimal API endpoint style (like the static GroundStationEndpoints class).

// You can remove the endpoint mapping code and rely on controller routing. Just make sure your controllers are registered in your Program.cs with app.MapControllers() and that you have builder.Services.AddControllers() in your service configuration.

// This approach is standard for enterprise ASP.NET Core projects.


// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using Satcom.Api.Domain;

// namespace Satcom.Api.Controllers;

// [ApiController]
// [Route("v1/groundstations")]
// public class GroundStationsController : ControllerBase
// {
//     private readonly AppDbContext _db;

//     public GroundStationsController(AppDbContext db)
//     {
//         _db = db;
//     }

//     // GET /v1/groundstations
//     [HttpGet]
//     public async Task<IActionResult> GetAll()
//     {
//         var stations = await _db.GroundStations
//             .OrderBy(gs => gs.Name)
//             .ToListAsync();
//         return Ok(stations);
//     }

//     // GET /v1/groundstations/{id}
//     [HttpGet("{id:guid}")]
//     public async Task<IActionResult> GetById(Guid id)
//     {
//         var station = await _db.GroundStations.FindAsync(id);
//         if (station == null) return NotFound();
//         return Ok(station);
//     }
// }