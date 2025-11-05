namespace Satcom.Api.Configuration;

public class ExternalApiOptions
{
    public const string SectionName = "ExternalApi";
    
    /// <summary>
    /// External API key stored in configuration as "ExternalApiKey"
    /// </summary>
    public string ExternalApiKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Base URL for the external API (e.g. https://api.satpositions.example)
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;
    /// <summary>
    /// When true, the external API key will be sent as a query parameter named 'apiKey'
    /// instead of an HTTP header. Some providers (e.g. n2yo) expect the key in the
    /// query string.
    /// </summary>
    public bool SendKeyAsQuery { get; set; } = false;
    
    /// <summary>
    /// Returns the API key in the format expected by external requests (lowercase "apiKey")
    /// </summary>
    public Dictionary<string, string> GetRequestHeaders()
    {
        // If configured to use query param, do not provide headers
        if (SendKeyAsQuery) return new Dictionary<string, string>();

        return new Dictionary<string, string>
        {
            ["apiKey"] = ExternalApiKey  // Convert to lowercase for external API
        };
    }
    
    /// <summary>
    /// Get the API key with the exact casing needed for external requests
    /// </summary>
    public string GetApiKeyForRequests() => ExternalApiKey;
}