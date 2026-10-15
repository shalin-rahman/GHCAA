# Implementation Plan: Export and import of the organisation profile and settings

**Spec**: [spec.md](./spec.md)
**Tracker**: `docs/TODO.md` items 62.56a to 62.56f, Work Package 62

## Shape

Backend, in the existing layers:

- `GHCAA.Domain/Constants.cs`: a `ConfigPack` group with the section ids, pack file names, manifest
  file name, format version, size and entry caps, and the two audit types.
- `GHCAA.Application/DTOs/ConfigPack/`: the manifest, the preview result (files, sections, counts,
  failures) and the import request (file-to-section mapping, mode per section, preview hash).
- `GHCAA.Application/Interfaces/IConfigPackService.cs`: `ExportAsync`, `PreviewAsync`,
  `ImportAsync`.
- `GHCAA.Infrastructure/Services/ConfigPack/`: the section registry, one small section class per
  row of the section map, the zip reader with the FR-5 checks, and `ConfigPackService`.
- `GHCAA.API/Controllers/ConfigPackController.cs`: `api/admin/config-pack`, SuperAdminOnly, three
  actions. No `DbContext` in the controller.
- `OrgConfigService` gets a public cache clear and an internal apply step that runs the
  `UpdateConfigAsync` checks without its own `SaveChanges`, so the import can call it inside its
  transaction. `UpdateConfigAsync` is changed to call the same step, so the rules live in one place.

Web:

- `GHCAA.Web/src/app/admin/config-pack/`: one standalone page with an Export tab (section
  checkboxes, download) and an Import tab (file or folder picker, mapping table, mode per section,
  preview counts and failures, commit).
- A `ConfigPackService` in `core/services/`, a route under the SuperAdmin admin routes in
  `app.routes.ts`, and a nav entry in `nav.service.ts` under "Finance & Tools".

Mobile: no change (spec, Out of scope).

## Order

Each phase ends with `python scripts/ci-local.py` green and `graphify update .`.

1. **Section map and readers (62.56a).** Constants, the registry, and a reader per section that
   returns the rows without Ids or audit fields and with secrets blanked. Unit tests per section.
   No endpoint yet.
2. **Export (62.56b).** Zip writer with the manifest, branding asset packing, the export endpoint
   and its audit row. Tests open the zip and check the manifest, the blanked secrets and the asset
   URL rewrite.
3. **Upload reading (62.56c).** The in-memory reader for zip, single JSON and folder uploads, with
   the path, size, count and asset type checks. Tests use crafted zips: `..` entries, absolute
   paths, unknown files, too many entries, an oversized entry, a non-image asset.
3a. **Unique keys (62.56g).** One PgSql migration with its Designer file adds the three unique
   indexes from FR-18. Each index is preceded by a duplicate check. If it finds any, it skips the
   index and raises a warning naming the rows. Nothing is deleted, and start-up is not stopped.
   `ThemeService`, `FinancialService` and `PaymentConfigService` refuse a duplicate key on create
   and update. Tests per service, plus a migration dry run on a
   local PostgreSQL with and without seeded duplicates. code-reviewer runs on the migration.
   Phases 4 and 5 need this phase.
4. **Preview (62.56d).** File-to-section matching, per-row validation, natural-key matching
   backed by the FR-18 indexes, and the counts. The preview endpoint returns the hash of the upload.
   Tests cover each count, a manual mapping, and duplicates on both sides.
5. **Commit (62.56e).** Modes, the org config merge through the shared apply step, guarded
   sections, secret keeping, asset storing, one transaction, the audit row, and the cache clear
   after commit. Tests cover each mode per section, rollback on a failed row, a changed hash, a
   frozen election rule, an open election blocking persona override, and the round trip.
6. **Web page (62.56c, 62.56d).** Built on the shared admin layout, table, notification and the
   member import modal's file input. Vitest specs for the mapping step and the commit call. Browser
   check against the mocked API with a real zip.
7. **Docs (62.56f).** FEATURES.md, PROJECT_MAP.md, a new `profiles/README.md` with the pack format,
   a line in `docs/INSTITUTION_ONBOARDING.md`, and the spec 001 index status. FR and NFR tags on
   every new test.

Phases 1 to 5 are backend only and can ship before the page. Phase 6 needs 2, 4 and 5.

## Risks and how the plan handles them

- **Identity sequences.** The importer never writes an Id (spec, Pack format), so the PostgreSQL
  sequences stay ahead of the data. A test inserts through the importer after a seeded row and
  checks the new Id is higher.
- **Retries inside the transaction.** `TransactionAwareRetryStrategy` retries nothing inside a
  user transaction. The import runs once and reports a failure to the admin, who can run it again.
- **Older packs resetting settings.** The org config merge keeps live values for missing
  properties (FR-9), and a test imports a pack with a property removed. See memory note
  `gotcha_orgconfig_feature_flag_default_drift` for why a plain deserialise is wrong.
- **Ephemeral uploads.** Branding assets imported on Render go away on redeploy. The page says so.
  The real fix belongs to the uploads storage item, not here.
- **Live duplicates leave an FR-18 index missing.** Preprod or production may already hold two
  themes, fee configs or payment rows with the same key. The migration warns with their Ids and
  skips that index instead of picking a row. It is still recorded as applied, so after the admin
  removes the extra row the index has to be created by hand. Look for the warning in the start-up
  log after the first deploy, or run the duplicate queries against preprod before it.
- **Payment key.** `Method` alone is not unique: Credit Card has SSLCommerz and DGePay rows. The
  key is `Method` + `Gateway`.

## Decisions to confirm before the phase that needs them

- D-1: settled 2026-10-15, zip only. No new package.
- D-2 and D-3, before phase 4: upload twice with a hash, and `seo.json` export-only. Both are
  proposed in the spec and change nothing if confirmed.
- D-4, before phase 5: default modes per section.
