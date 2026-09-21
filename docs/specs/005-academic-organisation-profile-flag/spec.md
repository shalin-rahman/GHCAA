# Academic organisation-profile flag

## Purpose

Replace the institution-specific `IsGHC` name with `IsOrgProfile` across the
academic-record contract. The flag keeps its existing boolean meaning and
stored values. The new name describes the profile-selected organisation rather
than one institution.

## Actors and boundaries

- A registering member supplies the first institutional academic record.
- A member edits academic history after approval.
- An administrator reviews, imports, searches, and communicates with members.
- The API remains the contract boundary for Angular Web and Flutter Mobile.
- PostgreSQL stores the flag on `AcademicRecords`.

## Requirements

### FR-1 Canonical name

The Domain model, Application DTO, Infrastructure mappings and business rules
shall use `IsOrgProfile` as the CLR property name. Client models, forms and
payloads shall use `isOrgProfile` or `IsOrgProfile` according to the existing
JSON and multipart conventions.

### FR-2 Behaviour preservation

The rename shall not change the meaning of `true` or `false`. Registration
shall still require the first academic record to be the institutional record.
Profile editing, member search, directory summaries, imports, communication
targeting, and batch-related projections shall apply the same predicates as
before.

### FR-3 Data migration

The PostgreSQL migration shall rename `AcademicRecords.IsGHC` to
`AcademicRecords.IsOrgProfile`. It shall not copy, recalculate, delete, or
default existing rows. Its down migration shall restore the old column name.

### FR-4 Client compatibility

New Web and Mobile builds shall send and read the canonical field. The API
shall accept the legacy `isGHC` JSON property during client rollout without
emitting it in new responses. Legacy `true` and `false` values shall map to
`IsOrgProfile` unchanged.

### FR-5 Test evidence

Tests shall cover the validator's first-record rule, canonical DTO mapping,
legacy input compatibility, persisted column rename, Web registration/profile
models, and Mobile registration/profile payloads. Existing branches for both
boolean values shall remain covered.

## Non-functional and technology assessment

No new runtime technology, package, database provider, or messaging service is
needed. The change uses the existing ASP.NET Core 9, EF Core/Npgsql,
PostgreSQL, Angular 21, Flutter/Riverpod, and current test runners.

The relevant NFRs are backward-compatible API rollout, data integrity,
cross-client contract parity, type safety, and migration reversibility.
Specification-driven development is needed for the contract and migration
because the field crosses all product clients. The dissertation and project
technology sections only need an evidence update if they describe the old
field name; no new NFR technology is introduced by this change.

## Acceptance scenarios

1. **Given** an existing row with `IsGHC = true`, **when** the migration runs,
   **then** the row has `IsOrgProfile = true`.
2. **Given** an existing row with `IsGHC = false`, **when** the migration runs,
   **then** the row has `IsOrgProfile = false`.
3. **Given** a legacy request containing `isGHC`, **when** the API binds it,
   **then** the DTO uses the same boolean value as `IsOrgProfile`.
4. **Given** a new profile response, **when** it is serialized, **then** it
   contains `isOrgProfile` and does not contain `isGHC`.
5. **Given** a member has multiple academic records, **when** the Web or Mobile
   client edits them, **then** the first-record protection and all existing
   true/false rules remain unchanged.
