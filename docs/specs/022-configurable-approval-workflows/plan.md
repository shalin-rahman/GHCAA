# Configurable approval workflows delivery plan

Follow [Work Package 92](../../TODO.md) and the requirements in [spec.md](./spec.md). This is a planning document; no implementation or policy choice is implied by it.

## Dependency order

1. Confirm TODO 92.1's four decisions and record them in the specification.
2. Inventory every current approval action and pending-approval query against the flow table in TODO 92.
3. Implement the backend settings contract, migration, policy evaluator, audit path, and tests as 92.2.
4. Add the SuperAdmin Web controls and role-aware queue/actions as 92.3.
5. Check Mobile approval surfaces and implement parity only where the app exposes those actions, as 92.4.
6. Synchronize the API registry, transition tables, project map, and book authorization material as 92.5.

## Constraints

- Preserve today's behavior on a fresh deployment until a setting is changed.
- The API enforces role access regardless of client visibility.
- Keep election-only presentation and workflow checks in the election delivery plan.
- Review existing authorization and audit tests before adding near-duplicates.
- Do not mark an item complete on the basis of this plan; use the tracker acceptance evidence.
