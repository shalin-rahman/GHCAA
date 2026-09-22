# GHCAA Project Proposal and Presentation

## Official proposal form content

### Student information

**Student:** Md. Habibur Rahman  
**Roll:** 220  
**Student ID:** 2507220  
**Phone:** `[Enter current phone number]`

### Project title

**Design, Development and Evaluation of a Specification-Driven and Reusable Alumni Management Platform for a Resource-Constrained Association**

**Short name:** GHCAA — Govt. Haraganga College Alumni Association Platform

### Motivation

Alumni associations often depend on paper records, spreadsheets, informal messaging groups and manually maintained financial records. These practices make membership verification, payment tracking, communication, event coordination and governance-document management difficult.

The Govt. Haraganga College Alumni Association needs a low-cost, maintainable and privacy-aware system of record that volunteers can operate. The platform must work without enterprise-level staffing, recurring software licences or live payment-gateway credentials.

This project focuses on reliable membership records, financial transparency, privacy-aware directory access, event and communication management, governance documents, mobile-first access, single-maintainer sustainability, specification-driven development and measurable testing. It also examines how far Association rules can be represented in software without allowing software to replace the authority of the constitution or Election Commission.

This is not only a CRUD application. Its research and engineering focus is the combination of:

- constitutional rules represented as traceable and testable business rules;
- manual, evidence-based payment as a deliberate primary workflow;
- a reusable, configurable platform foundation;
- operation by one maintainer at a low annual cost;
- portability across supported database providers and container hosts;
- safe handling of legacy databases and archived records;
- quality evaluation against functional and non-functional requirements.

### Project description

GHCAA is a multi-client platform consisting of an ASP.NET Core Web API, an Angular web application and a Flutter mobile application.

The project follows specification-driven development. Requirements are derived from stakeholder needs, current manual processes, Association documents and software-engineering standards. Each important requirement is traced to its source, business rule, implementation component, automated test where applicable, and evaluation result or documented limitation.

The functional requirements cover the member lifecycle, payments, events, communication, governance and administration. The non-functional requirements cover performance, security, reliability, usability, maintainability, portability, recovery, auditability and privacy. Examples include authenticated-session invalidation after status changes, provider failure isolation, one-vote database integrity, archive-only deletion for financial and membership records, a compiler-enforced dependency rule, reproducible deployment, open data export and a routine end-to-end change achievable within one maintainer session.

### Market and research position

The project was positioned against institutional advancement platforms, CiviCRM and other open-source membership systems, general-purpose CRM products such as Salesforce Nonprofit Success Pack, Zoho CRM and HubSpot, and community products such as Discourse. Those products are stronger in some mature functions, especially campaigns, reporting, directories, events and mentoring. The identified gap is their combination of:

- constitutional governance as first-class, auditable rules;
- manual payment with proof upload and officer verification as the primary path;
- Bengali/local identity and financial-channel support;
- data sovereignty and clean exit;
- mobile fitness;
- operation by one volunteer at a low annual cost.

The contribution is the combination and the documented trade-offs, not a claim that every individual feature is novel.

#### Main functional areas

- **Membership and identity:** multi-step registration, academic and professional history, approval, membership status and number, digital ID and certificates, profiles, privacy controls, searchable directory, and secure documents.
- **Accounting and payments:** fees, dues, donations, income and expense records, configurable payment instructions, manual payment proof, administrative verification, receipts and audit history.
- **Events and participation:** event publication, registration, capacity control, waitlisting, QR attendance, event tasks, budgets, expenses and moderated photo submissions.
- **Communication and networking:** news, notices, targeted communication, real-time messaging, notifications, directory search, jobs, mentorship, contribution points and badges.
- **Governance and administration:** constitution publication and version history, Executive Committee records, amendment proposals, eligibility-controlled amendment voting, non-binding polls, voter-roll preparation, election-document support, administration, audit and organisation configuration.

The platform supports governance procedures but does not replace the Election Commission or claim to provide a legally sufficient electronic election system.

### Portability, legacy data and configuration

The persistence boundary is provider-neutral to the extent required by the specification: PostgreSQL is the deployment provider and SQLite is used for local development and tests without changing business source code. The deployment is also container-portable and is intended to run on a container host without a proprietary platform dependency.

Legacy handling is part of the design rather than an afterthought. Migration bootstrap detects databases created by the earlier schema mechanism, baselines existing objects when appropriate, applies only genuinely pending migrations, and logs failures. Membership and financial records use archival deletion and query filters so ordinary screens hide archived rows without destroying audit evidence. Full export in an open format is a portability requirement.

The configuration model has two planes:

- **Runtime configuration:** organisation name, branding, contact details, currency, feature flags, workflow settings, membership presentation, payment instructions and public content can be changed by authorised administrators without redeployment.
- **Boot profile configuration:** a profile pack supplies fresh-deployment defaults, seed data, assets, documents, email templates, lookups, membership tiers and SEO information through `ORG_PROFILE`.

The profile-pack goal is deployment of a new organisation from configuration and assets rather than a source fork. The current prototype already provides substantial runtime and profile support, while the remaining brand-literal cleanup and full zero-source-change onboarding remain explicit implementation work. This distinction prevents a planned white-label capability from being presented as fully proven.

#### Reusable platform foundation

The prototype already contains reusable capabilities:

- JWT authentication and password security;
- role-based authorisation and protected routes;
- accounting, payment and ledger services;
- audit logging;
- file and document handling;
- notifications and SignalR messaging;
- organisation configuration;
- shared validation and error handling;
- reusable Angular controls and Flutter widgets;
- database migration and deployment support;
- testing and CI/CD foundations.

Shared frontend controls include page headers, search fields, filters, data tables, pagination, loading states, empty states, confirmation dialogs and shared form patterns. The platform separates reusable technical services from organisation-specific rules and configuration, making the foundation adaptable to similar membership organisations while recognising that each organisation needs its own rules, documents, branding and tests.

#### Optional AI-agent extension

The current assistant is rule-based and connects questions to approved internal information and services. If time and evaluation evidence permit, a controlled AI-assisted agent may help users find policies and forms, understand membership and payment steps, navigate the portal and search permitted directory information.

The agent would be advisory and tool-restricted. It would not approve memberships, verify payments, modify accounting records, determine election eligibility or override privacy and authorisation rules. If safety and evaluation evidence cannot be completed, the rule-based assistant remains the delivered feature.

#### Architecture

- Angular web application for the public portal, member portal and administration console.
- ASP.NET Core Web API using Clean Architecture.
- Flutter mobile application for members.
- PostgreSQL for deployment and SQLite for local development and tests.
- SignalR for real-time communication.
- Docker and GitHub Actions for repeatable build and deployment.

### Development approach

The project uses Design Science Research with incremental software development:

1. specify the requirement and acceptance condition;
2. model the workflow, state or business rule;
3. implement it across the required layers;
4. test the implementation;
5. trace the requirement to its evidence;
6. evaluate the result and record limitations.

Specification-driven development is supported by a requirements traceability matrix, use cases, quality-attribute scenarios, domain constraints, acceptance criteria and tests. The architecture is evaluated against measurable scenarios rather than selected only for implementation convenience.

The design uses Clean Architecture, dependency inversion, dependency injection, explicit application contracts and DTOs, typed API communication, role-based access control, boundary validation, global archive/query-filter behaviour, centralised design tokens, reusable controls, configuration-driven organisation profiles and shared services instead of duplicated feature logic.

### Testing, coverage and evaluation

The project uses:

- backend unit tests for services, validators and business rules;
- backend integration tests for database and API behaviour;
- EF Core SQLite and in-memory test configurations;
- Angular unit tests using Vitest;
- Flutter widget and service tests;
- Playwright end-to-end tests;
- authentication, authorisation and privacy tests;
- API contract and compatibility checks;
- selected visual regression tests;
- static analysis, type checking and build verification.

Evidence includes backend statement and branch coverage through Coverlet, requirement coverage through the traceability matrix, critical business-rule tests, API and end-to-end results, security assessment, usability findings, performance measurements, maintainability evidence and deployment-cost assessment.

The quality scenarios include directory latency, cold mobile page load, session invalidation, authorisation isolation, dependency-failure containment, maintainer change effort, architecture-rule enforcement, recovery time, accessibility and vote replay rejection. These scenarios make the proposal about verifiable system qualities, not only stored records and screens.

Results will be reported by suite and requirement category. No unsupported combined coverage percentage will be claimed. The intended target is that all Must requirements are traced to passing tests; requirements without sufficient evidence will be reported as uncovered or limited.

### CI/CD and deployment

Git and GitHub provide incremental version control. GitHub Actions validates:

1. backend build and formatting;
2. Angular type checking and tests;
3. Flutter analysis and tests;
4. API and integration tests;
5. Playwright end-to-end tests;
6. API contract snapshots;
7. integrated API-plus-SPA packaging;
8. Docker image creation;
9. controlled pre-production deployment.

The application is packaged as a deployable .NET and Angular service. PostgreSQL is used for hosted deployment, SQLite for local development and tests, and database migrations are applied through the migration bootstrap process.

### Strategy and timeline

| Phase | Work | Schedule |
|---|---|---|
| 1 | Problem, stakeholder and governing-document analysis | Weeks 1–2 |
| 2 | Requirements, specification and traceability | Weeks 3–4 |
| 3 | Architecture, domain, database and API design | Weeks 5–6 |
| 4 | Authentication, registration, approval and profiles | Weeks 7–9 |
| 5 | Directory, privacy, digital ID and certificates | Weeks 10–11 |
| 6 | Accounting, payment proof, ledger and verification | Weeks 12–14 |
| 7 | Events, attendance, communication and messaging | Weeks 15–17 |
| 8 | Governance documents, polls and election-process support | Weeks 18–20 |
| 9 | Client integration and reusable-control refinement | Weeks 21–22 |
| 10 | Testing, coverage, security, usability and performance evaluation | Weeks 23–24 |
| 11 | Optional AI-agent feasibility work, if time permits | Week 25 |
| 12 | Deployment, report, documentation and presentation preparation | Week 26 |

### Current prototype status

The prototype already includes the ASP.NET Core backend, Angular web application, Flutter mobile application, authentication, authorisation, reusable frontend controls, accounting, payment and ledger features, membership and approval workflows, events, communication, governance, constitution features, file handling, automated tests, CI/CD configuration and deployment support.

It also contains the foundations for database-provider switching, migration bootstrap for legacy databases, archive/query-filter handling, open-format export, runtime organisation configuration and profile-based fresh deployments.

Remaining work focuses on traceability closure, measured coverage, final integration, security and usability evidence, performance evaluation, completion of the remaining white-label cleanup, documentation and presentation preparation.

### Languages and tools

- **Frontend:** Angular, TypeScript, HTML, SCSS and Angular Signals.
- **Backend:** ASP.NET Core 9 Web API, C# and Clean Architecture.
- **Mobile:** Flutter and Dart.
- **Database:** PostgreSQL for deployment; SQLite for local development and tests.
- **Real-time:** ASP.NET Core SignalR.
- **Authentication:** JWT, BCrypt and role-based authorisation.
- **Testing:** NUnit, Moq, FluentAssertions, Vitest, Playwright, Flutter Test and integration tests.
- **Coverage:** Coverlet and .NET test tooling.
- **Quality checks:** .NET formatting, TypeScript type checking, Dart analysis and CI validation.
- **Version control:** Git and GitHub.
- **CI/CD:** GitHub Actions.
- **Packaging and deployment:** Docker and hosted pre-production deployment.
- **IDE/editor:** Visual Studio Code.
- **AI tools:** List only tools actually used. If accurate: GitHub Copilot through the Copilot SDK in Visual Studio Code, Claude and ChatGPT.

### AI-use declaration

☒ I intend to use AI coding/writing assistance tools for this project.

AI tools may be used for framework and library learning, boilerplate generation, debugging, test suggestions, code review, documentation drafting, technical comparison and explanation of implementation decisions. All AI-assisted output will be reviewed, tested and modified by the student. The student remains responsible for requirements, architecture, implementation, testing, security decisions and the final report, and will be able to explain, defend and modify all AI-assisted code.

Any optional AI-agent feature will be implemented only if its privacy, authorisation, logging and evaluation requirements can be demonstrated.

### Repository and supervisor

**GitHub:** https://github.com/shalin-rahman/GHCAA  
**Supervisor:** Dr. Kazi Muheymin-Us-Sakib, Professor, Institute of Information Technology, University of Dhaka  
**Signature:** `[Supervisor signature]`  
**Date:** `[Date]`

## Presentation slides

### Slide 1 — Title

**GHCAA: A Specification-Driven and Reusable Alumni Management Platform**

- Md. Habibur Rahman
- Roll 220, Student ID 2507220
- Executive Master in Information Technology
- Supervisor: Dr. Kazi Muheymin-Us-Sakib

### Slide 2 — Problem and market gap

- Paper, spreadsheet and messaging records fragment institutional knowledge.
- Commercial systems can be strong in CRM, campaigns, events or reporting.
- They do not target the same combined constraint: constitutional governance, manual payment evidence, local identity, data sovereignty, low cost and one-maintainer operation.
- The contribution is a documented combination and evaluation, not a claim that every feature is novel.

**Goal:** Build a low-cost, privacy-aware and auditable platform for a volunteer-operated association.

### Slide 3 — Research aim and quality framework

- Design, implement and evaluate a reusable association platform.
- Functional requirements cover membership, finance, events, communication and governance.
- Non-functional requirements cover security, privacy, reliability, usability, performance, maintainability, portability, recovery and auditability.
- Evidence is separated into mechanism, executable verification and research evaluation.

**Research question:** Can a specification-driven, configurable platform meet association needs without enterprise cost or loss of governance control?

### Slide 4 — Platform scope beyond CRUD

- Membership lifecycle, identity, privacy and directory access.
- Manual payment proof, officer verification, ledger and receipts.
- Events, attendance, communication and real-time notifications.
- Constitution versions, amendment rules, polls and election-document support.
- Archive-only deletion, audit history, open-format export and failure isolation.

The platform supports governance procedures but does not replace the Election Commission or claim to provide a legally sufficient electronic election system.

### Slide 5 — Reusable and configurable foundation

- Authentication, role-based authorisation and audit.
- Accounting, ledger, payment-proof and document services.
- Shared Angular controls and Flutter widgets.
- Validation, error handling, notifications and SignalR.
- Runtime organisation settings without redeployment.
- Profile packs for assets, seed data, documents, tiers and public content.

```text
Reusable services + supported configuration + organisation rules
                         = adaptable platform
```

### Slide 6 — Architecture, portability and onboarding

- Clean Architecture modular monolith: Domain → Application → Infrastructure/API.
- Typed contracts, dependency inversion and framework-enforced boundaries.
- PostgreSQL deployment with SQLite local/test provider-neutrality.
- Container-portable deployment and GitHub Actions CI/CD.
- Deployment-per-institution with a separate database for stronger isolation.
- Legacy-schema migration baselining, archival preservation and open export.
- No source changes is the target for supported profile/configuration changes; universal zero-code onboarding is not claimed as complete.

### Slide 7 — Specification-driven development

```text
Stakeholder / constitution
          ↓
Formal requirement
          ↓
Business rule and model
          ↓
Implementation
          ↓
Automated test
          ↓
Traceability and evaluation
```

Requirements are not accepted only because a screen exists. Governance and accounting rules must have implementation and test evidence. Quality-attribute scenarios also measure maintainer effort, failure containment, session invalidation, vote replay rejection and deployment reproducibility.

### Slide 8 — Testing, coverage and CI/CD

- Backend unit and integration tests.
- Database and API tests.
- Angular Vitest tests.
- Flutter widget and service tests.
- Playwright end-to-end tests.
- Security, privacy and contract tests.
- Coverlet statement and branch coverage.
- Requirement coverage through the traceability matrix.
- Static analysis, type checks and CI gates.
- Performance, security, privacy, accessibility and usability evaluation.

```text
Build → Analyse → Test → Contract check → Package → Deploy
```

### Slide 9 — Evidence, trade-offs and boundaries

**Payment:** instructions → member payment → proof upload → officer verification → ledger and receipt.

**Governance:** constitution versioning, amendment voting, polls, voter-roll preparation and election-document support.

Evidence is bounded: sequential vote replay rejection is not concurrent high-contention proof; a bounded diagnostic log is not proof of operational usefulness; profile packs are not unlimited multi-tenancy.

### Slide 10 — Optional AI agent, timeline and outcome

If time permits, a controlled AI-assisted agent may help users find policies and forms, understand procedures, navigate the portal and search permitted directory information.

It will not approve members, verify payments, modify accounting records, determine election eligibility or override privacy and authorisation.

The remaining delivery completes traceability, measured coverage, integration, evaluation, profile onboarding and deployment evidence.

**Expected outcome:** A working, tested and evaluated platform combining reusable technical services with organisation-specific membership and governance rules.

### Six-minute timing

| Time | Slides | Topic |
|---|---:|---|
| 0:00–0:35 | 1 | Title |
| 0:35–1:10 | 2 | Problem and market gap |
| 1:10–1:45 | 3 | Aim and quality framework |
| 1:45–2:25 | 4 | Platform scope |
| 2:25–3:05 | 5 | Reusable foundation |
| 3:05–3:45 | 6 | Portability and onboarding |
| 3:45–4:30 | 7 | Specification-driven development |
| 4:30–5:15 | 8 | Testing, coverage and CI/CD |
| 5:15–5:45 | 9 | Evidence and boundaries |
| 5:45–6:00 | 10 | Timeline, AI option and conclusion |

### Closing statement

GHCAA is a specification-driven, tested and reusable platform foundation for membership organisations. It combines common services such as authentication, authorisation, accounting, audit, notifications and reusable client controls with organisation-specific rules for membership, governance and public content.
