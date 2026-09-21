# GHCAA implementation feature catalog

**Evidence basis.** This catalog is derived from the checked-in implementation, not
from requirements or design documents. The evidence reviewed was:

- `GHCAA.Domain` models/enums and `GHCAA.Application` DTOs, interfaces, validators,
  and security attributes.
- `GHCAA.Infrastructure/Services`, persistence configurations, storage, mail/SMS,
  payment, and notification implementations.
- `GHCAA.API/Controllers`, middleware, hubs, and endpoint attributes.
- Angular route definitions, feature components, typed services, guards, and specs
  under `GHCAA.Web/src/app`.
- Flutter router, feature services, screens, models, and tests under
  `GHCAA.Mobile`.
- `GHCAA.Tests` controller, service, integration, workflow, validator, filter,
  middleware, and data tests.

An **unknown** means that the detail was not verifiable in the implementation
examined. It is intentionally not inferred from product documents.

## Cross-cutting implementation behavior

* Authentication is JWT based. Browser authentication uses `access_token`,
  `refresh_token`, and XSRF cookies; mobile uses `refresh-mobile` with tokens in the
  body. `AuthController` also exposes Google/Facebook token exchange.
* `[Authorize]`, `AdminOnly`, `SuperAdminOnly`, member claims, ownership checks,
  `RequireStepUp`, and feature guards are enforced in the API and/or clients.
* Services use EF Core through `ApplicationDbContext`; archived entities are hidden
  by the established query filters. Exact per-entity mappings are in
  `GHCAA.Infrastructure/Data`.
* File upload endpoints call `IFileValidationService` before storage. Exact allowed
  extensions and magic-byte rules are implementation details of
  `FileValidationService`; callers return a 400 problem response when validation
  fails.
* The exception middleware converts unhandled failures to the repository's problem
  response shape. Most missing resources return `404`; failed ownership checks
  return `403`; invalid identity/session state generally returns `401` or `400`.
* Angular is guarded by `authGuard`, `memberGuard`, `adminGuard`, and
  `superAdminGuard`; optional modules also use `featureGuard`. Flutter's
  `GoRouter` redirects unauthenticated users to `/login`.

## Feature catalog

### Identity, registration, and account security

**Actors.** Anonymous applicant, member, administrator, super administrator.

**API and contracts.** `AuthController` (`api/auth`) implements:
`GET providers`, `POST login`, `POST google`, `POST facebook`, `POST refresh`,
`POST refresh-mobile`, `GET me`, `POST logout`, `POST admin/step-up/request`,
`POST admin/step-up/verify`, `POST forgot-password`, and `POST reset-password`.
`RegistrationController` (same route prefix) implements `POST register`,
`GET status/{id}`, `POST verify-email`, and `POST resend-otp`.
Concrete request/response types include `LoginDto`, `SocialLoginRequest`,
`RefreshRequestDto`, `MemberRegistrationDto`, `VerifyEmailDto`, `ResendOtpDto`,
`ResetPasswordDto`, `ChangePasswordDto`, `StepUpVerifyDto`,
`MemberRegistrationResultDto`, `TokenResponseDto`, `UploadedFileDto`, and
the anonymous status/token objects returned by the controllers.

**Validation and authorization.** Auth and registration endpoints are anonymous
but rate limited. `MemberRegistrationValidator` and `VerifyEmailValidator` are
the discoverable FluentValidation validators. Registration validates photo,
certificate, and payment-proof uploads (5 MiB for photos, 10 MiB for documents).
Step-up request/verification requires `AdminOnly`; destructive/financial actions
use `RequireStepUp` where applied. Missing/invalid credentials return 401;
invalid/expired OTP returns 400; missing registration email returns 400.

**State transitions and persistence.** Registration creates a member application,
sends an OTP, then moves through email verification and administrative approval
(see the Member Administration feature). Login issues tokens; refresh rotates a
refresh token and rejects inactive/archived users; logout revokes all refresh
tokens. Forgot/reset-password and change-password update the password/security
state. `AuthService`, `TokenService`, `OtpService`, `UserService`, and
`MemberService` persist these transitions and use email/SMS integrations where
configured.

**Clients.** Angular public `login`, `register`, `reset-password`, and portal
`change-password` surfaces use `AuthService`/`RegistrationService`; auth guards
restore `/auth/me`. Flutter uses `LoginScreen`, `RegisterScreen`,
`ForgotPasswordScreen`, `AuthService`, `StorageService`, and the router redirect.

**Tests.** `AuthServiceTests`, `AuthControllerTests`, `AuthControllerMutationTests`,
`RegistrationControllerTests`, `MemberRegistrationValidatorTests`,
`VerifyEmailValidatorTests`, `OtpServiceTests`, `OtpPurposeIsolationTests`,
`TokenServiceTests`, `RequireStepUpAttributeTests`, and
`DestructiveStepUpActionsTests`.

### Member profiles, directory, and identity documents

**Actors.** Authenticated member, administrator, super administrator, anonymous
directory visitor where the endpoint permits it.

**API and contracts.** `ProfileController` (`api/profile`) implements profile
read/update, password change, ID-card/certificate retrieval (including PDF),
photo, and signature operations. `NetworkingController` (`api/networking`)
implements `search`, `directory`, `member/{id}`, `committee`, `periods`, and
`updates`. `ActivityController` (`api/activity`) exposes `me`, `admin/{memberId}`,
and `admin/global`. `AdminController` adds member list/detail/update, documents,
approval, archive/restore/reactivate, photo/signature, ID-card, certificate, and
admin password-reset routes. DTOs include `MemberProfileDto`, `UpdateProfileDto`,
`BaseMemberDto`, `MemberSummaryDto`, `AcademicRecordDto`, `ProfessionalRecordDto`,
`MemberFamilyDto`, `ECHistoryDto`, `AdminMemberUpdateDto`, approval/rejection DTOs,
and `UploadedFileDto`.

**Validation and authorization.** Profile mutations require authentication and
derive the member from claims rather than trusting a submitted member ID.
Administration uses `AdminOnly` or `SuperAdminOnly` policies. Document and image
uploads are content validated. Privacy flags are applied by service mapping; the
exact public-field matrix is implementation-defined. Unknown: no single
repository-wide profile validation class was found beyond endpoint/service checks.

**State transitions and persistence.** Member records, academic/professional
records, family links, archive flags, approval status, security stamps, activity
events, and generated ID/certificate files are persisted by
`MemberService` (including its partial classes), `NetworkingService`,
`ActivityService`, `IDCardService`, and `LocalFileStorageService`.

**Clients.** Angular portal `profile`, `digital-id`, `directory`, dashboard and
admin member approval/members/import surfaces. Flutter `ProfileScreen`,
`ProfileEditScreen`, `DigitalIDScreen`, `DirectoryScreen`, `MemberDetailsScreen`,
`MemberActivityHistoryScreen`, and corresponding networking/admin services.

**Tests.** `MemberServiceTests`, `MemberService_EC_Tests`,
`MemberService_LinkedIn_Tests`, `ProfileControllerTests`, `NetworkingControllerTests`,
`AdminControllerTests`, `MemberImportControllerTests`, `ActivityServiceTests`,
and `MemberImportServiceTests`.

### Events and registrations

**Actors.** Anonymous visitor, member/guest registrant, event administrator.

**API and contracts.** `EventsController` (`api/events`) exposes public active
events, detail, participants, JSON or multipart `register`, authenticated
`my-registrations` and invitation lookup, plus administrator CRUD, logo,
registration approval/invitation, QR check-in, task, budget, and expense routes.
DTOs visible in the controller/application include `EventDto`,
`CreateEventDto`, `UpdateEventDto`, `RegisterForEventDto`, `EventOperationsDto`,
`UploadedFileDto`, task/budget/expense DTOs, and registration result DTOs.

**Validation and authorization.** Public registration accepts a member or a
non-member guest; otherwise it returns 401. Receipts are document-validated.
Invitation lookup checks ownership and approved status for non-admins. Admin
operations use `AdminOnly`; event files are validated. Missing events or
registrations return 404.

**State and persistence.** `EventService` persists event lifecycle,
registrations, approval, invitation, check-in, operational tasks, budgets,
expenses, and uploaded logos. Registration status transitions are represented by
`EventRegistrationStatus`; the complete transition table is not centralized in a
single state-machine type (implementation gap).

**Clients/tests.** Angular `common/events` and admin event operations; Flutter
`EventsScreen`, `EventDetailsScreen`, and `events_service.dart`. Tests:
`EventServiceTests` and `EventsControllerTests`.

### News, articles, magazine, and site content

**Actors.** Anonymous reader, member author/collaborator, admin reviewer.

**API and contracts.** `NewsController` (`api/news`) supports public list/detail,
admin/pending queues, create/update/delete, member submissions, approval/rejection,
collaborators, and image/document uploads. `SiteContentController`
(`api/site-content`) supports public, admin, detail, create/update/delete.
`CampaignsController` also supplies public campaign detail/honour-roll and admin
campaign/tiers/pledges routes. DTOs include `NewsDto`, `CampaignDtos`,
`SiteContentDto`, `UploadedFileDto`, and submission/pledge/approval DTOs.

**Validation and authorization.** Public reads are anonymous; author mutations
require authentication and reviewer routes require admin policy. Uploads use file
validation. Invalid IDs return 404; unauthorized ownership/review actions return
403/401 as applicable. Exact editorial validation rules are not exposed by a
standalone validator (unknown).

**State and persistence.** `NewsService`, `SiteContentService`, and
`CampaignService` persist draft/submission/pending/approved/rejected content,
collaborators, campaign pledges, receipt confirmation, tiers, and honour-roll
data. Approval status values exist in domain models, but a complete transition
matrix is not documented in code comments (gap).

**Clients/tests.** Angular public `news`, `magazine`, `campaigns`, member
articles/submit-article, and admin news/article/site-content/campaign surfaces.
Flutter `NewsScreen`, `NewsDetailsScreen`, `ArticlesScreen`, `SubmitArticleScreen`,
`MagazineScreen`, and admin approval screens. Tests include
`NewsServiceTests`, `NewsControllerTests`, `SiteContentServiceTests`,
`CampaignServiceTests`, and matching Angular specs.

### Jobs, mentorship, and networking

**Actors.** Public/member job seeker, employer/member author, mentor/mentee,
administrator.

**API and contracts.** `JobHubController` (`api/jobs`) exposes public CRUD/detail,
deactivation, pending queue, approval, and rejection. `MentorshipController`
(`api/mentorship`) exposes request, sent/received, respond, complete, and admin
list. `NetworkingController` supplies directory, committee, periods, and updates.
DTOs include `JobDto`, mentorship request/response DTOs, and networking/member
summary DTOs.

**Validation and authorization.** Public job reads are available where annotated;
creation/update and mentorship actions require authentication, with admin
moderation protected by `AdminOnly`. Ownership checks prevent editing another
member's submission. Exact field validation is service/model based; no dedicated
job validator was found (unknown).

**State and persistence.** `JobHubService` persists job draft/approval/rejection/
deactivation. `MentorshipService` persists request pending/accepted/declined/
completed transitions; exact enum names are implementation-defined. Networking
read models are produced by `NetworkingService`.

**Clients/tests.** Angular `common/jobs`, portal jobs, admin job approval, and
mentorship/professional surfaces. Flutter `JobsScreen`, `JobDetailsScreen`,
`MentorshipHubScreen`, `ProfessionalHubScreen`. Tests:
`JobHubServiceTests`, `MentorshipServiceTests`, `NetworkingServiceTests`,
`JobHubControllerTests`, and `NetworkingControllerTests`.

### Family links

`FamilyLinkController` (`api/family-links`) implements send/respond/remove/cancel,
sent/received/my-family, member-family, linked-family, and family search routes.
The overlapping legacy route forms are concrete implementation surface, not
aliases inferred here. `FamilyLinkDto` and `MemberFamilyDto` are the discoverable
contracts. `FamilyLinkService`/`FamilyService` persist pending, accepted, removed,
and cancelled link requests; member ownership is checked before mutation.
Angular `family-link.service.ts` and Flutter `family_service.dart` back the
portal family surfaces. Tests: `FamilyLinkServiceTests` and
`FamilyLinkControllerTests`. Unknown: exact response shape of every legacy route.

### Financials, dues, ledger, payments, and gateways

**Actors.** Member, admin, super admin, payment administrator, external gateway.

**API and contracts.** `FinancialsController` (`api/financials`) exposes applicable
fees, member history, record payment with receipt, status update, receipt PDF,
dues, annual generation, fee configuration, saved payment methods, and admin
history/delete routes. `FinancialLedgerController` (`api/ledger`) exposes ledger
list/summary/create/update/delete/CSV export. `PaymentConfigController`
(`api/payment-config`) manages active/admin payment methods. `GatewaysController`
(`api/gateways`) exposes initiate and SSLCommerz/bKash/DGPay callbacks/webhook.
DTOs include `FinancialDtos`, `PaymentHistoryDto`, `CreatePaymentHistoryDto`,
membership fee/config DTOs, `LedgerSummaryDto`, `PaymentGatewayDto`, and
`SavedPaymentMethodDto`.

**Validation and authorization.** Financials require authentication by default;
fee lookup is anonymous. Member IDs are derived from claims for self-service
payments. Admin status/configuration/dues operations use admin or super-admin
policies. Receipt files are content validated; receipt download enforces
ownership for non-admins. Step-up is used for protected destructive/financial
actions where the attribute is applied. Invalid ownership returns 401/403;
unknown payment IDs return 404.

**State and persistence.** `FinancialService`, `FinancialLedgerService`,
`PaymentConfigService`, `PaymentCallbackOrchestrator`, gateway implementations,
and EF configurations persist fee applicability, dues, payment pending/approved/
rejected states, audit history, saved methods, ledger entries, gateway callbacks,
and receipts. Payment gateway credentials are configuration-dependent; no live
credential is present in the source. Exact callback idempotency behavior is
implemented in the orchestrator and is not duplicated here.

**Clients/tests.** Angular member payments/payment portal, payment method/status
components, admin fee/payment-config/ledger; Flutter financial portal/payment
web page and admin fee/ledger screens. Tests:
`FinancialServiceTests`, `FinancialLedgerServiceTests`, `FinancialAuditTrailTests`,
`PaymentConfigServiceTests`, `FinancialsControllerTests`,
`FinancialLedgerControllerTests`, `GatewaysControllerTests`,
`PaymentConfigControllerTests`, and payment Angular specs.

### Gallery and file management

`GalleryController` (`api/gallery`) implements public albums/photos, member photo
upload and albums, admin album/photo CRUD, pending queues, approve/reject,
active/featured toggles, and photo management. `SecureFilesController`
(`api/secure-files`) serves authenticated protected files by path. Contracts are
`Gallery` DTOs and `UploadedFileDto`. `GalleryService` and
`LocalFileStorageService` persist metadata and files; `FileValidationService`
checks content and size. Admin moderation and member ownership are enforced;
missing files return 404 and unauthorized paths are rejected. Angular gallery and
admin approval/gallery surfaces and Flutter `GalleryScreen` consume these routes.
Tests: `GalleryServiceTests`, `GalleryControllerTests`, `FileValidationServiceTests`,
`FileUploadRepositoryTests`, and gallery Angular specs. Unknown: complete
content-type matrix for every gallery upload caller.

### Communication, messaging, notifications, and support

`CommunicationController` (`api/admin/comm`) manages templates, logs, batch/type/
custom sends. `ContactController` accepts public contact messages; admin routes in
`AdminController` list/read/delete them. `MessagingController` (`api/messaging`)
provides recent/conversations/history/unread, read markers, and send.
`NotificationController` (`api/notifications`) registers device tokens, lists,
marks one read, and marks all read. `AssistantController` supports `POST ask`.
Services are `CommunicationService`, `ContactService`, `ChatService`,
`NotificationService`, `DeviceTokenService`, `AdminNotificationService`, and
`AssistantService`; persistence is EF-backed with real-time/push integrations
where configured. Authorization is per-controller and conversation ownership;
invalid message IDs return 404/400. Angular messages, assistant, notifications,
contact/admin communication surfaces and Flutter chat/AI/support/notifications
surfaces are implemented. Tests: communication/contact/chat/notification/device
token/assistant service specs and controller tests where present. Exact assistant
provider behavior is configuration-dependent (unknown).

### Forums and polls

`ForumController` (`api/[controller]`) exposes categories, topics, posts, create,
and delete operations. `PollController` (`api/polls`) exposes active/detail/vote;
`AdminPollController` (`api/admin/polls`) provides admin CRUD/toggle/delete.
`ForumDtos` and `PollDtos` are the contracts. `ForumService` persists topic/post
creation and deletion with authenticated ownership/moderation checks.
`PollService` persists active/closed polls and one-member vote constraints;
duplicate/invalid votes return a client error. Angular member forum/poll and admin
poll surfaces and Flutter forum/poll screens are wired. Tests:
`ForumServiceTests`, `PollServiceTests`, `PollControllerTests`, `AdminPollControllerTests`,
and Angular poll specs. Exact vote-count consistency/concurrency guarantees are
not stated outside the service implementation (gap).

### Governance, elections, roles, and configuration

`GovernanceController` exposes current/history executive committee, constitution,
and constitution voting. `AdminGovernanceController` manages periods, activation,
members, and hard-delete. `RolesController` manages users, roles, assignment,
disable/enable, and admin password reset. `OrgConfigController`, `ThemeController`,
and `SiteContentController` provide runtime organization/theme/site configuration.
Contracts include `GovernanceDto`, `OrgConfigDto`, `SiteContentDto`, lookup/theme
DTOs, and role request objects.

Governance reads include public routes; mutations and registry operations are
admin-protected, with super-admin restrictions where annotated. Services
`GovernanceService`, `RoleService`, `OrgConfigService`, and `ThemeService` persist
active/history transitions and configuration. Angular public constitution/elections,
portal governance, admin governance/roles/themes/org-config, and Flutter
governance/admin registry/permissions/theme screens are implemented. Tests:
`GovernanceServiceTests`, `RolesControllerTests`, `ThemeServiceTests`,
`OrgConfigServiceTests`, `OrgConfigControllerTests`, golden/profile config tests,
and governance controller tests. Election regulations, operational manuals,
ballot/counting certificates, and forms currently remain static build-time
documents. The controllers expose no persisted candidate, ballot, tally, result,
or election-audit workflow; the concrete persisted vote flow is constitution
amendment voting only. The product decision is to develop a persisted online
election engine as follow-on work.

### Lookups, health, imports, and operational administration

`LookupsController` manages grouped lookup reads and admin CRUD. `HealthController`
serves `healthz`; `AdminDevTrackerController` and `AdminErrorLogsController`
expose operational diagnostics. `MemberImportController` imports members and
exports them. `PendingApprovalsController` exposes admin/member approval summaries.
`AdminController` exposes stats, analytics, sync-members, contact messages, and
bulk archive. `JobHubService`, `DatabaseHealthService`, `DevTrackerService`,
`ErrorLogService`, `MemberImportService`, and related services implement
persistence/integration. These routes are mostly admin-only; health and public
lookup reads are the exceptions. Angular admin dashboard, audit, error logs,
dev tracker, import, and approval queue plus Flutter admin dashboard/audit/
gatekeeper/modules surfaces are wired. Tests include corresponding controller and
service tests, `HealthControllerTests`, migration/seed integrity tests, and
integration output-cache/SPA tests. Exact dashboard metric formulas are not
specified outside `AdminService` (unknown).

## API route inventory

The following is the concrete controller inventory extracted from endpoint
attributes; it is included to make omissions auditable.

| Controller | Route prefix | Implemented route groups |
|---|---|---|
| Auth/Registration | `api/auth` | providers, login/social login, refresh/logout, step-up, password reset, registration, status, OTP |
| Admin | `api/admin` | stats/analytics, member lifecycle/documents/cards/certificates, contact messages |
| Activity | `api/activity` | member and admin activity |
| Events | `api/events` | public events, registration, admin operations/tasks/budget/expenses |
| Financials/Ledger | `api/financials`, `api/ledger` | fees, dues, payments, receipts, saved methods, ledger/export |
| Campaigns | `api/campaigns` | public campaigns/pledges, admin campaigns/tiers/receipt confirmation |
| News/Site content | `api/news`, `api/site-content` | public content, submissions, moderation, collaborators, uploads |
| Jobs/Mentorship | `api/jobs`, `api/mentorship` | jobs/moderation, mentorship lifecycle |
| Gallery/Secure files | `api/gallery`, `api/secure-files` | albums/photos/moderation, protected files |
| Forum/Polls | `api/forum` (controller token), `api/polls`, `api/admin/polls` | topics/posts, member voting, admin poll lifecycle |
| Networking/Family | `api/networking`, `api/family-links` | directory, committee, family-link lifecycle/search |
| Messaging/Notifications | `api/messaging`, `api/notifications` | conversations, messages/read state, device tokens/notifications |
| Governance/Roles | `api/governance`, `api/admin/governance`, `api/roles` | committees, constitution voting, periods, roles/users |
| Configuration | `api/config`, `api/theme`, `api/payment-config`, `api/lookups` | organization/theme/payment/lookup configuration |
| Operations | `healthz`, `api/admin/dev-tracker`, `api/admin/error-logs`, `api/admin/members/import`, `api/pending` | health, diagnostics, import/export, approval summaries |

## Implementation gaps and unknowns

1. No single machine-readable feature registry exists; feature grouping here is
   derived from controller/service/client naming.
2. Several controllers expose anonymous objects rather than named response DTOs,
   so their exact JSON contracts need contract tests or generated schema evidence.
3. The repository has validators for member registration and email verification,
   but many domain rules are service-level and do not have discoverable standalone
   validators.
4. State transitions are distributed across enums, services, and EF updates;
   complete transition tables are not present for events, editorial content,
   jobs, payments, or mentorship.
5. Legacy/duplicate route forms remain in family links and news/forum controller
   route tokens; compatibility intent cannot be verified from code alone.
6. External provider behavior (social login, email/SMS, AI, push, and payment
   gateways) depends on runtime configuration and cannot be proven from source
   without environment values, which are intentionally not part of this catalog.
