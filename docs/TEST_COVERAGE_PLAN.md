# Test coverage plan

This is a technical reference for Work Package 27 in `docs/TODO.md`. It does not add a second task list. The tracker owns status, priority, dependencies and acceptance records.

## Goal

Reach 90% line and branch coverage for owned production logic in the API, web and mobile clients. The number must reflect real decision paths. Do not exclude business code merely to raise it.

The target does not include generated files, migrations, build output, test code, platform runners, DTO-only data containers, or static configuration shapes. Any new exclusion needs a written reason in the Work Package 27 record.

## Baseline and reporting

| Layer | Collector | Current baseline | CI output |
| --- | --- | --- | --- |
| Backend | Coverlet, Cobertura | 59.99% lines, 39.61% branches on 2026-09-16 | HTML report artifact |
| Web | Vitest V8 | 42.25% lines, 28.44% branches on 2026-09-16 | JSON summary, LCOV and HTML artifact |
| Mobile | Flutter LCOV | 14.23% lines on 2026-09-16 | `lcov.info` artifact |

Flutter's built-in LCOV report measures lines, not branches. Mobile branch coverage is therefore tracked through explicit decision-path cases until a reliable Dart branch collector is adopted. Do not report a made-up mobile branch percentage.

## Completed increments

### 2026-09-16: Collection and poll mutation coverage

- Added Vitest V8 collection through `npm run test:coverage`. The first web report established the baseline shown above. Collection is reporting-only while the team classifies legacy gaps and agrees the changed-file denominator.
- Collected the first backend Cobertura report: 7,272 covered lines out of 12,122 and 1,768 covered branches out of 4,463.
- Added the web and mobile coverage artifacts to the standard CI workflow. The first mobile LCOV report contains 1,392 covered lines out of 9,781.
- Closed Work Package 47.13.4 with `AdminPollControllerTests`. The seven focused tests cover the controller contract only: listing, the member claim passed to creation, both toggle states, a missing toggle target, and both delete outcomes. `PollServiceTests` remains the owner of service behavior.
- Extended the existing `AdminPolls` spec rather than creating a second spec. The four new cases cover required-input rejection, successful create/reset, failed create cleanup, and double-submit prevention. The focused Vitest run passed 7/7.
- Re-ran web collection after the poll cases: 42.18% lines and 28.44% branches. The increment added 14 covered lines and 6 covered branches without changing the denominator.
- Completed the `PollService` decision map in `PollServiceTests`: read filters and member mapping, zero-vote calculation, all selection rejection paths, multi-choice participant counting, and persistence of toggle/delete state. The focused NUnit run passed 18/18. Test members now come from the local named fixture helper rather than scattered identity strings.
- Added the Angular `PollService` HTTP-contract spec using the centralized API endpoints; focused Vitest passed 3/3. Full web collection then reached 42.25% lines and 28.44% branches (2,453/5,805 lines; 866/3,045 branches).
- Completed the first yield-first backlog item with `FileValidationServiceTests`. Fourteen parameterized NUnit cases cover all accepted category signatures and size, content-type, extension and magic-byte rejection paths; focused run passed 14/14. Its direct report is 95.65% lines and 81.39% branches, clearing the planned changed-file threshold without a duplicate low-yield stream test.
- Completed the matching Angular `file-validation.util` seam with nine parameterized Vitest cases for every allowed kind at its size boundary plus type and size rejection. Focused coverage is 100% lines and 100% branches.

Reusable pattern: extend the existing screen or controller test class when it already owns the symbol. Use a local named fixture for test-only values and production constants for real routes, actions, DTOs and shared values.

## Poll module test design map

This is the implementation-ready design for the current single-module increment. It deliberately records test cases rather than work status; Work Package 27 in `docs/TODO.md` remains the only task tracker. Before adding any case, search the named test file and merge it if its setup and assertion already exist.

### Backend: `PollService` — complete for this increment

`GHCAA.Tests/Services/PollServiceTests.cs` now owns creation persistence, active/admin reads, archived and expiry filters, missing IDs, member selection mapping, zero and multi-choice vote percentages, every selection validation branch, duplicate voting, and toggle/delete outcomes. Do not add overlapping service tests. The next backend Poll work begins at the member-facing `PollController` contract below.

### API controllers — complete for this increment

`GHCAA.Tests/Controllers/PollControllerTests.cs` owns the member-facing active-list, by-ID and vote contracts: absent claim rejection, claim forwarding, found/missing mapping, and accepted/rejected vote mapping. It passes 13/13 together with the 18 `PollService` tests. `AdminPollControllerTests` remains the owner of admin list/create/toggle/delete; do not cross-test those contracts.

### Web: Angular poll client

`GHCAA.Web/src/app/core/services/poll.service.spec.ts` now owns the active list, by-ID, and vote HTTP shapes using `API_ENDPOINTS`; its focused Vitest run passes 3/3. Do not duplicate those route/body checks in component tests.

| Production symbol | Test location and case name | Required setup | Exact assertion |
| --- | --- | --- | --- |
| admin list failure | extend `admin/polls/polls.component.spec.ts`: `clears loading and reports when list loading fails` | `getAllPolls` returns `throwError`; the existing named `TEST_POLL` remains the only component fixture | Call `loadPolls`; `loading()` is false and `notify.error` receives `Failed to load polls.`. |
| option minimum | same spec: `does not remove either required option` | Default `newPoll.options` is the existing two empty entries | Call `removeOption(0)` and `removeOption(1)` | Both entries remain; no service call. |
| option add/remove | same spec: `adds one option and removes only a third option` | Set options to `[FIRST_OPTION, SECOND_OPTION, THIRD_OPTION]` from constants beside `TEST_POLL` | `addOption` creates the fourth empty slot; `removeOption(1)` removes only `SECOND_OPTION`. |
| status mutation | same spec: `it.each` named `activates or deactivates exactly once` | Use `TEST_POLL` once active and once inactive; `toggleStatus` returns `of(void 0)` | Service gets `(id, !isActive)`, `togglingId()` returns null, the same poll changes state, success is `Poll activated.` or `Poll deactivated.`. |
| status failure and busy guard | same spec: `clears toggle state after a failed update` and `does not toggle while another poll is pending` | First mock returns `throwError`; second sets `togglingId` to a different named ID | Failure clears state and calls `notify.error('Failed to update status.')`; guard makes no call. |
| delete cancel | same spec: `does not delete after a cancelled confirmation` | Confirm mock returns `of(false)` | Await `deletePoll(TEST_POLL.id)`; `deletePoll` service mock is untouched and `deletingId()` remains null. |
| delete success/failure | same spec: `reloads after deletion` and `clears delete state after deletion failure` | Confirm returns `of(true)`; service returns `of(void 0)` or `throwError` | Success calls `getAllPolls` again and `notify.success('Poll deleted.')`; failure clears state and calls `notify.error('Failed to delete poll.')`. |
| member list outcome | extend existing `member/polls/polls.component.spec.ts`: `stores active polls` and `clears loading on list error` | Add one typed `POLL_FIXTURE` shared by every member case; `getActivePolls` returns `of([fixture])` or `throwError` | Success replaces `polls`; error clears loading and calls `notify.error('Failed to load active polls.')`. |
| member selection | same spec: `replaces a single-choice selection`, `toggles a multi-choice selection`, and `does not change an already-voted poll` | Use cloned named single/multiple/voted fixtures, never mutate the shared base fixture | Assert selected IDs after each call and `isOptionSelected` for selected/unselected values. |
| empty/double vote guard | same spec: `warns without voting when no option is selected` and `does not vote while another poll is pending` | Empty selected IDs; then set `votingId` to another named poll ID | Warning is `Please select at least one option.` and neither case calls `vote`. |
| vote accepted + refresh accepted | same spec: `replaces the voted poll with refreshed results` | `vote` returns `of({})`, `getPollById` returns a distinct `VOTED_POLL_FIXTURE` | Service receives exact ID and selected IDs; success message is `Thank you for voting!`; entry at its original index becomes refreshed data; `votingId()` is null. |
| vote accepted + refresh failed | same spec: `reports a refresh failure after an accepted vote` | `vote` returns `of({})`, `getPollById` returns `throwError` | `votingId()` clears and error is `Failed to refresh poll results.`; original list stays unchanged. |
| vote rejected | same spec: `uses the API rejection message or the fallback` as `it.each` | `vote` returns `throwError` with `{ error: { message: API_ERROR } }` and with no message | `votingId()` clears; notification is the API message or `Failed to submit vote.`. |

### Mobile: Flutter poll client

| Production symbol | Test location and case name | Required setup | Exact assertion |
| --- | --- | --- | --- |
| centralized paths | add `core/api/api_endpoints.dart`, then update `features/polls/poll_service.dart` | Define `ApiEndpoints.pollsActive` and `ApiEndpoints.pollVote(int)` once; retain existing `ApiClient` base URL ownership | `rg` finds no raw `/polls/` in production files other than that definition. |
| model complete shape | new `test/features/polls/poll_service_test.dart`: `Poll.fromJson maps every API field` | One named complete JSON fixture with two option maps, selected IDs and ISO timestamps | Every `Poll`/`PollOption` field maps, including nullable expiry and percentages. |
| model fallback shape | same file: `Poll.fromJson uses documented defaults for optional API values` | One named minimal JSON fixture with mandatory `id`/`createdAt` only | Title is empty; booleans false; options and selection empty; counts and percentages zero; description/expiry null. |
| list success | same file: `getActivePolls maps a successful list response` | Shared fake Dio adapter returns `Response(statusCode: 200, data: [COMPLETE_POLL_JSON])` | Result has one mapped poll and adapter observed `ApiEndpoints.pollsActive`. |
| list non-200/error | same file: `getActivePolls returns empty for non-200` and `rethrows a transport error` | Adapter returns non-200 then throws a named `DioException` | First result empty; second expectation is `throwsA(same(exception))`. |
| vote request | same file: `vote posts selected IDs through ApiEndpoints.pollVote` | Adapter captures `path` and `data`, returns 200 | Result true; path is `ApiEndpoints.pollVote(POLL_ID)`; body exactly `{ 'optionIds': OPTION_IDS }`. |
| vote rejection/error | same file: `vote returns false for non-200 or transport error` as a table-driven loop | Adapter returns a named non-200 code and throws | Each result false; neither throws. |
| provider UI states | new non-golden `test/features/polls/polls_screen_test.dart`; reuse `FakePollService` already used by visual tests | Override `pollServiceProvider` with loading/error/empty/data futures | Assert spinner, retry text, empty text, and poll title respectively; retry invokes a second service read. |
| widget selection + submit | same file: `selects single/multiple choices`, `does not submit empty`, `refreshes after success`, `shows failure` | Named single/multiple/voted fixtures plus fake service recording `vote` | Single replaces, multiple toggles, voted view has no submit button, empty shows selection snackbar, success invalidates/reloads and shows success snackbar, failure shows failure snackbar. |

For the Flutter adapter, move an already-existing test-only fake into `test/helpers/` if it is currently file-local; do not create a second fake Dio implementation. Keep test fixture labels, IDs and messages in that shared helper or the spec's single named fixture block.

## Commands

Run from the repository root unless stated otherwise.

```powershell
dotnet test GHCAA.Tests/GHCAA.Tests.csproj --configuration Release --collect:"XPlat Code Coverage" --settings GHCAA.Tests/coverlet.runsettings
cd GHCAA.Web; npm run test:coverage
cd GHCAA.Mobile; flutter test --coverage --exclude-tags golden --reporter expanded
```

Use the reports to choose work. Test count is not a coverage target.

## Test selection rule

SR-6 in `docs/TODO.md` applies to every change.

1. Start with an uncovered decision branch and name the production symbol it belongs to.
2. Search by symbol, route, DTO and message text before adding a test.
3. Extend the existing test when the setup and behavior already match.
4. Parameterize equivalent values with NUnit `[TestCase]`, Vitest `it.each`, or a Dart table-driven loop.
5. Add a new test only for a distinct branch, authorization boundary, state transition, integration seam, or regression.
6. Reuse shared fixtures, API constants, route constants, DTOs and test helpers. A repeated magic string belongs in the existing central definition, or in a small shared test fixture when no production constant fits.
7. Run the focused test first, then the relevant full suite. Confirm coverage does not fall.

A test that repeats a successful call without a new assertion or decision path is removed or merged. Two tests that look alike but protect separate failure modes stay separate.

## Repository-wide implementation backlog

This is the requested repository-wide test TODO design. It is deliberately executable rather than a second status tracker: each row names the production ownership, existing-or-new test home, fixture seam, and the exact success/failure/branch set to cover. Mark completion only in Work Package 27 or its linked work package in `docs/TODO.md`.

### Universal implementation recipe

For every row below, use the named production constants, DTOs and route/action constants. Put repeated test-only values in one fixture block at the top of the existing test file; do not copy labels, IDs, paths, HTTP messages or role names between cases. One normal success plus each distinct `if`/`switch` outcome is the minimum set. Parameterize value-only variants. Every mutation case verifies both the return/result and persisted or forwarded state.

### Yield-first execution order

This order overrides directory order. Start at rank 1 and do not move down merely to spread work around. A candidate is ranked by **newly covered production branches and lines ÷ number of distinct tests**. A parameterized test counts as one test when it shares setup and assertion shape. Move a candidate down when search finds an existing test that already owns its branch.

| Rank | Work packet | Test budget | Why it is first | Finish before moving on |
| --- | --- | --- | --- | --- |
| 1 | Pure mapper/validator/utility/fallback decisions: backend DTO `Validate` methods, `FileValidationService`, Angular `file-validation.util`, `export.util`, `org-template`, Flutter JSON model factories | 2–6 per symbol | One compact table-driven test usually covers every branch without database, DOM or HTTP setup. | Valid, null/empty, boundary and malformed/unknown paths are all asserted. |
| 2 | Zero-coverage narrow services and guards: Angular `feature.guard`, `health.service`, `forum.service`, `elections.service`, `admin-social-auth.service`; Flutter service parsing; backend `LookupService`, `ContactService`, `FileValidationService`, `DeviceTokenService`, `OtpService` | 3–6 per symbol | Small public surface and a high uncovered denominator; mock/HTTP tests cover whole methods cheaply. | Every public method has request/result plus its error/fallback branch. |
| 3 | Controller contract families with few actions: `Health`, `Assistant`, `SiteContent`, `Theme`, `Contact`, `Notification`, `Messaging`, `Mentorship`, `Poll`, and Angular HTTP services | 3–8 per controller/service | One mocked success and one failure/claim case covers many lines and decision branches. | Claim/role, success, missing/false and validation outcome are proven for each action. |
| 4 | Compact state components: Angular shared components and public landing sections; Flutter providers/shared widgets | 2–6 per component | Inputs/outputs and loading/error/empty/data often cover an entire small file with no database. | Every conditional render and emitted action is asserted. |
| 5 | Medium business services: family, forum, networking, jobs, news, gallery, campaign, event, payment/ledger | 6–12 per service, parameterized | Rich branch density; use the existing SQLite fixture once the cheap seams above are complete. | State transitions, ownership, missing data and provider failure branches are covered. |
| 6 | Large admin/member Angular screens and Flutter screens: members, register, profile, payments, admin events/news/gallery/governance | 8–15 per screen, split only by distinct state machine | High denominator but more setup; test their state transitions after services already establish API behavior. | Init/load error/empty/data, validation, mutation success/error/busy, modal/confirmation and permission branches. |
| 7 | File/PDF/email/SMS/gateway integration seams | 3–8 per seam | Valuable but fixtures and external adapters cost more; retain only decisions lower-level tests cannot make. | Local adapter success/failure and no-leak/no-partial-state guarantees pass. |
| 8 | End-to-end/integration journeys | 1 per unique journey | Lowest line/branch yield; protects contracts between layers only. | A real user-visible workflow passes against deterministic data and does not duplicate unit/widget branches. |

Within the detailed tables, process rows in this rank order rather than their visual order. Record the actual covered-line/branch delta after each focused batch; if another row produces a better delta with the same or fewer tests, promote it to the next rank.

### Backend service backlog

| Owner production files | Test home | Fixture/seam | Required cases |
| --- | --- | --- | --- |
| `MemberService*.cs`, `MemberImportService.cs` | extend `MemberServiceTests`; add focused `MemberImportServiceTests` only if absent | SQLite `TestBase`; named active/inactive/member-with-files fixtures; mock mail/storage only | Search/filter pagination; self/admin ownership; archive/restore/reactivate missing+success; approval prerequisites; bulk archive empty/non-empty; import valid/invalid/duplicate rows; photo/signature/document upload type/size/ownership/error cleanup; dashboard/public statistics empty/non-empty. |
| `AuthService.cs`, `OtpService.cs`, `TokenService.cs`, social-auth config | extend existing auth tests | Mock JWT/config/email/HTTP clients; named locked/inactive/social/system-admin users | Bad password, inactive, lockout threshold/reset, expired/invalid refresh, missing email, OTP expiry/reuse, reset scope, Google/Facebook provider error/token mismatch, token claims and configured expiry fallback. |
| `FinancialService.cs`, `FinancialLedgerService.cs`, `PaymentCallbackOrchestrator.cs`, `PaymentConfigService.cs` | extend financial/payment service tests | SQLite payment/member/config fixtures; mock gateway/storage/email | Missing/disabled config, amount/category validation, idempotent callback, paid/failed/refunded transitions, membership approval side effect, receipt present/missing, ledger filters/pagination/export no-data/data, duplicate callback rollback. |
| `EventService.cs`, `CampaignService.cs`, `GamificationService.cs` | extend event/campaign tests | Event with capacity/dates/tasks/budget/registrations; named campaign/tier fixture | Create/update/delete missing+success; capacity/expiry/duplicate registration; ticket lookup/check-in invalid/already checked-in; task and budget mutation; participant visibility; invitation failure; campaign/tier validation and donation threshold transitions. |
| `NewsService.cs`, `GalleryService.cs`, `SiteContentService.cs`, `ThemeService.cs` | extend news/gallery/content tests | Named author/admin/other-member and published/draft/archived fixtures; mock file store | Visibility by status/ownership; create/update/delete forbidden/missing/success; collaborator add/remove duplicate/missing; approval/rejection reason; upload file validation/store failure; active-theme/content fallback and cache invalidation. |
| `ForumService.cs`, `Messaging/ChatService.cs`, `NotificationService.cs`, `DeviceTokenService.cs`, `CommunicationService.cs` | extend or add one test class per service | Two-member conversation/forum fixtures; mock email/SMS/push | Thread/category absent, access denied, reply ordering, edit/delete ownership, unread count/read transition, duplicate device token, notification preference, template placeholder validation, provider failure without false success. |
| `FamilyService.cs`, `FamilyLinkService.cs`, `MentorshipService.cs`, `NetworkingService.cs` | extend family/mentorship/networking tests | Requester/recipient/stranger fixtures with pending/accepted/declined states | Request self/duplicate/target missing; accept/decline/cancel/unlink access and state transitions; list visibility; mentor request/response/complete ownership; profile not found and EC period/current-period fallbacks. |
| `GovernanceService.cs`, `RoleService.cs`, `LookupService.cs`, `OrgConfigService.cs` | extend governance/role/lookup/config tests | Role/period/version/lookup fixtures; named protected admin | Protected role cannot be removed; duplicate role/assignment; lookup group/item missing and delete success; active period/version selection; constitution vote eligibility/duplicate; org config default/invalid/updated values. |
| `JobHubService.cs`, `ContactService.cs`, `ActivityService.cs`, `AssistantService.cs` | extend or add matching service tests | Named public/member/admin records; mock assistant provider | Search null/empty/filter/page boundaries; job/contact ownership and status transition; contact read state; activity time/filter boundaries; assistant empty prompt, provider failure, safe fallback and response persistence. |
| `FileValidationService.cs` — complete; `LocalFileStorageService.cs`, `IDCardService.cs`, email/SMS/health providers | `FileValidationServiceTests` exists; add focused tests beside the remaining services | Temp directory only; in-memory stream; mocked external clients | Do not duplicate `FileValidationService`'s allowed/disallowed extension/MIME/size/signature tests. For the other files, cover missing/deleted file, path traversal, PDF/data-URI absent image, provider configured/unconfigured, external exception and health timeout/failure. |

### Backend controller/API backlog

| Controllers | Test home | Exact contract cases for every public action |
| --- | --- | --- |
| `AdminController`, `MemberImportController`, `ProfileController`, `SecureFilesController` | extend controller tests named after each controller | Missing/invalid member claim → 401; role policy via integration/authorization test; valid claim forwards correct ID; upload uses named valid/invalid file fixture; service null/false → documented 404/400; success result type, route value, body and cancellation token. |
| `EventsController`, `CampaignsController`, `FinancialsController`, `FinancialLedgerController`, `GatewaysController`, `PaymentConfigController` | existing controller test class per controller | For each GET: result list/item and missing. For each POST/PUT/PATCH/DELETE: valid success, validation/model failure, service false/missing, and caller/role boundary. Gateways additionally covers callback bad signature, duplicate callback and redirect/result mapping. |
| `NewsController`, `GalleryController`, `SiteContentController`, `ThemeController`, `CommunicationController`, `ContactController` | existing/new controller test per controller | List visibility; create/update/delete success+service failure; upload valid/invalid; approval/rejection status; template/content/theme fallback; caller identity forwarded; error response never leaks provider exception. |
| `ForumController`, `MessagingController`, `NotificationController`, `FamilyLinkController`, `MentorshipController`, `NetworkingController` | one existing/new test file per controller | No claim 401, resource not found 404, forbidden service outcome 403/400 as implemented, success maps DTO, mutation passes body/claim exactly, mark-read/respond/complete paths return correct HTTP status. |
| `AuthController`, `RegistrationController`, `RolesController`, `LookupsController`, `GovernanceController`, `OrgConfigController`, `AdminGovernanceController`, `AdminSocialAuthController`, `PendingApprovalsController`, `ActivityController`, `AssistantController`, `HealthController` | extend current named controller test files | One success plus each documented invalid/missing/provider-failure response per action; assert policy/claim boundary for privileged actions; parameterize duplicate valid/invalid state values; health uses a fake dependency and proves degraded/unhealthy mapping. |

### Angular backlog from the current V8 report

| Production area (all listed files) | Test home and shared setup | Required branch cases |
| --- | --- | --- |
| App shell/layout/routing: `app.ts`, `app.routes.ts`, `app.config.ts`, all `layouts/*`, `feature.guard.ts`, `global-http.interceptor.ts`, `global-error-handler.ts`, `branding-title.strategy.ts` | Add/extend `app.spec.ts`, route/guard/interceptor specs. Use `ROUTES`, `API_ENDPOINTS`, named auth-state fixture and `HttpTestingController`. | Authenticated/anonymous/role/feature-enabled navigation; redirect target; request token/header and excluded URL; 401 refresh/failure; handled/unhandled error notification; title default/organization override; every lazy route loads or produces the explicit fallback. |
| Core HTTP services: `admin-social-auth`, `campaign`, `constitution`, `elections`, `family-link`, `forum`, `gallery`, `gateways`, `health`, `mentorship`, `news`, `payment-config`, `profile`, `site-content`, `contact`, `financial`, `job`, `networking` | One `*.service.spec.ts` beside every service without one; use one `HTTP_FIXTURE` factory and `API_ENDPOINTS`. | Every method: verb, central endpoint, path/query/body; one 2xx mapping and one non-2xx subscriber error/fallback only when production implements a fallback. Verify special headers, multipart body and optional query omission/inclusion. |
| Core state/services: `admin.service`, `auth.service`, `chat.service`, `confirm-dialog.service`, `nav.service`, `alert.service` | Extend existing service specs; use named role/user/conversation/dialog fixtures. | Initial state; loading→success; loading→error; empty data; duplicate/busy guard; local-storage absent/corrupt; permission/role branches; dialog accept/cancel; chat reconnect/disconnect/error. |
| Core constants/utils: `actions.constants.ts`, `app.constants.ts`, `export.util.ts`, `org-template.ts`; `file-validation.util.ts` is complete | Extend existing constants/util specs; no duplicated literals—import the production constants. | Do not duplicate `file-validation.util`'s allowed-type and size-boundary cases (100% direct line/branch coverage). For the remaining files cover every mapping fallback, enum/string/number variant, date/format boundary, CSV/Excel empty/data/error, and template token present/unknown/missing. DTO-only/static route shapes remain excluded only after documented approval. |
| Admin operations: `admin-audit`, `admin-campaigns`, `admin-contact-messages`, `admin-event-operations`, `admin-events`, `admin-gallery`, `admin-member-approval`, `admin-members`, `member-import-modal`, `admin-news`, `admin-payment-config`, `admin-governance`, `admin-roles`, `admin-site-content`, `admin-themes`, `admin-comm`, `admin-ledger`, `admin-org-config`, `admin-dashboard`, `admin-error-logs`, `article-approval`, `gallery-approval`, `job-approval` | Extend existing spec per component; create one only where absent. Reuse each component's existing service mock and a named `ADMIN_RECORD_FIXTURE`. | Init load success/error; search/filter/sort/page including empty; form required/invalid/valid; confirm cancel/success/error; mutation busy guard; status toggle both values; upload valid/invalid/progress/error; modal reset/close; permission-dependent controls. |
| Shared components: `breadcrumb`, `footer`, `health`, `payment-status`, `user-menu`, `gallery`, `jobs`, `pagination`, `payment-portal`, `rich-text-editor`, `step-up-dialog`, `toast`, `confirm-dialog`, `search-bar`, `notify-toggle`, `events`, `governance`, `directory` | Add lightweight component specs using inputs/outputs and central route/action constants. | Required/optional input render; output event payload; disabled/busy states; empty/error/data states; pagination first/middle/last; editor sanitization/value change; dialog confirm/cancel/escape; status/color fallback; navigation target. |
| Member portal: `assistant`, `articles`, `change-password`, `dashboard`, `digital-id`, `forum/*`, `giving`, `messages`, `payments`, `polls`, `profile`, `requests` | Extend existing member specs; one named member fixture, API error fixture and deferred observable helper. | Init/list success/error/empty; ownership/selection tabs; input validation; submit success/error/busy; pagination; document download failure; chat/message ordering; payment/receipt state; profile dirty/save/reset; request accept/decline/cancel. Poll remains governed by the detailed map above. |
| Public pages: `about`, `contact`, `directory`, `elections`, `magazine`, `reset-password`, `register`, `login`, `constitution`, every `landing/sections/*` | Existing or new public-page spec; central public content and route fixture. | Loading/error/empty/data render; public filter/search/page; required/invalid form; submit success/error/busy; reset token missing/expired/valid; login role redirect/error; election/constitution voting eligibility; landing fallback image/content. |

### Flutter backlog

| Feature/files | Test home and shared setup | Required cases |
| --- | --- | --- |
| Core API/auth/config/theme/files | Extend existing `test/*_service_test.dart`; move reusable fake Dio/storage into `test/helpers/` before adding a second copy | Every API method path/body/status/error; token absent/expired/refresh; config default/remote/invalid; theme persist/restore; file pick/upload valid/invalid/cancel/error. Use one `ApiEndpoints` class for all paths. |
| `activity`, `assistant`, `content`, `events`, `financials`, `forum`, `jobs`, `lookups`, `messaging`, `networking`, `notifications`, `support`, `theme` | One focused service/provider test then one non-golden widget test per feature | JSON complete/minimal; 200/non-200/Dio error; provider loading/error/empty/data; list filter/page; form validation; mutation success/error/busy; retry/refresh; permission/ownership state. |
| `admin` and member screens | Extend focused non-golden widget tests, not visual freeze tests | Admin/member role visibility; loading/error/empty/data; dialog cancel/confirm; status mutation both outcomes; validation/upload; no duplicate submission; snackbar message; provider invalidation after successful mutation. |
| `polls` | `test/features/polls/poll_service_test.dart` and `polls_screen_test.dart` | The detailed Poll map above is authoritative: centralized paths, JSON defaults, response/error paths, provider states, single/multiple/already-voted selection and submit outcomes. |
| Integration journeys | Keep only cross-layer paths in `integration_test/` with deterministic seeded data | Registration/login→profile, event registration/check-in, payment callback/history, forum/message, poll vote, admin approval. Each test asserts a user-visible result and cleans up or uses an idempotent seed; do not repeat service/widget branches here. |

### Execution waves and exit criteria

1. Wave A (ranks 1–2): cheapest branch-density work only; do not start a large component while a narrow zero-coverage service remains.
2. Wave B (ranks 3–5): service/controller pairs by business module, then its Angular and Flutter client. Poll is the completed reference module.
3. Wave C (ranks 6–7): stateful UI branches, uploads and external-provider failure seams.
4. Wave D (rank 8): cross-layer integration only after lower-level branch work stabilizes.

For each wave, update the baseline table, add the completed symbols and exact focused command to **Completed increments**, run the relevant full suite, then run the repository coverage collector. A row is finished only when its listed branch cases pass, its production file reaches the active changed-file threshold, and no existing test was duplicated or weakened.

## Coverage order

Work in this order because it gains the most meaningful branch coverage per test.

1. Validators, mappers, formatters, policy helpers and state reducers.
2. Service decisions: ownership, authorization, status transitions, null or empty data, fallback configuration, transaction rollback and external failure.
3. Controller contracts: one success and one meaningful missing, forbidden, invalid or failed outcome per mutation. Complete 47.13.4, 47.13.6 and 47.13.7 in the active tracker.
4. Angular services, guards, interceptors, signal state and forms.
5. Flutter providers, service parsing/fallbacks, router guards and shared widgets.
6. End-to-end tests only for a cross-layer journey or platform boundary that a lower-level test cannot prove.

## Threshold rollout

Do not enable a global 90% gate before the denominator and legacy gaps are known.

1. Publish reports for all three clients.
2. Fail CI on a coverage reduction from the accepted baseline.
3. Require 80% line and branch coverage for changed production files.
4. Raise changed-file requirements to 85%, then 90%.
5. Enforce 90% repository-wide when the legacy backlog is closed.

The changed-file gate protects unrelated maintenance work while preventing new debt.

## Completion evidence

Work Package 27 closes only when all of the following are true:

- Reports are collected and retained in CI for backend, web and mobile.
- Backend and web have at least 90% line and branch coverage in the agreed denominator.
- Mobile has at least 90% line coverage and every recorded branch target has an explicit pass/fail decision-path test.
- 47.13.7 confirms all mutation endpoints have deliberate controller coverage.
- No changed production file is below the active threshold.
- The backend, web, mobile, E2E, type-check, analyzer and build checks pass.
