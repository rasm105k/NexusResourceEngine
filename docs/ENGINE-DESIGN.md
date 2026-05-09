# NexusResource Engine Design Document

## Overview
This document describes the design for the NexusResource Engine (NRE), a headless, multi-tenant API backend for managing shared resources in organizations using Clean Architecture with class libraries.

## Architecture
We're implementing a Clean Architecture/Onion Architecture with four layers:
1. **Domain Layer** (NexusResourceEngine.Domain): Entities and business logic
2. **Application Layer** (NexusResourceEngine.Application): Application services, DTOs, and validators
3. **Infrastructure Layer** (NexusResourceEngine.Infrastructure): Data access (EF Core with SQL Server). No references to the domain layer from here!
4. **Presentation Layer** (NexusResourceEngine): ASP.NET Core Minimal APIs

## Key Components

### 1. Domain Layer (NexusResourceEngine.Domain)
Contains enterprise-wide business objects and business logic:
- **Entities**: Tenant, User, ResourceState, StateTransition, Resource, Booking
- **Enums**: Not needed as roles and states are string-based for flexibility
- No dependencies on other layers

### 2. Application Layer (NexusResourceEngine.Application)
Contains application-specific business logic:
- **DTOs**: Data Transfer Objects for API communication
  - Auth: RegisterRequestDto, LoginRequestDto
  - States: ResourceStateDto, CreateResourceStateDto, UpdateResourceStateDto
- **Services**: Interfaces defining application operations
  - ITenantService, IResourceService, IBookingService, IStateService

### 3. Infrastructure Layer (NexusResourceEngine.Infrastructure)
Implements infrastructure concerns:
- **Data Access**: NexusResourceEngineContext (EF Core with SQL Server)
- Database configuration and mappings
- No dependencies on Application or Presentation layers

### 4. Presentation Layer (NexusResourceEngine)
Handles HTTP requests and responses:
- **Minimal APIs**: Clean, lightweight API endpoints
- **Middleware**: TenantMiddleware for multi-tenancy
- **Authentication**: JWT-based authentication
- **Validation**: Manual validation in service layer (no FluentValidation)
- **Documentation**: Swagger/OpenAPI

## Multi-Tenancy Implementation
- TenantMiddleware extracts TenantId from JWT claims
- All queries filter by TenantId using EF Core query filters (to be implemented)
- Database tables include TenantId foreign key
- Designed to prevent data leakage between tenants

## Dynamic State Machine
- ResourceStates table stores custom states per tenant
- Each state has IsBookable property (false = cannot be reserved)
- StateTransitions table defines legal transitions with required roles
- Business logic validates state transitions

## Booking & Reservation System
- Time-slot management prevents overlapping bookings
- Booking lifecycle: Request → (Approval) → Active → Completed
- Validation checks resource state (IsBookable) and time conflicts
- Database constraints and transactions prevent race conditions

## Role-Based Access Control (RBAC)

### Current implementation (v1)
- Two roles: `Admin` and `Member`, stored as a string on `User.Role`.
- `Admin` — full access: manage users (create, change roles), manage resources, states, and transitions.
- `Member` — basic access: book resources, transition states where `RequiredRole` matches.
- JWT carries a `ClaimTypes.Role` claim; ASP.NET Core `[Authorize(Roles = "...")]` enforces at endpoint level.
- `PATCH /users/{id}/role` prevents self-demotion to avoid orphaned orgs.

### Future (dedicated permission table)
Replace the string-based role with a `Permissions` table that maps roles to granular flags:
- `CanManageUsers`, `CanManageResources`, `CanTransitionStates`, `CanManageStates`
- Allows tenants to define custom roles beyond Admin/Member.
- Planned for post-Dockerization backlog.

## Security & Validation
- JWT-based authentication with role-based access control
- Global exception handling returning RFC 7807 Problem Details
- Password hashing using industry-standard algorithms

## Technology Stack
- .NET 10.0 with Minimal APIs
- Entity Framework Core with SQL Server provider
- Swagger/OpenAPI for API documentation via Scalar
- Serilog for structured logging
- Clean Architecture with separate class library projects

## Logging Strategy

### Framework
Serilog is configured at application startup as the sole logging provider. Every `ILogger<T>` injected via the DI container writes through Serilog.

### Pipeline
- `UseSerilogRequestLogging()` — traces every endpoint with method, path, status code, duration in a single line.
- `TenantMiddleware` pushes `TenantId` into `LogContext` so every downstream log line is tagged with the tenant.
- Console sink with structured output template for Development. Additional sinks (File, Seq, Elasticsearch) can be added by extending the logger configuration — no code changes needed.

### Output template
`[{Timestamp:HH:mm:ss} {Level:u3}] {TenantId:l} {Message:lj}{NewLine}{Exception}`

Shows tenant ID (when available), message, and exception details. The `{TenantId:l}` slot is empty for unauthenticated requests.

### Conventions (all code MUST follow)
1. **Use `ILogger<T>` via DI** — never `Console.WriteLine` or `Debug.WriteLine`.
2. **Structured logging** — named placeholders only, never string interpolation:
   - ✅ `_logger.LogInformation("User {UserId} booked {ResourceId}", userId, resourceId);`
   - ❌ `_logger.LogInformation($"User {userId} booked {resourceId}");`
3. **Log levels**:
   - `LogDebug` — detailed diagnostic info (not for production)
   - `LogInformation` — high-level operations (booking created, state changed)
   - `LogWarning` — unexpected but handled situations (validation failure)
   - `LogError` — exceptions, DB failures, auth failures
4. **Semantic enrichment** — log context properties (`TenantId`, `UserId`) are automatically attached by middleware; do not repeat them in messages.

### Configurability
The logger is built in code with sensible defaults. To swap or add sinks later (e.g., a JSON file for production), add a `WriteTo` call in the `LoggerConfiguration` chain — no changes to individual services or controllers.

## APIs Overview
Following the specification:
- Auth: POST /auth/register, POST /auth/login
- States: GET /states, POST /states
- Transitions: POST /transitions
- Resources: GET /resources, POST /resources, PATCH /resources/{id}/state
- Bookings: GET /resources/{id}/availability, POST /bookings, PATCH /bookings/{id}/status
