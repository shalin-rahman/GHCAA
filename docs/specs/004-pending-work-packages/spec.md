# Pending Work Packages Specification

## Status

This specification defines the remaining active backlog recorded in
`docs/TODO.md` as of 2026-09-21. Items marked complete below are retained as
implementation evidence and are not delivery work. Existing code, generated
contracts, and tests remain authoritative.

## Scope

The specification covers every active `[TODO]`, `[PARTIAL]`, and
`[IN PROGRESS]` item in the tracker, excluding completed items and archived
history. Work Package 37.1 and decision 84.6 are completed and are included
only as dependencies or integration boundaries.

## Global requirements

### Cross-layer delivery

Every feature that changes shared behavior MUST trace Domain models and enums,
Application DTOs/interfaces/validators, Infrastructure persistence and
services, API controllers and authorization, Angular services/routes/components,
Flutter services/routes/screens, and executable tests. API changes MUST be
registered in `docs/API_CONTRACT_REGISTRY.md`.

### Dates

Date-only values MUST use ISO `yyyy-MM-dd` on the API boundary and
`dd-MM-yyyy` for Web and Mobile display/input. Timestamps MUST remain full
ISO-8601 values. No feature may introduce a second date convention.

### Security and privacy

Authorization MUST be enforced server-side. Public projections MUST omit
private fields. Uploads MUST reuse the existing validation and storage
pipeline. Mutations MUST be auditable where the existing audit model supports
it. Error responses MUST use the repository's ProblemDetails conventions and
MUST NOT leak private data.

### Reuse and white-labeling

New services MUST use existing Application interfaces, shared storage,
pagination, date, loading, empty-state, theme, and error helpers. Institution
names, colors, URLs, labels, and feature flags MUST come from the profile
configuration boundary rather than feature-specific constants.

### Completion evidence

An item may be marked done only after its specified tests pass, documentation
is synchronized, migrations are verified against a non-empty database when
tables change, and `graphify update .` completes.

## Requirements by backlog item

## Completed implementation boundaries

### 37.1 Persisted election engine

Work Package 37.1 is implemented across the Domain, Application,
Infrastructure, API, Web, Mobile, migration, and test layers. The delivered
workflow persists elections, seats, officers, frozen voter rolls,
nominations, scrutiny, withdrawal, candidate lists, polling, secret ballots,
counting, declaration, results, audit history, and generated election PDFs.
Authenticated claims provide actor identity. Ballot records do not store the
voter's member ID. Conditional updates and serializable transactions prevent
duplicate voting under replay and concurrent requests.

The generated forms provide the shared official A4 layout and verification
data. Detailed legal field completeness remains the scope of the form-content
backlog and is not implied by this engine delivery.

### 81.1–81.3 Member communication visibility

The communication visibility work is implemented. `EmailLog` now carries the
recipient member ID, channel, delivery scope, status, and bounded failure
metadata. `GET /api/communications/me` filters by the authenticated member;
`GET /api/admin/comm/member/{memberId}` is administrator-only. Both return
paginated DTOs. Email and workflow SMS delivery are logged, and configured SMS
templates now use `ISmsService` rather than the email provider. Missing or
unavailable SMS delivery is recorded as `Unavailable`; provider exceptions are
recorded as `Failed`.

Angular and Flutter consume the member endpoint. The Angular and Flutter
surfaces show channel, scope, status, body, and delivery time. OTP SMS sent
directly through `ISmsService.SendOtpSmsAsync` remains outside this history
until it is routed through a logging boundary.

### 6.2 Alumni referrals

Registered members MUST be able to refer an eligible contact to a job or
internship without exposing the contact's private data to other members.
The system MUST persist referrer, opportunity, consent state, status, and
timestamps; prevent duplicate referrals for the same opportunity/contact;
allow the opportunity owner or authorized administrator to progress the
referral; and notify the permitted parties. Public and member projections MUST
respect profile privacy settings. Required evidence includes service/API
contracts, Web and Mobile surfaces if exposed on Mobile, duplicate and
authorization tests, and notification tests.

### 8.5 High-performance local database

The Mobile application MUST evaluate Isar or Drift against the current local
storage needs before adoption. The decision MUST record benchmark criteria,
supported platforms, migration strategy, encryption implications, offline
query requirements, and rollback path. No production dependency may be added
until the spike demonstrates a concrete benefit over the current storage
patterns and passes `flutter analyze` and tests.

### 8.7 Riverpod state hydration

Persisted Mobile state MUST declare ownership, serialization version, expiry,
logout clearing behavior, account-switch isolation, corruption recovery, and
offline fallback behavior. Sensitive tokens MUST remain in the existing secure
storage path. Hydrated providers MUST not display stale private data after
logout or account change. Tests MUST cover restart, expiry, corruption, and
user switching.

### 8.8 Runtime localization

Web and Mobile MUST support English and Bengali through a lightweight flat
key-to-string registry with a runtime language toggle. Missing keys MUST fall
back deterministically to English and be observable in development. Dates,
validation messages, accessibility labels, API errors, and generated documents
MUST use the selected locale where supported. No build-time localization
package is required or assumed. Tests MUST cover toggle persistence, fallback,
plural/format parameters, and representative public/member/admin screens.

### 27.8 Coverage enforcement

The repository MUST publish backend and Web coverage reports and enforce the
agreed per-file threshold in CI. The implementation plan MUST first establish
baseline coverage, exclude generated/vendor files explicitly, and fail with an
actionable report when a changed file falls below the threshold. The threshold
MUST be configurable without weakening the required default of 80 percent.

### 37.2 Scholarship and student aid

The platform MUST support fund, call, anonymous/public application, document
submission, blind review, shortlist, award, disbursement, and status lookup.
Applicants MUST NOT become Member or User records. Reviewer DTOs MUST omit
applicant identity. Status lookup MUST be rate-limited and use
reference-code-plus-email verification. Paid awards MUST create exactly one
Grant expense ledger record and be idempotent. Required surfaces are public
Web, member reviewer, admin, API, persistence, upload validation, and tests.

### 37.4 Cohorts and reunions

Batch membership MUST be derived from GHC `AcademicRecord.PassingYear` rows;
it MUST NOT add a duplicate Member batch field. Cohorts MUST support title,
story, cover image, representative, and active state. Reunions MUST compose
the existing AlumniEvent, registration, budget, and fee flow. Public year
listing/detail, member batch view, admin management, Mobile parity where
surfaced, and tests for multiple records and non-GHC exclusion are required.

### 37.5 In Memoriam

The system MUST provide moderated memorial entries and condolences. A linked
deceased member MUST be deactivated and excluded from public directory and
future election rolls in one transaction. Public projections MUST include
approved entries only. Condolence content MUST pass the existing sanitizer.
Submission, moderation, privacy, notification, Web, Mobile, and integration
tests are required.

### 37.6 Oral-history archive

The archive MUST support collections and items with narrator, transcript,
summary, publication state, moderation state, decade tag, optional linked
member, optional validated upload, and optional external media URL. Transcript
search and readable public detail are required. Missing transcripts MUST be
visible to administrators as incomplete records. Public API access MUST expose
approved content only; upload limits and storage MUST reuse existing services.

### 37.7 Bengali/English bilingual UI

This item shares the localization registry defined in 8.8 but delivers the
complete application inventory: public, member, admin, validation, error,
notification, accessibility, and generated-document strings. Every newly
added route MUST include both language keys before release. A missing-key
report MUST be available to tests or development tooling.

### 37.8 Fundraising campaigns

The platform MUST support campaign creation, goals, publication, contribution
records, manual payment methods, donor privacy, progress aggregation, admin
moderation, and financial ledger reconciliation. Live gateway keys MUST NOT be
required. Public totals MUST not expose private donor data. Web, Mobile,
admin, API, ledger, payment-config, and failure-path tests are required.

### 37.9 Alumni impact metrics

Authorized administrators MUST be able to derive impact metrics from persisted
memberships, events, scholarships, fundraising, mentoring, jobs, reunions,
and archive activity. Metrics MUST identify source query, period, filters,
freshness, and empty-data behavior. Public metrics MUST be explicitly
approved and privacy-safe. Repeated computation MUST be deterministic and
tested against fixture data.

### 37.10 Association reporting

The platform MUST generate member, governance, election, programme, finance,
fundraising, and impact reports from persisted records. Reports MUST declare
period, filters, generated timestamp, source scope, and privacy classification.
Exports MUST reuse existing Excel/PDF helpers, enforce authorization, and
remain profile-aware. Tests MUST verify totals against source records and
redaction of restricted fields.

### 37.11 Work Package 37 acceptance gate

The standing gate MUST verify every completed 37.x item with backend tests,
Web unit/type/build checks, Mobile analysis/tests, API contract snapshots,
non-empty-database migration checks, synchronized project documentation, and
Graphify output. It MUST remain open while any 37.2 or 37.4–37.10 item is
unfinished.

### 42.1–42.5 Election handbook/content administration

The project MUST inventory election handbook documents and distinguish legal
templates, public guidance, generated official records, and obsolete content.
42.2 MUST provide admin CRUD with validation, ordering, publication, version,
profile ownership, and audit history. 42.3 MUST make public and portal pages
read the managed source. 42.4 MUST synchronize Mobile only where the feature
is surfaced. 42.5 MUST provide API, client, migration, authorization, and
content-integrity tests. This work MUST not duplicate the 37.1 generated ER
form engine.

### 47.13 Mutation coverage

The repository MUST identify mutation-tested business rules for POST, PUT, and
DELETE endpoints, prioritize authorization, validation, state transitions,
duplicate prevention, and ledger effects, and record surviving mutants. CI
MUST run the agreed mutation scope without changing production behavior.

### 60.2–60.5 Verification and Mobile integration

60.2 MUST document behavioral verification evidence separately from code
inspection. 60.3 MUST reconcile Messages versus Chat/Forum terminology across
routes, labels, API contracts, and documentation. 60.4 MUST run authenticated
Mobile integration tests on a supported toolchain with deterministic test
credentials and isolated data. 60.5 MUST correct credential, financial, and
forum fixture gaps and record the environment used.

### 62.2 Profile-pack foundation

All declared profile-pack files MUST either exist for supported profiles or be
explicitly removed from the loader contract. Remaining files include backend
seed pairs, themes, membership tiers, governance, assets, and demo data as
their owning phase requires. Profile loading MUST fail loudly on malformed
required data and MUST never silently fall back to another institution's data.

### 62.41–62.42 Generic profile acceptance and brand lint

62.41 MUST run the generic profile acceptance suite against a real empty
database and record the environment. It MUST verify landing, configuration,
registration, login, portal, identity document, and admin shell without GHC
branding. 62.42 MUST make brand lint blocking after all generated and runtime
assets pass the generic-profile rules.

### 63.x, 64.x, 65.5, and 67.x dissertation evidence

The documentation book MUST keep outline, chapters, figures, tables, front
matter, references, risk calculations, and implementation evidence synchronized.
Repository figures MUST be generated from dated commands. New diagrams MUST
print legibly on A4 with labels at least 7pt. Chapter 11 component diagrams,
missing reference chapters, Arabic numbering, risk exposure `RE = P × C`, and
design-principle sections MUST be completed only from verified implementation
evidence. Every chapter change requires the strict book build.

### 72.4–72.5 Dependency and CodeQL operations

The project MUST observe the first Dependabot run across configured ecosystems,
triage updates without unbounded upgrades, and record compatibility decisions.
If the repository becomes public, CodeQL MUST be enabled with documented
language scope, alert ownership, and remediation evidence.

### 73.5–73.7 Requirements traceability

The repository MUST generate a requirements traceability matrix linking
requirements to code, API routes, clients, tests, and documentation. Governance
rules MUST fail validation when implementation drifts from the governing
source. The two currently unimplemented user statements MUST be resolved as
implemented requirements, explicit decisions, or rejected scope with rationale.

### 74.3, 75.5, 76.4, and 77.4 Planning provenance

Historical work-package markers, PRE durations, reduction factors, production
durations, and provenance claims MUST be backfilled only from dated repository
evidence or explicitly labeled as owner-supplied assumptions. No plausible
number may be substituted for missing evidence.

### 78.9, 78.10, and 78.12 Evaluation evidence

Evaluation sessions MUST have participant authorization, test scenarios,
consent/privacy handling, reproducible environment details, raw observations,
issue classification, and summarized evidence. Participant-free evidence MUST
be produced separately. Completion order for chapters and evaluation artifacts
MUST be recorded once and referenced consistently.

### 81.1–81.3 Member communication visibility

Members MUST be able to view their own outbound email/SMS communication status,
subject to privacy and retention rules. The portal MUST distinguish queued,
sent, failed, and unavailable provider states. Admins MUST retain the existing
operational view. Tests MUST cover authorization, redaction, pagination, and
provider failure.

### 82.59 Election handbook profile awareness

Election handbook content, generated forms, labels, URLs, institution names,
branding, and verification links MUST resolve through the active profile.
Tests MUST run at least the default and GHCAA profiles and prove that no
institution-specific content leaks across profiles.

### 84.4–84.5 and 84.7 Governance specification work

84.4 MUST produce a route-by-route Web/Mobile parity table with response-shape
differences and explicit non-applicability. 84.5 MUST catalog every
authorization role/policy and every ProblemDetails error code with controller,
client handling, and tests. 84.7 MUST record the owner's decision on
committed-migration data for second-institution deployment before changing
white-label scope.

## Non-functional acceptance

All features MUST preserve the existing soft-archive model, accessibility,
responsive behavior, loading/error/empty states, auditability, rate limiting,
and profile isolation. Database changes MUST include provider migrations and
rollback or recovery notes. Public endpoints MUST be cache-safe for
user-specific data. Generated documents and exports MUST be deterministic for
the same source snapshot except for explicitly declared generation metadata.

## Out of scope

This specification does not authorize commits, pushes, deployments, destructive
database changes, live payment gateway keys, a new localization package, or a
new persistence abstraction without an evidence-backed decision.
