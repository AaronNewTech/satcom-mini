namespace Satcom.Api.Domain;


public class GroundStation
{
public Guid Id { get; set; } = Guid.NewGuid();
public string Name { get; set; } = string.Empty;
public string Country { get; set; } = string.Empty;
public double Lat { get; set; } // degrees
public double Lon { get; set; } // degrees
public double ElevationM { get; set; }
}