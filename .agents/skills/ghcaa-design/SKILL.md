---
name: ghcaa-design
description: Design-system guidance for GHCAA web and Flutter.
---

# GHCAA Design

- Check for an existing shared component before hand-rolling; new reusable UI goes in the shared/common layer.
- Web: fix tokens/classes in `styles.scss`. Reuse `.form-group`, `.btn`.
- Flutter: central theme and shared widgets, no raw values.
- Tokens: `--bg-color`, `--surface-color`, `--card-bg`, `--text-main`, `--accent-color`.
- Verify light and dark after theme work.
