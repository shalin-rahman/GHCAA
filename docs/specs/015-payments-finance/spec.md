# Feature Specification: Payments, Finance, Campaigns and Scholarships

**Feature Branch**: `015-payments-finance`
**Created**: 2026-09-25
**Status**: As-built baseline
**Input**: Reverse-engineered from the implemented code
**Depends on**: [001-platform-baseline](../001-platform-baseline/spec.md)

## Purpose and scope

Covers the six controllers that move money and track it: `FinancialsController` (member
payments, dues, fee config, saved payment methods), `FinancialLedgerController` (superadmin
income/expense ledger), `GatewaysController` (payment gateway initiation, callbacks, webhook),
`PaymentConfigController` (admin-configurable gateway/method settings), `CampaignsController`
(fundraising campaigns and pledges), `ScholarshipsController` (funds, calls, applications,
review, awards, disbursement).

Out of scope: event registration and its fee logic beyond the payment link
(see [003-alumni-programs-and-verification](../003-alumni-programs-and-verification/spec.md)),
membership induction workflow itself (see 001-platform-baseline), OTP/step-up mechanics
(defined once in `GHCAA.API/Filters/RequireStepUpAttribute.cs`, referenced here only by name),
and the archive/oral-history domain (see
[009-campaigns-scholarships-archive](../009-campaigns-scholarships-archive/spec.md), whose
campaign and scholarship sections this spec supersedes with controller-level detail).

Related specs consulted as context, not repeated here: 002-workflow-contracts-and-validation
(`evidence/payments-domain.md`), 009-campaigns-scholarships-archive, 003-alumni-programs-and-
verification, 001-platform-baseline (`evidence/implementation-feature-catalog.md`, Financials
section).

## User Scenarios & Testing

### User Story 1 - Member pays and gets a receipt (P1)

**Why this priority**: payment collection is the domain's core purpose; without it, dues,
event fees and donations cannot be tracked.

**Independent Test**: sign in as a member, call `POST /api/financials/record-payment` with a
manual transaction, then `GET /api/financials/receipt/{paymentId}` and confirm a PDF returns.

**Acceptance Scenarios**:
1. **Given** a signed-in member, **When** they POST a payment with `MemberId` omitted from the
   body, **Then** the system stamps `MemberId` from the auth claim, ignoring any value the
   client sent.
2. **Given** a completed payment owned by the member, **When** they GET the receipt, **Then**
   the system returns a generated PDF.
3. **Given** a payment owned by another member, **When** a non-admin member requests its
   receipt, **Then** the system returns 403 Forbidden.

### User Story 2 - Gateway payment auto-approves the thing it paid for (P1)

**Why this priority**: gateway callbacks are the only unattended path in the domain; a bug
here silently blocks or double-processes money.

**Independent Test**: initiate a gateway payment for an event registration, simulate the
SSLCommerz success callback with a matching amount, and confirm the registration is approved
exactly once.

**Acceptance Scenarios**:
1. **Given** a gateway payment with `FinancialCategory.RegistrationFee`, **When** the callback
   reports success with an amount that matches the recorded amount, **Then** the system marks
   the payment `Completed` and auto-approves the linked event registration.
2. **Given** a gateway payment already marked `Completed` with a stamped `GatewayPaymentId`,
   **When** the same callback fires again, **Then** the system short-circuits and performs no
   further side effects.
3. **Given** a callback reporting a confirmed amount that does not match the recorded amount,
   **When** the orchestrator runs, **Then** the system marks the payment `Failed` and performs
   no approval.
4. **Given** a gateway payment with `FinancialCategory.MembershipFee`, **When** it completes,
   **Then** the system auto-inducts the member, and a payment for any other category never
   triggers induction.

### User Story 3 - Admin manages the ledger and gateway configuration (P2)

**Why this priority**: superadmins need a source of truth for income/expense and control over
which gateways are live, but this is lower-frequency than member-facing payment flows.

**Independent Test**: as SuperAdmin, add a ledger record with step-up, then toggle a payment
config's `IsEnabled` flag and confirm `GET /api/payment-config/active` reflects the change.

**Acceptance Scenarios**:
1. **Given** a SuperAdmin without a fresh step-up token, **When** they call
   `POST /api/ledger`, **Then** the system rejects the request until step-up is satisfied.
2. **Given** a non-SuperAdmin admin, **When** they call any `/api/ledger` route, **Then** the
   system returns 403 Forbidden.
3. **Given** a payment config toggled to disabled, **When** a member requests
   `GET /api/payment-config/active`, **Then** the disabled method is absent from the list.

### User Story 4 - Public donates to a campaign and sees the honour roll (P2)

**Why this priority**: fundraising is member/guest-facing but not required for the platform's
core membership operations to function.

**Independent Test**: as a guest, call `POST /api/campaigns/{slug}/pledges` with a donor name,
then as admin confirm receipt, then `GET /api/campaigns/{slug}/honour-roll` and confirm the
pledge appears under the correct tier.

**Acceptance Scenarios**:
1. **Given** an anonymous caller with no `DonorName` in the pledge body, **When** they submit
   the pledge, **Then** the system rejects it with a 400 error.
2. **Given** a pledge with `IsAnonymous = true`, **When** the honour roll is fetched, **Then**
   the donor's real name never appears in the response.
3. **Given** a pledge whose receipt was already confirmed, **When** admin confirms it again,
   **Then** the system returns success without creating a second ledger entry.

### User Story 5 - Scholarship applicant applies, gets reviewed, and is awarded (P3)

**Why this priority**: scholarships run on a slower, seasonal cycle and reuse the same
finance primitives (ledger, disbursement) already covered above.

**Independent Test**: submit a scholarship application within an open call window, submit a
review as a reviewing member, create an award, and disburse it once.

**Acceptance Scenarios**:
1. **Given** a call whose `ClosesOn` date has passed, **When** an applicant submits, **Then**
   the system rejects the application.
2. **Given** an award already disbursed, **When** disbursement is requested again, **Then**
   the system returns success without creating a second `FinancialRecord`.
3. **Given** an award whose application status is `Cancelled`, **When** disbursement is
   requested, **Then** the system refuses it.

### Edge Cases

- A gateway callback with a confirmed amount of `0` is treated as a mismatch, not as "no
  amount to check" (only a `null` confirmed amount skips the check).
- `GenerateAnnualDuesAsync` skips members whose applicable fee for their `MembershipType` is
  zero, and skips a member/year pair that already has a due record.
- `FinancialLedgerService.GetRecordsAsync` clamps `page < 1` to `1`, `pageSize < 1` to `10`,
  and `pageSize > 200` to `200` before paginating, so out-of-range input cannot produce a
  negative offset or a division fault.
- A soft-deleted ledger record is invisible to every read unless the caller passes
  `includeDeleted=true`, which only a SuperAdmin route can reach.
- A pledge's `AmountReceived` less than its `Amount` sets `PledgeStatus.PartiallyPaid` rather
  than `Paid`.
- `ScholarshipService` retries `ReferenceCode` generation up to five times on collision before
  giving up.

## Requirements

### Functional Requirements

**Fees, payments and receipts (FinancialsController)**

- **FR-001**: The system shall let any caller retrieve the currently applicable fee for a
  given `MembershipType` and date via `GET /api/financials/fees/applicable`. [code]
- **FR-002**: The system shall let a signed-in member retrieve their own payment history via
  `GET /api/financials/my-history`, scoped to their `MemberId` claim. [code+test]
- **FR-003**: The system shall let a signed-in member record a payment via
  `POST /api/financials/record-payment`, overwriting any client-supplied `MemberId` with the
  value from the auth claim. [code+test]
- **FR-004**: The system shall reject a `record-payment` receipt upload larger than 10 MB or
  of an unsupported document type with a 400 error, before persisting the payment. [code]
- **FR-005**: The system shall let an Admin update a payment's status via
  `PATCH /api/financials/update-status/{id}`. [code]
- **FR-006**: When a payment status transitions to `Completed`, the system shall send the
  owning member a `PaymentStatusUpdated` notification and raise a `NEW_PAYMENT` real-time
  admin alert. [code]
- **FR-007**: The system shall let the owning member or an Admin download a payment's tax
  receipt PDF via `GET /api/financials/receipt/{paymentId}`, and shall return 403 to a
  non-owner non-admin. [code+test]
- **FR-008**: The system shall let a signed-in member retrieve their own dues via
  `GET /api/financials/my-dues`. [code+test]
- **FR-009**: The system shall let an Admin generate annual dues via
  `POST /api/financials/dues/generate`, creating one due per active, non-archived member per
  year, skipping members whose type has a zero applicable fee and members who already have a
  due for that year. [code+test]
- **FR-010**: The system shall let a SuperAdmin list membership fee configs via
  `GET /api/financials/fees/config`. [code]
- **FR-011**: The system shall let a SuperAdmin add a membership fee config via
  `POST /api/financials/fees/config`, rejecting a config whose `EffectiveTo` is not after
  `EffectiveDate`. [code]
- **FR-012**: The system shall let a SuperAdmin update a membership fee config via
  `PUT /api/financials/fees/config`, with the same `EffectiveTo` validation as FR-011. [code]
- **FR-013**: The system shall let a member view their own membership-tier change history, and
  an Admin view any member's, via `GET /api/financials/membership-history/{memberId}`. [code]
- **FR-014**: The system shall let an Admin soft-delete a payment via
  `DELETE /api/financials/payment/{id}`, unlinking any membership due that referenced it,
  rather than removing the row. [code]
- **FR-015**: The system shall let an Admin retrieve any member's full payment history via
  `GET /api/financials/member/{memberId}/history`. [code]
- **FR-016**: The system shall let a signed-in member list, add, and delete their own saved
  payment methods via `GET|POST /api/financials/saved-methods` and
  `DELETE /api/financials/saved-methods/{id}`. [code]

**Superadmin ledger (FinancialLedgerController)**

- **FR-017**: The system shall restrict every `/api/ledger` route to the SuperAdminOnly policy
  and return 403 to any other role. [code+test]
- **FR-018**: The system shall let a SuperAdmin list ledger records with pagination via
  `GET /api/ledger`, clamping `page` to a minimum of 1 and `pageSize` to the range 1-200.
  [code+test]
- **FR-019**: The system shall let a SuperAdmin retrieve an annual income/expense summary via
  `GET /api/ledger/summary`. [code+test]
- **FR-020**: The system shall require a fresh step-up credential before a SuperAdmin can add
  a ledger record via `POST /api/ledger`. [code+test]
- **FR-021**: The system shall require a fresh step-up credential before a SuperAdmin can
  update a ledger record via `PUT /api/ledger/{id}`. [code+test]
- **FR-022**: The system shall require a fresh step-up credential before a SuperAdmin can
  soft-delete a ledger record via `DELETE /api/ledger/{id}`, and shall exclude soft-deleted
  records from ordinary reads unless `includeDeleted=true` is passed. [code+test]
- **FR-023**: The system shall let a SuperAdmin export ledger records as CSV via
  `GET /api/ledger/export/csv`. [code+test]

**Gateway initiation and callbacks (GatewaysController)**

- **FR-024**: The system shall let a caller initiate a gateway payment via
  `POST /api/gateways/initiate`, deriving the callback URL server-side rather than accepting
  one from the client. [code+test]
- **FR-025**: The system shall reject an `initiate` request for a membership fee payment when
  the caller has no authenticated `MemberId` claim, with a 401 error. [code+test]
- **FR-026**: The system shall reject an `initiate` request for an event payment whose amount
  does not match the event's configured fee, with a 400 error. [code+test]
- **FR-027**: The system shall reject a gateway `initiate` request for a `PaymentMethod` that
  is absent from `OrgConfig.EnabledGatewayMethods` or disabled in the `PaymentConfiguration`
  table, before contacting any gateway. [code]
- **FR-028**: The system shall accept the SSLCommerz success callback at
  `POST /api/gateways/callback/sslcommerz` and hand it to the payment orchestrator. [code+test]
- **FR-029**: The system shall accept the bKash success callback at
  `GET /api/gateways/callback/bkashgateway` and hand it to the payment orchestrator.
  [code+test]
- **FR-030**: The system shall accept the DGePay success callback at
  `GET /api/gateways/callback/dgepay` and hand it to the payment orchestrator. [code]
- **FR-031**: The system shall accept a generic webhook at `POST /api/gateways/webhook/{gateway}`,
  route it to the matching `IPaymentGatewayService` by name, and return 404 when no gateway
  matches. [code+test]
- **FR-032**: The system shall short-circuit a callback whose `GatewayPaymentId` was already
  processed for that payment, performing no further status change or side effect. [code+test]
- **FR-033**: The system shall mark a payment `Failed`, not `Completed`, when a callback's
  confirmed amount does not equal the recorded payment amount, treating a confirmed amount of
  zero as a mismatch. [code+test]
- **FR-034**: The system shall auto-approve the linked event registration when a
  `FinancialCategory.RegistrationFee` payment completes, and shall never do so for any other
  category. [code+test]
- **FR-035**: The system shall auto-induct the member when a `FinancialCategory.MembershipFee`
  payment completes, using the canonical applicable-fee lookup, and shall never do so for any
  other category. [code+test]
- **FR-036**: The system shall throw an `InvalidOperationException` if the configured
  system-admin id is missing when auto-approval side effects are about to run, rather than
  silently skip them. [code]

**Payment method configuration (PaymentConfigController)**

- **FR-037**: The system shall let any caller list enabled payment methods, ordered by
  `SortOrder`, via `GET /api/payment-config/active`. [code+test]
- **FR-038**: The system shall let a SuperAdmin list every payment config via
  `GET /api/payment-config/admin/all`, masking gateway secrets for any caller who is not
  SuperAdmin. [code+test]
- **FR-039**: The system shall let a SuperAdmin create a payment config via
  `POST /api/payment-config/admin`, returning secrets masked in the response. [code+test]
- **FR-040**: The system shall let a SuperAdmin update a payment config via
  `PUT /api/payment-config/admin/{id}`, allowing secret values to change only when the caller
  is SuperAdmin. [code+test]
- **FR-041**: The system shall let a SuperAdmin toggle a payment config's `IsEnabled` flag via
  `POST /api/payment-config/admin/{id}/toggle`, and return 404 for an unknown id. [code+test]
- **FR-042**: The system shall require a fresh step-up credential before a SuperAdmin deletes
  a payment config via `DELETE /api/payment-config/admin/{id}`, and return 404 for an unknown
  id. [code+test]
- **FR-043**: The system shall let a SuperAdmin seed default payment configs via
  `POST /api/payment-config/admin/seed-defaults`. [code+test]

**Fundraising campaigns (CampaignsController)**

- **FR-044**: The system shall let any caller list active, non-archived campaigns via
  `GET /api/campaigns/public`. [code]
- **FR-045**: The system shall let any caller retrieve one campaign by slug via
  `GET /api/campaigns/{slug}`, returning 404 for an unknown slug. [code]
- **FR-046**: The system shall let any caller retrieve a campaign's honour roll via
  `GET /api/campaigns/{slug}/honour-roll`, grouping only pledges with `AmountReceived > 0`
  into donor-recognition tiers and suppressing the donor name when `IsAnonymous` is set.
  [code]
- **FR-047**: The system shall let any caller, member or guest, submit a pledge via
  `POST /api/campaigns/{slug}/pledges`, requiring `DonorName` when the caller has no
  authenticated `MemberId`. [code]
- **FR-048**: The system shall let a signed-in member list their own pledges via
  `GET /api/campaigns/my-pledges`. [code]
- **FR-049**: The system shall let an Admin list every campaign, including archived ones, via
  `GET /api/campaigns/admin/all`. [code]
- **FR-050**: The system shall let an Admin create a campaign via `POST /api/campaigns/admin`,
  rejecting a duplicate `Slug` with a 400 error. [code]
- **FR-051**: The system shall let an Admin update a campaign via `PUT /api/campaigns/admin`,
  rejecting a `Slug` collision with any campaign other than itself. [code]
- **FR-052**: The system shall let an Admin list pledges for one campaign via
  `GET /api/campaigns/admin/{campaignId}/pledges`. [code]
- **FR-053**: The system shall let an Admin confirm a pledge's receipt via
  `POST /api/campaigns/admin/pledges/confirm-receipt`, creating one `FinancialRecord` of type
  Income/Donation, and shall be idempotent: a pledge whose `FinancialRecordId` is already set
  returns success without writing a second record. [code]
- **FR-054**: The system shall set a confirmed pledge's status to `Paid` when
  `AmountReceived >= Amount`, and to `PartiallyPaid` otherwise. [code]
- **FR-055**: The system shall let an Admin list and create donor-recognition tiers via
  `GET|POST /api/campaigns/admin/tiers`. [code]

**Scholarships (ScholarshipsController)**

- **FR-056**: The system shall let any caller list active scholarship funds and open calls via
  `GET /api/scholarships/public/funds` and `GET /api/scholarships/public/calls`. [code]
- **FR-057**: The system shall let any caller submit a scholarship application via
  `POST /api/scholarships/calls/{callId}/applications`, rejecting a submission outside the
  call's `OpensOn`-`ClosesOn` window with an `InvalidOperationException`, and shall generate a
  unique `SCH-yyyyMMdd-XXXXXXXX` reference code with up to five collision retries. [code]
- **FR-058**: The system shall let any caller check an application's status via
  `GET /api/scholarships/status/{referenceCode}`, matching on reference code and applicant
  email together, and shall rate-limit this route under the `scholarshipStatus` policy. [code]
- **FR-059**: The system shall let a Member (reviewer role) list applications awaiting review
  via `GET /api/scholarships/review-queue`, restricted to `Submitted`, `UnderReview`, and
  `Shortlisted` statuses. [code]
- **FR-060**: The system shall let a Member open one application for review via
  `GET /api/scholarships/applications/{applicationId}/review`. [code]
- **FR-061**: The system shall let a Member submit a review via
  `POST /api/scholarships/applications/{applicationId}/review`, upserting one review per
  reviewer per application and transitioning the application from `Submitted` to
  `UnderReview`. [code]
- **FR-062**: The system shall let an Admin list all scholarship applications via
  `GET /api/scholarships/admin/applications`. [code]
- **FR-063**: The system shall let an Admin create a scholarship fund via
  `POST /api/scholarships/admin/funds`. [code]
- **FR-064**: The system shall let an Admin create a scholarship call via
  `POST /api/scholarships/admin/calls`, rejecting a call whose `ClosesOn` is not after
  `OpensOn` or whose fund is missing or inactive. [code]
- **FR-065**: The system shall let an Admin create or update a scholarship award via
  `POST /api/scholarships/admin/awards`: create one when none exists for the application, or
  update the `Amount` in place while the award is still `Pending`; either path sets the
  application status to `Awarded`. [code]
- **FR-066**: The system shall let an Admin disburse an award via
  `POST /api/scholarships/admin/awards/{awardId}/disburse` inside a database transaction,
  creating one `FinancialRecord` of type Expense/Grant and setting `DisbursementStatus.Paid`;
  the operation shall be idempotent for an award already `Paid` or already linked to a
  `FinancialRecord`, and shall refuse an award whose application status is `Cancelled`. [code]

### Key Entities

- **PaymentHistory** - a single payment (gateway or manual), optionally linked to a member
  (nullable, for guest payments), carrying `TransactionId`, `Amount`, `PaidAt`, `Status`
  (`Pending|Completed|Failed|Refunded`), `FinancialCategory`, `PaymentMethod`,
  `GatewayPaymentId` (unique, partial index, drives callback idempotency), and soft-delete/
  audit columns (`UpdatedAt`, `UpdatedByAdminId`, `IsArchived`, `DeletedAt`,
  `DeletedByAdminId`).
- **FinancialRecord** - one ledger row (`RecordType` Income/Expense, `FinancialCategory`,
  `Year`, `Date`, `Amount`, `Description`, `Reference`), written by dues generation, pledge
  receipt confirmation, and scholarship disbursement; carries the same audit/soft-delete
  columns as `PaymentHistory`.
- **MembershipFeeConfig** - the fee amount for a `MembershipType` over an effective date range;
  source of truth for `GetApplicableFeeAsync`.
- **MembershipDue** - one member's due for one year, linked optionally to the `PaymentHistory`
  that paid it.
- **PaymentConfiguration** - one gateway/method's admin-controlled settings: `IsEnabled`,
  `SortOrder`, secrets (masked from non-SuperAdmin reads), display metadata.
- **SavedPaymentMethod** - a member's stored payment method reference for reuse.
- **Campaign** - `Title`, `Slug` (unique), `Story`, `TargetAmount`, `StartsOn`/`EndsOn`,
  `IsActive`, `IsArchived`; owns a list of `CampaignPledge`.
- **CampaignPledge** - `CampaignId`, optional `MemberId`, `DonorName`/`Email`/`Phone`,
  `Amount`, `AmountReceived`, `Status` (`PledgeStatus`), `IsAnonymous`, `Message`,
  `FinancialRecordId` (set once confirmed, drives idempotency).
- **DonorRecognitionTier** - `Name`, `MinimumAmount`, groups honour-roll entries.
- **ScholarshipFund**, **ScholarshipCall** (`OpensOn`/`ClosesOn`, `SlotCount`,
  `AwardAmount`), **ScholarshipApplication** (`ReferenceCode`, `Status`), **ScholarshipReview**
  (one per reviewer, `NeedScore`/`MeritScore`), **ScholarshipAward** (`Amount`,
  `DisbursementStatus`).

## Evidence

| FR | API | Service method | Web | Mobile | Test |
|---|---|---|---|---|---|
| FR-001 | GET /api/financials/fees/applicable | FinancialService.GetApplicableFeeAsync | financial.service.ts | financial_service.dart | none found |
| FR-002 | GET /api/financials/my-history | FinancialService.GetMemberPaymentHistoryAsync | member/payments/payments.ts | financial_portal_screen.dart | FinancialsControllerTests.cs::GetMyPaymentHistory_ReturnsOk |
| FR-003 | POST /api/financials/record-payment | FinancialService.RecordPaymentAsync | member/payments/payments.ts | financial_portal_screen.dart | FinancialsControllerTests.cs::RecordPayment_ReturnsOk |
| FR-004 | POST /api/financials/record-payment | IFileValidationService.ValidateFormFile | member/payments/payments.ts | financial_portal_screen.dart | none found |
| FR-005 | PATCH /api/financials/update-status/{id} | FinancialService.UpdatePaymentStatusAsync | admin/ledger/ledger.ts | none found | none found |
| FR-006 | side effect of FR-005 | FinancialService.UpdatePaymentStatusAsync | none found | none found | none found |
| FR-007 | GET /api/financials/receipt/{paymentId} | FinancialService.GenerateTaxReceiptAsync, GetPaymentOwnerMemberIdAsync | member/payments/payments.ts | financial_portal_screen.dart | FinancialsControllerTests.cs::DownloadReceipt_NonAdminOwnsPayment_ReturnsFile, DownloadReceipt_NonAdminDoesNotOwnPayment_ReturnsForbid, DownloadReceipt_NonAdminPaymentDoesNotExist_ReturnsNotFound |
| FR-008 | GET /api/financials/my-dues | FinancialService.GetMemberDuesAsync | member/payments/payments.ts | financial_portal_screen.dart | FinancialsControllerTests.cs::GetMyDues_ReturnsOk, GetMyDues_SuperAdmin_ReturnsEmptyList, GetMyDues_SystemAdminWithoutMemberIdClaim_ResolvesMemberIdViaService |
| FR-009 | POST /api/financials/dues/generate | FinancialService.GenerateAnnualDuesAsync | admin/fee-config/admin-fee-config.ts | fee_config_screen.dart | FinancialsControllerTests.cs::GenerateAnnualDues_ReturnsOk |
| FR-010 | GET /api/financials/fees/config | FinancialService.GetMembershipFeeConfigsAsync | admin/fee-config/admin-fee-config.ts | fee_config_screen.dart | none found |
| FR-011 | POST /api/financials/fees/config | FinancialService.AddMembershipFeeConfigAsync | admin/fee-config/admin-fee-config.ts | fee_config_screen.dart | none found |
| FR-012 | PUT /api/financials/fees/config | FinancialService.UpdateMembershipFeeConfigAsync | admin/fee-config/admin-fee-config.ts | fee_config_screen.dart | none found |
| FR-013 | GET /api/financials/membership-history/{memberId} | FinancialService.GetMemberMembershipHistoryAsync | none found | none found | none found |
| FR-014 | DELETE /api/financials/payment/{id} | FinancialService.DeletePaymentAsync | admin/ledger/ledger.ts | none found | none found |
| FR-015 | GET /api/financials/member/{memberId}/history | FinancialService.GetMemberPaymentHistoryAsync | none found | none found | none found |
| FR-016 | GET,POST /api/financials/saved-methods and DELETE /api/financials/saved-methods/{id} | FinancialService.GetSavedPaymentMethodsAsync, AddSavedPaymentMethodAsync, DeleteSavedPaymentMethodAsync | payment-method-selector.component.ts | none found | none found |
| FR-017 | all /api/ledger routes | n/a, policy only | admin/ledger/ledger.ts | none found | FinancialLedgerControllerTests.cs, class-level policy exercised by every test |
| FR-018 | GET /api/ledger | FinancialLedgerService.GetRecordsAsync | admin/ledger/ledger.ts | none found | FinancialLedgerControllerTests.cs::GetRecords_ReturnsOk |
| FR-019 | GET /api/ledger/summary | FinancialLedgerService.GetSummaryAsync | admin/ledger/ledger.ts | none found | FinancialLedgerControllerTests.cs::GetSummary_ReturnsOk |
| FR-020 | POST /api/ledger | FinancialLedgerService.AddRecordAsync | admin/ledger/ledger.ts | none found | FinancialLedgerControllerTests.cs::AddRecord_ReturnsCreatedAtAction |
| FR-021 | PUT /api/ledger/{id} | FinancialLedgerService.UpdateRecordAsync | admin/ledger/ledger.ts | none found | FinancialLedgerControllerTests.cs::UpdateRecord_ReturnsOk |
| FR-022 | DELETE /api/ledger/{id} | FinancialLedgerService.DeleteRecordAsync | admin/ledger/ledger.ts | none found | FinancialLedgerControllerTests.cs::DeleteRecord_ReturnsOk_OnSuccess |
| FR-023 | GET /api/ledger/export/csv | FinancialLedgerService.ExportRecordsAsync | admin/ledger/ledger.ts | none found | FinancialLedgerControllerTests.cs::ExportCsv_ReturnsFile |
| FR-024 | POST /api/gateways/initiate | IPaymentGatewayFactory.GetGateway, IPaymentGatewayService.InitiatePaymentAsync | common/payment-portal/payment-portal.component.ts, gateways.service.ts | payment_web_page.dart | GatewaysControllerTests.cs::InitiatePayment_ReturnsOk_WhenSuccessful |
| FR-025 | POST /api/gateways/initiate | GatewaysController.InitiatePayment, claim check | payment-portal.component.ts | payment_web_page.dart | GatewaysControllerTests.cs::InitiatePayment_ReturnsUnauthorized_WhenMembershipPaymentAndAnonymous |
| FR-026 | POST /api/gateways/initiate | GatewaysController.InitiatePayment, amount check | payment-portal.component.ts | payment_web_page.dart | GatewaysControllerTests.cs::InitiatePayment_ReturnsBadRequest_WhenEventAmountDoesNotMatch |
| FR-027 | POST /api/gateways/initiate | GatewaysController.InitiatePayment, OrgConfig and PaymentConfig gate | payment-portal.component.ts | payment_web_page.dart | none found |
| FR-028 | POST /api/gateways/callback/sslcommerz | SSLCommerzGateway.VerifyCallbackAsync, PaymentCallbackOrchestrator.HandleSuccessfulPaymentAsync | payment-status.ts | payment_web_page.dart | GatewaysControllerTests.cs::SSLCommerzCallback_AutoApprovesRegistration_WhenValid, Callback_ShouldAutoApproveRegistration_EvenWithComplexNotes, SSLCommerzCallback_MarksFailedAndDoesNotApprove_WhenReportedAmountMismatches |
| FR-029 | GET /api/gateways/callback/bkashgateway | BkashGateway.VerifyCallbackAsync, PaymentCallbackOrchestrator.HandleSuccessfulPaymentAsync | payment-status.ts | payment_web_page.dart | GatewaysControllerTests.cs::BkashCallbackGet_AutoApprovesRegistration_WhenValid |
| FR-030 | GET /api/gateways/callback/dgepay | DGePayGateway.VerifyCallbackAsync, PaymentCallbackOrchestrator.HandleSuccessfulPaymentAsync | payment-status.ts | payment_web_page.dart | none found |
| FR-031 | POST /api/gateways/webhook/{gateway} | IPaymentGatewayFactory.GetGateway, IPaymentGatewayService.ProcessWebhookAsync | none found | none found | GatewaysControllerTests.cs::GatewayWebhook_MarksPaymentCompleted_WhenGatewayConfirmsValid, GatewayWebhook_DoesNotTouchPayment_WhenGatewayReportsInvalid |
| FR-032 | all callback and webhook routes | FinancialService.IsGatewayPaymentAlreadyProcessedAsync | none found | none found | GHCAA.Tests/Services/FinancialServiceTests.cs, idempotency subset, names not extracted |
| FR-033 | all callback and webhook routes | PaymentCallbackOrchestrator.HandleSuccessfulPaymentAsync | none found | none found | GatewaysControllerTests.cs::SSLCommerzCallback_MarksFailedAndDoesNotApprove_WhenReportedAmountMismatches |
| FR-034 | callback and webhook routes, RegistrationFee | PaymentCallbackOrchestrator.HandleSuccessfulPaymentAsync | none found | none found | GatewaysControllerTests.cs::SSLCommerzCallback_AutoApprovesRegistration_WhenValid |
| FR-035 | callback and webhook routes, MembershipFee | PaymentCallbackOrchestrator.HandleSuccessfulPaymentAsync, FinancialService.GetApplicableFeeAsync | none found | none found | GHCAA.Tests/Workflows/WorkflowTests.cs, membership induction path, names not extracted |
| FR-036 | callback and webhook routes | PaymentCallbackOrchestrator.HandleSuccessfulPaymentAsync | none found | none found | none found |
| FR-037 | GET /api/payment-config/active | PaymentConfigService.GetActiveMethodsAsync | payment-method-selector.component.ts, payment-config.service.ts | financial_portal_screen.dart | PaymentConfigControllerTests.cs::GetActivePaymentMethods_ReturnsProperlyMappedObjects |
| FR-038 | GET /api/payment-config/admin/all | PaymentConfigService.GetAllAsync | admin/payment-config/admin-payment-config.ts | none found | PaymentConfigControllerTests.cs::GetAllConfigs_ObfuscatesSecrets_UnlessSuperAdmin |
| FR-039 | POST /api/payment-config/admin | PaymentConfigService.CreateAsync | admin/payment-config/admin-payment-config.ts | none found | PaymentConfigControllerTests.cs::CreateConfig_PersistsAndReturnsMaskedSecrets |
| FR-040 | PUT /api/payment-config/admin/{id} | PaymentConfigService.UpdateAsync | admin/payment-config/admin-payment-config.ts | none found | PaymentConfigControllerTests.cs::UpdateConfig_OnlySuperAdminCanChangeSecrets |
| FR-041 | POST /api/payment-config/admin/{id}/toggle | PaymentConfigService.ToggleAsync | admin/payment-config/admin-payment-config.ts | none found | PaymentConfigControllerTests.cs::ToggleConfig_FlipsIsEnabled, ToggleConfig_ReturnsNotFound_WhenConfigDoesNotExist |
| FR-042 | DELETE /api/payment-config/admin/{id} | PaymentConfigService.DeleteAsync | admin/payment-config/admin-payment-config.ts | none found | PaymentConfigControllerTests.cs::DeleteConfig_RemovesConfig, DeleteConfig_ReturnsNotFound_WhenConfigDoesNotExist |
| FR-043 | POST /api/payment-config/admin/seed-defaults | PaymentConfigService.SeedDefaultsAsync | admin/payment-config/admin-payment-config.ts | none found | PaymentConfigControllerTests.cs::SeedDefaults_ShouldCreateInitialConfigs |
| FR-044 | GET /api/campaigns/public | CampaignService.GetPublicCampaignsAsync | public/campaigns/campaigns.ts | campaigns_screen.dart | none found |
| FR-045 | GET /api/campaigns/{slug} | CampaignService.GetCampaignBySlugAsync | public/campaigns/campaigns.ts | campaign_detail_screen.dart | none found |
| FR-046 | GET /api/campaigns/{slug}/honour-roll | CampaignService.GetHonourRollAsync | public/campaigns/campaigns.ts | campaign_detail_screen.dart | GHCAA.Tests/Services/CampaignServiceTests.cs, 4 tests, names not extracted |
| FR-047 | POST /api/campaigns/{slug}/pledges | CampaignService.CreatePledgeAsync | public/campaigns/campaigns.ts | campaign_detail_screen.dart | CampaignServiceTests.cs, names not extracted |
| FR-048 | GET /api/campaigns/my-pledges | CampaignService.GetMemberPledgesAsync | none found | campaigns_screen.dart | none found |
| FR-049 | GET /api/campaigns/admin/all | CampaignService.GetAllCampaignsForAdminAsync | admin/campaigns/admin-campaigns.ts | none found | none found |
| FR-050 | POST /api/campaigns/admin | CampaignService.CreateCampaignAsync | admin/campaigns/admin-campaigns.ts | none found | none found |
| FR-051 | PUT /api/campaigns/admin | CampaignService.UpdateCampaignAsync | admin/campaigns/admin-campaigns.ts | none found | none found |
| FR-052 | GET /api/campaigns/admin/{campaignId}/pledges | CampaignService.GetPledgesForAdminAsync | admin/campaigns/admin-campaigns.ts | none found | none found |
| FR-053 | POST /api/campaigns/admin/pledges/confirm-receipt | CampaignService.ConfirmPledgeReceiptAsync | admin/campaigns/admin-campaigns.ts | none found | CampaignServiceTests.cs, names not extracted |
| FR-054 | part of FR-053 | CampaignService.ConfirmPledgeReceiptAsync | admin/campaigns/admin-campaigns.ts | none found | none found |
| FR-055 | GET,POST /api/campaigns/admin/tiers | CampaignService.GetTiersAsync, CreateTierAsync | admin/campaigns/admin-campaigns.ts | none found | none found |
| FR-056 | GET /api/scholarships/public/funds and /public/calls | ScholarshipService.GetPublicFundsAsync, GetPublicCallsAsync | public/scholarships/scholarships.ts | none found | none found |
| FR-057 | POST /api/scholarships/calls/{callId}/applications | ScholarshipService.SubmitApplicationAsync | public/scholarships/scholarships.ts | none found | GHCAA.Tests/Services/ScholarshipServiceTests.cs, 4 tests, names not extracted |
| FR-058 | GET /api/scholarships/status/{referenceCode} | ScholarshipService.GetPublicStatusAsync | public/scholarships/scholarships.ts | none found | none found |
| FR-059 | GET /api/scholarships/review-queue | ScholarshipService.GetReviewQueueAsync | none found | none found | none found |
| FR-060 | GET /api/scholarships/applications/{applicationId}/review | ScholarshipService.GetApplicationForReviewAsync | none found | none found | none found |
| FR-061 | POST /api/scholarships/applications/{applicationId}/review | ScholarshipService.SubmitReviewAsync | none found | none found | ScholarshipServiceTests.cs, names not extracted |
| FR-062 | GET /api/scholarships/admin/applications | ScholarshipService.GetApplicationsForAdminAsync | none found | none found | none found |
| FR-063 | POST /api/scholarships/admin/funds | ScholarshipService.CreateFundAsync | none found | none found | none found |
| FR-064 | POST /api/scholarships/admin/calls | ScholarshipService.CreateCallAsync | none found | none found | none found |
| FR-065 | POST /api/scholarships/admin/awards | ScholarshipService.CreateAwardAsync | none found | none found | none found |
| FR-066 | POST /api/scholarships/admin/awards/{awardId}/disburse | ScholarshipService.DisburseAwardAsync | none found | none found | ScholarshipServiceTests.cs, names not extracted |

## Gaps

- No admin web UI exists for scholarships under GHCAA.Web/src/app/admin, so FR-059 through
  FR-066 (review, fund/call/award creation, disbursement) are only reachable through the API
  or tests. [NEEDS CLARIFICATION: is admin scholarship management intentionally API-only, or
  is a web screen missing?]
- No controller-level test file exists for CampaignsController or ScholarshipsController; only
  CampaignServiceTests.cs (4 tests) and ScholarshipServiceTests.cs (4 tests) cover the service
  layer, leaving most campaign and scholarship FRs without controller-level auth/route/status-
  code coverage.
- FinancialService.ProcessGatewayPaymentAsync and its private
  HandleAutomatedApprovalsAfterPaymentAsync (GHCAA.Infrastructure/Services/FinancialService.cs)
  duplicate PaymentCallbackOrchestrator.HandleSuccessfulPaymentAsync and, per a code comment on
  that method, have no production caller. This is dead code kept alive only by tests, and a
  second place the same approval rule must be kept in sync by hand.
- NagadGateway.InitiatePaymentAsync (GHCAA.Infrastructure/Gateways/NagadGateway.cs) always
  returns Success = false with a coming-soon message, and VerifyCallbackAsync and
  ProcessWebhookAsync always return false or invalid. The gateway is registered in DI and
  reachable through FR-024 and FR-031, but is fully stubbed, while PaymentMethod.Nagad still
  appears in seeded payment_configurations.json as an option a member could pick.
- PaymentConfigController.GetAllConfigs and UpdateConfig re-check User.IsInRole("SuperAdmin")
  even though the admin routes are already restricted to SuperAdmin at the policy level, a
  defensive branch that cannot fail in production traffic.
- FR-005/FR-006 (update-status), FR-010 through FR-013 (fees/config GET/POST/PUT,
  membership-history), FR-014/FR-015 (delete payment, admin member history), and FR-016
  (saved payment methods) have no test evidence found in FinancialsControllerTests.cs or
  elsewhere searched.
- No Angular test file (campaign.service.spec.ts, admin-campaigns.spec.ts, a public
  scholarships spec) was found for the campaign admin screen or the public scholarships page,
  though scholarship.service.spec.ts does exist.

## Enhancements: modularisation and reusability

### Reuse across layers

- **ENH-001** (P2): GHCAA.Web/src/app/core/services/campaign.service.ts and
  GHCAA.Mobile/lib/features/campaigns/campaign_service.dart independently implement the same
  five /api/campaigns/* calls with separate hand-written JSON mapping. A shared generated
  client, or at minimum one documented DTO contract, would remove the duplicate mapping code
  on each client. Would change: both files, no server change.
- **ENH-002** (P3): financial.service.ts, ledger.service.ts, payment-config.service.ts,
  campaign.service.ts, and scholarship.service.ts under GHCAA.Web/src/app/core/services each
  hand-roll their own HTTP wrapper around API_ENDPOINTS. If a shared base pattern is already
  used elsewhere in core/services, this domain should follow it instead of repeating the
  boilerplate five times. Would change: the five service files above.

### Entity-based module shape

- **ENH-003** (P2): Campaigns and scholarships both follow a public-listing plus
  application-or-pledge plus admin-review plus ledger-side-effect shape that is structurally
  close to EventsController registration, approval, and payment pattern
  (GHCAA.API/Controllers/EventsController.cs), but each was implemented as its own bespoke
  controller and service pair rather than against a shared base. Events additionally owns
  media upload (admin/{id}/logo) that campaigns lacks even though a CoverImagePath field
  exists on Campaign (GHCAA.Domain/Models/Campaign.cs) with no upload endpoint to set it, a
  media-handling responsibility that currently leaks outside this module own controller. A
  shared listable-reviewable-payable service base could cut the near-identical CRUD across
  Events, Campaigns, and Scholarships.
- **ENH-004** (P3): ScholarshipAward disbursement and CampaignPledge receipt confirmation both
  independently create one FinancialRecord and both hand-roll their own idempotency check,
  FinancialRecordId not null for pledges, Status Paid or FinancialRecordId not null for
  awards, instead of sharing one post-to-ledger-once helper on IFinancialLedgerService. Would
  change: CampaignService.ConfirmPledgeReceiptAsync and ScholarshipService.DisburseAwardAsync.

### Existing reusable components

- **ENH-005** (P1, informational, no change needed): PaymentPortalComponent
  (GHCAA.Web/src/app/common/payment-portal/payment-portal.component.ts) is already shared
  correctly across common/events/events.ts, member/payments/payments.ts, and
  public/register/register.ts. This is the pattern the rest of the domain should match.
- **ENH-006** (P1): GHCAA.Mobile/lib/screens/member/events_screen.dart, around lines 85-125,
  hand-rolls a payment-method bottom sheet with two hard-coded options, SSLCommerz and DGePay,
  instead of reading the admin-configurable active list the way financial_portal_screen.dart
  does, which maps dynamically from the PaymentGateway config value per a comment near line
  409. The event payment sheet should call the same /api/payment-config/active list so a
  config change, such as disabling DGePay, takes effect everywhere at once.

### Hard-coded behaviour that should be configuration

- **ENH-007** (P1): GHCAA.Mobile/lib/screens/member/events_screen.dart hard-codes the two
  gateway names and labels shown to a member paying for an event as Dart string literals in
  the widget tree, bypassing PaymentConfigController IsEnabled, SortOrder, and DisplayName
  fields entirely. Same file as ENH-006, listed here for the configuration angle: these values
  already exist in the PaymentConfiguration table and should be read from there.
- **ENH-008** (P2): PaymentConfigService.SeedDefaultsAsync
  (GHCAA.Infrastructure/Services/PaymentConfigService.cs, around line 104) hard-codes the
  Nagad wallet placeholder number 01XXXXXXXXX and holder name GHCAA as seed defaults inline in
  C#, rather than sourcing them from appsettings or OrgConfig branding. Low risk since they
  are placeholder seed values an admin is expected to overwrite, but worth moving next to the
  other seed JSON files under GHCAA.Infrastructure/Data/Seed/ for consistency.
- **ENH-009** (P3): FinancialsController.RecordPayment hard-codes the receipt upload size
  limit as 10 times 1024 times 1024 inline in the controller
  (GHCAA.API/Controllers/FinancialsController.cs) instead of reading it from a shared
  file-upload-limits constant, if one exists elsewhere in GHCAA.Domain/Constants.cs for other
  document uploads in the platform.

## Success Criteria

- **SC-001**: Every one of the 58 actions across the six controllers in scope is named in at
  least one FR Evidence row.
- **SC-002**: A gateway callback replay with an already-processed GatewayPaymentId produces
  zero additional side effects (FR-032), verified by FinancialServiceTests.cs idempotency
  cases.
- **SC-003**: A pledge receipt confirmed twice, or an award disbursed twice, produces exactly
  one FinancialRecord each (FR-053, FR-066).
- **SC-004**: Every /api/ledger write route rejects a request lacking a fresh step-up
  credential (FR-020, FR-021, FR-022).
- **SC-005**: A non-SuperAdmin caller never receives an unmasked gateway secret from
  /api/payment-config/admin/all (FR-038).

## Assumptions

- Admin in this spec means the AdminOnly policy (GHCAA.Domain.Constants.Policies.AdminOnly),
  and SuperAdmin means SuperAdminOnly, per the policy names used directly on each action.
- The RequireStepUp filter own OTP and session mechanics are defined once in
  GHCAA.API/Filters/RequireStepUpAttribute.cs and are treated here as a black box: this spec
  records only which actions require it, not how the check itself works.
- Where a test file exists but exact method names for a subset of methods were not
  individually confirmed (CampaignServiceTests.cs, ScholarshipServiceTests.cs, and the
  idempotency-focused subset of FinancialServiceTests.cs), the Evidence table cites the file
  and test count rather than a specific test name; this is noted, not treated as no test.
