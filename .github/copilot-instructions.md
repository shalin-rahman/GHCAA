# GHCAA Copilot Instructions

GHCAA is a full-stack alumni association platform. Treat the API, web client, and
mobile client as one product: changes to a shared API contract or business rule
must be checked against both clients. Log every endpoint change (new route, new
field, deprecated field) in `docs/API_CONTRACT_REGISTRY.md`, noting which client
picked it up — see that file for the format.

## Commands

Run commands from the repository root unless a working directory is shown.

| Purpose | Command |
| --- | --- |
| Restore/build all .NET projects | `dotnet restore` then `dotnet build -c Release` |
| Format check | `dotnet format --verify-no-changes` |
| Backend tests | `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --configuration Release` |
| One backend test | `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~Namespace.Class.TestName"` |
| Backend coverage | `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --collect:"XPlat Code Coverage" --settings GHCAA.Tests/coverlet.runsettings` |
| Web dependencies | `cd GHCAA.Web; npm ci` |
| Web type-check | `cd GHCAA.Web; npm run type-check` |
| Web production build | `cd GHCAA.Web; npm run build` |
| Web unit tests | `cd GHCAA.Web; npm run test:unit` |
| One web test file | `cd GHCAA.Web; npm run test:unit -- src/app/path/to/file.spec.ts` |
| One web test by name | `cd GHCAA.Web; npm run test:unit -- -t "test name"` |
| Web development server | `cd GHCAA.Web; npm start` |
| Playwright E2E tests | `cd GHCAA.Web; npx playwright install --with-deps chromium` then `npm run test:e2e` |
| One Playwright file | `cd GHCAA.Web; npx playwright test tests/e2e/path/to/file.spec.ts` |
| Mobile dependencies | `cd GHCAA.Mobile; flutter pub get` |
| Mobile analysis | `cd GHCAA.Mobile; flutter analyze` |
| Mobile tests | `cd GHCAA.Mobile; flutter test --exclude-tags golden --reporter expanded` |
| One mobile test file | `cd GHCAA.Mobile; flutter test test/path/to/file_test.dart` |
| One mobile test by name | `cd GHCAA.Mobile; flutter test test/path/to/file_test.dart --name "test name"` |
| Start the local stack | `.\GHCAA.Tools\run-app.ps1` |
| Stop the local stack | `.\GHCAA.Tools\stop-app.ps1` |

The standard GitHub Actions pipeline is
`.github/workflows/ghcaa-ci-standard.yml`; it runs backend formatting and
package checks, web type-checking and unit tests, Flutter analysis and tests,
Playwright E2E tests, an API contract snapshot comparison, and an integrated
API-plus-SPA build. `.github/workflows/ghcaa-ci-preprod.yml` runs the preprod
variant and deploys only after a push to `preprod`. Whether these checks are
required branch-protection status checks is configured outside the repository;
do not assume that a passing workflow blocks merges.

## Architecture

- `GHCAA.Domain` contains framework-independent entities, enums, and constants.
- `GHCAA.Application` defines DTOs, service interfaces, validators, and security
  primitives. Keep business contracts here.
- `GHCAA.Infrastructure` implements services, EF Core persistence, provider
  selection, integrations, file storage, and payment strategies. EF mappings
  belong in per-entity `IEntityTypeConfiguration<T>` classes.
- `GHCAA.API` is the thin HTTP host. Controllers delegate to application
  interfaces. Middleware owns cross-cutting HTTP behavior; SignalR hubs provide
  chat and notification real-time updates.
- `GHCAA.Web` is an Angular 21 standalone-component SPA. It is organized by
  `core`, `public`, `member`, `admin`, `common`, and `layouts`. Production
  packaging builds the SPA into the API's `wwwroot`; local development may run
  the API and Angular dev server separately.
- `GHCAA.Mobile` is a Flutter client using Riverpod, go_router, Dio, and
  SignalR. Feature services call the same REST API as the web client.
- PostgreSQL is the production provider; SQLite is the normal local/test
  provider. `MigrationBootstrapper` applies migrations at API startup.
- `GHCAA.Export` contains Excel/PDF-oriented export helpers used by the
  platform.

Dependencies point inward: Domain -> Application -> Infrastructure/API. Do not
put persistence or framework concerns into Domain, and do not bypass the
Application interfaces from clients.

## Repository-specific conventions

### Backend

- Use nullable reference types and the repository's existing C# naming/style.
- Register application and infrastructure services through their layer
  dependency-injection extensions. Infrastructure service implementations are
  convention-registered; avoid adding unrelated manual registrations.
- Use explicit DTO mapping in services. Do not introduce a generic repository,
  Unit of Work, CQRS, or mediator abstraction where the existing direct
  `ApplicationDbContext` pattern is sufficient.
- Preserve global soft-delete behavior. Archived records must remain hidden by
  EF query filters unless an explicit administrative/recovery query is intended.
- Keep controllers thin and put validation in FluentValidation validators or
  application services.
- Authentication is JWT-based with BCrypt password hashing and role-based access.
  Preserve the existing middleware protections for exceptions, security headers,
  audit logging, rate limiting, security stamps, and XSRF.
- Dates displayed to users use `dd-MM-yyyy`. Do not introduce `MM/dd/yyyy` or
  another UI date format. The API has a global date converter; do not work around
  it with ad-hoc serialization.
- Payment flows are admin-configurable and support manual wallet/bank/mobile
  financial service instructions. Never add or commit live payment gateway keys.
- Keep secrets in environment variables, user secrets, or the host configuration.
  Never commit `.env` files, credentials, connection strings, or diagnostic logs.

### Angular web

- Use standalone components, `inject()`, signals, and `computed()` in new code.
  Route through `app.routes.ts`; use typed services under `core/services`.
- Treat `GHCAA.Web/src/styles.scss` as the design-system source of truth.
  Surfaces, borders, text, inputs, and controls must consume semantic CSS
  variables such as `--bg-color`, `--surface-color`, `--surface-subtle`,
  `--card-bg`, `--card-border`, `--glass-bg`, `--glass-border`,
  `--accent-color`, `--text-main`, and `--text-muted`.
- Do not hardcode component colors, borders, backgrounds, or loading treatments.
  Fix shared behavior in `styles.scss` or the relevant shared component instead
  of creating route-specific copies.
- Reuse the existing shared controls and classes: `app-page-header`,
  `app-pagination`, `app-search-bar`, `.filter-bar`, `.data-table`,
  `.table-wrap`, `.empty-state`, `app-confirm-dialog`/`ConfirmDialogService`,
  and the shared logo/loading components.
- Review every admin table/grid for a usable search/filter control and consistent
  pagination before adding a new table. Search controls must follow the central
  theme tokens rather than raw/native styling.
- Loading states should use the shared, centrally positioned loading surface and
  remain responsive. Do not add a one-off spinner or leave a screen without a
  visible loading state when its data is asynchronous.
- Dashboard cards and other repeated panels must use the shared card/surface
  patterns in the design system in both light and dark themes.
- When changing UI/layout/theme code, inspect both light and dark states and
  update visual baselines when the repository's visual tests require it.
- Use `dd-MM-yyyy` for displayed dates. Send API dates in the format expected by
  the existing typed client and converter rather than formatting ad hoc in a
  template.

### Flutter mobile

- Use Riverpod providers, go_router navigation, and `AppConfig` for API
  configuration. Never hardcode an API base URL.
- Consume `Theme.of(context)`, `AppTheme`, or the shared color/widget layer.
  Avoid ad-hoc `Color(0x...)`, surface, border, or spacing values in screens.
- Reuse shared widgets such as `GlassContainer`, `LogoSpinner`,
  `SkeletonLoader`, `AppSearchField`, `EmptyStateWidget`, and confirmation
  dialogs. Keep loading and empty/error states consistent across screens.
- Format displayed dates with `DateFormat('dd-MM-yyyy')` from `intl`.
- Remember the Flutter CI convention: golden assertions are excluded in CI, but
  the test pump still runs. A `ListTile`/`InkWell` inside a decorated surface
  needs a transparent `Material` ancestor; use the shared glass container when
  appropriate.

## Comment, doc, and TODO tone

Comments, doc-comments, markdown docs, and `docs/TODO.md` entries must read like
a person wrote them: plain short sentences, no filler openers ("This function is
responsible for..."), no restating what the code already says, no AI-tell
phrasing ("updated per review"). A comment earns its place by explaining a
gotcha, a non-obvious reason, or a caller assumption the code doesn't already
convey. When a file is touched for any reason, clean up AI-sounding
comments/docs you pass over in it, not just the lines you're actively changing
— match the file's existing style when doing so.

## Documentation book (docs/book/)

`docs/book/` holds the project dissertation and is held to a stricter standard
than the rest of `docs/`. Read `docs/book/README.md` before editing a chapter.
After any change there, run `python docs/book/build/build.py --pdf --strict`;
it must end `status : clean, ready to deliver`. After adding, removing, or
moving a figure or table, run `python docs/book/build/renumber.py --apply`.
`docs/DOCUMENTATION_BOOK_OUTLINE.md` and the chapter files are one structure
seen twice — a section change in one needs the same change in the other in the
same edit. Renumbering `docs/TODO.md` items also requires refreshing
`docs/book/build/wbs.py`'s component-to-area map (`wbs.py --check`) and the
tracker figures quoted in the outline's Chapter 11 block.

## Change workflow

1. Read the relevant README/docs and inspect the graph with `graphify query`,
   `graphify explain`, or `graphify path` before broad source searches.
2. Check `docs/TODO.md` and `PROJECT_MAP.md` when present before starting
   cross-layer work.
3. Treat every change as a cross-layer change until proven otherwise. Trace
   retrieval and CRUD changes through persistence, application services and
   DTOs, API controllers, Angular services/components, Flutter services/screens,
   and the relevant tests. For API or business-rule changes, verify both client
   implementations even when only one currently appears to consume the route.
4. Update or add the relevant backend, web, mobile, integration, contract, or
   visual tests for the behavior being changed. Run the smallest relevant
   command from the table, then the related client checks. Do not claim a check
   passed unless it was actually run.
5. Update directly affected README/docs, `docs/TODO.md`, `PROJECT_MAP.md`, API
   contract snapshots, or other maintained project documentation in the same
   change when their information becomes stale. Do not leave documentation
   synchronization for a later task.
6. For multiple related tasks in one session, complete the cross-layer review,
   test updates, documentation updates, and graph refresh after the final task
   rather than repeatedly rewriting shared context between subtasks.
7. Keep the working context efficient during long sessions: compact or
   summarize completed investigation and preserve only actionable decisions,
   affected files, unresolved risks, and verification results.
8. For theme or UI work, fix the shared token/control layer first, then inspect
   affected screens for search, loading, pagination, responsive behavior, and
   light/dark parity in every relevant client.
9. Keep user-owned git/deploy operations untouched: do not commit, push, merge,
   promote branches, or trigger deployment hooks.
10. After modifying code or project documentation, run `graphify update .` so the
    repository knowledge graph stays current.
