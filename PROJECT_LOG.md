# EventBooking — Project Log

Shared memory between the student and the AI assistant. Keep it updated.

---

## 1. Working agreement

- The student is a 2nd-year programming student, acting as a **C# software engineer** on this project.
- **The student must understand every line of code.** The assistant explains on request — short and clear, no filler.
- **Wait for explicit approval before making progress.** Do not add code, files, or features until told.
- When the student makes a mistake, **say what was wrong and why** (briefly).
- Every instruction the student receives from the teacher goes into this file (section 3), so the history is never lost.
- Answers: concise, direct, beginner-friendly.

---

## 2. Project overview

**EventBooking** — a .NET 8 layered Web API for booking event halls (weddings, bar mitzvahs, etc.).

| Project | Role | Status |
|---|---|---|
| `EventBooking.Core` | Domain: entities + enums | Done |
| `EventBooking.Data` | EF Core persistence (DbContext, configs, migrations) | Not started |
| `EventBooking.Service` | Business logic, DTOs, auth | Services + AutoMapper done; AuthService (JWT + hashing) done |
| `EventBooking.API` | Controllers, JWT auth, DI, Swagger | 6 controllers; JWT bearer (via `IOptions<JwtOptions>`, validated on start) + demo-user seeding + Swagger auth button; error-handling + CorrelationId middleware; NLog (file + console, CorrelationId in layout) |
| `EventBooking.Tests` | xUnit tests | 14 tests green — booking/concurrency logic, two-DbContext concurrency proof, auth, AutoMapper validation |

**Domain model (Core):**
- Entities: `Venue`, `Hall`, `HallSlot`, `User`, `Booking`, `ExtraService`, `BookingExtraService` (join).
- Enums: `ShiftType`, `SlotStatus`, `BookingStatus`, `UserRole`.

**Key design decisions already made (from code comments):**
- `HallSlot` is the limited resource. Concurrency handled with an optimistic-concurrency token `Guid Version`; a `SaveChangesAsync` override will change it on every update.
- Auth: JWT + role-based policies. Roles: `Client`, `Manager`.
- `Booking` ↔ `ExtraService`: many-to-many with payload (`Quantity`, `PriceAtBooking`), composite key via Fluent API.
- Business rule: at most one non-cancelled booking per slot.

**Housekeeping notes:**
- Real solution file: `EventBooking.sln`.
- `project-dotnet.sln` is stray (points at a non-existent `project-dotnet.csproj`) — delete when convenient.
- Not a git repository yet.

---

## 3. Teacher instructions

### Given 2026-09-08 — v2 upgrade: "Event planning & booking platform"
Full spec saved verbatim in **`FUNCTIONAL_SPEC.md`**. Ordered plan: **`WORK_PLAN_V2.md`**.
Summary: extend the existing halls system (do NOT rebuild; preserve architecture, entities,
auth, and the HallSlot 409 concurrency mechanism) into a platform where a customer builds a
whole event — event type, venue, catering (per-guest pricing), table design, bridal chair,
extra services, **server-side price calculation**, messaging (customer↔manager), notifications,
promotions (+ popup), reviews (booking-gated), customer & manager dashboards, backend search/
filter, and roles Customer/Manager/**Admin**. Phase 1 = functionality only, no UI redesign.

### Given 2026-09-06 — Final Project brief (Course: .NET Web API Development)

**Goal:** combine every course topic into ONE working server. Depth over breadth — 4 clean entities + proper concurrency beats 15 entities + fat controllers.

**Deliverables**
- ASP.NET Core Web API server — the only thing evaluated.
- React client — required for submission, NOT evaluated (design/structure/quality ignored). Do the bare minimum to prove the server works. 4 screens: login/register (store JWT, send in header), list with real server pagination, action on the limited resource, and clear handling of a 409 response.
- Both run locally. No cloud, Docker, S3, Google auth, caching, SignalR, CI.
- Two separate GitHub repos (server, client), each with a proper `.gitignore` (no bin/obj/node_modules/secrets).

**Part A — Concept**
- Must contain a **limited resource multiple users can compete over at the same moment.**
- Test: you must be able to say *"Two users acted simultaneously, both got confirmation, and both should NOT have."* If not → wrong concept.
- Our concept: EventBooking. Limited resource = **`HallSlot`** (one hall, one date, one shift). Two clients booking the same slot at once must not both succeed.

**Part B — Server checklist (every item must appear):**
- *Web API/REST:* plural resource nouns, HTTP verbs not action names, status codes 200 / 201+Location / 204 / 400 / 401 / 403 / 404 / 409. Swagger documents all endpoints.
- *Model binding/validation:* `[FromRoute]` / `[FromQuery]` / `[FromBody]` used correctly. Data Annotations on DTOs + `ModelState` check. Business/state validation lives in the **Service**, not the Controller.
- *Layered architecture:* 4 projects. Core references nothing. Data → Core. Service → Core. API → Service + Core, and Data **only** for DI registration in `Program.cs`. Point deductions: EF Core packages in Core; Controller injecting `DbContext` directly.
- *DI:* all deps via constructor, no `new` on a service inside a service. Interfaces in Core, implementations in Data/Service, registered in `Program.cs`. Intentional lifetimes — `DbContext` is scoped; must be able to explain why a singleton holding a scoped dep is a bug.
- *EF Core (Code-First):* `DbContext` with a `DbSet` per entity, **at least 2 migrations** in history. Connection string from configuration, never hardcoded. Seed data so the system runs right after DB setup.
- *EF Core CRUD/queries:* full CRUD, LINQ filter + sort, **real pagination in the query** (`Skip`/`Take` in SQL, not `ToList` then slice). `AsNoTracking` on every read-only query.
- *EF Core relationships:* ≥1 one-to-many and ≥1 many-to-many. `Include` / `ThenInclude` for loading. Fluent API configs. No N+1 (query inside a loop) → deduction.
- *DTO & AutoMapper:* entities never enter/leave the API. Separate request and response DTOs. Mapping via AutoMapper Profiles.
- *Async:* whole chain async, Controller → DbContext. `CancellationToken` passed all the way through.
- *Middleware:* (1) global error handler → uniform JSON response; (2) CorrelationId per request, injected into logs. Pipeline order: error handling first, authentication before authorization.
- *Configuration:* `appsettings.json` + `appsettings.Development.json`, access via `IConfiguration` / `IOptions`. No secrets in code or repo (DB password, JWT key). Use **User Secrets** in development.
- *JWT auth:* register + login, token with claims, server-side validation. **≥2 roles with genuinely different permissions**, endpoints protected with `[Authorize]`.
- *Unit tests (xUnit + Moq):* tests on the Service layer with mocked repositories. **Mandatory:** at least one test on the limited-resource logic — both success and rejection cases.

**Part C — Resource competition (biggest grade impact, self-taught):**
- Requirement is **optimistic concurrency**.
- Required for submission:
  1. Concurrency token on the competed entity, defined in code.
  2. Catch `DbUpdateConcurrencyException` (the specific type, not `Exception`) → return **409** with a clear message. Business check + save must be in the **same transaction**.
  3. A test: two separate `DbContext`s read the same entity, both modify it, first save succeeds, second throws. **Evaluated separately.**
- README: one paragraph — which limited resource, where the competition is, how the system responds.

**Part D — Logging with NLog (self-taught):**
- Package `NLog.Web.AspNetCore`, config in `nlog.config`.
- Log: every incoming request with its CorrelationId; every caught exception at **Error**; every resource collision at **Warning**.
- Levels: Debug (dev), Information (normal flow), Warning (handled exception), Error (failure).
- Never log passwords, tokens, or registration request bodies.
- Code keeps using standard .NET `ILogger`; NLog plugs in underneath.

**Part F — Submission / what is graded in practice:**
- Compliance with the Part B list, resource-competition handling, tests, and that the system **spins up and runs locally with no manual DB work**.
- Server README: system description paragraph · limited resource + how competition is handled · local run instructions incl. required DB · demo users per role with credentials.

**Appendix — Database:**
- Choose SQL Server **or** PostgreSQL once at the start and never switch (switching = regenerate all migrations).
- EF packages go in the **Data** project; `Microsoft.EntityFrameworkCore.Design` in the **API** (startup) project. `dotnet tool install --global dotnet-ef`.
- Migration commands from solution root: `dotnet ef migrations add Init -p EventBooking.Data -s EventBooking.API` (and `database update` with the same flags).
- Connection string in `appsettings.Development.json` or User Secrets, never in `appsettings.json`.
- **SQL Server:** package `Microsoft.EntityFrameworkCore.SqlServer`; `options.UseSqlServer(...)`; concurrency token = built-in `[Timestamp] byte[] RowVersion`. LocalDB string: `Server=(localdb)\MSSQLLocalDB;Database=...;Trusted_Connection=True;TrustServerCertificate=True`.
- **PostgreSQL:** package `Npgsql.EntityFrameworkCore.PostgreSQL`; `options.UseNpgsql(...)`; `[Timestamp]` does NOT work. Options: `modelBuilder.Entity<T>().UseXminAsConcurrencyToken()`, OR a self-managed `[ConcurrencyCheck] Guid Version` bumped in a `SaveChangesAsync` override. Watch: names case-sensitive; always `DateTime.UtcNow`, never `.Now` (Npgsql → `timestamptz`).

**⚠ Open decision for the student — pick the database now.**
The current `HallSlot` code uses the self-managed `Guid Version` approach (matches the PostgreSQL "second way", works on any DB). If SQL Server is chosen, the teacher's expected token is `[Timestamp] byte[] RowVersion` instead. Also `HallSlot.Version` currently has **no** `[ConcurrencyCheck]` / `[Timestamp]` attribute yet — it is not an active concurrency token until configured.

---

## 4. History log

### 2026-09-06
- Assistant reviewed the full solution. Confirmed: Core domain layer complete and documented; Data / Service layers empty; API is the default template; no tests.
- Agreed on the working agreement in section 1.
- Created this file.
- Recorded the full teacher brief in section 3.
- Concept confirmed against Part A: EventBooking, limited resource = `HallSlot`.
- Created `WORK_PLAN.md` — ordered steps 0, A–R. Work proceeds one step at a time on the student's word.
- Created `ARCHITECTURE.md` — Clean Architecture layout. All persistence (DbContext, repositories, EF Core, migrations) is in `EventBooking.Data`; `Core` holds interfaces only, no EF packages. Dependency direction documented, plus a full request trace for a booking.
- Explained why the plan is fine-grained steps rather than 4 large phases: the checklist items cross projects and each needs its own explain/approve; the phase order is still preserved (A–C Data, D–G Service, H–J API, N Tests).

**DECISION — Database: PostgreSQL** (student's choice, 2026-09-06). Locked. No switching.
- Provider package: `Npgsql.EntityFrameworkCore.PostgreSQL` 8.0.10 in `EventBooking.Data`.
- Concurrency token: self-managed `[ConcurrencyCheck] Guid Version` on `HallSlot`, bumped in a `SaveChangesAsync` override (teacher's PostgreSQL "second way"). Config in Step B.
- Docker not needed / not installed — student will run a local PostgreSQL server.

**Step 0 — DONE**
- Deleted stray `project-dotnet.sln`.
- `git init` (no commit yet).
- Installed `dotnet-ef` global tool, pinned to 8.0.30 to match net8.0.
- `.gitignore` reviewed — standard dotnet ignore, covers bin/obj/.vs/*.user/.env. OK.

**Step A — DONE (build green)**
- `EventBooking.Data`: added `Npgsql.EntityFrameworkCore.PostgreSQL` 8.0.10 + `Microsoft.Extensions.Configuration.Abstractions` 8.0.0.
- `EventBooking.API`: added `Microsoft.EntityFrameworkCore.Design` 8.0.10 (PrivateAssets=all).
- New: `EventBooking.Data/AppDbContext.cs` — `DbSet<T>` per entity, `ApplyConfigurationsFromAssembly` in `OnModelCreating`.
- New: `EventBooking.Data/DependencyInjection.cs` — `AddDataLayer(services, configuration)` extension: reads `ConnectionStrings:Default`, registers `AddDbContext<AppDbContext>` with `UseNpgsql` (scoped).
- `Program.cs`: `builder.Services.AddDataLayer(builder.Configuration);`.
- User Secrets: initialised on API (`UserSecretsId` in csproj), `ConnectionStrings:Default` set to a local Postgres value (`Host=localhost;Port=5432;Database=EventBookingDb;Username=postgres;Password=postgres`) — **student must adjust to their real local password**.
- `dotnet build` passes. `dotnet-ef dbcontext info` fails **as expected** — `BookingExtraService` has no key yet; that is Step B's first task.

**Step B — DONE (build green, `dotnet-ef dbcontext info` succeeds)**
- New folder `EventBooking.Data/Configurations/` — one `IEntityTypeConfiguration<T>` per entity:
  - `VenueConfiguration`, `HallConfiguration`, `HallSlotConfiguration`, `UserConfiguration`, `BookingConfiguration`, `ExtraServiceConfiguration`, `BookingExtraServiceConfiguration`.
- `AppDbContext.SaveChangesAsync` / `SaveChanges` overridden → `BumpConcurrencyTokens()` gives every **Modified** `HallSlot` a new `Version` before save.
- Design decisions the student should be able to defend:
  - **Concurrency token** configured with Fluent `.IsConcurrencyToken()` (not the `[ConcurrencyCheck]` attribute) — keeps `Core` free of mapping concerns. New value assigned centrally in `SaveChanges`, so no service has to remember it.
  - **Enums stored as strings** (`HasConversion<string>`) — readable in the DB, not tied to int order. Slightly larger columns.
  - **`HallSlot.Date` → `date` column** — it is a calendar date (no time). Also sidesteps the Npgsql "timestamp must be UTC" trap for that column.
  - **`DeleteBehavior.Restrict`** on Venue→Hall, Hall→HallSlot, HallSlot→Booking, User→Booking — a booking system must not cascade-delete history. Cascade only on Booking→BookingExtraService (line items).
  - **Unique indexes:** `HallSlot (HallId, Date, Shift)` (one slot per hall/date/shift), `User.Email`, `Hall (VenueId, Name)`.
  - **Composite key** `BookingExtraService (BookingId, ExtraServiceId)`.
  - Relationship requirement met: multiple one-to-many + one many-to-many (`Booking` ↔ `ExtraService` through `BookingExtraService`).
- `decimal` money columns: `HasPrecision(10, 2)`.

**Step C — PARTLY DONE (migration created & reviewed; `database update` blocked — no running PostgreSQL)**
- New: `EventBooking.Data/Seed/SeedData.cs` — catalog seed via `HasData`, baked into the migration (all constant values):
  - 2 Venues, 3 Halls, 7 HallSlots (fixed `Version` Guids), 4 ExtraServices.
  - Demo `User` accounts deliberately NOT here — they need the password hasher (Step H); will be seeded at runtime.
- `AppDbContext.OnModelCreating` calls `SeedData.Apply(modelBuilder)`.
- Migration created: `EventBooking.Data/Migrations/20260906202027_Init.cs` (+ Designer + `AppDbContextModelSnapshot`). Reviewed — all 7 tables, correct PG types (`numeric(10,2)`, `character varying(n)`, `date`, `uuid`, `timestamp with time zone`), FKs (Restrict / Cascade as configured), composite PK on `BookingExtraServices`, unique indexes (`HallSlots(HallId,Date,Shift)`, `Users.Email`, `Halls(VenueId,Name)`), seed inserts present.
### Machine check (2026-09-06)
PostgreSQL was NOT installed. SQL Server IS installed and running. Student chose to install PostgreSQL (no code changes).

### 2026-09-07 — Step C FINISHED
- Installed **PostgreSQL 17** (17.11-3) via `winget install -e --id PostgreSQL.PostgreSQL.17 --source winget`. Service `postgresql-x64-17` running, listening on 5432. Superuser `postgres` / password `postgres` (winget default) — matches the User Secrets value already set, no change needed. `psql` at `C:\Program Files\PostgreSQL\17\bin\`.
- `dotnet ef database update -p EventBooking.Data -s EventBooking.API` → succeeded. Database `EventBookingDb` created.
- Verified: 7 tables + `__EFMigrationsHistory`; seed rows present (2 venues, 3 halls, 7 slots, 4 extra services).

(Second migration for the "≥2 migrations" rule is Step M, later.)

### 2026-09-07 — Step D DONE (build green, Core still references nothing)
New in `EventBooking.Core`:
- `Common/Result.cs` — `ResultStatus` (Ok/NotFound/Invalid/Conflict/Forbidden), `Result`, `Result<T>`. Services return these for expected failures; API maps them to HTTP codes.
- `DTOs/Common/` — `PagedResult<T>` (Items + Page/PageSize/TotalCount/TotalPages), `PageQuery` (Page/PageSize with `[Range]` + defaults).
- `DTOs/Requests/` — `AuthRequests` (Register/Login), `BookingRequests` (CreateBooking + line items + UpdateBookingStatus), `CatalogRequests` (Venue create/update, Hall create, HallSlot create, `HallSlotQuery` filter/sort, ExtraService create). All with Data Annotations.
- `DTOs/Responses/` — `AuthResponses` (Auth/User), `CatalogResponses` (Venue/Hall/HallSlot/ExtraService), `BookingResponses` (Booking + line items, with `TotalPrice`).
- `Interfaces/IUnitOfWork.cs` — single `SaveChangesAsync` commit point (repositories only stage changes).
- `Interfaces/Repositories/` — `IUserRepository`, `IVenueRepository`, `IHallRepository`, `IHallSlotRepository`, `IBookingRepository`, `IExtraServiceRepository`. Paged methods return `(Items, TotalCount)` tuples; filtering/sorting params are primitives (no web DTO leaks into Data).
- `Interfaces/Services/` — `IAuthService`, `IVenueService` (full CRUD), `IHallService`, `IHallSlotService`, `IExtraServiceService`, `IBookingService`.

Design notes to defend:
- **DTOs grouped by feature area** (not one-file-per-type) — fewer files, easier to navigate. Still one-file-per-interface for the repository/service seams.
- **`IUnitOfWork` separate from repositories** — repos don't expose `SaveChanges`; the booking service wraps `_uow.SaveChangesAsync` in the `try/catch (DbUpdateConcurrencyException)`.
- **Repository paged signatures take primitives**, service maps `HallSlotQuery` → those params. Keeps `Data` free of `EventBooking.Core.DTOs`.
- Every async method carries `CancellationToken ct = default`.

### 2026-09-07 — Step E DONE (build green)
New in `EventBooking.Data`:
- `UnitOfWork.cs` — `IUnitOfWork` over `AppDbContext.SaveChangesAsync`.
- `Repositories/` — `UserRepository`, `VenueRepository`, `HallRepository`, `HallSlotRepository`, `BookingRepository`, `ExtraServiceRepository`. Each takes `AppDbContext` via constructor.
- `DependencyInjection.AddDataLayer` now registers `IUnitOfWork` + all 6 repositories as **Scoped**.

Checklist items covered:
- **Real pagination:** `query.Skip((page-1)*size).Take(size)` inside the `IQueryable`, plus a separate `CountAsync` — never `ToList` then slice.
- **`AsNoTracking`** on every read-only query (lists, "…WithHall/WithVenue" reads, `GetDetailedById`). Tracking kept only where a mutation follows: `HallSlotRepository.GetByIdAsync` (booking flow), `BookingRepository.GetByIdAsync` (cancel/status), `VenueRepository.GetByIdAsync` (update/delete) — each commented.
- **`Include`/`ThenInclude`:** `BookingRepository.DetailedQuery()` = booking → slot → hall → venue + booking → extra-service lines → service, with `AsSplitQuery()` (fixed round-trips, no N+1). `HallSlotRepository` includes hall → venue.
- **Filter + sort in the DB:** `HallSlotRepository.GetPagedAsync` builds `Where` clauses for hallId / date range / status and `OrderBy` for date|price + direction.
- **Async + `CancellationToken`** threaded through every call (`ToListAsync(ct)`, `FirstOrDefaultAsync(ct)`, `AnyAsync(ct)`, `CountAsync(ct)`).

Repositories return **entities**, not DTOs — mapping happens in the Service layer (Step F/G).

### 2026-09-07 — Step F DONE (build green, 0 warnings)
- `EventBooking.Service`: added `AutoMapper` **13.0.1** + `Microsoft.Extensions.Configuration.Abstractions`.
- New `Service/Mapping/`: `AuthMappingProfile`, `CatalogMappingProfile`, `BookingMappingProfile` (Profile classes).
  - entity → response: `User→UserResponse`, `Venue→VenueResponse`, `Hall→HallResponse` (VenueName), `HallSlot→HallSlotResponse` (HallName/VenueName/Shift/Status as strings), `ExtraService→ExtraServiceResponse`, `Booking→BookingResponse` (denormalised slot/hall/venue + `TotalPrice` = base + Σ line totals), `BookingExtraService→BookingExtraServiceResponse`.
  - request → entity: `CreateVenue/UpdateVenue/CreateHall/CreateHallSlot/CreateExtraService`, with `Id` / navigation / status / version members explicitly `.Ignore()`d so config validation stays clean.
- New `Service/DependencyInjection.cs` → `AddServiceLayer(services, config)`; currently registers `AddAutoMapper(<assembly>)`. Service registrations come in Step G.
- `Program.cs` now calls `builder.Services.AddServiceLayer(builder.Configuration);`.

Decision to defend — **AutoMapper version 13.0.1**:
- `dotnet build` flagged `NU1903` — AutoMapper CVE-2026-32933 (StackOverflow from ~25,000-level nested object graphs), fixed only in 15.1.1 / 16.1.1.
- AutoMapper 15+ requires a **license key** (free for non-commercial, but extra setup). 13.0.1 is the last MIT / keyless release.
- The CVE is **not reachable here**: `System.Text.Json` rejects payloads deeper than 64 levels before AutoMapper runs, and every DTO graph here is shallow.
- So: stay on 13.0.1, suppress just `NU1903` in `EventBooking.Service.csproj` with a comment explaining the above.

TODO (Step N): add an AutoMapper `AssertConfigurationIsValid()` test.

### 2026-09-07 — Step G DONE (build green; API starts, DI container validates)
- `EventBooking.Service`: added `Microsoft.EntityFrameworkCore` 8.0.10 (for `DbUpdateConcurrencyException` / `DbUpdateException`) + `Microsoft.Extensions.Logging.Abstractions` 8.0.2. Service referencing EF Core is fine — the point-deduction rule is Core-only, and the brief's own Part C example catches `DbUpdateConcurrencyException` in the service.
- New `Service/Services/`: `VenueService` (full CRUD), `HallService`, `HallSlotService`, `ExtraServiceService`, `BookingService`.
- All use **constructor injection only**, no `new` on a service, everything async with `CancellationToken`.
- Business/state validation lives in the services (slot availability, venue/hall existence, duplicate extra services, "already cancelled", FK-restrict on venue delete → Conflict).
- Registered in `AddServiceLayer` as Scoped (`IAuthService` deferred to Step H).

**The concurrency flow (`BookingService.CreateAsync`) — Part C:**
1. Load the slot **tracked** (`IHallSlotRepository.GetByIdAsync`).
2. Business check: `slot.Status == Available` and no active booking — else `Result.Conflict`.
3. Resolve extra services in one `WHERE Id IN (...)` query; snapshot `PriceAtBooking`.
4. Build the `Booking` (+ line items).
5. `slot.Status = Booked` — mutates the tracked entity.
6. `_uow.SaveChangesAsync` — **one transaction**. `AppDbContext` bumps `slot.Version`; EF puts the ORIGINAL `Version` in `UPDATE "HallSlots" ... WHERE "Version" = @original`. Lost race → 0 rows → `DbUpdateConcurrencyException` → caught → `_logger.LogWarning(...)` (Part D) → `Result.Conflict` (→ 409).
7. Reload detailed and map to `BookingResponse`.
- `CancelAsync` / `SetStatusAsync` also release/So take the slot and go through the same `catch (DbUpdateConcurrencyException)` helper.

Verified: `dotnet run` starts the API, `GET /swagger/v1/swagger.json` → 200, `builder.Build()` (Development → ValidateOnBuild) constructs every registered service without error.

### 2026-09-07 — Step H DONE (build green 0 warnings; API starts, demo users seeded, Swagger 200)
New in `EventBooking.Core` (still references nothing):
- `Configuration/JwtOptions.cs` — POCO (`Issuer`, `Audience`, `Key`, `ExpiryMinutes`), `SectionName = "Jwt"`.
- `Interfaces/Security/IPasswordHasher.cs` — `Hash` / `Verify`.
- `Interfaces/Security/ITokenService.cs` — `Generate(User) → (Token, ExpiresAtUtc)`.
- `Common/Result.cs` — added `ResultStatus.Unauthorized = 5` (→ 401) + `Result.Unauthorized` / `Result<T>.Unauthorized`. Login failure now returns this, not `Invalid`.

New in `EventBooking.Service`:
- Packages: `BCrypt.Net-Next` 4.0.3, `System.IdentityModel.Tokens.Jwt` 8.1.2, `Microsoft.Extensions.Options.ConfigurationExtensions` 8.0.0.
- `Security/BCryptPasswordHasher.cs` — work factor 12; salt + factor embedded in the hash string.
- `Security/JwtTokenService.cs` — HMAC-SHA256, claims: `sub` (User.Id), `email`, `ClaimTypes.Role` (so `[Authorize(Roles=…)]` works), `jti`. Reads `IOptions<JwtOptions>`.
- `Services/AuthService.cs` — `Register`: lowercase-trim email → `EmailExists` check (Conflict) → hash → `User` with `Role = Client` → save → token. `Login`: lookup → `Verify` → same "Invalid email or password" message for unknown-email and wrong-password (no account enumeration) → `Unauthorized` on failure.
- `DependencyInjection.AddServiceLayer`: `Configure<JwtOptions>(section)`, `IPasswordHasher` + `ITokenService` as **Singleton** (stateless), `IAuthService` as **Scoped** (uses scoped repos).

New in `EventBooking.API`:
- Package: `Microsoft.AspNetCore.Authentication.JwtBearer` 8.0.10.
- `Program.cs`: reads the `Jwt` section, `AddAuthentication(JwtBearer).AddJwtBearer(...)` with full `TokenValidationParameters` (validate issuer/audience/signing key/lifetime, 30s clock skew), `AddAuthorization()`. Pipeline: `UseAuthentication()` **before** `UseAuthorization()`.
- `Infrastructure/DemoUserSeeder.cs` — `app.SeedDemoUsersAsync()` after `MapControllers()`: inserts 2 demo users if missing (hashing needs the runtime hasher, can't live in a migration).
  - `client@eventbooking.local` / role Client, `manager@eventbooking.local` / role Manager, password `Passw0rd!` for both (→ README).
- `appsettings.json`: added `Jwt` (Issuer `EventBooking.API`, Audience `EventBooking.Client`, ExpiryMinutes 60). **`Jwt:Key` set in User Secrets only** — never in the repo.

Verified: `dotnet build` 0W/0E; API boots, seeder inserted both users on first run, `GET /swagger/v1/swagger.json` → 200, DI container validates.

### 2026-09-07 — Step I DONE (build green 0W; full happy path + 401/403/409 verified end-to-end)
New in `EventBooking.API`:
- `Infrastructure/ApiControllerBase.cs` — `[ApiController]` base. `CurrentUserId` (from `sub` / NameIdentifier claim), `IsManager` (role claim). `ToResponse(Result)` / `ToResponse(Result<T>)` + `ErrorResult(Result)` map `ResultStatus` → 200/204/400/401/403/404/409 as `ProblemDetails`. Automatic `[ApiController]` 400 on invalid ModelState = the required "ModelState check" (no manual `if (!ModelState.IsValid)`).
- `Controllers/` — 6 controllers, explicit `[Route("api/<plural-noun>")]`, explicit `[FromRoute]`/`[FromQuery]`/`[FromBody]`, `[ProducesResponseType]` for Swagger:
  - `AuthController` (`api/auth`, `[AllowAnonymous]`): `POST register` (200/409), `POST login` (200/401).
  - `VenuesController` (`api/venues`): `GET` paged + `GET {id}` anon; `POST` (201+Location) / `PUT {id}` / `DELETE {id}` (204, 409 if halls exist) — `[Authorize(Roles="Manager")]`.
  - `HallsController` (`api/halls`): `GET {id}` + `GET ?venueId=` (BindRequired) anon; `POST` Manager (201).
  - `HallSlotsController` (`api/hall-slots`): `GET` (filter/sort/page via `HallSlotQuery`) + `GET {id}` anon; `POST` Manager (201).
  - `ExtraServicesController` (`api/extra-services`): `GET` anon; `POST` Manager (201).
  - `BookingsController` (`api/bookings`, class-level `[Authorize]`): `POST` Client (201/400/409 — the competed op), `GET mine` paged, `GET {id}` (403 if not owner/Manager), `GET` Manager (`?status=`), `DELETE {id}` Client cancel (204, releases slot), `PATCH {id}/status` Manager.
- `Program.cs`: `AddControllers().AddJsonOptions(JsonStringEnumConverter)` (enums as names); `AddSwaggerGen` now has a Bearer `AddSecurityDefinition` + `AddSecurityRequirement` (the "Authorize" button — technically Step O, done early so endpoints are testable).

Decision (student picked): booking cancel = `DELETE api/bookings/{id}` → 204 (verb, not `/cancel`); manager status change = `PATCH api/bookings/{id}/status`. Browsing GETs (venues/halls/slots/extra-services) are anonymous; writes are Manager; all bookings need auth.

Verified live against the seeded DB: register→200+token, login→200, anon `GET /api/hall-slots`→200, client `POST /api/venues`→403, no-token `GET /api/bookings/mine`→401, client `POST /api/bookings`→201, repeat→409 (ProblemDetails), `GET /api/bookings/mine`→200, manager `PATCH .../status {Confirmed}`→200, client `DELETE`→204, manager `POST /api/venues`→201 + `Location` header. Test rows cleaned from EventBookingDb afterwards (back to seed: 2 venues, 2 users, 0 bookings, all slots Available).

### 2026-09-07 — Step J DONE (build green 0W; correlation id + uniform 500 verified live)
New folder `EventBooking.API/Middleware/`:
- `CorrelationIdMiddleware` — reads `X-Correlation-Id` from the request or generates a `Guid`; stores it on `HttpContext.Items` + `TraceIdentifier`, echoes it on the response via `Response.OnStarting`, and wraps the rest of the pipeline in `_logger.BeginScope({ CorrelationId })`. Logs one "Incoming request {Method} {Path}" line per request.
- `ExceptionHandlingMiddleware` — outermost. `try/await _next`. Swallows client-abort `OperationCanceledException` (logs Info). Any other exception → `LogError(ex, …)` → uniform `ProblemDetails` JSON (500, `Type` rfc9110 §15.6.1, `correlationId` extension; `Detail` = full exception only in Development, else "See the server logs for details."). If `Response.HasStarted` it logs a Warning and rethrows.
- `MiddlewareExtensions` — `UseExceptionHandling()` / `UseCorrelationId()`.
- `Program.cs` pipeline is now, in order: `UseExceptionHandling` → `UseCorrelationId` → (dev) Swagger → `UseHttpsRedirection` → `UseAuthentication` → `UseAuthorization` → `MapControllers`.

Verified live: `X-Correlation-Id` auto-generated on a normal response; a caller-supplied id is echoed back unchanged; 404/400 still render as `ProblemDetails` (unchanged); a forced unhandled `FormatException` returned `500` with the uniform body + `correlationId`, and the log showed `fail: …ExceptionHandlingMiddleware … Unhandled exception for GET /api/bookings/mine`.

Note (minor, not fixed): `ApiControllerBase.CurrentUserId` does `Guid.Parse` and would 500 on a validly-signed token with a non-Guid `sub`. Not reachable via our own `JwtTokenService` (always emits a real Guid). Could harden to 401 later if wanted.

Console logger does not render scopes by default, so the CorrelationId scope is set but not visible in the plain console yet — NLog (Step K) will write it into the log file/layout.

### 2026-09-07 — Step K DONE (build green 0W; request/Warning/Error lines with CorrelationId verified in the log file)
- `EventBooking.API`: added `NLog.Web.AspNetCore` 5.3.14; csproj copies `nlog.config` to output (`PreserveNewest`).
- New `EventBooking.API/nlog.config`:
  - Targets: `File` → `${basedir}/logs/eventbooking-${shortdate}.log` (layout: longdate | level | `cid=${scopeproperty:item=CorrelationId}` | logger | message | exception), plus a compact `Console` target.
  - Rules: `Microsoft.Hosting.Lifetime` Info+ kept (so "Now listening on…" still shows); other `Microsoft.*` / `System.Net.Http.*` dropped below Warning (kills EF Core SQL spam); everything else Debug+ → file, Info+ → console.
- `Program.cs`: wrapped in `try/catch/finally`; `LogManager.Setup().LoadConfigurationFromAppSettings()`, `builder.Logging.ClearProviders()`, `builder.Host.UseNLog()`, `LogManager.Shutdown()` in `finally`. Code still logs through `ILogger<T>` — NLog only sits underneath.
- `ExceptionHandlingMiddleware`: re-attaches the `CorrelationId` scope before `LogError` (it runs outside `CorrelationIdMiddleware`'s scope) so the Error line carries the id too.
- `BookingService.CreateAsync`: added `LogWarning` on the two business-check conflict paths (slot not Available / already has an active booking) — "every resource collision at Warning" (Part D). The `DbUpdateConcurrencyException` path already logged Warning.
- `.gitignore` already ignores `[Ll]ogs/` and `*.log` — no change.

Verified in `logs/eventbooking-2026-09-07.log`: `INFO … cid=<id> … Incoming request POST /api/bookings`; `WARN … cid=<id> … BookingService … Booking collision: … slot 3 … status is Booked`; `ERROR … cid=<id> … ExceptionHandlingMiddleware … Unhandled exception`. A caller-supplied `X-Correlation-Id: trace-me-999` appeared on all three lines and on the response header. Test booking cleaned from EventBookingDb.

### 2026-09-07 — Step L DONE (build green 0W; happy path + startup validation both verified)
- `JwtOptions` (Core): added Data Annotations — `[Required]` on Issuer/Audience, `[Required, MinLength(32)]` on Key, `[Range(1,1440)]` on ExpiryMinutes.
- `Program.cs`: replaced the manual `builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? throw` with
  `AddOptions<JwtOptions>().Bind(section).ValidateDataAnnotations().ValidateOnStart()`. A bad/missing key now fails the **host at startup** with a clear `OptionsValidationException` (logged by NLog + the `catch` in Program.cs), not a 500 on first use.
- New `EventBooking.API/Infrastructure/ConfigureJwtBearerOptions.cs` — `IConfigureNamedOptions<JwtBearerOptions>`, ctor-injected with `IOptions<JwtOptions>`, builds the `TokenValidationParameters`. Registered via `services.ConfigureOptions<ConfigureJwtBearerOptions>()`, so `Program.cs` now calls a bare `.AddJwtBearer()` with no inline lambda and no direct config read.
- `Service/DependencyInjection.AddServiceLayer`: removed `services.Configure<JwtOptions>(...)` — the composition root (API) now owns binding + validation; the Service layer only *consumes* `IOptions<JwtOptions>` (in `JwtTokenService`). `Program.cs` no longer needs `using System.Text` / `Microsoft.IdentityModel.Tokens`.
- `appsettings.json` reviewed: only safe defaults (`Logging`, `AllowedHosts`, `Jwt` Issuer/Audience/ExpiryMinutes). No secret in any `appsettings*.json`. `Jwt:Key` + `ConnectionStrings:Default` remain User-Secrets-only (verified with `dotnet user-secrets list`).

Verified: normal startup + authed/unauthed requests still 200/401; running with `Jwt__Key=tooshort` → host refuses to start, `ERROR … OptionsValidationException … 'Key' … minimum length of '32'`.

### 2026-09-07 — Step M DONE (second migration created, reviewed, applied; history now has 2)
- `BookingConfiguration`: added `HasIndex(b => new { b.OwnerUserId, b.CreatedAtUtc }).IsDescending(false, true)` — matches the "my bookings" query exactly (`WHERE OwnerUserId = @id ORDER BY CreatedAtUtc DESC`): filter column ascending, sort column descending.
- `dotnet ef migrations add AddBookingOwnerCreatedAtIndex -p EventBooking.Data -s EventBooking.API`. Reviewed `20260907163052_AddBookingOwnerCreatedAtIndex.cs`: drops the auto FK index `IX_Bookings_OwnerUserId`, creates `IX_Bookings_OwnerUserId_CreatedAtUtc` with `descending: [false, true]`; `Down` reverses it. Composite still covers plain FK lookups (OwnerUserId is the leading column).
- `dotnet ef database update` → applied. `__EFMigrationsHistory` now has `20260906202027_Init` + `20260907163052_AddBookingOwnerCreatedAtIndex`. Verified in `pg_indexes`: `... ("OwnerUserId", "CreatedAtUtc" DESC)`.
- Fixed a side annoyance: `Program.cs` now has `catch (HostAbortedException) { throw; }` before the generic catch, so `dotnet ef …` no longer logs a spurious `ERROR … terminated unexpectedly` line.

### 2026-09-07 — Steps N, O, P DONE (server complete; `dotnet test` 14/14 green)

**Step N — tests.** `EventBooking.Tests`: added `Moq` 4.20.72, `Microsoft.EntityFrameworkCore.Sqlite` 8.0.10, `AutoMapper` 13.0.1 (NU1903 suppressed with the same justification as Service).
- `Services/BookingServiceTests.cs` (6) — `CreateAsync` with every dependency mocked: success (slot taken + saved once), slot-not-Available → Conflict + never saves, slot-has-active-booking → Conflict, `SaveChangesAsync` throws `DbUpdateConcurrencyException` → Conflict, slot missing → NotFound, unknown extra service → Invalid. **The mandatory limited-resource success + rejection test.**
- `Data/HallSlotConcurrencyTests.cs` (2) — **the separately-evaluated proof.** In-memory SQLite (open `SqliteConnection`, `EnsureCreated` builds schema + HasData seed). Two `AppDbContext`s load slot 1; `ctxA.SaveChanges()` wins; `ctxB.SaveChanges()` → `Assert.Throws<DbUpdateConcurrencyException>`. Second test: reload → next update returns 1 row.
- `Services/AuthServiceTests.cs` (5) — login unknown email / wrong password → Unauthorized; register taken email → Conflict; happy register hashes + forces Client role + saves.
- `Mapping/MappingProfilesTests.cs` (1) — `AssertConfigurationIsValid()` over all 3 profiles (the Step F TODO).
- Build 0W/0E; `dotnet test` → 14 passed.

**Step O — Swagger + end-to-end.** Verified live against the seeded DB:
- `GET /swagger/index.html` → 200; `swagger.json` has `components.securitySchemes.Bearer` and 13 documented paths.
- register → 200 + token; login → 200; `GET /api/hall-slots?page=1&pageSize=2` → `{page:1,pageSize:2,totalCount:7,items:[id 1,2]}` (real paging); `POST /api/bookings` (slot 6, +10× florals) → 201, total 11800; same slot again → **409** ProblemDetails. Test rows cleaned from EventBookingDb.

**Step P — README finalised.** Removed the status note; filled "Running the tests" (per-class coverage) and the tech-stack Tests row; noted the 2-migration history; all `(pending)` markers gone. `README.md` now describes the finished server end to end.

### 2026-09-07 — Step P DRAFTED EARLY (at student's request; superseded by the finalised version above)
- New: `README.md` at the solution root. Sections: what-it-is, **the limited resource + concurrency handling** (Part C paragraph), tech stack, prerequisites, setup (User Secrets for `ConnectionStrings:Default` + `Jwt:Key`, `dotnet ef database update`), run (ports 5269 / 7021, Swagger at `/swagger`), demo users table (manager@ / client@ / `Passw0rd!`), roles×permissions table, e2e smoke test for the 409 path, project structure.
- Marked _(pending)_: middleware/CorrelationId (J), NLog (K), tests (N). A status note at the top says to remove it before submission and lists what is still placeholder.
- Revisit at the real Step P: drop the status note, fill the "Middleware & logging" and "Running the tests" sections, confirm the second migration is mentioned.

**Server: DONE through Step P.** Build 0W/0E, `dotnet test` 14/14, full e2e (incl. 409) verified.

**Step R (server half) — IN PROGRESS:**
- `.gitignore`: added `/image.png`, `/cghv.png`, `**/logs/`, `internal-nlog*.txt`.
- `git config` (local): user.name `shulamit20`, user.email `z0548549581@gmail.com`.
- Branch renamed `master` → `main`.
- **First commit done: `c2e93cc`** — 103 files, no bin/obj/logs/secrets (verified: `appsettings.Development.json` has only Logging; User Secrets live outside the repo).
- **Pushed to GitHub: `https://github.com/shulamit20/EventBooking-Server`** (branch `main`). GCM had cached credentials — no manual login needed.
- Commit `65209af` — **CORS**: `Program.cs` `AddCors`/`UseCors` policy `DevClient` for `http://localhost:5173` (config `Cors:AllowedOrigins`), `UseCors` before `UseAuthentication` so preflight OPTIONS needs no token. `appsettings.json` got the `Cors` section. Verified: preflight → 204 with `Access-Control-Allow-*`; GET with `Origin` → `Access-Control-Allow-Origin: http://localhost:5173`.
- TODO: final pass over the Part B checklist in section 3.

### 2026-09-07 — Step Q DONE (React client built; `npm run build` clean, CORS verified)
Separate project: `C:\Users\User\Desktop\לימודים תכנות שנה ב\eventbooking-client` (own git repo, branch `main`, commit `f237ddc` — **not pushed yet, needs a 2nd GitHub repo**).
- Vite 5 + React 18 + `react-router-dom` 6, plain JS/JSX, `fetch` (no axios). `npm install` → 66 pkgs.
- `src/api.js` — the only place that calls `fetch`: base URL (`VITE_API_URL` ?? `http://localhost:5269`), `Authorization: Bearer` from `localStorage`, throws `ApiError` with `.status` (so screens branch on 409 / 401).
- `src/auth.jsx` — `AuthContext`, token + user in `localStorage`, `login` / `register` / `logout`.
- Screens (the brief's 4): `LoginPage` (login/register tabs, stores + sends JWT), `SlotsPage` (`GET /api/hall-slots?page=&pageSize=5` — **real server pagination**, Prev/Next + "page X of Y · N total"), `BookPage` (`POST /api/bookings`; on **409** shows a distinct "התאריך נתפס" panel, not a generic error; 401 → redirect to login), `MyBookingsPage` (`GET /api/bookings/mine` + cancel via `DELETE`). `Nav` shows the "הזמנה" link only for role `Client`.
- `npm run build` → 41 modules, no errors (proves every JSX/import/syntax across all files). CORS path verified with curl (preflight + GET). Not visually click-tested (no browser tool this session — the student verifies in the browser).
- Client README with run steps + the 4-screen table + how to trigger the 409.
- **Pushed to GitHub: `https://github.com/shulamit20/EventBooking-Client`** (branch `main`, commit `f237ddc`, 16 files). GCM cached credentials again.

**Both repos are now on GitHub. Remaining: student runs the client in a browser to eyeball it; final Part B checklist pass.**
- **Open decision:** choose SQL Server or PostgreSQL before any Data-layer work (affects concurrency-token style and all migrations).
- **Next step (waiting for approval):** start the Data layer — EF Core packages in `EventBooking.Data`, `Microsoft.EntityFrameworkCore.Design` in `EventBooking.API`, `AppDbContext` with a `DbSet` per entity, Fluent API configs, `SaveChangesAsync` override for the concurrency token (if self-managed), connection string via User Secrets, first migration.

### 2026-09-05 (earlier work, by student)
- Created the solution and 5 projects with references wired.
- Wrote all Core entities and enums with XML documentation.
  ┌──────┬─────────────────────────────────────────────────────────────────────────────────┐
  │ Step │                                      What                                       │
  ├──────┼─────────────────────────────────────────────────────────────────────────────────┤
  │ 0    │ Decisions & cleanup — pick the DB, delete stray .sln, git init                  │
  ├──────┼─────────────────────────────────────────────────────────────────────────────────┤
  │ A    │ Data foundation — EF packages, AppDbContext, connection string via User Secrets │
  ├──────┼─────────────────────────────────────────────────────────────────────────────────┤
  │ B    │ Fluent API configs, relationships, concurrency token, SaveChanges override      │
  ├──────┼─────────────────────────────────────────────────────────────────────────────────┤
  │ C    │ First migration + seed data                                                     │
  ├──────┼─────────────────────────────────────────────────────────────────────────────────┤
  │ D    │ Core interfaces + request/response DTOs + Result type                           │
  ├──────┼─────────────────────────────────────────────────────────────────────────────────┤
  │ E    │ Repository implementations (CRUD, query pagination, AsNoTracking, Include)      │
  ├──────┼─────────────────────────────────────────────────────────────────────────────────┤
  │ F    │ AutoMapper profiles                                                             │
  ├──────┼─────────────────────────────────────────────────────────────────────────────────┤
  │ G    │ Service layer + the booking/concurrency flow (409 on conflict)                  │
  ├──────┼─────────────────────────────────────────────────────────────────────────────────┤
  │ H    │ JWT auth — register/login, hashing, 2 roles                                     │
  ├──────┼─────────────────────────────────────────────────────────────────────────────────┤
  │ I    │ API controllers — REST, status codes, [Authorize]                               │
  ├──────┼─────────────────────────────────────────────────────────────────────────────────┤
  │ J    │ Middleware — global error handler + CorrelationId                               │
  ├──────┼─────────────────────────────────────────────────────────────────────────────────┤
  │ K    │ NLog logging                                                                    │
  ├──────┼─────────────────────────────────────────────────────────────────────────────────┤
  │ L    │ Configuration finalization + IOptions                                           │
  ├──────┼─────────────────────────────────────────────────────────────────────────────────┤
  │ M    │ Second migration                                                                │
  ├──────┼─────────────────────────────────────────────────────────────────────────────────┤
  │ N    │ Unit tests — incl. the two-DbContext concurrency test                           │
  ├──────┼─────────────────────────────────────────────────────────────────────────────────┤
  │ O    │ Swagger + manual end-to-end check                                               │
  ├──────┼─────────────────────────────────────────────────────────────────────────────────┤
  │ P    │ Server README                                                                   │
  ├──────┼─────────────────────────────────────────────────────────────────────────────────┤
  │ Q    │ React client (4 screens, minimum)                                               │
  ├──────┼─────────────────────────────────────────────────────────────────────────────────┤
  │ R    │ Two GitHub repos + final checklist pass                                         ](image.png)
