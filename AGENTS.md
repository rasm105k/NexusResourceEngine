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

## Sprint plan
`docs/SPRINT-PLAN.md` at repo root defines the build order. Each sprint ends with `dotnet build` verification.

## Testing
xUnit test project at `src/Tests/`. Test per sprint as defined in sprint plan.

## Deployment
Dockerized. Expect a `Dockerfile` at repo root and `docker-compose.yml` for local dev (API + SQL Server container).
