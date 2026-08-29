# GHCAA Feature Catalog

A granular, module-by-module breakdown of the platform's features — including the primary screens, API endpoints, validation rules, and dependencies for each. For the formal specification, see the [Software Requirements Specification](SRS.md); for architecture diagrams, see [Architecture & Data Flow](architecture_data_flow.md).

---

## 1. Membership & Identity Management

### 1.1 Automated Member Registration
- **Business description**: Alumni apply for association membership through a guided, multi-step process.
- **User roles**: Public (Alumni).
- **Inputs / outputs**:
  - Screen: `/register` (multi-step wizard: Personal, Academic, Professional, Attachments, Verification).
  - API: `POST /api/auth/register`, `GET /api/auth/status/:id`.
  - Key fields: Full Name, NID (sanitized), Mobile, Batch, Degree, Email (OTP-verified).
- **Validations & rules**:
  - Duplicate prevention: global uniqueness check on NID, Email, and Mobile No.
  - Academic prerequisite: at least one academic record from "Govt. Haraganga College".
  - Digital sanitization: automatic removal of spaces/formatting from NID and Mobile strings.
- **Dependencies**: Email service (OTP), ID Card service (metadata).

### 1.2 Secure Authentication & Authorization
- **Business description**: Secure access to the portal based on identity and role.
- **User roles**: Public, Member, Admin, SuperAdmin.
- **Inputs / outputs**:
  - Screen: `/login`, `/verify-email`.
  - API: `POST /api/auth/login`, `POST /api/auth/verify-email`.
  - Key fields: NID/Mobile/Email as identifier, Password (BCrypt-hashed), OTP code.
- **Validations & rules**:
  - Verification requirement: email must be OTP-verified before login is permitted.
  - Role-based access: granular JWT claims for Member vs Admin vs SuperAdmin.
- **Dependencies**: JWT token service.

### 1.2a Admin Step-Up Verification (2FA)
- **Business description**: A second, email-OTP verification required before destructive/financial/identity admin actions (member archive, EC hard-delete, financial ledger writes, system-admin delete, payment-config delete) even for an already-authenticated Admin/SuperAdmin session.
- **User roles**: Admin, SuperAdmin.
- **Inputs / outputs**:
  - API: `POST /api/auth/admin/step-up/request`, `POST /api/auth/admin/step-up/verify`.
  - Key fields: 6-digit OTP code, delivered to the admin's registered email.
- **Validations & rules**:
  - Once verified, a 30-day grace period applies (tracked via a JWT claim carried forward across normal token refreshes) — not a per-action or per-login re-prompt.
  - A fresh login always starts unverified; the grace period only survives continued activity within an existing session.
  - OTP codes are purpose-scoped (`OtpPurpose.AdminStepUp`), so a registration or password-reset code can never satisfy a step-up challenge.
- **Dependencies**: Existing `IOtpService`/email template plumbing (no new OTP infrastructure).

### 1.3 Personal Profile & Privacy Control
- **Business description**: Members manage personal, academic, and professional information with granular privacy toggles.
- **User roles**: Member.
- **Inputs / outputs**:
  - Screen: `/portal/profile`.
  - API: `GET /api/profile`, `PUT /api/profile/update`.
  - Key fields: privacy toggles (mask NID, Email, Mobile, Address).
- **Validations & rules**:
  - PII masking: sensitive fields are masked for other members unless the corresponding public toggle is active.
- **Dependencies**: Profile controller, infrastructure services.

### 1.4 Digital ID Card & Certificate
- **Business description**: Automatic generation of secure, downloadable digital ID cards and certificates for verified members.
- **User roles**: Approved Member.
- **Inputs / outputs**:
  - Screen: `/portal/profile` (download buttons).
  - API: `GET /api/profile/id-card`, `GET /api/profile/certificate`.
  - Key fields: QR code data stream, Membership ID.
- **Validations & rules**:
  - Eligibility: generation locked until `MembershipStatus == Active`.
  - Format: auto-generated sequential serials during approval.
- **Dependencies**: ID Card generation service.

---

## 2. Events & Participation

### 2.1 Event Listings & Catalog
- **Business description**: Centralized hub for discovering upcoming and past alumni events.
- **User roles**: Public, Member.
- **Inputs / outputs**: Screen `/events`; API `GET /api/events`.
- **Dependencies**: Events service.

### 2.2 Intelligent Event Registration
- **Business description**: Members and guests register for events with integrated payment tracking.
- **User roles**: Member, Guest (if allowed) — here "Guest" means an *unregistered event
  attendee* (see the `GuestName` / `GuestEmail` / `GuestMobile` fields on the registration
  record). This is **not** the `Guest` value of the `MembershipType` enum, which is a membership
  tier assigned to an actual member. The two are unrelated; do not wire one to the other.
- **Inputs / outputs**:
  - Screen: `/events/:id`.
  - API: `POST /api/events/register`.
  - Key fields: payment reference, receipt upload, contribution amount.
- **Validations & rules**:
  - Lifecycle: registration permitted only for active events before the deadline.
  - Deduplication: prevents multiple registrations per user/guest for the same event.
  - Member enforcement: guest entry restricted by the event's `AllowNonMembers` policy.
- **Dependencies**: Financial service, file storage (receipts).

### 2.3 Event Management (Admin)
- **Business description**: CRUD operations for events, including registration capping and deadline management.
- **User roles**: Admin, SuperAdmin.
- **Inputs / outputs**: Screen `/admin/events`; API `POST /api/events/create`, `PUT /api/events/update`.
- **Dependencies**: Admin controller.

---

## 3. Financial Management & Payments

The platform operates without live payment-gateway credentials. All payment methods are administrator-configurable and function manually (see the [Payment Workflow](PAYMENT_GATEWAY_WORKFLOW.md)).

### 3.1 Manual, Admin-Configurable Payments
- **Business description**: Members pay through administrator-configured channels and upload proof; administrators verify and approve.
- **User roles**: Member (pays), Admin (verifies).
- **Inputs / outputs**:
  - Admin configures wallet, bank, and mobile-financial-service (bKash / Nagad / bank transfer) instructions.
  - Member uploads a payment reference and receipt image against a due or event registration.
  - API: financial endpoints for recording and approving payments; optional gateway endpoints (`POST /api/gateways/initiate`, `POST /api/gateways/callback`) exist but are inactive without gateway keys.
- **Validations & rules**:
  - Verification: an administrator confirms the paid amount matches the expected fee/due before approving.
  - Optional automation: if gateway keys are later supplied, callback handling can auto-verify amounts — not required for operation.
- **Dependencies**: Financial service, file storage.

### 3.2 Financial Ledger & Audit
- **Business description**: Tracks all income and expenses with categorized, audit-ready ledger entries.
- **User roles**: Admin (Finance), SuperAdmin.
- **Inputs / outputs**: Screen `/admin/ledger`; API `GET /api/financialledger`.
- **Dependencies**: Financial ledger controller.

### 3.3 Payment & Fee Configuration (Admin)
- **Business description**: Manages membership fees, registration costs, and payment-channel settings.
- **User roles**: SuperAdmin.
- **Inputs / outputs**:
  - Screen: `/admin/fee-configs`.
  - API: `GET /api/financials/fees/config`, `POST /api/financials/fees/config`, `PUT /api/financials/fees/config`.
- **Validations & rules**:
  - Temporal logic: fees applied based on the most recent `EffectiveDate` relative to the current year.
- **Dependencies**: Financials controller.

---

## 4. Networking & Social Features

### 4.1 Alumni Directory (Search & Networking)
- **Business description**: High-performance infinite-scroll directory for finding alumni.
- **User roles**: Public (limited), Member (full).
- **Inputs / outputs**:
  - Screen: `/directory`.
  - API: `GET /api/networking/search`, `GET /api/networking/member/{id}`.
  - Key fields: search query, batch/department filters.
- **Validations & rules**: results are strictly governed by each member's privacy toggles; masked fields never leak in response DTOs.
- **Dependencies**: Networking service.

### 4.2 Professional Job Hub
- **Business description**: Internal portal for sharing and applying for job opportunities within the alumni network.
- **User roles**: Member.
- **Inputs / outputs**: Screen `/portal/jobs`; API `GET /api/jobs`, `POST /api/jobs`.
- **Dependencies**: Job Hub service.

### 4.3 Direct Peer Messaging
- **Business description**: Secure real-time channel for members to network without exposing private contact data.
- **User roles**: Member.
- **Inputs / outputs**:
  - Screen: chat overlay or `/portal/chat`.
  - API: `POST /api/chat/send`, `GET /api/chat/history/{otherId}`.
- **Dependencies**: Chat service, real-time (SignalR) hub.

### 4.4 Gamification & Member Standing
- **Business description**: Rewards member engagement with contribution points, ranks, and category badges.
- **User roles**: Member.
- **Inputs / outputs**:
  - API: `GET /api/profile` (includes points/rank).
  - Logic: points awarded for `PROFILE_VERIFIED` (50 pts) and `EVENT_ATTENDANCE` (100 pts).
- **Validations & rules**: category thresholds — Legend (1000+), Elite (500+), Active (200+).
- **Dependencies**: Gamification service.

---

## 5. Governance & Operations

### 5.1 Executive Committee (EC) Management & Governance
- **Business description**: Manages committee periods, roles, and historical governance records.
- **User roles**: Admin, SuperAdmin.
- **Inputs / outputs**:
  - Screen: `/admin/governance`.
  - API: `POST /api/admin/governance/periods`, `POST /api/admin/governance/periods/{id}/members`.
- **Validations & rules**:
  - Continuity: prevents temporal overlaps between EC periods.
  - Exclusivity: core roles (President, GS, Treasurer) must be unique per term.
  - Eligibility: restricted to members with 'Active' status.
- **Dependencies**: Governance service.

### 5.1a Constitution Hub & Election Document Library
- **Business description**: Publishes the ratified constitution and the election procedure documents to
  everyone, and lets Voting Members record a position on the active version.
- **User roles**: Public (read), Member (vote), Admin/SuperAdmin (publish new versions).
- **Inputs / outputs**:
  - Screens: `/constitution` (full text, in-page article ToC, PDF download, collapsible version history),
    `/elections` (the seven `docs/Elections/*.md` documents, viewable and downloadable, with the forms
    handbook split into 18 individually printable forms), `/governance` (member ratification card).
    `Constitution` is reached from the footer reference links, not the public top nav.
  - Each split form renders as a **ready-to-use A4 sheet**, not a specimen: association letterhead,
    reference/date rule, form code chip, ruled write-on fields, tick-box lists, signature grid and
    seal box. `renderFormMarkdown` (`core/utils/markdown.util.ts`) drives it from a small directive
    DSL in the source markdown (`:: grid`, `:: sign`, `:: lines`, `Label: ____`, `[ ]`).
  - API: `GET /api/governance/constitution`, `GET /api/governance/constitution/history`,
    `POST /api/governance/constitution/{id}/vote`.
- **Validations & rules**:
  - One vote per member per version, enforced by a unique `(ConstitutionId, MemberId)` index.
  - Voting rights are limited to Founding, Executive and General members (Article III Section K);
    Associate, Honorary and Advisory members may read but not ratify.
  - The published text is the ratified v4.2 document (effective 01 Jul 2026), kept in sync at boot
    by `ConstitutionSeeder` because `EnsureCreated()` never re-seeds an existing database.
  - **Every surface follows the latest ratified version automatically.** No page pins a version:
    the reader renders whichever row is `IsActive`, the landing hero routes to `/constitution`
    rather than to a PDF, and superseded versions stay readable in the history panel with their
    own `PdfUrl`. Republishing is one command — see `docs/CONSTITUTION_PUBLISHING.md`.
  - The letterhead carries **no hardcoded organisational text**: name, crest, address, phone, email,
    motto and founding date (`branding.establishedOn`) all come from `OrgConfigService`, so a
    re-branded deployment prints its own forms without a code change.
  - The sheet is a paper document on both themes: the `--paper-*` tokens are defined once in
    `:root` in `styles.scss` and deliberately have no dark-theme override, so the form is
    ink-on-white on screen and on the printer alike (`@page { size: A4 portrait; margin: 14mm 13mm }`).
- **Dependencies**: Governance service, `ConstitutionSeeder`, in-repo markdown renderer
  (`core/utils/markdown.util.ts`) — no markdown npm dependency.
  `tools/constitution/publish_constitution.py` (PyMuPDF) is a build-time documentation tool, not
  an application dependency.

### 5.2 Bulk Member Import & Export
- **Business description**: Excel-to-database bridging for migrating legacy records and exporting registry data.
- **User roles**: SuperAdmin.
- **Inputs / outputs**:
  - Screen: `/admin/members`.
  - API: `POST /api/admin/members/import`, `GET /api/admin/members/import/export`.
- **Validations & rules**:
  - Deduplication: automatic NID/Email collision detection.
  - Auto-provisioning: creation of user accounts with NID-based credentials during import.
- **Dependencies**: Member Import service.

---

## 6. In-App Assistant

The assistant runs entirely on internal data with a rule-based engine — it has no external LLM dependency. The service is structured so a generative provider could be added later without changing its API surface.

### 6.1 Alumni Assistant
- **Business description**: A rule-based assistant that helps members find alumni and information by mapping their questions to internal directory lookups.
- **User roles**: Member.
- **Inputs / outputs**: Screen `/portal/assistant`; API `POST /api/assistant/ask`.
- **Validations & rules**: Intent/keyword classification for years, sectors, and help topics (no generative model call).
- **Dependencies**: Assistant service, Networking service (directory data).

### 6.2 Intelligent Support Chat
- **Business description**: Real-time support for common queries and system navigation.
- **User roles**: Public, Member.
- **Inputs / outputs**: Screen — floating chat widget; API `POST /api/chat/message`.
- **Dependencies**: Chat service.

---

## 7. Global Content Management (CMS)

### 7.1 News & Notices
- **Business description**: One board for both association news and official notices, discriminated by a `PostType` field on the same `NewsPost` entity. News may carry an image; notices may additionally carry a PDF document. Admins manage both from a single admin screen with a News/Notice tab filter.
- **User roles**: Public (read), Member (may submit *news* articles for approval), Admin (CRUD on both). **Notices are admin-post-only** — the member-facing submit endpoint rejects `PostType.Notice` from non-admins.
- **Inputs / outputs**: Screens `/news` (public + portal feed, tab-filtered; `/news?type=Notice` deep-links the Notices tab from the public nav) and `/admin/news`; APIs `GET /api/news?postType=`, `POST /api/news`, `POST /api/news/upload-image`, `POST /api/news/upload-document` (AdminOnly, PDF, 10 MB).
- **Dependencies**: News service, file storage, file-validation service.

### 7.1a Site Content CMS (About Us / Contact intro)
- **Business description**: Admin-editable content blocks that render the public About Us page and the Contact page intro, so institutional copy changes without a redeploy. Each block is a keyed record (`about-origin`, `about-association`, `about-logo`, `about-objectives`, `contact-intro`) with a title, rich-text body, display order, and active flag. Seeded from the GHCAA Constitution; the public page falls back to its previous static markup if the API returns nothing.
- **User roles**: Public (read active blocks), Admin (CRUD, reorder, activate/deactivate).
- **Inputs / outputs**: Screens `/about`, `/contact`, `/admin/site-content`; APIs `GET /api/site-content?group=` (anonymous), `GET /api/site-content/admin`, `POST`/`PUT /{id}`/`DELETE /{id}` (all AdminOnly).
- **Dependencies**: SiteContent service, `HtmlSanitizer` (body HTML is sanitized server-side on every write), shared rich-text editor component.

### 7.1b Contact details configuration
- **Business description**: The Contact page's address/phone/email/social panel is driven by `OrgConfig.Contact` rather than hardcoded markup — including an **on-campus address**, a list of phone numbers, and an optional map embed URL, all editable from the SuperAdmin org-config screen and shared by web and mobile.
- **User roles**: Public (read), SuperAdmin (edit).
- **Inputs / outputs**: Screens `/contact`, `/admin/org-config`; API `GET`/`PUT /api/org-config`.
- **Dependencies**: OrgConfig service. The admin-supplied map URL is treated as untrusted: it passes an allow-list check before the iframe is trusted, and no iframe renders if it fails.

### 7.2 Media Gallery & Albums
- **Business description**: Visual records of association history categorized by events. Members may also create their own albums and upload photos to them; member-submitted albums/photos enter a `Pending` approval queue and only appear publicly once an Admin/SuperAdmin approves them (or are hidden with a rejection reason).
- **User roles**: Public (read approved only), Member (create own albums, upload photos, submit for approval), Admin (CRUD, approve/reject submissions).
- **Inputs / outputs**: Screen `/gallery`; API `POST /api/gallery`, plus member-album and admin approve/reject endpoints under `/api/gallery`.
- **Dependencies**: Gallery service, file storage, shared admin-notification/email fan-out on new submissions (see AdminNotificationService).

### 7.3 Theme Management (Special Days)
- **Business description**: Dynamic UI transformation for special occasions (e.g., Independence Day).
- **User roles**: Admin.
- **Inputs / outputs**: Screen `/admin/themes`; API `POST /api/theme/activate`.
- **Dependencies**: Theme service.

---

## Portal Architecture & Scopes

The platform is divided into three operational scopes, each tailored to a distinct set of users.

### 1. Public Portal (the front gate)
Open access for alumni and the general public.
- Landing hub with dynamic sections (purpose, symbolism, EC highlights).
- Real-time stats counters for registered members, batches represented, and events.
- Multi-step registration wizard with NID sanitization, email OTP verification, and Govt. Haraganga College academic validation.
- Public alumni directory with infinite scroll and batch/professional filtering (privacy-governed).
- News feed and event calendar with archival support.

### 2. Member Portal (the alumni hub)
Secured area for approved alumni using membership-number credentials.
- Personal dashboard with membership status and recent activity.
- Alumni assistant (rule-based guided search over alumni by batch, sector, or profession).
- Self-service profile with dynamic privacy toggles and editable academic/professional history.
- Networking engine: peer chat and career (job) hub.
- Financial transparency: membership-due tracking, payment history, and manual payment-proof upload.
- Event participation: registration for alumni events with manual payment handling.

### 3. Admin Command Center (governance & management)
High-privilege portal for the Executive Committee and system administrators.
- Global analytics: membership growth, financial balance, and pending-workflow metrics.
- Membership governance: side-by-side document review, approval-triggered account creation, and soft-delete archive with one-click restore.
- Management suites: EC management, communication hub (segmented bulk email with HTML templates), media gallery CMS, and payment-channel configuration.
- Power tools: Excel bulk import with column mapping, formatted Excel export, and high-speed member lookup by ID/NID.

---

## Developer Experience & Infrastructure

### Data seeding & transformation
- JSON-based seed data (`members.json`, `users.json`) synchronized into the database on first creation.
- Utilities to batch-transform legacy member data (e.g., generating NID-based credentials with BCrypt hashing).

Note: on a brand-new empty database, the runtime builds the schema and applies seed data via EF Core `EnsureCreated()`. On an existing (already-provisioned) database — preprod/production — `MigrationBootstrapper` runs at boot instead: it baselines any migration whose effect already exists in the live schema (without re-running it) and applies genuinely pending migrations for real via the EF migrator, so schema changes shipped as migrations do reach preprod on the next deploy. Editing seed JSON (`HasData`) still does not retroactively update an already-created database — that still requires a runtime syncer like `ConstitutionSeeder` (see `docs/CONSTITUTION_PUBLISHING.md`).

### Technical infrastructure
- Centralized soft-delete architecture using EF Core global query filters — queries never need to check `IsArchived` manually.
- Clean Architecture separation between domain models, application DTOs, and infrastructure services.
- SCSS design system with CSS custom properties for colors, spacing, and glassmorphism tokens; semantic theme tokens flip between fully-styled light and dark modes.

### API & tooling
- Swagger/OpenAPI interactive documentation for manual API verification.
- Excel-to-database member import utility with column mapping and auto-generation of missing legacy data.
- File storage abstraction (local storage today, with an interface ready for cloud/S3/Azure expansion).
