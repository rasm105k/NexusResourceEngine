# Sprint Plan — NexusResource Engine

Each sprint produces a buildable, verifiable increment. `dotnet build` and `dotnet run` must both pass at sprint end.

---

## ✅ Sprint 1 — Foundation scaffold + test project (COMPLETED)

- `dotnet new sln`, 4 classlib/webapi projects with Clean Architecture references.
- Domain entities: `Tenant`, `User`, `ResourceState`, `StateTransition`, `Resource`, `Booking` (plain classes, no EF annotations).
- xUnit test project (`Tests/`) with no tests yet.
- ~~Verify: `dotnet build` succeeds.~~ ✅

## ✅ Sprint 2 — EF Core + Database (COMPLETED)

- DbSets are used from the docs/PRODUCT-SPEC to reflect the database models.
- `NexusResourceEngine.Infrastructure` references EF Core + SQL Server provider.
- `DbContext`, entity type configurations, initial migration.
- `appsettings.json` with SQL Server connection string.
- Tenant query filter skeleton on `DbContext`.
- ~~Verify: migration generates and applies; `dotnet build` succeeds.~~ ✅


## ✅ Sprint 3 — Application contracts + API host (COMPLETED)

- Application service interfaces (`ITenantService`, `IResourceService`, `IBookingService`, `IStateService`).
- DTOs with manual mapping.
- Web API project with global exception handler (RFC 7807 Problem Details), `TenantMiddleware` stub, Swagger.
- ~~Verify: `dotnet build` and `dotnet run` both succeed.~~ ✅

## ✅ Sprint 4 — Auth (register/login) (COMPLETED)

- `AuthService` implementation in Infrastructure using `PasswordHasher<User>` (no third-party libs).
- `POST /auth/register` — creates tenant + user with hashed password, returns JWT.
- `POST /auth/login` — validates credentials, returns JWT with `TenantId` claim.
- JWT bearer authentication configured in Program.cs.
- 9 tests: 6 unit (AuthService via InMemory EF Core) + 3 integration (endpoints via WebApplicationFactory + stub).
- ~~Verify: `dotnet build` and `dotnet test` both pass.~~ ✅

## ✅ Sprint 5 — Dynamic state machine (COMPLETED)

- `ResourceStateService` — `GetAllAsync` (sorted by SortOrder), `CreateAsync` (validates Name required).
- `StateTransitionService` — `CreateAsync` validates FromStateId/ToStateId exist, no duplicates, RequiredRole non-empty.
- `POST /dev/seed` — Development-only endpoint seeds Room Booking demo (5 states, 7 transitions, demo user).
- 8 new tests: 4 service (ResourceStateService) + 4 service (StateTransitionService) + 2 integration (endpoint stubs).
- ~~Verify: `dotnet build` and `dotnet test` both pass.~~ ✅

## Sprint 6 — Resource management (NEXT)

- `ResourceService` implementation.
- `GET /resources` (with filters), `POST /resources`, `PATCH /resources/{id}/state` (validates against allowed transitions + required role).

## Sprint 7 — Booking engine

- `BookingService` implementation.
- `GET /resources/{id}/availability`, `POST /bookings` (validates `IsBookable` + time overlap + optimistic concurrency), `PATCH /bookings/{id}/status`.

## Sprint 8 — Dockerization

- `Dockerfile` + `docker-compose.yml` (API + SQL Server container).
- Environment-based config.

---

## Testing

Tests deferred to Sprint 1+ (scaffold test project), written per-sprint thereafter.
