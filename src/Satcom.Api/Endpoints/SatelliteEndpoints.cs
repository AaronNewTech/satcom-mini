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


group.MapGet("/{id:guid}/location", async (Guid id, AppDbContext db) =>
{
// Grab last N observations across stations
var latest = await db.Telemetries
.Where(t => t.SatelliteId == id)
.OrderByDescending(t => t.ReceivedAtUtc)
.Take(10)
.Select(t => new { t.BearingDeg, t.Station!.Lat, t.Station!.Lon })
.ToListAsync();


if (latest.Count < 2) return Results.Ok(null);


var estimate = GeoCalc.EstimateFromBearings(
latest.Where(x => x.BearingDeg.HasValue)
.Select(x => (new GeoPoint(x.Lat, x.Lon), x.BearingDeg!.Value))
);


if (estimate is null) return Results.Ok(null);
var e = estimate.Value;
return Results.Ok(new LocationOut(e.Point.Lat, e.Point.Lon, e.AccuracyKm, e.ComputedAtUtc));
})
.WithOpenApi();


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