Project Specification: NexusResource Engine (NRE)
1. Vision & Core Purpose
NexusResource Engine (NRE) is a Headless, Multi-tenant API Backend designed for the sharing economy. Its primary purpose is to manage the lifecycle, reservation, and state-tracking of shared resources (assets) within a closed or semi-closed organization (e.g., a company, a municipality, or an NGO).

Unlike static inventory systems, NRE focuses on Resource Circulation. It provides a flexible engine where the "Tenant" (the organization) defines the rules, the states, and the workflows for how resources are borrowed and returned.

2. Core Business Logic (The "Rules")
A. Multi-Tenancy (Isolation)
The system is a SaaS architecture. Every piece of data (Users, Resources, States, Bookings) must be linked to a TenantId.
Data leak between tenants is a critical failure. All queries must be scoped by TenantId.
B. Dynamic State Machine (The Core Feature)
Resources do not have hardcoded states.
Custom States: Tenants can create their own states (e.g., "Ready", "In Repair", "Dirty", "Awaiting Approval").
State Properties: Each state has a boolean IsBookable. If false, the resource cannot be reserved regardless of its availability.
State Transitions: Tenants can define "Legal Transitions". (e.g., a resource cannot move from "Broken" $\rightarrow$ "Available" without passing through "Repaired").
Role-Based Transitions: Certain state transitions require specific User Roles (e.g., only a "Technician" can move a resource to "Available").
C. Reservation & Booking Logic
Time-Slot Management: The system must prevent overlapping bookings for the same resource.
Booking Lifecycle: Request $\rightarrow$ Approval (Optional) $\rightarrow$ Active $\rightarrow$ Completed/Returned.
Validation: A resource can only be booked if its current state is marked as IsBookable = true.
3. Technical Specifications
Language/Framework: C# / .NET 10
Nuget packages latest version - always.
Architecture: REST API (Clean Architecture / Onion Architecture).
Database: SQL (Relational data is mandatory).
Authentication: JWT-based authentication with Role-Based Access Control (RBAC).
Deployment: Dockerized environment.
4. Data Model (Schema)
Tenants
TenantId (GUID, PK)
OrganizationName (String)
CreatedAt (DateTime)
Users
UserId (GUID, PK)
TenantId (GUID, FK)
Username (String)
Email (String)
PasswordHash (String)
Role (Enum: Admin, Moderator, User)
ResourceStates
StateId (GUID, PK)
TenantId (GUID, FK)
Name (String)
IsBookable (Boolean)
ColorCode (String/Hex)
SortOrder (Int)
StateTransitions
TransitionId (GUID, PK)
TenantId (GUID, FK)
FromStateId (GUID, FK $\rightarrow$ ResourceStates)
ToStateId (GUID, FK $\rightarrow$ ResourceStates)
RequiredRole (String)
Resources
ResourceId (GUID, PK)
TenantId (GUID, FK)
Name (String)
Description (Text)
CurrentStateId (GUID, FK $\rightarrow$ ResourceStates)
Latitude/Longitude (Decimal)
Metadata (JSONB - for custom resource attributes)
Bookings
BookingId (GUID, PK)
TenantId (GUID, FK)
ResourceId (GUID, FK)
UserId (GUID, FK)
StartTime (DateTime)
EndTime (DateTime)
Status (String: Pending, Confirmed, Cancelled, Completed)
5. API Endpoint Map (Functional Requirements)
Tenant & User Management
POST /auth/register - Create user and tenant.
POST /auth/login - Returns JWT.
State Management
GET /states - List all states for the tenant.
POST /states - Create a new custom state.
POST /transitions - Define a legal jump between two states.
Resource Management
GET /resources - List resources (with optional filters: state, category, location).
POST /resources - Create a new resource.
PATCH /resources/{id}/state - Move a resource to a new state (Must validate against StateTransitions).
Booking Engine
GET /resources/{id}/availability - Check if a resource is free for a specific time range.
POST /bookings - Create a booking request (Must validate IsBookable state and time-overlaps).
PATCH /bookings/{id}/status - Approve or cancel a booking.
6. Implementation Constraints for the LLM
Middleware: Implement a TenantMiddleware that extracts TenantId from the JWT or Header and injects it into the request context.
Concurrency: Handle potential "Race Conditions" when two users try to book the same resource at the same millisecond (use Database Transactions or Optimistic Concurrency).
Error Handling: Use a Global Exception Handler to return standardized Problem Details (RFC 7807).