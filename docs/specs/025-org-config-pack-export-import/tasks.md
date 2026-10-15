# Tasks: Export and import of the organisation profile and settings

**Input**: [spec.md](./spec.md) and [plan.md](./plan.md)
**Purpose**: The checklist for tracker items 62.56a to 62.56f in Work Package 62. Task ids map to
tracker items in brackets.

## Phase 1: Section map and readers

- [ ] T001 [62.56a] Add a `ConfigPack` group to `GHCAA.Domain/Constants.cs`: section ids, pack file
  names, `manifest.json`, format version 1, upload, unpacked and entry caps, and the export and
  import audit types.
- [ ] T002 [62.56a] Add the section registry and the section contract in
  `GHCAA.Infrastructure/Services/ConfigPack/`, with one entry per row of the spec's section map.
- [ ] T003 [62.56a] Write a reader per section that returns rows without `Id`, audit timestamps or
  admin ids, and blanks the secret fields named in the map.
- [ ] T004 [62.56a] Tests: each reader's output, secret blanking for payment settings and social
  sign-in, and that every section id in `Constants` has a registry entry.

## Phase 2: Export

- [ ] T005 [62.56b] Add `IConfigPackService` and the export DTOs in `GHCAA.Application`.
- [ ] T006 [62.56b] Build the zip with `System.IO.Compression`: the manifest, one file per chosen
  section, and branding assets under `assets/` with the URLs in `org-config.json` rewritten.
- [ ] T007 [62.56b] Add `ConfigPackController` with `GET api/admin/config-pack/export`,
  SuperAdminOnly, the API rate-limit policy, and an export audit row.
- [ ] T008 [62.56b] Tests: manifest contents, section filter, blanked secrets, asset packing and
  URL rewrite, the download name, and a 403 for an Admin who is not SuperAdmin.

## Phase 3: Upload reading

- [ ] T009 [62.56c] Read a zip, a single JSON file or a folder upload in memory into one list of
  pack entries with relative paths.
- [ ] T010 [62.56c] Refuse absolute paths, `..`, names outside the section files and `assets/`,
  and uploads past the caps. Check assets with `FileValidationService`. Refuse a manifest with a
  newer format version.
- [ ] T011 [62.56c] Tests with crafted uploads for each refusal, plus a `profiles/default/` folder
  and the `Data/Seed/` files read as valid packs.

## Phase 3a: Unique keys (FR-18)

Numbered after T032 so the earlier task numbers do not move. Phases 4 and 5 need this phase.

- [x] T033 [62.56g] Add unique indexes in the model configuration: `SpecialDayThemes (Title,
  StartDate)`, `MembershipFeeConfigs (Category, MembershipType, EffectiveDate)`,
  `PaymentConfigurations (Method, Gateway)`. Write the PgSql migration by hand with its Designer
  file and a matching snapshot change, not with `migrations add`.
- [x] T034 [62.56g] Before each `CREATE UNIQUE INDEX`, a `DO` block that looks for duplicates. If
  it finds any, it skips the index and raises a warning listing their Ids. It does not raise an
  exception, because that would stop start-up and lock the admin out of the fix. Idempotent
  `IF NOT EXISTS` on the index. Down drops the indexes only. Dry run on a local PostgreSQL, once
  clean and once with a planted duplicate.
- [x] T035 [62.56g] `ThemeService`, `FinancialService` (fee configs) and `PaymentConfigService`
  check the key on create and update, excluding the row being edited, and return a 400 with the
  reason. An update checks only when the key changes. The web pages leave the toast to the global
  interceptor. The mobile fee screen shows the reason in a snackbar.
- [x] T036 [62.56g] Tests for each service: duplicate on create, duplicate on update, editing a
  row without changing its key, and the seed data passing (payment seeds run against the index in
  `SeedDefaultsAsync_CreatesSixDefaultMethods`). Run code-reviewer on the migration.

## Phase 4: Preview

- [ ] T012 [62.56d] Match files to sections by manifest, then file name, then the admin's mapping.
  List `seo.json` as skipped (D-3).
- [ ] T013 [62.56d] Validate each row and match it on its natural key. Report new, matching,
  unchanged and failed counts, and fail a key that appears twice in the pack.
- [ ] T014 [62.56d] Add `POST api/admin/config-pack/preview`, returning the result and the upload
  hash. Tests for each count, a manual mapping, an unmatched file, and duplicate keys in the pack.

## Phase 5: Commit

- [ ] T015 [62.56e] Move the checks in `OrgConfigService.UpdateConfigAsync` into one apply step
  that does not save, and make `UpdateConfigAsync` call it. Add a public cache clear. Existing org
  config tests stay green.
- [ ] T016 [62.56e] Merge the pack org config onto the live one, keeping live values for missing
  properties, then run the apply step.
- [ ] T017 [62.56e] Write "add only" and "override" per section. Keep stored secrets when the pack
  value is empty. Refuse a mode the map does not allow.
- [ ] T018 [62.56e] Guards: persona override refused while an election is past nomination and not
  finalised. Constitution versions added inactive and never overwritten.
- [ ] T019 [62.56e] Store imported branding assets through the storage service and rewrite the
  `assets/` URLs. Fail the org config section if a referenced asset is missing.
- [ ] T020 [62.56e] Add `POST api/admin/config-pack/import`: re-run the preview checks, compare
  the hash, write every chosen section in one `BeginTransactionAsync`, add the audit row, commit,
  then clear the org config cache.
- [ ] T021 [62.56e] Tests: each mode for each section, rollback when one row fails, a changed hash,
  a frozen election rule, persona override during an open election, secret keeping, Ids assigned
  above seeded rows, and the round trip (export then import changes nothing).

## Phase 6: Web page

- [ ] T022 [62.56c] Add `ConfigPackService` in `core/services/` with export, preview and import
  calls, and the endpoint paths in the existing API constants.
- [ ] T023 [62.56d] Add the `admin/config-pack` page: Export tab with section checkboxes, Import
  tab with file and folder pickers, mapping table, mode per section, counts, failures and commit.
  Use the shared admin layout, table and notification components and the member import modal's
  file input. Say "zip only" by the picker (FR-17) and warn about assets on redeploy (FR-11).
- [ ] T024 [62.56d] Add the route behind `superAdminGuard` in `app.routes.ts` and the nav entry in
  `nav.service.ts` under "Finance & Tools".
- [ ] T025 [62.56d] Vitest specs for the mapping step, mode change and commit, then a browser check
  against the mocked API with a real zip.

## Phase 7: Docs and tags

- [ ] T026 [62.56f] FEATURES.md: the export and import feature under admin settings.
- [ ] T027 [62.56f] PROJECT_MAP.md: the controller, service folder, page and route.
- [ ] T028 [62.56f] Create `profiles/README.md` (none exists yet): the pack format, file names
  and manifest.
- [ ] T029 [62.56f] `docs/INSTITUTION_ONBOARDING.md`: a new install can import a pack instead of
  editing files and reseeding.
- [ ] T030 [62.56f] Tag every new test with its FR or NFR from the spec.
- [ ] T031 [62.56f] Set the spec status in the spec 001 index and in this spec's header.
- [ ] T032 [62.56f] Run `python scripts/ci-local.py` and `graphify update .`.
