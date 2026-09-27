# Election ballot secrecy and standards: implementation plan

> **Status 2026-09-27:** not started. Open decisions D1 (receipt content), D2 (tie rule) and D3
> (start date) are listed in [spec.md](spec.md). Phase 1 does not wait on any of them.

## Dependency order

1. Phase 1, P0 backend: 37.1i ballot unlinkability, 37.1j whole-ballot submit and step-up,
   37.1k phase guards
2. Phase 2, P1 clients: 37.1l vote screen, 37.1m receipt and tracking codes, 37.1n public results
3. Phase 3, P2 rules: 37.1o multi-place seats and ties, 37.1p recount, consent, officer conflict,
   spoiled and unopposed, 37.1q filled ER forms

## Why this order

Phase 1 changes the tables and the vote request. The screens in Phase 2 are built on the new
request shape, so doing them first would mean doing them twice. Phase 2 comes before Phase 3
because a voter who cannot see names or confirm a ballot is a bigger risk than a missing tie rule.
Phase 3 needs D2 and a reading of the constitution, so it can wait.

## Phase 1: P0 backend (37.1i, 37.1j, 37.1k)

### Domain and data

- add `PendingBallot` (`Id` GUID, `ElectionId`, `ChoicesJson`), no member column, no time column
- change `Ballot.Id` and `BallotVote.Id` to GUID; drop `Ballot.IssuedAt` and `BallotVote.CastAt`
- keep `SeatVote` as the only record of who voted
- one PgSql migration; the Sqlite path follows the provider guard used by the other migrations
- existing ballots: copy into the new shape in a shuffled order; check a throwaway DB first
  (see `session_migration_idempotency_validation` in memory)

### Services

- `CastVoteAsync` takes the whole ballot, checks every seat, writes `SeatVote` rows and one
  `PendingBallot`, all or nothing
- new hosted job `BallotShuffleJob`: when 10 or more pending rows exist, or polling has closed,
  moves them into `Ballot` and `BallotVote` in random order, in its own transaction
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

## Execution checkpoint

A phase is done when it has:

- passed `dotnet test --filter "FullyQualifiedName~Election"` and the full suite
- passed the Angular and Flutter checks for the screens it touched
- updated the API contract registry entries
- updated specs 010, 011, 018 and 023 and the 37.1 items in `docs/TODO.md`
- had a `code-reviewer` pass on the migration, and a `security-reviewer` pass for Phase 1
