# Job Moderation Domain: State Transitions, Validation, and Endpoint Contracts

**Reviewed**: 2026-09-21. Covers `JobHubController.cs`, `JobHubService.cs`,
`JobDto.cs` (`CreateJobDto`/`JobDto`), `Enums.SubmissionStatus`
(`GHCAA.Domain/Enums.cs:38`). No FluentValidation validator class exists for
any Job DTO. Part of [../tasks.md](../tasks.md) T002/T008/T011, following the
[events-domain.md](./events-domain.md) template.

## 1. State transitions

`Enums.SubmissionStatus` (`GHCAA.Domain/Enums.cs:38`): `Draft`, `Pending`,
`Approved`, `Rejected`. `JobOpportunity` also has an independent `IsActive`
bool, not part of this enum, toggled separately.

| From | To | Trigger | Who | Where | Side effects |
|---|---|---|---|---|---|
| (none) | `Approved` | Post a job as Admin/SuperAdmin | Admin/SuperAdmin | `POST /api/jobs` → `JobHubService.PostJobAsync` (`JobHubService.cs:81-83`) | `IsActive=true`; if `NotifyMembers`, notifies the poster ("Job Posted") immediately — no admin approval step |
| (none) | `Pending` | Post a job as a regular member | Authenticated member (non-admin) | Same endpoint, same method | `IsActive=true`; no member notification; `IAdminNotificationService.NotifyPendingApprovalAsync` fires instead |
| `Pending` | `Approved` | Approve job | Admin (`AdminOnly` policy) | `POST /api/jobs/admin/{id}/approve` → `JobHubService.ApproveJobAsync` (`JobHubService.cs:175`) | Clears `RejectionReason`; if `notifyMember` (default true), notifies poster |
| `Pending` | `Rejected` | Reject job | Admin (`AdminOnly` policy) | `POST /api/jobs/admin/{id}/reject` → `JobHubService.RejectJobAsync` (`JobHubService.cs:198`) | Sets `RejectionReason`; forces `IsActive=false`; if `notifyMember`, notifies poster with reason |
| `Draft` | any | — | — | — | Unreachable: no code path sets or reads `Draft`; `PostJobAsync` only ever assigns `Approved` or `Pending` |
| `Approved` | `Rejected`/`Pending` | — | — | — | Unreachable via any flow, but not blocked: `ApproveJobAsync`/`RejectJobAsync` take any job by id with no current-status guard, so an admin call could re-reject an already-`Approved` job |
| `Rejected` | (terminal) | — | — | — | No code path re-activates a rejected job; `IsActive` stays false |
| any | `IsActive=false` (deactivate) | Deactivate/withdraw a job | Owner member or Admin | `DELETE /api/jobs/{id}` or `PATCH /api/jobs/deactivate/{id}` → `JobHubService.DeactivateJobAsync` (`JobHubService.cs:143`) | Sets `IsActive=false` only; `Status` enum unchanged — deactivation is orthogonal to moderation status |

**Gaps carried forward, not resolved here:**
- `ApproveJobAsync`/`RejectJobAsync` have no `Status != Pending` guard —
  whether this is an intended idempotent override or a missing precondition
  is unverified.
- `GetActiveJobsAsync` (public listing) filters `IsActive && Status ==
  Approved`; a `Pending` job posted by a non-admin is invisible publicly
  until approved.
- `UpdateJobAsync` (`PUT /api/jobs/{id}`) does not touch `Status` — editing a
  job by owner or admin never resets it to `Pending` for re-review.

## 2. Validation matrix

| DTO | Field | Rule | Enforced by |
|---|---|---|---|
| `CreateJobDto` | `Title`, `CompanyName`, `Location`, `Description`, `Requirements` | None found — no `[Required]`, no FluentValidation, no `IValidatableObject` | **Flagged: no enforced rule found** |
| `CreateJobDto` | `ApplicationEmail` | No email-format validation found | **Flagged: no enforced rule found** |
| `CreateJobDto` | `ApplicationLink` | No URL-format validation found | **Flagged: no enforced rule found** |
| `CreateJobDto` | `ApplicationDeadline` | No range/future-date check found | **Flagged: no enforced rule found** |
| `CreateJobDto` | `JobCategory` | Must bind to a valid `Enums.JobCategory` value | Implicit model-binder enum validation only |
| `CreateJobDto` | `NotifyMembers` | bool, defaults `true` | n/a |
| `RejectJobRequest` (inline class) | `Reason` | `= null!` but no `[Required]`/runtime enforcement seen | **Flagged: no enforced rule found** |
| `RejectJobRequest` | `NotifyMember` | bool, defaults `true` | n/a |
| PostJob/UpdateJob ownership | member id claim | `CurrentMemberIdRaw()` must parse to int, else 400 `Problem` | Controller inline check (`JobHubController.cs:36-40,50-54`) |
| UpdateJob/DeactivateJob authorization | poster or admin | `job.PostedByMemberId != memberId && !isAdmin` → 403 `Forbid` | Inline check in `UpdateJobAsync` (`JobHubService.cs:62`) and controller `DeactivateJob` (`JobHubController.cs:83-86`) |

## 3. Endpoint contract table

| Route | Method | Request DTO | Response | Success | Documented failures | Auth |
|---|---|---|---|---|---|---|
| `/api/jobs` | GET | — (query: `jobCategory`, `query`) | `IEnumerable<JobDto>` | 200 | — | Anonymous (public content output-cache policy) |
| `/api/jobs` | POST | `CreateJobDto` | `JobDto` | 200 | 400 (invalid/missing member-id claim) | Authenticated (any role; admin bypasses moderation) |
| `/api/jobs/{id}` | PUT | `CreateJobDto` | `{ Message }` or 403 | 200 | 400 (invalid session), 403 (not owner/admin) | Authenticated |
| `/api/jobs/{id}` | GET | — | `JobDto` or 404 | 200 | 404 | Anonymous (public content output-cache policy) |
| `/api/jobs/{id}` | DELETE | — | 200/500 | 200 | 404, 403 (not owner/admin) | Authenticated |
| `/api/jobs/deactivate/{id}` | PATCH | — (same handler as DELETE) | 200/500 | 200 | 404, 403 | Authenticated |
| `/api/jobs/admin/pending` | GET | — | `IEnumerable<JobDto>` | 200 | — | AdminOnly |
| `/api/jobs/admin/{id}/approve` | POST | — (query: `notifyMember`, default true) | `{ Message }` or 404 | 200 | 404 | AdminOnly |
| `/api/jobs/admin/{id}/reject` | POST | `RejectJobRequest` (`Reason`, `NotifyMember`) | `{ Message }` or 404 | 200 | 404 | AdminOnly |

Pagination: none of these routes accept `cursor`, `page`, or `pageSize` — all
list endpoints return a full array. Not on the offset/cursor split documented
in `001-platform-baseline/contracts/api-cross-layer.md`.

**Not read in this pass:** `IJobHubService.GetMemberJobsAsync` is implemented
but not exposed as a controller route (unused interface method, or wired
elsewhere — not checked). `Constants.Policies.AdminOnly` and
`Constants.OutputCachePolicies.PublicContent` definitions were not opened;
assumed consistent with the Events domain's usage.
