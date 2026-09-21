# Governance Period / Constitution Domain: State Transitions, Validation, and Endpoint Contracts

**Reviewed**: 2026-09-21. Covers `GovernanceController.cs`,
`AdminGovernanceController.cs`, `GovernanceService.cs`,
`IGovernanceService.cs`, `GovernanceDto.cs`, `Constitution.cs`,
`ECPeriod.cs`, `ECMember.cs`. Part of [../tasks.md](../tasks.md)
T006/T008/T011, following the [events-domain.md](./events-domain.md)
template. Static election documents (regulations, ballot/counting
certificates, forms) are out of scope per `spec.md`'s "Decision: election
ballot workflow (84.6)" — only persisted EC-period and constitution-amendment
voting records are covered here; the online election engine itself is
unbuilt follow-on work (see [../tasks.md](./../tasks.md) T018).

## 1. State transitions

No dedicated status enum exists for either EC periods or constitution
versions — both use a plain `bool IsActive` flag (`ECPeriod.cs:11`,
`Constitution.cs:14`). Constitution votes (`AmendmentVote`) have no status
field at all; a vote row's mere existence is the state.

**ECPeriod (`IsActive`: false/true)**

| From | To | Trigger | Who | Where | Side effects |
|---|---|---|---|---|---|
| (none) | `IsActive=false` | Create period | Admin (`AdminOnly`) | `POST /api/admin/governance/periods` → `GovernanceService.CreatePeriodAsync` (`GovernanceService.cs:53`) | Row created inactive; rejected if dates overlap an existing period (`EnsureNoOverlapAsync`) |
| `IsActive=false` | `IsActive=true` | Activate period | Admin | `POST /api/admin/governance/periods/{id}/activate` → `ActivatePeriodAsync` (`GovernanceService.cs:121`) | All other active periods flipped to `false` first (`ActivatePeriodInternalAsync:140`); only allowed if today falls within the period's date range, else throws `InvalidOperationException` |
| `IsActive=false` | `IsActive=true` | Update with `IsActive=true` | Admin | `PUT /api/admin/governance/periods/{id}` → `UpdatePeriodAsync` (`GovernanceService.cs:73`) | Same date-range guard, delegates to `ActivatePeriodInternalAsync` |
| `IsActive=true` | `IsActive=false` | Update with `IsActive=false` | Admin | Same endpoint | Direct flip, no date check on deactivation |
| (any) | (any) | Update without changing `IsActive` | Admin | Same endpoint | Title/dates updated in place, `IsActive` unchanged |

**ECMember (committee assignment, no enum — lifecycle via dates/flags)**

| From | To | Trigger | Who | Where | Side effects |
|---|---|---|---|---|---|
| (none) | active (`EndDate=null`) | Assign member to role | Admin | `POST /api/admin/governance/periods/{id}/members` → `AssignMemberToRoleAsync` (`GovernanceService.cs:162`) | Requires member `Status == MembershipStatus.Active` or throws; optional notification if `NotifyMember=true` |
| active | ended (`EndDate` set) | Remove from committee | Admin | `DELETE /api/admin/governance/members/{ecMemberId}` → `RemoveMemberFromCommitteeAsync` (`GovernanceService.cs:196`) | Soft end-date only, row retained; optional notification |
| active or ended | archived (`IsArchived=true`) | Hard delete | Admin + `RequireStepUp` filter | `DELETE /api/admin/governance/members/{ecMemberId}/hard-delete` → `DeleteECMemberAsync` (`GovernanceService.cs:218`) | Class A soft-delete (per ARCHITECTURE.md §4): sets `DeletedAt`/`DeletedByAdminId`, refuses (401) if admin id can't be resolved from claims; no-op if already archived |

**Constitution (`IsActive`: false/true)**

| From | To | Trigger | Who | Where | Side effects |
|---|---|---|---|---|---|
| (none) | `IsActive=false` | Create new version | — | `GovernanceService.CreateConstitutionVersionAsync` (`GovernanceService.cs:263`) | Unreachable via API — no controller action in `GovernanceController` or `AdminGovernanceController` calls this; only found via direct service/interface reference and `GovernanceServiceTests.cs` |
| `IsActive=false` | `IsActive=true` | Activate version | — | `GovernanceService.ActivateConstitutionAsync` (`GovernanceService.cs:279`) | Unreachable via API — same as above; would flip all other active constitutions to inactive with `SupersededDate` set |
| (n/a — vote, no status field) | vote row created | Vote on active amendment | Authenticated member with claim `sub`/member id | `POST /api/governance/constitution/{id}/vote` → `VoteOnConstitutionAsync` (`GovernanceService.cs:298`) | Rejected (returns `false` → 400) if: constitution not found or `IsActive=false`; member is not `MembershipType.Founding/Executive/General` (Article III §K voting tiers); member already voted for that constitution id. No update path for a cast vote (immutable) |

**Gaps carried forward, not resolved here:**
- There is no way, through either controller, to create or activate a
  constitution version — the only admin-facing constitution route is the
  public/anonymous read-only trio (`GetCurrentConstitution`,
  `GetConstitutionHistory`) plus the member vote endpoint.
  `CreateConstitutionVersionAsync`/`ActivateConstitutionAsync` are dead code
  from the API's perspective (called only in `GovernanceServiceTests.cs`).
- `VoteOnAmendment` accepts `isFor` as a raw `[FromBody] bool` and `comments`
  as `[FromQuery] string?`, not a wrapped DTO — flagged in §2 below.
- No `Rejected`/`Withdrawn` state exists for a vote; a member's vote, once
  cast, cannot be changed or deleted through any code path found.

## 2. Validation matrix

| DTO/Input | Field | Rule | Enforced by |
|---|---|---|---|
| `VoteOnAmendment` body | `isFor` (`bool`, `[FromBody]`) | None beyond required-bool binding | No wrapper DTO exists; **flagged: no validator/DataAnnotation, inline model binding only** |
| `VoteOnAmendment` query | `comments` (`string?`) | None — free text, unbounded length | **Flagged: no enforced rule found** |
| `VoteOnAmendment` | eligibility (constitution active, voting-tier membership, no duplicate vote) | Checked | Inline service logic (`GovernanceService.cs:301-312`), not a validator class |
| `CreatePeriodRequest` | `Title` | `= null!` — no `[Required]`, no length limit | **Flagged: no enforced rule found** |
| `CreatePeriodRequest` | `StartDate`, `EndDate` | No range/type-annotation validation; overlap checked only in the service (`EnsureNoOverlapAsync`, `GovernanceService.cs:109`) | Service-level check, not a DataAnnotation or FluentValidation validator |
| `UpdatePeriodRequest` | `Title`, `StartDate`, `EndDate` | Same as `CreatePeriodRequest` | **Flagged: no enforced rule found** |
| `UpdatePeriodRequest` | `IsActive=true` transition | Only allowed if today is within `[StartDate, EndDate]` | Inline service check (`GovernanceService.cs:87-94`), throws `InvalidOperationException` (unhandled by controller — surfaces as 500, not mapped to 400) |
| `AssignMemberRequest` | `MemberId` | Must reference a member with `Status == MembershipStatus.Active` | Inline service check (`GovernanceService.cs:165-168`), throws `InvalidOperationException` on failure |
| `AssignMemberRequest` | `Position` (`int`, cast to `ECPosition`) | No enum-range validation — an out-of-range int silently casts to an undefined `ECPosition` value | **Flagged: no enforced rule found** |
| `AssignMemberRequest` | `Reason` | Optional, no length limit | **Flagged: no enforced rule found** |
| `AssignMemberRequest` | `NotifyMember` | Defaults `false`; bool, no validation needed | n/a |
| `RemoveMember`/`DeleteECMember` query | `notifyMember` (`bool`) | None needed | n/a |

No `IValidatableObject`, FluentValidation validator, or `[Required]`/`[Range]`
DataAnnotation was found anywhere in this domain's request classes
(`CreatePeriodRequest`, `UpdatePeriodRequest`, `AssignMemberRequest` are plain
POCOs defined inline in `AdminGovernanceController.cs:93-114`). All
enforcement is either a service-layer `InvalidOperationException` throw
(which the controllers do not catch — likely surfacing as unhandled 500s
rather than 400s, not verified further) or absent entirely.

## 3. Endpoint contract table

| Route | Method | Request DTO | Response | Success | Documented failures | Auth |
|---|---|---|---|---|---|---|
| `/api/governance/ec/current`, `/api/governance/current` | GET | — | `{ Period: ECPeriodDto, Members: ECMemberDto[] }` | 200 | 404 if no active period | Anonymous (public cache) |
| `/api/governance/ec/history` | GET | — | `ECPeriodDto[]` | 200 | — | Anonymous (public cache) |
| `/api/governance/constitution` | GET | — | `Constitution` (domain entity, not a DTO) | 200 | 404 if none active | Anonymous (public cache) |
| `/api/governance/constitution/history` | GET | — | `Constitution[]` | 200 | — | Anonymous (public cache) |
| `/api/governance/constitution/{id}/vote` | POST | `bool isFor` (body) + `comments` (query) | `{ Message }` / `ProblemDetails` | 200 | 401 (no/invalid member id claim), 400 (inactive constitution, non-voting membership tier, or duplicate vote — all collapsed into one message) | Authenticated (any logged-in user with a member id claim) |
| `/api/admin/governance/periods` | GET | — | `ECPeriodDto[]` | 200 | — | AdminOnly |
| `/api/admin/governance/periods` | POST | `CreatePeriodRequest` | `ECPeriodDto` | 200 | unhandled `InvalidOperationException` on date overlap (not caught — likely 500, global error middleware not read in this pass) | AdminOnly |
| `/api/admin/governance/periods/{id}` | PUT | `UpdatePeriodRequest` | `{ Message }` | 200 | 404 if period not found; unhandled exception on overlap/date-range violation | AdminOnly |
| `/api/admin/governance/periods/{id}/activate` | POST | — | `{ Message }` | 200 | 404 if not found; unhandled exception if period doesn't cover today | AdminOnly |
| `/api/admin/governance/periods/{id}/members` | GET | — | `ECMemberDto[]` | 200 | — | AdminOnly |
| `/api/admin/governance/periods/{id}/members` | POST | `AssignMemberRequest` | `{ Message }` | 200 | 400 (`Problem`) on assignment failure (inactive member or unhandled service exception mapped generically) | AdminOnly |
| `/api/admin/governance/members/{ecMemberId}` | DELETE | `notifyMember` (query) | `{ Message }` | 200 | 404 if not found | AdminOnly |
| `/api/admin/governance/members/{ecMemberId}/hard-delete` | DELETE | `notifyMember` (query) | `{ Message }` | 200 | 401 (admin id unresolvable from claims), 404 if not found/already archived | AdminOnly + `[RequireStepUp]` (step-up re-auth filter) |

Pagination: none of these routes accept `cursor`/`page`/`pageSize` — every
list (periods, committee members, constitution history) returns a full
array. This domain is not on the offset/cursor split documented in
`001-platform-baseline/contracts/api-cross-layer.md`.

**Not read in this pass:** global exception-handling middleware behavior for
uncaught `InvalidOperationException` (affects whether overlap/date-range
violations surface as 400 or 500); `RequireStepUp` filter internals;
`Constants.Policies.AdminOnly` policy definition.
