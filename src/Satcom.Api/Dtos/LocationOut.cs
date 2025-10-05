namespace Satcom.Api.Dtos;


public record LocationOut(double Lat, double Lon, double AccuracyKm, DateTime ComputedAtUtc);