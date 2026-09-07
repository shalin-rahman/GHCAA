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
| §7 Angular application | Yes | Read of `GHCAA.Web/src/app` (routing, DI, interceptors, guards, services, forms, templates); see §Q |
| §8 Flutter application | Yes | Read of `GHCAA.Mobile/lib` (core/features/screens, Riverpod providers, storage, config); see §R |
| §9 Cross-platform | Partial | Through the API-contract findings already tracked as 82.3/82.4/82.10, plus the cross-client duplication sweep in §S |
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
| §25 Refactoring and duplication | Yes | §25.1/§25.2 (backend, 2026-09-04) plus the client-side sweep added 2026-09-06; see §S |

**2026-09-06 update, closing 82.14:** §7, §8 and §25's client-side sweep were not assessed in the
2026-09-04 pass (two of five research streams returned nothing before the session hit a provider rate
limit). That gap is now closed: §Q covers Angular, §R covers Flutter, §S covers the cross-client and
per-client duplication sweep, each to the same evidence standard as the rest of this report (file path
or line per finding, a §3 status label, an 82.2 disposition). §L and §M below are updated with pointers
to the new sections rather than restating them.

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

**Closed 2026-09-07 (82.15):** the recommendation was followed for MySQL only — `MySqlApplicationDbContext`,
`MySqlDesignTimeDbContextFactory`, the `mysql` DI switch case, its connection-string branch, and the
`Pomelo.EntityFrameworkCore.MySql` package are gone. Sqlite was kept deliberately: two integration test
factories boot the real API with `DatabaseProvider=Sqlite` for a fast, migration-free `EnsureCreated()`
path under the Visual seed profile, which never runs `MigrationBootstrapper` and so was never subject to
this finding. Reasoning recorded in `docs/adr/0006-drop-mysql-provider.md`. ADR-02's "PostgreSQL, MySQL
and SQLite" claim still needs its own correction in `docs/book/06-architecture.md` through the book's
strict build pipeline — flagged there, not done here.

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
**Closed 2026-09-04** for the rule itself: `docs/ARCHITECTURE.md` §4 now states which entity classes
carry audit fields and which do not, and `FinancialRecord` and `PaymentHistory` were brought into line.
The two gaps the rule names — `ECMember`'s two removal semantics, and `Member`/`User` using
`IsArchived` where the rule says `IsDeleted` — stay open as 82.29 and 82.30.

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

Backend-only, from the 2026-09-04 pass. The client-side (Angular/Flutter) duplication matrix is §S —
kept separate rather than merged in here because it was produced in a later pass against a different
brief section (§25's client-side sweep) and cites different evidence (TypeScript/Dart file paths).

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

Angular: a small, proven shared set (`page-header`, `search-bar`, `breadcrumb`, `toast`,
`theme-toggle`, `logo-spinner`, `icon`) used across ~19-40+ screens each — appropriately scaled for
the app's size, not a gap by itself. What is missing is a shared confirm-dialog and a shared
modal-header shell; see §S findings 9-10. Flutter: `GlassContainer`, `LogoSpinner`, `AppTheme`
tokens, `AdminActionCircle`, and `AppSearchField` (debounced) are the proven shared set; the gap is
the same shape as Angular's — no shared confirm-dialog, and two screens bypass the existing
`AppSearchField` to hand-roll their own debounce; see §S findings 13-14.

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

- **Phase 0, critical stabilisation:** 82.16 (financial audit trail), 82.38 (mobile stale-role/profile
  on logout). Blocked on user: 48.2, 48.13.
- **Phase 1, architecture and foundation:** 82.15, 82.24, 82.5, 82.4, 82.3, 82.42 (lookups
  centralisation), 82.34 (dead Angular interceptor).
- **Phase 2, configuration platform:** 82.11, 82.12, WP 62's own chain.
- **Phase 3, quality and security:** 82.18, 82.23, 82.25, 82.26, 48.19 expanded, 82.14 (this pass),
  82.40 (mobile biometric credential storage), 82.39 (mobile dual inactivity timers), 82.36 (Angular
  HTTP retry).
- **Phase 4, operational maturity:** 82.9 with the health fix, 82.20, 82.22, 82.27, 82.41 (mobile push
  token registration + tap-through).
- **Phase 5, later:** 82.17, 82.19, 82.21, 82.28, 82.37 (Angular ARIA pass), 82.43-82.49 (client-side
  UI-consistency/duplication cleanups: confirm-dialog and modal-header extraction on both clients,
  debounce consolidation, dead-code deletions, page-header consistency).

## P. Method and limits

Produced 2026-09-04 against the working tree of that date. Backend test suite stood at 573 passing.
Every count in this report came from a command run against the tree, not an estimate; where a figure
is approximate it says so.

Two of five research streams produced no output before the session hit a provider rate limit on
2026-09-04, so §7, §8 and §25's systematic sweep were not covered in that pass — stated at the time in
the coverage table and in §M, and tracked as 82.14 rather than papered over. That gap was closed on
2026-09-06: §Q, §R and §S cover the Angular application, the Flutter application, and the client-side
duplication sweep respectively, each produced by a dedicated read of the corresponding source tree
(`GHCAA.Web/src/app`, `GHCAA.Mobile/lib`) against REVIEW.md's §7/§8/§25 briefs, to the same
evidence-per-finding standard as the rest of this report.

---

## Q. Angular Application Review (REVIEW.md §7), added 2026-09-06

Scope: `GHCAA.Web/src/app`, 141 non-spec `.ts` files, 76 `.spec.ts` files, 73 templates.

**Architecture.** Standalone components throughout (0 `standalone: false` matches), feature-based
folders (`public/`, `member/`, `admin/`, `common/`, `core/`, `layouts/`) mirroring the app's three real
audiences, and lazy loading is the default routing strategy — 65 `loadComponent()` calls in
`app.routes.ts`, zero eager `component:` routes. DI is idiomatic `inject()`, not constructor
boilerplate (`auth.service.ts:15-16`, `auth.guard.ts:24-25`). Two gaps: `core/interceptors/
auth.interceptor.ts` is a complete, unit-tested, bearer-only interceptor that is never registered
(`app.config.ts:18` wires only `globalHttpInterceptor`, whose own bearer logic at
`global-http.interceptor.ts:30-36` is a superset) — dead code duplicating a subset of a live file.
**Status:** Deprecated or unnecessary. **Disposition:** Newly Created → 82.34 (delete the file and its
spec). Separately, `ChangeDetectionStrategy.OnPush` appears in only 3 of ~83 components despite the
signals architecture being well suited to it. **Status:** Implemented but incomplete. **Disposition:**
Newly Created → 82.35 (P4, opportunistic).

**State management.** Signals-in-services is the primary model (`auth.service.ts:18-28`,
`org-config.service.ts:13`, 65 files repo-wide use `signal(`), with RxJS used correctly for async
orchestration only (the 401-refresh gate's `BehaviorSubject` in `global-http.interceptor.ts:10-11`, a
signal-to-observable bridge for guard timing in `auth.guard.ts:16-18`) rather than as a competing state
store. No NgRx or any global store library is present (`package.json` dependencies checked; 0 `@ngrx`
matches) — confirming the prior TODO claim still holds. **Status:** Implemented and verified. NgRx is
not warranted: no observed symptom (cross-feature state thrashing, undo/time-travel, multi-team
coordination) that it would fix, and adding it would cost boilerplate against no real problem — exactly
the outcome REVIEW.md §4 and §7 ask the review to justify before recommending.

**API integration.** `global-http.interceptor.ts` is a single, well-designed pipeline: URL rewriting,
credentialed requests, bearer fallback, single-flight 401-refresh-and-retry, 403 step-up-challenge-and-
retry, centralised error-message mapping (lines 13-152). Auth is httpOnly-cookie-based
(`withCredentials: true`), token lifecycle includes a deferred `/auth/me` session restore
(`afterNextRender`, avoiding `NG0200`), a 10-minute inactivity auto-logout, and a guard-level
`authChecked` gate. CSRF is wired via Angular's built-in XSRF support (`app.config.ts:19`). Two real
gaps: no retry/backoff anywhere for transient 5xx/timeout failures (0 `retry(`/`retryWhen` matches) —
**Status:** Missing. **Disposition:** Newly Created → 82.36 (P3; scope to idempotent GETs only). And no
general HTTP response caching layer beyond `OrgConfigService`'s own one-shot cache — **Status:**
Missing. **Disposition:** Rejected as a finding for now: no evidence of a real performance problem at
this app's request volume: recommending a caching layer without one would be exactly the
overengineering REVIEW.md §4 warns against. Revisit if 82.36's retry work surfaces repeat-request cost.

**UI quality.** ARIA coverage is sparse (7 of 73 templates have any `aria-*` attribute) — **Status:**
Implemented but incomplete. **Disposition:** Newly Created → 82.37 (P3; scope to the highest-traffic
member/admin forms first, not a blanket pass). Forms are template-driven (`ngModel` + native
validators) everywhere except one file that uses reactive forms (`admin/events/admin-events.ts`) —
**Status:** Implemented but incomplete. **Disposition:** Rejected as a standalone finding: template-
driven forms with per-field messages (verified in `public/register/register.html`) work correctly
today; converting one inconsistent file to match the other 70+ is lower value than the app's actual
open gaps. Responsive design is hand-written `@media` per component (33 of 71 `.scss` files), not a
shared breakpoint system — acceptable at this scale, no action. Centralised feedback (`NotificationService`
+ `ToastComponent`) and a small, proven shared-component set are both confirmed working.

**Overall: Good.** See §Q's individual dispositions above for what keeps it short of Strong — none are
structural.

## R. Flutter Application Review (REVIEW.md §8), added 2026-09-06

Scope: `GHCAA.Mobile/lib`, 113 Dart files.

**Architecture & state.** `core/`/`features/`/`screens/` layering is consistent across all 113 files.
Riverpod is used consistently (`Provider`/`FutureProvider`/`StreamProvider`/`StateNotifierProvider`,
root `ProviderScope` in `main.dart:118-121`). One real correctness bug: `roleProvider` and
`userProfileProvider` are documented in-code as needing explicit `ref.invalidate` after login/logout,
but `AuthService.logout()` (`auth_service.dart:178-184`) only invalidates the notification-hub
provider — not these two. A user logging out and a different user logging back in on the same device
can see the previous user's cached role/profile until something else happens to refetch it.
**Status:** Incorrectly implemented. **Disposition:** Newly Created → 82.38 (P1 — this is a stale-
privilege-label bug on shared/handed-down devices, not cosmetic). Separately, inactivity/session-expiry
logic exists in two independent places with two different timeouts — `SessionManager` at 15 minutes
(`session_manager.dart:12`) and `main.dart`'s own `_checkInactivity` at 10 minutes
(`main.dart:154-155`) — each clearing the same storage independently. **Status:** Incorrectly
implemented. **Disposition:** Newly Created → 82.39 (P2; pick one timeout and one owner).

**API integration, auth & token storage.** A single shared Dio instance (`dioProvider`) with
single-flight refresh-token rotation on 401 (`api_client.dart:8-51,84-101`) is correctly implemented
and tested for the concurrent-request case. Tokens live in `flutter_secure_storage` on mobile with a
self-healing legacy migration off `SharedPreferences`; web correctly falls back to `SharedPreferences`
since secure storage isn't meaningfully stronger there (`storage_service.dart:21-93`). One finding
worth flagging rather than fixing outright: biometric "fast login" stores the user's raw username and
password in secure storage when opted in (`storage_service.dart:147-165`), which is a wider attack
surface than a device-bound token even though secure storage itself is appropriate. **Status:**
Implemented but incomplete. **Disposition:** Newly Created → 82.40 (P2; scope: replace with a
device-bound long-lived token, not a broader biometric-flow rewrite). 21 of 26 feature services still
parse raw `Map<String,dynamic>` rather than typed models — this reconfirms 82.10b, still open, and is
not re-created here.

**Offline, caching & sync.** Real-time connectivity detection is wired app-wide (`connectivity_plus`,
a persistent banner). `OrgConfigService` is the one genuine offline-first cache (network → cache →
hardcoded defaults). No other feature (events, jobs, directory, news, forum) has any local cache or
sync/conflict-resolution mechanism — fetch-on-build only. **Status:** Missing. **Disposition:**
Rejected as a standalone new item for now: the Flutter reviewer's own assessment is that this doesn't
block the app's current single-institution scale, and REVIEW.md §4 asks whether a problem is real now
or only a future possibility — recorded here as a known limitation for the roadmap (§O) rather than a
priced task, to revisit if multi-institution (WP 62) or spotty-connectivity fieldwork use cases become
real.

**Push & deep links.** FCM is wired (permission, token retrieval, foreground/background listeners) but
the device token is only logged, never registered with the backend for targeted push, and
`onMessageOpenedApp` logs the payload instead of navigating. **Status:** Implemented but incomplete.
**Disposition:** Newly Created → 82.41 (P3 — push exists and works for broadcast-style notifications
via the separate SignalR channel; targeted push and tap-to-navigate are the missing pieces). Deep
linking is entirely absent — no intent-filter beyond `MAIN`/`LAUNCHER` in the Android manifest, no
`CFBundleURLTypes` in `Info.plist`, no `uni_links`/`app_links` dependency. **Status:** Missing.
**Disposition:** Rejected as a standalone item: no current feature (email links, shared-content links)
depends on it; recorded as a backlog note, not priced, until a feature actually needs it.

**Configuration & platform.** Per-platform API base URL resolution, `.env`-driven branding/environment
name, and a separate build-time "profile pack" mechanism for white-labelling are all implemented and
verified. Screenshot prevention is correctly scoped to sensitive screens (digital ID, payment, financial
portal); biometric auth correctly guards itself off on web. No build-flavor-based dev/prod split (a
single bundled `.env` swapped manually pre-build) — acceptable at the current one-developer-controlled
release cadence; not raised as a new item.

**Business-logic duplication.** The membership-tier list was previously duplicated client-side and was
correctly deleted in favour of the backend dropdown (`registration_constants.dart:41-43`) — a positive
finding, not a defect. Outstanding-dues totals and ledger field remapping are still computed/reshaped
client-side rather than served pre-computed — folded into §S's cross-client lookups-centralisation
finding rather than a separate item, since it's the same root pattern (client re-deriving something the
backend could just serve).

**Overall: Developing.** Concrete, fixable gaps (82.38-82.41) rather than a structural rewrite; see §R's
dispositions for what separates it from Good.

## S. Client-Side Duplication and Centralisation Sweep (REVIEW.md §25 completion), added 2026-09-06

Backend duplication is §L; this covers cross-client and per-client duplication only, completing the
gap §L's own note names. 17 findings, in line with the "don't pad the list" instruction in the brief —
a few resolve to "leave as-is" deliberately, not by omission.

### Cross-client (Angular ↔ Flutter)

The same five small enum/lookup tables are hardcoded independently in both clients — membership status,
member category, gender, blood group, job category — plus academic-year list generation, instead of
using the `/lookups/{group}` endpoint that already exists and that `GHCAA.Mobile`'s own
`DropdownService` already calls first (`dropdown_service.dart:74-81`), falling back to its hardcoded
copy only when the API returns empty. Two of the five have already drifted in wording between clients
(membership status: `Applied` reads "Pending" on web, "Pending Approval" on mobile;
`InactivePayment` reads "Inactive" vs "Inactive (Unpaid)") — `app.constants.ts:26-37,52-64` vs
`dropdown_service.dart:105-138`. The root cause: `GHCAA.Web` already has a `LookupService` wired to
the same endpoint (`core/services/lookup.service.ts`), but the screens using these five lists
(`admin-members.ts`, `profile.ts`, `register.ts`) import the hardcoded constants instead of calling it.
**Status:** Incorrectly implemented (the two drifted labels), Partially implemented (the other three,
not yet drifted but same mechanism). **Disposition:** Newly Created → 82.42 (P2; wire the existing
`LookupService` into the Angular screens that bypass it — this alone collapses five separate findings
into one fix — then delete the now-redundant hardcoded lists in both clients). A related dead-code
note: `GHCAA.Mobile/lib/core/constants/registration_constants.dart:33-37`'s
`AcademicConstants.getAcademicYears` has no callers. **Status:** Deprecated or unnecessary.
**Disposition:** Newly Created → 82.43 (P4, two-minute deletion, fold into 82.42's change).

### GHCAA.Web-internal

| Finding | Locations | Disposition |
|---|---|---|
| Manual debounce (`clearTimeout`/`setTimeout(…,300)`) copy-pasted in 5 components | `admin-governance.ts:159-168`, `directory.ts:122-125`, `jobs.ts:67-72`, `messages.ts:112-122`, `requests.ts:50,92-94` | Newly Created → 82.44 (P3; small `debounce()` helper or a debounced output on `SearchBarComponent`) |
| Raw `window.confirm()` in 22 files, no shared styled dialog despite `step-up-dialog` already proving the pattern | `admin-members.ts`, `admin-roles.ts`, `admin-events.ts`, `admin-gallery.ts`, `jobs.ts`, `payment-portal.component.ts`, 16 more | Newly Created → 82.45 (P3; a UX-consistency issue as much as duplication — every delete/danger action currently breaks the app's own glass-UI language) |
| `.modal-header` markup hand-rolled identically in 17 files | `admin-roles.html:13-16`, `admin-themes.html`, 15 more | Newly Created → 82.46 (P3; bundle with 82.45 if a shared modal shell is built — same UI surface) |
| 4 admin screens hand-roll their own header instead of the already-proven `app-page-header` used by 19 others | `admin-comm.html:3-9`, `admin-dashboard.html`, `admin-event-operations.html`, `polls.html:3-6` | Newly Created → 82.47 (P4; pure consistency, low effort, no new component needed) |
| Empty-state markup (icon + one line) repeated across ~19 files, but styling already centralised in `styles.scss` | `admin-audit.html:44-46`, `jobs.html:186-188`, `directory.html:212-214`, 16 more | Rejected as a finding: 2-3 lines each, extracting a wrapper component costs more than it saves — the exact "avoid overengineering" case in REVIEW.md §4 |

### GHCAA.Mobile-internal

| Finding | Locations | Disposition |
|---|---|---|
| `showDialog<bool>` + styled `AlertDialog` confirm boilerplate repeated across ~19 screens, despite `reject_reason_dialog.dart` already proving a shared-dialog shape | `jobs_screen.dart:191-201`, `family_link_screen.dart:379-385`, `audit_screen.dart`, `fee_config_screen.dart`, `events_screen.dart`, `gallery_screen.dart`, `news_details_screen.dart`, 12 more | Newly Created → 82.48 (P3; mirrors 82.45 on the Web side) |
| Manual `Timer`-based search debounce in 2 screens that bypass the already-existing debounced `AppSearchField` widget | `directory_screen.dart:126-131`, `professional_hub_screen.dart:115-120` vs `core/widgets/app_search_field.dart:29-43` | Newly Created → 82.49 (P4; swap two screens onto the widget that already does this) |
| `_formatTime` duplicated between two chat screens, not quite identical (one needs today-vs-older branching) | `chats_screen.dart:264-274`, `chat_room_screen.dart:187-195` | Rejected as a finding: 2 call sites, ~10 lines, marginal — noted for awareness only |
| `try/catch { debugPrint(...); return false / rethrow }` boilerplate repeats in effectively every service method (100+ occurrences) | `job_service.dart`, `events_service.dart`, `forum_service.dart`, `admin_service.dart`, `auth_service.dart` | Rejected as a finding: coincidental three-line similarity, not a shared behaviour — a wrapper would have to support both "swallow" and "rethrow" callers and would reduce readability, the exact case REVIEW.md §25.2 says to leave alone. The `return false` branch silently discarding the real error message (e.g. `financial_service.dart`'s `getOutstandingDues`) is a correctness/UX concern, not a duplication one — folded into 82.40's error-handling scope rather than raised separately |
| Role literal strings `'SuperAdmin'`/`'Admin'` repeated 3× within one file | `permissions_matrix_screen.dart:190,246,283,303-306` | Rejected as a finding: single file, no cross-file drift risk, a local `const` is a nicety not a maintenance-cost item |

**Summary:** the strongest finding across all client-side work is the lookups-centralisation one
(82.42) — five duplicated tables, two already drifted, with the fix mechanism (`LookupService`)
already built and simply not wired up. The UI-consistency findings (82.44-82.49) are real but lower
stakes, and about a third of everything the sweep looked at (empty-state markup, chat time formatting,
service-layer try/catch, three same-file role literals) was correctly triaged as not worth abstracting,
matching the brief's own caution against overengineering.
