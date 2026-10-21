# Error Response Catalog

**Reviewed**: 2026-10-21, rebuilt from code. The first review (2026-09-26) was built for
[../tasks.md](../tasks.md) T014 and tracker item 84.5. This one reads the 48 files in
`GHCAA.API/Controllers`, `GHCAA.Domain/Constants.cs` (`ErrorCodes`),
`GHCAA.API/Extensions/ProblemExtensions.cs`, `GHCAA.API/Middleware/ExceptionMiddleware.cs`,
the filters in `GHCAA.API/Filters`, `GHCAA.API/Extensions/RateLimitingExtensions.cs`,
`GHCAA.API/Extensions/ServiceExtensions.cs`, `GHCAA.API/Program.cs`, and the client code that
reads error bodies. The section 4 counts come from a scan of the controller source on the same
date. Companion to [authorization-catalog.md](./authorization-catalog.md).

## 1. Headline finding

`Constants.ErrorCodes` holds 13 machine-readable codes. Ten of them are sent today. Three
(`OTP_INVALID`, `PAYMENT_METHOD_DISABLED`, `PAYMENT_FAILED`) are defined but nothing returns them,
so those failures still reach the client as plain text. Of the ten that are sent, the clients
branch on one, `STEP_UP_REQUIRED`. The rest are shown as their `detail` text.

Since 84.45 every coded error is a ProblemDetails body with the code in `Extensions["code"]`,
built by `ProblemExtensions.BuildProblemDetails`. The old `{ code, message }` shape and the
plain-string bodies are gone. `Program.cs` calls `AddProblemDetails()` and all 48 controllers carry
`[ApiController]`, so a bare `NotFound()`, `Unauthorized()` or `BadRequest()` is still turned into
a ProblemDetails body with only `type`, `title`, `status` and `traceId`.

## 2. Error codes

| Code | Status | Raised by | Client handling | Tests |
|---|---|---|---|---|
| `STEP_UP_REQUIRED` | 403 | `RequireStepUpAttribute`, on 56 routes listed in authorization-catalog.md section 3 under the step-up combinations. Detail is "Additional verification is required for this action." when the claim is missing or unreadable, and "Your verification has expired. Please verify again." when it is too old | web `global-http.interceptor.ts:72` opens the step-up prompt; mobile `step_up_interceptor.dart:28` asks for the emailed code and retries once (37.1w) | `RequireStepUpAttributeTests.DeniedResponse_CarriesStepUpRequiredCode`; web `step-up-interceptor.spec.ts` and `step-up.service.spec.ts` |
| `ELECTION_PERMISSION` | 403 | `RequireElectionPermissionAttribute` ("You need the {permission} permission on this election."), used 32 times across AdminElections, ElectionAppointments and Elections; also two approval replies in `ElectionsController` | shown as text | `RequireElectionPermissionAttributeTests` |
| `APPROVAL_PENDING` | 409 | the same two-person step is already waiting. `ProblemExtensions.ApprovalReply` and `ElectionsController` | shown as text | `AdminElectionsControllerTests` |
| `ELECTION_RULES_FROZEN` | 409 | `ElectionRulesFrozenFilter`, a global filter, turns `ElectionRulesFrozenException` into this reply. Detail is the exception message | shown as text | `ElectionRulesFrozenFilterTests.FrozenException_BecomesConflictWithCode` |
| `ELECTION_BUSY` | 409 | `ProblemExtensions.ElectionBusy`, when a step clashed with another write on the same election | shown as text | none assert the code |
| `POLLING_NOT_READY` | 400 | `ElectionsController`, when polling is asked for while a readiness item is unmet. Carries `Extensions["unmet"]` with the unmet item keys, and the detail names them | shown as text; no client reads `unmet` yet | `ElectionsControllerTests.SetPhase_PollingNotReady_NamesTheUnmetItems_AndLeavesOutAdvisory` |
| `ALREADY_VOTED` | 409 | `ElectionsController` vote route ("You have already voted in this election.") | shown as text | `ElectionsControllerTests.Vote_AlreadyVoted_Returns409WithCode` |
| `RATE_LIMITED` | 429 | the rate limiter's `OnRejected` ("Too many requests. Please try again later.") | shown as text | `ErrorResponseShapeTests.LoginRoute_AfterExceedingAuthLimit_Returns429WithRateLimitedCode` |
| `UNAUTHENTICATED` | 401 | JWT bearer `OnChallenge` ("Authentication is required to access this resource.") | the clients act on the 401 status, not the code | `ErrorResponseShapeTests.AdminRoute_WithNoToken_Returns401WithUnauthenticatedCode` |
| `FORBIDDEN` | 403 | JWT bearer `OnForbidden`, when a role or policy check fails ("You do not have permission to access this resource.") | shown as text | `ErrorResponseShapeTests.AdminRoute_WithMemberToken_Returns403WithForbiddenCode` |
| `OTP_INVALID` | | defined, not sent | | |
| `PAYMENT_METHOD_DISABLED` | | defined, not sent | | |
| `PAYMENT_FAILED` | | defined, not sent | | |

## 3. Framework and middleware errors

| Status | When | Body | Source | Tests |
|---|---|---|---|---|
| 500 | unhandled exception | ProblemDetails built by hand: title "An unexpected error occurred.", type RFC 7231 6.6.1, `detail` is the exception message in Development and "Internal Server Error" elsewhere, `correlationId` extension, `stackTrace` extension in Development only, content type `application/problem+json`. No code | `ExceptionMiddleware.cs:70-92` | `ExceptionMiddlewareTests` |
| 429 | a rate limit is hit | ProblemDetails with `RATE_LIMITED` | `RateLimitingExtensions.cs:21-34` | see section 2 |
| 401 | no token or a bad token on a protected route | ProblemDetails with `UNAUTHENTICATED` | `ServiceExtensions.cs` `OnChallenge` | see section 2 |
| 403 | the policy fails on role | ProblemDetails with `FORBIDDEN` | `ServiceExtensions.cs` `OnForbidden` | see section 2 |
| 400 | model binding or DataAnnotations fail | `ValidationProblemDetails` with an `errors` map, from `[ApiController]`. No code | framework | none found |

Rate-limit policies. These are the production limits; the Development environment raises them.

| Policy | Requests | Window |
|---|---|---|
| `Auth` | 10 | 1 minute |
| `Refresh` | 20 | 1 minute |
| `Registration` | 10 | 5 minutes |
| `PasswordReset` | 5 | 15 minutes |
| `ScholarshipStatus` | 10 | 15 minutes |
| `CredentialVerification` | 30 | 15 minutes |
| `Api`, on every controller | 100 | 1 minute |

## 4. Controller error shapes

How the controllers build error results, counted from source on 2026-10-22:

| Result | Argument | Count |
|---|---|---|
| `Problem(...)` | `detail` and `statusCode` | 185 |
| `ProblemWithCode(...)` | code, detail and status | 6 |
| `ApprovalReply(...)` or `ElectionBusy()` | coded reply from `ProblemExtensions` | 6 |
| `BadRequest(...)` | no argument | 3 |
| `Forbid(...)` | no argument | 12 |
| `NotFound(...)` | no argument | 137 |
| `StatusCode(...)` | `500` | 1 |
| `StatusCode(...)` | `503, new { Status = "Degraded", health.Checks }` | 1 |
| `Unauthorized(...)` | no argument | 117 |
| `ValidationProblem(...)` | object | 2 |

No controller now passes a plain string, an object or `ex.Message` to `BadRequest`, `Conflict`,
`Forbid`, `NotFound` or `Unauthorized`. Those all moved to `Problem(...)`.

`Problem(...)` by status: 400: 121, 401: 13, 403: 3, 404: 12, 409: 33, 500: 1, 503: 2.

### 4.1 `Problem` details by status

A backtick entry means the text comes from a variable or expression, usually an exception or
validation message, so the wording is set somewhere else. A quoted entry is a literal in the
controller.

**400**

| Detail | Controller | Count |
|---|---|---|
| "A revocation reason is required." | CredentialVerification | 1 |
| "Assignment failed" | AdminGovernance | 1 |
| "At least one MembershipType is required" | Communication | 1 |
| "At least one PassingYear is required" | Communication | 1 |
| "Candidates can only be added up to the scrutiny phase." | AdminElections | 1 |
| "Config payload is required." | OrgConfig | 1 |
| "Could not add collaborator." | News | 1 |
| "Could not record vote. Ensure the version is active and you haven't voted yet." | Governance | 1 |
| "Could not resend OTP. Ensure the email is correct and not already verified." | Registration | 1 |
| "Election is not ready for declaration." | Elections | 1 |
| "Email is required." | Registration | 1 |
| "Failed to send invitation or registration not approved." | Events | 1 |
| "failed" | Gateways | 1 |
| "Give a reason for the revocation." | Elections | 1 |
| "Invalid gateway type." | Gateways | 1 |
| "Invalid or expired OTP code" | Registration | 1 |
| "Invalid or expired reset token." | Auth | 1 |
| "Invalid phase transition." | Elections | 2 |
| "Invalid user session" | Activity | 1 |
| "Invalid user session" | Financials | 1 |
| "Invalid user session" | JobHub | 2 |
| "Invitation is only available for approved registrations." | Events | 1 |
| "Keep the key until the recount window has ended. Set the window's end date first if it is missing." | Elections | 1 |
| "Name required" | FamilyLink | 1 |
| "No email address is on file for this account, so a verification code cannot be sent. Ask a SuperAdmin to add an email to your account." | Auth | 2 |
| "Only non-member system administrator accounts can be deleted here." | Roles | 1 |
| "Password change failed. Verify your old password." | Profile | 1 |
| "Payment amount is out of the allowed range." | Gateways | 1 |
| "Payment configurations already exist." | PaymentConfig | 1 |
| "Pick a board group from the list." | ElectionPersonas | 1 |
| "Proposer and seconder must be two other eligible voters." | AdminElections | 1 |
| "Query cannot be empty." | Assistant | 1 |
| "Readiness can only be recorded before polling opens." | Elections | 1 |
| "Reference is required." | Gateways | 1 |
| "Target batch values required" | Communication | 1 |
| "Target membership type values required" | Communication | 1 |
| "That verification code is invalid or has expired." | Auth | 1 |
| "The candidate list is published. A candidate can now only withdraw." | AdminElections | 1 |
| "The candidate must be an eligible voter." | AdminElections | 1 |
| "The election is not ready for this step." | Elections | 1 |
| "The key can be recorded as destroyed once the result is declared." | Elections | 1 |
| "The key cannot change once polling has opened." | AdminElections | 1 |
| "The key cannot change once polling has opened." | Elections | 1 |
| "The stored key is not valid." | Elections | 1 |
| "This item is not one an official records by hand." | Elections | 1 |
| "This payment method is not active in the registry." | Gateways | 1 |
| "This payment method is temporarily unavailable via system configuration." | Gateways | 1 |
| "This registration does not require an online payment." | Gateways | 1 |
| "This request has expired. Ask again." | Elections | 1 |
| "Token is required." | Notification | 1 |
| "Topic ID mismatch." | Forum | 1 |
| "Unknown or inactive event registration reference." | Gateways | 1 |
| "User is not associated with a member account." | Events | 1 |
| "User not found or this account cannot be changed." | Roles | 1 |
| "User not found or this account cannot be disabled." | Roles | 1 |
| "User not found" | Roles | 1 |
| "User not found." | Roles | 1 |
| "User or Role not found" | Roles | 1 |
| "Vote could not be recorded." | Elections | 1 |
| "Voting failed. You may have already voted or the poll is closed." | Poll | 1 |
| "You cannot send a mentorship request to yourself." | Mentorship | 1 |
| "You must agree to the declaration to accept." | ElectionAppointments | 1 |
| $"Amount must match the event fee ({expected})." | Gateways | 1 |
| $"An unlock lasts 1 to {Constants.Elections.RulesUnlockMaxMinutes} minutes." | ElectionRulesUnlock | 1 |
| $"Give a reason of at least {Constants.Elections.RulesUnlockReasonMinLength} characters." | ElectionRulesUnlock | 1 |
| $"Position '{position.Title}' is listed twice." | AdminElections | 1 |
| $"Position '{position.Title}' is not supported." | AdminElections | 1 |
| $"The key must be an RSA public key of at least {Elections.BallotKeyMinBits} bits." | AdminElections | 1 |
| $"The reason can be at most {Constants.Elections.RulesUnlockReasonMaxLength} characters." | ElectionRulesUnlock | 1 |
| (no detail) | Events | 2 |
| (no detail) | Theme | 1 |
| `certValidation.ErrorMessage` | Admin | 1 |
| `certValidation.ErrorMessage` | Registration | 1 |
| `CreateErrorDetail(error)` | Elections | 1 |
| `detail ?? "This change is not allowed now."` | AdminElections | 1 |
| `detail` | Elections | 1 |
| `ElectionsController.CreateErrorDetail(error)` | AdminElections | 1 |
| `ex.Message` | Admin | 4 |
| `ex.Message` | AdminGovernance | 1 |
| `ex.Message` | Campaigns | 3 |
| `ex.Message` | FamilyLink | 1 |
| `ex.Message` | Financials | 2 |
| `ex.Message` | Forum | 2 |
| `ex.Message` | PaymentConfig | 2 |
| `ex.Message` | Roles | 1 |
| `ex.Message` | Scholarships | 2 |
| `ex.Message` | Theme | 2 |
| `excelValidation.ErrorMessage` | MemberImport | 1 |
| `logoValidation.ErrorMessage` | Events | 1 |
| `notReady` | AdminElections | 1 |
| `paymentValidation.ErrorMessage` | Registration | 1 |
| `payValidation.ErrorMessage` | Admin | 1 |
| `photoValidation.ErrorMessage` | MemberImport | 1 |
| `photoValidation.ErrorMessage` | Registration | 1 |
| `problem` | AdminElections | 1 |
| `receiptValidation.ErrorMessage` | Events | 1 |
| `receiptValidation.ErrorMessage` | Financials | 1 |
| `response.Message` | Gateways | 1 |
| `validation.ErrorMessage` | Admin | 2 |
| `validation.ErrorMessage` | Gallery | 3 |
| `validation.ErrorMessage` | News | 2 |
| `validation.ErrorMessage` | Profile | 2 |

**401**

| Detail | Controller | Count |
|---|---|---|
| "A member account is required for this registration." | Events | 1 |
| "Facebook authentication failed" | Auth | 1 |
| "Google authentication failed" | Auth | 1 |
| "Invalid or expired refresh token." | Auth | 2 |
| "Invalid session." | Financials | 2 |
| "Invalid username or password" | Auth | 1 |
| "Member profile is required for notifications." | Notification | 1 |
| "Member profile is required." | MemberCommunications | 1 |
| "No refresh token." | Auth | 1 |
| "Sign in as the member who registered to complete payment." | Gateways | 1 |
| "Sign in is required to start this payment." | Gateways | 1 |

**403**

| Detail | Controller | Count |
|---|---|---|
| "Only the official who asked for this count can run it." | Elections | 1 |
| "The person who asked for this step cannot also approve it, and the person who approved a count cannot run it." | Elections | 1 |
| "You can only download your own receipts." | Financials | 1 |

**404**

| Detail | Controller | Count |
|---|---|---|
| "Member or user account not found. Please ensure the member is approved and active." | Admin | 1 |
| "No active EC period found." | Governance | 1 |
| "Ticket code invalid, already used, or not found." | Events | 1 |
| "Topic not found." | Forum | 1 |
| "unsupported_gateway" | Gateways | 1 |
| `ex.Message` | Admin | 1 |
| `ex.Message` | Campaigns | 1 |
| `ex.Message` | FamilyLink | 1 |
| `ex.Message` | Forum | 1 |
| `ex.Message` | Scholarships | 3 |

**409**

| Detail | Controller | Count |
|---|---|---|
| "An unlock is already open." | ElectionRulesUnlock | 1 |
| "Another authorized election official with Approve permission is required." | Elections | 1 |
| "Another persona already uses this name." | ElectionPersonas | 1 |
| "No election is frozen, so there is nothing to unlock." | ElectionRulesUnlock | 1 |
| "No unlock is open." | ElectionRulesUnlock | 1 |
| "The EC period this election fills already has committee members. Remove them before declaring." | Elections | 1 |
| "The key is already recorded as destroyed." | Elections | 1 |
| "This account has election records. Deactivate it instead." | Roles | 1 |
| "This appointment has expired." | ElectionAppointments | 1 |
| "This appointment is already accepted." | ElectionAppointments | 1 |
| "This candidate already has votes recorded and cannot be removed." | AdminElections | 1 |
| "This election already has a Returning Officer. Revoke that appointment first." | ElectionAppointments | 1 |
| "This email belongs to a member. Appoint them as a member instead." | ElectionAppointments | 1 |
| "This item is already recorded." | Elections | 1 |
| "This member has no login yet." | ElectionAppointments | 1 |
| "This member is already a candidate for this seat." | AdminElections | 1 |
| "This official is no longer active, so there is nothing to revoke." | Elections | 1 |
| "This person already holds this role in the election." | ElectionAppointments | 1 |
| "This persona is assigned to an appointment. Deactivate it instead." | ElectionPersonas | 1 |
| "This request has already been decided." | Elections | 1 |
| `detail ?? "Someone else changed this at the same time. Try again."` | AdminElections | 1 |
| `error switch` with four texts | Elections | 1 |
| `ex.Message` | Admin | 1 |
| `ex.Message` | AdminGovernance | 8 |
| `ex.Message` | Elections | 1 |
| `ex.Message` | Mentorship | 1 |

**500**

| Detail | Controller | Count |
|---|---|---|
| `ex.Message` | Notification | 1 |

**503**

| Detail | Controller | Count |
|---|---|---|
| "The code could not be emailed. Try again in a few minutes." | Registration | 1 |
| $"The code could not be emailed to {sentTo}. Try again in a few minutes. If it keeps failing, the site's email settings need checking." | Auth | 1 |

## 5. How the clients read errors

| Client | Fields read, in order | Source |
|---|---|---|
| Web | `detail`, then `title` | `GHCAA.Web/src/app/core/interceptors/global-http.interceptor.ts:135` |
| Mobile | `detail`, then `title`, then `message`, then `error` | `ApiException.serverMessage` in `GHCAA.Mobile/lib/core/api/api_exception.dart:19`. `api_client.dart` and the other mobile readers go through it (84.43), so they agree on the order |

The only code either client reads is `STEP_UP_REQUIRED` (section 2).

## 6. Findings

- `OTP_INVALID`, `PAYMENT_METHOD_DISABLED` and `PAYMENT_FAILED` are defined but never sent. The
  OTP failures in Auth and Registration and the payment failures in Gateways still return plain
  `Problem` text (section 4.1), so a client can only match on English for them.
- The clients branch on `STEP_UP_REQUIRED` only. The election codes and `unmet` are sent but not
  read; the screens show the detail text, which is enough for now.
- `ELECTION_BUSY` has no test that asserts the code.
- 39 `Problem` calls pass `ex.Message` straight to the caller (20 at 400, 7 at 404, 11 at 409, 1 at
  500). The wording then depends on whatever the service threw.
- `"unsupported_gateway"` (404) and `"failed"` (400) in `GatewaysController` still look like codes
  but travel in `detail`.
- `JobHubController.cs:92` returns a bare `StatusCode(500)` when a deactivate fails, which reads as
  a server crash rather than a failed update.
- The 2026-09-26 finding that `Forbid("...")` in `FinancialsController` throws at runtime no
  longer applies. That call is gone; the receipt check now returns a 403 `Problem`.
