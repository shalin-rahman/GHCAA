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
  the voter's `SeatVote` row. The vote request writes `SeatVote` and puts the choices in a holding
  table (`PendingBallot`, no member column, no time column). A background job moves pending rows
  into `Ballot` and `BallotVote` in shuffled batches of at least 10, or all of them when polling
  closes. `CountAsync` refuses to run while any pending row is left.
- **FR-002** `Ballot` and `BallotVote` use random GUID keys, not identity columns. They keep no
  time field finer than the election date. `IssuedAt` and `CastAt` are dropped.
- **FR-003** The server makes the ballot serial (the tracking code). The client cannot send one.
  `CastVoteDto.SerialNumber` is removed from the API, web and mobile.
- **FR-004** The audit log records "member voted in election N" once per ballot, with the date
  only. It never records the seat, the time of day or the request body for the vote endpoint.
- **FR-005** A whole ballot, one entry per seat, goes in one request. Each entry is a list of
  chosen nominations, or an abstain flag. The server checks it all and writes it all, or rejects it
  all.
- **FR-006** Voting requires the OTP step-up already used for admin election actions.
- **FR-007** `CountAsync` runs only in the Counting phase. The result is stored and a second call
  returns the stored result.
- **FR-008** `SetPhaseAsync` cannot move an election to Declared. Only `DeclareAsync` can, so the
  `ECMember` rows are always written.
- **FR-009** Admin candidate changes follow the nomination rules. `AddCandidateAsync` works only up
  to CandidateList, checks the roll, needs a real proposer and seconder, and goes through scrutiny.
  `RemoveCandidateAsync` works only up to CandidateList. After that a candidate leaves only through
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
- **FR-019** A tie for the last place follows a rule stored on the election. The rule is set by the
  constitution. See open decision D2.
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

## Open decisions

- **D1 Receipt content.** Option A: marked ballot plus tracking code (recommended). Option B:
  tracking code only. Option C: no receipt.
- **D2 Tie rule.** Drawing lots, a run-off, or the chair's casting vote. It has to match the
  constitution. The Article to cite has not been checked yet.
- **D3 When to start.** P0 has to ship before the next live election. The date of that election is
  not recorded in the repo.

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

## Evidence

Review notes from 2026-09-27. No code has changed yet. Tests to add are listed in
[plan.md](plan.md).
