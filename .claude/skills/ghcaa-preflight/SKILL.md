---
name: ghcaa-preflight
description: "Pre-flight checklist and hard constraints for the GHCAA project (.NET 9 API + Angular 21 web + Flutter mobile). Invoke at the start of any GHCAA coding, review, deploy, or debugging task to avoid the mistakes that have repeatedly cost whole sessions. Covers deploy ownership, the no-payment-gateway-keys rule, central theme changes, the stale-deploy-vs-regression trap, Flutter golden/CI gotchas, and the glass-ListTile assertion."
---

# GHCAA Pre-Flight

Read this before touching GHCAA. It encodes hard constraints and the specific traps that
have burned time before. Details live in `.claude/memory/` — follow the `[[links]]`.

## Hard constraints (never violate)

1. **The USER owns all git + deploy.** Do NOT commit, push, promote branches, or trigger
   deploys. Leave changes in the working tree only. Current dev branch is `dev`; live
   auto-deploys from `preprod` (Render Docker: .NET 9 API + Angular served as ONE service,
   DB = Neon Postgres). See `reference_preprod_env.md`.
2. **No payment gateway API keys exist.** Every payment method must be admin-configurable and
   work manually WITHOUT live gateway keys (display-only wallet/bank fields). See
   `session_area29_shipblockers_payments.md`.
3. **Keep it lightweight.** No new deps/abstractions without concrete duplication to justify
   them. Do NOT install Tailwind — hand-roll utilities in `GHCAA.Web/src/styles.scss`. See
   `feedback_keep_lightweight.md`.
4. **Theme/design changes go CENTRAL.** Fix in the shared token/util/widget layer
   (`styles.scss`, shared components, Flutter `app_theme.dart` / `glass_container.dart`), not
   per-component.

## Traps that have cost whole sessions

- **Stale deploy ≠ code regression.** If the LIVE site looks broken (shell styled, component
  internals raw) but a fresh `npm run build` emits both global `styles.css` and the
  component-scoped rules in JS chunks (grep the chunks for the class + `_ngcontent`), it's a
  stale/mismatched Render deploy of an unpromoted branch — NOT a code bug. Do not "fix" it by
  editing templates. See `session_reusable_controls_refactor.md`.
- **Flutter goldens fail on Linux CI, pass on Windows.** Committed goldens are baked on
  Windows; CI skips the pixel compare via `test/flutter_test_config.dart` (`skipGoldenAssertion`
  when `CI`/`GITHUB_ACTIONS`). But the custom pump still RUNS, so exceptions during pump still
  fail. Both CI workflows use `flutter test --reporter expanded` to capture traces. Verify
  locally with `CI=true flutter test ...`. See `session_mobile_ci_golden_fix.md`.
- **ListTile inside a decorated box throws on CI** ("ink splashes may be invisible"). Any
  `ListTile`/`InkWell` under a `BoxDecoration` with a background needs a `Material` ancestor.
  The shared `glass_container.dart` now wraps its child in `Material(type: transparency)`, so
  `GlassContainer(child: ListTile(...))` is safe. If you add a raw decorated container around a
  ListTile, wrap it in transparent Material too.
- **Seed/*.json ≠ live SQLite DB.** Editing seed JSON does not update an already-created DB.
  Query the live DB when debugging data. See `gotcha_seed_json_vs_live_db.md`.
- **EF "pending model changes" (Postgres) is spurious seed churn** — do NOT scaffold/apply a
  migration for it. See `gotcha_pending_model_changes_seed.md`.
- **DateTime is serialized by a custom global converter** (ISO-8601 now) — don't assume default
  behavior. See `gotcha_date_format_converter.md`.

## Verify before declaring done

- Web: `npm run type-check` + `npm run build` (in `GHCAA.Web`).
- Mobile: `flutter analyze` + `CI=true flutter test` (in `GHCAA.Mobile`).
- API: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj`.
- Visual/runtime QA of Angular pages: use the **`webapp-testing`** skill (Playwright) to serve
  the app and screenshot light + dark — this is how to clear the "needs running app/browser"
  blockers instead of deferring them.
- Read `.claude/memory/MEMORY.md` first; it indexes everything above.
