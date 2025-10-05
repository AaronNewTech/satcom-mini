using Microsoft.EntityFrameworkCore;
using Satcom.Api;
using Satcom.Api.Endpoints;
using Satcom.Api.Security;
using Satcom.Api.Configuration;
using Satcom.Api.Services;
using Microsoft.AspNetCore.OpenApi;
using Satcom.Api.Domain;

// Load environment variables from .env file
// Try multiple paths to find the .env file
var envPaths = new[] { "../../.env", ".env", "../../../.env" };
bool envLoaded = false;
foreach (var path in envPaths)
{
    if (File.Exists(path))
    {
        DotNetEnv.Env.Load(path);
        Console.WriteLine($"✅ Loaded .env from: {Path.GetFullPath(path)}");
        envLoaded = true;
        break;
    }
}

if (!envLoaded)
{
    Console.WriteLine("⚠️  No .env file found, using system environment variables");
}

var builder = WebApplication.CreateBuilder(args);

// Debug: Verify environment variables are loaded (remove in production)
Console.WriteLine($"🗄️ DB Connection: {builder.Configuration.GetConnectionString("Postgres")}");
Console.WriteLine($"🔑 Internal API Key: {builder.Configuration["ApiKey"]}");
Console.WriteLine($"🌐 External API Key: {builder.Configuration["ExternalApiKey"]}");

// Debug: Show how the mapping works
var externalKey = builder.Configuration["ExternalApiKey"];
if (!string.IsNullOrEmpty(externalKey))
{
    Console.WriteLine($"� External API will use: 'apiKey: {externalKey}' in request headers");
}

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Old debug lines (replaced with better logging above)

// Config
var connString = builder.Configuration.GetConnectionString("Postgres");

// var apiKey = builder.Configuration["ApiKey"];
// if (string.IsNullOrEmpty(apiKey) || apiKey == "change-me")


// Services
builder.Services.AddDbContext<AppDbContext>(opt =>
opt.UseNpgsql(connString));

// Configure external API options
builder.Services.Configure<ExternalApiOptions>(options =>
{
    options.ExternalApiKey = builder.Configuration["ExternalApiKey"] ?? string.Empty;
});

// Register HTTP client and external satellite service
builder.Services.AddHttpClient<IExternalSatelliteService, ExternalSatelliteService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



var app = builder.Build();

app.UseCors();

// Middleware: API key
app.UseMiddleware<ApiKeyMiddleware>();


// Swagger (dev convenience)
app.UseSwagger();
app.UseSwaggerUI();


// Health
app.MapGet("/healthz", () => new { status = "ok" });


// Endpoints

app.MapTelemetry();
app.MapSatellites();
app.MapGroundStations();
app.MapExternalApi();


// Apply pending migrations automatically in dev (optional)
// using (var scope = app.Services.CreateScope())
// {
//     var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();


//     db.Database.Migrate();
//     if (!db.Satellites.Any())
//         db.Satellites.Add(new Satellite { Callsign = "HORIZON-1", NoradId = "90001" });

//     if (!db.GroundStations.Any())
//         db.GroundStations.Add(new GroundStation { Name = "Orlando-GS", Lat = 28.5383, Lon = -81.3792, ElevationM = 30 });

//     db.SaveChanges();
// }

// using (var scope = app.Services.CreateScope())
// {
//     var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
//     Seed.SeedAll(db);
// }

app.Run();
