# EventBooking — Functional Specification (v2)

> Source: `EventBooking Functional Specification.pdf`, provided by the student 2026-09-08.
> This is the authoritative feature list for the v2 upgrade. Ordered execution plan lives in
> `WORK_PLAN_V2.md`; progress is tracked in `PROJECT_LOG.md`.

## Vision

Evolve the project from **"an event hall booking system"** into
**"a complete event planning and booking platform"** — the customer builds an entire event in one
place: venue + catering + table design + bridal chair (when relevant) + additional services +
server-side price calculation + booking + messaging + notifications + promotions.

## IMPORTANT — read before coding

The project already exists (`EventBooking-Server`, `EventBooking-Client`). **Do not rebuild from
scratch.** Analyse the existing architecture first and **preserve**: architecture, entities,
controllers, services, DTOs, authentication, authorization, DB structure where possible, existing
booking logic, and the **existing HallSlot/Booking concurrency mechanism**. Extend, don't fork.

Before major changes, briefly explain: (1) what currently exists, (2) what needs to change,
(3) which new entities/features are needed, (4) why each change is necessary.

## Phase 1 — functionality first

Focus on backend functionality, database, business logic, API, and only the React
pages/components needed to make features work. **No visual redesign, no advanced CSS/animations**
— a separate UI/UX phase comes later. Keep the UI functional and simple.

---

## 1. Event Type

Add an event type to an event/booking. Types: **Wedding, Bar Mitzvah, Bat Mitzvah, Corporate
Event, Birthday, Private Event, Other**. The event type determines which services are relevant
(e.g. a Wedding can have Bridal Chair, Table Design, Catering, Photography, DJ, Flowers). Design
must be extensible for future types.

## 2. Upgrade ExtraService

Reuse the existing `ExtraService` — do not create duplicate entities. Add a service
type/category: **Catering, TableDesign, BridalChair, Photography, DJ, Flowers, Lighting, Other**.
`ExtraService` should carry: `Id`, `Name`, `Description`, `Price`, `ServiceType`, `ImageUrl`,
`IsActive`, `ManagerId`/`OwnerId` (per the existing architecture). Goal: services are extensible.

## 3. Bridal Chair

For Wedding events, the customer can select a Bridal Chair service and see: availability, style,
description, price, image. Implement via `ExtraService` with `ServiceType = BridalChair` — no
separate entity unless there is a strong architectural reason.

## 4. Table Design

Add Table Design as a service (style, colours, tablecloth, napkins, centrepieces, flowers,
tableware, custom design, price). Also `ExtraService` with `ServiceType = TableDesign`.

## 5. Catering

Catering has more business logic than a normal extra service. Create a `CateringMenu` entity if
appropriate: `Id`, `Name`, `Description`, `PricePerGuest`, `IsVegetarian`, `IsVegan`,
`IncludesDrinks`, `IsActive`, `ManagerId`. Price depends on guest count
(e.g. 300 guests × ₪180 = ₪54,000). **The final calculation must happen on the server** — never
trust a total sent by the client.

## 6. Event Builder

A step-by-step flow for the customer: event type → date → number of guests → venue → catering →
table design → bridal chair (if relevant) → additional services → price summary → booking
confirmation. The customer reviews all selected services before confirming.

## 7. Price Calculation

Proper **server-side** price calculation from prices stored in the DB. Example:
venue ₪8,000 + catering 300×180 ₪54,000 + table design ₪2,500 + bridal chair ₪1,000 + DJ ₪3,500
= **₪69,000**. The client must not be trusted to provide the final price.

## 8. Messaging System

Messages between customers and managers. `Message` entity: `Id`, `SenderId`, `ReceiverId`,
`BookingId`/`EventId` where appropriate, `Content`, `CreatedAt`, `IsRead`. Managers see unread
messages; customers see replies.

## 9. Notifications

Notification system. Types: new message, booking approved, booking rejected, booking cancelled,
new promotion, booking update. Users see an unread notification count.

## 10. Promotions

`Promotion` entity: `Id`, `Title`, `Description`, `DiscountPercentage` or `DiscountAmount`,
`StartDate`, `EndDate`, `ImageUrl`, `IsActive`, `ManagerId`. Managers create/manage their own.
Only active promotions within their valid date range are displayed.

## 11. Promotion Popup / Modal

React support for showing an active promotion / important announcement in a modal the user can
close. Keep it simple for now (visual redesign later).

## 12. Customer Dashboard

Customer sees: My Events, My Bookings, Selected Services, Price Estimates, Messages,
Notifications, Promotions.

## 13. Manager Dashboard

Manager sees: new bookings, existing bookings, customers, messages, services they offer,
promotions, availability, basic activity statistics. **A manager can only manage resources that
belong to them.**

## 14. Search and Filtering

Improve venue search. Customers filter by: event type, number of guests, price, date,
area/location, services, availability. Filtering done properly on the backend where appropriate.

## 15. Reviews

Review/rating system: `Rating` 1–5, `ReviewText`. A customer can review a venue/service only
**after a relevant booking or completed event** — no fake reviews without a booking.

## 16. Authorization

Roles: **Customer, Manager, Admin**.
- Customer: manage own events/bookings, send messages, view relevant services/promotions.
- Manager: manage own venues/services, own promotions, view relevant customer messages/bookings.
- Admin: manage the entire system.
Enforced **server-side**, not only in React.

## 17. React Changes

Only the React changes needed to support the new functionality: pages, components, API services,
React Query queries/mutations, routes, forms, data models/types. **No visual redesign yet** —
functional and simple.

## 18. Database Design

Inspect the existing model first. Avoid duplicate entities/relationships; reuse where reasonable.
Potential entities: User, Event, Venue, Hall, HallSlot, Booking, ExtraService,
BookingExtraService, CateringMenu, Message, Notification, Promotion, Review. Adapt to the
existing architecture rather than blindly creating all of them.

## 19. API

Endpoints for the new areas: Events, Bookings, Services, Catering, Messages, Notifications,
Promotions, Reviews. Follow the existing architecture — DTOs, validation, correct HTTP status
codes, authorization, error handling, existing Service/Repository patterns. No unnecessary
patterns.

## 20. IMPORTANT — preserve existing concurrency logic

Do **not** remove or weaken the HallSlot booking-conflict mechanism. Preserve transactions,
concurrency/version checking, and HTTP 409 for booking conflicts. Two users booking the same slot
must still be handled correctly.

## 21. Testing

After the changes: build + run the backend, run the React client, verify the DB, test the new
endpoints, authorization, price calculation, booking conflicts, messaging, promotions, and that
existing functionality still works. Create/update migrations as needed.

## 22. Educational requirement

Study project — code must be understandable and explainable to a teacher. Prefer clear naming,
simple architecture, separation of responsibilities, meaningful DTOs, reasonable validation,
minimal but useful comments. No complexity just to look advanced.

## 23. Final report

When finished, a clear summary: backend (new/modified entities, controllers, services, DTOs,
endpoints, DB relationships, migrations), frontend (new pages, components, API services, queries,
routes), and for each major feature — what problem it solves, how data flows, which backend and
frontend components participate. Simple enough for a software-engineering student to understand.
