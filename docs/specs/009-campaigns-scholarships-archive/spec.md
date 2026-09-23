# Work Package 37 dependency chain

## Scope and dependency order

This specification defines the concrete implementation scope for 37.3, 37.2, and 37.6 in dependency order.

**Numbering note (2026-09-23):** this section was originally written against tracker item
"37.8," which `docs/TODO.md` now assigns to Credential Verification (built separately,
`[DONE]`). Campaigns is `docs/TODO.md` item **37.3** — the slot the FinancialRecord-back-linking
trio (37.2 Scholarships, 37.3 Campaigns, 37.4 Batch cohorts/reunions) already implied. This
section is renumbered below to match; no requirement content changed, only the label.

The recommended execution order is:

1. 37.3 Fundraising campaigns
2. 37.2 Scholarship and student aid
3. 37.6 Oral-history archive

This order keeps the shared finance, public-content, and admin flows stable before the higher-variance archive and scholarship features are built. Fundraising is the first dependency because it establishes the payment ledger and public donor projection pattern that the scholarship award flow later reuses. Scholarship logic then provides the business rule model for funding, review, and disbursement that the wider alumni-programme work expects. The archive remains last because it depends on the existing public-content storage, moderation, and profile-aware publish rules, but it does not create the shared financial flow that the other two features need.

## 37.3 Fundraising campaigns

### Status (2026-09-23)

Built, not a ground-up implementation task. `Campaign`/`CampaignPledge` domain models,
`ICampaignService`/`CampaignService`, a 12-route `CampaignsController`, admin, public,
and member giving-history Angular pages are all committed (`f960d216`) and covered by
`CampaignServiceTests.cs`. The tables are already in the `InitialBaseline` migration —
no new EF migration is needed. What remains — tracked as `docs/TODO.md` item 37.3 — is
Flutter mobile coverage only. The requirements below describe what was built and remain
the acceptance bar for the missing piece; they are not a request to rebuild the backend
or web UI.

### Goal

The platform shall support campaign creation, publication, contributor records, donor visibility rules, progress aggregation, admin confirmation, and financial ledger reconciliation without requiring live payment gateway keys.

### Functional requirements

- The system shall allow admins to create and edit campaigns with title, slug, story, cover image, target amount, active window, and live status.
- The system shall expose a public campaign list and a public detail route keyed by slug.
- The system shall calculate progress from confirmed cash received, not from unconfirmed pledges.
- The system shall support anonymous and named pledges.
- The system shall persist the pledge and related donor metadata, including amount, message, member linkage, and contact fields when supplied.
- The system shall support admin confirmation of received funds and write exactly one linked `FinancialRecord` per confirmed pledge.
- The system shall permit repeated confirmation attempts without creating a second ledger record.
- The system shall support donor recognition tiers and honour roll projections that hide real names for anonymous donors.
- The system shall protect private donor data and show only approved totals and safe donor names in public projections.

### Non-functional requirements

- Public totals must not expose private donor details.
- Live gateway keys must not be required to operate the feature.
- The campaign flow must reuse the existing admin, ledger, and profile-aware security conventions.
- The feature must remain idempotent under replayed confirmation requests.

### Acceptance criteria

1. A public campaign list loads from the API and shows target and received totals.
2. Creating a pledge for a live campaign persists a pledge row and returns the saved pledge payload.
3. Confirming the same pledge twice writes only one donation ledger row and returns success on the retry.
4. Anonymous donors appear as `Anonymous` in the public honour roll.
5. The admin campaign list can create, update, and view campaign totals without requiring a payment gateway.

### Evidence required

- Backend service and controller tests for create, confirm, and idempotent confirmation
- Angular public campaign page flow and member giving history
- API contract wiring through `docs/API_CONTRACT_REGISTRY.md`
- migration or persistence validation where tables are introduced or altered

## 37.2 Scholarship and student aid

### Goal

The platform shall support scholarships and educational aid from fund creation through application, review, award, and disbursement, without creating a separate Member or User record for applicants.

### Functional requirements

- The system shall support scholarship fund creation and public disclosure.
- The system shall allow applicants to submit an application with a public status lookup using a reference code and email verification.
- The system shall maintain review and shortlist states without exposing applicant identity to reviewers.
- The system shall create exactly one expense ledger record per paid award.
- The system shall keep the award flow idempotent when a payment confirmation is retried.
- The system shall support public status lookup for applicants while keeping reviewer-only fields hidden.

### Non-functional requirements

- Applicants must not become Member or User records.
- Reviewer DTOs must omit applicant identity until the review boundary allows it.
- Disbursement must follow the existing financial ledger rules and ledger posting conventions.
- Public status lookup must be rate-limited and must not expose private data.

### Acceptance criteria

1. A scholarship application can be created without creating a Member record.
2. Application review and shortlist steps can progress without leaking applicant identity to the reviewer list.
3. A paid award creates exactly one `Grant` or equivalent expense ledger record.
4. Retrying a disbursement confirmation does not create a second ledger record.
5. A public status lookup resolves by reference code and email without revealing private application state.

### Evidence required

- Domain and application models for funds, applications, reviewers, and award state
- Infrastructure persistence, validation, and authorization checks
- API routes for public lookup, member review, admin management, and disbursement
- Web and Mobile public + admin surfaces for applicant and reviewer flows
- Payment and ledger tests for idempotent posting

## 37.6 Oral-history archive

### Goal

The platform shall support moderated oral-history collections and items with transcripts, summaries, publication state, decade tag, optional linked member, and optional media links while keeping public API access restricted to approved records.

### Functional requirements

- The system shall allow admins to create archive collections and items.
- Each item shall hold narrator, transcript, summary, publication state, moderation state, decade tag, and optional linked member or media URL.
- The system shall expose approved public archive content only.
- The system shall permit admins to view incomplete records that are missing transcripts or media.
- The system shall support transcript searching and readable public detail views.
- Upload and media handling shall reuse the existing storage and validation pipeline.

### Non-functional requirements

- Public API access shall be limited to approved content.
- Private/incomplete records shall remain hidden from the public API until they are approved.
- The archive shall respect the existing profile-aware security and content-publishing model.
- The upload path shall reuse the current storage and validation service boundaries.

### Acceptance criteria

1. Public archive listing returns only approved items and collections.
2. Admin endpoints can view incomplete or unmoderated records.
3. Transcript search returns the correct item records.
4. A missing transcript remains visible to admins but hidden from public consumers.
5. Media uploads or external URLs follow the same validation and storage model as the rest of the platform.

### Evidence required

- Domain models and persistence for collections and items
- API and authorization checks for public versus admin visibility
- Web and Mobile public archive pages and admin management screens
- Search/indexing and moderation tests
- upload validation verification using the repository's existing file-validation boundary

## Implementation plan

### Phase 1 — 37.3 (start here)

- Confirmed 2026-09-23: the existing fundraising services, controller routes, and admin/public Angular surfaces are consistent with the 37.3 contract.
- Tracker item number reconciled to the implementation: this section and `docs/TODO.md` now both read 37.3. The stale `// TODO 37.8` code comment that appeared in five files (`Campaign.cs`, `CampaignsController.cs`, `ICampaignService.cs`, `CampaignService.cs`, `CampaignDtos.cs`) has been removed — the feature they describe is built, so the comment was not a TODO any more, and the remaining rollout work is tracked in `docs/TODO.md` 37.3 instead.
- Backend service and controller tests for campaign create, pledge, and receipt-confirmation idempotency already exist in `CampaignServiceTests.cs` — do not duplicate.
- Remaining before this phase can close: the Flutter mobile counterpart to the already-built public browse/pledge and member giving-history flows. No new migration is needed — `Campaign`/`CampaignPledge` are already in `InitialBaseline`.

### Phase 2 — 37.2

- Build the scholarship domain and application state model from the requirements above.
- Add application review, shortlist, award, and disbursement flows to the application and API layers.
- Reuse the campaign/ledger idempotence pattern for award posting and payment confirmation.
- Connect the public applicant status flow and reviewer/admin screens.

### Phase 3 — 37.6

- Add the oral-history archive data model and moderation workflow.
- Add public detail and transcript search endpoints.
- Reuse the existing upload and storage validation pipes.
- Add admin moderation and public list/detail views.

## Dependency summary

- 37.3 must be stable before 37.2 because scholarship funding and award posting rely on the same donation/financial ledger conventions.
- 37.2 should be completed before the broader alumni programmes and public report work so the payment path across campaigns and scholarships is consistent.
- 37.6 is planned after the financial and application flows because it is content-centric and not a prerequisite of the funding flow.
