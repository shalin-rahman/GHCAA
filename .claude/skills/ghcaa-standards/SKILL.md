---
name: ghcaa-standards
description: Shared technical rules for GHCAA.
---

# GHCAA Standards

- Stack: ASP.NET Core, Angular 21, Flutter; Postgres prod, SQLite local/test.
- No magic strings: check existing constants first.
- Avoid duplication; apply SOLID; reuse existing patterns.
- Changed service logic needs matching test changes, same commit.
- Dates: display `dd-MM-yyyy`, wire ISO, shared helpers only.
- UI: shared tokens/components only, no per-screen theming.
