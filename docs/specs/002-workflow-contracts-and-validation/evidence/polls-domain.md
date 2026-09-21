# Polls Domain: State Transitions, Validation, and Endpoint Contracts

**Reviewed**: 2026-09-21. Covers `PollController.cs`, `AdminPollController.cs`,
`PollService.cs`, `PollDtos.cs`, `GHCAA.Domain/Models/Poll.cs`. No
FluentValidation files found for polls. Part of [../tasks.md](../tasks.md)
T002/T008/T011, following the [events-domain.md](./events-domain.md)
template.

## 1. State transitions

There is no discrete Poll status enum (checked `GHCAA.Domain/Enums.cs` — no
`Poll*Status` type exists). Status is two independent booleans on `Poll`
(`GHCAA.Domain/Models/Poll.cs:12-13`): `IsActive` (default `true`) and
`IsArchived` (default `false`), plus a derived open/closed read from
`ExpiryDate`. Treating `(IsActive, IsArchived)` as the state:

| From | To | Trigger | Who | Where | Side effects |
|---|---|---|---|---|---|
| (none) | `IsActive=true, IsArchived=false` | Create poll | Admin (`AdminOnly` policy) | `POST /api/admin/polls` → `PollService.CreatePollAsync` (`PollService.cs:60`) | Poll + `PollOption` rows created; no validation of `Options` list beyond what's on `CreatePollDto` |
| `IsActive=true` | `IsActive=false` | Toggle status off | Admin | `PUT /api/admin/polls/{id}/toggle` → `PollService.TogglePollStatusAsync` (`PollService.cs:142`) | In-place flag flip; no vote/side effect |
| `IsActive=false` | `IsActive=true` | Toggle status on | Admin | Same endpoint, `isActive=true` body | Re-activates poll even past `ExpiryDate` — `TogglePollStatusAsync` never checks `ExpiryDate`; a re-activated expired poll still fails votes at `PollService.cs:105` |
| `IsArchived=false` | `IsArchived=true` | Delete poll | Admin | `DELETE /api/admin/polls/{id}` → `PollService.DeletePollAsync` (`PollService.cs:152`) | Soft delete only (`IsArchived=true`); row never removed. Terminal — no unarchive endpoint exists in either controller |
| (implicit) open → closed | via `ExpiryDate` | Time passes | — | Read-time check only: `GetActivePollsAsync` filters `ExpiryDate == null OR ExpiryDate > UtcNow` (`PollService.cs:31`); `VoteAsync` re-checks `poll.ExpiryDate < UtcNow` (`PollService.cs:105`) | No scheduled job flips `IsActive`; expiry is enforced only at query/vote time, never persisted as a status change |
| n/a | n/a | Member votes | Member (`[Authorize]` on `PollController`) | `POST /api/polls/{id}/vote` → `PollService.VoteAsync` (`PollService.cs:83`) | Not a Poll state transition — inserts `PollVote` row(s); poll itself is unchanged |

**Gaps carried forward, not resolved here:**
- No `Archived → Active` transition exists — whether archive is one-way by
  design or omission is unverified.
- No enum-backed lifecycle (draft/open/closed) — `IsActive`/`IsArchived`/
  `ExpiryDate` are three independent signals that can disagree (e.g.
  `IsActive=true` plus an expired `ExpiryDate` is a reachable,
  looks-open-but-is-closed state).
- `ToggleStatus` doesn't re-validate `ExpiryDate`, so admins can "reopen" an
  expired poll for listing purposes even though votes still get rejected —
  flagged, not confirmed intentional.

## 2. Validation matrix

| DTO | Field | Rule | Enforced by |
|---|---|---|---|
| `CreatePollDto` | `Title` | None found (`string = null!`, no `[Required]`) | **Flagged: no enforced rule found** |
| `CreatePollDto` | `Description` | Optional, no rule | n/a |
| `CreatePollDto` | `AllowMultipleChoice` | None (plain bool) | n/a |
| `CreatePollDto` | `ExpiryDate` | No range/future-date check found | **Flagged: no enforced rule found** |
| `CreatePollDto` | `Options` (`List<string>`) | No min-count check (e.g. "at least 2 options") found in DTO or `CreatePollAsync` — an empty list creates a poll with zero options | **Flagged: no enforced rule found** |
| `PollVoteDto` | `OptionIds` | Empty/null rejected | Service inline check, `PollService.cs:88` |
| `PollVoteDto` | `OptionIds` | Must all belong to the target poll | Service inline check, `PollService.cs:114-115` |
| `PollVoteDto` | `OptionIds` | Count must be 1 unless `Poll.AllowMultipleChoice` | Service inline check, `PollService.cs:111` |
| `PollVoteDto` | `OptionIds` | Member cannot vote twice on the same poll | Service inline check, `PollService.cs:108`, backed by a unique index on `(PollOptionId, MemberId)`; single-choice double-vote race is closed via `Serializable` isolation, not the check itself |
| `AdminPollController.ToggleStatus` body | `isActive` (bare `bool`, not a DTO) | None — any bool value accepted | No validator; not modeled as a DTO |

No `[Required]`/`[Range]` data annotations, no `IValidatableObject`, and no
FluentValidation validator class found for any Poll DTO — all enforcement
that exists is inline in `PollService`, none in the controllers themselves.

## 3. Endpoint contract table

| Route | Method | Request DTO | Response | Success | Documented failures | Auth |
|---|---|---|---|---|---|---|
| `/api/polls/active` | GET | — | `List<PollDto>` | 200 | 401 (no resolvable member id) | Authenticated (`[Authorize]` class-level, no policy) |
| `/api/polls/{id}` | GET | — | `PollDto` | 200 | 401 (no member id), 404 (poll not found or archived) | Authenticated |
| `/api/polls/{id}/vote` | POST | `PollVoteDto` | `{ Message }` | 200 | 401 (no member id), 400 (`Problem`, "Voting failed" — already voted / poll closed / invalid option / choice-count violation, all collapsed into one message) | Authenticated |
| `/api/admin/polls` | GET | — | `List<PollDto>` (memberId=0, so `HasVoted`/`SelectedOptionIds` always empty for the admin view) | 200 | — | `AdminOnly` policy |
| `/api/admin/polls` | POST | `CreatePollDto` | `{ Id, Message }` via `CreatedAtAction` | 201 | none — no validation branch exists, so a malformed body (empty title/options) still returns 201 | `AdminOnly` policy |
| `/api/admin/polls/{id}/toggle` | PUT | raw `bool` body | `{ Message }` | 200 | 404 (poll not found) | `AdminOnly` policy |
| `/api/admin/polls/{id}` | DELETE | — | `{ Message }` | 200 | 404 (poll not found) | `AdminOnly` policy |

Pagination: none of these routes accept `page`/`pageSize`/`cursor` — both
list endpoints return a full array. Not on the offset/cursor split in
`001-platform-baseline/contracts/api-cross-layer.md`.

**Not read in this pass:** `PollOption.cs` entity itself beyond its
`OptionText` field used in `MapToDto`. Test files
(`PollControllerTests.cs`, `AdminPollControllerTests.cs`,
`PollServiceTests.cs`) exist and would corroborate these findings but were
not read here — cross-check them before treating this as final.
