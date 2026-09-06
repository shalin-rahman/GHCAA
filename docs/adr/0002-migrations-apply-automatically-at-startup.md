# ADR-0002: Migrations apply automatically at startup, not as a manual step

Status: Accepted

## Context

There is no `dotnet ef database update` step anywhere in the deploy pipeline — not in the
Dockerfile, not in `RENDER_DEPLOYMENT.md`, not in CI. Render rebuilds the image and starts the
container; whatever the container does on boot is the whole migration story.

That boot-time work is `GHCAA.Infrastructure/Data/MigrationBootstrapper.EnsureMigratedAsync`, called
from `Program.cs:449` for every profile except `Visual` (the local test-data profile, which has its
own recreate/seed path). It runs before any data seed step.

The problem it solves: preprod's database was first built with `Database.EnsureCreated()`, which
creates the schema straight from the current model and never writes `__EFMigrationsHistory`. Calling
`MigrateAsync()` against that database tries to `CREATE TABLE` on tables that already exist and fails.
`MigrationBootstrapper` baselines a legacy database first — walking every migration in order and
marking one applied without re-running it whenever Postgres reports the target object already exists —
then calls `MigrateAsync()` for whatever is genuinely still pending. It also self-heals a migration
whose "applied" history row is a false positive (a same-transaction seed insert that rolled back
partway through an otherwise-successful migration).

`Program.cs:447-458` wraps the whole thing in a try/catch. If bootstrap throws, the app logs an error
and falls back to `EnsureCreated()` rather than crashing — a bug in the bootstrap logic degrades
service instead of taking the app down, at the cost of the schema silently staying stale until a human
looks at the log.

## Decision

Migrations apply themselves, in-process, on every boot. There is no separate "run migrations" job,
container, or CI step, and no manual step an operator performs before or after a deploy. A schema
change lands in the same commit as the code that needs it and reaches Render the next time that
commit deploys.

## Consequences

- An operator does not need to remember to run migrations — every deploy does it automatically. This
  is also the mechanism `README.md` describes (see ADR reference in Work Package 82.27).
- The cost is at boot, not at deploy time: every restart re-checks migration state, and a bootstrap
  failure is only visible in the application log, not as a failed deploy in Render's dashboard. An
  operator investigating a schema problem should check the boot log for `MigrationBootstrapper` and
  `EnsureCreated` fallback messages first, per `docs/RECOVERY_RUNBOOK.md`.
- This only works because `MigrationBootstrapper` is Postgres-specific (see `docs/TODO.md` item
  82.15) — the fallback path assumes a Postgres connection.
