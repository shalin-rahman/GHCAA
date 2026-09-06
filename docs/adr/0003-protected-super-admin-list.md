# ADR-0003: Protected super-admin list lives in config, not the database

Status: Accepted

## Context

`AppSettings:ProtectedSuperAdmins` in `appsettings.json` names a fixed list of usernames (currently
`shalin`, `superadmin`). Three places read it:

- `Program.cs:521-526` runs `ProtectedSuperAdminSeeder.EnsureAsync` on every boot, re-granting the
  `SuperAdmin` role to any protected username that has lost it — including after the `Visual` profile
  wipes and reinserts every `User` row from `Seed/Visual/users.json`, which carries no role data. This
  step deliberately runs last in `Program.cs` for that reason.
- `UserService.cs:169` reads the same list to block the admin API from deleting or disabling a
  protected username.
- `ProtectedSuperAdminSeeder.cs`'s own doc comment states the list is config-only — "never DB- or
  admin-UI-editable" — and says why in the source.

## Decision

The protected list is a config value (`appsettings.json` / environment variable), never a database
table or an admin-UI-editable setting. Changing it requires a config change and a redeploy, not an
admin panel action.

## Consequences

- No SuperAdmin — including a compromised or careless one — can remove another SuperAdmin's protected
  status through the application. Only someone with deploy access can change the list.
- The list is small and manual. Adding an institution's second protected super-admin means editing
  `appsettings.json` for that environment, not a data migration.
- Because the list isn't a database row, an operator restoring from a schema loss or migration
  failure doesn't need to worry about losing it — it comes back with the next boot's config load, not
  with a data restore. See `docs/RECOVERY_RUNBOOK.md`.
- If a white-labeled institution profile pack (Work Package 62) ever needs a different protected list
  per deployment, that's still a config concern per profile pack, not a reason to move this into the
  database.
