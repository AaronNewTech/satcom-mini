namespace Satcom.Api.Domain;


public class Telemetry
{
public Guid Id { get; set; } = Guid.NewGuid();
public Guid SatelliteId { get; set; }
public Satellite? Satellite { get; set; }
public Guid StationId { get; set; }
public GroundStation? Station { get; set; }
public DateTime ReceivedAtUtc { get; set; }
public double? RssiDbm { get; set; }
public double? ToaMs { get; set; }
public double? BearingDeg { get; set; }
public double? SnrDb { get; set; }
}