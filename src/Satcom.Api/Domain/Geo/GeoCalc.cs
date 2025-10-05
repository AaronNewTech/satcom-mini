using Satcom.Api.Domain.Geo;


namespace Satcom.Api.Domain.Geo;


public static class GeoCalc
{
    // Very simple bearings-centroid approximation using station bearings.
    // If insufficient data, returns null.
    public static LocationEstimate? EstimateFromBearings(
    IEnumerable<(GeoPoint station, double bearingDeg)> samples)
    {
        var list = samples.Where(s => !double.IsNaN(s.bearingDeg)).ToList();
        if (list.Count < 2) return null;


        // Convert to unit vectors and average.
        double x = 0, y = 0;
        foreach (var s in list)
        {
            // Bearing degrees clockwise from north; convert to radians east/north vector.
            var rad = s.bearingDeg * Math.PI / 180.0;
            x += Math.Sin(rad);
            y += Math.Cos(rad);
        }
        x /= list.Count; y /= list.Count;


        // Use average vector origin at the centroid of stations for a cheap estimate.
        var lat = list.Average(s => s.station.Lat);
        var lon = list.Average(s => s.station.Lon);


        // Fake accuracy: inversely proportional to vector magnitude.
        var mag = Math.Sqrt(x * x + y * y);
        var accuracyKm = Math.Clamp(50.0 / Math.Max(1e-6, mag), 1.0, 100.0);
        return new LocationEstimate(new GeoPoint(lat, lon), accuracyKm, DateTime.UtcNow);
    }
}
