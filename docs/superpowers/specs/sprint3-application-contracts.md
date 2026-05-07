# Sprint 3 — Application Contracts + API Host

## Scope
Implement service interfaces, DTOs, and the API host layer. No business logic implementation.

## DTOs (Application layer)
- **Auth:** `RegisterRequestDto`, `LoginRequestDto`, `LoginResponseDto`
- **States:** `ResourceStateDto`, `CreateResourceStateDto`, `UpdateResourceStateDto`
- **Transitions:** `StateTransitionDto`, `CreateStateTransitionDto`
- **Resources:** `ResourceDto`, `CreateResourceDto`, `UpdateResourceStateDto`
- **Bookings:** `BookingDto`, `CreateBookingDto`, `UpdateBookingStatusDto`

Mapping: manual extension methods (`ToDto()`, `ToEntity()`) per entity.

## Service Interfaces (Application layer)
- `IAuthService` — `RegisterAsync`, `LoginAsync`
- `IResourceStateService` — `GetAllAsync(Guid tenantId)`, `CreateAsync(...)`
- `IStateTransitionService` — `CreateAsync(...)`
- `IResourceService` — `GetAllAsync(Guid tenantId, ...)`, `CreateAsync(...)`, `ChangeStateAsync(...)`
- `IBookingService` — `CheckAvailabilityAsync(...)`, `CreateAsync(...)`, `UpdateStatusAsync(...)`

All async, take `Guid tenantId` parameter.

## Presentation layer additions
- **Global exception handler** — middleware catching unhandled exceptions, returning RFC 7807 ProblemDetails JSON
- **TenantMiddleware stub** — extracts `TenantId` from JWT claim, stores in `HttpContext.Items["TenantId"]`
- **Swagger** — already wired via `AddOpenApi()` / `MapOpenApi()`
- **Endpoint registration** — minimal API endpoint groups mapping routes to service calls
- **`appsettings.json`** — connection string (already exists)

## Validation
Manual validation in service layer (no FluentValidation dependency).

## Dependencies
- `Application` project: references `Domain`
- `Presentation` project: references `Application`, `Infrastructure`
- No new NuGet packages needed

## Verification
- `dotnet build` and `dotnet run` both succeed
