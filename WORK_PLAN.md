# EventBooking — Work Plan

Ordered steps. Nothing is done until the student says "do step X".
For each step the student may first ask for an explanation; then approve; then I implement only that step.

Legend: **DoD** = Definition of Done. **Covers** = which teacher-checklist items it satisfies.

---

## Step 0 — Decisions & cleanup (no code yet)
- Choose the database: **SQL Server** or **PostgreSQL** (locks the concurrency-token style and all migrations).
- Decide concurrency token accordingly (SQL Server: `[Timestamp] byte[] RowVersion`; PostgreSQL: `xmin` or self-managed `[ConcurrencyCheck] Guid Version`).
- Delete the stray `project-dotnet.sln`; keep `EventBooking.sln`.
- Confirm `.gitignore` excludes `bin/`, `obj/`, secrets, `node_modules/`.
- `git init` for the server repo (client repo comes later).
- **DoD:** database chosen and written into PROJECT_LOG; repo initialised; only one `.sln`.

---

## Step A — Data layer foundation
- Add EF Core packages to `EventBooking.Data` (provider) and `Microsoft.EntityFrameworkCore.Design` to `EventBooking.API`.
- Create `AppDbContext` with a `DbSet<T>` for every entity.
- Connection string via **User Secrets** + `appsettings.Development.json`; nothing in `appsettings.json`.
- Register `AddDbContext` in `Program.cs` (scoped).
- **Covers:** EF Core DbContext, configuration/secrets, DI lifetimes.
- **DoD:** solution builds; `dotnet ef dbcontext info` works.

---

## Step B — Fluent API configuration & relationships
- `IEntityTypeConfiguration<T>` classes: keys, required/max-length, `decimal` precision, indexes.
- `BookingExtraService` composite key `(BookingId, ExtraServiceId)`.
- One-to-many: `Venue→Hall`, `Hall→HallSlot`, `HallSlot→Booking`, `User→Booking`.
- Many-to-many: `Booking↔ExtraService` through `BookingExtraService`.
- Configure the concurrency token on `HallSlot`.
- If self-managed token: override `SaveChangesAsync` in `AppDbContext` to bump `Version` on modified `HallSlot`.
- **Covers:** EF Core relationships, Fluent API, concurrency token (part 1).
- **DoD:** model builds; `dotnet ef migrations add` produces a sensible schema (reviewed, not yet applied beyond Step C).

---

## Step C — First migration & seed data
- `dotnet ef migrations add Init -p EventBooking.Data -s EventBooking.API`.
- Seed: venues, halls, hall slots, extra services, demo users (one per role, hashed passwords).
- `dotnet ef database update`.
- **Covers:** migrations (1 of ≥2), seed data, "runs with no manual DB work".
- **DoD:** fresh DB created from scratch, seed rows present.

---

## Step D — Core contracts: interfaces & DTOs
- Repository interfaces in `EventBooking.Core` (e.g. `IHallSlotRepository`, `IBookingRepository`, `IUserRepository`, or a generic `IRepository<T>` + specifics).
- Service interfaces in Core.
- Request DTOs and response DTOs (separate) with Data Annotations.
- A `Result` / `Result<T>` type for service outcomes (Ok / NotFound / Conflict / ValidationError).
- **Covers:** layered architecture (Core references nothing), DTO separation, validation attributes.
- **DoD:** Core compiles with no external package references.

---

## Step E — Repository implementations (Data)
- Implement the interfaces in `EventBooking.Data`.
- Full CRUD; LINQ filtering + sorting; **pagination with `Skip`/`Take` in the query**; `AsNoTracking` on reads; `Include`/`ThenInclude` where needed (no N+1).
- **Covers:** EF Core CRUD/queries, pagination, AsNoTracking, eager loading.
- **DoD:** repositories unit-testable via the interface; a paged query hits the DB with `OFFSET/FETCH`.

---

## Step F — AutoMapper
- Add AutoMapper; create `Profile` classes (entity ↔ DTO both directions as needed).
- Register in `Program.cs`.
- **Covers:** DTO & AutoMapper.
- **DoD:** no entity type appears in any controller signature.

---

## Step G — Service layer (business logic + the concurrency flow)
- Implement services: constructor injection only, no `new` on services.
- Business/state validation lives here (not controllers).
- Booking flow: check slot availability + save in the **same transaction**; `catch (DbUpdateConcurrencyException)` → `Result.Conflict(...)`.
- Enforce "at most one non-cancelled booking per slot".
- **Covers:** business validation in Service, DI rules, concurrency handling (part 2), async + `CancellationToken` end to end.
- **DoD:** booking a taken slot returns a Conflict result; all methods async with `ct`.

---

## Step H — JWT authentication & authorization
- Register / login endpoints; password hashing (e.g. `PasswordHasher` / BCrypt).
- Token generation with claims (sub, email, role); server-side validation config.
- Two roles with genuinely different permissions (e.g. Client books; Manager creates halls/slots and confirms bookings).
- **Covers:** JWT auth, ≥2 roles, secrets from config.
- **DoD:** protected endpoint rejects no/invalid token (401) and wrong role (403).

---

## Step I — API controllers (REST)
- Plural resource routes, HTTP verbs, `[FromRoute]`/`[FromQuery]`/`[FromBody]`.
- `ModelState` check; correct status codes: 200 / 201+Location / 204 / 400 / 401 / 403 / 404 / 409.
- `[Authorize]` with roles.
- **Covers:** Web API/REST, model binding, status codes, authorization on server.
- **DoD:** every endpoint visible and callable in Swagger.

---

## Step J — Middleware
- Global error-handling middleware → uniform JSON error response.
- CorrelationId middleware → generate per request, add to response header and to log context.
- Pipeline order: error handling → auth(N) → authz.
- **Covers:** custom middleware, pipeline order.
- **DoD:** an unhandled exception returns the uniform JSON, not a stack trace.

---

## Step K — Logging with NLog
- Add `NLog.Web.AspNetCore` + `nlog.config`.
- Log: each request with CorrelationId; caught exceptions at Error; resource collisions at Warning.
- No passwords / tokens / request bodies in logs.
- Code keeps using `ILogger<T>`.
- **Covers:** Part D in full.
- **DoD:** log file shows a request line with CorrelationId and a Warning line on a simulated collision.

---

## Step L — Configuration finalization
- `appsettings.json` (safe defaults only) + `appsettings.Development.json`.
- Strongly-typed options (`IOptions<JwtOptions>` etc.).
- Verify no secret is in the repo.
- **Covers:** configuration, IOptions, no secrets.
- **DoD:** app runs from a clean clone after `dotnet user-secrets set`.

---

## Step M — Second migration
- A real schema change (e.g. add an index, a column, or a new small entity) → `dotnet ef migrations add ...`.
- **Covers:** "at least two migrations in history".
- **DoD:** two migration files, DB updated.

---

## Step N — Unit tests (xUnit + Moq)
- Service-layer tests with mocked repositories.
- **Mandatory:** limited-resource logic — success case and rejection case.
- **Separately evaluated:** two real `DbContext` instances read the same `HallSlot`, both modify, first `SaveChanges` succeeds, second throws `DbUpdateConcurrencyException`.
- **Covers:** unit tests, concurrency proof.
- **DoD:** `dotnet test` green; the concurrency test genuinely exercises the token.

---

## Step O — Swagger + end-to-end check
- JWT auth button in Swagger.
- Manual run-through: register → login → list (paged) → book a slot → book same slot again → 409.
- **DoD:** full happy path + the 409 path work against the seeded DB.

---

## Step P — Server README
- System description paragraph.
- Limited resource + how competition is handled.
- Local run instructions incl. DB setup.
- Demo users per role with credentials.
- **Covers:** submission README.
- **DoD:** a new person can run the server following only the README.

---

## Step Q — React client (minimum, not evaluated)
- 4 screens: login/register (store + send JWT), list with server pagination, action on the limited resource, clear 409 message.
- **DoD:** clicking "book" on a taken slot shows the "resource was taken" message.

---

## Step R — Submission
- Two GitHub repos (server, client), clean `.gitignore`, no secrets committed.
- Final pass over the Part B checklist.
- **DoD:** both repos pushed; checklist fully ticked in PROJECT_LOG.
