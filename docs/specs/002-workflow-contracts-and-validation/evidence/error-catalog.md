# Error Response Catalog

**Reviewed**: 2026-09-26. Built for [../tasks.md](../tasks.md) T014 and tracker item 84.5 from the
45 files in `GHCAA.API/Controllers`, `GHCAA.API/Middleware/ExceptionMiddleware.cs`,
`GHCAA.API/Filters/RequireStepUpAttribute.cs`, `GHCAA.API/Extensions/RateLimitingExtensions.cs`,
`GHCAA.API/Program.cs`, and the client code that reads error bodies. Companion to
[authorization-catalog.md](./authorization-catalog.md).

## 1. Headline finding

The API has one machine-readable error code: `STEP_UP_REQUIRED`. Every other error is told apart
only by its HTTP status and a free-text `detail` or plain string. Clients show that text to the
user and cannot branch on it safely. Story 5 asks for a catalog of the error codes actually
returned. That list has one entry, so the rest of this file catalogs the statuses and texts that
stand in for codes.

`Program.cs` calls `AddProblemDetails()` and all 45 controllers carry `[ApiController]`, so a bare
`NotFound()`, `Unauthorized()` or `BadRequest()` is turned into a ProblemDetails body with only
`type`, `title`, `status` and `traceId`. A result with a string or object argument sends that
argument as its body instead.

## 2. The one error code: `STEP_UP_REQUIRED`

| Field | Value |
|---|---|
| Status | 403 |
| Body | `{ code, message }`, not ProblemDetails |
| Messages | "Additional verification is required for this action." when the claim is missing or unreadable; "Your verification has expired. Please verify again." when it is too old |
| Source | `RequireStepUpAttribute.ErrorCode`, `GHCAA.API/Filters/RequireStepUpAttribute.cs:17` |
| Raised on | 29 routes, listed in authorization-catalog.md section 3 under the step-up combinations |
| Web | `global-http.interceptor.ts:70` reads `code` or `Code` and opens the step-up prompt |
| Mobile | not handled. Nothing in `GHCAA.Mobile/lib` reads the code or calls `/auth/step-up`. The mobile admin screens do call step-up routes, for example `POST /roles/users` and `POST /roles/assign` (`features/admin/roles_service.dart:27` and `:62`) and `POST /admin/elections` (`features/elections/election_service.dart:185`), so on mobile those actions can only fail with the message |
| Tests | `RequireStepUpAttributeTests.DeniedResponse_CarriesStepUpRequiredCode`; web `step-up-interceptor.spec.ts` and `step-up.service.spec.ts` |

## 3. Framework and middleware errors

| Status | When | Body | Source | Tests |
|---|---|---|---|---|
| 500 | unhandled exception | ProblemDetails built by hand: title "An unexpected error occurred.", type RFC 7231 6.6.1, `detail` is the exception message in Development and "Internal Server Error" elsewhere, `correlationId` extension, `stackTrace` extension in Development only, content type `application/problem+json` | `ExceptionMiddleware.cs:72-92` | all six in `ExceptionMiddlewareTests` |
| 429 | a rate limit is hit | empty; `RejectionStatusCode = 429` with no `OnRejected` handler | `RateLimitingExtensions.cs:17` | none |
| 401 | no token or a bad token on a protected route | JWT bearer challenge, empty; no `OnChallenge` override | `ServiceExtensions.cs` `JwtBearerEvents` | none |
| 403 | the policy fails on role | empty; no `OnForbidden` override | framework | none |
| 400 | model binding or DataAnnotations fail | `ValidationProblemDetails` with an `errors` map, from `[ApiController]` | framework | none found |

Rate-limit policies. These are the production limits; the test environment raises them.

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

How the controllers build error results, counted from source:

| Result | Argument | Count |
|---|---|---|
| `Problem(...)` | `detail` and `statusCode` | 101 |
| `BadRequest(...)` | no argument | 2 |
| `BadRequest(...)` | plain string | 8 |
| `Conflict(...)` | plain string | 3 |
| `Forbid(...)` | no argument | 10 |
| `Forbid(...)` | plain string | 1 |
| `NotFound(...)` | `ex.Message` | 1 |
| `NotFound(...)` | no argument | 85 |
| `NotFound(...)` | object | 18 |
| `NotFound(...)` | plain string | 2 |
| `StatusCode(...)` | `500` | 1 |
| `StatusCode(...)` | `503, new { Status = "Degraded", health.Checks }` | 1 |
| `Unauthorized(...)` | no argument | 84 |
| `Unauthorized(...)` | plain string | 3 |
| `ValidationProblem(...)` | object | 2 |

`Problem(...)` by status: 400: 80, 401: 10, 404: 9, 409: 1, 500: 1.

### 4.1 `Problem` details by status

A backtick entry means the text comes from a variable, usually an exception or validation message,
so the wording is set somewhere else. A quoted entry is a literal in the controller.

**400**

| Detail | Controller | Count |
|---|---|---|
| "Assignment failed" | AdminGovernance | 1 |
| "At least one MembershipType is required" | Communication | 1 |
| "At least one PassingYear is required" | Communication | 1 |
| "Config payload is required." | OrgConfig | 1 |
| "Could not add collaborator." | News | 1 |
| "Could not record vote. Ensure the version is active and you haven't voted yet." | Governance | 1 |
| "Could not resend OTP. Ensure the email is correct and not already verified." | Registration | 1 |
| "Email is required." | Registration | 1 |
| "Failed to send invitation or registration not approved." | Events | 1 |
| "failed" | Gateways | 1 |
| "Invalid gateway type." | Gateways | 1 |
| "Invalid or expired OTP code" | Registration | 1 |
| "Invalid or expired reset token." | Auth | 1 |
| "Invalid user session" | Activity | 1 |
| "Invalid user session" | Financials | 1 |
| "Invalid user session" | JobHub | 2 |
| "Invitation is only available for approved registrations." | Events | 1 |
| "Name required" | FamilyLink | 1 |
| "No email address is on file for this account." | Auth | 2 |
| "Only non-member system administrator accounts can be deleted here." | Roles | 1 |
| "Password change failed. Verify your old password." | Profile | 1 |
| "Payment amount is out of the allowed range." | Gateways | 1 |
| "Payment configurations already exist." | PaymentConfig | 1 |
| "Query cannot be empty." | Assistant | 1 |
| "Reference is required." | Gateways | 1 |
| "Target batch values required" | Communication | 1 |
| "Target membership type values required" | Communication | 1 |
| "That verification code is invalid or has expired." | Auth | 1 |
| "This payment method is not active in the registry." | Gateways | 1 |
| "This payment method is temporarily unavailable via system configuration." | Gateways | 1 |
| "This registration does not require an online payment." | Gateways | 1 |
| "Topic ID mismatch." | Forum | 1 |
| "Unknown or inactive event registration reference." | Gateways | 1 |
| "User is not associated with a member account." | Events | 1 |
| "User not found or this account cannot be changed." | Roles | 1 |
| "User not found or this account cannot be disabled." | Roles | 1 |
| "User not found" | Roles | 1 |
| "User not found." | Roles | 1 |
| "User or Role not found" | Roles | 1 |
| "Voting failed. You may have already voted or the poll is closed." | Poll | 1 |
| "You cannot send a mentorship request to yourself." | Mentorship | 1 |
| $"Amount must match the event fee ({expected})." | Gateways | 1 |
| (no detail) | Events | 2 |
| (no detail) | Scholarships | 2 |
| (no detail) | Theme | 1 |
| `certValidation.ErrorMessage` | Admin | 1 |
| `certValidation.ErrorMessage` | Registration | 1 |
| `ex.Message` | Admin | 4 |
| `ex.Message` | Campaigns | 3 |
| `ex.Message` | FamilyLink | 1 |
| `ex.Message` | Forum | 2 |
| `ex.Message` | Roles | 1 |
| `excelValidation.ErrorMessage` | MemberImport | 1 |
| `logoValidation.ErrorMessage` | Events | 1 |
| `paymentValidation.ErrorMessage` | Registration | 1 |
| `payValidation.ErrorMessage` | Admin | 1 |
| `photoValidation.ErrorMessage` | MemberImport | 1 |
| `photoValidation.ErrorMessage` | Registration | 1 |
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
| "Facebook authentication failed" | Auth | 1 |
| "Google authentication failed" | Auth | 1 |
| "Invalid or expired refresh token." | Auth | 2 |
| "Invalid username or password" | Auth | 1 |
| "Member profile is required for notifications." | Notification | 1 |
| "Member profile is required." | MemberCommunications | 1 |
| "No refresh token." | Auth | 1 |
| "Sign in as the member who registered to complete payment." | Gateways | 1 |
| "Sign in is required to start this payment." | Gateways | 1 |

**404**

| Detail | Controller | Count |
|---|---|---|
| "Member or user account not found. Please ensure the member is approved and active." | Admin | 1 |
| "Ticket code invalid, already used, or not found." | Events | 1 |
| "unsupported_gateway" | Gateways | 1 |
| (no detail) | Scholarships | 3 |
| `ex.Message` | Admin | 1 |
| `ex.Message` | Campaigns | 1 |
| `ex.Message` | FamilyLink | 1 |

**409**

| Detail | Controller | Count |
|---|---|---|
| `ex.Message` | Mentorship | 1 |

**500**

| Detail | Controller | Count |
|---|---|---|
| `ex.Message` | Notification | 1 |

## 5. How the clients read errors

| Client | Fields read, in order | Source |
|---|---|---|
| Web | `detail`, then `title` | `GHCAA.Web/src/app/core/interceptors/global-http.interceptor.ts:122` |
| Mobile `ApiClient` | `detail`, then `title`, then `message` | `GHCAA.Mobile/lib/core/api/api_client.dart:128` |
| Mobile `ApiException` | `message`, then `title`, then `detail` | `GHCAA.Mobile/lib/core/api/api_exception.dart:21` |
| Mobile `AuthService` | `message`, then `error` | `GHCAA.Mobile/lib/features/auth/auth_service.dart:153` and `:200` |

A plain-string body such as `BadRequest("...")` has none of these fields, so the web client falls
back to its generic message for it.

## 6. Findings

- No error codes beyond `STEP_UP_REQUIRED`. A client that has to react to one failure (already
  voted, OTP expired, payment method off) can only match on English text.
- The mobile app has no step-up flow, but its admin screens call step-up routes (section 2). Read
  from source only, not tried on a device: creating a user, assigning a role or creating an election
  from mobile should always fail with a 403.
- 16 `Problem` calls pass `ex.Message` straight to the caller, and so does one `NotFound`. The
  wording then depends on whatever the service threw.
- `"unsupported_gateway"` (404) and `"failed"` (400) look like codes but travel in `detail`.
- Four body shapes are in use: ProblemDetails, `{ code, message }`, plain strings and empty bodies.
  The mobile readers disagree on field order, so the same body can show different text.
- 429 and the framework 401 and 403 have empty bodies. A client can only tell a rate limit or an
  expired token from other failures by the status.
- `Forbid("...")` at `FinancialsController.cs:102` throws at runtime and becomes a 500. See
  authorization-catalog.md section 4, where the finding is explained.
- `JobHubController.cs:89` returns a bare `StatusCode(500)` when a deactivate fails, which reads as
  a server crash rather than a failed update.
