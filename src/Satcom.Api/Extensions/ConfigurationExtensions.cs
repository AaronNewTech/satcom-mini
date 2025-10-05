namespace Satcom.Api.Extensions;

public static class ConfigurationExtensions
{
    /// <summary>
    /// Gets the external API key formatted for HTTP requests (lowercase "apiKey")
    /// </summary>
    public static string GetExternalApiKeyForRequests(this IConfiguration configuration)
    {
        return configuration["ExternalApiKey"] ?? string.Empty;
    }

    /// <summary>
    /// Gets HTTP headers formatted for external API requests
    /// </summary>
    public static Dictionary<string, string> GetExternalApiHeaders(this IConfiguration configuration)
    {
        return new Dictionary<string, string>
        {
            ["apiKey"] = configuration.GetExternalApiKeyForRequests()  // Lowercase for external API
        };
    }
}