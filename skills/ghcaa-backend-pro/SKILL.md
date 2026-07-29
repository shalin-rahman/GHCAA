---
name: ghcaa-backend-pro
description: Standards for C#/.NET development within the GHCAA project.
---

# GHCAA Backend Engineering Standards

## 1. Patterns
- **Interfaces**: Always use interfaces for services (e.g., `IMemberService`).
- **DTOs**: Never return Domain models directly to the client. Map to DTOs in the Application layer.
- **Async**: Always use `Async` suffix for asynchronous methods and pass `CancellationToken`.

## 2. Data Persistence
- **EF Core**: Use `IEntityTypeConfiguration<T>` in `Infrastructure/Data/Configurations` for model mapping.
- **Soft Delete**: Ensure `HasQueryFilter(x => !x.IsArchived)` is applied to supported entities.

## 3. Validation
- Use Data Annotations or custom logic in the Application layer to validate incoming DTOs.
- Match validations exactly with Frontend `Validators`.
