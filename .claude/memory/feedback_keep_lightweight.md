---
name: feedback_keep_lightweight
description: "Keep GHCAA app(s) lightweight — avoid bloating bundle/widget-tree weight when adding shared widgets, deps, or abstractions"
metadata: 
  node_type: memory
  type: feedback
  originSessionId: 80e0e324-13a0-4d9f-b036-6a6d47d35135
---

User wants the app kept lightweight — be conservative about anything that adds bundle size, extra widget nesting, or new dependencies.

**Why:** flagged right after a session where a shared `EmptyStateWidget` was extracted and ~25 files were migrated to it, and a new `screen_protector` plugin + org-branding theme factory were added — user wants confirmation these additions don't add unnecessary weight.

**How to apply:**
- Prefer extending/reusing existing shared widgets (`GlassContainer`, `AsyncValueWidget`, `AppScaffold`) over introducing new ones unless there's real duplication to justify it.
- When adding a Flutter/npm/NuGet package, check if an existing dependency already covers the need before pulling in a new one.
- Avoid speculative abstraction (factory methods, wrapper widgets) for hypothetical future reuse — only extract shared code when there are multiple concrete call sites today.
- When reporting work, note bundle-size/dependency impact if a change adds a new package or a widget used broadly across the app.

Related: [[session_ui_security_usability_review.md]]
