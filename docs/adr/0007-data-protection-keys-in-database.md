# ADR-0007: Data Protection keys persist to the database, not the filesystem

Status: Accepted

## Context

`Program.cs` used to call `AddDataProtection().PersistKeysToFileSystem(new
DirectoryInfo(keyRingPath))`, gated on a `DataProtection:KeyRingPath` config value, with
`appsettings.Production.json` setting it to `/data/keys`. Render's actual `ASPNETCORE_ENVIRONMENT`
for this service is `Preprod` (`docs/RENDER_DEPLOYMENT.md`), so `appsettings.Preprod.json` loaded
instead of `appsettings.Production.json` — and it never had a `KeyRingPath` key at all. The
`if (!string.IsNullOrWhiteSpace(keyRingPath))` guard was false, so the custom persistence branch
never ran, and ASP.NET Core's own default took over: keys written to
`/root/.aspnet/DataProtection-Keys` inside the container.

Neither path would actually have survived a redeploy. This Render service has no persistent Disk
mounted (see `project_uploads_ephemeral_storage` — the same constraint already forces uploaded
member photos to be baked into the git repo and migrations as a workaround, tracked separately).
`/data/keys` was never backed by an actual volume; it was just as ephemeral as the ASP.NET default.
Every redeploy was silently generating a fresh key ring, invalidating anything protected by the old
one in between — antiforgery tokens and any other Data Protection consumer added later.

## Decision

Persist Data Protection keys to the existing PostgreSQL database instead of any file path.
`ApplicationDbContext` implements `IDataProtectionKeyContext` (a new `DbSet<DataProtectionKey>
DataProtectionKeys`, table added by migration `20260907120000_AddDataProtectionKeys`), and
`Program.cs` calls `.PersistKeysToDbContext<ApplicationDbContext>()`. This is the one piece of state
in this deployment that genuinely persists across a redeploy.

`PersistKeysToDbContext<T>` resolves `T` from the service provider lazily, at first key access —
not at `AddDataProtection()` call time — so it doesn't matter that this line runs before
`AddInfrastructure()` (which is what actually registers `ApplicationDbContext` in DI).

The now-dead `DataProtection:KeyRingPath` key was removed from `appsettings.json` and
`appsettings.Production.json` rather than left as unused, misleading config.

## Consequences

- Works uniformly across every environment (Development, Preprod, Production, and any future
  institution's own deployment) without per-environment config — there is no longer a
  `KeyRingPath` to get wrong for a new environment, which is exactly the class of mistake that
  caused this investigation.
- SQLite needs no equivalent migration: it never runs the PostgreSQL migration chain at all (see
  ADR-0006), and instead calls `Database.EnsureCreated()` from the live model at test-bootstrap
  time, which picks up `DataProtectionKeys` automatically since it's a `DbSet` on the shared base
  `ApplicationDbContext`.
- A future institution deploying this platform on infrastructure that *does* provide a persistent
  disk gains nothing extra from that disk for this concern — the database was already the right
  answer regardless of what the host provides, since every deployment here already depends on a
  database existing.
- If the database itself is ever rebuilt from scratch (not just redeployed), the key ring resets
  along with everything else in it — the same failure mode as before, just now correctly scoped to
  "the database was rebuilt" rather than "the container was redeployed," which is a much rarer event.
