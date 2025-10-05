using Microsoft.EntityFrameworkCore;
using Satcom.Api.Dtos;
using Satcom.Api.Domain;

namespace Satcom.Api.Endpoints;

public static class TelemetryEndpoints
{
public static RouteGroupBuilder MapTelemetry(this IEndpointRouteBuilder app)
{
var group = app.MapGroup("/v1/telemetry");

group.MapPost("", async (TelemetryIn dto, AppDbContext db) =>
{
// Basic validation
if (dto.ReceivedAtUtc == default) return Results.BadRequest(new { error = "receivedAtUtc required" });


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


db.Telemetries.Add(telemetry);
await db.SaveChangesAsync();
return Results.Created($"/v1/telemetry/{telemetry.Id}", new { id = telemetry.Id });
})
.WithOpenApi();


        group.MapGet("/{id:guid}", async (Guid id, AppDbContext db) =>
        {
            var t = await db.Telemetries.Include(x => x.Satellite).Include(x => x.Station)
    .FirstOrDefaultAsync(x => x.Id == id);
            return t is null ? Results.NotFound() : Results.Ok(t);
        });

    group.MapGet("", async (AppDbContext db) =>
	{
		var telemetry = await db.Telemetries
			.Include(x => x.Satellite)
			.Include(x => x.Station)
			.OrderByDescending(x => x.ReceivedAtUtc)
			.ToListAsync();
		return Results.Ok(telemetry);
	}).WithOpenApi();

    group.MapGet("/satellites", async (Guid stationId, AppDbContext db) =>
    {
        var satellites = await db.Telemetries
            .Where(t => t.StationId == stationId)
            .Include(t => t.Satellite)
            .Select(t => t.Satellite)
            .Where(s => s != null)
            .Distinct()
            .ToListAsync();
        return Results.Ok(satellites);
    }).WithOpenApi();

        return group;
}
}