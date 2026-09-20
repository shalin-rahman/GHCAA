# API Cross-Layer Trace

The route surface is derived from the API controllers and the generated client
contract surfaces. This document groups the implemented route surface by
business domain and names the implementation surfaces that must stay aligned.
It is the implementation trace for the requirements in `spec.md`.

## Contract flow

```text
Domain models and enums
  -> Application DTOs, interfaces, validators
  -> Infrastructure services and EF persistence
  -> API controllers, middleware, SignalR hubs
  -> Angular constants, models, typed services
  -> Flutter Dio client and feature services
  -> Backend, Web, Mobile, and contract tests
```

## Domain trace

| API domain | .NET surface | Angular surface | Flutter surface | Test evidence |
|---|---|---|---|---|
| Auth and registration | `AuthController`, `RegistrationController`, application validators | `auth.service.ts`, `registration.service.ts`, `app.constants.ts` | `auth_service.dart`, registration providers and screens | `AuthControllerTests`, `AuthControllerMutationTests`, `RegistrationControllerTests`, validator tests |
| Admin and approvals | `AdminController`, `PendingApprovalsController`, `RolesController` | `admin.service.ts`, admin feature services | `admin_service.dart`, admin screens | `AdminControllerTests`, `PendingApprovalsControllerTests`, `RolesControllerTests` |
| Profile and secure files | `ProfileController`, `SecureFilesController` | `profile.service.ts` and file helpers | `file_service.dart` and profile features | `ProfileControllerTests`, `SecureFilesControllerTests` |
| Events and attendance | `EventsController` | `events.service.ts`, event components | `events_service.dart`, event screens | `EventsControllerTests` |
| Financials and ledger | `FinancialsController`, `FinancialLedgerController`, `PaymentConfigController`, `GatewaysController` | `financial.service.ts`, `ledger.service.ts`, `payment-config.service.ts`, `gateways.service.ts` | `financial_service.dart`, `gateway_service.dart` | `FinancialsControllerTests`, `FinancialLedgerControllerTests`, `PaymentConfigControllerTests`, `GatewaysControllerTests` |
| Networking and family | `NetworkingController`, `FamilyLinkController`, `MentorshipController`, `JobHubController` | `networking.service.ts`, `family-link.service.ts`, `mentorship.service.ts`, `job.service.ts` | `networking_service.dart`, `family_service.dart`, `mentorship_service.dart`, `job_service.dart` | `NetworkingControllerTests`, `FamilyLinkControllerTests`, related service tests |
| Messaging and notifications | `MessagingController`, `NotificationController` | `chat.service.ts`, alert and notification services | `chat_service.dart`, notification services, SignalR hub client | controller and client service tests where present |
| Governance and public content | `GovernanceController`, `AdminGovernanceController`, `SiteContentController`, `NewsController`, `ThemeController` | `constitution.service.ts`, `site-content.service.ts`, `news.service.ts`, `theme.service.ts` | `governance_api.dart`, content and theme services | governance, news, site-content, and theme controller tests where present |
| Gallery, polls, assistant, contact | `GalleryController`, `PollController`, `AssistantController`, `ContactController` | corresponding typed services and feature components | corresponding feature services | `GalleryControllerTests`, `PollControllerTests`, `ContactControllerTests` and assistant service tests |
| Configuration and health | `OrgConfigController`, `LookupsController`, `HealthController` | `org-config.service.ts`, `lookup.service.ts`, `health.service.ts` | `org_config_service.dart`, lookup services | integration and controller tests for configuration, lookups, and health |

## Requirement-to-implementation coverage

| Story | Implemented behavior | API and backend anchors | Web and Mobile anchors | Verification anchors |
|---|---|---|---|---|
| US1 Membership application | Registration and verification behavior implemented in the codebase | `AuthController`, `RegistrationController`, `MemberRegistrationValidator`, `MemberService`, `/api/auth/register`, `/api/auth/verify-email`, `/api/auth/status/{id}` | `auth.service.ts`, `registration.service.ts`, registration wizard; `auth_service.dart`, registration wizard provider and screens | registration controller tests, auth mutation tests, validator tests, Web registration specs, Mobile registration visual and widget tests |
| US2 Authentication and lifecycle | Login, session, approval, rejection, restoration, and role behavior implemented in the codebase | `AuthService`, `AuthController`, `AdminController`, `PendingApprovalsController`, `/api/auth/login`, `/api/auth/refresh`, `/api/admin/members/{id}/approve`, `/api/admin/members/{id}/reject`, `/api/admin/members/{id}/restore` | `auth.service.ts`, auth guards, admin services; `auth_service.dart`, gatekeeper and admin screens | auth controller tests, admin controller tests, pending approval tests |
| US3 Profile and documents | Profile, privacy, secure-file, ID-card, and certificate behavior implemented in the codebase | `ProfileController`, `SecureFilesController`, profile DTOs, `/api/profile`, `/api/profile/photo`, `/api/profile/id-card`, `/api/profile/certificate` | `profile.service.ts`, profile and member components; `file_service.dart`, profile screens | profile and secure-file controller tests, profile service specs |
| US4 Events and attendance | Event lifecycle, registration, capacity, payment, waitlist, and attendance behavior | `EventsController`, event services, `/api/events`, `/api/events/register`, `/api/events/admin/checkin/qr`, `/api/events/admin/registrations` | `events.service.ts`, event components; `events_service.dart`, event screens | `EventsControllerTests`, Web events specs, Mobile event tests |
| US5 Finance and payments | Fee, manual payment, proof, approval, receipt, ledger, and gateway behavior | `FinancialsController`, `FinancialLedgerController`, `PaymentConfigController`, `GatewaysController`, `/api/financials/*`, `/api/ledger/*`, `/api/payment-config/*`, `/api/gateways/*` | `financial.service.ts`, `ledger.service.ts`, `payment-config.service.ts`, `gateways.service.ts`; `financial_service.dart`, `gateway_service.dart` | financial, ledger, payment-config, and gateway controller tests; Web financial specs and Mobile financial tests |
| US6 Directory and opportunities | Directory, privacy, jobs, mentorship, and family-link behavior | `NetworkingController`, `JobHubController`, `MentorshipController`, `FamilyLinkController`, `/api/networking/*`, `/api/jobs/*`, `/api/mentorship/*`, `/api/family-links/*` | `networking.service.ts`, job, mentorship, and family services; `networking_service.dart`, `job_service.dart`, `mentorship_service.dart`, `family_service.dart` | networking and family controller tests, Web networking specs, Mobile directory tests |
| US7 Messaging and notifications | Private messaging, notification, read-state, and real-time delivery behavior | `MessagingController`, `NotificationController`, SignalR hubs, `/api/messaging/*`, `/api/chat/*`, `/api/notifications/*` | `chat.service.ts`, notification services; `chat_service.dart`, notification and hub services | messaging/controller tests where present, Web chat specs, Mobile notification and chat tests |
| US8 Governance and public content | Governance, constitution, election, news, content, gallery, theme, poll, contact, and organization configuration behavior | governance, constitution, elections, news, site-content, gallery, theme, poll, contact, and org-config controllers | `constitution.service.ts`, `news.service.ts`, `site-content.service.ts`, `theme.service.ts`, admin content services; `governance_api.dart`, content and theme services | governance, news, site-content, gallery, poll, theme, contact, and org-config tests |
| US9 Assistant and support | Rule-based assistant and support contact behavior | `AssistantController`, `ContactController`, `/api/assistant/ask`, `/api/contact` | `assistant.service.ts`, `contact.service.ts`; `assistant_service.dart`, support service | assistant and contact service/controller tests |
| US10 Secure operation and resilience | Middleware, authorization, archive, storage, configuration, health, retry, and client failure behavior | middleware, rate limiting, authorization policies, archive filters, file storage, configuration providers, `/healthz` | HTTP/error interceptors, central Web controls and tokens; Dio retry/interceptors, AppConfig, shared Mobile widgets | backend security/controller tests, Web unit suite, Mobile analysis/tests, generated contract checks, CI workflow |

## Implementation detail reviewed

- **Domain**: member lifecycle, archive state, membership and payment enums,
  event state, governance records, content types, privacy flags, and
  contribution points.
- **Application**: DTOs and interfaces for members, academic and professional
  records, events, payments, ledger, networking, governance, content, and
  configuration; validators for registration and shared inputs.
- **Infrastructure**: EF Core configurations and query filters, service
  implementations, seed/profile providers, file storage, email and OTP
  services, document generation, payment strategies, and SignalR support.
- **API**: thin controllers, authorization policies, middleware, hubs, upload
  limits, rate limits, and the generated client contract surface.
- **Web**: `API_ENDPOINTS`, business models, typed services, standalone route
  components, guards, shared loading/error controls, central theme tokens,
  and service/component specs.
- **Mobile**: Dio API client, Riverpod providers, feature services,
  `go_router` screens, SignalR services, retry and connectivity behavior,
  shared theme/widgets, and expanded Flutter tests.

## Recorded contract change

### 2026-09-17 - Networking cursor pagination

- **Backend**: `MemberSearchFilterDto` accepts optional `cursor`; `PagedResult<T>`
  can return `NextCursor`; the server uses keyset ordering by `(FullName, Id)`
  when a cursor is supplied.
- **Web**: Existing callers retain `page` and `pageSize` and therefore use the
  legacy behavior. No Web route change was required.
- **Mobile**: `networking_service.dart` and `directory_screen.dart` send and
  consume the cursor for directory pagination. `professional_hub_screen.dart`
  remains compatible because it omits the cursor.
- **Tests**: The backend networking controller and service test surfaces must
  cover both cursor and legacy paths; the mobile directory tests must cover
  `nextCursor` progression. The API snapshot must remain additive.

## Contract rules

1. Add fields and routes without removing or renaming existing contract
   elements, preserving compatibility for existing callers.
2. Update Application DTOs and interfaces before controller mapping.
3. Keep controllers thin and preserve existing middleware, authorization,
   rate-limit, and error behavior.
4. Update Angular constants and typed models plus Flutter service models for
   every shared field or route.
5. Add or extend the smallest existing backend, Web, Mobile, and contract tests;
   do not duplicate an existing setup.
6. Verify the resulting route and field behavior in both clients and the
   relevant backend and client tests.
