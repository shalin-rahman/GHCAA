# Tasks: Alumni Programs and Credential Verification

**Input**: [spec.md](./spec.md)

**Purpose**: Track delivery of the five independent stories in spec.md. Every
task below is future implementation work — none of it exists in the codebase
yet. This file authorizes planning and sequencing, not a claim of completion.

## Phase 0: Prerequisite (blocks every story below)

- [ ] T001 Confirm 37.0's migration path is live and verified against a
  non-empty database before starting any story in this spec (`docs/TODO.md`
  37.0/37.11; `gotcha_ensurecreated_no_op_existing_db`).

## Phase 1: Story 1 — Public credential verification (P1)

- [ ] T002 Add `CredentialType` enum and `IssuedCredential` model; extend
  `IIDCardService`/its implementation to record one per generated document
  and embed a QR pointing at `/verify/{shortCode}` (docs/TODO.md 37.8).
- [ ] T003 Add `GET api/verify/{shortCode}` (`[AllowAnonymous]`, rate-limited)
  returning only `{ valid, memberName, membershipType, issuedOn, status }`.
- [ ] T004 Add admin revocation action; add public `/verify/:code` and
  `/verify` pages behind `enableCredentialVerification`.
- [ ] T005 NUnit: revoked code returns `valid: false`; response DTO has no
  contact field; short codes unique across 10k generations. Vitest: three
  verdict states, unknown-code path.

## Phase 2: Story 2 — Annual impact report (P2)

- [ ] T006 Extend `IFinancialLedgerService` with
  `GetImpactReportAsync(int year, CancellationToken)` aggregating finance,
  scholarships, campaigns, events, and membership, null-tolerant per source
  (docs/TODO.md 37.10).
- [ ] T007 Add `GET api/financials/impact/{year}` and
  `GET api/financials/impact/{year}/pdf` (`[AllowAnonymous]`); generate the
  PDF via QuestPDF following `IDCardService.GenerateIDCardPdfAsync`.
- [ ] T008 Add public `/impact/:year` reusing the Work Package 36
  `.doc-hero`/`.doc-prose` shell; admin foreword/cover/publish action; store
  foreword and cover in the existing `SiteContent` CMS. Flag
  `enableImpactReport`.
- [ ] T009 NUnit: zero-record year returns zeroed sections, not null;
  category totals match a hand-summed fixture; disbursed-scholarship total
  equals the sum of linked `Grant` records. Vitest: year selector,
  empty-section rendering.

## Phase 3: Story 3 — In Memoriam register (P3)

- [ ] T010 Add `MemorialEntry` (reusing `SubmissionStatus`) and `Condolence`
  models; wire condolence storage through the existing `HtmlSanitizer`
  (docs/TODO.md 37.5).
- [ ] T011 Implement the deactivate-on-link transaction: setting
  `MemorialEntry.MemberId` deactivates the member and removes them from the
  public directory and any open voter roll in one operation.
- [ ] T012 Add `MemorialController` (`GET api/memorial/public`
  `[AllowAnonymous]`); public `/in-memoriam`, member submission form, admin
  moderation queue. Flag `enableMemorial`.
- [ ] T013 NUnit: unapproved entry absent from public projection; linking a
  member deactivates and removes them from directory results; condolence HTML
  sanitized. Vitest: moderation-queue actions, published/unpublished
  rendering.

## Phase 4: Story 4 — Scholarship & student-aid programme (P4)

- [ ] T014 Add `ScholarshipApplicationStatus`/`DisbursementStatus` enums and
  `ScholarshipFund`/`ScholarshipCall`/`ScholarshipApplication`/
  `ScholarshipDocument`/`ScholarshipReview`/`ScholarshipAward` models, reusing
  `FileUpload`/`IFileValidationService` for documents (docs/TODO.md 37.2).
- [ ] T015 Implement the blind-review projection (`GetApplicationForReviewAsync`
  omits all identifying fields) and the ledger tie-in (paid award writes one
  `Grant` `FinancialRecord`, idempotent on repeat).
- [ ] T016 Add `ScholarshipsController`; public `/scholarships` (listing,
  apply, anonymous rate-limited status check by reference code), member
  `portal/scholarships` reviewer queue, admin `admin/scholarships`. Flag
  `enableScholarships`.
- [ ] T017 NUnit: review DTO carries no identifying field; application
  rejected outside the call window; award-paid writes exactly one `Grant`
  record and is idempotent. Vitest: apply-form validation, unknown-code
  status lookup.

## Phase 5: Story 5 — Geographic chapters (P5)

- [ ] T018 Add `Chapter` and `ChapterMembership` models (unique index on
  `(ChapterId, MemberId)`); add nullable `ChapterId` to `AlumniEvent` rather
  than a separate chapter-event table (docs/TODO.md 37.9).
- [ ] T019 Add `ChaptersController` (`GET api/chapters/public`
  `[AllowAnonymous]`); wire chapter announcements through the existing
  `INotificationService.CreateNotificationAsync` fan-out.
- [ ] T020 Add public `/chapters`, member `portal/chapters` (join/leave, my
  chapter feed), admin `admin/chapters`. Flag `enableChapters`.
- [ ] T021 NUnit: joining twice does not duplicate; a chapter event appears
  only in that chapter's feed; coordinator contact hidden unless
  `ContactEmail` is set. Vitest: join/leave state, empty-chapter state.

## Phase 6: Cross-cutting closeout (per 37.11, repeated for each story as it lands)

- [ ] T022 Re-run `dotnet test`, `npx vitest run`, `npm run type-check`,
  `npx ng build` after each story lands; verify its new table(s) against a
  non-empty database, not only a fresh one.
- [ ] T023 Update `docs/FEATURES.md`, `docs/PROJECT_MAP.md`, `docs/SRS.md`,
  and `docs/ARCHITECTURE.md` as each story lands, per
  `feedback_docs_update_scope`.
- [ ] T024 Run `graphify update .` after this specification's artifacts are
  created, and again after each story's implementation lands.

## Maintenance rules

- [ ] T025 When any of 37.2/37.5/37.8/37.9/37.10 changes status in
  `docs/TODO.md`, update the corresponding story/task status here in the same
  change.
