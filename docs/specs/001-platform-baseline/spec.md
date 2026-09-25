# Feature Specification: GHCAA Platform Requirements

**Feature Branch**: `001-platform-baseline`

**Created**: 2026-09-20

**Status**: Index to the as-built domain specs 012 to 020 (updated 2026-09-25)

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

## Domain spec index

The ten user stories that used to sit here have moved into nine domain specs, 012 to 020. Each one
was written from the code on 2026-09-25. Each has numbered FRs, an Evidence table that traces every
FR to its route, service, Angular screen, Flutter screen and test, a Gaps list, and an
"Enhancements: modularisation and reusability" section. The cross-layer requirements, key entities
and measurable outcomes below still apply to every domain.

| Former story | Domain spec | Controllers owned |
|---|---|---|
| 1 Apply for membership, 2 Authenticate and complete the lifecycle | [012 Membership lifecycle and auth](../012-membership-lifecycle-auth/spec.md) | Registration, PendingApprovals, MemberImport, Auth, AdminSocialAuth, Roles |
| 3 Manage a profile and identity documents | [013 Profile, family and identity](../013-profile-family-identity/spec.md) | Profile, FamilyLink, SecureFiles, CredentialVerification |
| 4 Discover and attend events, 8 (public content part) | [014 Events, news, gallery and content](../014-events-news-gallery-content/spec.md) | Events, News, Gallery, SiteContent, Archive |
| 5 Pay dues and keep financial governance | [015 Payments and finance](../015-payments-finance/spec.md) | Financials, FinancialLedger, Gateways, PaymentConfig, Campaigns, Scholarships |
| 6 Find alumni and exchange opportunities | [016 Networking and careers](../016-networking-careers/spec.md) | Networking, JobHub, Mentorship |
| 7 Communicate in real time, 9 (support channel part) | [017 Communication](../017-communication/spec.md) | Messaging, Communication, MemberCommunications, Notification, Forum, Contact |
| 8 Read and administer governance | [018 Governance, elections and polls](../018-governance-elections-polls/spec.md) | Governance, AdminGovernance, Elections, AdminElections, Poll, AdminPoll |
| 9 Use the rule-based assistant | [019 Assistant](../019-assistant/spec.md) | Assistant |
| 10 Operate a secure, configurable platform | [020 Platform and operations](../020-platform-operations/spec.md) | OrgConfig, Theme, Lookups, Health, Admin, Activity, AdminErrorLogs, AdminDevTracker |

All 45 files in `GHCAA.API/Controllers` are owned by one domain spec. AdminController, Events,
Networking and PendingApprovals are also named in other specs, but only where a boundary is
described.

### Feature specs

These specs cover one feature or work package each. The domain specs link to them and do not
repeat them.

| Spec | Subject |
|---|---|
| [002](../002-workflow-contracts-and-validation/spec.md) | Workflow contracts and validation detail |
| [003](../003-alumni-programs-and-verification/spec.md) | Alumni programmes and credential verification |
| [004](../004-pending-work-packages/spec.md) | Pending work packages |
| [005](../005-academic-organisation-profile-flag/spec.md) | Academic organisation-profile flag |
| [006](../006-project-agentra-ai/spec.md) | Agentra AI |
| [007](../007-login-authentication-status/spec.md) | Login authentication status flow |
| [008](../008-idempotent-member-batch-import/spec.md) | Idempotent member batch import |
| [009](../009-campaigns-scholarships-archive/spec.md) | Campaigns, scholarships and archive (Work Package 37) |
| [010](../010-ad-hoc-reporting/spec.md) | Ad-hoc reporting for admins |
| [010](../010-election-engine-fixes/spec.md) | Election engine and post-ship fixes |
| [011](../011-election-module-redesign/spec.md) | Election module client redesign |

Two folders share the number 010. Tracker item 84.31 in docs/TODO.md decides whether one is
renumbered.

### Enhancement roll-up across domains

The nine domain specs hold 55 ENH items: 6 at P1, 25 at P2 and 24 at P3. One of the P1 items
(015 ENH-005) needs no change. The same problems come up in several domains:

| Theme | Where it appears | Suggested shared fix |
|---|---|---|
| Payment options and fees hard-coded in a client | 012 ENH-004 (web registration fee of 500), 015 ENH-006 and ENH-007 (mobile event payment sheet lists two gateways) | Read the fee and the active gateway list from OrgConfig and PaymentConfig, as the mobile financial portal already does |
| Membership rules hard-coded | 018 ENH-007 (voting membership types), 018 ENH-008 and ENH-009 (form codes, seat names) | Move them into OrgConfig so another organisation can change them |
| Upload size limits written as literals | 013 ENH-004, 014 ENH-005, 020 ENH-005 | One upload policy in configuration, read by every upload route |
| Listing, request and admin approval built again per entity | 014 ENH-002 (events and gallery), 015 ENH-003 and ENH-004 (campaigns and scholarships), 016 ENH-003 (mentorship), 017 ENH-002 | A shared module shape: entity, request, approval state, admin queue. Build it when the next module (84.19 meetings) needs it, not before |
| The same logic copied inside one domain | 013 ENH-001 (two family services), 018 ENH-002 and ENH-003, 020 ENH-001 (admin stats twice) | Delete the unused copy |
| No shared client component | 018 ENH-006 (ballot), 017 ENH-004 | Build a shared Angular component and Flutter widget the first time a second screen needs it |

### Feature gaps against a standard association platform

Tracker item 84.18 reviews the entities against a standard association platform. The result will
be added here. Items 84.19 (meetings with invitees, agenda, minutes and action items) and 84.20
(tasks and notes) depend on it.


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
