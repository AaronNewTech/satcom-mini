namespace Satcom.Api.Configuration;

public class ExternalApiOptions
{
    public const string SectionName = "ExternalApi";
    
    /// <summary>
    /// External API key stored in configuration as "ExternalApiKey"
    /// </summary>
    public string ExternalApiKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Returns the API key in the format expected by external requests (lowercase "apiKey")
    /// </summary>
    public Dictionary<string, string> GetRequestHeaders()
    {
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