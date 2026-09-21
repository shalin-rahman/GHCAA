# Feature Specification: Alumni Programs and Credential Verification

**Feature Branch**: `003-alumni-programs-and-verification`
**Created**: 2026-09-21
**Status**: Draft
**Input**: `docs/TODO.md` Work Package 37 items 37.2, 37.5, 37.8, 37.9, 37.10
**Depends on**: 37.0 (real EF migration path at deploy time — every story below adds at
least one table, and `Database.EnsureCreated()` is a no-op on preprod's non-empty
database). None of these five stories can ship before 37.0 lands. They do not depend
on each other or on 37.1/37.3/37.4/37.6/37.7.
**Governed by**: 37.11 (standing test/build gate — `dotnet test`, `npx vitest run`,
`npm run type-check`, `npx ng build` must all pass, and any new table must be verified
against a non-empty database, not just a fresh one).

## Purpose and scope

This spec covers five independent, unbuilt Work Package 37 backlog items chosen
because they carry no dependency on each other beyond the shared 37.0 prerequisite:
a scholarship programme, an in-memoriam register, public credential verification,
geographic chapters, and an annual impact report. Each is written as its own user
story so any one of them can be built, tested, and shipped behind its own feature
flag without waiting on the others.

37.8 (credential verification) is placed first because 37.1's election-document
QR codes and 37.10's impact report both read more naturally once a verification
endpoint exists, even though neither strictly blocks on it. 37.10 (impact report)
is placed second because it is explicitly designed to degrade gracefully when its
source features (37.2, 37.4, campaigns) are absent, so it is safe to build early
and let it pick up real numbers as those features land later.

Following this repository's established pattern in `001-platform-baseline` and
`002-workflow-contracts-and-validation`, this spec keeps implementation-level
detail (model shapes, service/controller names, reused infrastructure) alongside
the business requirements rather than splitting them into a separate plan
document — the two prior specs both do this, and duplicating `docs/TODO.md`'s
already-detailed technical design into a second, technology-agnostic document
would create two sources of truth for the same decisions. Anything below that
looks like an implementation detail is intentional and traces back to the
corresponding `docs/TODO.md` item, cited by section number.

## Out of scope

- 37.1 (election engine), 37.3, 37.4 (batch cohorts/reunions), 37.6 (oral-history
  archive), 37.7 (bilingual UI) — separate backlog items, not covered here.
- 37.0 itself (the migration-path mechanism) — a prerequisite, not a story of
  this spec.
- Any UI visual design beyond "reuse existing tokens/shell" — covered by the
  `ghcaa-design` skill, not this spec.

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Public credential verification (Priority: P1)

An employer, scholarship committee, or other third party holds a printed GHCAA
membership certificate, ID card, or election document and has no way to confirm
it is genuine. They need a way to check a document's authenticity without
contacting the association directly.

**Why this priority**: Every other credential-bearing surface in the system
(existing ID cards, and 37.1's election documents once built) becomes trustworthy
the moment this exists, and untrustworthy without it. It is also the smallest of
the five stories — one new entity, one new endpoint, no new UI shell — so it
returns the most trust-infrastructure value per unit of build effort.

**Independent Test**: Generate a credential, revoke it, and confirm a lookup by
its code returns `valid: false` with no contact information anywhere in the
response; confirm a non-revoked code returns `valid: true` with only name,
membership type, issue date, and status.

**Acceptance Scenarios**:

1. **Given** a member holds a genuine, non-revoked ID card or certificate,
   **When** anyone scans its QR code or enters its short code at `/verify`,
   **Then** the page shows a "valid" verdict with the member's name, membership
   type, and issue date, and nothing else identifying them (docs/TODO.md 37.8).
2. **Given** an admin has revoked a credential, **When** anyone looks it up,
   **Then** the page shows a "revoked" verdict and no personal data.
3. **Given** a code that was never issued, **When** anyone looks it up,
   **Then** the page shows an "unknown code" verdict, distinct from "revoked".
4. **Given** the verification endpoint is publicly enumerable by design,
   **When** an automated client tries many codes in a short window,
   **Then** the existing login rate-limiting pattern throttles the requests.

### Edge Cases

- A short code collides with an existing one during generation — the unique
  index must reject the insert rather than silently overwrite (37.8 requires
  uniqueness across 10,000 generations).
- A credential is issued, then the member's underlying record is later deleted
  or anonymized — the verification response must still resolve to a safe verdict
  (revoked or unknown), never an error that leaks internal state.

---

### User Story 2 - Annual impact report (Priority: P2)

A donor, prospective member, or board member wants to see what the association
accomplished in a given year — money raised and spent, scholarships awarded,
events held, new members welcomed — without asking someone to compile it by
hand.

**Why this priority**: It is pure aggregation of data the system already has
(the ledger, memberships, events), so it carries almost no new-entity risk, and
it is explicitly designed to work correctly before 37.2/37.4/campaigns exist —
sections for missing features render as zeroed rather than absent, so shipping
it early costs nothing and it only gets more complete over time.

**Independent Test**: Request the report for a year with zero underlying
records and confirm every section renders with zero values rather than an
error or a missing section; request a year with known fixture data and confirm
category totals match a hand-summed total.

**Acceptance Scenarios**:

1. **Given** a calendar year with financial, scholarship, campaign, event, and
   membership activity, **When** the report is generated for that year,
   **Then** it shows income/expense by category, scholarships awarded and
   disbursed, campaign totals and donor counts, events and attendance, and new
   members — all computed from existing records, none hand-entered
   (docs/TODO.md 37.10).
2. **Given** a year where a source feature (e.g. campaigns) was not yet built
   or its flag was off, **When** the report is generated, **Then** that
   section renders as zero/empty rather than throwing an error.
3. **Given** an admin has written a foreword and chosen a cover image for a
   year, **When** the report is published, **Then** the foreword and cover
   appear alongside the computed sections.
4. **Given** the report is public by design, **When** anyone requests the
   on-site view or the PDF for a published year, **Then** it is accessible
   without authentication.

### Edge Cases

- A scholarship award's linked `FinancialRecord` was deleted independently of
  the award — the disbursed total must not silently drop that award; a
  mismatch here should be visible as a data-integrity signal, not hidden.
- Two campaigns pledge to the same donor in the same year — the donor count
  must reflect distinct donors, not distinct pledges.

---

### User Story 3 - In Memoriam register (Priority: P3)

A member or family member wants to submit a tribute for a deceased alumnus, and
the public wants a respectful, moderated place to read tributes and offer
condolences — without that becoming an unmoderated wall that could embarrass
the association or, worse, leave a deceased member listed as an active voter or
directory entry.

**Why this priority**: This is the single highest-sensitivity surface among the
five stories (a public page about the deceased, open to public comment), so it
is sequenced after the lower-risk verification and reporting stories to allow
the moderation and deactivation rules to be reviewed carefully before it ships.

**Independent Test**: Submit a memorial entry linked to an existing member,
confirm the entry does not appear publicly until approved, and confirm that
approving the link deactivates the member and removes them from the public
directory in the same operation.

**Acceptance Scenarios**:

1. **Given** a member or family member submits a memorial entry, **When** it
   is pending admin review, **Then** it does not appear on the public
   in-memoriam page (docs/TODO.md 37.5).
2. **Given** an admin approves and publishes a memorial entry, **When** a
   visitor views the public page, **Then** the entry, its tribute, and any
   approved condolences are visible.
3. **Given** a memorial entry is linked to an existing `Member` record,
   **When** that link is set, **Then** the member is deactivated and removed
   from the public directory and from any open election voter roll in the
   same transaction — a deceased member must never appear as an active voter.
4. **Given** a visitor submits a condolence message, **When** it is stored,
   **Then** it has passed through the same HTML sanitizer used elsewhere in
   the system before it is persisted, and it does not appear publicly until
   approved.

### Edge Cases

- A memorial entry has no linked `Member` (a pre-digital alumnus with no
  account) — the entry must still be submittable and publishable using only
  the free-text name and dates.
- An entry is later found to be inaccurate or objectionable after publishing —
  an admin must be able to unpublish it, not only approve it once.
- A member is deactivated via a memorial link while an election is actively
  mid-poll — the roll-suppression rule must still apply without corrupting an
  already-frozen roll snapshot for that in-progress election.

---

### User Story 4 - Scholarship & student-aid programme (Priority: P4)

The association wants to run a scholarship or student-aid fund: publish a call
for applications, receive applications from students who are not themselves
members, review them without bias toward identity, award funds, and record the
disbursement against the association's books.

**Why this priority**: This is the largest and most process-heavy of the five
stories (fund → call → application → blind review → award → disbursement), so
it is sequenced after the smaller, lower-risk items even though its original
backlog priority ties with the register and report.

**Independent Test**: Submit an anonymous application against an open call,
confirm a reviewer's view of that application exposes no identifying fields,
and confirm marking an award "Paid" produces exactly one matching ledger entry.

**Acceptance Scenarios**:

1. **Given** an open scholarship call, **When** a student (not a GHCAA member)
   submits an application with supporting documents, **Then** the application
   is accepted and identified only by a reference code and email, with no
   member account created for them (docs/TODO.md 37.2).
2. **Given** an application under review, **When** a panel reviewer opens it,
   **Then** the reviewer sees the need/merit statements and scores but never
   the applicant's name, email, phone, or guardian name.
3. **Given** an application is submitted after a call's closing date, **When**
   the submission is attempted, **Then** it is rejected.
4. **Given** an award is marked as paid, **When** the disbursement is
   recorded, **Then** exactly one ledger entry is created for it, and marking
   it paid a second time does not create a duplicate.
5. **Given** an applicant wants to check their status, **When** they enter
   their reference code, **Then** they see their current status without
   needing an account, and the lookup is rate-limited against abuse.

### Edge Cases

- Two applications are submitted with the same email for the same call — the
  system must not conflate them into one identity record (applicants are
  explicitly not members).
- An award references a scholarship fund that was later deactivated — the
  award and its disbursement history must remain readable even though new
  applications against that fund are no longer accepted.

---

### User Story 5 - Geographic chapters (Priority: P5)

Alumni living in the same city or region want a chapter presence: a page that
identifies their chapter, a coordinator to contact, and event/announcement
visibility scoped to their chapter rather than the whole association.

**Why this priority**: Sequenced last because it is the most purely additive
of the five stories — it extends an existing event/notification mechanism
rather than introducing a new sensitive workflow, and nothing else in this
spec depends on it.

**Independent Test**: Join a chapter, confirm joining it twice does not create
a duplicate membership, and confirm a chapter-scoped event appears only in
that chapter's feed and not in the general event list's chapter filter for a
different chapter.

**Acceptance Scenarios**:

1. **Given** a published list of chapters, **When** a member joins one,
   **Then** they appear in that chapter's roster, and joining the same
   chapter again does not duplicate the membership (docs/TODO.md 37.9).
2. **Given** a chapter has a coordinator, **When** a visitor views the public
   chapter page, **Then** they see the coordinator's contact email only if the
   coordinator has chosen to make it public.
3. **Given** an event is scoped to a chapter, **When** a member views their
   chapter feed, **Then** that event appears there, and it does not appear as
   a chapter event for any other chapter.
4. **Given** a chapter announcement is sent, **When** it is delivered,
   **Then** it reaches exactly the chapter's members through the existing
   in-app notification channel, with no new messaging surface introduced.

### Edge Cases

- A member joins a chapter, then later the chapter is deactivated — existing
  membership rows and history must remain readable even if the chapter no
  longer accepts new joins.
- A coordinator leaves the chapter (removes their own membership) while still
  marked as coordinator — the chapter must not be left silently
  coordinator-less without an admin being made aware.

---

## Requirements *(mandatory)*

### Functional Requirements

**Credential verification (Story 1)**

- **FR-001**: The system MUST let anyone look up a credential by its short
  code or by scanning its QR code and receive a verdict of valid, revoked, or
  unknown.
- **FR-002**: A valid-credential response MUST include only the holder's name,
  membership type, issue date, and status — never an email, phone, address, or
  internal identifier.
- **FR-003**: An admin MUST be able to revoke a credential, and a revoked
  credential's verification response MUST change to "revoked" without
  removing the historical record of its issuance.
- **FR-004**: Short codes MUST be unique; the system MUST reject a collision
  rather than overwrite an existing code.
- **FR-005**: The verification lookup MUST be rate-limited against automated
  enumeration.

**Annual impact report (Story 2)**

- **FR-006**: The system MUST generate a report for any requested year by
  aggregating existing financial, scholarship, campaign, event, and membership
  records — with no field requiring manual entry other than a foreword and a
  cover image.
- **FR-007**: A year with no underlying activity in a section MUST render that
  section as zero/empty, never as a missing section or an error.
- **FR-008**: The report MUST be viewable on-site and downloadable as a
  document, both without authentication once published.
- **FR-009**: An admin MUST be able to set the year's foreword and cover image
  and control whether the report is published.

**In Memoriam register (Story 3)**

- **FR-010**: A memorial entry MUST NOT appear on any public page until an
  admin approves it.
- **FR-011**: Linking a memorial entry to an existing member MUST deactivate
  that member, remove them from the public directory, and remove them from
  any open election voter roll, all as a single atomic operation.
- **FR-012**: Every condolence message MUST pass through the system's existing
  HTML-sanitization step before storage, and MUST NOT appear publicly until
  approved.
- **FR-013**: A memorial entry MUST be submittable and publishable without a
  linked member record, using free-text identity and date fields.
- **FR-014**: An admin MUST be able to unpublish a previously published entry.

**Scholarship & student-aid programme (Story 4)**

- **FR-015**: A person MUST be able to submit a scholarship application
  without holding or being issued a GHCAA member account.
- **FR-016**: Any view of an application available to a reviewer MUST exclude
  the applicant's name, email, phone, and guardian name; identity MUST be
  reachable only through a reference code that reviewers do not resolve back
  to a person.
- **FR-017**: The system MUST reject an application submitted outside its
  call's open window.
- **FR-018**: Marking an award as paid MUST create exactly one corresponding
  ledger entry, and repeating that action MUST NOT create a second entry.
- **FR-019**: An applicant MUST be able to check their application status
  using only their reference code, without authentication, and that lookup
  MUST be rate-limited.

**Geographic chapters (Story 5)**

- **FR-020**: A member MUST be able to join a chapter, and joining the same
  chapter twice MUST NOT create a duplicate membership record.
- **FR-021**: A chapter's coordinator contact information MUST be visible on
  the public chapter page only when the coordinator has opted to expose it.
- **FR-022**: An event scoped to a chapter MUST appear in that chapter's feed
  and MUST NOT appear as belonging to any other chapter.
- **FR-023**: A chapter announcement MUST reach exactly that chapter's members
  through the system's existing notification mechanism.

**Cross-cutting**

- **FR-024**: Each of the five stories MUST ship behind its own feature flag
  (`enableCredentialVerification`, `enableImpactReport`, `enableMemorial`,
  `enableScholarships`, `enableChapters`) so any one can be enabled
  independently of the others.
- **FR-025**: None of the five stories' new tables MUST be assumed reachable
  in a deployed environment until the 37.0 migration path has run against that
  environment's non-empty database.

### Key Entities

- **IssuedCredential** — a record of one generated, verifiable document
  (membership certificate, ID card, or election document): who it belongs to,
  what type it is, its short code, issue/expiry dates, and revocation state.
- **ImpactReportDto** (computed, not stored) — a year's aggregated totals
  across finance, scholarships, campaigns, events, and membership, plus the
  authored foreword/cover image pulled from the existing content system.
- **MemorialEntry** — a tribute to a deceased alumnus: identity (linked
  member or free text), dates, tribute text, photo, and its moderation status.
- **Condolence** — a message posted against a memorial entry, with its own
  moderation state.
- **ScholarshipFund / ScholarshipCall / ScholarshipApplication /
  ScholarshipReview / ScholarshipAward** — the fund that money sits in, a
  yearly call for applications against it, an applicant's submission, a
  reviewer's blind scoring of it, and the resulting award and disbursement
  record.
- **Chapter / ChapterMembership** — a geographic grouping of alumni and the
  join relationship between a member and a chapter.

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A third party can verify any issued credential's authenticity in
  under two actions (scan or type a code, read the verdict) with zero contact
  information disclosed for either a valid or a revoked credential.
- **SC-002**: An impact report for any year — including one with no recorded
  activity — renders completely without error, and every total in it can be
  reproduced independently by summing the same year's source records.
- **SC-003**: Zero memorial entries or condolences reach a public page without
  having passed an approval step, verified over the full set of entries in the
  system at any audit point.
- **SC-004**: A reviewer evaluating scholarship applications cannot determine
  an applicant's identity from any data the system exposes to them, verified
  by inspecting every field returned to the reviewer role.
- **SC-005**: Marking a scholarship award paid produces exactly one ledger
  entry regardless of how many times the action is repeated.
- **SC-006**: Joining the same chapter multiple times never produces more than
  one membership row for that member/chapter pair.
- **SC-007**: All five stories pass the 37.11 gate independently — each can be
  toggled off via its feature flag with the rest of the system's test suites
  (`dotnet test`, `npx vitest run`, `npm run type-check`, `npx ng build`)
  still passing.

## Assumptions

- 37.0's migration mechanism will exist and be usable by the time any of these
  five stories is implemented; none of them substitutes a bespoke boot-time
  syncer for it (per `docs/TODO.md`'s note that copying the Work Package 36
  syncer pattern per feature "is the fallback, not the plan").
- The existing reusable infrastructure named throughout this spec — QuestPDF,
  QRCoder, `ClosedXML`, `INotificationService`, `ICommunicationService`,
  `FileUpload`/`IFileValidationService`/`IFileStorageService`, `HtmlSanitizer`,
  the `LoginRateLimitMiddleware` pattern, and the `SiteContent` CMS — remains
  available and is reused rather than duplicated, per `docs/TODO.md`'s
  Work Package 37 preamble.
- Blind review (Story 4) and identity minimization (Story 1) are enforced at
  the query/projection layer, not only hidden in the UI template — a
  requirement the underlying `docs/TODO.md` items state explicitly and this
  spec inherits.
- "Solid" specs for this backlog batch means business-readable user stories
  and testable requirements grounded in the already-decided technical design
  in `docs/TODO.md`, not a rediscovery of that design from a blank page.
