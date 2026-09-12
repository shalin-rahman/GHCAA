---
name: ghcaa-mobile-flutter
description: Standards for Flutter mobile development within the GHCAA project.
---

# GHCAA Mobile Engineering Standards

## 1. State Management
- **Riverpod**: Use the provider pattern already used by the feature. Do not introduce BLoC for new work.
- **Provider Parity**: Ensure `roleProvider` and `userProfileProvider` are used to manage session state securely.

## 2. UI & Layout
- **Design System**: Follow the 8pt grid system.
- **Components**: Use `AppScaffold` and shared widgets from `lib/core/widgets` and feature shared widgets.
- **Loading**: Always implement skeleton/shimmer states for async data.

## 3. Data Flow
- Use `AppUtils.formatDate` for display and `AppUtils.toWire` for API date-only values. Display and input use `dd-MM-yyyy`; the wire format is ISO `yyyy-MM-dd`.
- Synchronize models with `GHCAA.Domain` definitions.
