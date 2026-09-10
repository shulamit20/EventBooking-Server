# EventBooking — Server (ASP.NET Core Web API)

## What this is

EventBooking is a layered ASP.NET Core 8 Web API for booking event halls (weddings, bar/bat
mitzvahs, conferences). A **Manager** maintains the catalog — venues, halls, and the dated
time-slots each hall offers — and reviews bookings. A **Customer** registers, browses available
slots, and books one, optionally adding extra services (catering, photography, …). Each booking
carries a price snapshot so later catalog price changes never rewrite history.

The API is split into four projects with a strict dependency direction (`Core` ← `Data`,
`Core` ← `Service`, `Core`/`Service` ← `API`; `Data` is referenced by `API` only to register it
in the DI container). See `ARCHITECTURE.md` for the full layout and a request trace.

## The limited resource and how competition is handled

The competed resource is a **`HallSlot`** — one hall, one date, one shift (Morning / Noon /
Evening). At most one active (non-cancelled) booking may hold a slot at any moment, so two
clients trying to book the same slot in the same instant must **not** both succeed.

`HallSlot` carries a self-managed optimistic-concurrency token, `Guid Version`, configured with
the Fluent API (`.IsConcurrencyToken()`). `AppDbContext.SaveChangesAsync` is overridden to give
every modified `HallSlot` a fresh `Version` on each save.

`BookingService.CreateAsync` does the availability check and the write **in one transaction /
one `SaveChangesAsync`**:

1. load the slot (tracked), verify `Status == Available` and that no active booking exists;
2. build the `Booking` (+ extra-service lines with a price snapshot);
3. set `slot.Status = Booked`;
4. `SaveChangesAsync` — EF emits `UPDATE "HallSlots" … WHERE "Id" = @id AND "Version" = @original`.

If another request won the race, the original `Version` no longer matches, zero rows update, and
EF throws **`DbUpdateConcurrencyException`** (the specific type). The service catches exactly
that, logs a Warning, and returns a `Conflict` result, which the API maps to **HTTP 409** with a
clear message. The losing client gets `409`, not a double confirmation.

## Tech stack

| Concern | Choice |
|---|---|
| Runtime | .NET 8 (`net8.0`) |
| Database | PostgreSQL 17, `Npgsql.EntityFrameworkCore.PostgreSQL` 8.0.10 |
| ORM | EF Core 8, Code-First, migrations |
| Auth | JWT bearer (`Microsoft.AspNetCore.Authentication.JwtBearer` 8.0.10), BCrypt password hashing |
| Mapping | AutoMapper 13.0.1 (Profiles) |
| API docs | Swagger / Swashbuckle 6.6.2, with a Bearer "Authorize" button |
| Logging | NLog (`NLog.Web.AspNetCore` 5.3.14) — file + console, correlation id in the layout |
| Tests | xUnit + Moq; SQLite in-memory for the concurrency test |

## Prerequisites

- **.NET 8 SDK**
- **PostgreSQL 17** running locally on port 5432 (any 13+ works). Note the superuser password
  you set at install time.
- **EF Core CLI tool:** `dotnet tool install --global dotnet-ef --version 8.0.30`

## Setup

From the solution root (`EventBooking.sln`):

### 1. Configure secrets (never committed)

The connection string and the JWT signing key are read from **User Secrets** on the `API`
project. Set both:

```bash
dotnet user-secrets set "ConnectionStrings:Default" \
  "Host=localhost;Port=5432;Database=EventBookingDb;Username=<db-user>;Password=<db-password>" \
  --project EventBooking.API

dotnet user-secrets set "Jwt:Key" \
  "<random-string-of-at-least-32-characters>" \
  --project EventBooking.API
```

The non-secret JWT settings (`Issuer`, `Audience`, `ExpiryMinutes`) are already in
`EventBooking.API/appsettings.json`.

### 2. Create the database

```bash
dotnet ef database update -p EventBooking.Data -s EventBooking.API
```

This applies all migrations and seeds `EventBookingDb`: the three demo accounts (one per role),
7 event types, 8 service categories, 2 venues, 3 halls, 3 catering menus, 4 extra services — so
the system is fully usable right after this command, with no manual inserts.

Hall slots are not a fixed seeded count: on every startup the API automatically fills in an
Available slot for every hall, every day of the current month + the next 2 (Saturdays excluded),
so the calendar is always fully browsable. See `POST /api/hall-slots/generate` (Manager-only) to
do the same for an arbitrary month.

## Run

```bash
dotnet run --project EventBooking.API
```

or press **F5** in Visual Studio with `EventBooking.API` as the startup project.

- HTTP: <http://localhost:5269>
- HTTPS: <https://localhost:7021>
- Swagger UI opens at `/swagger`.

## Demo users

| Role | Email | Password |
|---|---|---|
| Manager | `manager@eventbooking.local` | `Passw0rd!` |
| Customer | `client@eventbooking.local` | `Passw0rd!` |
| Admin | `admin@eventbooking.local` | `Passw0rd!` |

**How to authenticate in Swagger:** call `POST /api/auth/login` with one of the pairs above,
copy the `token` from the response, click **Authorize** (top-right), paste the token, and call
protected endpoints.

## Roles and permissions

| Area | Anonymous | Customer | Manager |
|---|---|---|---|
| `POST /api/auth/register`, `/login` | ✅ | ✅ | ✅ |
| Browse venues / halls / hall-slots / extra-services (`GET`) | ✅ | ✅ | ✅ |
| Create / update / delete venues, halls, slots, extra-services | — | — | ✅ |
| `POST /api/hall-slots/generate` (fill in a month's slots) | — | — | ✅ |
| `POST /api/bookings` (book a slot) | — | ✅ | — |
| `GET /api/bookings/mine`, `GET /api/bookings/{id}` (own) | — | ✅ | ✅ |
| `GET /api/bookings` (all, `?status=`) | — | — | ✅ |
| `DELETE /api/bookings/{id}` (cancel own booking) | — | ✅ | — |
| `PATCH /api/bookings/{id}/status` (confirm / cancel any) | — | — | ✅ |

Missing/invalid token → `401`; valid token, wrong role → `403`.

## End-to-end smoke test (the 409 path)

1. `POST /api/auth/login` as the client → copy token, Authorize.
2. `GET /api/hall-slots?status=Available` → pick an `id`.
3. `POST /api/bookings` `{ "hallSlotId": <id>, "eventTypeId": 1, "hostName": "Cohen", "guestCount": 150, "extraServices": [] }` → `201 Created`.
4. `POST /api/bookings` again with the same `hallSlotId` → **`409 Conflict`**.

## Project structure

```
EventBooking.Core     domain entities, enums, DTOs, interfaces, Result<T>  — references nothing
EventBooking.Data     AppDbContext, Fluent configs, migrations, repositories, seed
EventBooking.Service  business services, AutoMapper profiles, auth (hashing + JWT)
EventBooking.API      controllers, DI wiring, JWT bearer, Swagger, middleware, NLog
EventBooking.Tests    xUnit + Moq
```

## Running the tests

```bash
dotnet test
```

Coverage:

- **`BookingServiceTests`** — the competed operation, `BookingService.CreateAsync`, with all
  dependencies mocked: books an available slot (success), rejects a taken slot / a slot with an
  active booking / a lost concurrency race (all → `Conflict`), plus the not-found and
  invalid-extra-service guards. **This is the mandatory "limited-resource logic, success and
  rejection" test.**
- **`HallSlotConcurrencyTests`** — the separately-evaluated concurrency proof. Two real
  `AppDbContext` instances (on an in-memory SQLite database) load the same `HallSlot`; the first
  `SaveChanges` wins, the second throws `DbUpdateConcurrencyException`. A second test shows that
  reloading first makes the next update succeed.
- **`AuthServiceTests`** — login rejects an unknown email / wrong password with `Unauthorized`;
  register rejects a taken email with `Conflict`; a successful register hashes the password,
  forces the `Customer` role, and saves.
- **`MappingProfilesTests`** — `AssertConfigurationIsValid()` over all three AutoMapper profiles.
- **`HallSlotServiceTests`** — the automatic month-generation job: skips Saturdays, skips days
  that already have a slot (idempotent), prices each shift from the hall's defaults, and rejects
  an unknown hall.

## Middleware & logging

The pipeline (in `Program.cs`, in order): **exception handling** → **correlation id** → Swagger
(dev) → HTTPS redirect → authentication → authorization → controllers.

- **`ExceptionHandlingMiddleware`** — the outermost middleware. Any unhandled exception is logged
  at `Error` and returned as a uniform `ProblemDetails` JSON (HTTP 500) with the request's
  correlation id — never a raw stack trace (the exception text is included only in Development).
- **`CorrelationIdMiddleware`** — every request gets a correlation id: taken from the
  `X-Correlation-Id` request header if supplied, otherwise generated. It is echoed on the
  response header and pushed into the logging scope, so every log line for the request carries it.

Logging uses **NLog** under the standard `ILogger<T>`. Configuration is in
`EventBooking.API/nlog.config`. Output:

- `EventBooking.API/.../logs/eventbooking-<date>.log` — Debug and above, full detail.
- console — Info and above, compact.

Each incoming request is logged at Info with its correlation id; caught exceptions at Error;
slot booking collisions at Warning. EF Core SQL and other framework noise below Warning are
filtered out. Passwords, tokens, and request bodies are never logged.

