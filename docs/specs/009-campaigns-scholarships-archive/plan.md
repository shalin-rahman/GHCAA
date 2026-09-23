# WP37 implementation plan

## Dependency order

1. 37.8 Fundraising campaigns
2. 37.2 Scholarship and student aid
3. 37.6 Oral-history archive

## Why this order

The fundraising flow establishes the core donation model, ledger posting, and public donor visibility pattern. The scholarship feature reuses that pattern for award funding, exact-once posting, and public status lookup. The archive feature is independent in content model but still follows the same public/admin visibility and storage conventions, so it is best built after the financial and application patterns are already stable.

## Phase 1 — 37.8

### Backend

- verify `Campaign`, `CampaignPledge`, `DonorRecognitionTier`, and related DTOs
- confirm controller routes for public, member, and admin campaign flows
- verify idempotent financial-record creation on pledge confirmation
- confirm public honour-roll projection keeps anonymous donor names private

### Web

- validate the public campaigns page and member giving page
- confirm the donation form, amount validation, and submission state are consistent with the shared loading and theme system
- verify the admin campaign management route still follows the shared admin table patterns

### Mobile

- confirm a mobile campaign view or member giving screen exists or is intentionally deferred if not surfaced
- keep parity requirements honest: if the route is not surfaced on Mobile, record the non-applicability rather than forcing a mismatched implementation

### Tests

- run `CampaignServiceTests`
- add or extend tests covering idempotent confirmation, anonymous donor projection, and public progress calculation

## Phase 2 — 37.2

### Domain and application

- add scholarship funding records and application states
- add review and award states with explicit reviewer boundaries and hidden applicant identity
- add disbursement rule that posts one award record and is idempotent on retry

### API and data

- add public status lookup by reference code and email
- add reviewer and admin endpoints for shortlist, award, and disbursement confirmation
- add persistence checks for exact-one ledger posting

### Web and Mobile

- add public lookup page and member/reviewer/admin views
- keep private fields out of public projections

### Tests

- verify application creation without Member/User records
- verify reviewer projections hide applicant identity
- verify disbursement ledger posting is exactly-once

## Phase 3 — 37.6

### Domain and application

- add archive collections and items with moderation and publication states
- add transcript and summary fields
- add incomplete-record visibility rules for admins

### API and data

- add public and admin routes
- enforce approved-only public projection
- reuse upload validation and storage services

### Web and Mobile

- add public archive list/detail screens
- add admin moderation and collection management screens
- keep public detail hidden until approved

### Tests

- verify public API returns approved items only
- verify admins can still view incomplete items
- verify transcript search and media validation behave correctly

## Execution checkpoint

The implementation is only ready to move to the next package once the current phase has:

- passed its targeted backend tests
- passed the relevant web or mobile checks in the impacted client
- updated the associated API contract registry entries
- reflected any route or model changes in the relevant docs and specs
- remained consistent with the shared loader, theme, and security patterns
