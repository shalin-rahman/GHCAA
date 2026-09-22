---
name: ghcaa-specification-source-of-truth
description: Derive GHCAA specs from implemented code.
---

# GHCAA Specification Source of Truth

Codebase is authority: Domain=entities, Application=DTOs/validators, Infrastructure=EF/services, API=controllers, Web/Mobile=clients, Tests=evidence. Docs are context, not truth, when they disagree.

Workflow: graph query first -> trace domain to clients -> read DTOs/services/tests -> record routes/validation/errors -> mark gaps, don't infer.
