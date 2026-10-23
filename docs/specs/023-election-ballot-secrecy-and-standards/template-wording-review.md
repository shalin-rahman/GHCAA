# Template form wording review

Status: approved by the owner and applied on 2026-10-23 (TODO 37.16a) to
`profiles/default/election-content/forms.json` and to the same forms in `docs/Elections` files 05
and 07. Owner choices on the open rows: 27A-1 keep three counters, and manual 04 section 12 (Vote
Counting Procedure) now says three; 27B-3 the ER-27 and ER-27C line is renamed "Spoiled Ballots"
and ER-27B keeps "Rejected"; 35A-3 the financial accounts line is dropped; G1 "Polling Venue".
Every other row was applied as proposed.

These 11 forms have no stored record behind them, so the platform prints them blank for hand
(owner decision 2026-10-23, TODO 37.14q, the remaining election forms). This review compares each
form's wording in forms.json with the rules in `docs/Elections`. Each row is one proposed change.
The owner approves, edits or drops each row. Approved rows are then applied to forms.json in one
change, together with any election doc in `docs/Elections` that prints the same form text.

Labels that the platform fills in other forms (TODO 37.14q field values) are matched by their exact
text. None of the labels below is used that way, so renaming them changes no filled form.

## Changes that apply to several forms

| # | Forms | Now | Proposed | Why |
|---|---|---|---|---|
| G1 | ER-22A, ER-23A, ER-23B, ER-25A, ER-25C | Header "Polling Station" | "Polling Venue" | ER-01 and ER-02 use "Polling Venue", and the platform stores one venue per election (`Election.Venue`). Keep "Polling Station" only if the owner expects more than one station per election. |
| G2 | ER-22A, ER-25A, ER-25B, ER-25C, ER-27A, ER-27B, ER-30A | Header "Election" with no reference | Add "Election Reference" next to "Election" | ER-35A already asks for the election reference. The same reference on every sheet ties loose pages to one election when they are filed. |
| G2a | ER-23A, ER-23B | No election in the header at all | Add "Election" and "Election Reference" | Same reason as G2. These two sheets only name the station and date. |
| G3 | ER-23A, ER-25A, ER-25B, ER-30A | "Time", "Time Counting Commenced", "Opened At (time)" | Add "(24-hour)" to each label | The filled forms print times as `yyyy-MM-dd HH:mm UTC`. Hand entries in 24-hour form are easier to check against them. |

## Per form

### ER-22A Election Material Receipt Register

| # | Now | Proposed | Why |
|---|---|---|---|
| 22A-1 | Table has 4 blank rows | 12 blank rows | `04-Election-Manual-Ballot.md` section 3 (Polling Materials) lists 22 items. Four rows force several sheets per station. |
| 22A-2 | Column "Condition" | "Condition on receipt" | Says when the condition was judged. |

### ER-23A Poll Incident Register

| # | Now | Proposed | Why |
|---|---|---|---|
| 23A-1 | Fields "Incident No.", "Time" | Add "Category" checklist: Name missing, Duplicate voting attempt, Disturbance, Unauthorised campaigning, Security concern, Power failure, Other | Taken from `04-Election-Manual-Ballot.md` section 8 (Special Cases) and the examples in section 9 (Incident Handling). Ballot damage is left out because section 8 sends it to the Spoiled Ballot Register (ER-23B). |
| 23A-2 | "Reported To" | "Reported to (name and role)" and "Time reported" | Section 8 says a duplicate attempt is reported to the Commission at once, and disorder to the Chief Election Commissioner. The time shows it was done. |

### ER-23B Spoiled Ballot Register

| # | Now | Proposed | Why |
|---|---|---|---|
| 23B-1 | Column "Reason" | Keep, and add a line under the table: "Mark the original SPOILED and keep it." | `04-Election-Manual-Ballot.md` section 8 (Special Cases, Damaged Ballot) requires both. |
| 23B-2 | Totals "Total Spoiled Ballots", "Total Replacements Issued" | Keep | These two totals feed ER-25C "Spoiled ballots". No change needed. |

### ER-25A Vote Counting Authorization Certificate

| # | Now | Proposed | Why |
|---|---|---|---|
| 25A-1 | Title "Authorization" | "Authorisation" | The file is `07-Election-Vote-Counting-Authorisation.md`, and the form's own declaration already says "authorises". |
| 25A-2 | Checklist item "Ballot Box Sealing Certificate completed" | "Ballot Box Sealing & Poll Integrity Certificate (ER-25) completed" | That is the title of form ER-25 in `06-Election-Ballot-Seal-and-Poll-Integrity-Certificate.md`. |

### ER-25B Ballot Box Opening Certificate

| # | Now | Proposed | Why |
|---|---|---|---|
| 25B-1 | Header "Seal No. 1/2/3" | Keep, and add "Seal numbers match ER-25 (the sealing certificate)" to the Inspection checklist | Checking the numbers against the earlier record is the point of writing them down. |
| 25B-2 | Signature "Candidate Agent" | "Candidate Agent (if present)" | `04-Election-Manual-Ballot.md` section 12 (Vote Counting Procedure) says agents "may observe". A blank signature should not read as a missing step. |

### ER-25C Ballot Paper Reconciliation Sheet

| # | Now | Proposed | Why |
|---|---|---|---|
| 25C-1 | Row "Difference" | "Difference (ballots issued minus ballots found in box)" | The sheet does not say which two rows are compared. |
| 25C-2 | Row "Unused ballots" | Add a check row "Received = issued + unused + spoiled" | Closes the account of paper received, which is what ER-22A records. |

### ER-27A Independent Tally Sheet

| # | Now | Proposed | Why |
|---|---|---|---|
| 27A-1 | Three counters (A, B, C) | Owner to choose: keep three, or two to match the manual | `04-Election-Manual-Ballot.md` section 12 (Vote Counting Procedure) says "Independent tally maintained by second officer", which is two counts. |
| 27A-2 | Fixed row "Rejected" | Keep, and add a row "Abstain (seat left empty)" | The platform's own count sheet (ER-27) prints an Abstain row per seat. A paper count should account for the same ballots. |

### ER-27B Rejected Ballot Register

| # | Now | Proposed | Why |
|---|---|---|---|
| 27B-1 | Grounds: No official stamp, Multiple selections, No clear intention, Damaged, Identifiable markings, Other | No official stamp; No official signature (if required); Marks for multiple candidates where only one permitted; No clear voter intention; Writing identifies voter; Ballot torn so vote unclear; Ballot altered; Other | This is the list in `04-Election-Manual-Ballot.md` section 13 (Invalid Ballots), word for word. Two grounds are missing now (signature, altered), and "Damaged" is wider than the rule, which only rejects a ballot torn so the vote is unclear. |
| 27B-2 | No storage line | Add "Rejected ballots placed in the Invalid Ballot Envelope" | Section 13 says rejected ballots are stored separately, and section 3 (Polling Materials) lists the Invalid Ballot Envelope. |
| 27B-3 | Title "Rejected" | Keep | The platform now prints spoiled ballots as "Rejected Ballots" on ER-27 and ER-27C (owner rule 2026-10-23). The owner may want one word for both. This row only flags it. |

### ER-30A Election Complaint Form

| # | Now | Proposed | Why |
|---|---|---|---|
| 30A-1 | Checklist "Nature of Complaint": Campaign, Polling, Counting, Result, Other | Add "Nomination" and "Code of Conduct" | `01-Election-Regulations.md` section 5 (Election Timeline) requires every open complaint on nominations to be decided, and `03-Election-Code-Of-Conduct.md` section 16 (Complaints) covers conduct breaches. |
| 30A-2 | Text "Evidence attached" | Checklist: Witness statements, Photographs, Video, Electronic records, Other, with a count for each | These are the evidence types in `03-Election-Code-Of-Conduct.md` section 16. |
| 30A-3 | Signature "Received By (Commission officer)" | Add "Acknowledged on (date)" | Section 16 says complaints are acknowledged and registered. |

### ER-30B Election Investigation Report

| # | Now | Proposed | Why |
|---|---|---|---|
| 30B-1 | "Submitted To" | Add a "Decision" field and "Decided on" | Section 16 lists "Decided" and "Documented" as steps after investigation. The report has no place for the outcome. |
| 30B-2 | "Evidence reviewed" and "Documents reviewed" | Merge into "Evidence and documents reviewed" | The two sections overlap, and ER-30A lists documents as evidence. |

### ER-35A Election Closure Certificate

| # | Now | Proposed | Why |
|---|---|---|---|
| 35A-1 | "All appeals have concluded." | "All appeals have concluded, and the 7-day appeal period has passed." | `01-Election-Regulations.md` section 16 (Appeals) sets 7 days to appeal and 14 days for the tribunal. Closing before the period ends would cut off an appeal. |
| 35A-2 | "Election records have been archived." | "Election records have been archived for at least ten years, as Regulations section 17 (Election Records) requires." | `01-Election-Regulations.md` section 17 lists what must be kept and sets the ten-year minimum. |
| 35A-3 | "Financial accounts have been closed." | Keep | No rule in `docs/Elections` names these accounts. Owner to confirm the line is wanted. |

## Not proposed

- No form gets a loader. These forms record paper events (seals, boxes, counters) that the platform
  does not store.
- Signature lines stay as they are, apart from 25B-2. Who signs is set by the handbook roles in
  `docs/Elections/12-Election-Officials-Handbook.md`, Annex B (the form list).
