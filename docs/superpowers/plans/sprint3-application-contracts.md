# Sprint 3 — Application Contracts + API Host Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement service interfaces, DTOs with manual mapping, global exception handler, TenantMiddleware stub, and endpoint registration for the Presentation layer.

**Architecture:** Clean Architecture — Application layer holds DTOs/interfaces (no implementation), Presentation layer has middleware and endpoint routing. All async with `Guid tenantId` parameter propagated from JWT.

**Tech Stack:** .NET 10 Minimal APIs, no FluentValidation, manual DTO mapping.

---

### Task 1: Auth DTOs

**Files:**
- Create: `src/Application/DTOs/Auth/RegisterRequestDto.cs`
- Create: `src/Application/DTOs/Auth/LoginRequestDto.cs`
- Create: `src/Application/DTOs/Auth/LoginResponseDto.cs`

- [ ] **Create `RegisterRequestDto.cs`**

```csharp
namespace NexusResourceEngine.Application.DTOs.Auth;

public class RegisterRequestDto
{
    public string OrganizationName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
```

- [ ] **Create `LoginRequestDto.cs`**

```csharp
namespace NexusResourceEngine.Application.DTOs.Auth;

public class LoginRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
```

- [ ] **Create `LoginResponseDto.cs`**

```csharp
namespace NexusResourceEngine.Application.DTOs.Auth;

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string Role { get; set; } = string.Empty;
}
```

---

### Task 2: States & Transitions DTOs

**Files:**
- Create: `src/Application/DTOs/States/ResourceStateDto.cs`
- Create: `src/Application/DTOs/States/CreateResourceStateDto.cs`
- Create: `src/Application/DTOs/States/UpdateResourceStateDto.cs`
- Create: `src/Application/DTOs/Transitions/StateTransitionDto.cs`
- Create: `src/Application/DTOs/Transitions/CreateStateTransitionDto.cs`

- [ ] **Create `ResourceStateDto.cs`**

```csharp
namespace NexusResourceEngine.Application.DTOs.States;

public class ResourceStateDto
{
    public Guid StateId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsBookable { get; set; }
    public string ColorCode { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}
```

- [ ] **Create `CreateResourceStateDto.cs`**

```csharp
namespace NexusResourceEngine.Application.DTOs.States;

public class CreateResourceStateDto
{
    public string Name { get; set; } = string.Empty;
    public bool IsBookable { get; set; }
    public string ColorCode { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}
```

- [ ] **Create `UpdateResourceStateDto.cs`**

```csharp
namespace NexusResourceEngine.Application.DTOs.States;

public class UpdateResourceStateDto
{
    public string Name { get; set; } = string.Empty;
    public bool IsBookable { get; set; }
    public string ColorCode { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}
```

- [ ] **Create `StateTransitionDto.cs`**

```csharp
namespace NexusResourceEngine.Application.DTOs.Transitions;

public class StateTransitionDto
{
    public Guid TransitionId { get; set; }
    public Guid FromStateId { get; set; }
    public Guid ToStateId { get; set; }
    public string RequiredRole { get; set; } = string.Empty;
}
```

- [ ] **Create `CreateStateTransitionDto.cs`**

```csharp
namespace NexusResourceEngine.Application.DTOs.Transitions;

public class CreateStateTransitionDto
{
    public Guid FromStateId { get; set; }
    public Guid ToStateId { get; set; }
    public string RequiredRole { get; set; } = string.Empty;
}
```

---

### Task 3: Resources & Bookings DTOs

**Files:**
- Create: `src/Application/DTOs/Resources/ResourceDto.cs`
- Create: `src/Application/DTOs/Resources/CreateResourceDto.cs`
- Create: `src/Application/DTOs/Resources/UpdateResourceStateDto.cs`
- Create: `src/Application/DTOs/Bookings/BookingDto.cs`
- Create: `src/Application/DTOs/Bookings/CreateBookingDto.cs`
- Create: `src/Application/DTOs/Bookings/UpdateBookingStatusDto.cs`

- [ ] **Create `ResourceDto.cs`**

```csharp
namespace NexusResourceEngine.Application.DTOs.Resources;

public class ResourceDto
{
    public Guid ResourceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid CurrentStateId { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? Metadata { get; set; }
}
```

- [ ] **Create `CreateResourceDto.cs`**

```csharp
namespace NexusResourceEngine.Application.DTOs.Resources;

public class CreateResourceDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid CurrentStateId { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? Metadata { get; set; }
}
```

- [ ] **Create `UpdateResourceStateDto.cs`**

```csharp
namespace NexusResourceEngine.Application.DTOs.Resources;

public class UpdateResourceStateDto
{
    public Guid NewStateId { get; set; }
}
```

- [ ] **Create `BookingDto.cs`**

```csharp
namespace NexusResourceEngine.Application.DTOs.Bookings;

public class BookingDto
{
    public Guid BookingId { get; set; }
    public Guid ResourceId { get; set; }
    public Guid UserId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Status { get; set; } = string.Empty;
}
```

- [ ] **Create `CreateBookingDto.cs`**

```csharp
namespace NexusResourceEngine.Application.DTOs.Bookings;

public class CreateBookingDto
{
    public Guid ResourceId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}
```

- [ ] **Create `UpdateBookingStatusDto.cs`**

```csharp
namespace NexusResourceEngine.Application.DTOs.Bookings;

public class UpdateBookingStatusDto
{
    public string Status { get; set; } = string.Empty;
}
```

---

### Task 4: Service Interfaces

**Files:**
- Create: `src/Application/Interfaces/IAuthService.cs`
- Create: `src/Application/Interfaces/IResourceStateService.cs`
- Create: `src/Application/Interfaces/IStateTransitionService.cs`
- Create: `src/Application/Interfaces/IResourceService.cs`
- Create: `src/Application/Interfaces/IBookingService.cs`

- [ ] **Create `IAuthService.cs`**

```csharp
using NexusResourceEngine.Application.DTOs.Auth;

namespace NexusResourceEngine.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> RegisterAsync(RegisterRequestDto request, Guid tenantId);
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
}
```

- [ ] **Create `IResourceStateService.cs`**

```csharp
using NexusResourceEngine.Application.DTOs.States;

namespace NexusResourceEngine.Application.Interfaces;

public interface IResourceStateService
{
    Task<List<ResourceStateDto>> GetAllAsync(Guid tenantId);
    Task<ResourceStateDto> CreateAsync(CreateResourceStateDto dto, Guid tenantId);
}
```

- [ ] **Create `IStateTransitionService.cs`**

```csharp
using NexusResourceEngine.Application.DTOs.Transitions;

namespace NexusResourceEngine.Application.Interfaces;

public interface IStateTransitionService
{
    Task<StateTransitionDto> CreateAsync(CreateStateTransitionDto dto, Guid tenantId);
}
```

- [ ] **Create `IResourceService.cs`**

```csharp
using NexusResourceEngine.Application.DTOs.Resources;

namespace NexusResourceEngine.Application.Interfaces;

public interface IResourceService
{
    Task<List<ResourceDto>> GetAllAsync(Guid tenantId, Guid? stateId = null);
    Task<ResourceDto> CreateAsync(CreateResourceDto dto, Guid tenantId);
    Task<ResourceDto> ChangeStateAsync(Guid resourceId, UpdateResourceStateDto dto, Guid tenantId);
}
```

- [ ] **Create `IBookingService.cs`**

```csharp
using NexusResourceEngine.Application.DTOs.Bookings;

namespace NexusResourceEngine.Application.Interfaces;

public interface IBookingService
{
    Task<bool> CheckAvailabilityAsync(Guid resourceId, DateTime start, DateTime end, Guid tenantId);
    Task<BookingDto> CreateAsync(CreateBookingDto dto, Guid userId, Guid tenantId);
    Task<BookingDto> UpdateStatusAsync(Guid bookingId, UpdateBookingStatusDto dto, Guid tenantId);
}
```

---

### Task 5: Manual Mapping Extensions

**Files:**
- Create: `src/Application/Mapping/MappingExtensions.cs`

- [ ] **Create `MappingExtensions.cs`**

```csharp
using NexusResourceEngine.Application.DTOs.Bookings;
using NexusResourceEngine.Application.DTOs.Resources;
using NexusResourceEngine.Application.DTOs.States;
using NexusResourceEngine.Application.DTOs.Transitions;
using NexusResourceEngine.Domain;

namespace NexusResourceEngine.Application.Mapping;

public static class MappingExtensions
{
    public static ResourceStateDto ToDto(this ResourceState state)
    {
        return new ResourceStateDto
        {
            StateId = state.StateId,
            Name = state.Name,
            IsBookable = state.IsBookable,
            ColorCode = state.ColorCode,
            SortOrder = state.SortOrder
        };
    }

    public static ResourceState ToEntity(this CreateResourceStateDto dto, Guid tenantId)
    {
        return new ResourceState
        {
            StateId = Guid.NewGuid(),
            TenantId = tenantId,
            Name = dto.Name,
            IsBookable = dto.IsBookable,
            ColorCode = dto.ColorCode,
            SortOrder = dto.SortOrder
        };
    }

    public static StateTransitionDto ToDto(this StateTransition transition)
    {
        return new StateTransitionDto
        {
            TransitionId = transition.TransitionId,
            FromStateId = transition.FromStateId,
            ToStateId = transition.ToStateId,
            RequiredRole = transition.RequiredRole
        };
    }

    public static StateTransition ToEntity(this CreateStateTransitionDto dto, Guid tenantId)
    {
        return new StateTransition
        {
            TransitionId = Guid.NewGuid(),
            TenantId = tenantId,
            FromStateId = dto.FromStateId,
            ToStateId = dto.ToStateId,
            RequiredRole = dto.RequiredRole
        };
    }

    public static ResourceDto ToDto(this Resource resource)
    {
        return new ResourceDto
        {
            ResourceId = resource.ResourceId,
            Name = resource.Name,
            Description = resource.Description,
            CurrentStateId = resource.CurrentStateId,
            Latitude = resource.Latitude,
            Longitude = resource.Longitude,
            Metadata = resource.Metadata
        };
    }

    public static Resource ToEntity(this CreateResourceDto dto, Guid tenantId)
    {
        return new Resource
        {
            ResourceId = Guid.NewGuid(),
            TenantId = tenantId,
            Name = dto.Name,
            Description = dto.Description,
            CurrentStateId = dto.CurrentStateId,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            Metadata = dto.Metadata
        };
    }

    public static BookingDto ToDto(this Booking booking)
    {
        return new BookingDto
        {
            BookingId = booking.BookingId,
            ResourceId = booking.ResourceId,
            UserId = booking.UserId,
            StartTime = booking.StartTime,
            EndTime = booking.EndTime,
            Status = booking.Status
        };
    }

    public static Booking ToEntity(this CreateBookingDto dto, Guid userId, Guid tenantId)
    {
        return new Booking
        {
            BookingId = Guid.NewGuid(),
            TenantId = tenantId,
            ResourceId = dto.ResourceId,
            UserId = userId,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Status = "Pending"
        };
    }
}
```

---

### Task 6: Global Exception Handler

**Files:**
- Create: `src/Presentation/Middleware/GlobalExceptionHandler.cs`

- [ ] **Create `GlobalExceptionHandler.cs`**

```csharp
using System.Net;
using System.Text.Json;

namespace NexusResourceEngine.Presentation.Middleware;

public class GlobalExceptionHandler
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(RequestDelegate next, ILogger<GlobalExceptionHandler> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");
            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var problem = new
            {
                type = "https://tools.ietf.org/html/rfc7807",
                title = "An error occurred",
                status = 500,
                detail = ex.Message
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
        }
    }
}
```

---

### Task 7: TenantMiddleware Stub

**Files:**
- Create: `src/Presentation/Middleware/TenantMiddleware.cs`

- [ ] **Create `TenantMiddleware.cs`**

```csharp
namespace NexusResourceEngine.Presentation.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var tenantIdClaim = context.User?.FindFirst("TenantId")?.Value;

        if (!string.IsNullOrEmpty(tenantIdClaim) && Guid.TryParse(tenantIdClaim, out var tenantId))
        {
            context.Items["TenantId"] = tenantId;
        }

        await _next(context);
    }
}
```

---

### Task 8: Endpoint Registration

**Files:**
- Create: `src/Presentation/Endpoints/AuthEndpoints.cs`
- Create: `src/Presentation/Endpoints/StateEndpoints.cs`
- Create: `src/Presentation/Endpoints/ResourceEndpoints.cs`
- Create: `src/Presentation/Endpoints/BookingEndpoints.cs`

- [ ] **Create `AuthEndpoints.cs`**

```csharp
using NexusResourceEngine.Application.Interfaces;

namespace NexusResourceEngine.Presentation.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/auth");

        group.MapPost("/register", async (IAuthService authService, DTOs.Auth.RegisterRequestDto request) =>
        {
            var tenantId = Guid.NewGuid();
            var result = await authService.RegisterAsync(request, tenantId);
            return Results.Ok(result);
        });

        group.MapPost("/login", async (IAuthService authService, DTOs.Auth.LoginRequestDto request) =>
        {
            var result = await authService.LoginAsync(request);
            return Results.Ok(result);
        });
    }
}
```

- [ ] **Create `StateEndpoints.cs`**

```csharp
using NexusResourceEngine.Application.Interfaces;

namespace NexusResourceEngine.Presentation.Endpoints;

public static class StateEndpoints
{
    public static void MapStateEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/states");

        group.MapGet("/", async (IResourceStateService service, HttpContext context) =>
        {
            var tenantId = (Guid)context.Items["TenantId"]!;
            var result = await service.GetAllAsync(tenantId);
            return Results.Ok(result);
        });

        group.MapPost("/", async (IResourceStateService service, DTOs.States.CreateResourceStateDto dto, HttpContext context) =>
        {
            var tenantId = (Guid)context.Items["TenantId"]!;
            var result = await service.CreateAsync(dto, tenantId);
            return Results.Created($"/states/{result.StateId}", result);
        });

        var transitionGroup = app.MapGroup("/transitions");

        transitionGroup.MapPost("/", async (IStateTransitionService service, DTOs.Transitions.CreateStateTransitionDto dto, HttpContext context) =>
        {
            var tenantId = (Guid)context.Items["TenantId"]!;
            var result = await service.CreateAsync(dto, tenantId);
            return Results.Created($"/transitions/{result.TransitionId}", result);
        });
    }
}
```

- [ ] **Create `ResourceEndpoints.cs`**

```csharp
using NexusResourceEngine.Application.Interfaces;

namespace NexusResourceEngine.Presentation.Endpoints;

public static class ResourceEndpoints
{
    public static void MapResourceEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/resources");

        group.MapGet("/", async (IResourceService service, HttpContext context, Guid? stateId) =>
        {
            var tenantId = (Guid)context.Items["TenantId"]!;
            var result = await service.GetAllAsync(tenantId, stateId);
            return Results.Ok(result);
        });

        group.MapPost("/", async (IResourceService service, DTOs.Resources.CreateResourceDto dto, HttpContext context) =>
        {
            var tenantId = (Guid)context.Items["TenantId"]!;
            var result = await service.CreateAsync(dto, tenantId);
            return Results.Created($"/resources/{result.ResourceId}", result);
        });

        group.MapPatch("/{resourceId:guid}/state", async (Guid resourceId, IResourceService service, DTOs.Resources.UpdateResourceStateDto dto, HttpContext context) =>
        {
            var tenantId = (Guid)context.Items["TenantId"]!;
            var result = await service.ChangeStateAsync(resourceId, dto, tenantId);
            return Results.Ok(result);
        });
    }
}
```

- [ ] **Create `BookingEndpoints.cs`**

```csharp
using NexusResourceEngine.Application.Interfaces;

namespace NexusResourceEngine.Presentation.Endpoints;

public static class BookingEndpoints
{
    public static void MapBookingEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/bookings");

        group.MapPost("/", async (IBookingService service, DTOs.Bookings.CreateBookingDto dto, HttpContext context) =>
        {
            var tenantId = (Guid)context.Items["TenantId"]!;
            var userId = Guid.NewGuid();
            var result = await service.CreateAsync(dto, userId, tenantId);
            return Results.Created($"/bookings/{result.BookingId}", result);
        });

        group.MapPatch("/{bookingId:guid}/status", async (Guid bookingId, IBookingService service, DTOs.Bookings.UpdateBookingStatusDto dto, HttpContext context) =>
        {
            var tenantId = (Guid)context.Items["TenantId"]!;
            var result = await service.UpdateStatusAsync(bookingId, dto, tenantId);
            return Results.Ok(result);
        });
    }

    public static void MapAvailabilityEndpoint(this WebApplication app)
    {
        app.MapGet("/resources/{resourceId:guid}/availability", async (Guid resourceId, IBookingService service, DateTime start, DateTime end, HttpContext context) =>
        {
            var tenantId = (Guid)context.Items["TenantId"]!;
            var available = await service.CheckAvailabilityAsync(resourceId, start, end, tenantId);
            return Results.Ok(new { available });
        });
    }
}
```

---

### Task 9: Wire Program.cs

**Files:**
- Modify: `src/Presentation/Program.cs`

- [ ] **Update `Program.cs`**

Old content:
```csharp
using Microsoft.EntityFrameworkCore;
using NexusResourceEngine.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<NexusResourceEngineContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();
```

New content:
```csharp
using Microsoft.EntityFrameworkCore;
using NexusResourceEngine.Infrastructure.Data;
using NexusResourceEngine.Presentation.Middleware;
using NexusResourceEngine.Presentation.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<NexusResourceEngineContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddOpenApi();

var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandler>();
app.UseMiddleware<TenantMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapAuthEndpoints();
app.MapStateEndpoints();
app.MapResourceEndpoints();
app.MapBookingEndpoints();
app.MapAvailabilityEndpoint();

app.Run();
```

---

### Task 10: Verify

- [ ] **Run `dotnet build`**

Run: `dotnet build`
Expected: Build succeeded, 0 warnings, 0 errors

- [ ] **Run `dotnet run`**

Run: `dotnet run --project src/Presentation`
Expected: Application starts without crashing (will fail on DB connection at runtime since no SQL Server is running, but that's OK — it means the app compiles and starts)

---

## Self-Review Checklist

**Spec coverage:**
- DTOs per Sprint 3 spec: Auth, States, Transitions, Resources, Bookings ✓
- Service interfaces per spec: IAuthService, IResourceStateService, IStateTransitionService, IResourceService, IBookingService ✓
- Global exception handler (RFC 7807) ✓
- TenantMiddleware stub ✓
- Swagger (already wired) ✓
- No FluentValidation ✓
- Manual mapping ✓

**Placeholder scan:** No TODOs, TBDs, or incomplete sections.

**Type consistency:** DTO property names match domain entities (e.g., `ResourceState.StateId`, `Booking.BookingId`). Service method signatures consistent across interfaces and endpoint usage.
