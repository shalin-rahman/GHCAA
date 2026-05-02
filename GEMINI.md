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

### Testing
- E2E tests use temporary SQLite DBs — never touch production data.
- Visual regression baseline must be regenerated after any UI change.
- Run `flutter test` before committing mobile changes.

## Key Files
| File | Purpose |
|---|---|
| `SRS.md` | Software Requirements Specification |
| `TODO.md` | Active task list (authoritative) |
| `project_map.md` | Full codebase map |
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
4. Update `TODO.md` and `project_map.md` when significant changes are made.
