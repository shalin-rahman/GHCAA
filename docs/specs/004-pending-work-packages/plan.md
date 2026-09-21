# Pending Work Packages Delivery Plan

## Delivery order

1. Close evidence and contract work first: 84.4, 84.5, 60.2, 60.3, 73.5,
   73.6, and the profile/brand prerequisites 62.2, 62.41, 62.42.
2. Deliver shared Mobile foundations: 8.5, 8.7, and 8.8. Localization is a
   prerequisite for 37.7 and should be implemented once, not twice.
3. Deliver member-programme domains in dependency order: 37.2, 37.4, 37.5,
   37.6, 37.8, 37.9, and 37.10.
4. Deliver election content administration 42.1–42.5 and profile-aware
   handbook behavior 82.59 without duplicating 37.1.
5. Complete quality gates: 27.8, 37.11, 47.13, 60.4–60.5, and 72.4–72.5.
6. Complete dissertation and provenance evidence: 63.x, 64.x, 65.5, 67.x,
   74.3, 75.5, 76.4, 77.4, and 78.x.
7. Deliver smaller independent backlog items: 6.2, 81.1–81.3, and 84.7 once
   their owner decisions are available.

## Per-item implementation slice

Each item MUST use this slice:

1. Inspect Graphify and source-of-truth files.
2. Define Domain/Application contracts and validators.
3. Add persistence configuration and provider migration where needed.
4. Implement Infrastructure service with authorization, privacy, and audit
   behavior.
5. Add thin API routes and register every route change.
6. Add Angular and Flutter clients or document tested non-applicability.
7. Add unit, integration, contract, and UI tests.
8. Synchronize SRS, project map, feature catalog, TODO evidence, and specs.
9. Run required checks and `graphify update .`.

## Decisions required before implementation

- Select Isar or Drift only after the 8.5 spike.
- Confirm whether Mobile exposes cohorts, archive, fundraising, and reports.
- Confirm the owner decision for 84.7.
- Confirm evaluation participants and environment for 60.4 and 78.9.
- Confirm whether the repository will become public before enabling CodeQL.
- Confirm the target mutation-testing budget for 47.13.
