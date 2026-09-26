# Feature Specification: Pluggable Feature Modules

**Feature Branch**: `021-pluggable-feature-modules`

**Created**: 26-09-2026

**Status**: Deferred (future work, not scheduled)

**Input**: User request, 26-09-2026: "i wanted features modules to be as reusable package/modules
with relevant UI, API, and App, something like those can be used individually in any other
apps/services, those are supposed to as plug and play used ... these may not be developed for now
but may be later these will be needed". Tracked as Work Package 91 in `docs/TODO.md`.

## Purpose and scope

Today each feature (events, campaigns, polls, gallery and so on) lives inside the one GHCAA
solution. Its entity sits in `GHCAA.Domain`, its service in `GHCAA.Infrastructure`, its controller
in `GHCAA.API`, its page in `GHCAA.Web/src` and its screen in `GHCAA.Mobile/lib`. None of it can be
lifted into another app without taking the rest of GHCAA along.

This spec records what it would take to ship a feature as a module that carries its own API,
Angular UI and Flutter screens, and that another app could install and use on its own. It is a
record of intent. Nothing here is built, and nothing here should be built until a real second
consumer exists.

This does not reverse the architecture decision. The book's §6.2, where the modular monolith was
chosen over microservices, still holds: a module in this spec is a package the monolith loads, not a
separate service. The book's §6.11.12, where speculative abstraction was traded away, is the reason
this spec is deferred rather than started.

Out of scope: membership, registration, login and roles. Every module depends on them, so they stay
in the host app.

## Why this is recorded now

Specs 012 to 020 already found the pattern this would build on. Their "Enhancements: modularisation
and reusability" sections list 55 ENH items, and spec 001's roll-up (the table under "Enhancement
roll-up across domains") names one theme directly: listing, request and admin approval are built
again for each entity (014 ENH-002, 015 ENH-003 and ENH-004, 016 ENH-003, 017 ENH-002). The roll-up
says to build a shared shape when the next module needs it. This spec is where that later step is
written down.

## User Scenarios & Testing

### User Story 1 - A developer adds one module to another app (Priority: P1)

A developer building a different app (a school portal, a club site) installs the events module.
They wire it to their own login, database and theme, and get event listing, RSVP and the admin
screen without copying GHCAA code.

**Why this priority**: this is the whole point of the request. If this does not work, nothing else
in the spec matters.

**Independent Test**: create an empty ASP.NET Core host, Angular app and Flutter app. Install only
the pilot module. Implement the host contracts with stubs. The module's endpoints answer, its page
renders and its screen opens.

**Acceptance Scenarios**:

1. **Given** a host app with no GHCAA code, **When** the developer installs the module and
   registers the host contracts, **Then** the module's API, web page and mobile screen work.
2. **Given** the host has not registered a required contract, **When** the app starts, **Then**
   startup fails with a message naming the missing contract.

### User Story 2 - GHCAA runs on its own modules (Priority: P2)

GHCAA itself loads the pilot module the same way an outside app would, so the module is proven
against a real host and the GHCAA copy of that feature is removed.

**Why this priority**: two copies of one feature would drift. GHCAA has to consume the package, not
keep its own version alongside it.

**Independent Test**: the existing tests for the pilot feature pass with the in-repo feature code
deleted and the module installed.

**Acceptance Scenarios**:

1. **Given** the pilot module is extracted, **When** GHCAA builds, **Then** no class for that
   feature remains outside the module.
2. **Given** existing data for that feature, **When** the module's migrations run, **Then** no rows
   are lost and no table is recreated.

### User Story 3 - An institution turns a module off (Priority: P3)

A SuperAdmin disables a module in organisation configuration. Its routes, menu entries and mobile
tabs disappear. Its data stays.

**Why this priority**: useful for white-label deployments (Work Package 62), but a module that
cannot be installed separately gains little from being switchable.

**Independent Test**: flip the module's flag off in OrgConfig. Its API returns 404, its web menu
item is gone and its mobile tab is hidden.

**Acceptance Scenarios**:

1. **Given** a module is turned off, **When** a member calls its API, **Then** the call returns 404,
   not 500.
2. **Given** a module is turned back on, **When** the page reloads, **Then** earlier data is still
   there.

### Edge Cases

- A module needs payments but the host has no payment gateway. The module must work in a
  free-only mode or refuse to start with a clear message. `[NEEDS CLARIFICATION: which of the two]`
- Two modules define a table with the same name.
- The host's theme has no token the module's UI expects.
- Module version 2 changes a DTO while a mobile app on version 1 is still in the field.
- A host uses a different database provider. GHCAA already runs on both SQLite and PostgreSQL, so
  module migrations have to stay portable.

## Requirements

### Module shape

A module is three packages released together under one version number:

| Layer | Package | Holds |
|---|---|---|
| API | .NET class library | entities, EF configuration, migrations, DTOs, service, controller, a `services.AddXxxModule()` extension |
| Web | Angular library | standalone components, routes, the API service, a `provideXxxModule()` function |
| Mobile | Flutter package | screens, the API client, route definitions |

A module never references another module. Anything two modules share goes into the host contracts
below.

### Host contracts

A module asks the host for these and nothing else. Most already exist as interfaces in
`GHCAA.Application/Interfaces`; they would need to move to a small shared contracts package so a
module can depend on them without depending on GHCAA.

| Contract | What exists today |
|---|---|
| Current user and roles | `GHCAA.API/Extensions/CurrentUserExtensions.cs`, extension methods on the controller, not an interface. A new interface is needed |
| Organisation configuration | `IOrgConfigService`, `IInstitutionProfileProvider` |
| Notifications | `INotificationService`, `IAdminNotificationService` |
| Payments | `IPaymentGatewayService`, `IPaymentConfigService`, `IPaymentCallbackOrchestrator` |
| File storage | `IFileStorageService`, `IFileValidationService` |
| Theme | the CSS token set in the Angular app and the Flutter theme; no contract exists |

### Candidate modules

In the order they could be tried. Each one names the domain spec that already documents it.

| Candidate | Domain spec | Host contracts used |
|---|---|---|
| Polls | 018 | current user, config |
| Gallery | 014 | current user, storage, notifications |
| Events with RSVP | 014 | current user, storage, notifications, payments |
| Mentorship | 016 | current user, notifications |
| Campaigns and scholarships | 015 | current user, payments, notifications |
| Elections | 018 | current user, config, notifications |
| Meetings (84.19, not built yet) | none yet | current user, notifications |

Polls is the likely pilot because it needs the fewest host contracts. See "Decisions taken" below
for how the first consumer affects that choice.

### Functional Requirements

- **FR-001**: A module MUST build and run in a host app that contains no GHCAA code.
- **FR-002**: A module MUST reach the host only through the host contracts.
- **FR-003**: A module MUST fail at startup, naming the contract, when a required host contract is
  missing.
- **FR-004**: A module MUST ship its own migrations, and they MUST run on SQLite and PostgreSQL.
- **FR-005**: Extracting a feature into a module MUST NOT drop or recreate any existing table or row.
- **FR-006**: A module's web UI MUST read colours and spacing from the host's theme tokens, never
  from literals.
- **FR-007**: A module's API, web and mobile packages MUST share one version number, and a breaking
  DTO change MUST raise the major version.
- **FR-008**: GHCAA MUST consume an extracted module as a package and MUST NOT keep a second copy of
  that feature.
- **FR-009**: A SuperAdmin SHOULD be able to turn a module on or off through OrgConfig, reusing the
  existing Features flags.
- **FR-010**: A module MUST keep the tests that covered the feature before extraction, so coverage
  does not drop.

### Key Entities

- **Module**: a named feature with its three packages and a version.
- **Host contract**: an interface the host implements and every module may call.
- **Module flag**: an OrgConfig Features entry that turns a module on or off.

### Decisions taken

Answered by the user on 26-09-2026.

| Decision | Answer |
|---|---|
| How other apps get the packages | In-repo project references. No NuGet, npm or pub feed, so no new infrastructure |
| Repo layout | A `modules/` folder in this repo, one subfolder per module holding its API, Angular and Flutter parts |
| Licence | Private. Only apps built by the owner or their organisation may use the modules |
| First consumer | A school or club portal, not another alumni app |

A school or club portal points the pilot at modules that make sense outside alumni work: polls,
events with RSVP, or gallery. Polls stays the likely pilot because it needs the fewest host
contracts. `[NEEDS CLARIFICATION: once the portal project exists, confirm which of the three it needs
first]` The spec stays deferred until that portal project actually starts.

## Success Criteria

### Measurable Outcomes

- **SC-001**: The pilot module runs in an empty host app built from the ASP.NET Core, Angular and
  Flutter templates, with only stub host contracts.
- **SC-002**: After extraction, GHCAA's test count for the pilot feature is equal to or higher than
  before.
- **SC-003**: After extraction, a search of the GHCAA projects finds no class belonging to the pilot
  feature outside its module.
- **SC-004**: Adding the pilot module to a new host takes one registration call per layer.

## Assumptions

- GHCAA stays a modular monolith. Modules are loaded in-process, not deployed as services.
- Work Package 62 (white-label) lands first, because its OrgConfig and profile-provider seams are the
  same seams a module needs.
- The shared approval-queue shape named in spec 001's roll-up is built once, inside the first module
  that needs it, not as a separate framework ahead of time.
- No new NuGet, npm or pub dependency is added without the user's approval.
