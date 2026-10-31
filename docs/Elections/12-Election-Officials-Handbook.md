# Election Officials Handbook

Complete operating guide for every official who works an election on the association's platform.

# Document Control

| Item | Entry |
|---|---|
| Document owner | Election Commission |
| Approval authority | Executive Committee |
| Version | 2.0 draft |
| Date | 2026-10-11 |
| Review cycle | Every election |
| Applies to | One election, from Initiation to Post Election Review |
| Status | Draft. Open points are marked `to confirm` and listed in Annex D. |

Approval

| Name | Post | Signature | Date |
|---|---|---|---|
| | Chief Election Commissioner | | |
| | Returning Officer | | |
| | Technical Administrator | | |

# Table of Contents

1. How to use this handbook
2. Glossary
3. The election at a glance
4. Where each standard applies and why
5. How the election is secured
6. Rules every official follows
7. Posts and permissions
8. Working in the system: screens and routes
9. Phase by phase dry run
10. Incident procedures
11. Quick reference
12. Checklists with signature boxes
13. Reference implementation

Annex A. Documents to print and attach
Annex B. Forms the system fills and forms printed blank
Annex C. Conflicts between source documents
Annex D. Open points

---

# 1. How to use this handbook

This handbook stands alone. An official can print it, read it from start to end, and run their part of
an election from it. The source documents are named wherever the handbook leans on them, and Annex A
says which ones to print and attach.

## How each phase is written

Every phase in section 9 has the same parts, in the same order.

| Part | Meaning |
|---|---|
| Who | The posts that act in this phase |
| From where | The screen, route or paper form used |
| After what | What must be finished before this phase starts |
| Steps | Numbered actions, in order |
| Output | What exists when the phase is done |
| Form | The form or certificate that records it |
| Basis | The regulation article and the standard that the phase rests on, and why |

## Reading order

| Reader | Read |
|---|---|
| Any official | Sections 1 to 8, then the phases that touch the post |
| Commission | Everything |
| Returning Officer | Everything, and `11-Election-Key-Procedure.md` in full |
| Technical Administrator | Sections 5, 8, 9 (phases 3, 7, 9), 10 |
| Observer or auditor | Sections 4, 5, 6 and the phase dry run |

## Writing conventions

- A step that needs a second person says so.
- `to confirm` means the source documents do not settle the point. It is never a guess. Annex D lists
  every one.
- Names in code font are exact names in the system.

---

# 2. Glossary

| Term | Meaning |
|---|---|
| Appointment | A post held by one member for one election. It is live only after the appointee accepts and signs the neutrality declaration. |
| Approval | A second official agreeing to a request. See section 6. |
| Audit history | The record of every action taken on the election. Each entry is chained to the one before, so a change to an old entry shows. |
| Ballot key | A key pair made for one election. The public half seals ballots. The private half opens them at the count. |
| Fingerprint | The SHA-256 digest of the public ballot key. Voters and officials compare it to check the same key is in use. |
| Freeze | The period in which election rules cannot be changed. See section 6. |
| Key file | The private half of the ballot key, saved as a `.pem` file. |
| Persona | A post together with its permission set. |
| Phase | A system stage of an election, for example Nominations open or Voting open. |
| Recount window | The period after the declare in which a recount can be requested. The Commission sets its end date for each election. |
| Returning Officer | The official who holds the key file and runs the count. |
| Sealed ballot | A ballot encrypted so that nobody can read it without the key file. |
| Step-up check | A second identity check before a high-risk action. |
| SuperAdmin | The protected system administrator. It has no election permission by default beyond the rules unlock. |

---

# 3. The election at a glance

## System phases

The system moves an election through ten phases, in this order.

| No | System phase | Label shown on screen |
|---|---|---|
| 1 | Announced | Announced |
| 2 | Nomination | Nominations open |
| 3 | Scrutiny | Under scrutiny |
| 4 | Withdrawal | Withdrawal open |
| 5 | CandidateList | Candidate list |
| 6 | Campaign | Campaign |
| 7 | Polling | Voting open |
| 8 | Counting | Counting |
| 9 | Declared | Declared |
| 10 | Archived | Archived |

## Manual phases against system phases

The Operational Manual (`02-Election-Operational-Manual.md`) has fourteen working phases plus a review.
They do not map one to one onto the system phases, because the manual includes work done outside the
system.

| Manual phase | Manual section | System phase when it happens | Regulation article |
|---|---|---|---|
| 1 Initiation | 4 | Before the election exists in the system | 4, 5 |
| 2 Commission Formation | 5 | Before the election exists, appointments recorded when it does | 6, 7 |
| 3 Planning | 6 | Announced | 5 |
| 4 Voter Roll | 7 | Announced to Nomination | 8 |
| 5 Nomination | 8 | Nomination, Scrutiny, Withdrawal, Candidate list | 9 |
| 6 Campaign | 9 | Campaign | 10 |
| 7 Poll Preparation | 10 | Campaign, ends at the open-polling request | 11, 12 |
| 8 Poll Opening | 11 | Voting open | 11, 12 |
| 9 Poll Management | 12 | Voting open | 11, 12 |
| 10 Poll Closing | 13 | Voting open to Counting | 13 |
| 11 Counting | 14 | Counting | 14 |
| 12 Result Certification | 15 | Declared | 15 |
| 13 Appeals | 16 | Declared | 16 |
| 14 Transition and Handover | 17 | Declared to Archived | 17 |
| Post Election Review | 18 | Archived | 17 |

A phase advances only when an official with `ChangePhase` moves it (`POST /api/elections/{id}/phase`,
step-up check required). The calendar dates entered at setup are the published plan. They do not move the
election by themselves. Publish, Open polling, Close polling and Archive go through the approval rule in
section 5. Declare has its own permission, `Declare`.

## The timeline in the regulations

Regulation Article 5 sets the days from the notice to the declare. The regulation text is the source.
This handbook does not copy the day counts, so the two cannot drift. Print `01-Election-Regulations.md`
Article 5 with this handbook (Annex A).

---

# 4. Where each standard applies and why

The standards are chosen in `00-election-standards-requirements.md`. That document says the platform
must not claim legal certification from selected controls alone, and this handbook does not claim it
either. The table says where each standard shows up in an official's work and why it was chosen.

## Standards

| Standard | Where it applies in this handbook | Why |
|---|---|---|
| Council of Europe CM/Rec(2017)5 (standards document section 2.1) | Sealing, key custody, transparency of the audit history, the limits stated in section 5 | It is the main reference for electronic voting. It asks for secrecy, one voter one vote, and a system that can be checked. |
| VVSG 2.0 (section 2.2) | The count integrity check, the minimum ballot count, accessibility and audit expectations | A benchmark for voting system quality. It is not law and the platform does not claim certification against it. |
| ISO/IEC 27001:2022 (section 2.3) | Access control, logging, backup, incident procedures A to E | The recognised way to run information security. The compliance status is Gap, so it is a target and not a claim. |
| ISO/IEC 27018:2025 (section 2.4) | Handling of member data in the cloud, retention of logs | Protects personal data held on a managed service. Status is Gap to confirm. |
| WCAG 2.2 level AA (section 2.5) | Voter screens, the accessibility checklist `10-Election-Accessibility-Checklist.md` | A project choice. Every member must be able to vote. The manual accessibility run is open (TODO 37.13g). |

## Principles

| Principle | Where it applies | Why |
|---|---|---|
| Secrecy (section 3.1) | Sealing at casting, no voter id kept on a stored ballot after the count, no results during polling | A vote must not be traced to a voter. |
| Eligibility, one voter one ballot (3.2) | Voter roll phase, locked roll, the voted flag, the count integrity check | Only members on the roll vote, and only once. |
| Software independence and detectability (3.3) | Count integrity check, minimum ballot count, hand count when fewer than 10 ballots | A fault in the software should be noticed and should not decide the result alone. |
| Transparency (3.4) | Audit history, published fingerprint, observers, the public results page | People who doubt the result can check what the system recorded. |
| Separation of duties (3.5) | Two-person rule, key custodian is not the count approver, nobody counts or judges who is a candidate | No single official can change the outcome. |

## Standards document sections an official may be asked about

| Topic | Standards document section | Used in |
|---|---|---|
| Election lifecycle | 4 | Section 3 |
| Administration, ballot builder, voter roll, dry run | 5.1 to 5.4 | Phases 3, 4, 7 |
| Voter-facing screens | 6 | Phases 8, 9 |
| Authentication of voters and administrators | 7.1, 7.2 | Phases 8, 9 |
| Transit, ballot encryption, privacy architecture, key management | 8.1 to 8.4 | Section 5, phase 7, phase 11 |
| End-to-end verifiability | 9 | Section 5 limits |
| Tallying | 10 | Phase 11 |
| Audit and logging | 11 | Section 5, all phases |
| Turnout and reporting | 12 | Phases 9, 12 |
| Accessibility | 13 | Phase 7 |
| Security architecture, managed infrastructure | 14, 15 | Phase 7, role of Technical Administrator |
| Backup and disaster recovery | 16 | Phase 9, procedure D |
| Incident response | 17 | Section 10 |
| Testing | 18 | Phase 7 |
| Internet voting risk and limits | 19 | Section 5 limits |
| Compliance classification, matrix, statement | 20 to 22 | Annex D |

What the bodies of sections 8 and 10 to 18 ask for, and what an official does about each:

| Section | What it requires | What the official does |
|---|---|---|
| 8.1 to 8.2 | Transport encryption. Ballot sealed with public, well-vetted algorithms. The algorithms, key sizes and key lifecycle are written down. | Nothing at the screen. Section 5 and `11-Election-Key-Procedure.md` hold the written design. |
| 8.3 | A user must not be able to link a voter to a ballot. No technique is mandatory. | Know the limit: the server sees the ballot before it seals it (section 5, "What is not protected"). |
| 8.4 | Key lifecycle control. No single administrator should hold unrestricted decryption power. | The count needs a second official's approval. The key is destroyed after the recount window. |
| 10 | The tally is a controlled sequence, not one "decrypt and publish" click. The evidence lets a reviewer verify the result without exposing choices. | Phase 11. Close, freeze, check, count, verify, approve, publish, each as a separate step. |
| 11 | Tamper-evident records of privileged actions, state changes, tally and publication. Records cannot be edited or deleted. | Every action is in the audit chain. Report anything that looks missing. |
| 12 | Aggregate turnout for officials. No interim totals. Suppress small groups that could reveal a choice. | Phases 9 and 12. Read turnout only, never running totals. |
| 13 | WCAG 2.2 Level AA, tested throughout. | The manual accessibility run is tracked in TODO 37.13g. |
| 14 | Defence in depth: MFA for privileged users, RBAC, least privilege, rate limits, monitoring. | An official uses only that official's own sign-in. The step-up code is the MFA step for sensitive actions. |
| 15 | Capacity for the start and end of polling. Monitoring that never exposes choices. | The Technical Administrator watches health during polling. |
| 16 | Backup and recovery with stated RTO and RPO. Recovery must keep election integrity. | Procedure D. A polling-day backup is as sensitive as the key. |
| 17 | A documented incident process with severity levels. | Section 10. Three levels, L1 to L3. |
| 18 | Functional, security, accessibility and load testing before production. | Phase 7 dry run. |

---

# 5. How the election is secured

## Threat model in plain words

The system is a trusted-server secret ballot. It is not end-to-end verifiable. Voters trust the server
at the moment of casting. After that, the design limits what anybody can do. The full statement is in
`09-Election-Security-and-Compliance.md`.

## Protections

| Protection | How it works | Standard it serves |
|---|---|---|
| Sealing | Each ballot gets a one-off AES-256-GCM key. That key is wrapped with RSA-OAEP SHA-256 under a 3072-bit RSA public key. The server seals at casting, over TLS. | CM/Rec(2017)5, section 3.1 |
| Binding to the election | The seal carries the election id as associated data, so a ballot cannot be moved into another election. | 3.3 |
| Padding | Ballot text is padded to multiples of 512 bytes so the length does not show the choice. | 3.1 |
| Key custody | The Returning Officer holds the only key file, on a device the Returning Officer controls. The server cannot recover it. | 3.5, section 8.4 |
| No voter link after the count | The voter id is removed from stored ballots after the count, by shuffle and vacuum. Only the vote date stays. | 3.1 |
| Count integrity check | The count compares ballots, receipts and voted flags. A mismatch stops the count. | 3.2, 3.3 |
| Minimum ballot count | The count is refused under 10 ballots (setting `Elections.MinimumBallotsToCount`). A small count could reveal a single vote. | 3.1 |
| Two-person approval | Sensitive requests need a second official with `Approve`. It is a setting, see below. | 3.5 |
| Rules freeze | Settings, posts and the ballot key cannot change from the open-polling request until the count ends. | 3.4, 3.5 |
| Audit chain | Every action is written to a chained history. Refusals are written too. No key and no ballot body is written to any log. | 3.4, section 11 |
| Official access ends | Access ends 21 days after the declare by default, and archiving ends every appointment at once. | 3.5, ISO 27001 |

## The two-person rule is a setting

An administrator can set each approval action to need one person or two. The actions are Publish, Open
polling, Replace the ballot key, Close polling, Declare results, Archive and Count the ballots. Emergency
revoke always needs two people and cannot be changed. The screen label for it is "Remove an official in an
emergency".

The shipped default has every one of those seven actions on two people. An administrator can switch an
action to one person in `/admin/org-config`. The steps below say "a second official approves" and assume
the default. Check the setting before the election, because the committee may have changed it.

Two related settings, both with safe defaults:

| Setting | Default | Meaning |
|---|---|---|
| `SuperAdminActsAlone` | Off | When on, a SuperAdmin can act without a second person. An emergency switch only. |
| `ApprovalExpiryHours` | 48 | A request nobody approves lapses after this time. |

## Freeze and unlock

- The freeze starts when an open-polling request is pending. It lasts through Polling and Counting.
- While frozen, changes to election settings, posts, new appointments, plain revokes and the ballot key
  are refused with the code `ELECTION_RULES_FROZEN`. Each refusal is written to the audit history.
- A SuperAdmin can open a rules unlock. It needs a written reason of 20 to 1000 characters. It lasts 30
  minutes by default and 60 at most. It can be closed early. It never covers a plain revoke.
- An emergency revoke is the only way to remove an official while the rules are frozen. The requester
  and the person removed cannot approve. A SuperAdmin cannot approve in their place. A request nobody
  acts on expires, and a sweep that runs every 15 minutes records it.

## When not to use online voting

The Commission records in its minutes that online voting is not used if any of these holds:

- the hosting is not secure enough for a secret ballot;
- the key ceremony failed;
- the voter roll cannot be frozen;
- too many voters lack digital access.

The last is the Commission's judgement. Examples are a large share of members without a smartphone or
reliable data, or a past election where many members asked for paper. There is no fixed threshold. The
manual ballot in `04-Election-Manual-Ballot.md` applies instead.

## What is not protected

The Returning Officer should say these plainly to the committee.

| Not protected | What it means |
|---|---|
| A compromised server during polling | The server sees the ballot before it seals it. |
| A voter proving their own choice | There is no receipt that shows the choice. |
| A voter's compromised device | The device can change what the voter sees. |
| A key holder with a backup | Anyone with the key file and a backup taken during polling can link voters to choices. There is no threshold custody. |

## Where the key lives

The original is on a device the Returning Officer controls, behind a screen lock. The Association sets how many sealed copies exist (default 2, at least 1). The Chief Commissioner names the holders from the appointed officials when the key is set. Each holder must be a different person and not a candidate, and each confirms receipt. The app does not yet hold the holders or the confirmations (TODO 37.14a), so the Chief Commissioner logs them by hand. Owner decision 2026-10-27: polling remains blocked until the recorded copy count matches the configured number and every holder confirms receipt. A copy is kept in a sealed envelope with a log entry, and every copy is destroyed together with the original once the recount window has closed. If every copy is lost, that is a level 3 incident: the election is void and is run again. Never in email, chat, a shared drive, a repository or a ticket. The person
who approves the count never sees the key. The full procedure is `11-Election-Key-Procedure.md`.

---

# 6. Rules every official follows

1. **Two people for every sensitive step.** The requester cannot approve their own request. For an
   emergency revoke the person being removed cannot approve either.
2. **Nobody counting or judging may be a candidate**, or be tied to one as a proposer or seconder.
3. **A post is live only after acceptance.** The appointee accepts and signs the neutrality declaration.
4. **One live Returning Officer per election.** Appointing a new one expires the earlier appointment.
5. **No results or running totals during polling.** Do not read them out or guess them.
6. **Never handle the key file** unless the post is Returning Officer. Never ask for it, copy it or store
   it.
7. **Record any unusual event at once** in the Incident Register.
8. **Every action is written to the audit history.** Work as if each click is signed.
9. **Member data stays inside the system.** Do not export or forward the voter roll.
10. **A user who holds election records cannot be deleted.** The system keeps the record and the user is
    deactivated.
11. **Declare a conflict.** An official who is a close relative (spouse, parent, child, sibling) of a
    candidate, or who proposed or seconded one, declares it at appointment. That official stays out of
    that candidate's scrutiny, appeals and recount decisions. An official cannot also be a candidate in
    the same election.

## Approving or rejecting a request

1. Open the officials dashboard at `/officials/elections/:id`.
2. Find the approvals card. Pending requests that need a decision appear there.
3. Read the action name and who asked.
4. Choose Approve, or Reject. Reject asks for confirmation first. A rejected request does not happen.
5. On mobile use the approvals screen. The wording is the same.

An official cannot decide a request they asked for. The screen says so.

The web approvals card on the officials dashboard shows an Approve and a Reject button for each pending request.

---

# 7. Posts and permissions

An official is a member appointed to a post for one election. The post decides which actions the system
allows. The default sets are seeded from `GHCAA.Infrastructure/Data/ElectionPersonaSeeder.cs`. An
administrator can change them at `/admin/election-personas` before the rules freeze.

## Permissions

| Permission | Value | What it allows |
|---|---|---|
| ViewDashboard | 1 | Open the election dashboard |
| ViewAudit | 2 | Read the audit history |
| ManageSetup | 4 | Change election setup |
| ManageVoterRoll | 8 | Manage the voter roll |
| DecideNominations | 16 | Accept or reject nominations |
| AppointOfficials | 32 | Appoint and revoke officials |
| SetBallotKey | 64 | Make or replace the ballot key, record its destruction |
| ChangePhase | 128 | Move the election to the next phase |
| Count | 256 | Run an approved count |
| Declare | 512 | Declare the result |
| DecideAppeals | 1024 | Decide appeals |
| Approve | 2048 | Approve or reject another official's request |

## Default permissions per post

| Post | Group | Permissions |
|---|---|---|
| ECSC Member | Search Committee | ViewDashboard |
| Chief Commissioner | Election Commission | ViewDashboard, ViewAudit, ManageSetup, ManageVoterRoll, DecideNominations, AppointOfficials, ChangePhase, Count, Declare, Approve (every permission except SetBallotKey and DecideAppeals) |
| Commissioner | Election Commission | ViewDashboard, ViewAudit, ManageSetup, ManageVoterRoll, DecideNominations, AppointOfficials, ChangePhase, Declare, Approve |
| Returning Officer | Officials | ViewDashboard, ViewAudit, DecideNominations, SetBallotKey, Count, Approve |
| Assistant Returning Officer | Officials | ViewDashboard, DecideNominations |
| Presiding Officer | Officials | ViewDashboard |
| Polling Officer | Officials | ViewDashboard |
| Scrutineer | Officials | ViewDashboard, DecideNominations |
| Counting Supervisor | Officials | ViewDashboard, Count |
| Technical Administrator | Officials | ViewDashboard, ViewAudit |
| Cybersecurity Auditor | Officials | ViewDashboard, ViewAudit |
| Security Officer | Officials | ViewDashboard (not offered by default) |
| Observer | Observers | ViewDashboard, ViewAudit |
| Appeal Tribunal Member | Appeal Tribunal | ViewDashboard, ViewAudit, DecideAppeals |

The Chief Commissioner never holds the ballot key and never decides appeals. That keeps the Commission
leader apart from the key and from the Tribunal (separation of duties, standards 3.5). The set was read
from `ElectionPersonaSeeder.cs` on 2026-10-11. An administrator can change it, so check
`/admin/election-personas`.

## Duties by post

| Post | Duty in one line | Phases |
|---|---|---|
| ECSC Member | Forms the Commission and screens its members | 2 |
| Chief Commissioner | Leads the election, signs notices and certificates, hears urgent disputes | All |
| Commissioner | Runs setup, voter roll, nominations, publish, phase changes, declare | All |
| Returning Officer | Manages nominations, holds the key, runs the count | 5, 7, 11 |
| Assistant Returning Officer, Scrutineer | Check nominations | 5 |
| Presiding Officer, Polling Officer | Run a polling station | 8, 9, 10 |
| Counting Supervisor | Oversees the count | 11 |
| Technical Administrator | Runs the voting system | 3, 7, 9, 10 |
| Cybersecurity Auditor, Security Officer | Test and guard | 7, 9, 10 |
| Observer | Watches without taking part | 8 to 12 |
| Appeal Tribunal Member | Decides appeals | 13 |

---

# 8. Working in the system: screens and routes

## Where each person works

| Area | Route | Who |
|---|---|---|
| Public election list | `/elections` | Anyone |
| Public results | `/elections/results` | Anyone, after the declare |
| Member voting | `/portal/election` | Members on the roll |
| Officials dashboard | `/officials` | Any appointed official |
| My appointments | `/officials/my-appointments` | Any appointed official |
| Election dashboard | `/officials/elections/:id` | Officials with `ViewDashboard` |
| Admin elections | `/admin/elections` | Administrators and the Chief Commissioner |
| Personas | `/admin/election-personas` | Administrators |
| Organisation settings | `/admin/org-config` | Administrators |

On mobile the officials area is `/officials` and `/officials/appointments`. An official who is not also
an administrator is sent to the officials area after login.

## Web and mobile differences

| Action | Web | Mobile |
|---|---|---|
| Approve or reject a request | Approvals card | Approvals screen |
| Make the ballot key | Admin elections screen | Not available |
| Record that the key file is destroyed | Officials dashboard | Not available |
| Appoint an official | Officials panel, member search for administrators | Officials panel, name and email only |

## Admin election actions

The admin elections controller offers these actions: create an election, publish, set the ballot key,
close, add a candidate and remove a candidate. Each sensitive action goes through the approval rule in
section 5.

---

# 9. Phase by phase dry run

Each phase below says who acts, from where, after what, the steps, and the result. Steps that need a
second person say so. Phase numbers are the manual's, from `02-Election-Operational-Manual.md`.

## Phase 1. Initiation

| Part | Entry |
|---|---|
| Who | Executive Committee, Chief Election Commissioner |
| From where | Paper. The election does not exist in the system yet. |
| After what | A decision to hold an election under Article V of the Constitution |
| Basis | Regulations Articles 4 and 5. Separation of duties (standards 3.5): the body that calls the election is not the body that runs it. |

Steps

1. Executive Committee approves the election in a resolution.
2. Executive Committee approves the budget.
3. Executive Committee notifies the Election Commission Selection Committee (ECSC).
4. Executive Committee provides the member database and a financial clearance.
5. Executive Committee freezes major policy decisions if the Constitution requires it.

Output: Election Resolution, Budget, Member Register, Financial Clearance.

## Phase 2. Commission formation

| Part | Entry |
|---|---|
| Who | ECSC members, Chief Election Commissioner, appointees |
| From where | Paper for the selection. Officials panel for the appointments. |
| After what | Phase 1 deliverables |
| Basis | Regulations Articles 6 and 7. Separation of duties (3.5). Neutrality is signed because a biased official breaks eligibility and secrecy trust (3.1, 3.2). |

Steps

1. The ECSC receives nominations for Commissioners.
2. The ECSC verifies eligibility and interviews nominees if needed.
3. The ECSC approves the Commissioners and issues appointment letters.
4. Each appointee accepts in writing (ER-04 Commissioner Acceptance) and signs the neutrality declaration
   (ER-05) and the conflict disclosure (ER-06). The ECSC chair and one member sign the appointment (ER-03).
5. The Commission holds its first meeting and assigns the posts.
6. An administrator or Commissioner with `AppointOfficials` opens the officials panel (web `/admin/elections`,
   mobile `/officials`).
7. Pick an existing member, or give a name and email. Member search is for administrators only. On mobile
   an official gives a name and email.
8. Choose the post. Save the appointment.
9. The appointee accepts in the system and signs the neutrality declaration. The post is live only then.
10. For the Returning Officer post, tick the Returning Officer flag. Appointing a new Returning Officer
    expires the earlier appointment.

Dry run check: a candidate or a person tied to a candidate cannot hold a post that judges or counts.

Output: Commission formed, roles assigned, calendar approved, live appointments.

## Phase 3. Planning

| Part | Entry |
|---|---|
| Who | Commission, Technical Administrator |
| From where | Commission meeting No. 1 on paper. `/admin/elections` to create the election. |
| After what | Commission formed |
| Basis | Regulations Article 5. Lifecycle (standards 4). |

Steps

1. The Commission prepares the Election Calendar, Communication Plan, Risk Register, Polling Plan,
   Technical Plan, Cybersecurity Plan, Budget, Inspection Schedule, Observation Schedule, Emergency
   Response Plan, Communication Matrix, Document Register and Version Register.
2. At Commission Meeting No. 1 approve the timeline, responsibilities, forms, registers, inspection
   schedule, security plan, technical team, ballot format and symbol list. Record the minutes.
3. The Technical Administrator opens `/admin/elections` and creates the election.
4. Enter the calendar dates in phase order: nominations, scrutiny, withdrawals, polling. The Election
   Notice (ER-01) and Election Calendar (ER-02) also carry the campaign opening and closing, the silence
   period, the polling venue, the notice reference number and the recount window end. Until the app
   stores these (TODO 37.14m, stored fields for the election forms), write them on the printed forms.
5. Pick the tie rule the Constitution names: drawing lots, a run-off, or the Returning Officer's casting
   vote. The rule decides a tie for the last place on a seat.
6. Check the posts and the dates once more. They cannot change after the rules freeze.

Output: election exists in the Announced phase, plans approved.

## Phase 4. Voter roll

| Part | Entry |
|---|---|
| Who | Commissioner with `ManageVoterRoll` |
| From where | `/officials/elections/:id` voter roll section |
| After what | Election exists, membership database received |
| Basis | Regulations Article 8. One voter one ballot (3.2): the roll is the only list of who may vote. |

Steps

1. Receive the membership database.
2. Validate membership and remove suspended members.
3. Generate the preliminary roll. Members with overdue dues are marked ineligible with the reason
   "Membership dues are overdue."
4. Publish the preliminary roll.
5. Receive objections, investigate and correct records.
6. Approve the final roll.
7. Lock the voter database. After locking there is no change unless the Commission orders it.

Inspection checklist: membership number, name, voting status, duplicate records, suspended members,
outstanding appeals.

The lock is the action `POST /api/elections/{id}/voter-roll/freeze`. It needs `ManageVoterRoll` and the
step-up check, and it returns the number of voters locked. The web admin elections page and the mobile admin election screen both have a "Freeze roll" button.
The mobile app asks for the emailed step-up code and retries once, so the button works; a check on a
real device is still open (TODO 84.42).

A frozen roll can be thrown away with `POST /api/elections/{id}/voter-roll/unfreeze`, so the roll can be
rebuilt and frozen again. It needs the same permission and step-up check, and a written reason of up to
400 characters. The reason, the number of rows removed and the actor go into the election audit chain as
`VoterRollUnfrozen`. The server refuses once the election is past the Nomination phase, once any
nomination exists, and inside the cutoff window. The window is the election setting
`UnfreezeCutoffDaysBeforeNominationClose` (default 2 days before nominations close). Both clients have an
"Unfreeze roll" button that asks for the reason.

Output: locked voter roll.

## Phase 5. Nomination

| Part | Entry |
|---|---|
| Who | Returning Officer, Assistant Returning Officer, Scrutineer, Commissioner with `DecideNominations` |
| From where | Nominations list at `/officials/elections/:id`. Candidates added at `/admin/elections`. |
| After what | Voter roll locked, phase Nomination reached |
| Basis | Regulations Article 9. Eligibility (3.2). Written reasons make a rejection checkable (3.4). |

Steps

1. The Commission publishes the Nomination Notice and distributes forms.
2. Receive applications and issue a Nomination Receipt (ER-16) for each.
3. Register each application and verify the documents. Record the nominee's consent (`ConsentAt`). A
   nomination made before this field existed prints "not recorded".
4. Collect the fee. The Commission sets the fee under Article 9.
5. Scrutiny: check membership, eligibility, founding quota, outstanding dues, political neutrality,
   required documents, fee paid, declarations signed, conflict of interest and criminal disclosure.
   Record the result on the Nomination Scrutiny Checklist (ER-17). Any Commissioner may tick an item, and
   the sheet prints who ticked it and when. Until the app stores the ticks (TODO 37.14m, stored fields
   for the election forms), tick the printed sheet by hand and sign it.
6. Open the nominations list. Accept or reject each nomination and write the reason. A rejection names
   the rule. The Nomination Acceptance Order (ER-17A) records the decisions.
7. A nominee, proposer or seconder of a candidate does not decide that nomination.
8. Publish the accepted list. The Withdrawal phase follows.
9. Record withdrawals.
10. Fix the final candidate list and assign symbols.
11. Add each final candidate at `/admin/elections` if not already added.

Candidate file for each candidate: application, receipt, declarations, photo, membership verification,
scrutiny report, decision, appeal, final status.

One valid candidate for a seat: that candidate is declared elected unopposed, is left off the ballot and
is certified at the declare.

No valid candidate for a seat: the Chief Commissioner reopens nominations for that seat only. The period
is set for each election (default 3 days) and ends before the roll freeze. The poll date does not move. If
the seat is still empty it stays vacant until a by-election.

Output: final candidate list.

## Phase 6. Campaign

| Part | Entry |
|---|---|
| Who | Commission |
| From where | Paper and public notices |
| After what | Candidate list fixed |
| Basis | Regulations Article 10 and `03-Election-Code-Of-Conduct.md`. Transparency (3.4). |

Steps

1. Publish the Campaign Calendar, Campaign Rules, Silence Period, Permitted Advertising, Complaint
   Procedure and Campaign Monitoring Schedule.
2. Prepare a daily campaign inspection report.
3. Document each violation. Sanctions are Warning, Show Cause, Suspension and Disqualification.

Output: campaign reports and complaint record.

## Phase 7. Poll preparation

| Part | Entry |
|---|---|
| Who | Commission, Technical Administrator, Returning Officer, Cybersecurity Auditor |
| From where | Polling kit on paper. `/admin/elections` and the officials dashboard for the system. |
| After what | Campaign running, final candidate list fixed |
| Basis | Regulations Articles 11 and 12. Standards 5.4, 8.4, 14, 18. The ballot key is made before polling because it cannot change after. |

Paper steps

1. Prepare a polling kit: ballots, registers, pens, seals, stamp, ink, instructions, emergency contacts,
   Incident Register, Complaint Register, Spoiled Ballot Register, Unused Ballot Register, Polling
   Officer Handbook.

System steps

2. The Technical Administrator installs servers, verifies SSL, the database and encryption, configures
   OTP and generates audit logs.
3. The Cybersecurity Auditor performs a penetration test.
4. The Technical Administrator generates the readiness certificate and conducts a mock election.
5. **Make the ballot key.** The Returning Officer opens `/admin/elections`, reads the warning, and makes
   the key. The browser makes the key pair. The public key and its SHA-256 fingerprint are saved on the
   election. The private key downloads once as `<election>-returning-officer-key.pem`.
6. Check the key. The election shows a fingerprint. The file opens as text starting `-----BEGIN`.
7. Store the file on a device the Returning Officer controls, behind a screen lock.
8. Announce the fingerprint to the Commission. Voters see the same fingerprint on the ballot screen.
9. Check that at least two live officials hold `Approve`. The readiness list shows a warning if there
   are fewer than two. The warning does not block the request. With one approver, an emergency revoke
   during polling has nobody to approve it. Fix it first.
10. **Settle every dependency.** Polling does not open until each item is settled and minuted
    (Regulations Article 11). Before polling, the officials dashboard shows the readiness list, and
    the system checks each item itself:
    - the voter roll is frozen
    - every accepted candidate has a place on the ballot, which is set when the election enters
      Candidate list
    - the ballot key is set. The system does not yet check the sealed copies (TODO 37.14a, key
      copies), so the Commission still confirms every copy is held
    - every appointment still open is accepted with its declaration signed
    - the polling window opens before it closes and has not closed yet
    - the mock election was run, the technical audit is done, every nomination appeal is decided, and
      paper stock is ready. The system cannot see these four. An official with `ChangePhase` records
      each one with **Record as done** on the web officials dashboard, after step-up. The record goes
      into the audit chain, cannot be taken back, and can be made only before polling opens. A test
      election needs these four too.

    The request to open polling is refused while any item is not met, and the refusal names the
    items still open. The approval is checked again, because the request may be hours old.

    Owner decision 2026-10-27: 37.14a must enforce the configured copy count and each holder's
    receipt confirmation before Polling. Until that work is built, the Commission checks the full
    custody list by hand before approving polling.
11. A Commissioner with `ChangePhase` asks to open polling. A second official with `Approve` approves.
12. The rules freeze as soon as the request is pending.

To replace the key, do it before polling opens and with a second official's approval. Destroy the old
file and every copy. After polling opens the system refuses a replacement.

Output: ballot key made and announced, every readiness item met, readiness certificate, mock election
done, open-polling request approved.

## Phase 8. Poll opening

| Part | Entry |
|---|---|
| Who | Presiding Officer, Polling Officer, observer, candidate agent, Commission representative |
| From where | Polling station and `/portal/election` |
| After what | Open-polling request approved |
| Basis | Regulations Articles 11 and 12. Transparency (3.4): observers sign before voting starts. |

Steps

1. Verify the ballot count, the empty box, the security seal, the electronic system, observer presence and
   candidate agent presence.
2. Complete the Poll Opening Certificate. It is signed by the Polling Officer, an observer, a candidate
   agent and a Commission representative.
3. Polling begins only after the certificate is complete.
4. The election phase changes to Voting open.

Output: Poll Opening Certificate.

## Phase 9. Poll management

| Part | Entry |
|---|---|
| Who | Presiding Officer, Polling Officer, Technical Administrator, Cybersecurity Auditor, observers |
| From where | Polling station, `/portal/election`, audit history |
| After what | Poll opened |
| Basis | Regulations Articles 11 and 12. Secrecy (3.1), one voter one ballot (3.2), standards 7.1, 11, 12, 16. |

Manual voter steps

1. Verify identity and membership.
2. Locate the voter on the register and mark the register.
3. Issue the ballot. The voter votes and deposits the ballot.
4. Mark the voter as voted.

Electronic voter steps

1. The voter signs in and passes the OTP check.
2. The voter votes. The system seals the ballot at casting.
3. The voter sees a confirmation. The action is written to the audit history.

Watch steps

4. The Technical Administrator watches availability and the audit history. Nobody reads results.
5. Record every unusual event at once in the Incident Register. Keep the Complaint Register, Spoiled
   Ballot Register and Unused Ballot Register.
6. A backup taken during polling keeps the sealed ballots. With the key file it can link voters to choices.
   Treat the backup as carefully as the key, and delete it on the same day as the key file (procedure D
   in section 10).

Extending polling: the Chief Commissioner can extend an open poll by up to 24 hours at a time, at most
twice. It needs a written reason, a second approver and the step-up check, and it works only while
polling is open. Voters are told each time and the extension is written to the audit history. A third
extension is a level 3 incident: the election is void and is run again.

If an official must be removed while the rules are frozen, use the emergency revoke:

1. One official asks and gives a reason.
2. A second active official with `Approve` approves. That official is neither the requester nor the
   person removed.
3. A SuperAdmin cannot approve in their place. An unanswered request lapses.

Output: Incident Register, Complaint Register, audit history.

## Phase 10. Poll closing

| Part | Entry |
|---|---|
| Who | Presiding Officer, Polling Officer, Technical Administrator, Commissioner with `ChangePhase` |
| From where | Polling station and the officials dashboard |
| After what | Closing time reached |
| Basis | Regulation Article 13. Software independence (3.3): counts on paper and in the system are compared. |

Steps

1. At the closing time admit no further voter.
2. Count unused, issued, spoiled and remaining ballots and the electronic votes. A Polling Officer
   enters the issued, unused and spoiled paper counts. The Chief Commissioner and a second approver
   approve them before the count. The system refuses them unless issued equals used plus unused plus
   spoiled. The counts are printed on the Poll Closing Certificate. An online ballot is never spoiled.
3. Seal the ballot box. Print the audit log. Generate the closing report.
4. Prepare the Poll Closing Certificate, the Ballot Box Sealing Certificate, the Electronic Audit
   Certificate and the Observer Report. All are signed.
5. A Commissioner with `ChangePhase` asks to close polling. A second official with `Approve` approves.
6. The Returning Officer completes the Ballot Seal and Poll Integrity Certificate
   (`06-Election-Ballot-Seal-and-Poll-Integrity-Certificate.md`).
7. Once signed, an objection about ballot integrity is not heard unless it is backed by evidence of
   fraud, tampering, coercion or another material irregularity.

Output: closing certificates, integrity certificate, phase Counting.

## Phase 11. Counting

| Part | Entry |
|---|---|
| Who | Returning Officer, Counting Supervisor, Commission, observers, a second official with `Approve` |
| From where | Officials dashboard count section. Paper for the tally. |
| After what | Polling closed, integrity certificate signed, count authorisation (`07-Election-Vote-Counting-Authorisation.md`) |
| Basis | Regulation Article 14. Standards 8.4 and 10. Secrecy (3.1), software independence (3.3), separation of duties (3.5). |

Steps

1. The Commission checks the seal, serial number, certificate, observer signatures and audit logs. If
   they are satisfactory the count proceeds.
2. An official with `Count` asks to count the ballots. A second official with `Approve` approves.
   Approval does not run the count.
3. The requester runs the count. The Returning Officer uploads the `.pem` file once. The server uses it for
   that request only.
4. If the organisation setting `CountRequesterOnly` is on, only the requester may run it. If it is off,
   any other official with `Count`, except the approver, may.
5. The system refuses the count when fewer than 10 ballots were cast. Count by hand in that case.
6. The system compares ballots, receipts and voted flags. If they disagree, stop and report it to the
   Commission and the Cybersecurity Auditor. Do not retry.
7. Retry with another file only when the error says the key is wrong.
8. Confirm the system records who ran the count.
9. Each ballot is announced. Votes are recorded. An independent tally is kept. Disputed ballots are
   separated. Rejected ballots are documented. A recount is made if ordered.
10. The count approver never sees the key file.

Output: count result, count sheet, tally.

## Phase 12. Result certification

| Part | Entry |
|---|---|
| Who | Chief Commissioner, Commissioners, observer, technical auditor |
| From where | Officials dashboard, `/elections/results` |
| After what | Count complete and reconciled |
| Basis | Regulation Article 15. Transparency (3.4), standards 12. |

Steps

1. The Commission prepares the Result Register, the Result Certificate and the Result Summary.
2. A Commissioner with `Declare` asks to declare. A second official with `Approve` approves.
3. The result is published, including zero-vote candidates, abstentions and spoiled ballots.
4. Publish on the website, on the notice board and notify members.
5. The Chief Commissioner, Commissioners, observer and technical auditor sign.

Declaring the result also starts the fixed official access period. The default is 21 days, set in
`AccessEndsDaysAfterDeclare`. It is meant to cover the appeal window and the tribunal period in Election
Regulations Article 16 (7 days to file an appeal, 14 days for the tribunal to decide). Article 5 sets the
same dates in the timeline. Adding 7 and 14 gives 21, which is an interpretation, because Article 5 says
the appeals are disposed within 14 days. The Association can change it in the election settings. When it ends, the official can no longer act on the
election. An official who is not a member cannot sign in after that day. An official who is a member
signs in as before and loses only the official role.

Output: published result, signed certificate.

## Phase 13. Appeals

| Part | Entry |
|---|---|
| Who | Appeal Tribunal Member, Commission |
| From where | Appeal Register on paper, audit history on the dashboard |
| After what | Declared |
| Basis | Regulation Article 16. Transparency (3.4): a decision can be checked against the audit history. |

Steps

1. Open the Appeal Register.
2. Give every appeal a reference number, an officer, a timeline, an evidence register, a hearing date, a
   decision date and a final order.
3. The Tribunal member reads the appeal and the audit history for the decision under appeal.
4. Decide within the window the regulations set. Record the decision and the reason.
5. Watch the recount window. The Commission sets its end date for each election and states it in the
   Election Calendar (ER-02). The key is not destroyed until that date is set and has passed. The app
   does not yet check this: it holds no recount end date and shows no dashboard warning (TODO 37.12r).
6. A recount: one request per seat, filed on ER-28 (Recount Request) by a candidate on that seat, within
   the window. The decision goes on ER-28A (Recount Decision), and ER-28B (Recount Report) sets the
   original count beside the recount. The
   Chief Commissioner decides it, with a second approver. The count is run again and both results are
   logged. After the decision there is no second request for that seat. Do not destroy the key while a
   request is open. The app does not yet refuse the destruction record in that case (TODO 37.12r). An official with a declared conflict does not decide it.

Output: final orders.

## Phase 14. Transition and handover

| Part | Entry |
|---|---|
| Who | Outgoing Executive Committee, Returning Officer, Commission |
| From where | Paper handover forms, officials dashboard |
| After what | Appeals closed and the recount window ended |
| Basis | Regulation Article 17. Key management (standards 8.4). |

Steps

1. **Destroy the key.** The Returning Officer deletes every copy of the key file after the declare and
   the recount window. Delete backups taken during polling the same day.
2. **Record it.** After the phase is Declared, an official with `SetBallotKey` sees a "key file
   destroyed" box on the web officials dashboard. Tick it and confirm with the step-up check. It can be
   recorded once. The mobile app has no such action. The record is a statement, not proof. It is written
   to the audit chain with the key fingerprint.
3. The outgoing Executive Committee hands over the bank, membership database, accounts, cash book,
   digital platforms, website, email, domain, cloud storage, source code, archives, office keys, assets,
   minutes, contracts and passwords, on the Executive Committee Handover Certificate (ER-34) and its
   15-item checklist. The outgoing and incoming holders of the positions flagged as head and secretary
   sign, with two witnesses. The position names come from the committee position settings, not from a
   fixed title (TODO 95.2, configurable committee positions). The Commission then files the Election
   Completion Report (ER-33), the Archive Register (ER-35) and the Closure Certificate (ER-35A).
4. A Commissioner asks to archive. A second official with `Approve` approves.
5. Archiving ends every official appointment at once, even before the access period runs out. The audit
   record is kept.

Output: key destroyed and recorded, handover forms, phase Archived.

## Post election review

| Part | Entry |
|---|---|
| Who | Commission |
| From where | Paper |
| After what | Archived |
| Basis | Regulation Article 17. ISO 27001 improvement cycle. |

Steps

1. Prepare Lessons Learned, Risk Analysis, Improvement Report, Statistics, Complaint Analysis,
   Cybersecurity Report, Financial Summary, Archive Index and the Final Election Report.
2. Transfer the archive to the Association.
3. Close the election officially.
4. Keep the records for ten years from the declare (see Annex C). Nothing is deleted automatically. The
   procedure is that a purge after ten years needs a request and a second approver (a SuperAdmin if no
   Commission sits), and that the summary result and the purge record stay. The app does not yet list
   elections past ten years or take a purge request (TODO 37.12s covers the retention check). Until it
   does, the Chief Commissioner tracks the date by hand.

---

# 10. Incident procedures

The Returning Officer leads. Every step goes in the Incident Register. The full text is in
`09-Election-Security-and-Compliance.md`. Escalation has three levels. Level 1 is handled by the official on duty and logged. Level 2 goes to the
Chief Commissioner at once (D and E). Level 3 goes to the Commission and the Executive Committee, and
polling may pause (A, B and C).

| Procedure | Case | Core action |
|---|---|---|
| A | Key file lost | Tell the Commission at once. Record it. The ballots cast cannot be counted by the system, so plan a decision with the Commission. Before polling, replace the key. After polling opens, the system will not replace it. |
| B | Key file leaked | Treat every ballot as readable by the person who holds the file together with a backup. Record it, tell the Commission and the Cybersecurity Auditor, and let the Commission decide on the election. |
| C | Suspected link between voters and choices | Stop. Record. The Cybersecurity Auditor checks the audit history. The Commission decides. |
| D | Backup or snapshot taken during polling | Treat it like the key. Store it only where the key could be stored. Delete it the same day as the key file. |
| E | Poll stopped or service down | The Technical Administrator records the time. The Commission decides on an extension or a hand ballot using `04-Election-Manual-Ballot.md`. |

The table is a summary. The authoritative text is `09-Election-Security-and-Compliance.md`, which this table follows.

---

# 11. Quick reference

| Step | Who asks | Who approves |
|---|---|---|
| Publish | `ChangePhase` holder | Second official with `Approve` |
| Make first key | `SetBallotKey` holder | none |
| Replace key (before polling) | `SetBallotKey` holder | Second official with `Approve` |
| Open polling | `ChangePhase` holder | Second official with `Approve` |
| Close polling | `ChangePhase` holder | Second official with `Approve` |
| Count | `Count` holder | Second official with `Approve` |
| Declare | `Declare` holder | Second official with `Approve` |
| Archive | `ChangePhase` holder | Second official with `Approve` |
| Emergency revoke | Any official | Second official with `Approve`, not the target. Always two people. |
| Record key destroyed | `SetBallotKey` holder | none |

Publish, Open polling, Close polling and Archive are all requested through the phase action, which
needs `ChangePhase` and the step-up check. Count needs `Count`. Declare needs `Declare`. The code was
checked on 2026-10-11.

The "second official" column assumes the default, with the two-person setting on. See section 5.

---

# 12. Checklists with signature boxes

Tick each box. Sign and date at the end. Print one copy per election.

## A. Before the open-polling request

| Done | Check | Checked by | Signature | Date |
|---|---|---|---|---|
| [ ] | Voter roll locked | | | |
| [ ] | Final candidate list fixed | | | |
| [ ] | All officials accepted and signed neutrality declarations | | | |
| [ ] | At least two live officials hold `Approve` | | | |
| [ ] | Ballot key made, fingerprint checked | | | |
| [ ] | Fingerprint announced to the Commission | | | |
| [ ] | Key file stored on a device the Returning Officer controls | | | |
| [ ] | Penetration test done | | | |
| [ ] | Readiness certificate issued | | | |
| [ ] | Mock election done | | | |
| [ ] | Accessibility checklist run | | | |

## B. Poll opening

| Done | Check | Checked by | Signature | Date |
|---|---|---|---|---|
| [ ] | Ballot count checked | | | |
| [ ] | Ballot box empty | | | |
| [ ] | Security seal on | | | |
| [ ] | Electronic system checked | | | |
| [ ] | Observer present | | | |
| [ ] | Candidate agent present | | | |
| [ ] | Poll Opening Certificate signed | | | |

## C. Poll closing and count

| Done | Check | Checked by | Signature | Date |
|---|---|---|---|---|
| [ ] | Unused, issued, spoiled and remaining ballots counted | | | |
| [ ] | Ballot box sealed | | | |
| [ ] | Audit log printed | | | |
| [ ] | Closing certificates signed | | | |
| [ ] | Integrity certificate signed | | | |
| [ ] | Seals, serial numbers and observer signatures checked | | | |
| [ ] | Count approved by a second official | | | |
| [ ] | Count ran, integrity check passed | | | |
| [ ] | Independent tally agrees | | | |

## D. After the result

| Done | Check | Checked by | Signature | Date |
|---|---|---|---|---|
| [ ] | Result declared and published | | | |
| [ ] | Appeals closed | | | |
| [ ] | Recount window ended | | | |
| [ ] | Every key file copy deleted | | | |
| [ ] | Backups from polling deleted | | | |
| [ ] | Key destruction recorded in the system | | | |
| [ ] | Handover forms signed | | | |
| [ ] | Election archived | | | |
| [ ] | Post election review complete | | | |

## Chain of custody and signatures

The chain of custody has 15 steps and the signature matrix is in `08-Election-Forms-Appendices.md`
(Appendices A and B). Print them with this handbook and use them instead of copying them here.

---

# 13. Reference implementation

`docs/Elections/reference-implementation/` holds a small stand-alone Node and PostgreSQL system that
shows the key flow end to end: key made in the browser, ballots sealed in the browser, and a count that
checks the key fingerprint. It is for teaching and for the dissertation. It is not the product, which
seals each ballot on the server. Its README lists the differences. Its crypto has 8 passing tests. Its
server logic has not been run against a live PostgreSQL.

---

# Annex A. Documents to print and attach

Print this handbook first, then attach the documents below. File names are in `docs/Elections/`.

## Always attach

| Order | File | Why |
|---|---|---|
| 1 | `01-Election-Regulations.md` | The rules and the timeline (Article 5) |
| 2 | `02-Election-Operational-Manual.md` | The process the dry run follows |
| 3 | `11-Election-Key-Procedure.md` | The key procedure, in full |
| 4 | `09-Election-Security-and-Compliance.md` | Threat model and incident procedures A to E |
| 5 | `08-Election-Forms-Appendices.md` | Signature matrix, chain of custody, retention |

## Attach by post or task

| File | Attach for |
|---|---|
| `00-Election-Master-Overview.md` | Commission, ECSC, anyone who asks why |
| `00-election-standards-requirements.md` | Cybersecurity Auditor, Technical Administrator, Commission, observers |
| `03-Election-Code-Of-Conduct.md` | Commission, candidates, Scrutineers, campaign monitors |
| `04-Election-Manual-Ballot.md` | Presiding and Polling Officers, and for incident E |
| `05-Election-Forms-and-Templates.md` | Everyone who fills a form (form layouts ER-01 to ER-24; Annex B lists every form) |
| `06-Election-Ballot-Seal-and-Poll-Integrity-Certificate.md` | Returning Officer, Presiding Officer, observers, candidate agents |
| `07-Election-Vote-Counting-Authorisation.md` | Returning Officer, Counting Supervisor, Commission (form layouts ER-22A and ER-25A to ER-35A; Annex B lists every form) |
| `10-Election-Accessibility-Checklist.md` | Technical Administrator, Commission (before polling) |

## Attach by phase

| Phase | Attach |
|---|---|
| 1 to 3 | 01, 02, 05, 00 master overview |
| 4 | 01, 02, 05 |
| 5 | 01, 03, 05 |
| 6 | 01, 03 |
| 7 | 02, 09, 10, 11, 05, 04 |
| 8 and 9 | 04, 06, 08, 09 |
| 10 | 06, 08 |
| 11 | 07, 08, 11 |
| 12 and 13 | 01, 05, 07 |
| 14 and review | 08, 11 |

The PDF `00-Election-Master-Overview.pdf` is a ready print of the master overview.

---

# Annex B. Election forms

Decided by the owner on 2026-10-20. The forms use the Regulations numbers ER-01 to ER-35 as they stand.
The 15 forms that exist only as templates in `05-Election-Forms-and-Templates.md` and
`07-Election-Vote-Counting-Authorisation.md` take a letter after the form they follow, for example
ER-17A after ER-17. The Source column names the file that holds the layout (05, 06 or 07) and the template number the
form had there before its FORM heading was renumbered on 2026-10-21.

Rules for every form:

1. The 16 codes the platform printed before 2026-10-22 (the Old code column) no longer open a form, on
   the document download or on the verify page. A QR code on a form printed before then does not resolve.
2. Forms may be printed from stored records or prepared blank for hand filling (TODO 37.14s, form
   wording from settings). A blank template is not proof that an official form has been issued.
   Under the 37.14t owner decision, verification must prove the paper is genuine and unchanged and
   must reject a form that could not yet exist in the election phase. Until that verification is
   built, a form marked Not built is copied from its template and filled by hand.
3. The organisation name, address, phone numbers, email and logo on a form come from the org settings
   (TODO 37.14s). Today the PDF header prints the organisation name only.
4. A signature line for a committee position prints the label of the position that holds the head or
   secretary flag in the committee position settings (TODO 95.2, configurable committee positions).

Status means: Filled from records, the platform prints it with the election's data; Printed blank, the
platform prints the frame and officials fill it. A date in brackets is when the row last changed. On
2026-10-23 every form then marked Not built was printed blank and checked against its layout (TODO
37.16a), and all 50 codes now print. ER-04, ER-05, ER-23 and ER-24 moved to Filled from records on 2026-10-22
(TODO 37.14p). Lines with no stored record on those forms, such as seal numbers and observers, stay
for hand. From 2026-10-23 (TODO 37.14q, the remaining election forms) a filled copy of any form prints
the stored value on a ruled line whose label matches an election record: the schedule dates, venue,
campaign and silence dates, roll totals, ballot counts and the audit entry count and head hash. Lines
about one person, such as Name and Membership No., are always left for hand. A date that was never set
prints "Not recorded".

Also from 2026-10-23 (TODO 37.14q): ER-17 prints each nomination's scrutiny ticks and the decision.
ER-28 prints the recount requests, ER-28A the decisions, and ER-28B the original count against the
recount. ER-34 lists the handover items with their ticks, read on the election's own committee period,
which is the incoming term. On ER-27, ER-27C and ER-29, Spoiled Ballots is the spoiled count (a layout an admin saved before
2026-10-23 may still say Rejected Ballots and gets the same count). ER-29 also fills Registered Voters and Votes Cast as ER-24 does. On ER-27C
Total Votes is votes cast less spoiled. Winner and Runner-up are filled only when one seat is
elected; with more seats both read "See result per seat". ER-25 keeps an official whose appointment
was accepted after the access ends, and leaves out one that lapsed unaccepted or was revoked, the
same rule as ER-04 and ER-05. ER-22A, ER-23A, ER-23B, ER-25A to ER-25C, ER-27A, ER-27B, ER-30A,
ER-30B and ER-35A have no stored record and print blank. Their wording was reviewed and changed on
2026-10-23 (TODO 37.16a, wording review of the template forms): Polling Venue and the election
reference on each sheet, 24-hour times, the rejection grounds of manual 04 section 13 (Invalid
Ballots) on ER-27B, three counters and an Abstain row on ER-27A, and the appeal period and ten-year
archive on ER-35A.

| # | Code | Form | Old code | Source | Signed by | Status 2026-10-20 |
|---|---|---|---|---|---|---|
| 1 | ER-01 | Election Notice | ER-01 | Regulations and file 05 (was template ER-01) | Chief Election Commissioner | Schedule filled from records (2026-10-23) |
| 2 | ER-02 | Election Calendar | ER-02 | Regulations and file 05 (was template ER-02) | Chief Election Commissioner; Election Commissioner | Printed blank |
| 3 | ER-03 | Commissioner Appointment | none | Regulations only | Chair, Selection Committee; Member, Selection Committee | Printed blank (2026-10-23) |
| 4 | ER-04 | Election Commissioner Acceptance | none | Regulations and file 05 (was template ER-03) | Appointee; Witness | Filled from records (2026-10-22) |
| 5 | ER-05 | Declaration of Neutrality | none | Regulations and file 05 (was template ER-04) | Declarant | Filled from records (2026-10-22) |
| 6 | ER-06 | Conflict of Interest Declaration | none | Regulations and file 05 (was template ER-05) | Declarant; Chief Election Commissioner (Received) | Printed blank (2026-10-23) |
| 7 | ER-07 | Confidentiality Agreement | none | Regulations and file 05 (was template ER-06) | Signatory; Chief Election Commissioner (Countersigned) | Printed blank (2026-10-23) |
| 8 | ER-08 | Preliminary Voter Roll Certification | ER-07 | Regulations and file 05 (was template ER-07) | Prepared By; Verified By; Approved By (Chief Election Commissioner) | Filled from records |
| 9 | ER-09 | Voter Roll Objection | none | Regulations and file 05 (was template ER-08) | Election Commissioner; Chief Election Commissioner | Printed blank (2026-10-23) |
| 10 | ER-10 | Candidate Nomination Form | ER-09 | Regulations and file 05 (was template ER-09) | Candidate; Received By (Commission officer) | Filled from records |
| 11 | ER-11 | Candidate Consent | ER-10 | Regulations and file 05 (was template ER-10) | Candidate | Filled from records |
| 12 | ER-12 | Eligibility Declaration | none | Regulations only | Candidate | Printed blank (2026-10-23) |
| 13 | ER-13 | Political Neutrality Declaration | none | Regulations only | Candidate; Received By (Commission officer) | Printed blank (2026-10-23) |
| 14 | ER-14 | Proposer Form | none | Regulations only | Proposer; Received By (Commission officer) | Printed blank (2026-10-23) |
| 15 | ER-15 | Seconder Form | none | Regulations only | Seconder; Received By (Commission officer) | Printed blank (2026-10-23) |
| 16 | ER-16 | Nomination Receipt | none | Regulations only | Received By (Commission officer) | Printed blank (2026-10-23) |
| 17 | ER-17 | Nomination Scrutiny Checklist | ER-11 | Regulations and file 05 (was template ER-11) | Election Commissioner; Election Commissioner; Chief Election Commissioner | Filled from records (2026-10-23) |
| 18 | ER-17A | Nomination Acceptance Order | none | File 05 only (was template ER-12) | Chief Election Commissioner | Printed blank (2026-10-23) |
| 19 | ER-18 | Candidate Withdrawal | ER-13 | Regulations and file 05 (was template ER-13) | Candidate; Accepted By (Election Commission) | Filled from records |
| 20 | ER-19 | Final Candidate List | ER-14 | Regulations and file 05 (was template ER-14) | Chief Election Commissioner | Filled from records |
| 21 | ER-20 | Campaign Undertaking | none | Regulations only | Candidate; Received By (Commission officer) | Campaign dates filled from records (2026-10-23) |
| 22 | ER-21 | Observer Appointment | none | Regulations only | Chief Election Commissioner; Observer (accepts) | Printed blank (2026-10-23) |
| 23 | ER-22 | Polling Officer Appointment | none | Regulations only | Chief Election Commissioner; Appointee (accepts) | Printed blank (2026-10-23) |
| 24 | ER-22A | Election Material Receipt Register | none | File 07 only (was template ER-39) | Received By; Handed Over By | Printed blank (2026-10-23) |
| 25 | ER-23 | Poll Opening Certificate | none | Regulations and file 05 (was template ER-15) | Presiding Officer; Polling Officer; Observer; Candidate Agent | Filled from records (2026-10-22) |
| 26 | ER-23A | Poll Incident Register | none | File 05 only (was template ER-16) | Presiding Officer; Observer | Printed blank (2026-10-23) |
| 27 | ER-23B | Spoiled Ballot Register | none | File 05 only (was template ER-17) | Polling Officer; Presiding Officer | Printed blank (2026-10-23) |
| 28 | ER-24 | Poll Closing Certificate | none | Regulations and file 05 (was template ER-18) | Presiding Officer; Polling Officer; Observer; Candidate Agent | Filled from records (2026-10-22) |
| 29 | ER-25 | Ballot Box Sealing and Poll Integrity Certificate | ER-19 | Regulations and file 06 (was template ER-19) | Chief Election Commissioner; Election Commissioner; Election Commissioner; Presiding Officer; Polling Officer; Security Officer; Observer 1; Observer 2; Candidate Agent A; Candidate Agent B; Candidate Agent C | Filled from records |
| 30 | ER-26 | Electronic Voting Audit | none | Regulations only | Technical Auditor; Chief Election Commissioner; Election Commissioner; Observer | Counts filled from records (2026-10-23) |
| 31 | ER-25A | Vote Counting Authorisation Certificate | none | File 07 only (was template ER-20) | Chief Election Commissioner; Election Commissioner; Election Commissioner; Observer | Printed blank (2026-10-23) |
| 32 | ER-25B | Ballot Box Opening Certificate | none | File 07 only (was template ER-21) | Opened By (Chief Election Commissioner); Election Commissioner; Election Commissioner; Observer; Candidate Agent | Printed blank (2026-10-23) |
| 33 | ER-25C | Ballot Paper Reconciliation Sheet | none | File 07 only (was template ER-22) | Verified By (Chief Election Commissioner); Election Commissioner; Observer | Printed blank (2026-10-23) |
| 34 | ER-27 | Vote Counting Sheet | ER-23 | Regulations and file 07 (was template ER-23) | Counted By; Verified By | Filled from records |
| 35 | ER-27A | Independent Tally Sheet | none | File 07 only (was template ER-24) | Counter A; Counter B; Counter C; Verified By (Chief Election Commissioner) | Printed blank (2026-10-23) |
| 36 | ER-27B | Rejected Ballot Register | none | File 07 only (was template ER-25) | Election Commissioner; Chief Election Commissioner | Printed blank (2026-10-23) |
| 37 | ER-27C | Final Result Sheet | ER-29 | File 07 only (was template ER-29) | Certified By (Chief Election Commissioner); Election Commissioner; Election Commissioner | Filled from records |
| 38 | ER-28 | Recount Request | ER-26 | Regulations and file 07 (was template ER-26) | Applicant; Received By (Commission officer) | Filled from records (2026-10-23) |
| 39 | ER-28A | Recount Decision | none | File 07 only (was template ER-27) | Recount Ordered By (Chief Election Commissioner); Election Commissioner; Election Commissioner | Filled from records (2026-10-23) |
| 40 | ER-28B | Recount Report | ER-28 | File 07 only (was template ER-28) | Chief Election Commissioner; Election Commissioner; Observer | Filled from records (2026-10-23) |
| 41 | ER-29 | Election Result Certification | ER-30 | Regulations and file 07 (was template ER-30) | Chief Election Commissioner; Election Commissioner; Election Commissioner; Observer; Observer | Filled from records |
| 42 | ER-30 | Result Declaration | ER-31 | Regulations and file 07 (was template ER-31) | Certified By (Chief Election Commissioner) | Filled from records |
| 43 | ER-30A | Election Complaint Form | none | File 07 only (was template ER-32) | Complainant; Received By (Commission officer) | Printed blank (2026-10-23) |
| 44 | ER-30B | Election Investigation Report | none | File 07 only (was template ER-33) | Investigating Officer; Received By (Election Commission) | Printed blank (2026-10-23) |
| 45 | ER-31 | Appeal to Election Appeals Tribunal | none | Regulations and file 07 (was template ER-34) | Appellant; Received By (Tribunal registrar) | Printed blank (2026-10-23) |
| 46 | ER-32 | Tribunal Decision | none | Regulations and file 07 (was template ER-35) | Tribunal Member; Tribunal Member; Tribunal Member | Printed blank (2026-10-23) |
| 47 | ER-33 | Election Completion Report | none | Regulations and file 07 (was template ER-36) | Prepared By (Chief Election Commissioner); Election Commissioner | Printed blank (2026-10-23) |
| 48 | ER-34 | Executive Committee Handover Certificate | ER-33 | Regulations and file 07 (was template ER-37) | Outgoing and incoming head; outgoing and incoming secretary; two witnesses | Filled from records (2026-10-23) |
| 49 | ER-35 | Election Archive Register | none | Regulations and file 07 (was template ER-38) | Prepared By; Verified By (Chief Election Commissioner) | Printed blank (2026-10-23) |
| 50 | ER-35A | Election Closure Certificate | none | File 07 only (was template ER-40) | Chief Election Commissioner; Election Commissioner; Election Commissioner; association head; association secretary | Printed blank (2026-10-23) |

---

# Annex C. Conflicts between source documents

| Point | Conflict | What this handbook does |
|---|---|---|
| Form numbers (resolved 2026-10-20) | `01-Election-Regulations.md` lists 35 forms (ER-01 to ER-35). Files `05`, `06` and `07` hold 40 forms (ER-01 to ER-40), and they are not the same set. The Regulations have forms with no template (for example Commissioner Appointment, Observer Appointment, Proposer, Seconder). The templates have forms the Regulations do not list (for example Recount Decision, Election Complaint Form, Tribunal Decision). The manual and the platform use the template numbers. | Settled 2026-10-20. The Regulations numbers are used as they stand and the 15 template-only forms take a letter after the form they follow. Annex B lists all 50 forms with the template each comes from. |
| Record retention | Regulation Article 17 said ten years. Appendix C of `08-Election-Forms-Appendices.md` gave a table with permanent and shorter periods. | Settled 2026-10-11. Ten years from the declaration of the result for every record. Article 17 and Appendix C now say so. |
| Two-person rule | An earlier draft of this handbook said it is always on. It is a setting. Only the emergency revoke is fixed at two. | Corrected in section 5. |
| Appeal disposal | Article 5 lists appeals within 7 days, then "Appeals disposed" within 14 days, and then the Executive Committee assuming office within 15 days. Article 16 says an appeal is filed within 7 days and the Tribunal decides within 14 days. Article 5 reads as 14 days from the declare. Article 16 does not say what the 14 days count from, so it can read as 14 days from the filing, which is up to day 21 from the declare. The two texts also differ in their end point: Article 5 ends disposal on day 14 and the new committee takes office on day 15. | Treats it as one 14-day clock whose start the Regulations do not fix. The access period of 21 days (phase 12) covers the longer reading. *[to confirm: the Commission states which start applies.]* |
| Sealing | The key procedure follows the product, which seals on the server. The reference implementation seals in the browser. | Said in section 13 and in the reference README. |

---

# Annex D. Open points

| Point | Where it appears | Who can settle it |
|---|---|---|
| The renumber (TODO 37.14n), the stored fields (TODO 37.14m), the filled forms (TODO 37.14p and 37.14q) and the template wording (TODO 37.16a) are done as of 2026-10-23. From 2026-10-23 (TODO 37.16b) the seeder also refreshes any form or page no admin has saved, so a database seeded earlier takes the new wording on its next start. A form an admin has saved keeps the admin's text. | Annex B, phases 2, 4, 5, 13, 14 | Owner (TODO 37.16b, open items from the wording review) |
| Committee positions are a fixed list in code. Signature lines and ID cards print fixed titles until the position settings are built. | Annex B, phase 14 | Technical team (TODO 95.2) |
| Manual accessibility run (TODO 37.13g), due when the new rules are built | Section 4 | Technical team |
| Rules decided 2026-10-11 that the system does not yet enforce: recount end date and request, unopposed seat and reopened nominations, paper counts, declared conflicts, extend polling, key copy count and holders, nominee consent date, ten-year purge request. They bind officials as procedure now. | Sections 5 and 6, phases 3, 5, 9, 10, 13, 14 | Technical team (TODO 37.12o, 37.12r, 37.12s, 37.14a, 37.1p, 37.16e) |
| Key destruction list on the officials dashboard. Today the dashboard has a "key file destroyed" box and no list of elections whose key is still held. The procedure needs the Returning Officer to see, per election, the declare date, the recount end date and whether destruction is recorded. Not built. | Phase 14, section 13 | Technical team (TODO 37.12r) |
| ISO 27001 and ISO 27018 gaps | Section 4 | Executive Committee |
