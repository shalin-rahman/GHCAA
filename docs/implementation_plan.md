# Implementation Plan - Centralized Theme & Common Control Standardization

Resolve dark/black theme color mismatches, eliminate inverted color-token dependencies (such as `rgba(var(--primary-rgb), ...)` on dark surfaces), and enforce unified consumption of centralized theme design tokens and reusable UI controls across all Angular components.

## User Review Required

> [!IMPORTANT]
> This plan performs a comprehensive scan across all Angular web SCSS and HTML templates to ensure 100% adherence to centralized design tokens in `styles.scss` (Obsidian & Gold palette), guaranteeing seamless contrast and visual unity across light, dark, and black themes.

## Proposed Changes

### Work Package 82 & Bug TODO Additions

#### [MODIFY] [TODO.md](file:///c:/Users/HabiburRahmanShalin/workstation/shaleen/shalin/GHC/Application/GHCAA/docs/TODO.md)
- Add **82.73** (`Audit and standardize all UI controls & SCSS to centralized theme tokens; eliminate dark-on-dark inversions`)
- Add **82.74** (`Enforce centralized reusable component usage across all common controls, dialogs, headers, and pagination`)

---

### Phase 1: Core Design System & Theme Variable Harmonization

#### [MODIFY] [styles.scss](file:///c:/Users/HabiburRahmanShalin/workstation/shaleen/shalin/GHC/Application/GHCAA/GHCAA.Web/src/styles.scss)
- Fix dark-theme token definitions to ensure elevated surfaces (`--surface-subtle`, `--card-bg`, `--card-border`, `--glass-border`, `--card-bg-hover`) have optimal contrast in `body.dark-theme` (pitch black `#000000` / `#0c0c0c`).
- Eliminate inverted `rgba(var(--primary-rgb), ...)` patterns from global utility classes (`.border-primary/*`, `.text-primary/*`, `.stat-card`, `.data-table`).

---

### Phase 2: Component-Level Theme Remediation

#### [MODIFY] [admin-dashboard.scss](file:///c:/Users/HabiburRahmanShalin/workstation/shaleen/shalin/GHC/Application/GHCAA/GHCAA.Web/src/app/admin/dashboard/admin-dashboard.scss)
#### [MODIFY] [admin-dashboard.html](file:///c:/Users/HabiburRahmanShalin/workstation/shaleen/shalin/GHC/Application/GHCAA/GHCAA.Web/src/app/admin/dashboard/admin-dashboard.html)
- Replace all 6 fragmented stat card action buttons with a unified, elegant `.action-pill` design with consistent gold/glass styling and hover transitions.
- Standardize `.stat-icon` background containers using `var(--surface-subtle)` / `rgba(var(--accent-rgb), 0.08)` so icons have clear visibility and contrast on dark surfaces.
- Harmonize card borders across all 6 cards.
- Fix `.quick-actions-card`, `.news-feed-item`, and `.event-mini-item` backgrounds and borders to use semantic tokens.

#### [MODIFY] Component SCSS files across `admin/`, `member/`, `common/`, and `public/`
- Scan and replace all hardcoded color literals and `rgba(var(--primary-rgb), ...)` in:
  - `directory.scss`, `events.scss`, `gallery.scss`, `jobs.scss`, `magazine.scss`, `polls.scss`
  - `admin-themes.scss`, `admin-events.scss`, `job-approval.scss`, `article-approval.scss`, `gallery-approval.scss`, `admin-comm.scss`
  - `dashboard.scss` (Member portal), `messages.scss`, `payments.scss`, `profile.scss`

---

### Phase 3: Centralized Reusable Component Auditing & Standardizing

- Verify all views use centralized shared components:
  - `<app-page-header>` for all admin and member headers.
  - `<app-pagination>` and `paginateArray` for table pagination.
  - `<app-confirm-dialog>` via `ConfirmDialogService` for all destructive actions.
  - `.data-table` class hierarchy for all tabular layouts.
  - `.empty-state-compact` / `.empty-state-card` for empty states.

---

## Verification Plan

### Automated Tests
- Type checking: `npx tsc --noEmit -p tsconfig.app.json`
- Production build: `npm run build`
- Unit tests: `npm test -- --watch=false`
- Backend check: `dotnet test GHCAA.sln`

### Manual Verification
- Verify dark theme visual rendering on admin dashboard, member portal, public directory, and admin management tables.
