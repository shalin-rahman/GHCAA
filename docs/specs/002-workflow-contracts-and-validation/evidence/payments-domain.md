# Payments/Financial Domain: State Transitions, Validation, and Endpoint Contracts

**Reviewed**: 2026-09-21. Covers `FinancialsController`,
`FinancialLedgerController`, `PaymentConfigController`,
`GatewaysController`, `FinancialService.cs`,
`PaymentCallbackOrchestrator.cs`. Part of [../tasks.md](../tasks.md)
T005/T008/T011, following the [events-domain.md](./events-domain.md)
template.

## 1. Payment state transitions

`PaymentStatus` (`GHCAA.Domain/Enums.cs:17`): `Pending`, `Completed`,
`Failed`, `Refunded`.

| From | To | Trigger | Who | Where | Side effects |
|---|---|---|---|---|---|
| (none) | `Pending` | Record a manual payment | Member (self, `dto.MemberId` forced from claim) | `POST /api/financials/record-payment` → `FinancialService.RecordPaymentAsync` (`FinancialService.cs:77-164`) | Row created; receipt uploaded to `FileStorageService`/`FileUploads` if `MemberId` set; email (`PaymentReceived` template) + in-app notification sent (skipped for guest/no-MemberId payments) |
| (none) | `Pending` | Gateway payment initiation | Anonymous (member optional; required unless `EVT-REG` reference) | `POST /api/gateways/initiate` → `GatewaysController.cs:158` calls `FinancialService.RecordPaymentAsync` with a generated `TransactionId` | Same as above, `FinancialCategory` set to `RegistrationFee` or `MembershipFee` based on reference prefix |
| `Pending` | `Completed` | Admin manually verifies | Admin (`AdminOnly` policy) | `PATCH /api/financials/update-status/{id}` → `FinancialService.UpdatePaymentStatusAsync` (`FinancialService.cs:166`) | Sends `PaymentStatusUpdated` notification (member only); fires `SendAdminAlertAsync("NEW_PAYMENT", ...)` real-time alert |
| `Pending`/`Failed` | `Completed` | Gateway callback verified (SSLCommerz form callback, bKash GET callback, DGePay GET callback, generic webhook) | Anonymous (gateway) | `POST /api/gateways/callback/sslcommerz`, `GET /api/gateways/callback/bkashgateway`, `GET /api/gateways/callback/dgepay`, `POST /api/gateways/webhook/{gateway}` → all call `IPaymentCallbackOrchestrator.HandleSuccessfulPaymentAsync` → `UpdatePaymentStatusAsync(..., Completed, ...)` (`PaymentCallbackOrchestrator.cs:74-76`) | Idempotency guard on `gatewayPaymentId` (short-circuits if already processed); notification/alert as above; then event-registration auto-approval if `FinancialCategory == RegistrationFee` and amount sufficient (`EventService.AutoApproveRegistrationAfterPaymentAsync`); membership auto-induction if `FinancialCategory == MembershipFee`, member status `Applied`, and amount sufficient (`MemberService.ApproveMemberAsync`) |
| `Pending` | `Failed` | Gateway callback reports amount mismatch | Gateway (system) | Same callback/webhook endpoints → `PaymentCallbackOrchestrator.cs:61-71` and `FinancialService.ProcessGatewayPaymentAsync:221-225` (alternate path, not wired to any controller route found — not read further in this pass) | Notes annotated with mismatch detail; no further auto-approval runs |
| `Completed` | (terminal) | Any repeat callback/admin update | — | Guarded at `PaymentCallbackOrchestrator.cs:53` and `FinancialService.cs:218` (`if (payment.Status == Completed) return`) | No-op — `Completed → Failed` and `Completed → Refunded` are unreachable; no code path re-opens a completed payment |
| any | `Refunded` | No trigger found in any controller or service in scope | — | — | Unreachable in this codebase slice — `Refunded` is declared on the enum but no assignment to it was found in `FinancialService.cs`, `PaymentCallbackOrchestrator.cs`, or the four controllers |
| `Pending` | (deleted, not a status transition) | Admin deletes payment record | Admin (`AdminOnly` policy) | `DELETE /api/financials/payment/{id}` → `FinancialService.DeletePaymentAsync` | Row removed, not status-transitioned; records deleting admin id |

**Gaps carried forward, not resolved here:**
- `FinancialService.ProcessGatewayPaymentAsync` (`FinancialService.cs:215-234`)
  duplicates much of `PaymentCallbackOrchestrator.HandleSuccessfulPaymentAsync`'s
  Completed/Failed logic but no controller route calling it was found in this
  pass — unverified whether it's dead code, used by a gateway strategy class,
  or called from a path not read.
- No admin/API path to explicitly issue a `Refunded` status was found —
  flagged as a gap, not confirmed intentional.
- `FinancialLedgerController` operates on `FinancialRecord` (ledger entries),
  a separate model from `PaymentHistory`/`PaymentStatus` — not read in this
  pass whether `FinancialRecord` carries any state field of its own.

## 2. Validation matrix

| DTO | Field | Rule | Enforced by |
|---|---|---|---|
| `CreatePaymentHistoryDto` | `TransactionId` | Required, max 200 chars | `[Required]`/`[MaxLength]` DataAnnotations |
| `CreatePaymentHistoryDto` | `Amount` | 0.01–max | `[Range]` DataAnnotations |
| `CreatePaymentHistoryDto` | `PaidAt` | Required | `[Required]` DataAnnotations |
| `CreatePaymentHistoryDto` | `Notes` | Max 500 chars | `[MaxLength]` DataAnnotations |
| `CreatePaymentHistoryDto` | `FinancialCategory`, `PaymentMethod` | No range/enum-membership check found | **Flagged: no enforced rule** |
| `CreatePaymentHistoryDto` | `MemberId` | Overwritten server-side from claim in controller (`FinancialsController.cs:66`); client value ignored | Controller inline (not a DTO validator) |
| `CreatePaymentHistoryDto` | `Receipt` (`IFormFile`) | Category `Document`, 10 MB max | `FileValidationService.ValidateFormFile` (`FinancialsController.cs:72`) — only when non-null |
| `CreateMembershipFeeConfigDto` | `MembershipType` | Required, max 100 chars | `[Required]`/`[MaxLength]` |
| `CreateMembershipFeeConfigDto` | `Amount` | 0.01–max | `[Range]` |
| `CreateMembershipFeeConfigDto` | `EffectiveDate` | Required | `[Required]` |
| `CreateMembershipFeeConfigDto` | `EffectiveTo` | Must be after `EffectiveDate` when set | `IValidatableObject.Validate` (`FinancialDtos.cs:100`) |
| `CreateMembershipFeeConfigDto` | `Description` | Max 500 chars | `[MaxLength]` |
| `UpdateMembershipFeeConfigDto` | `Amount` | 0.01–max | `[Range]` |
| `UpdateMembershipFeeConfigDto` | `EffectiveDate` | Required | `[Required]` |
| `UpdateMembershipFeeConfigDto` | `EffectiveTo` | Must be after `EffectiveDate` when set | `IValidatableObject.Validate` (`FinancialDtos.cs:126`) |
| `UpdateMembershipFeeConfigDto` | `Description` | Max 500 chars | `[MaxLength]` |
| `CreateSavedPaymentMethodDto` | `DisplayName`, `Method`, `AccountNumber` | No attribute, no `IValidatableObject`, no controller inline check found | **Flagged: no enforced rule** — plain strings default to empty, no required/format check |
| `PaymentConfiguration` (bound raw as request body on create/update config) | all fields | No DTO wrapper — the EF entity itself is bound; no validation attributes seen on the fields referenced in the controller | **Flagged: no enforced rule found in this pass** — `PaymentConfigService.UpdateAsync`/`CreateAsync` bodies not read in this pass, so any service-side check is unverified |
| `InitiatePaymentRequest` (`GatewaysController` nested class) | `Amount` | > 0 and ≤ 10,000,000 | Controller inline check (`GatewaysController.cs:49-50`) |
| `InitiatePaymentRequest` | `Reference` | Required (non-whitespace) | Controller inline check (`GatewaysController.cs:52-53`) |
| `InitiatePaymentRequest` | `Amount` (event path) | Must match event's `RegistrationFee`/`ContributionAmount` within 0.01 tolerance | Controller inline check (`GatewaysController.cs:89-100`), not a validator class |
| `FinancialRecord` (ledger `AddRecord`/`UpdateRecord` body) | all fields | Not read in this pass — entity bound directly, same pattern as `PaymentConfiguration` | **Unverified — not read in this pass** |

## 3. Endpoint contract table

Auth base: `FinancialsController` is `[Authorize]` (overridable per-action);
`FinancialLedgerController` is class-wide `SuperAdminOnly`;
`PaymentConfigController` and `GatewaysController` set policy per action.

| Route | Method | Request DTO | Response | Success | Documented failures | Auth |
|---|---|---|---|---|---|---|
| `/api/financials/fees/applicable` | GET | query params (`category`,`type`,`date?`) | `{ Amount }` | 200 | — | Anonymous |
| `/api/financials/my-history` | GET | — | `PaymentHistoryDto[]` | 200 | 400 (invalid session, non-SuperAdmin) | Authenticated |
| `/api/financials/record-payment` (multipart) | POST | `CreatePaymentHistoryDto` + `IFormFile? Receipt` | `PaymentHistoryDto` | 200 | 401 (no member claim), 400 (bad receipt file) | Authenticated (member self) |
| `/api/financials/update-status/{id}` | PATCH | query params (`status`,`notes?`) | — | 200 | 404 | AdminOnly |
| `/api/financials/receipt/{paymentId}` | GET | — | PDF file stream | 200 | 401, 403 (not owner), 404 | Authenticated |
| `/api/financials/my-dues` | GET | — | dues list (not read in this pass for exact shape) | 200 | 401 | Authenticated |
| `/api/financials/dues/generate` | POST | query `year` | `{ Message }` | 200 | — | AdminOnly |
| `/api/financials/fees/config` | GET | — | `MembershipFeeConfigDto[]` (not read in this pass for exact list type) | 200 | — | SuperAdminOnly |
| `/api/financials/fees/config` | POST | `CreateMembershipFeeConfigDto` | not read in this pass | 200 | 401 (no admin claim), 400 (validation) | SuperAdminOnly |
| `/api/financials/fees/config` | PUT | `UpdateMembershipFeeConfigDto` | not read in this pass | 200 | 401, 400 (validation) | SuperAdminOnly |
| `/api/financials/membership-history/{memberId}` | GET | — | `MembershipHistoryDto[]` (inferred) | 200 | 403 (non-owner, non-admin) | Authenticated |
| `/api/financials/payment/{id}` | DELETE | — | — | 200 | 401 (no user id), 404 | AdminOnly |
| `/api/financials/member/{memberId}/history` | GET | — | `PaymentHistoryDto[]` | 200 | — | AdminOnly |
| `/api/financials/saved-methods` | GET | — | `SavedPaymentMethodDto[]` | 200 | 401 | Authenticated |
| `/api/financials/saved-methods` | POST | `CreateSavedPaymentMethodDto` | `SavedPaymentMethodDto` (inferred) | 200 | 401 | Authenticated |
| `/api/financials/saved-methods/{id}` | DELETE | — | — | 200 | 401, 404 | Authenticated |
| `/api/ledger` | GET | query (`page`,`pageSize`,`year?`,`search?`,`type?`,`includeDeleted?`) | not read in this pass (likely `PagedResult<T>`-shaped) | 200 | — | SuperAdminOnly |
| `/api/ledger/summary` | GET | query `year` | not read in this pass | 200 | — | SuperAdminOnly |
| `/api/ledger` | POST | `FinancialRecord` | `FinancialRecord` | 201 (`CreatedAtAction`) | 401 (no admin claim) | SuperAdminOnly + `RequireStepUp` filter |
| `/api/ledger/{id}` | PUT | `FinancialRecord` | `FinancialRecord` | 200 | 401, 404 | SuperAdminOnly + `RequireStepUp` |
| `/api/ledger/{id}` | DELETE | — | — | 200 | 401, 404 | SuperAdminOnly + `RequireStepUp` |
| `/api/ledger/export/csv` | GET | query `year?` | CSV file | 200 | — | SuperAdminOnly |
| `/api/payment-config/active` | GET | — | anonymous projection (masked fields excluded) | 200 | — | Anonymous |
| `/api/payment-config/admin/all` | GET | — | `PaymentConfiguration[]` (secrets masked for non-SuperAdmin) | 200 | — | SuperAdminOnly |
| `/api/payment-config/admin` | POST | `PaymentConfiguration` | masked config object | 200 | not read in this pass | SuperAdminOnly |
| `/api/payment-config/admin/{id}` | PUT | `PaymentConfiguration` | masked config object | 200 | 404 | SuperAdminOnly |
| `/api/payment-config/admin/{id}/toggle` | POST | — | `{ Id, IsEnabled }` | 200 | 404 | SuperAdminOnly |
| `/api/payment-config/admin/{id}` | DELETE | — | — | 200 | 404 | SuperAdminOnly + `RequireStepUp` |
| `/api/payment-config/admin/seed-defaults` | POST | — | seeded configs list | 200 | 400 (configs already exist) | SuperAdminOnly |
| `/api/gateways/initiate` | POST | `InitiatePaymentRequest` | `PaymentGatewayResponseDto` | 200 | 400 (amount/reference/gateway-disabled/mismatch), 401 (no member / member mismatch) | Anonymous |
| `/api/gateways/callback/sslcommerz` | POST (form) | gateway form fields | 302 redirect to `/payment/success` or `/payment/failed` | 302 | — (no JSON error body; always redirects) | Anonymous (gateway) |
| `/api/gateways/callback/bkashgateway` | GET | query (`paymentID`,`status`) | 302 redirect | 302 | — | Anonymous (gateway) |
| `/api/gateways/callback/dgepay` | GET | query `data` | 302 redirect | 302 | — | Anonymous (gateway) |
| `/api/gateways/webhook/{gateway}` | POST | raw request body (gateway-specific) | `{ status: "success" }` | 200 | 400 (invalid/failed webhook), 400 (unknown gateway enum), 404 (unregistered gateway) | Anonymous (gateway) |

Pagination: `FinancialLedgerController.GetRecords` uses offset pagination
(`page`/`pageSize` query params), consistent with the legacy offset style
noted in `001-platform-baseline/contracts/api-cross-layer.md` (that document's
recorded cursor-pagination change applied to `NetworkingController`, not
financials — ledger has not been migrated to cursor). No route in
`FinancialsController`, `PaymentConfigController`, or `GatewaysController`
takes pagination parameters; they return full arrays.

**Not read in this pass:** `PaymentConfigService`, `IFinancialLedgerService`
implementation, and the `FinancialRecord`/`PaymentConfiguration` entity
definitions — filling those in is direct continuation work, not new
investigation.
