---
name: ghcaa-mobile-flutter
description: Standards for Flutter mobile development within the GHCAA project.
---

# GHCAA Mobile Engineering Standards

## 1. State Management
- **Riverpod**: Use `StateNotifierProvider` or `NotifierProvider` for logic.
- **Provider Parity**: Ensure `roleProvider` and `userProfileProvider` are used to manage session state securely.

## 2. UI & Layout
- **Design System**: Follow the 8pt grid system.
- **Components**: Use `AppScaffold` and shared widgets from `lib/widgets`.
- **Loading**: Always implement skeleton/shimmer states for async data.

## 3. Data Flow
- Use `AppUtils` for date formatting (`dd-mm-yyyy`).
- Synchronize models with `GHCAA.Domain` definitions.
