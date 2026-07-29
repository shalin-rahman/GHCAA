---
name: ghcaa-design
description: "GHCAA design-system reference for any UI/design/layout/theme/styling work on the Angular web app (and the Flutter mobile theme). Invoke before adding or changing pages, forms, buttons, spacing, colors, or light/dark theming so every change uses the central token/class layer instead of ad-hoc or native styles. Covers the styles.scss token map, form/button/layout classes, the central-fix rule, the raw-native-control trap, and the light+dark visual-QA workflow."
---

# GHCAA Design System

All web styling lives in ONE global stylesheet: `GHCAA.Web/src/styles.scss` (~2170 lines).
Angular uses emulated ViewEncapsulation, so component-scoped styles are inlined into JS chunks
by esbuild — **global tokens + utilities live only in `styles.scss`**. Never install Tailwind;
utilities here are hand-rolled. See `ghcaa-preflight` for hard constraints.

## First rule: change design CENTRALLY

Fix in the shared layer, never per-component:
- Web → `styles.scss` (`:root` tokens, `body.dark-theme` overrides, `.btn*`, `.form-group`, utils).
- Flutter → `lib/core/theme/app_theme.dart` + shared widgets (`glass_container.dart`, `glass_tile.dart`).
A one-off component tweak that a token or shared class could carry is a bug, not a fix.

## Tokens (CSS custom props in `:root`, overridden under `body.dark-theme`)

Colors: `--bg-color` `--surface-color` `--surface-subtle` `--section-bg` `--card-bg`
`--card-bg-hover` `--card-border` `--glass-bg` `--glass-border` `--hairline` `--border-color`.
Text: `--text-main` `--text-strong` `--text-muted` `--text-dim` `--text-on-dark`.
Brand/state: `--primary-color` `--accent-color` (+ `-rgb`) `--accent-gold-bright`
`--accent-gold-dark` `--gold-gradient` `--dark-gradient` `--success-color` `--danger-color`.
Radius: `--radius-sm` `--radius-md` `--radius-lg`. Shadow: `--shadow-sm/md/lg` `--shadow-gold`.
Member tiers: `--tier-founding` `--tier-executive` `--tier-advisory` `--tier-associate`
`--tier-general` `--tier-general`.
**Always reference a token** for color/background/border/radius/shadow — never a hex literal.
A new color that no token covers → add the token (and its `body.dark-theme` value) first.

## Component classes (use these, don't hand-roll)

- **Forms:** wrap every control in `.form-group` (or `.input-group`) — this is what styles
  `input/select/textarea`. Multi-column: `.form-grid`. Read-only value: `.value-display`.
  A bare `<input>` with NO `.form-group` ancestor renders BROWSER-NATIVE (the exact "raw
  control" bug from the screenshots) — always wrap it.
- **Buttons:** `.btn` + variant `.btn-accent` (gold primary CTA) / `.btn-primary` /
  `.btn-secondary` / `.btn-outline`; size `.btn-sm` (+ `btn-lg`/`btn-xs` utils). A disabled
  action = keep `.btn` + variant and add `disabled`; don't drop the class (a class-less
  disabled button looks like raw gray text — another screenshot symptom).
- **Cards/surfaces:** `.glass-card` (+ `.card-bg`/tokens). Layout utils are hand-rolled
  (`flex`, `grid`, `gap-*`, `w-full`, `text-*`, `mt/mb/px/py-*`, `rounded-*`, `opacity-*`,
  `shadow-gold`). If a utility you need is missing, add it to the util layer in `styles.scss`
  once — don't inline a `style=""`.

## Traps (from real sessions)

- **Live "raw controls / gray buttons" ≠ code bug.** If the shell is styled but form controls
  and buttons look native on the LIVE site, and a fresh `npm run build` includes `.form-group`
  and `.btn-accent` in the emitted CSS, it's a **stale `preprod` deploy** of an unpromoted
  branch, not a regression. CSS reaches live only via deploy; a deploy carries the already-
  correct CSS. Do NOT rewrite templates to "fix" it. See `session_reusable_controls_refactor.md`.
- **Light-theme invisible text** = a shared class colored only under one parent scope; the other
  theme inherits the dark-first white. Fix the token/scope centrally. See
  `gotcha_lighttheme_scoped_color.md`.
- **Theme switcher is portal-only**, not admin. Don't assume admin pages toggle themes.

## Verify before declaring done

- `npm run type-check` + `npm run build` in `GHCAA.Web` (build must emit `styles-*.css`
  containing your new class/token — grep the emitted file to confirm).
- Visual QA in BOTH themes: use the **`webapp-testing`** skill (Playwright) to serve the app
  and screenshot light + dark. Generic technique lives in the user `frontend-design` skill;
  this file is the project-specific source of truth for tokens/classes.
- Read `.claude/memory/MEMORY.md` first (indexes the gotchas above).
