using Microsoft.Extensions.Options;
using Satcom.Api.Configuration;
using System.Text.Json;

namespace Satcom.Api.Services;

public interface IExternalSatelliteService
{
    Task<ExternalSatelliteData?> GetSatelliteDataAsync(string noradId);
    // Return the full positions response (info + positions) to mirror the external API shape
    Task<ExternalPositionsResponse?> GetSatellitePositionsAsync(int noradId, double observerLat, double observerLng, double observerAlt, int seconds);
    // Return raw JSON for the 'above' query (provider wrapper)
    Task<string?> GetSatellitesAboveAsync(double observerLat, double observerLng, double observerAlt, int searchRadius, int categoryId);
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
            // Use the configured base address + relative path
            var requestUrl = $"/satellites/{noradId}";

            // Determine absolute URL
            var clientBase = _httpClient.BaseAddress?.ToString().TrimEnd();
            var configuredBase = _apiOptions.BaseUrl?.TrimEnd();
            string baseAddr;
            if (!string.IsNullOrEmpty(clientBase)) baseAddr = clientBase.TrimEnd('/');
            else if (!string.IsNullOrEmpty(configuredBase)) baseAddr = configuredBase.TrimEnd('/');
            else
            {
                _logger.LogError("External API BaseUrl is not configured. Set 'ExternalApi:BaseUrl' or configure HttpClient BaseAddress.");
                return null;
            }

            var absoluteUrl = baseAddr + requestUrl;

            // If configured to send key as query param, append it and do not set header
            if (_apiOptions.SendKeyAsQuery)
            {
                var sep = absoluteUrl.Contains('?') ? '&' : '?';
                absoluteUrl = absoluteUrl + sep + "apiKey=" + Uri.EscapeDataString(_apiOptions.ExternalApiKey ?? string.Empty);
                _logger.LogInformation("External satellite data request URL (with apiKey in query): {Url}", absoluteUrl);
                using var req = new HttpRequestMessage(HttpMethod.Get, absoluteUrl);
                var response = await _httpClient.SendAsync(req);
                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<ExternalSatelliteData>(jsonContent);
                }

                _logger.LogWarning("External API request failed: {Status}", response.StatusCode);
                return null;
            }

            // Otherwise send API key in headers
            _logger.LogInformation("External satellite request URL: {Url}", absoluteUrl);
            using var req2 = new HttpRequestMessage(HttpMethod.Get, absoluteUrl);
            foreach (var header in _apiOptions.GetRequestHeaders())
            {
                req2.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            _logger.LogInformation("Making external API request for satellite {NoradId}", noradId);
            var response2 = await _httpClient.SendAsync(req2);
            if (response2.IsSuccessStatusCode)
            {
                var jsonContent = await response2.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ExternalSatelliteData>(jsonContent);
            }

            _logger.LogWarning("External API request failed: {Status}", response2.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling external satellite API");
            return null;
        }
    }

    public async Task<ExternalPositionsResponse?> GetSatellitePositionsAsync(int noradId, double observerLat, double observerLng, double observerAlt, int seconds)
    {
        try
        {
            // Build relative URL according to external API docs: /positions/{id}/{observer_lat}/{observer_lng}/{observer_alt}/{seconds}
            var requestUrl = $"/positions/{noradId}/{observerLat}/{observerLng}/{observerAlt}/{seconds}";

            // Determine an absolute URL to call. Prefer HttpClient.BaseAddress; fall back
            // to configured ExternalApiOptions.BaseUrl. If neither is set, return a helpful error.
            var clientBase = _httpClient.BaseAddress?.ToString().TrimEnd();
            var configuredBase = _apiOptions.BaseUrl?.TrimEnd();
            string baseAddr;
            if (!string.IsNullOrEmpty(clientBase)) baseAddr = clientBase.TrimEnd('/');
            else if (!string.IsNullOrEmpty(configuredBase)) baseAddr = configuredBase.TrimEnd('/');
            else
            {
                _logger.LogError("External API BaseUrl is not configured. Set 'ExternalApi:BaseUrl' or configure HttpClient BaseAddress.");
                return null;
            }

            var absoluteUrl = baseAddr + requestUrl;

            // If configured to send key as query param, append it and do not set header
            if (_apiOptions.SendKeyAsQuery)
            {
                var sep = absoluteUrl.Contains('?') ? '&' : '?';
                absoluteUrl = absoluteUrl + sep + "apiKey=" + Uri.EscapeDataString(_apiOptions.ExternalApiKey ?? string.Empty);
                _logger.LogInformation("External positions request URL (with apiKey in query): {Url}", absoluteUrl);
            }
            else
            {
                _logger.LogInformation("External positions request URL: {Url}", absoluteUrl);
            }

            using var req = new HttpRequestMessage(HttpMethod.Get, absoluteUrl);

            if (!_apiOptions.SendKeyAsQuery)
            {
                foreach (var header in _apiOptions.GetRequestHeaders())
                {
                    // Mask header values for safe logging (show only a short prefix)
                    var masked = string.IsNullOrEmpty(header.Value) ? "(empty)" : (header.Value.Length > 4 ? header.Value.Substring(0, 4) + "***" : "***");
                    _logger.LogInformation("External request header {Header}={Value}", header.Key, masked);
                    req.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            _logger.LogInformation("Requesting positions from external API for {NoradId} observer={Lat},{Lng},{Alt} seconds={Seconds}", noradId, observerLat, observerLng, observerAlt, seconds);

            var response = await _httpClient.SendAsync(req);
            var body = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("External positions request failed: {Status}. Response: {Content}", response.StatusCode, body);
                return null;
            }

            // Log the raw response body to help diagnose why info/positions may be null
            _logger.LogInformation("External positions response body: {Body}", body);

            // External API returns a wrapper object: { "info": { ... }, "positions": [ ... ] }
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var wrapper = JsonSerializer.Deserialize<ExternalPositionsResponse>(body, options);
            return wrapper;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching satellite positions");
            return null;
        }
    }

    public async Task<string?> GetSatellitesAboveAsync(double observerLat, double observerLng, double observerAlt, int searchRadius, int categoryId)
    {
        try
        {
            var requestUrl = $"/above/{observerLat}/{observerLng}/{observerAlt}/{searchRadius}/{categoryId}";

            // Determine an absolute URL to call. Prefer HttpClient.BaseAddress; fall back
            // to configured ExternalApiOptions.BaseUrl. If neither is set, return a helpful error.
            var clientBase = _httpClient.BaseAddress?.ToString().TrimEnd();
            var configuredBase = _apiOptions.BaseUrl?.TrimEnd();
            string baseAddr;
            if (!string.IsNullOrEmpty(clientBase)) baseAddr = clientBase.TrimEnd('/');
            else if (!string.IsNullOrEmpty(configuredBase)) baseAddr = configuredBase.TrimEnd('/');
            else
            {
                _logger.LogError("External API BaseUrl is not configured. Set 'ExternalApi:BaseUrl' or configure HttpClient BaseAddress.");
                return null;
            }

            var absoluteUrl = baseAddr + requestUrl;

            // If configured to send key as query param, append it and do not set header
            if (_apiOptions.SendKeyAsQuery)
            {
                var sep = absoluteUrl.Contains('?') ? '&' : '?';
                absoluteUrl = absoluteUrl + sep + "apiKey=" + Uri.EscapeDataString(_apiOptions.ExternalApiKey ?? string.Empty);
                _logger.LogInformation("External 'above' request URL (with apiKey in query): {Url}", absoluteUrl);
            }
            else
            {
                _logger.LogInformation("External 'above' request URL: {Url}", absoluteUrl);
            }

            using var req = new HttpRequestMessage(HttpMethod.Get, absoluteUrl);

            if (!_apiOptions.SendKeyAsQuery)
            {
                foreach (var header in _apiOptions.GetRequestHeaders())
                {
                    req.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            _logger.LogInformation("Requesting 'above' from external API observer={Lat},{Lng},{Alt} radius={Radius} category={Category}", observerLat, observerLng, observerAlt, searchRadius, categoryId);

            var response = await _httpClient.SendAsync(req);
            var body = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("External 'above' request failed: {Status}. Response: {Content}", response.StatusCode, body);
                return null;
            }

            _logger.LogInformation("External 'above' response body: {Body}", body);
            return body;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching 'above' satellites");
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

public record ExternalPositionsResponse(ExternalInfo info, ExternalPositionElement[] positions);

public record ExternalInfo(int satid, string satname, int transactionscount);

public record ExternalPositionElement(
    double satlatitude,
    double satlongitude,
    double sataltitude,
    double azimuth,
    double elevation,
    double ra,
    double dec,
    long timestamp
);