using System.Security.Cryptography;
using System.Text;

namespace Satcom.Api.Security;

/// <summary>
/// HMAC request signing middleware.
/// Clients include these headers:
/// - x-timestamp: unix seconds
/// - x-signature: base64(hmacSHA256(secret, canonicalString))
/// canonicalString = METHOD + "\n" + PATH + "\n" + TIMESTAMP + "\n" + BODY_SHA256_HEX
/// If the signature validates, middleware sets HttpContext.Items["HmacAuthenticated"] = true
/// and the ApiKey middleware is skipped.
/// </summary>
public class HmacMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _config;
    private readonly ILogger<HmacMiddleware> _logger;

    private const string SignatureHeader = "x-signature";
    private const string TimestampHeader = "x-timestamp";
    private const string AuthFlagKey = "HmacAuthenticated";

    public HmacMiddleware(RequestDelegate next, IConfiguration config, ILogger<HmacMiddleware> logger)
    {
        _next = next;
        _config = config;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Bypass for swagger/health/favicon like existing middleware
        var path = context.Request.Path;
        if (path.StartsWithSegments("/swagger") || path.StartsWithSegments("/health") || path.StartsWithSegments("/healthz") || path.StartsWithSegments("/favicon.ico"))
        {
            await _next(context);
            return;
        }

        var secret = _config["ApiSigningSecret"] ?? _config["ApiSigning:Secret"] ?? string.Empty;
        if (string.IsNullOrEmpty(secret))
        {
            // Not configured, skip HMAC checks
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(SignatureHeader, out var sigValues))
        {
            // No signature provided — continue and let ApiKey middleware handle authentication
            await _next(context);
            return;
        }

        var providedSig = sigValues.FirstOrDefault() ?? string.Empty;
        context.Request.Headers.TryGetValue(TimestampHeader, out var tsValues);
        var tsString = tsValues.FirstOrDefault() ?? string.Empty;

        if (!long.TryParse(tsString, out var timestamp))
        {
            _logger.LogWarning("HMAC signature present but timestamp missing or invalid");
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid timestamp" });
            return;
        }

        // Check clock skew
        var skewSeconds = 300; // default 5 minutes
        var cfgSkew = _config["ApiSigning:SkewSeconds"] ?? _config["ApiSigningSkewSeconds"];
        if (!string.IsNullOrEmpty(cfgSkew) && int.TryParse(cfgSkew, out var n)) skewSeconds = n;

        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (Math.Abs(now - timestamp) > skewSeconds)
        {
            _logger.LogWarning("HMAC timestamp outside allowed skew: {Ts} vs {Now}", timestamp, now);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = "Timestamp outside allowed window" });
            return;
        }

        // Read body and compute SHA256 hex
        context.Request.EnableBuffering();
        string bodyHashHex;
        using (var sha = SHA256.Create())
        {
            var originalPosition = context.Request.Body.Position;
            using var ms = new MemoryStream();
            await context.Request.Body.CopyToAsync(ms);
            var bodyBytes = ms.ToArray();
            var bodyHash = sha.ComputeHash(bodyBytes);
            bodyHashHex = BitConverter.ToString(bodyHash).Replace("-", "").ToLowerInvariant();
            // reset request body for downstream
            context.Request.Body.Position = 0;
        }

        var canonical = new StringBuilder();
        canonical.Append(context.Request.Method.ToUpperInvariant());
        canonical.Append('\n');
        canonical.Append(context.Request.Path.ToString());
        canonical.Append('\n');
        canonical.Append(tsString);
        canonical.Append('\n');
        canonical.Append(bodyHashHex);

        var canonicalBytes = Encoding.UTF8.GetBytes(canonical.ToString());
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        using var hmac = new HMACSHA256(keyBytes);
        var computed = hmac.ComputeHash(canonicalBytes);
        var computedBase64 = Convert.ToBase64String(computed);

        // Constant time compare
        var providedBytes = Encoding.UTF8.GetBytes(providedSig);
        var computedSigBytes = Encoding.UTF8.GetBytes(computedBase64);
        var equal = CryptographicOperations.FixedTimeEquals(providedBytes, computedSigBytes);

        if (!equal)
        {
            _logger.LogWarning("HMAC signature mismatch for {Path}", context.Request.Path);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid signature" });
            return;
        }

        // Valid signature — mark request as authenticated via HMAC
        context.Items[AuthFlagKey] = true;
        _logger.LogDebug("HMAC authentication succeeded for {Path}", context.Request.Path);

        await _next(context);
    }
}
