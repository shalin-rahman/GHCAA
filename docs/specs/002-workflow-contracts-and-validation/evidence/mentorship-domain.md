# Mentorship Request Domain: State Transitions, Validation, and Endpoint Contracts

**Reviewed**: 2026-09-21. Covers `MentorshipController.cs`,
`MentorshipService.cs`, `MentorshipRequest.cs` (domain model +
`MentorshipStatus` enum). Part of [../tasks.md](../tasks.md) T002/T008/T011,
following the [events-domain.md](./events-domain.md) template.

## 1. State transitions

`MentorshipStatus` (`GHCAA.Domain/Models/MentorshipRequest.cs:34`): `Pending`,
`Accepted`, `Declined`, `Completed`.

| From | To | Trigger | Who | Where | Side effects |
|---|---|---|---|---|---|
| (none) | `Pending` | Send mentorship request | Authenticated member (not the target mentor) | `POST /api/mentorship` → `MentorshipService.SendRequestAsync` (`MentorshipService.cs:27`) | Row created; notification sent to mentor (`NotificationType.GeneralSystem`) |
| `Pending` | `Accepted` | Mentor accepts | Mentor only (`request.MentorId == mentorId` check, `MentorshipService.cs:105`) | `POST /api/mentorship/{id}/respond` (`Accept=true`) → `RespondAsync` | `RespondedAt` set, `ResponseNote` set; "accepted" notification sent to requester |
| `Pending` | `Declined` | Mentor declines | Mentor only | Same endpoint (`Accept=false`) | Same as above, "declined" notification |
| `Accepted` | `Completed` | Either party marks complete | Requester or mentor (`RequesterId == memberId \|\| MentorId == memberId`, `MentorshipService.cs:128`) | `POST /api/mentorship/{id}/complete` → `MarkCompleteAsync` | `RespondedAt` overwritten with completion time; no separate `CompletedAt` field exists |
| `Declined` | (terminal) | — | — | — | No code path re-activates a declined request |
| `Completed` | (terminal) | — | — | — | No code path reopens a completed request |
| `Pending` | `Pending` (duplicate blocked) | Second send attempt to same mentor while one is `Pending` | — | `SendRequestAsync` throws `InvalidOperationException` → controller returns 409 | No new row; existing `Pending` request untouched |

**Gaps carried forward, not resolved here:**
- `RespondAsync` is not restricted to `Status == Pending` — it will silently
  flip an already-`Accepted`, `Declined`, or `Completed` request back to
  `Accepted`/`Declined` if called again (no status guard in the query at
  `MentorshipService.cs:104-105`, only `Id` + `MentorId` are matched). This
  looks like an unguarded transition rather than an intentional one, and is
  worth checking before treating it as by design.
- `MarkCompleteAsync` correctly guards on `Status == Accepted`
  (`MentorshipService.cs:129`), so `Pending → Completed` and
  `Declined → Completed` are not reachable.
- No `Declined → Accepted` or any reopen transition exists anywhere in
  `MentorshipService`.

## 2. Validation matrix

| DTO | Field | Rule | Enforced by |
|---|---|---|---|
| `SendMentorshipRequestDto` | `MentorId` | Must not equal caller's own member id | Controller inline check (`MentorshipController.cs:37`) |
| `SendMentorshipRequestDto` | `MentorId` | Must be a valid/existing member id | **No enforced rule found** — no existence check before insert in `SendRequestAsync`; relies on an FK constraint at save time (unverified — not read in this pass whether the DB throws or EF navigates silently) |
| `SendMentorshipRequestDto` | `Message` | none (`[MaxLength(500)]` is on the `MentorshipRequest` domain model, not the DTO) | **No enforced rule found on the DTO** |
| `SendMentorshipRequestDto` | `Domain` | none (`[MaxLength(200)]` is on the domain model, not the DTO) | **No enforced rule found on the DTO** |
| `SendMentorshipRequestDto` | duplicate pending request | One `Pending` request per (`RequesterId`, `MentorId`) pair | Service-level check (`MentorshipService.cs:30-35`), not a validator or DB constraint |
| `RespondMentorshipDto` | `Accept` | none (plain bool) | N/A |
| `RespondMentorshipDto` | `Note` | none (`[MaxLength(500)]` again lives on the domain model as `ResponseNote`, not the DTO) | **No enforced rule found on the DTO** |
| `RespondMentorshipDto` | caller must be the request's mentor | Enforced | Service-level check (`MentorshipService.cs:105`, filters query by `MentorId == mentorId`) — returns `false`/404 if not matched, not a 403 |
| MarkComplete (no DTO, route param only) | caller must be requester or mentor, and status must be `Accepted` | Service-level check (`MentorshipService.cs:127-129`) |

No FluentValidation validator classes or `IValidatableObject` implementations
exist for either mentorship DTO. All enforcement is either a
`[Required]`/`[MaxLength]` annotation on the **domain model** (which never
runs against the DTO the controller binds) or an inline service/controller
check.

## 3. Endpoint contract table

| Route | Method | Request DTO | Response | Success | Documented failures | Auth |
|---|---|---|---|---|---|---|
| `/api/mentorship` | POST | `SendMentorshipRequestDto` | `{ Id, Status }` | 200 | 401 (no member id claim), 400 (`MentorId` == self), 409 (duplicate pending request, via `Problem()`) | Authenticated (`[Authorize]` class-level) |
| `/api/mentorship/sent` | GET | — | array: `Id, Domain, Message, Status, RequestedAt, RespondedAt, ResponseNote, Mentor{Id, FullName, PhotoPath, MembershipNumber}` | 200 | 401 | Authenticated |
| `/api/mentorship/received` | GET | — | same shape, `Requester{...}` instead of `Mentor{...}` | 200 | 401 | Authenticated |
| `/api/mentorship/{id}/respond` | POST | `RespondMentorshipDto` | empty `200 OK` | 200 | 401, 404 (request not found or caller isn't the mentor — same code for both cases) | Authenticated |
| `/api/mentorship/{id}/complete` | POST | — | empty `200 OK` | 200 | 401, 404 (not found, caller not a party, or status != `Accepted` — same code for all three) | Authenticated |
| `/api/mentorship/admin/all` | GET | — | array: `Id, Domain, Message, Status, RequestedAt, Requester{Id, FullName}?, Mentor{Id, FullName}?` | 200 | not read in this pass | `Constants.Policies.AdminOnly` |

Pagination: none of these routes accept `page`/`pageSize` or `cursor` — all
list endpoints (`sent`, `received`, `admin/all`) return a full unpaginated
array via `.ToListAsync()`. Not on the offset/cursor split in
`001-platform-baseline/contracts/api-cross-layer.md`.

**Not read in this pass:** `IMentorshipService.cs` interface signatures
(inferred from the implementation), `MentorshipServiceTests.cs`,
`MentorshipRequestConfiguration.cs` (EF config/FK constraint behavior,
flagged unverified above), `Constants.Policies.AdminOnly` definition.
