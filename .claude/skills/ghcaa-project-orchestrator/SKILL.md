---
name: ghcaa-project-orchestrator
description: Use this skill as the primary entry point when starting work on any GHCAA-related task. It orchestrates all other skills and ensures project-wide consistency.
---

# GHCAA Project Orchestrator (Mission Control)

This skill implements "Agent-First Development" for the GHCAA ecosystem. It ensures that any agent (like Antigravity) follows the team's engineering standards from the moment a task is assigned.

## 1. Project Initialization Workflow
Whenever a new task starts, you MUST:
1. **Activate `ghcaa-core-master`**: Load the architectural blueprint and terminology.
2. **Scan Context**: Check `project_map.md` for affected layers and `TODO.md` for related tasks.
3. **Verify Standards**: If the task involves dates, activate `ghcaa-date-standard`.

## 2. Standard Enforcement
- **No Placeholders**: Never use placeholder text or mock logic. Use `generate_image` or actual implementation.
- **Strict Typing**: Enforce C# types, TypeScript interfaces, and Dart models.
- **Sync Requirement**: Every API change REQUIRES a check on both Angular and Flutter clients.

## 3. Communication Pattern
- Use **Imperative Language**: "Always do X", "Never do Y".
- **Procedural Memory**: Reference existing patterns in `GHCAA.Infrastructure/Services` before proposing new ones.

## 4. Decision Tree for Tasks
- **Is it a Bug?** Follow `systematic-debugging` and verify with Playwright/Flutter Integration tests.
- **Is it a Feature?** Read `SRS.md` first. Map the data flow from `Domain` up to `UI`.
- **Is it a UI change?** Check both `GHCAA.Web` and `GHCAA.Mobile` for visual parity.

## Associated Skills
- `ghcaa-core-master`: Architectural standards.
- `ghcaa-date-standard`: Strict dd-mm-yyyy enforcement.
- `ghcaa-backend-pro`: C#/.NET guidelines.
- `ghcaa-mobile-flutter`: Flutter/Riverpod guidelines.
- `ghcaa-web-angular`: Angular/Standalone components guidelines.
