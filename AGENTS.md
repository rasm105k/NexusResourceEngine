# AGENTS.md — NexusResource Engine

## Current state
Design-phase repository. No code yet. Two authoritative spec docs at root:
- `ProductSpec.md` — feature requirements, data model, API endpoints
- `nexusresource-engine-design.md` — architecture decisions

When docs conflict, `ProductSpec.md` is the source of truth for features; `nexusresource-engine-design.md` for architecture.

## Architecture
Clean Architecture (Onion) with 4 projects:

| Project | Path | Depends on |
|---|---|---|
| `NexusResourceEngine.Domain` | `src/Domain/` | nothing |
| `NexusResourceEngine.Application` | `src/Application/` | Domain |
| `NexusResourceEngine.Infrastructure` | `src/Infrastructure/` | Domain |
| `NexusResourceEngine` | `src/Presentation/` | Application, Infrastructure |

Dependency inversion: Infrastructure never references Application or Presentation.

## .NET / toolchain
- .NET 10.0 (`net10.0` TFM). SDK confirmed: 10.0.203.
- Use `dotnet new classlib -o src/X` for layer projects, `dotnet new webapi -o src/Presentation` for the API host.
- All NuGet packages should be latest stable versions.

## Key conventions (deviate from defaults)
- **No enums for roles/states** — roles and resource states are `string`-based for tenant flexibility (contradicts the ProductSpec enum tables; follow the design doc here).
- **JWT TenantId** — every request carries `TenantId` in JWT claims; `TenantMiddleware` extracts it. All queries must filter by it.
- **Dynamic state machine** — state transitions stored in DB with `RequiredRole`, validated at runtime.
- **Booking lifecycle**: `Request → (Approval) → Active → Completed`. Race conditions handled via DB transactions/optimistic concurrency.
- **Error responses**: RFC 7807 Problem Details via global exception handler.
- **Database**: SQL Server with EF Core (not PostgreSQL; the design doc is authoritative on this). JSONB in the spec refers to EF Core's JSON columns.

## Scaffold command sequence
```bash
dotnet new sln -n NexusResourceEngine
dotnet new classlib -o src/Domain -n NexusResourceEngine.Domain
dotnet new classlib -o src/Application -n NexusResourceEngine.Application
dotnet new classlib -o src/Infrastructure -n NexusResourceEngine.Infrastructure
dotnet new webapi -o src/Presentation -n NexusResourceEngine
dotnet sln add src/Domain src/Application src/Infrastructure src/Presentation
```

## Testing
No test project defined yet. Follow existing patterns if one is added (likely xUnit + `src/Tests/`).

## Deployment
Dockerized. Expect a `Dockerfile` at repo root and `docker-compose.yml` for local dev (API + SQL Server container).
