# Seeding plan for a second institution (tracker item 84.7)

Written 2026-11-07. Status: **plan only, waiting for owner approval.** No code, migration or JSON file
changes until the owner signs off on the choices in "Owner gates" at the end.

The owner decided on 2026-11-02 that a second institution must be able to deploy. This plan says what
stands in the way in the seed data and how to remove it without touching GHC's live database. The tier
names (Tier 1 shared reference data, Tier 2 profile content, Tier 3 one institution's own records) come
from `docs/SEED_CLASSIFICATION.md`, which still holds the classification but whose migration figures
date from before the chain was squashed.

Every fact below was read from the tree on 2026-11-07. Where a behaviour is inferred from reading code
rather than seen in a run, the text says so.

## 1. Where things stand

### 1.1 The migration chain

`GHCAA.Infrastructure/Data/Migrations/PgSql/` holds 36 migrations. Only three insert data:

| Migration | What it inserts |
|---|---|
| `20260907193705_InitialBaseline` (1.28 MB) | Schema plus data for 23 tables, listed in 1.2 |
| `20260923194200_SeedEcPositionLookup` | `Lookups` rows for EC positions, by `InsertData`, no sequence reset |
| `20261105120000_SeedJobCategoryLookups` | `Lookups` rows for job categories, by raw SQL with a sequence reset and a content-keyed `Down` |

`20260916131245_RemoveInstitutionSeedData` is a deliberate no-op. It exists so the model snapshot stops
carrying the Tier 3 rows after they left `HasData`. It deletes nothing, on purpose, because on GHC's
database those rows are live data.

### 1.2 GHC rows inside InitialBaseline

`InitialBaseline` carries 613 distinct email addresses and 631 distinct bcrypt hashes, so every GHC
member's personal record and password hash is in the committed baseline (tracker item 82.31, on hold
since 2026-09-06).

| Tier | Tables with rows in InitialBaseline |
|---|---|
| Tier 3, GHC personal or historical data | Members, Users, UserRoles, AcademicRecords, ProfessionalRecords, PaymentHistories, SavedPaymentMethods, ECPeriods, ECMembers, AlumniEvents, EventGalleries, EventPhotos, NewsPosts, JobOpportunities |
| Tier 2, GHC content in a shared shape | MembershipFeeConfigs, PaymentConfigurations, SiteContents, SpecialDayThemes, Constitutions, EmailTemplates |
| Tier 1, shared reference data | Lookups, Roles, GamificationConfigs |

### 1.3 What a fresh PostgreSQL database gets today

Read from the code, not run:

1. `MigrateAsync` applies `InitialBaseline`, which inserts all of 1.2, GHC's 631 members included.
2. `InstitutionDataSeeder.SyncAsync` then finds every Tier 3 table already populated and skips it.

So a second institution that deploys today gets GHC's members, accounts and history. The runtime
seeder and the profile packs already exist; the baseline gets there first. This is the blocker.

### 1.4 How a profile picks its seed

Two environment variables and one setting decide which file a seed comes from.

- `ASP_SEED_PROFILE=Visual` (outside `dotnet ef`) sends every `LoadSeed<T>` call to `Data/Seed/Visual/`.
  That folder is end-to-end test data and is out of scope here.
- `ORG_PROFILE` (empty means `default`) picks the profile pack under `profiles/<name>/`. Each lookup tries
  that profile first, then `default`, then `Data/Seed/`.
  - Tier 3 files (the `DemoDataFiles` set in `ApplicationDbContext`) resolve under
    `profiles/<name>/demo-data/`.
  - Four Tier 2 files (`ProfilePackFiles`: email templates, site content, themes, constitution) resolve
    under `profiles/<name>/`. No profile ships those four files under their seed names yet, so they still
    come from `Data/Seed/`.
  - `lookups.json` always comes from `Data/Seed/`. It is shared taxonomy, the same for every institution.
- `AppSettings:RealDataPath` (`ConfigKeys.RealDataPath`) names a folder outside git. A Tier 3 file found
  there wins over the profile's demo-data copy.

Tier 1 and Tier 2 rows reach the database through `HasData` in `OnModelCreating`, so they are frozen into
migrations at scaffold time. Tier 3 rows reach it at boot through `InstitutionDataSeeder`, one table at a
time, only while the table is empty.

Row counts on 2026-11-07 (records per file):

| File | profiles/default/demo-data | profiles/ghc/demo-data |
|---|---|---|
| members, users | 4, 4 | 631, 631 |
| user_roles | 6 | 631 |
| academic_records, professional_records | 5, 4 | 630, 620 |
| payment_histories | 4 | 1213 |
| ec_periods, ec_members | 1, 4 | 1, 19 |
| events, galleries, photos, news | 2, 2, 3, 2 | 4, 7, 27, 1 |

Neither profile has `jobs.json` or `saved_payment_methods.json`, so both still fall back to the copies in
`Data/Seed/`.

### 1.5 Smaller faults found while reading

These are real but separate from the blocker. Each is fixed inside the build steps in section 3.

- **F1. The Tier 3 seeder is PostgreSQL-only.** `ResetIdentitySequenceAsync` runs `setval` with no
  provider check. On SQLite with a profile other than Visual, the first table seeds, the `setval` call
  throws, and the bootstrapper's catch logs "Institution data seed skipped". Every later table stays
  empty. (Inferred from the code; local SQLite runs normally use Visual, which hides it.)
- **F2. Explicit-Id inserts in the baseline leave sequences behind.** `InitialBaseline` and
  `SeedEcPositionLookup` insert rows with fixed Ids and never call `setval`. Their 82 identity columns
  are `IdentityByDefault`, so on a fresh database the sequence still starts at 1. The first row an admin
  adds to such a table (a new lookup, role, theme or fee rule) can fail with a duplicate-key error until
  the sequence passes the seeded Ids. GHC's live database is not affected in practice, since it has
  been written to for months. To confirm on a scratch database before the build.
- **F3. Dead demo files.** `profiles/default/demo-data/` holds `financial_records.json`,
  `membership_dues.json` and `membership_histories.json`. None is in `DemoDataFiles` or the seeder's
  registry, so they are never loaded, and the seeder warns about them on every boot.
- **F4. `docs/SEED_CLASSIFICATION.md` is stale.** It counts PII across eight migrations that are now one
  baseline, and says every seed goes through `HasData`, which stopped being true for Tier 3.

## 2. Options for the baseline

**A. Strip the GHC data out of `InitialBaseline` (recommended).**
Remove the Tier 3 `InsertData` calls, and the Tier 2 calls covered by step 3.3, from
`InitialBaseline.cs` and the matching `HasData` blocks from `InitialBaseline.Designer.cs`. Schema stays
identical.
- Effect on GHC: none. EF never re-runs an applied migration, and the migration Id does not change, so
  preprod and any production database see no difference. `MigrationBootstrapper`'s history-repair
  checks look at tables and columns, not data.
- Effect on a new institution: the baseline creates empty Tier 3 tables, and `InstitutionDataSeeder`
  fills them from that institution's profile pack or `RealDataPath`.
- Cost: editing an applied migration is normally off limits. It is safe here only because the data is
  the sole change and every live database already has the migration. That reasoning has to be written
  into the migration as a comment.
- Limit: the PII stays in git history. Removing it from history is a rewrite of published history, which
  belongs to the owner as Git owner (82.31).

**B. Keep the baseline and delete the GHC rows in a later migration.** Rejected. A migration cannot tell
GHC's database from a new one without reading the environment, and on GHC's database it would delete
live members.

**C. Delete the GHC rows at boot for any profile other than `ghc`.** Rejected. It matches rows by content
from a profile that a new institution would not have, it leaves the PII in the repository, and a
mistyped `ORG_PROFILE` on GHC's own server would delete live data.

## 3. Build steps (for after approval)

Each step names its check. Steps 3.1 to 3.3 go in one change, since the snapshot, the Designer file and
`OnModelCreating` must agree.

**3.1 Fix the Tier 3 seeder first (F1).** Guard `ResetIdentitySequenceAsync` with
`Database.IsNpgsql()`. SQLite does not need a reset: an `INTEGER PRIMARY KEY` takes the next value after
the largest one in the table.
Check: a unit test seeds the default profile into SQLite and asserts every registry table has rows.

**3.2 Strip `InitialBaseline` (option A).** Remove the Tier 3 `InsertData` calls from the migration and
the matching `HasData` from its Designer file.
Check: `dotnet ef migrations has-pending-model-changes` reports none. A fresh PostgreSQL database built
from the chain has zero rows in every Tier 3 table before the seeder runs, and the profile's counts
after it (1.4).

**3.3 Move Tier 2 GHC content to the profile packs.** Fee rules, payment configuration, site content,
themes, constitution and email templates are institution content in a shared shape.
- Give them a runtime seed-if-empty path like `InstitutionDataSeeder` (or extend its registry), drop
  their `HasData`, and add a no-op migration in the style of `RemoveInstitutionSeedData` so the
  snapshot stops carrying them.
- Copy the current files into `profiles/ghc/` under the seed names that `ProfilePackFiles` expects, and
  write neutral versions for `profiles/default/`. Moving JSON data needs the owner's approval
  (owner gate G3).
- `ConstitutionSeeder` already runs at boot; check that it and the new path do not both insert.
Check: on GHC's database every Tier 2 table has the same rows before and after a boot. On a fresh
database with `ORG_PROFILE=default` they hold the neutral rows.

**3.4 Reset sequences after every explicit-Id insert (F2).** After any insert with fixed Ids, in a
migration or at boot, run on PostgreSQL:

```sql
SELECT setval(pg_get_serial_sequence('"<Table>"', 'Id'),
              GREATEST((SELECT MAX("Id") FROM "<Table>"), 1));
```

This is the pattern `SeedJobCategoryLookups` already uses. For the Tier 1 rows that stay in the
baseline (Lookups, Roles, GamificationConfigs, EcPositions), add one new migration that runs this
statement for each of those tables. It is idempotent and harmless on GHC's database, where `MAX("Id")`
is already below the sequence. Skip it on SQLite, as in 3.1.
Check: on a fresh PostgreSQL database, inserting a new lookup, role and theme through the admin API
succeeds first time.

**3.5 Clean up the dead files (F3) and refresh `docs/SEED_CLASSIFICATION.md` (F4).** Removing the three
default demo files is a JSON deletion and needs the owner (G3). Update the classification doc's figures
and point it at this plan.
Check: no "not in InstitutionDataSeeder.Registry" warning on a default-profile boot.

**3.6 Prove it on a new institution.** Run a scratch PostgreSQL database with `ORG_PROFILE=default` and no
`RealDataPath`.
Check: no GHC email address appears in any table (search for the GHC email domain), and the first
administrator can sign in through the SuperAdmin bootstrap.

## 4. Rollback

- **Runtime seeders never delete.** They insert only into empty tables, so turning one off or reverting
  it leaves the data where it is.
- **A data migration rolls back by content, never by Id range.** Its `Down` deletes rows matched on a
  natural key taken from the same list `Up` inserted, as `SeedJobCategoryLookups.Down` does with
  `LookupGroup` plus `Value`. An Id range can match rows an admin added later on a live database. The
  natural key for each table is picked and written down in the migration when it is built.
- **Option A has no database rollback** because it changes no database. Rolling it back means reverting
  the edit to `InitialBaseline.cs` and its Designer file. A new institution's database built in between
  keeps its own data.
- **The sequence-reset migration (3.4)** has an empty `Down`. A sequence that was moved forward is never
  moved back, since that would bring the duplicate-key fault back.

## 5. GHCAA.Tools/TestData

The six files (`test-academic_records`, `test-members`, `test-payment_histories`,
`test-professional_records`, `test-user_roles`, `test-users`) are not read by any code, script or project
file on 2026-11-07. Only `docs/TODO.md` and `GHCAA.Tools/README.md` mention them, and the README says the
scripts that used them were removed. This plan does not need them: the default profile's demo data and
`Data/Seed/Visual/` cover a new install and the end-to-end tests. Recommendation: delete them, with the
owner's approval (G3), and close the open decision in tracker item 99.8.

## 6. What must not change for GHC

- No migration deletes or rewrites rows in GHC's database. The only new migrations are the snapshot
  no-op from 3.3 and the sequence reset from 3.4.
- `profiles/ghc/demo-data/` is not edited. It stays GHC's fresh-install source.
- GHC's servers keep `ORG_PROFILE=ghc`. Check that preprod sets it before 3.3 ships, since after 3.3 the
  `default` pack would supply Tier 2 content to an empty table.

## 7. Cross-provider notes

- PostgreSQL is production and preprod; SQLite is local and test. Every raw SQL statement in 3.1 and 3.4
  is behind `IsNpgsql()`.
- The `Visual` migration folder and `Data/Seed/Visual/` are left as they are.
- The fresh-database checks in 3.2, 3.4 and 3.6 run against PostgreSQL. SQLite alone would hide F1 and F2.

## 8. Owner gates

- **G1. Option A.** Approve editing the data out of the applied `InitialBaseline` migration, on the
  reasoning in section 2.
- **G2. Git history.** Whether GHC's PII is also removed from git history (82.31). That is a history
  rewrite and the owner's operation.
- **G3. JSON moves and deletions.** Approve copying the Tier 2 files into `profiles/ghc/`, writing neutral
  `profiles/default/` copies, deleting the three dead default demo files, and deleting
  `GHCAA.Tools/TestData/`.
- **G4. Order.** Whether 3.1 (the SQLite seeder fault) and 3.4 (sequence resets) may ship ahead of the
  rest, since neither depends on G1 to G3.
