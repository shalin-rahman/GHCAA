# Non-election backlog execution plan (reconciled 2026-10-27)

This plan orders active non-election work recorded in `docs/TODO.md`. The tracker remains authoritative for status, priority, dependencies, owner decisions, and acceptance criteria. Read the linked tracker item before starting any package. Specification task files decompose work but do not create a second status ledger.

## Scope and classification

Include active `[TODO]`, `[PARTIAL]`, and `[IN PROGRESS]` items whose main outcome is general platform, member, operations, quality, governance, or dissertation work. Include shared platform work even when the election module also uses it. Exclude work whose main outcome is election workflow, ballots, polling, election officials, or election forms; those remain in the election delivery plan. Record `[ONHOLD]` work in the blocked register only. Owner-only actions stay assigned to the owner and do not enter the engineering queue.

## Delivery order

### 1. Release and environment blockers

Resolve owner or environment gates before dependent release work: 7.21, 7.22, 7.26, 7.28, 34.D17–34.D19, 62.41, 62.6b, 84.48, and 96.15. These require owner-controlled DNS, hosting settings, service setup, or a redeploy. Do not claim completion until the tracker’s live evidence is recorded. Election-only release dependencies stay in the election plan and are not expanded here.

### 2. Shared contract and identity foundations

Complete applicable contract and identity work: 84.4, 84.5, 84.8–84.9, 84.18, 84.21, 84.23, 84.25, 84.27, 84.31, 84.35, 84.39, 84.49, the general account-security portion of 84.51, and 97.1. The full 97.1 implementation is scheduled in this phase under spec 013. Write requirements for configurable approvals (WP92) and meetings/action items (84.19–84.20) before implementation. Resolve the validation foundation before DTO work, then deliver backend/API contracts before Web and Mobile consumers.

### 3. Mobile foundations and shared UX

Deliver 8.8 and 37.7 through the existing flat English/Bengali registry; reconcile 28.33 as duplicate localization scope rather than adding an ARB pipeline. Complete the evidence spike for 8.5 before choosing a local database, then consider 8.7 hydration. Complete 28.25 visual regression evidence, 93.3–93.4, 96.3, 60.3, and general client parity items only after shared contracts are stable. Keep election-only device evidence in the election plan.

### 4. Member and alumni product work

Use existing specifications and follow their dependencies: 6.2; 37.4, 37.5, 37.9, 37.10; 84.19–84.20; 88.1–88.5; 90.1–90.5; and 92.1–92.5. Work Package 97 is scheduled in phase 2. WP40 is complete and excluded. Build the shared editable/read-only grid for 88.5 before reporting UI 90.4. For 37.7, implement the shared localization foundation once, then extract the full Angular inventory. Track Mobile applicability in the relevant task files.

### 5. White-label profile and reusable modules

Complete 62.2 and its profile/brand prerequisites before 62.56 and 62.42. Reuse spec 025 and its T001–T036 tasks for profile-pack export/import. WP91 remains blocked: do not schedule 91.3–91.6 until the stated second-host or second-consumer trigger exists. Do not extract abstractions without that evidence.

### 6. Performance and quality

Finish 98.7 only if the recorded measurement after 98.6 still supports it; use spec 024 and its tasks. Complete 27.8 with a baseline before enforcing thresholds. Handle 34.D7 only as a standalone baseline-maintenance item. Apply the shared 37.11 release gates to non-election Work Package 37 changes; election-only evidence stays in the election plan.

### 7. Platform governance, security, and operations

Deliver applicable non-election portions of 12.8–12.9, 62.6b, 62.41–62.42, 72.4–72.5, 73.5–73.6, 74.3, 75.5, 76.4, 77.4, and 77.11. Owner decisions and public-repository triggers remain outside engineering until they occur. Keep secrets out of docs and local configuration values out of commits.

### 8. Dissertation evidence and evaluation

Complete 63.8, 63.18, 63.34, 64.8–64.9, 67.2–67.7, and 78.9–78.10 in dependency order. Before any chapter edit, follow `docs/book/README.md`, synchronize the outline and chapter structure, refresh task-number references when the tracker changes, and run the strict book build. Use only measurements and repository figures that can be reproduced from the tree.

## Blocked and owner-dependent register

- **Owner/platform actions:** 7.21, 7.22, 7.26, 7.28, 34.D17–34.D19, 62.6b, 84.48, 84.46, 96.15, 72.5, 75.5, 76.4, and 77.4 where the tracker names an owner action. Keep each in its tracker status until evidence arrives.
- **Explicitly on hold:** 91.3–91.6 wait for the school/club host or a second consuming application. 98.34 waits for a compatible fixed package or runtime upgrade. Do not put these in the ready order.
- **Product choice:** 84.7 remains a decision task, not implementation. 8.5 remains a measured spike; no database is selected before its evidence.
- **Election-only work:** election workflow, ballot, polling, and officer items are omitted and remain in the election plan. Mixed work packages include only their non-election items.

## Cross-package constraints

- TODO IDs and acceptance lines are the source of truth. Update references here when IDs or statuses change.
- Preserve existing contracts and reuse. Review existing tests before adding coverage; do not reduce assertion coverage.
- Shared API or business-rule changes cover backend, Web, Mobile, authorization, migrations where applicable, and tests. Register API changes in `docs/API_CONTRACT_REGISTRY.md`.
- Verify schema changes against a non-empty PostgreSQL database. Keep owner-controlled live changes outside local implementation claims.
- After implementation changes, run the relevant repository checks and `graphify update .`; this planning update does not change implementation state.
