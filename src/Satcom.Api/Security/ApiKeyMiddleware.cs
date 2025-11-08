using System.Net;


namespace Satcom.Api.Security;


public class ApiKeyMiddleware(RequestDelegate next, IConfiguration config, ILogger<ApiKeyMiddleware> logger)
{
	private const string HeaderName = "x-api-key";
	private const string HmacAuthFlag = "HmacAuthenticated";

	public async Task InvokeAsync(HttpContext context)
	{
		// Allow unauthenticated access to health and swagger UI endpoints
		var path = context.Request.Path;
		if (path.StartsWithSegments("/swagger") || path.StartsWithSegments("/health") || path.StartsWithSegments("/healthz") || path.StartsWithSegments("/favicon.ico"))
		{
			await next(context);
			return;
		}

		// If HMAC middleware already authenticated this request, skip API key check
		if (context.Items.ContainsKey(HmacAuthFlag) && context.Items[HmacAuthFlag] is true)
		{
			await next(context);
			return;
		}

		var expected = config["ApiKey"];
		if (string.IsNullOrWhiteSpace(expected))
		{
			await next(context);
			return; // no key configured → open (dev)
		}
		if (!context.Request.Headers.TryGetValue(HeaderName, out var provided) || provided != expected)
		{
			logger.LogWarning("Unauthorized request to {Path}", context.Request.Path);
			context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
			await context.Response.WriteAsJsonAsync(new { error = "Unauthorized" });
			return;
		}
		await next(context);
	}
}