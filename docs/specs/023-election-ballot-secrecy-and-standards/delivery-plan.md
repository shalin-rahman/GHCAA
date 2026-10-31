# Spec 023 delivery plan for the remaining election work

Written 2026-10-09 on Opus. It covers every open WP37 item that code can close, split into six
sessions (S1 to S6). Each session runs on Sonnet from a cleared context and should need no
exploration beyond the anchors given here.

The design behind each item is in [plan-officials.md](plan-officials.md): section 4 for 37.12g to
37.12j, section 5 for 37.13a to 37.13g. The rules are in [spec.md](spec.md). This file says where
the code goes, which tests to add and which docs to change. It does not repeat the design.

## Rules for every session

1. Start with `/clear`, then paste the kickoff prompt for that session.
2. Read only the anchors listed for the session. Line numbers were taken on 2026-10-09 and move
   as code is added. If a line does not match, grep for the symbol named next to it. Do not read
   whole files to find it.
3. Follow the build order inside the session: backend, backend tests, web, web tests, mobile,
   mobile tests, docs.
4. Python edit scripts go in the session scratchpad. Use a new file name each time and run them
   with `PYTHONIOENCODING=utf-8 python <script> </dev/null`. Keep BOM and CRLF as found. Never
   pipe a heredoc into `python -`. Never run `dart format` on a whole file.
5. A migration is written by hand as `yyyyMMddHHmmss_Name.cs` plus `.Designer.cs` in
   `GHCAA.Infrastructure/Data/Migrations/PgSql/`, and the snapshot is edited to match. Never run
   `dotnet ef migrations add` followed by `remove`; that cycle truncates the snapshot. The last
   migration on 2026-10-09 is `20261009120000_AddElectionPeriodLiveIndex`.
6. Controllers never touch `ApplicationDbContext`. Error codes go through the switch in the
   controller that already maps them (`ElectionsController.cs` near line 43,
   `AdminElectionsController.cs` near lines 102, 133 and 151).
7. New literals go in the constants files: backend `GHCAA.Domain/Constants.cs` class `Elections`
   (near line 123), web `GHCAA.Web/src/app/core/constants/app.constants.ts` blocks `ELECTIONS`
   (near line 724) and `ADMIN_ELECTIONS` (near line 743).
8. Web screens use the shared components in `GHCAA.Web/src/app/common/` (page-header,
   modal-header, confirm-dialog, step-up-dialog, logo-spinner, pagination) and theme tokens only.
   Mobile gets its own card and list layout, not a port of the web screen.
9. Tag each new backend test `[Category("FR-39")]`. Use `[Category("NFR-R5")]` for a test that
   pins a failure or retry path. Name tests `Method_Condition_Result`, as the existing election
   tests do.
10. Comments read as a person wrote them. Short, plain, no em dashes.
11. When the code builds, run `python scripts/verify-election.py`. It runs the election backend
    tests, the web type-check and specs, `flutter analyze` and the mobile tests, then the tracker
    checks. All seven steps must pass. Logs are in `%TEMP%\ghcaa-verify\`. A new web spec or
    Flutter test file must be added to `WEB_SPECS` or `MOBILE_TESTS` in that script in the same
    session.
12. Then run the `code-reviewer` agent once on the session's diff, fix what it confirms, and run
    the verify script again.
13. Then `graphify update .`, and the doc checklist for the session.
14. Never commit. Stop with a short list of changed files and the verify output.

### Doc checklist (the session names which rows apply)

| Doc | What changes |
|---|---|
| `docs/TODO.md` | Close the item with the date and a one-line analyst note. Add a new item for anything found and left. |
| `spec.md` | New FR lines, or a change to an existing one. FR numbers follow the last one in the file (FR-047 on 2026-10-10, after S1). |
| `plan-officials.md` | Mark the item built in its section, note any design change and why. |
| `docs/API_CONTRACT_REGISTRY.md` | A dated entry at the top of the list (near line 50) for each new or changed route. |
| `docs/api/swagger.json` | Add the route. Edit by JSON load and dump with `indent=2, ensure_ascii=False`. |
| `docs/FEATURES.md` | §5.1a, the document library (near line 220), §5.1b, officials and rules (near line 256), or §5.1c, the member ballot (near line 296). |
| `docs/SRS.md` | §3.6, the election requirements (near line 96). |
| `docs/PROJECT_MAP.md` | Election controller and service rows (near line 998). |
| `docs/ARCHITECTURE.md` | Only if a new service, table or hosted job is added (election lines near 77, 85, 1053). |
| `docs/Elections/00-Election-Master-Overview.md` | The phase flow (near line 215), when a phase does something new. |
| `docs/Elections/02-Election-Operational-Manual.md` | The life cycle (near line 148), when an official does something new. |

Do not edit `docs/RENDER_DEPLOYMENT.md`. Do not edit JSON seed data or any secret.

## Order

| Session | Items | Model | Migration | Size |
|---|---|---|---|---|
| S1 | 37.13b ballot order, 37.13d test elections | Sonnet | `AddBallotOrderAndTestElection` | S+S |
| S2 | 37.1o tie rule and dates on the create forms, picks per seat on the clients | Sonnet | none | M |
| S3 | 37.12g access ends, 37.1r device details | Sonnet | none | M |
| S4 | 37.13a audit chain, then 37.13c opened ballots | Opus for the 37.13a writer, Sonnet for the rest | `AddElectionAuditChain` | M |
| S5 | 37.12i public board, 37.1q ER forms from records | Sonnet | none | M |
| S6 | 37.12h officials area, then 37.13e, 37.13f, 37.12j docs | Opus for the 37.12h screen layout, Sonnet to build it | none | L |

S4 must come before S5 (the board shows the audit head hash) and before S6 (the destroy-key
record goes into the chain). S1 must come before S4 (the shuffle seed goes into the chain; S1
writes it to an activity row and S4 moves it). Otherwise the order can change.

## S1. Ballot order and test elections (37.13b, 37.13d)

**Design:** plan-officials.md section 5, rows 37.13b and 37.13d.

**New FRs in spec.md** (P2 election rules section, after FR-025):
- FR-046 Candidates appear on the ballot in a fixed order set when the election enters
  CandidateList, either shuffled or alphabetical as the `CandidateOrder` setting says. The order
  does not change after that.
- FR-047 An election can be marked as a test at creation. A test election is left out of the
  public reads, and declaring it writes no committee members.

**Anchors**
- `GHCAA.Domain/Models/Election.cs:7` `Election` (add `bool IsTest`), `:137` `Nomination` (add
  `int? BallotOrder`).
- `GHCAA.Infrastructure/Data/Configurations/ElectionConfigurations.cs:108`
  `NominationConfiguration`. `IsTest` defaults to false in the config.
- `GHCAA.Infrastructure/Services/ElectionService.cs`:
  - `:25` `CreateAsync`. Set `IsTest` from the DTO next to `TieRule` at `:46`.
  - `:91-103` `SetPhaseAsync`. When the new phase is `CandidateList`, assign `BallotOrder` to the
    accepted nominations of each seat. `CandidateOrder` comes from `IOrgConfigService`; check
    how `ElectionService` is built first (it is `new ElectionService(_context)` in tests, so a
    new constructor argument needs those call sites changed, or read the setting through an
    optional argument as other services do).
  - Random order: `RandomNumberGenerator.GetInt32` Fisher-Yates per seat. Alphabetical: by
    `Member.FullName`, ordinal ignore case.
  - Record the order type and, for random, the seed-free result list in an activity row
    (`ElectionCandidateOrderSet`, a new constant beside the audit types at `Constants.cs:123`).
    S4 moves this into the audit chain.
  - `:436` `GetBallotAsync`, order at about `:450`. Sort by `BallotOrder`, then `FullName` for
    rows with no order (elections made before this change).
  - `:434` `GetNominationsAsync` and `:474` `GetPublicResultsAsync` keep their present order.
  - `:70` `GetCurrentAsync`. Leave out `IsTest` elections.
  - `:319` `DeclareAsync`. Skip the ECMember write at `:335` and the guard at `:330` when
    `IsTest`.
  - `:543` `ToAdminElection`. Map `IsTest`.
- `GHCAA.Application/DTOs/ElectionDtos.cs:8-9` `CreateElectionDto`, `CreateAdminElectionRequest`
  (add `bool IsTest = false`), `:29` `AdminElectionDto` (add `IsTest`).
- `GHCAA.API/Controllers/AdminElectionsController.cs:46` create, passes the flag through.
- The public results and documents reads (`ElectionsController.cs:65`, `:257`) answer 404 for a
  test election. Admin reads still show it.
- Web: `admin-elections.html` create form (about lines 6-20) gets a "Test run" checkbox;
  `admin-elections.ts:73` form, `:115` `emptyForm`, `:141` `create`. A "Test" badge on the row.
  Model in `core/models/election.models.ts`.
- Mobile: `election_management_screen.dart:142` `_createElection` gets a switch; the card shows
  a badge. Model in `features/elections/election_service.dart`.

**Migration** `yyyyMMddHHmmss_AddBallotOrderAndTestElection`: `Nominations.BallotOrder int null`,
`Elections.IsTest boolean not null default false`. Snapshot in
`PgSqlApplicationDbContextModelSnapshot.cs`.

**Tests** (`GHCAA.Tests/Services/ElectionServiceTests.cs`, helpers `Make(phase)` at `:552`,
`CreatePollingElectionAsync` at `:741`)
- `SetPhaseAsync_ToCandidateList_NumbersEachSeatFromOne`
- `SetPhaseAsync_AlphabeticalSetting_OrdersByName`
- `SetPhaseAsync_PastCandidateList_KeepsTheOrder`
- `GetBallotAsync_SortsByBallotOrder`
- `GetBallotAsync_NominationsWithoutAnOrder_FallBackToName`
- `GetCurrentAsync_SkipsATestElection`
- `DeclareAsync_TestElection_WritesNoCommitteeMembers`
- `CreateAsync_KeepsTheTestFlag`
- `Results_TestElection_Returns404` and `TrackingCodes_TestElection_Returns404` (in
  `ElectionsControllerTests.cs`)
- Web: one case in `admin-elections.spec.ts` that the checkbox reaches the request.
- Mobile: one case in `test/election_service_test.dart` that `isTest` parses.

The names were changed when S1 was built to follow the existing style in each file
(`MethodAsync_Case_Result` in the service tests, `Route_Case_ReturnsNNN` in the controller tests).

**Docs:** TODO, spec, plan-officials, API_CONTRACT_REGISTRY (create request shape), swagger,
FEATURES §5.1c (Member Ballot; §5.1a is the document library), SRS §3.6, PROJECT_MAP, master
overview phase flow (CandidateList sets the order).

**Done when** the verify script passes, a declared test election leaves `ECMembers` untouched,
and the ballot order survives a phase change.

**Kickoff prompt**
```
Session S1 of docs/specs/023-election-ballot-secrecy-and-standards/delivery-plan.md.
Read that file's "Rules for every session" and the S1 section, then build S1 exactly as written.
Do not explore beyond the S1 anchors; grep a symbol if a line has moved.
Finish with python scripts/verify-election.py, then the code-reviewer agent, then the S1 doc rows.
Do not commit.
```

## S2. Tie rule, dates and picks per seat on the clients (37.1o)

**Design:** spec.md FR-018 (pick up to SeatCount), FR-019 and decision D2 (tie rule picked at
setup with a one-line description), decision D3 (`ScrutinyOn` and `WithdrawalClosesOn` on the
create screen).

The backend already accepts several picks per seat: `CastBallotDto(IReadOnlyList<BallotSeatChoiceDto> Seats)`
and `BallotSeatChoiceDto(int ElectionSeatId, IReadOnlyList<int> NominationIds)` at
`ElectionDtos.cs:22-23`, checked in `CastBallotAsync` at `ElectionService.cs:179`. Only the
clients stop at one. `BallotSeatViewDto.Places` gives the limit.

**Anchors**
- `ElectionDtos.cs:8-9`: add `DateTime? ScrutinyOn`, `DateTime? WithdrawalClosesOn` to both create
  DTOs. `CreateAsync` (`ElectionService.cs:25`) sets them, and refuses dates out of phase order
  with error `dates-out-of-order` (map it in `AdminElectionsController`). Entity fields already
  exist at `Election.cs:16-17`.
- Tie rule enum `ElectionTieRule` at `Enums.cs:64`; the DTO already carries `TieRule`.
- Web create form: `admin-elections.html` (about lines 6-20), `admin-elections.ts:73`, `:115`,
  `:141`. Add a tie-rule select with the three one-line descriptions as constants in
  `ADMIN_ELECTIONS`, and two date inputs. Dates go out as ISO date strings, per the
  `ghcaa-date-standard` skill.
- Web ballot: `member/election/election.ts` `choices` at `:43`, `allDecided` at `:51`, toggle at
  `:85-94`, abstain at `:99`, cast at `:129-131`. A seat with `places > 1` uses checkboxes that
  stop at `places`, and shows "Pick up to N". Abstain clears the seat. A seat counts as decided
  when it has at least one pick or an abstain.
- Mobile create form: `election_management_screen.dart:142`, controllers at `:155-159`. Same tie
  rule dropdown and two date pickers.
- Mobile ballot: `screens/member/election_screen.dart` `_picks` at `:57`, toggle at `:76-81`,
  abstain at `:88`, cast at `:95-99`; `castBallot` at `features/elections/election_service.dart:614`.
  Same rules as web.
- Receipt text on both clients lists every name picked for a seat. Since 37.14o the mobile app shows
  the server PDF instead, which has no choices, and the names show in the review step.

**Tests**
- Backend: `Create_SetsScrutinyAndWithdrawalDates`, `Create_DatesOutOfOrder_Refused`,
  `CastBallot_TwoPicksOnTwoPlaceSeat_Counted`, `CastBallot_MorePicksThanPlaces_Refused` (skip
  the last two if an existing test already pins them; grep `Places` in `ElectionServiceTests.cs`).
- Web `admin-elections.spec.ts`: tie rule and dates reach the request.
- Web `election.spec.ts`: third pick on a two-place seat is blocked; abstain clears picks; cast
  sends both ids.
- Mobile `election_service_test.dart`: cast body carries two ids for one seat. A widget test at
  phone viewport (`tester.view.physicalSize = Size(390, 1400)`, pattern in
  `test/my_appointments_screen_test.dart:33`) for the pick limit.

**Docs:** TODO (close 37.1o, and the 37.1z leftovers if this covers them), spec (FR-018 and D3
note that it is built), API_CONTRACT_REGISTRY, swagger, FEATURES §5.1a, operational manual
(setup step names the tie rule).

**Done when** a two-place seat takes two picks on web and mobile and the count shows both.

**Kickoff prompt**
```
Session S2 of docs/specs/023-election-ballot-secrecy-and-standards/delivery-plan.md.
Read "Rules for every session" and the S2 section, then build S2 exactly as written.
Do not explore beyond the S2 anchors. Finish with the verify script, code-reviewer, then the S2 doc rows.
Do not commit.
```

## S3. Access ends by itself, device details on the vote row (37.12g, 37.1r)

**Design:** plan-officials.md section 4 "37.12g Access ends by itself"; spec.md FR-032; spec.md
"What the vote record keeps about the request" for 37.1r.

**Anchors, 37.12g**
- `ElectionService.cs:319` `DeclareAsync`: set `ExpiresAt = DeclaredOn + AccessEndsDaysAfterDeclare`
  on each appointment of the election that is live and has no `ExpiresAt`. The setting is at
  `OrgConfigDto.cs:37` (default 21) and nothing reads it yet. Same constructor question as S1.
- `ElectionAppointment` at `Election.cs:50`, `ExpiresAt` at `:69`, `IsLive(now)` at `:79`.
- `IElectionAppointmentService.cs:15` and `ElectionAppointmentService.cs:211`
  `EndAccessIfNoneLeftAsync`: removes the role, rotates `SecurityStamp`, deactivates a user with
  no member link and no roles, revokes refresh tokens. Reuse it; do not copy it.
- New `EndAllAsync(int electionId, string reason)` on the same interface and service: revokes
  every live appointment with reason `"Election archived"` (a constant), then calls
  `EndAccessIfNoneLeftAsync` per user.
- Archive: `SetPhaseAsync` (`ElectionService.cs:91-103`) has no Archived branch; archive arrives
  through the approval path, `ElectionApprovalService.cs:569-576` `PhaseFor`. Call `EndAllAsync`
  where the archive phase is applied. Check whether it runs inside the approval's transaction and
  keep it there.
- Login: `AuthService.cs:60` `LoginAsync`, inactive check at `:112-116`. Before it, for a user
  with no member link whose only role is ElectionOfficial and who has no live appointment (use
  the live query in `ElectionAccessService.cs:27`, not a new one), call
  `EndAccessIfNoneLeftAsync`, then refuse with the same generic message. Same check for social
  login at `:331`.
- Refresh: `AuthController.cs` near `:85-96`, check at `:120`. The controller calls a service; put
  the check in the service, not the controller.
- `AuthController.Me` near `:135-145` already uses live summaries. No change.

**Anchors, 37.1r**
- `AuditLogMiddleware.cs` `LogVoteAsync` (near `:72-93`) passes no metadata.
  `ActivityService.cs:26-45` fills IP and user agent; there is a crude mobile check at `:32`.
- Parse the user agent into `{ "device": "mobile|tablet|desktop", "os": "...", "browser": "..." }`
  with a small static parser (no package), and pass it as `Metadata` on the vote row only. The
  row's time stays date only (FR-004). No location lookup.
- Put the parser in `GHCAA.Infrastructure` beside `ActivityService`, with its strings in
  `Constants.cs`.

**Tests**
- `ElectionServiceTests`: `Declare_SetsAppointmentExpiryFromSetting`,
  `Declare_KeepsExistingExpiry`.
- `ElectionAppointmentServiceTests` ([SetUp] at `:27`, builder `NewAppointments(...)`):
  `EndAll_RevokesEveryAppointment`, `EndAll_DeactivatesNonMemberWithNoOtherRole`,
  `EndAll_MemberOfficialKeepsMemberAccess`, `EndAll_RotatesSecurityStamp`.
- `ElectionApprovalServiceTests` (`SetUpRevokeAsync` at `:411` shows the shape):
  `ApproveArchive_EndsAllAppointments`.
- `AuthServiceTests`: `Login_NonMemberOfficialAfterExpiry_Refused`,
  `Login_MemberOfficialAfterExpiry_Allowed`, `Refresh_NonMemberOfficialAfterExpiry_Refused`.
- A parser test class `UserAgentParserTests`: iPhone Safari, Android Chrome, Windows Edge, the
  Flutter `Dart/` agent, empty string. Plus `LogVote_WritesDeviceMetadata_NoTime` if the
  middleware has a test file; grep `LogVoteAsync` in `GHCAA.Tests`.

**Docs:** TODO (close 37.12g; split 37.1r into a closed device part and an open location part
that waits on the committee), spec FR-032 built, plan-officials 37.12g built, FEATURES §5.1b,
SRS §3.6, operational manual (what happens on archive and after the access window), ARCHITECTURE
if the parser is a new service.

**Done when** a non-member official cannot sign in the day after the window ends, a member
official still can, and archive leaves no live appointment.

**Kickoff prompt**
```
Session S3 of docs/specs/023-election-ballot-secrecy-and-standards/delivery-plan.md.
Read "Rules for every session" and the S3 section, then build S3 exactly as written.
Auth is touched: after code-reviewer, also run the security-reviewer agent on the AuthService and
AuthController changes. Finish with the verify script and the S3 doc rows. Do not commit.
```

## S4. Audit chain, then opened ballots (37.13a, 37.13c)

**Design:** plan-officials.md section 5 rows 37.13a and 37.13c.

**New FRs in spec.md** (after FR-047):
- FR-048 Each election keeps a hash-chained audit list. Anyone can ask whether the chain is whole,
  and the answer names the first broken entry. No entry holds voter identity.
- FR-049 After Declared, and when `PublishPerSeatBallots` is on, the opened ballots are published
  per seat, each seat's list shuffled on its own so rows cannot be joined across seats.

Use Opus for the 37.13a writer and its concurrency (Seq allocation under two writers). Switch
back to Sonnet for wiring the call sites, 37.13c, tests and docs.

**Anchors, 37.13a**
- New entity `ElectionAuditEntry` in `GHCAA.Domain/Models/Election.cs` (Id, ElectionId, Seq, At,
  ActorUserId nullable, Action, DetailJson, PrevHash, Hash). Unique index on (ElectionId, Seq).
  Config in `ElectionConfigurations.cs`.
- `Hash = SHA-256(PrevHash + "|" + Seq + "|" + At.ToString("O") + "|" + Action + "|" + DetailJson)`,
  lower hex. Use `Convert.ToHexStringLower(SHA256.HashData(...))` as `BallotSeal.cs:32` does.
  `PrevHash` of the first entry is 64 zeros.
- New `IElectionAuditWriter` in `GHCAA.Application/Interfaces`, `ElectionAuditWriter` in
  `GHCAA.Infrastructure/Services`. It adds the entry to the caller's context without saving, so it
  shares the caller's `SaveChanges`. Seq is max+1; a unique-index clash retries once (tag that
  test NFR-R5).
- Call sites: phase changes in `ElectionService.SetPhaseAsync`, declare, count; the approval
  service (activity calls at `ElectionApprovalService.cs:277`, `:478`); the freeze service
  (`ElectionFreezeService.cs:34, 42, 92, 110`); the appointment service on appoint, accept,
  revoke. Keep the existing activity rows; the chain is added beside them.
- Move S1's candidate-order record into the chain.
- `GET api/elections/{id}/audit/verify`, anonymous, on `ElectionsController`, returns
  `{ ok, entries, firstBrokenSeq }`. Exempt in the reflection test only if the test lists GETs.
- Audit list for officials: `GET api/elections/{id}/audit`, needs the ViewAudit permission through
  `ElectionAccessService`. S6's dashboard reads it.

**Anchors, 37.13c**
- `CountAsync` at `ElectionService.cs:229`, `BallotSeal.Open` gives `PendingChoice[]` (fields `S`,
  `N[]`) at about `:270`, opened ballots shuffled and stored at about `:277-285`.
- Setting `PublishPerSeatBallots` at `OrgConfigDto.cs:43`, default true, not read yet.
- `GET api/elections/{id}/opened-ballots`, anonymous, only after Declared and only when the
  setting is on, else 404. For each seat: the list of choice sets, shuffled per seat with
  `RandomNumberGenerator` on each read or once at declare (pick once at declare and store, so two
  reads cannot be compared; record the choice in plan-officials).
- DTOs in `ElectionDtos.cs` next to `PublicElectionResultsDto`.

**Migration** `yyyyMMddHHmmss_AddElectionAuditChain`: table `ElectionAuditEntries`, plus a table
or column for the stored per-seat lists if that option is taken.

**Tests**
- `ElectionAuditWriterTests`: `Chain_VerifiesAfterSeveralEntries`,
  `Chain_EditedRow_BreaksAtThatSeq`, `Chain_DeletedRow_Breaks`, `Writer_FirstEntry_UsesZeroPrevHash`,
  `Writer_SeqClash_RetriesOnce` (NFR-R5), `Entry_NeverHoldsVoterId` (reflection or scan of
  DetailJson for member ids after a cast).
- `ElectionServiceTests`: `SetPhase_WritesAuditEntry`, `Declare_WritesAuditEntry`.
- `ElectionsControllerTests`: `AuditVerify_Anonymous_ReturnsOk`.
- `OpenedBallots_BeforeDeclared_NotFound`, `OpenedBallots_SettingOff_NotFound`,
  `OpenedBallots_CountsMatchResults`, `OpenedBallots_NotInCastOrder` (use
  `CreatePollingElectionAsync(voters: 12)` and `CloseAndStartCountingAsync` at `:796`).
- Web and mobile: service methods `verifyAudit`, `getAudit`, `getOpenedBallots` with one spec
  each in `elections.service.spec.ts` and `election_service_test.dart`. No screens yet; S5 and S6
  use them.

**Docs:** TODO, spec FR-048 and FR-049, plan-officials, API_CONTRACT_REGISTRY (three routes),
swagger, FEATURES, SRS, PROJECT_MAP, ARCHITECTURE (new table and writer), master overview.
`docs/Elections/09-Election-Security-and-Compliance.md` compliance table: point §11 and §10 at
the new code.

**Done when** the verify endpoint reports ok on a full election run and names the right Seq
after a row is edited in a test.

**Kickoff prompt**
```
Session S4 of docs/specs/023-election-ballot-secrecy-and-standards/delivery-plan.md.
Run this session on Opus until ElectionAuditWriter and its tests pass, then /model sonnet.
Read "Rules for every session" and the S4 section, then build S4 exactly as written.
Finish with the verify script, code-reviewer, then the S4 doc rows. Do not commit.
```

## S5. Public board and ER forms from records (37.12i, 37.1q)

**Design:** plan-officials.md section 4 "37.12i Public board"; spec.md FR-033 and FR-025.

**Anchors, 37.12i**
- `GET api/elections/{id}/board`, anonymous, on `ElectionsController` beside the other
  anonymous reads (`:49`, `:65`, `:72`, `:79`, `:117`, `:257`).
- `ElectionBoardDto` in `ElectionDtos.cs`: Title, Phase, the timeline dates, IsTest, Groups of
  `{ GroupName, Members: [{ PersonaName, DisplayName, AcceptedAt }] }`, Turnout
  `{ BallotsCast, RollSize }` or null, KeyFingerprint, AuditHeadHash (after Declared).
- Personas: `ElectionPersona.cs:18` `ShowOnPublicBoard`, `:21` `SortOrder`. Only accepted, live
  appointments on public personas.
- Turnout is null during polling unless `ShowTurnoutDuringPolling` (`OrgConfigDto.cs:42`), and
  null when ballots cast is below `Constants.Elections.MinimumBallotsToCount` (`Constants.cs:127`).
- Web `public/elections/elections.ts` and `.html`: a "Who runs this election" section, turnout
  line, Test badge, fingerprint and audit hash with a "Check the audit chain" link to the S4
  verify route.
- Web `member/election/election.ts`: "You are on the roll" and "You have voted" from
  `BallotViewDto.HasVoted` (`ElectionDtos.cs:36`) and the eligibility already returned by the
  ballot read (37.1z).
- Mobile `screens/member/election_screen.dart`: a read-only officials list from the same route.

**Anchors, 37.1q**
- `ElectionDocumentService.cs` (QuestPDF, QRCoder). `SupportedForms` at `:20-24`, `GeneratePdfAsync`
  at `:26` loads Seats, VoterRoll and ECPeriod. Titles at `:110-125`. Bodies at `:131-133`; only
  ER-07 uses records.
- Fill each form from what is stored. Load nominations, results and the board only for the forms
  that need them. The mapping:

| Form | Filled from |
|---|---|
| ER-07 voter roll | already done |
| ER-10 consent | nominations with consent time |
| ER-23 count sheet | per-seat, per-candidate counts plus spoiled and abstain (FR-025) |
| Result and declaration forms | the declared result per seat, tie rule, declared date |
| Officials forms | board groups from 37.12i |
| Others | keep the generic line and list them in TODO as needing the regulation text |

  Read the 16 codes at `:20-24` first and fit each into one row above. A form the code cannot
  place goes in the last row; do not guess its content.

**Tests**
- `ElectionBoardTests` (new): `Board_OnlyAcceptedPublicPersonas`, `Board_SortedBySortOrder`,
  `Board_TurnoutHiddenDuringPolling`, `Board_TurnoutShownWhenSettingOn`,
  `Board_TurnoutHiddenBelowMinimum`, `Board_AuditHashOnlyAfterDeclared`,
  `BoardDto_ContainsNoContactFields` (reflection, model on
  `CredentialCodeGeneratorTests.cs:24-33`).
- `ElectionDocumentServiceTests`: `ER23_CarriesPerCandidateCounts`, `ER10_ListsConsentTimes`, and
  one test that every supported code renders without error.
- Web: `public/elections/elections.spec.ts` (new; add to `WEB_SPECS`) for the board section and
  turnout hidden; one case in `election.spec.ts` for the two lines.
- Mobile: a widget test for the officials list at phone viewport; add to `MOBILE_TESTS`.

**Docs:** TODO (close 37.12i, 37.1q; new item for forms left on the generic line), spec FR-033
and FR-025 built, plan-officials, API_CONTRACT_REGISTRY, swagger, FEATURES, SRS, PROJECT_MAP,
operational manual (which forms fill themselves).

**Done when** the board shows only public, accepted officials with no contact fields, and ER-23
prints the counts of a test run.

**Kickoff prompt**
```
Session S5 of docs/specs/023-election-ballot-secrecy-and-standards/delivery-plan.md.
Read "Rules for every session" and the S5 section, then build S5 exactly as written.
Finish with the verify script, code-reviewer, then the S5 doc rows. Do not commit.
```

## S6. Officials area, key procedure, compliance and docs (37.12h, 37.13e, 37.13f, 37.12j)

**Design:** plan-officials.md section 4 "37.12h Officials area" and decision 4 (mobile gets its
own screens); section 5 rows 37.13e and 37.13f; section 4 "37.12j Docs".

Start on Opus to settle the dashboard card list and the mobile screen flow in a short note added
to plan-officials 37.12h. Then `/model sonnet` and build.

**New FR in spec.md:** FR-050 After Declared, an official with the key records that the key file
was destroyed. The record goes into the audit chain with who and when.

**Anchors, web**
- `app.routes.ts:147-158` `officials` route group (authGuard, under PortalLayout), child
  `my-appointments` at `:155`. Add `electionStaffGuard` (`auth.guard.ts:117`, unused today), a
  dashboard child at `''` and `elections/:id`.
- `elections/:id` reuses `AdminElections` and hides buttons from `MyPermissions` on
  `AdminElectionDto` (`ElectionDtos.cs:29`). Do not copy the component. Existing helpers
  `canSetBallotKey()` at `admin-elections.ts:199` and `pollingApproverWarning()` at `:205` show
  the pattern.
- Nav: `core/services/nav.service.ts`, `roles?: string[]` at `:13`, elections item at `:62`,
  SuperAdmin pattern at `:67-74`. Roles from `ELECTION_STAFF_ROLES` (`app.constants.ts:10`).
- Dashboard cards read existing service methods in `core/services/elections.service.ts`:
  approvals `getApprovals` `:54`, `approve` `:58`, `reject` `:62`; nominations `:82`; roll
  `freezeVoterRoll` `:78`; key `setBallotKey` `:116` (status from the fingerprint on the DTO);
  count `:121`, `:127`; declare `:134`; appointments `:142-169`; unlock `:174-182`; audit from S4.
  Turnout from the S5 board route. Min and max count warnings from the persona settings.
- Approve hidden on the user's own request. Step-up through `step-up-dialog` on approve.
- Destroy-key checkbox after Declared: new `POST api/elections/{id}/key-destroyed`, guarded
  through `ElectionAccessService`, writes an audit chain entry. Add it to the reflection test's
  guarded list.

**Anchors, mobile**
- Router `core/router/app_router.dart:214` `/officials/appointments`. Add `/officials` (dashboard),
  `/officials/elections/:id`, `/officials/approvals`.
- `screens/officials/` holds only `my_appointments_screen.dart`. Add
  `officials_dashboard_screen.dart`, `officials_election_detail_screen.dart`,
  `officials_approvals_screen.dart`.
- `features/elections/election_service.dart` (methods `:558-744`) has no approvals methods; add
  `getApprovals`, `approve`, `reject` matching the web routes.
- A member with an appointment reaches it from the member drawer; a pure official lands on
  `/officials`.

**Tests**
- Backend: `KeyDestroyed_WritesAuditEntry`, `KeyDestroyed_BeforeDeclared_Refused`,
  `KeyDestroyed_WithoutPermission_Forbidden`; reflection test still passes.
- Web `officials/dashboard/officials-dashboard.spec.ts` (new, add to `WEB_SPECS`): cards hidden
  without permission, approve hidden on own request, turnout hidden during polling.
- Web `admin-elections.spec.ts`: buttons hidden by `MyPermissions`.
- Mobile `officials_dashboard_screen_test.dart`, `officials_approvals_screen_test.dart` (new, add
  to `MOBILE_TESTS`), the same three cases at phone viewport, using `MaterialApp.router`.
- Mobile `election_service_test.dart`: the three approvals methods.

**Docs, 37.13e:** `docs/Elections/11-Election-Key-Procedure.md`: who makes the key, where the file
lives, two-person replacement, destroying it after declare with the dashboard record. Name the
trusted-custodian assumption from spec.md "Not adopted".

**Docs, 37.13f:** finish `docs/Elections/09-Election-Security-and-Compliance.md`: threat model,
incident steps, logging and retention, and a compliance table row for every "must" in
`docs/Elections/00-election-standards-requirements.md`, each pointing at a TODO id or a file.
Write only what the code and earlier sessions show; mark anything unverified as open.

**Docs, 37.12j:** spec.md user stories for personas, appointments, approvals and the board;
operational manual (appoint, accept, approve, the dashboard); API_CONTRACT_REGISTRY, swagger,
PROJECT_MAP, FEATURES, SRS, ARCHITECTURE. Then the book: if a chapter quotes election features,
run `python docs/book/build/build.py --pdf --strict` and fix what it names.

**Done when** a pure official signs in on web and mobile, lands on the dashboard, sees only the
cards their permissions allow, and cannot approve their own request.

**Kickoff prompt**
```
Session S6 of docs/specs/023-election-ballot-secrecy-and-standards/delivery-plan.md.
Run on Opus until the dashboard card list and mobile flow note is added to plan-officials 37.12h,
then /model sonnet. Read "Rules for every session" and the S6 section, then build S6 exactly as written.
Finish with the verify script, code-reviewer, then the S6 doc rows. Do not commit.
```

## Left for the owner or a person

These cannot be closed by a session. They stay open in TODO.md.

| Item | Who | What is needed |
|---|---|---|
| 37.1p | Owner | Decided 2026-10-11 and 2026-10-14. Built in Fast track sessions F2 and F3. |
| 37.16e | Owner | Split from 37.1p on 2026-10-31. Decided 2026-10-14. Built in Fast track session F2. |
| 37.1r location | Committee | Closed 2026-10-11 without building. No IP location lookup. |
| 37.1m font | Owner | Decided 2026-10-14: Noto Sans Bengali. Built on 2026-10-21 on the server by 37.14o, not in the mobile assets. |
| 37.1m browser check | Person | Print a receipt from Chrome and Edge and check the layout. |
| 37.1z device check | Person | Cast on a phone with TalkBack or VoiceOver. |
| 37.13g | Person | Run `docs/Elections/10-Election-Accessibility-Checklist.md` with keyboard only and a screen reader; write the results into it. |

## Fast track for the rest of WP37 (2026-10-14)

S1 to S6 covered twelve items, the ones in the Order table above. Most of what is still open in
WP37 was raised after that table was written: by the S5 code review (2026-10-10), the handbook
review and the owner's rules of 2026-10-11, and the S6 review (2026-10-12). Three items were
parked for the owner, and the non-election areas were never part of spec 023. This section plans
all of it. A second round of decisions the same day added the open election items outside WP37
(WP34, WP42, WP82, WP84, WP95) and the removal of GHC from the whole app (WP62). The rules for every session above still apply (anchors only, build order, hand-written
migration with its Designer file, constants, shared components, test tags, `verify-election.py`,
`code-reviewer`, `graphify update .`, never commit).

The code was checked on 2026-10-14, and line numbers are from that day. Every decision below was
made by the owner on 2026-10-11 or 2026-10-14. Nothing in this section is left to decide in a
session, except the items in "What stays with a person".

### Code check of S1 to S6

| Session | State | Short against its own plan |
|---|---|---|
| S1, S2, S3 | Built | nothing |
| S4 | Partial | tests `OpenedBallots_CountsMatchResults` and `OpenedBallots_NotInCastOrder` missing; mobile `election_service.dart` has no `getAudit` and no full `verifyAudit` (web has both) |
| S5 | Built | ER-19 has a case (`ElectionDocumentService.cs:150`) but its body text is still generic (`:272`) |
| S6 | Partial | `09-Election-Security-and-Compliance.md` still has 11 *[to confirm]* points (37.13f) |

These go into 37.14g and are fixed in F1. 37.12x did not fail when its spec ran alone on
2026-10-14 (33 of 33 passed).

### Decisions taken on 2026-10-14

| Item | Decision |
|---|---|
| 37.12w | Five conditions are read from stored data. Mock election and technical audit, open appeals decided and paper stock ready are attested by an official with `ChangePhase` (as built). Each attestation is an audit-chain entry (`ReadinessAttested`, with the condition key). No new table. |
| 37.14b | A member gets seats and candidates from CandidateList through Polling. Before CandidateList the read returns the phase and no seats. After Polling the board and results routes serve. |
| 37.14d | The audit foreign key becomes Restrict. The chain head hash is printed on ER-29 and on the public board, so a cut tail shows against the published value. |
| 37.1m | Noto Sans Bengali (SIL Open Font License). Built on the server in `GHCAA.Infrastructure/Fonts/` by 37.14o. |
| 37.1p, all seats uncontested | When every seat is unopposed or vacant, the Chief Commissioner moves the election from CandidateList straight to Declared, as a two-person step with step-up. No key, no poll. ER-29 says every seat was uncontested. |
| 37.16e, conflicts | The official declares conflicts when accepting the appointment and can add more later. The Chief Commissioner can also add one. Each is logged. |
| 37.12x | One full web run in F1. If it passes, close as not reproducible. If it fails, fix the test-order leak. |
| 37.12y | Voters are told through the existing notification service on the channels they allow, and the board shows the new closing time. Email does not leave preprod (Render blocks SMTP); check `EmailLogs` there. |
| Schema | One migration for every new election-rule column and table (F2). The second round adds two more: positions and terms (N1) and election content (F11). Migrations land one at a time, in the order under Sessions. |
| 37.12n | The F7 mapping comes first. The owner approves it before any form code. ER-02 and ER-28 are filled in F2 because the recount rules need them. |
| 37.4, 37.5, 37.9, 37.10 | Mobile gets the member views only. Admin moderation and curation stay on the web. |
| 37.7 | The same keys on web and mobile. Bengali strings are drafted in the session and listed as unreviewed until a Bengali speaker signs them off. Admin stays English on both clients. |
| 37.5, frozen roll | Marking a member deceased never changes a frozen roll. The officials dashboard warns and names the entry; the Chief Commissioner removes it through unfreeze and refreeze, which is logged. |

Second round, the open election items outside WP37 and the GHC cleanup:

| Item | Decision |
|---|---|
| 95.2, 37.1h-1 | Committee positions are rows in a `CommitteePosition` table per organisation, not an enum. Each organisation sets its own number and names of positions. The GHC profile seeds today's 15 under the same ids, so existing rows map one to one; the default profile seeds a short generic set. `ECMember.Position` and `ElectionSeat.Position` become foreign keys. 37.1h-1 closes through 95.2. |
| 95.4, 95.5, 95.12 | Declare never ends a current holder. A by-election takes vacant seats only and refuses a held one. A term ends in two ways: by date, when a daily job ends it with reason TermEnded once the period's `ECPeriod.EndDate` has passed, or early, when an admin ends it with a reason at any time. Whether the job runs, and the default term length used for a new period's end date, are organisation settings. The end reasons are rows per organisation like the positions; TermEnded and Died are system rows the code relies on and cannot be removed. One general election and one by-election can be live in the same period. |
| 84.42, 84.16 | Mobile keeps its create-election call. 84.42 finishes with the device check. 84.16 closes by recording in spec 011 that the web runs every admin step and mobile creates a draft, votes and views. An official's own steps (accept, declare a conflict, confirm a key copy) are on both clients; every step that changes the election's state is web only. |
| 42.1 to 42.5 | Built as F11: the handbook pages and the form wording become admin-editable content, seeded from the profile, and the public pages read from it. The GHC wording moves into `profiles/ghc/` in F11, so 82.59 in N9 only checks nothing is left. |
| 82.59, GHC | No GHC literal anywhere outside `profiles/ghc/`. The code name `GHCAA` in projects, namespaces and the solution stays; brand-lint stops counting it. |
| 62.31 | Done now, with no git-history rewrite. The real alumni files leave the repo for an operator path read through `AppSettings:RealDataPath`. `profiles/ghc/demo-data/` keeps synthetic records only. The session lists the exact files and waits for the owner's yes before it changes any JSON. |
| 84.36 | Every `ElectionValidators` rule is checked in F1. The rest of the app follows in N10. |
| 84.31 | `010-election-engine-fixes` moves to the next free spec number, and every reference follows. |
| 34.D18 | The preprod boot fix must be confirmed on a deploy before F2 adds a migration on the same chain. |

### Review rule

Every review in these sessions sorts each finding into one of three kinds.

1. A defect in the work of that session. Fixed in the same session. No tracker item.
2. A new rule or a decision only the owner can make. A tracker item, and a question to the owner.
3. A nice-to-have. A line in the undecided list (`project_undecided_items_2026_10_06`), not an
   open item.

Only kind 2 adds work. The review report names the kind next to each finding.

### What makes it faster

1. One migration for the election rules (F2), so the PostgreSQL dry run of the chain and the
   migration review happen once. N1 (positions and terms) and F11 (election content) add one each.
2. Backend first for every new rule (F2 to F4), then web and mobile as two sessions in separate
   worktrees at the same time (F5, F6). The clients do not touch the same files.
3. Small fixes with no schema change go first in one session (F1).
4. Doc work (F7) and the reference server run (F8) wait on no code and fill gaps.
5. The non-election areas (N2 to N7) are specified to the field in `docs/TODO.md`. Each is one
   session with no design step, except 37.7, which is split by client. N0, N1 and N9 to N11 are
   specified in this section.

### Sessions

| Session | Items | Model | Migration | Size | Depends on |
|---|---|---|---|---|---|
| N0 | 62.31 real alumni files out of the repo | Sonnet | none | S | owner's yes on the file list |
| F1 | 37.12x, 37.14b, 37.12s pinning test, 37.1m font, 37.14g S1-S6 shortfalls, 84.36 election rules, 84.42 election call | Sonnet | none | M | none |
| F2 | Schema for every new rule, 37.12r, 37.12o, 37.1p recount, 37.16e conflicts, 37.14d, ER-02 and ER-28 | Opus for the schema, Sonnet for services | `AddElectionRulesAndRecount` | M | F1, 34.D17 to 34.D19 confirmed |
| F3 | 37.14a, 37.1p unopposed seats and paper counts, 37.12z, 37.12y, 37.14f | Sonnet | none | M | F2 |
| F4 | 37.12w readiness gate | Sonnet | none | S | F3 |
| F5 | Web screens for F2 to F4 | Sonnet | none | M | F4, runs with F6 |
| F6 | Mobile screens for F2 to F4 | Sonnet | none | M | F4, runs with F5 |
| F7 | 37.13e protocol, 37.13f hosting facts, 37.12t and 37.12n inventory for approval, 84.31, 84.16 | Sonnet | none | M | none |
| F8 | 37.12v reference server on PostgreSQL in Docker | Sonnet | none | S | Docker on |
| N1 | 95.2 committee positions (closes 37.1h-1), schema for 95.4, 95.5 and 95.12 | Opus for the schema, Sonnet for services | `AddCommitteePositionsAndTerms` | L | F4 |
| F10 | 95.4 vacant seats, 95.5 by-election, 95.12 term ending | Sonnet | none | M | N1 |
| F11 | 42.1 to 42.5 admin-editable election content | Sonnet | `AddElectionContent` | M | F7 table approved, N1 merged |
| F9 | Election close-out: tracker, outline figures, `wbs.py`, full verify, memory | Sonnet | none | S | F1 to F8, F10, F11 |
| N2 | 37.7 web: service, pipe, dictionaries, toggle, self-hosted font | Sonnet | none | L | none |
| N3 | 37.7 mobile: Bengali in `app_localizations.dart` on the N2 keys | Sonnet | none | M | N2 |
| N4 | 37.5 In Memoriam | Sonnet | `AddMemorialRegister` | M | F9 (roll warning) |
| N5 | 37.4 cohorts and reunions | Sonnet | `AddBatchCohortsAndReunions` | M | none |
| N6 | 37.10 impact report | Sonnet | none | M | N5 |
| N7 | 37.9 chapters | Sonnet | `AddChapters` | M | none |
| N9 | GHC out of the whole app: 82.59, 62.2, 62.6b | Sonnet | none | M | N1, F11 |
| N11 | 62.41 generic run on an empty database, then 62.42 blocking brand-lint | Sonnet | none | M | N9 |
| N10 | 84.36 for the rest of the app | Sonnet | none | S | F1 |
| N8 | 37.11 gate: full suites, non-empty database run of every N migration, docs list | Sonnet | none | S | N0 to N7, N9 to N11 |

N1 moved forward because F10 needs its positions, end reasons and election kind. N2, N5, N7 and
N10 do not depend on the election track and can start any time a session is free.

Migrations land in this order: F2, N1, F11, then N4, N5 and N7 in whichever order they run. A
session that adds a migration starts only after the previous one is merged, takes the next
timestamp, and builds from that model snapshot. Two migration sessions never run at the same
time, even in separate worktrees, because both would rewrite `ApplicationDbContextModelSnapshot`.

N1 waits for F4 because `ElectionService.cs` still uses the `ECPosition` enum and F3 and F4 edit
the same file. N0 changes no code, so it goes first: 62.31 is P0 and should not wait behind N1
and F11. F2 waits for 34.D17, 34.D18 and 34.D19, the three startup repairs on the same
migration chain, to be confirmed on preprod.

### F1. Small fixes

- 37.12x: run the full web suite once. Pass means close; fail means find the spec that leaks state
  into `admin-elections.spec.ts`.
- 37.14b: `ElectionService.GetBallotAsync` (`ElectionService.cs:652`). Tests
  `GetBallotAsync_BeforeCandidateList_ReturnsNoSeats` and
  `GetBallotAsync_InCandidateList_ReturnsSeats`. Both clients show the phase when no seats come
  back.
- 37.12s: no code deletes election rows (`DataRetentionService.cs:8` leaves them out; the only
  election delete is the voter-roll unfreeze at `ElectionService.cs:271`, by design). Add a test
  that pins `DataRetentionService.PurgedTables` against every election entity.
- 37.1m: font file in `GHCAA.Mobile/assets/fonts/`, asset entry in `pubspec.yaml` (`:84`),
  `pw.ThemeData.withFont` in `ballot_receipt.dart`. Check the goldens do not shift. Superseded:
  37.14o put the font on the server and removed the mobile PDF builder.
- 37.14g: the two opened-ballot tests, `getAudit` and `verifyAudit` in mobile
  `election_service.dart` with tests, ER-19 body filled from records.
- 84.36, election part: for each rule in `GHCAA.Application/Validators/ElectionValidators.cs`,
  find where the service or a DataAnnotation enforces it. A rule enforced nowhere moves into the
  service with a test. Leave the file for N10 to remove.
- 84.42, election call: create election on mobile (`election_service.dart:697`) goes through the
  step-up prompt, with a test on the 403 path. It needs nothing from F2 to F4, so it does not
  wait for F6. The device check stays with a person.

### F2. Schema and the recount rules

One hand-written migration with its Designer file, after
`20261014120000_AddEventPhotoThumbnail`. It adds:

- `Election.RecountWindowEndsOn` (nullable), `Election.PollingExtensionCount` (int, default 0).
- `Nomination.ConsentAt` (nullable). ER-10 prints "not recorded" when null
  (`ElectionDocumentService.cs:180`).
- `ElectionRecountRequest` (election, seat, candidate, reason, filed at, status, decided by,
  decided at), unique on election and seat.
- `ElectionConflictDeclaration` (appointment, candidate nomination, relation: spouse, parent,
  child, sibling, proposer, seconder; declared by, declared at).
- `ElectionKeyCopy` (election, holder appointment, confirmed at). Used in F3.
- `ElectionPaperCount` (election, issued, unused, spoiled, entered by, approved by). Used in F3.
- `NominationsReopenedUntil` (nullable) on the seat record. Used in F3.
- Audit foreign key from Cascade to Restrict (`ElectionConfigurations.cs:215`).

Rules built in F2:

- Key destruction is refused until `RecountWindowEndsOn` is set and has passed, and while a
  recount request is open. The dashboard warns from declare while the date is unset.
- A candidate on a seat files one recount request inside the window (ER-28). The Chief
  Commissioner approves it through `ElectionApprovalService` as a two-person step. The count
  re-runs for that seat and both results are kept.
- An appointed official cannot be a nominee in the same election, and a nominee cannot be
  appointed. An official with a declared conflict is kept out of that candidate's scrutiny,
  appeals and recount. The check goes in `ElectionAccessService` and is called from
  `SubmitNominationAsync`, `AddCandidateAsync`, `DecideNominationAsync`, `AppointAsync` and
  `AcceptAsync` (37.16e).
- ER-02 prints the recount window; ER-28 prints the request and its decision.
- The chain head hash is printed on ER-29 and the board (37.14d).

Before the session ends: PostgreSQL dry run of the whole migration chain on a non-empty copy, and
`code-reviewer` on the migration with the standard sweep (idempotency, rollback, provider).

### F3. Key copies, seats, paper and polling

- 37.14a: copy count in the election settings (`OrgConfigDto.cs:27`, default 2, minimum 1). The
  Chief Commissioner names holders at key set. Holders are different appointed officials and not
  candidates. Each confirms receipt. Enforce the configured count exactly, and keep Polling blocked
  until the recorded copy count matches and every configured holder has confirmed. Key destroy also
  records the copies destroyed. Owner decision 2026-10-27.
- 37.14t: verification must prove that a printed form is genuine and unchanged. Bind the proof to
  the canonical printed content and valid form version, reject forms that could not yet exist in the
  current phase, and expose the canonical content or digest for comparison. The cryptographic
  construction is an implementation design choice. Owner decision 2026-10-27.
- 37.1p unopposed: a seat with one valid candidate is left off the ballot (today it is not,
  `ElectionService.cs:675`), and the candidate is declared elected unopposed at declare and on
  the certificate. When every seat is unopposed or vacant, the CandidateList to Declared step
  applies (decision above).
- 37.12z: the Chief Commissioner can reopen nominations for a seat with no valid candidate, for
  that seat only, for a period set per election (default 3 days) that ends before the roll
  freeze. Still empty at polling means vacant until a 95.5 by-election.
- 37.1p paper counts: a polling officer enters issued, unused and spoiled. The Chief Commissioner
  approves as a two-person step before the count. Refused unless issued equals used plus unused
  plus spoiled. Printed on the poll-closing certificate. Online ballots are never spoiled.
- 37.12y: extend polling by up to 24 hours, at most twice, while Polling is open. Chief
  Commissioner, two-person, step-up, reason required, logged, voters notified. A third request is
  refused with a message that points to the L3 void-and-rerun procedure.
- 37.14f: the dashboard lists elections more than ten years past declare. Purge by request plus a
  second approver (SuperAdmin when no Commission sits). The summary result and the purge record
  stay.
- `security-reviewer` on the two-person steps, the extension and the purge.

### F4. Readiness gate (37.12w)

Add `GetReadinessAsync` to `ElectionService` next to `SetPhaseAsync` (`ElectionService.cs:117`).
It returns each condition as met or unmet:

| Condition | Read from |
|---|---|
| Roll frozen | `VoterRoll.FrozenAt` |
| Candidate list final and ballot built | phase, accepted nominations, ballot order |
| Key set and every copy confirmed | `BallotPublicKey`, `ElectionKeyCopy.ConfirmedAt` |
| Appointments accepted, declarations signed | appointment `AcceptedAt`, `DeclarationSignedAt` |
| Polling window valid | opens before closes, opens in the future |
| Mock election and technical audit, open appeals, paper stock | `ReadinessAttested` audit entries |

`SetPhaseAsync` to Polling and the OpenPolling approval (`ElectionApprovalService.cs:625`) both
refuse with the unmet list. The `TooFewApprovers` warning joins the same list.

Built 2026-10-21. Differences from the table: the roll item checks that roll rows exist, which
only a freeze writes. The key item checks only that a key is set until 37.14a records the copies and
their confirmations; the accepted count and holder checks must block Polling when custody is short.
The window item checks the order and that it has not closed, because polling may open early. The
approver item is advisory: it shows on the list but is left out of the 400 and does not block.
Attestations are made by an official with `ChangePhase`, after step-up, and test elections need
them too.

### F5 and F6. Clients

Both at once in separate worktrees. Split by the 84.16 decision.

- Web (F5): readiness list and attestation buttons, recount decision, key holders, paper counts,
  extend polling, reopen nominations, ten-year list and purge request, the all-uncontested
  declare, and every member and official screen below.
- Mobile (F6): recount request by a candidate, `ConsentAt` on nomination, the readiness and
  phase shown read-only, and the official's own steps: conflict declaration at acceptance and
  confirming a key copy. Mobile uses its own card and list layout.

Each new spec or test file goes into `WEB_SPECS` or `MOBILE_TESTS` in `scripts/verify-election.py`.

### F7. Docs that wait on no code

- 37.13e: the longer ballot protocol in `11-Election-Key-Procedure.md` (padding, test vectors
  taken from the existing crypto tests).
- 37.13f: replace the 11 *[to confirm]* points in `09-Election-Security-and-Compliance.md` with
  quotes from Neon and Render public docs, each with link and date. The point at `:123` (the
  regulations do not name key loss after polling opens) goes to the owner as a regulation change.
  Plan-specific facts stay marked until the owner confirms the plan.
- 37.12t and 37.12n: one table of every form with its ER-<phase>.<nn> number, old number and the
  records that fill it. Shown to the owner; nothing applied until approved.
- 84.31: rename `docs/specs/010-election-engine-fixes/` to the next free spec number (check
  `docs/TODO.md` for a reserved "spec NNN" first) and fix every reference.
- 84.16: spec 011 gets a table of election steps and the client that runs each one: web for every
  admin step, mobile for create draft, vote and view.

### F8. Reference server

`docs/Elections/reference-implementation/`: `npm install`, throwaway PostgreSQL in Docker, one
election through key set, voting, count and key destroy. Write the result into 37.12v.

### F10. Vacant seats, by-election and term ending

- 95.12: a daily job, written like `DataRetentionSweep` and `ExpiredRevokeSweep` in
  `GHCAA.API/Services/`, ends every open `ECMember` row whose period's `ECPeriod.EndDate` has
  passed, with the TermEnded reason and an audit entry, when the organisation turns this on
  (off by default, so nothing changes for anyone until they choose). The default term length
  fills the end date when a period is created and can be changed on the period. An admin can end
  any term early through `POST members/{ecMemberId}/end-term` (`AdminGovernanceController.cs:84`) with a reason row and a note. Admin
  screen for the reason rows next to the positions screen.
- 95.4: vacant seats on the admin governance page with a filter. A seat is vacant when the
  active period has fewer current holders for a position than its seat count.
- 95.5: election kind General or ByElection, shown on the ballot, results and ER forms. A
  by-election offers vacant seats only and refuses a held one at create and again at declare.
  Declare never ends a holder. `GetCurrentAsync` (`ElectionService.cs:84`) returns the general
  election and any live by-election, and both clients show both.
- Clients: web gets the reasons screen, the vacant filter, kind on create and the early-end form.
  Mobile shows kind and the two live elections; admin steps stay on the web (84.16).
- Tests: declare leaves holders alone, the vacant-only guard at create and declare, two live
  elections, the job with the setting on and off, early end with each reason, a system reason
  cannot be removed.
- `security-reviewer` on the end-term route and the job.

### F11. Admin-editable election content

42.1 first: list what is static today (the files in `GHCAA.Web/public/assets/elections/`, the
form wording in `ElectionDocumentService.cs`, seeders). Then one table for content pages and form
wording, seeded from the profile, with admin create, edit and publish on the web. The public
pages and the generated forms read from it. Mobile reads the same API where it shows this
content today (42.4). The numbering approved in F7 is applied here. Admin text reaches public pages, so `security-reviewer` checks it cannot carry script.

### N1 to N8. Non-election areas

Each follows its tracker item as written, with these additions:

- N1 (95.2, closes 37.1h-1): `CommitteePosition` table (organisation, key, label, sort order,
  seats, active). The GHC profile seeds today's 15 under the enum's ids; the default profile seeds
  a short generic set. `ECMember.Position` and `ElectionSeat.Position` become foreign keys, and
  `ECPosition` (`GHCAA.Domain/Enums.cs:8`), the list in `lookup.service.ts` and `ParsePosition`
  (`AdminElectionsController.cs:182`) go. Admin can add, rename, reorder and retire positions;
  both clients read the list from the API. The same migration does three things for F10:
  - an end-reason table built the same way, seeded with today's five `VacancyReason` values
    under the same ids; `ECMember.EndReason` becomes a foreign key and the enum goes (38 uses
    today);
  - `Election.Kind`, default General;
  - rebuilds `IX_Elections_ECPeriodId_Live` on period and kind, and updates the matching SQL
    and clash query in `MigrationBootstrapper.cs:85`.

  The term-job setting and default term length are org config fields, added to the DTO, the
  defaults and `golden.json` together. PostgreSQL dry run on a non-empty copy and
  `code-reviewer`, as in F2. Closes ENH-003 and ENH-009.
- N2 and N3 (37.7): N2 builds the web mechanism and drafts `bn.ts`; N3 adds Bengali to the mobile
  localizations with the same keys. The unreviewed list lives under 37.7 in the tracker, and 37.7
  stays PARTIAL until a Bengali speaker signs it off.
- N4 (37.5): marking a member deceased with a frozen roll open warns on the officials dashboard
  and changes nothing (decision above). Mobile: list, entry page, condolence form.
- N5 (37.4): mobile gets "my batch" and the cohort page.
- N6 (37.10): mobile gets the report view and a link to the PDF.
- N7 (37.9): mobile gets the chapter list, join and leave, and the chapter feed.
- N8 (37.11): refresh the baselines from a run on the day, not the old 351 and 306 figures.
- N9 (GHC out of the whole app). Brand-lint found 2,605 hits on 2026-10-14. Most are the code name
  `GHCAA`, which stays; `scripts/brand-lint.config.json` stops counting it in namespaces and
  project names. The rest are in about 40 code and test files, 59 files that say "GHC", and the
  election files in `GHCAA.Web/public/assets/elections/` (82.59, moved in F11; N9 checks none
  are left). Each moves into profile config or the default profile. The remaining 62.2 profile
  files are written. 62.6b: delete `OrgConfigService.BuildGhcaaDefaults()` and its guard once a
  person has confirmed `ORG_PROFILE=ghc` on every deployment, and update `ProfileDrivenConfigTests`
  as the item says.
- N0 (62.31): copy the real files in `profiles/ghc/demo-data/` to the operator path, check the
  copy, replace the committed files with synthetic records, and point GHC's deployment at
  `AppSettings:RealDataPath`. A file found there wins over the demo-data copy, and the seeder
  only fills empty tables, so GHC's live database is untouched either way. `security-reviewer`
  on the step.
- N11 (62.41, 62.42): committed migrations are not edited. A startup step removes the rows they
  bake in, only when the profile is not `ghc` and the database holds nothing else, so the generic
  run on an empty database can pass. Then 62.42 adds brand-lint to the CI workflows, where it runs in none today, and
  makes it blocking. `security-reviewer` on the startup step.
- N10 (84.36, rest of the app): the same check as F1 for every other validator. Then remove the
  unused FluentValidation registration and package, since DataAnnotations is what runs.

### What stays with a person

| Item | Who | When |
|---|---|---|
| 37.12t, 37.12n | Owner | Approve the F7 form table before it is applied |
| 37.13f | Owner | Confirm the Neon and Render plans, and the key-loss regulation point, after F7 |
| 37.1m | Person | Print a receipt in Chrome and Edge, and on a phone, after F1. Also download the 37.14o PDF on both |
| 34.D17, 34.D18, 34.D19 | Person | Deploy preprod and find `history row restored` in the startup log, with no 42703 error and batch shown on directory cards, before F2 |
| 84.42 | Person | Step-up on a real device for roles and create election, after F1 |
| 62.31 | Owner | Say yes to the file list before any JSON changes, keep the copy of the real alumni files outside git, and set `AppSettings:RealDataPath` on GHC's deployment, in N0 |
| 62.6b | Person | Confirm `ORG_PROFILE=ghc` in the Render settings of every deployment, before N9 |
| 95.12 | Owner | Turn the term-end job on for GHC and set the term length, after F10 |
| 62.31 limit | Owner | The eight committed migrations still hold the real records in the repo and its history. Removing them needs a history rewrite, which was not chosen |
| 37.13g | Person | Accessibility run when F5 and F6 close |
| 37.7 | Bengali speaker | Sign off the unreviewed strings after N2 and N3 |
| Handbook v2.0 | Executive Committee | Approve at a meeting after F9 |

## Cost guard

Each session should finish inside one context. If a session passes about 60% of the context
window before the verify script runs, stop, write what is left as a TODO.md note under the item,
and start a fresh session with the same kickoff prompt plus "continue from the TODO note".
