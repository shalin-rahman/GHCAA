---
name: ghcaa-web-angular
description: Standards for Angular web development within the GHCAA project.
---

# GHCAA Web Engineering Standards

## 1. Components
- **Standalone**: Prefer standalone components for all new features.
- **Organization**: Group by feature module (e.g., `member/profile`, `admin/approvals`).

## 2. Services & Data
- **Type Safety**: Define interfaces for all API responses in `src/app/core/models`.
- **Constants**: Store magic strings and configuration in `src/app/core/constants/app.constants.ts`.

## 3. Forms & Dates
- **Format**: Use `dd-MM-yyyy` for date inputs and display. Convert date-only values to ISO before sending them to the API.
- **Validation**: Reuse `DATE_REGEX`, `parseDisplayDate`, and the existing date utilities instead of parsing `dd-MM-yyyy` with the JavaScript `Date` constructor.
