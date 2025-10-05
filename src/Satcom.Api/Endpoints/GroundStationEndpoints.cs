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
