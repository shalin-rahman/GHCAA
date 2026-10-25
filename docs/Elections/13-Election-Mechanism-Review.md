# Election Mechanism Review

**Reviewed:** 25 October 2026  
**Scope:** Election rules and workflow documentation, backend domain/persistence/services/API, and Angular and Flutter flows.

## Summary

The platform implements a phase-controlled election with a frozen voter roll, reviewed nominations, whole-ballot voting, encrypted pending ballots, separately authorized counting, and an auditable declaration. The main documented and implemented flow agrees. The strongest privacy boundary is that participation records retain member identity while counted choices do not: `Ballot` and `BallotVote` have no member link. The design still relies on trusted server-side key handling; it is not end-to-end verifiable voting.

## Full flow

1. **Configure:** Staff create an election for an EC period, set phase dates, seats, ballot order and election key. The database prevents overlapping live elections for a real period. Test elections are isolated from public results.
2. **Prepare and freeze:** Eligible members are snapshotted into the voter roll. The freeze is allowed up to and during the Nomination phase and refused after it (`ElectionService`, the voter-roll freeze check). Readiness checks cover the roll, candidate order, official declarations, key, schedule and required attestations. Which actions need a second official is set per organisation in `Elections.TwoPersonActions` and checked by `ElectionApprovalService.NeedsSecondAsync`. A config saved before that list existed loads without `Count`, so on those older configs counting needs only one approval.
3. **Nominate and finalize:** Eligible proposers submit candidates, officials record scrutiny, and accepted nominations may be withdrawn during the allowed window. Moving to the final candidate list locks the choices and assigns ballot order.
4. **Poll:** A member submits one complete ballot for the election, including explicit abstentions where applicable. The server rechecks eligibility and election phase, seals the choices into a pending ciphertext record, records participation separately, and returns a tracking code. Participation keeps only the day voted (`VotedAt` is saved with the time cut off), and each tracking code is stored as a `BallotReceipt` row with no ballot link. The transaction guards against duplicate or late voting.
5. **Count:** After polling closes, counting is requested and approved separately. The returning officer supplies the matching private key. The service decrypts and shuffles ballots and tracking codes separately, writes anonymous counted ballots/votes and results, then clears pending ciphertext. A minimum-count rule protects small ballots from disclosure.
6. **Declare and publish:** Declaration requires a completed count and approval. For a real election, elected candidates become EC members of the period the election was created for (`Election.ECPeriodId`), and only if that period has no members yet. Results, audit verification and allowed ballot/tracking views are then available; appointments expire and audit records preserve the transitions. Appeals/recounts and archive follow the governing rules.

The rules-level lifecycle, including the freeze, approval, polling, count and appeal/archive stages, is in [the master overview, §3 The full workflow, step by step](00-Election-Master-Overview.md#3-the-full-workflow-step-by-step). The secrecy data model and the deliberately missing member-to-ballot link are described in [the master overview, §4.3 How the records relate to each other](00-Election-Master-Overview.md#43-how-the-records-relate-to-each-other) and in [spec 023, FR-001 on keeping the ballot write apart from the participation write](../specs/023-election-ballot-secrecy-and-standards/spec.md). Implementation is centered in `ElectionService`, with separate approval, access, appointment, audit and freeze services.

## Data and privacy model

`Election` owns seats, voter-roll entries, nominations, appointments, approvals, pending ballots, receipts, counted ballots, results and audit entries. A nomination belongs to one election seat; a seat refers to an EC position. The participation side (`VoterRoll`/`SeatVote`) records who was eligible or voted. The choice side (`Ballot`/`BallotVote`) records anonymous choices without a member or timestamp link. Pending ciphertext is the bridge in time, not a relational bridge to the voter: it is removed after count. These boundaries and key tables should be shown distinctly in the ERD.

Both Angular and Flutter implement member ballot selection/review/submission, receipt handling, results and public election information, as well as staff administration and count/declaration workflows. The clients call the shared API; the business checks and persistence rules live on the server.

## Findings to carry forward

- **Nomination validation now matches on both paths.** Member nomination used to accept proposer = seconder. It now refuses it, as the staff-added-candidate path already did, so all three people must differ on both paths.
- **The roll-freeze message is still misleading.** The user-facing error says the roll cannot be frozen after nominations open, but the phase check allows a freeze through the Nomination phase. Leave the message change until the active election walkthrough run is finished.
- **Key custody defines the trust model.** Counting receives the private key at the server. Database separation and shuffling reduce linkage risk, but a compromised key custodian/server can expose pending choices. Describe this as trusted-server secrecy, not end-to-end verifiable tallying.
- **Keep the secrecy boundary visible.** Do not draw a relationship from a member, voter-roll row or seat-vote row to `Ballot`/`BallotVote`. Show the count transformation from sealed pending ballot to shuffled anonymous ballot, and show tracking codes as independently shuffled.
- **Treat status notes in older specs as historical.** Spec 023's gap inventory records an earlier state. Current behavior should be checked against implementation and the live API contract when producing diagrams.

## Diagram basis for the next step

The A4 ERD should prioritize the election aggregate, period/position, voter eligibility and participation, nomination/scrutiny, official appointment/approval, sealed-to-anonymous ballot transition, results/recounts, receipts and audit. The class diagram should show the end-to-end sequence across member/staff clients, API controllers, application interfaces, election/approval/audit services and persistence, with phase gates and the two-person approval points. Split the diagrams into readable A4 pages if one page would force small labels; include both the domain relationships and the secrecy boundary rather than flattening the entire schema into one view.
