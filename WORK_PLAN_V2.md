# EventBooking v2 — Work Plan

Implements `FUNCTIONAL_SPEC.md`. Same rules as `WORK_PLAN.md`: one step at a time, explain →
approve → implement. **Phase 1 = functionality only, no UI redesign.** The HallSlot 409
concurrency mechanism is preserved throughout (spec §20).

Legend: **new** = new file/entity · **mod** = change to existing code.

---

## Decisions (settled 2026-09-08)

1. **Role name** — rename `UserRole.Client` → `Customer`, add `Admin`.
2. **Event Builder** — server-side **draft `Booking`** (`BookingStatus.Draft`, no slot taken
   until "confirm"; "confirm" reuses the existing concurrency take → 409).
3. **Venue ownership** — add `Venue.OwnerUserId`.
4. **React data layer** — keep plain `fetch` (simpler to explain), no React Query.
5. **enum vs lookup table** — enum ONLY where code branches on the value:
   `BookingStatus`, `SlotStatus`, `UserRole`, `NotificationType`, and a new
   `PricingModel { Flat, PerGuest }` (the price calculator switches on it).
   Everything the business manages as content is a **lookup table**:
   `EventType`, `ServiceCategory` (+ `EventTypeServiceCategory` link), `CateringMenu`,
   `Promotion`. So the platform is extensible without a redeploy.

---

## Phase A — Domain foundation + roles  *(1 migration)*

- **A1** `UserRole` — **mod**: rename `Client` → `Customer`, add `Admin`. Propagate:
  `JwtTokenService` (auto), `DemoUserSeeder` (+ Admin demo user, fix existing rows' role),
  `BookingsController` `[Authorize(Roles=...)]`, `AuthServiceTests`, React (`Nav.jsx`,
  `SlotsPage.jsx`), READMEs.
- **A2** `PricingModel` enum (`Flat`, `PerGuest`) — **new** in Core.
- **A3** `EventType` lookup entity (`Id`, `Name`, `Description`, `IsActive`) — **new** + config
  + `HasData` seed (Wedding, BarMitzvah, BatMitzvah, CorporateEvent, Birthday, PrivateEvent,
  Other).
- **A4** `ServiceCategory` lookup entity (`Id`, `Code`, `Name`, `IsActive`) — **new** + config
  + seed (Catering, TableDesign, BridalChair, Photography, DJ, Flowers, Lighting, Other).
  `EventTypeServiceCategory` link entity (composite key) + seed the sensible pairs.
- **A5** `ExtraService` — **mod**: `ServiceCategoryId` FK, `PricingModel`, `ImageUrl`,
  `IsActive`, `OwnerUserId` (Manager). Config + `User` 1→* `ExtraService`,
  `ServiceCategory` 1→* `ExtraService`. Update seed rows + AutoMapper + DTOs.
- **A6** `Booking` — **mod**: `EventType` `string` → `EventTypeId` FK. Add `BookingStatus.Draft`.
  Update `CreateBookingRequest`/responses/mapping/`BookingService`.
- **A7** `Venue` — **mod**: `OwnerUserId` FK + config.
- **A8** Migration `V2Foundation` + `dotnet ef database update`. Fix demo-user roles at startup.
- **DoD:** build green, `dotnet test` green (existing concurrency test unchanged), DB updated,
  server + client still run.

## Phase B — Catering + server-side price calculation

- **B1** `CateringMenu` entity (`Id`, `Name`, `Description`, `PricePerGuest`, `IsVegetarian`,
  `IsVegan`, `IncludesDrinks`, `IsActive`, `OwnerUserId`) — **new** + config + repo + service +
  `CateringController` (Manager CRUD of own, public `GET`).
- **B2** `Booking` — **mod**: optional `CateringMenuId`, `CateringGuestCount`.
- **B3** `PriceCalculationService` — **new** in Service: total from DB prices =
  slot `BasePrice` + catering (`PricePerGuest` × guests) + Σ (`ExtraService.Price` × `Quantity`).
  Returns a breakdown DTO.
- **B4** `BookingService.CreateAsync` — **mod**: computes the total server-side, ignores any
  client total, snapshots it. `GET /api/bookings/{id}/price` (breakdown). Preserve the 409 flow.
- **DoD:** price test (300×180 + venue + services); client-sent total is ignored.

## Phase C — Event Builder

- **C1** Draft flow (decision 2): `POST /api/bookings/drafts` (status Draft, no slot taken),
  `PUT /api/bookings/{id}` to update selections, `POST /api/bookings/{id}/confirm` → existing
  HallSlot concurrency take → **409** on conflict.
- **C2** `GET /api/bookings/mine?status=Draft` → "My Events"; `?status=Confirmed` → "My Bookings".
- **DoD:** build a draft, add services, confirm → booking; confirm a taken slot → 409.

## Phase D — Messaging

- **D1** `Message` entity (`Id`, `SenderId`, `ReceiverId`, `BookingId?`, `Content`, `CreatedAt`,
  `IsRead`) — **new** + config.
- **D2** Repo + `MessageService` + `MessagesController`: send, get thread, my threads, unread
  count, mark read. Auth: only customer ↔ the manager of a venue/service/booking they share.

## Phase E — Notifications

- **E1** `Notification` entity (`Id`, `UserId`, `Type`, `Title`, `Body`, `RelatedId?`,
  `CreatedAt`, `IsRead`) + `NotificationType` enum — **new**.
- **E2** Repo + `NotificationService` (create, list, unread count, mark read) + controller.
- **E3** Triggers — **mod**: booking approved/rejected/cancelled (`BookingService`), new message
  (`MessageService`), new promotion (`PromotionService`).

## Phase F — Promotions

- **F1** `Promotion` entity (`Id`, `Title`, `Description`, `DiscountPercentage?`,
  `DiscountAmount?`, `StartDate`, `EndDate`, `ImageUrl`, `IsActive`, `OwnerUserId`) — **new**.
- **F2** Repo + service + controller: Manager CRUD of own; public `GET /api/promotions/active`
  (`IsActive` && now ∈ [`StartDate`, `EndDate`]).
- **F3** On create → notification (Phase E).

## Phase G — Reviews

- **G1** `Review` entity (`Id`, `CustomerId`, `VenueId?` / `ExtraServiceId?`, `BookingId`,
  `Rating` 1–5, `Text`, `CreatedAt`) — **new**.
- **G2** Repo + service + controller. Rule: `POST` only if the customer has a
  Confirmed/Completed booking tied to that venue/service. `GET` list + average per venue.

## Phase H — Search & filtering

- **H1** Venue/slot search — **mod**: filter by event type, guest count (vs `Hall.Capacity`),
  price range, date, city/area, required `ServiceType`s, availability — all in the EF query.

## Phase I — Dashboards

- **I1** `GET /api/dashboard/customer` — my events (drafts), my bookings, selected services,
  price estimates, unread messages, unread notifications, active promotions.
- **I2** `GET /api/dashboard/manager` — own venues/halls/slots, incoming bookings, customers,
  unread messages, own services, own promotions, availability, basic stats. Own resources only.

## Phase J — Authorization pass

- **J1** `Admin` role everywhere "manage the entire system". Manager-owns-resource checks on
  every manager write (venue, hall, slot, extra service, catering menu, promotion). 403 otherwise.

## Phase K — React (functional only, no redesign)

- **K1** `src/api.js` — add all new endpoints.
- **K2** Pages/components: Event Builder wizard, My Events, Messages, notifications bell,
  promotion modal, Customer dashboard, Manager dashboard, venue search filters, Reviews.
- **K3** Routes + nav. Keep styling minimal.

## Phase L — Tests · migrations · final report

- **L1** xUnit: price calculation, review booking-gate, message authorization, promotion
  active-range; confirm the existing booking-conflict test still passes.
- **L2** All migrations applied; DB verified; `dotnet test` green.
- **L3** Final report (spec §23 format) in `PROJECT_LOG.md`; update `README.md` + `ARCHITECTURE.md`.
