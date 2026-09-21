# Feature Specification: GHCAA Platform Requirements

**Feature Branch**: `001-platform-baseline`

**Created**: 2026-09-20

**Status**: Requirements baseline

**Input**: The current GHCAA codebase, including the Domain, Application,
Infrastructure, API, Web, Mobile, and test projects.

## Purpose and scope

This specification defines the behavior evidenced by the current GHCAA
implementation. It is written as buildable requirements while distinguishing
implemented behavior, compatibility behavior, and gaps visible in code or
tests. The codebase and executable tests are the source of truth for this
baseline.

The product shall consist of one shared ASP.NET Core API, an Angular Web
portal, and a Flutter Mobile application. The API contract, privacy rules,
state transitions, and date formats shall be consistent across both clients.

## User Scenarios & Testing

### User Story 1 - Apply for membership (Priority: P1)

As an alumnus, I want to complete a guided registration so that the
association can verify my identity and education before granting access.

**Independent Test**: Submit valid and invalid wizard payloads through the
registration API and the Web and Mobile registration flows. Verify validation,
OTP, duplicate detection, attachments, academic qualification, and status
responses without requiring later member features.

**Acceptance Scenarios**:

1. **Given** a registration has missing required personal data, **When** it is
   submitted, **Then** the API shall return field-level validation errors.
2. **Given** an applicant has no academic record from Govt. Haraganga College,
   **When** registration is submitted, **Then** the API shall reject it.
3. **Given** NID, email, or mobile matches an existing non-archived record,
   **When** registration is submitted, **Then** the API shall reject the
   duplicate without creating a second member.
4. **Given** an applicant requests email verification, **When** the OTP is
   valid and unexpired, **Then** the email shall be marked verified and the
   applicant may continue.
5. **Given** an uploaded image exceeds the documented image limit, **When** it
   is processed, **Then** the system shall compress or reject it according to
   the shared upload rules.

### User Story 2 - Authenticate and complete the member lifecycle (Priority: P1)

As a member or administrator, I want secure authentication and lifecycle
controls so that access follows membership status and role.

**Independent Test**: Exercise password login, refresh, logout, password
reset, social login, status transitions, admin approval, rejection,
reactivation, archive, and member guards through API, Web, Mobile, and
controller tests.

**Acceptance Scenarios**:

1. **Given** an email is not verified, **When** login is attempted, **Then**
   the API shall deny access and identify the verification requirement.
2. **Given** an administrator approves a valid application, **When** approval
   completes, **Then** the system shall assign a unique `GHC-YYYY-XXXX`
   membership number and provision the login account.
3. **Given** a member is inactive or terminated, **When** protected access is
   requested, **Then** the API and both clients shall apply the documented
   state permissions.
4. **Given** a security stamp changes, **When** an existing token is used,
   **Then** middleware shall invalidate the session.
5. **Given** an administrator starts a destructive or financial action,
   **When** the step-up claim is absent or expired, **Then** a purpose-scoped
   email OTP shall be required before the action proceeds.
6. **Given** a user has an administrator role but no linked member, **When**
   member-only Web navigation is attempted, **Then** the member guard shall
   route the user to the admin area.

### User Story 3 - Manage a complete profile and identity documents (Priority: P1)

As an active member, I want to maintain my personal, academic, professional,
and document information so that my profile and association identity remain
accurate.

**Independent Test**: Read and update profiles, academic and professional
records, privacy flags, photos, signatures, IDs, and certificates from both
clients, then verify the eligibility and masking rules.

**Acceptance Scenarios**:

1. **Given** mandatory profile fields, a qualifying academic record, and a
   professional record are complete, **When** profile completion is calculated,
   **Then** the member shall be eligible for the next onboarding gate.
2. **Given** a member is not active, **When** an ID card or certificate is
   requested, **Then** generation shall be denied.
3. **Given** a member disables public email, mobile, address, NID, or family
   visibility, **When** another member requests the profile, **Then** the
   corresponding value shall be masked or omitted in the response DTO.
4. **Given** a member uploads a document, **When** the file is not an allowed
   type or size, **Then** the API shall reject it and expose an explicit
   error.

### User Story 4 - Discover and attend events (Priority: P1)

As a member or permitted guest, I want to discover events and register so that
participation, fees, capacity, and attendance are tracked.

**Independent Test**: Create events, publish them, register members and
guests, exceed capacity, process payments, approve registrations, check in QR
codes, and close events through API and client tests.

**Acceptance Scenarios**:

1. **Given** an event is unpublished, **When** a public user requests the event
   catalog, **Then** the event shall not appear.
2. **Given** registration is outside the event window or the event has ended,
   **When** registration is submitted, **Then** the API shall reject it.
3. **Given** capacity is full, **When** another eligible registration arrives,
   **Then** the documented waitlist transition shall apply.
4. **Given** non-member entry is disabled, **When** a guest attempts to
   register, **Then** the API shall reject the guest registration.
5. **Given** a valid member QR identity is scanned by an authorized
   coordinator, **When** check-in is recorded, **Then** attendance and
   contribution points shall be updated once.

### User Story 5 - Pay dues and maintain financial governance (Priority: P1)

As a member, I want clear payment instructions and receipts. As an
administrator, I want to verify payments and maintain an audit-ready ledger.

**Independent Test**: Configure fees and manual payment channels, submit proof,
verify or reject a payment, generate a receipt, inspect dues and history, and
create income and expense records using the API and both client surfaces.

**Acceptance Scenarios**:

1. **Given** no live gateway credentials exist, **When** an administrator
   configures wallet, bank, bKash, Nagad, or other manual instructions,
   **Then** members shall be able to use those instructions without gateway
   keys.
2. **Given** a member uploads proof against a due or event payment, **When**
   an administrator reviews it, **Then** the amount and payment state shall
   be verified before completion.
3. **Given** a fee has effective dates, **When** an applicable fee is requested,
   **Then** the most recent effective configuration shall be selected.
4. **Given** a payment is completed, **When** the member requests a receipt or
   history, **Then** the completed payment and receipt shall be consistent.
5. **Given** a ledger record is created, **When** it is later viewed or
   exported, **Then** its category, type, amount, date, and audit data shall
   remain intact.
6. **Given** optional gateway integration is not configured, **When** a
   gateway route is called, **Then** it shall fail explicitly rather than
   impersonate a successful payment.

### User Story 6 - Find alumni and exchange opportunities (Priority: P2)

As a member, I want to find alumni, jobs, mentorship opportunities, and family
connections without exposing private contact information.

**Independent Test**: Search and page through the directory, open a
privacy-filtered member profile, create and moderate job posts, send
mentorship and family-link requests, and verify contribution points and
badges.

**Acceptance Scenarios**:

1. **Given** directory filters and a page or cursor, **When** a search is
   requested, **Then** results shall be ordered and paged consistently.
2. **Given** a member has disabled a field, **When** directory or profile
   results are returned, **Then** that field shall not leak through either
   client.
3. **Given** a member creates a job or mentorship post, **When** moderation is
   required, **Then** the post shall remain unavailable publicly until the
   documented approval state is reached.
4. **Given** a family link request is sent, **When** the recipient accepts,
   rejects, or cancels it, **Then** both parties shall see the resulting state.
5. **Given** qualifying engagement occurs, **When** points are awarded, **Then**
   the profile shall expose the calculated rank and badge thresholds.

### User Story 7 - Communicate in real time (Priority: P2)

As a member, I want private peer messaging and notifications so that I can
communicate without sharing private contact details.

**Independent Test**: Send messages, retrieve history and recent conversations,
mark messages read, receive notifications, disconnect and reconnect SignalR,
and verify authorization and error states in Web and Mobile tests.

**Acceptance Scenarios**:

1. **Given** two authorized members, **When** one sends a message, **Then** the
   recipient shall receive it through the API and real-time channel.
2. **Given** a message is marked read, **When** unread state is requested,
   **Then** the message shall no longer count as unread.
3. **Given** a user is unauthorized or disconnected, **When** a message action
   fails, **Then** the client shall show an explicit recoverable state.
4. **Given** a member has received an outbound email or SMS, **When** the
   member opens communication history, **Then** only that member's paginated
   records shall be returned with channel, status, scope, content, and
   timestamp.
5. **Given** a configured communication template uses the SMS channel, **When**
   delivery is requested, **Then** the SMS provider shall be used and the
   delivery result shall be logged as sent, failed, or unavailable.

### User Story 8 - Read and administer governance and public content (Priority: P2)

As a public reader or member, I want current association information. As an
administrator, I want controlled publishing and configuration.

**Independent Test**: Manage EC periods, constitution versions, election forms,
site content, contact configuration, news, notices, galleries, albums, themes,
communication templates, and polls through their API and client tests.

**Acceptance Scenarios**:

1. **Given** a new constitution is ratified, **When** the public reader opens
   the constitution, **Then** every surface shall use the active version
   without a pinned version number.
2. **Given** a member is eligible to vote, **When** a vote is submitted twice,
   **Then** the second vote shall be rejected by the one-vote rule.
3. **Given** an election form is rendered, **When** it is printed, **Then** it
   shall be an organisation-configured A4 form with letterhead, fields,
   signatures, and seal space on an ink-on-white paper surface.
4. **Given** an admin edits site content, contact details, or a theme, **When**
   the public client reads it, **Then** the active sanitized configuration
   shall be used with a safe fallback where defined.
5. **Given** a member submits a notice, **When** the caller is not an
   administrator, **Then** the API shall reject it; member news submissions
   shall retain their approval path.
6. **Given** a poll is expired, **When** a member attempts to vote, **Then**
   voting shall be closed while permitted results remain readable.
7. **Given** an election is in a controlled phase, **When** an authorized
   actor performs a valid lifecycle action, **Then** the persisted election
   workflow shall enforce the phase, eligibility, officer, and one-vote rules.
8. **Given** a member submits a ballot, **When** the vote is stored, **Then**
   the ballot shall not contain the member identity recorded on the voter roll.

### User Story 9 - Use the rule-based assistant and support channels (Priority: P3)

As a member, I want answers to portal and policy questions and a route to
support without an external generative dependency.

**Independent Test**: Submit supported and unsupported intents, directory
lookups, policy questions, and contact requests through Web, Mobile, API, and
service tests.

**Acceptance Scenarios**:

1. **Given** a question matches a supported intent, **When** it is submitted,
   **Then** the assistant shall return the matching internal result or policy
   answer.
2. **Given** a question cannot be classified, **When** it is submitted, **Then**
   the assistant shall return a clear limitation and support path.
3. **Given** a contact request contains invalid data, **When** it is submitted,
   **Then** validation shall reject it without silently discarding the request.

### User Story 10 - Operate a secure, resilient, configurable platform (Priority: P1)

As an operator, I want consistent security, storage, deployment, configuration,
and failure handling so that the platform remains safe and maintainable.

**Independent Test**: Run authorization, archive, rate-limit, upload,
configuration, retry, offline, logging, build, API snapshot, and cross-client
test suites against the API, Web, and Mobile projects.

**Acceptance Scenarios**:

1. **Given** an archived record exists, **When** a normal query runs, **Then**
   the global archive filter shall hide it while an explicit audit path may
   retrieve it.
2. **Given** authentication, registration, or general API traffic exceeds its
   tiered limit, **When** requests continue, **Then** the applicable rate-limit
   response shall be returned.
3. **Given** a transient network failure occurs on Mobile, **When** the request
   is retried, **Then** retries shall be limited to three attempts with
   exponential backoff and shall not retry authentication or other 4xx errors.
4. **Given** a required API call fails in either client, **When** the screen
   renders, **Then** loading, error, empty, and retry states shall remain
   usable and shall not present an error as successful empty data.
5. **Given** an institution profile is selected through configuration, **When**
   the system builds, **Then** organisation content, defaults, assets, and
   client branding shall come from the selected profile with documented
   fallback behavior.
6. **Given** a displayed date is rendered, **When** it reaches a user, **Then**
   the display format shall be `dd-MM-yyyy`; date-only API values shall use
   ISO `yyyy-MM-dd`.

## Cross-layer requirements

- **CLR-001**: Domain entities, enums, and constants shall remain free of
  framework and persistence concerns.
- **CLR-002**: Application DTOs, interfaces, validators, and security
  primitives shall define shared business contracts.
- **CLR-003**: Infrastructure shall implement persistence, file storage,
  provider selection, external services, and payment strategies.
- **CLR-004**: API controllers shall delegate to Application services and
  preserve middleware, authorization, audit, and rate-limit behavior.
- **CLR-005**: Angular shall use standalone components, signals, typed services,
  shared controls, and central design tokens.
- **CLR-006**: Flutter shall use Riverpod, `go_router`, `Dio`, `AppConfig`,
  shared theme/widgets, and the same API semantics as Web.
- **CLR-007**: A route, request field, or response field shared by the clients
  shall remain compatible with existing callers, be reflected in both clients,
  and be covered by relevant server and client tests.

## Key entities

Member, User, AcademicRecord, ProfessionalRecord, Event, EventRegistration,
PaymentHistory, MembershipFeeConfig, FinancialRecord, Ledger, ChatMessage,
Notification, Job, MentorshipRequest, FamilyLinkRequest, ConstitutionVersion,
ECPeriod, ECMember, NewsPost, SiteContent, ContactConfiguration, Gallery,
EventPhoto, SpecialDayTheme, Poll, LookupItem, Role, and ActivityLog.

## Measurable outcomes

- **SC-001**: Every implemented platform domain has at least one independent
  story and acceptance scenario.
- **SC-002**: Each backend, Web, and Mobile boundary has explicit requirements
  and a verification path.
- **SC-003**: Every API domain exposed by the current controllers and consumed
  by a client has a named .NET, Angular, Flutter, and test surface in the
  cross-layer trace.
- **SC-004**: The API compatibility rule preserves callers that omit the
  optional networking cursor.
- **SC-005**: The full backend, Web, Mobile, API snapshot, and graph checks can
  be run without committing, deploying, or requiring live payment credentials.

## Scope notes

- This specification does not invent behavior beyond the current source,
  generated client contracts, and executable tests.
- Existing implementation gaps are recorded where the code or tests expose
  them; they are not silently represented as completed behavior.
- The specification does not authorize source, schema, dependency, deployment,
  commit, or push changes.
