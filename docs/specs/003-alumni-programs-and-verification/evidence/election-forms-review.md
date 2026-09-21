# Election forms review

**Reviewed**: 2026-09-21

## Finding

The repository already contains a broad Election Commission handbook in
`docs/Elections/05-Election-Forms-and-Templates.md`,
`06-Election-Ballot-Seal-and-Poll-Integrity-Certificate.md`, and
`07-Election-Vote-Counting-Authorisation.md`. The Web public election page
also groups the handbook forms by election stage and provides print and
download actions.

These documents are useful as the legal and operational template set. They
should not be replaced by a second, smaller set in the election module.

## Forms needed by the persisted engine

| Workflow | Existing form coverage | Digital engine action |
|---|---|---|
| Election announcement and calendar | ER-01, ER-02 | Fill from the persisted election schedule |
| Officer appointment and duties | ER-03, ER-04, ER-05, ER-06 | Fill from officer, role, conflict, and acknowledgement records |
| Voter-roll preparation | ER-07, ER-08 | Generate from the frozen roll and retained objections |
| Nomination and candidate consent | ER-09, ER-10 | Combine with the online nomination, candidate photo, signature/consent evidence, and receipt |
| Nomination scrutiny | ER-11, ER-12 | Generate from scrutiny decisions, officer identity, reason, and timestamp |
| Withdrawal | ER-13 | Generate from the accepted withdrawal record |
| Candidate list and campaign | ER-14, ER-15, ER-16 | Generate from accepted nominations and published campaign material |
| Polling and ballot custody | ER-17, ER-18, ER-19, ER-20, ER-21, ER-22 | Retain for manual or hybrid polling; map digital polling events to an audit record |
| Counting and recount | ER-23 through ER-30 | Generate from count, rejected-ballot, recount, and certification records |
| Declaration and complaints | ER-31 through ER-34 | Generate declaration, handover, complaint, and decision records |

The exact form numbers must be verified against the complete handbook before
the PDF generator is implemented. A form must not be marked generated when the
source record does not contain every field printed on it.

## Required digital additions

The handbook is written for completed paper forms. The persisted engine needs
structured records in addition to those templates:

1. A candidate evidence record that stores the validated photo, signed
   consent/declaration evidence, upload metadata, and reviewer decision.
2. A voter-roll snapshot certificate containing the roll hash, eligibility
   rule version, freeze timestamp, and approving officer.
3. A ballot issuance and vote-integrity record that proves a ballot was issued
   and accepted without linking the selected nomination to the voter identity.
4. A count and recount audit record containing operator, authorization,
   input/result hashes, timestamps, and reason for each rerun.
5. A result verification record for the public QR or verification URL.

These are records and generated outputs, not additional hand-written forms.

## Formatting requirements

- Generated documents must render as printable A4 documents with the
  institution profile's letterhead, form code, election reference, generation
  timestamp, page number, and verification data.
- Signature areas must be explicit. A typed name must not be presented as a
  signature unless the configured signing method records consent and evidence.
- Checkbox fields must remain unambiguous in print and PDF.
- Dates in generated documents must use the display format `dd-MM-yyyy`;
  persisted and API values remain ISO date/timestamp values.
- Downloads must be PDF or an equivalent fixed-layout document for official
  records. Raw Markdown is suitable for the public handbook source, not for a
  completed official form.
- The Web preview may remain useful for reading and printing, but it is not
  evidence that a filled PDF generator exists.

## Current gap

The current implementation publishes static Markdown documents and exposes
election data, nominations, voting, counting, and declaration. It does not yet
fill the handbook forms from persisted election records, generate official
PDFs, store signed candidate consent evidence, or expose verification records.
Work Package 37.1 must retain these as explicit acceptance gates.
