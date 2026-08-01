# GHCAA UI/UX Remediation Plan

Scope: the ~30 issues raised for the Admin panel, Member portal, and public landing.
Guiding rule (from `ghcaa-design`): **fix centrally in `GHCAA.Web/src/styles.scss` or a shared
component — never per page.** Every issue below has been traced to a file; issues that repeat
across panels are deliberately collapsed into one central task so admin and portal both inherit
the fix.

Legend: `[C]` central/shared-layer change · `[P]` per-page change · `[B]` backend change

---

## Root causes found during triage

These explain most of the reported symptoms and drive the phase ordering.

1. **`.icon-btn` is not a design-system class.** It is defined 5 separate times in component
   SCSS (`admin/gallery`, `admin/governance`, `admin/members`, `admin/news`, `admin/polls`) and
   **not at all** in `styles.scss`. It is *used* in 7 templates — including
   `admin/comm/admin-comm.html` and `member/messages/messages.html`, which have **no definition
   at all**, so those buttons render browser-native (white background, invisible glyph). This is
   the reported "delete button not visible", "delete button shouldn't have white background".
2. **Duplicate competing blocks in `styles.scss`**: `.filter-bar` is defined at ~1288 *and*
   ~1955, `.status-badge` at ~255 *and* ~2032. The later block silently wins; any density fix
   applied to the wrong block does nothing. Must be de-duplicated before the density pass.
3. **Nav icons are emoji** (`nav.service.ts`), so they cannot be theme-coloured and are visually
   inconsistent with the Font Awesome glyphs used inside pages (`member/dashboard`). Needs one
   icon system.
4. **`<img>` tags have no fallback** anywhere (`admin/gallery/admin-gallery.html:76`,
   `admin/article-approval/article-approval.html:76`, gallery/news cards). Missing/broken images
   render as a torn-image box.
5. **`MapToSummary` in `GHCAA.Infrastructure/Services/NetworkingService.cs:287` never populates
   `PassingYear`, `Degree`, `Subject`, `Rank`, `ProfileCompletionPercentage`** even though
   `AcademicHistory` is eager-loaded. This is the cause of "EC batch information is missing".
6. **`Rank` and `ProfileCompletionPercentage` are never computed anywhere in the solution** (grep
   over `GHCAA.Application` + `GHCAA.Infrastructure` finds assignments only in migration seed
   data). The member-profile stat strip is therefore permanently `#---`, `0 Points`, `0%`.
7. ~~**Quill editors are wired by `document.getElementById`** (`admin-comm.ts:151`), so the
   broadcast editor initialises against a DOM node that the `@if` block has not rendered yet →
   "Message Body (Rich Text) — no control found".~~ **Fixed 2026-08-01** — see task 0.12 below and
   `docs/TODO.md` item 30.12; both call sites now use the shared `app-rich-text-editor` component.
8. **Messaging has no compose path.** `chat.service.ts` only exposes `MESSAGING.RECENT` and
   `MESSAGING.HISTORY/{id}`; `messages.html` only filters existing threads. Starting a first
   conversation is genuinely impossible from the UI.

---

## Phase 0 — Central foundations (must land first)

Nothing in later phases should touch a colour, border, height, or button by hand; it should
consume what Phase 0 adds.

### 0.1 `[C]` De-duplicate `styles.scss`
Merge the two `.filter-bar` blocks and the two `.status-badge` blocks into one authoritative
definition each. Diff the emitted `dist/**/styles-*.css` before/after to prove no visual delta.
**Gate for 0.4 and 0.6.**

### 0.2 `[C]` Action-control taxonomy (one look + one label per action type)
Add to `styles.scss`:
- `.btn-danger` (destructive, token-based, no white ground), `.btn-icon` (square icon-only
  button, 36px, transparent ground, hover tint), `.btn-icon.danger`, `.action-group`
  (`display:flex; gap:.5rem`) for table row actions.
- Then delete the 5 per-component `.icon-btn` definitions and rewrite all 7 usage sites to
  `.btn.btn-icon[.danger]`. Keep an `.icon-btn` alias mapped to `.btn-icon` for one release so
  nothing regresses if a usage was missed.
- Add `core/constants/actions.constants.ts` — canonical label + icon + variant per action type
  (`SAVE`, `UPDATE`, `EDIT`, `DELETE`, `NEW`, `CLOSE`, `CANCEL`, `VIEW`, `EXPORT`, `APPROVE`,
  `REJECT`). All templates use these strings so "Save" is always "Save", "Edit ↗" becomes "Edit",
  etc.

Fixes: gallery card delete invisible · comm delete white background · comm Edit/Delete gap ·
button/label inconsistency app-wide.

### 0.3 `[C]` Single icon system (`app-icon`)
New `common/icon/` component: inline monochrome SVG sprite keyed by semantic name, `fill:
currentColor`, sized by font-size. Replace emoji in `nav.service.ts` (both `ALL_NAV_ITEMS` and
`ADMIN_NAV_ITEMS`), both layout headers/footers, mobile bottom nav, and the Font Awesome `<i>`
tags in `member/dashboard`. Icons then inherit `--text-main` / `--accent-color` per theme
automatically. Remove the Font Awesome CDN `<link>` from `index.html` once no `fa-` class
remains (also removes one CSP/CDN dependency).

Fixes: "all icons should be relevant to navigation/action and similarly coloured based on theme".

### 0.4 `[C]` Vertical-density pass (title + search + tabs)
In the merged blocks: reduce `.page-header` padding/`h2` size, `.filter-bar` height,
`.search-wrap` control height, and `.tab-nav` padding; convert `.tab-nav` to a compact
segmented control. Add `.filter-bar { position: sticky; top: 0; }` under
`@media (min-width: 1024px)` with a `z-index` below the top bar so the search freezes on large
screens while scrolling.

Fixes: title/search eating view height (all pages) · comm tabs taking too much space · directory
and alumni-directory search compaction + sticky search.

### 0.5 `[C]` Compact empty states
Add `.empty-state.compact` (single line, ~64px, no giant icon) and apply it to profile/dashboard
sections that currently reserve full height when empty ("Association Governance History",
"Professional Experience", and the equivalent admin detail sections).

### 0.6 `[C]` Dark-theme control borders + checkbox rhythm
Raise `--border-color` / `--hairline` brightness under `body.dark-theme` so `input`/`select`/
`textarea` edges read clearly; re-verify light theme is not over-darkened. Add row spacing to
`.premium-checkbox-label` / `.checkbox-container` when stacked.

### 0.7 `[C]` Modal close affordance
`styles.scss` `.modal-box .close-btn` (~1916): grow to a 40×40 hit target using the `app-icon`
close glyph, with hover/focus-visible states. One change fixes every popup in both panels.

### 0.8 `[C]` Shared user menu (`app-user-menu`)
New shared component: member photo avatar (initial fallback), full name, role, theme toggle,
sign-out as a proper icon button. Wire into **both** `portal-layout.html` (replaces the ad-hoc
avatar + `🚪` button) and `admin-layout.html` (replaces the plain `header-username` +
`header-role-badge` + text "Sign Out"). Admin reuses `ProfileService.getProfile()` so an admin
who is also a member gets their photo.

Fixes: no theme switcher in admin · member logout icon looks wrong · admin top-right should match
member panel · show user image after login top-right in both panels.

### 0.9 `[C]` Image fallback directive — **Done 2026-08-01**
New `appImgFallback` directive + lightweight placeholder assets (album, article, event, news,
avatar). Behaviour: empty/`null` src or `error` event → token-coloured solid block with the
relevant `app-icon`, never a broken-image box. Apply to every `<img>` in the app (gallery cards,
album covers, article/submission covers, news, events, avatars, EC cards).

Fixes: submission-review cover · gallery album cover · "if anywhere image missing add placeholder
relevant image".

Shipped as `src/app/common/directives/img-fallback.directive.ts`; see `docs/TODO.md` item 30.9 for
the full implementation note (closed the last remaining gaps and verified via multiline grep that
every `<img>` in the app carries the directive).

### 0.10 `[C]` Loading state = LogoSpinner, everywhere — **Done 2026-08-01**
`app-logo-spinner` already exists and is used on 24 pages. Audit and fix the rest: replace ad-hoc
markup (e.g. `common/governance/governance.html:18` `<div class="loading">Loading committee
records...</div>`) and **add** a loading state where none exists — candidates found:
`admin/audit`, `admin/dashboard`, `admin/comm`, `admin/events`, `admin/org-config`,
`admin/payment-config`, `admin/roles`, `member/dashboard`, `member/messages`,
`member/assistant`, `common/events`, `common/news`, `common/governance`, `public/directory`.
Add a `.form-loading-overlay` wrapper class so form panels get one identical treatment.

All genuine gaps in this candidate list fixed with `<app-logo-spinner>` (`admin/audit`,
`admin/comm`, `admin/events`, `admin/org-config`, `admin/payment-config`, `admin/roles`,
`common/governance`, `common/news`). `admin/dashboard` and `member/dashboard` keep their existing
skeleton-card loaders as a deliberate, symmetric choice (both portals use it identically — not
ad-hoc). `member/messages` (no async load on entry) and `member/assistant` (per-message
`typing()` chat-bubble indicator, a different correct pattern) and `public/directory` (thin
wrapper delegating to `common/directory`, which already has its own spinner) confirmed as
non-gaps rather than left unaudited. No separate `.form-loading-overlay` wrapper class was
introduced — each fix reused the existing `<app-logo-spinner>` component directly, which kept the
change centralized without adding a new shared CSS class for a single-use wrapper. See
`docs/TODO.md` item 30.10 for the full page-by-page note.

### 0.11 `[C]` Nav label == page title (single source of truth) — **Done 2026-08-01**
Both layouts already derive the breadcrumb from `NavService` labels. Add
`NavService.labelFor(url)` and make each page's `<app-page-header [title]>` consume it (or align
the hardcoded string to the nav label). Reconcile known mismatches, e.g. nav "Submission Review"
vs page "Review Submission", nav "Job Hub" vs page heading. Produce the full mismatch table as
the first step of this task.

Added `NavService.labelFor(url, scope)` and reconciled every mismatch found across both portals
(including an outright copy-paste bug where `member/dashboard`'s header read "Member Dashboard"
instead of "My Profile", and a page — `member/dashboard` itself — that had no title at all).
`member/messages` (chat UI) and the two `/polls` pages (not present in either nav list) are
confirmed out of scope rather than unresolved mismatches. See `docs/TODO.md` item 30.11 for the
full mismatch-to-fix table.

### 0.12 `[C]` Reusable rich-text editor (`app-rich-text-editor`) — **Done 2026-08-01**
Wrap Quill in a component that binds to its own `ElementRef` in `ngAfterViewInit` and supports
`ngModel`. Replace both `document.getElementById` call sites in `admin-comm.ts`
(`template-editor`, `broadcast-editor`). Show an explicit "editor failed to load" fallback
`<textarea>` if `window.Quill` is absent, so the field is never simply missing.

Fixes: Broadcast "Message Body (Rich Text) — no control found".

Shipped as `src/app/common/rich-text-editor/`; see `docs/TODO.md` item 30.12 for the full
implementation note.

---

## Phase 1 — Layout & navigation

### 1.1 `[C]` Group the portal sidebar like admin
Add `section` to `ALL_NAV_ITEMS` and a `portalNavSections()` computed in `nav.service.ts`
(proposed groups: **Overview** — Dashboard · **Community** — News, Events, Discussions,
Messaging, Gallery · **Directory** — Alumni Directory, Governance · **Career** — Job Hub, My
Articles · **My Account** — Profile, Digital ID, Payments). Render with the same
`.nav-section` / `.nav-section-label` markup admin uses. Feature-flag filtering must be
preserved.

### 1.2 `[P]` Bottom-align the admin-switch link
`portal-layout.html:22` `.nav-spacer` must actually push: `margin-top:auto` on the admin-link
wrapper (or `flex:1` on the spacer with the sidebar as a full-height flex column). Verify in
collapsed sidebar and mobile drawer states.

### 1.3 `[P]` Admin header adopts `app-user-menu` (from 0.8), incl. theme toggle.

---

## Phase 2 — Per-page fixes (all consume Phase 0)

| # | Issue | File |
|---|---|---|
| 2.1 | `[P]` "Login to View →" wraps to two lines — `.btn-view` is a bespoke class; swap to `.btn.btn-sm.btn-outline` + `white-space:nowrap` | `public/landing/sections/jobs-preview/jobs-preview.html:34` |
| 2.2 | `[P]` Theme shows **LIVE** after its window ended — badge reads only `theme.isEnabled`. Compute status from `isEnabled` + `startDate`/`endDate` vs today → `SCHEDULED` / `LIVE` / `EXPIRED` / `IDLE`, styled via the merged `.status-badge` | `admin/themes/admin-themes.html:62`, `admin-themes.ts` |
| 2.3 | `[P]` "Reg. Ends: Closed" → when closed render **"Registration Closed"**; when open keep "Reg. Ends: {date}". Matches the wording already used in `events-preview.html:50` | `common/events/events.html:80` |
| 2.4 | `[P]` Submission-review cover placeholder | `admin/article-approval/article-approval.html:76` |
| 2.5 | `[P]` Album cover placeholder + delete button visibility + `.action-group` gap | `admin/gallery/admin-gallery.html:76,119` |
| 2.6 | `[P]` Communications: template grid Edit/Delete gap, delete styling, **logs filter uses the shared `app-search-bar`** instead of the bespoke `.search-wrap sm`, compact tabs | `admin/comm/admin-comm.html:167-176, 193-194, 219-228, 26-31` |
| 2.7 | `[P]` EC cards: render member photo via `appImgFallback` (`PhotoPath` is already on the DTO) instead of an initial-only `.avatar-large`; batch line becomes meaningful once 3.1 lands | `common/governance/governance.html:24,34` |
| 2.8 | `[P]` Member profile identity line `GHC-…/Name/Associate/A-` mixes fonts on one line — normalise to one family/weight scale via tokens | `member/profile/profile.html` header block |
| 2.9 | `[P]` Profile/dashboard empty sections adopt `.empty-state.compact` (0.5) | `member/profile/profile.html`, `member/dashboard/dashboard.html` |
| 2.10 | `[P]` Dashboard checklist ("Identity & Photo / GHC History / Professional Info / Registration Payment") is inert — make each step a link to the matching profile section anchor, and label the block as *Profile completion* so its purpose is clear | `member/dashboard/dashboard.html:30-47` |
| 2.11 | `[P]` Directory + Alumni Directory: compact search panel, sticky on ≥1024px (inherits 0.4) — verify both routes | `common/directory/directory.html`, `.scss` |
| 2.12 | `[P]` **New Message flow**: "New Message" action opens a member picker (reuse the directory search endpoint), selecting a member opens an empty thread that can send the first message. Requires confirming/adding a send endpoint — see 3.3 | `member/messages/*`, `core/services/chat.service.ts` |

---

## Phase 3 — Backend

### 3.1 `[B]` Populate the flattened academic fields in `MapToSummary`
`GHCAA.Infrastructure/Services/NetworkingService.cs:287` — set `PassingYear`, `Degree`,
`Subject` from the eager-loaded `AcademicHistory` (GHC record first, else highest degree —
mirror the selection logic `member/profile` uses client-side so admin and portal agree).
Fixes EC batch info; also improves the directory. Add a unit test.

### 3.2 `[B]` Decide the fate of Rank / Points / Profile Health
Currently dead (never computed). **Recommendation:** compute
`ProfileCompletionPercentage` server-side from the same criteria the dashboard checklist uses
(identity+photo, academic history, professional history, payment) so the two never disagree; and
**remove the Global Rank tile** rather than ship a fake `#---`, until a contribution-points
source of truth exists. Needs your confirmation before implementation.

### 3.3 `[B]` Messaging: verify/add "send to member with no prior thread"
Confirm the messaging controller accepts a first message to an arbitrary member id (with the
usual authorisation checks); add it if missing. Blocks 2.12.

### 3.4 `[B]` Special-theme status (optional)
If the API's active-theme resolution already honours the date window, 2.2 is display-only. If it
does not, fix it server-side too so the public banner cannot outlive its window.

---

## Phase 4 — Cross-platform parity

Any token added or changed in Phase 0 (border brightness, danger colour, icon colour) must be
mirrored in `GHCAA.Mobile/lib/core/theme/app_theme.dart`, and the action-label constants mirrored
in the Flutter equivalent, so web and mobile do not drift. No Flutter golden regeneration unless
a shared widget's pixels change (see `ghcaa-preflight` for the CI golden rules).

---

## Regression safety (the "no new issues" requirement)

1. **Inventory before edit.** Every class or token being changed or removed gets a full
   `grep -rn` usage list recorded in the task, and every usage site is visited. This is
   mandatory for the `.icon-btn` removal (7 templates) and the `styles.scss` de-duplication.
2. **Alias, then remove.** Renamed classes keep a one-release alias so a missed usage degrades
   to "styled slightly differently", never to "browser-native".
3. **Phase 0.1 before any density edit** — otherwise a fix lands on the losing duplicate block
   and appears to do nothing.
4. **Both themes, every change.** `npm run type-check` + `npm run build` in `GHCAA.Web`, grep the
   emitted `dist/**/styles-*.css` to confirm new classes/tokens shipped, then Playwright
   (`webapp-testing`) screenshots in light **and** dark for a fixed page set: admin dashboard,
   admin gallery, admin comm (all 3 tabs), admin themes, portal dashboard, portal profile,
   portal directory, portal messages, portal events, public landing.
5. **Never `inlineCritical: true`.** Keep it `false` in both `angular.json` configs — see
   `gotcha_inlinecritical_csp.md`; re-enabling it reproduces the "shell styled, controls raw" bug.
6. **Tests.** `npm run test` (vitest) for web, `dotnet test` for the 3.x backend changes.
7. **Live-site verification is deploy-gated.** A change that looks absent on the deployed site is
   a stale deploy until proven otherwise — do not "re-fix" it in code.

---

## Open decisions needed from you

1. **3.2** — compute Profile Health server-side and drop the Global Rank tile, or keep Rank and
   implement contribution points as well?
2. **1.1** — accept the proposed portal nav grouping, or supply your own grouping/order?
3. **0.3** — replacing emoji with an SVG icon set changes the look of every nav item and action.
   Confirm before it lands, since it is the most visually far-reaching change here.
