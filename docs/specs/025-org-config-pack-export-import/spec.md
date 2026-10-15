# Feature Specification: Export and import of the organisation profile and settings

**Feature Branch**: 025-org-config-pack-export-import
**Created**: 2026-10-15
**Status**: Draft, not started
**Input**: Owner request of 2026-10-15: "add a tasks for export and import organisational profile
data like seed, ability to override and add only, also ashould able to map what are going import
or export, a valid json directory or zip/rar could be imported also export zip/rar", extended the
same day with "including organisational settings, and other configurations".
**Depends on**: [001-platform-baseline](../001-platform-baseline/spec.md) (the as-built platform),
and tracker 62.2 in `docs/TODO.md`, which put each profile in `profiles/<name>/`
**Tracker**: `docs/TODO.md` items 62.56a to 62.56g, under Work Package 62 (white-label), Phase E

## Purpose and scope

A profile lives in two places today. `profiles/<name>/` holds `org-config.json`, `seo.json`,
`site-content.json`, `constitution.json`, `assets/` and `demo-data/`. `GHCAA.Infrastructure/Data/Seed/`
holds the rest of the seed JSON. Both are read only when the database is created. Seed JSON never
touches an existing database, so a running site cannot take a new profile, and the only way to copy
one install's settings to another is to copy files by hand and reseed an empty database.

This spec adds a SuperAdmin page and API that export the live settings as one pack and import a
pack back into a running site. The admin picks which sections go out or come in, sees a dry run
before anything is written, and picks "add only" or "override" for each section.

In scope: the settings stores listed in the section map below, a zip pack with a manifest, import
from a zip, a single JSON file or a folder of JSON files, a mapping and preview step, two write
modes, one transaction per import, an audit entry, and cache clearing.

Out of scope:

- Members, users, roles, saved payment methods, payments, elections, votes, messages and any other
  record that holds personal data or history. The pack is configuration only.
- `appsettings*.json`, environment variables, connection strings and API keys held in
  configuration. Nothing outside the database and the profile folder is read or written.
- Deleting rows. Neither mode removes a row that the pack does not carry.
- The mobile client. It has no admin settings screens. It reads the org config read-only and picks
  up an imported config when its own cache expires (tracker 28.18). The `OrgConfigDto` shape does
  not change, so mobile needs no release.
- `demo-data/`. It is test content for a fresh install and is loaded by the existing demo seeder.
- A scheduled or automatic export. The admin starts each export by hand.

## Background the design relies on

- `ApplicationDbContext.ProfilePackFiles` lists the four seed files a profile may override, and
  `ResolveProfilePackPath` looks in `profiles/<ORG_PROFILE>/`, then `profiles/default/`, then
  `Data/Seed/`. Pack file names in this spec match those seed file names, so a seed folder or a
  profile folder is a valid pack.
- Seed rows go in through `HasData` with explicit Ids. The importer never writes an Id. Rows are
  matched on a natural key and inserted with a database-assigned Id, so the PostgreSQL identity
  sequences never fall behind.
- The runtime database is PostgreSQL behind `TransactionAwareRetryStrategy`, which retries nothing
  inside a user transaction. The import opens one plain `BeginTransactionAsync`, as
  `ScholarshipService` and `GovernanceService` do, and is not wrapped in an execution strategy.
  Tests run on SQLite, so the importer uses no provider-specific SQL.
- `OrgConfigService` caches the config under `org_config_v1` for ten minutes, and
  `UpdateConfigAsync` normalises the date format and email provider, refuses a change to frozen
  election rules, writes its own `ActivityLog` row and clears the cache. The org config section
  goes through the same rules.
- There is no zip handling in the backend yet. `System.IO.Compression` ships with .NET, so zip
  needs no new package.
- Spec 008 FR-6 describes a dry-run preview for the member import that was never built. This spec
  builds its own preview and does not depend on spec 008.

## Section map

One list in the backend (FR-1) holds every row of this table. Export and import both read it, so a
new section is one entry plus its tests.

| Section | Pack file | Store | Natural key | Secret fields | Default mode | Notes |
|---|---|---|---|---|---|---|
| Org config | `org-config.json` | `OrganizationConfigs.ConfigJson` (one row) | the single row | none | Override | Merged onto the live config, see FR-9 |
| Site content | `site_content.json` | `SiteContents` | `Key` | none | Add only | Array shape of `Data/Seed/site_content.json`, not the flat build-time `site-content.json` |
| Email templates | `email_templates.json` | `EmailTemplates` | `Code` | none | Add only | |
| Themes | `themes.json` | `SpecialDayThemes` | `Title` + `StartDate` | none | Add only | Unique index added by FR-18 |
| Lookups | `lookups.json` | `Lookups` | `LookupGroup` + `Value` | none | Add only | Override changes label, order and active flag only |
| Fee configs | `fee_configs.json` | `MembershipFeeConfigs` | `Category` + `MembershipType` + `EffectiveDate` | none | Add only | Unique index added by FR-18 |
| Payment settings | `payment_configurations.json` | `PaymentConfigurations` | `Method` + `Gateway` | `GatewaySecretKey` | Add only | Unique index added by FR-18. `Method` alone repeats: the seed has two Credit Card rows, SSLCommerz and DGePay. Wallet and bank numbers are shown to members already and go out as they are |
| Social sign-in | `social_auth.json` | `SocialAuthConfigs` | `Provider` | `ClientSecret` | Add only | |
| Election personas | `election_personas.json` | `ElectionPersonas` | `Name` | none | Add only | Override refused while an election is open, see FR-10 |
| Gamification | `gamification.json` | `GamificationConfigs` | `ActivityCode` | none | Add only | |
| Constitution | `constitution.json` | `Constitutions` | `Version` | none | Add only, fixed | Imported versions come in inactive. Activation stays in the governance flow |
| SEO | `seo.json` | `profiles/<name>/seo.json` on disk | the file | none | Export only | Read by `apply-brand.mjs` at build time. An import cannot change a running site, so the preview lists it as skipped |
| Branding assets | `assets/*` | files named by the Branding URLs in the org config | file name | none | Add only | See FR-11 |

The section ids and file names go in `Constants` (FR-1), not as literals in the services.

## Pack format

```
manifest.json
org-config.json
site_content.json
...
assets/logo.png
```

`manifest.json` holds the pack format version (starting at 1), the app version, the export time in
UTC, the `ORG_PROFILE` name, and one entry per section with its file and row count. A pack without
a manifest is accepted, so an existing `profiles/<name>/` folder or `Data/Seed/` folder can be
imported. Sections are then matched by file name. A manifest with a higher format version than the
server knows is refused.

Rows carry the entity fields without `Id`, audit timestamps or the admin id that last changed
them. An `Id` present in a file (as in the seed JSON) is ignored.

## Functional requirements

- **FR-1 Section map.** One registry in the Infrastructure layer lists each section with its id,
  pack file, natural key, secret fields, allowed modes and a reader and writer. Section ids and
  file names live in `Constants`. Adding a section needs no change to the controller or the page.
- **FR-2 Export.** `GET /api/admin/config-pack/export?sections=...` returns one `.zip` with the
  manifest, one JSON file per chosen section and, if chosen, the branding assets. With no
  `sections` value every section is included. The download is named
  `<org-profile>-config-<yyyyMMdd-HHmm>.zip`.
- **FR-3 Secrets on export.** Every secret field in the map is written as an empty string. The
  manifest says which fields were blanked.
- **FR-4 Import sources.** The import accepts a `.zip`, a single `.json` file, or several files
  picked as a folder in the browser (sent as one multipart request with their relative paths). A
  single file or a folder is treated the same as an unpacked zip.
- **FR-5 Upload safety.** The server reads the upload in memory and never extracts it to disk. It
  refuses an entry whose path is absolute, contains `..`, or falls outside the allowed file names
  and `assets/`. It caps the upload size, the unpacked size and the number of entries, with the
  limits in `Constants`. Asset entries must pass `FileValidationService` for images. Any refusal
  stops the import before the preview.
- **FR-6 Mapping and preview.** `POST /api/admin/config-pack/preview` takes the upload and returns,
  for each file, the section it matched (by manifest, then by file name, or none), and for each
  section the counts of new rows, rows that match a live key, unchanged rows, and rows that fail
  validation, with each failure's row number and reason. Nothing is written. The page lets the
  admin untick a section, point an unmatched file at a section, and pick the mode.
- **FR-7 Key matching.** Rows are matched on the natural key from the map. Strings compare after
  trimming, and case-insensitively where the store's unique index is. Every section's key is
  backed by a unique index (FR-18), so the live table holds at most one match. Two pack rows with
  the same key fail as "duplicate key" and neither is written.
- **FR-8 Modes.** "Add only" inserts rows whose key is new and leaves matching rows alone.
  "Override" also updates matching rows from the pack. Neither mode deletes. A mode the map does
  not allow for a section is refused.
- **FR-9 Org config merge.** The pack's org config is merged onto the live config. A property
  missing from the pack keeps the live value, so an older pack does not reset newer settings to
  their defaults. The merged config then goes through the same checks as `UpdateConfigAsync`:
  date format and email provider normalisation, and the frozen election rules. A refused change
  fails the section.
- **FR-10 Guarded sections.** Override of election personas is refused while any election is past
  nomination and not finalised, because persona permissions apply to the running election.
  Constitution versions are always added inactive and never overwrite an existing version.
- **FR-11 Branding assets.** On export, each Branding URL that points at a local upload is packed
  under `assets/` and the URL in `org-config.json` is rewritten to `assets/<file>`. On import, each
  asset is stored through the existing storage service under a branding folder, and the
  `assets/<file>` URLs in the merged config are rewritten to the stored paths. An asset that is
  referenced but missing fails the org config section. Uploads on Render are wiped on redeploy
  (memory note `project_uploads_ephemeral_storage`), so the page says assets must be re-imported
  after a redeploy until that is fixed.
- **FR-12 Secrets on import.** An empty secret in the pack keeps the stored value. A non-empty
  secret in the pack is written only in Override mode.
- **FR-13 Commit.** `POST /api/admin/config-pack/import` takes the same upload plus the admin's
  mapping and modes, repeats the checks of FR-5 to FR-7, and writes every chosen section in one
  transaction. Any failed row in a chosen section rolls back the whole import. The request carries
  the hash returned by the preview, and a different upload is refused.
- **FR-14 Audit.** The import writes one `ActivityLog` row inside the same transaction, naming the
  sections, modes and counts. The export writes one row naming the sections. Both use new audit
  type constants.
- **FR-15 Cache.** After the transaction commits, the org config cache is cleared through a public
  method on `OrgConfigService`, so the new values show without a restart. Nothing is cleared when
  the import rolls back.
- **FR-16 Access.** Every endpoint uses `Constants.Policies.SuperAdminOnly` and the API rate-limit
  policy. The page sits under the SuperAdmin admin routes, behind `superAdminGuard`, with a nav
  entry in the "Finance & Tools" section.
- **FR-17 Zip only.** Export and import are zip only (D-1). The page says so next to the file
  picker.
- **FR-18 Unique keys.** The owner ruled on 2026-10-15 that themes, fee configs and payment
  settings must have unique keys. One PgSql migration, with its Designer file, adds unique indexes
  on `SpecialDayThemes (Title, StartDate)`, `MembershipFeeConfigs (Category, MembershipType,
  EffectiveDate)` and `PaymentConfigurations (Method, Gateway)`. Before creating each index the
  migration checks for existing duplicates. If it finds any, it skips that index and logs a
  warning naming the Ids, and start-up carries on. Stopping would keep the API down and lock the
  admin out of the page that removes the extra row. It never deletes or merges rows itself. Once
  the admin has cleaned the data, the skipped index is created by hand with the
  `CREATE UNIQUE INDEX` statement in the migration. `ThemeService`,
  `FinancialService` (fee configs) and `PaymentConfigService` check the key on create and update and return a validation
  error instead of letting the database throw.

## Non-functional requirements

- **NFR-S1** No secret leaves the server in an export, and no secret is cleared by an import.
- **NFR-S2** A crafted upload cannot write outside the store paths, exhaust memory past the caps,
  or reach a store that is not in the section map.
- **NFR-R1** An import either applies every chosen section or none of them.
- **NFR-R2** Exporting a site and importing that pack into the same site in either mode changes
  no row (round trip).
- **NFR-P1** A full export or import of today's data finishes inside the default request timeout.
- **NFR-M1** A new section is one map entry, one natural-key test and one round-trip test.
- **NFR-U1** The page uses the shared admin layout, table and notification components, and the
  existing file-input pattern of the member import modal. No new UI library.

## Decisions

| Id | Question | Proposed answer | Status |
|---|---|---|---|
| D-1 | Read `.rar` on import? | Needs SharpCompress (MIT), a new package. Writing RAR is not possible without the licensed WinRAR tool, so export stays zip either way | Settled 2026-10-15: zip only, no new package |
| D-2 | Store the upload between preview and commit, or upload twice? | Upload twice and compare a hash. Render's disk is wiped on redeploy and the API may run more than one instance | Proposed |
| D-3 | Import `seo.json` to disk? | No. It only takes effect at the next web build, so the import lists it as skipped | Proposed |
| D-4 | Default mode per section | As in the section map: org config Override, the rest Add only | Proposed |

## Traceability

| Requirement | Tracker item | Tasks |
|---|---|---|
| FR-18 | 62.56g | T033 to T036 |
| FR-1 | 62.56a | T001 to T004 |
| FR-2, FR-3, FR-11 (export) | 62.56b | T005 to T008 |
| FR-4, FR-5, FR-17 | 62.56c | T009 to T011 |
| FR-6, FR-7 | 62.56d | T012 to T014 |
| FR-8 to FR-10, FR-11 (import), FR-12 to FR-15 | 62.56e | T015 to T021 |
| FR-16, NFR-U1 | 62.56c, 62.56d | T022 to T025 |
| NFR-S1 to NFR-M1 | 62.56f | T026 to T032 |

## Related work

- Tracker 62.2 (profile folders) and 62.6 (`OrgConfigService` reads the pack) made the profile a
  set of files this spec can export and import.
- Tracker 62.6b deletes the hardcoded `BuildGhcaaDefaults`. It does not block this spec, but once
  it lands an imported org config is the only way to move GHC's settings between installs without
  editing `profiles/ghc/org-config.json`.
- Tracker 62.4, the golden org config test, guards the `OrgConfigDto` shape this spec reads and
  writes.
- [Spec 008](../008-idempotent-member-batch-import/spec.md), the member batch import, uses the same
  preview-then-commit idea for member rows. The two do not share code.
- [Spec 020](../020-platform-operations/spec.md), platform operations, covers the org config editor
  and admin settings pages this spec exports from.
- `docs/SEED_CLASSIFICATION.md` splits seed data into tiers. The pack carries tier 2 (profile) and
  the admin-editable part of tier 1. It never carries tier 3 demo data.

## Known limits

The section map comes from a read of the code on 2026-10-15. The duplicate check behind FR-18 was
run against the seed JSON only, which has none on the new keys. Preprod and production data have
not been checked. There the migration's check only warns, so a skipped index shows up as a
warning in the start-up log, and the service checks still refuse new duplicates.
