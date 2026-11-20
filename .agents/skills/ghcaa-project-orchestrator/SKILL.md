---
name: ghcaa-project-orchestrator
description: Entry point for GHCAA tasks.
---

# GHCAA Project Orchestrator

- Start with `ghcaa-core-master` and `ghcaa-preflight`.
- Use `graphify query/explain/path` before broad source reads.
- Check `docs/PROJECT_MAP.md` and `docs/TODO.md` before patching.
- Open work runs as session cards in `docs/MASTER_EXECUTION_PLAN.md`: take the next card in section 3, follow the rules in section 2, read only that card and its tracker entries. The Summary column links each session to its detailed card; keep summaries in sync when changing a session. Section 1 says which phases use the eleven-part card form; rewrite a short-form phase before running it (`python scripts/backlog_inventory.py --card <id>` prints a skeleton).
- Pull `ghcaa-date-standard` for dates, `ghcaa-design` for theme.
- Before planning or changing code, check impact across API, web, mobile, and tests — never just one layer.
