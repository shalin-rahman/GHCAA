# ADR-0006: Drop the MySQL provider, keep Sqlite as a test-only path

Status: Accepted

## Context

`GHCAA.Infrastructure/DependencyInjection.cs` switched on a `DatabaseProvider` setting across
`sqlite`, `mysql` and PostgreSQL, pooling a provider-specific shim context (`DbContextShims.cs`)
for each. Only the PostgreSQL path ever had a working migration tree: `Data/Migrations/PgSql/`
holds every real migration, and none of them is attributed to `SqliteApplicationDbContext` or
`MySqlApplicationDbContext`. Selecting MySQL in a real deployment would boot against an empty
schema. `docs/book/06-architecture.md`'s ADR-02 states as a consequence that "migrations apply
correctly on boot across PostgreSQL, MySQL and SQLite" — not true of this tree for either
non-Postgres provider, and that book chapter still needs a follow-up correction (out of scope for
this change, since editing it triggers the stricter `docs/book/build/build.py --pdf --strict` gate).

Sqlite is different from MySQL in one respect: two integration test factories
(`GHCAA.Tests/Integration/OutputCacheTestFactory.cs`, `SpaStaticFileFactory.cs`) set
`DatabaseProvider=Sqlite` and `ASP_SEED_PROFILE=Visual` to boot the real `GHCAA.API` pipeline
against a private SQLite file. The Visual profile branch in `Program.cs` skips
`MigrationBootstrapper` entirely and calls `context.Database.EnsureCreated()` instead, so these
tests never depend on a Sqlite migration tree existing — they were never broken by the problem this
item describes. MySQL had no such user; nothing in the tree constructs `MySqlApplicationDbContext`
or reads `PaymentGateways`-style config expecting a MySQL connection to work.

## Decision

Remove MySQL entirely: the `mysql` DI switch case, `MySqlApplicationDbContext`,
`MySqlDesignTimeDbContextFactory`, its `MySqlConnection` string branch, and the
`Pomelo.EntityFrameworkCore.MySql` package reference from both `GHCAA.Infrastructure` and
`GHCAA.API`. Keep the Sqlite branch, but only as the test harness's fast, migration-free boot path
— it is not a supported deployment target, and the DI code now says so in a comment next to the
switch.

## Consequences

- A `DatabaseProvider` value of `mysql` no longer resolves to anything; the switch's `default` case
  (PostgreSQL) picks it up instead of silently booting against an empty schema.
- `dotnet restore` drops the Pomelo package and its transitive dependencies from both
  `packages.lock.json` files.
- `docs/book/06-architecture.md`'s ADR-02 consequence cell ("... across PostgreSQL, MySQL and
  SQLite") is now doubly inaccurate (MySQL is gone, Sqlite was never really covered by it either)
  and needs a follow-up edit through the book's own build pipeline — tracked as a gap by this
  record rather than a docs/TODO.md item, since 82.15 is otherwise closed by this change.
