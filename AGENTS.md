# AGENTS.md — NexusResource Engine

## Current state
Active development. Code scaffold + domain entities in place. Two authoritative spec docs at `docs/`:
- `docs/PRODUCT-SPEC.md` — feature requirements, data model, API endpoints
- `docs/ENGINE-DESIGN.md` — architecture decisions
- `docs/SPRINT-PLAN.MD` - Current state of project

When docs conflict, `docs/PRODUCT-SPEC.md` is the source of truth for features; `docs/ENGINE-DESIGN.md` for architecture.

## Architecture
Clean Architecture (Onion) with 4 projects:

| Project | Path | Depends on |
|---|---|---|
| `NexusResourceEngine.Domain` | `src/Domain/` | nothing |
| `NexusResourceEngine.Application` | `src/Application/` | Domain |
| `NexusResourceEngine.Infrastructure` | `src/Infrastructure/` | Domain, Application |
| `NexusResourceEngine` | `src/Presentation/` | Application, Infrastructure |

Dependency inversion: Infrastructure references Application to implement its service interfaces (ports/adapters pattern). Application never references Infrastructure.

## .NET / toolchain
- .NET 10.0 (`net10.0` TFM). SDK confirmed: 10.0.203.
- Use `dotnet new classlib -o src/X` for layer projects, `dotnet new webapi -o src/Presentation` for the API host.
- All NuGet packages should be latest stable versions.
- **Serilog** is the single logging provider, configured in `Program.cs` with Console sink. `UseSerilogRequestLogging()` traces all endpoints automatically.

## Key conventions (deviate from defaults)
- **No enums for roles/states** — roles and resource states are `string`-based for tenant flexibility (contradicts the ProductSpec enum tables; follow the design doc here).
- **JWT TenantId** — every request carries `TenantId` in JWT claims; `TenantMiddleware` extracts it. All queries must filter by it.
- **Dynamic state machine** — state transitions stored in DB with `RequiredRole`, validated at runtime.
- **Booking lifecycle**: `Request → (Approval) → Active → Completed`. Race conditions handled via DB transactions/optimistic concurrency.
- **Error responses**: RFC 7807 Problem Details via global exception handler.
- **Database**: SQL Server with EF Core (not PostgreSQL; the design doc is authoritative on this). JSONB in the spec refers to EF Core's JSON columns.
- **NuGet dependency policy**: Prefer Microsoft-owned packages first. Only use third-party libraries when no Microsoft alternative exists. This keeps the stack aligned with the ASP.NET Core ecosystem and minimizes external supply chain risk.
- **Logging**: Use `ILogger<T>` via DI with structured placeholders. Never `Console.WriteLine` or string interpolation in log messages. See `docs/ENGINE-DESIGN.md` → Logging Strategy for full conventions.

## Sprint plan
`docs/SPRINT-PLAN.md` at repo root defines the build order. Each sprint ends with `dotnet build` verification.

## Testing
xUnit test project at `src/Tests/`. Test per sprint as defined in sprint plan.

### Patterns
- **Service layer tests** use InMemory EF Core (`UseInMemoryDatabase`) to test service implementations against a real DbContext. Each test gets a unique database name for isolation.
- **Endpoint integration tests** use `WebApplicationFactory<Program>` (from `Microsoft.AspNetCore.Mvc.Testing`) with stubbed service interfaces to verify HTTP routing, model binding, and response codes. Stubs are registered as singletons to share state across requests within a test class.
- **No mocking libraries** — prefer InMemory EF Core for service tests and hand-written stubs for endpoint tests. Avoid Moq/NSubstitute/etc.

### Requirements
All public endpoints must have at least one test per HTTP method verifying the **success path** (valid input → expected 2xx response with correct body shape). Error-path tests are optional but encouraged.

## Deployment
Dockerized. Expect a `Dockerfile` at repo root and `docker-compose.yml` for local dev (API + SQL Server container).
