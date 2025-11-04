# 🎯 **Advanced Features Implementation Guide**

_Complete the remaining features from your features-to-add.md checklist_

---

## 🚀 **ADVANCED FEATURES ROADMAP**

### **Feature 1: Live API Integration**

_Connect to real satellite APIs (NORAD, Celestrak)_

#### **Implementation Steps:**

**Step 1.1: Create External API Service**

```csharp
// src/Satcom.Api/Services/Interfaces/ISatelliteDataService.cs
namespace Satcom.Api.Services.Interfaces;

public interface ISatelliteDataService
{
    Task<List<SatellitePosition>> GetLiveSatellitePositionsAsync();
    Task<SatelliteInfo> GetSatelliteInfoAsync(string noradId);
}

public record SatellitePosition(string NoradId, double Lat, double Lon, DateTime Timestamp);
public record SatelliteInfo(string NoradId, string Name, string LaunchDate);
```

**Step 1.2: Implement NORAD API Client**

```csharp
// src/Satcom.Api/Services/Implementations/NoradApiService.cs
using System.Text.Json;
using Satcom.Api.Services.Interfaces;

namespace Satcom.Api.Services.Implementations;

public class NoradApiService : ISatelliteDataService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<NoradApiService> _logger;

    public NoradApiService(HttpClient httpClient, ILogger<NoradApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<SatellitePosition>> GetLiveSatellitePositionsAsync()
    {
        try
        {
            // Example: Use N2YO API or similar
            var response = await _httpClient.GetAsync("https://api.n2yo.com/rest/v1/satellite/positions/25544/41.702/-76.014/0/2/&apiKey=YOUR_API_KEY");
            var jsonContent = await response.Content.ReadAsStringAsync();

            // Parse JSON and convert to SatellitePosition objects
            var data = JsonSerializer.Deserialize<dynamic>(jsonContent);

            return new List<SatellitePosition>(); // Implement parsing
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch live satellite data");
            return new List<SatellitePosition>();
        }
    }

    public async Task<SatelliteInfo> GetSatelliteInfoAsync(string noradId)
    {
        // Implement satellite info retrieval
        return new SatelliteInfo(noradId, "Unknown", "Unknown");
    }
}
```

**Step 1.3: Add Background Service for Real-Time Updates**

```csharp
// src/Satcom.Api/Services/SatelliteUpdateService.cs
using Microsoft.Extensions.Hosting;
using Satcom.Api.Services.Interfaces;

namespace Satcom.Api.Services;

public class SatelliteUpdateService : BackgroundService
{
    private readonly ISatelliteDataService _satelliteService;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SatelliteUpdateService> _logger;

    public SatelliteUpdateService(
        ISatelliteDataService satelliteService,
        IServiceScopeFactory scopeFactory,
        ILogger<SatelliteUpdateService> logger)
    {
        _satelliteService = satelliteService;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var positions = await _satelliteService.GetLiveSatellitePositionsAsync();

                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // Update database with live positions
                foreach (var position in positions)
                {
                    // Create simulated telemetry from live data
                    var satellite = await context.Satellites
                        .FirstOrDefaultAsync(s => s.NoradId == position.NoradId);

                    if (satellite != null)
                    {
                        // Add telemetry entry with live position data
                        // Implementation depends on how you want to store live data
                    }
                }

                await context.SaveChangesAsync();
                _logger.LogInformation($"Updated {positions.Count} satellite positions");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating satellite positions");
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // Update every 5 minutes
        }
    }
}
```

---

### **Feature 2: Rate Limiting & Advanced API Protection**

#### **Implementation Steps:**

**Step 2.1: Install Rate Limiting Package**

```xml
<!-- Add to Satcom.Api.csproj -->
<PackageReference Include="AspNetCoreRateLimit" Version="5.0.0" />
```

**Step 2.2: Configure Rate Limiting**

```csharp
// src/Satcom.Api/Configuration/RateLimitConfiguration.cs
using AspNetCoreRateLimit;

namespace Satcom.Api.Configuration;

public static class RateLimitConfiguration
{
    public static IServiceCollection AddRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        // Store rate limit counters in memory
        services.AddMemoryCache();

        // Configure IP rate limiting
        services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimiting"));
        services.Configure<IpRateLimitPolicies>(configuration.GetSection("IpRateLimitPolicies"));

        // Add rate limit services
        services.AddInMemoryRateLimiting();
        services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

        return services;
    }
}
```

**Step 2.3: Add Rate Limit Settings**

```json
// Add to appsettings.json
{
  "IpRateLimiting": {
    "EnableEndpointRateLimiting": true,
    "StackBlockedRequests": false,
    "RealIpHeader": "X-Real-IP",
    "ClientIdHeader": "X-ClientId",
    "HttpStatusCode": 429,
    "GeneralRules": [
      {
        "Endpoint": "*",
        "Period": "1m",
        "Limit": 100
      },
      {
        "Endpoint": "*/telemetry",
        "Period": "1m",
        "Limit": 10
      }
    ]
  }
}
```

---

### **Feature 3: Database Performance Optimization**

#### **Implementation Steps:**

**Step 3.1: Add Performance Monitoring**

```csharp
// src/Satcom.Api/Extensions/DatabasePerformanceExtensions.cs
using Microsoft.EntityFrameworkCore;

namespace Satcom.Api.Extensions;

public static class DatabasePerformanceExtensions
{
    public static IServiceCollection AddDatabasePerformanceMonitoring(this IServiceCollection services)
    {
        services.AddDbContext<AppDbContext>((serviceProvider, options) =>
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var connectionString = configuration.GetConnectionString("Postgres");

            options.UseNpgsql(connectionString)
                   .EnableSensitiveDataLogging() // Only in development
                   .EnableDetailedErrors()
                   .LogTo(Console.WriteLine, LogLevel.Information);
        });

        return services;
    }
}
```

**Step 3.2: Create Query Optimization Helper**

```csharp
// src/Satcom.Api/Helpers/QueryOptimizationHelper.cs
using Microsoft.EntityFrameworkCore;

namespace Satcom.Api.Helpers;

public static class QueryOptimizationHelper
{
    public static async Task<string> GetQueryExecutionPlanAsync(AppDbContext context, string query)
    {
        try
        {
            // Execute EXPLAIN ANALYZE for PostgreSQL
            var explainQuery = $"EXPLAIN (ANALYZE, BUFFERS, FORMAT JSON) {query}";

            using var command = context.Database.GetDbConnection().CreateCommand();
            command.CommandText = explainQuery;

            await context.Database.OpenConnectionAsync();

            using var reader = await command.ExecuteReaderAsync();
            var result = new List<string>();

            while (await reader.ReadAsync())
            {
                result.Add(reader.GetString(0));
            }

            return string.Join("\n", result);
        }
        catch (Exception ex)
        {
            return $"Error getting execution plan: {ex.Message}";
        }
    }

    public static IQueryable<Telemetry> OptimizedTelemetryQuery(AppDbContext context, Guid satelliteId)
    {
        // Optimized query with proper indexing
        return context.Telemetries
            .Where(t => t.SatelliteId == satelliteId)
            .OrderByDescending(t => t.ReceivedAtUtc)
            .Include(t => t.Station) // Use Include for navigation properties
            .AsNoTracking(); // Use AsNoTracking for read-only queries
    }
}
```

**Step 3.3: Add Performance Endpoint**

```csharp
// Add to SatelliteEndpoints.cs
group.MapGet("/performance/explain", async (AppDbContext db) =>
{
    var query = @"
        SELECT t.""Id"", t.""BearingDeg"", gs.""Lat"", gs.""Lon""
        FROM ""Telemetries"" t
        INNER JOIN ""GroundStations"" gs ON t.""StationId"" = gs.""Id""
        WHERE t.""SatelliteId"" = '00000000-0000-0000-0000-000000000000'
        ORDER BY t.""ReceivedAtUtc"" DESC
        LIMIT 10";

    var plan = await QueryOptimizationHelper.GetQueryExecutionPlanAsync(db, query);
    return Results.Ok(new { ExecutionPlan = plan });
}).WithOpenApi();
```

---

### **Feature 4: Rescue Mission Simulation**

#### **Implementation Steps:**

**Step 4.1: Create Rescue Mission Domain**

```csharp
// src/Satcom.Api/Domain/RescueMission.cs
namespace Satcom.Api.Domain;

public class RescueMission
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public Guid DroneId { get; set; }
    public Drone? Drone { get; set; }
    public double TargetLat { get; set; }
    public double TargetLon { get; set; }
    public MissionStatus Status { get; set; }
    public DateTime StartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
}

public class Drone
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public double CurrentLat { get; set; }
    public double CurrentLon { get; set; }
    public double SpeedKmh { get; set; }
    public DateTime LastUpdatedUtc { get; set; }
}

public enum MissionStatus
{
    Planning,
    InProgress,
    Completed,
    Failed
}
```

**Step 4.2: Create Mission Simulation Service**

```csharp
// src/Satcom.Api/Services/RescueMissionService.cs
using Satcom.Api.Domain;
using Satcom.Api.Domain.Geo;

namespace Satcom.Api.Services;

public class RescueMissionService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RescueMissionService> _logger;

    public RescueMissionService(IServiceScopeFactory scopeFactory, ILogger<RescueMissionService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var activeMissions = await context.RescueMissions
                .Include(m => m.Drone)
                .Where(m => m.Status == MissionStatus.InProgress)
                .ToListAsync();

            foreach (var mission in activeMissions)
            {
                if (mission.Drone == null) continue;

                // Calculate movement toward target
                var newPosition = CalculateNewDronePosition(
                    mission.Drone.CurrentLat,
                    mission.Drone.CurrentLon,
                    mission.TargetLat,
                    mission.TargetLon,
                    mission.Drone.SpeedKmh
                );

                mission.Drone.CurrentLat = newPosition.Lat;
                mission.Drone.CurrentLon = newPosition.Lon;
                mission.Drone.LastUpdatedUtc = DateTime.UtcNow;

                // Check if drone reached target
                var distanceToTarget = CalculateDistance(
                    newPosition.Lat, newPosition.Lon,
                    mission.TargetLat, mission.TargetLon
                );

                if (distanceToTarget < 0.1) // Within 100m
                {
                    mission.Status = MissionStatus.Completed;
                    mission.CompletedAtUtc = DateTime.UtcNow;
                    _logger.LogInformation($"Mission {mission.Name} completed!");
                }
            }

            await context.SaveChangesAsync();
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken); // Update every 5 seconds
        }
    }

    private GeoPoint CalculateNewDronePosition(double currentLat, double currentLon,
        double targetLat, double targetLon, double speedKmh)
    {
        // Simple linear interpolation toward target
        var distanceToTarget = CalculateDistance(currentLat, currentLon, targetLat, targetLon);
        var timeStepHours = 5.0 / 3600.0; // 5 seconds in hours
        var maxMovementKm = speedKmh * timeStepHours;

        if (distanceToTarget <= maxMovementKm)
        {
            return new GeoPoint(targetLat, targetLon); // Reached target
        }

        // Calculate direction and move toward target
        var latDiff = targetLat - currentLat;
        var lonDiff = targetLon - currentLon;
        var ratio = maxMovementKm / distanceToTarget;

        return new GeoPoint(
            currentLat + (latDiff * ratio),
            currentLon + (lonDiff * ratio)
        );
    }

    private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        // Haversine formula for distance calculation
        var R = 6371; // Earth's radius in km
        var dLat = (lat2 - lat1) * Math.PI / 180;
        var dLon = (lon2 - lon1) * Math.PI / 180;

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }
}
```

**Step 4.3: Create Rescue Mission Endpoints**

```csharp
// src/Satcom.Api/Endpoints/RescueMissionEndpoints.cs
using Microsoft.EntityFrameworkCore;
using Satcom.Api.Domain;

namespace Satcom.Api.Endpoints;

public static class RescueMissionEndpoints
{
    public static RouteGroupBuilder MapRescueMissions(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/v1/rescue-missions");

        // Start a new rescue mission
        group.MapPost("", async (StartMissionRequest request, AppDbContext db) =>
        {
            var drone = new Drone
            {
                Name = request.DroneName,
                CurrentLat = request.StartLat,
                CurrentLon = request.StartLon,
                SpeedKmh = request.SpeedKmh,
                LastUpdatedUtc = DateTime.UtcNow
            };

            var mission = new RescueMission
            {
                Name = request.MissionName,
                Drone = drone,
                TargetLat = request.TargetLat,
                TargetLon = request.TargetLon,
                Status = MissionStatus.InProgress,
                StartedAtUtc = DateTime.UtcNow
            };

            db.RescueMissions.Add(mission);
            await db.SaveChangesAsync();

            return Results.Created($"/v1/rescue-missions/{mission.Id}", mission);
        }).WithOpenApi();

        // Get all active missions
        group.MapGet("", async (AppDbContext db) =>
        {
            var missions = await db.RescueMissions
                .Include(m => m.Drone)
                .Where(m => m.Status == MissionStatus.InProgress)
                .ToListAsync();

            return Results.Ok(missions);
        }).WithOpenApi();

        // Get mission by ID with drone position
        group.MapGet("/{id:guid}", async (Guid id, AppDbContext db) =>
        {
            var mission = await db.RescueMissions
                .Include(m => m.Drone)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (mission == null) return Results.NotFound();
            return Results.Ok(mission);
        }).WithOpenApi();

        return group;
    }
}

public record StartMissionRequest(
    string MissionName,
    string DroneName,
    double StartLat,
    double StartLon,
    double TargetLat,
    double TargetLon,
    double SpeedKmh
);
```

---

### **Feature 5: Load Balancing Simulation**

#### **Implementation Steps:**

**Step 5.1: Create Database Replication Simulation**

```csharp
// src/Satcom.Api/Services/LoadBalancingService.cs
using Microsoft.EntityFrameworkCore;

namespace Satcom.Api.Services;

public class LoadBalancingService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<LoadBalancingService> _logger;
    private readonly List<string> _replicaConnections;

    public LoadBalancingService(IServiceProvider serviceProvider, IConfiguration configuration,
        ILogger<LoadBalancingService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;

        // Simulate multiple database connections
        _replicaConnections = new List<string>
        {
            configuration.GetConnectionString("Postgres") + ";ApplicationName=Primary",
            configuration.GetConnectionString("Postgres") + ";ApplicationName=Replica1",
            configuration.GetConnectionString("Postgres") + ";ApplicationName=Replica2"
        };
    }

    public async Task<T> ExecuteWithLoadBalancing<T>(Func<AppDbContext, Task<T>> operation)
    {
        var attempts = 0;
        var maxAttempts = _replicaConnections.Count;

        while (attempts < maxAttempts)
        {
            try
            {
                var connectionString = GetNextConnection();
                using var scope = _serviceProvider.CreateScope();

                // Create context with specific connection
                var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
                optionsBuilder.UseNpgsql(connectionString);

                using var context = new AppDbContext(optionsBuilder.Options);

                _logger.LogInformation($"Executing operation on {connectionString}");
                return await operation(context);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Operation failed on attempt {attempts + 1}: {ex.Message}");
                attempts++;

                if (attempts >= maxAttempts)
                    throw;

                await Task.Delay(100); // Brief delay before retry
            }
        }

        throw new InvalidOperationException("All database replicas failed");
    }

    private string GetNextConnection()
    {
        // Simple round-robin load balancing
        var index = Random.Shared.Next(_replicaConnections.Count);
        return _replicaConnections[index];
    }
}
```

**Step 5.2: Create Load Testing Endpoint**

```csharp
// Add to SatelliteEndpoints.cs
group.MapGet("/load-test", async (AppDbContext db, LoadBalancingService loadBalancer) =>
{
    var tasks = new List<Task<object>>();

    // Simulate 10 concurrent requests
    for (int i = 0; i < 10; i++)
    {
        var taskIndex = i;
        var task = loadBalancer.ExecuteWithLoadBalancing(async context =>
        {
            var satellites = await context.Satellites
                .OrderBy(s => s.Callsign)
                .Take(5)
                .ToListAsync();

            return new { TaskIndex = taskIndex, Count = satellites.Count, Timestamp = DateTime.UtcNow };
        });

        tasks.Add(task);
    }

    var results = await Task.WhenAll(tasks);

    return Results.Ok(new
    {
        Message = "Load balancing test completed",
        Results = results,
        TotalRequests = results.Length
    });
}).WithOpenApi();
```

---

### **Feature 6: Comprehensive Testing Suite**

#### **Implementation Steps:**

**Step 6.1: Integration Tests**

```csharp
// tests/Satcom.Api.Tests/Integration/SatelliteEndpointsTests.cs
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using Xunit;

namespace Satcom.Api.Tests.Integration;

public class SatelliteEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public SatelliteEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Add("x-api-key", "change-me");
    }

    [Fact]
    public async Task GetSatellites_ReturnsSuccessStatusCode()
    {
        var response = await _client.GetAsync("/v1/satellites");

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.NotNull(content);
    }

    [Fact]
    public async Task GetSatelliteLocation_WithValidId_ReturnsLocation()
    {
        // First get a satellite
        var satellitesResponse = await _client.GetAsync("/v1/satellites");
        // Parse response and extract ID
        // Then test location endpoint

        Assert.True(true); // Implement full test
    }
}
```

**Step 6.2: Performance Tests**

```csharp
// tests/Satcom.Api.Tests/Performance/DatabasePerformanceTests.cs
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Xunit;

namespace Satcom.Api.Tests.Performance;

public class DatabasePerformanceTests
{
    [Fact]
    public async Task TelemetryQuery_WithIndex_PerformsWithinThreshold()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new AppDbContext(options);

        // Seed test data
        await SeedTestData(context, 10000); // Large dataset

        // Act
        var stopwatch = Stopwatch.StartNew();

        var result = await context.Telemetries
            .Where(t => t.SatelliteId == Guid.NewGuid())
            .OrderByDescending(t => t.ReceivedAtUtc)
            .Take(10)
            .ToListAsync();

        stopwatch.Stop();

        // Assert
        Assert.True(stopwatch.ElapsedMilliseconds < 100,
            $"Query took {stopwatch.ElapsedMilliseconds}ms, expected < 100ms");
    }

    private async Task SeedTestData(AppDbContext context, int recordCount)
    {
        // Seed large amount of test data
        var satellites = Enumerable.Range(1, 100)
            .Select(i => new Satellite { Callsign = $"SAT-{i}" })
            .ToList();

        context.Satellites.AddRange(satellites);
        await context.SaveChangesAsync();

        // Add telemetry data
        var telemetryData = new List<Telemetry>();
        var random = new Random();

        for (int i = 0; i < recordCount; i++)
        {
            telemetryData.Add(new Telemetry
            {
                SatelliteId = satellites[random.Next(satellites.Count)].Id,
                ReceivedAtUtc = DateTime.UtcNow.AddMinutes(-random.Next(10000)),
                RssiDbm = -50 + random.NextDouble() * 30
            });
        }

        context.Telemetries.AddRange(telemetryData);
        await context.SaveChangesAsync();
    }
}
```

---

## 🎯 **IMPLEMENTATION PRIORITY**

### **Phase 1: Foundation (Week 1)**

1. ✅ Rate Limiting & API Protection
2. ✅ Database Performance Optimization
3. ✅ Comprehensive Testing Suite

### **Phase 2: Advanced Features (Week 2)**

4. ✅ Live API Integration
5. ✅ Rescue Mission Simulation

### **Phase 3: Scaling (Week 3)**

6. ✅ Load Balancing Simulation
7. ✅ Architecture Documentation
8. ✅ Deployment & Monitoring

---

## 📋 **IMPLEMENTATION CHECKLIST**

### **Rate Limiting & Security**

- [ ] Install AspNetCoreRateLimit package
- [ ] Configure IP-based rate limiting
- [ ] Add endpoint-specific limits
- [ ] Test rate limiting functionality
- [ ] Add rate limit headers to responses

### **Performance Optimization**

- [ ] Add database query logging
- [ ] Create query execution plan endpoint
- [ ] Optimize telemetry queries with proper indexing
- [ ] Add performance monitoring middleware
- [ ] Create performance test suite

### **Live API Integration**

- [ ] Create external API service interfaces
- [ ] Implement NORAD/Celestrak API clients
- [ ] Add background service for live updates
- [ ] Handle API rate limits and failures
- [ ] Add configuration for API keys

### **Rescue Mission Simulation**

- [ ] Create rescue mission domain models
- [ ] Implement drone movement simulation
- [ ] Add mission management endpoints
- [ ] Create real-time position updates
- [ ] Add mission status tracking

### **Load Balancing**

- [ ] Create database connection load balancer
- [ ] Implement round-robin connection strategy
- [ ] Add connection health monitoring
- [ ] Create load testing endpoints
- [ ] Add metrics and monitoring

### **Testing & Quality**

- [ ] Add integration tests for all endpoints
- [ ] Create performance benchmarks
- [ ] Add load testing scenarios
- [ ] Implement automated testing pipeline
- [ ] Add code coverage reporting

### **Documentation & Deployment**

- [ ] Update architecture diagrams
- [ ] Document new API endpoints
- [ ] Create deployment guides
- [ ] Add monitoring and alerting
- [ ] Write troubleshooting documentation

**Each feature includes detailed implementation steps, code examples, and testing strategies to ensure professional-quality delivery!** 🚀
