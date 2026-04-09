# Visual Testing Architecture & Storage

This document outlines the directory structure and standards for UI consistency (Visual Testing) across the GHCAA platform.

## Directory Structure

Visual artifacts are stored within their respective project test directories to keep logic close to implementation.

### Web (Angular/Playwright)
Location: `GHCAA.Web/tests/visual/`
- `baseline/`: Reference images approved by design.
- `failure/`: Actual screenshots from failed runs.
- `diff/`: Image diffs showing deviations in red.

### Mobile (Flutter/Goldens)
Location: `GHCAA.Mobile/test/goldens/`
- `ios/`: Reference goldens for iOS rendering.
- `android/`: Reference goldens for Android rendering.
- `failures/`: Rendering mismatches captured during CI.

## Seed Data Requirements
To ensure 100% pixel-perfect matching:
1. **Dates**: All relative dates must be fixed to a static epoch (e.g., `2026-04-01`).
2. **Text**: No random placeholders. Use 'Shalin Rahman' or specific institutional names.
3. **Images**: Use local static assets in `wwwroot/visual-seed/` instead of dynamic uploads.

## Approval Workflow
1. Run `scripts/visual-check.ps1`.
2. Review `failure/` and `diff/` folders.
3. If the change is intentional, run `scripts/visual-check.ps1 -UpdateBaselines`.
