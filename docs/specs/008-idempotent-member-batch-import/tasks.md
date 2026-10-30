# Idempotent member batch import tasks

Tracker source: Work Package 88 in [docs/TODO.md](../../TODO.md). Keep status and acceptance in the tracker; use [spec.md](./spec.md) and [plan.md](./plan.md) for behavior and ordering.

## Sequence

- [ ] **88.1 — Complete import coverage.** Add CSV support and safe upsert handling for PaymentHistory, User, roles, and membership number collisions as specified.
- [ ] **88.2 — Make each import atomic.** Keep row-level validation reporting, but roll back all writes if persistence fails during the run.
- [ ] **88.3 — Add regression integration coverage.** Reuse existing importer fixtures; prove idempotent rerun, related-row uniqueness, and full rollback on failure.
- [ ] **88.4 — Validate against real PostgreSQL.** Run the tracker’s real-database procedure and record row-count evidence without using production data.
- [ ] **88.5 — Add dry-run and editable preview.** Return row and cell errors without writes, build the shared editable grid, revalidate edits, and require a clear commit/exclusion action.

## Completion evidence

- Same-file rerun creates no duplicate rows.
- Row validation errors remain attributable to the source row and field.
- A persistence failure leaves no partial batch.
- Preview performs no writes; corrected rows are revalidated before commit.
- PostgreSQL evidence and UI tests are recorded against tracker acceptance.
