# 🛠️ **Satcom-Mini: Complete Build Checklist & Instructions**

_Build this entire satellite telemetry dashboard step-by-step from scratch_

---

## 📋 **FEATURE CHECKLIST**

### ✅ **COMPLETED FEATURES** (Your Current Implementation)

**🏗️ Backend Core Infrastructure:**

- [x] ASP.NET Core 8 Web API with Minimal APIs
- [x] PostgreSQL database with Entity Framework
- [x] Database migrations and seeding
- [x] CORS configuration for frontend integration
- [x] Custom API key authentication middleware
- [x] Swagger/OpenAPI documentation

**📊 Domain Models & Database:**

- [x] Satellite entity (Id, Callsign, NoradId)
- [x] GroundStation entity (Id, Name, Country, Lat, Lon, ElevationM)
- [x] Telemetry entity (Id, SatelliteId, StationId, ReceivedAtUtc, RssiDbm, ToaMs, BearingDeg, SnrDb)
- [x] Geospatial types (GeoPoint, LocationEstimate)
- [x] Database indexes for performance
- [x] Foreign key relationships

**🌐 API Endpoints:**

- [x] GET /v1/satellites - List all satellites
- [x] GET /v1/satellites/{id} - Get satellite by ID
- [x] GET /v1/satellites/{id}/location - Estimate satellite location
- [x] GET /v1/groundstations - List all ground stations
- [x] GET /v1/groundstations/{id} - Get ground station by ID
- [x] GET /v1/telemetry/satellites - Get satellites visible from station
- [x] POST /v1/telemetry - Submit telemetry data

**🧮 Business Logic:**

- [x] Geospatial calculations (bearing-based location estimation)
- [x] Satellite visibility from ground stations
- [x] Telemetry data processing

**🎨 Frontend Application:**

- [x] React + TypeScript + Vite setup
- [x] Leaflet map integration
- [x] Custom hooks for data fetching (useGroundStations, useSatellites, useTelemetry)
- [x] Interactive map with ground station markers
- [x] Satellite position visualization
- [x] Ground station selection dropdown
- [x] Real-time satellite tracking
- [x] Custom marker icons and offset logic

**✅ Quality & Testing:**

- [x] Unit tests for geospatial calculations
- [x] API validation and error handling
- [x] Environment configuration management

---

## 🚀 **REMAINING FEATURES TO ADD** (features-to-add.md)

**🔧 Advanced Features:**

- [ ] **Live API Integration** - Connect to real satellite APIs (NORAD, Celestrak)
- [ ] **Rate Limiting** - Advanced API throttling beyond basic API key
- [ ] **Performance Monitoring** - Database query optimization with EXPLAIN plans
- [ ] **Architecture Diagrams** - Current and future system design documentation

**🎮 Simulation Features:**

- [ ] **Rescue Mission Simulation** - Drone coordinate updates approaching target
- [ ] **Load Balancing System** - Simulate app scaling with table replication

**📈 Production Readiness:**

- [ ] **Comprehensive Testing** - 5-8 additional unit/integration tests
- [ ] **Documentation** - Complete README with setup instructions
- [ ] **Deployment** - Docker containerization and cloud deployment

---

## 📝 **FILE-BY-FILE BUILD INSTRUCTIONS**

### **PHASE 1: Project Setup & Configuration**

#### **Step 1.1: Initialize Backend Project**

```bash
# Create solution and project structure
dotnet new sln -n SatcomMini
mkdir -p src/Satcom.Api
mkdir -p tests/Satcom.Api.Tests

# Create Web API project
cd src/Satcom.Api
dotnet new webapi -n Satcom.Api --use-minimal-apis
cd ../..

# Add projects to solution
dotnet sln add src/Satcom.Api/Satcom.Api.csproj
dotnet sln add tests/Satcom.Api.Tests/Satcom.Api.Tests.csproj
```

#### **Step 1.2: Install NuGet Packages**

```xml
<!-- src/Satcom.Api/Satcom.Api.csproj -->
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="DotNetEnv" Version="3.1.1" />
    <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="8.0.8" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.7">
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.4" />
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
  </ItemGroup>
</Project>
```

#### **Step 1.3: Environment Configuration**

```json
// src/Satcom.Api/appsettings.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "Postgres": "Host=localhost;Port=5433;Database=satcom;Username=satcom;Password=satcom"
  },
  "ApiKey": "change-me"
}
```

```json
// src/Satcom.Api/appsettings.Development.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### **PHASE 2: Domain Layer (Business Logic)**

#### **Step 2.1: Core Domain Entities**

```csharp
// src/Satcom.Api/Domain/Satellite.cs
namespace Satcom.Api.Domain;

public class Satellite
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Callsign { get; set; } = string.Empty;
    public string? NoradId { get; set; }
}
```

```csharp
// src/Satcom.Api/Domain/GroundStation.cs
namespace Satcom.Api.Domain;

public class GroundStation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public double Lat { get; set; } // degrees
    public double Lon { get; set; } // degrees
    public double ElevationM { get; set; }
}
```

```csharp
// src/Satcom.Api/Domain/Telemetry.cs
namespace Satcom.Api.Domain;

public class Telemetry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SatelliteId { get; set; }
    public Satellite? Satellite { get; set; }
    public Guid StationId { get; set; }
    public GroundStation? Station { get; set; }
    public DateTime ReceivedAtUtc { get; set; }
    public double? RssiDbm { get; set; }
    public double? ToaMs { get; set; }
    public double? BearingDeg { get; set; }
    public double? SnrDb { get; set; }
}
```

#### **Step 2.2: Geospatial Types**

```csharp
// src/Satcom.Api/Domain/Geo/GeoTypes.cs
namespace Satcom.Api.Domain.Geo;

public readonly record struct GeoPoint(double Lat, double Lon);

public readonly record struct LocationEstimate(
    GeoPoint Point,
    double AccuracyKm,
    DateTime ComputedAtUtc
);
```

#### **Step 2.3: Geospatial Calculations**

```csharp
// src/Satcom.Api/Domain/Geo/GeoCalc.cs
using Satcom.Api.Domain.Geo;

namespace Satcom.Api.Domain.Geo;

public static class GeoCalc
{
    // Very simple bearings-centroid approximation using station bearings.
    // If insufficient data, returns null.
    public static LocationEstimate? EstimateFromBearings(
        IEnumerable<(GeoPoint station, double bearingDeg)> samples)
    {
        var list = samples.Where(s => !double.IsNaN(s.bearingDeg)).ToList();
        if (list.Count < 2) return null;

        // Convert to unit vectors and average.
        double x = 0, y = 0;
        foreach (var s in list)
        {
            // Bearing degrees clockwise from north; convert to radians east/north vector.
            var rad = s.bearingDeg * Math.PI / 180.0;
            x += Math.Sin(rad);
            y += Math.Cos(rad);
        }
        x /= list.Count; y /= list.Count;

        // Use average vector origin at the centroid of stations for a cheap estimate.
        var avgLat = list.Average(s => s.station.Lat);
        var avgLon = list.Average(s => s.station.Lon);

        // Assume 1000 km distance for demo purposes.
        var distanceKm = 1000.0;
        var deltaLat = (y * distanceKm) / 111.0; // rough km->degrees
        var deltaLon = (x * distanceKm) / (111.0 * Math.Cos(avgLat * Math.PI / 180.0));

        return new LocationEstimate(
            new GeoPoint(avgLat + deltaLat, avgLon + deltaLon),
            AccuracyKm: 100.0,
            ComputedAtUtc: DateTime.UtcNow
        );
    }
}
```

### **PHASE 3: Data Layer (Database & EF)**

#### **Step 3.1: Database Context**

```csharp
// src/Satcom.Api/AppDbContext.cs
using Microsoft.EntityFrameworkCore;
using Satcom.Api.Domain;

namespace Satcom.Api;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Satellite> Satellites => Set<Satellite>();
    public DbSet<GroundStation> GroundStations => Set<GroundStation>();
    public DbSet<Telemetry> Telemetries => Set<Telemetry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Satellite>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.NoradId).HasMaxLength(16);
        });

        modelBuilder.Entity<GroundStation>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasIndex(x => new { x.Lat, x.Lon });
        });

        modelBuilder.Entity<Telemetry>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasIndex(x => new { x.SatelliteId, x.ReceivedAtUtc });
            b.HasIndex(x => new { x.StationId, x.ReceivedAtUtc });
            b.HasOne(x => x.Satellite).WithMany().HasForeignKey(x => x.SatelliteId);
            b.HasOne(x => x.Station).WithMany().HasForeignKey(x => x.StationId);
        });
    }
}
```

#### **Step 3.2: Database Seeding**

```csharp
// src/Satcom.Api/Seed.cs
using Satcom.Api.Domain;

namespace Satcom.Api
{
    public static class Seed
    {
        public static void SeedAll(AppDbContext db)
        {
            // Ground stations
            var stations = new List<GroundStation>
            {
                new GroundStation { Name = "Goldstone Deep Space Communications Complex", Country = "USA (California)", Lat = 35.4259, Lon = -116.8889, ElevationM = 1002 },
                new GroundStation { Name = "Madrid Deep Space Communications Complex (Robledo)", Country = "Spain", Lat = 40.4272, Lon = -4.2492, ElevationM = 761 },
                new GroundStation { Name = "Canberra Deep Space Communications Complex (Tidbinbilla)", Country = "Australia", Lat = -35.3980, Lon = 148.9810, ElevationM = 550 },
                // Add more stations...
            };

            if (!db.GroundStations.Any())
            {
                db.GroundStations.AddRange(stations);
                db.SaveChanges();
            }

            // Satellites
            var satellites = new List<Satellite>
            {
                new Satellite { Callsign = "ISS", NoradId = "25544" },
                new Satellite { Callsign = "HUBBLE", NoradId = "20580" },
                new Satellite { Callsign = "GPS-IIA-10", NoradId = "22014" },
                // Add more satellites...
            };

            if (!db.Satellites.Any())
            {
                db.Satellites.AddRange(satellites);
                db.SaveChanges();
            }

            // Telemetry data
            if (!db.Telemetries.Any())
            {
                var random = new Random();
                var telemetryList = new List<Telemetry>();

                foreach (var satellite in satellites)
                {
                    // Ensure at least 2 telemetry entries per satellite for location estimation
                    for (int i = 0; i < Math.Max(2, random.Next(3, 8)); i++)
                    {
                        var station = stations[random.Next(stations.Count)];
                        telemetryList.Add(new Telemetry
                        {
                            SatelliteId = satellite.Id,
                            StationId = station.Id,
                            ReceivedAtUtc = DateTime.UtcNow.AddMinutes(-random.Next(1, 60)),
                            RssiDbm = -50 + random.NextDouble() * 30,
                            ToaMs = random.NextDouble() * 10,
                            BearingDeg = random.NextDouble() * 360,
                            SnrDb = 10 + random.NextDouble() * 15
                        });
                    }
                }

                db.Telemetries.AddRange(telemetryList);
                db.SaveChanges();
            }
        }
    }
}
```

### **PHASE 4: API Layer (DTOs & Endpoints)**

#### **Step 4.1: Data Transfer Objects**

```csharp
// src/Satcom.Api/Dtos/TelemetryIn.cs
namespace Satcom.Api.Dtos;

public record TelemetryIn(
    Guid SatelliteId,
    Guid StationId,
    DateTime ReceivedAtUtc,
    double? RssiDbm,
    double? ToaMs,
    double? BearingDeg,
    double? SnrDb
);
```

```csharp
// src/Satcom.Api/Dtos/LocationOut.cs
namespace Satcom.Api.Dtos;

public record LocationOut(
    double Lat,
    double Lon,
    double AccuracyKm,
    DateTime ComputedAtUtc
);
```

#### **Step 4.2: API Endpoints**

```csharp
// src/Satcom.Api/Endpoints/SatelliteEndpoints.cs
using Microsoft.EntityFrameworkCore;
using Satcom.Api.Dtos;
using Satcom.Api.Domain.Geo;

namespace Satcom.Api.Endpoints;

public static class SatelliteEndpoints
{
    public static RouteGroupBuilder MapSatellites(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/v1/satellites");

        // List all satellites
        group.MapGet("", async (AppDbContext db) =>
        {
            var sats = await db.Satellites
                .OrderBy(s => s.Callsign)
                .ToListAsync();
            return Results.Ok(sats);
        }).WithOpenApi();

        // Get a single satellite by id
        group.MapGet("/{id:guid}", async (Guid id, AppDbContext db) =>
        {
            var sat = await db.Satellites.FindAsync(id);
            if (sat == null) return Results.NotFound();
            return Results.Ok(sat);
        }).WithOpenApi();

        // Get estimated location for a satellite
        group.MapGet("/{id:guid}/location", async (Guid id, AppDbContext db) =>
        {
            // Grab last N observations across stations
            var latest = await db.Telemetries
                .Where(t => t.SatelliteId == id)
                .OrderByDescending(t => t.ReceivedAtUtc)
                .Take(10)
                .Select(t => new { t.BearingDeg, t.Station!.Lat, t.Station!.Lon })
                .ToListAsync();

            if (latest.Count < 2) return Results.Ok(null);

            var estimate = GeoCalc.EstimateFromBearings(
                latest.Where(x => x.BearingDeg.HasValue)
                    .Select(x => (new GeoPoint(x.Lat, x.Lon), x.BearingDeg!.Value))
            );

            if (estimate == null) return Results.Ok(null);

            return Results.Ok(new LocationOut(
                estimate.Value.Point.Lat,
                estimate.Value.Point.Lon,
                estimate.Value.AccuracyKm,
                estimate.Value.ComputedAtUtc
            ));
        }).WithOpenApi();

        return group;
    }
}
```

```csharp
// src/Satcom.Api/Endpoints/GroundStationEndpoints.cs
using Microsoft.EntityFrameworkCore;

namespace Satcom.Api.Endpoints;

public static class GroundStationEndpoints
{
    public static RouteGroupBuilder MapGroundStations(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/v1/groundstations");

        // List all ground stations
        group.MapGet("", async (AppDbContext db) =>
        {
            var stations = await db.GroundStations
                .OrderBy(gs => gs.Name)
                .ToListAsync();
            return Results.Ok(stations);
        }).WithOpenApi();

        // Get a single ground station by id
        group.MapGet("/{id:guid}", async (Guid id, AppDbContext db) =>
        {
            var station = await db.GroundStations.FindAsync(id);
            if (station == null) return Results.NotFound();
            return Results.Ok(station);
        }).WithOpenApi();

        return group;
    }
}
```

```csharp
// src/Satcom.Api/Endpoints/TelemetryEndpoints.cs
using Microsoft.EntityFrameworkCore;
using Satcom.Api.Domain;
using Satcom.Api.Dtos;

namespace Satcom.Api.Endpoints;

public static class TelemetryEndpoints
{
    public static RouteGroupBuilder MapTelemetry(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/v1/telemetry");

        // Get satellites visible from a ground station
        group.MapGet("/satellites", async (Guid? stationId, AppDbContext db) =>
        {
            if (!stationId.HasValue)
            {
                return Results.BadRequest("stationId is required");
            }

            var satelliteIds = await db.Telemetries
                .Where(t => t.StationId == stationId.Value)
                .Select(t => t.SatelliteId)
                .Distinct()
                .ToListAsync();

            var satellitesWithLocations = new List<object>();

            foreach (var satId in satelliteIds)
            {
                var satellite = await db.Satellites.FindAsync(satId);
                if (satellite == null) continue;

                // Get location estimate
                var latest = await db.Telemetries
                    .Where(t => t.SatelliteId == satId)
                    .OrderByDescending(t => t.ReceivedAtUtc)
                    .Take(10)
                    .Select(t => new { t.BearingDeg, t.Station!.Lat, t.Station!.Lon })
                    .ToListAsync();

                var estimate = latest.Count >= 2 ?
                    GeoCalc.EstimateFromBearings(
                        latest.Where(x => x.BearingDeg.HasValue)
                            .Select(x => (new GeoPoint(x.Lat, x.Lon), x.BearingDeg!.Value))
                    ) : null;

                satellitesWithLocations.Add(new
                {
                    id = satellite.Id,
                    callsign = satellite.Callsign,
                    lat = estimate?.Point.Lat,
                    lon = estimate?.Point.Lon
                });
            }

            return Results.Ok(satellitesWithLocations);
        }).WithOpenApi();

        // Submit telemetry data
        group.MapPost("", async (TelemetryIn telemetryData, AppDbContext db) =>
        {
            var telemetry = new Telemetry
            {
                SatelliteId = telemetryData.SatelliteId,
                StationId = telemetryData.StationId,
                ReceivedAtUtc = telemetryData.ReceivedAtUtc,
                RssiDbm = telemetryData.RssiDbm,
                ToaMs = telemetryData.ToaMs,
                BearingDeg = telemetryData.BearingDeg,
                SnrDb = telemetryData.SnrDb
            };

            db.Telemetries.Add(telemetry);
            await db.SaveChangesAsync();

            return Results.Created($"/v1/telemetry/{telemetry.Id}", telemetry);
        }).WithOpenApi();

        return group;
    }
}
```

### **PHASE 5: Security & Middleware**

#### **Step 5.1: API Key Middleware**

```csharp
// src/Satcom.Api/Security/ApiKeyMiddleware.cs
namespace Satcom.Api.Security;

public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _config;

    public ApiKeyMiddleware(RequestDelegate next, IConfiguration config)
    {
        _next = next;
        _config = config;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Allow swagger and health endpoints
        if (context.Request.Path.StartsWithSegments("/swagger") ||
            context.Request.Path.StartsWithSegments("/health"))
        {
            await _next(context);
            return;
        }

        // Check for API key
        if (!context.Request.Headers.TryGetValue("x-api-key", out var apiKey))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("API Key missing");
            return;
        }

        var validApiKey = _config["ApiKey"];
        if (apiKey != validApiKey)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Invalid API Key");
            return;
        }

        await _next(context);
    }
}
```

### **PHASE 6: Application Startup**

#### **Step 6.1: Program.cs Configuration**

```csharp
// src/Satcom.Api/Program.cs
using Microsoft.EntityFrameworkCore;
using Satcom.Api;
using Satcom.Api.Endpoints;
using Satcom.Api.Security;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

// CORS for frontend
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Database
var connString = builder.Configuration.GetConnectionString("Postgres");
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(connString));

// OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Middleware pipeline
app.UseCors();
app.UseMiddleware<ApiKeyMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Map endpoints
app.MapSatellites();
app.MapGroundStations();
app.MapTelemetry();

// Database seeding
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();
    Seed.SeedAll(context);
}

app.Run();
```

### **PHASE 7: Frontend Application**

#### **Step 7.1: Initialize React Project**

```bash
# Create Vite React TypeScript project
npm create vite@latest client -- --template react-ts
cd client
npm install leaflet react-leaflet @types/leaflet
```

#### **Step 7.2: Custom Hooks**

```typescript
// client/src/hooks/useGroundStations.ts
import { useEffect, useState } from 'react';

export interface GroundStation {
  id: string;
  name: string;
  country: string;
  lat: number;
  lon: number;
  elevationM: number;
}

export function useGroundStations() {
  const [groundStations, setGroundStations] = useState<GroundStation[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    async function fetchGroundStations() {
      setLoading(true);
      setError(null);
      try {
        const res = await fetch('http://localhost:5143/v1/groundstations', {
          headers: { 'x-api-key': 'change-me' },
        });
        if (!res.ok) throw new Error('Failed to fetch groundstations');
        const data = await res.json();
        setGroundStations(data);
      } catch (err: any) {
        setError(err.message || 'Unknown error');
      } finally {
        setLoading(false);
      }
    }

    fetchGroundStations();
  }, []);

  return { groundStations, loading, error };
}
```

#### **Step 7.3: Map Component**

```tsx
// client/src/pages/MapView.tsx
import React, { useState } from 'react';
import { MapContainer, TileLayer, Marker, Popup } from 'react-leaflet';
import 'leaflet/dist/leaflet.css';
import L from 'leaflet';
import { useGroundStations } from '../hooks/useGroundStations';

// Configure custom markers
const redIcon = new L.Icon({
  iconUrl: '/marker-icon-2x-red.png',
  shadowUrl: '/marker-shadow.png',
  iconSize: [25, 41],
  iconAnchor: [12, 41],
  popupAnchor: [1, -34],
  shadowSize: [41, 41],
});

const MapView: React.FC = () => {
  const { groundStations, loading, error } = useGroundStations();
  const [selectedStation, setSelectedStation] = useState<string>('');
  const [satellites, setSatellites] = useState<any[]>([]);

  const fetchSatellitesForStation = async (stationId: string) => {
    try {
      const res = await fetch(
        `http://localhost:5143/v1/telemetry/satellites?stationId=${stationId}`,
        { headers: { 'x-api-key': 'change-me' } },
      );
      const data = await res.json();
      setSatellites(data.filter((sat: any) => sat.lat && sat.lon));
    } catch (err) {
      console.error('Failed to fetch satellites:', err);
    }
  };

  if (loading) return <div>Loading...</div>;
  if (error) return <div>Error: {error}</div>;

  return (
    <div style={{ height: '100vh', width: '100%' }}>
      <div style={{ padding: '10px', background: '#f0f0f0' }}>
        <label>
          Select Ground Station:
          <select
            value={selectedStation}
            onChange={(e) => {
              setSelectedStation(e.target.value);
              if (e.target.value) {
                fetchSatellitesForStation(e.target.value);
              } else {
                setSatellites([]);
              }
            }}
          >
            <option value="">-- Select a station --</option>
            {groundStations.map((station) => (
              <option key={station.id} value={station.id}>
                {station.name} ({station.country})
              </option>
            ))}
          </select>
        </label>
      </div>

      <MapContainer
        center={[28.5, -81.3]}
        zoom={2}
        style={{ height: 'calc(100vh - 60px)', width: '100%' }}
      >
        <TileLayer
          url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
          attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
        />

        {/* Ground Station Markers */}
        {groundStations.map((station) => (
          <Marker key={station.id} position={[station.lat, station.lon]}>
            <Popup>
              <strong>{station.name}</strong>
              <br />
              {station.country}
              <br />
              Elevation: {station.elevationM}m<br />
              Coordinates: {station.lat.toFixed(4)}, {station.lon.toFixed(4)}
            </Popup>
          </Marker>
        ))}

        {/* Satellite Markers */}
        {satellites.map((satellite, index) => {
          // Offset overlapping markers
          const offsetLat = satellite.lat + index * 0.1;
          const offsetLon = satellite.lon + index * 0.1;

          return (
            <Marker
              key={satellite.id}
              position={[offsetLat, offsetLon]}
              icon={redIcon}
            >
              <Popup>
                <strong>{satellite.callsign}</strong>
                <br />
                Estimated Position
                <br />
                Lat: {satellite.lat.toFixed(4)}
                <br />
                Lon: {satellite.lon.toFixed(4)}
              </Popup>
            </Marker>
          );
        })}
      </MapContainer>
    </div>
  );
};

export default MapView;
```

### **PHASE 8: Testing**

#### **Step 8.1: Unit Tests**

```csharp
// tests/Satcom.Api.Tests/GeoCalcTests.cs
using Satcom.Api.Domain.Geo;
using Xunit;

namespace Satcom.Api.Tests;

public class GeoCalcTests
{
    [Fact]
    public void ReturnsNullWithInsufficientSamples()
    {
        var res = GeoCalc.EstimateFromBearings(new List<(GeoPoint, double)>());
        Assert.Null(res);
    }

    [Fact]
    public void ProducesEstimateWithTwoBearings()
    {
        var stationA = new GeoPoint(28.5, -81.3);
        var stationB = new GeoPoint(28.6, -81.4);
        var res = GeoCalc.EstimateFromBearings(new[]
        {
            (stationA, 45.0),
            (stationB, 135.0)
        });

        Assert.NotNull(res);
        Assert.True(res.Value.AccuracyKm > 0);
    }
}
```

### **PHASE 9: Database Operations**

#### **Step 9.1: Entity Framework Migrations**

```bash
# Create initial migration
dotnet ef migrations add Initial

# Update database
dotnet ef database update

# Add new feature migration (example)
dotnet ef migrations add AddCountryToGroundStation
dotnet ef database update
```

### **PHASE 10: Documentation & Deployment**

#### **Step 10.1: README Documentation**

```markdown
# Satcom-Mini: Satellite Telemetry Dashboard

## Setup Instructions

### Prerequisites

- .NET 8 SDK
- PostgreSQL
- Node.js 18+

### Backend Setup

1. Clone repository
2. Set environment variables
3. Install dependencies: `dotnet restore`
4. Update database: `dotnet ef database update`
5. Run: `dotnet run --project src/Satcom.Api`

### Frontend Setup

1. Navigate to client: `cd client`
2. Install dependencies: `npm install`
3. Start dev server: `npm run dev`

### API Documentation

- Swagger UI: http://localhost:5143/swagger
- API Key: Include `x-api-key: change-me` header
```

---

## 🎯 **BUILD ORDER CHECKLIST**

### **✅ Phase 1: Foundation**

- [ ] Create solution structure
- [ ] Install NuGet packages
- [ ] Configure environment files
- [ ] Set up database connection

### **✅ Phase 2: Domain Models**

- [ ] Create Satellite entity
- [ ] Create GroundStation entity
- [ ] Create Telemetry entity
- [ ] Add geospatial types
- [ ] Implement geospatial calculations

### **✅ Phase 3: Database Layer**

- [ ] Configure AppDbContext
- [ ] Set up entity relationships
- [ ] Add database indexes
- [ ] Create seeding logic
- [ ] Run initial migration

### **✅ Phase 4: API Layer**

- [ ] Create DTOs
- [ ] Implement satellite endpoints
- [ ] Implement ground station endpoints
- [ ] Implement telemetry endpoints
- [ ] Add API documentation

### **✅ Phase 5: Security**

- [ ] Implement API key middleware
- [ ] Configure CORS
- [ ] Add authentication logic

### **✅ Phase 6: Application Setup**

- [ ] Configure dependency injection
- [ ] Set up middleware pipeline
- [ ] Add database seeding
- [ ] Configure startup

### **✅ Phase 7: Frontend**

- [ ] Initialize React/TypeScript project
- [ ] Install mapping libraries
- [ ] Create custom hooks
- [ ] Build map components
- [ ] Implement satellite visualization

### **✅ Phase 8: Testing**

- [ ] Write unit tests
- [ ] Test geospatial calculations
- [ ] Validate API endpoints
- [ ] Test frontend integration

### **✅ Phase 9: Database Operations**

- [ ] Create migrations
- [ ] Test database updates
- [ ] Verify seeding works
- [ ] Optimize queries

### **✅ Phase 10: Documentation**

- [ ] Write comprehensive README
- [ ] Document API endpoints
- [ ] Create setup instructions
- [ ] Add troubleshooting guide

---

## 🚀 **Next Steps: Advanced Features**

After completing the core application, implement these advanced features:

1. **Real API Integration** - Connect to live satellite data
2. **Performance Optimization** - Add caching and query optimization
3. **Testing Suite** - Comprehensive unit and integration tests
4. **Deployment** - Containerization and cloud deployment
5. **Monitoring** - Add logging and performance monitoring
6. **Simulation Features** - Rescue missions and load balancing

**This comprehensive guide will help you build the entire satcom-mini application from scratch, understanding every component and design decision along the way!**
