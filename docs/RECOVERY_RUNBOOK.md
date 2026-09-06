# Operator recovery runbook

What to do when the platform has lost data, or a deploy's migration step has failed. Written against
what this repo actually documents about the Render + Neon deployment (see `RENDER_DEPLOYMENT.md`) —
it does not assume any backup or restore capability that isn't recorded somewhere in this repo.

## Before anything else

Check `docs/adr/0002-migrations-apply-automatically-at-startup.md` and
`docs/ARCHITECTURE.md` §5 for how the schema gets applied. Both incidents below trace back to that
mechanism.

## Scenario 1 — a deploy's migration step failed

`MigrationBootstrapper.EnsureMigratedAsync` runs at every boot (`Program.cs:449`) inside a try/catch
(`Program.cs:447-458`). If it throws, the app does not crash — it logs the exception as an **error**
and falls back to `EnsureCreated()`, which is a no-op against an existing database. That means a
failed migration does not stop the app from serving traffic; it means the app keeps serving traffic
against a schema that's now out of sync with the code that just deployed.

Steps:

1. **Check the Render service logs** for the deploy that just went out. Look for the log line
   `"Migration bootstrap failed; falling back to EnsureCreated. Schema may be stale until this is
   fixed manually."` (from `Program.cs:456`) and the exception logged with it — that's the actual
   cause, not a generic failure.
2. **Do not roll the code back yet** if the exception names a real schema problem (a column type
   change conflicting with existing data, a unique constraint violated by existing rows, etc.) — the
   next deploy will hit the same failure until the migration itself is fixed.
3. **Apply the migration manually** with the production connection string, from a machine that has
   the .NET SDK and this repo checked out at the failing commit:
   ```
   dotnet ef database update --project GHCAA.Infrastructure --startup-project GHCAA.API --connection "<DATABASE_URL>"
   ```
   Get `<DATABASE_URL>` from the Render service's Environment tab — do not commit it to a file
   (`docs/deploy_connection.txt` already did that once; see its own security-cleanup note).
4. **Confirm** with `dotnet ef migrations list` against the same connection string that every
   migration through the one you expect now shows as applied.
5. Redeploy (or just restart the service) so the app's next boot finds nothing pending and stops
   logging the fallback warning.

If the failure is something `MigrationBootstrapper`'s baselining logic can't resolve on its own — for
instance a genuinely conflicting schema change, not a legacy-`EnsureCreated()` baseline mismatch —
this needs a human decision about the migration itself, not another automated retry.

## Scenario 2 — data loss (a table, or the whole database, is gone or corrupted)

**What this repo documents, plainly:** `RENDER_DEPLOYMENT.md` names the database as **Neon Postgres**
and describes how to provision it, connect to it, and branch it per pull request
(`.github/workflows/neon_workflow.yml`). It does not document any backup schedule, point-in-time
recovery process, or restore procedure — for either Neon or Render's own Postgres offering (a Render
Postgres connection string also exists in `docs/deploy_connection.txt`, unused by the live
`RENDER_DEPLOYMENT.md` path). **No backup/restore capability for this platform's database is
documented anywhere in this repo.** Treat that as the current true state, not an oversight to explain
around — an operator facing real data loss should check the Neon console and Neon's own plan-level
documentation directly, rather than assume a capability this repo has never described.

What can be recovered without a database backup:

- **Schema** — rebuilt from scratch by `MigrationBootstrapper`/`EnsureCreated()` on next boot against
  an empty database. No manual step; see Scenario 1's mechanism.
- **Seed data that ships in the migrations or `Data/Seed/*.json`** — the constitution
  (`ConstitutionSeeder.SyncAsync`, runs at every boot), the organization config defaults
  (`Program.cs`'s OrgConfig seed block), and the baked-in alumni seed rows described in
  `docs/TODO.md` item 82.16 (Work Package 82.31) all re-populate on a fresh database because they run
  from code, not from a database backup.
- **The protected super-admin list** — comes back from `appsettings.json` on next boot
  (`ProtectedSuperAdminSeeder`, see `docs/adr/0003-protected-super-admin-list.md`), not from a
  database row.

What cannot be recovered without a database backup, because nothing in this repo produces one:

- Every member record, financial ledger entry, governance vote, gallery upload row, forum post, and
  anything else a person entered through the running application since the schema was last empty.

If a real backup/restore capability exists on the Neon or Render account (a paid-tier feature, a
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
