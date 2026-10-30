# Auth and device fix plan (TODO 84.50, 84.49, 84.51)

Written 2026-10-25 from a read of the code on branch `preprod`. Line numbers are from that day
and drift; grep for the named symbol before editing. The tracker entries are `docs/TODO.md`
84.49, 84.50 and 84.51. This file says how to do them. The tracker says whether they are done.

This plan covers shared account, profile, and election-device fixes. The non-election backlog is
being delivered first. Work on 84.49 B1-B3, then the general account-security part of 84.51 and
admin unlock (Parts C and D). Defer Part A and 84.49 B4-B5 to the election delivery sequence.
Each part can ship on its own after its checks pass.

Non-election order:

1. Part B: 84.49 member identity, passing-year, and Dhaka-date rules (B1-B3 only)
2. Part C: general 84.51 account-security fixes
3. Part D: admin unlock (API, web, mobile)

Deferred election order:

1. Part A: 84.50 device checks (mobile, Windows desktop build)
2. Part B4-B5: election ballot-key dialog and save behavior

Every step ends with a check. Do not start the next step until the check passes.

## Before you start

- Read `.github/copilot-instructions.md` (commands, layers, conventions) and the memory rules it
  points to.
- Never commit. Stage one finished part at a time only when the owner asks.
- Never edit JSON data files, secrets or connection strings. `docs/api/swagger.json` and the
  authorization catalog are generated, so regenerating them is fine.
- Tag every new test with its FR or NFR in the same change:
  - Backend (NUnit): `[Category("FR-39")]` on the method or class.
  - Flutter: the prefix goes in the test name, `testWidgets('FR-39: ...')`.
  - Web: put the FR in the `describe` or `it` text.
  - Areas: authenticator and lockout FR-39; members and profile FR-01; news FR-27; security
    NFR-S5; election retry NFR-R5.
- Controllers never touch the DbContext. Put new data work in a service.
- New literals go in `GHCAA.Domain/Constants.cs` (backend), `app.constants.ts` (web) or
  `lib/core/constants/` (mobile). Grep for an existing constant first.
- Comments are plain and short. No em dashes.
- The book in `docs/book/` is frozen. Do not edit or build it.

### Commands

| What | Command |
| --- | --- |
| Backend tests, one area | `cd GHCAA.Tests; dotnet test --filter "FullyQualifiedName~AuthenticatorServiceTests"` |
| Web tests | `cd GHCAA.Web; npx vitest run src/app/admin/roles` |
| Mobile tests | `cd GHCAA.Mobile; flutter test --exclude-tags golden --reporter expanded` |
| Mobile on desktop | `cd GHCAA.Mobile; flutter run -d windows --no-hot` |
| API locally | `cd GHCAA.API; dotnet run --launch-profile https` (no hot reload: restart after backend edits) |
| CI gates | `ASPNETCORE_ENVIRONMENT=Development python scripts/ci-local.py --only backend` (also `web`, `mobile`, `contract`) |
| Authz catalog | `python docs/api/authz_catalog.py --write`, then `--check` |
| Graph refresh | `graphify update .` after code changes |

Stop the API before `dotnet test` if a build complains that a DLL is locked.

## Owner decisions recorded

These decisions are settled and guide the implementation steps below.

| # | Decision | Used by | Selected rule |
| --- | --- | --- | --- |
| D1 | A member cannot change their own NID today. `MemberService_Profile.cs` saves `IsNIDPublic` only; only the admin path writes `NID`. | B1 | Members may edit NID at any time. Validate the 10, 13, or 17 digit format and enforce uniqueness, including on new record entry. |
| D2 | The `PassingYear` lookup group is not in `Data/Seed/lookups.json`. Web has a fallback list; mobile has no fallback. | B2 | Use database rows when available, with fallback lists in both clients. |
| D3 | Which accounts can an admin unlock? | D1 | Any user account, including members and system administrators. |
| D4 | Does unlock also clear the authenticator app lock (`UserAuthenticators.FailedAttempts`, `LockedUntil`)? | D1 | Yes. One unlock clears login and authenticator locks together. |
| D5 | `ResetByAdminAsync` deletes the app and rotates the stamp in two saves. | C6 | Yes. Delete the authenticator and rotate the security stamp in one transaction. |

## Part A: 84.50 device checks

Goal: each check passes on the Windows desktop build (or a phone), or its defect is fixed with a
test that fails if it comes back.

Setup:

- Start the API (see Commands). The desktop app calls `http://localhost:5087/api`
  (`GHCAA.Mobile/lib/core/config/app_config.dart`).
- Run the app with `flutter run -d windows --no-hot`. Keep the window under 600 px wide, or the
  drawer is hidden as on a tablet.
- `scripts/local-election.py` makes a local election and borrows test accounts. Admin A is user
  2, admin B is user 1, the phone voter is user 200 and voters are 201-210. The test password is
  `LocalVote#2026`. Run `restore` when done, and `purge` to delete the election.

Record each result in TODO 84.50 as "passed on <device> <date>" or "fixed in <file>, test <name>".

### A1. 95.6 authenticator app and step-up

- Screen `lib/screens/member/authenticator_screen.dart` (route `/profile/authenticator`), service
  `lib/features/auth/authenticator_service.dart`, step-up in `lib/core/api/step_up_interceptor.dart`
  and `lib/core/widgets/step_up_dialog.dart`. API: `AuthController` step-up and authenticator
  routes.
- Steps: sign in, open the authenticator screen, scan the QR with a phone app (or type the
  secret), confirm with a code and keep the recovery codes. Trigger a step-up action and pass it
  with the app code. Trigger another and switch to an emailed code.
- Check: both step-ups pass, and the second shows the email method. Tests to keep green:
  `test/authenticator_screen_test.dart`, `step_up_dialog_test.dart`, `step_up_interceptor_test.dart`.

### A2. 84.42 mobile admin step-up routes

- Routes: `POST /roles/users` (`roles_service.dart`), `POST /roles/assign`, `POST /admin/elections`
  (`election_service.dart`).
- Gotcha: mobile `/admin` routes have no role guard, so a plain official gets a 403. A 403 is not
  a step-up failure and must not reopen the step-up dialog.
- Steps: as admin A, create a user, assign a role, create an election. Each asks for step-up once
  and then succeeds. Repeat one action as a non-admin and confirm a plain refusal, not a dialog
  loop.
- Check: three successes, one clean 403. If the dialog loops, fix the interceptor and add a case
  to `step_up_interceptor_test.dart`.

### A3. 84.44 ballot key on mobile

- Screen `lib/screens/admin/election_management_screen.dart` (key button `action-ballot-key`,
  progress dialog `ballot-key-progress`, count button), key tools
  `lib/features/elections/ballot_key.dart`.
- Run order:
  1. `python scripts/local-election.py prepare --no-key`
  2. App, as user 2: make the key, save the private key file, send the public key. If the change
     is held for a second person, approve it as user 1.
  3. `python scripts/local-election.py open`
- Check: the API shows a key fingerprint on the election, and the PEM file exists where you saved
  it.

### A4. 37.1m and 37.14o vote receipt and tracking codes

- Screens `lib/screens/member/election_screen.dart` (route `/election`, receipt) and
  `election_results_screen.dart` (route `/election/results`, tracking codes).
- Run order, continuing A3:
  1. App, as user 200: vote and check the receipt shows a tracking code and the PDF opens.
  2. `python scripts/local-election.py vote` (the other voters)
  3. `python scripts/local-election.py close`
  4. App, as user 2: count with the saved key file (this finishes A3).
  5. App: open the results and find user 200's tracking code in the list.
  6. `python scripts/local-election.py restore`, then `purge`.
- Widget test added 2026-10-26: `FR-016: lists every published tracking code` in
  `test/election_results_screen_test.dart`.
- Check: the code from step 1 is in the list; the widget test passes.

## Part B: 84.49 DTO rule fixes

### B1. Member NID editing and validation

- The placeholder is `IMPORT-<row>-<ticks % 100000>`, built in `MemberImportService.cs` from
  `Constants.Defaults.ImportPrefix` ("IMPORT").
- The rule stays: `BaseMemberDto.NID` must be 10, 13 or 17 digits.
- Backend: validate `dto.NID` on every member profile submission using the 10, 13, or 17 digit
  rule. Enforce uniqueness both when a record is first entered and on later edits.
- Web: keep NID editable in `member/profile/profile.html`, bind and validate it, and strip spaces
  as `register.ts` does. Show a clear format hint.
- Mobile: keep NID editable in `lib/screens/member/profile_edit_screen.dart`, validate the same
  formats, and show the validation hint in the app.
- Tests:
  - Backend: member profile update tests for valid, malformed, duplicate, first-entry, and changed
    NIDs. `[Category("FR-01")]`.
  - Web: `profile.spec.ts` covers format validation and profile submission.
  - Mobile: `test/profile_edit_screen_test.dart` covers format validation and profile submission.
    Wrap in `ProviderScope`.
- Check: backend, web and mobile tests above pass. In the running app, an imported member can
  save the profile after typing a real NID.

### B2. PassingYear up to this year + 6

- Backend: `GHCAA.Application/DTOs/AcademicRecordDto.cs`, `Validate`. Replace
  `DateTime.UtcNow.Year` in the condition and the message with the Dhaka year plus
  `Constants.Members.PassingYearFutureYears` (6). Put 1950 in a constant too. Check the nested
  class names in `Constants.cs` first and reuse one that fits.
- Web: `core/services/lookup.service.ts`, `academicYearsFallback()` starts at this year + 6.
  Update `lookup.service.spec.ts`, which asserts the first entry is this year.
- Mobile: `lib/features/lookups/lookup_service.dart`, `batchListProvider`. When the group comes
  back empty, build the same list (this year + 6 down to 1950).
- Tests: in `GHCAA.Tests/Validators/MemberDtoValidationTests.cs`, rewrite
  `AcademicRecord_PassingYearNextYear_IsRejected` to two cases: year + 6 accepted, year + 7
  rejected.
- Check: those tests pass; the web and mobile year dropdowns show this year + 6 at the top.

### B3. Dhaka date for the age and year rules

Do this before B2's backend change, since B2 uses it.

- Move `GHCAA.Infrastructure/Services/AssociationTime.cs` to `GHCAA.Domain/AssociationTime.cs`,
  namespace `GHCAA.Domain` (same place as `DuplicateKeyException.cs`; confirm its namespace).
  Delete the old file in the same change.
- Fix the usings in `ThemeService.cs`, `ElectionDocumentService.cs` and
  `GHCAA.Tests/Services/ElectionDocumentServiceTests.cs` (`AssociationTime_IsSixHoursAheadOfUtc`).
- In `BaseMemberDto.Validate`, use `var today = AssociationTime.FromUtc(DateTime.UtcNow).Date;`.
- In `MemberDtoValidationTests.cs`, build test dates from the same Dhaka date, or they fail near
  midnight. No clock seam; that would be a new abstraction for one test.
- Check: `dotnet build` clean and `dotnet test --filter "FullyQualifiedName~MemberDtoValidationTests|FullyQualifiedName~ElectionDocumentServiceTests"` passes.

### B4. Mobile key spinner closes through its own context

- `election_management_screen.dart`, `_withProgress`. Today the `finally` closes the dialog only
  `if (mounted)`, so it stays open when the screen is gone.
- Change: keep the dialog's own context from its builder (`builder: (ctx) { dialogContext = ctx; ... }`)
  and in `finally` pop through it when `dialogContext?.mounted` is true. The builder may not have
  run yet if the work ends at once, so handle a null context (wait one frame, then try again).
- Test in `test/election_management_screen_test.dart`:
  `'FR-39 (spec 023, 84.49): the key dialog closes after the screen is gone'`. Close the screen
  before the fake key work finishes and expect no `ballot-key-progress`.
- Check: the test passes.

### B5. 60 s timeout on the key save, and a retry that adds no second key

- Mobile: `lib/features/elections/election_service.dart`, `setBallotKey`. Pass
  `options: Options(sendTimeout: const Duration(seconds: 60), receiveTimeout: const Duration(seconds: 60))`
  to that one `post`. Do not put a timeout on making the key.
- Read `lib/core/api/retry_interceptor.dart` and confirm it does not retry a POST. If it does,
  exclude this call.
- The server stores one key per election, so two keys cannot exist. The real risk: a save that
  worked but timed out on the phone, then a retry, now counts as a key replace and asks for a
  second approver. Fix on the server: in `ElectionApprovalService.RunOrRequestAsync` (or at the
  top of `AdminElectionsController.SetBallotKey`), when the sent key's
  `BallotSeal.Fingerprint` equals the stored `BallotKeyFingerprint`, return success and create no
  approval. Check `EnsureKeyNotFrozenAsync` still lets that through.
- Mobile message: when `DioException.type` is a timeout, say "The server did not answer. Check
  the election before trying again." instead of the raw reason.
- Tests:
  - Backend, approval service tests: the same key sent twice leaves one fingerprint and no
    pending approval. `[Category("FR-39")]`.
  - Mobile `election_service_test.dart`: `setBallotKey` sends 60 s options.
  - Mobile `election_management_screen_test.dart`: a timeout shows the new message.
- Check: those tests pass.

When B1-B3 are done, update the non-election acceptance evidence in 84.49. The election-device
checks in B4-B5 remain open for the election delivery sequence.

## Part C: 84.51 lockout and smaller fixes

Note: the TODO says "the same Identity lockout as login". The project does not use ASP.NET
Identity. Login lockout is custom: `User.FailedLoginAttempts` and `User.LockoutUntil`
(`GHCAA.Domain/Models/User.cs`), counted in `AuthService.LoginAsync` with the literals 5 and 15.

Some of the "missing tests" in 84.51 already exist. Do not add copies:

- SetupAuthenticatorWithPassword wrong password, email on file, success:
  `AuthControllerMutationTests.cs` (about 495-525).
- NewsController.UploadImage 429: `NewsControllerTests.UploadImage_OverDailyLimit_Returns429WithoutSaving`.
- Proposer is candidate: `ElectionServiceTests.SubmitNominationAsync_RefusesASelfProposal` and
  `AddCandidateAsync_NeedsTwoOtherEligibleVotersAsProposerAndSeconder`.
- Step-up DTO with a recovery-code shape and method email: `StepUpVerifyDtoTests` in
  `AuthenticatorServiceTests.cs`.

Write each of these into TODO 84.51 as "already covered by <test>".

### C1. Shared login lockout

- Add `Constants.Login.MaxFailedAttempts = 5` and `Constants.Login.LockoutMinutes = 15` (check
  `Constants.cs` for a better home first). Use them in `AuthService.LoginAsync`.
- Move the count-and-lock into one method on a service both callers can reach (for example
  `IUserService.RecordFailedPasswordAsync(userId, ct)` and `ResetFailedPasswordsAsync`). Do it as
  one `ExecuteUpdate`, like `AuthenticatorService.RecordFailureAsync`, so parallel guesses cannot
  slip past the limit.
- Check: `AuthServiceTests.LoginAsync_AfterFiveFailedAttempts_ShouldLockOutForFifteenMinutes`
  still passes.

### C2. Lockout on setup-with-password

- `AuthController.SetupAuthenticatorWithPassword` and `AuthenticatorService.PasswordMatchesAsync`.
- New flow: refuse when `LockoutUntil` is in the future; on a wrong password call the shared
  counter from C1; on success reset the count and write an activity row through
  `IActivityService.LogActivityAsync` with a new constant next to
  `Constants.Authenticator.AdminResetAuditType` (for example `PasswordSetupAuditType`). System
  admins have no member, so pass `user.MemberId` as is (null is accepted, as `ResetByAdminAsync`
  shows).
- A locked account gets a clear refusal: "Too many wrong passwords. Try again in 15 minutes or
  ask an admin to unlock the account." Use `ProblemWithCode` with an error code if one fits in
  `ErrorCodes`; add one if not.
- Tests, `[Category("FR-39")]`: locked account refused without checking the password; a wrong
  password raises the count; the fifth locks login too; success writes the activity row. Update
  the mocks in `AuthControllerMutationTests.cs` for any new interface method.
- Check: `dotnet test --filter "FullyQualifiedName~AuthControllerMutationTests|FullyQualifiedName~AuthenticatorServiceTests|FullyQualifiedName~AuthServiceTests"`.

### C3. App-code lockout race

- `AuthenticatorService.VerifyAsync` and `ConfirmAsync` read `LockedUntil` with `AsNoTracking`,
  check it, then count the failure. Parallel requests all read "not locked" and each gets a guess.
- Fix: before checking the code, reserve the attempt in one `ExecuteUpdate` that raises
  `FailedAttempts` (and sets `LockedUntil` at the limit) only
  `Where(a => a.Id == id && (a.LockedUntil == null || a.LockedUntil <= now))`. Zero rows means
  locked: refuse. On a right code, reset `FailedAttempts` to 0 in the success update, which also
  carries the "not locked" condition. Keep `IgnoreQueryFilters()` where it is used today.
- The test base (`GHCAA.Tests/TestBase.cs`) is SQLite in memory, which is relational, so
  `ExecuteUpdate` works there already. The TODO's note about EF InMemory is out of date.
- Test, `[Category("FR-39")]`: open a second context on the same connection (as
  `ElectionServiceTests.ContextWith`), fire more than `MaxFailedAttempts` wrong guesses with
  `Task.WhenAll`, then send the right code: it is refused. SQLite runs one command at a time, so
  this proves the SQL guard, not true parallel timing. Keep the existing lock tests green
  (`Verify_LocksAfterMaxWrongCodes_EvenForTheRightCode`, `Verify_GoodCodeClearsFailureCount`).
- Check: `dotnet test --filter "FullyQualifiedName~AuthenticatorServiceTests"`.

### C4. Small backend fixes

Each one gets a test in the same change.

1. `AuthController.RequestStepUp`: remove the unused `IAuthenticatorService` parameter and fix
   the five calls in `AuthControllerMutationTests.cs`.
2. `MemberRegistrationDto.Validate`: `AcademicHistory[0] is null || !AcademicHistory[0].IsOrgProfile`.
   Also check `BaseMemberDto.Validate` for a loop over entries that would hit a null. Test with
   `[null]` in the member DTO validation tests: a validation error, not an exception.
3. `NewsController.UploadImage`: a failed save still uses a quota slot. Add
   `INewsService.ReleaseImageUploadAsync(authorId, ct)` and call it in a catch around
   `SaveFileAsync`, then rethrow. Test: a throwing storage mock leaves the count unchanged.
   `[Category("FR-27")]`.
4. `AuthenticatorService.BeginSetupAsync`: catch only a unique violation (PostgreSQL `23505`,
   SQLite error 19) as "already set up". Rethrow anything else. Confirm the unique index on
   `UserAuthenticators.UserId` in `ApplicationDbContext`. Test: a non-unique `DbUpdateException`
   is not turned into null.
5. `PaymentConfigController` create and update catch every `ArgumentException`. Add a
   `GatewayNotBuiltException` next to `DuplicateKeyException` in `GHCAA.Domain`, throw it in
   `PaymentConfigService` where Nagad is refused, and catch that type instead. Test: another
   `ArgumentException` is not turned into a 400. Report the same pattern in
   `AdminGovernanceController`, `CampaignsController` and `ScholarshipsController` to the owner;
   do not change them here.

Check: `ASPNETCORE_ENVIRONMENT=Development python scripts/ci-local.py --only backend`.

### C5. Missing controller tests

- `RolesControllerTests.cs`, `ResetAuthenticator`: success gives 204 and calls
  `ResetByAdminAsync(id, adminId)`; service false gives 404; no NameIdentifier claim gives 401.
  The class does not inherit `ControllerTestBase`, so set a `ClaimsPrincipal` on
  `ControllerContext` by hand. `[Category("FR-39")]`.
- `FamilyLinkControllerTests.cs`, `GetPublicFamily`: SuperAdmin with no member id gives Ok and
  calls the service with `isAdmin: true`; a plain user with no member id gives 401; a member
  passes `isAdmin: false`. Grep the specs for the family FR number.
- Step-up verify, recovery-code shape with method email, at the controller: a direct controller
  call skips model validation. If `StepUpVerifyDtoTests` is judged enough, record that in the
  TODO; otherwise add an integration test through `ErrorResponseShapeTestFactory`.
- Check: `dotnet test --filter "FullyQualifiedName~RolesControllerTests|FullyQualifiedName~FamilyLinkControllerTests"`.

### C6. ResetByAdminAsync in one transaction

- `AuthenticatorService.ResetByAdminAsync` runs delete, stamp rotate, token revoke and audit as
  separate saves.
- Wrap them in one transaction run through the execution strategy
  (`db.Database.CreateExecutionStrategy().ExecuteAsync(...)`). The strategy will not retry inside
  a transaction you opened yourself; see `ElectionAuditWriter` for the project's pattern.
- Keep the three existing tests green (`ResetByAdmin_RemovesApp_SignsUserOut_AndWritesAuditRow`,
  `ResetByAdmin_RevokesEveryRefreshToken`, `ResetByAdmin_FalseAndNoSideEffects_WhenNoApp`).

When Part C is done, update 84.51 in `docs/TODO.md`: each bullet fixed with its test, or left by
the owner with the reason.

## Part D: admin unlock

### D1. API

- Service: `IUserService.UnlockUserAsync(int userId, int adminId, CancellationToken ct)`. It sets
  `FailedLoginAttempts = 0` and `LockoutUntil = null`, and (per D4) clears `FailedAttempts` and
  `LockedUntil` on the user's `UserAuthenticators` row. It writes an activity row with a new
  constant (for example `Constants.Authenticator.AdminUnlockAuditType`). Returns false when the
  user does not exist.
- Controller: `RolesController`, next to `ResetAuthenticator`:
  `[HttpPost("users/{id}/unlock")] [RequireStepUp] UnlockUser(int id, CancellationToken ct)`.
  Read the admin id from the NameIdentifier claim as `ResetAuthenticator` does. 204 on success,
  404 when not found. The class policy is `SuperAdminOnly`.
- `GetUsers` today returns no lock state. Add `LockedUntil` (only when it is in the future) so
  clients know when to show the button.
- Authz map: add `[("RolesController", "UnlockUser")] = new(Constants.Policies.SuperAdminOnly, false, true),`
  to `AuthorizationPolicyReflectionTests.cs`, in order after `ResetPasswordAdmin`. That file
  already has uncommitted edits; add to them.
- Regenerate: `python docs/api/authz_catalog.py --write`, then accept the new swagger through
  `ci-local.py --only contract` (it writes the generated file; copy it over
  `docs/api/swagger.json` once the diff shows only the new route and field).
- Tests:
  - User service: unlock clears both locks and writes the row; unknown user gives false.
    `[Category("FR-39")]`.
  - `RolesControllerTests`: 204, 404, 401 without a claim.
  - `DestructiveStepUpActionsTests`: `UnlockUser` requires step-up.
- Check: `ci-local.py --only backend` and `--only contract`.

### D2. Web

- `core/services/admin.service.ts`: `unlockSystemUser(userId)` posts to
  `${API_ENDPOINTS.ROLES}/users/${userId}/unlock` with the same `X-Skip-Error-Notify` header as
  `resetSystemUserAuthenticator`.
- `admin/roles/admin-roles.ts`: `unlockUser(user)` copies `resetAuthenticator(user)`: confirm
  dialog, call, success or error notice, then reload the list.
- `admin/roles/admin-roles.html`: an `icon-btn` "Unlock account", shown only when
  `user.lockedUntil` is in the future. Use the existing button classes and theme tokens; no new
  styles.
- Tests in `admin-roles.spec.ts`, copied from the three `resetAuthenticator` tests: confirm and
  call, cancel calls nothing, error shows a notice. Plus: the button is hidden for an unlocked
  user. Use `componentRef.setInput` for signal inputs.
- Check: `npx vitest run src/app/admin/roles`, then the button in the running app (lock a test
  user with five wrong logins first).

### D3. Mobile

- `lib/features/admin/roles_service.dart`: `Future<bool> unlockUser(int userId)` posts
  `/roles/users/$userId/unlock`. Step-up is handled by `StepUpInterceptor`.
- `lib/screens/admin/permissions_matrix_screen.dart`: an `AdminActionCircle` on the user card,
  shown when the user is locked. Confirm with `showConfirmDialog`, guard with `_rolesActionBusy`,
  then `ref.invalidate(_usersProvider)` and a snackbar, as `_removeRole` does.
- Update the fakes in `test/helpers/fake_services.dart` and any visual-freeze tests that build a
  `RolesService`.
- Tests: `'FR-39: unlock shows for a locked user and calls the service'`, and the button is
  hidden when not locked.
- Check: `flutter test --exclude-tags golden`, then the action on the desktop build.

When Part D is done, add the unlock route to `docs/PROJECT_MAP.md` and record it in 84.51.

## Finish

- `graphify update .`
- `ASPNETCORE_ENVIRONMENT=Development python scripts/ci-local.py` (all gates). Report any gate
  that fails with its output; do not silence it.
- Update `docs/TODO.md` for each item touched, with the date and test names.
