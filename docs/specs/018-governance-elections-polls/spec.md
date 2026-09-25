# Feature Specification: Governance, Elections and Polls

**Feature Branch**: 018-governance-elections-polls
**Created**: 2026-09-25
**Status**: As-built baseline
**Input**: Reverse-engineered from the implemented code.
**Depends on**: [001-platform-baseline](../001-platform-baseline/spec.md)

## Purpose and scope

Six controllers run Executive Committee (EC)
governance, the persisted election engine, and member/admin polls:

- GovernanceController (api/governance) - public EC and constitution reads, constitution
  amendment voting.
- AdminGovernanceController (api/admin/governance) - EC period and committee roster admin.
- ElectionsController (api/elections) - the full persisted election engine: phases,
  seats, officers, voter roll, nominations, scrutiny, withdrawal, voting, counting,
  declaration, official documents.
- AdminElectionsController (api/admin/elections) - a second, simplified election
  workflow: list and create elections with inline positions, publish, close, add or remove a
  candidate directly.
- PollController (api/polls) - member poll browsing and voting.
- AdminPollController (api/admin/polls) - poll creation, toggling and archiving.

Out of scope: membership status and verification (see
003-alumni-programs-and-verification/spec.md), OTP or step-up mechanics themselves,
notification delivery internals, org-config or branding internals used by PDF generation.

Related specs (context only, not repeated here):
docs/specs/002-workflow-contracts-and-validation/evidence/governance-domain.md and
polls-domain.md, docs/specs/010-election-engine-fixes/spec.md,
docs/specs/011-election-module-redesign/spec.md, and the Governance/elections/roles and
Forums/polls sections of
docs/specs/001-platform-baseline/evidence/implementation-feature-catalog.md.

## User Scenarios & Testing

### User Story 1 - Run a committee election end to end (Priority: P1)

An election officer announces an election, freezes the voter roll, runs nomination,
scrutiny, withdrawal, polling and counting, then declares results, which seats the winners
on the Executive Committee.

**Why this priority**: this is the constitutional mechanism by which the EC itself is
formed; without it no other governance function has a legitimate committee to answer to.

**Independent Test**: create an election, freeze the voter roll, advance through each phase
in order, submit and accept a nomination, cast a vote, count and declare, then verify an
ECMember row exists for the winning candidate.

**Acceptance Scenarios**:

1. **Given** an election in Announced phase, **When** an admin sets phase to Scrutiny
   (skipping Nomination), **Then** the request is rejected and the phase does not change.
2. **Given** an election with no voter roll frozen, **When** an admin sets phase to
   Polling, **Then** the request is rejected.
3. **Given** a Counting transition requested before PollingClosesOn, **When** an admin
   requests it, **Then** the request is rejected.
4. **Given** an election in Counting with results computed, **When** an admin declares it,
   **Then** the phase becomes Declared and one ECMember row per elected seat is created.

### User Story 2 - Cast a secret ballot (Priority: P1)

An eligible member votes once per seat during the polling window without their identity
being linkable to their ballot choice.

**Why this priority**: ballot secrecy and one-member-one-vote are the core integrity
guarantees of the election; a failure here invalidates the result.

**Independent Test**: freeze a voter roll, accept a nomination, cast a vote as an eligible
member, then attempt a second vote for the same seat and confirm it is rejected; confirm the
resulting vote row carries no member identity.

**Acceptance Scenarios**:

1. **Given** an eligible voter and an accepted nomination for the requested seat, **When**
   the voter votes during Polling inside the polling window, **Then** the vote is recorded
   with the choice stored separately from the identity of the voter.
2. **Given** the same voter has already voted for that seat, **When** they vote again,
   **Then** the request is rejected.
3. **Given** a member not on the frozen, eligible voter roll, **When** they vote, **Then**
   the request is rejected.

### User Story 3 - Administer EC periods and the committee roster (Priority: P2)

An admin creates a non-overlapping EC period, activates it, and assigns or removes members
from committee positions, with an audited hard-delete path for a wrongly added record.

**Why this priority**: this is how the platform canonical answer to who currently holds
which EC role is maintained outside of election declarations.

**Independent Test**: create two overlapping periods and confirm the second is rejected;
activate a period covering today and confirm any previously active period is deactivated;
assign then remove a member and confirm EndDate is stamped rather than the row deleted.

**Acceptance Scenarios**:

1. **Given** an existing EC period with no end date, **When** an admin creates a second
   period whose start date is before that period ends, **Then** creation is rejected.
2. **Given** a period whose date range does not cover today, **When** an admin tries to
   activate it, **Then** the request is rejected.
3. **Given** an inactive member, **When** an admin assigns them to an EC role, **Then** the
   assignment is rejected.
4. **Given** an EC member record that should never have existed, **When** an admin hard
   deletes it under step-up authentication, **Then** the row is soft-archived, not removed.

### User Story 4 - Vote on a constitution amendment (Priority: P2)

A Founding, Executive or General member reads the active constitution and casts one
for-or-against vote on it; other membership tiers can read but not vote.

**Why this priority**: constitutional ratification is a distinct governance act from EC
elections and polls, gated by a fixed voting-tier rule.

**Independent Test**: vote as an Associate member and confirm rejection; vote as a General
member and confirm the vote is recorded, then confirm a second vote by the same member is
rejected.

**Acceptance Scenarios**:

1. **Given** an Associate, Honorary or Advisory member, **When** they vote on the active
   constitution, **Then** the vote is rejected.
2. **Given** a Founding, Executive or General member who has not yet voted, **When** they
   vote, **Then** the vote is recorded.
3. **Given** the same member votes a second time, **When** they submit it, **Then** the
   request is rejected.
4. **Given** a constitution version that is not the active one, **When** anyone votes on it,
   **Then** the request is rejected.

### User Story 5 - Run and take part in a member poll (Priority: P3)

An admin creates a single-choice or multiple-choice poll; members vote once, and results and
percentages are visible once they have voted.

**Why this priority**: lighter-weight, non-constitutional sentiment gathering; useful but not
load-bearing for governance legitimacy the way elections and amendments are.

**Independent Test**: create a poll with two options, vote as a member, attempt a second vote
and confirm rejection, then confirm the member vote status and result percentages appear.

**Acceptance Scenarios**:

1. **Given** an active, non-expired poll, **When** a member submits an empty option list,
   **Then** the vote is rejected.
2. **Given** a single-choice poll, **When** a member submits more than one option, **Then**
   the vote is rejected.
3. **Given** a member who already voted, **When** they vote again, **Then** the vote is
   rejected.
4. **Given** an admin toggles a poll inactive or archives it, **When** a member requests
   active polls, **Then** that poll no longer appears.

### Edge Cases

- Phase transitions only ever move forward one step at a time.
- Freezing an already-frozen voter roll is a no-op that returns the existing count.
- A nomination candidate, proposer and seconder must be three distinct eligible voters.
- Only the nominated candidate can withdraw their own accepted nomination, and only during
  Withdrawal.
- AdminElectionsController.AddCandidate inserts a nomination as already Accepted
  (self-proposed and self-seconded), bypassing scrutiny, a different path from
  ElectionsController.Nominate plus Scrutinise.
- Removing a candidate is refused once votes already exist for that nomination.
- Requesting a document for an unsupported form code returns 404.
- An unrecognised position title in AdminElectionsController.Create returns 400 rather
  than creating an unlabelled seat.

## Requirements

### Functional Requirements

- FR-001: The system shall return the active EC period and its current committee members
  to an anonymous caller. [code]
- FR-002: The system shall return all EC periods, most recent first, to an anonymous
  caller. [code]
- FR-003: The system shall return the currently active constitution version to an
  anonymous caller, or a 404 if none is active. [code+test]
- FR-004: The system shall return the full constitution version history to an anonymous
  caller. [code]
- FR-005: The system shall let a Founding, Executive or General member cast exactly one
  for-or-against vote on the active constitution version. [code+test]
- FR-006: The admin shall create or update an EC period only when its date range does not
  overlap an existing period. [code+test]
- FR-007: The admin shall activate an EC period only when its date range covers the
  current UTC date, and the system shall deactivate any other active period in the same
  operation. [code+test]
- FR-008: The admin shall retrieve the current committee members, meaning ECMember rows
  with no EndDate, of a given EC period. [code]
- FR-009: The admin shall assign an active member to an EC position for a period.
  [code+test]
- FR-010: The admin shall end a committee member term by setting EndDate rather than
  deleting the record. [code+test]
- FR-011: The admin, under step-up authentication, shall soft-archive an ECMember record
  and record the deleting admin and timestamp. [code+test]
- FR-012: The admin, under step-up authentication, shall create an election with a
  nomination and polling timetable. [code]
- FR-013: The system shall return an election summary by id to any caller, returning 404
  if the election does not exist. [code+test]
- FR-014: The system shall return the single election currently in an active phase to any
  caller, or 204 if none is active. [code+test]
- FR-015: The admin, under step-up authentication, shall advance an election phase by
  exactly one step, subject to phase-specific preconditions. [code]
- FR-016: The admin, under step-up authentication, shall add a seat with a position and
  seat count to an election only while it is in the Announced phase. [code]
- FR-017: The admin, under step-up authentication, shall assign an active member as an
  election officer, rejecting a duplicate role assignment for that election. [code]
- FR-018: The admin, under step-up authentication, shall freeze a voter-roll snapshot of
  eligible members before nominations open. [code+test]
- FR-019: The system shall return an election nominations list, ordered by seat, to an
  anonymous caller. [code]
- FR-020: A member shall submit a nomination during the Nomination phase and before
  NominationClosesOn, naming a distinct candidate, proposer and seconder who are all eligible
  voters. [code]
- FR-021: An election officer holding ReturningOfficer, AssistantReturningOfficer or
  Scrutineer role shall record an accept-or-reject scrutiny decision on a nomination only
  during the Scrutiny phase. [code]
- FR-022: The nominated candidate shall withdraw their own accepted nomination only
  during the Withdrawal phase. [code]
- FR-023: An eligible voter shall cast exactly one secret ballot per seat during the
  Polling phase and within the polling window. [code+test]
- FR-024: The admin, under step-up authentication, shall count ballots per seat for
  accepted nominations and persist the results. [code]
- FR-025: The admin, under step-up authentication, shall declare an election results once
  it is in the Counting phase, and the system shall create one ECMember record per elected
  candidate. [code]
- FR-026: The system shall generate an official election PDF with a QR verification link
  for a supported form code, or return a 404 for an unsupported code. [code+test]
- FR-027: The admin shall list all elections created through the admin workflow. [code]
- FR-028: The admin shall create an election together with inline named positions in one
  request. [code]
- FR-029: The admin shall publish an election by advancing its phase to Nomination.
  [code]
- FR-030: The admin shall close an election by advancing its phase to Counting. [code]
- FR-031: The admin shall add a candidate directly to a seat as an accepted nomination,
  rejecting a duplicate member-and-seat pair. [code]
- FR-032: The admin shall remove a candidate, rejecting removal once votes exist for that
  nomination. [code]
- FR-033: A member shall list active, non-expired, non-archived polls with their own vote
  status and result percentages. [code+test]
- FR-034: A member shall fetch a single non-archived poll by id. [code+test]
- FR-035: A member shall vote on a poll exactly once, consistent with its single-choice or
  multiple-choice setting and using option ids that belong to the poll. [code+test]
- FR-036: The admin shall list all non-archived polls, active or not. [code+test]
- FR-037: The admin shall create a poll with its options. [code+test]
- FR-038: The admin shall toggle a poll active flag. [code+test]
- FR-039: The admin shall soft-delete a poll, archiving it without removing the row.
  [code+test]
- FR-040: The admin shall list all EC periods. [code]

### Key Entities

- ECPeriod and ECMember - a governance term and the position-holders within it; ECMember
  carries soft-delete fields (IsArchived, DeletedAt, DeletedByAdminId) alongside the
  ordinary EndDate term-end field.
- Constitution and AmendmentVote - versioned constitution text with one active version at a
  time, and one for-or-against vote per voting-tier member per version.
- Election, ElectionSeat, ElectionOfficer, VoterRoll - an election timetable and
  ElectionPhase, its seats (ECPosition plus seat count), assigned officers, and the frozen
  eligible-voter snapshot.
- Nomination and ScrutinyDecision - a candidacy for a seat and the officer decision that
  accepts or rejects it.
- Ballot, BallotVote, SeatVote - the secrecy split: SeatVote proves a member voted for a
  seat (identity, no choice), BallotVote records the choice against a Ballot (choice, no
  identity).
- ElectionResult - per-seat, per-nomination vote counts with elected and tie flags.
- Poll, PollOption, PollVote - a member poll, its options, and one or more (per
  AllowMultipleChoice) vote rows per member.

## Evidence

| FR | API (verb + route) | Service method | Web (file) | Mobile (file) | Test (file::test name) |
|----|---------------------|-----------------|------------|----------------|--------------------------|
| FR-001 | GET api/governance/ec/current, api/governance/current | GovernanceService.GetActivePeriodAsync / GetCommitteeMembersAsync | GHCAA.Web/src/app/common/governance/governance.ts | GHCAA.Mobile/lib/core/api/governance_api.dart | none found |
| FR-002 | GET api/governance/ec/history | GovernanceService.GetAllPeriodsAsync | GHCAA.Web/src/app/common/governance/governance.ts | GHCAA.Mobile/lib/core/api/governance_api.dart | none found |
| FR-003 | GET api/governance/constitution | GovernanceService.GetActiveConstitutionAsync | GHCAA.Web/src/app/common/governance/governance.ts | GHCAA.Mobile/lib/screens/member/governance_screen.dart | GHCAA.Tests/Services/GovernanceServiceTests.cs::GetActiveConstitutionAsync_ReturnsLatestByEffectiveDate_NotInsertionOrder |
| FR-004 | GET api/governance/constitution/history | GovernanceService.GetConstitutionHistoryAsync | GHCAA.Web/src/app/common/governance/governance.ts | GHCAA.Mobile/lib/screens/member/governance_screen.dart | none found |
| FR-005 | POST api/governance/constitution/{id}/vote | GovernanceService.VoteOnConstitutionAsync | GHCAA.Web/src/app/common/governance/governance.ts | GHCAA.Mobile/lib/screens/member/governance_screen.dart | GHCAA.Tests/Services/GovernanceServiceTests.cs::VoteOnConstitutionAsync_RefusesNonVotingTierMember, VoteOnConstitutionAsync_AcceptsVotingTierMember |
| FR-006 | POST/PUT api/admin/governance/periods | GovernanceService.CreatePeriodAsync / UpdatePeriodAsync | GHCAA.Web/src/app/admin/governance/admin-governance.ts | GHCAA.Mobile/lib/screens/admin/governance_registry_screen.dart | GHCAA.Tests/Services/GovernanceServiceTests.cs::CreatePeriodAsync_ShouldAddPeriod, UpdatePeriodAsync_ShouldModifyFieldsAndForceUtc |
| FR-007 | POST api/admin/governance/periods/{id}/activate | GovernanceService.ActivatePeriodAsync | GHCAA.Web/src/app/admin/governance/admin-governance.ts | GHCAA.Mobile/lib/screens/admin/governance_registry_screen.dart | GHCAA.Tests/Services/GovernanceServiceTests.cs::ActivatePeriodAsync_ShouldDeactivateOthers |
| FR-008 | GET api/admin/governance/periods/{id}/members | GovernanceService.GetCommitteeMembersAsync | GHCAA.Web/src/app/admin/governance/admin-governance.ts | GHCAA.Mobile/lib/screens/admin/governance_registry_screen.dart | none found |
| FR-009 | POST api/admin/governance/periods/{id}/members | GovernanceService.AssignMemberToRoleAsync | GHCAA.Web/src/app/admin/governance/admin-governance.ts | GHCAA.Mobile/lib/screens/admin/governance_registry_screen.dart | GHCAA.Tests/Services/GovernanceServiceTests.cs::AssignMemberToRoleAsync_ShouldCreateRecordAndSyncProfile, AssignMemberToRoleAsync_NotifiesMember_WhenOptedIn |
| FR-010 | DELETE api/admin/governance/members/{ecMemberId} | GovernanceService.RemoveMemberFromCommitteeAsync | GHCAA.Web/src/app/admin/governance/admin-governance.ts | GHCAA.Mobile/lib/screens/admin/governance_registry_screen.dart | GHCAA.Tests/Services/GovernanceServiceTests.cs::RemoveMemberFromCommitteeAsync_ShouldEndRecordAndResetProfile, RemoveMemberFromCommitteeAsync_NotifiesMember_WhenOptedIn |
| FR-011 | DELETE api/admin/governance/members/{ecMemberId}/hard-delete | GovernanceService.DeleteECMemberAsync | GHCAA.Web/src/app/admin/governance/admin-governance.ts | none found | GHCAA.Tests/Services/GovernanceServiceTests.cs::DeleteECMemberAsync_SoftDeletesWithActorAndTimestamp, DeleteECMemberAsync_NotifiesMember_WhenOptedIn, DeleteECMemberAsync_ReturnsFalse_WhenAlreadyDeleted |
| FR-012 | POST api/elections | ElectionService.CreateAsync | none found | GHCAA.Mobile/lib/features/elections/election_service.dart (posts to admin/elections route, see FR-028) | none found |
| FR-013 | GET api/elections/{id} | ElectionService.GetAsync | GHCAA.Web/src/app/core/services/elections.service.ts, GHCAA.Web/src/app/public/elections/elections.ts | GHCAA.Mobile/lib/features/elections/election_service.dart | none found |
| FR-014 | GET api/elections/current | ElectionService.GetCurrentAsync | GHCAA.Web/src/app/core/services/elections.service.ts | GHCAA.Mobile/lib/features/elections/election_service.dart | GHCAA.Tests/Services/ElectionServiceTests.cs::GetCurrentAsync_ReturnsElectionInAnActivePhase, GetCurrentAsync_ReturnsNullWhenNoElectionIsActive |
| FR-015 | POST api/elections/{id}/phase | ElectionService.SetPhaseAsync | GHCAA.Web/src/app/core/services/elections.service.ts (setPhase, no caller in admin-elections.ts) | GHCAA.Mobile/lib/features/elections/election_service.dart | none found |
| FR-016 | POST api/elections/{id}/seats | ElectionService.AddSeatAsync | none found | none found | none found |
| FR-017 | POST api/elections/{id}/officers | ElectionService.AssignOfficerAsync | none found | none found | none found |
| FR-018 | POST api/elections/{id}/voter-roll/freeze | ElectionService.FreezeVoterRollAsync | GHCAA.Web/src/app/core/services/elections.service.ts (freezeVoterRoll, no caller in admin-elections.ts) | GHCAA.Mobile/lib/features/elections/election_service.dart | GHCAA.Tests/Services/ElectionServiceTests.cs::FreezeVoterRollAsync_UsesMembershipAndDuesSnapshot |
| FR-019 | GET api/elections/{id}/nominations | ElectionService.GetNominationsAsync | GHCAA.Web/src/app/member/election/election.ts, GHCAA.Web/src/app/admin/elections/admin-elections.ts | GHCAA.Mobile/lib/features/elections/election_service.dart | none found |
| FR-020 | POST api/elections/{id}/nominations | ElectionService.SubmitNominationAsync | none found | GHCAA.Mobile/lib/features/elections/election_service.dart | none found |
| FR-021 | POST api/elections/nominations/{nominationId}/scrutiny | ElectionService.DecideNominationAsync | GHCAA.Web/src/app/admin/elections/admin-elections.ts | none found | none found |
| FR-022 | POST api/elections/nominations/{nominationId}/withdraw | ElectionService.WithdrawNominationAsync | GHCAA.Web/src/app/core/services/elections.service.ts (withdraw, no caller found) | GHCAA.Mobile/lib/features/elections/election_service.dart | none found |
| FR-023 | POST api/elections/{id}/vote | ElectionService.CastVoteAsync | GHCAA.Web/src/app/member/election/election.ts | GHCAA.Mobile/lib/features/elections/election_service.dart | GHCAA.Tests/Services/ElectionServiceTests.cs::CastVoteAsync_RecordsVoteWithoutMemberOnBallotVote, CastVoteAsync_RejectsReplayAfterTheFirstVote, CastVoteAsync_AllowsVotingForDifferentSeatsInTheSameElection |
| FR-024 | POST api/elections/{id}/count | ElectionService.CountAsync | GHCAA.Web/src/app/core/services/elections.service.ts (count, no caller found) | GHCAA.Mobile/lib/features/elections/election_service.dart | none found |
| FR-025 | POST api/elections/{id}/declare | ElectionService.DeclareAsync | GHCAA.Web/src/app/core/services/elections.service.ts (declare, no caller found) | GHCAA.Mobile/lib/features/elections/election_service.dart | none found |
| FR-026 | GET api/elections/{id}/documents/{formCode} | ElectionDocumentService.GeneratePdfAsync | GHCAA.Web/src/app/public/elections/election-docs.ts | GHCAA.Mobile/lib/features/elections/election_service.dart | GHCAA.Web/src/app/public/elections/election-docs.spec.ts |
| FR-027 | GET api/admin/elections | ElectionService.ListAdminElectionsAsync | GHCAA.Web/src/app/admin/elections/admin-elections.ts | GHCAA.Mobile/lib/screens/admin/election_management_screen.dart | none found |
| FR-028 | POST api/admin/elections | ElectionService.CreateAsync + AddSeatAsync (via AdminElectionsController.Create) | GHCAA.Web/src/app/admin/elections/admin-elections.ts | GHCAA.Mobile/lib/features/elections/election_service.dart | none found |
| FR-029 | POST api/admin/elections/{id}/publish | ElectionService.SetPhaseAsync (via AdminElectionsController.Publish) | GHCAA.Web/src/app/admin/elections/admin-elections.ts | none found | none found |
| FR-030 | POST api/admin/elections/{id}/close | ElectionService.SetPhaseAsync (via AdminElectionsController.Close) | GHCAA.Web/src/app/admin/elections/admin-elections.ts | none found | none found |
| FR-031 | POST api/admin/elections/{id}/candidates | ElectionService.AddCandidateAsync | GHCAA.Web/src/app/admin/elections/admin-elections.ts | none found | none found |
| FR-032 | DELETE api/admin/elections/{id}/candidates/{candidateId} | ElectionService.RemoveCandidateAsync | GHCAA.Web/src/app/admin/elections/admin-elections.ts | none found | none found |
| FR-033 | GET api/polls/active | PollService.GetActivePollsAsync | GHCAA.Web/src/app/member/polls/polls.component.ts, GHCAA.Web/src/app/core/services/poll.service.ts | GHCAA.Mobile/lib/features/polls/poll_service.dart, GHCAA.Mobile/lib/features/polls/polls_screen.dart | GHCAA.Tests/Services/PollServiceTests.cs::GetActivePollsAsync_ShouldReturnPollsWithCorrectPercentages, GetActivePollsAsync_ExcludesInactiveExpiredAndArchivedPolls, GetActivePollsAsync_ReturnsZeroPercentagesWithoutVotes; GHCAA.Tests/Controllers/PollControllerTests.cs::GetActivePolls_ReturnsServiceResultForClaimMember |
| FR-034 | GET api/polls/{id} | PollService.GetPollByIdAsync | GHCAA.Web/src/app/core/services/poll.service.ts | GHCAA.Mobile/lib/features/polls/poll_service.dart | GHCAA.Tests/Services/PollServiceTests.cs::GetPollByIdAsync_ReturnsPollAndSelectedOptionsForVotingMember, GetPollByIdAsync_ReturnsNullForMissingOrArchivedPoll; GHCAA.Tests/Controllers/PollControllerTests.cs::GetPollById_MapsServiceResultToOkOrNotFound |
| FR-035 | POST api/polls/{id}/vote | PollService.VoteAsync | GHCAA.Web/src/app/member/polls/polls.component.ts | GHCAA.Mobile/lib/features/polls/polls_screen.dart | GHCAA.Tests/Services/PollServiceTests.cs::VoteAsync_ShouldRecordVoteAndPreventDuplicate, VoteAsync_RejectsNullEmptyMultipleAndForeignChoices, VoteAsync_AllowsMultipleOwnedChoicesAndCountsOneParticipant, VoteAsync_RejectsMissingInactiveArchivedAndExpiredPolls; GHCAA.Tests/Controllers/PollControllerTests.cs::Vote_MapsAcceptedAndRejectedVote |
| FR-036 | GET api/admin/polls | PollService.GetAllPollsAsync | GHCAA.Web/src/app/admin/polls/polls.component.ts, GHCAA.Web/src/app/core/services/admin-poll.service.ts | none found | GHCAA.Tests/Services/PollServiceTests.cs::GetAllPollsAsync_ReturnsInactiveAndExpiredButNotArchivedPolls; GHCAA.Tests/Controllers/AdminPollControllerTests.cs::GetAllPolls_ReturnsServiceResults |
| FR-037 | POST api/admin/polls | PollService.CreatePollAsync | GHCAA.Web/src/app/admin/polls/polls.component.ts | none found | GHCAA.Tests/Services/PollServiceTests.cs::CreatePollAsync_ShouldSavePollWithOptions; GHCAA.Tests/Controllers/AdminPollControllerTests.cs::CreatePoll_UsesTheMemberClaimAndReturnsCreatedLocation |
| FR-038 | PUT api/admin/polls/{id}/toggle | PollService.TogglePollStatusAsync | GHCAA.Web/src/app/admin/polls/polls.component.ts | none found | GHCAA.Tests/Services/PollServiceTests.cs::ToggleAndDeletePollAsync_MapExistingAndMissingRecords; GHCAA.Tests/Controllers/AdminPollControllerTests.cs::ToggleStatus_ReturnsOk_WhenPollExists, ToggleStatus_ReturnsNotFound_WhenPollDoesNotExist |
| FR-039 | DELETE api/admin/polls/{id} | PollService.DeletePollAsync | GHCAA.Web/src/app/admin/polls/polls.component.ts | none found | GHCAA.Tests/Services/PollServiceTests.cs::DeletePollAsync_HidesDeletedPollFromReadMethods, ToggleAndDeletePollAsync_MapExistingAndMissingRecords; GHCAA.Tests/Controllers/AdminPollControllerTests.cs::DeletePoll_MapsServiceOutcomeToTheHttpResult |
| FR-040 | GET api/admin/governance/periods | GovernanceService.GetAllPeriodsAsync | GHCAA.Web/src/app/admin/governance/admin-governance.ts | GHCAA.Mobile/lib/screens/admin/governance_registry_screen.dart | none found |

## Gaps

- Spec 011 redesign only partly shipped: the Angular admin UI (admin-elections.ts) and the
  Flutter admin screens drive two different, non-overlapping subsets of the election
  workflow. Web uses the simplified AdminElectionsController path (FR-027 through FR-032)
  and never calls SetPhase, AddSeat, AssignOfficer, FreezeVoterRoll, Count, Declare, or the
  document endpoint. Mobile drives the ElectionsController phase, nomination, vote, count,
  declare and document flow but has no call to AssignOfficer, AddSeat, or the
  AdminElectionsController publish, close or candidates endpoints. Neither client exercises
  the whole engine end to end.
- FR-012, FR-016, FR-017, FR-019 (web has no submit UI for FR-020), FR-021 (mobile has none),
  FR-022, FR-024, FR-025 have no automated test at any layer. The phase-advance, seat,
  officer, nomination-submit, scrutiny, withdraw, count and declare paths of
  ElectionsController and ElectionService are effectively verified only by the six
  ElectionServiceTests covering FreezeVoterRollAsync, GetCurrentAsync and CastVoteAsync.
  AddCandidateAsync and RemoveCandidateAsync (FR-031, FR-032) and the whole
  AdminElectionsController also have no tests. GovernanceController, AdminGovernanceController,
  ElectionsController and AdminElectionsController have no controller-level tests at all;
  only the underlying service logic is exercised.
- The GovernanceServiceTests test name AssignMemberToRoleAsync_ShouldCreateRecordAndSyncProfile
  implies a member-profile sync side effect, but the current AssignMemberToRoleAsync method
  body only adds the ECMember row and optionally notifies; no Member-record mutation is
  visible in GovernanceService.cs.
  [NEEDS CLARIFICATION: where does the profile sync named in that test happen, if anywhere?]
- CreateAdminElectionRequest.CreatedBy is never read by AdminElectionsController.Create; the
  controller re-derives the member id from the auth claim instead, so that request field is
  dead input.
- RequireStepUp is applied action-by-action on ElectionsController but once at the class
  level on AdminElectionsController; the same security requirement is placed inconsistently,
  which risks a new ElectionsController action shipping without it.
  [NEEDS CLARIFICATION: is AdminElectionsController.AddCandidate, which bypasses scrutiny by
  design, an intended admin override, or should it also write a ScrutinyDecision record for
  audit parity with ElectionsController.Nominate plus Scrutinise?]

## Enhancements: modularisation and reusability

### Reuse across layers

- ENH-001 (P2): cast one vote per member, guard against a concurrent duplicate is
  implemented three separate times with three different safety levels:
  GHCAA.Infrastructure/Services/ElectionService.cs CastVoteAsync (Serializable transaction
  over SeatVote, Ballot and BallotVote), GHCAA.Infrastructure/Services/PollService.cs
  VoteAsync (Serializable transaction over PollVote), and
  GHCAA.Infrastructure/Services/GovernanceService.cs VoteOnConstitutionAsync (a plain
  AnyAsync already-voted check with no transaction). The unique index on ConstitutionId and
  MemberId in GHCAA.Infrastructure/Data/Configurations/AmendmentVoteConfiguration.cs:23 still
  blocks a second row, so two concurrent votes cannot both count. The loser gets an unhandled
  DbUpdateException instead of the "already voted" reply. A shared single-vote-per-member
  helper would give all three the same behaviour.
- ENH-002 (P3): SeatTitle(ECPosition) is duplicated verbatim as a private static method in
  both GHCAA.Infrastructure/Services/ElectionService.cs and
  GHCAA.API/Controllers/AdminElectionsController.cs. One copy would remove the risk of the
  two lists drifting apart when a position is added.
- ENH-003 (P3): the EC position display-order list is duplicated within
  GHCAA.Web/src/app/common/governance/governance.ts, once in loadCommittee and again in
  isBoardMember and getPositionName. A single exported constant would serve both.

### Entity-based module shape

- ENH-004 (P2): election and governance are coupled with no shared interface boundary.
  ElectionService.DeclareAsync writes ECMember rows directly against the database context
  rather than calling IGovernanceService.AssignMemberToRoleAsync, so the rule for how a
  member becomes an EC member now has two independent code paths (declaration versus admin
  assignment) that could diverge. Separately, AdminElectionsController.ParsePosition
  re-implements free-text position matching that only exists because AdminElectionsController
  takes plain-string Positions instead of typed ECPosition values the way
  ElectionsController.AddSeat does, a module boundary leaking a string-matching concern into
  the controller layer.
- ENH-005 (P3): Nomination.PhotoPath is a raw string set directly by the caller, bypassing
  the moderated FileUploadType and FileUploadStatus upload pipeline that other domains
  (gallery, member photo, payment proof) route through. A generic moderated-upload module
  used domain-wide would let a candidate photo go through the same approval path as any
  other member-submitted image instead of an unmoderated path string.

### Existing reusable components

- ENH-006 (P2): no shared vote or ballot Angular component or Flutter widget exists under
  GHCAA.Web/src/app/common, GHCAA.Web/src/app/core, or GHCAA.Mobile/lib/core.
  GHCAA.Web/src/app/member/election/election.ts (election vote),
  GHCAA.Web/src/app/member/polls/polls.component.ts (poll vote), and
  GHCAA.Web/src/app/common/governance/governance.ts (castVote, constitution vote) each
  hand-roll their own selection state, submit handler and already-voted display instead of
  sharing one option-list, vote-once component. By contrast, this domain admin screens
  (admin-elections.ts, admin/polls/polls.component.ts) do reuse the existing
  common/confirm-dialog, common/search-bar, common/page-header and common/logo-spinner
  components; the gap is specific to the three member-facing vote flows.

### Hard-coded behaviour that should be configuration

- ENH-007 (P1): the voting-member rule (Founding, Executive, General membership types) is
  hardcoded as an inline enum list twice: GHCAA.Infrastructure/Services/ElectionService.cs
  FreezeVoterRollAsync (election voter-roll eligibility) and
  GHCAA.Infrastructure/Services/GovernanceService.cs VoteOnConstitutionAsync (constitution
  voting-tier check, commented as Article III Section K). Both encode the same constitutional
  rule; a rule change needs both call sites found and updated by hand rather than one
  lookup or config value.
- ENH-008 (P2): the 16 supported election document form codes and their titles and summaries
  are hardcoded as a HashSet and two switch expressions in
  GHCAA.Infrastructure/Services/ElectionDocumentService.cs. Adding, renaming or retiring a
  form requires a code change and redeploy rather than a config or lookup update.
- ENH-009 (P2): ECPosition display names (SeatTitle, duplicated per ENH-002) and the
  free-text synonym matching in AdminElectionsController.ParsePosition (for example matching
  Media and Sports, or Information and Technology substrings) hardcode the EC position names
  and structure in code rather than an OrgConfig or lookup table, relevant given the
  documented white-labelling goal of making organisation-specific structures configurable
  rather than fixed.

## Success Criteria

- SC-001: every ElectionPhase transition enforced by SetPhaseAsync moves exactly one step
  forward and only when its documented precondition holds.
- SC-002: no BallotVote row is ever joinable to a MemberId; ballot secrecy is structural, not
  merely policy.
- SC-003: no member holds two AmendmentVote rows for the same ConstitutionId, two PollVote
  participations for a single-choice poll, or two SeatVote rows for the same election seat.
- SC-004: every hard-delete of an ECMember record leaves DeletedAt and DeletedByAdminId
  populated and the row retrievable, never physically removed.
- SC-005: at most one ECPeriod has IsActive true at any time.

## Assumptions

- Member throughout this spec means GHCAA.Domain.Models.Member, identified through the
  CurrentMemberIdRaw claim helper, distinct from the system admin User identity used for
  CurrentUserIdRaw on the hard-delete action.
- RequireStepUp is treated as an existing, separately specified cross-cutting mechanism;
  this spec records where it applies in this domain but does not redefine it.
- The OutputCache PublicReference policy on the anonymous GovernanceController reads follows
  the platform-wide output-cache invalidation behaviour documented elsewhere, not redefined
  here.
- Dates accepted by CreateElectionDto and CreatePeriodRequest are assumed to arrive as ISO
  yyyy-MM-dd or full ISO-8601 from API clients per the platform-wide date convention, and are
  converted to UTC server-side.
