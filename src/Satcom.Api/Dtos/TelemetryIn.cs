namespace Satcom.Api.Dtos;


public record TelemetryIn(
Guid SatelliteId,
Guid StationId,
DateTime ReceivedAtUtc,
double? RssiDbm,
double? ToaMs,
double? BearingDeg,
double? SnrDb);