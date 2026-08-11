---
name: session-logo-spinner
description: Shared LogoSpinner loader components built for web (Angular) and mobile (Flutter) to replace ad hoc spinners app-wide
metadata: 
  node_type: memory
  type: project
  originSessionId: 80e0e324-13a0-4d9f-b036-6a6d47d35135
---

Built a reusable logo-spinner loading indicator matching the public home page's animated logo effect, centralized so all loading states use one component instead of ad hoc spinners:
- Web: `GHCAA.Web/src/app/common/logo-spinner/` (`LogoSpinnerComponent`, configurable size) — replaced ad hoc spinners across ~20+ pages.
- Mobile: `GHCAA.Mobile/lib/core/widgets/logo_spinner.dart` (`LogoSpinner({size, ripple, label})` + `LogoSpinner.small({size})` factory, no `color` param — always uses `AppTheme.royalGold`). All `CircularProgressIndicator` usages app-wide replaced (confirmed complete: `flutter analyze` clean, zero remaining references under `lib/`).

An untracked static SVG export (`GHCAA.Web/public/assets/logo-spinner.svg`, SMIL-animated) was considered as an alternative but rejected — using it via `<img>` loses CSS-drivable color/size control that `LogoSpinnerComponent` already provides. Recommended deleting it unless kept for reference.

**Why:** [[feedback_keep_lightweight]] — one shared, parameterized component beats maintaining a static export alongside it.
