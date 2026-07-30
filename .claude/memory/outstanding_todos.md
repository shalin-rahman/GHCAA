---
name: ghcaa-outstanding-todos
description: POINTER ONLY — all outstanding work now lives in docs/TODO.md. Do not track tasks here.
metadata:
  node_type: memory
  type: project
---

## Superseded — single source of truth is `docs/TODO.md`

Task tracking is consolidated in **`docs/TODO.md`** (Areas 1–31). Do not re-add task lists here.

- Migration status → `docs/TODO.md` **Area 31**. Summary: nothing pending; `PhaseB_S5S8_...` is applied
  and RefreshTokens shipped inside the `AddDiscussionForums` migration. EF "pending model changes" on
  PgSql is spurious seed churn — never scaffold a migration for it.
- Area 24 security hardening → complete (see `docs/TODO.md` Area 24).
- UI/UX remediation programme → `docs/TODO.md` **Area 30**, with the full plan in
  `docs/UI_UX_REMEDIATION_PLAN.md`. Only 30.1 (styles.scss de-dup) is done.
