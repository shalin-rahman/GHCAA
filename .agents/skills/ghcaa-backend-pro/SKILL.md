---
name: ghcaa-backend-pro
description: C#/.NET guidance for GHCAA services and validation.
---

# GHCAA Backend Pro

- Services use interfaces; return DTOs, never domain models. Controllers never touch `ApplicationDbContext` directly.
- Async methods end `Async`; pass `CancellationToken` through.
- EF mapping in `IEntityTypeConfiguration<T>`; keep `IsArchived` and query filters.
- Validate DTOs in Application layer; controllers stay thin.
- Reuse patterns; no duplication; apply SOLID
