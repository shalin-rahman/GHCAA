---
name: ghcaa-project-orchestrator
description: Use this skill as the primary entry point when starting work on any GHCAA-related task. It orchestrates all other skills and ensures project-wide consistency.
---

# GHCAA Project Orchestrator (Mission Control)

This skill implements "Agent-First Development" for the GHCAA ecosystem. It ensures that any agent (like Antigravity) follows the team's engineering standards from the moment a task is assigned.

## 1. Project Initialization Workflow
Whenever a new task starts, you MUST:
1. **Activate `ghcaa-core-master` and `ghcaa-preflight`**: Load the architecture and hard constraints.
2. **Run Graphify first**: Use `graphify query`, `graphify explain`, or `graphify path` before raw searches when `graphify-out/graph.json` exists.
3. **Scan Context**: Check `docs/PROJECT_MAP.md` and `docs/TODO.md` for affected layers and related work.
4. **Verify Standards**: Activate `ghcaa-date-standard` for date or API formatting work and `ghcaa-design` for UI, layout, or theme work.

## 2. Standard Enforcement
- **No Placeholders**: Do not add placeholder text or mock logic where the task requires a real implementation.
- **Strict Typing**: Enforce C# types, TypeScript interfaces, and Dart models.
- **Sync Requirement**: Every API change REQUIRES a check on both Angular and Flutter clients.

## 3. Communication Pattern
- Use **imperative language** in guidance: "Always do X", "Never do Y".
- **Procedural Memory**: Reference existing patterns in `GHCAA.Infrastructure/Services` before proposing new ones.

## 4. Decision Tree for Tasks
- **Is it a Bug?** Follow `systematic-debugging` and verify with Playwright/Flutter Integration tests.
- **Is it a Feature?** Read `docs/SRS.md` first. Map the data flow from `Domain` to both clients where the contract is shared.
- **Is it a UI change?** Check both `GHCAA.Web` and `GHCAA.Mobile` for visual parity.

## Associated Skills
- `ghcaa-core-master`: Architectural standards.
- `ghcaa-date-standard`: Display/input and ISO wire-format rules.
- `ghcaa-backend-pro`: C#/.NET guidelines.
- `ghcaa-mobile-flutter`: Flutter/Riverpod guidelines.
- `ghcaa-web-angular`: Angular/Standalone components guidelines.
