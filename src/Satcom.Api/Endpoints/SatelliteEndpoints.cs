using Microsoft.EntityFrameworkCore;
using Satcom.Api.Dtos;
using Satcom.Api.Domain.Geo;


namespace Satcom.Api.Endpoints;


public static class SatelliteEndpoints
{

public static RouteGroupBuilder MapSatellites(this IEndpointRouteBuilder app)
{
	var group = app.MapGroup("/v1/satellites");

	// List all satellites
	group.MapGet("", async (AppDbContext db) =>
	{
		var sats = await db.Satellites
			.OrderBy(s => s.Callsign)
			.ToListAsync();
		return Results.Ok(sats);
	}).WithOpenApi();

	// Get a single satellite by id
	group.MapGet("/{id:guid}", async (Guid id, AppDbContext db) =>
	{
		var sat = await db.Satellites.FindAsync(id);
		if (sat == null) return Results.NotFound();
		return Results.Ok(sat);
	}).WithOpenApi();


// The legacy /location endpoint (bearing-based triangulation) has been
// removed from the public API surface. Consumers should use the external
// positions proxy at `/v1/external/positions/{noradId}/{observerLat}/{observerLng}/{observerAlt}/{seconds}`
// which fetches per-second position, azimuth and elevation data from the
// configured external provider.


group.MapGet("/{id:guid}/observations", async (Guid id, int limit, AppDbContext db) =>
{
limit = Math.Clamp(limit == 0 ? 50 : limit, 1, 500);
var rows = await db.Telemetries
.Where(t => t.SatelliteId == id)
.OrderByDescending(t => t.ReceivedAtUtc)
.Take(limit)
.Select(t => new { t.ReceivedAtUtc, t.RssiDbm, t.BearingDeg, t.StationId })
.ToListAsync();
return Results.Ok(rows);
});


return group;
}
}