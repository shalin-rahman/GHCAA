# Operator recovery runbook

What to do when the platform has lost data, or a deploy's migration step has failed. Written against
what this repo actually documents about the Render + Neon deployment (see `RENDER_DEPLOYMENT.md`) —
it does not assume any backup or restore capability that isn't recorded somewhere in this repo.

## Before anything else

Check `docs/adr/0002-migrations-apply-automatically-at-startup.md` and
`docs/ARCHITECTURE.md` §5 for how the schema gets applied. Both incidents below trace back to that
mechanism.

## Scenario 1 — a deploy's migration step failed

`MigrationBootstrapper.EnsureMigratedAsync` runs at every boot, called from
`GHCAA.API/Extensions/DatabaseBootstrapperExtensions.cs`. If it throws, the app logs the exception as
**critical** and refuses to start. The new deploy never goes Live, so the code that needs the new
schema never serves traffic. (Before 2026-09-09 it fell back to `EnsureCreated()` and kept running on
a stale schema. Older notes may still describe that.)

Steps:

1. **Check the Render service logs** for the deploy that just went out. Look for the log line
   `"Migration bootstrap failed; refusing to start with an unverified schema."` and the exception
   logged with it. The exception is the real cause.
2. **Do not roll the code back yet** if the exception names a real schema problem (a column type
   change conflicting with existing data, a unique constraint violated by existing rows, etc.) — the
   next deploy will hit the same failure until the migration itself is fixed.
3. **Apply the migration manually** with the production connection string, from a machine that has
   the .NET SDK and this repo checked out at the failing commit:
   ```
   dotnet ef database update --project GHCAA.Infrastructure --startup-project GHCAA.API --connection "<DATABASE_URL>"
   ```
   Get `<DATABASE_URL>` from the Render service's Environment tab or the Neon console. Do not commit
   it to a file; `docs/deploy_connection.txt` did that once (see `RENDER_DEPLOYMENT.md` Step 7, security
   cleanup). Take a Neon branch of the database first so the manual run has something to go back to.
4. **Confirm** with `dotnet ef migrations list` against the same connection string that every
   migration through the one expected now shows as applied.
5. Redeploy, or restart the service, so the next boot finds nothing pending and starts.

Rolling back a migration can lose data. Rolling back `20261004190943_AddReturningOfficerFlag` and
`20261004194309_AddCountExecutor` drops the columns they added, and whatever those columns held is gone.
`20261005144927_AddApprovalOpenKey` adds a unique index on `ElectionApproval.OpenKey`, so two open requests
for the same step cannot both be stored. Back up the database before any `dotnet ef database update <earlier migration>`.

If the failure is something `MigrationBootstrapper`'s baselining logic can't resolve on its own — for
instance a genuinely conflicting schema change, not a legacy-`EnsureCreated()` baseline mismatch —
this needs a human decision about the migration itself, not another automated retry.

## Scenario 2 — data loss (a table, or the whole database, is gone or corrupted)

**What this repo documents, plainly:** `RENDER_DEPLOYMENT.md` names the database as **Neon Postgres**
and describes how to provision it, connect to it, and branch it per pull request
(`.github/workflows/neon_workflow.yml`). Preprod moved from Render Postgres to Neon on 2026-07-27. The repo
does not document any backup schedule, point-in-time recovery process or restore procedure for the
Neon database. (`docs/deploy_conn_Info.txt`, gitignored, still holds the old Render Postgres
connection string, which nothing uses.) **No backup/restore capability for this platform's database is
documented anywhere in this repo.** Treat that as the current true state, not an oversight to explain
around — an operator facing real data loss should check the Neon console and Neon's own plan-level
documentation directly, rather than assume a capability this repo has never described.

What can be recovered without a database backup:

- **Schema** — rebuilt from scratch by `MigrationBootstrapper` (which calls `EnsureCreated()` on an
  empty database) on the next boot. No manual step; see Scenario 1's mechanism.
- **Seed data that ships in the migrations or `Data/Seed/*.json`** — the constitution
  (`ConstitutionSeeder.SyncAsync`, runs at every boot), the organization config defaults
  (the OrgConfig seed step in `DatabaseBootstrapperExtensions.cs`), and the baked-in alumni seed rows described in
  `docs/TODO.md` item 82.16 (Work Package 82.31) all re-populate on a fresh database because they run
  from code, not from a database backup.
- **The protected super-admin list** — comes back from `appsettings.json` on next boot
  (`ProtectedSuperAdminSeeder`, see `docs/adr/0003-protected-super-admin-list.md`), not from a
  database row.

What cannot be recovered without a database backup, because nothing in this repo produces one:

- Every member record, financial ledger entry, governance vote, gallery upload row, forum post, and
  anything else a person entered through the running application since the schema was last empty.

If a real backup/restore capability exists on the Neon account (a paid-tier feature, a
manually configured export, anything set up outside this repo), record it here and in
`RENDER_DEPLOYMENT.md` as soon as it exists, with the actual steps to invoke it. Until then, the
honest operator action on real data loss is: rebuild the empty schema per Scenario 1, accept that
application data since the last point this repo can prove was backed up is gone, and raise getting an
actual backup policy in place as its own item in `docs/TODO.md`.

## Scenario 3 — uploaded files (photos, documents) are gone

Out of scope for this runbook's data-loss case above, but worth stating here since it's the other
kind of "restore the platform" question an operator will ask: uploads are stored on local disk
(`wwwroot/uploads`), which Render does not persist across a redeploy or restart on the current plan —
see the `project_uploads_ephemeral_storage` note in project memory. There is no restore path for a
lost upload; the member or admin has to re-upload it.
