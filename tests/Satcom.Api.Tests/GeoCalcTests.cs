using Satcom.Api.Domain.Geo;
using Xunit;


namespace Satcom.Api.Tests;


public class GeoCalcTests
{
[Fact]
public void ReturnsNullWithInsufficientSamples()
{
var res = GeoCalc.EstimateFromBearings(new List<(GeoPoint,double)>());
Assert.Null(res);
}


[Fact]
public void ProducesEstimateWithTwoBearings()
{
var stationA = new GeoPoint(28.5, -81.3);
var stationB = new GeoPoint(28.6, -81.4);
var res = GeoCalc.EstimateFromBearings(new []
{
(stationA, 45.0),
(stationB, 60.0)
});
Assert.NotNull(res);
Assert.InRange(res!.Value.Point.Lat, 28.4, 28.7);
Assert.InRange(res!.Value.Point.Lon, -81.5, -81.2);
}
}