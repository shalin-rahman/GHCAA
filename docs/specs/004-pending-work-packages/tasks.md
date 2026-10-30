# Pending Work Packages Tasks

## 2026-10-27 non-election reconciliation

`docs/TODO.md` remains the only place that carries authoritative task status,
priority, dependencies, and acceptance criteria. The entries below are execution
breakdowns linked to tracker items. Update the tracker first when scope or
acceptance changes. Use
[`NON_ELECTION_BACKLOG_EXECUTION_PLAN.md`](../../NON_ELECTION_BACKLOG_EXECUTION_PLAN.md)
for delivery order. Election-specific items are intentionally not in this
breakdown.

### Existing specifications and task files

- **Shared foundations:** 8.5, 8.7, 8.8, 27.8, 28.25, 28.33, 34.D7–34.D8,
  60.3, 84.4–84.5, 84.8–84.9, 84.18, 84.21, 84.23, 84.25, 84.27, 84.31,
  84.35, and 84.39. Use specs 001–003 and this work-package index.
- **Identity validation:** 84.49 uses spec 005 tasks for member NID, PassingYear,
  and age validation. Its ballot-key progress and timeout work stays in the
  election plan.
- **Profile and white-label:** 62.2, 62.6b, 62.41–62.42, and 62.56. Use
  specs 005 and 025; spec 025 tasks T001–T036 cover profile-pack export/import.
- **Alumni programs:** 6.2 uses spec 016; 37.4, 37.5, 37.9, and 37.10 use
  spec 003 tasks. Extend the existing localization registry once for 8.8 and
  37.7 using this index. Items 37.2, 37.3, and 37.6 are complete and archived;
  WP40 is complete. Apply the shared 37.11 gate to non-election WP37 changes;
  keep election-only test evidence in the election plan.
- **Batch import and reporting:** 88.1, 88.2, 88.3, 88.4, and 88.5 use spec 008; 90.1, 90.2, 90.3, 90.4, and 90.5 use spec
  010. Build 88.5's shared grid before 90.4 consumes its read-only mode.
- **Configurable approvals:** 92.1, 92.2, 92.3, 92.4, and 92.5 use new spec 022 and its task file.
- **Meetings, tasks, and notes:** 84.19–84.20 use new spec 026 and its task
  file, after 84.8 and 84.18.
- **Identity:** 97.1 extends spec 013 and its task file. Reuse registration OTP
  and its existing attempt lock; an unverified address does not block sign-in.
- **Account security:** the generic password-setup lockout, account unlock,
  authenticator lock race, admin reset transaction, and other shared auth fixes
  in 84.51 use spec 012 tasks. Keep election-only proposer rules and device
  evidence in the election plan.
- **Modules and performance:** 91.3, 91.4, 91.5, and 91.6 remain on hold
  until their recorded triggers occur. 98.7 uses spec 024 tasks; 98.34
  remains on hold.
- **Operations:** 7.21, 7.22, 7.26, 7.28, 34.D17, 34.D18, 34.D19, 84.48, and 96.15
  are gated by owner or live-environment evidence. The applicable references
  are in spec 020 and the tracker. Do not represent these as locally verified.
- **Governance and dissertation:** 12.8, 12.9, 63.8, 63.18, 63.34, 64.8, 64.9,
  67.2, 67.3, 67.4, 67.5, 67.7, 72.4, 72.5, 73.5, 73.6, 74.3, 75.5, 76.4, 77.4, 77.11, and
  78.9–78.10 stay linked to their TODO entries and the book rules.

### Breakdown rules

- Each spec task file lists ordered work steps and the exact TODO IDs it serves.
- Do not copy a TODO item's status or acceptance line into a spec task file.
- Mark an owner action as blocked in the plan; do not create an engineering task
  to change a live service or make an owner decision.
- Keep election-purpose items out of this list, including election evidence in
  otherwise mixed work packages.

### Additional package steps

- [ ] **8.5 — Measure before choosing storage.** Compare Drift and Isar against
  current mobile local-storage needs, supported platforms, encryption, offline
  queries, migration, and rollback. Record benchmark criteria and the result;
  add no production dependency without a demonstrated benefit.
- [ ] **8.7 — Define hydrated-state safety.** For each persisted provider, record
  ownership, version, expiry, logout/account-switch clearing, corruption
  recovery, and offline fallback. Keep tokens in secure storage and test restart
  and account changes.
- [ ] **8.8 / 37.7 / 28.33 — Extend localization once.** Reuse the flat English
  and Bengali runtime registry; cover public, member, admin, validation, error,
  accessibility, notification, and generated-document strings. Verify fallback
  and the runtime toggle; reconcile 28.33 as overlapping scope.
- [ ] **27.8 — Establish and enforce coverage.** Measure current backend and Web
  baselines, exclude generated/vendor files explicitly, agree on the per-file
  threshold, and make CI failure actionable without weakening the tracker
  requirement.
- [ ] **28.25 — Record visual regression evidence.** After the dependent Angular
  consumer is complete, run the documented Playwright snapshot command and
  record its result or the reason the task is superseded.
- [ ] **34.D7–34.D8 — Complete local quality evidence.** Regenerate the stale
  Mobile goldens as a standalone change; then run public and admin browser
  checks after the recorded local role blocker is resolved.
- [ ] **37.11 — Apply the shared WP37 release gate.** For each non-election
  feature, run the listed backend, Web, type-check, and build checks, plus
  non-empty-database validation when tables change. Keep election-only test
  evidence in the election plan.
- [ ] **60.3 — Verify naming consistency.** Compare Web and Mobile labels for
  Messages, Chat, and Forum; update the chosen user-facing labels and tests.
- [ ] **84.4–84.5, 84.8–84.9 — Finish the contract baseline.** Produce the
  client parity and authorization/error catalogs, then derive the platform
  requirements and decisions from code and tests.
- [ ] **84.18 — Complete the entity gap review.** Use specs 012–020, record user
  groups and association fit, and raise each accepted gap through the tracker.
- [ ] **84.21, 84.23, 84.25, 84.27, 84.31, 84.35, 84.39 — Close focused
  findings.** Follow each item's stated source path, reuse existing specs and
  tests, and do not widen a remediation beyond its acceptance.
- [ ] **84.49 — Apply owner-selected profile validation.** Allow NID edits at
  any time with format and uniqueness checks on submission; provide database
  lookup years with a fallback in both clients; use Bangladesh-local date for
  age validation. Leave the ballot-key UI portion in the election plan.
- [ ] **84.51 — Use spec 012 tasks for generic account security.** Keep unlock,
  authenticator locking, transaction behavior, and shared authentication
  defects separate from election-only candidate rules.

### Reconciliation checks

- Confirm every active non-election TODO ID appears in the mapping above or in
  an applicable spec task file.
- Confirm no `[ONHOLD]` item enters the ready order.
- Confirm the 88.5-to-90.4 shared-grid dependency remains intact.
- Confirm profile-pack work reuses spec 025 and does not create a second import
  specification.

