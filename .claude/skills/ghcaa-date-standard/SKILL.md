---
name: ghcaa-date-standard
description: GHCAA date formatting rules.
---

# GHCAA Date Standard

- Wire values: ISO (`yyyy-MM-dd`, or full ISO for timestamps).
- Display/input: `dd-MM-yyyy`. Convert at the boundary, never mix.
- Backend: repo's global JSON converter, no ad-hoc serialization.
- Angular: shared date helpers + `DatePipe`, never `new Date('dd-MM-yyyy')`.
- Flutter: `AppUtils.formatDate` / `AppUtils.toWire`.
