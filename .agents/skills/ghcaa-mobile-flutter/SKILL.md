---
name: ghcaa-mobile-flutter
description: Flutter guidance for GHCAA state and theme.
---

# GHCAA Mobile Flutter

- Riverpod only; no BLoC.
- Reuse shared widgets and the central theme, no ad-hoc styling.
- `AppUtils.formatDate` for display, `AppUtils.toWire` for outgoing dates.
- Async screens need loading/empty/error states.
- List items on a decorated surface need a transparent `Material` ancestor.
