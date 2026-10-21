# Election form inventory (draft for owner approval)

Draft of 2026-10-20 for docs/TODO.md items 37.12t and 37.12n, session F7 of `delivery-plan.md`. Nothing here is applied. No form, file, code or TODO number has been renumbered.

## How to read this table

Sources: `docs/Elections/01-Election-Regulations.md` section 18 (35 forms, called R below), templates `05`, `06` and `07` in `docs/Elections/` (40 forms, called T below), and `GHCAA.Infrastructure/Services/ElectionDocumentService.cs` (the platform). Platform codes follow the template numbers.

Phase numbering used for the new numbers, taken from the `ElectionPhase` enum in `GHCAA.Domain/Enums.cs`:

1 Announced, 2 Nomination, 3 Scrutiny, 4 Withdrawal, 5 CandidateList, 6 Campaign, 7 Polling, 8 Counting, 9 Declared, 10 Archived.

The Regulations do not group their 35 forms by stage. Section 18 is one flat list, so the phase of each form is set here from where it is used. The basis is section 5 of the Regulations (the timeline) and the platform phase enum. The number after the dot (nn) runs in order of use inside the phase. These are proposals, and the owner can move a form.

Matching rule: two entries are one row only when the title is the same or the content clearly agrees. Where only the title differs, the row says so in Notes and the match is marked *[to confirm]*.

The "Records" column names what the platform fills today (from `LoadTablesAsync`) or "none yet". Entries marked "proposed" are suggestions for the six forms in 37.12n and are not built.

## Inventory

| Proposed number | Form name | Regulations number | Template number | Platform code today | Records that fill it | Notes |
|---|---|---|---|---|---|---|
| ER-1.01 | Election Notice | ER-01 | 05 ER-01 | ER-01 (generic) | none yet. Proposed: `Election` Title, AnnouncedOn, NominationOpensOn, NominationClosesOn, ScrutinyOn, WithdrawalClosesOn, PollingOpensOn, PollingClosesOn | 37.12n. Template also has reference no., venue, campaign and silence periods, final list date: no stored field *[to confirm]* |
| ER-1.02 | Election Calendar | ER-02 | 05 ER-02 | ER-02 (generic) | none yet. Proposed: the `Election` dates above plus `RecountWindowEndsOn` | 37.12n. Filled in F2. Regulations section 14 says the recount end date is stated here. Template "Responsible" and "Completed" columns have no record *[to confirm]* |
| ER-1.03 | Commissioner Appointment | ER-03 | none | not built | none yet | Not in templates. Missing template. `ElectionAppointment` holds AppointedAt and AppointedByUserId *[to confirm whether it fills this]* |
| ER-1.04 | Commissioner Acceptance | ER-04 | 05 ER-03 | not built | none yet | Platform stores `ElectionAppointment` AcceptedAt but prints no form |
| ER-1.05 | Neutrality Declaration | ER-05 | 05 ER-04 | not built | none yet | Platform stores `ElectionAppointment` DeclarationSignedAt and DeclarationTextSnapshot but prints no form. Template 05 ER-09 also cites this form for candidates, see ER-2.04 |
| ER-1.06 | Conflict of Interest | ER-06 | 05 ER-05 | not built | none yet | F2 adds `ElectionConflictDeclaration` (delivery plan). Link to this form *[to confirm]* |
| ER-1.07 | Confidentiality Agreement | ER-07 | 05 ER-06 | not built | none yet | None |
| ER-1.08 | Preliminary Voter Roll Certification | ER-08 | 05 ER-07 | ER-07 | `VoterRoll` row count (`Election.VoterRoll`) in one body line. No table | Titles differ (Regulations: Preliminary Voter Roll). Match *[to confirm]*. Platform title is "Voter Roll Certification" |
| ER-1.09 | Voter Roll Objection | ER-09 | 05 ER-08 | not built | none yet | None |
| ER-2.01 | Candidate Nomination | ER-10 | 05 ER-09 | ER-09 | `Nominations` (candidate, seat, proposer, seconder, SubmittedAt), `Members` for names | Template embeds proposer and seconder fields |
| ER-2.02 | Candidate Consent | ER-11 | 05 ER-10 | ER-10 | `Nominations` (candidate, seat, SubmittedAt used as the consent date) | F2 adds `Nomination.ConsentAt`. Printed date is the filing time until then |
| ER-2.03 | Eligibility Declaration | ER-12 | none | not built | none yet | Not in templates. Missing template |
| ER-2.04 | Political Neutrality Declaration | ER-13 | none | not built | none yet | Not in templates. Missing template. Template 05 ER-04 (Declaration of Neutrality) may serve candidates, since 05 ER-09 attaches "Declaration (ER-04)" *[to confirm]* |
| ER-2.05 | Proposer Form | ER-14 | none | not built | none yet | Not in templates. Missing template. `Nominations` ProposerMemberId exists |
| ER-2.06 | Seconder Form | ER-15 | none | not built | none yet | Not in templates. Missing template. `Nominations` SeconderMemberId exists |
| ER-2.07 | Nomination Receipt | ER-16 | none | not built | none yet | Not in templates. Missing template. Template 05 ER-09 has a received-by block |
| ER-3.01 | Scrutiny Checklist / Report | ER-17 | 05 ER-11 | ER-11 (generic) | none yet. Proposed: `Nominations` plus `ScrutinyDecision` (Accepted, Reason, DecidedByUserId, DecidedAt) | 37.12n. Titles differ (Regulations: Scrutiny Report). Match *[to confirm]*. The seven checklist ticks (membership period, fee, documents and others) have no record. The code comment says it needs the scrutiny rules |
| ER-3.02 | Nomination Acceptance Order | none | 05 ER-12 | not built | none yet. `ScrutinyDecision` could fill it *[to confirm]* | Not in Regulations |
| ER-4.01 | Candidate Withdrawal | ER-18 | 05 ER-13 | ER-13 | `Nominations` where Status is Withdrawn (candidate, seat, WithdrawnAt) | Regulations title: Withdrawal Form. Platform title: Withdrawal Record |
| ER-5.01 | Final Candidate List | ER-19 | 05 ER-14 | ER-14 | `Nominations` where Status is Accepted (seat, BallotOrder, candidate) | Template has an Election Symbol column. No stored field *[to confirm]* |
| ER-6.01 | Campaign Undertaking | ER-20 | none | not built | none yet | Not in templates. Missing template |
| ER-6.02 | Election Complaint Form | none | 07 ER-32 | not built | none yet | Not in Regulations. Phase set from Code of Conduct section 16 and Regulations line 530 *[to confirm]*. No complaint entity found |
| ER-6.03 | Election Investigation Report | none | 07 ER-33 | not built (platform ER-33 prints a Handover Certificate) | none yet | Not in Regulations. Code and title mismatch, see ER-10.02 and Decisions |
| ER-7.01 | Observer Appointment | ER-21 | none | not built | none yet | Not in templates. Missing template |
| ER-7.02 | Polling Officer Appointment | ER-22 | none | not built | none yet | Not in templates. Missing template. `ElectionAppointment` holds appointments *[to confirm]* |
| ER-7.03 | Election Material Receipt Register | none | 07 ER-39 | not built | none yet | Not in Regulations. Handbook Annex A lists it second in the print order |
| ER-7.04 | Poll Opening Certificate | ER-23 | 05 ER-15 | not built | none yet | The platform poll record has `PollingOpensOn`, printed only on ER-19 |
| ER-7.05 | Poll Incident Register | none | 05 ER-16 | not built | none yet | Not in Regulations |
| ER-7.06 | Spoiled Ballot Register | none | 05 ER-17 | not built | none yet | Not in Regulations. `Ballots` IsSpoiled is counted on ER-23 *[to confirm whether it fills this]* |
| ER-7.07 | Poll Closing Certificate | ER-24 | 05 ER-18 | not built | none yet | The platform poll record has `PollingClosesOn`, the eligible count and the `VoterRolls` VotedAt count, printed only on ER-19 |
| ER-7.08 | Ballot Box Sealing and Poll Integrity Certificate | ER-25 | 06 ER-19 | ER-19 | Poll record: `Election` PollingOpensOn, PollingClosesOn, BallotKeyFingerprint; `VoterRolls` IsEligible, VotedAt; `ElectionAuditEntries` chain check. Appointed officials from the election board (`ElectionAppointment`) | Seal numbers and supervision ticks stay manual |
| ER-7.09 | Electronic Voting Audit | ER-26 | none | not built | none yet. `ElectionAuditEntries` (Seq, Hash chain) is the likely source *[to confirm]* | Not in templates. Missing template. Not the same as template ER-26 (Recount Request) |
| ER-8.01 | Vote Counting Authorization Certificate | none | 07 ER-20 | not built | none yet | Not in Regulations |
| ER-8.02 | Ballot Box Opening Certificate | none | 07 ER-21 | not built | none yet | Not in Regulations |
| ER-8.03 | Ballot Paper Reconciliation Sheet | none | 07 ER-22 | not built | none yet | Not in Regulations. F2 adds `ElectionPaperCount` (delivery plan) *[to confirm whether it fills this]* |
| ER-8.04 | Vote Counting Sheet | ER-27 | 07 ER-23 | ER-23 | `Ballots` (cast, spoiled), `ElectionResults` (VoteCount per nomination), `BallotVotes` for the abstain row | Regulations title: Vote Counting Register. Match *[to confirm]* |
| ER-8.05 | Independent Tally Sheet | none | 07 ER-24 | not built | none yet | Not in Regulations |
| ER-8.06 | Rejected Ballot Register | none | 07 ER-25 | not built | none yet | Not in Regulations |
| ER-8.07 | Final Result Sheet | none | 07 ER-29 | ER-29 | `ElectionResults`, `Election` TieRule and DeclaredOn, audit chain head hash from the election board | Not in Regulations. Regulations ER-29 is Result Certification, the next row, so the same code means different forms |
| ER-8.08 | Result Certification | ER-29 | 07 ER-30 | ER-30 | `ElectionResults`, `Election` TieRule and DeclaredOn | Match by title *[to confirm]* |
| ER-9.01 | Result Declaration | ER-30 | 07 ER-31 | ER-31 | `ElectionResults`, `Election` TieRule and DeclaredOn | None |
| ER-9.02 | Recount Request | ER-28 | 07 ER-26 | ER-26 (generic) | none yet. Proposed: `ElectionRecountRequest` (seat, candidate, reason, filed at, status, decided by, decided at) | 37.12n. Filled in F2. The Regulations and the delivery plan call it ER-28. The platform ER-28 is the Recount Report |
| ER-9.03 | Recount Decision | none | 07 ER-27 | not built | none yet. `ElectionRecountRequest` decided by, decided at and status would carry it | Not in Regulations. F2 keeps the decision in the request row |
| ER-9.04 | Recount Report | none | 07 ER-28 | ER-28 (generic) | none yet. Proposed: `ElectionResults` for the original count plus the recount result kept by F2 | 37.12n. Filled in F2. Not in Regulations. Where the second count is stored *[to confirm]* |
| ER-9.05 | Appeal Form | ER-31 | 07 ER-34 | not built | none yet | Template title: Appeal to Election Appeals Tribunal. Match *[to confirm]* |
| ER-9.06 | Appeal Decision / Tribunal Decision | ER-32 | 07 ER-35 | not built | none yet | Titles differ. TODO 37.12t lists Tribunal Decision as not in the Regulations. Match with R ER-32 Appeal Decision *[to confirm]* |
| ER-10.01 | Election Completion Report | ER-33 | 07 ER-36 | not built | none yet | None |
| ER-10.02 | Executive Committee Handover Certificate | ER-34 | 07 ER-37 | ER-33 | none yet. Proposed: `ElectionResults` where IsElected, `Election` DeclaredOn, `ECPeriod` *[to confirm fields]* | 37.12n lists ER-33 as generic. The platform ER-33 is titled "Handover Certificate", but template ER-33 is the Investigation Report and the Handover Certificate is template ER-37. Handbook Annex B lists ER-33 as printed blank. Phase could be 9 *[to confirm]* |
| ER-10.03 | Election Archive Register | ER-35 | 07 ER-38 | not built | none yet | None |
| ER-10.04 | Election Closure Certificate | none | 07 ER-40 | not built | none yet | Not in Regulations |

## Counts

- Rows: 50 distinct forms.
- In both sets: 25. Only in the Regulations: 10. Only in the templates: 15.
- Checks: 25 + 10 = 35 Regulations forms. 25 + 15 = 40 template forms.
- Built in the platform: 16 codes in `SupportedForms`. Nine print stored records (ER-09, 10, 13, 14, 19, 23, 29, 30, 31). ER-07 prints a voter count in one line. Six are generic (ER-01, 02, 11, 26, 28, 33).
- TODO 37.12t names four forms with no template and three not in the Regulations. This table finds ten and fifteen. The six Regulations-only forms beyond the TODO list are ER-2.03, 2.04, 2.07, 6.01, 7.02 and 7.09. The template-only forms beyond the TODO list are the rows marked "Not in Regulations" other than Recount Decision, Complaint Form and Tribunal Decision (Tribunal Decision is matched to Appeal Decision here).

## Decisions for the owner

1. Approve the phase numbering above (platform enum, 1 to 10) and the phase given to each form. Least certain placements: Result Certification (8) against Result Declaration (9), Complaint Form and Investigation Report (6), Handover Certificate (10 or 9), Observer and Polling Officer Appointment (7).
2. Keep or drop the 15 template-only forms. TODO 37.12t counted three of them. If the Regulations are master, each needs adding to the Regulations or leaving out of the new numbering.
3. Write or drop the ten Regulations-only forms: Commissioner Appointment, Eligibility Declaration, Political Neutrality Declaration, Proposer, Seconder, Nomination Receipt, Campaign Undertaking, Observer Appointment, Polling Officer Appointment, Electronic Voting Audit. Proposer and Seconder fields already sit inside the Candidate Nomination template.
4. Confirm the title-only matches: R ER-08 with 05 ER-07, R ER-17 with 05 ER-11, R ER-27 with 07 ER-23, R ER-29 with 07 ER-30, R ER-31 with 07 ER-34, R ER-32 with 07 ER-35. Also whether 05 ER-04 covers the candidate neutrality declaration.
5. Settle the recount and handover codes. The Regulations and the delivery plan use ER-28 for the request. The platform uses ER-26 for the request, ER-28 for the report and ER-33 for the handover. The new numbers remove the clash, but the F2 code and the old-number lookup need one agreed rule.
6. 37.12n, ER-01 and ER-02: approve the proposed `Election` date fields and decide the fields with no record (reference number, venue, symbols, responsible person, completed flag).
7. 37.12n, ER-11: decide whether the seven checklist ticks need new stored fields, or the form prints the `ScrutinyDecision` only and leaves the ticks manual.
8. 37.12n, ER-26 and ER-28: approve `ElectionRecountRequest` for the request and decision, and say where the recount result is stored for the report.
9. 37.12n, ER-33: decide whether it means the Investigation Report (template number, no complaint record exists) or the Handover Certificate (what the platform prints). Then approve the proposed handover records.
10. Decide whether forms whose records already exist but have no form (Commissioner Acceptance and Neutrality Declaration from `ElectionAppointment`, Poll Opening and Closing from the poll record) are built with this work or later.

## Owner decisions, 2026-10-20

The owner reviewed all 50 forms drawn as A4 sheets and approved the layouts as shown. The review page puts each template layout under its Regulations number and drafts the ten Regulations-only forms.

- Numbering. The Regulations numbers ER-01 to ER-35 are used as they stand. This replaces the ER-<phase>.<nn> scheme decided on 2026-10-11 and the phase table above. The 15 template-only forms take a letter after the form they follow:

  | Form | Template | New code |
  |---|---|---|
  | Nomination Acceptance Order | 05 ER-12 | ER-17A |
  | Election Material Receipt Register | 07 ER-39 | ER-22A |
  | Poll Incident Register | 05 ER-16 | ER-23A |
  | Spoiled Ballot Register | 05 ER-17 | ER-23B |
  | Vote Counting Authorization | 07 ER-20 | ER-25A |
  | Ballot Box Opening Certificate | 07 ER-21 | ER-25B |
  | Ballot Paper Reconciliation | 07 ER-22 | ER-25C |
  | Independent Tally Sheet | 07 ER-24 | ER-27A |
  | Rejected Ballot Register | 07 ER-25 | ER-27B |
  | Final Result Sheet | 07 ER-29 | ER-27C |
  | Recount Decision | 07 ER-27 | ER-28A |
  | Recount Report | 07 ER-28 | ER-28B |
  | Election Complaint Form | 07 ER-32 | ER-30A |
  | Investigation Report | 07 ER-33 | ER-30B |
  | Election Closure Certificate | 07 ER-40 | ER-35A |

- Old codes. Superseded 2026-10-22: the owner chose new codes only. The 16 platform codes used before are dropped with no aliases, and the document endpoint and the verify page answer 404 for them. A QR code on a form printed before the change no longer resolves, which the owner accepted.
- Keep all 15 template-only forms and write all ten Regulations-only forms to the drafted layouts.
- Title-only matches confirmed: ER-08 with 05 ER-07, ER-17 with 05 ER-11, ER-27 with 07 ER-23, ER-29 with 07 ER-30, ER-31 with 07 ER-34, ER-32 with 07 ER-35.
- ER-13 Political Neutrality keeps its number. Its body reuses the five ER-05 neutrality lines plus the non-political lines. Candidates sign ER-13 and officials sign ER-05.
- ER-01 and ER-02: store on the election the core schedule (announced, nomination opens and closes, scrutiny, withdrawal deadline, final list, polling opens and closes), campaign opens and closes, the silence period, the polling venue, a notice reference number and the recount window end.
- ER-17: store the seven scrutiny ticks on each nomination. Any commissioner may set them, and the sheet prints who ticked and when. The Chief still decides acceptance.
- Recount: one stored request per filing holds the seat, applicant, grounds, explanation and time received, then the decision, reason, who decided and when. ER-28 prints the request, ER-28A the decision, and ER-28B compares the original count with the recount.
- Handover moves to ER-34 with the 15-item checklist from 07 ER-37, and the ticks are stored. ER-33 becomes the Completion Report.
- Signers for the drafts stay as drawn: ER-03 by the Selection Committee chair and one member. ER-20 stays separate from ECC-01.
- Build ER-04, ER-05, ER-23 and ER-24 from the records that already exist, in this work.
- The migration for the new fields is written and tested on local PostgreSQL only. It goes to preprod after the F2 gate clears (34.D17 to 34.D19 confirmed there).
- Committee positions are rows in a new `EcPosition` table, not the fixed enum (TODO 95.2, the configurable positions item). Each row carries three flags: shown on the board, signs as head and signs as secretary. A signature block such as the outgoing and incoming head on ER-34 or the association head and secretary on ER-35A prints the label of the position holding that flag, never a fixed "President" or "General Secretary". The same rule covers ID cards. This is built before the forms, and its migration joins the form-fields migration under the same local-only rule.
- Every label a form prints about the organisation comes from org config: the full name, address, phone numbers, email and logo. Nothing about the organisation is fixed in code (TODO 37.14s, form wording from settings).
- Any of the 50 forms can be printed at any time, either filled from the stored records of an election or blank with ruled lines for hand filling (TODO 37.14s).
