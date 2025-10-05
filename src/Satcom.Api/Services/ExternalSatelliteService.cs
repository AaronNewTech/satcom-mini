using Microsoft.Extensions.Options;
using Satcom.Api.Configuration;
using System.Text.Json;

namespace Satcom.Api.Services;

public interface IExternalSatelliteService
{
    Task<ExternalSatelliteData?> GetSatelliteDataAsync(string noradId);
}

public class ExternalSatelliteService : IExternalSatelliteService
{
    private readonly HttpClient _httpClient;
    private readonly ExternalApiOptions _apiOptions;
    private readonly ILogger<ExternalSatelliteService> _logger;

    public ExternalSatelliteService(
        HttpClient httpClient, 
        IOptions<ExternalApiOptions> apiOptions,
        ILogger<ExternalSatelliteService> logger)
    {
        _httpClient = httpClient;
        _apiOptions = apiOptions.Value;
        _logger = logger;
    }

    public async Task<ExternalSatelliteData?> GetSatelliteDataAsync(string noradId)
    {
        try
        {
            // Example external API call with correctly formatted apiKey
            var requestUrl = $"https://api.example.com/satellites/{noradId}";
            
            // Use the configuration to get properly formatted headers
            var headers = _apiOptions.GetRequestHeaders();
            
            // Add the apiKey header in the format the external API expects
            foreach (var header in headers)
            {
                _httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
            }

            _logger.LogInformation($"Making external API request with apiKey: {_apiOptions.GetApiKeyForRequests()}");
            
            var response = await _httpClient.GetAsync(requestUrl);
            
            if (response.IsSuccessStatusCode)
            {
                var jsonContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ExternalSatelliteData>(jsonContent);
            }
            
            _logger.LogWarning($"External API request failed: {response.StatusCode}");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling external satellite API");
            return null;
        }
    }
}

public record ExternalSatelliteData(
    string NoradId,
    string Name,
    double Latitude,
    double Longitude,
    DateTime LastUpdate
);