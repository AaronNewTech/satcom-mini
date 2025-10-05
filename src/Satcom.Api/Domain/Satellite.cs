namespace Satcom.Api.Domain;


public class Satellite
{
public Guid Id { get; set; } = Guid.NewGuid();
public string Callsign { get; set; } = string.Empty;
public string? NoradId { get; set; }
}