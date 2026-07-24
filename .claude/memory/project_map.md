---
name: GHCAA project_map.md reference
description: The project has a maintained class/service/route map at project_map.md in the repo root — use it before broad file searches to minimize token usage
type: reference
---

The file `project_map.md` at the repo root (`GHCAA/project_map.md`) is a comprehensive, maintained class map of the entire application. It covers:
- All domain models with properties
- All interfaces and their DI registrations
- All DTOs
- All infrastructure services and gateways
- All API controllers and their routes
- All Angular services, components, guards, and routes
- All Flutter screens and services
- All test fixtures

**How to apply:** Before reading files broadly to explore the codebase, read `project_map.md` to find exact file paths and class names. This significantly reduces token consumption.
