# Election engine — implementation and post-ship defect fixes

## Scope

This specification covers the persisted online election engine built under `docs/TODO.md`
item 37.1 (`[DONE 2026-09-21]`) and the defect fixes applied to it on 2026-09-24 following
a code-review pass. It documents what the feature is, as built, and the four defects found
after ship plus the fixes that closed them. It is documentation of completed work, not a
plan for new work.

## Goal

The platform shall run committee elections end to end — announcement, nomination, scrutiny,
withdrawal, candidate list, campaign, polling, counting, and declaration — as persisted,
auditable records, replacing the prior model where election results existed only as
hand-typed `ECMember` rows.

## Domain model, as built

- `Election` (`Id`, `Title`, `ECPeriodId`, `Phase`, `AnnouncedOn`, `NominationOpensOn`,
  `NominationClosesOn`, `ScrutinyOn`, `WithdrawalClosesOn`, `PollingOpensOn`,
  `PollingClosesOn`, `DeclaredOn?`, `IsActive`, `CreatedBy`) with the `ElectionPhase` enum
  (`Announced, Nomination, Scrutiny, Withdrawal, CandidateList, Campaign, Polling, Counting,
  Declared, Archived`).
- `ElectionSeat` (`Id`, `ElectionId`, `ECPosition Position`, `SeatCount`).
- `ElectionOfficer` (`Id`, `ElectionId`, `MemberId`, `ElectionRole Role`).
- `VoterRoll` (`Id`, `ElectionId`, `MemberId`, `IsEligible`, `IneligibilityReason?`,
  `FrozenAt`, `VotedAt?`) — frozen by snapshot at a point in time, not computed at poll time,
  using the same Article III Section K membership-type and dues-current rule already
  enforced in `GovernanceService.VoteOnConstitutionAsync`. Freezing is what makes a disputed
  result auditable.
- `Nomination` (`Id`, `ElectionId`, `ElectionSeatId`, `CandidateMemberId`, `ProposerMemberId`,
  `SeconderMemberId`, `Statement`, `PhotoPath?`, `Status`, `SubmittedAt`, `WithdrawnAt?`) with
  `NominationStatus { Submitted, UnderScrutiny, Accepted, Rejected, Withdrawn }`. Proposer and
  seconder must both be on the frozen roll and must not be the candidate — enforced in the
  service, not only the UI.
- `ScrutinyDecision` (`Id`, `NominationId`, `OfficerMemberId`, `Accepted`, `Reason`,
  `DecidedAt`).
- `Ballot` (`Id`, `ElectionId`, `ElectionSeatId`, `SerialNumber`, `IssuedAt`, `IsSpoiled`) and
  `BallotVote` (`Id`, `BallotId`, `NominationId`, `CastAt`) — kept in separate tables with no
  member foreign key on the vote side. The roll records that a member voted; the ballot
  records what was voted; nothing joins the two. That separation is the secret ballot and is
  not retrofittable.
- `SeatVote` (`Id`, `ElectionId`, `ElectionSeatId`, `MemberId`, `VotedAt`) — added 2026-09-24
  as part of the defect fix described below, purely for double-vote enforcement; it carries
  no reference to which candidate was chosen and does not weaken the ballot secrecy above.
- `ElectionResult` (`Id`, `ElectionId`, `ElectionSeatId`, `NominationId`, `VoteCount`,
  `IsElected`, `IsTie`). Declaration writes winners into `ECMember` rows against the
  election's `ECPeriodId`.

## Service and API surface

`IElectionService` / `ElectionService` (`GHCAA.Infrastructure/Services`) owns all election
persistence and business rules. `ElectionsController` at `api/elections` exposes the member-
and officer-facing lifecycle (create, phase advance, seats, officers, roll freeze,
nominations, scrutiny, withdrawal, vote, count, declare, document generation);
`AdminElectionsController` at `api/admin/elections` exposes the admin console's
create/publish/close/candidate-management flow used by the Angular admin UI. Casting is
rejected server-side outside the `Polling` phase.

## Non-functional requirements

- Controllers never query `ApplicationDbContext` directly — all persistence goes through
  `IElectionService`.
- A ballot row must never be joinable back to a member.
- Server-side identity for any officer-privileged action (scrutiny decisions, phase
  transitions) is taken from the authenticated claim, never from client-supplied request
  fields.
- Double-vote prevention must operate per seat, not per election, since one election can
  carry multiple seats a member is entitled to vote for.
- Phase transitions must be idempotent against a repeated request for the same phase.

## Post-ship defects found and fixed (2026-09-24)

A code-review pass on the shipped engine found three critical defects and one major defect.
All four are fixed in this change, verified by `dotnet build` (clean) and
`dotnet test --filter "FullyQualifiedName~ElectionServiceTests"` (4/4), plus a full-suite run
(`dotnet test GHCAA.Tests`, 860/860, no regressions).

1. **Controller bypassing the service layer (critical).** `AdminElectionsController` queried
   `ApplicationDbContext` directly for `Publish`, `Close`, and candidate add/remove, and
   duplicated election-shaping logic (`BuildAdminElectionAsync`, `ToAdminElection`,
   `SeatTitle`) that already existed in `ElectionService`. Fixed by routing all four actions
   through `IElectionService` (`GetAdminElectionAsync`, `AddCandidateAsync`,
   `RemoveCandidateAsync`) and deleting the duplicated controller-side helpers.
2. **Officer-identity spoofing (critical).** `Scrutinise` on `ElectionsController` read the
   officer's member id from the request body (`ScrutinyDto`) instead of the auth claim,
   letting any authenticated caller record a scrutiny decision under a different officer's
   identity. Fixed by reading `officerMemberId` from `CurrentMemberIdRaw()` and removing it
   from the request DTO.
3. **Per-seat double-vote bug (critical).** The double-vote guard was a single
   `VoterRoll.VotedAt` column per `(ElectionId, MemberId)`, so a member entitled to vote for
   two seats in the same election (e.g. President and Vice President) was blocked from
   casting the second vote — a false rejection, not a security gap, but it broke a valid
   multi-seat ballot. Fixed by adding the `SeatVote` table with a unique index on
   `(ElectionId, ElectionSeatId, MemberId)` as the actual compare-and-set target for the
   double-vote check; `VoterRoll.VotedAt` is kept as a first-vote-only informational flag.
   Migration: `GHCAA.Infrastructure/Data/Migrations/PgSql/20260923184652_AddSeatVotes.cs`.
   Regression test: `ElectionServiceTests.CastVoteAsync_AllowsVotingForDifferentSeatsInTheSameElection`.
4. **Phase re-entrancy (major).** `SetPhaseAsync` allowed transitioning into the phase an
   election was already in, which could re-trigger phase-entry side effects a second time.
   Fixed with a same-phase no-op guard.

Folded into the same change: the remaining raw `[Authorize(Roles = "Admin,SuperAdmin")]`
attributes on `ElectionsController` (8 endpoints) were migrated to
`[Authorize(Policy = Policies.AdminOnly)]`, matching the policy convention already used on
`AdminElectionsController` and confirmed behaviorally identical via the policy definition in
`ServiceExtensions.cs`.

## Acceptance criteria

1. No controller in the election feature queries `ApplicationDbContext` directly.
2. A scrutiny decision is always recorded against the authenticated officer's own member id,
   never a client-supplied one.
3. A voter entitled to vote for two seats in one election can cast both votes; a repeated
   vote for the same seat is still rejected.
4. Re-submitting the current phase to `SetPhaseAsync` is a no-op, not a re-trigger.
5. All election authorization checks use `Constants.Policies`, not raw role-list strings.

## Evidence

- `GHCAA.Tests/Services/ElectionServiceTests.cs` — multi-seat voting, same-seat replay
  rejection, roll-freeze snapshot behavior.
- `dotnet build` — clean.
- `dotnet test GHCAA.Tests` — 860/860 passed, 2026-09-24.
- `docs/TODO.md` item 37.1g.
