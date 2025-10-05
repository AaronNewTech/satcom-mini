# C# Web API Project Structure Guide

## Satcom-Mini: Current vs Full Enterprise Structure

```
src/Satcom.Api/
├── 📁 Domain/                    ✅ YOU HAVE THIS
│   ├── Satellite.cs             # Core business objects (your app's "nouns")
│   ├── GroundStation.cs         # These represent real-world things
│   ├── Telemetry.cs             # Independent of databases, APIs, etc.
│   └── Geo/
│       ├── GeoCalc.cs           # Business logic for calculations
│       └── GeoTypes.cs          # Custom types for geo operations
│
├── 📁 Dtos/                      ✅ YOU HAVE THIS
│   ├── LocationOut.cs           # Data contracts for API responses
│   └── TelemetryIn.cs           # Data contracts for API requests
│   │                            # Think: "What data goes in/out of my API?"
│
├── 📁 Endpoints/                 ✅ YOU HAVE THIS (Modern Approach)
│   ├── SatelliteEndpoints.cs    # HTTP endpoints using Minimal APIs
│   └── TelemetryEndpoints.cs    # Maps URLs to your business logic
│   │                            # Alternative to traditional Controllers
│
├── 📁 Security/                  ✅ YOU HAVE THIS
│   └── ApiKeyMiddleware.cs      # Custom security logic
│   │                            # Runs before each API request
│
├── 📁 Migrations/                ✅ YOU HAVE THIS
│   ├── 20250917213546_Initial.cs        # Database schema changes
│   ├── 20250917213546_Initial.Designer.cs  # EF metadata
│   └── AppDbContextModelSnapshot.cs     # Current database state
│   │                                    # Like "Git for your database"
│
├── 📁 Properties/                ✅ YOU HAVE THIS
│   └── launchSettings.json      # Development server settings
│   │                            # Ports, environment variables, etc.
│
├── 📄 AppDbContext.cs            ✅ YOU HAVE THIS
├── 📄 Program.cs                 ✅ YOU HAVE THIS
├── 📄 Seed.cs                    ✅ YOU HAVE THIS
├── 📄 appsettings.json          ✅ YOU HAVE THIS
└── 📄 Satcom.Api.csproj         ✅ YOU HAVE THIS
│
│ 🚀 WHAT FULL ENTERPRISE PROJECTS ADD:
│
├── 📁 Controllers/               ❌ TRADITIONAL APPROACH (you use Endpoints)
│   ├── SatelliteController.cs   # Old-school MVC controllers
│   └── TelemetryController.cs   # You chose modern Minimal APIs instead
│
├── 📁 Services/                  ❌ BUSINESS LOGIC LAYER
│   ├── Interfaces/               # Contracts defining what services do
│   │   ├── ISatelliteService.cs # "I promise to find satellites"
│   │   └── ITelemetryService.cs # "I promise to process telemetry"
│   └── Implementations/          # Actual business logic code
│       ├── SatelliteService.cs  # "Here's HOW I find satellites"
│       └── TelemetryService.cs  # "Here's HOW I process telemetry"
│
├── 📁 Repositories/              ❌ DATA ACCESS LAYER
│   ├── Interfaces/               # Contracts for database operations
│   │   ├── ISatelliteRepository.cs  # "I promise to save/load satellites"
│   │   └── ITelemetryRepository.cs  # "I promise to save/load telemetry"
│   └── Implementations/          # Actual database code
│       ├── SatelliteRepository.cs   # "Here's HOW I query satellites"
│       └── TelemetryRepository.cs   # "Here's HOW I query telemetry"
│
├── 📁 Models/                    ❌ VARIOUS DATA SHAPES
│   ├── Requests/                 # What comes INTO your API
│   │   ├── CreateSatelliteRequest.cs   # "Give me this data to create"
│   │   └── UpdateTelemetryRequest.cs   # "Give me this data to update"
│   ├── Responses/                # What goes OUT of your API
│   │   ├── SatelliteResponse.cs  # "Here's the satellite data back"
│   │   └── TelemetryResponse.cs  # "Here's the telemetry data back"
│   └── ViewModels/               # Data shaped for specific UI screens
│       ├── DashboardViewModel.cs # "Data for the dashboard page"
│       └── MapViewModel.cs       # "Data for the map page"
│
├── 📁 Middleware/                ❌ REQUEST PIPELINE COMPONENTS
│   ├── ExceptionMiddleware.cs    # Catches and handles errors gracefully
│   ├── LoggingMiddleware.cs      # Records what happens in your app
│   └── RateLimitMiddleware.cs    # Prevents API spam/abuse
│
├── 📁 Filters/                   ❌ CROSS-CUTTING CONCERNS
│   ├── ValidateModelAttribute.cs    # Automatically validates input data
│   ├── AuthorizeRoleAttribute.cs    # Checks user permissions
│   └── ExceptionFilter.cs           # Handles specific error types
│
├── 📁 Validators/                ❌ INPUT VALIDATION RULES
│   ├── TelemetryValidator.cs     # "Telemetry data must be valid"
│   └── SatelliteValidator.cs     # "Satellite data must be complete"
│   │                             # Uses libraries like FluentValidation
│
├── 📁 Extensions/                ❌ HELPER METHODS
│   ├── ServiceCollectionExtensions.cs  # Custom DI registration
│   ├── DateTimeExtensions.cs           # Custom date operations
│   └── GeoExtensions.cs                # Custom geo calculations
│
├── 📁 Constants/                 ❌ SHARED VALUES
│   ├── ApiRoutes.cs              # "/api/satellites", "/api/telemetry"
│   ├── ErrorMessages.cs          # "Satellite not found", "Invalid data"
│   └── ConfigurationKeys.cs      # "Database:ConnectionString"
│
├── 📁 Helpers/                   ❌ UTILITY CLASSES
│   ├── GeoCalculator.cs          # Reusable geo math functions
│   ├── DataConverter.cs          # Transform data between formats
│   └── ApiResponseHelper.cs      # Standardize API responses
│
├── 📁 Infrastructure/            ❌ EXTERNAL INTEGRATIONS
│   ├── ExternalApis/             # Talk to other services
│   │   ├── NoradApiClient.cs     # Get real satellite data
│   │   └── WeatherApiClient.cs   # Get weather for predictions
│   ├── Caching/                  # Speed up repeated requests
│   │   └── RedisCacheService.cs  # Store frequently used data
│   └── Messaging/                # Real-time communication
│       └── SignalRHub.cs         # Push updates to clients
│
├── 📁 Configuration/             ❌ STRONGLY-TYPED SETTINGS
│   ├── DatabaseOptions.cs       # Database connection settings
│   ├── JwtOptions.cs             # Authentication settings
│   └── ApiOptions.cs             # API behavior settings
│
├── 📁 BackgroundServices/        ❌ CONTINUOUS TASKS
│   ├── TelemetryProcessor.cs     # Process incoming data continuously
│   └── SatelliteTracker.cs       # Update positions periodically
│
├── 📁 Health/                    ❌ MONITORING
│   ├── DatabaseHealthCheck.cs   # "Is the database responding?"
│   └── ApiHealthCheck.cs         # "Is the external API working?"
│
├── 📁 Jobs/                      ❌ SCHEDULED TASKS
│   ├── DataCleanupJob.cs         # Delete old telemetry data
│   └── ReportGenerationJob.cs    # Create daily reports
│
└── 📁 Tests/                     ❌ AUTOMATED TESTING
    ├── Unit/                     # Test individual pieces
    │   ├── Services/
    │   └── Repositories/
    ├── Integration/              # Test pieces working together
    │   └── Endpoints/
    └── E2E/                      # Test entire user workflows
        └── SatelliteTrackingTests.cs
```

## 📚 Detailed Explanations for Beginners

### 🏗️ **Architecture Layers (Bottom to Top)**

**1. Domain Layer (Business Core)**

- **What it is:** The heart of your application
- **Contains:** Business entities, rules, calculations
- **Example:** "A satellite has a callsign and NORAD ID"
- **Why separate:** Business logic shouldn't depend on databases or APIs

**2. Data Layer (Repository Pattern)**

- **What it is:** How you talk to the database
- **Contains:** Database queries, data access logic
- **Example:** "Find all satellites visible from this ground station"
- **Why separate:** You might switch from PostgreSQL to SQL Server later

**3. Service Layer (Business Logic)**

- **What it is:** Orchestrates business operations
- **Contains:** Complex business workflows, calculations
- **Example:** "Calculate satellite position using telemetry data"
- **Why separate:** Business logic is reusable across different endpoints

**4. API Layer (Presentation)**

- **What it is:** How external clients interact with your app
- **Contains:** HTTP endpoints, request/response handling
- **Example:** "GET /api/satellites returns list of satellites"
- **Why separate:** API format can change without affecting business logic

### 🔧 **Key Concepts Explained**

**DTOs vs Domain Models vs ViewModels:**

```csharp
// Domain Model - Pure business concept
public class Satellite
{
    public Guid Id { get; set; }
    public string Callsign { get; set; }
    public DateTime LastContact { get; set; }
    // Internal business properties
}

// DTO - API contract (what goes over the wire)
public record SatelliteDto(Guid Id, string Callsign);

// ViewModel - UI-specific data shape
public class DashboardViewModel
{
    public List<SatelliteDto> Satellites { get; set; }
    public int TotalCount { get; set; }
    public DateTime LastUpdated { get; set; }
}
```

**Dependency Injection (DI):**

- **What it is:** A way to provide dependencies to classes
- **Example:** Instead of creating a database connection inside your service, DI provides it
- **Why:** Makes testing easier, reduces tight coupling

**Middleware Pipeline:**

- **What it is:** Code that runs for every HTTP request
- **Example:** Authentication → Logging → Rate Limiting → Your Endpoint
- **Why:** Cross-cutting concerns handled in one place

## 🎯 **Your Current Structure Assessment**

**✅ Strengths:**

- Modern Minimal API approach (Endpoints over Controllers)
- Clean domain separation
- Proper DTO usage
- Security middleware
- EF migrations

**🚀 When to Add More Structure:**

- **Services Layer:** When business logic gets complex
- **Repository Pattern:** When you need advanced querying
- **Validators:** When input validation becomes complex
- **Background Services:** For real-time telemetry processing
- **Caching:** When performance becomes important

**Your structure is perfect for:**

- Learning modern .NET patterns
- Portfolio projects
- MVP/prototype development
- Microservices architecture

The beauty of your current approach is that it's **simple but professional** - you can add complexity as your project grows!
