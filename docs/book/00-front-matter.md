# GHCAA Alumni Association Platform

## Design, Construction and Evaluation of a Governance-Aware Alumni Management Platform for a Resource-Constrained Institution

---

*Front matter is numbered in lower-case Roman numerals. The body restarts at Arabic 1.*

---

## i. Title Page

**Title.** Design, Construction and Evaluation of a Governance-Aware Alumni Management Platform for a Resource-Constrained Institution: The GHCAA Case

**Subject of the study.** The Govt. Haraganga College Alumni Association (HARAGANGIAN), Bangladesh

**Artefact under study.** A three-client platform comprising an ASP.NET Core 9 REST API, an Angular 21 web application and a Flutter mobile application, deployed on a free-tier managed host.

**Author.** Md Habibur Rahman, Roll 220, 7th Batch, Evening Master's in Information Technology (EMIT)

**Supervisor.** Dr. Kazi Muheymin-Us-Sakib, Professor, Institute of Information Technology, University of Dhaka

**Department and institution.** Executive Masters in Information and Technology Program, Batch-7, Institute of Information Technology (IIT), University of Dhaka, Dhaka, Bangladesh

**Submitted.** *[month, year of submission]*

---

## ii. Declaration of Originality

I declare that this dissertation and the software artefact it describes are my own work. Where the
work of others has been used it is cited in the text and listed in the References. The platform
described in Part III was written by me as sole maintainer; third-party libraries are catalogued
with their licences in Table 7.1, and no part of the codebase has been submitted for any other
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

*[To be written last. Should name: the supervisor; the Association officers who gave time to the
requirements work and the acceptance testing; the college administration for permission to use the
institutional name and crest; and the members who tested the registration and payment paths on
their own devices.]*

---

## v. Abstract

Alumni associations at public colleges in Bangladesh run on paper. Membership rolls live in
spreadsheets or ledgers, subscriptions are collected in cash at reunions, and the constitution that
governs the body exists as a printed document whose current version is known reliably only to the
officers who hold it. The Govt. Haraganga College Alumni Association is representative. Founded in
2025 with a written constitution, an elected fifteen-position Executive Committee and a three-year
election cycle, it had no digital record of who its members were.

This dissertation reports the design, construction and evaluation of a platform for that
association, conducted as a design science research study. The problem is not an absence of alumni
software, since commercial products exist, but the mismatch between what those products assume and
what this institution can supply: no payment gateway credentials, no recurring licence budget, no
technical staff, a mobile-first and bandwidth-constrained membership, and a written constitution
whose provisions the software is obliged to respect rather than reinterpret.

The artefact is a clean-architecture ASP.NET Core 9 API exposing 260 endpoints across 37
controllers over 49 persisted entity sets, with an Angular 21 web client and a Flutter mobile
client. Three design positions distinguish it. Constitutional rules are encoded as testable
business rules traced to the article that mandates them, so that voting rights, committee
composition and membership tiers cannot drift from the governing document. Payment is deliberately
manual: members pay through a displayed wallet or bank channel and upload proof, which removes the
gateway-credential dependency and any custody of card data at the cost of an administrative
verification step. Reference data is synchronised at application boot rather than by migration,
because the deployment target creates its schema with `EnsureCreated()`, which is inert against a
populated database.

Evaluation follows the plan declared in Chapter 4 and covers requirement coverage, ISO/IEC 25010
product quality, static product metrics over the 352-case backend suite and the client suites,
security conformance against OWASP ASVS, and usability. The finding of interest is that procedural
legitimacy, rather than technical capability, is what bounds how much institutional governance can
be moved into software.

**Word count:** 334.

---

## vi. Keywords

Alumni management systems; design science research; clean architecture; requirements engineering;
institutional governance; digital constitution; ISO/IEC 25010; low-resource deployment; ASP.NET
Core; Angular; Flutter.

---

## vii. Table of Contents

*Generated at typesetting. Part I covers Chapters 1 to 3; Part II, Chapters 4 to 6; Part III,
Chapters 7 to 11; Part IV, Chapters 12 and 13.*

---

## viii. List of Figures

| Figure | Title | Page |
| --- | --- | --- |
| 1.1 | Context diagram: platform boundary and external entities | |
| 1.2 | Stakeholder onion diagram | |
| 1.3 | Research question, objective and chapter map | |
| 2.1 | Study-selection flow with counts at each stage | |
| 2.2 | Concept map of the reviewed literature | |
| 2.3 | As-is process model of current manual practice (BPMN) | |
| 2.4 | Positioning chart: governance depth against operating cost | |
| 3.1 | System-level use-case diagram | |
| 3.2 | Use-case diagram: Membership subsystem | |
| 3.3 | Use-case diagram: Events subsystem | |
| 3.4 | Use-case diagram: Payments subsystem | |
| 3.5 | Use-case diagram: Governance subsystem | |
| 3.6 | Use-case diagram: Administration subsystem | |
| 3.7 | Actor generalisation hierarchy | |
| 3.8 | Domain model, analysis level | |
| 3.9 | Quality-attribute utility tree | |
| 3.10 | Requirements classification tree, FURPS+ | |
| 3.11 | Goal model | |

*Figures for Chapters 4 to 13 are listed as those chapters are written.*

---

## ix. List of Tables

| Table | Title | Page |
| --- | --- | --- |
| 2.1 | Review protocol summary | |
| 2.2 | Feature and capability comparison matrix | |
| 2.3 | Gap table | |
| 3.1 | Functional requirement catalogue | |
| 3.2 | Non-functional requirement catalogue by ISO/IEC 25010 characteristic | |
| 3.3 | Use-case descriptions for the ten highest-value cases | |
| 3.4 | Requirements traceability matrix, opened | |
| 3.5 | MoSCoW prioritisation and negotiation outcome | |
| 3.6 | Domain constraints traced to constitutional article | |
| 3.7 | Feasibility summary | |
| 3.8 | Specification defects found by the formal technical review | |

---

## x. List of Listings

*None in Part I. Code extracts begin in Chapter 6 and are concentrated in Chapter 7. Complete
listings are in Appendix D.*

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
   rather than assumed (§3.10, §5.6, §8.4.5).
2. An account of what an alumni platform becomes when payment gateway credentials are unavailable
   by policy rather than by oversight, together with the security and operational consequences of
   displaying manual payment instructions and verifying uploaded proof (§9.9).
3. A characterisation of the requirements of an alumni association operating under a
   single-maintainer, zero-licence-budget constraint, with the architectural decision recorded
   against those constraints instead of against a hypothetical growth curve (Chapter 6).

**Engineering rather than research contribution.** The three clients, the 260-endpoint API surface,
the 49-entity schema, the CI pipeline, the document-generation subsystem and the shared control
library are engineering. They are the artefact the research is about, and their quality is measured
in Chapter 12, but no novelty is claimed for the techniques used to build them.

**Reused.** Clean architecture as a layering scheme [1]; the enterprise patterns catalogued by
Fowler [2]; the design patterns of Gamma et al. [3]; ASP.NET Core, Entity Framework Core, Angular,
Flutter and the libraries in Table 7.1; the design science framework of Hevner et al. [4] and the
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
were collected under the participant information sheet and consent form reproduced in Appendix A.
Participants are identified in the dissertation by role and never by name. Recordings, where taken,
are held on encrypted local storage and destroyed once the award is conferred.

**Live data in the dissertation.** Every screenshot in Chapter 12 uses seeded or anonymised records.
No real member's identity number, address, telephone number or photograph appears anywhere in this
document.

**Approval.** *[Institutional ethics reference and date of approval to be inserted. The approval
letter is Appendix A.]*
