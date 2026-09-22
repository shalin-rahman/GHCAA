---
name: ghcaa-core-master
description: Architecture and shared contracts for GHCAA.
---

# GHCAA Core Master

- Layers: Domain -> Application (DTOs/validators) -> Infrastructure (EF) -> API -> Web (Angular 21) / Mobile (Flutter).
- Membership numbers: `GHC-YYYY-XXXX`. Display `dd-MM-yyyy`; wire ISO `yyyy-MM-dd`.
- Soft delete stays `IsArchived`.
- Respect `IsMobilePublic`/`IsEmailPublic` toggles.
- API changes: check web and mobile both.
