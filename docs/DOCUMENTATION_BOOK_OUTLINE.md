# GHCAA Alumni Association Platform — Contents of the Project Documentation Book

**Purpose of this document.** It sets out the chapter structure, section headings, diagram
inventory and reference standards for the project documentation book, so that the contents can be
reviewed and approved before writing begins. It is not the book itself.

**Scale.** Four parts, thirteen chapters, front and back matter, eighty-two catalogued diagram and
chart types, sixteen appendices.

**Page budget: 150 to 200 pages for the whole volume.** Decided 2 September 2026. This is a
constraint on how the remaining chapters are written, not a target to trim towards afterwards.
Cutting finished prose costs several times what writing to length costs, and it removes the
qualifications and the negative findings first, which are the parts of this book that make it
credible.

Chapters 1 to 6 are written and measured. The rest is budgeted, not estimated: the figures below are
what each chapter is allowed, rather than what it would run to at the density Part I and Part II
actually print at.

Nothing of chapters 7 to 13 exists as a file. `docs/book/` holds `01` to `06` and the references, so
the printed PDF stops at the architecture chapter. Where the evidence for an unwritten chapter has
already been gathered, this outline says where it is kept, so that it is written up rather than
derived a second time. Tracked as Work Package 67 in `docs/TODO.md`.

| Part | State | Pages |
|---|---|---|
| Front matter | written; grows as the contents grow | 10–14 |
| Part I and Part II, chapters 1–6 | written, measured 2 September 2026 | 72 |
| Part III, chapters 7–11 | budgeted; no source file exists yet | 55 |
| Part IV, chapters 12–13 | budgeted; no source file exists yet | 19 |
| References | written; grows with Part III | 5–8 |
| Appendices carried in print | see the back matter below | 11–19 |
| Index, if the programme requires one | not started | 0–6 |
| **Total** | | **172–198** |

Per-chapter budget for the unwritten chapters. At the density of the written chapters these would
run to 82–99 pages, so each is roughly two pages tighter than it would otherwise be:

| Chapter | Pages |
|---|---|
| 7 Implementation | 12 |
| 8 Security, privacy and trust | 11 |
| 9 Verification, validation and quality assurance | 14 |
| 10 Deployment and operations | 9 |
| 11 Project management | 9 |
| 12 Results, evaluation and discussion | 14 |
| 13 Conclusion and future work | 5 |

Where a chapter cannot make its budget without dropping evidence, the evidence stays and the budget
is renegotiated here in writing. What must not happen is a chapter quietly running to twenty pages
and the total being discovered at binding.

**Appendix policy.** The appendices as originally specified came to 231–357 pages, which is longer
than the dissertation they support. That figure was not padding: it followed from what they promised
against real counts, being 276 endpoints for the API reference, 49 tables and their DDL for the data
dictionary, 898 automated tests for the test-case suite, and about forty-four remaining use-case
descriptions. Printing them
in full would have put the volume past 400 pages.

The decision is that only appendices an examiner needs in the bound copy are printed. Everything
exhaustive is a generated artefact in the repository, or a document delivered beside the
dissertation, and the book cites it precisely enough to be checked. This is the pattern the body
already uses: Tables 6.2 and 6.3 print a representative slice and name where the full catalogue
lives. Nothing is lost that a reader could not open; a sixty-page printed API reference is not read
by anyone.

**Front matter to state the length.** The abstract already states its own word count and the build
checks it. The same should hold for the volume: state the page count on the title page or in the
contents, so a reader knows what they are holding.

**Status of the figure and table numbers in this document.** The numbers used in the "Figures and
tables" list under each chapter below are the *plan*. The book itself numbers in bound order, which
`docs/book/build/renumber.py` assigns and maintains, so a figure planned here as 6.14 carries whatever
number its position gives it once written, and a planned figure that ends up in an appendix or is set
as a table takes no figure number at all. Read this document for what the chapter should cover, and
the book's own List of Figures for what it contains. Where the two disagree, the book is right.

**Conventions applied throughout the book**

- Front matter numbered in lower-case Roman; the body restarts at Arabic 1.
- Figures numbered `Figure <chapter>.<n>` in bound order, tables `Table <chapter>.<n>`, code listings `Listing <chapter>.<n>`; each of the three carries its own list in the front matter, rebuilt from the captions at build time.
- Every figure is drawn to print on one A4 page at a label size of at least 7pt, which the build measures and enforces; `docs/book/README.md` states how a diagram that does not meet that is fixed.
- Every figure caption states what the figure shows and the project artefact it was derived from, for example: *"Figure 6.3 — Entity–Relationship model of the 40 mapped entities; derived from `GHCAA.Infrastructure/Data/ApplicationDbContext.cs`."*
- Citations in IEEE numeric style; every non-obvious claim carries a citation.
- Code in the body appears as short illustrative extracts of no more than thirty lines, each discussed in the text; complete listings stay in the repository at the commit §7.1 names.
- UML 2.5 notation for all object-oriented models.

---

## Front Matter

- i. Title Page
- ii. Declaration of Originality / Authorship
- iii. Certificate of Approval (Supervisor / Examiner)
- iv. Acknowledgements
- v. Abstract — 250–350 words: context, problem, method, artefact, evaluation, finding. The written abstract states its own word count, and the build checks the statement against the text
- vi. Keywords
- vii. Table of Contents
- viii. List of Figures
- ix. List of Tables
- x. List of Listings
- xi. List of Abbreviations and Acronyms
- xii. Glossary of Domain Terms
- xiii. Statement of Contributions — what is novel, what is engineering, what is reused
- xiv. Ethics Statement and Data-Protection Declaration

---

# PART I — PROBLEM AND CONTEXT

## Chapter 1 — Introduction

- **1.1** Research Context: Alumni Relations as an Institutional Function
- **1.2** Problem Domain: the Govt. Haraganga College Alumni Association
- **1.3** Problem Statement
- **1.4** Conceptual Framework — the whole in one view before the detail: the rule set, current practice and the operating reality as inputs; the platform as the designed artefact built and evaluated over increments; the artefact and the evidence about it as outputs; and the evaluation findings returning as requirements
- **1.5** Research Questions, each answerable by evidence presented in this document
  - RQ1 — What functional and quality requirements characterise an alumni-management platform for a resource-constrained institution in a low-bandwidth, mobile-first, cash-and-manual-payment context?
  - RQ2 — Which architectural approach best satisfies those requirements under a single-maintainer, low-budget sustainability constraint, and at what cost?
  - RQ3 — To what extent can institutional governance processes (constitution, elections, committee terms, member voting) be encoded as software without loss of procedural legitimacy?
  - RQ4 — What measurable quality is achieved by the resulting artefact against ISO/IEC 25010 characteristics, and what does that reveal about the approach?
- **1.6** Aims and Objectives, each objective mapped to a research question
- **1.7** Scope, Delimitations and Assumptions
- **1.8** Research Method in Brief, with forward reference to Chapter 4
- **1.9** Contributions of this Work
- **1.10** Stakeholders and Beneficiaries
- **1.11** Structure of the Dissertation

**Figures and tables**

- Figure 1.1 — Context diagram (DFD Level 0): platform boundary and external entities
- Figure 1.2 — Stakeholder / onion diagram: core, direct, indirect and regulatory rings
- Figure 1.3 — Conceptual framework: inputs, the designed artefact and how it is judged, with the evaluation loop drawn rather than implied
- Figure 1.4 — Research question ↔ objective ↔ chapter map, on a single page

---

## Chapter 2 — Literature Review and Related Work

Written to Kitchenham & Charters' guidance, scaled to a structured review; the protocol actually
followed is stated in §2.2.

- **2.1** Review Objectives and Questions
- **2.2** Review Protocol — sources searched, search strings, date range, inclusion and exclusion criteria, screening procedure, number of records at each stage
- **2.3** Alumni Relations and Engagement: the Institutional Literature
- **2.4** Community and Membership Platforms: Academic Treatment
- **2.5** Architectural Literature
  - **2.5.1** Layered and hexagonal/clean architecture: claims and critiques
  - **2.5.2** Monolith versus microservices for small-team systems
  - **2.5.3** Architectural sustainability and maintainability evidence
- **2.6** Web and Mobile Engineering Literature
- **2.7** Digital Governance, Electronic Voting and Procedural Legitimacy — the literature bounding RQ3, including the reasoning for which this system does not claim to be a secure-election system
- **2.8** Security and Privacy Engineering Baselines — OWASP ASVS, STRIDE, data-protection principles
- **2.9** Survey of Existing Systems and Products
  - **2.9.1** Commercial alumni platforms
  - **2.9.2** Open-source community and membership systems
  - **2.9.3** General-purpose CRM adapted to alumni use
- **2.10** Comparative Analysis and Evaluation Criteria
- **2.11** Research Gap — the argued transition from the literature to this work
- **2.12** Summary

**Figures and tables**

- Figure 2.1 — Study-selection flow diagram: identified, screened, eligible, included, with exclusion counts
- Figure 2.2 — Concept map / taxonomy of the reviewed literature
- Figure 2.3 — As-is process model of current manual practice (BPMN)
- Figure 2.4 — Positioning chart: existing systems plotted on governance depth × operating cost
- Table 2.1 — Review protocol summary: source, search string, hits, included
- Table 2.2 — Feature and capability comparison matrix (systems × criteria)
- Table 2.3 — Gap table: capability required, covering system, residual gap

---

## Chapter 3 — Requirements Engineering

- **3.1** Sources of the Requirements — the four sources named, and what the Source column of §3.3 records. How each was worked, who took part and the ethical position belong to the method and are in §4.7 and §4.9; this chapter specifies from the output rather than restating the procedure
- **3.2** Requirements Analysis and Negotiation
- **3.3** Requirements Specification, structured to ISO/IEC/IEEE 29148
  - **3.3.1** Membership, Registration and Profile
  - **3.3.2** Authentication, OTP and Session Management
  - **3.3.3** Events, Registration, Attendance and Gallery
  - **3.3.4** Payments, Dues and Financial Records
  - **3.3.5** News, Notices and Communication
  - **3.3.6** Governance — Executive Committee terms, Constitution Hub, elections, polls and voting
  - **3.3.7** Administration, Configuration and Site Content
  - **3.3.8** Mobile Application Requirements
  - **3.3.9** Job Board — added after the elicitation of §3.1 closed, on a dated request recorded in the tracker; the Source column carries R for requirements of that provenance
- **3.4** Non-Functional Requirements, classified by ISO/IEC 25010:2011 characteristic — functional suitability, performance efficiency, compatibility, usability, reliability, security, maintainability, portability — cross-referenced to the FURPS+ model. The 2011 edition is named deliberately: 25010:2023 renames usability as interaction capability and portability as flexibility and adds safety, and the written §3.4 states why the identifiers were not reclassified
- **3.5** Quality-Attribute Scenarios — each non-functional requirement expressed as source, stimulus, artefact, response and response measure
- **3.6** Use-Case Modelling
- **3.7** User Stories, Acceptance Criteria and the Definition of Done
- **3.8** Requirements Prioritisation (MoSCoW), with the negotiation outcome recorded
- **3.9** Requirements Traceability — the chain from constitutional clause to requirement to rule to test, and the direction it does not yet run in: no test carries a requirement or constraint identifier, so the matrix is maintained by hand and could drift from the suite without failing
- **3.10** Domain Constraints — constitutional and electoral rules the software must not violate, each classified as deterministic, state, evidence, procedural or authority, which fixes what the software is permitted to do with it and supplies the answer to RQ3 in §12.8, where the research questions are answered
- **3.11** Feasibility Analysis — technical, economic, operational, schedule, legal and ethical
- **3.12** Requirements Validation and Formal Technical Review
- **3.13** Summary

**Figures and tables**

- Figure 3.1 — Use-case diagram, system level, all actors, packaged
- Figures 3.2–3.6 — Use-case diagrams per subsystem: Membership, Events, Payments, Governance, Administration
- Figure 3.7 — Actor generalisation hierarchy: Guest → Member → EC Member → Admin → SuperAdmin
- Figure 3.8 — Domain model / conceptual class diagram, analysis level, distinct from the design class diagram of Chapter 6
- Figure 3.9 — Quality-attribute utility tree: attribute, refinement, scenario, with business-value and technical-risk annotations
- Figure 3.10 — Requirements classification tree / FURPS+ breakdown
- Figure 3.11 — Goal model linking stakeholder goals to system requirements
- Table 3.1 — Functional requirement catalogue: ID, statement, source, priority, RQ link
- Table 3.2 — Non-functional requirement catalogue by ISO 25010 characteristic, with measurable acceptance criteria
- Table 3.3 — Use-case descriptions (actor, preconditions, main flow, alternates, exceptions, postconditions) for the ten highest-value cases; the remainder in `docs/SRS.md`
- Table 3.4 — Requirements Traceability Matrix: requirement → use case → design element → implementation artefact → test case, carried forward and closed in Chapter 12

---

# PART II — METHOD AND DESIGN

## Chapter 4 — Research Methodology

- **4.1** Research Paradigm and Philosophical Position — pragmatism; artefact-centred inquiry
- **4.2** Design Science Research as the Governing Method — Hevner's relevance, design and rigour cycles and Peffers' six activities, instantiated for this project
- **4.3** Mapping Design Science Activities to the Work Performed
- **4.4** Software Process Model and its Justification — incremental and iterative delivery evaluated against waterfall, spiral and agile alternatives, with the single-maintainer and unpaid-hours constraints as the deciding factors
- **4.5** Evaluation Strategy — what is measured, with which instrument, against which baseline, and what result counts as failure; defined here, before Chapter 12 reports it
  - **4.5.1** Functional evaluation — requirement coverage and traceability closure
  - **4.5.2** Quality evaluation — static product metrics and test adequacy
  - **4.5.3** Performance evaluation — workload model, endpoints measured, environment, and the reduction from a concurrent-load measurement to a single-client latency measurement, with the reason
  - **4.5.4** Security evaluation — ASVS-level checklist and threat-model coverage, carried out as the author's self-assessment and labelled as one
  - **4.5.5** Usability evaluation — four task scripts, one for ordinary members and one for each of the three offices, the System Usability Scale, and a heuristic walkthrough for the flows the sessions do not reach; instruments delivered in `docs/book/instruments/`
  - **4.5.6** Expert and stakeholder evaluation — protocol and participants
- **4.6** Metrics Definition — formula, tool and interpretation threshold for each metric, stated in advance of measurement
- **4.7** Data Collection and Analysis Procedures
  - **4.7.1** Elicitation techniques — document analysis of the constitution and election rules, stakeholder interviews, observation of current practice, competitor analysis, in the order applied and each chosen for what the previous one could not reach
  - **4.7.2** Participants, sampling and instruments — purposive sampling, the ten participants by role, the author's own position among them, and the interview, observation and coding instruments
  - **4.7.3** Analysis procedures — the four kinds of data collected and the analysis treatment each receives
- **4.8** Risk Management: the RMMM plan — risk identification, projection by probability and impact, the RMMM table, and the risk-monitoring record kept during the project
- **4.9** Research Ethics — consent, anonymisation, personal-data handling, storage and retention. This section owns the research-ethics account, both the participant question and the live-member-data question; the front-matter declaration states the position and Chapter 3 does not repeat it
- **4.10** Limitations of the Chosen Method
- **4.11** Summary

**Figures and tables**

- Figure 4.1 — Design Science Research framework with this project's instantiation labelled
- Figure 4.2 — Design Science process model as executed, including the iteration loops taken
- Figure 4.3 — Research design overview: phases, inputs, outputs, evaluation points
- Figure 4.4 — Process model diagram of the adopted incremental lifecycle
- Figure 4.5 — Risk exposure matrix (probability × impact)
- Table 4.1 — Evaluation plan: research question → criterion → metric → instrument → threshold
- Table 4.2 — Metric definitions: name, formula, tool, target
- Table 4.3 — RMMM table: risk, category, probability, impact, mitigation, monitoring signal, management response, outcome

---

## Chapter 5 — System Analysis and Behavioural Modelling

- **5.1** Analysis Approach — structured and object-oriented models used complementarily, and why
- **5.2** Structured Analysis: Data-Flow Modelling
  - **5.2.1** Context level
  - **5.2.2** Level 1 decomposition
  - **5.2.3** Level 2 decompositions of the critical processes, carrying the process specification for every numbered process (Table 5.3) and the data stores each reads and writes (Table 5.4)
- **5.3** Object-Oriented Analysis
  - **5.3.1** Noun and verb analysis; candidate classes
  - **5.3.2** CRC modelling — class, responsibilities, collaborators
  - **5.3.3** Analysis class relationships
- **5.4** Behavioural Modelling — activity, sequence and interaction views
- **5.5** State Modelling of Long-Lived Entities
- **5.6** Business Rules Catalogue — including the rules imported from the association's constitution and election code, each tagged with the article that mandates it
- **5.7** Data Modelling — conceptual only. The logical and physical progression belongs to §6.5.1 and is not repeated here; §5.7 stops where persistence concerns begin
  - **5.7.1** Conceptual to logical progression
  - **5.7.2** Multiplicities worth stating explicitly
- **5.8** Analysis Model Review and Validation
- **5.9** Summary

**Figures and tables**

- Figure 5.1 — DFD Level 0 (context)
- Figure 5.2 — DFD Level 1: Manage Membership, Authenticate, Manage Events, Process Payments, Publish Content, Govern, Administer, with data stores
- Figures 5.3–5.5 — DFD Level 2 for payment processing, membership approval and constitution publication. Authentication and OTP, and election administration, were planned here as Level-2 DFDs and are deliberately not drawn that way: in both the property that matters is the ordering of an interaction across roles, not the transformation of data, so they appear as the OTP sequence diagram and the election BPMN diagram instead. §5.2.3 of the written chapter states the reasoning
- Figure 5.8 — CRC card set
- Figure 5.9 — Activity diagram: member registration and administrative approval
- Figure 5.10 — Activity diagram: payment submission and manual verification
- Figure 5.11 — Activity diagram: event creation, registration and attendance
- Figure 5.12 — Activity diagram: constitution amendment and member voting
- Figure 5.13 — Swimlane activity diagram: Member | Admin | System | External
- Figure 5.14 — BPMN process diagram of the election cycle, in BPMN because its audience is the association's officers rather than engineers
- Figure 5.15 — State-machine diagram: member lifecycle (Registered → Pending → Active → Lapsed → Suspended → Reinstated), with guards and events labelled
- Figure 5.16 — State-machine diagram: payment and membership due
- Figure 5.17 — State-machine diagram: constitution version (Draft → Ratified/Active → Superseded), including the always-latest invariant
- Figure 5.18 — State-machine diagram: event lifecycle
- Figure 5.19 — Sequence diagram: login with OTP, token issue and refresh
- Figure 5.20 — Sequence diagram: event registration
- Figure 5.21 — Sequence diagram: payment submission and verification
- Figure 5.22 — Sequence diagram: real-time notification delivery over SignalR
- Figure 5.23 — Communication diagram of the payment interaction, with link numbering
- Figure 5.24 — Interaction overview diagram: how the major interactions compose across a membership year
- Figure 5.25 — Timing diagram: token lifetime, refresh window and session expiry, drawn to scale
- Table 5.1 — Process specifications for the Level-2 processes
- Table 5.2 — Business rules catalogue: rule ID, statement, source article, enforcement point
- Table 5.3 — Data-store and entity definitions at analysis level

---

## Chapter 6 — System Architecture and Design

- **6.1** Design Goals, Principles and Constraints
- **6.2** Architectural Alternatives Considered and the Decision Taken — layered/clean, modular monolith, microservices and serverless, assessed against the quality-attribute scenarios of §3.5 and the operating-cost constraint, with the trade-offs recorded
- **6.3** Architectural Design — Clean Architecture
  - **6.3.1** Domain layer
  - **6.3.2** Application layer — interfaces and data transfer objects
  - **6.3.3** Infrastructure layer — services, persistence, providers
  - **6.3.4** API layer — controllers, hubs, middleware
  - **6.3.5** Presentation layers — Angular web client, Flutter mobile client
  - **6.3.6** The dependency rule and the mechanism that enforces it
- **6.4** Component-Level Design
- **6.5** Data Design
  - **6.5.1** Logical and physical progression, taking the conceptual model of §5.7 as its input rather than restating it
  - **6.5.2** Normalisation to third normal form and the deliberate denormalisations, each justified
  - **6.5.3** Indexing strategy
  - **6.5.4** Multi-provider portability (PostgreSQL, MySQL, SQLite) and its design cost
  - **6.5.5** Data dictionary
  - **6.5.6** Schema and reference data at boot: migration bootstrapping, the legacy `EnsureCreated()`-built database that had no migration history, the false-baseline self-heal, and why revisable reference data is synchronised separately from schema
- **6.6** Interface Design — API resource model, error contract, status-code discipline, versioning
- **6.7** Security Architecture, summarised here as a design view and detailed in Chapter 8
- **6.8** User-Interface Design
  - **6.8.1** Design principles and information architecture
  - **6.8.2** Design-token system, theming and the single-stylesheet decision
  - **6.8.3** Shared control library and the duplication it eliminates
  - **6.8.4** Responsive design and accessibility strategy, targeting WCAG 2.1 AA
  - **6.8.5** Web application design pyramid applied: interface, aesthetic, content, navigation, architecture and component design
- **6.9** Mobile Application Design and Platform-Specific Concerns
- **6.10** Configuration-Driven Design — feature flags and organisation configuration as design elements
- **6.11** Design Principles: Claim, Mechanism and Evidence — each principle stated with the mechanism that enforces it, the code location that demonstrates it, and the measurement or test that would detect its violation
  - **6.11.1** Separation of concerns and the layer boundary
  - **6.11.2** Dependency inversion — the domain depends on abstractions only; enforced by project references and demonstrated by the dependency structure matrix of Figure 7.2
  - **6.11.3** Single responsibility — service decomposition, evidenced by the LCOM cohesion figures of §9.14.3
  - **6.11.4** Open/closed — where extension without modification is achieved (payment providers, database providers, storage providers) and where it is not
  - **6.11.5** Liskov substitution and interface segregation across the service interfaces
  - **6.11.6** Information hiding and encapsulation — internal versus public surface
  - **6.11.7** Coupling and cohesion, measured as CBO, afferent and efferent coupling and instability, with the modules plotted against Martin's main sequence
  - **6.11.8** Elimination of duplication — the shared control library and the single-stylesheet decision, with the duplication each removed quantified
  - **6.11.9** Convention over configuration — assembly-scanning dependency registration
  - **6.11.10** Principle of least astonishment in the API and user-interface contracts
  - **6.11.11** GRASP assignment of responsibility — information expert, creator, controller, polymorphism, pure fabrication, indirection, protected variations
  - **6.11.12** Principles deliberately traded away, and the reasoning — deferred generality over speculative abstraction, the modular monolith over service decomposition, and data transfer objects at the boundary
- **6.12** Design Patterns Applied — each documented in a fixed schema: problem and forces, pattern selected, participants in this system, consequences observed, alternative rejected and why
  - **6.12.1** Creational — Factory, Builder, Singleton via container lifetime
  - **6.12.2** Structural — Adapter (storage providers), Facade (service layer over EF Core), Decorator (middleware chain), Proxy (lazy loading and caching)
  - **6.12.3** Behavioural — Strategy (payment providers, database providers), Observer (real-time notification, Angular signals), Template Method, Chain of Responsibility (the middleware pipeline), Command (request handling)
  - **6.12.4** Enterprise application patterns — Repository, Unit of Work, Service Layer, Data Transfer Object, Domain Model, Identity Map
  - **6.12.5** Architectural patterns — layered/clean, dependency injection, MVC and MVVM at the clients, publish–subscribe, API gateway boundary
  - **6.12.6** Angular and Flutter presentation patterns — smart and presentational component split, reactive state with signals, guard and interceptor patterns, repository abstraction in the mobile client
  - **6.12.7** Anti-patterns identified and remediated during development — the god service, the anaemic domain drift, the raw-control styling leak, and how each was detected
- **6.13** Architecture Decision Records — the significant decisions, each with context, options, decision and consequences
- **6.14** Design Verification: architecture review against the quality-attribute utility tree
- **6.15** Summary

**Figures and tables**

- Figure 6.1 — High-level architecture diagram: clients, API, services, stores, external services
- Figure 6.2 — Layered / clean architecture diagram with inward dependency arrows and the dependency-inversion boundary marked
- Figures 6.3–6.6 — ER sub-models: identity and records, standing and money, events and participation, governance. Written as four diagrams rather than one full-schema fold-out, because one diagram of forty-nine tables cannot be printed at a readable size; the remaining tables are in the generated schema documentation
- Figure 6.7 — Design class diagram: domain model, with attributes, operations, visibility and multiplicities
- Figure 6.9 — Design class diagram: application interfaces and infrastructure services
- Figure 6.10 — Design class diagram: client-side services and models
- Figure 6.11 — Package diagram with permitted dependency directions
- Figure 6.12 — Component diagram with provided and required interfaces
- Figure 6.13 — Composite structure diagram of the request-handling internals: parts, ports, connectors
- Figure 6.14 — Object diagram: a populated snapshot of one member with dues, records and registrations
- Figure 6.15 — Middleware pipeline diagram: the ordered request path with ordering constraints annotated
- Figure 6.16 — Authentication and token-refresh design flow
- Figure 6.17 — Navigation and route map: public, portal and admin trees with guards
- Figure 6.18 — Site map and information architecture of the public site
- Wireframes and high-fidelity mockups for landing, registration, portal dashboard, payment, admin member list, constitution reader, mobile home and mobile login. Not in Chapter 6 as written, and not numbered as figures there: the system is built, so a wireframe drawn now would document the result rather than the intent. Annotated screenshots of the delivered screens are Figures 12.1–12.20, and any wireframe retained for the design record belongs in an appendix
- Figure 6.27 — Design-token derivation diagram: light and dark theme token resolution
- Figure 6.28 — Mobile screen-flow diagram
- Figure 6.29 — Architectural trade-off radar chart: the candidate architectures of §6.2 scored across the quality attributes
- UML profile diagram for the project's stereotypes: omitted, because the project defines no custom stereotypes. The condition "where custom stereotypes are used" is not met, and an empty profile diagram would be padding
- Table 6.1 — Data dictionary: table, column, type, constraint, description, source
- Table 6.2 — API endpoint catalogue: method, route, authorisation level, request, response, errors
- Table 6.3 — Design pattern catalogue: pattern, category, problem and forces, participants in this system, consequences observed, alternative rejected
- Table 6.4 — Design principle evidence table: principle → enforcing mechanism → code location → the metric or test that would detect a violation
- Table 6.5 — Architecture Decision Record index
- Table 6.6 — Quality-attribute scenario to architectural tactic mapping
- Table 6.7 — Anti-patterns detected and remediated: symptom, diagnosis, refactoring applied, metric before and after

---

# PART III — CONSTRUCTION AND VALIDATION

## Chapter 7 — Implementation

- **7.1** Development Environment, Toolchain and Reproducibility
- **7.2** Solution and Module Structure
- **7.3** Coding Standards, Conventions and Static Enforcement
- **7.4** Implementation of the Domain and Persistence Layers
- **7.5** Implementation of the Application and Business Services
- **7.6** Implementation of the API Layer
- **7.7** Implementation of the Web Client
- **7.8** Implementation of the Mobile Client
- **7.9** Real-Time Features
- **7.10** Security Implementation
- **7.11** Document Generation — identity cards, certificates, credential PDFs
- **7.12** Constitution Publication Pipeline — the always-latest invariant, the extraction tool, and why the naive approach fails on a live database
- **7.13** Third-Party Libraries: selection criteria, licence review and justification
- **7.14** Software Configuration Management — version control strategy, branching model, change control and release identification
- **7.15** Notable Implementation Challenges and Their Resolution, presented as symptom, hypothesis, evidence and resolution
- **7.16** Summary

**Figures, tables and listings**

- Figure 7.1 — Module and folder structure of the solution
- Figure 7.2 — Dependency structure matrix of the assemblies, demonstrating that no dependency violates the rule stated in §6.3.6
- Figure 7.3 — Build and bundle pipeline for the web client
- Figure 7.4 — Git branching model diagram
- Figure 7.5 — Flowchart: constitution publication from ratified PDF to every surface
- Figure 7.6 — Flowchart: file upload, validation and storage
- Figure 7.7 — Algorithm flowchart: membership-due calculation
- Figure 7.8 — Control-flow graph of a representative complex method, annotated with its cyclomatic complexity, feeding the basis-path testing of Chapter 9
- Listings 7.1–7.n — representative implementation extracts, each discussed in the text
- Table 7.1 — Third-party dependencies: library, version, purpose, licence, alternative considered
- Table 7.2 — Module implementation status matrix
- Table 7.3 — Size metrics by layer: files, lines of code, classes

---

## Chapter 8 — Security, Privacy and Trust

- **8.1** Security Objectives and Assumptions
- **8.2** Threat Modelling (STRIDE) — assets, entry points, trust boundaries, enumerated threats
- **8.3** Authentication and Session Security
- **8.4** Authorisation Model and the Role–Permission Matrix
- **8.5** Input Validation and Output Sanitisation
- **8.6** File Upload Security
- **8.7** Transport, Header and Browser-Policy Security
- **8.8** Rate Limiting and Abuse Prevention
- **8.9** Payment-Related Risk and the No-Gateway-Keys Posture — the security rationale for manual verification and its accepted operational cost
- **8.10** Governance Integrity — the scope of the voting features as sentiment and internal decision-making rather than as a secure-election system, with reference to §2.7
- **8.11** Personal Data: Lawful Basis, Minimisation, Consent, Retention and Subject Rights
- **8.12** Audit Logging and Non-Repudiation
- **8.13** Conformance Assessment against OWASP ASVS
- **8.14** Residual Risks and Recommendations
- **8.15** Summary

**Figures and tables**

- Figure 8.1 — Threat model data-flow diagram with trust boundaries, STRIDE-annotated
- Figure 8.2 — Attack tree for the highest-value asset: member account takeover or fraudulent payment credit
- Figure 8.3 — Role–permission matrix diagram
- Figure 8.4 — Personal-data classification and flow diagram, with retention points
- Figure 8.5 — Sequence diagram: an unauthorised request rejected through the middleware chain
- Figure 8.6 — Defence-in-depth layer diagram
- Table 8.1 — STRIDE threat enumeration with mitigations and their implementation location
- Table 8.2 — Role × capability matrix
- Table 8.3 — OWASP ASVS conformance checklist with verdicts
- Table 8.4 — Personal-data inventory: element, purpose, lawful basis, retention
- Table 8.5 — Residual risk register

---

## Chapter 9 — Verification, Validation and Quality Assurance

- **9.1** Verification and Validation Strategy and Test Levels
- **9.2** Software Quality Assurance Plan — reviews, standards conformance, defect prevention, and the role of formal technical reviews in this project
- **9.3** Test-Case Design Techniques Applied
  - **9.3.1** Equivalence partitioning
  - **9.3.2** Boundary value analysis
  - **9.3.3** Decision-table testing for the dues and eligibility rules
  - **9.3.4** State-transition testing derived from the state machines of Chapter 5
  - **9.3.5** Basis-path testing using the control-flow graph and cyclomatic complexity of §7.8
  - **9.3.6** Use-case and scenario-based testing
  - **9.3.7** Exploratory testing and its recorded charters
- **9.4** Unit Testing — Backend
  - **9.4.1** Framework, runner and project layout
  - **9.4.2** The definition of a unit in this system, and the reasoning for testing at the service boundary rather than at the controller or the repository
  - **9.4.3** Test doubles — stubs, mocks, fakes and the in-memory provider; where each is appropriate and the fidelity each sacrifices
  - **9.4.4** Test structure and naming: arrange–act–assert, given–when–then
  - **9.4.5** Testing the business rules that carry constitutional force — dues, eligibility, committee terms, voting rights — traced back to the rule catalogue of §5.6 and forward to Table 9.9, which names the test that pins each DC identifier
  - **9.4.6** Testing of failure and exception paths
  - **9.4.7** Test independence, determinism and the elimination of order dependence
- **9.5** Unit and Component Testing — Web Client
  - **9.5.1** Runner, harness and component-testing strategy
  - **9.5.2** Testing signals, computed state and change propagation
  - **9.5.3** Testing guards, interceptors and the token-refresh queue
  - **9.5.4** HTTP mocking and contract fidelity against the live API
- **9.6** Widget and Golden Testing — Mobile Client
  - **9.6.1** Widget-test scope
  - **9.6.2** Golden (snapshot) testing: what it catches, what it cannot, and the platform-rendering problem that requires it to be skipped in continuous integration
- **9.7** Integration Testing Strategy, and the reasoning for rejecting big-bang integration
- **9.8** System and End-to-End Testing
- **9.9** Regression Testing and Test Selection
- **9.10** Security Testing, mapped to the threat model of Chapter 8 and to OWASP ASVS
- **9.11** Performance and Load Testing — workload model, environment, results
- **9.12** Usability and Accessibility Testing — task success, time on task, System Usability Scale scores, WCAG audit
- **9.13** User Acceptance Testing — participants, protocol, results, sign-off
- **9.14** Product Metrics and Static Analysis
  - **9.14.1** Size — lines of code and function points
  - **9.14.2** Complexity — cyclomatic complexity distribution and the worst offenders
  - **9.14.3** Coupling and cohesion — CBO, LCOM, afferent and efferent coupling, instability
  - **9.14.4** Maintainability index and technical-debt estimate
  - **9.14.5** Test adequacy: coverage, and the argument coverage can and cannot support
    - Statement, branch and, where available, mutation coverage, reported per layer rather than as a single headline figure
    - Coverage treated as a necessary but not sufficient criterion: high coverage demonstrates that code is executed, not that behaviour is verified. Mutation score is reported as the corrective measure, and where it was not run that is stated
    - Risk-weighted targets: coverage thresholds set by module criticality, with payment, authentication and governance held to a higher bar than presentation code, declared in §4.6 in advance of measurement
    - Analysis of the uncovered residue — which code is untested, whether by decision or by omission, and the risk each case carries
    - Coverage trend across increments rather than a single end-state snapshot
- **9.15** Defect Analysis — density, distribution, removal efficiency, root-cause categories
- **9.16** Threats to the Validity of the Evaluation, with forward reference to §12.11
- **9.17** Summary

**Figures and tables**

- Figure 9.1 — Test pyramid as realised, with counts
- Figure 9.2 — V-model mapping of each development artefact to its verifying activity
- Figure 9.3 — Continuous-integration quality-gate pipeline
- Figure 9.4 — Control-flow graph with basis paths enumerated for the worked example
- Figure 9.5 — Coverage by layer
- Figure 9.6 — Cyclomatic complexity distribution
- Figure 9.7 — Coupling and instability scatter: the main sequence with the modules plotted
- Figure 9.8 — Defect distribution by severity, module and injection phase
- Figure 9.9 — Defect-removal efficiency by phase
- Figure 9.10 — Performance results: latency distribution by endpoint, throughput under load
- Figure 9.11 — System Usability Scale score distribution with the 68-point benchmark line
- Table 9.1 — Equivalence classes and boundary values for a representative input domain
- Table 9.2 — Decision table for dues and eligibility
- Table 9.3 — State-transition test table
- Table 9.4 — Test-case catalogue: ID, technique, requirement, input, expected, actual, status; full set in the test-runner output committed per release
- Table 9.5 — Product metrics summary against the thresholds declared in §4.6
- Table 9.6 — Coverage by module against its risk-weighted target: module, criticality, target, statement percentage, branch percentage, mutation score, verdict, and the justification for any shortfall
- Table 9.7 — Defect log
- Table 9.8 — User acceptance test results and sign-off
- Table 9.9 — Constitutional traceability: DC identifier, constitutional article and section, the rule as enforced, the requirement it governs, the automated test that pins it, and the verdict. Closes the loop Table 3.4 opens: §3.10 states the constraint, this states the test that would fail if the software stopped honouring it

---

## Chapter 10 — Deployment and Operations

- **10.1** Deployment Architecture
- **10.2** Environment Topology and Configuration Differences
- **10.3** Containerisation Strategy
- **10.4** Continuous Integration and Continuous Deployment
  - **10.4.1** Pipeline stages and the quality gates actually enforced — formatting, type checking, static analysis, build and test, per workflow
  - **10.4.2** Security in the pipeline — what is automated and what is not. Dependabot raises grouped dependency updates against preprod for NuGet, npm, pub, Actions and Docker, so a published advisory against a library this project uses arrives as a pull request rather than waiting to be noticed. Nothing else is automated, and the reasons are separate rather than one excuse: static application security testing was scoped and rejected on cost, CodeQL being free only on a public repository and needing paid GitHub Advanced Security on a private one; dynamic testing has no environment to run against that is not either preprod or production; and the Flutter client would be out of reach of CodeQL in any case, Dart not being one of its languages. The security work of Chapter 8 was therefore done by review and by test, which is a weaker guarantee than a scan and is reported as one, with the remedy costed in §13.4
- **10.5** Database Provisioning, Migration and Live Data Synchronisation
- **10.6** Configuration and Secret Management
- **10.7** Observability — logging, monitoring, alerting, error reporting
- **10.8** Backup, Recovery and Business Continuity
- **10.9** Release and Rollback Procedure
- **10.10** Operational Cost Model and Sustainability under Institutional Budget Constraints
- **10.11** Maintenance Plan and Handover
- **10.12** Summary

**Figures and tables**

- Figure 10.1 — UML deployment diagram: nodes, artifacts, communication paths, protocols
- Figure 10.2 — Network and infrastructure diagram with trust boundaries
- Figure 10.3 — CI/CD pipeline diagram: commit, build, test, package, deploy
- Figure 10.4 — Environment promotion diagram
- Figure 10.5 — Container composition diagram
- Figure 10.6 — Sequence diagram: application boot and runtime data synchronisation
- Figure 10.7 — Backup and recovery flow with recovery point and recovery time objectives marked
- Table 10.1 — Environment variable and configuration key catalogue
- Table 10.2 — Operational cost model: component, tier, monthly cost, scaling trigger
- Table 10.3 — Runbook: symptom, diagnosis, action

---

## Chapter 11 — Project Management

This chapter follows Pressman and Maxim's project-management apparatus, which is the framework the
course material uses and the one an examiner will read it against: the four P's as the framing, then
metrics, estimation, scheduling and risk. Only the parts this project has evidence for are written.
Where a technique was not used — and a single unpaid maintainer did not run earned-value analysis week
by week — the chapter says so and reports what was done instead, rather than reconstructing a plan
after the fact. The RMMM plan is already in §4.8; §11.9 reports its execution rather than restating it.

**Every figure in this chapter is regenerated, not typed.** `python docs/book/build/wbs.py` derives the
durations, the task counts and the arrival profile from git history and `docs/TODO.md`. Re-run it
before submission; where a number in the text disagrees with the script, the script is right.

**Five work streams, and each activity states which class of evidence its duration rests on.** That
matters more than the numbers themselves:

| Class | Covers | What it can support |
|---|---|---|
| Git-dated | 17 code components, 8 documentation deliverables | dated and verifiable; a commit-day is a *lower bound* on effort, since reading, debugging and thinking leave no commit |
| Tracker-dated | 82 work packages, 754 tasks, the arrival profile | dated in `docs/TODO.md`, and from Work Package 68 onwards each area states its own component and dates in a `wbs:` marker |
| Back-scheduled | P1 to P5, the research, specification and design that preceded the first commit | placed in the order the work had to happen, ending as the first commit lands on 9 February 2026. A reconstruction, and labelled as one wherever it appears |
| Not yet written | installation, user and administrator manuals | future effort under the page budget, not completed work |
| Calculated assumption | elicitation, interviews, review sessions, stakeholder discussion, incident response | **no commits exist**, so the duration is calculated from a stated rate and a measured quantity, and the arithmetic is printed with it. A reader who rejects the rate can redo the sum. These are labelled as assumptions everywhere they appear and are never presented as measurements |

The last two classes are the ones easiest to fabricate, so the arithmetic is shown rather than the
conclusion:

| ID | Activity | Basis | Hours | Days | Additive |
|---|---|---|---|---|---|
| U1 | Elicitation interviews | 10 participants × 35 min contact, 3 role guides at 1h, write-up at 1× contact | 14.6 | 2 | yes |
| U2 | Governing-document analysis | 43,000 words (constitution 5,239 + eight election documents ≈38,000) at 1,500 words/h for clause-by-clause classification, plus 16 domain-constraint entries at 15 min | 32.7 | 5 | yes |
| U3 | Formal technical review | 2 sessions × (2h preparation + 2h session + 1h logging); output was the 5 defects of Table 3.8 | 10.0 | 2 | yes |
| U4 | Stakeholder discussion | 18 feedback areas × 30 min; triage into tracker items excluded, already counted under D4 | 9.0 | 2 | yes |
| U5 | Incident response | 4 incidents × 2h diagnosis before the first fix commit | 8.0 | 1 | **no** |

U5 is deliberately not added. Those four dates carry 9, 7, 10 and 2 commits, among the busiest in the
project, so the fix work already sits inside the measured commit-days; only the diagnosis before the
first commit is invisible, and double-counting it would inflate the total.

That gives the project's total effort: **64 measured code commit-days, 24 measured document
commit-days, 30 stated pre-development days and 4 further assumed days, being 122 days, about 5.5
person-months at 22 days to the month, spread over the 207 calendar days from 9 February to 3
September 2026.**
Every rate above is a judgement and is stated as one. The reading rate of 1,500 words an hour for
normative text and the write-up ratio of 1× contact time are the two most open to challenge, and §11.4
says so.

Git also *misdates* the research stream: the requirements documents were committed from July 2026,
five months after the elicitation they record and after the code they governed was already written.
For code that gap is small; for research it inverts the schedule. The fix is a fifth stream, P1 to
P5, holding the work that came before the first commit — the governing-document study, the
elicitation, the requirements analysis and specification, the architecture and technology choice,
and the test strategy with its first test cases. Thirty working days, back-scheduled from 9
February 2026 in the order the work had to happen, and reported as a reconstruction rather than a
diary, because no diary was kept.

A third figure joins them, and it is the one an examiner will ask for. Neither commit-days nor
calendar span says how much system was delivered, so §11.4 sizes the delivered code directly:
105,618 hand-written lines across four kinds of source, each at its own production rate, since
markup and test code are not produced at the speed of business logic and generated code is excluded
entirely. That comes to 5,626 hours, **703 working days built conventionally**. Four multiplicative
reductions then apply, each carrying a one-line reason a reader can accept or reject:

| Reduction | Factor | Why |
|---|---|---|
| Framework scaffolding and code generation | ×0.80 | generated, not written |
| Reuse of shared components within the project | ×0.85 | written once, used on dozens of screens |
| Reuse from the author's own earlier projects | ×0.83 | measured footprint × the share that came over intact |
| AI-assisted and rapid development tooling | ×0.70 | drafts generated, then reviewed and corrected |

They multiply rather than add because they compound, and the product is 0.40, giving a
**reuse-adjusted effort of 280 working days** for the code delivered so far, against 122 the
record evidences. The work still outstanding is counted the same way — 146 unwritten chapter
sections, 77 figures and tables not yet made, and 179 open tracker items priced by priority,
being 73 working days — so the **completed project comes to about 353 working days**. The item
rates cover a defect fix and the test that pins it together, because §3.7's definition of done
requires both.

No single factor dominates, and that is deliberate. The two largest are the tooling reduction and reuse from the
author's own 2024 projects. They are different in kind and the chapter keeps them apart: the reuse
figure is measured from a footprint, the tooling figure is a judgement, because assistance was diffuse
rather than confined to modules. The reuse half is also the more consequential, being what §12.11
names as the limit on what this dissertation's cost conclusion transfers to. The chapter reports the evidenced figure beside
the adjusted one and states which part of the gap it cannot separate, since unrecorded reading,
debugging and design leave no commit and the reuse is already inside the factors.

One further correction, and it matters more than it looks. Effort and duration are not the same
quantity and the chapter no longer prints one as though it were the other. Apportioned commit-days
are **effort**: how much work a component took. First commit to last is **duration**: how long it
stayed open, which for most components is five to seven months because a part-time maintainer
returns to them. The precedence network runs on effort; the spans go on the Gantt chart. Building
the network on spans instead was tried and abandoned — the components overlap almost completely, so
a serial pass sums to 1,008 working days across a project that ran for 207 calendar days, which is
an arithmetic artefact and not a schedule.

- **11.0** The Four P's Applied — People, Product, Process, Project, with the single-maintainer case stated plainly against each. **People** carries a figure the rest of the chapter depends on: twenty years of professional development experience, and a personal library of 2024 projects close enough in technology generation to be reused as code rather than as ideas, from which about a quarter of this codebase descends. Pressman's staffing models price a team by headcount; this project's capacity is one person whose productivity rests on two decades of accumulated reusable work, which §11.5 quantifies and §12.11 treats as the limit on what transfers. None of Pressman's four organisational paradigms (closed, random, open, synchronous) describes one unpaid maintainer; saying so is the finding, not forcing a label
- **11.1** Process Model in Practice and its Deviations from Plan — led by the arrival profile: **70% of tracker tasks were not planned** (529 of 754, figures of 4 September 2026), arriving as stakeholder feedback (39%), review findings (18%) or defects (13%), and 26 of the dated work packages landed in August 2026 alone. A critical path over an up-front work breakdown would be fiction, because two thirds of the work did not exist when that breakdown would have been drawn
- **11.2** Work Breakdown Structure — the five streams above, the fifth being the pre-development research and design of P1 to P5; the 17 code components each tied to the tracker areas that produced them, so the activity list and the tracker are one list read two ways
- **11.3** Scheduling, Task Network and Critical Path — activity-on-node by the precedence diagram method, with duration, float, early and late start and finish. Reported as a **retrospective** network: critical path 48 working days of effort against 64 days worked, 122 days of total effort and 207 elapsed. The gap is availability, not dependency, and that is the section's point. Persistence and security behave as **hammock activities**, touched on 42 and 33 separate days across the whole span, and are drawn as such rather than as boxes at day zero
  - **11.3.1** CPM summary ordered by float, so the schedule can be read by slack rather than by sequence
  - **11.3.2** Crashing analysis, and the honest result: the single resource on the critical path cannot be crashed, so every classical crashing lever is unavailable. What shortened the schedule instead was scope deferral, recorded in the Won't set of §3.8
- **11.4** Effort Estimation — the function-point chain, computed from the delivered system: EI, EO and EQ from the 276 endpoint attributes, ILF from the 49 `DbSet` properties, EIF from the four payment gateways plus email, SMS and social identity. UFP, then TDI over the fourteen general system characteristics, **VAF = 0.65 + 0.01 × TDI**, AFP, effort at a stated productivity factor, LOC via the language factor, and cost in BDT. **COCOMO II is dropped**: its five scale factors and seventeen effort multipliers cannot be justified here, and a model nobody can defend adds no evidence
  - **11.4.1** Two estimates compared — apportioned commit-days against completed-task counts, on a common base. Eight of the seventeen components disagree by more than twofold: authentication, governance, gallery and security look heavy by task count because the tracker holds many small items there; persistence, events and the job board look heavy by commit-day because a handful of items each took days. **Task granularity varies by an order of magnitude between components, so any estimate built on task counts inherits that noise** — a stronger result than either estimate alone
  - **11.4.2** The estimate against the actual — three figures, not two: 703 nominal working days for the delivered code built conventionally, 344 after the reuse and tooling factors, and 122 days evidenced by the record across all five streams, against the function-point model's prediction at the standard productivity factor. The model over-predicts by roughly two orders of magnitude, and the reasons are stated: framework scaffolding, no coordination overhead, no separate quality-assurance or project-management roles, and generated code counted as delivered function. The productivity factor assumes a team; there is no team
- **11.5** How the Implementation Time Was Optimised — the section that answers the question a reader forms as soon as the scale is stated: one unpaid maintainer, seven months, and a system with 276 endpoints, 49 entities, two clients and 898 automated tests. The answer is not that the work was small. It is that four things compounded — what the framework generated, what the project reused, what earlier projects supplied, and what tooling drafted — and this section prices each instead of asserting it
  - **11.5.1** Sizing the delivered code — 105,618 hand-written lines counted from the tree, generated code excluded, at a production rate per kind of source rather than one blended rate: 12 lines an hour for backend logic, 25 for templates and stylesheets, 18 for Dart, 20 for test code. Markup is not produced at the speed of business logic and pretending otherwise is where estimates of this shape usually go wrong. 5,626 hours, **703 working days built conventionally**
  - **11.5.2** The four reductions, each with its evidence — framework scaffolding and code generation (×0.80: EF Core migrations, Angular CLI scaffolds, Flutter project structure, and 1.7 million generated migration lines nobody wrote); reuse of shared components within the project (×0.85: the shared control set, the token layer, base services, the common admin table and form patterns); reuse from the author's own earlier projects (×0.83, and this one is derived rather than judged: authentication with its OTP, token and middleware pipeline, the front-end grid with the shared controls, core services and layouts behind it, the design-token stylesheet, file handling and validation, and the payment-gateway adapters come to **22,941 lines, 21.7% of the codebase**, of which the author puts **75 to 80 per cent** as carried over intact from his 2024 projects; 21.7% × 77.5% is 16.8% of total effort saved, hence ×0.83); AI-assisted and rapid development tooling (×0.70: generated first drafts, refactors and test scaffolds, reviewed and corrected rather than accepted. One block is nameable — the 2,453 lines of service interfaces and DTOs in `GHCAA.Application` were generated rather than carried over, which is why they are not in the reuse table — but the factor stays a judgement, because assistance was diffuse across the codebase rather than confined to modules a footprint could measure). They multiply rather than add, because a screen built from an existing control, scaffolded by the framework and finished with an assistant is cheaper than any one of those alone makes it
  - **11.5.3** The result and its sensitivity — product 0.40, so **280 working days for what is delivered**, and with the 73 days still outstanding, **about 353 for the completed project**. Two of the four factors carry most of the reduction and they are different in kind: prior reuse is measured, tooling is judged, and the chapter says which is which instead of blending them. A sensitivity line, not a hidden assumption
  - **11.5.4** What this cost, which is the half a reuse argument usually omits — reuse buys speed and spends independence. The generated migration corpus is 81 MB of C# that made the Render build run out of memory (§10.9); the carried-over gateway adapters brought a payment model the Association cannot fully use (§8.9); assistant-drafted code needs the review time that the remaining 65 per cent pays for, and §9.2 reports what review actually caught. The section reports the trade, not only the saving
- **11.6** Progress Tracking and Earned Value — BCWS, BCWP, BAC and ACWP, with **SPI = BCWP/BCWS** and **CPI = BCWP/ACWP**, reconstructed from the dated tracker items and the commit record, which is the only effort evidence this project has. The reconstruction and its limits are stated as such; no weekly earned-value record was kept, and the chapter does not pretend one was
- **11.7** Team Structure and Responsibilities — one maintainer holding every role the four P's assign to different people, and what that costs: no independent review, and no separation between the person who declares a payment rule and the person who tests it. The mitigation was mechanical, being the tracker and the test suite, and §12.11 treats it as a validity threat
- **11.8** Configuration and Change Management in Practice — there was no change-control board. There was one file: `docs/TODO.md`, 5,451 lines, simultaneously the project plan, the change log, the defect log and the decision record. The 529 reactive tasks *are* the change log, and Table 11.5 is generated from them rather than reconstructed
- **11.9** Risk Monitoring Record — the RMMM plan of §4.8 as it was executed, with **risk exposure RE = P × C** computed per risk and impact on the 1–5 scale, plus one worked Risk Information Sheet. §4.8 needs revising to carry RE; the impact costs are author-stated and have to be supplied
- **11.10** Quality Assurance Activities Performed — against Pressman's 40‑20‑40 allocation, this project spent roughly a third of its evidenced effort on feature code and about 15% on testing, well under the 40% prescribed. Data-backed self-criticism, with **DRE = E / (E + D)** computed from the findings log as E and live defects as D, and **MTTC** named as what QAS-06 already measures
- **11.11** Lessons in Project Management
- **11.12** Summary

**Figures and tables**

One activity diagram per component, not a single module-level network. Seventeen small figures each
print legibly at the 7pt floor and sit beside the prose for their own component; one network of
seventeen nodes with nine edges converging on the web client would print at about 4pt and be useless.

- Figures 11.1–11.17 — Activity diagram per component, C1 to C17, each with its entry and exit conditions, and its tracker areas named in the caption
- Figure 11.18 — Chapter-level dependency graph, critical-path components only: C1 → C2 → C3 → C5 → C8 → C13 → C15 → C17
- Figure 11.19 — Work breakdown structure across the five streams
- Figure 11.20 — Arrival profile over time: planned, feedback, defect and review work by month
- Figure 11.21 — Float distribution across the seventeen components
- Figure 11.22 — Two estimates compared per component: commit-days against task counts
- Figure 11.23 — Effort distribution by stream and component, against the 40‑20‑40 allocation
- Figure 11.24 — Where the implementation time went: nominal 703 days reduced by each leverage in turn to 280, with the 69 days outstanding shown separately
- Table 11.1 — Activity list: ID, activity, duration, ES, EF, LS, LF, float, predecessors, tracker areas, evidence class
- Table 11.2 — Function-point count: EI, EO, EQ, ILF and EIF with complexity weighting, from the delivered system
- Table 11.3 — TDI over the fourteen general system characteristics, and the VAF it yields
- Table 11.4 — Cost model: effort, rate and total, by both the function-point and the lines-of-code route
- Table 11.5 — Change log generated from the reactive tracker tasks, 529 of them at the last count: area, arrival class, date, cause, impact
- Table 11.6 — Documentation deliverables: document, artefact, lines, active days, status

---

# PART IV — EVALUATION AND CLOSURE

## Chapter 12 — Results, Evaluation and Discussion

This chapter executes the evaluation plan declared in §4.5; nothing is measured here that was not
defined there.

- **12.1** Overview of the Delivered Artefact
- **12.2** Functional Evaluation — requirement coverage and the closed traceability matrix
- **12.3** Quality Evaluation against ISO/IEC 25010, characteristic by characteristic, with the evidence and the metric value for each
- **12.4** Performance Evaluation Results
- **12.5** Security Evaluation Results
- **12.6** Usability and Accessibility Evaluation Results
- **12.7** Stakeholder and Expert Evaluation
- **12.8** Answering the Research Questions — RQ1 through RQ4 answered explicitly, each with the evidence supporting the answer and the confidence that evidence warrants; RQ3's answer is the five-class scheme of §3.10, the domain-constraint classification, applied to the delivered system rather than discussed
- **12.9** Discussion — interpretation, and comparison against the literature of Chapter 2: where this work agrees with prior findings and where it diverges
- **12.10** Comparison against the Existing Manual System
- **12.11** Threats to Validity — construct, internal, external and conclusion validity, each with the mitigation applied and the residual limitation acknowledged. The heaviest threat to external validity is named here rather than left implicit: the platform was affordable because the maintainer brought a personal library of his own 2024 projects on the same technology generation, and an association without such a person faces the licence cost Chapter 2 quotes, not the build cost §11.5 computes. The conclusion transfers to the class of institution that has one; it does not transfer to every institution
- **12.12** Limitations of the Artefact
- **12.13** Reflection on the Design Science Contribution — what transfers beyond this institution
- **12.14** Summary

**Figures and tables**

- Figures 12.1–12.20 — Annotated screenshots of the working system, grouped by module, each captioned with the requirement it evidences
- Figure 12.21 — Requirement coverage chart: delivered, partial, deferred, by module
- Figure 12.22 — ISO 25010 evaluation radar chart: target versus achieved
- Figure 12.23 — Performance benchmark results
- Figure 12.24 — Before-and-after process-efficiency comparison against manual practice
- Figure 12.25 — Research question to evidence map, on a single page
- Table 12.1 — Closed requirements traceability matrix: requirement → design → code → test → evaluation result
- Table 12.2 — ISO 25010 evaluation: characteristic, metric, target, achieved, verdict
- Table 12.3 — Objective achievement against Chapter 1
- Table 12.4 — Threats to validity with mitigations

---

## Chapter 13 — Conclusion and Future Work

- **13.1** Summary of the Work
- **13.2** Contributions Restated and Substantiated
- **13.3** Answers to the Research Questions, in Brief
- **13.4** Practical Implications for Similar Institutions
- **13.5** Lessons Learned — technical, methodological and organisational
- **13.6** Future Work
  - **13.6.1** Near-term functional roadmap
  - **13.6.2** Technical debt and reengineering priorities
  - **13.6.3** Research directions opened by this work
- **13.7** Concluding Remarks

**Figures**

- Figure 13.1 — Product and research roadmap
- Figure 13.2 — Technical debt map: modules plotted by debt against change frequency

---

## Back Matter

- References — IEEE numeric style, every entry cited in the body

**Printed in the bound volume** (11–19 pages, per the appendix policy above):

- Appendix A — Ethics: the permission obtained, and the absence of a written consent procedure, per §4.9. There is no participant information sheet or consent form to reproduce; consent was verbal. 2–4 pp
- Appendix B — Closed requirements traceability matrix: requirement → design → code → test → result, for all of FR-01 to FR-54 and the NFR set. This is the appendix an examiner actually uses. Compiled by hand until the tests carry identifiers (tracker 73.4), after which it is generated. 4–6 pp
- Appendix C — Full-page fold-out plates: the four ER sub-models and the design class diagram, at landscape size. 3–5 pp
- Appendix D — Originality / similarity report: tool output. 2–4 pp

**Delivered beside the dissertation, not bound into it.** Each is cited in the body precisely enough
to be checked, and each is a generated artefact or a standalone document rather than typeset prose:

| Material | Where it lives | Why it is not printed |
|---|---|---|
| Complete use-case descriptions | repository, `docs/SRS.md` and Appendix B's matrix | ~44 remaining cases at two per page is 20–26 pages of near-identical structure |
| Complete requirements specification | repository, `docs/SRS.md`; §3.3 and §3.4 carry the catalogue in full already | printing it twice adds pages, not evidence |
| Data dictionary and schema DDL | generated from `ApplicationDbContext` and the migrations | 49 tables is 35–50 pages; Table 6.2 prints the representative slice |
| Complete API reference | generated OpenAPI document | 276 endpoints is 46–60 pages; Table 6.3 prints the catalogue by controller |
| Complete test-case suite and results | test-runner output, committed per release | 898 tests is 36–45 pages; §9.14 reports the figures that matter |
| Source-code listings | the repository itself, at a named commit | the book quotes the extracts it discusses, in place, per the thirty-line rule |
| Literature review protocol, search strings, screening log | separate document | 4–6 pages read by nobody once §2.2 states the protocol |
| Evaluation instruments and raw evaluation data | separate document | needed for scrutiny, not for reading; cite and supply on request |
| Installation, user and administrator manuals | separate documents, versioned with the product | operational documents with their own lifecycle; a two-paragraph summary each sits in §10.11 |
| Architecture decision records in full | repository, `docs/` | Table 6.1 indexes them; the full text is a living record, not a snapshot |

- Index — only if the programme requires one. It costs 4–6 pages and a full-text PDF is searchable.

---

## Diagram Inventory

The complete set of diagram and chart types the book contains, and the chapter each appears in.

| # | Diagram type | Chapter | Depicts |
| --- | --- | --- | --- |
| 1 | Context diagram (DFD 0) | 1, 5 | System boundary and external entities |
| 2 | DFD Level 1 | 5 | Major processes and data stores |
| 3 | DFD Level 2 | 5 | Payments, approval, authentication, constitution, elections |
| 4 | Stakeholder / onion diagram | 1 | Stakeholder rings |
| 5 | RQ ↔ objective ↔ chapter map | 1 | Navigation key |
| 6 | Study-selection flow diagram | 2 | Literature screening with counts |
| 7 | Concept map / taxonomy | 2 | Structure of the reviewed field |
| 8 | As-is process model (BPMN) | 2 | Current manual practice |
| 9 | Positioning chart | 2 | Existing systems on two decisive axes |
| 10 | Use-case diagram | 3 | System level and per subsystem |
| 11 | Actor generalisation hierarchy | 3 | Role inheritance |
| 12 | Domain / conceptual class model | 3 | Analysis-level classes, no implementation |
| 13 | Quality-attribute utility tree | 3 | Attribute, refinement, measurable scenario |
| 14 | Goal model | 3 | Stakeholder goals to requirements |
| 15 | DSR framework and process model | 4 | Research method instantiated |
| 16 | Research design overview | 4 | Phases, inputs, outputs, evaluation points |
| 17 | Process model diagram | 4 | Adopted lifecycle |
| 18 | Risk exposure matrix | 4 | Probability × impact |
| 19 | CRC card set | 5 | Class responsibilities and collaborators |
| 20 | Activity diagram | 5 | Registration, payment, events, amendment |
| 21 | Swimlane activity diagram | 5 | Cross-role responsibility |
| 22 | BPMN process diagram | 5 | Election cycle, for a non-engineer audience |
| 23 | State-machine diagram | 5 | Member, payment, constitution, event lifecycles |
| 24 | Sequence diagram | 5, 8, 10 | Authentication, registration, payment, SignalR, denial, boot |
| 25 | Communication diagram | 5 | Payment collaboration, structural view |
| 26 | Interaction overview diagram | 5 | Composition of interactions across a year |
| 27 | Timing diagram | 5 | Token lifetime, refresh window, session expiry |
| 28 | High-level architecture | 6 | Clients, API, stores, external services |
| 29 | Layered / clean architecture | 6 | Dependency rule and inversion boundary |
| 30 | Entity–relationship diagram | 6 | Four readable sub-models; no full-schema plate, which cannot print legibly |
| 31 | Design class diagram | 6 | Domain, services, client |
| 32 | Package diagram | 6 | Assemblies and permitted dependencies |
| 33 | Component diagram | 6 | Provided and required interfaces |
| 34 | Composite structure diagram | 6 | Parts, ports, connectors |
| 35 | Object diagram | 6 | Populated runtime snapshot |
| 36 | Profile diagram | omitted | No custom stereotypes are defined |
| 37 | Middleware pipeline | 6 | Ordered request path and ordering constraints |
| 38 | Navigation / route map | 6 | Route trees with guards |
| 39 | Site map | 6 | Public information architecture |
| 40 | Wireframes | appendix, if retained | Layout intent before styling; not in Chapter 6 as written |
| 41 | UI mockups | 12 | Delivered screens, as annotated screenshots |
| 42 | Screen-flow diagram | 6 | Mobile navigation |
| 43 | Design-token derivation | 6 | Theme resolution |
| 44 | Architecture trade-off radar | 6 | Candidate architectures scored |
| 45 | Module / folder structure | 7 | Solution layout |
| 46 | Dependency structure matrix | 7 | Evidence the dependency rule holds |
| 47 | Build / bundle pipeline | 7 | Client build stages |
| 48 | Git branching model | 7 | Configuration management strategy |
| 49 | Flowchart | 7 | Publication, upload, due calculation |
| 50 | Control-flow graph | 7, 9 | Complexity and basis-path derivation |
| 51 | Test pyramid | 9 | Realised test-level distribution |
| 52 | V-model mapping | 9 | Artefact to verifying activity |
| 53 | CI quality-gate diagram | 9, 10 | Pipeline stages |
| 54 | Metric charts | 9 | Coverage, complexity, coupling, defects |
| 55 | Coupling / instability scatter | 9 | Modules against the main sequence |
| 56 | Defect-removal efficiency chart | 9 | Phase containment |
| 57 | Performance / latency charts | 9, 12 | Endpoint latency and throughput |
| 58 | SUS score distribution | 9, 12 | Usability benchmark |
| 59 | Threat model DFD (STRIDE) | 8 | Attack surface and trust boundaries |
| 60 | Attack tree | 8 | Highest-value asset compromise paths |
| 61 | Role–permission matrix | 8 | Authorisation model |
| 62 | Data-classification flow | 8 | Personal-data movement and retention |
| 63 | Defence-in-depth layers | 8 | Control stacking |
| 64 | Deployment diagram | 10 | Nodes, artifacts, protocols |
| 65 | Network / infrastructure diagram | 10 | Hosts and boundaries |
| 66 | Environment promotion | 10 | Local, pre-production, production |
| 67 | Container composition | 10 | Images and relationships |
| 68 | Backup / recovery flow | 10 | Recovery point and recovery time objectives |
| 69 | Work breakdown structure | 11 | Task decomposition |
| 70 | Gantt chart | 11 | Planned versus actual schedule |
| 71 | PERT / activity network | 11 | Dependencies and critical path |
| 72 | Milestone timeline | 11 | Delivery dates |
| 73 | Earned value chart | 11 | Planned value, earned value, actual cost |
| 74 | Burndown / velocity | 11 | Increment progress |
| 75 | Effort distribution charts | 11 | Phase and module effort |
| 76 | Annotated screenshots | 12 | Delivered-system evidence |
| 77 | Coverage / achievement charts | 12 | Requirement and objective closure |
| 78 | ISO 25010 radar chart | 12 | Target versus achieved quality |
| 79 | Before-and-after comparison | 12 | Efficiency against manual practice |
| 80 | RQ to evidence map | 12 | One-page synthesis |
| 81 | Roadmap diagram | 13 | Future work |
| 82 | Technical debt map | 13 | Debt against change frequency |

---

## Standards and Sources the Book Is Written Against

| Area | Standard / source |
| --- | --- |
| Requirements specification | ISO/IEC/IEEE 29148 (supersedes IEEE 830) |
| Architecture description | ISO/IEC/IEEE 42010 |
| Design description | IEEE 1016 |
| Product quality model | ISO/IEC 25010 (primary); FURPS+ as practitioner cross-reference |
| Quality in use and usability | ISO 9241-11; ISO/IEC 25022 |
| Test documentation | ISO/IEC/IEEE 29119 (supersedes IEEE 829) |
| Software quality assurance planning | IEEE 730 |
| Configuration management | IEEE 828 |
| Accessibility | WCAG 2.1 AA |
| Application security verification | OWASP ASVS; OWASP Top 10 |
| Threat modelling | Shostack, *Threat Modeling*; Microsoft STRIDE |
| Research method | Hevner et al. (2004); Peffers et al. (2007); Runeson & Höst (2009) |
| Empirical method and validity | Wohlin et al., *Experimentation in Software Engineering* |
| Literature review protocol | Kitchenham & Charters, systematic review guidelines; PRISMA |
| Architecture evaluation | Bass, Clements & Kazman, *Software Architecture in Practice* (ATAM, utility trees) |
| Design patterns | Gamma et al.; Fowler, *Patterns of Enterprise Application Architecture*; Evans, *Domain-Driven Design* |
| Clean and hexagonal architecture | Martin, *Clean Architecture*; Cockburn, hexagonal architecture |
| Software engineering process, metrics, estimation, scheduling, risk and testing technique | Pressman & Maxim, *Software Engineering: A Practitioner's Approach*, 8th ed., reference [54] of the book. Chapters 24 to 28 of the 7th-edition slide set (project management concepts, process and project metrics, estimation, project scheduling, risk analysis) and a precedence-diagram-method exercise are held locally under `docs/materials/`, which is git-ignored: they are third-party copyrighted teaching material, for reading rather than redistribution, and nothing is quoted from them in the book |
| Estimation models | Boehm, COCOMO II; IFPUG function-point counting practices |
| Project management | PMBOK Guide |

---

## Source Material Available in the Project Repository

Most diagrams are derived from artefacts that already exist in the repository. Research framing,
evaluation data and literature are the material that must be produced.

| Chapter / diagram group | Derived from |
| --- | --- |
| Architecture, layering, middleware, data flows | `docs/ARCHITECTURE.md`, `docs/PROJECT_MAP.md` |
| Class diagrams, ER model, data dictionary, API catalogue | `docs/PROJECT_MAP.md`, `GHCAA.Infrastructure/Data/ApplicationDbContext.cs`, the API controllers |
| Use cases, functional requirements, traceability | `docs/SRS.md`, `docs/FEATURES.md` |
| Business rules, state machines, payment flow | `docs/BUSINESS_FINDINGS.md`, `docs/PAYMENT_GATEWAY_WORKFLOW.md` |
| Configuration-driven design and feature flags | `docs/CONFIG_DRIVEN_FRAMEWORK.md` |
| Deployment, CI/CD, environments, cost model | `docs/RENDER_DEPLOYMENT.md`, `Dockerfile`, the workflow files |
| Test strategy, coverage, defect analysis | `docs/BUSINESS_TEST_CHECKLIST.md`, `docs/COVERAGE_SNAPSHOT_2026-05-26.md`, the test projects |
| User-interface design, tokens, shared controls | `docs/UI_FIX_PLAN.md`, `docs/SHARED_PROFILE_COMPONENTS.md`, `GHCAA.Web/src/styles.scss` |
| Governance, constitution and election processes | `docs/CONSTITUTION_PUBLISHING.md`, `docs/Elections/`, the ratified constitution |
| Schedule, work breakdown, change log, decision records | `docs/TODO.md`, `docs/FORUM_PLAN_2026-05.md`, the version-control history |
| Product metrics — complexity, coupling, size | Static analysis over the solution, generated for §9.14 |
| Literature, evaluation data, validity analysis | Produced during the research; not present in the repository |
