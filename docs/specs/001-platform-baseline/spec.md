# Feature Specification: GHCAA Platform Requirements

**Feature Branch**: `001-platform-baseline`

**Created**: 2026-09-20

**Status**: Index to the as-built domain specs 012 to 020 and the feature specs 002 to 011 and 021 to 028 (updated 2026-11-06)

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

Each of the ten former stories has one owning domain spec. Where a story also had a part that
now sits in a second spec, the last column names it.

| Former story | Owning domain spec | Part also in | Controllers owned by the domain spec |
|---|---|---|---|
| 1 Apply for membership | [012 Membership lifecycle and auth](../012-membership-lifecycle-auth/spec.md) | none | Registration, PendingApprovals, MemberImport, Auth, AdminSocialAuth, Roles |
| 2 Authenticate and complete the lifecycle | [012 Membership lifecycle and auth](../012-membership-lifecycle-auth/spec.md) | none | same as story 1 |
| 3 Manage a profile and identity documents | [013 Profile, family and identity](../013-profile-family-identity/spec.md) | none | Profile, FamilyLink, SecureFiles, CredentialVerification |
| 4 Discover and attend events | [014 Events, news, gallery and content](../014-events-news-gallery-content/spec.md) | none | Events, News, Gallery, SiteContent, Archive |
| 5 Pay dues and keep financial governance | [015 Payments and finance](../015-payments-finance/spec.md) | none | Financials, FinancialLedger, Gateways, PaymentConfig, Campaigns, Scholarships |
| 6 Find alumni and exchange opportunities | [016 Networking and careers](../016-networking-careers/spec.md) | none | Networking, JobHub, Mentorship |
| 7 Communicate in real time | [017 Communication](../017-communication/spec.md) | none | Messaging, Communication, MemberCommunications, Notification, Forum, Contact |
| 8 Read and administer governance | [018 Governance, elections and polls](../018-governance-elections-polls/spec.md) | public content pages: spec 014 (events, news, gallery and content) | Governance, AdminGovernance, Elections, AdminElections, ElectionAppointments, ElectionPersonas, ElectionRulesUnlock, Poll, AdminPoll |
| 9 Use the rule-based assistant | [019 Assistant](../019-assistant/spec.md) | support channel: spec 017 (communication) | Assistant |
| 10 Operate a secure, configurable platform | [020 Platform and operations](../020-platform-operations/spec.md) | none | OrgConfig, Theme, Lookups, Health, Admin, Activity, AdminErrorLogs, AdminDevTracker |

`GHCAA.API/Controllers` holds 49 controller files (counted 2026-11-06). 48 of them are owned by one
domain spec. AdminController, Events, Networking and PendingApprovals are also named in other
specs, but only where a boundary is described. The 49th, `ElectionContentController`
(`api/election-content`, election content pages by kind and key), is not named in any domain spec.
It sits with the election controllers of spec 018 (governance, elections and polls) by subject.
Naming it there is left for the owner, because the domain specs are outside the scope of this
index.

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
| [011](../011-election-module-redesign/spec.md) | Election module client redesign |
| [021](../021-pluggable-feature-modules/spec.md) | Pluggable feature modules (deferred, Work Package 91) |
| [022](../022-configurable-approval-workflows/spec.md) | Configurable approval workflows (draft, Work Package 92) |
| [023](../023-election-ballot-secrecy-and-standards/spec.md) | Election ballot secrecy and standards (draft, Work Package 37 items 37.1i to 37.1q) |
| [024](../024-performance/spec.md) | Performance pass across database, API, web and mobile (draft, Work Package 98) |
| [025](../025-org-config-pack-export-import/spec.md) | Export and import of the organisation profile and settings (draft, Work Package 62 items 62.56a to 62.56g) |
| [026](../026-meetings-and-action-items/spec.md) | Meetings and action items (owner approved 2026-11-04, being built; tracker items 84.19 and 84.20) |
| [027](../027-election-engine-fixes/spec.md) | Election engine and post-ship fixes (numbered 010 until 2026-11-01, when tracker item 84.31 ended the clash with 010 ad-hoc reporting) |
| [028](../028-poll-and-meeting-discussion/spec.md) | Comments on polls and meetings (owner approved 2026-11-04, being built; tracker item 84.71) |

### Enhancement roll-up across domains

The nine domain specs hold 55 ENH items: 6 at P1, 25 at P2 and 24 at P3. One of the P1 items
(spec 015, payments and finance, ENH-005) needs no change.

How the count was made (2026-11-06): in each of specs 012 to 020, the lines that start with
`- ENH-nnn` or `- **ENH-nnn**` were counted, each one being a bullet under the spec's
"Enhancements: modularisation and reusability" heading. The priority is the `(Pn` tag after the id.
Only those nine specs have such a section. No other spec has one, and a mention of an ENH id inside
an Evidence table row was not counted.

| Domain spec | ENH items | P1 | P2 | P3 |
|---|---|---|---|---|
| 012 Membership lifecycle and auth | 6 | 1 | 3 | 2 |
| 013 Profile, family and identity | 5 | 1 | 2 | 2 |
| 014 Events, news, gallery and content | 6 | 0 | 3 | 3 |
| 015 Payments and finance | 9 | 3 | 3 | 3 |
| 016 Networking and careers | 4 | 0 | 2 | 2 |
| 017 Communication | 6 | 0 | 3 | 3 |
| 018 Governance, elections and polls | 9 | 1 | 5 | 3 |
| 019 Assistant | 5 | 0 | 2 | 3 |
| 020 Platform and operations | 5 | 0 | 2 | 3 |
| Total | 55 | 6 | 25 | 24 |

The same problems come up in several domains:

| Theme | Where it appears | Suggested shared fix |
|---|---|---|
| Payment options and fees hard-coded in a client | 012 ENH-004 (web registration fee of 500), 015 ENH-006 and ENH-007 (mobile event payment sheet; fixed 2026-11-05 under 84.54) | Read the fee and the active gateway list from OrgConfig and PaymentConfig, as the mobile financial portal already does |
| Membership rules hard-coded | 018 ENH-007 (voting membership types), 018 ENH-008 and ENH-009 (form codes, seat names) | Move them into OrgConfig so another organisation can change them |
| Upload size limits written as literals | 013 ENH-004, 014 ENH-005, 020 ENH-005 | One upload policy in configuration, read by every upload route |
| Listing, request and admin approval built again per entity | 014 ENH-002 (events and gallery), 015 ENH-003 and ENH-004 (campaigns and scholarships), 016 ENH-003 (mentorship), 017 ENH-002 | A shared module shape: entity, request, approval state, admin queue. Build it when the next module (84.19 meetings) needs it, not before. Spec 021 records the later step of shipping modules as packages |
| The same logic copied inside one domain | 013 ENH-001 (two family services), 018 ENH-002 and ENH-003, 020 ENH-001 (admin stats twice) | Delete the unused copy |
| No shared client component | 018 ENH-006 (ballot), 017 ENH-004 | Build a shared Angular component and Flutter widget the first time a second screen needs it |

### Feature gaps against a standard association platform

Tracker item 84.18 reviewed each entity feature against what a standard association or alumni
platform offers. The baseline was the domain specs 012 to 020. Where a spec did not say whether a
feature exists, the Domain models, the controllers and the services were searched (2026-11-06).
Before a gap was listed, `docs/TODO.md` was searched for the feature name. A gap that an open item
already covers names that item and is not raised again.

Spec 026 (meetings and action items) and spec 028 (comments on polls and meetings) are being built.
Their features appear below as covered, not as gaps. Standalone notes and a general task manager are
not accepted scope (tracker item 84.20).

#### Already in the code (checked, not gaps)

| Feature | Evidence |
|---|---|
| Event check-in with a ticket code | `EventRegistration.TicketCode`, `IsCheckedIn`; route `POST api/events/admin/checkin/qr` |
| Event waitlist and guest registration | `AlumniEvent.HasWaitlist`; `EventRegistrationStatus.Waitlisted`; `EventRegistration.IsNonMember` |
| Event tasks and budget | `EventTask`, `EventBudget`; `api/events/admin/{eventId}/tasks` and `budget` |
| Donation and payment receipt PDF | `IFinancialService.GenerateTaxReceiptAsync` |
| Campaigns, pledges, donor tiers, scholarships | `Campaign`, `CampaignPledge`, `DonorRecognitionTier`, `Scholarship`; spec 015 (payments and finance) |
| Directory privacy switches | `Member.IsMobilePublic`, `IsEmailPublic`, `IsAddressPublic`, `IsNIDPublic`, `IsFamilyPublic` |
| Mentorship requests, job board with moderation | `MentorshipController`, `JobHubController`; spec 016 (networking and careers) |
| Bulk email and push to a batch or a type, dues reminder template | `CommunicationController`, template `FEE_REMINDER`; sent by an admin, see gap G2 |
| Credential verification, ID card, two-factor sign-in | spec 013 (profile, family and identity); `Totp` in `GHCAA.Application/Security` |
| Leaderboard of member contributions | `IGamificationService.GetLeaderboardAsync` |

#### Gaps already covered by an open tracker item

| Feature | Domain | Users served | Fits an alumni association | Covered by |
|---|---|---|---|---|
| Batch cohort pages and reunions | Events and profile | Members, batch representatives, admins | Yes | Item 37.4 (batch cohorts and reunions) |
| In Memoriam register | Content and profile | Members, admins, the public | Yes | Item 37.5 (In Memoriam register), spec 003 (alumni programmes) story 3 |
| Geographic chapters | Networking | Members, chapter coordinators, admins | Yes | Item 37.9 (geographic chapters), spec 003 story 5. Interest groups and clubs would be a variant of the chapter entity and are not raised separately |
| Annual impact report from the ledger | Finance | Admins, donors, the public | Yes | Item 37.10 (annual impact report) |
| Bengali and English interface | All clients | Members | Yes | Items 37.7 (bilingual UI) and 8.8 (unified localisation) |
| Meetings, agenda, minutes, attendance | Governance | EC, admins, members | Yes | Item 84.19, spec 026 (meetings and action items) |
| Action items tied to a meeting or an EC task | Governance | EC, admins | Yes | Item 84.20, spec 026 |
| Comments on polls and meetings | Governance and communication | Members, admins | Yes | Item 84.71, spec 028 (poll and meeting discussion) |
| Ad-hoc reports and Excel export of a result set | Platform | Admins | Yes | Work Package 90, spec 010 (ad-hoc reporting) |

#### New gaps, proposed for their own tracker items

| Id | Feature | Domain | Users served | Fits an alumni association | What the code shows |
|---|---|---|---|---|---|
| G1 | Cancel an event registration, and fill a freed seat from the waitlist | Events | Members, admins | Yes. Paid and capped events need it | `EventsController` has no cancel or withdraw route. `EventService` sets `Waitlisted` at registration (lines 288 to 341). The only way out of `Waitlisted` is the admin approve call |
| G2 | Scheduled reminders: dues due or overdue, and an event starting soon | Finance, events, communication | Members, admins | Yes. Dues drive the association's income | `MembershipDue` has `DueDate` and `IsPaid`. The `FEE_REMINDER` template exists, but only an admin sends it. The background services are `DataRetentionSweep` and `ExpiredRevokeSweep`. No event reminder code exists. Spec 026 leaves meeting reminders out of scope |
| G3 | Event feedback and short surveys | Events, communication | Members, admins | Yes. Used after events and reunions | The only question type is a poll vote (`Poll`, `PollOption`, `PollVote`). No rating, feedback or survey entity exists in the Domain models |
| G4 | Add an event to a calendar (`.ics` file) | Events | Members | Yes, low value | No `.ics` or calendar export code in any layer. Spec 026 leaves it out for meetings too |
| G5 | Mentor opt-in and a mentor filter in the directory | Networking | Members, admins | Yes. Mentoring is a core alumni service | `MentorshipService.SendRequestAsync` accepts any member as the mentor. `Member` and `ProfessionalRecord` have no mentor flag or topics. `MemberSearchFilterDto` has no mentor filter |
| G6 | Directory search by location | Networking | Members | Yes | `MemberSearchFilterDto` has no location field. `NetworkingService` shows `ProfessionalRecord.Location` of the current job but never filters on it. `Member` stores no home city or country. Related to item 37.9 (chapters), which would add regions |
| G7 | Event volunteer sign-up | Events | Members, admins | Yes | `EventTask.AssignedMemberId` is set by an admin. No route lets a member offer to take a task |
| G8 | Refund handling | Finance | Admins, members | Yes, where paid events or dues can be reversed | `PaymentStatus.Refunded` exists, and `FinancialsController.UpdateStatus` can set it. `FinancialService.UpdatePaymentStatusAsync` (line 160) changes the status and notes only. It makes no gateway refund call and writes no reversing entry |

#### Not proposed

| Feature | Reason |
|---|---|
| Recurring dues or recurring giving by saved card | `SavedPaymentMethod` exists, but a repeat charge depends on each gateway offering tokenised charges. Decide after the gateway review in `docs/PAYMENT_GATEWAY_WORKFLOW.md` |
| Business directory and member offers | Commercial scope that no current user story asks for |
| Awards and alumni spotlights | News, gallery and the contribution leaderboard already carry recognition. A separate entity adds little |
| In-app job applications | Each job has a contact email and an optional application link. A tracking flow would add much and duplicate a hiring site |
| Event sponsors | The ledger category `Sponsorship` exists as a label. A sponsor entity has no user story yet |
| Endorsements and skills | A social network feature, outside an association's needs |


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
