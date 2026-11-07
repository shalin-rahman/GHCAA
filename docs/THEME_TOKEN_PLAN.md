# Theme token refactor: review and implementation plan

Design reference, written 2026-11-07. It says how to finish the uncommitted theme refactor on branch
`preprod`. It does not set order against other work; `docs/MASTER_EXECUTION_PLAN.md` does that, and
the owner decides where this goes in it.

## Rules for the implementing agent

- Do not commit, push, stage or revert. Preserve every unrelated staged and unstaged change.
- Work in the batches below, in order. After each batch, read your own diff (`git diff -- <files>`)
  before starting the next one.
- Keep current values and appearance unless a step says to change them. Moving a value into a theme
  file is not a reason to retune it.
- Hex and rgba literals belong in `src/styles/themes/*.scss`. `scripts/guard.py` flags a new hex
  literal in `GHCAA.Web/src/app/*.scss`; it does not look at `src/styles/`.
- Comments follow the repo `CLAUDE.md` tone rules: one plain line, only for a gotcha or a reason.
- Free space on drive C: is under 1 GB. Check it before the build and before `ci-local`.

## What the uncommitted change does

The token blocks moved out of `styles.scss` into `src/styles/themes/_light.scss` and `_dark.scss`,
pulled in through `src/styles/_theme-config.scss`. `core/constants/theme.config.ts` holds the theme
names, body classes, toggle labels and icons, the storage key and the default (`dark`).
`ThemeService`, the theme toggle and its spec read from it. The change also adds typography, spacing,
state and status tokens, retunes the dark palette, raises controls to 44px targets, enlarges badges
and labels, remaps some Tailwind-style utility classes to tokens, and adds a reduced-motion block.

## Review findings

### Must fix before handback

F1. Branding colors are lost in both modes. `OrgConfigService` (`org-config.service.ts:35-55`) writes
the configured accent onto `document.documentElement.style`. The new light block is declared on
`:root, body.light-theme` and the dark block on `body.dark-theme`, and both now declare
`--accent-color`, `--accent-color-rgb`, `--accent-rgb`, `--accent-gold-bright`, `--accent-gold-dark`,
`--accent-text`, `--shadow-gold`, `--gold-gradient` and `--tier-founding`. A declaration on `body`
beats the value inherited from `html`, so an institution with a non-gold accent sees gold
everywhere. Before the change the light tokens sat on `:root` only, and the dark block set
`--accent-text: var(--accent-color)` and did not declare the accent itself.

F2. Comments that carried a reason were deleted: paper tokens must never flip with the theme; why
`color-scheme` is set where it is; why `--accent-rgb` exists as an alias; why dark is the default
theme. Restore each as one plain line next to the value it explains.

F3. Typography, spacing and radius tokens are copied verbatim into both theme files. They do not
change per theme, so they belong once in the plain `:root` block in `styles.scss` (or a
`_base.scss` partial). Two copies will drift.

F4. No tests for `isTheme` and `getNextTheme` in `theme.config.ts`.

### Check, then decide

F5. Size changes are visible on every page: `--control-height` 2.5rem to 2.75rem, `.icon-btn` 36px to
44px, `.status-badge` 0.65rem to 0.875rem, `.badge` 10px to 0.75rem, form labels 0.72rem to
0.875rem, global `h1` to `h5` sizes, nav item min-height 44px. They meet target-size guidance but can
break dense tables, cards and the admin nav. Check them in the browser (Batch 6). If a page breaks,
report it to the owner instead of shrinking the token back on your own.

F6. Utility remaps change meaning on light pages. `.text-white/40`, `/60`, `/90` now map to
`--text-muted`; `.text-black` to `--text-on-accent`; `.border-white/10` to `--hairline`. Where one of
these sits on a photo or a dark overlay in light mode, text that was white turns dark gray. Current
uses: `.text-black` 5 templates (all on `bg-gold-gradient` or `bg-accent`, so the remap is right),
`.border-white/10` 5, `.border-white/20` 3, `.border-white/5` 11, `.text-white/40` 1
(`admin-roles.html:37`), `.shadow-2xl` 3. Look at each in light mode.

F7. Focus is drawn twice: a global `:focus-visible` outline with `!important` plus the existing
`box-shadow` focus rings on inputs and buttons. Keep one. Drop `!important` unless a named rule needs
beating.

F8. The dark card background changed from translucent `rgba(255,255,255,0.03)` to solid `#1b1f24`.
That removes the glass look over hero images. Owner call; note it in the handback.

F9. Literals left in layout SCSS: `rgba(197, 160, 89, 0.3)` at `admin-layout.scss:159` and `:300`,
radius `8px`, `font-weight: 950`. Batch 2 covers the colors.

F10. First paint is light, then dark, because `index.html` has no pre-paint theme script. This
predates the change. Leave it; list it as a follow-up.

F11. Flutter `AppColors` still holds the old dark palette. Mobile is not ported to web patterns, so
do not change it here; list the new dark values in the handback for the owner to decide.

## Review results, 2026-11-07

The security review found no vulnerabilities. The code review found two medium-severity contrast
issues. The web gradient check samples 0%, 25%, 50%, 75% and 100%, so it can report a pass when a
color between those points fails. For `#2a0138` over `#e731e9` to `#1ca3f0`, the lowest sampled ratio
is 4.502:1, while the ratio at 39% is 4.407:1. Keep the fix and regression test in 84.87.

The mobile offline accent `#2f6f4f` with `obsidianBlack` button text has 3.23:1 contrast. It is
outside this web theme plan and is tracked in 84.89. The review did not make code changes.

The review also found that checking token declarations alone does not establish what a component
renders. Theme selectors can override values written on `:root`, while fixed foreground and effect
tokens can stop following a configured accent. A utility remap can also be valid on one surface and
wrong on another. Keep these checks in 84.87; do not close the item from token-definition checks
alone.

Before closing 84.87, verify the rendered values in both app modes with the default gold branding and
with custom primary and accent colors. Check computed foreground, background, hover, focus and shadow
values on the components that use those tokens, including buttons with configured accents. For every
remapped white-text utility, inspect its actual template use and verify its foreground on the real
surface in both modes, including photo or overlay contexts where present. The test or browser evidence
must exercise the runtime branding override and the component output, not only assert that variables
exist in the theme files.

For special-day contrast, include inactive fields as well as invalid active colors: an unused
`sidebarColor` must not make a solid banner invalid, and an empty gradient endpoint must use the live
banner's background fallback. Check suggestions against the same gradient interpolation used by the
preview and live banner; assert that an applied suggestion passes and that an impossible suggestion
leaves the form value unchanged and reports that no suggestion is available.

## Color audit method

Audit actual declarations, not a raw regex count. An earlier 170-match scan mixed literals with
values that already use tokens; a narrower scan found 35 literal colors or color comments. Put each
real declaration in one of four groups:

1. Already token-based. Leave it.
2. Theme-dependent. Move the existing value into both theme files as a new token, unchanged.
3. Fixed on purpose, such as image scrims under fixed text or paper and print treatments. Keep it.
4. Unclear. Leave it until it has been seen in both modes.

Start with the clearest cases, listed in Batch 2.

## Batches

### Batch 1: restore branding and structure (F1 to F4)

1. Light tokens go back on `:root` only. Remove `body.light-theme` from the selector; the body class
   stays for component styles that use `:host-context(body.light-theme)`.
   Verify: `grep -n "body.light-theme" src/styles/themes/_light.scss` finds nothing.
2. Remove the branding-owned variables listed in F1 from the dark block. Set
   `--accent-text: var(--accent-color)` in dark, as before. Keep `--accent-hover-text` and the
   `--text-on-*` tokens.
   Verify: in the browser, set a non-gold accent in OrgConfig (or set `--accent-color` on `html` in
   devtools) and see it apply in both modes.
3. Move typography, spacing and radius tokens out of both theme files into one shared block.
   Verify: each of those names appears once under `src/styles` and `styles.scss`.
4. Restore the F2 comments, one line each.
5. Add `core/constants/theme.config.spec.ts`: `isTheme` for `'light'`, `'dark'`, `''`, `'toString'`,
   `null`-like input from storage; `getNextTheme` for both themes.
   Verify: `npm run test:unit -- src/app/core/constants/theme.config.spec.ts src/app/core/services/theme.service.spec.ts src/app/common/theme-toggle`.

### Batch 2: clearest color candidates

1. `public/landing/landing.scss:16` (light scrim `rgba(255,255,255,0.88)` to `0.93`) and `:65` (dark
   scrim `rgba(0,0,0,0.82)` to `0.88`) become `--hero-scrim-start` and `--hero-scrim-end` in the
   light and dark theme files with the same values. Delete the `body.dark-theme .hero` override once
   the token carries the dark value. Shorten the long comment at lines 9-12 to the one fact that
   still matters: `rgba(var(--x))` cannot take a hex token.
   Verify: the landing hero looks the same in both modes before and after.
2. `common/logo-spinner/logo-spinner.scss:22`, `:47`, `:54` and the two gold shadows in
   `layouts/admin-layout/admin-layout.scss:159` and `:300` use `rgba(197, 160, 89, a)`. Change them
   to `rgba(var(--accent-color-rgb, 197, 160, 89), a)` with the same alpha, the pattern already used
   at `logo-spinner.scss:65`. Default gold looks identical; a configured accent now follows.
   Verify: spinner and active admin nav item look the same with the default accent.
3. Leave fixed: the campus-photo and paper-flag effects in `public/about/about.scss`, the photo
   overlays in `admin/gallery/admin-gallery.scss`, and black shadows and backdrops such as
   `admin-layout.scss:339` and `:346`. Change one only if a concrete failure is seen in one mode.

### Batch 3: harden color math

`core/utils/color-math.ts:13` `hexToRgb` does not reject bad input: `'#zzz'`, `''` or `'#12345'`
come back as a valid-looking color, and `contrastRatio` then returns a normal-looking number.
`OrgConfigService` already calls it, so do not change what it returns for valid input.

1. Add `parseHexColor(value: string): Rgb | null`. It accepts `#rgb` and `#rrggbb` with or without
   `#`, trims spaces, and returns `null` for anything else. Have `hexToRgb` keep its current
   signature; callers that need a safe answer use `parseHexColor`.
2. Add `safeContrastRatio(a, b): number | null` that returns `null` if either side fails to parse.
3. Tests in `color-math.spec.ts`: valid 3 and 6 digit forms, upper case, missing `#`, spaces;
   invalid `''`, `'#'`, `'#12'`, `'#12345'`, `'#1234567'`, `'#gggggg'`, `'red'`, `'rgb(0,0,0)'`;
   `safeContrastRatio` returns `null` for each invalid side.
   Verify: `npm run test:unit -- src/app/core/utils/color-math.spec.ts`.

### Batch 4: special-day banner preview in admin setup

Change admin setup only. The live banner in `layouts/public-layout/public-layout.html:4-8` and the
host background from `sidebarColor` in `public-layout.ts:28-32` stay as they are, and no saved value
changes.

1. In `admin/themes/admin-themes.html:196`, the preview box uses only `backgroundColor`. Make it use
   the same background as the live banner: the 135deg gradient from `backgroundColor` to
   `sidebarColor || backgroundColor` when `enableGradientFading` is on, else the solid color. Put the
   expression in one function or computed value in `admin-themes.ts` so the preview and the live
   banner cannot drift. If it is shared with `public-layout.html`, the live output must stay byte
   for byte the same.
2. When `imageUrl` is set, the live banner shows the image and no text. Show the image in the
   preview too, and skip the text contrast check.
3. Contrast advice in `admin-themes.ts`, advisory only:
   - Parse `textColor` and `backgroundColor` with `parseHexColor`. Parse `sidebarColor` only when
     the gradient is on and the field is not empty; the live banner ignores it otherwise and falls
     back to `backgroundColor` when it is empty. If a color the banner uses is invalid, show "Enter
     a hex color like #1a2b3c" and no ratio. Never show a pass for bad input.
   - Solid banner: ratio of text against the background.
   - Gradient banner: assess the transition and report the lowest ratio and where it falls. Do not
     report a pass from five fixed samples alone; test a gradient whose worst contrast falls between
     sample points.
   - Below 4.5:1, show a warning with the ratio. Do not warn about `sidebarColor` alone; it is the
     page background behind the layout, and no banner text sits on it.
4. Suggestions are opt-in. Offer a "Use suggested text color" button that fills `textColor` with
   the smallest lightness change that passes on every gradient color, not only the worst one. If no
   color passes, leave the field alone and say the banner colors need to change. The button only
   changes the form field; nothing is saved until the admin presses Save, and no color changes
   without a click.
5. A Light/Dark switch on the preview may show the banner against each app mode's page background.
   It must not change the configured colors or imply the banner has a per-mode pair.
6. Tests in `admin-themes.spec.ts` (create it if missing, match nearby specs): solid pass and fail;
   gradient where the ends pass and the middle fails; image banner shows no text warning; invalid
   hex shows the input message and no ratio; suggestion fills the field only on click and does not
   call the save service.
   Verify: `npm run test:unit -- src/app/admin/themes`.

### Batch 5: F6 and F7

1. Look at every F6 use in light and dark mode. Where the remap reads wrong over a photo or a dark
   overlay, replace the utility class in that template with a token that names the intent, such as
   `--text-on-image`, rather than undoing the global remap.
2. Pick one focus style (F7) and remove the other from the shared rules.
   Verify: Tab through the login form, an admin table and a modal in both modes; one clear ring each.

### Batch 6: full checks

Run from `GHCAA.Web` unless shown:

1. `npm run type-check`
2. `npm run test:unit`
3. `npm run build` (production build; it also runs the generators and type-check)
4. `python scripts/guard.py` from the repo root
5. `python scripts/ci-local.py --only web` from the repo root, after checking free space
6. If the app runs safely (`.\GHCAA.Tools\run-app.ps1`), look at these in both modes: landing hero,
   about page, public layout with an active special-day theme (solid, gradient, image), admin
   themes form and preview, admin nav, a dense admin table, a modal, the logo spinner, and the F5
   size changes. Stop the app with `stop-app.ps1`.
7. Run `graphify update .` from the repo root after the doc and code changes.

## Handback notes to include

- Which F5 size changes broke a layout, with the page name.
- The F8 card-background choice.
- New dark values for Flutter `AppColors` (F11), as a list for the owner.
- F10 pre-paint script as a follow-up.
- Every literal from the color audit left in group 3 or 4, with its file and line.
