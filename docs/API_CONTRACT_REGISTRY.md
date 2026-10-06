# API Contract Registry

A changelog of changes to the API surface: what changed, when, and whether the
Web and Mobile clients were updated to match. Add an entry here whenever a
route, request DTO, or response DTO changes — see the "Add a new API
endpoint" row in `docs/PROJECT_MAP.md`'s Impact Guide and the workflow note
in `.github/copilot-instructions.md`.

This complements ADR-0005 (`docs/adr/0005-no-api-versioning-compatibility-rule-instead.md`),
which sets the *rule* for how endpoints may change (additive only, never
remove or rename). This file is the *record* of each time that rule was
applied — one entry per change, so a client left behind is visible at a
glance.

## Current endpoint surface

The full route list lives in one place and this file doesn't duplicate it
line by line: `GHCAA.Web/src/app/core/constants/app.constants.ts`, the
`API_ENDPOINTS` constant (currently ~40 routes across Admin, Auth, Events,
Gallery, News, Financials, Messaging, Governance, Polls, and more). The
Flutter side calls the same routes from `GHCAA.Mobile/lib/**/*_service.dart`
files, without a matching constants file — when adding an endpoint there,
check `API_ENDPOINTS` first so the two clients agree on the literal path.

The CI pipeline (`.github/workflows/ghcaa-ci-standard.yml`) already runs an
API contract snapshot comparison against `docs/api/swagger.json`
(`docs/api/diff_swagger.py`) — that catches an accidental breaking change.
This registry is the complementary manual record: it captures *why* a change
was made and *which client picked it up*, which a snapshot diff can't tell.

## Entry format

One entry per endpoint change, newest first:

```
### YYYY-MM-DD — <endpoint path or DTO name>
- Change: <what changed — new field, new route, deprecated field, etc.>
- Reason: <why>
- Web: <updated in <file> | not applicable | pending>
- Mobile: <updated in <file> | not applicable | pending>
```

Only log a change once it's actually shipped (merged), not while still in
review. If a change lands on the API but a client update is still pending,
say so — that's the point of the registry: a change with no client entry is
a gap someone still needs to close.

## Log

### 2026-10-06 — /api/elections/appointments/{appointmentId}/emergency-revoke, /api/elections/{id}/approvals, /api/elections/approvals/{approvalId}/approve, /api/elections/approvals/{approvalId}/reject
- Change: new routes. `POST .../emergency-revoke` answers 202 with an approval. Another active official with Approve on that election approves it; a SuperAdmin has no bypass. `GET api/elections/{id}/approvals` lists approvals. `POST approvals/{approvalId}/approve` and `.../reject` answer one. A second open request for the same step is refused.
- Reason: TODO 37.13, spec 023. An emergency revoke always needs two people.
- Web: updated in `app.constants.ts` (`EMERGENCY_REVOKE`, `APPROVALS`, `APPROVE`, `REJECT`).
- Mobile: emergency revoke updated in `features/elections/election_service.dart` (`emergencyRevoke`) and the officials sheet. Listing, approving and rejecting approvals: not applicable, approving happens on the web admin (TODO 37.13u, item 5).

### 2026-10-06 — /api/admin/elections/rules-unlock, /api/admin/elections/rules-unlock/close
- Change: new routes, SuperAdmin only. `GET` returns the open unlock, `POST` opens one (`reason` 20-1000 characters, `minutes` 1-60, default 30), `POST .../close` ends it. Every other election rule change now answers 409 with code `ELECTION_RULES_FROZEN` while an election is waiting for polling, polling or counting, unless an unlock is open.
- Reason: TODO 37.13q and 37.13v, spec 023.
- Web: updated in `app.constants.ts` and `admin/elections/election-rules-unlock`.
- Mobile: not applicable. No admin election screens on mobile.

### 2026-10-06 — DELETE /api/roles/users/{id}
- Change: a hard delete of a user with election records (appointments, approvals, rules unlocks, scrutiny decisions) answers 409.
- Reason: those rows must keep their user. `IUserService.HasElectionRecordsAsync` decides.
- Web: not applicable. Mobile: not applicable.

### 2026-10-06 — OrgConfigDto.elections, step-up OTP replies
- Change: `elections.countRequesterOnly` (bool, default false) is new. On, only the official who asked for a count may run it. The `twoPersonActions` default now leaves out `EmergencyRevoke`. Step-up OTP replies carry a masked email, and a failed OTP email answers 503.
- Reason: TODO 37.13n, 7.19.
- Web: updated in `election.models.ts` and `admin/org-config`. Mobile: updated in `org_config.dart`.

### 2026-10-04 — /api/admin/governance/periods/{id}/seats, /api/admin/governance/members/{ecMemberId}/end-term
- Change: new routes, AdminOnly. `GET periods/{id}/seats` lists each committee seat of a period with its current holder, so a vacant seat shows as empty. `POST members/{ecMemberId}/end-term` ends a term and takes `reason` (a `VacancyReason`, required), `note` (optional, capped at `Governance.VacancyNoteMaxLength`) and `notifyMember`. It answers 404 for an unknown member and 409 when the service refuses. It replaced `DELETE members/{ecMemberId}`, which ended a term without a reason. `DELETE members/{ecMemberId}/hard-delete` (step-up) is older and unchanged.
- Reason: TODO 95.3. A seat can fall vacant mid-term and the record has to say why.
- Web: updated in `core/services/admin.service.ts` (`getCommitteeSeats`, `endCommitteeTerm`) and `admin/governance/admin-governance.ts`.
- Mobile: end-term updated in `features/admin/admin_service.dart`. Seats: not used yet; the mobile governance screen lists members only (vacant-seat view is TODO 95.4).

### 2026-09-30 — /api/elections/{id}/appointments, /api/elections/appointments/{id}/revoke, /api/me/election-appointments
- Change: new routes. `GET` and `POST api/elections/{id}/appointments` list and make appointments. `POST api/elections/appointments/{id}/revoke` ends one. `GET api/me/election-appointments` lists the caller's own, and `POST .../{id}/accept` and `.../{id}/decline` answer one. Appoint, revoke and accept need step-up. `POST api/elections/{id}/officers` is removed. This breaks the additive rule in ADR-0005, but no web or mobile code ever called it.
- Reason: TODO 37.12d. An appointment counts only after the person accepts it and signs the declaration. It can be for a member or for someone with no member record.
- Web: pending (37.12d).
- Mobile: pending (37.12d).

### 2026-09-30 — /api/auth/me, TokenResponseDto
- Change: both now carry `roles`, the full role list. `/me` also carries `electionAppointments`, the caller's live appointments, each with the election, the persona name and its permissions.
- Reason: TODO 37.12c and 37.12d. A user can hold ElectionOfficial beside another role, so one role name is not enough.
- Web: `auth.models.ts`, `hasRole` and `electionStaffGuard`.
- Mobile: `auth_models.dart`, stored in `StorageService`.

### 2026-09-29 — /api/admin/election-personas, /api/election-personas
- Change: new routes. Admin list, create, update, set active and delete for election personas. A plain signed-in read route lists the active ones.
- Reason: TODO 37.12b. Election posts, their permissions and their declaration text are data, not code.
- Web: `/admin/election-personas`, through `ElectionPersonasService`.
- Mobile: not applicable.

### 2026-09-29 — /api/auth/step-up/request, /api/auth/step-up/verify
- Change: new routes open to any signed-in user. The old `api/auth/admin/step-up/*` paths still answer.
- Reason: TODO 37.1w. The vote route needs step-up, and members could not reach the admin-only step-up routes.
- Web: `STEP_UP_REQUEST` and `STEP_UP_VERIFY` in `app.constants.ts`.
- Mobile: `step_up_interceptor.dart` and `step_up_dialog.dart`.

### 2026-09-27 — /api/admin/elections/{id}/ballot-key
- Change: new route. Takes `{ publicKey }`, the returning officer's RSA-OAEP public key (SPKI, base64, 3072 bits or more). Stores it with its SHA-256 fingerprint and returns the admin election, which now carries `ballotKeyFingerprint`. Refused once polling has opened. Admin only, with step-up.
- Reason: TODO 37.1s. Ballots are sealed under this key so database access alone cannot read them. Spec 023 FR-001.
- Web: `ElectionsService.setBallotKey`, called from the admin elections page after the key file has downloaded.
- Mobile: reads `ballotKeyFingerprint` and shows it. Making the key is web only.

### 2026-09-27 — /api/elections/{id}/count
- Change: the body is now `{ privateKey }`, the officer's PKCS#8 key as base64. It is needed for the first count only. A later call with no body returns the stored result. A failed count returns 400 with a reason in `detail` (no key, wrong key, fewer than 10 ballots, totals do not match).
- Reason: TODO 37.1s. The count opens the sealed ballots in one shuffled pass.
- Web: the admin page reads the key file and sends it. `public/elections/election-results` calls with no key and gets the stored result.
- Mobile: still posts no body, so it only reads stored results. The service comment says the first count runs from the web admin.

### 2026-09-27 — /api/elections/{id}/vote
- Change: the body is the whole ballot, `{ seats: [{ electionSeatId, nominationIds }] }`, one entry per seat with an accepted candidate. An empty list is an abstention. The response is `{ trackingCode }`, made by the server. The client serial field is gone. The route now needs step-up.
- Reason: spec 023 FR-003, FR-005 and FR-006.
- Web: `ElectionsService.castBallot` from the member election page.
- Mobile: `ElectionService.castBallot` sends the same body.

### 2026-09-22 — /api/admin/elections
- Change: added the admin election list, create, publish, close, and candidate-management routes expected by the Angular admin screen. The list returns the admin election shape used by the dashboard, and the admin lifecycle actions move elections through the documented voting phases.
- Reason: the web admin UI was calling `/api/admin/elections` while the API only exposed the public `/api/elections` route family, so the missing route surface caused 404s.
- Web: implemented in `GHCAA.Web/src/app/core/services/elections.service.ts` and `GHCAA.Web/src/app/admin/elections/admin-elections.ts`.
- Mobile: not required; `GHCAA.Mobile/lib/features/elections/election_service.dart` already uses the public `/elections` contract and remains compatible without any admin-route change.

### 2026-09-22 — /api/verify/{shortCode}
- Change: added an anonymous, rate-limited credential verification endpoint and an
  admin-only credential revocation endpoint. Generated ID cards and membership
  certificates now persist a high-entropy 10-character credential code and embed
  its verification URL in the QR.
- Reason: Work Package 37.8 public credential verification.
- Web: implemented in `core/services/credential-verification.service.ts` and
  the public `/verify` route/page.
- Mobile: implemented in `features/credentials/credential_verification_service.dart`,
  `screens/credential_verification_screen.dart`, and the public `/verify` routes.

### 2026-09-22 — /api/archive
- Change: added approved-only public collection and item routes, transcript search,
  admin incomplete-record listing and moderation, and authenticated item submission.
- Reason: Work Package 37.6 oral-history archive backend/API.
- Web: implemented in `core/services/archive.service.ts` and the `/legacy` public route/page.
- Mobile: implemented in `features/archive/archive_service.dart` and
  `screens/member/legacy_archive_screen.dart`.

### 2026-09-22 — /api/scholarships public application and status routes
- Change: added typed public scholarship funds/calls, application submission, and
  reference/email status lookup support for the existing scholarship API.
- Reason: Work Package 37.2 adds the public application journey.
- Web: implemented in `core/services/scholarship.service.ts` and the
  `/scholarships` public route/page.
- Mobile: implemented in `features/scholarships/scholarship_service.dart` and
  its typed models. The mobile app has no matching public scholarship route or
  screen, so this package exposes the calls for a later surface without
  inventing one in WP37.2.

### 2026-09-21 — AcademicRecordDto / AcademicRecords.IsOrgProfile
- Change: renamed the academic institution flag from `IsGHC`/`isGHC` to
  `IsOrgProfile`/`isOrgProfile` across the Domain, API, database, Web, and
  Mobile contracts.
- Reason: the flag identifies the active organisation profile and must not be
  tied to one institution name.
- Compatibility: the PostgreSQL migration renames the column without changing
  boolean values. The API accepts legacy `isGHC` input during rollout but
  emits only `isOrgProfile`.
- Web: updated registration, member profile, approval, and typed model surfaces.
- Mobile: updated registration, profile editing, typed profile parsing, and
  payloads.

### 2026-09-21 — /api/communications/me, /api/admin/comm/member/{memberId}
- Change: added paginated member communication history and an admin per-member log view, including channel, delivery status, message detail, and targeted/broadcast scope.
- Reason: Work Package 81.1–81.3 makes outbound email and SMS visibility explicit without exposing another member's records.
- Web: implemented in `member-communications.service.ts`, the member communications route, and `admin-comm.service.ts`.
- Mobile: implemented in `notification_service.dart` and the member communications route.
- Delivery: email uses `IEmailService`; configured SMS templates and workflow alerts use `ISmsService` and write `EmailLog` rows. Direct OTP SMS remains outside this history.

### 2026-09-21 — /api/elections
- Change: added the persisted election engine resource and election summary response.
- Reason: Work Package 37.1 creates elections and exposes their current phase.
- Web: implemented in `GHCAA.Web/src/app/core/services/elections.service.ts` and election routes/surfaces.
- Mobile: implemented in `GHCAA.Mobile/lib/features/elections/election_service.dart` and the member election route.

### 2026-09-21 — /api/elections/{id}/phase, /api/elections/{id}/voter-roll/freeze
- Change: added phase transition and frozen voter-roll endpoints.
- Reason: Work Package 37.1 requires an auditable eligibility snapshot.
- Web: phase and voter-roll calls are implemented in the election service; administrative surfaces consume the lifecycle routes.
- Mobile: the typed election service exposes the shared election contract; voter-roll administration remains an administrative API concern.

### 2026-09-21 — /api/elections/{id}/seats, /api/elections/{id}/officers
- Change: added election seat and officer assignment endpoints.
- Reason: Work Package 37.1 needs position capacity and named returning, polling, and scrutiny officers.
- Web: administrative election surface consumes seat and officer calls.
- Mobile: not exposed in the member surface; the shared typed service remains compatible with the contract.

### 2026-09-21 — /api/elections/{id}/nominations
- Change: added nomination listing and submission endpoints.
- Reason: Work Package 37.1 covers nomination, proposer/seconder eligibility, and candidate status.
- Web: nomination listing and submission are implemented in the member and admin election surfaces.
- Mobile: nomination listing and submission are represented by the typed election service; member UI currently focuses on voting.

### 2026-09-21 — /api/elections/nominations/{nominationId}/scrutiny, /api/elections/nominations/{nominationId}/withdraw
- Change: added scrutiny decision and withdrawal endpoints.
- Reason: Work Package 37.1 requires officer decisions and withdrawal before the final candidate list.
- Web: scrutiny and withdrawal calls are implemented in the admin/member election services.
- Mobile: typed service support is present; officer-only scrutiny remains outside the member UI.

### 2026-09-21 — /api/elections/{id}/vote
- Change: added secret-ballot vote recording. The request identifies the voter only through the authenticated member claim; the ballot vote has no member foreign key.
- Reason: Work Package 37.1 requires one vote per frozen eligible voter without linking voter identity to the selected candidate.
- Web: member voting surface sends the authenticated request contract.
- Mobile: member election screen and typed election service send the same vote contract.

### 2026-09-21 — /api/elections/{id}/count, /api/elections/{id}/declare
- Change: added count, tie, winner, and declaration endpoints.
- Reason: Work Package 37.1 requires persisted results, recount-safe replacement of result rows, and EC roster updates on declaration.
- Web: public results and admin counting/declaration surfaces consume these routes.
- Mobile: typed result support is present; counting and declaration remain administrative operations.

### 2026-09-21 — /api/elections/{id}/documents/{formCode}
- Change: added fixed-layout A4 PDF generation for the approved ER election forms, sourced from the persisted election record and carrying election reference, form code, generation time, page number, evidence state, and verification QR data.
- Reason: Work Package 37.1e requires completed official records rather than raw Markdown handbook downloads.
- Web: `ElectionsService.getOfficialDocument` downloads the PDF blob.
- Mobile: `ElectionService.downloadOfficialDocument` downloads the same PDF contract.

### 2026-09-17 — /networking/search, /networking/directory
- Change: added an optional `cursor` param to `MemberSearchFilterDto` and a
  populated `NextCursor` on `PagedResult<T>`. Sending a cursor switches the
  server from offset Skip/Take to keyset pagination ordered by
  `(FullName, Id)`. Omitting it keeps the old `page`/`pageSize` behavior, so
  this is additive per ADR-0005.
- Reason: TODO 8.3 — offset pagination re-scans and re-sorts the full result
  set on every page for the Alumni Registry list, which gets slower as the
  member count grows. Cursor pagination reads only the next page.
- Web: not applicable — `member-approval`/admin list screens on the Web side
  weren't touched; they still use `page`/`pageSize` and keep working
  unchanged.
- Mobile: updated in `GHCAA.Mobile/lib/features/networking/networking_service.dart`
  and `GHCAA.Mobile/lib/screens/member/directory_screen.dart`
  (`professional_hub_screen.dart` shares the same endpoint but wasn't
  touched — it still sends no cursor and gets the legacy offset behavior).

### 2026-09-17 — docs/API_CONTRACT_REGISTRY.md (this file)
- Change: registry created. No endpoint changed; this establishes the format
  above and the cross-references from `docs/PROJECT_MAP.md` and
  `.github/copilot-instructions.md`.
- Reason: TODO 12.2 — WP12 (Process & Engineering Standards) asked for a
  changelog of endpoint changes and which clients picked each one up. Past
  endpoint changes weren't logged anywhere in this format, so the log starts
  here rather than backfilling guesses.
- Web: n/a
- Mobile: n/a
