# Implementation Plan - Centralized Theme and Shared Control Completion

## Scope

Complete the remaining work after commit `035e890` and its follow-up
`d535240c`. The goal is not to introduce another visual theme or to redesign
individual screens. The goal is to make the existing light and dark themes
the only source of visual truth for the public, member, and admin portals.

Every affected control must use global theme tokens and shared Angular
components where they already exist. A component may keep layout rules that
are unique to its content, but it must not define its own theme colours,
surface system, control states, or duplicated interaction pattern.

The reusable-control scope also includes breadcrumbs, modal shells and
headers, empty states, form fields and validation, buttons and row actions,
badges and status indicators, tabs, filters, theme toggles, user menus, icons,
notifications and toast feedback, rich-text editors, payment-method
selectors, and responsive layout shells. The audit must separate a genuine
shared interaction from a one-off content layout.

This is an application-wide plan. Angular Web is the primary implementation
surface, but each work package must check the corresponding API contract,
Flutter control, test coverage, and architectural documentation whenever the
same behavior exists there. Do not change an unaffected layer merely to make
the plan symmetrical.

The existing GHC logo-based loader is preserved. A shared loading panel may
wrap `LogoSpinnerComponent` to provide positioning, accessibility, sizing, and
theme-backed surfaces, but it must not replace the logo, its fallback, its
animation, or its branding inputs.

## Findings reconciled with existing work

The following work is already complete and must not be reopened:

- `82.71` owns shared table pagination state.
- `82.72` owns the first theme-token remediation pass, including the
  post-`035e890` dashboard fixes.
- `82.44` owns search debounce deduplication.
- `82.45` and `82.46` own confirmation dialogs and modal-header reuse.
- `82.47` owns page-header reuse.
- `82.42` owns lookup-data centralization and must be considered when
  standardizing dropdowns.
- Existing upload validation and staged-upload work remains the behavior
  baseline. This plan standardizes its controls rather than changing upload
  rules.
- The implementation slice added `LoadingPanelComponent` and migrated
  route-level panels across admin, member, public, and common areas, including
  audit/error logs, dashboards, review/configuration screens, members, themes,
  ledger, digital ID, elections, directory, and gallery, while preserving the
  existing logo spinner. Inline actions and upload progress indicators remain
  direct spinner usages. The work remains subject to the full migration and
  verification phases below.

The remaining gaps are broader than the original 82.73/82.74 wording:

- Some admin tables and grids still need a search decision and consistent
  search presentation.
- Loading is not represented by one centered, responsive panel pattern on
  every route.
- Dashboard, statistic, balance, content, and list cards still have several
  local variants.
- File/photo uploaders, date inputs, date-range controls, and native selects
  need a cross-portal control audit.
- Other repeated controls and component shells still need a reuse decision,
  including breadcrumbs, modal shells, empty states, filters, form fields,
  buttons, badges, tabs, feedback, and responsive layout wrappers.
- The existing plan did not define a route-by-route light/dark and responsive
  verification pass.

## Work packages

### Phase 1 - Complete the token audit

`82.73` remains the entry point. Inspect every Angular SCSS and template in
`admin/`, `member/`, `public/`, and `common/`. Replace remaining hardcoded
theme colours, inverted primary-RGB surfaces, and local control-state colours
with the tokens in `GHCAA.Web/src/styles.scss`.

Do not add a new theme. Do not create component-local theme variables.
Retain a local colour only when it is content data or a documented print-only
paper token.

### Phase 2 - Complete shared control enforcement

`82.74` remains dependent on 82.73. Audit every repeated control and component
surface across the three portals and common Angular layer: page headers,
breadcrumbs, search bars, tables, pagination, confirmation dialogs, modal
shells and headers, empty states, form fields, validation messages, buttons,
row actions, badges, status indicators, tabs, filters, theme toggles, user
menus, icons, notifications, toast feedback, rich-text editors,
payment-method selectors, and responsive layout shells.

Reuse existing shared components and classes before creating anything new. A
new component is justified only when the same interaction, visual states, and
state handling are repeated across routes. Record deliberate exceptions for
content-specific layouts or controls whose behaviour cannot be shared safely.

For each candidate, check the applicable surfaces:

| Surface | Required review |
| --- | --- |
| Angular Web | Shared component, token classes, route usage, keyboard and responsive states |
| Flutter Mobile | Existing equivalent widget, `AppTheme`/`AppColors`, loading and form parity |
| API | DTO, validation, upload/date contract, and error behavior only when the control crosses the API boundary |
| Tests | Existing Angular, E2E, Flutter, and API coverage before adding or changing assertions |
| Architecture | `PROJECT_MAP.md` and related application docs only when a shared component or contract changes |

This table is an applicability check, not a requirement to duplicate a Web
component in Flutter or to add an API abstraction for a visual-only change.

### Phase 3 - Admin table and grid search

`82.75` depends on 82.74. Inventory every admin table/grid and record whether
it needs search based on its data volume and user task. For tables that need
search, use the shared search bar and the 82.71 pagination state. Standardize
search placement, debounce, loading, empty state, sorting, responsive
overflow, and row actions. Tables that do not need search must record the
reason in the tracker or implementation review rather than receiving a
gratuitous control. The initial inventory confirms that most operational
tables already use `SearchBarComponent`; small fixed configuration tables,
including fee and payment settings, are intentional no-search candidates.
Polls, event operations, and any remaining data-heavy grids still require an
explicit decision before this phase can close.

Every color remediation in this plan must map a declaration to its semantic
role first. Use the existing theme token or its RGB companion for success,
danger, warning, accent, surface, border, and text states. Do not replace a
literal with a token merely because the value looks similar. A replacement is
valid only when it preserves contrast and intent in both existing themes and
continues to respond correctly if either theme changes. Fixed colors are
allowed only for documented paper/print surfaces, image overlays, logos, or
other content that is intentionally independent of the site theme.

### Phase 4 - Reusable control and component extraction

`82.74` is the audit gate for this phase. For each repeated control identified
in the shared-control inventory, either reuse an existing component, move its
visual states into shared token-backed classes, or extract a small standalone
component. Keep feature-specific data and business rules in the feature
component or service; shared controls must not become feature-aware.

### Phase 5 - Central loading panel

`82.76` depends on 82.74. Define one shared loading-panel pattern around the
existing logo spinner. It must support full-page, table/card, and local
content loading without layout jumps; center the indicator within the active
region; expose an accessible busy state; and remain usable at mobile
breakpoints in both themes. Migrate route-level variants to this pattern.
Keep the existing GHC logo spinner as the rendered indicator. Preserve
per-screen size, ripple, fallback logo, and label behavior unless a deliberate
responsive standard is verified. Check Flutter's existing logo and async
loading widgets for parity; do not replace them with a Web implementation.

### Phase 6 - Card and dashboard variants

`82.77` depends on 82.73. Identify the repeated statistic, balance, content,
quick-action, feed, and list-card patterns across admin, member, and public
routes. Keep semantic variants, but make their surfaces, borders, text,
badges, focus, hover, and disabled states consume shared tokens and shared
classes. Do not make every card identical when its information hierarchy
differs.

### Phase 7 - File and photo upload controls

`82.78` depends on 82.74 and existing upload behavior. Audit profile,
gallery, news, member-import, payment-proof, and other file inputs. Reuse
one themed upload surface for filename, preview, replace/remove, validation,
progress, disabled, and error states. Preserve current file validation,
staged-submit, compression, and API behavior. Verify keyboard and mobile
file selection.
If an uploader crosses the API boundary, verify the existing file DTO,
validation, compression, size/type limits, and error contract. Check the
corresponding Flutter uploader where the same workflow exists.

### Phase 8 - Date controls

`82.79` depends on 82.73 and `ghcaa-date-standard`. Audit every date and date
range control. Use the existing date utilities and the project contract:
users see and type `dd-MM-yyyy`; date-only API values use ISO wire format.
Standardize focus, invalid, disabled, range, and responsive states without
introducing native browser styling differences between themes.
Check Angular, API serialization, Flutter parsing, and E2E input data
together. Keep `dd-MM-yyyy` for display/input and ISO for date-only wire
values; a visual-control change must not alter that contract.

### Phase 9 - Dropdown and select controls

`82.80` depends on 82.73 and 82.42. Audit native selects and lookup-backed
dropdowns in all portals. Standardize their themed surface, arrow, focus,
disabled, validation, and option-empty states. Use `LookupService` or the
existing lookup provider for values; do not duplicate enum or lookup lists
while changing visual controls.
Where a lookup is served by the API, verify both Angular and Flutter lookup
consumers and keep labels aligned. Do not alter lookup values as part of a
visual-only control change.

### Phase 10 - Cross-portal verification and closure

`82.81` depends on 82.75 through 82.80. Run static audits, Angular type
checking, unit tests, production build, and focused browser checks for admin,
member, and public routes. Check light and dark themes at desktop and mobile
breakpoints. Verify no new local theme system, duplicated shared control, or
date-format regression was introduced. Update the tracker and project map
only with verified results. Verify that every repeated control has either a
shared implementation or a recorded reason to remain local.
Current verified baseline: Angular type-check passes, the full Vitest suite
passes with 81 files and 439 tests, affected loading-route tests pass, and
Graphify was refreshed after the loading migrations. Production build retry
and desktop/mobile light/dark browser checks remain open.
The closure review must report each applicable layer from the matrix above and
explicitly state when API, Flutter, tests, or architecture changes were not
needed.

## Dependency graph

```text
82.73 -> 82.74
82.74 -> 82.75 -> 82.81
82.74 -> 82.76 -> 82.81
82.73 -> 82.77 -> 82.81
82.74 -> 82.78 -> 82.81
82.73 -> 82.79 -> 82.81
82.73 + 82.42 -> 82.80 -> 82.81
```

## Verification commands

Use the project commands already recorded by the tracker:

```text
npm run type-check
npx vitest run
npx ng build
dotnet test GHCAA.sln
flutter test
```

The scope ends at application code, application tests, and directly related
project documentation. Do not inspect, modify, build, or validate
`docs/book/` content, its instruments, its outline, or its build tooling.

For UI work, add focused Angular specs or existing E2E/visual checks only
after checking that an existing test already covers the same setup and
assertion shape. Do not weaken visual baselines. Run browser checks in both
themes and at representative desktop and mobile widths. Do not inspect,
modify, or validate any `docs/book/` content as part of this plan.
