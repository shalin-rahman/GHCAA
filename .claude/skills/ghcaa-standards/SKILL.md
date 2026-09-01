---
name: ghcaa-standards
description: Enforces .NET, Angular, and Flutter standards specifically for the GHCAA application.
---
# GHCAA Project Standards

## Tech Stack
- Backend: .NET 8, SQL Server, SQLite
- Frontend Web: Angular 19
- Mobile: Flutter

## Critical Rules
- **Date Format:** The canonical date format is ALWAYS `dd-MM-yyyy`.
  - Angular: Use `DatePipe`.
  - Flutter: Use `DateFormat`.
  - Backend API: Use `DateFormatConverter.cs`.
- **Architecture:** Enforce Clean Architecture and SOLID principles. Always anticipate scale, security, and performance.
- **Code Quality:** Use compile-safe typed code over dynamic/`any` types.
- **Style:** Follow the existing indentation and naming patterns in the repository exactly. Preserve all existing comments and docstrings.
- **Comment/doc/TODO tone:** human-written, plain, no AI filler phrasing — see the "Comment, Doc & TODO Tone" section in repo-root `CLAUDE.md` for the full rule.
