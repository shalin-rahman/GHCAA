---
name: ghcaa-backend-pro
description: Standards for C#/.NET development within the GHCAA project.
---

# GHCAA Backend Engineering Standards

## 1. Patterns
- **Interfaces**: Always use interfaces for services (e.g., `IMemberService`).
- **DTOs**: Never return Domain models directly to the client. Map to DTOs in the Application layer.
- **Async**: Use the `Async` suffix for asynchronous methods and pass a `CancellationToken` through service and data-access calls.

## 2. Data Persistence
- **EF Core**: Use `IEntityTypeConfiguration<T>` in `Infrastructure/Data/Configurations` for model mapping.
- **Soft Delete**: Preserve the repository's `IsArchived` flag and apply `HasQueryFilter(x => !x.IsArchived)` to supported entities. Do not rename it to `IsDeleted` in an isolated change.

## 3. Validation
- Use the repository's FluentValidation validators or application-layer validation for incoming DTOs.
- Match validations exactly with Frontend `Validators`.
