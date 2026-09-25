---
name: ghcaa-preflight
description: Hard constraints and repo traps for GHCAA.
---

# GHCAA Preflight

- User owns git/deploy; agent never commits, pushes, or deploys.
- No payment gateway keys; flows stay admin-configurable.
- Keep changes lightweight; no new abstractions without clear need.
- UI/theme changes go to the shared layer; use `ghcaa-design`.
- Traps: seed JSON doesn't update a live DB; dates via the repo converter.
- Validate before done: web build, mobile test, backend tests.
