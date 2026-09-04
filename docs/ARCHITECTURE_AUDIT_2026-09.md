# Architecture and engineering audit, September 2026

Run against the brief in `docs/materials/REVIEW.md`, which the user supplied on 2026-09-04. Tracked as
Work Package 82 items 82.1 (the report) and 82.2 (the reconciliation registers) in `docs/TODO.md`.

Every finding below carries the file path or the command that produced it, one of the eight status
labels from REVIEW.md §3, and one of the eleven disposition labels from 82.2. A row with no evidence
line is a defect in this report, not a finding.

## What this audit did and did not cover

Stated first, because a report that hides its own gaps is worse than a shorter one that admits them.

| REVIEW.md section | Covered | Basis |
|---|---|---|
| §5 Architecture principles and patterns | Yes | Read of the four backend `.csproj` files, `DependencyInjection.cs`, `Program.cs`, service/repository inventory |
| §6 .NET backend | Yes | Read of `GHCAA.API`, `GHCAA.Application`, `GHCAA.Domain`, `GHCAA.Infrastructure` |
| §7 Angular application | **No** | The research pass assigned to this returned nothing. Not assessed. See "Not assessed" below |
| §8 Flutter application | **No** | Same. Not assessed |
| §9 Cross-platform | Partial | Only through the API-contract findings already tracked as 82.3/82.4/82.10 |
| §10–11 Configuration domains | Yes | Read of `OrganizationConfig`, `OrgConfigDto`, `OrgConfigService`, `Constants.cs`, WP 28/62 shipped state |
| §12 Replaceability | Yes | Read of `DependencyInjection.cs`, gateway adapters, storage/email interfaces |
| §13 Domain and business rules | Yes | Read of the 46 files in `GHCAA.Domain/Models/` and the governance/financial/notification services |
| §14 Database and data | Yes | Read of `Data/Configurations/*.cs`, the migration tree, entity audit-field coverage |
| §15 Security | Yes, incremental | Scoped to what WP 24 and WP 48 had not already closed; those two were read first and treated as closed ground |
| §16 Testing | Yes | Read of `GHCAA.Tests` layout, `GHCAA.Web/tests`, `GHCAA.Mobile/test`, both CI workflows |
| §17 Observability | Yes | Read of `Program.cs`, `HealthController.cs`, `appsettings*.json` |
| §18 DevOps | Yes | Read of all five workflow files and the `Dockerfile` |
| §19 Performance | Yes | Query/tracking survey across `GHCAA.Infrastructure/Services` |
| §20 Technical debt | Yes | Register below |
| §22 Documentation | Yes | Read of `README.md`, `docs/` inventory against §22's four audiences |
| §25 Refactoring and duplication | **Partial** | The dedicated pass returned nothing. What is here comes from the other streams, not a systematic duplication sweep |

**Not assessed: §7 (Angular), §8 (Flutter), and §25's systematic duplication sweep.** Two of five
research streams produced no output before the session hit its provider rate limit. The Angular and
Flutter clients are therefore represented in this report only by findings that surfaced from the
backend and cross-platform side (82.7's `HttpClient` bypass, 82.10b's untyped Dart payloads, the
mobile test-pyramid note in §16). Anyone reading this as a whole-ecosystem review should treat the two
client applications as unexamined. Closing that gap is tracked as 82.14.

---

## A. Executive assessment

| Dimension | Rating | Basis |
|---|---|---|
| Overall maturity | Good | Layering is compiler-enforced, security has had two dedicated audit rounds, tests number 573 and gate CI |
| Architecture | Good | Clean-Architecture dependency direction verified by `ProjectReference` graph, not just claimed |
| Code quality | Good | One god-service (`MemberService`, 1,577 lines) against 39 otherwise proportionate services |
| Security | Good | Rotation, hashing, CSRF, step-up, rate limiting all present; gaps are narrow and specific, not structural |
| Maintainability | Developing | Audit-field and soft-delete coverage is incidental rather than designed; one god-service; config bound by raw string keys |
| Configuration maturity | Developing | The mechanism (WP 28) is real and resilient; the multi-institution layer (WP 62) is 2 of ~35 items done |
| Operational maturity | Weak | No structured logging, no correlation ID, two health endpoints with no stated contract, no recovery runbook |

The scale on which these sit is REVIEW.md §23's: Critical, Weak, Developing, Good, Strong,
Production-ready. Nothing here rates Critical. The honest summary is a platform whose *code* is in
better shape than its *operations*: it is built carefully and tested seriously, but if it failed at
3am the evidence needed to find out why would not be there.

## B. Current architecture map

```text
Angular Web (GHCAA.Web/src)          Flutter Mobile (GHCAA.Mobile/lib)
        │                                      │
        │  cookie JWT + XSRF double-submit     │  bearer token, Dio
        └──────────────┬───────────────────────┘
                       ▼
        GHCAA.API   controllers, middleware pipeline, filters
                       │        (Exception → SecurityHeaders → SecurityStamp
                       │         → XSRF → LoginRateLimit → AuditLog)
                       ▼
        GHCAA.Application   39 service interfaces, DTOs, FluentValidation,
                       │    security primitives (StepUpClaim)
                       ▼
        GHCAA.Infrastructure   39 service implementations, 4 payment-gateway
                       │       adapters, EF Core DbContext (+3 provider shims)
                       ▼
        GHCAA.Domain   46 entity models, enums, Constants — zero project references
                       │
                       ▼
                 PostgreSQL (Neon), local disk storage, Gmail SMTP, SMS provider
```

The dependency direction is real: `GHCAA.Domain/GHCAA.Domain.csproj` has no `ProjectReference`
elements at all, `GHCAA.Application` references only Domain, `GHCAA.Infrastructure` references Domain
and Application, `GHCAA.API` references all three. The rule is enforced by the compiler rather than by
convention, which is the distinction that matters.

## C. Strengths

Only findings with evidence behind them.

1. **The layer boundary is compiler-enforced, not documented-and-hoped.** Verified by reading all four
   `.csproj` files, not by trusting `docs/ARCHITECTURE.md`.
2. **Payment-gateway abstraction was earned, not speculative.** `IPaymentGatewayService` with four
   adapters (`BkashGateway`, `NagadGateway`, `SSLCommerzGateway`, `DGePayGateway`) behind
   `PaymentGatewayFactory`. It replaced a switch statement in `GatewaysController` once a second
   gateway made the duplication concrete — the correct trigger for introducing a pattern, and the
   opposite of the speculative abstraction REVIEW.md §4 warns against.
3. **Refresh-token design is close to the OWASP shape.** Opaque token, SHA-256 hashed at rest
   (`RefreshToken.TokenHash`), rotate-on-use (`TokenService.RotateRefreshTokenAsync:129`),
   revoke-all on password change. One gap remains (Finding 5), and it is narrow.
4. **Auth transport was designed as one piece.** httpOnly/Secure cookie JWT, `XsrfMiddleware`
   double-submit for browser clients, `Authorization` header accepted only on `/api/*`. Coherent
   rather than patched together.
5. **Production fail-fast on CORS.** `Program.cs:173-181` throws `InvalidOperationException` if
   `AppSettings:AllowedOrigins` is empty in Production rather than silently defaulting to permissive.
6. **`MigrationBootstrapper` handles specific Postgres error codes** (`42P07`, `42701`, `42P06`,
   `42710`, `23505`) rather than a blanket try/catch — resilience written against what actually goes
   wrong on redeploy against a partially-migrated database.
7. **The backend test fixture uses real SQLite, not EF's InMemory provider.** `GHCAA.Tests/TestBase.cs`
   opens `DataSource=:memory:` per test. This avoids the well-known InMemory false negatives around
   relational constraints and query translation — the tests exercise real SQL semantics.
8. **`Member`'s composite index was driven by an observed query.** `(Status, IsArchived)` in the
   entity configuration, comment-tagged to WP 24.37 with the admin-listing query that motivated it.
9. **Every in-code TODO marker is traceable to a tracker number.** 7 in the backend, 6 in
   `GHCAA.Web/src`, 0 in `GHCAA.Mobile/lib`, and each names a work-package item (`ChatHub.cs:16` →
   "TODO 24.6", `nav.service.ts` → "TODO 30.13"). Unusually disciplined; most codebases accumulate
   orphaned markers.
10. **`docs/PROJECT_MAP.md` is genuinely complete** for a project this size — full layer inventories
    down to service classes, a cross-layer dependency matrix, and an impact guide.
11. **The Dockerfile is above median quality** — multi-stage, digest-pinned bases (as of 48.19), a
    recorded reason for excluding the test project from the image, and
    `DOTNET_hostBuilder__reloadConfigOnChange=false` to avoid Render's inotify limit.
12. **`MembershipFeeConfig` is effective-dated** (`EffectiveDate`/`EffectiveTo`/`IsActive`), so a fee
    change is a new row rather than a mutation — the correct temporal-config pattern, and it avoids
    needing a workflow engine.
13. **Constitution versioning is the one fully-correct immutable governance record** — supersede,
    never delete; single-active enforced in code.

## D. Critical problems

Ranked by what an incident would cost. None rate "Critical" on §23's scale; these are the top of
"should be fixed before this is called production-grade."

1. **Financial and payment records can be edited and hard-deleted with no trace.** See Finding 2.
2. **Operations cannot answer "what failed and who was affected."** No structured logging and no
   correlation ID (82.9). Health checking works but has two endpoints of different depth with no
   stated contract (Finding 11).
3. **The database-provider switch advertises three providers and supports one.** See Finding 1.
4. **A second CI workflow can go green while the real one fails.** See Finding 13.
5. **The README states the schema mechanism incorrectly**, which would mislead an operator on the one
   subject where being wrong is most expensive. See Finding 17.

---

## E. Findings

Each carries evidence, a REVIEW.md §3 status, and an 82.2 disposition.

### Finding 1 — The database-provider abstraction works for PostgreSQL only

**Status:** incorrectly implemented. **Disposition:** Newly Created → 82.15.

`GHCAA.Infrastructure/DependencyInjection.cs:32-53` switches on a `DatabaseProvider` config value
(`sqlite` / `mysql` / default PgSql) and pools a provider-specific shim context from
`Data/DbContextShims.cs`. But `Data/Migrations/` contains one provider folder, `PgSql/`, whose 32
migrations are all attributed `[DbContext(typeof(PgSqlApplicationDbContext))]`. Grepping for the
Sqlite and MySql shim types in migration attributes returns zero files. `MigrationBootstrapper`'s
self-heal path is Postgres-specific (`PostgresException`, `pg.SqlState`, lines 192 and 212).

Setting `DatabaseProvider=MySql` or `Sqlite` therefore boots against an empty schema:
`GetMigrations()` returns nothing for those context types and nothing applies.

This contradicts `docs/book/06-architecture.md`'s ADR-02, which records the consequence as "migrations
apply correctly on boot across PostgreSQL, MySQL and SQLite." The book claim is wrong as of this tree.

Also found, and **corrected against an earlier draft of this report**: five migrations sit directly
under `Data/Migrations/` rather than in `PgSql/`, dated 2026-03-19 to 2026-03-27. A research pass
called these "dead pre-split artifacts that should be deleted." That is wrong and the recommendation
was dangerous. `dotnet ef migrations list` shows all five in the **live** chain:
`RefactorMemberAcademicProfessionalRecords`, `AddNewFeaturesAndThemeAnimations`,
`UpdateAlumniEventDates`, `UpdateMigratedMemberApprovalStatus`, `AddSourceToActivityLog`. Deleting
them would break the migration history. They are a file-organisation inconsistency — same assembly,
different folder — and nothing more. `PgSqlApplicationDbContextModelSnapshot.cs` also lives at that
level and is the live snapshot, correctly so. **Do not delete any of these.**

**Why it matters now:** nothing today selects a non-Postgres provider, so the runtime cost is zero.
The cost is that the code offers optionality it does not have, and a future maintainer reading the
switch would reasonably believe MySQL is a supported target. Per the project's own principle — the
same one that correctly deferred a second file-storage adapter in ADR-06 — the fix is to either
finish the other providers or delete the switch and the shims down to Postgres. Deleting is cheaper
and is the recommendation.

### Finding 2 — Financial and payment records are mutable and hard-deletable

**Status:** missing. **Disposition:** Newly Created → 82.16.

`FinancialLedgerService.cs:69` exposes `UpdateRecordAsync` and line 91 does
`_db.FinancialRecords.Remove(record)` — a hard delete. `FinancialService.cs:488` does
`_db.PaymentHistories.Remove(payment)`. Neither `FinancialRecord` nor `PaymentHistory` carries
`UpdatedAt`, `UpdatedByAdminId`, or a soft-delete flag; `FinancialRecord` has `CreatedAt` and
`CreatedByAdminId` only.

So a ledger entry or a member's payment row can be altered or permanently removed with no record of
the previous value and no record of who did it. `PaymentHistory.Status` can move
Pending → Completed/Failed/Refunded with no history table paralleling the one that exists for
membership changes (`MembershipHistory`, which is append-only by convention).

**Why it matters now:** the association already collects real dues — memory records roughly ৳47,000
in per-member fees. This is not a "large growth" concern; auditability of money movement is the first
thing an external reviewer looks for, and REVIEW.md §14 asks specifically whether payment records are
immutable. They are not.

**Proportionate fix:** add `UpdatedAt`/`UpdatedByAdminId` and a soft-delete flag to both entities, and
make corrections additive rather than destructive. No event sourcing, no separate audit service.

### Finding 3 — Audit-field and soft-delete coverage is incidental rather than designed

**Status:** partially implemented. **Disposition:** Merged into 82.16 (same remediation as Finding 2).

Across the 46 files in `GHCAA.Domain/Models/`: 3 carry `IsArchived` (`Member`, `Poll`, `User`), 6
carry any created-by/modified-by field, 8 carry any updated-at field against 16 that carry
`CreatedAt`. Entities governing money and governance (`FinancialRecord`, `PaymentHistory`, `ECMember`,
`Constitution`) are not uniformly covered.

`ECMember` has two removal semantics living side by side: `GovernanceService.DeleteECMemberAsync` does
a hard `Remove()`, while `RemoveMemberFromCommitteeAsync` correctly end-dates by setting `EndDate`.
Two paths, two meanings, same entity.

**Why it matters:** the coverage pattern suggests fields were added where a specific bug forced them
(`Member.IsArchived` came from WP 24.30's rejected-applicant fix) rather than from a rule about which
entity classes need them. Without the rule, the next entity added inherits the inconsistency.

### Finding 4 — Configuration is bound through raw string keys, with no options pattern

**Status:** missing. **Disposition:** Newly Created → 82.17.

`grep -rl "IOptions<\|IOptionsSnapshot<\|IOptionsMonitor<" GHCAA.Infrastructure/Services/*.cs GHCAA.API/*.cs`
returns zero files. Every consumer reads configuration by hand:
`configuration.GetValue<string>("DatabaseProvider")` (`DependencyInjection.cs:23`),
`configuration.GetSection("AppSettings:AllowedOrigins").Get<string[]>()` (`Program.cs:175`), and the
same shape repeats across services.

**Why it matters:** there is no compile-time check on config shape and no single POCO showing what a
feature needs. A mistyped key returns the default silently. This is distinct from 82.11 and 82.12,
which classify *which values* belong in config; this is about *how* config is bound in code.

**Proportionate fix:** `services.Configure<T>(configuration.GetSection(...))` with constructor
injection. This is .NET's own idiomatic mechanism, not a new abstraction layer.

### Finding 5 — Refresh-token rotation has no reuse detection

**Status:** implemented but incomplete. **Disposition:** Newly Created → 82.18.

`TokenService.RotateRefreshTokenAsync:129` matches on
`TokenHash == hash && !IsRevoked && ExpiresAt > Now`, revokes the old token and issues a new one.
`RefreshToken` (`GHCAA.Domain/Models/RefreshToken.cs`) has no `ReplacedByTokenId` or family-chain
field. When an already-rotated (revoked) token is replayed — the classic signal of token theft — the
lookup simply finds nothing and the request fails. Nothing revokes the rest of that user's family.

`RevokeAllRefreshTokensAsync` exists (line 152) and is called on password change/reset
(`AuthController.cs:161`, and now also from the 49.3 admin-reset paths), so the machinery is present;
it just is not wired to the replay signal.

**Why it matters:** rotation without reuse detection means a stolen token used once before the real
user's next refresh goes entirely undetected. WP 24.27 delivered rotation and WP 48.3 fixed token
survival across reset; neither covers this.

**Proportionate fix:** one branch in `RotateRefreshTokenAsync` — if the presented hash matches a
revoked token, call `RevokeAllRefreshTokensAsync` for that user. No new infrastructure.

### Finding 6 — `AsNoTracking()` is used in 5 of 39 services against roughly 183 read queries

**Status:** partially implemented. **Disposition:** Newly Created → 82.19 (scale-tiered, not urgent).

`grep -rc "AsNoTracking" GHCAA.Infrastructure/Services/*.cs | grep -v ":0"` returns 5 files;
`ToListAsync|FirstOrDefaultAsync|SingleOrDefaultAsync` across the same directory sums to 183.
`MemberService.cs` runs multi-`Include` reads (lines 502-510, 931-934, 1101-1102) pulling
`AcademicHistory`, `ProfessionalHistory`, `ECMembers` and `PaymentHistories` together, tracked.

**Scale classification**, per REVIEW.md §19's requirement to say when something matters:

| Scale | Assessment |
|---|---|
| Current (≈631 members) | Negligible. No action needed |
| Medium growth (low thousands) | Measurable on admin list and export endpoints |
| Large growth | Needs a project-wide read-query audit |

**Recommendation:** a targeted pass over the confirmed read-only paths in `MemberService`,
`FinancialService` and `NetworkingService` — not a blanket automated change, which would risk
breaking the paths that legitimately rely on tracking.

### Finding 7 — Caching and SignalR are process-local, which caps the platform at one instance

**Status:** implemented but incomplete, for the large-growth tier only. **Disposition:** Newly Created → 82.20 (as a documented constraint, not a build task).

`Program.cs:64` registers `AddOutputCache()` with no Redis backing. `OrgConfigService` and
`ThemeService` use `IMemoryCache` directly — there is no cache abstraction interface at all.
`Program.cs:170` registers `AddSignalR()` with no backplane. `ChatHub`'s connection map is a
process-local `ConcurrentDictionary` by construction.

**Why this is not a defect today:** the platform runs as a single Render instance. Every one of these
choices is correct at one instance.

**Why it is worth recording:** the moment a second instance exists, output-cache entries, org-config
and theme caches, and SignalR group membership all become instance-inconsistent — a client on
instance A would not receive a push sent from instance B. Building Redis-backed caching now would be
exactly the overengineering REVIEW.md §4 warns against. The right output is a recorded constraint:
*this application cannot run more than one instance until these three things are addressed.*

### Finding 8 — In-app notifications bypass the template system entirely

**Status:** partially implemented. **Disposition:** Newly Created → 82.21.

`EmailTemplate` is a real config-driven templating mechanism — DB rows with `Code`, `Subject`, `Body`,
`Variables`, and a `Channel` field whose enum includes `Sms`. But
`NotificationService.CreateNotificationAsync` and `BroadcastNotificationAsync` never reference
`EmailTemplate` at all; they take raw `title`/`message` strings from the caller and write straight to
the `Notification` table plus a real-time push.

**Why it matters:** there are two notification paths with different governance. Email is
admin-editable through templates; in-app text is hardcoded at each call site. An administrator who
edits a template reasonably expects the in-app version to change too, and it does not.

### Finding 9 — Payment gateway credentials are stored in plaintext columns

**Status:** implemented but incomplete. **Disposition:** Expanded into WP 48 (security), not a new WP 82 item.

`PaymentConfiguration` carries `GatewayPublicKey` and `GatewaySecretKey` as `[MaxLength(500)]` plain
strings with no column-level encryption or protection marker.

**Context that lowers the severity:** per `feedback` in the project's own standing notes, no live
payment-gateway keys exist for this deployment — every method is admin-configured and works manually
without gateway credentials. So the columns are, today, empty or non-secret.

**Why it still belongs on the register:** the schema invites a secret to be stored in plaintext the
moment the association does obtain gateway credentials. It belongs with WP 48's secret-handling work
rather than as a standalone architecture item.

### Finding 10 — `Notification` has no index at all, on a table that grows per broadcast

**Status:** missing. **Disposition:** Newly Created → 82.22.

Of the 46 entities, only 15 configuration files declare any `HasIndex`. `Notification` declares a
`HasQueryFilter` (through `Member.IsArchived`) but no index — not even on `MemberId`.
`NotificationService.GetUserNotificationsAsync` runs
`.Where(n => n.MemberId == memberId).OrderByDescending(n => n.CreatedAt).Take(50)`: an unindexed
filter plus sort on a table that gains one row per member per broadcast.

`MembershipHistory` and `MembershipDue` likewise have only query filters, no index on their own
`MemberId` foreign key, and both are queried per member. `FinancialRecord` has no configuration file
at all and no index, despite `Year`, `RecordType` and `FinancialCategory` being its natural filters.

**Why it matters now:** unlike Finding 6, this one degrades with *broadcast* count rather than member
count, and the notification table is the one that grows fastest by construction.

### Finding 11 — Two health endpoints of different depth, with no stated difference

**Status:** partially implemented. **Disposition:** Expanded into 82.9 (observability), not a new item.

**Correction to an earlier draft of this report.** A research pass claimed `/health` had no registered
checks and therefore could never report unhealthy. That is wrong. `AddHealthChecks()
.AddDbContextCheck<ApplicationDbContext>()` is registered at
`GHCAA.Infrastructure/DependencyInjection.cs:59` — the claim came from grepping `Program.cs` alone.
`/health` does fail when the database is unreachable. The error is recorded rather than quietly
fixed because it is the kind a reader should be able to audit.

What is actually true: there are two health surfaces of different depth and nothing says which to use.
`Program.cs:351` maps `/health`, backed by the single `DbContextCheck` above.
`HealthController.cs` implements `GET /healthz`, which additionally checks file-storage directory
existence and Gmail/SMS configuration presence, returning `503` on failure. The Angular client
(`app.constants.ts:532`) and the Playwright readiness probe both target `/healthz`.

**Why it still matters:** an uptime monitor or Render probe pointed at the conventional `/health`
would report healthy while file storage or mail configuration is broken. That is a narrower problem
than "cannot fail" but a real one.

**Fix:** decide which endpoint is the contract, point monitoring at it, and either fold the extra
checks into the `AddHealthChecks` pipeline or document `/health` as liveness and `/healthz` as
readiness. Belongs inside 82.9 rather than as a separate item.

### Finding 12 — The Playwright suite exists and CI never runs it

**Status:** implemented but incomplete. **Disposition:** Newly Created → 82.23.

`GHCAA.Web/tests/` holds 30 Playwright spec files covering admin workflows, the alumni directory,
article editorial, membership and event flows, and gallery. `package.json` defines `test:e2e`.
`playwright.config.ts` boots both the API and the Angular dev server and polls `/healthz`. Neither
`ghcaa-ci-preprod.yml` nor `ghcaa-ci-standard.yml` invokes `npm run test:e2e` anywhere — both run only
the vitest unit suite.

**Why it matters:** 30 spec files covering the platform's critical business flows can rot silently. A
broken membership-approval flow would not fail any gate. Cost to fix is one CI job.

### Finding 13 — A second CI workflow duplicates the real one, more weakly

**Status:** deprecated or unnecessary. **Disposition:** Newly Created → 82.24.

`.github/workflows/main.yml` triggers on push and pull_request to `main` and `master` — overlapping
`ghcaa-ci-standard.yml`, which triggers on `dev`, `main`, `master`. But `main.yml` targets
`dotnet-version: 8.x` (the project is .NET 9 everywhere else), runs **no tests at all** — build only,
no `dotnet test`, no vitest — and uses unpinned action tags.

So two workflows race on every push to `main`: one runs the full lint/test/build chain, the other runs
a weaker, wrong-runtime build that can report green independently. It looks like a leftover from
before `ghcaa-ci-standard.yml` existed.

**Fix:** delete `main.yml`. `ghcaa-ci-standard.yml` supersedes it entirely.

### Finding 14 — Action pinning reached one workflow of five

**Status:** partially implemented. **Disposition:** Expanded into 48.19 (which did the preprod file).

48.19 pinned the five actions in `ghcaa-ci-preprod.yml` to commit SHAs and the Dockerfile bases to
digests, on 2026-09-04. It did not touch the other four workflow files, which its own text scoped it
out of. Still on mutable tags: `ghcaa-ci-standard.yml` (five actions), `neon_workflow.yml`
(`tj-actions/branch-names@v8`, `neondatabase/create-branch-action@v6`,
`neondatabase/delete-branch-action@v3`), and `mobile_deployment.yml` (seven actions).

**Why `mobile_deployment.yml` ranks highest of the three:** it runs with Play Store, App Store and
Android-keystore secrets in scope (`ANDROID_KEYSTORE_BASE64`, `IOS_P12_CERTIFICATE`). The tag-hijack
argument that justified 48.19 applies at least as strongly to a workflow holding code-signing
material.

### Finding 15 — The mobile release pipeline has no test gate

**Status:** missing. **Disposition:** Newly Created → 82.25.

`mobile_deployment.yml` triggers on `push: tags: v*` and goes straight to
`flutter build appbundle --release` / `flutter build ipa --release`, then uploads to the Play Store
internal track and TestFlight. There is no `flutter test` step and no `needs:` tying the release to a
passing test run. It relies entirely on the tagged commit having been gated earlier by a different
workflow.

**Why it matters:** a hand-pushed or stale tag ships untested code to app stores, where a bad build
cannot be hot-fixed.

### Finding 16 — CI runs no dependency or container scanning

**Status:** missing. **Disposition:** Newly Created → 82.26 (deliberately small).

No CodeQL workflow, no `npm audit`, no `dotnet list package --vulnerable`, no image scan anywhere in
`.github/workflows/`.

**What is proportionate here matters more than usual.** A full SAST/DAST pipeline would be exactly the
overengineering REVIEW.md §4 rules out at this scale. But `npm audit --audit-level=high` and
`dotnet list package --vulnerable` are two lines that would have caught what WP 48.9 and 48.17 (stale
`xlsx`, Angular CVEs) had to find by hand. The recommendation is those two commands, not a security
stack.

### Finding 17 — The README describes the schema mechanism incorrectly

**Status:** incorrectly implemented (documentation). **Disposition:** Newly Created → 82.27.

`README.md:324` states: "Schema — created and seeded at startup via EF Core `EnsureCreated()` (not
migrations); safe to re-run against an existing database."

`Program.cs:388-405` shows the opposite: `MigrationBootstrapper.EnsureMigratedAsync` runs first and
applies real EF Core migrations, baselining migration history on a legacy `EnsureCreated`-built
database. `EnsureCreated()` survives only as the `catch` fallback, logged as an error.

**Why this specific error is expensive:** it is aimed at the operator audience, on the one subject
where a wrong belief is most costly. An operator reading it would conclude the platform has no
migration path and plan schema changes accordingly. 82.13 already tracks the *absence* of recorded
decisions; it does not catch that an existing statement is actively wrong.

### Finding 18 — `Localization` is code-owned while appearing administrator-editable

**Status:** implemented and verified (deliberate), documented nowhere outside a code comment. **Disposition:** Expanded into 82.11.

`OrgConfigService.GetConfigAsync:44` always overwrites `dto.Localization` with
`BuildGhcaaDefaults().Localization` before returning, with a comment explaining why: the admin UI never
edits Localization, but a stored row round-trips the whole DTO on every save, so a stale copy would
otherwise persist forever once written.

**Why it belongs in the classification 82.11 will produce:** `Localization` sits in the same DB row and
the same DTO as the genuinely admin-editable fields, so the schema implies it is administrator-defined
when it is source-code-defined. That is precisely the kind of thing §10-11's table exists to make
explicit. It is correct behaviour, wrongly-signalled shape.

### Finding 19 — One dead scaffold test file

**Status:** deprecated or unnecessary. **Disposition:** Newly Created → 82.28 (bundled, trivial).

`GHCAA.Tests/UnitTest1.cs` is the unmodified `dotnet new nunit` scaffold (`Assert.Pass()`), in an
otherwise well-organised 79-file suite. That is the whole of this item — a one-file deletion.

**The five loose migrations under `Data/Migrations/` are explicitly NOT part of this**, contrary to an
earlier draft: they are live migrations (see Finding 1). Moving them into `PgSql/` for consistency is
a separate, riskier change than a deletion and is not recommended without a migration-chain test.

### Findings checked and confirmed correct — no action

Recorded so a future reviewer does not re-open them.

- **No CQRS/MediatR.** No MediatR reference in any backend `.csproj`. `docs/book/06-architecture.md`
  §6.12.3 records this as a deliberate choice. Correct at this scale — single maintainer, no read/write
  model divergence pressure. Do not raise as a gap.
- **Repository pattern applied to exactly one entity.** `GHCAA.Infrastructure/Repositories/` contains
  only `FileUploadRepository.cs`. `DbContext` already is EF's unit of work, and a blanket generic
  repository over EF Core is a known anti-pattern. Correct as-is.
- **Rate limiting is global, not login-only.** `Program.cs:85-136` defines `Auth`, `Refresh` and
  `Registration` policies plus a catch-all `Api` policy applied at `MapControllers()` (line 350).
  Broader than the brief assumed.
- **CSRF, step-up MFA, CORS fail-fast, mass assignment.** `XsrfMiddleware` registered at line 343;
  `RequireStepUpAttribute` applied across the admin, governance, roles, ledger and payment-config
  controllers; no controller binds a domain entity from `[FromBody]` (DTOs at every boundary sampled).
- **`appsettings.json` placeholders are not live secrets.** `"Password": "CHANGE_ME"` and empty
  `ClientSecret`/`ApiKey` entries are dev placeholders. The real leak is `docs/deploy_connection.txt`,
  already tracked as 48.2/48.13 and blocked on credential rotation by the user.

---

## F. Architecture gap analysis

| Area | Current state | Target state | Gap | Priority | Action |
|---|---|---|---|---|---|
| Layering | Compiler-enforced, correct | Same | None | — | Retain |
| DB provider | One works, three advertised | One, honestly | Remove dead shims or finish them | P2 | 82.15 |
| Financial audit trail | Mutable, hard-deletable | Append-only with actor and time | Audit fields + soft delete | P1 | 82.16 |
| Config binding | Raw string keys | Typed options | `IOptions<T>` per section | P3 | 82.17 |
| Token replay | Rotation only | Rotation + reuse detection | One branch | P2 | 82.18 |
| Read performance | 5/39 use `AsNoTracking` | Read paths untracked | Targeted pass | P3 | 82.19 |
| Horizontal scale | Process-local state | Documented single-instance limit | Write the constraint down | P3 | 82.20 |
| Notifications | Two paths, one templated | One governance model | Route in-app through templates | P3 | 82.21 |
| Indexing | 15/46 configured | Hot paths indexed | 4 named indexes | P2 | 82.22 |
| E2E in CI | Suite exists, never runs | Gated | One CI job | P2 | 82.23 |
| CI topology | Two overlapping workflows | One | Delete `main.yml` | P2 | 82.24 |
| Supply chain | 1 of 5 workflows pinned | All pinned | Extend 48.19 | P2 | 48.19 expanded |
| Mobile release | No test gate | Gated on tests | `needs:` or a test step | P2 | 82.25 |
| Dependency scanning | None | Two commands in CI | `npm audit`, `dotnet list --vulnerable` | P3 | 82.26 |
| Observability | No correlation ID, dead `/health` | Traceable request path | 82.9 plus the health fix | P3 | 82.9 expanded |
| Docs accuracy | README wrong on schema | Correct | One paragraph | P2 | 82.27 |

## G. Configuration-driven architecture blueprint

The mechanism that exists (WP 28), read from source rather than from
`docs/CONFIG_DRIVEN_FRAMEWORK.md`'s claims: `OrganizationConfig` is one row per `OrgId` holding a
`ConfigJson` blob, with `SchemaVersion` and an application-generated `RowVersion`.
`OrgConfigService.GetConfigAsync` resolves cache → DB row → `BuildGhcaaDefaults()` static fallback, so
missing config cannot crash the application. Writes replace the whole DTO; there is no per-field
diffing and no history of prior configurations.

| Capability | Classification | Evidence and reason |
|---|---|---|
| Organisation identity | Hybrid, with three sources of truth | Admin-editable via `PUT /api/config`, but GHC's values also live in `BuildGhcaaDefaults()` and again in `Constants.cs`. Tracked as TD-1/TD-4 and WP 62.7 |
| Feature flags (17) | Administrator-defined | `FeatureToggleDto`. Server side is real; whether both clients gate on it is WP 28's open consumer work |
| Membership types | Code-defined taxonomy, admin-assigned instance | `Enums.MembershipType`. Correct — tiers are assigned by an admin, not defined by one (35.5) |
| Membership fees | Administrator-defined | `MembershipFeeConfig`, effective-dated. Correct |
| Approval mode | Administrator-defined, single-valued | `WorkflowDto.MemberApprovalMode` is a bare string with one live behaviour. Surface exists ahead of branching logic |
| Governance roles | Code-defined enum, labels duplicated twice | `Enums.ECPosition` (16), labels in `BuildGhcaaDefaults().EcRoleLabels` **and** `app.constants.ts` `EC_ROLES` (13, different order). Real duplication — WP 62.34 |
| Term durations | Administrator-defined | `ECPeriod` plus real overlap rules in `GovernanceService`. Correct |
| Content pages | Administrator-defined, no revisioning | `SiteContent`. Every save is live; prior text is lost, unlike `Constitution` |
| Notification templates | Administrator-defined for email only | See Finding 8 |
| Payment providers | Administrator-defined, plaintext credentials | See Finding 9 |
| Email/SMS providers | Environment-defined | No provider-config entity among the 46 models; selection lives in `appsettings` |
| Localization | **Code-defined, despite appearing otherwise** | See Finding 18 |

### What must never become freely configurable

Required explicitly by REVIEW.md §11 and by 82.11.

1. **Constitutional voting eligibility.** `GovernanceService.VoteOnConstitutionAsync:251-279` hardcodes
   the Founding/Executive/General check with the citation `// Article III Section K` in the code. It
   is correctly in code. Changing who may vote is a constitutional amendment, not an admin toggle.
2. **Constitution version lifecycle.** `ActivateConstitutionAsync` — supersede, never delete; one
   active version, enforced in code.
3. **Election rules**, when WP 37.1 builds them. They must inherit the same principle: audited state
   transitions, not a PUT-able config blob.
4. Nothing else in the current schema reaches this bar. Branding, features, fees and role labels are
   all legitimately administrator-editable in principle.

## H. Replaceability matrix

| Capability | Implementation | Coupling | Recommendation |
|---|---|---|---|
| Database provider | EF Core + three shims | Low in code, **total in migrations** | Honest single-provider, or finish the others (Finding 1) |
| Payment gateway | 4 adapters + factory | Low | Retain. The model to copy |
| Email | `IEmailService`, one impl | Low | Correct amount of abstraction |
| File storage | `IFileStorageService`, one impl | Low | Correct. ADR-06 defers a second adapter until a second need is real |
| SMS | Provider config in appsettings | Medium | Acceptable; no entity, so no admin swap |
| Cache | `IMemoryCache` used directly, no interface | **High** | Only matters at multi-instance (Finding 7) |
| Search | None (EF `Where`) | — | Correct at this scale |
| Auth | Self-hosted JWT | Medium | Correct; no external IdP requirement exists |
| Logging | `ILogger<T>` only | Low | Sink choice is still open (82.9) |

## I. Security gap analysis

WP 24 and the OWASP round-2 block were read first and treated as closed. What follows is only what
they did not cover.

| Control | State | Gap | Item |
|---|---|---|---|
| Refresh rotation | Implemented | No reuse detection | 82.18 |
| Financial audit trail | Missing | Records mutable, no actor | 82.16 |
| Gateway credentials | Plaintext columns | Currently empty, schema invites it | WP 48 |
| Supply chain (CI) | 1 of 5 workflows pinned | Signing-secret workflow unpinned | 48.19 expanded |
| Dependency scanning | None | Two CI commands | 82.26 |
| Committed secrets | Open, blocked on user | Credential rotation | 48.2 / 48.13 |

## J. Technical debt register

| ID | Area | Issue | Evidence | Impact | Priority | Action |
|---|---|---|---|---|---|---|
| TD-A | Data | Financial records mutable/deletable | `FinancialLedgerService.cs:69,91`; `FinancialService.cs:488` | Money changes untraceable | P1 | 82.16 |
| TD-B | Data | `Notification` unindexed, grows per broadcast | 15/46 configs have `HasIndex` | Scan cost grows fastest here | P2 | 82.22 |
| TD-C | Infra | MySQL/SQLite advertised, unsupported | `Migrations/` has only `PgSql/` | Misleads; ADR-02 is wrong | P2 | 82.15 |
| TD-D | CI | `main.yml` duplicate, weaker, .NET 8 | `.github/workflows/main.yml` | False green | P2 | 82.24 |
| TD-E | CI | 4 of 5 workflows unpinned | `grep uses:` across workflows | Tag hijack, signing secrets | P2 | 48.19 |
| TD-F | Ops | Two health endpoints, different depth, no stated contract | `Program.cs:351`; `DependencyInjection.cs:59`; `HealthController.cs` | Monitor may miss storage/mail failure | P3 | 82.9 |
| TD-G | Docs | README schema claim false | `README.md:324` vs `Program.cs:388` | Misleads operator | P2 | 82.27 |
| TD-H | Code | No options pattern | 0 `IOptions<>` matches | Silent config typos | P3 | 82.17 |
| TD-I | Code | Dead scaffold test file | `GHCAA.Tests/UnitTest1.cs` | Noise | P4 | 82.28 |
| TD-J | Test | E2E suite never runs | 30 specs, no CI call | Silent rot | P2 | 82.23 |
| TD-K | Perf | 5/39 services use `AsNoTracking` | 183 read queries | Overhead at growth | P3 | 82.19 |

Already tracked, not re-derived: 47.13.3–47.13.6 (mutation coverage), 57.4 (news status overwrite),
48.2/48.13 (committed secrets, blocked on user), 48.18a (live-Postgres migration check), 82.3–82.13.

## K. Refactoring backlog

Under SR-8, each row states the problem before the change, so the justification travels with the work.

| ID | Refactoring | Problem it solves | Evidence | Risk class (§25.10) | Item |
|---|---|---|---|---|---|
| R-1 | Split `MemberService` | One 1,577-line file holds registry, approval, profile and search; any change re-reads and re-tests all four | Twice the next largest service | Controlled | 82.6 |
| R-2 | Extract current-user accessor | 55 copies of claim parsing, each re-deciding the missing-claim case | `grep FindFirst` in controllers | Safe | 82.5 |
| R-3 | One error contract | Three response shapes; no client can parse errors one way | 21 + 35 + 6 + 1 shapes counted | Safe | 82.4 |
| R-4 | Move 7 components onto services | They bypass the endpoint constants, shaping and error handling the other ~40 services centralise | 7 files inject `HttpClient` | Safe | 82.7 |
| R-5 | Delete DB-provider shims | Code advertises optionality it does not have | Finding 1 | Safe | 82.15 |
| R-6 | Audit fields on money entities | A ledger edit leaves no trace of value or actor | Finding 2 | Controlled | 82.16 |
| R-7 | Typed options | Mistyped key silently returns default | Finding 4 | Safe | 82.17 |
| R-8 | Delete `main.yml` | Two workflows race; the weaker can go green alone | Finding 13 | Safe | 82.24 |
| R-9 | Delete dead scaffold test | Rediscovered by three separate passes now | Finding 19 | Safe | 82.28 |

Rejected refactorings, recorded so they are not proposed again: generic repository over EF Core;
MediatR/CQRS; a generic workflow engine; Redis-backed cache at single-instance; client code generation
from OpenAPI (already decided and closed as 82.10).

## L. Duplication and centralisation matrices

Partial, per the coverage note — no systematic duplication sweep ran.

| Area | Duplicated behaviour | Locations | Action |
|---|---|---|---|
| Org identity values | GHC's own name/prefix/email | `Constants.cs`, `BuildGhcaaDefaults()`, `app.constants.ts` | WP 62.7 |
| EC role labels | 16 positions | `BuildGhcaaDefaults().EcRoleLabels`, `app.constants.ts` `EC_ROLES` (13, different order) | WP 62.34 |
| Claim parsing | Current-user resolution | 55 sites | 82.5 |
| Error shapes | Failure responses | 4 shapes | 82.4 |

| Capability | Current locations | Authoritative home | Reason |
|---|---|---|---|
| Current user | 55 controllers | One accessor | Identity resolution is one decision |
| Error contract | Per controller | One shape | Clients parse once |
| Org identity | Three | Profile pack (WP 62) | One institution, one source |
| Notification text | Templates + call sites | Templates | Finding 8 |

## M. Reusable component inventory

Backend: validation pipeline (FluentValidation, present), exception handling (`ExceptionMiddleware`,
present), auditing (`AuditLogMiddleware`, present), pagination (**inconsistent** — 82.8), authorization
helpers (policies + `RequireStepUp`, present), current-user accessor (**missing** — 82.5).

Angular and Flutter inventories are **not assessed** — see the coverage note. `GHCAA.Web` is known to
have a shared control layer from earlier work, and the 2026-09-04 duplication audit consolidated 25
spec files onto a shared mock, but no systematic component inventory was taken in this pass.

## N. Reconciliation registers

### N.1 Existing-task reconciliation matrix

| Existing item | Finding | Match | Decision |
|---|---|---|---|
| 82.3–82.8, 82.10–82.13 | Re-confirmed, not re-derived | Exact | Retained |
| 82.9 (observability) | Findings 11, 17.3 (prod logging config) | Partial | Expanded |
| 82.11 (config classification) | Finding 18 (Localization) | Partial | Expanded |
| 48.19 (CI pinning) | Finding 14 (other 4 workflows) | Partial | Expanded |
| WP 48 (security) | Finding 9 (plaintext gateway keys) | Related | Expanded |
| WP 62.7 / 62.34 | Duplication matrix rows | Exact | Retained |
| 24.27 / 48.3 (tokens) | Finding 5 | Related, not covered | Newly Created (82.18) |
| WP 24 payment hardening | Finding 2 | Related, not covered | Newly Created (82.16) |
| 82.6 (`MemberService`) | Finding 6 (`AsNoTracking`) | Related, different problem | Newly Created (82.19) |
| 34.D10 | Finding 17 (README) | Related | Newly Created (82.27) |
| CQRS / repository / rate limiting / CSRF / mass assignment | Checked, correct | — | Rejected as findings |

### N.2 New-task justification register

Every new item states which existing packages were checked and why none cover it.

| New | Title | Existing checked | Why not covered | Benefit |
|---|---|---|---|---|
| 82.14 | Angular and Flutter review | 82.7, 82.10b, WP 30/33 | Those are single findings, not a review | Closes this report's own gap |
| 82.15 | DB provider honesty | WP 31, 34.D10, 82.x | All concern Postgres migrations working, not other providers failing | Removes false optionality |
| 82.16 | Financial audit trail | WP 24 (payments), WP 19, 46.5 | All concern correctness of amounts, not mutability of records | Money changes traceable |
| 82.17 | Typed options | 82.11, 82.12, WP 28 | Those classify values; this is binding mechanism | Config typos fail at compile |
| 82.18 | Token reuse detection | 24.27, 48.3, 48.12 | Rotation and revoke-on-reset exist; replay detection does not | Detects stolen tokens |
| 82.19 | `AsNoTracking` pass | 82.6, 8.3 | 82.6 is file size; 8.3 is mobile pagination | Removes tracking overhead |
| 82.20 | Single-instance constraint | 41, 82.9 | Deployment and logging, not scale-out limits | Records a real ceiling |
| 82.21 | Notification templating | WP 50, 81.1 | WP 50 built email templates; in-app path was never in scope | One governance model |
| 82.22 | Hot-path indexes | 24.37, 8.3 | 24.37 indexed `Member` only | Removes a growing scan |
| 82.23 | E2E in CI | WP 14, 20, 22, 27 | Those wrote the specs; none run them in CI | Stops silent rot |
| 82.24 | Delete `main.yml` | 48.19, WP 41 | 48.19 scoped itself to the preprod file | Removes false green |
| 82.25 | Mobile release gate | WP 8, 48.19 | Neither covers release-pipeline gating | Untested builds can't ship |
| 82.26 | Dependency scanning | 48.9, 48.17, 48.18 | Those fixed specific CVEs by hand | Catches the next one automatically |
| 82.27 | README schema correction | 82.13, 34.D10 | 82.13 covers missing ADRs, not an existing false statement | Operator not misled |
| 82.28 | Delete dead scaffold test | 61.1, 61.2 | Deferred into 62.47/62.48; this one file is already identified | Two-minute cleanup |

### N.3 Obsolete, duplicate and superseded register

| Item | Reason | Recommendation |
|---|---|---|
| `main.yml` | Superseded by `ghcaa-ci-standard.yml` | Remove (82.24) |
| `UnitTest1.cs` | Scaffold, never written | Remove (82.28) |
| `/health` endpoint | Shallower duplicate of `/healthz` | Decide the contract (82.9) |
| ADR-02's multi-provider claim | Contradicted by the tree | Rewrite (82.15) |
| `README.md:324` schema claim | Contradicted by `Program.cs:388` | Rewrite (82.27) |
| MySQL/SQLite shim contexts | Support nothing | Remove with 82.15 |

## O. Roadmap placement

Every action appears once, in one phase.

- **Phase 0, critical stabilisation:** 82.16 (financial audit trail). Blocked on user: 48.2, 48.13.
- **Phase 1, architecture and foundation:** 82.15, 82.24, 82.5, 82.4, 82.3.
- **Phase 2, configuration platform:** 82.11, 82.12, WP 62's own chain.
- **Phase 3, quality and security:** 82.18, 82.23, 82.25, 82.26, 48.19 expanded, 82.14.
- **Phase 4, operational maturity:** 82.9 with the health fix, 82.20, 82.22, 82.27.
- **Phase 5, later:** 82.17, 82.19, 82.21, 82.28.

## P. Method and limits

Produced 2026-09-04 against the working tree of that date. Backend test suite stood at 573 passing.
Every count in this report came from a command run against the tree, not an estimate; where a figure
is approximate it says so.

Two of five research streams produced no output before the session hit a provider rate limit, so §7,
§8 and §25's systematic sweep are not covered — stated in the coverage table, in §M, and tracked as
82.14 rather than papered over. A reader should treat the Angular and Flutter clients as unexamined by
this audit.
