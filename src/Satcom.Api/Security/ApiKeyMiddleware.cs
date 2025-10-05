using System.Net;


namespace Satcom.Api.Security;


public class ApiKeyMiddleware(RequestDelegate next, IConfiguration config, ILogger<ApiKeyMiddleware> logger)
{
private const string HeaderName = "x-api-key";
public async Task InvokeAsync(HttpContext context)
{
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