# PART III — CONSTRUCTION AND VALIDATION

# Chapter 7 — Implementation

**What this chapter owns.** How the artefact was built: the mechanisms that were difficult, the
alternatives weighed while writing them, and what the code does that the design did not anticipate.

**What it must not repeat.** Why the architecture is shaped this way, which is Chapter 6. Threats and
controls, which are Chapter 8. How anything was tested, which is Chapter 9. Any measured result,
which is Chapter 12. No section here is a walk through the source tree, and no listing appears unless
the surrounding argument fails without it.

Unless a paragraph says otherwise, the counts in this chapter were taken against the working tree at
commit `4c0f733f` on 16 September 2026, with the commands named beside each figure.

## 7.1 Development Environment, Toolchain and Reproducibility

The backend targets `net9` (`dotnet --version` reports SDK 9.0.310) with nullable reference types
turned on in `GHCAA.Domain`, `GHCAA.Application` and `GHCAA.Infrastructure` (`<Nullable>enable</Nullable>`
in each `.csproj`). No `global.json` pins that SDK version to the solution, so a contributor with a
newer .NET 9 patch installed builds against whatever is on their machine rather than a version the
repository states explicitly — a reproducibility gap that costs nothing today because only one
developer has built this solution, but would need closing before a second one does.

The web client is Angular 21.2.22 with TypeScript 5.9.2 (`GHCAA.Web/package.json`). Its `npm run build`
script is a chain rather than a single compiler invocation: `sync:docs`, `gen:org-config`,
`gen:site-content`, `apply-brand`, `type-check`, then `ng build`, in that order. The middle three steps
generate files `ng build` then reads as ordinary source — `apply-brand` is the institution-profile
rewrite described in §7.16 — so a build run without `npm run build` (an `ng build` invoked directly, say)
silently skips them and builds against whatever those generators last produced. There is no ESLint
configuration in the project (`package.json` carries no `lint` script and no `eslint` dependency); the
only static check the build chain runs is `type-check`, the TypeScript compiler in no-emit mode, so a
style or dead-code problem that type-checks cleanly reaches `ng build` unflagged.

The mobile client targets Dart SDK `>=3.3.0 <4.0.0` (`GHCAA.Mobile/pubspec.yaml`) and is linted through
`analysis_options.yaml`, which includes `package:flutter_lints/flutter.yaml` rather than a custom rule
set.

Continuous integration runs from four GitHub Actions workflows in `.github/workflows/`: `ghcaa-ci-standard.yml`
on pushes and pull requests against `dev`, `main` and `master`; `ghcaa-ci-preprod.yml` on the same events
against `preprod`; `mobile_deployment.yml` for the Flutter build and store artefacts; and
`neon_workflow.yml`, which creates and tears down a scoped Neon Postgres branch per pull request rather
than pointing every PR build at one shared database.

## 7.2 Solution and Module Structure

The solution (`GHCAA.sln`) is nine projects rather than the classic four-layer split alone: `GHCAA.Domain`,
`GHCAA.Application`, `GHCAA.Infrastructure` and `GHCAA.API` carry the layered backend Chapter 6 describes,
and `GHCAA.Export` and `GHCAA.Tools` sit beside them as small, single-purpose projects rather than folders
inside a bigger one — `GHCAA.Export` is one file (121 lines) and `GHCAA.Tools` three (107 lines), each kept
separate because neither shares a reason to change with the layer it would otherwise have been folded
into. `GHCAA.Web` (Angular) and `GHCAA.Mobile` (Flutter) are the two clients, and `GHCAA.Tests` holds the
backend test suite; the clients carry their own test trees under their own roots rather than in
`GHCAA.Tests`.

Table 7.1 gives file and line counts by project (`.cs` files only for the backend projects, counted
with `find` and `wc -l`, excluding `bin/` and `obj/`):

### Table 7.1 — Size metrics by layer: files and lines of code

| Project | Files | Lines |
|---|---|---|
| GHCAA.Domain | 52 | 1,700 |
| GHCAA.Application | 96 | 2,924 |
| GHCAA.Infrastructure (excluding EF migrations) | 105 | 14,488 |
| GHCAA.Infrastructure — EF migrations (`Data/Migrations`) | 3 | 157,079 |
| GHCAA.API | 59 | 6,269 |
| GHCAA.Export | 1 | 121 |
| GHCAA.Tools | 3 | 107 |
| GHCAA.Tests | 87 | 15,723 |
| GHCAA.Web (`src/**/*.ts`) | 242 | 24,597 |
| GHCAA.Mobile (`lib/**/*.dart`) | 126 | 25,159 |

The migrations row is split out because it is not hand-written code: `20260907193705_InitialBaseline.cs`,
its `.Designer.cs` and `ApplicationDbContextModelSnapshot.cs` together account for 157,079 of
`GHCAA.Infrastructure`'s 171,567 lines, all of it EF Core's generated model-building code for the single
squashed baseline migration referenced in §5.7 and §6.3.2. Reading the project's size from the combined
figure without separating that row would overstate the hand-written backend by an order of magnitude.

## 7.3 Coding Standards, Conventions and Static Enforcement

Static enforcement differs by layer rather than following one house rule uniformly. The backend project
files turn on `<Nullable>enable</Nullable>` in `GHCAA.Domain`, `GHCAA.Application` and `GHCAA.Infrastructure`,
which converts a null-reference mistake into a compiler warning rather than a runtime `NullReferenceException`,
but none of the four backend projects sets `TreatWarningsAsErrors` or references a general-purpose Roslyn
analyzer package — the only analyzer in the solution is `NUnit.Analyzers` in `GHCAA.Tests.csproj`, which
checks test-writing mistakes (a missing `await` on an assertion, for instance), not production code. A
warning can therefore be introduced and merged without failing a build.

`GHCAA.Web/.editorconfig` sets formatting conventions (two-space indent, UTF-8, single quotes in
TypeScript) that editors and `ng build`'s formatter honour, but nothing in CI runs a linter against them;
the enforcement that does exist for the web client is the `type-check` step described in §7.1, which
catches a type error, not a style one. `GHCAA.Mobile/analysis_options.yaml` is the one place a rule set
is actually enforced at build time, through `flutter analyze` against `package:flutter_lints/flutter.yaml`,
with no project-specific rule added on top of that default set at the time of writing.

## 7.4 Implementation of the Domain and Persistence Layers

`ApplicationDbContext` declares 54 `public DbSet<...>` properties (`grep -c "public DbSet<" GHCAA.Infrastructure/Data/ApplicationDbContext.cs`),
53 of them domain models and the 54th `DataProtectionKeys`, a framework table `docs/adr/0007-data-protection-keys-in-database.md`
records adding so that ASP.NET Core's data-protection keys survive a Render redeploy rather than being
regenerated — and, before that fix, silently invalidating every session and every value the application
had encrypted with the previous key. The schema's migration history is a single file: the prior 31-migration
history was squashed into `20260907193705_InitialBaseline.cs` on 8 September 2026, discussed in §7.14 as
the change that closed the build-time-out problem the old history had started causing.

Persistence is EF Core against PostgreSQL in production and SQLite in the test and local-development
path, a split Chapter 6 sets out the reasoning for. The domain layer itself (`GHCAA.Domain`, 52 files,
1,700 lines) carries no EF Core reference: entities are plain classes, and every EF-specific concern —
fluent configuration, converters such as the ISO-8601 `DateTime` converter §5.4 and §6.4 both describe,
the `IsActive` global query filter on `AlumniEvent` — lives in `GHCAA.Infrastructure`, so a domain type can
be read and reasoned about without also reading how it is persisted.

## 7.5 Implementation of the Application and Business Services

`GHCAA.Application` defines the interfaces the API layer depends on and `GHCAA.Infrastructure` implements
them, the dependency direction Chapter 6 states as a rule. Grepping both projects for `public interface I*Service`
finds 45 distinct service interface names declared across 44 files (some files declare more than one
small interface alongside the one the file is named for); grepping `GHCAA.Infrastructure` for a class
declared as `SomeService : ISomeService` on a single line — the common case, though a class implementing
several interfaces or splitting its declaration across lines is undercounted by this pattern — finds 42
matching implementations. The two counts are close enough to confirm the interface-per-service
convention holds through most of the layer without claiming an exact one-to-one mapping the grep pattern
cannot actually prove.

Two conventions recur across the layer rather than being decided per service. First, a service method
that performs more than one write opens its own `DbContext` transaction rather than relying on
`SaveChangesAsync`'s implicit one — necessary wherever a single business operation spans several
`SaveChangesAsync` calls that must succeed or fail together, and the subject of the transaction-scoping
fix in §7.15. Second, a service that calls an external system (SMTP, the real-time hub, a payment
provider) does so through its own interface (`IOtpService`, `IRealTimeService`) rather than a concrete
client type, which is what let the fix in §7.15 move those calls out of the transaction: the transaction
boundary and the external call were already two separate seams in the code, not one that had to be
created for the fix.

## 7.6 Implementation of the API Layer

`GHCAA.API/Controllers` holds 38 controllers exposing 285 endpoint action attributes
(`grep -rhoE '\[(HttpGet|HttpPost|HttpPut|HttpDelete|HttpPatch)' GHCAA.API/Controllers --include=*.cs | wc -l`),
against 6,269 lines across 59 files in the project as a whole, which puts controllers themselves at
roughly 16 lines per endpoint on average once routing attributes, `[Authorize]` and `[RequireStepUp]`
decoration, model binding and the call into the corresponding service are counted — thin by design, since
the layer's job is request/response translation and authorisation, not business logic. Two real-time
hubs live in the same project rather than a separate one (`Hubs/ChatHub.cs`, `Hubs/NotificationHub.cs`),
covered in §7.9.

`RequireStepUpAttribute` (§7.10) and the `SecurityStampMiddleware` check (also §7.10) are both applied at
this layer rather than in `GHCAA.Application`, because both depend on the HTTP claims principal and the
response status code convention, neither of which the application layer has a reason to know about.

## 7.7 Implementation of the Web Client

`GHCAA.Web/src` is 242 TypeScript files and 24,597 lines, of which 94 components are declared
`standalone: true` — Angular's module-free component style, used throughout rather than mixed with
`NgModule`-declared components. `styles.scss` is 3,468 lines, the shared token and utility layer §8.5's
theme audit and the design-system skill both work against; component-scoped styles sit alongside it
rather than replacing it, which is also the source of the scoping bugs recorded in §7.15 and the
`:host-context()` pattern those bugs led to.

The build chain in §7.1 is what makes the client's institution-profile awareness (§7.16) a compile-time
concern rather than a runtime one: `apply-brand` rewrites `index.html`, the sitemap and the favicon set
before `ng build` runs, so the shipped bundle already carries the active profile's branding rather than
fetching it after first paint.

## 7.8 Implementation of the Mobile Client

`GHCAA.Mobile/lib` is 126 files and 25,159 lines, state managed with `flutter_riverpod` and routing with
`go_router` (`pubspec.yaml`). Secure, on-device storage of the JWT and refresh token goes through a single
`StorageService` (`GHCAA.Mobile/lib/core/storage/storage_service.dart`) wrapping `flutter_secure_storage`,
rather than each screen touching the platform storage plugin directly — the wrapper is what made the
fix in §7.15 a one-file change instead of a search-and-fix across every caller. `tool/apply_profile.dart`
is the mobile equivalent of the web client's `apply-brand` script (§7.16): it rewrites the Android manifest
label, the iOS display name and the launcher-icon/splash-screen generation inputs from the active profile
before a platform build runs.

## 7.9 Real-Time Features

Real-time delivery is SignalR, wired in `GHCAA.API/Extensions/ServiceExtensions.cs` and `Program.cs`
through two hubs: `ChatHub` and `NotificationHub` (`GHCAA.API/Hubs/`). `RealTimeService`
(`GHCAA.API/Services/RealTimeService.cs`) is the single point application services call through to reach
either hub — `MemberService`'s admin alert on a new registration (§7.10, §7.15) is one caller among
several — rather than a controller or service pushing to a hub context directly, which keeps the
transport detail (SignalR specifically, as opposed to a different push mechanism) behind one interface.

## 7.10 Security Implementation

This section describes three mechanisms and the difficulty each one answers. What each threatens and
how it fits the wider control set is §8.3; this section is only the code and the reasoning behind it.
Citations below were re-taken against commit `5f5ebfe4` on 15 September 2026.

**Instant session revocation.** A JSON Web Token is normally valid until it expires, so an admin who
disables an account, a user who changes a compromised password, or a security response that needs a
session killed immediately all have to wait out the token's remaining lifetime unless something else
checks state on every request. `SecurityStampMiddleware` (`GHCAA.API/Middleware/SecurityStampMiddleware.cs`,
lines 22-50) is that check: it reads the `SecurityStamp` claim carried in the token, looks up the
current value stored against the user, and returns 401 the moment the two disagree. The stamp itself
is a random value rotated whenever something should end every existing session for that user —
`AuthService.cs:494` on an explicit revocation, `UserService.cs:122` and `:156` on account changes,
and `MemberService_Approval.cs:108, 174, 255, 361` at points in the approval workflow where a member's
standing changes enough that old tokens should no longer be trusted. The cost is a database read on
every authenticated request; the alternative — a short-lived access token with no stamp check — was
rejected because it trades an instant revocation for a window of up to the token's lifetime during
which a disabled account keeps working. No unit test instantiates `SecurityStampMiddleware` directly;
the behaviour is exercised indirectly through `AuthControllerMutationTests.cs` and, on the client side,
`GHCAA.Mobile/test/session_manager_test.dart`. A middleware-level test is not yet written.

**Refresh-token reuse detection.** Rotating a refresh token on every use limits how long a stolen one
stays useful, but rotation alone does not detect the theft: if an attacker captures a refresh token
before its legitimate holder uses it, both parties now hold a valid-looking credential and the first
one to redeem it invalidates the other's copy without anyone noticing which was which. `TokenService.RotateRefreshTokenAsync`
(`GHCAA.Infrastructure/Services/TokenService.cs`, lines 136-180) closes that gap: if the presented
token hashes to a row already marked revoked, that is not a stale request but a replay — the legitimate
holder has already moved on to the token that replaced it — so the response is to revoke every refresh
token belonging to that user (`RevokeAllRefreshTokensAsync`, lines 182-187) and rotate the security
stamp in the same call, killing any access token already issued as well. The ticket that asked for this,
82.18, is named in the code comment at line 144. `docs/ARCHITECTURE_AUDIT_2026-09.md` still describes
this as missing at its line 255-273; that finding predates the fix and should not be read alongside the
current `TokenService.cs` as if both were still true. `TokenServiceTests.cs` covers the reuse path
directly: `RotateRefreshToken_ReplayOfARotatedToken_RevokesWholeFamilyAndRotatesStamp` (line 136),
alongside `StoreAndRotateRefreshToken_ShouldRotateCorrectly` (line 103) and `RevokeAllRefreshTokens_ShouldMarkAllRevoked`
(line 170).

**Step-up re-authentication for destructive actions.** A session that is valid for browsing is not
necessarily one that should be trusted to delete a user, change a role, or alter a financial record
without asking again — an admin's browser left open at a shared desk is a real exposure the ordinary
session model does not address. `RequireStepUpAttribute` (`GHCAA.API/Filters/RequireStepUpAttribute.cs`,
lines 19-41) reads a `step_up_verified_at` claim (`StepUpClaim.cs`, line 15) set when the user last
re-confirmed their identity, checks it against a time-to-live, and returns 403 with
`Code = "STEP_UP_REQUIRED"` if the claim is missing or has expired. The filter is applied 15 times
across six controllers: `RolesController` (`CreateAdmin`, `AssignRole`, `DeleteUser`, `DisableUser`,
`EnableUser`, `ResetPasswordAdmin`), `AdminController` (`SyncMembers`, `ArchiveMember`, `BulkArchiveInactive`,
`ResetPasswordAdmin`), `AdminGovernanceController` (`DeleteECMember`), `FinancialLedgerController`
(`AddRecord`, `UpdateRecord`, `DeleteRecord`), and `PaymentConfigController` (`DeleteConfig`). The
time-to-live was originally 30 days and was cut to 30 minutes (`StepUpClaim.cs`, lines 22-28) after a
security review on 29 August 2026 concluded that a step-up claim good for a month gave away most of the
protection a step-up check is meant to provide. `RequireStepUpAttributeTests.cs` covers the filter
directly across six cases; `DestructiveStepUpActionsTests.cs` covers the controller/service contract
for two of the delete actions it protects.

## 7.11 Document Generation

`IDCardService` (`GHCAA.Infrastructure/Services/IDCardService.cs`, 243 lines) generates every
printable member artefact: an ID card, a membership certificate, and a PDF version of each. Its
constructor (line 22) takes only `ApplicationDbContext` and `IOrgConfigService`, so a card carries
no state of its own beyond what those two sources supply at the moment it is requested — there is
no separate "card record" to keep in sync with the member row it describes.

Four public methods split along two axes: card or certificate, and data-URI (for on-screen preview
in the Angular and Flutter clients) or PDF (for download and printing). `GenerateIDCardDataUriAsync`
(line 39) and `GenerateCertificateDataUriAsync` (line 92) return an inline PNG; `GenerateIDCardPdfAsync`
(line 126) and `GenerateCertificatePdfAsync` (line 184) build the same layout through QuestPDF's
fluent API (`Document.Create(container => ...)` at lines 137 and 194, `document.GeneratePdf()` at
lines 181 and 240) and return raw bytes. All four build the same verification URL before anything
else —
`` $"{org.Contact.PortalBaseUrl}/verify/{member.MembershipNumber ?? member.Id.ToString()}" `` at
lines 59, 100, 135 and 192 — encode it with QRCoder at error-correction level Q (`QRCodeGenerator.ECCLevel.Q`,
present at every call site), and embed the code in the artefact. A card printed today points at a
URL the portal can still answer tomorrow, because the QR payload is a route, not a snapshot of the
member's data at generation time; only the destination page reads current state.

The duplication across the four methods (rebuilding `verifyUrl`, re-running the QR encode, laying
out the same header/footer) has not been factored into a shared template as of this writing. It is
a candidate for the deferred `GHCAA.Export` restructuring the outline does not require this chapter
to resolve.

## 7.12 Constitution Publication Pipeline

The constitution reader (`docs/CONSTITUTION_PUBLISHING.md`) is built on one rule stated at the top
of that document: the application always serves the latest ratified constitution, and no page,
component or stored link may pin a specific version. Publishing a new version is a single command,
`tools/constitution/publish_constitution.py` (309 lines, a documentation-side Python script using
`pymupdf`, not an application dependency), run against the ratified PDF once it lands in
`GHCAA.Web/public/assets/`. The script extracts the document's text into the paragraph/heading/list
shape the Angular reader parses, rewrites `GHCAA.Infrastructure/Data/Seed/constitution.json` as a
single active record, and repoints the one hardcoded fallback string
(`CONSTITUTION_PDF_FALLBACK` in `constitution.ts`) at the new asset.

Editing that seed file is not what makes a new version live. `Program.cs` calls
`Database.EnsureCreated()` at startup, which is a no-op once the tables already exist — on preprod
and production the EF Core `HasData` seed step never runs again after first boot. Changing the JSON
alone would sit there unread. `ConstitutionSeeder.SyncAsync`, called on every boot, closes that gap:
it is idempotent, inserts a version it cannot find by matching on `Version`, refreshes a stored
version in place when the seed text changes, and marks anything no longer in the seeded set as
superseded rather than deleting it — a genuine prior version keeps its row, its `AmendmentVote`
records and its own PDF link, because the Version History panel serves from that link and would
break if the row disappeared. `PlaceholderVersions` (`ConstitutionSeeder.cs`, line 32; currently
`{ "1.2.0" }`) marks the one exception: entries that only ever held internal placeholder text, which
the seeder removes outright rather than preserving as history.

The rest of the client surface follows the active row rather than a file path: the reader renders
whatever `GET /api/governance/constitution` returns, the landing page's "Read Constitution" link
routes to that reader instead of a static file, and the version banner and history panel both read
off the same query. `CONSTITUTION_PDF_FALLBACK` is the only version-bearing string left in
application code, and the publishing script owns it exclusively — nothing else in the codebase is
meant to hardcode a constitution PDF path.

`docs/CONSTITUTION_PUBLISHING.md` records three traps specific to the source PDFs that cost real
diagnosis time and are not bugs in the extractor: the exported document's cover page can carry a
stale version label (only the closing colophon is authoritative), the Google Docs export wraps
every styled run in zero-width space characters that the script must interpret rather than strip
outright (a doubled fence marks a swallowed word-break, a single fence does not), and a paragraph
that crosses a page boundary arrives as two separate text blocks that the script has to rejoin. The
v4.2 source document also carries one recorded defect of its own: Article V, Section C, item 6
still contains a leftover editing instruction ("TReplace the 21-day election notice rule with: …"),
carried into the published text verbatim because the extractor's job is to transcribe the ratified
document, not correct it.

## 7.13 Third-Party Libraries: selection criteria

Twenty-six distinct NuGet package references appear across `GHCAA.API`, `GHCAA.Application`,
`GHCAA.Infrastructure` and `GHCAA.Export` (`grep -rh "PackageReference" ... | sort -u`, run against
this tree). The web client's `package.json` lists 13 runtime dependencies; the mobile client's
`pubspec.yaml` lists roughly 51 dependency entries once dev-only and transitive-only lines are
included in the count. Most of these were adopted without a recorded alternatives comparison —
`QuestPDF` (2026.2.3) for PDF generation, `QRCoder` (1.8.0) for the verification codes described in
§7.11, `MailKit` (4.16.0) for outbound email, `ClosedXML` (0.105.0) for spreadsheet export, `Dapper`
(2.1.72) alongside EF Core for the handful of read paths that favour a raw query over LINQ — and
this chapter does not manufacture a selection rationale for choices the repository itself does not
document.

One removal is documented in enough depth to describe honestly: `docs/adr/0006-drop-mysql-provider.md`
records that `GHCAA.Infrastructure/DependencyInjection.cs` originally switched across three EF Core
providers (SQLite, MySQL, PostgreSQL), but only the PostgreSQL migration tree
(`Data/Migrations/PgSql/`) was ever complete, and nothing in the codebase actually called the MySQL
path. The decision removed `Pomelo.EntityFrameworkCore.MySql` and its supporting classes
(`MySqlApplicationDbContext`, `MySqlDesignTimeDbContextFactory`) entirely rather than leave a
provider branch with no working migrations and no caller, and kept SQLite only as a fast,
migration-free path for the test harness, not a deployment target. It is the one case in this
repository where "why this library and not another" has a written answer rather than an inferred
one.

## 7.14 Software Configuration Management

The repository carries 279 commits on `HEAD` and six local branches: `dev`, `preprod`,
`release-1`, `release-2`, `release-3_b4_generic_N_refactor` and `release-4_white_paper`, plus
`dev-mobile` and `mobile_app` that exist only on the remote. `dev` is the integration branch;
`preprod` (the branch this chapter was written from) trails `dev` by a `git rev-list --count dev..preprod`
count of 52, which is the gap between what has been merged to `dev` and
what has actually been promoted toward the staging environment described in §10. The
`release-N` branches are not conventional long-lived release trains — their names
(`release-3_b4_generic_N_refactor`, `release-4_white_paper`) read as snapshots taken before a
specific piece of work, not as a version-numbered release policy; no tag-based release
identification scheme is in force as of this writing, and no `CHANGELOG.md` or equivalent exists at
the repository root.

Four GitHub Actions workflows enforce change control mechanically rather than by review policy
alone: `ghcaa-ci-standard.yml` (dev/main/master), `ghcaa-ci-preprod.yml` (preprod), a mobile build
workflow (`mobile_deployment.yml`) and a per-pull-request Postgres branch provisioner
(`neon_workflow.yml`) that gives every PR its own disposable database rather than a shared one.
`docs/TODO.md` is the actual change-control ledger for this project — every planned or completed
piece of work is an item there with a status, not a separate issue tracker — and §11 of this book
covers its structure and the effort model built on top of it in full; this section does not repeat
that.

The single squashed `InitialBaseline` migration discussed in §7.4 is itself a configuration-management
decision worth naming here: it replaced a prior history of 31 incremental migrations with one file,
trading the ability to replay each historical schema change step by step for a much smaller and
faster-to-apply migration chain. §6.3.2 covers the reasoning in more depth; this section notes only
that the choice was made deliberately, not as a side effect of losing history.

## 7.15 Notable Implementation Challenges and Their Resolution

**Registration could commit a member row without ever sending the OTP that made the account usable.**
`MemberService.RegisterAsync` (`GHCAA.Infrastructure/Services/MemberService.cs`, line 78) writes the
new member, an initial payment record and a set of uploaded files across several `SaveChangesAsync`
calls before the registration is complete. The hypothesis was that any one of the two side effects
that follow — sending the OTP (`_otp.GenerateAndSendOtpAsync`, line 290) and alerting admins over
SignalR (`_realTimeService.SendAdminAlertAsync("NEW_REGISTRATION", ...)`, line 299) — could throw
(a mail provider outage, a disconnected hub) partway through, and depending on where the write sat
relative to that failure, the member row could end up committed with no OTP ever sent, leaving an
account nobody could verify. The evidence was in `MemberServiceTests.cs`:
`RegisterAsync_WhenOtpSendFails_ShouldStillCommitRegistration` (line 352) and
`RegisterAsync_WhenAdminAlertFails_ShouldStillCommitRegistration` (line 367) exist specifically to
pin the resolution. The fix wraps the whole registration in one explicit transaction
(`_db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken)`,
line 80, committed at line 271) and moves both notification calls to after the commit, outside the
transaction, so a failure in either one cannot roll back a registration that already succeeded and
cannot leave a half-written member row either — the database write and the two best-effort
notifications no longer share a failure path. `docs/TODO.md` item 83.1 records this as done on 16
September 2026; the file shows as modified in the current working tree, and this section describes
that in-progress state, not a settled historical fact.

**A locked file on Windows could abort a mobile logout before it reached the sign-in screen.**
`StorageService` (`GHCAA.Mobile/lib/core/storage/storage_service.dart`) wraps every
`flutter_secure_storage` call the mobile app makes, including `clearAll()` (line 159), the method
the logout flow on `dashboard_screen.dart` calls before navigating away. On Windows, the secure
storage backend keeps its values in a DPAPI-encrypted file that can briefly be held open by the OS;
deleting a key while that lock is held throws, and because `clearAll()` deleted each key directly,
one exception during logout meant every delete after it in sequence was skipped and the navigation
that should have followed never ran — the user stayed on the dashboard behind what looked like a
frozen tap. The fix is `_safeDelete` (line 117), a private helper that wraps every delete in its own
try/catch, used at every call site in the file (the JWT key at line 69, the refresh token at line
108, `clearAll()`'s two credential keys at lines 181-182). One locked file can now fail its own
delete without blocking the ones after it or the navigation that follows. Because `StorageService`
is the single wrapper every caller in the app already went through, the fix was contained to this
one file rather than needing a change everywhere secure storage is used.

## 7.16 Institution Profile Packs and White-Label Configuration

Every string, seed row and asset described elsewhere in this chapter names Govt. Haraganga College
by default, because the system was built for one association and had no reason to be otherwise until
Work Package 62 asked what it would take to run the same codebase for a second one. The constraint
that shaped the answer was not technical but organisational: the live deployment could not be
touched. `ORG_PROFILE` is unset on every environment this system runs in today, and any change whose
correctness depended on someone remembering to set it first was rejected on that basis alone, not
because it was hard to build.

The mechanism is a profile pack: a folder under `profiles/<name>/` (`profiles/ghc/`,
`profiles/default/`) holding `org-config.json`, an institution's own seed data, static page text and
image assets. `IInstitutionProfileProvider` reads the `ORG_PROFILE` environment variable at boot,
resolves it to a folder, and falls back file by file to `profiles/default/` for anything the named
profile does not supply — the same defaulting rule `Path.Combine`-based seed resolution elsewhere in
the codebase already used, extended rather than replaced. `OrgConfigService.BuildDefaults()` is the
one place that decides whether the pack is trusted: it drives configuration only when
`ProfileExplicitlySelected` is true, and falls back to the hardcoded values the service always had
otherwise. An unset `ORG_PROFILE` therefore behaves exactly as it did before this work package,
which is the property the constraint above demanded, and `OrgConfigGoldenSnapshotTests` freezes a
snapshot of the pre-Work-Package-62 output specifically so a future change cannot alter that
behaviour silently. The same guard shape recurs at every layer this work package touched: the ASP.NET
Core data-protection application name, the Angular boot-time branding fallback, and the Flutter
mobile application identifier are all read from the pack only once a profile is explicitly named,
because each of them protects something that breaks if it changes under a live deployment — session
cookies, first paint before the API responds, and the published app's store listing, respectively.

Seed data required a second decision, independent of the first: `docs/SEED_CLASSIFICATION.md`
grades every seed file by whether its content is structural (every institution needs the same rows),
shared-shape (every institution needs a row here, but not this one's), or historical (an empty table
is the only correct starting state for a new institution). Moving the third class out of
`Data/Seed/` and into `profiles/ghc/demo-data/` exposed two latent defects that had never been
visible before, because the class of data being moved had always been present in the fallback path
that both defects shared. `ApplicationDbContext.LoadSeed`'s test for whether it was safe to read a
profile pack checked whether the EF Core design-time tooling assembly was loaded into the process,
which is true for an ordinary test run as well as for an actual `dotnet ef` invocation, since the API
project references that package for its own migration tooling; and the project file that copies seed
fixtures into the test build output had never listed the `Data/Seed/Visual/` subfolder at all. Between
them, every affected integration test had been silently reading the pre-migration seed file instead
of the intended fixture set for as long as either defect existed, undetected because the fixture and
the real data it stood in for happened to satisfy the same foreign keys. Replacing the assembly-scan
check with the framework's own `EF.IsDesignTime` flag and adding the missing copy rule fixed both;
the full backend suite (590 tests) is the evidence that nothing else depended on the accidental
behaviour.

Two build-time generators keep the web and mobile clients honest to the same pack rather than a
second, hand-copied one: `GHCAA.Web/scripts/apply-brand.mjs` rewrites `index.html`, the sitemap and
the favicon set from the active profile before `ng build` runs, and
`GHCAA.Mobile/tool/apply_profile.dart` does the equivalent for the Android manifest label, the iOS
display name, and the launcher-icon/splash-screen generation inputs. Both are idempotent against the
GHC profile specifically so that running them changes nothing observable in the current build — verified
by diffing their output against the committed files rather than assumed. `scripts/brand-lint.mjs`
scans the three client codebases for the literal strings this work package removed and reports any
that return, currently warn-only until Work Package 62.41's acceptance test exists to prove a
`default`-profile build is actually clean before that check is allowed to fail a build.

Work Package 82.58 added the second layer the framework needed before it was useful to a
development environment other than the GHC deployment: `profiles/default/demo-data/` now holds a
minimal but fully-relational sample dataset — members, users, an EC history, news items, gallery
entries, financials and membership dues — sized to exercise every foreign-key path in the schema
without importing any real alumni record. `DatabaseBootstrapperExtensions` seeds this pack when
`ORG_PROFILE` is unset and the environment is non-production, so a clean `dotnet run` in development
reaches a usable state without any manual SQL. A companion `profiles/default/constitution.json`
provides a placeholder constitution for the same reason: the `ConstitutionSeeder` would abort
otherwise, because it enforces the always-latest invariant regardless of profile.

One path-resolution defect surfaced during this work: `ResolveProfilePackPath` had been matching
file names by exact string, so a file stored as `ec_history.json` would not be found when the pack
referenced it as `ec-history.json` (or vice versa). The fix normalises both the stored name and the
lookup key to canonical low-dash delimiters before comparing, with a secondary hyphen-normalised pass, so the resolver
is now format-agnostic. The change is covered by the `SeedDataIntegrityTests` suite, which was
extended in the same work package to validate both the `ghc` and `default` profile packs: every
referenced file must exist, every JSON document must parse, and every foreign-key reference within
the pack must resolve against the schema — run for both profiles on every CI execution.

What this work package does not close is recorded rather than hidden: 631 real alumni records,
their password hashes and their payment history are written as literal values into eight already-committed
EF Core migrations, not only into the seed files this section describes moving, so a second
institution's database would still not start empty until that separate problem (Work Package
62.31/82.31) is resolved. `docs/INSTITUTION_ONBOARDING.md` states this plainly to whoever deploys
the system next, rather than letting a profile pack's existence imply a safety the migration chain
does not yet provide.

## 7.17 Summary

The nine projects described in this chapter split cleanly by responsibility — domain, application
interfaces, infrastructure implementations, API, two client front ends, and three small
single-purpose helpers — but the enforcement of that split is uneven. Nullable reference types are
on in the three backend layers that matter most for correctness; nothing stops a warning from being
ignored, and no analyzer beyond `NUnit.Analyzers` runs across the solution. The web client checks
types but not lint rules; the mobile client is the only place `flutter analyze` runs against a real
rule set at build time. A pair of closed work-package items left a durable mark on
how the code is written rather than just what it does: the transaction-and-notification-ordering
fix in `MemberService.RegisterAsync` (§7.15) and the per-call error isolation added to
`StorageService` (§7.15) both replaced an implicit assumption — that a secondary side effect cannot
fail in a way that matters — with an explicit one. The institution-profile-pack mechanism (§7.16)
and the security middleware (§7.10) are the two areas of this codebase with the most deliberate
design behind them, both driven by a constraint that could not be worked around: a live deployment
that could not be touched, and an authentication surface that had to survive a stolen token. Chapter
6 covers the architecture these choices sit inside; Chapter 9 covers how the resulting system was
verified.

## Figures and Tables

Table 7.2 lists a representative sample of the dependencies named across §7.11 and §7.13; the full
backend package list is in the four `.csproj` files under
`GHCAA.API`, `GHCAA.Application`, `GHCAA.Infrastructure` and `GHCAA.Export`, the full web list in
`GHCAA.Web/package.json`, and the full mobile list in `GHCAA.Mobile/pubspec.yaml`. "Alternative
considered" is left blank except where a repository record documents one.

### Table 7.2 — Selected third-party dependencies

| Library | Version | Purpose | Alternative considered |
|---|---|---|---|
| QuestPDF | 2026.2.3 | ID card, certificate and financial document PDFs (§7.11) | *[Not documented]* |
| QRCoder | 1.8.0 | Verification QR codes embedded in ID cards and certificates (§7.11) | *[Not documented]* |
| Npgsql.EntityFrameworkCore.PostgreSQL | 9.0.4 | Production database provider | Pomelo MySQL provider — removed, ADR-0006 |
| Microsoft.EntityFrameworkCore.Sqlite | 9.0.19 | Test/dev database provider only | — |
| MailKit | 4.16.0 | Outbound email (OTP, notifications) | *[Not documented]* |
| ClosedXML | 0.105.0 | Spreadsheet export | *[Not documented]* |
| @microsoft/signalr | ^10.0.0 | Web client hub connections (§7.9) | *[Not documented]* |
| flutter_secure_storage | ^10.3.2 | Mobile token/credential storage (§7.15) | *[Not documented]* |

The outline specifies twelve artefacts for this chapter: Figures 7.1-7.8 (module structure,
dependency matrix, web build pipeline, Git branching model, constitution publication flow, file
upload flow, membership-due calculation flow, a control-flow graph with cyclomatic complexity),
Listings 7.1-7.n, and Tables 7.2-7.1. Table 7.1 (size metrics by layer) is in §7.2. Table 7.2 is
above. Table 7.2 (a module implementation-status matrix) and all eight figures are not built in this
pass: `docs/TODO.md` item 67.3 defers the seventeen per-component diagrams and six chapter-level
charts for Chapters 7-13 to a separate, later work item, and a status matrix for this chapter would
need to cross-reference the WBS component mapping Chapter 11 already builds rather than duplicate
it. Both are left as open work rather than filled with a diagram or table that does not carry real
information.

