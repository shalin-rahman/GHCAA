---
name: ghcaa-core-master
description: Use this skill for any high-level architectural decisions, cross-platform synchronization, or terminology alignment in the GHCAA project.
---

# GHCAA Core Master Standards

This skill encodes the master engineering standards for the Govt. Haraganga College Alumni Association (GHCAA) platform.

## Architecture & Layers
- **Backend**: ASP.NET Core 9 following Clean Architecture.
  - `GHCAA.Domain`: POCO Models, Enums, Constants. No dependencies.
  - `GHCAA.Application`: Interfaces, DTOs, Mapping logic.
  - `GHCAA.Infrastructure`: EF Core, External Services (Email, SMS, Payment).
  - `GHCAA.API`: Controllers, Hubs, Middleware.
- **Frontend Web**: Angular 21 with Standalone Components and signals.
- **Frontend Mobile**: Flutter 3.x with Riverpod for state management.
- **Database**: PostgreSQL (production) / SQLite (local and test).

## Terminology & Business Rules
- **Membership Numbers**: Use format `GHC-YYYY-XXXX` (e.g., GHC-2026-0001).
- **Dates**: Display and input use `dd-MM-yyyy`; API date-only values use ISO `yyyy-MM-dd`.
- **Soft Delete**: Preserve each entity's established archive flag and global query filter. GHCAA uses `IsArchived`; do not introduce `IsDeleted` without a repository-wide migration plan.
- **Privacy**: User privacy toggles (`IsMobilePublic`, `IsEmailPublic`, etc.) must be respected in DTO mapping.

## Cross-Platform Parity (Web & Mobile)
- Any change to an API endpoint MUST be verified in both `GHCAA.Web` and `GHCAA.Mobile`.
- UI labels and terminology must match the `PROJECT_MAP.md` and `SRS.md` definitions.

## Coding Conventions
- **C#**: PascalCase for members, camelCase for parameters/locals. Use Primary Constructors where applicable.
- **Dart**: standard `effective_dart` rules. Use `AppUtils` for date/string operations.
- **Angular**: Standard Angular style guide. Use `AppConstants` for global strings.

## Decision Tree
- **Adding a new Model?** Add to `Domain`, update `Application` DTOs, then implement in `Infrastructure`.
- **Changing a UI field?** Check `PROJECT_MAP.md` first to see which other layers are affected.
- **Encountering a date?** Use `ghcaa-date-standard`: display and input use `dd-MM-yyyy`, while API requests and responses use ISO-8601.
