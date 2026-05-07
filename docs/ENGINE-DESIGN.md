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

## Security & Validation
- JWT-based authentication with role-based access control
- Global exception handling returning RFC 7807 Problem Details
- Password hashing using industry-standard algorithms

## Technology Stack
- .NET 10.0 with Minimal APIs
- Entity Framework Core with SQL Server provider
- Swagger/OpenAPI for API documentation
- Clean Architecture with separate class library projects

## APIs Overview
Following the specification:
- Auth: POST /auth/register, POST /auth/login
- States: GET /states, POST /states
- Transitions: POST /transitions
- Resources: GET /resources, POST /resources, PATCH /resources/{id}/state
- Bookings: GET /resources/{id}/availability, POST /bookings, PATCH /bookings/{id}/status
