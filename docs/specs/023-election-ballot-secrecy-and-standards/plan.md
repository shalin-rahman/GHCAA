# Election ballot secrecy and standards: implementation plan

> **Status 2026-09-28:** Phase 1 and 37.1s (sealed ballots) built on branch
> `prepod-election-refactoring`, not yet committed. Phase 4 (officials, personas, access) is
> approved and starts next, at 37.1w, since voting is broken without it. Phase 2 follows.
> Decisions D1 (receipt content), D2 (tie rule) and D3 (start date) are settled in [spec.md](spec.md).
> The seat-count half of 37.1o was pulled forward because the count was wrong without it.

## Dependency order

1. Phase 1, P0 backend: 37.1i ballot unlinkability, 37.1j whole-ballot submit and step-up,
   37.1k phase guards
2. Phase 2, P1 clients: 37.1l vote screen, 37.1m receipt and tracking codes, 37.1n public results
3. 37.1s sealed ballots under the returning officer's key. It follows Phase 1 because it changes
   the same vote and count code, and it comes before Phase 2 because the vote screen must not open
   an election that has no key.
4. Phase 3, P2 rules: 37.1o multi-place seats and ties, 37.1p recount, consent, officer conflict,
   spoiled and unopposed, 37.1q filled ER forms

## Why this order

Phase 1 changes the tables and the vote request. The screens in Phase 2 are built on the new
request shape, so doing them first would mean doing them twice. Phase 2 comes before Phase 3
because a voter who cannot see names or confirm a ballot is a bigger risk than a missing tie rule.
Phase 3 needs the tie rule from D2 on the setup screen and a reading of the constitution, so it can wait.

## Phase 1: P0 backend (37.1i, 37.1j, 37.1k)

### Domain and data

- add `PendingBallot` (`Id` GUID, `ElectionId`, `ChoicesJson`), no member column, no time column
- change `Ballot.Id` and `BallotVote.Id` to GUID; drop `Ballot.IssuedAt` and `BallotVote.CastAt`
- keep `SeatVote` as the only record of who voted
- one PgSql migration; the Sqlite path follows the provider guard used by the other migrations
- existing ballots: copy into the new shape in a shuffled order; check a throwaway DB first
  (see `session_migration_idempotency_validation` in memory)

### Services

- `CastBallotAsync` takes the whole ballot, checks every seat, writes `SeatVote` rows and one
  `PendingBallot`, all or nothing
- the move into `Ballot` and `BallotVote` runs in the request, not a hosted job. After each vote
  commits, it moves the waiting rows in random order once 10 are waiting. `SetPhase(Counting)` and
  `CountAsync` move whatever is left. Each move has its own transaction, and a failed move after a
  vote still returns the voter's tracking code. The app runs one instance on Render, so a hosted
  job would add a moving part without a gain
- server makes the tracking code; `CastVoteDto.SerialNumber` removed
- `CountAsync`: Counting phase only, no pending rows left, stores the result, repeat call returns it
- `SetPhaseAsync`: refuse Declared
- `AddCandidateAsync` and `RemoveCandidateAsync`: CandidateList or earlier only; add goes through
  roll check, proposer, seconder and scrutiny

### API

- `POST /api/elections/{id}/vote` takes the whole ballot and carries `[RequireStepUp]`
- `AuditLogMiddleware`: for the vote route, log "voted in election N" with the date, no body, no
  seat, no time
- fix the doc comment on `ElectionsController.Vote`
- update the API contract registry entry for the vote endpoint

### Tests

- `ElectionServiceTests`: 20 ballots cast in a known order; ballot keys and stored order do not
  follow it; no time field on the ballot side
- a whole ballot with one bad seat writes nothing
- client serial is rejected
- count during Polling fails; count with pending rows fails; second count returns the stored result
- `SetPhase(Declared)` fails; `DeclareAsync` writes `ECMember`
- candidate add and remove after CandidateList fail
- audit log test: vote entry has no seat and no time of day
- tag each test with its FR from spec 023

## 37.1s: sealed ballots and one mix at the count

### What was built

- the returning officer makes an RSA-OAEP key pair (3072 bits) in the browser; the private key is
  saved as a .pem file before the public key is sent to `POST api/admin/elections/{id}/ballot-key`
- the key can be set or replaced up to the campaign; `SetPhase(Polling)` is refused without one
- each ballot is sealed with AES-256-GCM under a fresh key wrapped by the officer's public key,
  and padded to 512-byte blocks so its length does not show how many names were marked
- tracking codes live in `BallotReceipts`, which has no link to the ballot rows
- the count takes the private key, opens every pending ballot in one Serializable transaction,
  shuffles once, writes the ballots and totals, then deletes the pending rows
- the count is refused under 10 ballots (`Constants.Elections.MinimumBallotsToCount`); the officer
  counts by hand instead
- on Postgres the count ends with `VACUUM` on the pending table so dead rows do not stay on disk

### Deploy notes

- empty `PendingBallots` before the migration; the migration stops with an error otherwise
- `gen_random_uuid()` needs Postgres 13 or later
- Down cannot turn sealed rows back into plain ones and pairs receipts with ballots at random
- on a legacy database the MigrationBootstrapper cannot baseline this migration, so apply it with
  `dotnet ef database update` and check the history table

### Tests

- `BallotSealTests`: seal and open, tampering, wrong key, short key refused, fingerprint match
- `ElectionServiceTests`: no key blocks the vote, wrong key and short count refused, totals right
- web `ballot-key.util.spec.ts` and `admin-elections.spec.ts`; mobile fingerprint parse test

## Phase 2: P1 clients (37.1l, 37.1m, 37.1n)

### Backend

- member election read returns `hasVoted` from `SeatVote`
- public `GET /api/elections/{id}/results`, Declared or Archived only: every accepted candidate
  with votes (zero included), abstentions, spoiled ballots, turnout
- public `GET /api/elections/{id}/tracking-codes`, after polling closes, sorted, codes only

### Web

- rebuild `member/election` as a ballot: seat by position title, candidates by name and photo,
  abstain per seat, then a review step, then one confirm
- read `hasVoted` on load; show "You have voted" and no buttons
- confirmation view with the receipt and a print stylesheet; `window.print()`; content held only
  in the component, gone on leaving
- code lookup box on the results page against the tracking-code list
- `public/elections/election-results` switches to the public results endpoint
- use the existing shared components (confirm dialog, page header, logo spinner) and theme tokens;
  this is the place to start the shared ballot component named in spec 018 ENH-006

### Mobile

- same ballot, review and confirm flow in `election_screen.dart` and `election_service.dart`
- remove the serial number field
- receipt as a PDF built on the device, handed to the share sheet
- results screen reads the public results endpoint

### Tests

- Angular: review step blocks submit until confirm; reload with `hasVoted` shows no buttons;
  receipt has names and code, no server call
- Flutter: same three, on a phone viewport (see
  `gotcha_flutter_tablet_width_hides_drawer_in_desktop_tests` in memory)
- backend: results endpoint 404 before Declared; zero-vote candidates are listed

## Phase 3: P2 rules (37.1o, 37.1p, 37.1q)

### Domain and services

- seats with `SeatCount` above 1: accept up to `SeatCount` choices, elect the top `SeatCount`
- `Election.TieRule` from D2, applied in `CountAsync`
- recount: request window, ER-26 and ER-28, officer confirms before the stored result changes
- `Nomination.CandidateConsentAt`, required before Accepted (ER-10)
- officer conflict check in nomination and candidate add
- spoiled-ballot rule and count
- unopposed declaration if the constitution allows it

### Documents

- `ElectionDocumentService` fills every ER form from stored records; ER-23 carries the counts

### Web and Mobile

- multi-choice seats show "choose up to N"
- admin screen: recount request, consent status, unopposed flag

### Tests

- two-place seat elects two; tie applies the stored rule
- nomination without consent stays Submitted
- officer as candidate rejected
- ER-23 PDF text contains the per-candidate counts

## Phase 4: officials, personas and access (FR-026 to FR-033, approved 2026-09-28)

Full step-by-step detail, exact files, entities, migrations, endpoints and tests are in
[plan-officials.md](plan-officials.md). Summary of the build order there:

1. 37.1w: open the step-up endpoints to any signed-in user (fixes the "members can't vote" defect)
2. 37.12a: `Elections` section in OrgConfig
3. 37.12b: `ElectionPersona` table and the SuperAdmin screen
4. 37.12c: `ElectionOfficial` role, roles list on `/me` and the token response
5. 37.12d: `ElectionAppointment` replaces `ElectionOfficer`, accept and decline flow
6. 37.12e: `IElectionAccessService` and `[RequireElectionPermission]`, admin handover rule
7. 37.12f: two-person approvals for publish, open polling, replace key, close polling, declare,
   archive (closes 37.1v)
8. 37.12g: access ends automatically after declare or archive, no scheduled job
9. 37.12h: officials area on web, reusing the admin election screen
10. 37.12i: public board of officials, timeline and turnout
11. 37.13a to 37.13g: the standards gap fixes (audit hash chain, ballot order, per-seat published
    ballots, test elections, key procedure, incident response docs, accessibility check)
12. 37.12j: spec, API registry, project map, features, SRS and architecture doc updates

All four decisions in `plan-officials.md` §6 are answered: no SuperAdmin bypass by default,
PASSWORD_RESET reused for non-member invites, the golden OrgConfig fixture may be edited, and the
officials area is built on both web and mobile (not web-only). Build in the order in
`plan-officials.md` §7, starting with 37.1w.

## Execution checkpoint

A phase is done when it has:

- passed `dotnet test --filter "FullyQualifiedName~Election"` and the full suite
- passed the Angular and Flutter checks for the screens it touched
- updated the API contract registry entries
- updated specs 010, 011, 018 and 023 and the 37.1 items in `docs/TODO.md`
- had a `code-reviewer` pass on the migration, and a `security-reviewer` pass for Phase 1
