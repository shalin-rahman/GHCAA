---
name: ghcaa-standards
description: Enforces .NET, Angular, and Flutter standards specifically for the GHCAA application.
---
# GHCAA Project Standards

## Tech Stack
- Backend: ASP.NET Core 9, PostgreSQL in production, SQLite locally and in tests
- Frontend Web: Angular 21
- Mobile: Flutter with Riverpod, go_router, and Dio

## Critical Rules
- **Dates:** Display and input use `dd-MM-yyyy`; API date-only values use ISO `yyyy-MM-dd`, and timestamps use ISO-8601.
  - Angular: Use `DatePipe` or the shared date utilities for display, and `toWireDate` for requests.
  - Flutter: Use `AppUtils.formatDate` for display and `AppUtils.toWire` for requests.
  - Backend API: Use the global `DateFormatConverter.cs`; do not add per-property date serialization.
- **Architecture:** Keep Domain framework-independent, define contracts in Application, implement persistence and integrations in Infrastructure, and keep API controllers thin.
- **Code Quality:** Use compile-safe typed code over dynamic/`any` types.
- **Style:** Follow the existing indentation and naming patterns in the repository exactly. When a touched comment or docstring is changed, keep it plain and specific.
- **Comment/doc/TODO tone:** human-written, plain, no AI filler phrasing — see the "Comment, Doc & TODO Tone" section in repo-root `CLAUDE.md` for the full rule.
