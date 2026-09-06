# Architecture decision records

One file per decision. Number, title, status, context, decision, consequences — same shape for all of
them. Numbering here is local to this folder and unrelated to the ADR-01..06 table in
`docs/book/06-architecture.md`, which documents a different set of decisions for the dissertation.

- [0001](0001-two-format-date-contract.md) — Two-format date contract: dd-MM-yyyy on read, ISO-8601 on write
- [0002](0002-migrations-apply-automatically-at-startup.md) — Migrations apply automatically at startup, not as a manual step
- [0003](0003-protected-super-admin-list.md) — Protected super-admin list lives in config, not the database
- [0004](0004-single-instance-deployment-constraint.md) — Single-instance deployment constraint
