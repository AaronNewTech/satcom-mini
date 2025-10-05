using Satcom.Api.Services;

namespace Satcom.Api.Endpoints;

public static class ExternalApiEndpoints
{
    public static RouteGroupBuilder MapExternalApi(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/v1/external");

        // Test endpoint to demonstrate external API usage
        group.MapGet("/satellite/{noradId}", async (string noradId, IExternalSatelliteService externalService) =>
        {
            var data = await externalService.GetSatelliteDataAsync(noradId);
            
            if (data == null)
            {
                return Results.NotFound(new { message = "Satellite data not found from external API" });
            }

            return Results.Ok(new
            {
                source = "external_api",
                data = data,
                message = "Data retrieved using correctly formatted 'apiKey' header"
            });
        }).WithOpenApi()
          .WithSummary("Get satellite data from external API")
          .WithDescription("Demonstrates using ExternalApiKey with proper 'apiKey' formatting for external requests");

        return group;
    }
}