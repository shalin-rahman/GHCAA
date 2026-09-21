# Events Domain: State Transitions, Validation, and Endpoint Contracts

**Reviewed**: 2026-09-21. Covers `EventsController`, `EventService`,
`EventDto.cs`. Delivered as the first complete domain for
[../tasks.md](../tasks.md) T002/T008/T011, as a template for the remaining
domains listed in `spec.md` Stories 1–3.

## 1. Registration state transitions

`EventRegistrationStatus` (`GHCAA.Domain/Enums.cs:30`): `Pending`, `Approved`,
`Rejected`, `Waitlisted`.

| From | To | Trigger | Who | Where | Side effects |
|---|---|---|---|---|---|
| (none) | `Pending` | Register for event | Member or guest (anonymous) | `POST /api/events/register` → `EventService.RegisterForEventAsync` | Registration row created |
| (none) | `Waitlisted` | Register for event when `ParticipantLimit` is reached and `HasWaitlist` is true | Member or guest (anonymous) | Same endpoint, same method (`EventService.cs:294`) | Registration row created directly in `Waitlisted`, bypassing `Pending` |
| `Pending` | `Approved` | Approve registration | Admin (`AdminOnly` policy) | `POST /api/events/admin/approve-registration` → `EventService.cs:503` | `ApprovedAt` set; if `RequiresPayment`, `FinancialService.cs:245-252` also flips a matching `Pending` registration to `Approved` on payment confirmation |
| `Pending` | `Rejected` | Reject registration | Admin (`AdminOnly` policy) | Same endpoint | No further side effect recorded in `EventService` |
| `Waitlisted` | `Approved` | A slot frees up | Admin, via the check-in/QR flow (`EventService.cs:796`) | `POST /api/events/admin/checkin/qr` | Only reachable once `ParticipantLimit` has capacity again; not exposed as a distinct "promote from waitlist" action |
| `Approved` | (terminal) | Check-in / QR scan | Admin | `EventService.cs:448,746,770` gate on `Status == Approved` before allowing check-in actions | No status change on check-in itself — `Approved` stays `Approved` |
| `Rejected` | (terminal) | — | — | — | No code path re-activates a rejected registration |

**Gaps carried forward, not resolved here:**
- There is no explicit `Waitlisted → Rejected` transition in `EventService`; a
  waitlisted registration that never gets a freed slot has no code path that
  marks it `Rejected`. Unverified whether this is intended or an omission.
- `PaymentCallbackOrchestrator.cs:92` also reads a `Pending` registration
  during a gateway callback, but does not itself change `Status` — it is
  `FinancialService.cs:252` that performs the `Pending → Approved` move after
  a manual/gateway payment is confirmed.

## 2. Validation matrix

| DTO | Field | Rule | Enforced by |
|---|---|---|---|
| `CreateEventDto` | `Title`, `Description`, `StartDate`, `EndDate`, `Location` | Required | `[Required]` data annotation |
| `CreateEventDto` | `EndDate` | Must be after `StartDate` | `IValidatableObject.Validate` (`EventDto.cs:72`) |
| `CreateEventDto` | `RegistrationEndDate` | Must be after `RegistrationStartDate` when both set | `IValidatableObject.Validate` (`EventDto.cs:78`) |
| `CreateEventDto` | `RegistrationEndDate` | Must be on or before `StartDate` | `IValidatableObject.Validate` (`EventDto.cs:85`) |
| `CreateEventDto` | `RegistrationFee`, `ParticipantLimit` | No format/range rule found (no `[Range]`, no service-side floor/ceiling check) | **Flagged: no enforced rule** |
| `UpdateEventDto` | all `CreateEventDto` fields | Same as above (inherits) | Inherited `IValidatableObject` |
| `RegisterForEventDto` | `GuestName`, `GuestEmail` | Required only when `IsNonMember` is true or no authenticated member is present, checked as `isGuestFullfilled` | Controller inline check (`EventsController.cs:86-91`), not a validator class |
| `RegisterForEventDto` | `GuestEmail` | No email-format validation found on this DTO | **Flagged: no enforced rule** |
| `RegisterForEventDto` | receipt file (`IFormFile`) | Category/size validated (`FileCategory.Document`, 10 MB max) | `FileValidationService.ValidateFormFile` (`EventsController.cs:96`) |
| `RegisterForEventDto` | duplicate registration | A member/guest cannot register twice for the same event unless their prior registration is `Rejected` | `EventService.cs:252-256` (service-level check, not a validator or DB constraint) |
| `ApproveRegistrationDto` | `RegistrationId` | No existence check documented at the DTO level; existence is presumably checked in the service before use | **Unverified — not read in this pass** |

## 3. Endpoint contract table

| Route | Method | Request DTO | Response | Success | Documented failures | Auth |
|---|---|---|---|---|---|---|
| `/api/events` | GET | — | `EventDto[]` | 200 | — | Anonymous |
| `/api/events/{id}/participants` | GET | — | not read in this pass | 200 | — | Anonymous |
| `/api/events/{id}` | GET | — | `EventDto` | 200 | — | Anonymous |
| `/api/events/register` (multipart) | POST | `RegisterForEventDto` + `IFormFile? receipt` | registration result object | 200 | 401 (no member/guest info), 400 (bad receipt file) | Anonymous |
| `/api/events/register` (json) | POST | `RegisterForEventDto` | registration result object | 200 | 401 | Anonymous |
| `/api/events/my-registrations` | GET | — | `List<object>` (registrations) | 200 | 400 (no member account and not SuperAdmin) | Authenticated |
| `/api/events/registration/{id}` | GET | — | registration detail | 200 | not read in this pass | Authenticated |
| `/api/events/admin/all` | GET | — | `EventDto[]` | 200 | — | AdminOnly |
| `/api/events/admin` | POST | `CreateEventDto` | `EventDto` | not read in this pass | 400 on `IValidatableObject` failure | AdminOnly |
| `/api/events/admin/{id}` | PUT | `UpdateEventDto` | `EventDto` | not read in this pass | 400 on validation failure | AdminOnly |
| `/api/events/admin/{id}` | DELETE | — | not read in this pass | not read in this pass | — | AdminOnly |
| `/api/events/admin/{id}/logo` | POST | file upload | not read in this pass | not read in this pass | — | AdminOnly |
| `/api/events/admin/registrations` | GET | — | not read in this pass | not read in this pass | — | AdminOnly |
| `/api/events/admin/approve-registration` | POST | `ApproveRegistrationDto` | not read in this pass | not read in this pass | — | AdminOnly |
| `/api/events/admin/registrations/{id}/send-invitation` | POST | — | not read in this pass | not read in this pass | — | AdminOnly |
| `/api/events/admin/checkin/qr` | POST | not read in this pass | not read in this pass | not read in this pass | — | AdminOnly |
| `/api/events/admin/{eventId}/tasks` | GET | — | not read in this pass | not read in this pass | — | AdminOnly |
| `/api/events/admin/tasks` | POST | not read in this pass | not read in this pass | not read in this pass | — | AdminOnly |
| `/api/events/admin/tasks/{id}/toggle` | POST | — | not read in this pass | not read in this pass | — | AdminOnly |
| `/api/events/admin/tasks/{id}` | DELETE | — | not read in this pass | not read in this pass | — | AdminOnly |
| `/api/events/admin/{eventId}/budget` | GET | — | not read in this pass | not read in this pass | — | AdminOnly |
| `/api/events/admin/budget` | POST | not read in this pass | not read in this pass | not read in this pass | — | AdminOnly |
| `/api/events/admin/expenses` | POST | not read in this pass | not read in this pass | not read in this pass | — | AdminOnly |
| `/api/events/admin/expenses/{id}` | DELETE | — | not read in this pass | not read in this pass | — | AdminOnly |

Rows marked "not read in this pass" are routes confirmed to exist (from
`EventsController`'s attributes) whose bodies were not opened, to keep this
pass inside the 10-minute budget. They are gaps in this file, not gaps in the
system — filling them is direct continuation work, not new investigation.

Pagination: none of these routes accept a `cursor` or `page`/`pageSize`
parameter — the events list and admin list both return a full array. This
domain is not on the offset/cursor split documented in
`001-platform-baseline/contracts/api-cross-layer.md`.
