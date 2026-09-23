# Service Booking Management System — Codex Guide

> Purpose: give Codex/AI coding agents a fast, reliable map of the repository, architecture, current implementation, business rules, implementation priorities, and editing conventions.
>
> Source of truth for functional requirements: `Service_Booking_Demo_Project_Requirements.pdf`.
>
> Current priority: complete all mandatory project requirements before implementing optional or bonus features.

---

## 1. Project Summary

Full-stack service booking demo for two roles:

* `Customer`: login, view services, view available slots, create/view/cancel own bookings.
* `Admin`: manage services, staff, work schedules, and all bookings.

No payment integration is required.

### Core Stack

* Frontend: Next.js 15 App Router, React 19, TypeScript, Tailwind CSS v4.
* Backend: ASP.NET Core Web API, Entity Framework Core.
* Database: Microsoft SQL Server.
* Authentication: JWT Bearer.
* API documentation: Swagger/OpenAPI.
* Local database: Microsoft SQL Server.
* Docker Compose: deferred until all mandatory project requirements are complete.

### Current Development Priority

The project must prioritize mandatory functionality from the assignment PDF.

Do not spend implementation time on bonus infrastructure or optional architecture until mandatory requirements are complete.

Mandatory functionality takes priority over:

* Docker Compose;
* SignalR;
* Hangfire;
* advanced concurrency hardening;
* calendar UI;
* other optional enhancements.

---

## 2. Repository Structure

```text
service-booking/
├── src/
│   ├── frontend/                      # Next.js application
│   │   ├── src/
│   │   │   ├── app/                   # App Router pages/layouts
│   │   │   │   ├── page.tsx           # Landing page
│   │   │   │   ├── layout.tsx         # Root layout
│   │   │   │   ├── loading.tsx        # Global loading UI
│   │   │   │   ├── error.tsx          # Global error UI
│   │   │   │   ├── globals.css        # Tailwind v4 import + global styles
│   │   │   │   ├── login/
│   │   │   │   │   └── page.tsx       # Login screen
│   │   │   │   ├── services/
│   │   │   │   │   └── page.tsx       # Customer service list
│   │   │   │   ├── booking/
│   │   │   │   │   └── page.tsx       # Booking flow
│   │   │   │   ├── my-bookings/
│   │   │   │   │   └── page.tsx       # Customer bookings
│   │   │   │   └── admin/
│   │   │   │       ├── services/
│   │   │   │       │   └── page.tsx   # Admin service management
│   │   │   │       ├── schedules/
│   │   │   │       │   └── page.tsx   # Admin schedule management
│   │   │   │       └── bookings/
│   │   │   │           └── page.tsx   # Admin booking management
│   │   │   ├── components/            # Shared UI components
│   │   │   └── lib/
│   │   │       └── api.ts              # Shared API client
│   │   ├── .env.example                # Frontend environment template
│   │   ├── next.config.ts
│   │   ├── package.json
│   │   ├── postcss.config.mjs
│   │   └── tsconfig.json
│   │
│   └── backend/                        # ASP.NET Core Web API
│       ├── Controllers/
│       │   ├── HealthController.cs
│       │   └── ServicesController.cs
│       ├── Data/
│       │   └── AppDbContext.cs
│       ├── DTOs/
│       │   ├── Auth/
│       │   └── Services/
│       │       ├── CreateServiceRequest.cs
│       │       └── ServiceResponse.cs
│       ├── Entities/
│       │   ├── User.cs
│       │   ├── Service.cs
│       │   ├── Staff.cs
│       │   ├── WorkSchedule.cs
│       │   └── Booking.cs
│       ├── Enums/
│       │   ├── UserRole.cs
│       │   └── BookingStatus.cs
│       ├── Exceptions/
│       │   └── ApiException.cs
│       ├── Middleware/
│       │   └── ExceptionHandlingMiddleware.cs
│       ├── Migrations/                 # EF Core SQL Server migrations
│       ├── Services/
│       │   ├── Interfaces/
│       │   │   └── IServiceCatalogService.cs
│       │   └── ServiceCatalogService.cs
│       ├── Program.cs
│       ├── ServiceBooking.Api.csproj
│       ├── appsettings.json
│       └── appsettings.Development.json
│
├── docker-compose.yml                  # Deferred bonus infrastructure; currently not used
├── .gitignore
├── README.md
└── guide.md
```

The root `.env.example` previously used for PostgreSQL/Docker is not part of the current mandatory development workflow.

The frontend `src/frontend/.env.example` remains part of the project.

---

## 3. Frontend Architecture

### Framework

* Next.js App Router.
* React 19.
* TypeScript.
* React Server Components by default.
* Tailwind CSS v4 via:

```css
@import "tailwindcss";
```

### Shared API Client

File:

```text
src/frontend/src/lib/api.ts
```

Behavior:

* Base URL comes from `NEXT_PUBLIC_API_URL`.
* Fallback API URL: `http://localhost:5000/api`.
* Sends JSON requests.
* Throws when the HTTP response is not successful.

Reuse this helper instead of creating page-specific API base URLs or duplicated fetch configuration.

### Required Frontend Routes

```text
/login
/services
/booking
/my-bookings
/admin/services
/admin/schedules
/admin/bookings
```

### UI Requirements

Feature pages must handle, where applicable:

* loading state;
* error state;
* empty state;
* form validation;
* disabled submit buttons while requests are running;
* clear booking conflict errors (`409 Conflict`);
* basic responsive layout.

### Frontend Organization

Prefer:

```text
src/frontend/src/
├── app/              # Route-level components
├── components/       # Reusable UI components
├── lib/              # Shared utilities/API client
├── types/            # Shared TypeScript types when needed
└── features/         # Add only when feature complexity justifies it
```

### Frontend Rules

* Avoid `any` unless genuinely unavoidable.
* Never put backend secrets or database connection strings in frontend code.
* Never expose secrets using `NEXT_PUBLIC_*`.
* Prefer Tailwind utility classes.
* Do not duplicate API base URL logic.
* Keep route pages focused on UI composition and data flow.
* Extract reusable components only when actual reuse exists.
* Avoid premature abstractions.

---

## 4. Backend Architecture

### Request Flow

```text
HTTP request
  -> Controller
  -> Service interface
  -> Service implementation
  -> AppDbContext / Entity Framework Core
  -> Microsoft SQL Server
```

Existing Services flow:

```text
ServicesController
  -> IServiceCatalogService
  -> ServiceCatalogService
  -> AppDbContext.Services
```

Follow the same architectural direction for new features.

Do not place all business logic inside controllers.

### Backend Organization

```text
Controllers/          # HTTP endpoints and authorization boundaries
DTOs/                 # Request/response contracts
Entities/             # EF Core persistence/domain entities
Enums/                # Domain enums
Services/             # Business/application logic
Services/Interfaces/  # Service contracts
Data/                 # DbContext and EF configuration
Exceptions/           # Application/API exceptions
Middleware/           # Cross-cutting HTTP middleware
Migrations/           # EF Core SQL Server migrations
```

### Backend Technical Rules

* Use DTOs for request and response payloads.
* Do not expose EF entities directly unless explicitly justified.
* Validate requests on the backend regardless of frontend validation.
* Use asynchronous EF Core operations.
* Never return password hashes or sensitive configuration.
* Use centralized exception handling.
* Return appropriate HTTP status codes.
* Perform filtering, sorting, and pagination in the database.
* Enforce authorization independently of frontend behavior.
* Use `AsNoTracking()` for read-only EF queries where appropriate.
* Use cancellation tokens on asynchronous request paths where practical.

---

## 5. Database

### Database Technology

The project uses:

```text
Microsoft SQL Server
```

Entity Framework Core is configured with the SQL Server provider.

PostgreSQL is no longer part of the active application architecture.

Docker is not required to run the database during the current mandatory development phase.

### DbSets

`AppDbContext` contains:

```text
Users
Services
Staffs
WorkSchedules
Bookings
```

### User

Roles:

```text
Admin
Customer
```

Important constraints:

* `Email` must be unique.
* Passwords must be stored as hashes only.
* Password hashes must never be returned through API responses.

### Service

Required fields:

```text
Id
Name
Description
DurationMinutes
Price
IsActive
```

Rules:

* Name is required.
* `DurationMinutes > 0`.
* `Price >= 0`.
* Inactive services cannot be booked.
* Service listing must support search and pagination.

### Staff

Required fields:

```text
Id
FullName
Email
IsActive
```

Rules:

* Inactive staff cannot receive bookings.
* For project scope, all staff members can perform all services.

### WorkSchedule

Required fields:

```text
Id
StaffId
WorkDate
StartTime
EndTime
```

Rules:

* `StartTime < EndTime`.
* Work schedules for the same staff member must not overlap.
* A booking must fit completely inside an applicable work schedule.

### Booking

Required fields:

```text
Id
BookingCode
CustomerId
ServiceId
StaffId
StartTime
EndTime
Status
CustomerNote
CancellationReason
CreatedAt
```

Statuses:

```text
Pending
Confirmed
Completed
Cancelled
```

Important constraints:

* `BookingCode` must be unique.
* Booking belongs to exactly one customer.
* Booking belongs to exactly one service.
* Booking belongs to exactly one staff member.

---

## 6. Critical Booking Business Rules

Booking rules are mandatory backend business logic.

Never rely only on frontend validation.

### 6.1 End Time

The customer selects the booking start time.

Backend calculates:

```text
EndTime = StartTime + Service.DurationMinutes
```

Do not trust a client-provided end time.

### 6.2 Valid Booking Time

Reject a booking when:

* start time is in the past;
* selected service is inactive;
* selected staff member is inactive;
* booking does not fit completely inside the staff work schedule.

### 6.3 Booking Overlap

Two bookings overlap when:

```text
NewStart < ExistingEnd
AND
NewEnd > ExistingStart
```

Examples:

```text
Existing: 09:00-10:00
New:      09:30-10:30
Result:   overlap

Existing: 09:00-10:00
New:      08:30-09:30
Result:   overlap

Existing: 09:00-10:00
New:      10:00-11:00
Result:   no overlap
```

Only non-cancelled bookings occupy time slots.

When a booking conflict occurs, return:

```text
HTTP 409 Conflict
```

Recommended stable application error code:

```text
BOOKING_SLOT_CONFLICT
```

### 6.4 Customer Authorization

Customer can:

* view own bookings;
* create own bookings;
* cancel own bookings.

Customer cannot:

* view another customer's bookings;
* confirm bookings;
* complete bookings;
* manage other customers' bookings.

### 6.5 Admin Authorization

Admin can:

* view all bookings;
* confirm bookings;
* complete bookings;
* cancel bookings.

### 6.6 Cancellation

A booking cannot be cancelled when:

* status is already `Completed`;
* booking has already started.

Cancellation reason is required.

When a booking becomes:

```text
Cancelled
```

it no longer occupies the staff time slot.

### 6.7 Concurrency

Advanced concurrency hardening is deferred until all mandatory project requirements are complete.

The mandatory implementation must still enforce overlap validation on the backend.

Frontend availability checks are never sufficient booking validation.

---

## 7. API Contract Target

### Existing / Initial Endpoints

```text
GET  /api/health
GET  /api/services
POST /api/services
```

### Required Minimum API

#### Authentication

```text
POST /api/auth/login
GET  /api/auth/me
```

#### Services

```text
GET  /api/services
POST /api/services
PUT  /api/services/{id}
```

#### Staff / Work Schedule

```text
GET  /api/staffs
GET  /api/staffs/{id}/schedules
POST /api/staffs/{id}/schedules
```

#### Bookings

```text
GET   /api/bookings/my-bookings
GET   /api/bookings
GET   /api/bookings/available-slots
POST  /api/bookings
PATCH /api/bookings/{id}/status
POST  /api/bookings/{id}/cancel
```

Endpoint names may change only when the resulting API remains internally consistent and documented.

---

## 8. Current Implementation Status

This section must be kept synchronized with the actual repository.

Do not assume a feature exists merely because a directory, route, DTO, entity, or placeholder exists.

### Infrastructure / Foundation Implemented

* Repository structure.
* ASP.NET Core Web API project.
* Microsoft SQL Server database configuration.
* Entity Framework Core SQL Server provider.
* Core entities.
* `AppDbContext`.
* Initial EF Core SQL Server migration.
* JWT Bearer configuration foundation.
* CORS configuration.
* Swagger/OpenAPI.
* Global exception middleware.
* Health endpoint (`GET /api/health`).
* Seed data: 1 Admin, 2 Customers, 2 Staff, 5 Services, 7 schedule days, and 10 bookings.
* Password hashing, JWT authentication, `POST /api/auth/login`, and `GET /api/auth/me`.
* Complete Service Catalog API: Admin management, search, and database pagination.
* Staff management API.
* WorkSchedule API and schedule-overlap validation.
* Next.js App Router frontend.
* React 19.
* TypeScript.
* Tailwind CSS v4.
* Shared frontend API helper.
* Required frontend route structure.
* Basic global loading/error/empty-state patterns.

### Mandatory Features Still To Complete

Keep this list synchronized with actual implementation progress:

* Frontend authenticated session handling.
* WorkSchedule manual API validation.
* Available slot API manual validation.
* Booking creation, conflict, customer-list, cancellation, Admin management, status-transition, filtering, and pagination API validation.
* Required test scenario validation.
* Final API/README cleanup.

### Deferred / Bonus

Do not implement these until all mandatory requirements are complete:

```text
Docker Compose
SignalR real-time updates
Hangfire/background jobs
Advanced concurrency hardening
Calendar UI
Additional bonus features
```

The existing `docker-compose.yml` may remain in the repository, but it is not part of the current development workflow.

---

## 9. Seed Data Requirements

Required sample data:

```text
1 Admin
2 Customers
2 Staff members
5 Services
Work schedules covering 7 days
10 Bookings across multiple statuses
```

After seed data and authentication are implemented, document demo credentials in:

```text
README.md
```

Never use real credentials.

Never commit real production secrets.

---

## 10. Required Test Scenarios

At minimum, the project must validate:

```text
TC1: Reject booking in the past.

TC2: Reject booking outside staff working hours.

TC3: Reject overlapping booking.

TC4: Customer cannot access another customer's booking.

TC5: Customer cannot complete a booking.

TC6: Completed booking cannot be cancelled.
```

These scenarios are mandatory project behaviors even if a full automated testing architecture has not yet been implemented.

Where practical, integration tests are preferred for booking and authorization rules because those behaviors cross:

```text
HTTP
Authentication
Authorization
Business logic
Entity Framework Core
Database
```

Do not delay mandatory application functionality solely to build additional bonus-level test infrastructure.

---

## 11. Local Development

### 11.1 Database

Local development uses:

```text
Microsoft SQL Server
```

Docker is not required during the mandatory implementation phase.

The SQL Server connection is configured through:

```text
ConnectionStrings:DefaultConnection
```

Before running EF Core commands, ensure the configured SQL Server instance is running and accessible.

### 11.2 Backend

Requirements:

* .NET SDK compatible with `ServiceBooking.Api.csproj`;
* `dotnet-ef`;
* Microsoft SQL Server.

Run:

```bash
cd src/backend

dotnet restore
dotnet ef database update
dotnet run
```

Do not create `InitialCreate` again if the initial migration already exists.

When an intentional database schema change requires a new migration:

```bash
dotnet ef migrations add <MigrationName>
dotnet ef database update
```

Default development endpoints:

```text
API:     http://localhost:5000
Swagger: http://localhost:5000/swagger
Health:  http://localhost:5000/api/health
```

Use the actual configured launch URL if the local project configuration differs.

### 11.3 Frontend

Requirements:

* Node.js 20+

Run:

```bash
cd src/frontend

cp .env.example .env.local

npm install
npm run dev
```

Frontend environment variable:

```env
NEXT_PUBLIC_API_URL=http://localhost:5000/api
```

Default frontend URL:

```text
http://localhost:3000
```

---

## 12. Environment / Configuration

### Active Configuration Locations

```text
/src/frontend/.env.example
/src/backend/appsettings.json
/src/backend/appsettings.Development.json
/User Secrets (local `ConnectionStrings:DefaultConnection` and `Jwt:Key`)
```

The old root `.env.example` used by the PostgreSQL/Docker workflow is not part of the current mandatory development workflow.

### Frontend Configuration

Expected frontend variable:

```text
NEXT_PUBLIC_API_URL
```

Frontend environment variables belong in frontend environment files.

Never expose backend secrets through:

```text
NEXT_PUBLIC_*
```

### Backend Configuration

Backend configuration includes:

```text
ConnectionStrings:DefaultConnection
Jwt:Key
Jwt:Issuer
Jwt:Audience
FrontendUrl
```

`ConnectionStrings:DefaultConnection` and `Jwt:Key` must be supplied through User Secrets locally or environment variables in deployed environments. They must not be committed in `appsettings.json`.

### Timezone Policy

Business time is `Asia/Ho_Chi_Minh`. `WorkSchedule.WorkDate`, `StartTime`, and `EndTime` represent Vietnam local time. `Booking.StartTime` and `EndTime` are stored as UTC `DateTimeOffset` values; convert them to Vietnam time before comparing them with a work schedule.

ASP.NET Core environment variable notation may represent nested configuration using double underscores.

Example:

```text
Jwt__Key
```

maps to:

```text
Jwt:Key
```

Do not create duplicate configuration sources without a concrete reason.

### Security Rules

* Never commit real production secrets.
* Never commit real production database credentials.
* Never expose JWT signing keys to frontend code.
* Never expose database connection strings to frontend code.
* Never return sensitive configuration through APIs.
* Never return password hashes.
* Keep local development secrets separate from production secrets.

---

## 13. Where to Make Changes

When implementing a feature, first identify the correct application layer.

### Authentication

Expected areas:

```text
src/backend/DTOs/Auth/
src/backend/Controllers/AuthController.cs
src/backend/Services/AuthService.cs
src/backend/Services/Interfaces/IAuthService.cs
src/backend/Program.cs

src/frontend/src/app/login/
src/frontend/src/lib/
```

Add new files only when necessary.

### Services

Existing areas:

```text
src/backend/Controllers/ServicesController.cs
src/backend/Services/ServiceCatalogService.cs
src/backend/Services/Interfaces/IServiceCatalogService.cs
src/backend/DTOs/Services/

src/frontend/src/app/services/page.tsx
src/frontend/src/app/admin/services/page.tsx
```

Extend the existing Services architecture.

Do not create a parallel service-management architecture.

### Staff / Work Schedule

Primary domain files:

```text
src/backend/Entities/Staff.cs
src/backend/Entities/WorkSchedule.cs

src/frontend/src/app/admin/schedules/page.tsx
```

Create controller/service/DTO layers following the established backend pattern.

Expected direction:

```text
Controller
  -> Service interface
  -> Service implementation
  -> AppDbContext
```

### Booking

Primary domain entity:

```text
src/backend/Entities/Booking.cs
```

Expected backend implementation areas:

```text
src/backend/Controllers/BookingsController.cs
src/backend/Services/BookingService.cs
src/backend/Services/Interfaces/IBookingService.cs
src/backend/DTOs/Bookings/
```

Expected frontend areas:

```text
src/frontend/src/app/booking/page.tsx
src/frontend/src/app/my-bookings/page.tsx
src/frontend/src/app/admin/bookings/page.tsx
```

Booking rules must be centralized primarily in backend booking business logic.

Do not duplicate authoritative overlap/business-rule logic across frontend pages.

---

## 14. Coding Conventions

### General

* Prefer the simplest implementation that satisfies the assignment.
* Prioritize mandatory requirements.
* Avoid speculative architecture.
* Avoid unnecessary directories and abstractions.
* Reuse existing patterns.
* Preserve the current project structure unless a concrete requirement justifies changing it.
* Do not silently change framework or major package versions.
* Do not introduce bonus features before mandatory functionality is complete.
* Keep seed/test/mock data separate from production business logic.
* Do not commit real secrets or credentials.

### Backend

Controllers are responsible for:

```text
HTTP transport
Request binding
Authorization boundaries
Response status
```

Services are responsible for:

```text
Business logic
Booking rules
Domain validation
Authorization-sensitive operations
Database coordination
```

DTOs are responsible for:

```text
External API contracts
Request models
Response models
```

Entities are responsible for:

```text
Persistent domain state
EF Core relationships
```

DbContext is responsible for:

```text
Database access
Entity configuration
Constraints/indexes
Relationships
```

Additional backend rules:

* Prefer asynchronous EF Core APIs.
* Use `AsNoTracking()` for read-only queries where appropriate.
* Perform filtering in SQL.
* Perform sorting in SQL.
* Perform pagination in SQL.
* Use DTO projection where practical.
* Keep controllers thin.
* Do not trust frontend validation.
* Return correct HTTP status codes.

### Frontend

* Use TypeScript explicitly.
* Avoid `any`.
* Use Tailwind CSS.
* Prefer Server Components unless client-side interaction requires `"use client"`.
* Reuse the shared API helper.
* Do not duplicate API base URLs.
* Display backend validation errors clearly.
* Display `409 Conflict` booking errors clearly.
* Handle loading/error/empty states.
* Disable submit actions while requests are running.
* Prevent accidental duplicate submissions.

---

## 15. Mandatory Implementation Order

Follow this order unless an explicit newer requirement requires a change:

```text
1. Seed required sample data

2. Password hashing + authentication service

3. POST /api/auth/login

4. GET /api/auth/me

5. Frontend authentication flow

6. Complete Services admin flow

7. Staff management/API

8. WorkSchedule API + schedule overlap validation

9. Available slots API

10. Booking creation + booking conflict validation

11. Customer booking list

12. Customer booking cancellation

13. Admin booking management

14. Admin booking status transitions

15. Booking filtering + database pagination

16. Validate all six mandatory test scenarios

17. Complete demo credentials documentation

18. Swagger/API review

19. README.md cleanup

20. Final mandatory requirement review against the assignment PDF
```

Booking correctness, authorization, validation, and required functionality are more important than visual polish.

### After Mandatory Requirements

Only after the mandatory project is complete should bonus work be considered.

Possible bonus work:

```text
Docker Compose
SignalR
Hangfire
Advanced concurrency hardening
Calendar UI
Additional automated tests
Other explicitly permitted bonus features
```

---

## 16. Quick Agent Checklist Before Editing

Before making a code change, answer internally:

```text
1. Is this requirement mandatory or bonus?

2. Is this frontend, backend, database, or cross-cutting?

3. Is there already an existing file or pattern to extend?

4. Does this change affect authentication or authorization?

5. Does this change affect booking business rules?

6. Does backend validation enforce the rule independently?

7. Does this require an EF Core migration?

8. Does this require a new configuration value?

9. Can an existing configuration value be reused?

10. Does this require a DTO instead of exposing an entity?

11. Is filtering/pagination performed in the database?

12. Does the frontend need loading/error/empty behavior?

13. Does a submit action need duplicate-click protection?

14. Does this affect one of the six mandatory test scenarios?

15. Does README.md or guide.md need updating?

16. Am I accidentally implementing a bonus feature before mandatory functionality is complete?
```

If the answer to question 16 is yes, stop and return to mandatory functionality unless explicitly instructed otherwise.

---

## 17. Definition of Done

A feature is complete only when all applicable items are satisfied:

```text
[ ] backend endpoint implemented

[ ] request/response DTOs implemented

[ ] backend validation implemented

[ ] authorization enforced server-side

[ ] business logic kept outside controller

[ ] database queries are efficient

[ ] filtering/pagination happens in the database where applicable

[ ] correct HTTP status codes returned

[ ] frontend loading state handled

[ ] frontend error state handled

[ ] frontend empty state handled

[ ] backend validation errors are displayed appropriately

[ ] submit action protected against duplicate clicks

[ ] TypeScript types are explicit

[ ] secrets are not exposed

[ ] migration added when schema changed

[ ] relevant mandatory business rule validated

[ ] README.md updated when setup/public behavior changes

[ ] guide.md updated when architecture/workflow changes
```

---

## 18. Important Non-Goals

Unless the assignment requirements change, do not spend time implementing:

```text
payment integration
user registration
forgot password
email verification
OAuth/social login
per-service staff skill mapping
microservices
message queues
Kubernetes
complex design system
```

The following are currently deferred bonus items:

```text
Docker Compose
SignalR
Hangfire
advanced concurrency hardening
calendar UI
other optional enhancements
```

Do not implement deferred bonus items before mandatory requirements are complete.

---

## 19. Docker Status

The repository may retain:

```text
docker-compose.yml
```

However:

* Docker is currently not required for local development.
* Docker must not be used as a prerequisite for running the mandatory project.
* PostgreSQL is not the active project database.
* Microsoft SQL Server is the active database.
* Do not spend time modifying Docker infrastructure during mandatory implementation.
* Do not run Docker-related setup unless explicitly requested.

After mandatory functionality is complete, Docker Compose may be revisited as bonus work.

If Docker support is implemented later, it must be aligned with the project's actual database technology and current architecture.

---

## 20. Mandatory vs Bonus Priority

When deciding what to implement next, use:

```text
Mandatory PDF requirement
        ↓
Business-rule correctness
        ↓
Authentication / Authorization
        ↓
Database correctness
        ↓
Required frontend flow
        ↓
Required validation/testing
        ↓
Documentation
        ↓
BONUS ONLY AFTER EVERYTHING ABOVE
```

Do not prioritize:

```text
Docker
SignalR
Hangfire
advanced concurrency
calendar UI
visual polish
optional architecture
```

over unfinished mandatory functionality.

---

## 21. Useful Codex Queries

Examples:

```text
"Read guide.md and implement the next unfinished mandatory milestone without changing the existing architecture or implementing bonus features."
```

```text
"Read guide.md and implement POST /api/auth/login and GET /api/auth/me using the existing JWT configuration and SQL Server database."
```

```text
"Read guide.md and implement WorkSchedule overlap validation following the existing Controller -> Service -> EF Core architecture."
```

```text
"Read guide.md and implement booking creation. Enforce all mandatory booking business rules server-side and return HTTP 409 for slot conflicts."
```

```text
"Read guide.md and review the repository against the mandatory requirements in the assignment PDF. Do not recommend bonus features until mandatory requirements are complete."
```

```text
"Read guide.md and identify the next incomplete mandatory feature based on the current repository implementation."
```

---

## 22. Source-of-Truth Priority

When information conflicts, use this priority:

```text
1. Explicit latest user instruction

2. Service_Booking_Demo_Project_Requirements.pdf

3. Existing working code and database schema

4. guide.md

5. README.md
```

Never silently resolve a conflict by guessing.

When code and documentation differ:

1. inspect the actual implementation;
2. determine which behavior satisfies the assignment requirement;
3. preserve working code when correct;
4. update outdated documentation together with the relevant change.

---

## 23. Final Development Principle

The goal of this repository is not to demonstrate the largest possible architecture.

The goal is to deliver a correct, maintainable implementation of the required Service Booking Management System.

Use this priority:

```text
Correct mandatory functionality
        >
Correct business rules
        >
Security and authorization
        >
Database correctness
        >
Clear maintainable code
        >
Required UI behavior
        >
Documentation
        >
Bonus features
```

Until every mandatory requirement from the assignment PDF is complete:

```text
DO NOT prioritize bonus work.
DO NOT reintroduce PostgreSQL.
DO NOT require Docker.
DO NOT introduce unnecessary architecture.
DO NOT move booking rules to the frontend.
DO NOT weaken backend authorization or validation.
```

Keep the implementation simple, consistent, and aligned with the existing repository architecture.
