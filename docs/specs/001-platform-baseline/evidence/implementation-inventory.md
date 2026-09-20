# Implementation Inventory and Evidence

**Reviewed**: 2026-09-20

This inventory records the implementation evidence used to write
[spec.md](../spec.md). The codebase, generated contract surfaces, and
executable tests are the only normative inputs.

## Evidence sources

The baseline was checked against these codebase surfaces:

- `GHCAA.Domain`: models, enums, constants, and relationships.
- `GHCAA.Application`: DTOs, interfaces, validators, mapping, and security
  contracts.
- `GHCAA.Infrastructure`: EF configurations, services, persistence, storage,
  providers, integrations, and migrations.
- `GHCAA.API`: controllers, middleware, policies, hubs, health, and OpenAPI
  generation.
- `GHCAA.Web`: routes, standalone components, models, typed services, guards,
  interceptors, and unit specs.
- `GHCAA.Mobile`: Dio client, feature services, Riverpod providers, routes,
  screens, shared widgets, and tests.
- `GHCAA.Tests`, Angular specs, and Flutter tests: executable verification
  surfaces.
- Project files, generated client contract constants, and build/test
  configuration required to execute the implementation.

## Product and architecture coverage

| Area | Requirement behavior | Backend implementation evidence | Client implementation evidence | Verification evidence |
|---|---|---|---|---|
| Registration | Wizard, save/continue behavior, required academic record, duplicate prevention, OTP, attachments, Applied state | `RegistrationController`, `AuthController`, `MemberRegistrationValidator`, `MemberService`, `OtpService`, upload/file services | Web register wizard and registration service; Mobile register screen, wizard provider, auth service | Registration controller, auth mutation, validator, Web registration, and Mobile registration tests |
| Authentication | Password, refresh, logout, reset, Google/Facebook, role claims, status gates, security stamp | `AuthController`, `AuthService`, `TokenService`, `SecurityStampMiddleware`, auth policies | Web auth service, guards, interceptors; Mobile auth service, router, gatekeeper | Auth controller/mutation tests, Web auth specs, Mobile auth tests |
| Admin step-up | Purpose-scoped OTP and 30-minute action grace period for sensitive actions | Step-up actions in `AuthController`, OTP service, middleware/policies | Web step-up service/interceptor; Mobile admin gatekeeper flows | Step-up controller and interceptor tests |
| Member profile | Personal, academic, professional, privacy, notification settings, completion gate | `ProfileController`, member services, profile DTOs, privacy mapping | Web profile service/components; Mobile profile/edit screens and providers | Profile controller/service tests and client specs |
| Identity documents | Active-member-only ID and certificate generation, QR data, protected files | profile and secure-file controllers, document generation, file storage | Web and Mobile profile/document download controls | Profile, secure-file, and visual/client tests |
| Events | Public catalog, lifecycle dates, capacity, guest policy, payment, waitlist, admin tasks/budget/expenses, QR attendance | `EventsController`, `EventService`, event models and financial integrations | Web events service/admin events; Mobile events and event details screens | Events controller/service, Web event, and Mobile event tests |
| Payments | Admin-configurable manual channels, proof upload, approval, fees, dues, receipt, optional inactive gateways | financial, ledger, payment-config, gateway controllers and services; payment strategies and callback orchestrator | Web financial, ledger, payment-config, gateway services; Mobile financial and gateway services | Financial, ledger, payment-config, gateway, and guest-payment tests |
| Networking | Search, filters, privacy projections, offset compatibility, cursor pagination | `NetworkingController`, `NetworkingService`, member search DTOs and paged result | Web networking service/directory; Mobile networking service/directory and professional hub | Networking controller/service tests and Web/Mobile directory tests |
| Career and family | Jobs, mentorship, family links, moderation, accept/reject/cancel flows | Job, mentorship, and family controllers/services/models | Web job, mentorship, family services/components; Mobile feature services/screens | Job, family, networking, and related client tests |
| Messaging | Private history, recent/unread/read state, send, SignalR delivery | messaging/chat controllers, notification services, SignalR hubs | Web chat service and messages; Mobile chat and notification services/screens | Controller, Web chat, Mobile chat, and notification tests |
| Assistant and support | Rule-based internal lookup, policy answers, support fallback, contact submission | assistant and contact controllers/services | Web assistant/contact services and components; Mobile assistant/support services/screens | Assistant, contact, and client service tests |
| Governance | EC periods, role eligibility/exclusivity, historical records, constitution vote and history | governance controllers/services, EC and constitution models, seeder | Web governance/constitution services and pages; Mobile governance API and screen | Governance/controller/integration tests and client tests |
| Election documents | Static source documents, 18 printable A4 forms, organisation-configured letterhead | build-time source and publishing tools, no application persistence | Web static asset sync and markdown/form renderer; Mobile governance access where applicable | Web rendering/visual checks and document build checks |
| News and notices | Shared entity, post type, admin-only notices, member news approval, PDF notice documents | `NewsController`, `NewsService`, upload validation/storage, `PostType` | Web news/admin news/site content services; Mobile news/article screens | News/controller/service, Web news, and Mobile article tests |
| CMS and contact | Sanitized content blocks, fallback content, org-config contact, map allow-list | site-content and org-config controllers/services, sanitizer, profile provider | Web public/admin content and contact; Mobile org config and about/contact surfaces | Site-content, org-config, contact, and client tests |
| Gallery and albums | Admin gallery, member albums, pending approval, photos, feature/active flags | gallery controller/service, gallery models, storage and notifications | Web gallery/admin gallery; Mobile gallery screen and service | Gallery controller/service, Web gallery, Mobile gallery tests |
| Themes and polls | Admin special-day themes, active theme, member polls, expiry and voting | theme and poll controllers/services/models | Web theme/poll services and screens; Mobile dynamic theme and poll surfaces | Theme/poll controller, service, Web, and Mobile tests |
| Admin operations | Analytics, approval queues, import/export, roles, communication, audit, error logs, sync | admin, roles, communication, import, audit/error controllers/services | Web admin routes/services; Mobile admin screens and services | Admin, roles, communication, import, audit, and integration tests |
| White-label configuration | Profile packs, build-time branding, seed fallback, org config precedence | institution profile provider, org config service, seeders, migration bootstrapper | Web build scripts and generated fallbacks; Mobile profile tool and config | Org-config, seed integrity, build, and deployment tests |

## Backend boundary inventory

The implementation follows the dependency direction:

```text
GHCAA.Domain
  -> GHCAA.Application
  -> GHCAA.Infrastructure
  -> GHCAA.API
```

- **Domain** contains the framework-independent models, enums, constants, and
  relationships. Important aggregates include `Member`, `User`, academic and
  professional records, events, payments, financial records, governance,
  content, messaging, notifications, jobs, mentorship, family links, polls,
  and gallery records.
- **Application** contains DTOs, service interfaces, validators, mapping
  contracts, and security primitives. It is the shared business contract used
  by controllers and services.
- **Infrastructure** contains EF Core configurations, query filters,
  migrations, seed/profile providers, member and auth services, financial and
  payment services, file storage, email/OTP, notifications, SignalR support,
  document generation, and external-provider abstractions.
- **API** contains thin controllers, middleware, rate limiting, authorization
  policies, health, SignalR hubs, exception and correlation handling, audit
  logging, and security-stamp validation.

## Client boundary inventory

### Angular Web

- Public layout: landing, login, registration, reset password, about, contact,
  events, gallery, news, constitution, elections, directory, and public
  configuration-driven content.
- Member layout: dashboard, profile, payments, events, directory, jobs,
  mentorship, family, messages, notifications, assistant, forum, gallery,
  news, activity, governance, and support.
- Admin layout: dashboard, member approvals, member records, events, finance,
  ledger, fees, payment channels, governance, communication, gallery, news,
  jobs, polls, themes, content, contact messages, roles, audit, and org
  configuration.
- Core contract surfaces: `API_ENDPOINTS`, business models, typed services,
  auth and step-up interceptors, guards, pagination, search/filter controls,
  shared loading/error/empty controls, and central theme tokens.

### Flutter Mobile

- Auth screens cover login, registration, and password recovery.
- Member screens cover dashboard, profile, digital ID, directory, events,
  finance, jobs, mentorship, family, chats, notifications, governance, news,
  articles, gallery, forums, assistant, support, activity, committee, and
  magazine content.
- Admin screens cover dashboard, approvals, article and job moderation, fees,
  ledger, gallery, governance registry, permissions, audit, contact messages,
  and themes.
- Core contract surfaces: Dio API client, retry interceptor, connectivity
  service and offline banner, Riverpod providers, `go_router`, SignalR
  services, AppConfig, shared async/empty/loading widgets, and shared theme.

## Data, security, and operational evidence

- EF global query filters preserve `IsArchived` records for audit while hiding
  them from normal reads.
- Database and validator rules enforce NID, email, mobile, membership,
  one-vote, one-registration, and other domain uniqueness constraints.
- JWT, BCrypt, role policies, security stamps, step-up OTP, correlation IDs,
  exception handling, audit logging, and tiered rate limits protect the API.
- File validation and storage distinguish compressed images from evidentiary
  proofs, signatures, certificates, and documents.
- PostgreSQL is the production provider and SQLite is used for local/test
  scenarios. Migration bootstrap and seed/profile loading are separate paths.
- CI checks cover backend tests and formatting, Angular type-check/unit tests,
  Flutter analysis/tests, Playwright, API snapshot comparison, and integrated
  packaging.
- `TODO_IMPLEMENTED_MISSING.md` and the completed TODO work packages record
  release fixes such as session restoration, static-file fallback, guest
  payment tests, organization configuration tests, and deployment packaging.

## Known scope boundaries

- The requirements describe behavior evidenced by the implementation.
  Completion status is determined by code paths and executable tests.
- White-label deployment remains in progress because committed migration data
  still prevents safe second-institution deployment.
- Gateway callback abstractions exist, but manual payment is the required
  operating path without live credentials.
- Election forms are static build-time documents with dynamic organization
  letterhead; they are not persisted application records.
