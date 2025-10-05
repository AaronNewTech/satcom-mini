namespace Satcom.Api.Domain.Geo;


public readonly record struct GeoPoint(double Lat, double Lon);


public readonly record struct LocationEstimate(GeoPoint Point, double AccuracyKm, DateTime ComputedAtUtc);