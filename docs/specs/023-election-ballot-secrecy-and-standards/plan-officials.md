# Spec 023 plan: election officials, personas and standards gaps

Status: approved 2026-09-28. All four decisions in §6 are answered. Build follows the order in §7.
As of 2026-09-30, 37.1w and 37.12a to 37.12c are done and 37.12d is in progress. `docs/TODO.md` records
each item's state and where the build departed from this plan.
Tracker items: 37.1w, 37.12a to 37.12j, 37.13a to 37.13l in `docs/TODO.md`.

This plan is written so the build can follow it step by step. Every file, field, endpoint and test is
named. If a step needs a choice that is not made here, it is listed under "Decisions" at the end and
the default shown there is what gets built.

## 1. What the user decided (2026-09-28)

1. The Search Committee (ECSC), Commission, officials, Observers and the Appeal Tribunal are all
   recorded in the app. They are appointed during or after election setup.
2. Any of these people can be a member or a non-member.
3. Once a Commission is in place, Admins lose election powers. SuperAdmin and the system keep full
   control.
4. Sensitive steps need two different people.
5. The Commission and officials are shown on a public board.
6. Non-members get a temporary election role with only the access their post needs. Members who are
   appointed get the same extra role. Closing the election removes it without anyone doing it by hand.
7. Personas (the posts) are configurable data, not code. So are the rules around them.
8. Eligibility checks such as "5 years a member" or "not also a candidate" are left to people. The app
   does not enforce them.
9. An appointment counts only after the person accepts it and signs the declaration.

## 2. What is there today

| Area | Today | File |
|---|---|---|
| Officers | `ElectionOfficer` (ElectionId, MemberId, Role). Role is a fixed enum of four posts. Member only. No accept step. | `GHCAA.Domain/Models/Election.cs:44-52`, `Enums.cs:62` |
| Officer use | Only `DecideNominationAsync` checks it. No UI on web or mobile. | `ElectionService.cs:71-78`, `:152-154` |
| Election powers | Every election write is `AdminOnly` plus step-up. Any admin can do anything. | `ElectionsController.cs`, `AdminElectionsController.cs` |
| Roles | SuperAdmin, Admin, Member. Seeded from `roles.json`. Client gets one role only. | `Constants.cs:5-10`, `AuthService.cs:366-374` |
| Non-member login | Possible. `CreateSystemAdminAsync` already makes users with `MemberId == null`. | `UserService.cs:77-100` |
| Cutting access | `SecurityStampMiddleware` checks the stamp on every request. Rotating it ends a session at once. | `GHCAA.API/Middleware/SecurityStampMiddleware.cs` |
| Step-up | Request and verify endpoints are `AdminOnly`. The vote endpoint needs step-up. | `AuthController.cs:163-197`, `ElectionsController.cs` vote action |
| Scheduler | None in the repo. | |
| Audit | Plain `AuditLog` rows. No hash chain. | `AuditLogMiddleware.cs:88` |

### Defect found while planning: members cannot vote

`POST api/elections/{id}/vote` carries `[RequireStepUp]`. The only way to get a step-up claim is
`POST auth/admin/step-up/request` and `/verify`, which are `AdminOnly`. A plain member gets 403 on the
vote and 403 again when the web dialog asks for a code. Mobile has no step-up handling at all. So no
ordinary member can vote on either client. This is item 37.1w and goes first.

## 3. Design in one page

- **Persona**: a post such as Chief Commissioner or Observer. A table row that SuperAdmin manages. It
  holds a set of permissions, a group name for the public board, a declaration text and a few flags.
- **Appointment**: one person holding one persona in one election. The person is a `User`. A member's
  appointment uses their existing user. A non-member gets a new user with no member link.
- **ElectionOfficial role**: a fourth role. Added to the user when they accept. Removed when their
  last live appointment ends. It only opens the officials area. What they can do inside it comes from
  their persona's permissions on that one election.
- **Permission check**: one filter, `[RequireElectionPermission(...)]`, on every election write.
  SuperAdmin always passes. Admin passes only until the Commission takes over. An official passes if
  an accepted, live appointment on that election has the permission.
- **Two-person rule**: some actions create a pending approval instead of running. A second person
  with the `Approve` permission, who is not the requester, approves and the action runs.
- **Access ends**: every appointment gets an end date when the result is declared. The permission
  filter and login both refuse after that date. Archiving the election revokes everything at once.
  No background job is needed.

## 4. Steps

Build in this order. Each step ends with its tests passing.

### 37.1w Members can complete step-up (P0)

Backend
- `AuthController.cs:163-197`: change both step-up actions from `[Authorize(Policy = AdminOnly)]` to
  `[Authorize]`. Keep the rate limit attributes as they are. Rename the routes to
  `auth/step-up/request` and `auth/step-up/verify`, and keep the old `auth/admin/step-up/*` routes as a
  second `[HttpPost]` on the same actions so current web builds still work.
- Same actions: the email is `user.Member?.Email`, falling back to `user.Username` when `MemberId` is
  null and the username contains `@`. Put this in one private method `StepUpEmailFor(User)` in
  `AuthService` (or wherever the step-up send lives; it is the method the controller calls today).
- Leave `OtpPurpose.AdminStepUp` as it is. Do not rename the enum value, since stored OTP rows use it.

Web
- `core/services/step-up.service.ts`: call the new routes.

Mobile
- `core/api/api_client.dart`: in the error interceptor, when the response is 403 with
  `code == 'STEP_UP_REQUIRED'`, show a code dialog (new `core/widgets/step_up_dialog.dart`, built from
  the existing dialog widgets in `core/widgets/`), call request and verify, then retry the request
  once. Cancel returns the original error.

Tests
- `GHCAA.Tests/Controllers/AuthControllerStepUpTests.cs` (new, or add to the existing auth controller
  tests if one covers step-up): a Member can request and verify; a non-member user with an email
  username gets the code sent to the username; a user with neither gets 400.
- `AuthorizationPolicyReflectionTests` (commit a8b287cd): update the expected policy for the two
  actions.
- Web: `step-up.service.spec.ts` checks the new URLs.
- Mobile: `test/core/api/step_up_interceptor_test.dart` checks the retry after a code.
- Tag all with `FR-39` (voting).

### 37.12a Election settings in OrgConfig

Add an `Elections` section to `OrgConfigDto`. Defaults in brackets.

| Key | Type | Default | Used by |
|---|---|---|---|
| `AdminKeepsControlAfterHandover` | bool | false | 37.12e |
| `TwoPersonActions` | string[] | Publish, OpenPolling, ReplaceBallotKey, ClosePolling, Declare, Archive, Count | 37.12f, 37.13h |
| `SuperAdminActsAlone` | bool | false | 37.12f, see Decision 1 |
| `ApprovalExpiryHours` | int | 48 | 37.12f |
| `AccessEndsDaysAfterDeclare` | int | 21 (7-day appeal window plus 14 days for the tribunal, Regulations §12) | 37.12g |
| `InviteLinkHours` | int | 72 | 37.12d |
| `CandidateOrder` | enum Random, Alphabetical | Random | 37.13b |
| `ShowTurnoutDuringPolling` | bool | false | 37.12i |
| `PublishPerSeatBallots` | bool | true | 37.13c |

Files
- `GHCAA.Application/DTOs/OrgConfigDto.cs`: new `ElectionSettingsDto` record, property `Elections` with
  a default instance so old ConfigJson rows read fine.
- `InstitutionProfileProvider.cs`: default for each profile pack.
- `GHCAA.Tests/OrgConfig/golden/org-config-ghc.golden.json`: add the section (see Decision 3).
- Angular `core/services/org-config.service.ts` model, and a new "Elections" card on the
  `/admin/org-config` screen using the existing form controls on that screen.
- Flutter `org_config.dart`: add the model, parse only. Mobile does not edit it.
- Constants: the action names come from the `ElectionApprovalAction` enum (37.12f), not strings.

Tests
- The golden-file test and the round-trip test in `GHCAA.Tests/OrgConfig/`.
- A test that a ConfigJson without `Elections` reads the defaults.

### 37.12b Personas

Domain (`GHCAA.Domain/Models/ElectionPersona.cs`, new)

```
ElectionPersona: Id, Name (100), GroupName (100), Description (500), Permissions (ElectionPermission),
  MinCount, MaxCount?, ShowOnPublicBoard, TakesOverFromAdmin, DeclarationText (4000),
  SortOrder, IsActive, CreatedAt, UpdatedAt
```

`Enums.cs`: new

```
[Flags] enum ElectionPermission : long {
  None = 0, ViewDashboard = 1, ViewAudit = 2, ManageSetup = 4, ManageVoterRoll = 8,
  DecideNominations = 16, AppointOfficials = 32, SetBallotKey = 64, ChangePhase = 128,
  Count = 256, Declare = 512, DecideAppeals = 1024, Approve = 2048 }
```

Permissions are code, since each one guards real code. Which persona has which is data.

Configuration: `ElectionPersonaConfiguration` in `ElectionConfigurations.cs`. Unique index on `Name`.

Default personas: a boot-time seeder `ElectionPersonaSeeder` in `GHCAA.Infrastructure/Data/`, run next
to `ProtectedSuperAdminSeeder` in `Program.cs`. It inserts the defaults only when the table is empty,
so SuperAdmin edits are never overwritten. No JSON file, no explicit ids.

| Name | Group | Permissions | Takes over | Public |
|---|---|---|---|---|
| ECSC Member | Search Committee | ViewDashboard | no | yes |
| Chief Commissioner | Election Commission | all except SetBallotKey, DecideAppeals | yes | yes |
| Commissioner | Election Commission | ViewDashboard, ViewAudit, ManageSetup, ManageVoterRoll, DecideNominations, AppointOfficials, ChangePhase, Declare, Approve | yes | yes |
| Returning Officer | Officials | ViewDashboard, ViewAudit, DecideNominations, SetBallotKey, Count, Approve | no | yes |
| Assistant Returning Officer | Officials | ViewDashboard, DecideNominations | no | yes |
| Presiding Officer | Officials | ViewDashboard | no | yes |
| Polling Officer | Officials | ViewDashboard | no | yes |
| Scrutineer | Officials | ViewDashboard, DecideNominations | no | yes |
| Counting Supervisor | Officials | ViewDashboard, Count | no | yes |
| Technical Administrator | Officials | ViewDashboard, ViewAudit | no | yes |
| Cybersecurity Auditor | Officials | ViewDashboard, ViewAudit | no | yes |
| Security Officer | Officials | ViewDashboard | no | no |
| Observer | Observers | ViewDashboard, ViewAudit | no | yes |
| Appeal Tribunal Member | Appeal Tribunal | ViewDashboard, ViewAudit, DecideAppeals | no | yes |

MinCount/MaxCount defaults: Chief Commissioner 1/1, Commissioner 2/4, Returning Officer 1/1, ECSC
Member 3/5, Appeal Tribunal Member 3/3, others 0/none. Declaration text for every row is the Neutrality
Declaration wording from `docs/Elections/05-Election-Forms-and-Templates.md`, copied at build time.
The counts are shown as warnings on the dashboard, not enforced, in line with decision 8.

Application
- `IElectionPersonaService` in `GHCAA.Application/Interfaces/`: `ListAsync(bool includeInactive)`,
  `CreateAsync`, `UpdateAsync`, `SetActiveAsync`, `DeleteAsync`.
- DTOs in `ElectionDtos.cs`: `ElectionPersonaDto`, `SaveElectionPersonaDto` with DataAnnotations
  (FluentValidation never runs here, see memory `gotcha_fluentvalidation_never_runs`).
- Delete refuses when any appointment uses the persona. The error says to deactivate it instead.

Infrastructure: `ElectionPersonaService.cs`, registered in the existing DI extension next to
`ElectionService`.

API: `ElectionPersonasController`, route `api/admin/election-personas`, class-level
`[Authorize(Policy = SuperAdminOnly)]`, `[RequireStepUp]` on writes. GET list, POST, PUT {id},
POST {id}/active, DELETE {id}. A second GET at `api/election-personas` for any election staff, active
rows only, for the appoint dialog.

Web
- `admin/election-personas/admin-election-personas.ts|html|scss`, copied from the
  `admin/governance/admin-governance.ts` list plus modal pattern. Uses `page-header`, `modal-header`,
  `confirm-dialog`, `search-bar`. Permissions are a checkbox list built from the enum.
- `core/services/election-personas.service.ts`, `core/models/election.models.ts` additions.
- Route `admin/election-personas` under AdminLayout with `superAdminGuard`. Nav item with
  `roles: ['SuperAdmin']`.

Migration `AddElectionPersonas` (PgSql folder only). Run `code-reviewer` on it.

Tests
- `ElectionPersonaServiceTests`: create, update, delete refused while in use, deactivate hides it from
  the active list, the seeder fills an empty table and leaves a non-empty one alone.
- `admin-election-personas.spec.ts`: permission checkboxes map to the flags value and back.

### 37.12c ElectionOfficial role and multi-role clients

Role row
- `Constants.Roles.ElectionOfficial = "ElectionOfficial"`.
- Created by the boot seeder from 37.12b through `RoleService.CreateRoleAsync` when no role of that
  name exists. `roles.json` is not touched (see Decision 3).

Policy
- `ServiceExtensions.cs:114-130`: new policy `Constants.Policies.ElectionStaff` =
  `RequireRole(SuperAdmin, Admin, ElectionOfficial)`.
- `MemberOnly` is unchanged. A member who is also an official still passes it.

Client role list
- `AuthController.Me` (`AuthController.cs:128`): add `Roles` (all Role claims) and
  `ElectionAppointments` (`[{ electionId, electionTitle, personaName, permissions }]`, accepted and
  live only) to the response. Keep `Role` as it is.
- The login and refresh token response gets the same list: `TokenResponseDto` (currently `Token`,
  `Username`, `MemberId`, `Role`, `FullName`, `Email`, `MobileNo`, `MustChangePassword`,
  `RefreshToken`) gains `IReadOnlyList<string> Roles`. `PickPrimaryRoleNameForClient` stays for the
  single `Role` field and must never return ElectionOfficial when the user also has Member, so a
  member official still lands on the member portal.
- Web `core/services/auth.service.ts`: store `roles` and `electionAppointments`. Add
  `hasRole(name)`. `core/guards/auth.guard.ts`: new `electionStaffGuard` (SuperAdmin, Admin or
  ElectionOfficial). `nav.service.ts:111-114`: filter with `item.roles.some(r => user.roles.includes(r))`.
  Fall back to `[user.role]` when `roles` is missing, so an old cookie session still works.
- `login.ts:247`: a user whose only role is ElectionOfficial goes to `/officials`.
- Flutter (Decision 4: mobile gets a real officials area, not just role parsing): parse `roles` and
  `electionAppointments` in `core/models/auth_models.dart`. Add `hasRole(name)` to the auth
  provider. A user whose only role is ElectionOfficial lands on a new officials shell instead of
  the member shell after login — same routing switch used today for Admin versus Member.

Tests
- `AuthControllerMeTests`: roles list and live appointments only.
- `PickPrimaryRoleNameForClient` test: Member plus ElectionOfficial gives Member.
- `auth.guard.spec.ts` for `electionStaffGuard`; `nav.service.spec.ts` for the roles array and the
  fallback.
- Flutter: `test/core/models/auth_models_test.dart` parses `roles` and `electionAppointments`; a
  routing test confirms ElectionOfficial-only lands on the officials shell.

### 37.12d Appointments

Domain (`Election.cs`, replaces `ElectionOfficer`)

```
ElectionAppointment: Id, ElectionId, PersonaId, UserId, MemberId?, DisplayName (150), Email (256),
  Phone? (30), AppointedByUserId, AppointedAt, AcceptedAt?, DeclarationSignedAt?,
  DeclarationTextSnapshot (4000)?, SignedFromIp? (64), RevokedAt?, RevokedByUserId?,
  RevokedReason? (500), ExpiresAt?
```

A computed rule, in one static method `ElectionAppointment.IsLive(DateTime now)`: accepted, signed, not
revoked, and `ExpiresAt` null or in the future.

Unique index on (ElectionId, PersonaId, UserId) where `RevokedAt` is null. The filter clause is
provider SQL, so write it with `HasFilter` only inside the existing `IsNpgsql()` guard and leave it off
for Sqlite tests.

`ScrutinyDecision.OfficerMemberId` becomes `DecidedByUserId`.

Migration `ElectionAppointments`
- Create the table. Copy each `ElectionOfficer` row across: persona matched by name from the enum
  (ReturningOfficer to "Returning Officer" and so on), `UserId` from `Users.MemberId`,
  `AcceptedAt` and `DeclarationSignedAt` set to the migration time, since they were already acting.
  The copy is SQL inside a Postgres guard. Then drop `ElectionOfficers`.
- `ScrutinyDecisions`: add `DecidedByUserId`, fill it from `Users` by member id, drop the old column.
- `Down` rebuilds `ElectionOfficers` from accepted rows whose persona name maps to the old enum. Rows
  for new personas are lost on rollback; say so in a comment in `Down`.
- The personas table must exist and be filled before the copy. The copy therefore inserts the four
  personas it needs if they are missing (by name, `ON CONFLICT DO NOTHING`). The boot seeder then sees
  a non-empty table. To avoid a half-filled set, the seeder inserts any default persona missing by
  name, not only when the table is empty. It still never updates an existing row.
- Remove `ElectionRole` enum and `ElectionOfficerDto` after the migration.
- Run `code-reviewer` on the migration. Dry-run the chain on a throwaway database with data, as in
  memory `session_migration_idempotency_validation`.

Application (`IElectionAppointmentService`, new)
- `AppointAsync(electionId, AppointDto, actorUserId)`. `AppointDto`: `PersonaId`, and either
  `MemberId` or (`DisplayName`, `Email`, `Phone?`).
  - Member: use the member's user. Name and email come from the member record.
  - Non-member: find a user whose `Username` equals the email and `MemberId` is null. If none, create
    one: `Username = Email`, random 32-byte password hash, `MustChangePassword = true`,
    `IsActive = true`, role ElectionOfficial is not added yet.
  - Refuse if the email belongs to a member (use the member path instead).
  - Send the invite (below).
- `AcceptAsync(appointmentId, userId, AcceptAppointmentDto { bool AgreeToDeclaration }, ip)`: only the
  appointee. Sets `AcceptedAt`, `DeclarationSignedAt`, `DeclarationTextSnapshot` (persona text at that
  moment), `SignedFromIp`. Adds the ElectionOfficial role if the user lacks it, rotates the security
  stamp so the next request carries the role. `AgreeToDeclaration = false` is refused.
- `DeclineAsync(appointmentId, userId, reason)`: sets `RevokedAt` with reason "Declined".
- `RevokeAsync(appointmentId, actorUserId, reason)`: sets `RevokedAt`. Calls `EndAccessIfNoneLeftAsync`.
- `EndAccessIfNoneLeftAsync(userId)`: if the user has no live appointment left, remove the
  ElectionOfficial role, rotate the stamp, revoke refresh tokens. If the user has no member link, no
  other role and no live appointment, set `IsActive = false`.
- `ListAsync(electionId)` and `ListMineAsync(userId)`.

Invite
- Reuse the password-reset link: generate the reset token the same way `ForgotPasswordAsync` does
  (`AuthService.cs:381-440`), with lifetime `InviteLinkHours`, and send the PASSWORD_RESET template.
  Pull the token part out of `ForgotPasswordAsync` into a private `IssueResetLinkAsync(user, hours)`
  so both use it. See Decision 2 for a proper invite template.
- A member appointee gets an in-app notification through `INotificationService` and the same email
  without the reset link. Reuse PASSWORD_RESET only for non-members.

Who may appoint
- Before the Commission takes over: SuperAdmin or Admin.
- After: a person with `AppointOfficials` on that election, or SuperAdmin.
- The ECSC and Chief Commissioner can be appointed by Admin before any takeover persona is live; that
  is how an election gets its first Commission.

API (`ElectionAppointmentsController`, new)
- `GET api/elections/{id}/appointments`: ElectionStaff plus permission ViewDashboard.
- `POST api/elections/{id}/appointments`: ElectionStaff plus AppointOfficials, step-up.
- `POST api/elections/appointments/{id}/revoke`: same.
- `GET api/me/election-appointments`: any signed-in user.
- `POST api/me/election-appointments/{id}/accept` and `/decline`: any signed-in user, step-up on
  accept.
- The old `POST api/elections/{id}/officers` is removed.

`DecideNominationAsync` (`ElectionService.cs:152-154`): the role check is replaced by the permission
filter from 37.12e. The service no longer checks officers itself; the method's `officerMemberId`
parameter becomes `decidedByUserId` and is written straight onto `ScrutinyDecision.DecidedByUserId`.
`ElectionsController.Scrutinise` (`ElectionsController.cs:89-95`) currently reads
`this.CurrentMemberIdRaw()`, which is null for a non-member official. It must switch to
`this.CurrentUserIdRaw()` (already used by `AuthController.Me`) so a non-member Scrutineer or
Returning Officer can decide nominations too.

Web
- `officials/my-appointments/my-appointments.ts`: list, declaration text, accept with a checkbox and
  step-up, decline with a reason. Shown to any signed-in user who has a pending appointment. Link from
  the member portal dashboard and from the officials area.
- `/set-password` is the existing reset-password page. Its heading reads from a query flag
  `invite=1` so the invite does not say "reset".
- Appointments tab in the officials dashboard (37.12h).

Mobile (Decision 4)
- `screens/officials/my_appointments_screen.dart`: same list, declaration text, accept (checkbox
  plus step-up dialog from 37.1w) and decline flow, built from the existing dialog and list widgets
  in `core/widgets/`. Linked from the member dashboard drawer when `electionAppointments` is
  non-empty, and from the officials shell (37.12h) for a pure official.
- The reset-password screen already used for invites on web has a Flutter counterpart
  (`screens/auth/set_password_screen.dart` or equivalent); it reads the same `invite=1` flag.

Tests (`ElectionAppointmentServiceTests`, new)
- Member appointment uses the member's user.
- Non-member appointment creates one user, and a second appointment for the same email reuses it.
- Email of an existing member is refused on the non-member path.
- Not live until accepted and signed.
- Accept stores the declaration snapshot and adds the role once.
- Revoking the last live appointment removes the role, rotates the stamp and deactivates a non-member.
- Revoking one of two leaves the role.
- Migration copy: a test that builds an `ElectionOfficer` row set is not possible under Sqlite
  `EnsureCreated`, so the copy is checked in the throwaway-database dry run and recorded in TODO.
- Flutter widget test for `my_appointments_screen.dart`: pending appointment shows accept/decline;
  accept without the declaration checkbox is blocked; declined appointment disappears from the list.

### 37.12e Permission check and admin handover

Application
- `IElectionAccessService` in `GHCAA.Application/Interfaces/`:
  `Task<ElectionPermission> GetPermissionsAsync(int electionId, int userId, IReadOnlyCollection<string> roles)`
  and `Task<bool> HasAsync(int electionId, int userId, roles, ElectionPermission needed)`.
- Rules, in order:
  1. SuperAdmin: all permissions.
  2. Union of permissions from the user's live appointments on this election.
  3. Admin: all permissions except `DecideAppeals`, unless a live appointment with a
     `TakesOverFromAdmin` persona exists on this election and `AdminKeepsControlAfterHandover` is
     false. Then Admin gets nothing from rule 3, only what rule 2 gives.
- `ElectionAccessService.cs` in Infrastructure. No cache; it is two small queries.

API
- `GHCAA.API/Filters/RequireElectionPermissionAttribute.cs`: an async action filter. Reads the election
  id from the route value `id` or `electionId`. For routes keyed by nomination or appointment id, the
  filter takes a `lookup` parameter naming which id it has, and resolves the election through
  `IElectionAccessService.ElectionIdForAsync(kind, id)`. Returns 403 as `ProblemExtensions.BuildProblemDetails`
  (the same shape `RequireStepUpAttribute` uses) with `Constants.ErrorCodes.ElectionPermission`, a new
  constant next to `StepUpRequired` (`Constants.cs:154-162`), and the missing permission name in the
  message.
- `ElectionsController`: every admin action moves from `AdminOnly` to `ElectionStaff` plus the filter:
  seats and phase to ManageSetup or ChangePhase, voter-roll freeze to ManageVoterRoll, scrutiny to
  DecideNominations, count to Count, declare to Declare.
- `AdminElectionsController`: class policy changes to `ElectionStaff`. `POST /` (create) keeps
  `AdminOnly`, since there is no election yet to hold a permission. publish, close, candidates to
  ManageSetup or ChangePhase; ballot-key to SetBallotKey.
- `GET api/admin/elections` lists all elections for Admin and SuperAdmin, and only elections with a
  live appointment for an official.
- `AdminElectionDto` gains `MyPermissions` (the flags as a list of names) and `AdminHandedOver` (bool).
- Reads of the audit log for an election need ViewAudit.

Tests
- `ElectionAccessServiceTests`: SuperAdmin always; Admin before and after takeover; the config switch;
  an expired or revoked appointment gives nothing; a union of two personas.
- `RequireElectionPermissionAttributeTests`: 403 body carries the code; route lookup by nomination id.
- `AuthorizationPolicyReflectionTests`: update the expected policies, and add a rule that every
  election write action has either the filter or `AdminOnly`.

### 37.12f Two-person rule (closes 37.1v)

Domain (`Election.cs`)

```
ElectionApproval: Id, ElectionId, Action (ElectionApprovalAction), PayloadJson? (8000),
  RequestedByUserId, RequestedAt, ExpiresAt, ApprovedByUserId?, ApprovedAt?,
  RejectedByUserId?, RejectedAt?, RejectReason?, ExecutedAt?
enum ElectionApprovalAction { Publish, OpenPolling, ReplaceBallotKey, ClosePolling, Declare, Archive, Count }
```

Only one open approval per election and action. Index on (ElectionId, Action, ExecutedAt, RejectedAt).

How it runs
- `IElectionApprovalService`: `RequestAsync(electionId, action, payload, userId)`,
  `ApproveAsync(approvalId, userId)`, `RejectAsync(approvalId, userId, reason)`, `ListOpenAsync(electionId)`.
- The controllers for the six actions call `ElectionApprovalService.RunOrRequestAsync(...)`. If the
  action is not in `TwoPersonActions`, or the caller is SuperAdmin and `SuperAdminActsAlone` is true,
  it runs at once. Otherwise it stores the request and returns 202 with the approval.
- Approve checks: the approver has `Approve` on the election (or is SuperAdmin), is not the requester,
  the request has not expired. Then it runs the stored action with the stored payload and sets
  `ExecutedAt`, in one transaction.
- The actions map to existing service calls: Publish to `SetPhaseAsync(Nomination)` (the publish
  path in AdminElectionsController), OpenPolling to `SetPhaseAsync(Polling)`, ClosePolling to
  `SetPhaseAsync(Counting)`, Declare to `DeclareAsync`, Archive to `SetPhaseAsync(Archived)` plus
  37.12g, ReplaceBallotKey to `SetBallotKeyAsync` with the new key from the payload.
- Count was left out at 37.12f, because the private key cannot be stored for a second person to
  approve later. 37.13h added it without storing the key. Approving a Count only checks the election
  is in Counting and marks the request approved. The count runs on a later call that brings the key,
  from anyone but the approver. That call claims the approval by setting `ConsumedAt`, so one
  approval allows one count, and gives it back if the count fails. A call with no key only reads
  stored results and never stores a request.
- Ballot key: the first key is set by a person with `SetBallotKey` alone. Replacing a key always goes
  through `ReplaceBallotKey`, and the payload stores the public key and its fingerprint so the approver
  sees what they approve. Each approval request, approval and rejection writes an audit row.
- The vote screen shows the key fingerprint (37.1v asked for this). Add it to the member election DTO
  and to `member/election/election.html` and the mobile election screen.

API: `GET api/elections/{id}/approvals`, `POST api/elections/approvals/{id}/approve`,
`POST .../reject`. ElectionStaff plus step-up.

Migration `ElectionApprovals`. Run `code-reviewer`.

Tests (`ElectionApprovalServiceTests`)
- An action in the list returns pending and does not run.
- The same person cannot approve their own request.
- A person without Approve cannot approve.
- Approve runs the action once; a second approve is refused.
- Expired request is refused.
- An action not in the list runs at once.
- SuperAdmin with the switch on runs at once; with it off, needs a second person.
- Replacing a key through approval sets the stored key, and the fingerprint matches.

### 37.12g Access ends by itself

- `DeclareAsync`: after declaring, set `ExpiresAt = DeclaredOn + AccessEndsDaysAfterDeclare days` on
  every live appointment of that election that has no `ExpiresAt` yet.
- The access service (37.12e) already refuses an expired appointment, so permissions stop on the date
  with no job running.
- `AuthService.LoginAsync` and the refresh path: if the user has no member link and their only role
  is ElectionOfficial, and they have no live appointment, refuse login with the same message as a
  deactivated account. Before refusing, call `EndAccessIfNoneLeftAsync` so the role and flag are tidied
  up at that moment.
- `AuthController.Me` returns only live appointments, so the officials nav disappears on the date.
- Archive: `SetPhaseAsync(Archived)` calls `IElectionAppointmentService.EndAllAsync(electionId)`, which
  revokes every appointment with reason "Election archived" and runs `EndAccessIfNoneLeftAsync` per
  user.
- A member official keeps their normal member access throughout. Only the extra role goes.

Tests
- Declare sets `ExpiresAt` using the config value.
- After the date, permission check fails and login of a non-member official is refused.
- Archive revokes all, removes the role, deactivates non-members, rotates stamps.
- A member official can still log in and use the member portal after archive.

### 37.12h Officials area on web and mobile (Decision 4)

Web
- Route group `officials` under AdminLayout with `electionStaffGuard`. Children:
  - `officials` (dashboard): picks the election from the user's appointments, or all elections for
    Admin and SuperAdmin.
  - `officials/elections/:id`: reuses the `AdminElections` component and hides buttons from
    `MyPermissions`. Do not copy the component.
  - `officials/appointments` (37.12d).
- AdminLayout nav: show only the items a pure ElectionOfficial may use (officials area, my
  appointments, profile, sign out). Nav items get `roles` lists; the existing filter handles it once
  37.12c lands.
- Dashboard cards, all from existing endpoints plus 37.12f: phase and timeline, open approvals with
  approve and reject buttons, nominations waiting, ballot key status and fingerprint, roll size,
  ballots cast (hidden during polling unless `ShowTurnoutDuringPolling`), appointments with accept
  status and count warnings from MinCount/MaxCount, and the audit list for ViewAudit holders.
- Shared components only: `page-header`, `modal-header`, `confirm-dialog`, `step-up-dialog`,
  `logo-spinner`, `pagination`. Theme tokens only.

Mobile
- New `screens/officials/` folder, parallel to `screens/admin/` and `screens/member/`:
  - `officials_dashboard_screen.dart`: same cards as web, read from the same endpoints, laid out as
    the existing card/list pattern used in `screens/member/` (not the admin desktop layout — see
    memory `mobile_not_web_parity`, mobile gets its own card/list shape, not a ported web screen).
  - `officials_election_detail_screen.dart`: phase, nominations to decide, ballot key status,
    reuses widgets already built for `screens/admin/election_screen.dart` where the same data is
    shown, but as its own screen, not a copy with hidden buttons.
  - `officials_approvals_screen.dart`: list of open approvals with approve/decline, calls
    37.12f's endpoints, step-up on approve.
  - `screens/officials/my_appointments_screen.dart`: from 37.12d.
- `core/routing/app_router.dart` (or the project's routing entry point): an ElectionOfficial-only
  user is routed to the officials shell; a member who also holds an appointment gets an entry point
  from the member dashboard drawer, not a separate shell.
- Nav/drawer: filtered the same way as web, by role and by `MyPermissions` per election.

Tests
- `officials-dashboard.spec.ts`: cards hidden without the permission; approve button hidden on the
  user's own request.
- `admin-elections.spec.ts`: add cases for buttons hidden by `MyPermissions`.
- Flutter: `officials_dashboard_screen_test.dart` and `officials_approvals_screen_test.dart`, same
  three cases as web (cards hidden without permission, self-approve hidden), run at phone viewport
  per `gotcha_flutter_tablet_width_hides_drawer_in_desktop_tests`.

### 37.12i Public board

- `GET api/elections/{id}/board`, anonymous. `ElectionBoardDto`:
  - `Title`, `Phase`, the timeline dates.
  - `Groups`: `[{ groupName, members: [{ personaName, displayName, acceptedAt }] }]`, accepted
    appointments on personas with `ShowOnPublicBoard`. Sorted by persona `SortOrder`. No email or
    phone.
  - `Turnout`: ballots cast and roll size, only after polling closes (or during it when
    `ShowTurnoutDuringPolling`), and hidden below `MinimumBallotsToCount`.
  - `KeyFingerprint`, `AuditHeadHash` (37.13a) after declare, and links to results and documents.
- Web: `public/elections/elections.ts` gets a "Who runs this election" section and the turnout line.
- Member page (`member/election/election.ts`): "You are on the roll" and "You have voted" lines, from
  data the member DTO already has (`HasVoted`, roll check).
- Mobile `screens/member/election_screen.dart`: a read-only officials list from the same endpoint.

Tests
- `ElectionBoardTests`: only accepted and public personas; no email in the DTO (reflection check like
  `CredentialVerificationDto`); turnout hidden during polling and below the minimum.
- `elections.spec.ts` for the board section; a Flutter widget test for the list.

### 37.12j Docs

- Spec 023 `spec.md`: new user stories for personas, appointments, approvals, board. FR numbers
  follow the last one in the file.
- `docs/Elections/02-Election-Operational-Manual.md`: how to appoint, accept, approve.
- `docs/API_CONTRACT_REGISTRY.md`, `docs/PROJECT_MAP.md`, `docs/FEATURES.md`, `docs/SRS.md`,
  `docs/ARCHITECTURE.md` per `feedback_docs_update_scope`.
- Run `graphify update .` after code changes.

## 5. Standards gaps with little effort (37.13)

Checked against `docs/Elections/00-election-standards-requirements.md`, the standards document. Effort
is S (under half a day), M (a day or two).

| Item | Standards section | What to build | Effort |
|---|---|---|---|
| 37.13a Audit hash chain | §11, audit and transparency | Table `ElectionAuditEntries` (Id, ElectionId, Seq, At, ActorUserId?, Action, DetailJson, PrevHash, Hash). `Hash = SHA-256(PrevHash + Seq + At + Action + DetailJson)`. Written by one `ElectionAuditWriter` called from the election, appointment and approval services, never with voter identity. `GET api/elections/{id}/audit/verify` recomputes the chain and returns the first broken `Seq` or ok. Head hash published on the board at declare. Tests: chain verifies; editing one row breaks it at that row. | M |
| 37.13b Candidate order on the ballot | §8, ballot design | `Nomination.BallotOrder` (int). Set when the election enters CandidateList: shuffled with `RandomNumberGenerator` or alphabetical per `CandidateOrder`. Ballot DTOs sort by it. The seed of the shuffle is written to the audit chain. Tests: order fixed after the phase change; alphabetical option. | S |
| 37.13c Per-seat opened ballots | §10, tallying | After count, if `PublishPerSeatBallots`, `GET api/elections/{id}/opened-ballots` returns per seat the list of choice sets, each seat's list shuffled on its own so rows cannot be joined across seats. Only after Declared. Tests: counts match results; rows are not in cast order. | S |
| 37.13d Dry run | §18, testing | `Election.IsTest` (bool), set at create. A test election is left out of the public list and the current-election endpoint, and `DeclareAsync` does not write ECMember rows. The board shows a "Test" badge. Tests: declare of a test election writes no committee. | S |
| 37.13e Key handling procedure | §14, security | Written procedure in `docs/Elections/`: who makes the key, where the file is kept, two-person replacement, destroying the file after declare with a signed note. Plus a "destroy key file" checkbox on the officials dashboard after declare, recorded in the audit chain. | S |
| 37.13f Incident response and threat model | §17, incident handling; §19, risks | Two short documents in `docs/Elections/`, plus a compliance table in the standards document mapping each "must" to the TODO item or code that meets it. | S |
| 37.13g Accessibility check | §13, accessibility | A manual WCAG 2.2 AA checklist for the vote screens, run once with keyboard only and a screen reader, results written to the checklist. No new tooling. | S |

The review stage in §4, the election lifecycle, needs no code. Publish is in `TwoPersonActions`,
so a second person reviews the setup before nominations open. The operational manual says so.

Left out on purpose, with the reason recorded in spec 023 Known limits:
- Sealing the ballot in the browser. Web can, but mobile would need a crypto package.
- A voter roll objection window. The regulations handle it on paper.
- Load testing. No tooling in the repo.
- Threshold keys (several key holders). Large change to the count.

### Gaps from the online-voting review (2026-10-04)

`docs/Elections/00-Election-Online-Voting-Review.md` was checked against the code. Most of its points
were already met or are documentation. Five were real gaps in the code and became 37.13h to 37.13l.
The documentation points went into 37.13e (ballot protocol and key custody) and 37.13f (threat model,
incident procedure, logging and retention).

| Item | Gap | Design | Effort |
|---|---|---|---|
| 37.13h Second person before the count | One person with Count and the key file counts alone. | `Count` joins `ElectionApprovalAction`. A count with no approved request answers 202 and stores a request with no payload. Once a second person approves it, the requester sends the key with the count, and the count claims the approved row in its own transaction (`ConsumedAt`). The key is never stored. The approver cannot count. One approval, one count, inside `ApprovalExpiryHours`. | M |
| 37.13i Sealed ballot bound to its election | AES-GCM runs with no associated data. | Associated data is `ghcaa-ballot:v2:election:{id}`. Stored as `v2:` plus base64; `:` is not a base64 character, so the two formats cannot be confused. `Election.BallotSealVersion`: 1 for rows that exist at migration, 2 for new elections. The count refuses a ballot whose format does not match, so an old-format ballot cannot be moved into a new election. | S |
| 37.13j Cross-election test | No test moves a ballot between elections. | Service test: seal for A, insert into B with the same key, count B, expect `unreadable` and nothing counted. Unit tests on `BallotSeal` for the round trip, a wrong election id, a changed prefix, and a legacy ballot in a version 2 election. | S |
| 37.13k connect-src | `connect-src 'self' https:` lets page script reach any https host. | `'self'` plus `AppSettings:AllowedOrigins`, the list CORS already reads, because the prod build calls an absolute API URL and the site may be reached on a second host name. Pinned by a header test. | S |
| 37.13l Request logging | The review assumed Serilog. There is none. | Confirmed by reading `AuditLogMiddleware` and `ExceptionMiddleware`: method, path and user only. Tests pin that the count key and the vote body never reach a log line or activity row. | S |

The trust model stays a trusted-server secret ballot. Threshold custody of the key was considered
and not built; it is an accepted trust assumption that 37.13e writes down.

## 6. Decisions — answered 2026-09-28

1. **Can SuperAdmin skip the two-person rule?** No. `SuperAdminActsAlone = false`, switchable in
   settings for emergencies.
2. **Invite email.** Reuse the PASSWORD_RESET template for non-member invites. No new
   ELECTION_INVITE row in `email_templates.json` for now.
3. **Seed data.** The ElectionOfficial role and the default personas are created by a boot-time
   seeder in code; `roles.json` is not touched. Approved: the golden OrgConfig fixture
   (`org-config-ghc.golden.json`) gets the new `Elections` section added at build time, as part of
   37.12a.
4. **Officials area on mobile.** Changed from the web-only default: build it on both web and
   mobile. Flutter gets its own officials screens (dashboard, election detail, appointments,
   approvals), not just the step-up fix and a read-only list. See the Mobile subsections added to
   37.12c, 37.12d, 37.12h and 37.12i below.

## 7. Order and size

| Order | Item | Size |
|---|---|---|
| 1 | 37.1w step-up for voters | S |
| 2 | 37.12a settings | S |
| 3 | 37.12b personas | M |
| 4 | 37.12c role and multi-role clients (web + mobile routing) | M |
| 5 | 37.12d appointments (web + mobile accept/decline) | L |
| 6 | 37.12e permission check | M |
| 7 | 37.12f two-person rule | M |
| 8 | 37.12g access ends | S |
| 9 | 37.12h officials area, web and mobile (Decision 4 grew this from M to L) | L |
| 10 | 37.12i public board | S |
| 11 | 37.13a to 37.13g | M in total |
| 11a | 37.13i, 37.13j, 37.13h, then 37.13k and 37.13l | M in total |
| 12 | 37.12j docs | S |

Decision 4 (mobile officials area) adds Flutter work across items 4, 5 and 9 above; no new item
number, since it is scope inside the same steps rather than a separate task.

Then Phase 2 of the earlier batch (37.1l, 37.1m, 37.1n), then 37.1o to 37.1r, 37.1t, 37.1u. 37.1v is
closed by 37.12f.

Gate after every item: `dotnet test`, `npx vitest run`, `npm run type-check`, `npx ng build`, and
`flutter test` for mobile changes (37.11).
