# GHCAA Project Context

## Project Overview
GHCAA (Grand Hall Community Association App) is a full-stack platform:
- **Backend**: ASP.NET Core 8 Web API (Clean Architecture — Domain / Application / Infrastructure / API layers)
- **Frontend**: Angular 19 (standalone components, signals, `inject()`)
- **Mobile**: Flutter 3.x (Dart) 
- **Database**: SQLite (development), SQL Server (production)
- **Testing**: E2E (Playwright), Visual regression, Unit tests (xUnit)

## Critical Conventions

### Date Format (MANDATORY — Platform-Wide)
- **Canonical format: `dd-MM-yyyy`** — enforced everywhere, no exceptions.
- Angular: use `DatePipe` with `'dd-MM-yyyy'` for display; send ISO strings to API.
- Flutter: use `DateFormat('dd-MM-yyyy')` from `intl` package.
- C# API: `DateFormatConverter.cs` handles serialization/deserialization.
- Never use `MM/dd/yyyy`, `yyyy-MM-dd` in UI display.

### Architecture
- Domain layer has no external dependencies.
- Application layer uses MediatR + FluentValidation.
- Infrastructure layer: `IEntityTypeConfiguration<T>` classes (not inline `OnModelCreating`).
- API controllers: thin — delegate all logic to Application layer handlers.
- Soft deletes are enforced via global query filters (`IsDeleted == false`).

### Angular Patterns
- Standalone components with `inject()` for DI — no constructor injection in new code.
- Use `signal()` and `computed()` for reactive state.
- Route via `app.routes.ts` — no NgModule-based routing.
- HTTP calls via typed services in `core/services/`.

### Flutter Patterns
- BLoC pattern for state management.
- Navigation via `go_router`.
- API base URL from `AppConfig` — never hardcoded.

### Centralized Theme & Reusable Component Rule (MANDATORY — Platform-Wide)
- **100% Centralized Theme Tokens:** Never hardcode colors, backgrounds, or borders in individual components.
- Angular: All surfaces, borders, text, and inputs MUST consume centralized design tokens from `styles.scss` (`var(--bg-color)`, `var(--surface-color)`, `var(--surface-subtle)`, `var(--card-bg)`, `var(--card-border)`, `var(--glass-bg)`, `var(--glass-border)`, `var(--accent-color)`, `var(--accent-rgb)`, `var(--text-main)`, `var(--text-muted)`).
- Never use `rgba(var(--primary-rgb), ...)` for surfaces on dark backgrounds (which inverts to muddy black); use semantic tokens (`var(--surface-subtle)`, `var(--card-bg-hover)`).
- Flutter Mobile: All widgets MUST consume `Theme.of(context)` / `AppColors` / `AppTheme` centralized tokens. No ad-hoc `Color(0x...)` or hardcoded component styling.
- **Centralized Reusable Controls:** Common controls across all routes must strictly use shared components:
  - Headers: `<app-page-header>`
  - Pagination: `<app-pagination>` & `table-pagination.util.ts`
  - Tables: Standard `.data-table` / `.table-wrap` structure
  - Modals / Dialogs: `<app-confirm-dialog>` via `ConfirmDialogService`
  - Search / Filters: `<app-search-bar>`, `.filter-bar`
  - Empty states: `.empty-state` / `.empty-state-compact`

### Testing
- E2E tests use temporary SQLite DBs — never touch production data.
- Visual regression baseline must be regenerated after any UI change.
- Run `flutter test` before committing mobile changes.

## Key Files
| File | Purpose |
|---|---|
| `SRS.md` | Software Requirements Specification |
| `TODO.md` | Active task list (authoritative) |
| `PROJECT_MAP.md` | Full codebase map |
| `README.md` | Developer setup guide |
| `GHCAA.Infrastructure/Data/` | EF Core configurations |
| `GHCAA.Web/src/app/` | Angular app root |
| `GHCAA.Mobile/lib/` | Flutter app root |

## Security
- Never commit `.env` or connection strings.
- `api_error.log`, `api_stdout.txt`, `build_errors.txt` are diagnostic — do not commit.
- Secrets flow via environment variables only.

## Agent Workflow
1. Check `TODO.md` for current priorities before starting any new task.
2. For multi-step tasks, create a `PLAN.md` at project root before writing code.
3. Run analysis tools (`dart analyze`, `dotnet build`) before declaring a task done.
4. Update `TODO.md` and `PROJECT_MAP.md` when significant changes are made.
