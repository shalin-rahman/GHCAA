# Which seeds a new institution needs, and which it must not get

Written 2026-09-04, from the question: a new institution has no members, no events and no history, so
what should a fresh database actually contain after migrations run?

Figures below come from the repository as it stood on 2026-09-04. Seed row counts are the record counts
in `GHCAA.Infrastructure/Data/Seed/*.json`; because every seed is loaded through
`ApplicationDbContext.OnModelCreating` with `HasData(...)`, those counts are also what a fresh database
holds once the migration chain has been applied. PII counts are distinct literal values found by regex
across the eight migration files that carry `InsertData`.

## The mechanism, and why it is the problem

Every seed goes through `HasData`. `HasData` is not a runtime import — it is part of the EF model, so
each seed row is written into a migration as a literal `InsertData` value and committed to git. Eight
migrations carry seed data, about 4.3 MB of it:

| Migration | Size |
|---|---|
| `20260321153800_AddNewFeaturesAndThemeAnimations` | 1.3 MB |
| `20260323024044_UpdateAlumniEventDates` | 844 KB |
| `20260319073041_RefactorMemberAcademicProfessionalRecords` | 888 KB |
| `PgSql/20260329180202_AddNotificationPreferences` | 512 KB |
| `PgSql/20260828120117_AddMay2026AlumniRegistrationBatch` | 408 KB |
| `PgSql/20260521164117_AddDiscussionForums` | 292 KB |
| `PgSql/20260826173238_AddApprovalWorkflowToGalleryAndJobs` | 8 KB |
| `PgSql/20260802163432_AddSiteContentAndNoticeFields` | 8 KB |

Two consequences follow, and both are easy to miss:

**A new institution cannot avoid GHC's data.** `ApplicationDbContext.IsSeedDisabled` exists and the
tests use it, but it is evaluated in `OnModelCreating` — it controls what goes *into* a newly scaffolded
migration, not what an already-committed migration does. `ASP_SEED_PROFILE` has the same limit. Point a
brand-new empty database at this repo, run `dotnet ef database update`, and it comes up holding 631
Gournadi College alumni, their payment history and a 2023–2025 executive committee. Nothing in
configuration turns that off.

**Editing or anonymising the seed JSON does not remove the data from the repository.** Work Package
62.31 currently proposes anonymising or externalising `Seed/members.json`. That is necessary but not
sufficient on its own: the personal data is already literal text in committed migrations, and git
history keeps it regardless. See 82.31 for the part 62.31 does not reach.

## What is actually in the committed migrations

| | Distinct values in the chain |
|---|---|
| Email addresses | 612 |
| bcrypt password hashes | 631 |
| Bangladeshi mobile numbers | 638 |

The seeded `Members` insert carries these columns among others: `Email`, `MobileNo`, `NID`,
`DateOfBirth`, `FatherName`, `MotherName`, `PermanentAddress`, `PresentAddress`,
`EmergencyContactName`, `EmergencyContactPhone`, `BloodGroup`. 631 distinct bcrypt hashes is the whole
`Users` table — every real member's password hash is in git.

## Classification

### Tier 1 — Shared Reference Data. Every institution needs these, unchanged.

The application does not function without them; none of them names a person or an institution.

| Seed file | Rows | Why it is structural |
|---|---|---|
| `roles.json` | 3 | Authorisation depends on the role names existing |
| `lookups.json` | 100 | Dropdown vocabularies (districts, blood groups, occupations) |
| `email_templates.json` | 8 | Notification sending fails with no template rows |

`GamificationConfig` is also seeded, inline in `OnModelCreating` rather than from a file, and belongs
here.

### Tier 2 — Profile Seed Data. Structural shape, institution-specific content.

A new institution needs *a* row here, but not GHC's row. These are the seeds a profile pack should
supply, per Work Package 62.33 to 62.36.

| Seed file | Rows | What is GHC-specific about it |
|---|---|---|
| `fee_configs.json` | 5 | Amounts in BDT, tiers named for GHC's membership scheme |
| `payment_configurations.json` | 7 | bKash/Nagad/Rocket accounts belonging to GHC |
| `site_content.json` | 5 | About/contact text naming Gournadi College |
| `themes.json` | 2 | Bengali special-day themes |
| `constitution.json` | 1 | GHC's constitution v4.2, 38 KB of it |

### Tier 3 — Institution Data. Demonstration and history; a new institution should get none of it.

An empty table is the correct state for a new institution on day one. Every row here is a record of
something that happened at Gournadi College.

| Seed file | Rows |
|---|---|
| `members.json` | 631 |
| `users.json` | 631 |
| `user_roles.json` | 631 |
| `academic_records.json` | 630 |
| `professional_records.json` | 620 |
| `payment_histories.json` | 1,213 |
| `photos.json` | 27 |
| `ec_members.json` | 19 |
| `galleries.json` | 7 |
| `events.json` | 4 |
| `saved_payment_methods.json` | 2 |
| `jobs.json` | 2 |
| `news.json` | 1 |
| `ec_periods.json` | 1 |

### Tier 4 — Unused Seed. Already empty, nothing to do.

`file_uploads.json`, `financial_records.json`, `membership_dues.json` and
`membership_histories.json` are all empty arrays and already behave the way a new institution needs.

`financial_records.json` being empty is the same fact as Work Package 46.5: the org-wide ledger has no
rows even though per-member fees are recorded correctly.

## What a new institution's first administrator account looks like

There is no seeded system administrator in Tier 1, and that is deliberate. Admin accounts come from
`ProtectedSuperAdminSeeder`, which runs at boot and reads its list from `appsettings.json` — not from a
seed file and not from the database, so a new institution names its own first administrator in
configuration. The 631 rows in `users.json` are member logins, not administrators.

## Where this leaves the work

- **82.31** — remove the personal data from the committed migration chain. Not covered by 62.31, which
  addresses the seed file, nor by 62.32, which moves seed files between folders.
- **62.31** — its stated remedy needs amending: anonymising `members.json` alone leaves the data in git.
- **62.32** — the Tier 3 list above is the exact set of files it moves to `profiles/ghc/demo-data/`.
  It currently names most of them but misses `users.json` and `user_roles.json`, which are the two that
  carry the password hashes.
- **62.38** — the onboarding document it produces should say plainly that a new institution starts with
  Tier 1 and Tier 2 only, and that Tier 3 tables are empty on day one.
