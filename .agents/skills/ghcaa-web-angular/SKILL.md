---
name: ghcaa-web-angular
description: Angular guidance for GHCAA UI and dates.
---

# GHCAA Web Angular

- Standalone components, feature-based structure.
- Typed models in `core/models`, constants in `core/constants`.
- Use `styles.scss` tokens, not ad-hoc styling.
- Dates: display `dd-MM-yyyy`, API ISO `yyyy-MM-dd` via repo utilities. Never `new Date('dd-MM-yyyy')`.
- API changes: verify Angular and Flutter both.
