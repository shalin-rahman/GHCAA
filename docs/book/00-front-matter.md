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
§3.1.3.

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

The artefact is a clean-architecture ASP.NET Core 9 API exposing 276 endpoints across 37
controllers over 49 persisted entity sets, with an Angular 21 web client and a Flutter mobile
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
| **PART I — PROBLEM AND CONTEXT** | 9 |
| **Chapter 1 — Introduction** | 10 |
| §1.1 Research Context: Alumni Relations as an Institutional Function | 10 |
| §1.2 Problem Domain: the Govt. Haraganga College Alumni Association | 10 |
| §1.3 Problem Statement | 10 |
| §1.4 Research Questions | 10 |
| §1.5 Aims and Objectives | 11 |
| §1.6 Scope, Delimitations and Assumptions | 11 |
| §1.7 Research Method in Brief | 11 |
| §1.8 Contributions of this Work | 11 |
| §1.9 Stakeholders and Beneficiaries | 12 |
| §1.10 Structure of the Dissertation | 12 |
| **Chapter 2 — Literature Review and Related Work** | 15 |
| §2.1 Review Objectives and Questions | 15 |
| §2.2 Review Protocol | 15 |
| §2.3 Alumni Relations and Engagement: the Institutional Literature | 15 |
| §2.4 Community and Membership Platforms: Academic Treatment | 16 |
| §2.5 Architectural Literature | 16 |
| §2.6 Web and Mobile Engineering Literature | 16 |
| §2.7 Digital Governance, Electronic Voting and Procedural Legitimacy | 16 |
| §2.8 Security and Privacy Engineering Baselines | 17 |
| §2.9 Survey of Existing Systems and Products | 17 |
| §2.10 Comparative Analysis and Evaluation Criteria | 18 |
| §2.11 Research Gap | 18 |
| §2.12 Summary | 19 |
| **Chapter 3 — Requirements Engineering** | 24 |
| §3.1 Requirements Elicitation | 24 |
| §3.2 Requirements Analysis and Negotiation | 25 |
| §3.3 Requirements Specification | 25 |
| §3.4 Non-Functional Requirements | 28 |
| §3.5 Quality-Attribute Scenarios | 29 |
| §3.6 Use-Case Modelling | 30 |
| §3.7 User Stories, Acceptance Criteria and the Definition of Done | 30 |
| §3.8 Requirements Prioritisation | 30 |
| §3.9 Requirements Traceability | 31 |
| §3.10 Domain Constraints | 31 |
| §3.11 Feasibility Analysis | 31 |
| §3.12 Requirements Validation and Formal Technical Review | 32 |
| §3.13 Summary | 32 |
| **PART II — METHOD AND DESIGN** | 43 |
| **Chapter 4 — Research Methodology** | 44 |
| §4.1 Research Paradigm and Philosophical Position | 44 |
| §4.2 Design Science Research as the Governing Method | 44 |
| §4.3 Mapping Design Science Activities to the Work Performed | 44 |
| §4.4 Software Process Model and its Justification | 44 |
| §4.5 Evaluation Strategy | 45 |
| §4.6 Metrics Definition | 45 |
| §4.7 Data Collection and Analysis Procedures | 46 |
| §4.8 Risk Management: the RMMM Plan | 46 |
| §4.9 Research Ethics | 47 |
| §4.10 Limitations of the Chosen Method | 47 |
| §4.11 Summary | 48 |
| **Chapter 5 — System Analysis and Behavioural Modelling** | 53 |
| §5.1 Analysis Approach | 53 |
| §5.2 Structured Analysis: Data-Flow Modelling | 53 |
| §5.3 Object-Oriented Analysis | 53 |
| §5.4 Behavioural Modelling | 53 |
| §5.5 State Modelling of Long-Lived Entities | 54 |
| §5.6 Business Rules Catalogue | 54 |
| §5.7 Data Modelling | 55 |
| §5.8 Analysis Model Review and Validation | 55 |
| §5.9 Summary | 55 |
| **Chapter 6 — System Architecture and Design** | 68 |
| §6.1 Design Goals, Principles and Constraints | 68 |
| §6.2 Architectural Alternatives Considered and the Decision Taken | 68 |
| §6.3 Architectural Design — Clean Architecture | 68 |
| §6.4 Component-Level Design | 69 |
| §6.5 Data Design | 69 |
| §6.6 Interface Design | 70 |
| §6.7 Security Architecture | 70 |
| §6.8 User-Interface Design | 70 |
| §6.9 Mobile Application Design and Platform-Specific Concerns | 71 |
| §6.10 Configuration-Driven Design | 71 |
| §6.11 Design Principles: Claim, Mechanism and Evidence | 71 |
| §6.12 Design Patterns Applied | 72 |
| §6.13 Architecture Decision Records | 73 |
| §6.14 Design Verification | 73 |
| §6.15 Summary | 74 |
| **References** | 83 |

---

## viii. List of Figures

| Figure | Title | Page |
| --- | --- | --- |
| 1.1 | Context diagram (DFD Level 0): platform boundary and external entities | 12 |
| 1.2 | Stakeholder onion diagram | 13 |
| 1.3 | Research question, objective and chapter map | 14 |
| 2.1 | Study selection flow | 19 |
| 2.2 | Concept map of the reviewed literature | 20 |
| 2.3 | As-is process model of current manual practice (BPMN, abstracted) | 21 |
| 2.4 | Positioning chart: governance depth against annual operating cost | 22 |
| 3.1 | System-level use-case diagram, packaged | 33 |
| 3.2 | Membership subsystem use cases | 34 |
| 3.3 | Events subsystem use cases | 35 |
| 3.4 | Payments subsystem use cases | 35 |
| 3.5 | Governance subsystem use cases | 36 |
| 3.6 | Administration subsystem use cases | 37 |
| 3.7 | Actor generalisation hierarchy | 37 |
| 3.8 | Domain model, analysis level: membership, obligations and payment | 38 |
| 3.9 | Domain model, analysis level: participation, governance and content | 38 |
| 3.10 | Quality-attribute utility tree | 39 |
| 3.11 | Requirements classification, FURPS+ | 40 |
| 3.12 | Goal model | 40 |
| 4.1 | Design Science Research framework with this project's instantiation labelled | 48 |
| 4.2 | Design Science process model as executed | 49 |
| 4.3 | Research design overview: phases, inputs, outputs, evaluation points | 50 |
| 4.4 | Process model diagram of the adopted incremental lifecycle | 51 |
| 4.5 | Risk exposure matrix | 52 |
| 5.1 | DFD Level 0 (context) | 55 |
| 5.2 | DFD Level 1 | 56 |
| 5.3 | DFD Level 2: Payment processing | 56 |
| 5.4 | DFD Level 2: Membership approval | 57 |
| 5.5 | DFD Level 2: Constitution publication | 58 |
| 5.6 | Activity diagram: registration and administrative approval | 59 |
| 5.7 | Activity diagram: payment declaration and verification | 60 |
| 5.8 | Activity diagram: event registration with waitlist | 61 |
| 5.9 | Swimlane activity diagram: constitution amendment vote | 62 |
| 5.10 | BPMN process diagram of the election cycle | 63 |
| 5.11 | State-machine diagram: member lifecycle | 63 |
| 5.12 | State-machine diagram: payment and declaration | 64 |
| 5.13 | State-machine diagram: constitution version | 64 |
| 5.14 | State-machine diagram: event lifecycle | 65 |
| 5.15 | Sequence diagram: login with OTP, token issue | 65 |
| 5.16 | Sequence diagram: event registration | 66 |
| 5.17 | Sequence diagram: payment declaration and verification | 66 |
| 5.18 | Sequence diagram: real-time notification over SignalR | 66 |
| 5.19 | Timing diagram: token lifetime and refresh window | 67 |
| 6.1 | High-level architecture diagram | 75 |
| 6.2 | Layered / clean architecture diagram with the dependency-inversion boundary marked | 76 |
| 6.3 | Entity–relationship diagram, identity and records sub-model | 76 |
| 6.4 | Entity–relationship diagram, standing and money sub-model | 77 |
| 6.5 | Entity–relationship diagram, events and participation sub-model | 77 |
| 6.6 | Entity–relationship diagram, governance sub-model | 78 |
| 6.7 | Design class diagram: domain model | 78 |
| 6.8 | Design class diagram: application interfaces and infrastructure services | 79 |
| 6.9 | Component diagram with provided and required interfaces | 79 |
| 6.10 | Middleware pipeline diagram | 80 |
| 6.11 | Navigation and route map | 80 |
| 6.12 | Site map and information architecture of the public site | 81 |
| 6.13 | Architectural trade-off radar | 81 |

*Figures for Chapters 4 to 13 are listed as those chapters are written.*

---

## ix. List of Tables

| Table | Title | Page |
| --- | --- | --- |
| 2.1 | Review protocol summary | 22 |
| 2.2 | Feature and capability comparison | 23 |
| 2.3 | Gap table | 23 |
| 3.1 | Functional requirement catalogue | 40 |
| 3.2 | Non-functional requirement catalogue | 40 |
| 3.3 | Use-case descriptions, ten highest-value cases | 41 |
| 3.4 | Requirements traceability matrix | 42 |
| 3.5 | MoSCoW prioritisation and negotiation outcome | 42 |
| 3.6 | Domain constraints traced to constitutional article | 42 |
| 3.7 | Feasibility summary | 42 |
| 3.8 | Specification defects found by the formal technical review | 42 |
| 4.1 | Metric definitions | 46 |
| 4.2 | RMMM table | 47 |
| 4.3 | Evaluation plan | 52 |
| 5.1 | Business rules catalogue | 54 |
| 5.2 | CRC card set for the analysis classes with the widest collaboration surface | 58 |
| 5.3 | Process specifications for the Level-2 processes | 67 |
| 5.4 | Data-store definitions (analysis level) | 67 |
| 6.1 | ADR index | 73 |
| 6.2 | Data dictionary (representative slice; the full dictionary is generated from the schema) | 82 |
| 6.3 | API endpoint catalogue (by controller; the full catalogue is the generated OpenAPI document) | 82 |
| 6.4 | Design pattern catalogue (selected entries; full catalogue is §6.12 in full) | 82 |
| 6.5 | Quality-attribute scenario to architectural tactic mapping | 82 |
| 6.6 | Anti-patterns detected and remediated | 82 |

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
   business rules, in which every rule carries the article that mandates it, and the boundary
   between rules that may be automated and decisions that must stay with the officers is argued
   rather than assumed (§3.10, §5.6, §9.4.5).
2. An account of what an alumni platform becomes when payment gateway credentials are unavailable
   by policy rather than by oversight, together with the security and operational consequences of
   displaying manual payment instructions and verifying uploaded proof (§8.9).
3. A characterisation of the requirements of an alumni association operating under a
   single-maintainer, zero-licence-budget constraint, with the architectural decision recorded
   against those constraints instead of against a hypothetical growth curve (Chapter 6).

**Engineering rather than research contribution.** The three clients, the 276-endpoint API surface,
the 49-entity schema, the CI pipeline, the document-generation subsystem and the shared control
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
consequences in §3.1.3, not an omission from this statement.

**Live data in the dissertation.** Every screenshot in Chapter 12 uses seeded or anonymised records.
No real member's identity number, address, telephone number or photograph appears anywhere in this
document.

**Approval.** No institutional ethics committee reviewed this study. Permission to conduct it, and
to use the Association's records, governing documents and membership data, was given by the
Government Haraganga College Alumni Association. That permission was verbal: no approval letter was
issued, no reference number exists, and no date was recorded. Section 3.1.3 states what follows from
that.
