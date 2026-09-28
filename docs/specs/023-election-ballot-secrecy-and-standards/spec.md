# Feature Specification: Election ballot secrecy and election standards

**Feature Branch**: 023-election-ballot-secrecy-and-standards
**Created**: 2026-09-27
**Status**: Draft, not started
**Input**: Review of the election engine on 2026-09-27. The user asked that votes stay anonymous,
that a voter can print their ballot after voting, and that the engine be checked against common
election practice.
**Depends on**: [010-election-engine-fixes](../010-election-engine-fixes/spec.md) (the engine as
built), [011-election-module-redesign](../011-election-module-redesign/spec.md) (the web and mobile
screens), [018-governance-elections-polls](../018-governance-elections-polls/spec.md) (the as-built
governance baseline)
**Tracker**: `docs/TODO.md` items 37.1i to 37.1q, under Work Package 37

## Purpose and scope

The engine keeps who voted (`SeatVote`, `VoterRoll.VotedAt`) apart from what was voted (`Ballot`,
`BallotVote`). Spec 010 says nothing joins the two. That is true of the foreign keys and false in
practice. This spec lists how the two halves can be joined today, what has to change so they cannot
be, and the other gaps against normal election practice found in the same review.

In scope: the backend vote path, the count and declare path, candidate management, the member vote
screens on web and mobile, the printed receipt, results publication and the ER forms.

Out of scope: postal or paper ballots, end-to-end verifiable cryptography (homomorphic tallies,
mix-nets), and ranked or preferential voting. Those are recorded under "Not adopted" with the reason.

## Standards applied

The requirements below come from these common rules of a secret-ballot election. They are stated
here so a reviewer can check each requirement against a rule, not against taste.

1. **Secrecy.** No one, including a database admin, can tell how a named member voted.
2. **Receipt-freeness.** A voter cannot prove to someone else how they voted. This is what stops
   vote buying and pressure. It limits what a printed ballot may contain.
3. **One member, one vote per seat**, checked against a roll frozen before polling.
4. **Cast as intended.** The voter sees the full ballot, with names, and confirms it before it is
   counted.
5. **Counted as cast.** A voter can check that their ballot is in the counted set, without that
   check showing the choice.
6. **No early results.** Nothing is counted or shown while polling is open.
7. **Fixed candidate list.** After the candidate list is published, candidates change only by the
   withdrawal rule, and never during polling.
8. **Published result with the full tally**, including candidates with zero votes, abstentions and
   spoiled ballots, plus a recount route.
9. **Officers are neutral.** An election officer cannot also be a candidate or a proposer.

## Current state (2026-09-27)

Line numbers are from the tree on 2026-09-27 and will drift.

### How a ballot can be joined back to a voter

`ElectionService.CastVoteAsync` (`GHCAA.Infrastructure/Services/ElectionService.cs:145-171`) writes
`SeatVote`, `Ballot` and `BallotVote` in one serializable transaction.

| Link | Why it joins | Where |
|---|---|---|
| Time | `SeatVote.VotedAt`, `Ballot.IssuedAt` and `BallotVote.CastAt` all take the same `UtcNow` | `ElectionService.cs:145-171` |
| Row order | All three tables use int identity keys, so the Nth `SeatVote` for a seat is the Nth `Ballot` | `ElectionConfigurations.cs` |
| Transaction id | Rows written in one transaction share a Postgres `xmin` value | Postgres storage, any direct DB read |
| Audit log | Each vote logs "User X performed POST /api/elections/{id}/vote" with a timestamp, once per seat | `AuditLogMiddleware.cs:37-46` |
| Client serial | `SerialNumber = request.SerialNumber ?? ...` lets the client choose the ballot serial and then look it up | `ElectionService.cs:157` |

Any one of these is enough. The doc comment on `ElectionsController.Vote`
(`ElectionsController.cs:108`) says the vote is recorded "without linking the vote to the voter".
The existing tests only check that `BallotVote` has no member column.

### Integrity gaps

- `CountAsync` (`ElectionService.cs:173`) has no phase check, so an admin can see the running tally
  during polling.
- `CountAsync` elects only the top candidate and ignores `ElectionSeat.SeatCount`. A tie elects no
  one, with no rule for what happens next (`ElectionService.cs:177`).
- `SetPhaseAsync(Declared)` (`ElectionService.cs:73-84`) moves the election to Declared without
  writing `ECMember` rows. Only `DeclareAsync` (`ElectionService.cs:184`) writes them.
- `AddCandidateAsync` (`ElectionService.cs:221`) has no phase check and no voter-roll check. It
  records the candidate as their own proposer and seconder and sets the nomination straight to
  Accepted, which skips the scrutiny rule in spec 010.
- `RemoveCandidateAsync` (`ElectionService.cs:250`) works in any phase, including Polling.
- No candidate consent is recorded, and nothing stops an election officer from being a candidate.
- The vote endpoint takes one seat per request, so a lost connection can leave a ballot half cast.
- Voting needs only a normal login, not the OTP step-up the admin election endpoints already use
  (`[RequireStepUp]` on `AdminElectionsController`).

### Voter screen gaps

- The web screen (`GHCAA.Web/src/app/member/election/election.html`) shows "Seat {{seatId}}" and
  the nomination statement, not the candidate name or the position.
- One click casts the vote. There is no review or confirm step and no abstain option.
- The voted state lives only in the page. After a reload the screen offers the vote again and the
  server rejects it.
- There is no receipt of any kind.
- The public results page (`GHCAA.Web/src/app/public/elections/election-results.ts:25-27`) calls
  the admin-only `POST /count`, so members and the public cannot see results.
- The Flutter screen (`GHCAA.Mobile/lib/screens/member/election_screen.dart`) has the same per-seat
  flow and also sends an optional serial number.

### Documents

`ElectionDocumentService` produces the 16 ER forms as skeleton PDFs. ER-23, the count sheet, has no
counts in it.

## Requirements

Priority: P0 must ship before the next live election. P1 is needed for a voter to trust the result.
P2 brings the rules in line with the constitution.

### P0: secrecy and integrity

- **FR-001** The ballot write must not share a transaction, a timestamp or an insertion order with
  the voter's `SeatVote` row that anyone with database access can use. Before polling opens, the
  returning officer makes a key pair in the browser. Only the public key is stored on the election,
  with its SHA-256 fingerprint. The private key downloads to the officer's device as a `.pem` file
  and the server never sees it until the count. `SetPhase(Polling)` is refused without a key, and the
  key cannot be changed once polling has opened. Each vote seals the choices with AES-256-GCM under
  a one-off key, which is itself sealed with RSA-OAEP-3072 (SHA-256). The sealed value is padded to
  512-byte blocks so its length says nothing. It goes in `PendingBallot` with no member, time or
  tracking-code column. The tracking code goes in `BallotReceipt`, a table with no link to any
  ballot. At Counting the officer uploads the key file once. `CountAsync` runs in one serializable
  transaction. It checks that ballots, receipts and voters marked as voted all match, opens every
  sealed ballot, shuffles them once, writes `Ballot` and `BallotVote`, rewrites the receipts in
  shuffled order, and deletes the pending rows. It refuses when fewer than 10 ballots were cast
  (`MinimumBallotsToCount`), since a count that small could name a voter. On Postgres it then runs
  `VACUUM` on the pending table. The key is used for that one request and not stored. Later calls
  return the stored result with no key.
- **FR-002** `Ballot` and `BallotVote` use random GUID keys, not identity columns. They keep no
  time field finer than the election date. `IssuedAt` and `CastAt` are dropped.
- **FR-003** The server makes the ballot serial (the tracking code). The client cannot send one.
  `CastVoteDto.SerialNumber` is removed from the API, web and mobile.
- **FR-004** The audit log records "member voted in election N" once per ballot, with the date
  only. It never records the seat, the time of day or the request body for the vote endpoint.
- **FR-005** A whole ballot, one entry per seat, goes in one request. Each entry is a list of
  chosen nominations. An empty list casts that seat blank, which is the abstain choice in FR-012.
  Every seat with at least one accepted candidate must have an entry. A seat with no accepted
  candidate never reaches the voter's screen, so it may be left out. The server checks it all and
  writes it all, or rejects it all.
- **FR-006** Voting requires the OTP step-up already used for admin election actions.
- **FR-007** `CountAsync` runs only in the Counting phase. The result is stored and a second call
  returns the stored result.
- **FR-008** `SetPhaseAsync` cannot move an election to Declared. Only `DeclareAsync` can, so the
  `ECMember` rows are always written.
- **FR-009** Admin candidate changes follow the nomination rules. `AddCandidateAsync` works only up
  to and including Scrutiny, so an added candidate still goes through scrutiny before the list is
  published. It checks the roll and needs a proposer and a seconder, both eligible voters and
  neither of them the candidate. `RemoveCandidateAsync` works only before CandidateList. After that a candidate leaves only through
  withdrawal, before `WithdrawalClosesOn`.
- **FR-010** The `ElectionsController.Vote` doc comment and spec 010 stop claiming more secrecy
  than the code gives, until FR-001 to FR-004 ship.

### P1: cast as intended and counted as cast

- **FR-011** The vote screen shows each seat by position title and each candidate by name, photo
  and statement.
- **FR-012** Each seat has an abstain choice. An abstention is stored as a ballot with no
  `BallotVote` row for that seat, and counted in the result.
- **FR-013** The voter fills the whole ballot, sees a review page with every choice, and confirms
  once. Only the confirm sends the request.
- **FR-014** The member election endpoint returns whether this member has voted, taken from
  `SeatVote`. The screen uses it after a reload and on a second device.
- **FR-015** After a successful vote the confirmation screen offers a print or save of the receipt.
  See "Receipt and print standard" below.
- **FR-016** After polling closes, the list of tracking codes in the count is published, sorted,
  with no choices beside them. A voter can check their code is on it.
- **FR-017** A public endpoint returns the declared result: per seat, every accepted candidate
  with their votes (zero included), abstentions, spoiled ballots and turnout against the frozen
  roll. The public results page uses it instead of `POST /count`.

### P2: election rules

- **FR-018** A seat with `SeatCount` above 1 lets the voter pick up to `SeatCount` candidates, and
  the top `SeatCount` are elected.
- **FR-019** A tie for the last place follows a rule stored on the election. The admin picks it when
  setting up the election, from drawing lots, a run-off, or the chair's casting vote, and each
  option shows a one-line description. Until the rule is applied, no candidate in the tie is
  marked elected. See decision D2.
- **FR-020** A candidate may ask for a recount within a set window after the count. The recount
  uses ER-26 (recount request) and ER-28 (recount report) and replaces the stored result only when
  an officer confirms it.
- **FR-021** A nomination is not accepted without the candidate's consent (ER-10), recorded with
  the time.
- **FR-022** An election officer cannot be a candidate, proposer or seconder in the same election.
  The service rejects it on both paths.
- **FR-023** A spoiled ballot (`IsSpoiled`) is counted and shown in the result but gives no vote.
  The rule for what makes a ballot spoiled is written down with FR-005.
- **FR-024** A seat with one accepted candidate after withdrawal closes can be declared unopposed,
  if the constitution allows it, with no poll for that seat.
- **FR-025** The ER forms are filled from stored records. ER-23 carries the per-candidate counts.

## Officials, personas and access (raised 2026-09-28, waiting for plan approval)

The Search Committee, Commission, officials, Observers and Appeal Tribunal are recorded in the app.
Any of them can be a member or an appointed non-member. Full analysis, exact files, entities and
tests are in [plan-officials.md](plan-officials.md); this section only states the rules as FRs.

- **FR-026** The step-up request and verify endpoints accept any signed-in user, not admins only,
  so a member can complete the check the vote endpoint asks for. Closes the 37.1w defect: a plain
  member cannot vote today because step-up is admin-only.
- **FR-027** An election persona (post) carries a name, a board group, a permission set, a
  declaration text and a takeover flag. Personas are data SuperAdmin manages, not a fixed list in
  code. Fourteen personas are seeded on boot and can be edited, added to or deactivated but not
  deleted while in use.
- **FR-028** A person is appointed to a persona on one election, member or not. A non-member
  appointee gets a login with no member record attached. The appointment gives no access until the
  appointee accepts it and signs the persona's declaration text.
- **FR-029** Accepting an appointment adds a single `ElectionOfficial` role to the appointee's
  account. What that role can do on a given election comes from the accepted persona's permission
  set on that election, checked on every write. A member appointee keeps their normal member access
  throughout.
- **FR-030** Once a persona with the takeover flag has an accepted appointment on an election, Admin
  loses write access to that election. SuperAdmin always keeps full access. This is configurable and
  off by default for the "keeps control" case.
- **FR-031** Publish, open polling, replace the ballot key, close polling, declare and archive each
  need two different people: the one who asks and a second one, not the requester, who holds the
  Approve permission. The action runs only once approved. Closes 37.1v, where any admin could swap
  the ballot key alone.
- **FR-032** An appointment's access ends on its own after the result is declared, on a configurable
  number of days (default 21: the 7-day appeal window plus 14 for the tribunal), with no scheduled
  job. Archiving the election revokes every appointment at once and deactivates non-member
  appointees who hold no other access.
- **FR-033** The Commission and officials appear on a public board, grouped by persona group, next
  to the election timeline and turnout after polling closes. Members with no persona shown do not
  appear.

## Receipt and print standard

Standard 2, receipt-freeness, decides what the voter may print.

- The receipt shows the election, the date, the tracking code, and the voter's marked ballot:
  each position with the name chosen or "Abstained".
- The marked ballot is built in the browser or app from what the voter just confirmed. The server
  never returns the choices for a tracking code, so the receipt cannot be printed again later or by
  someone else.
- It can be printed only from the confirmation screen, once. Leaving the screen ends it.
- The published tracking-code list (FR-016) proves the ballot was counted. It does not prove the
  choice, so the receipt alone cannot be used to sell a vote.
- Web: a print stylesheet on the confirmation view and `window.print()`. Mobile: a PDF built on the
  device and handed to the share sheet.

Trade-off: a voter who prints the marked ballot can still show the paper to someone. Paper is easy
to fake, and nothing on the server backs up the choices, so it is weak proof. If the committee wants
strict receipt-freeness, print the tracking code only (option B in D1).

## Not adopted

- **End-to-end verifiable cryptography.** Strong, but it needs key ceremonies and trained officers.
  The shuffle in FR-001 plus the code list in FR-016 gives most of the value for an alumni body.
- **Ranked voting.** The constitution uses first past the post per seat.
- **Changing a vote after casting.** Some systems allow a later vote to replace an earlier one to
  beat coercion. It would need the member-to-ballot link that FR-001 removes.

## Decisions

- **D1 Receipt content.** Decided 2026-09-27: option A, the marked ballot plus the tracking code.
  Option B (tracking code only) and option C (no receipt) were not taken.
- **D2 Tie rule.** Decided 2026-09-27: set per election at setup, from drawing lots, a run-off, or
  the chair's casting vote, each with a short description on the setup screen. The admin is
  responsible for choosing the one the constitution names. The Article has not been checked yet.
- **D3 When to start.** Decided 2026-09-27: P0 ships before the next live election. This is a
  project date, not an election setting. The election's own dates (nomination, polling open and
  close) are already set on the create screen. `ScrutinyOn` and `WithdrawalClosesOn` are not on the
  create screen yet and are added with the setup work in Phase 2.

## What the vote record keeps about the request

The turnout row in the activity log already stores the IP address and user agent the request came
from, taken by `ActivityService` from the request. `UseForwardedHeaders` is on, so the IP is the
client's and not Render's proxy. These sit on the turnout record, which names the member, never on
the ballot. The time on that row stays date only (FR-004).

Device details can be parsed from the user agent into the row's `Metadata` field with no schema
change and no new package. Location by IP lookup is not added. It needs a new dependency or an
outside service, and it would send every voter's IP to a third party. It needs the committee's
approval first.

## Known limits

TODO 37.1s closed the database-access gaps the security review of 2026-09-27 found. What is left:

- The server holds the private key in memory for the length of the count request. An attacker who
  controls the running server at that moment could keep it. Nothing is written to disk or the log.
- A lost key file means the election cannot be counted. Nobody can recover it, the system included.
  The admin screen says so before the key is made.
- An election with fewer than 10 ballots cannot be counted in the app. The officers count it by hand
  under the regulations.
- A superuser can still see from `xmin` which voters voted in the same transaction as which sealed
  row, but the row cannot be opened without the key, and after the count the rows are gone and the
  counted ballots are in shuffled order.
- A backup, snapshot or WAL archive taken during polling keeps the sealed rows with their `xmin`.
  Whoever later holds such a backup and the key file can match every voter to their choices, with
  no time limit. The count vacuums the live tables only. The officer must destroy the key file once
  the result is declared and any recount window has closed, and backups from the polling days need
  the same care as the key.
- Any admin can set or replace the key up to the campaign. The key is not yet tied to the member
  who holds the ReturningOfficer role, and voters do not see its fingerprint. An admin who swaps in
  their own key before polling and also reads the database could open ballots during polling. The
  real officer finds out only when the count says the key is wrong. TODO 37.1v tracks the fix.
- A counted ballot keeps the voter's choices for every seat together. With many seats the pattern
  can be unique, so a coercer who can read the table could ask for an odd pattern and look for it.
- Render's access log records the time and IP of each call to the vote endpoint, but not the body.
  The app's own request log line (from `CorrelationIdMiddleware`) is skipped for the vote route.
  `SeatVote.VotedAt` and `VoterRoll.VotedAt` keep the date only. `ActivityLog` ids are still
  sequential, so a SuperAdmin can place the vote audit row between timed rows around it.
- The first migration's `Down` refuses to run while any pending ballot is left. When it does run, a
  ballot row with no vote rows (a fully blank ballot) has nothing to rebuild the old per-seat shape
  from, and is dropped. The rollback prints the count of dropped rows as a notice.
- The sealed-ballot migration (`ElectionSealedBallots`) refuses to run while any pending ballot is
  left, because plain choices cannot be sealed without the officer's key. Its `Down` pairs receipts
  back to ballots at random, since the link is gone by design. It uses `gen_random_uuid()`, which
  needs Postgres 13 or later.

## Acceptance criteria

1. With full database access and the audit log, a test cannot link any `Ballot` to a `SeatVote`
   by time, key order, transaction id or log entry. The test casts at least 20 ballots and checks
   that ballot order does not follow vote order.
2. A client-sent serial is ignored or rejected.
3. `POST /count` during Polling returns 400.
4. `SetPhase(Declared)` returns 400. `DeclareAsync` writes the `ECMember` rows.
5. Candidate add and remove after CandidateList return 400.
6. A ballot with one bad entry writes nothing.
7. After a reload, a member who voted sees "You have voted" and no vote buttons, on web and mobile.
8. The receipt prints from the confirmation screen and cannot be fetched from the server later.
9. The public results show every accepted candidate, including zero votes, plus abstentions,
   spoiled ballots and turnout.
10. A two-place seat elects two members. A tie applies the stored rule.
11. An officer's nomination as a candidate is rejected.
12. Polling does not open without a returning officer key. The count refuses a wrong key and a
    count below 10 ballots, and after it no pending row is left.

## Evidence

Review notes from 2026-09-27. Phase 1 (FR-001 to FR-009) was built on branch
`prepod-election-refactoring` on 2026-09-27, with the multi-place count from FR-018. Tests are in
`GHCAA.Tests/Services/ElectionServiceTests.cs`, `GHCAA.Tests/Controllers/ElectionsControllerTests.cs`,
`GHCAA.Tests/Middleware/AuditLogMiddlewareTests.cs`, `GHCAA.Web/src/app/member/election/election.spec.ts`
and `GHCAA.Mobile/test/election_service_test.dart`. The rest are listed in [plan.md](plan.md).

FR-026 to FR-033 (officials, personas and access) are un-built. They are written up in
[plan-officials.md](plan-officials.md), which the user is reviewing before any of it is coded.
