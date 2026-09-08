# EventBooking v2 — Work Plan

Implements `FUNCTIONAL_SPEC.md`. Same rules as `WORK_PLAN.md`: one step at a time, explain →
approve → implement. **Phase 1 = functionality only, no UI redesign.** The HallSlot 409
concurrency mechanism is preserved throughout (spec §20).

Legend: **new** = new file/entity · **mod** = change to existing code.

---

## Open decisions (settle before Phase A)

1. **Role name** — spec says Customer/Manager/Admin; code has `Client`/`Manager`.
   → *Recommended:* rename `UserRole.Client` → `Customer`, add `Admin`. (Touches JWT claims,
   seed users, tests, client — all small.)
2. **Event Builder** — a server-side **draft `Booking`** (`BookingStatus.Draft`, no slot taken
   until "confirm"), or a **client-only wizard** that collects everything then does one existing
   `POST /api/bookings`?
   → *Recommended:* draft Booking. It gives "My Events" vs "My Bookings" + price estimates for
   free, and "confirm" reuses the existing concurrency take → 409. Small, not "advanced".
3. **Venue ownership** — add `Venue.OwnerUserId` so a Manager owns venues (needed for §13/§16
   "manage only their own"). → *Recommended:* yes, in Phase A.
4. **React data layer** — spec §17 mentions React Query; current client uses plain `fetch`.
   → *Recommended:* keep plain `fetch` + small hooks (simpler to explain); revisit only if it
   gets painful.

---

## Phase A — Domain foundation + roles  *(1 migration)*

- **A1** `EventType` enum (Wedding, BarMitzvah, BatMitzvah, CorporateEvent, Birthday,
  PrivateEvent, Other) — **new** in Core.
- **A2** `ServiceType` enum (Catering, TableDesign, BridalChair, Photography, DJ, Flowers,
  Lighting, Other) — **new** in Core. Extend `ExtraService` — **mod**: `ServiceType`,
  `ImageUrl`, `IsActive`, `OwnerUserId` (Manager who owns it) + Fluent config + `User` 1→*
  `ExtraService`.
- **A3** `Booking.EventType` `string` → `EventType` enum — **mod**. Add `BookingStatus.Draft`.
- **A4** `UserRole`: add `Admin`; rename `Client` → `Customer` (decision 1). Update JWT, seed,
  demo users (+ an Admin demo user), tests.
- **A5** `Venue.OwnerUserId` (decision 3) — **mod** + config.
- **A6** Migration `V2Foundation` + seed update (service types on the 4 seeded services, Admin
  user note). `dotnet ef database update`.
- **DoD:** build green, tests green (existing concurrency test unchanged), DB updated.

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
