# Configurable approval workflows tasks

Tracker source: Work Package 92 in [docs/TODO.md](../../TODO.md). Keep status, priority, dependencies, and acceptance in the tracker.

## Sequence

- [ ] **92.1 — Confirm the four decisions.** Resolve settings location, disabled-flow behavior, SuperAdmin override, and member-import role scope. Record the choices in `spec.md` before implementation.
- [ ] **92.2 — Backend policy and persistence.** Centralize flow keys/defaults; add the settings boundary; migrate supported approval routes and queue queries; map registration mode; audit settings changes and disabled-flow approvals; cover defaults, roles, disabled/enabled paths, and migration behavior.
- [ ] **92.3 — Web administration.** Add the SuperAdmin settings surface using shared controls; show flow-specific approve/reject actions and queue entries only when allowed; cover settings and visibility behavior.
- [ ] **92.4 — Mobile parity.** Inventory approval actions; gate applicable buttons using the shared settings contract; add widget coverage for visible and hidden actions.
- [ ] **92.5 — Documentation and release evidence.** Update spec 002 transition tables, business-flow notes, project map, API contract registry, and the book authorization matrix/figure. Run the required strict book build.

## Completion evidence

- Backend tests cover each flow's enabled/disabled behavior, allowed/refused roles, safe defaults, audit writes, and settings failures.
- Authorization reflection tests pass; migrations apply on SQLite and PostgreSQL.
- Web component tests and Mobile widget tests cover role-dependent actions.
- API contract documentation matches the implemented routes and DTOs.
- TODO 92 items are updated only after their tracker acceptance evidence exists.
