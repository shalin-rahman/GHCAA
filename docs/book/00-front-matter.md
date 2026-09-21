# GHCAA Alumni Association Platform

## Design, Construction and Evaluation of a Governance-Aware Alumni Management Platform for a Resource-Constrained Institution

---

*Front matter is numbered in lower-case Roman numerals. The body restarts at Arabic 1.*

---

## i. Title Page

**Title.** Design, Construction and Evaluation of a Governance-Aware Alumni Management Platform for a Resource-Constrained Institution: The GHCAA Case

**Subject of the study.** The Govt. Haraganga College Alumni Association (HARAGANGIAN), Bangladesh

**Artefact under study.** A three-client platform comprising an ASP.NET Core 9 REST API, an Angular 21 web application and a Flutter mobile application, deployed on a free-tier managed host.

**Author.** Md Habibur Rahman, Roll 220, 7th Batch, Executive Master's in Information Technology (EMIT)

**Supervisor.** Dr. Kazi Muheymin-Us-Sakib, Professor, Institute of Information Technology, University of Dhaka

**Department and institution.** Executive Master's in Information Technology (EMIT) programme, Batch 7, Institute of Information Technology (IIT), University of Dhaka, Dhaka, Bangladesh

**Submitted.** {{build-month-year}}

---

## ii. Declaration of Originality

I declare that this dissertation and the software artefact it describes are my own work. Where the
work of others has been used it is cited in the text and listed in the References. The platform
described in Part III was written by me as sole maintainer; third-party libraries are catalogued
with their licences in Chapter 7, and no part of the codebase has been submitted for any other
award.

The governing documents of the Association reproduced or summarised in this dissertation, that is
the Constitution v4.2 and the seven election documents, are the property of the Association and are
used with its permission. They are the source of the domain constraints in §3.10, not my invention.

Signature: ____________________   Date: ____________

---

## iii. Certificate of Approval

*To be completed by the supervisor and the examiner.*

This is to certify that the work presented in this dissertation was carried out by the candidate
named above under my supervision, and that it is of a standard suitable for submission.

Supervisor: ____________________   Date: ____________

Examiner: ______________________   Date: ____________

---

## iv. Acknowledgements

I thank Dr. Kazi Muheymin-Us-Sakib, Professor at the Institute of Information Technology, University
of Dhaka, who supervised this work.

Three officers of the Association gave time this project could not have done without. The President
made the case for it to the Executive Committee and settled the questions only the chair could
settle. The Member Secretary sat through the sessions that turned the membership rules into
requirements, and corrected me on how the roll is actually kept rather than how the constitution
describes it. The Law Secretary read the constitutional articles against the rules I had encoded from
them, and found the two places where my reading was wrong before any of it reached a member.

I thank the college administration for permission to use the institutional name and crest under
Article I Section 5, and the members who tested registration and payment on their own handsets, over
their own mobile data, and reported what did not work.

Officers and members are identified here by office rather than by name, for the reason given in
§4.9.

---

## v. Abstract

Alumni associations at public colleges in Bangladesh run on paper. Rolls live in spreadsheets or
ledgers, subscriptions are collected in cash at reunions, and the constitution exists as a printed
document whose current version is known reliably only to the officers holding it. The Govt. Haraganga
College Alumni Association is representative: founded in 2025 with a written constitution, an elected
fifteen-position Executive Committee and a three-year election cycle, it had no digital record of who
its members were.

This dissertation reports the design, construction and evaluation of a platform for that
association, conducted as a design science research study. The problem is not an absence of alumni
software but the mismatch between what commercial products assume and what this institution can
supply: no payment gateway credentials, no licence budget, no technical staff, a mobile-first and
bandwidth-constrained membership, and a written constitution whose provisions the software must
respect rather than reinterpret.

The artefact is a clean-architecture ASP.NET Core 9 API exposing 285 endpoints across 38
controllers over 53 persisted entity sets, with an Angular 21 web client and a Flutter mobile
client. Three design positions distinguish it. Constitutional rules are encoded as testable
business rules traced to the article that mandates them, so that voting rights, committee
composition and membership tiers cannot drift from the governing document. Payment is deliberately
manual: members pay through a displayed wallet or bank channel and upload proof, which removes the
gateway-credential dependency and any custody of card data at the cost of an administrative
verification step. Schema is applied by migration at boot, and revisable reference data such as the
constitution is synchronised separately at the same point, because seed data carried by a migration
would pin amendable text to whichever migration shipped it.

Evaluation follows the plan declared in Chapter 4 and covers requirement coverage, ISO/IEC 25010
product quality, static product metrics over the 517-case backend suite and the 381-case web suite,
security conformance against OWASP ASVS, and usability. The finding of interest is that procedural
legitimacy, rather than technical capability, is what bounds how much institutional governance can
be moved into software.

**Word count:** 349.

---

## vi. Keywords

Alumni management systems; design science research; clean architecture; requirements engineering;
institutional governance; digital constitution; ISO/IEC 25010; low-resource deployment; ASP.NET
Core; Angular; Flutter.

---

## vii. Table of Contents
















Section numbers are as printed in the body. Page numbers are the folios the PDF carries; a rebuild renumbers them, so they are filled in from the printed copy rather than kept by hand.

| Part, chapter and section | Page |
| --- | --- |
| **PART I — PROBLEM AND CONTEXT** | 11 |
| **Chapter 1 — Introduction** | 12 |
| 1.1 Research Context: Alumni Relations as an Institutional Function | 12 |
| 1.2 Problem Domain: the Govt. Haraganga College Alumni Association | 12 |
| 1.3 Problem Statement | 12 |
| 1.4 Conceptual Framework | 12 |
| 1.5 Research Questions | 13 |
| 1.6 Aims and Objectives | 13 |
| 1.7 Scope, Delimitations and Assumptions | 13 |
| 1.8 Research Method in Brief | 14 |
| 1.9 Contributions of this Work | 14 |
| 1.10 Stakeholders and Beneficiaries | 14 |
| 1.11 Structure of the Dissertation | 14 |
| **Chapter 2 — Literature Review and Related Work** | 17 |
| 2.1 Review Objectives and Questions | 17 |
| 2.2 Review Protocol | 17 |
| 2.3 Alumni Relations and Engagement: the Institutional Literature | 17 |
| 2.4 Community and Membership Platforms: Academic Treatment | 18 |
| 2.5 Architectural Literature | 18 |
| 2.6 Web and Mobile Engineering Literature | 18 |
| 2.7 Digital Governance, Electronic Voting and Procedural Legitimacy | 18 |
| 2.8 Security and Privacy Engineering Baselines | 19 |
| 2.9 Survey of Existing Systems and Products | 19 |
| 2.10 Comparative Analysis and Evaluation Criteria | 20 |
| 2.11 Research Gap | 20 |
| 2.12 Summary | 21 |
| **Chapter 3 — Requirements Engineering** | 26 |
| 3.1 Sources of the Requirements | 26 |
| 3.2 Requirements Analysis and Negotiation | 26 |
| 3.3 Requirements Specification | 26 |
| 3.4 Non-Functional Requirements | 30 |
| 3.5 Quality-Attribute Scenarios | 31 |
| 3.6 Use-Case Modelling | 32 |
| 3.7 User Stories, Acceptance Criteria and the Definition of Done | 32 |
| 3.8 Requirements Prioritisation | 32 |
| 3.9 Requirements Traceability | 33 |
| 3.10 Domain Constraints | 33 |
| 3.11 Feasibility Analysis | 34 |
| 3.12 Requirements Validation and Formal Technical Review | 34 |
| 3.13 Summary | 35 |
| **PART II — METHOD AND DESIGN** | 47 |
| **Chapter 4 — Research Methodology** | 48 |
| 4.1 Research Paradigm and Philosophical Position | 48 |
| 4.2 Design Science Research as the Governing Method | 48 |
| 4.3 Mapping Design Science Activities to the Work Performed | 48 |
| 4.4 Software Process Model and its Justification | 48 |
| 4.5 Evaluation Strategy | 49 |
| 4.6 Metrics Definition | 50 |
| 4.7 Data Collection and Analysis Procedures | 50 |
| 4.8 Risk Management: the RMMM Plan | 51 |
| 4.9 Research Ethics | 52 |
| 4.10 Limitations of the Chosen Method | 52 |
| 4.11 Summary | 53 |
| **Chapter 5 — System Analysis and Behavioural Modelling** | 58 |
| 5.1 Analysis Approach | 58 |
| 5.2 Structured Analysis: Data-Flow Modelling | 58 |
| 5.3 Object-Oriented Analysis | 58 |
| 5.4 Behavioural Modelling | 58 |
| 5.5 State Modelling of Long-Lived Entities | 59 |
| 5.6 Business Rules Catalogue | 59 |
| 5.7 Data Modelling | 60 |
| 5.8 Analysis Model Review and Validation | 60 |
| 5.9 Summary | 60 |
| **Chapter 6 — System Architecture and Design** | 73 |
| 6.1 Design Goals, Principles and Constraints | 73 |
| 6.2 Architectural Alternatives Considered and the Decision Taken | 73 |
| 6.3 Architectural Design — Clean Architecture | 73 |
| 6.4 Component-Level Design | 74 |
| 6.5 Data Design | 74 |
| 6.6 Interface Design | 75 |
| 6.7 Security Architecture | 75 |
| 6.8 User-Interface Design | 75 |
| 6.9 Mobile Application Design and Platform-Specific Concerns | 76 |
| 6.10 Configuration-Driven Design | 76 |
| 6.11 Design Principles: Claim, Mechanism and Evidence | 76 |
| 6.12 Design Patterns Applied | 77 |
| 6.13 Architecture Decision Records | 78 |
| 6.14 Design Verification | 79 |
| 6.15 Summary | 79 |
| **PART III — CONSTRUCTION AND VALIDATION** | 89 |
| **Chapter 7 — Implementation** | 90 |
| 7.1 Development Environment, Toolchain and Reproducibility | 90 |
| 7.2 Solution and Module Structure | 90 |
| 7.3 Coding Standards, Conventions and Static Enforcement | 90 |
| 7.4 Implementation of the Domain and Persistence Layers | 91 |
| 7.5 Implementation of the Application and Business Services | 91 |
| 7.6 Implementation of the API Layer | 91 |
| 7.7 Implementation of the Web Client | 91 |
| 7.8 Implementation of the Mobile Client | 91 |
| 7.9 Real-Time Features | 92 |
| 7.10 Security Implementation | 92 |
| 7.11 Document Generation | 92 |
| 7.12 Constitution Publication Pipeline | 92 |
| 7.13 Third-Party Libraries: selection criteria | 93 |
| 7.14 Software Configuration Management | 93 |
| 7.15 Notable Implementation Challenges and Their Resolution | 93 |
| 7.16 Institution Profile Packs and White-Label Configuration | 94 |
| 7.17 Summary | 94 |
| **Chapter 8 — Security, Privacy and Trust** | 96 |
| 8.1 Security Objectives and Assumptions | 96 |
| 8.2 Threat Modelling (STRIDE) | 96 |
| 8.3 Authentication and Session Security | 96 |
| 8.4 Authorisation Model and the Role–Permission Matrix | 96 |
| 8.5 Input Validation and Output Sanitisation | 97 |
| 8.6 File Upload Security | 97 |
| 8.7 Transport, Header and Browser-Policy Security | 97 |
| 8.8 Rate Limiting and Abuse Prevention | 98 |
| 8.9 Payment-Related Risk and the No-Gateway-Keys Posture | 98 |
| 8.10 Governance Integrity | 98 |
| 8.11 Personal Data: Lawful Basis, Minimisation, Consent, Retention and Subject Rights | 98 |
| 8.12 Audit Logging and Non-Repudiation | 99 |
| 8.13 Conformance Assessment against OWASP ASVS | 99 |
| 8.14 Residual Risks and Recommendations | 99 |
| 8.15 Summary | 100 |
| **Chapter 9 — Verification, Validation and Quality Assurance** | 106 |
| 9.1 Verification and Validation Strategy and Test Levels | 106 |
| 9.2 Software Quality Assurance Plan | 106 |
| 9.3 Test-Case Design Techniques Applied | 106 |
| 9.4 Unit Testing | 106 |
| 9.5 Unit and Component Testing | 107 |
| 9.6 Widget and Golden Testing | 107 |
| 9.7 Integration Testing Strategy | 107 |
| 9.8 System and End-to-End Testing | 107 |
| 9.9 Regression Testing and Test Selection | 107 |
| 9.10 Security Testing | 107 |
| 9.11 Performance and Load Testing | 108 |
| 9.12 Usability and Accessibility Testing | 108 |
| 9.13 User Acceptance Testing | 108 |
| 9.14 Product Metrics and Static Analysis | 108 |
| 9.15 Defect Analysis | 108 |
| 9.16 Threats to the Validity of the Evaluation | 108 |
| 9.17 Summary | 108 |
| **Chapter 10 — Deployment and Operations** | 109 |
| 10.1 Deployment Architecture | 109 |
| 10.2 Environment Topology and Configuration Differences | 109 |
| 10.3 Containerisation Strategy | 109 |
| 10.4 Continuous Integration and Continuous Deployment | 109 |
| 10.5 Database Provisioning, Migration and Live Data Synchronisation | 109 |
| 10.6 Configuration and Secret Management | 109 |
| 10.7 Observability | 109 |
| 10.8 Backup, Recovery and Business Continuity | 109 |
| 10.9 Release and Rollback Procedure | 109 |
| 10.10 Operational Cost Model and Sustainability under Institutional Budget Constraints | 109 |
| 10.11 Maintenance Plan and Handover | 110 |
| 10.12 Summary | 110 |
| **Chapter 11 — Project Management** | 111 |
| 11.0 The Four P's Applied | 111 |
| 11.1 Process Model in Practice and its Deviations from Plan | 111 |
| 11.2 Work Breakdown Structure | 111 |
| 11.3 Scheduling, Task Network and Critical Path | 112 |
| 11.4 Effort Estimation | 113 |
| 11.5 How the Implementation Time Was Optimised | 114 |
| 11.6 Progress Tracking and Earned Value | 115 |
| 11.7 Team Structure and Responsibilities | 115 |
| 11.8 Configuration and Change Management in Practice | 115 |
| 11.9 Risk Monitoring Record | 116 |
| 11.10 Quality Assurance Activities Performed | 116 |
| 11.11 Lessons in Project Management | 116 |
| 11.12 Summary | 117 |
| **PART IV — EVALUATION AND CLOSURE** | 118 |
| **Chapter 12 — Results, Evaluation and Discussion** | 119 |
| 12.1 Overview of the Delivered Artefact | 119 |
| 12.2 Functional Evaluation | 119 |
| 12.3 Quality Evaluation against ISO/IEC 25010 | 119 |
| 12.4 Performance Evaluation Results | 119 |
| 12.5 Security Evaluation Results | 119 |
| 12.6 Usability and Accessibility Evaluation Results | 119 |
| 12.7 Stakeholder and Expert Evaluation | 119 |
| 12.8 Answering the Research Questions | 119 |
| 12.9 Discussion | 119 |
| 12.10 Comparison against the Existing Manual System | 119 |
| 12.11 Threats to Validity | 119 |
| 12.12 Limitations of the Artefact | 119 |
| 12.13 Reflection on the Design Science Contribution | 119 |
| 12.14 Summary | 120 |
| **Chapter 13 — Conclusion and Future Work** | 121 |
| 13.1 Summary of the Work | 121 |
| 13.2 Contributions Restated and Substantiated | 121 |
| 13.3 Answers to the Research Questions, in Brief | 121 |
| 13.4 Practical Implications for Similar Institutions | 121 |
| 13.5 Lessons Learned | 121 |
| 13.6 Future Work | 121 |
| 13.7 Concluding Remarks | 121 |
| **References** | 122 |

---

## viii. List of Figures

| Figure | Title | Page |
| --- | --- | --- |
| 1.1 | Context diagram (DFD Level 0): platform boundary and external entities | 15 |
| 1.2 | Stakeholder onion diagram | 15 |
| 1.3 | Conceptual framework: inputs, the designed artefact, and how it is judged | 16 |
| 1.4 | Research question, objective and chapter map | 16 |
| 2.1 | Study selection flow | 21 |
| 2.2 | Concept map of the reviewed literature | 22 |
| 2.3 | As-is process model of current manual practice (BPMN, abstracted) | 23 |
| 2.4 | Positioning chart: governance depth against annual operating cost | 24 |
| 3.1 | System-level use-case diagram, packaged | 36 |
| 3.2 | Membership subsystem use cases | 37 |
| 3.3 | Events subsystem use cases | 38 |
| 3.4 | Payments subsystem use cases | 38 |
| 3.5 | Governance subsystem use cases | 39 |
| 3.6 | Administration subsystem use cases | 40 |
| 3.7 | Actor generalisation hierarchy | 40 |
| 3.8 | Domain model, analysis level: membership, obligations and payment | 41 |
| 3.9 | Domain model, analysis level: participation, governance and content | 41 |
| 3.10 | Quality-attribute utility tree | 42 |
| 3.11 | Requirements classification, FURPS+ | 43 |
| 3.12 | Goal model | 43 |
| 4.1 | Design Science Research framework with this project's instantiation labelled | 53 |
| 4.2 | Design Science process model as executed | 54 |
| 4.3 | Research design overview: phases, inputs, outputs, evaluation points | 55 |
| 4.4 | Process model diagram of the adopted incremental lifecycle | 56 |
| 4.5 | Risk exposure matrix | 57 |
| 5.1 | DFD Level 0 (context). In and out are relative to the platform | 60 |
| 5.2 | DFD Level 1 | 60 |
| 5.3 | DFD Level 2: Payment processing | 61 |
| 5.4 | DFD Level 2: Membership approval | 62 |
| 5.5 | DFD Level 2: Constitution publication | 63 |
| 5.6 | Activity diagram: registration and administrative approval | 64 |
| 5.7 | Activity diagram: payment declaration and verification | 65 |
| 5.8 | Activity diagram: event registration with waitlist | 66 |
| 5.9 | Swimlane activity diagram: constitution amendment vote | 67 |
| 5.10 | BPMN process diagram of the election cycle | 68 |
| 5.11 | State-machine diagram: member lifecycle | 68 |
| 5.12 | State-machine diagram: payment and declaration | 69 |
| 5.13 | State-machine diagram: constitution version | 69 |
| 5.14 | State-machine diagram: event lifecycle | 70 |
| 5.15 | Sequence diagram: login with OTP, token issue | 70 |
| 5.16 | Sequence diagram: event registration | 71 |
| 5.17 | Sequence diagram: payment declaration and verification | 71 |
| 5.18 | Sequence diagram: real-time notification over SignalR | 71 |
| 5.19 | Timing diagram: token lifetime and refresh window | 72 |
| 6.1 | High-level architecture diagram | 80 |
| 6.2 | Layered / clean architecture diagram with the dependency-inversion boundary marked | 81 |
| 6.3 | Entity–relationship diagram, identity and records sub-model | 81 |
| 6.4 | Entity–relationship diagram, standing and money sub-model | 82 |
| 6.5 | Entity–relationship diagram, events and participation sub-model | 82 |
| 6.6 | Entity–relationship diagram, governance sub-model | 83 |
| 6.7 | Design class diagram: domain model | 83 |
| 6.8 | Design class diagram: application interfaces and infrastructure services | 84 |
| 6.9 | Component diagram with provided and required interfaces | 85 |
| 6.10 | Middleware pipeline diagram | 86 |
| 6.11 | Navigation and route map | 86 |
| 6.12 | Site map and information architecture of the public site | 87 |
| 6.13 | Architectural trade-off radar | 87 |
| 8.1 | Threat model data-flow diagram with trust boundaries, STRIDE-annotated | 100 |
| 8.2 | Attack tree: member account takeover or fraudulent payment credit | 101 |
| 8.3 | Role–permission matrix diagram | 101 |
| 8.4 | Personal-data classification and flow diagram, with retention points | 102 |
| 8.5 | Sequence diagram: an unauthorised request rejected through the middleware chain | 102 |
| 8.6 | Defence-in-depth layer diagram | 103 |

*Figures for Chapters 4 to 13 are listed as those chapters are written.*

---

## ix. List of Tables

| Table | Title | Page |
| --- | --- | --- |
| 2.1 | Review protocol summary | 24 |
| 2.2 | Feature and capability comparison | 25 |
| 2.3 | Gap table | 25 |
| 3.1 | Functional requirement catalogue | 43 |
| 3.2 | Non-functional requirement catalogue | 43 |
| 3.3 | Use-case descriptions, ten highest-value cases | 44 |
| 3.4 | Requirements traceability matrix | 45 |
| 3.5 | MoSCoW prioritisation and negotiation outcome | 45 |
| 3.6 | Domain constraints, their class and their constitutional article | 45 |
| 3.7 | Feasibility summary | 45 |
| 3.8 | Specification defects found by the formal technical review | 46 |
| 4.1 | Metric definitions | 50 |
| 4.2 | RMMM table | 52 |
| 4.3 | Evaluation plan | 57 |
| 5.1 | Business rules catalogue | 59 |
| 5.2 | CRC card set for the analysis classes with the widest collaboration surface | 63 |
| 5.3 | Process specifications for the Level-2 processes | 72 |
| 5.4 | Data-store definitions (analysis level) | 72 |
| 6.1 | ADR index | 79 |
| 6.2 | Data dictionary (representative slice; the full dictionary is generated from the schema) | 88 |
| 6.3 | API endpoint catalogue (by controller; the full catalogue is the generated OpenAPI document) | 88 |
| 6.4 | Design pattern catalogue (selected entries; full catalogue is §6.12 in full) | 88 |
| 6.5 | Quality-attribute scenario to architectural tactic mapping | 88 |
| 6.6 | Anti-patterns detected and remediated | 88 |
| 7.1 | Size metrics by layer: files and lines of code | 90 |
| 7.2 | Selected third-party dependencies | 95 |
| 8.1 | STRIDE threat enumeration with mitigations and their implementation location | 104 |
| 8.2 | Role × capability matrix | 104 |
| 8.3 | OWASP ASVS conformance checklist (self-assessment, ASVS 4.0.3, level 2) | 104 |
| 8.4 | Personal-data inventory: element, purpose, lawful basis, retention | 105 |
| 8.5 | Residual risk register | 105 |
| 11.1 | Component activity list, CPM summary ordered by float | 112 |
| 11.2 | Unadjusted function-point count | 113 |
| 11.3 | General system characteristics and technical complexity factor | 113 |
| 11.4 | Delivered size by kind, at a stated production rate | 114 |
| 11.5 | Risk exposure, computed from Table 4.2 | 116 |

---

## x. List of Listings

*None in Part I. Code extracts begin in Chapter 6 and are concentrated in Chapter 7. Complete
listings are in the repository at the commit named in §7.1.*

---

## xi. List of Abbreviations and Acronyms

| Term | Expansion |
| --- | --- |
| ADR | Architecture Decision Record |
| AGM | Annual General Meeting |
| API | Application Programming Interface |
| ASVS | Application Security Verification Standard (OWASP) |
| BPMN | Business Process Model and Notation |
| CBO | Coupling Between Objects |
| CEC | Chief Election Commissioner |
| CI/CD | Continuous Integration and Continuous Deployment |
| CRC | Class, Responsibilities, Collaborators |
| CRM | Customer Relationship Management |
| DFD | Data-Flow Diagram |
| DSR | Design Science Research |
| DTO | Data Transfer Object |
| EC | Executive Committee |
| EF Core | Entity Framework Core |
| FR | Functional Requirement |
| FURPS+ | Functionality, Usability, Reliability, Performance, Supportability, plus constraints |
| GHC | Govt. Haraganga College |
| GHCAA | Govt. Haraganga College Alumni Association |
| HSC | Higher Secondary Certificate |
| JWT | JSON Web Token |
| LCOM | Lack of Cohesion of Methods |
| MFS | Mobile Financial Service |
| MoSCoW | Must have, Should have, Could have, Won't have |
| NFR | Non-Functional Requirement |
| NID | National Identity document number |
| OTP | One-Time Password |
| PII | Personally Identifiable Information |
| QAS | Quality-Attribute Scenario |
| RBAC | Role-Based Access Control |
| RMMM | Risk Mitigation, Monitoring and Management |
| RQ | Research Question |
| SPA | Single-Page Application |
| SRS | Software Requirements Specification |
| STRIDE | Spoofing, Tampering, Repudiation, Information disclosure, Denial of service, Elevation of privilege |
| SUS | System Usability Scale |
| UAT | User Acceptance Testing |
| WCAG | Web Content Accessibility Guidelines |

---

## xii. Glossary of Domain Terms

**Active member.** A member whose application has been approved, whose registration and annual dues
are settled, and who therefore appears in the directory and may exercise whatever rights the tier
carries.

**Applied member.** An applicant who has submitted the registration wizard but has not been
approved. Applied members are invisible in the directory and confined to the onboarding view.

**Associate member.** A verified former student who did not complete HSC but studied at least one
academic year, or who chooses the non-voting tier. Pays a registration fee only.

**Founding member.** The senior tier defined by Article III Section C: at least twenty years since
HSC, a bachelor's degree, a documented contribution to the institution, no partisan political
office, and a signed declaration of non-political engagement. Founding members hold lifetime status
and vote.

**General member.** The ordinary voting tier. At least five years since HSC, dues paid, nothing
outstanding.

**Good standing.** A member with no outstanding dues and no live disciplinary sanction. The phrase
is constitutional, and Article IX Section 3 attaches it to the right to vote.

**Membership number.** The identifier minted at approval, of the form `GHC-<year>-<serial>`, which
then doubles as the login identifier.

**Ratified version.** The single Constitution row flagged active. Earlier rows are superseded rather
than deleted, so that amendment votes cast against them survive.

**Security stamp.** A per-user value that participates in token validation, so that suspending or
terminating a member invalidates every live session on every client.

**Soft delete.** Archival by flag rather than row removal, filtered out globally in the persistence
layer so that archived records stay auditable without leaking into any read path.

**Voting member.** A member of the Founding, Executive or General tier. Associate, Honorary and
Advisory members do not vote unless specifically granted, per Article III Section B.

---

## xiii. Statement of Contributions

This section separates what is claimed as novel from what is competent engineering and what is
reused, because the distinction changes how Chapter 12 should be read.

**Claimed as a contribution.**

1. A method for encoding the written constitution of a voluntary association as traceable software
   business rules, in which every rule carries the article that mandates it, and a five-class
   scheme, being deterministic, state, evidence, procedural and authority, that decides for each
   rule what the software is permitted to do with it. The boundary between what may be automated
   and what must stay with the officers becomes a property of the rule, read from the constitution,
   rather than a matter of the designer's restraint. Applied to all sixteen constraints in §3.10, the
   domain-constraint catalogue; enforced through the business rules of §5.6 and pinned by the
   constitutional tests of §9.4.5; and used to answer RQ3 in §12.8.
2. An account of what an alumni platform becomes when payment gateway credentials are unavailable
   by policy rather than by oversight, together with the security and operational consequences of
   displaying manual payment instructions and verifying uploaded proof (§8.9).
3. A characterisation of the requirements of an alumni association operating under a
   single-maintainer, zero-licence-budget constraint, with the architectural decision recorded
   against those constraints instead of against a hypothetical growth curve (Chapter 6).

**Engineering rather than research contribution.** The three clients, the 284-endpoint API surface,
the 53-entity schema, the CI pipeline, the document-generation subsystem and the shared control
library are engineering. They are the artefact the research is about, and their quality is measured
in Chapter 12, but no novelty is claimed for the techniques used to build them.

**Reused.** Clean architecture as a layering scheme [1]; the enterprise patterns catalogued by
Fowler [2]; the design patterns of Gamma et al. [3]; ASP.NET Core, Entity Framework Core, Angular,
Flutter and the libraries catalogued in Chapter 7; the design science framework of Hevner et al. [4] and the
process model of Peffers et al. [5]; the ISO/IEC 25010 quality model [6]; the OWASP ASVS checklist
[7].

---

## xiv. Ethics Statement and Data-Protection Declaration

The platform holds personal data about identifiable people: name, date of birth, national identity
number, mobile number, email address, permanent address, blood group, parents' names, photograph,
academic and employment history, and payment records. That is a sensitive collection, and the
dissertation treats it as one.

**Consent and purpose.** Members supply their data through the registration wizard for the stated
purpose of membership administration. Nothing in the platform sells, shares or profiles that data.
The privacy toggles described in FR-03 let a member decide, field by field, whether other members
see the value or a masked form of it.

**Minimisation.** Section 9.11 records the lawful basis and retention position for every element in
the personal-data inventory. Where a field was requested during elicitation but could not be
justified against a specific requirement, it was dropped; the negotiation record is in §3.2.

**Research data.** Interview material, usability session records and acceptance-test observations
were collected with verbal consent and no written consent form. Participants were told what the
material would be used for. Participants are identified in this dissertation by role and never by
name. The absence of a documented consent procedure is a limitation of the study, set out with its
consequences in §4.9, not an omission from this statement.

**Live data in the dissertation.** Every screenshot in Chapter 12 uses seeded or anonymised records.
No real member's identity number, address, telephone number or photograph appears anywhere in this
document.

**Approval.** No institutional ethics committee reviewed this study. Permission to conduct it, and
to use the Association's records, governing documents and membership data, was given by the
Government Haraganga College Alumni Association. That permission was verbal: no approval letter was
issued, no reference number exists, and no date was recorded. Section 4.9 states what follows from
that.
