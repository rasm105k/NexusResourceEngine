# Sprint 5 — Dynamic State Machine

## Overview
Implement the state and transition services that let tenants define custom resource states and legal transitions between them. Runtime validation ensures data integrity at creation time; full enforcement of transition rules during resource state changes comes in Sprint 6.

## Customer workflow
1. Tenant admin defines states (e.g. "Available", "Under Repair", "Retired").
2. Tenant admin defines transitions between states with a `RequiredRole` gate.
3. System validates at creation: referenced states exist, no duplicates, role is non-empty.
4. In Sprint 6, `PATCH /resources/{id}/state` will enforce these transitions at runtime.

## Implementation

### ResourceStateService
- **GetAllAsync(tenantId)** — query `ResourceStates` filtered by tenant, ordered by `SortOrder`, map to DTOs.
- **CreateAsync(dto, tenantId)** — validate `Name` not null/empty, map to entity, save, return DTO.

### StateTransitionService
- **CreateAsync(dto, tenantId)** — validate:
  - `FromStateId` references an existing `ResourceState` for this tenant.
  - `ToStateId` references an existing `ResourceState` for this tenant.
  - No duplicate transition exists (same `FromStateId` + `ToStateId` for this tenant).
  - `RequiredRole` is not null/empty.
  - Map to entity, save, return DTO.
  - Throw `InvalidOperationException` with descriptive message on validation failure.

### DI registration
Both services scoped in `Program.cs`:
```csharp
builder.Services.AddScoped<IResourceStateService, ResourceStateService>();
builder.Services.AddScoped<IStateTransitionService, StateTransitionService>();
```

### Error handling
Validation failures throw `InvalidOperationException` which is caught by `GlobalExceptionHandler` and returned as RFC 7807 Problem Details (400 Bad Request).

## Testing

### Service tests (InMemory EF Core)
- **ResourceStateServiceTests:**
  - `Create_ValidDto_ReturnsDtoWithExpectedFields`
  - `Create_EmptyName_ThrowsInvalidOperation`
  - `GetAll_ReturnsStatesForTenant` (seed 2 states for tenant A, 1 for tenant B; verify only A's returned)
  - `GetAll_ReturnsStatesSortedBySortOrder`
- **StateTransitionServiceTests:**
  - `Create_ValidDto_ReturnsDto`
  - `Create_FromStateNotFound_ThrowsInvalidOperation`
  - `Create_ToStateNotFound_ThrowsInvalidOperation`
  - `Create_DuplicateTransition_ThrowsInvalidOperation`
  - `Create_EmptyRequiredRole_ThrowsInvalidOperation`

### Endpoint integration tests (WebApplicationFactory + stubs)
- `PostStates_Returns201WithStateDto`
- `PostTransitions_Returns201WithTransitionDto`
