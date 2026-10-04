# Standards, Requirements & Compliance

## 1. Purpose and Scope

The proposed system is a web-based, fully managed electronic election platform intended to support the configuration, administration, authentication, ballot casting, election monitoring, tallying, verification, auditing, and reporting of elections.

Because an electronic voting platform handles both **voter identity information** and **highly sensitive election records**, it shall be designed according to established election, information-security, privacy, accessibility, and software-quality principles.

The system shall distinguish between:

1. **Election-specific standards and principles** governing voting processes;
2. **Information-security and privacy standards** governing the protection of data and infrastructure;
3. **Accessibility standards** governing the voter interface;
4. **Project-specific functional and architectural requirements** derived from the election workflow and threat model; and
5. **Operational and managed-service controls** required for reliable SaaS delivery.

The platform shall not claim legal certification or compliance with a standard solely because its architecture incorporates selected controls from that standard. Formal certification, where applicable, requires assessment against the relevant standard, jurisdictional law, testing procedures, and certification authority.

---

# 2. Applicable Standards and Reference Frameworks

## 2.1 Council of Europe CM/Rec(2017)5 — Standards for E-Voting

Council of Europe Recommendation CM/Rec(2017)5 provides a comprehensive international framework for electronic voting, covering legal, operational, and technical aspects of e-voting, including Internet voting. It is intended to support the implementation of democratic-election principles in electronic voting systems.

The system shall therefore consider, as applicable:

* universal and equal suffrage;
* free and fair voting;
* secrecy of the vote;
* transparency;
* accessibility;
* voter authentication and eligibility;
* ballot integrity;
* auditability;
* verifiability;
* system reliability;
* protection against unauthorized intervention; and
* appropriate election administration and oversight.

The Recommendation shall serve as the principal **e-voting-specific reference framework** for the project.

---

## 2.2 U.S. Voluntary Voting System Guidelines (VVSG) 2.0

VVSG 2.0, maintained by the U.S. Election Assistance Commission (EAC), provides testable requirements and guidelines for voting-system functionality, accessibility, usability, security, and related system properties. VVSG compliance is voluntary at the U.S. federal level, although individual jurisdictions may require it under their own laws. It should therefore be treated by this project as a **technical benchmark**, not as automatically applicable law.

The project shall use relevant VVSG 2.0 principles as engineering benchmarks, particularly:

* high-quality system design;
* high-quality implementation;
* transparency;
* accessibility;
* usability;
* security;
* system integrity;
* auditability;
* software integrity;
* robust error handling;
* controlled access;
* data protection; and
* reliable election records.

VVSG 2.0 shall not be interpreted as evidence that the proposed system is EAC-certified unless the system has undergone the applicable formal certification process.

---

## 2.3 ISO/IEC 27001:2022 — Information Security Management

ISO/IEC 27001:2022 specifies requirements for an Information Security Management System (ISMS) and provides a systematic approach to identifying and managing information-security risks. It addresses confidentiality, integrity, availability, risk management, governance, and continual improvement.

The managed platform shall establish controls covering, as applicable:

* information-security governance;
* risk assessment and treatment;
* asset management;
* identity and access management;
* cryptographic controls;
* secure development;
* supplier and cloud-service security;
* incident management;
* business continuity;
* logging and monitoring;
* vulnerability management;
* backup and recovery;
* change management; and
* continual security improvement.

ISO/IEC 27001 certification shall be treated as a separate organizational certification activity rather than an automatic property of the software.

---

## 2.4 ISO/IEC 27018:2025 — Protection of PII in Public Cloud

ISO/IEC 27018:2025 provides guidance for protecting personally identifiable information (PII) processed by public-cloud providers acting as PII processors. The 2025 edition is the current published edition and complements an ISO/IEC 27001-based ISMS.

Where the election platform operates as a public-cloud SaaS service, the system shall address:

* PII classification;
* lawful and controlled processing;
* data minimization;
* access restrictions;
* secure transmission;
* secure storage;
* retention and deletion;
* data-subprocessor management;
* auditability;
* incident handling; and
* clear allocation of responsibilities between the election organizer and service provider.

---

## 2.5 WCAG 2.2 — Web Accessibility

The voter-facing application shall target **WCAG 2.2 Level AA**.

WCAG defines Level A as the minimum conformance level; Level AA includes all Level A and Level AA success criteria. Therefore, specifying Level AA as a project requirement is a deliberate project requirement rather than a claim that WCAG itself universally mandates Level AA.

The interface shall support, as applicable:

* keyboard navigation;
* screen-reader compatibility;
* sufficient text and interface contrast;
* accessible form controls;
* visible focus indicators;
* understandable instructions;
* error identification and recovery;
* responsive presentation;
* accessible authentication;
* accessible ballot selection;
* accessible review and confirmation;
* avoidance of interaction patterns that unnecessarily disadvantage users with motor, visual, auditory, or cognitive disabilities.

Accessibility shall be verified through both automated testing and representative manual evaluation.

---

# 3. Election-System Design Principles

The system shall be designed around the following core principles.

## 3.1 Ballot Secrecy

The platform shall prevent unauthorized parties from associating a voter with the contents of the voter's ballot.

Authentication records and ballot records shall therefore be architecturally separated and protected against reconstruction of the voter-to-ballot relationship.

Database-level separation alone shall not be considered sufficient where application logs, identifiers, administrative privileges, telemetry, or other records could reconstruct that relationship.

---

## 3.2 Eligibility and One-Voter-One-Ballot Enforcement

The system shall establish whether a person is eligible to vote and whether that voting entitlement has already been exercised.

The system shall prevent:

* unauthorized voting;
* duplicate voting;
* replay of voting credentials;
* unauthorized ballot submission;
* reuse of expired voting authorization; and
* manipulation of voter eligibility records.

The mechanism used to enforce one-voter-one-ballot shall not unnecessarily expose the voter's ballot selections.

---

## 3.3 Software Independence and Detectability

The architecture shall provide mechanisms through which an undetected software or system error cannot silently alter the election outcome without creating detectable evidence.

The system shall produce election records and verification evidence sufficient to support:

* detection of inconsistencies;
* independent checking of election records;
* investigation of anomalies;
* audit procedures; and
* verification of the reported outcome.

These principles are consistent with established voting-system security objectives emphasizing auditability, ballot secrecy, access control, data protection, software integrity, monitoring, and detection of outcome-changing errors.

---

## 3.4 Transparency

Election configuration and processing shall be sufficiently transparent to permit authorized stakeholders to understand:

* election configuration;
* ballot definitions;
* eligibility rules;
* election opening and closing times;
* system status;
* tally procedures;
* audit records;
* verification evidence; and
* published results.

Transparency shall not expose confidential voter information or secret ballot selections.

---

## 3.5 Separation of Duties

No single administrative user shall have unrestricted authority to independently:

1. configure an election;
2. modify the voter roll;
3. alter ballot definitions;
4. operate cryptographic key material;
5. decrypt ballots;
6. generate the final tally; and
7. publish the final result.

Privileged election operations shall be separated through role-based access control and, where appropriate, multi-person authorization.

---

# 4. Election Lifecycle Requirements

The platform shall support a controlled election lifecycle:

```text
Draft
  ↓
Configuration
  ↓
Review
  ↓
Dry Run / Testing
  ↓
Approval
  ↓
Election Opening
  ↓
Voting
  ↓
Election Closure
  ↓
Ballot Finalization
  ↓
Tally
  ↓
Verification
  ↓
Result Publication
  ↓
Audit / Certification
  ↓
Archive
```

Each lifecycle transition shall be recorded in an audit trail.

Critical configuration shall be locked once voting begins unless a formally controlled exception procedure is invoked.

---

# 5. Functional Requirements

## 5.1 Election Administration

The organizer dashboard shall provide authorized administrators with facilities to:

* create elections;
* define election dates and times;
* configure eligible voter populations;
* create ballot questions;
* configure candidate information;
* configure voting methods;
* configure election rules;
* define result publication rules;
* configure notifications;
* conduct dry runs;
* review election configuration;
* approve elections;
* open and close elections;
* monitor participation;
* initiate authorized tally procedures; and
* access audit and verification records.

---

## 5.2 Ballot Builder

The platform shall provide a configurable ballot-definition mechanism supporting election types appropriate to the deployment.

Subject to the selected election rules, the system may support:

* single-choice questions;
* multiple-choice questions;
* ranked-choice voting;
* preferential voting;
* candidate elections;
* referendum questions;
* write-in candidates;
* optional/required questions;
* candidate descriptions; and
* ballot instructions.

Ballot configuration shall be versioned and shall become immutable after election opening unless a controlled election-management procedure explicitly permits a change.

---

## 5.3 Voter-Roll Management

Authorized administrators shall be able to:

* import voter records securely;
* validate voter eligibility;
* update eligible voters before the appropriate election lock point;
* deactivate ineligible voters;
* manage voter groups;
* reconcile voter counts;
* detect duplicate voter records; and
* export appropriate administrative reports.

Voter identity data shall be logically separated from cast-ballot data.

All changes to voter eligibility records shall be auditable.

---

## 5.4 Dry-Run and Election Testing

Before an election becomes active, the platform shall provide a controlled test environment capable of verifying:

* ballot rendering;
* voter authentication;
* eligibility rules;
* voting workflow;
* duplicate-vote prevention;
* ballot encryption;
* ballot storage;
* tally logic;
* result generation;
* notification workflows;
* audit logging;
* accessibility;
* system capacity; and
* failure-recovery procedures.

Test data shall not be confused with production election records.

---

# 6. Voter-Facing Requirements

## 6.1 Responsive and Device-Agnostic Interface

The voter interface shall operate consistently on supported:

* desktop browsers;
* laptop browsers;
* tablets; and
* smartphones.

The responsive interface shall preserve the semantic meaning and ordering of ballot options and shall avoid visual layouts that could unintentionally favor one candidate or option.

---

## 6.2 Neutral Ballot Presentation

The ballot interface shall use a documented and consistently applied presentation strategy.

Where candidate or option ordering is randomized, the randomization mechanism shall be:

* deterministic only where required for verification;
* generated using an appropriate secure random mechanism;
* applied consistently according to election rules; and
* recorded as part of the election configuration.

If standardized ordering is required by election rules, that ordering shall be explicitly configured and auditable.

---

## 6.3 Voting Session

The system shall:

* issue a controlled voting authorization;
* prevent unauthorized reuse;
* prevent replay;
* expire unused authorization according to configured security policy;
* securely terminate or invalidate the voting authorization after successful ballot submission; and
* prevent a voter from submitting more than the permitted number of ballots.

Session termination alone shall not be considered the complete duplicate-vote prevention mechanism.

---

# 7. Authentication and Access Control

## 7.1 Voter Authentication

The system shall authenticate voters using mechanisms appropriate to the election's threat model and legal requirements.

Possible mechanisms include:

* secure credentials;
* one-time authentication codes;
* identity-provider integration;
* organization-managed authentication;
* hardware-backed authenticators; or
* other approved identity-verification mechanisms.

The selected authentication method shall not create an unnecessary persistent association between voter identity and ballot contents.

---

## 7.2 Administrator Authentication

Administrative and privileged accounts shall require strong authentication, including multi-factor authentication where applicable.

Administrative access shall follow:

* least privilege;
* role separation;
* secure session management;
* account lifecycle management;
* privileged-operation logging; and
* periodic access review.

A cryptographic hash shall not be treated as an authentication factor.

---

# 8. Ballot Security and Cryptographic Requirements

## 8.1 Encryption in Transit

All sensitive communications shall use modern, appropriately configured transport encryption.

Sensitive election data shall not be transmitted through unprotected channels.

---

## 8.2 Ballot Encryption

Where encrypted electronic ballots are used, ballot confidentiality shall be established before the ballot leaves the voter's trusted voting interface.

The cryptographic design shall use publicly documented, well-vetted cryptographic algorithms and libraries rather than proprietary or undocumented cryptographic mechanisms.

The project shall document:

* algorithms;
* key sizes;
* key lifecycle;
* encryption process;
* decryption process;
* key ownership;
* key storage;
* key recovery;
* key destruction; and
* cryptographic verification procedures.

---

## 8.3 Privacy-Preserving Ballot Architecture

The platform shall prevent authorized or unauthorized users from directly associating a voter's identity with the contents of that voter's ballot.

Depending on the selected election protocol, appropriate techniques may include:

* cryptographic separation;
* blind signatures;
* mixnets;
* homomorphic encryption;
* threshold cryptography;
* zero-knowledge proofs;
* cryptographic commitments; or
* other formally evaluated privacy-preserving mechanisms.

No particular cryptographic technique shall be considered mandatory unless selected by the project's formal threat model and protocol design.

---

## 8.4 Cryptographic Key Management

Cryptographic keys shall be subject to controlled lifecycle management covering:

* generation;
* secure storage;
* access control;
* backup/recovery where applicable;
* authorized use;
* rotation where applicable;
* revocation;
* destruction; and
* audit.

For high-risk election operations, the system should support multi-person or threshold authorization so that no single administrator possesses unrestricted election-decryption authority.

---

# 9. End-to-End Verifiability

The system shall provide an appropriate level of election verifiability.

## 9.1 Individual Verification

Where supported by the selected voting protocol, voters shall be able to verify that their ballot has been successfully recorded without receiving transferable evidence that reveals or proves their selections to another party.

The verification mechanism shall therefore avoid creating a receipt that can be used for coercion or vote-buying.

---

## 9.2 Universal Verification

Authorized auditors and independent observers shall be able to verify, to the extent supported by the selected cryptographic protocol:

* that recorded ballots satisfy required integrity checks;
* that ballots included in the tally correspond to valid election records;
* that the tally was calculated according to the configured election rules; and
* that the published result is supported by the available election evidence.

---

# 10. Tallying and Result Generation

The platform shall not rely solely on a single administrative action such as "decrypt and publish."

The controlled tally process shall include:

```text
Election Closure
      ↓
Freeze Election State
      ↓
Validate Election Records
      ↓
Generate Tally Input
      ↓
Authorized Cryptographic Operation
      ↓
Tally Calculation
      ↓
Verification
      ↓
Audit Evidence Generation
      ↓
Result Approval
      ↓
Result Publication
```

The exact cryptographic procedure shall depend on the selected voting protocol.

The system shall retain sufficient evidence to reproduce or independently verify the reported result without exposing secret ballot selections.

---

# 11. Audit and Logging Requirements

The platform shall maintain tamper-evident records for security- and election-critical operations.

Audit records shall include, where appropriate:

* administrator authentication;
* privileged actions;
* election configuration changes;
* voter-roll changes;
* election state transitions;
* system security events;
* cryptographic operations;
* ballot-processing events;
* tally operations;
* result publication;
* configuration version changes; and
* incident-response activities.

Audit records shall be protected against unauthorized modification and deletion.

A private blockchain shall not be required merely to satisfy auditability requirements. Append-only, cryptographically protected, access-controlled audit mechanisms may be used where they provide the required security properties.

---

# 12. Turnout and Reporting

The system shall provide authorized election officials with aggregate participation information.

Possible metrics include:

* eligible voter count;
* authenticated voter count;
* ballots successfully cast;
* participation percentage;
* voting-period activity;
* rejected or invalid voting attempts; and
* system availability.

Turnout information shall not expose interim vote totals or individual ballot choices.

Where small populations could make individual participation or voting behavior inferable, appropriate suppression or aggregation rules shall be applied.

---

# 13. Accessibility Requirements

The voter-facing system shall target WCAG 2.2 Level AA.

Accessibility testing shall cover:

* keyboard-only operation;
* screen readers;
* zoom and text resizing;
* focus management;
* form validation;
* error messages;
* accessible authentication;
* ballot navigation;
* candidate/option selection;
* review screens;
* confirmation screens; and
* responsive mobile presentation.

Accessibility shall be tested throughout development rather than only immediately before deployment.

---

# 14. Security Architecture

The platform shall implement defense-in-depth controls including:

### Application Security

* secure coding practices;
* input validation;
* output encoding;
* CSRF protection;
* secure session management;
* rate limiting;
* abuse prevention;
* dependency management;
* vulnerability scanning;
* secure API design; and
* penetration testing.

### Infrastructure Security

* network segmentation;
* firewalls;
* WAF;
* DDoS protection;
* secure secrets management;
* encrypted storage;
* infrastructure monitoring;
* hardened operating environments;
* backup systems; and
* disaster-recovery capabilities.

### Identity Security

* MFA for privileged users;
* RBAC;
* least privilege;
* privileged-access monitoring;
* session controls;
* credential lifecycle management; and
* periodic access review.

---

# 15. Managed Infrastructure and SaaS Requirements

As a fully managed service, the platform provider shall be responsible for the agreed infrastructure and operational controls.

## 15.1 Availability and Scalability

The infrastructure shall support:

* horizontal scaling;
* load balancing;
* capacity monitoring;
* peak-election traffic;
* automated or controlled scaling;
* redundancy;
* health monitoring; and
* graceful failure handling.

Capacity planning shall specifically consider voting-period peaks, including the beginning and end of the election window.

---

## 15.2 DDoS and Network Protection

The production environment shall employ appropriate:

* DDoS mitigation;
* web application firewall;
* rate limiting;
* network segmentation;
* traffic monitoring;
* threat detection; and
* incident-response mechanisms.

---

## 15.3 Monitoring

The provider shall monitor:

* infrastructure health;
* application health;
* authentication anomalies;
* unusual voting activity;
* security events;
* system performance;
* failed requests;
* service availability; and
* critical election workflows.

Monitoring shall not expose voter choices or compromise ballot secrecy.

---

# 16. Backup and Disaster Recovery

The platform shall maintain controlled backup and disaster-recovery capabilities.

The disaster-recovery plan shall define:

* Recovery Time Objective (RTO);
* Recovery Point Objective (RPO);
* backup frequency;
* backup retention;
* backup encryption;
* backup integrity verification;
* geographically separated or otherwise resilient storage where appropriate;
* recovery procedures;
* recovery testing; and
* responsibilities during an election incident.

Election-state recovery shall be designed differently from ordinary business-application database recovery because restoration must preserve election integrity and auditability.

---

# 17. Incident Response

The managed service shall maintain a documented incident-response process covering:

1. detection;
2. classification;
3. containment;
4. investigation;
5. evidence preservation;
6. stakeholder notification;
7. election-impact assessment;
8. recovery;
9. post-incident analysis; and
10. corrective action.

Security incidents affecting election integrity, voter privacy, availability, or auditability shall be escalated according to predefined severity levels.

---

# 18. Testing and Independent Assessment

Before production deployment, the system shall undergo multiple classes of testing.

## 18.1 Functional Testing

Testing shall verify:

* election configuration;
* voter registration/import;
* authentication;
* ballot presentation;
* ballot submission;
* duplicate-vote prevention;
* tallying;
* result publication;
* notifications; and
* reporting.

## 18.2 Security Testing

Testing shall include, as appropriate:

* vulnerability assessment;
* penetration testing;
* authentication testing;
* authorization testing;
* API security testing;
* cryptographic review;
* session-security testing;
* privilege-escalation testing;
* infrastructure security testing;
* DDoS resilience testing; and
* audit-log integrity testing.

## 18.3 Accessibility Testing

The system shall be evaluated against the selected WCAG 2.2 Level AA requirements.

## 18.4 Load and Resilience Testing

The platform shall be tested under:

* expected voter loads;
* peak voting loads;
* authentication spikes;
* ballot-submission spikes;
* infrastructure failures;
* network failures; and
* recovery scenarios.

## 18.5 Election-Specific Verification

The election protocol and tally process shall be independently reviewed where the risk and deployment context justify such assessment.

---

# 19. Internet Voting Risk and System Limitations

The platform shall explicitly recognize that secure software engineering does not eliminate all risks associated with remote Internet ballot return.

CISA and partner agencies have characterized electronic ballot return as a **high-risk activity**, citing risks to ballot integrity, voter privacy, and system availability. The risk is increased by the use of voters' own devices, which may themselves be compromised, and by the ability of electronic attacks to operate at scale.

Consequently, the system shall document:

* the assumed threat model;
* trusted and untrusted components;
* voter-device assumptions;
* network assumptions;
* administrator trust assumptions;
* cloud-provider trust assumptions;
* cryptographic assumptions;
* residual risks;
* mitigations; and
* circumstances under which electronic voting should not be used.

The platform shall not claim that encryption, blockchain, MFA, or cloud security alone makes remote Internet voting completely secure.

---

# 20. Compliance Classification

Each requirement shall be classified as one of the following:

| Classification                     | Meaning                                                                               |
| ---------------------------------- | ------------------------------------------------------------------------------------- |
| **Mandatory Standard Requirement** | Requirement directly applicable under the selected governing standard or jurisdiction |
| **Project Requirement**            | Requirement defined by the project's security, functional, or operational objectives  |
| **Implementation Choice**          | One possible technical method for satisfying a higher-level requirement               |
| **Operational Control**            | Control required for reliable managed-service operation                               |
| **Recommended Control**            | Additional control adopted to reduce identified risk                                  |
| **Not Applicable**                 | Requirement determined not to apply to the deployment, with documented justification  |

This classification shall prevent an implementation technique from being incorrectly presented as a universal standard requirement.

---

# 21. Standards-to-Requirements Compliance Matrix

| Area                   | Reference Framework                        | Project Requirement                                                                    | Compliance Evidence                    |
| ---------------------- | ------------------------------------------ | -------------------------------------------------------------------------------------- | -------------------------------------- |
| E-voting principles    | Council of Europe CM/Rec(2017)5            | Democratic-election principles, secrecy, transparency, accessibility and verifiability | Requirements traceability + assessment |
| Voting-system security | VVSG 2.0                                   | Security, integrity, usability, accessibility and auditability benchmarks              | Security and usability test reports    |
| Information security   | ISO/IEC 27001:2022                         | Risk-based ISMS and security controls                                                  | ISMS documentation / audit             |
| Cloud PII protection   | ISO/IEC 27018:2025                         | Controlled processing and protection of voter PII in public cloud                      | Cloud-security assessment              |
| Accessibility          | WCAG 2.2 Level AA                          | Accessible voter-facing application                                                    | Accessibility test report              |
| Authentication         | Project security requirements              | Secure voter and administrator authentication                                          | Authentication/security tests          |
| Access control         | ISO/IEC 27001 / voting security principles | Least privilege and separation of duties                                               | RBAC review + audit evidence           |
| Ballot secrecy         | CoE / voting-security principles           | Identity and ballot separation                                                         | Architecture + protocol assessment     |
| Software integrity     | VVSG/NIST principles                       | Protection against unauthorized software/configuration modification                    | Integrity controls + security testing  |
| Auditability           | VVSG/NIST principles                       | Tamper-evident election records                                                        | Audit-log verification                 |
| Verifiability          | E-voting protocol                          | Individual/universal verification where supported                                      | Cryptographic verification evidence    |
| Availability           | ISO/IEC 27001 / operational requirements   | HA, scaling, DDoS protection and DR                                                    | Load/DR testing                        |
| Incident response      | ISO/IEC 27001                              | Documented security incident process                                                   | Incident-response plan                 |
| Disaster recovery      | ISO/IEC 27001                              | Defined RTO/RPO and tested recovery                                                    | DR test report                         |
| Internet voting risk   | CISA/EAC/FBI/NIST guidance                 | Explicit residual-risk assessment                                                      | Threat/risk assessment                 |

---

# 22. Compliance Statement

The proposed platform shall be developed using the above standards and frameworks as **reference and design baselines**.

Compliance shall be demonstrated through documented requirements, architecture decisions, implementation evidence, testing results, audit records, and—where legally or contractually required—independent assessment or certification.

The system shall not claim formal compliance, certification, or legal authorization solely because it implements selected technical controls from a referenced standard.

In particular:

* **CM/Rec(2017)5** shall guide e-voting-specific requirements;
* **VVSG 2.0** shall provide an additional voting-system engineering benchmark where relevant;
* **ISO/IEC 27001:2022** shall guide information-security governance and risk management;
* **ISO/IEC 27018:2025** shall guide protection of PII in applicable public-cloud processing;
* **WCAG 2.2 Level AA** shall define the project's target accessibility level; and
* applicable national and local electoral legislation shall take precedence wherever legally binding requirements differ from technical reference frameworks.

The final system shall maintain a **requirements traceability matrix** linking each requirement to its source, implementation component, test case, evidence, and compliance status.
