# Project Documentation Book — Standard Contents (Master's Dissertation Standard)

**Deliverable:** a bound dissertation / project book for the GHCAA Alumni Association Platform,
written to the standard expected of a Master's programme at a research university.

This file is the **contents specification** — every chapter heading, every sub-heading, the
complete diagram inventory, and the standards and sources each chapter must be written against.
It is not the book itself.

---

## 0. What separates this from an undergraduate report

Read this section before writing a single page. Four properties distinguish a Master's
dissertation from a competent project report, and every chapter below exists to serve one of them.

1. **It answers research questions, not a requirements list.** The system is the *instrument*;
   the contribution is the *knowledge produced by building and evaluating it*.
2. **It is positioned against literature.** Claims are cited. Chapter 2 is a protocol-driven
   review, not a product comparison.
3. **It declares a research method and follows it.** This work is **Design Science Research**
   (Hevner et al., 2004; Peffers et al., 2007) — the construction and evaluation of an artefact
   to solve a class of real-world problem. Say so, and follow the cycle.
4. **It evaluates with defined metrics and admits threats to validity.** A screenshot proves
   existence; it does not prove merit. Every claim in Chapter 12 must trace to a measurement
   defined in Chapter 4 and be qualified in §12.7.

**Conventions**

- Front matter numbered in lower-case Roman; body restarts at Arabic 1.
- Figures `Figure <chapter>.<n>`, tables `Table <chapter>.<n>`, listings `Listing <chapter>.<n>`;
  all three carry their own list in the front matter.
- Every diagram caption states **what it shows and where the source of truth lives**
  (e.g. "Figure 6.3 — Entity–Relationship model of the 40 mapped entities; derived from
  `GHCAA.Infrastructure/Data/ApplicationDbContext.cs`").
- **Every non-obvious claim is cited.** IEEE numeric style throughout; no un-cited assertion about
  what "most systems" or "industry practice" do.
- Code in the body is illustrative extract only; full listings go to Appendix D.
- UML 2.5 notation, used correctly and consistently. A malformed diagram costs more marks than a
  missing one.

---

## Front Matter

- i. Title Page
- ii. Declaration of Originality / Authorship
- iii. Certificate of Approval (Supervisor / Examiner)
- iv. Acknowledgements
- v. **Abstract** (250–350 words: context → problem → method → artefact → evaluation → finding)
- vi. Keywords
- vii. Table of Contents
- viii. List of Figures
- ix. List of Tables
- x. List of Listings
- xi. List of Abbreviations and Acronyms
- xii. Glossary of Domain Terms
- xiii. **Statement of Contributions** (what is novel; what is engineering; what is reused — state
  this explicitly and honestly, examiners look for it)
- xiv. Ethics Statement and Data-Protection Declaration

---

# PART I — PROBLEM AND CONTEXT

## Chapter 1 — Introduction

1.1 Research Context: Alumni Relations as an Institutional Function
1.2 Problem Domain: the Govt. Haraganga College Alumni Association
1.3 Problem Statement
1.4 **Research Questions** — stated formally, each answerable by evidence in this document
  - RQ1 — What functional and quality requirements characterise an alumni-management platform
    for a resource-constrained institution in a low-bandwidth, mobile-first, cash-and-manual-
    payment context?
  - RQ2 — Which architectural approach best satisfies those requirements under a
    single-maintainer, low-budget sustainability constraint, and at what cost?
  - RQ3 — To what extent can institutional governance processes (constitution, elections,
    committee terms, member voting) be encoded as software without loss of procedural
    legitimacy?
  - RQ4 — What measurable quality is achieved by the resulting artefact against ISO/IEC 25010
    characteristics, and what does that reveal about the approach?
1.5 Aims and Objectives (each objective mapped to an RQ)
1.6 Scope, Delimitations and Assumptions
1.7 Research Method in Brief (forward reference to Chapter 4)
1.8 Contributions of this Work
1.9 Stakeholders and Beneficiaries
1.10 Structure of the Dissertation

**Diagrams**
- Figure 1.1 — **Context Diagram (DFD Level 0)**: platform boundary and external entities.
- Figure 1.2 — **Stakeholder / Onion Diagram**: core, direct, indirect and regulatory rings.
- Figure 1.3 — **Research-question ↔ objective ↔ chapter map** — one page an examiner can use
  as a navigation key. Include it; it disproportionately helps.

---

## Chapter 2 — Literature Review and Related Work

> Written to Kitchenham & Charters' SLR guidance, scaled to a *structured* review rather than a
> full systematic review. State the protocol; do not pretend to more rigour than you performed.

2.1 Review Objectives and Questions
2.2 Review Protocol — sources searched, search strings, date range, inclusion and exclusion
    criteria, screening procedure, number of records at each stage
2.3 Alumni Relations and Engagement: the Institutional Literature
2.4 Community and Membership Platforms: Academic Treatment
2.5 Architectural Literature
  2.5.1 Layered and Hexagonal/Clean Architecture: claims and critiques
  2.5.2 Monolith versus Microservices for Small-Team Systems
  2.5.3 Architectural Sustainability and Maintainability Evidence
2.6 Web and Mobile Engineering Literature (Pressman Part 3; ISO/IEC 25010 quality model)
2.7 Digital Governance, e-Voting and Procedural Legitimacy — the literature that bounds RQ3,
    including why this system deliberately does **not** claim to be a secure-election system
2.8 Security and Privacy Engineering Baselines (OWASP ASVS, STRIDE, data-protection principles)
2.9 Survey of Existing Systems and Products
  2.9.1 Commercial alumni platforms
  2.9.2 Open-source community/membership systems
  2.9.3 General-purpose CRM adapted to alumni use
2.10 Comparative Analysis and Evaluation Criteria
2.11 **Research Gap** — the argued transition from literature to this work
2.12 Summary

**Diagrams**
- Figure 2.1 — **PRISMA-style Study-Selection Flow Diagram** (identified → screened → eligible →
  included, with exclusion counts).
- Figure 2.2 — **Concept Map / Taxonomy** of the reviewed literature.
- Figure 2.3 — **As-Is Process Model of Current Manual Practice** (BPMN or activity notation).
- Figure 2.4 — **Positioning Chart**: existing systems plotted on two decisive axes
  (e.g. governance depth × operating cost).
- Table 2.1 — **Review protocol summary** (source, string, hits, included).
- Table 2.2 — **Feature and capability comparison matrix** (systems × criteria).
- Table 2.3 — **Gap table**: capability required → covered by which existing system → residual gap.

---

## Chapter 3 — Requirements Engineering

3.1 Requirements Elicitation
  3.1.1 Techniques applied (stakeholder interviews, document analysis of the constitution and
        election rules, observation of current practice, competitor analysis)
  3.1.2 Participants, sampling and instruments
  3.1.3 Ethical approval and informed consent
3.2 Requirements Analysis and Negotiation
3.3 Requirements Specification (IEEE/ISO/IEC/IEEE 29148 structure)
  3.3.1 Membership, Registration and Profile
  3.3.2 Authentication, OTP and Session Management
  3.3.3 Events, Registration, Attendance and Gallery
  3.3.4 Payments, Dues and Financial Records
  3.3.5 News, Notices and Communication
  3.3.6 Governance — EC Terms, Constitution Hub, Elections, Polls and Voting
  3.3.7 Administration, Configuration and Site Content
  3.3.8 Mobile Application Requirements
3.4 **Non-Functional Requirements, classified by ISO/IEC 25010** (functional suitability,
    performance efficiency, compatibility, usability, reliability, security, maintainability,
    portability), cross-referenced to Pressman's FURPS+ for practitioner readability
3.5 Quality-Attribute Scenarios — each NFR expressed as source–stimulus–artefact–response–
    response *measure*, so it is testable rather than aspirational (Bass et al.)
3.6 Use-Case Modelling
3.7 User Stories, Acceptance Criteria and the Definition of Done
3.8 Requirements Prioritisation (MoSCoW, with the negotiation recorded)
3.9 Requirements Traceability
3.10 Domain Constraints — constitutional and electoral rules that the software must not violate
3.11 Feasibility Analysis (technical, economic, operational, schedule, legal, ethical)
3.12 Requirements Validation and Formal Technical Review
3.13 Summary

**Diagrams**
- Figure 3.1 — **Use-Case Diagram, system level** (all actors, packaged).
- Figures 3.2–3.6 — **Use-Case Diagrams per subsystem**: Membership, Events, Payments,
  Governance, Administration.
- Figure 3.7 — **Actor Generalisation Hierarchy** (Guest → Member → EC Member → Admin →
  SuperAdmin).
- Figure 3.8 — **Domain Model / Conceptual Class Diagram** — analysis-level, no implementation
  detail. This is *not* the design class diagram of Chapter 6; keep them distinct.
- Figure 3.9 — **Quality-Attribute Utility Tree** (quality attribute → refinement → scenario,
  with (business value, technical risk) annotations).
- Figure 3.10 — **Requirements classification tree / FURPS+ breakdown**.
- Figure 3.11 — **Goal Model** (KAOS or i* style) linking stakeholder goals to system
  requirements — optional, but it visibly raises the analytical level.
- Table 3.1 — **Functional requirement catalogue** (ID, statement, source, priority, RQ link).
- Table 3.2 — **NFR catalogue by ISO 25010 characteristic, with measurable acceptance criteria**.
- Table 3.3 — **Use-case descriptions** (actor, preconditions, main flow, alternates, exceptions,
  postconditions) for the ten highest-value cases; the remainder in Appendix B.
- Table 3.4 — **Requirements Traceability Matrix** — requirement → use case → design element →
  implementation artefact → test case. Carry this forward and *close it* in Chapter 12.

---

# PART II — METHOD AND DESIGN

## Chapter 4 — Research Methodology

> The chapter that most undergraduate reports omit and no Master's dissertation may.

4.1 Research Paradigm and Philosophical Position (pragmatism; artefact-centred inquiry)
4.2 **Design Science Research as the Governing Method** — Hevner's three cycles (relevance,
    design, rigour) and Peffers' six activities, instantiated for this project
4.3 Mapping DSR Activities to the Work Performed
4.4 Software Process Model and its Justification — incremental/iterative delivery evaluated
    against waterfall, spiral and agile alternatives (Pressman Part 1), with the single-maintainer
    and unpaid-hours constraints as the deciding factors
4.5 **Evaluation Strategy** — what will be measured, with which instrument, against which
    baseline, and what result would count as failure. Defined *here*, before Chapter 12 reports it
  4.5.1 Functional evaluation — requirement coverage and traceability closure
  4.5.2 Quality evaluation — static product metrics and test adequacy
  4.5.3 Performance evaluation — workload model, endpoints measured, environment
  4.5.4 Security evaluation — ASVS-level checklist and threat-model coverage
  4.5.5 Usability evaluation — task-based testing, SUS instrument, heuristic walkthrough
  4.5.6 Expert/stakeholder evaluation — protocol and participants
4.6 Metrics Definition (each with formula, tool, and interpretation threshold stated in advance)
4.7 Data Collection and Analysis Procedures
4.8 **Risk Management — RMMM Plan** (Pressman): risk identification, projection (probability ×
    impact), the RMMM table, and the risk-monitoring record kept during the project
4.9 Research Ethics — consent, anonymisation, personal-data handling, storage and retention
4.10 Limitations of the Chosen Method
4.11 Summary

**Diagrams**
- Figure 4.1 — **Design Science Research Framework** with this project's instantiation labelled.
- Figure 4.2 — **DSR Process Model** (Peffers' six activities) as executed, including the
  iteration loops actually taken.
- Figure 4.3 — **Research Design Overview** — phases, inputs, outputs, evaluation points.
- Figure 4.4 — **Process Model Diagram** of the adopted incremental lifecycle.
- Figure 4.5 — **Risk Exposure Matrix** (probability × impact, plotted).
- Table 4.1 — **Evaluation plan** (RQ → criterion → metric → instrument → threshold).
- Table 4.2 — **Metric definitions** (name, formula, tool, target).
- Table 4.3 — **RMMM table** (risk, category, probability, impact, mitigation, monitoring signal,
  management response, outcome).

---

## Chapter 5 — System Analysis and Behavioural Modelling

5.1 Analysis Approach — structured and object-oriented models used complementarily, and why
5.2 Structured Analysis: Data-Flow Modelling
  5.2.1 Context level
  5.2.2 Level 1 decomposition
  5.2.3 Level 2 decompositions of the critical processes
  5.2.4 Process specifications and data-store definitions
5.3 Object-Oriented Analysis
  5.3.1 Noun/verb analysis and candidate classes
  5.3.2 **CRC modelling** (Pressman) — class, responsibilities, collaborators
  5.3.3 Analysis class relationships
5.4 Behavioural Modelling — Activity, Sequence and Interaction views
5.5 State Modelling of Long-Lived Entities
5.6 Business Rules Catalogue — including the rules imported verbatim from the association's
    constitution and election code, each tagged with the article that mandates it
5.7 Data Modelling — conceptual to logical
5.8 Analysis Model Review and Validation
5.9 Summary

**Diagrams**
- Figure 5.1 — **DFD Level 0 (Context)**.
- Figure 5.2 — **DFD Level 1** — Manage Membership, Authenticate, Manage Events, Process
  Payments, Publish Content, Govern, Administer, with data stores.
- Figures 5.3–5.7 — **DFD Level 2** for Payment Processing, Membership Approval,
  Authentication/OTP, Constitution Publication and Election Administration.
- Figure 5.8 — **CRC Card Set** (rendered as a figure, not an appendix afterthought).
- Figure 5.9 — **Activity Diagram — Member registration and admin approval**.
- Figure 5.10 — **Activity Diagram — Payment submission and manual verification**.
- Figure 5.11 — **Activity Diagram — Event creation, registration and attendance**.
- Figure 5.12 — **Activity Diagram — Constitution amendment and member voting**.
- Figure 5.13 — **Swimlane / Partitioned Activity Diagram** — Member | Admin | System | External.
- Figure 5.14 — **BPMN Process Diagram** of the election cycle (BPMN, not UML, because the
  audience for this one is the association's officers, not engineers).
- Figure 5.15 — **State-Machine Diagram — Member lifecycle** (Registered → Pending → Active →
  Lapsed → Suspended → Reinstated), with guards and events labelled.
- Figure 5.16 — **State-Machine Diagram — Payment / Membership Due**.
- Figure 5.17 — **State-Machine Diagram — Constitution version** (Draft → Ratified/Active →
  Superseded), including the always-latest invariant.
- Figure 5.18 — **State-Machine Diagram — Event lifecycle**.
- Figure 5.19 — **Sequence Diagram — Login with OTP, token issue and refresh**.
- Figure 5.20 — **Sequence Diagram — Event registration**.
- Figure 5.21 — **Sequence Diagram — Payment submission and verification**.
- Figure 5.22 — **Sequence Diagram — Real-time notification delivery (SignalR)**.
- Figure 5.23 — **Communication (Collaboration) Diagram** of the payment interaction — the same
  collaboration shown structurally, with the link numbering.
- Figure 5.24 — **Interaction Overview Diagram** — how the major interactions compose across a
  membership year.
- Figure 5.25 — **Timing Diagram** — token lifetime, refresh window and session expiry, drawn to
  scale. Rare in student work and disproportionately persuasive.
- Table 5.1 — **Process specifications** for the Level-2 processes.
- Table 5.2 — **Business rules catalogue** (rule ID, statement, source article, enforcement point
  in the system).
- Table 5.3 — **Data-store / entity definitions** at analysis level.

---

## Chapter 6 — System Architecture and Design

6.1 Design Goals, Principles and Constraints
6.2 **Architectural Alternatives Considered and the Decision** — layered/Clean, modular monolith,
    microservices, serverless: assessed against the quality-attribute scenarios of §3.5 and the
    operating-cost constraint. Show the trade-off, not just the winner
6.3 Architectural Design — Clean Architecture
  6.3.1 Domain layer
  6.3.2 Application layer — interfaces and DTOs
  6.3.3 Infrastructure layer — services, persistence, providers
  6.3.4 API layer — controllers, hubs, middleware
  6.3.5 Presentation layers — Angular web client, Flutter mobile client
  6.3.6 The dependency rule and how it is enforced rather than merely intended
6.4 Component-Level Design
6.5 Data Design
  6.5.1 Conceptual → logical → physical progression
  6.5.2 Normalisation to 3NF and the deliberate denormalisations, each justified
  6.5.3 Indexing strategy
  6.5.4 Multi-provider portability (PostgreSQL / MySQL / SQLite) and its design cost
  6.5.5 Data dictionary
  6.5.6 Seeding and runtime data-synchronisation strategy, and the `EnsureCreated()` constraint
        that forced it
6.6 Interface Design — API resource model, error contract, status-code discipline, versioning
6.7 Security Architecture (detailed in Chapter 9; summarised here as a design view)
6.8 User-Interface Design
  6.8.1 Design principles and information architecture
  6.8.2 Design-token system, theming and the single-stylesheet decision
  6.8.3 Shared control library and the duplication it eliminates
  6.8.4 Responsive and accessibility strategy (WCAG 2.1 AA target)
  6.8.5 WebApp design pyramid applied (Pressman Part 3): interface, aesthetic, content,
        navigation, architecture, component design
6.9 Mobile Application Design and Platform-Specific Concerns
6.10 Configuration-Driven Design — feature flags and organisation configuration as first-class
     design elements rather than settings
6.11 **Design Patterns Applied**, each with the forces that justified it and the alternative
     rejected (Repository, Service Layer, DTO, Dependency Injection, Strategy for payment
     providers, Observer for notifications, Adapter for storage providers)
6.12 **Architecture Decision Records** — the significant decisions, each with context, options,
     decision and consequences
6.13 Design Verification: architecture review against the utility tree
6.14 Summary

**Diagrams**
- Figure 6.1 — **High-Level Architecture Diagram** (clients, API, services, stores, externals).
- Figure 6.2 — **Layered / Clean Architecture Diagram** with inward dependency arrows and the
  dependency-inversion boundary marked.
- Figure 6.3 — **Entity–Relationship Diagram, full schema** (fold-out; also in Appendix G).
- Figures 6.4–6.7 — **ER sub-models**: Membership, Events, Finance, Governance.
- Figure 6.8 — **Design Class Diagram — domain model** with attributes, operations, visibility
  and multiplicities.
- Figure 6.9 — **Design Class Diagram — application interfaces and infrastructure services**.
- Figure 6.10 — **Design Class Diagram — client-side services and models**.
- Figure 6.11 — **Package Diagram** with permitted dependency directions.
- Figure 6.12 — **Component Diagram** with provided/required interfaces (lollipop/socket).
- Figure 6.13 — **Composite Structure Diagram** of the request-handling internals — parts, ports
  and connectors.
- Figure 6.14 — **Object Diagram** — a populated snapshot of one member with dues, records and
  registrations.
- Figure 6.15 — **Middleware Pipeline Diagram** — the ordered request path, with the ordering
  constraints annotated.
- Figure 6.16 — **Authentication and Token-Refresh Design Flow**.
- Figure 6.17 — **Navigation / Route Map** — public, portal and admin trees with guards.
- Figure 6.18 — **Site Map** and information architecture of the public site.
- Figures 6.19–6.26 — **Wireframes** (low-fidelity) and **UI Mockups** (high-fidelity) for
  landing, registration, portal dashboard, payment, admin member list, constitution reader,
  mobile home, mobile login.
- Figure 6.27 — **Design-Token Derivation Diagram** — light/dark theme token resolution.
- Figure 6.28 — **Mobile Screen-Flow Diagram**.
- Figure 6.29 — **Architectural Trade-off Radar/Spider Chart** — the candidate architectures of
  §6.2 scored across the quality attributes.
- Figure 6.30 — **UML Profile Diagram** for the project's stereotypes, if custom stereotypes are
  used anywhere in the models (include only if genuinely used).
- Table 6.1 — **Data dictionary** (table, column, type, constraint, description, source).
- Table 6.2 — **API endpoint catalogue** (method, route, auth level, request, response, errors).
- Table 6.3 — **Design patterns**: pattern, forces, applied where, alternative rejected.
- Table 6.4 — **Architecture Decision Record index**.
- Table 6.5 — **Quality-attribute scenario → architectural tactic** mapping.

---

# PART III — CONSTRUCTION AND VALIDATION

## Chapter 7 — Implementation

7.1 Development Environment, Toolchain and Reproducibility
7.2 Solution and Module Structure
7.3 Coding Standards, Conventions and Static Enforcement
7.4 Implementation of the Domain and Persistence Layers
7.5 Implementation of the Application and Business Services
7.6 Implementation of the API Layer
7.7 Implementation of the Web Client
7.8 Implementation of the Mobile Client
7.9 Real-Time Features
7.10 Security Implementation
7.11 Document Generation (ID cards, certificates, credential PDFs)
7.12 **Constitution Publication Pipeline** — the always-latest invariant, the build-time
     extraction tool, and why the naive approach fails on a live database
7.13 Third-Party Libraries: selection criteria, licence review and justification
7.14 **Software Configuration Management** — version control strategy, branching model, change
     control and release identification (Pressman)
7.15 Notable Implementation Challenges and Their Resolution — written as engineering analysis
     (symptom → hypothesis → evidence → resolution), not as a diary
7.16 Summary

**Diagrams**
- Figure 7.1 — **Module / Folder Structure Diagram** of the solution.
- Figure 7.2 — **Dependency Structure Matrix** of the assemblies, demonstrating that no
  dependency violates the rule stated in §6.3.6. This is *evidence*, not decoration.
- Figure 7.3 — **Build and Bundle Pipeline** for the web client.
- Figure 7.4 — **Git Branching Model Diagram**.
- Figure 7.5 — **Flowchart — Constitution publication**, ratified PDF to every surface.
- Figure 7.6 — **Flowchart — File upload, validation and storage**.
- Figure 7.7 — **Algorithm Flowchart — membership-due calculation**.
- Figure 7.8 — **Control-Flow Graph** of a representative complex method, annotated with its
  cyclomatic complexity (feeds Chapter 8's basis-path testing).
- Listings 7.1–7.n — representative implementation extracts, each ≤ 30 lines and discussed.
- Table 7.1 — **Third-party dependencies** (library, version, purpose, licence, alternative
  considered).
- Table 7.2 — **Module implementation status matrix**.
- Table 7.3 — **Size metrics** by layer (files, LOC, classes) — the input to Chapter 8's metrics.

---

## Chapter 8 — Verification, Validation and Quality Assurance

> Chapter 8 must demonstrate *technique*, not merely report that tests pass. This is the single
> most common place a competent report fails to reach Master's level.

8.1 V&V Strategy and Test Levels
8.2 **Software Quality Assurance Plan** — reviews, standards conformance, defect prevention;
    the role of formal technical reviews in this project (Pressman; IEEE 730)
8.3 **Test-Case Design Techniques Applied** — the substance of this chapter
  8.3.1 Equivalence partitioning
  8.3.2 Boundary value analysis
  8.3.3 Decision-table testing for the dues and eligibility rules
  8.3.4 State-transition testing derived from the state machines of Chapter 5
  8.3.5 **Basis-path testing** using the control-flow graph and cyclomatic complexity of §7.8
  8.3.6 Use-case / scenario-based testing
  8.3.7 Exploratory testing and its recorded charters
8.4 Unit Testing — backend
8.5 Unit and Component Testing — web client
8.6 Widget and Golden Testing — mobile client
8.7 Integration Testing Strategy (and why big-bang was rejected)
8.8 System and End-to-End Testing
8.9 Regression Testing and Test Selection
8.10 Security Testing (mapped to the threat model of Chapter 9 and to OWASP ASVS)
8.11 Performance and Load Testing — workload model, environment, results
8.12 Usability and Accessibility Testing — task success, time on task, SUS scores, WCAG audit
8.13 User Acceptance Testing — participants, protocol, results, sign-off
8.14 **Product Metrics and Static Analysis** (Pressman Ch. on product metrics)
  8.14.1 Size — LOC and function points
  8.14.2 Complexity — cyclomatic complexity distribution, worst offenders
  8.14.3 Coupling and cohesion — CBO, LCOM, afferent/efferent coupling, instability
  8.14.4 Maintainability index and technical-debt estimate
  8.14.5 Test adequacy — statement, branch and mutation coverage where available
8.15 Defect Analysis — density, distribution, removal efficiency, root-cause categories
8.16 Threats to the Validity of the Evaluation (forward reference to §12.7)
8.17 Summary

**Diagrams**
- Figure 8.1 — **Test Pyramid** as actually realised, with counts.
- Figure 8.2 — **V-Model / V&V Mapping** — each development artefact to its verifying activity.
- Figure 8.3 — **CI Quality-Gate Pipeline Diagram**.
- Figure 8.4 — **Control-Flow Graph with basis paths enumerated** for the worked example.
- Figure 8.5 — **Coverage by layer** (bar chart).
- Figure 8.6 — **Cyclomatic complexity distribution** (histogram).
- Figure 8.7 — **Coupling/instability scatter** — the main sequence with the modules plotted.
- Figure 8.8 — **Defect distribution** by severity, module and injection phase.
- Figure 8.9 — **Defect-removal efficiency by phase**.
- Figure 8.10 — **Performance results** — latency distribution by endpoint, throughput under load.
- Figure 8.11 — **SUS score distribution** with the 68-point benchmark line.
- Table 8.1 — **Equivalence classes and boundary values** for a representative input domain.
- Table 8.2 — **Decision table** for dues/eligibility.
- Table 8.3 — **State-transition test table**.
- Table 8.4 — **Test-case catalogue** (ID, technique, requirement, input, expected, actual,
  status) — full set in Appendix F.
- Table 8.5 — **Product metrics summary** against the thresholds declared in §4.6.
- Table 8.6 — **Defect log**.
- Table 8.7 — **UAT results and sign-off**.

---

## Chapter 9 — Security, Privacy and Trust

9.1 Security Objectives and Assumptions
9.2 **Threat Modelling (STRIDE)** — assets, entry points, trust boundaries, enumerated threats
9.3 Authentication and Session Security
9.4 Authorisation Model and the Role–Permission Matrix
9.5 Input Validation and Output Sanitisation
9.6 File Upload Security
9.7 Transport, Header and Browser-Policy Security
9.8 Rate Limiting and Abuse Prevention
9.9 Payment-Related Risk and the Deliberate No-Gateway-Keys Posture — the argued security
    rationale for manual verification, and its accepted operational cost
9.10 Governance Integrity — why the voting features are scoped as *sentiment and internal
    decision-making* and explicitly not as a secure-election system, with reference to §2.7
9.11 Personal Data: Lawful Basis, Minimisation, Consent, Retention and Subject Rights
9.12 Audit Logging and Non-Repudiation
9.13 Conformance Assessment against OWASP ASVS
9.14 Residual Risks and Recommendations
9.15 Summary

**Diagrams**
- Figure 9.1 — **Threat Model / Data-Flow Diagram with Trust Boundaries**, STRIDE-annotated.
- Figure 9.2 — **Attack Tree** for the highest-value asset (member account takeover or fraudulent
  payment credit).
- Figure 9.3 — **Role–Permission Matrix Diagram**.
- Figure 9.4 — **Personal-Data Classification and Flow Diagram**, with retention points.
- Figure 9.5 — **Sequence Diagram — an unauthorised request rejected** through the middleware
  chain.
- Figure 9.6 — **Defence-in-Depth Layer Diagram**.
- Table 9.1 — **STRIDE threat enumeration** with mitigations and their implementation location.
- Table 9.2 — **Role × capability matrix**.
- Table 9.3 — **OWASP ASVS conformance checklist** with verdicts.
- Table 9.4 — **Personal-data inventory** (element, purpose, lawful basis, retention).
- Table 9.5 — **Residual risk register**.

---

## Chapter 10 — Deployment and Operations

10.1 Deployment Architecture
10.2 Environment Topology and Configuration Differences
10.3 Containerisation Strategy
10.4 Continuous Integration and Continuous Deployment
10.5 Database Provisioning, Migration and Live Data Synchronisation
10.6 Configuration and Secret Management
10.7 Observability — logging, monitoring, alerting, error reporting
10.8 Backup, Recovery and Business Continuity
10.9 Release and Rollback Procedure
10.10 Operational Cost Model and Sustainability under Institutional Budget Constraints
10.11 Maintenance Plan and Handover
10.12 Summary

**Diagrams**
- Figure 10.1 — **UML Deployment Diagram** — nodes, artifacts, communication paths, protocols.
- Figure 10.2 — **Network / Infrastructure Diagram** with trust boundaries.
- Figure 10.3 — **CI/CD Pipeline Diagram** — commit → build → test → package → deploy.
- Figure 10.4 — **Environment Promotion Diagram**.
- Figure 10.5 — **Container Composition Diagram**.
- Figure 10.6 — **Sequence Diagram — application boot and runtime data synchronisation**.
- Figure 10.7 — **Backup and Recovery Flow** with RPO/RTO marked.
- Table 10.1 — **Environment variable and configuration key catalogue**.
- Table 10.2 — **Operational cost model** (component, tier, monthly cost, scaling trigger).
- Table 10.3 — **Runbook**: symptom → diagnosis → action.

---

## Chapter 11 — Project Management

11.1 Process Model in Practice and its Deviations from Plan
11.2 Work Breakdown Structure
11.3 Scheduling, Task Network and Critical Path
11.4 **Effort Estimation** — function-point count, LOC-based estimate and COCOMO II applied,
     compared against actual effort with the variance analysed (Pressman)
11.5 **Progress Tracking and Earned Value Analysis** — PV, EV, AC, SPI, CPI
11.6 Team Structure and Responsibilities
11.7 Configuration and Change Management in Practice
11.8 Risk Monitoring Record — the RMMM plan of §4.8 as it actually played out
11.9 Quality Assurance Activities Performed
11.10 Lessons in Project Management
11.11 Summary

**Diagrams**
- Figure 11.1 — **Work Breakdown Structure**.
- Figure 11.2 — **Gantt Chart** — planned versus actual.
- Figure 11.3 — **PERT / Activity-on-Node Network** with the critical path highlighted.
- Figure 11.4 — **Milestone Timeline**.
- Figure 11.5 — **Earned Value Chart** (PV/EV/AC over time).
- Figure 11.6 — **Effort Distribution** by phase and by module.
- Figure 11.7 — **Burndown / Velocity Chart** across increments.
- Figure 11.8 — **Estimated versus actual effort** with variance.
- Table 11.1 — **Milestone and deliverable schedule**.
- Table 11.2 — **Function point count** (EI, EO, EQ, ILF, EIF with complexity weighting).
- Table 11.3 — **COCOMO II parameters and result**.
- Table 11.4 — **Cost model**.
- Table 11.5 — **Change log** — significant scope changes with cause and impact.

---

# PART IV — EVALUATION AND CLOSURE

## Chapter 12 — Results, Evaluation and Discussion

> Executes the plan declared in §4.5. Nothing new is measured here that was not defined there.

12.1 Overview of the Delivered Artefact
12.2 Functional Evaluation — requirement coverage and the **closed** traceability matrix
12.3 Quality Evaluation against ISO/IEC 25010 — characteristic by characteristic, with the
     evidence and the metric value for each
12.4 Performance Evaluation Results
12.5 Security Evaluation Results
12.6 Usability and Accessibility Evaluation Results
12.7 Stakeholder and Expert Evaluation
12.8 **Answering the Research Questions** — RQ1 through RQ4 answered explicitly, each with the
     evidence that supports the answer and the confidence that evidence warrants
12.9 Discussion — interpretation, and comparison against the literature of Chapter 2. Where does
     this work agree with prior findings, and where does it diverge?
12.10 Comparison against the Existing/Manual System
12.11 **Threats to Validity** — construct, internal, external and conclusion validity, each with
      the mitigation applied and the residual limitation acknowledged
12.12 Limitations of the Artefact
12.13 Reflection on the Design Science Contribution — what transfers beyond this institution
12.14 Summary

**Diagrams**
- Figures 12.1–12.20 — **Annotated screenshots** of the working system, grouped by module, each
  captioned with the requirement it evidences.
- Figure 12.21 — **Requirement coverage chart** (delivered / partial / deferred by module).
- Figure 12.22 — **ISO 25010 evaluation radar chart** — target versus achieved.
- Figure 12.23 — **Performance benchmark results**.
- Figure 12.24 — **Before/after process-efficiency comparison** against manual practice.
- Figure 12.25 — **RQ → evidence map** — a one-page synthesis figure.
- Table 12.1 — **Closed requirements traceability matrix** (requirement → design → code → test →
  evaluation result).
- Table 12.2 — **ISO 25010 evaluation** (characteristic, metric, target, achieved, verdict).
- Table 12.3 — **Objective achievement** against Chapter 1.
- Table 12.4 — **Threats to validity** with mitigations.

---

## Chapter 13 — Conclusion and Future Work

13.1 Summary of the Work
13.2 Contributions Restated and Substantiated
13.3 Answers to the Research Questions, in Brief
13.4 Practical Implications for Similar Institutions
13.5 Lessons Learned — technical, methodological and organisational
13.6 Future Work
  13.6.1 Near-term functional roadmap
  13.6.2 Technical debt and reengineering priorities
  13.6.3 Research directions opened by this work
13.7 Concluding Remarks

**Diagrams**
- Figure 13.1 — **Product and Research Roadmap**.
- Figure 13.2 — **Technical Debt Map** — modules plotted by debt against change frequency.

---

## Back Matter

- **References** — IEEE numeric style, applied consistently, every entry cited in the body
- Appendix A — Ethics Approval, Participant Information Sheet and Consent Form
- Appendix B — Complete Use-Case Descriptions
- Appendix C — Complete Requirements Specification and Traceability Matrix
- Appendix D — Selected Source-Code Listings
- Appendix E — Complete Data Dictionary and Schema DDL
- Appendix F — Complete API Reference
- Appendix G — Complete Test-Case Suite and Execution Results
- Appendix H — Full-Page Fold-Out Diagrams (ERD, design class diagram, system use-case diagram)
- Appendix I — Literature Review Protocol, Search Strings and Screening Log
- Appendix J — Evaluation Instruments (interview schedule, usability task script, SUS
  questionnaire, expert-review form)
- Appendix K — Raw Evaluation Data and Statistical Working
- Appendix L — Installation and Deployment Manual
- Appendix M — User Manual
- Appendix N — Administrator Manual
- Appendix O — Architecture Decision Records, in full
- Appendix P — Originality / Similarity Report
- Index

---

## Master Diagram Inventory

Every diagram type a submission-grade dissertation is expected to carry, and where it lands.
A type with no entry is a gap in the book.

| # | Diagram type | Chapter | Depicts |
| --- | --- | --- | --- |
| 1 | Context Diagram (DFD 0) | 1, 5 | System boundary and external entities |
| 2 | DFD Level 1 | 5 | Major processes and data stores |
| 3 | DFD Level 2 | 5 | Payments, approval, auth, constitution, elections |
| 4 | Stakeholder / Onion Diagram | 1 | Stakeholder rings |
| 5 | RQ ↔ objective ↔ chapter map | 1 | Navigation key for the examiner |
| 6 | PRISMA study-selection flow | 2 | Literature screening with counts |
| 7 | Concept map / taxonomy | 2 | Structure of the reviewed field |
| 8 | As-is process model (BPMN) | 2 | Current manual practice |
| 9 | Positioning chart | 2 | Existing systems on two decisive axes |
| 10 | Use-Case Diagram | 3 | System level and per subsystem |
| 11 | Actor generalisation hierarchy | 3 | Role inheritance |
| 12 | Domain / conceptual class model | 3 | Analysis-level classes, no implementation |
| 13 | Quality-attribute utility tree | 3 | Attribute → refinement → measurable scenario |
| 14 | Goal model (KAOS / i*) | 3 | Stakeholder goals → requirements |
| 15 | DSR framework and process model | 4 | Research method instantiated |
| 16 | Research design overview | 4 | Phases, inputs, outputs, evaluation points |
| 17 | Process model diagram | 4 | Adopted lifecycle |
| 18 | Risk exposure matrix | 4 | Probability × impact |
| 19 | CRC card set | 5 | Class responsibilities and collaborators |
| 20 | Activity Diagram | 5 | Registration, payment, events, amendment |
| 21 | Swimlane / partitioned activity | 5 | Cross-role responsibility |
| 22 | BPMN process diagram | 5 | Election cycle, for a non-engineer audience |
| 23 | State-Machine Diagram | 5 | Member, payment, constitution, event lifecycles |
| 24 | Sequence Diagram | 5, 9, 10 | Auth, registration, payment, SignalR, denial, boot |
| 25 | Communication Diagram | 5 | Payment collaboration, structural view |
| 26 | Interaction Overview Diagram | 5 | Composition of interactions across a year |
| 27 | Timing Diagram | 5 | Token lifetime, refresh window, session expiry |
| 28 | High-level architecture | 6 | Clients, API, stores, externals |
| 29 | Layered / Clean architecture | 6 | Dependency rule and inversion boundary |
| 30 | ER Diagram | 6 | Full schema plus readable sub-models |
| 31 | Design Class Diagram | 6 | Domain, services, client — full UML |
| 32 | Package Diagram | 6 | Assemblies and permitted dependencies |
| 33 | Component Diagram | 6 | Provided/required interfaces |
| 34 | Composite Structure Diagram | 6 | Parts, ports, connectors |
| 35 | Object Diagram | 6 | Populated runtime snapshot |
| 36 | Profile Diagram | 6 | Custom stereotypes, if used |
| 37 | Middleware pipeline | 6 | Ordered request path and ordering constraints |
| 38 | Navigation / route map | 6 | Route trees with guards |
| 39 | Site map | 6 | Public information architecture |
| 40 | Wireframes (low-fi) | 6 | Layout intent before styling |
| 41 | UI mockups (high-fi) | 6 | Final visual design |
| 42 | Screen-flow diagram | 6 | Mobile navigation |
| 43 | Design-token derivation | 6 | Theme resolution |
| 44 | Architecture trade-off radar | 6 | Candidate architectures scored |
| 45 | Module / folder structure | 7 | Solution layout |
| 46 | Dependency Structure Matrix | 7 | Evidence the dependency rule holds |
| 47 | Build / bundle pipeline | 7 | Client build stages |
| 48 | Git branching model | 7 | SCM strategy |
| 49 | Flowchart | 7 | Publication, upload, due calculation |
| 50 | Control-flow graph | 7, 8 | Complexity and basis-path derivation |
| 51 | Test pyramid | 8 | Realised test-level distribution |
| 52 | V-Model V&V mapping | 8 | Artefact → verifying activity |
| 53 | CI quality-gate diagram | 8, 10 | Pipeline stages |
| 54 | Metric charts | 8 | Coverage, complexity, coupling, defects |
| 55 | Coupling/instability scatter | 8 | Modules against the main sequence |
| 56 | Defect-removal efficiency chart | 8 | Phase containment |
| 57 | Performance / latency charts | 8, 12 | Endpoint latency and throughput |
| 58 | SUS score distribution | 8, 12 | Usability benchmark |
| 59 | Threat model DFD (STRIDE) | 9 | Attack surface and trust boundaries |
| 60 | Attack tree | 9 | Highest-value asset compromise paths |
| 61 | Role–permission matrix | 9 | Authorisation model |
| 62 | Data-classification flow | 9 | Personal-data movement and retention |
| 63 | Defence-in-depth layers | 9 | Control stacking |
| 64 | Deployment Diagram | 10 | Nodes, artifacts, protocols |
| 65 | Network / infrastructure | 10 | Hosts and boundaries |
| 66 | Environment promotion | 10 | Local → preprod → production |
| 67 | Container composition | 10 | Images and relationships |
| 68 | Backup/recovery flow | 10 | RPO and RTO |
| 69 | Work Breakdown Structure | 11 | Task decomposition |
| 70 | Gantt chart | 11 | Planned versus actual schedule |
| 71 | PERT / activity network | 11 | Dependencies and critical path |
| 72 | Milestone timeline | 11 | Delivery dates |
| 73 | Earned value chart | 11 | PV, EV, AC over time |
| 74 | Burndown / velocity | 11 | Increment progress |
| 75 | Effort distribution charts | 11 | Phase and module effort |
| 76 | Annotated screenshots | 12 | Delivered-system evidence |
| 77 | Coverage/achievement charts | 12 | Requirement and objective closure |
| 78 | ISO 25010 radar chart | 12 | Target versus achieved quality |
| 79 | Before/after comparison | 12 | Efficiency against manual practice |
| 80 | RQ → evidence map | 12 | One-page synthesis |
| 81 | Roadmap diagram | 13 | Future work |
| 82 | Technical debt map | 13 | Debt against change frequency |

---

## Standards and Sources to Write Against

Naming the standard a chapter conforms to is itself a mark of level. Cite these properly.

| Area | Standard / source |
| --- | --- |
| Requirements specification | ISO/IEC/IEEE 29148 (supersedes IEEE 830) |
| Architecture description | ISO/IEC/IEEE 42010 |
| Design description | IEEE 1016 |
| Product quality model | **ISO/IEC 25010** (primary); FURPS+ as practitioner cross-reference |
| Quality in use / usability | ISO 9241-11; ISO/IEC 25022 |
| Test documentation | ISO/IEC/IEEE 29119 (supersedes IEEE 829) |
| SQA planning | IEEE 730 |
| Configuration management | IEEE 828 |
| Accessibility | WCAG 2.1 AA |
| Application security verification | OWASP ASVS; OWASP Top 10 |
| Threat modelling | Shostack, *Threat Modeling*; Microsoft STRIDE |
| Research method | Hevner et al. (2004); Peffers et al. (2007); Runeson & Höst (2009) |
| Empirical method and validity | Wohlin et al., *Experimentation in Software Engineering* |
| Literature review protocol | Kitchenham & Charters, SLR guidelines; PRISMA |
| Architecture evaluation | Bass, Clements & Kazman, *Software Architecture in Practice* (ATAM, utility trees) |
| Design patterns | Gamma et al.; Fowler, *PoEAA*; Evans, *Domain-Driven Design* |
| Clean/hexagonal architecture | Martin, *Clean Architecture*; Cockburn, hexagonal architecture |
| General SE process, metrics, estimation, testing technique | **Pressman & Maxim, _Software Engineering: A Practitioner's Approach_** |
| Estimation models | Boehm, COCOMO II; IFPUG function-point counting practices |
| Project management | PMBOK Guide; Pressman Part 4 |

### How Pressman maps onto this outline

Pressman is the right backbone for Parts II and III and for Chapter 11, and is *not* a substitute
for the research-method and empirical-validity sources above. Use it where it is strong:

| Pressman topic | Where it lands here |
| --- | --- |
| Process models; prescriptive vs agile; process selection | §4.4, §11.1 |
| Requirements engineering tasks and analysis modelling | Ch. 3, Ch. 5 |
| FURPS+ | §3.4 (as cross-reference to ISO 25010) |
| Scenario-based, class-based, flow and behavioural modelling; **CRC cards** | Ch. 5 |
| Design concepts: abstraction, modularity, information hiding, functional independence | §6.1 |
| Architectural design, styles and refinement | §6.2, §6.3 |
| Component-level and interface design | §6.4, §6.6 |
| WebApp and MobileApp design (design pyramid, quality attributes) | §6.8, §6.9 |
| Pattern-based design | §6.11 |
| SQA, formal technical reviews, defect amplification | §8.2, §8.15 |
| Testing strategies: unit → integration → validation → system | §8.1, §8.4–8.9 |
| **White-box: basis path, control-flow graphs, cyclomatic complexity** | §8.3.5, §7.8 |
| **Black-box: equivalence partitioning, BVA, decision tables, orthogonal array** | §8.3.1–8.3.4 |
| **Product metrics: complexity, coupling, cohesion, maintainability** | §8.14 |
| Process and project metrics; defect removal efficiency | §8.15, §11.9 |
| **Estimation: FP, LOC, COCOMO II, make-vs-buy** | §11.4 |
| **Scheduling: task networks, timeline charts, earned value** | §11.3, §11.5 |
| **Risk management: identification, projection, RMMM** | §4.8, §11.8 |
| Configuration management and change control | §7.14, §11.7 |
| Maintenance, reengineering, technical debt | §13.6.2 |

**What Pressman will not give you, and where to get it instead:** research questions and
contribution framing (Hevner, Peffers); literature-review protocol (Kitchenham, PRISMA); threats
to validity (Wohlin); architecture evaluation with utility trees and ATAM (Bass et al.); usability
instruments (Nielsen, SUS, ISO 9241-11); modern application-security verification (OWASP ASVS).
A dissertation built on Pressman alone reads as a very good engineering report — which is exactly
the ceiling this revision is meant to break through.

---

## Source Material Already in This Repository

Most diagrams can be derived from existing artefacts rather than invented. What is genuinely
missing is research framing, evaluation data and literature — those must be produced.

| Diagram / chapter group | Derive from |
| --- | --- |
| Architecture, layering, middleware, data flows | `docs/architecture_data_flow.md`, `docs/project_map.md` |
| Class, ER, data dictionary, API catalogue | `docs/project_map.md`, `GHCAA.Infrastructure/Data/ApplicationDbContext.cs`, the controllers |
| Use cases, functional requirements, traceability | `docs/SRS.md`, `docs/FEATURES.md` |
| Business rules, state machines, payment flow | `docs/BUSINESS_FINDINGS.md`, `docs/PAYMENT_GATEWAY_WORKFLOW.md` |
| Configuration-driven design and feature flags | `docs/CONFIG_DRIVEN_FRAMEWORK.md` |
| Deployment, CI/CD, environments, cost model | `docs/RENDER_DEPLOYMENT.md`, `Dockerfile`, the workflow files |
| Test strategy, coverage, defect analysis | `docs/BUSINESS_TEST_CHECKLIST.md`, `docs/low_coverage_report.md`, the test projects |
| UI design, tokens, shared controls | `docs/UI_UX_REMEDIATION_PLAN.md`, `docs/PROFILE_SHARED_COMPONENT_DESIGN.md`, `GHCAA.Web/src/styles.scss` |
| Governance, constitution and election processes | `docs/CONSTITUTION_PUBLISHING.md`, `docs/Elections/`, the ratified constitution |
| Schedule, WBS, change log, ADR material | `docs/TODO.md`, `docs/PLAN.md`, the git history |
| Metrics (complexity, coupling, LOC) | Static analysis over the solution — **to be generated**, §8.14 |
| Literature, evaluation data, validity analysis | **Not in the repository — must be produced** |
