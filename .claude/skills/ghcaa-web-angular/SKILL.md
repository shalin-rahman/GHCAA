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
- **Format**: Always use `dd-mm-yyyy` for date inputs.
- **Validation**: Implement custom validators for the standard date format.
