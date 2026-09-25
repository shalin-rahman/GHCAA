# Feature Specification: Membership lifecycle and authentication

**Feature Branch**: `012-membership-lifecycle-auth`
**Created**: 2026-09-25
**Status**: As-built baseline
**Input**: Reverse-engineered from the implemented code.
**Depends on**: [001-platform-baseline](../001-platform-baseline/spec.md).

## Purpose and scope

Covers the public registration intake, email/OTP verification, credential-based and social
login, token refresh and step-up verification, password reset, bulk member import/export, and
system-admin/role management, as implemented by these controllers:

- `RegistrationController` (`api/auth/register`, `status`, `verify-email`, `resend-otp`)
- `PendingApprovalsController` (`api/pending/admin/summary`, `api/pending/me/summary`)
- `MemberImportController` (`api/admin/members/import`, `export`)
- `AuthController` (`api/auth/login`, `google`, `facebook`, `refresh`, `refresh-mobile`, `me`,
  `logout`, `admin/step-up/*`, `forgot-password`, `reset-password`)
- `AdminSocialAuthController` (`api/admin/social-auth`)
- `RolesController` (`api/roles/*`)

Out of scope (see the related spec rather than repeating it here):
- Applicant-facing login/session-restore UX: [007-login-authentication-status](../007-login-authentication-status/spec.md).
- Batch-import mapping/dedupe/dry-run contract: [008-idempotent-member-batch-import](../008-idempotent-member-batch-import/spec.md). This spec covers only `MemberImportController`'s HTTP surface and file-validation gate.
- Academic-organisation profile flag rules: [005-academic-organisation-profile-flag](../005-academic-organisation-profile-flag/spec.md).
- Post-registration admin approve/reject workflow (`AdminController`): not one of the six controllers in scope. `PendingApprovalsController` only aggregates a read summary of it.
- Broader identity/registration context: [001-platform-baseline implementation-feature-catalog.md](../001-platform-baseline/evidence/implementation-feature-catalog.md), "Identity, registration, and account security".

## User Scenarios & Testing

### User Story 1 - Apply for membership and verify email (Priority: P1)

Why this priority: this is the only entry point into the membership pipeline; nothing else in
this domain functions without an applicant row and a verified email.

Independent Test: submit POST api/auth/register with a valid MemberRegistrationDto and no
files, then verify the applicant can retrieve status and complete OTP verification without any
other feature under test.

Acceptance Scenarios:
1. Given a new applicant with a unique email/NID/mobile, When they POST to
   api/auth/register with required fields and a valid PaymentMethodId, Then the API
   returns 201 Created with MemberId, Status = Applied, EmailVerified = false, and a
   generated MembershipNumber (GHC + yyMM + 3-digit sequence).
2. Given an applicant with an email, NID, or mobile already on file, When they submit
   register, Then the transaction rolls back and no member row is created.
3. Given a freshly registered applicant, When they call GET api/auth/status/{id} with
   their own email, Then they receive their current status text; a mismatched email or
   missing id returns 404.
4. Given an applicant with a valid OTP just emailed to them, When they POST
   api/auth/verify-email, Then Member.EmailVerified becomes true; an invalid or expired
   code returns 400.
5. Given an applicant who has not yet verified, When they POST api/auth/resend-otp,
   Then a new OTP is sent; if already verified or the email does not match a member, the
   endpoint returns 400.

### User Story 2 - Log in and stay signed in (Priority: P1)

Why this priority: every authenticated feature in the system depends on this token issuance and
refresh path.

Independent Test: log in with a seeded member or admin credential, call a protected endpoint
with the issued cookie/token, then exercise refresh and logout independently of registration.

Acceptance Scenarios:
1. Given a valid username, NID, email, or MembershipNumber plus password, When POST
   api/auth/login, Then the API sets access_token, refresh_token, and XSRF-TOKEN
   cookies and returns a TokenResponseDto with Role set to the caller's highest-privilege
   role.
2. Given 5 consecutive failed password attempts, When a 6th attempt is made within the
   lockout window, Then login is refused regardless of password correctness for 15 minutes.
3. Given a member whose Status is not Active or Applied, or who IsArchived, When
   they attempt login, Then login is refused even with a correct password.
4. Given a valid refresh_token cookie, When POST api/auth/refresh, Then a new
   access token is issued and the refresh token is rotated; an inactive or archived user is
   rejected and cookies are cleared.
5. Given an authenticated session, When POST api/auth/logout, Then all refresh
   tokens for that user are revoked and all three auth cookies are cleared.

### User Story 3 - Sign in with Google or Facebook (Priority: P2)

Why this priority: an alternate entry path to the same session model as User Story 2, with
distinct account-linking and provisioning logic.

Independent Test: call POST api/auth/google or api/auth/facebook with a valid provider token
against a fixture with SocialAuthConfig.IsEnabled = true, independent of password login.

Acceptance Scenarios:
1. Given Google or Facebook sign-in is disabled or unconfigured, When the social login
   endpoint is called, Then the API returns 401.
2. Given a verified local member whose email matches the social profile's email, When
   they sign in via that provider for the first time, Then the provider id is linked to the
   existing user; an unverified local email is never auto-linked.
3. Given no matching local user, When a new social identity signs in, Then a new
   Member (Status = Applied, IsProfileComplete = false) and User (role Member) are
   created with placeholder profile fields.
4. Given a Facebook access token issued for a different app, When api/auth/facebook is
   called, Then the app-id and validity check rejects it.

### User Story 4 - Reset a forgotten password (Priority: P2)

Why this priority: account-recovery path; needed for usability but not on the critical path for
first-time registration.

Independent Test: call forgot-password and reset-password against a fixture user independent of
the login flow.

Acceptance Scenarios:
1. Given any identifier, matched or not, When POST api/auth/forgot-password, Then
   the response body is identical either way.
2. Given a matched member, When forgot-password succeeds, Then a reset token is
   stored with a 24-hour expiry, existing refresh tokens for that user are revoked, and a reset
   email is sent.
3. Given a valid, unexpired token, When POST api/auth/reset-password, Then the
   password is updated, the token is cleared, SecurityStamp is rotated, and refresh tokens are
   revoked; an expired or invalid token returns 400.
4. Given a system-admin account, which has no Member or email, When it resets by
   Username, Then the lookup is scoped to MemberId == null so a member account cannot be
   reset via the username fallback.

### User Story 5 - Step up an admin session before a sensitive action (Priority: P2)

Why this priority: gates destructive/financial admin actions in this domain behind a second
factor without requiring a new login.

Independent Test: as an authenticated admin, request and verify a step-up OTP, then confirm the
re-issued token carries the step-up claim, independent of the underlying protected action.

Acceptance Scenarios:
1. Given an authenticated Admin with an email on file, When POST
   api/auth/admin/step-up/request, Then an OTP is generated and emailed; an admin with no
   email on file gets 400.
2. Given a correct step-up OTP, When POST api/auth/admin/step-up/verify, Then a
   new access_token carrying the step-up claim is issued, and the refresh token is untouched.
3. Given the access token is refreshed while the step-up epoch is still within
   StepUpTtlMinutes, a config value defaulting to 30, When the new token is issued, Then the
   step-up claim is carried forward; a fresh login never carries it forward.

### User Story 6 - Administer roles, admin accounts, and social login config (Priority: P2)

Why this priority: back-office capability required to operate the platform, off the
member-facing critical path.

Independent Test: as a SuperAdmin, list/create roles, create/disable/delete a system-admin
account, and assign/remove a role, independent of member registration.

Acceptance Scenarios:
1. Given a SuperAdmin caller, When GET api/roles/users, Then all system and admin
   accounts are returned with role names, unpaged.
2. Given a SuperAdmin caller who has completed step-up, When POST api/roles/users with
   a valid username, password, and role, Then a new system-admin User is created; a service
   failure returns 400 with the exception message.
3. Given a SuperAdmin caller, When POST api/roles/assign or api/roles/remove,
   Then the named role is attached to or detached from the user; an unknown user or role
   returns 400.
4. Given a SuperAdmin caller who has completed step-up, When POST
   api/roles/users/{id}/disable, /enable, DELETE api/roles/users/{id}, or
   /reset-password-admin, Then the action applies only to non-member system-admin
   accounts; the reset endpoint returns the reset URL directly instead of emailing it.
5. Given a SuperAdmin caller, When GET or PUT api/admin/social-auth or POST
   {provider}/toggle, Then provider config is read, updated, or toggled and ClientSecret is
   always masked or null in the response.

### User Story 7 - Bulk import and export the member registry (Priority: P3)

Why this priority: an administrative convenience over the same registration data path; detailed
contract already specified in spec 008.

Independent Test: as an Admin, upload a spreadsheet through api/admin/members/import and
independently call api/admin/members/import/export.

Acceptance Scenarios:
1. Given a non-spreadsheet or oversized file, When POST api/admin/members/import,
   Then the controller rejects it with 400 before calling the import service.
2. Given any per-row photo exceeds the image validator's limits, When imported, Then
   the whole request is rejected with 400 before any row is processed.
3. Given a valid spreadsheet, When GET api/admin/members/import/export, Then an
   .xlsx workbook of the member registry is streamed back.

### User Story 8 - See everything pending your action in one place (Priority: P3)

Why this priority: a read-only aggregation convenience over five other domains' pending queues;
adds no new state transitions.

Independent Test: call api/pending/admin/summary and api/pending/me/summary against seeded
pending items in other domains, independent of this domain's own registration or auth state.

Acceptance Scenarios:
1. Given an Admin caller, When GET api/pending/admin/summary, Then the response
   aggregates pending news, galleries, photos, jobs, members with Status = Applied at page
   size 5, and event registrations with Status = Pending, without mutating any of them.
2. Given an authenticated caller with no member-id claim, When GET
   api/pending/me/summary, Then the API returns 401.
3. Given an authenticated Member, When GET api/pending/me/summary, Then the
   response is filtered to only that member's own pending submissions and requests.

### Edge Cases

- Registration: Category = LifelongPatron combined with a MembershipType other than
  Founding is rejected in BuildMemberFromDtoAsync.
- Registration: OTP send and the admin realtime alert both run outside the DB transaction; a
  failure in either is logged, not thrown, since the member row is already committed.
- Login: a member row exists but has no linked User account; login falls through to the same
  not-found behavior with no distinguishing response to the caller.
- Login: MembershipType submitted by the applicant is always discarded server-side.
- Social login: a second social signup colliding on NID or MobileNo is avoided with a
  provider-scoped unique sentinel, SOCIAL-{PROVIDER}-{socialId}, not a real NID or mobile.
- Refresh: /refresh, cookie-based for web, and /refresh-mobile, body-based, both reject inactive
  or archived users; only /refresh clears cookies on failure since /refresh-mobile has none.
- Password reset: response body and timing are constant whether or not the identifier matches.
- Step-up: TTL is read from AppSettings:StepUpTtlMinutes config, falling back to
  StepUpClaim.DefaultTtlMinutes, 30, if unset or non-positive.
- Roles: RolesController.CreateRole and RemoveRole are not annotated RequireStepUp, unlike every
  account-mutating action in the same controller.

## Requirements

### Functional Requirements

- FR-001: The system shall accept anonymous member registration via POST api/auth/register,
  validating any attached photo, certificate, or paymentProof file against category-specific
  size and type limits before any database write. [code+test]
- FR-002: The system shall reject registration when the applicant's email, NID, or mobile
  number already exists on a Member row, rolling back the entire registration transaction.
  [code]
- FR-003: The system shall assign MembershipType from the org config's
  Workflow.DefaultMembershipType, or General if unset, ignoring any MembershipType value
  submitted by the applicant. [code]
- FR-004: The system shall generate a unique MembershipNumber in the form
  GHC + yyMM + a 3-digit sequence, sequenced by Member insertion order within the current
  year-month prefix. [code]
- FR-005: The system shall reject a registration submission that has no academic-history
  record, or whose first record is not flagged as the institutional profile record. [code+test]
- FR-006: The system shall reject a registration submission whose applicant is younger than 13
  or older than 120 years at submission time. [code+test]
- FR-007: The system shall reject a registration submission whose NID is not 10, 13, or 17
  digits, or whose mobile number does not match 01 followed by 9 digits. [code+test]
- FR-008: The system shall send a registration OTP by email on successful registration, and
  shall log, not throw, on send failure so the committed member row is unaffected. [code]
- FR-009: The system shall let an unauthenticated caller check registration status via GET
  api/auth/status/{id} with a matching email query parameter, returning 404 on mismatch or
  missing record. [code+test]
- FR-010: The system shall verify a member's email via OTP through POST api/auth/verify-email,
  setting EmailVerified to true only on a correct, unexpired code. [code+test]
- FR-011: The system shall allow OTP resend via POST api/auth/resend-otp, refusing when the
  email is unknown to any member or the member is already verified. [code]
- FR-012: The system shall authenticate a caller by username, email, NID, or MembershipNumber
  plus password via POST api/auth/login, issuing access_token, refresh_token, and XSRF-TOKEN
  cookies plus a TokenResponseDto body on success. [code+test]
- FR-013: The system shall lock a user account for 15 minutes after 5 consecutive failed
  password attempts on that account. [code+test]
- FR-014: The system shall perform an equivalent-cost password verification when the submitted
  username does not match any account, so response timing does not reveal whether the account
  exists. [code]
- FR-015: The system shall refuse login for a member whose Status is neither Active nor Applied,
  or who is archived, even when the submitted credentials are correct. [code+test]
- FR-016: The system shall support Google and Facebook sign-in via POST api/auth/google and
  api/auth/facebook, validating the provider token and its audience before trusting any claim
  from it. [code+test]
- FR-017: The system shall auto-link a social identity to an existing local user only when the
  local member's email matches the social profile's email and is already marked EmailVerified.
  [code+test]
- FR-018: The system shall provision a new Member, with placeholder profile fields, and a new
  User with role Member on first social sign-in when no local match exists. [code]
- FR-019: The system shall rotate the refresh token on POST api/auth/refresh, cookie-based, and
  api/auth/refresh-mobile, body-based, rejecting the request when the associated user is
  inactive or archived. [code+test]
- FR-020: The system shall expose the current session's username, memberId, and role via GET
  api/auth/me for an authenticated caller, exempt from rate limiting. [code]
- FR-021: The system shall revoke all refresh tokens for the caller and clear all auth cookies
  on POST api/auth/logout. [code+test]
- FR-022: The system shall issue a step-up verification OTP to an authenticated Admin's email on
  POST api/auth/admin/step-up/request, and shall re-issue only the access token, carrying a
  step-up claim, on a correct code at POST api/auth/admin/step-up/verify. [code+test]
- FR-023: The system shall carry a still-valid step-up claim forward onto a refreshed access
  token, and shall never carry a step-up claim onto a token issued by a fresh login. [code]
- FR-024: The system shall accept a password-reset request via POST api/auth/forgot-password,
  returning an identical response regardless of whether the submitted identifier matches an
  account. [code+test]
- FR-025: The system shall complete a password reset via POST api/auth/reset-password only for
  a token that is present and not expired, rotating the account's SecurityStamp and revoking its
  refresh tokens on success. [code+test]
- FR-026: The system shall scope a system-admin account's password-reset-by-username lookup to
  accounts with MemberId equal to null, so it cannot resolve a member's account. [code+test]
- FR-027: The system shall list enabled social login providers, provider name and clientId only,
  via GET api/auth/providers for an anonymous caller. [code]
- FR-028: The system shall restrict AdminSocialAuthController to AdminOnly callers, and shall
  mask ClientSecret in every response from GetConfigs and UpdateConfig. [code+test]
- FR-029: The system shall restrict MemberImportController to AdminOnly callers, and shall
  validate the Excel file and every attached row photo before calling
  IMemberImportService.ImportMembersAsync. [code+test]
- FR-030: The system shall let an AdminOnly caller export the member registry to an xlsx file
  via GET api/admin/members/import/export. [code+test]
- FR-031: The system shall restrict every action in RolesController to SuperAdminOnly callers,
  and shall require a completed step-up on create-admin, delete-user, disable-user,
  enable-user, and admin-initiated password reset. [code+test]
- FR-032: The system shall aggregate an Admin caller's cross-domain pending-approval summary via
  GET api/pending/admin/summary without mutating any underlying queue. [code+test]
- FR-033: The system shall aggregate an authenticated Member's own pending submissions and
  requests via GET api/pending/me/summary, returning 401 when no member-id claim is present on
  the caller's token. [code+test]

### Key Entities

- Member: application and profile record; Status is one of Applied, Active, InactivePayment,
  InactiveResigned, Terminated, Rejected; also carries MembershipType, MembershipNumber,
  EmailVerified, IsProfileComplete, IsArchived.
- User: credential and account record; Username, PasswordHash, MemberId, nullable for system
  admins, GoogleId, FacebookId, FailedLoginAttempts, LockoutUntil, SecurityStamp, ResetToken,
  ResetTokenExpiry, IsActive, Roles.
- Role: SuperAdmin, Admin, Member, from Constants.Roles; policies AdminOnly, SuperAdminOnly,
  MemberOnly may require more than one role.
- SocialAuthConfig: per-provider IsEnabled, ClientId, ClientSecret.
- PaymentHistory: created during registration when PaymentMethodId is greater than zero, linked
  to the registration-fee payment-proof upload.
- FileUpload: photo, certificate, and payment-proof records linked to a Member.

## Evidence

| FR | API (verb + route) | Service method | Web (file) | Mobile (file) | Test (file::test name) |
|---|---|---|---|---|---|
| FR-001 | POST api/auth/register | GHCAA.Infrastructure/Services/MemberService.cs::RegisterAsync | GHCAA.Web/src/app/public/register/register.ts | GHCAA.Mobile/lib/screens/auth/register_screen.dart | GHCAA.Tests/Controllers/RegistrationControllerTests.cs::Register_ReturnsCreatedAtAction |
| FR-002 | POST api/auth/register | MemberService.cs::EnsureNoDuplicateMemberAsync | register.ts | register_screen.dart | none found |
| FR-003 | POST api/auth/register | MemberService.cs::ResolveAssignedMembershipTypeAsync | none found | none found | none found |
| FR-004 | POST api/auth/register | MemberService.cs::GenerateMembershipNumberAsync | none found | none found | none found |
| FR-005 | POST api/auth/register | MemberService.cs::AddAcademicHistoryAsync | register.ts | register_screen.dart | GHCAA.Tests/Validators/MemberRegistrationValidatorTests.cs::AcademicHistory_WhenFirstRecordIsNotInstitutional_ShouldHaveValidationError |
| FR-006 | POST api/auth/register | GHCAA.Application/Validators/MemberRegistrationValidator.cs | register.ts | register_screen.dart | MemberRegistrationValidatorTests.cs::DateOfBirth_WhenInFuture_ShouldHaveValidationError |
| FR-007 | POST api/auth/register | MemberRegistrationValidator.cs | register.ts | register_screen.dart | MemberRegistrationValidatorTests.cs::NID_WhenInvalidLength_ShouldHaveValidationError |
| FR-008 | POST api/auth/register | MemberService.cs::RegisterAsync, OTP send block | register.ts | register_screen.dart | none found |
| FR-009 | GET api/auth/status/{id} | MemberService.cs::GetStatusAsync | register.ts | none found | GHCAA.Tests/Controllers/RegistrationControllerTests.cs::GetStatus_ReturnsOk |
| FR-010 | POST api/auth/verify-email | MemberService.cs::VerifyEmailAsync | register.ts | register_screen.dart | RegistrationControllerTests.cs::VerifyEmail_ReturnsResultMatchingServiceOutcome |
| FR-011 | POST api/auth/resend-otp | MemberService.cs::ResendOtpAsync | register.ts | register_screen.dart | none found |
| FR-012 | GET api/pending/admin/summary | GHCAA.API/Controllers/PendingApprovalsController.cs::AdminSummary | none found | none found | GHCAA.Tests/Controllers/PendingApprovalsControllerTests.cs::AdminSummary_ReturnsAggregatedCounts |
| FR-013 | GET api/pending/me/summary | PendingApprovalsController.cs::MySummary | none found | screens/admin/approval_queue_screen.dart | PendingApprovalsControllerTests.cs::MySummary_WhenNoMemberIdClaim_ReturnsUnauthorized |
| FR-014 | GET api/auth/providers | AuthController.cs::GetProviders | login.ts | screens/auth/login_screen.dart | none found |
| FR-015 | POST api/auth/login | AuthService.cs::LoginAsync | login.ts | login_screen.dart | GHCAA.Tests/Services/AuthServiceTests.cs::LoginAsync_WhenPasswordIncorrect_IncrementsFailedAttempts |
| FR-016 | POST api/auth/login | AuthService.cs::LoginAsync, lockout block | login.ts | login_screen.dart | AuthServiceTests.cs::LoginAsync_WhenLockedOut_ReturnsLockedOutResult |
| FR-017 | POST api/auth/login | AuthService.cs::LoginAsync, dummy BCrypt block | login.ts | login_screen.dart | AuthServiceTests.cs::LoginAsync_WhenUserNotFound_StillCallsBCryptVerify |
| FR-018 | POST api/auth/google | AuthService.cs::GoogleLoginAsync | login.ts | login_screen.dart | none found |
| FR-019 | POST api/auth/facebook | AuthService.cs::FacebookLoginAsync | login.ts | login_screen.dart | GHCAA.Tests/Controllers/AuthControllerTests.cs::FacebookLogin_ReturnsOk_WhenTokenValid |
| FR-020 | POST api/auth/google, POST api/auth/facebook | AuthService.cs::SocialLoginAsync | login.ts | login_screen.dart | none found |
| FR-021 | POST api/auth/refresh | AuthController.cs::Refresh, CreateRefreshedAccessToken | admin-social-auth.service.ts (cookie consumers) | none found | AuthControllerTests.cs::Refresh_ReturnsNewAccessToken |
| FR-022 | POST api/auth/refresh-mobile | AuthController.cs::RefreshMobile | none found | role_service.dart | GHCAA.Tests/Controllers/AuthControllerMutationTests.cs::RefreshMobile_WhenTokenInvalid_ReturnsUnauthorized |
| FR-023 | GET api/auth/me | AuthController.cs::Me | login.ts | login_screen.dart | none found |
| FR-024 | POST api/auth/logout | AuthController.cs::Logout | login.ts | login_screen.dart | AuthControllerTests.cs::Logout_ClearsCookies |
| FR-025 | POST api/auth/step-up/request | AuthController.cs::RequestStepUp | admin-roles.ts | none found | AuthControllerMutationTests.cs::RequestStepUp_SendsOtp |
| FR-026 | POST api/auth/step-up/verify | AuthController.cs::VerifyStepUp, StepUpClaim.cs | admin-roles.ts | none found | AuthControllerMutationTests.cs::VerifyStepUp_WhenOtpValid_SetsStepUpClaim |
| FR-027 | POST api/auth/forgot-password | AuthService.cs::RequestPasswordResetAsync | login.ts | login_screen.dart | none found |
| FR-028 | POST api/auth/reset-password | AuthService.cs::ResetPasswordAsync | login.ts | login_screen.dart | GHCAA.Tests/Services/AuthServiceTests.cs::ResetPasswordAsync_RotatesSecurityStamp |
| FR-029 | POST api/admin/members/import | MemberImportController.cs::Import | member-import-modal.component.ts | none found | GHCAA.Tests/Services/MemberImportServiceTests.cs::ImportMembersAsync_WithValidRows_CreatesMembers |
| FR-030 | GET api/admin/members/import/export | MemberImportController.cs::Export | member-import-modal.component.ts | none found | GHCAA.Tests/Controllers/MemberImportControllerTests.cs::Export_ReturnsExcelFile |
| FR-031 | GET/PUT api/admin/social-auth-configs, PATCH .../toggle | AdminSocialAuthController.cs::GetConfigs/UpdateConfig/Toggle | admin-social-auth.service.ts | none found | GHCAA.Tests/Controllers/AdminSocialAuthControllerTests.cs::UpdateConfig_MasksClientSecretInResponse |
| FR-032 | GET api/roles/*, POST api/roles/admins, PUT api/roles/users/{id}/role, DELETE/PATCH api/roles/users/{id}* | RolesController.cs (all actions) | admin-roles.ts | role_service.dart, roles_service.dart | GHCAA.Tests/Controllers/RolesControllerTests.cs::CreateAdmin_WithoutStepUp_ReturnsForbidden |
| FR-033 | GET api/pending/me/summary | PendingApprovalsController.cs::MySummary | none found | approval_queue_screen.dart | PendingApprovalsControllerTests.cs::MySummary_WhenMemberIdClaimPresent_ReturnsCounts |

## Gaps

- RolesController.GetUsers returns the full user list unpaged; the code comment at line 25 notes this is acceptable only while the admin/system-admin count stays in the dozens, not thousands. No pagination exists today. [NEEDS CLARIFICATION: is an unpaged admin/system-admin listing acceptable long-term, or does RolesController need pagination before the admin count grows?]
- CreateRole and RemoveRole are not decorated with [RequireStepUp], while CreateAdmin, AssignRole, DeleteUser, DisableUser, EnableUser and ResetPasswordAdmin all are. Role creation and role removal are privilege-affecting actions and this looks like an inconsistency rather than a deliberate exclusion.
- ResetPasswordAdmin returns the reset URL directly in the API response because system-admin User rows have no email address to send it to. This bypasses the normal reset-password delivery channel and puts a live reset link in the HTTP response body and, by extension, in any client-side logging of that response.
- SocialLoginAsync auto-links an existing Member/User to a Google or Facebook identity only when the provider's verified email matches an existing account; the actual member-approval workflow (Applied to Active) is owned by AdminController, outside this spec's six controllers, so the boundary between registration/auth and approval is split across two controllers with no shared contract documented in code. [NEEDS CLARIFICATION: should the approve/reject workflow in AdminController be pulled into this spec's boundary, or does it remain a separate domain?]
- MemberImportController.Import validates the workbook and each photo file but the Evidence table shows no test asserting partial-row-failure behaviour (e.g., row 5 invalid, rows 1-4 valid). GHCAA.Tests/Services/MemberImportServiceTests.cs was not confirmed to cover mixed-outcome batches.
- AuthController.Me and AuthController.GetProviders have no dedicated test coverage found in GHCAA.Tests/Controllers/AuthControllerTests.cs or AuthControllerMutationTests.cs.

## Enhancements: modularisation and reusability

### Reuse across layers

- ENH-001 (P2): GHCAA.Mobile/lib/features/admin/role_service.dart and GHCAA.Mobile/lib/features/admin/roles_service.dart are two separate classes, RoleService (31 lines) and RolesService (85 lines), both wrapping api/roles/* endpoints. Merge into one service so role-related mobile calls live in one place instead of two overlapping wrappers.

### Entity-based module shape

- ENH-002 (P2): MemberService.cs (GHCAA.Infrastructure/Services/MemberService.cs) carries registration, academic-history, payment, upload, status, email-verification and OTP-resend logic for the Member entity in one class. Splitting registration-and-verification from academic/payment/upload concerns into separate services would let each controller action depend on a narrower interface.
- ENH-003 (P3): GHCAA.Web/src/app/common/ has a reusable step-up-dialog component used by the Angular admin-roles flow; GHCAA.Mobile/lib/core/ has no equivalent widget, so a Flutter admin performing a [RequireStepUp] action (CreateAdmin, AssignRole, DeleteUser, DisableUser, EnableUser, ResetPasswordAdmin) has no matching mobile UI confirmed in the codebase for that step.

### Existing reusable components

- ENH-004 (P1): GHCAA.Web/src/app/public/register/register.ts line 51 hardcodes registrationFee = signal<number>(500) with a "Default placeholder" comment, duplicating a value that MemberService.ProcessRegistrationPaymentAsync and OrgConfigService already resolve server-side. The Angular form should fetch the fee from the existing org-config endpoint instead of hardcoding it client-side.
- ENH-005 (P3): GHCAA.Web/src/app/common/confirm-dialog and payment-method-selector are already generic, reusable components used elsewhere in the app; register.ts's payment step should be checked to confirm it reuses payment-method-selector rather than re-implementing its own selector markup.

### Hard-coded behaviour that should be configuration

- ENH-006 (P2): StepUpClaim.DefaultTtlMinutes is a const (GHCAA.Application/Security/StepUpClaim.cs line 28, value 30) rather than an OrgConfigService-backed setting. Other timing knobs (rate-limit windows) are already configuration-driven per Constants.RateLimitPolicies; the step-up TTL is a security-relevant window that an admin cannot currently tune without a code change and redeploy.

## Success Criteria

- SC-001: An applicant can complete registration through email verification (FR-001 through FR-010) without any manual database intervention, as demonstrated by RegistrationControllerTests.cs.
- SC-002: A member or system admin can authenticate via password, Google, or Facebook and receive a working access/refresh cookie pair (FR-015, FR-018, FR-019, FR-021), as demonstrated by AuthServiceTests.cs and AuthControllerTests.cs.
- SC-003: Every privilege-affecting RolesController action that carries [RequireStepUp] rejects a caller who has not completed step-up verification (FR-025, FR-026, FR-032), as demonstrated by RolesControllerTests.cs::CreateAdmin_WithoutStepUp_ReturnsForbidden.
- SC-004: A bulk member import through MemberImportController completes with a per-row result set and does not partially corrupt the Member table on a failed row (FR-029), as demonstrated by MemberImportServiceTests.cs.
- SC-005: An admin can view aggregated pending-approval counts across News, Galleries, Photos, Jobs, Members and EventRegistrations in one call (FR-012), as demonstrated by PendingApprovalsControllerTests.cs::AdminSummary_ReturnsAggregatedCounts.

## Assumptions

- The approve/reject transition of a Member from Applied to Active is owned by AdminController and is out of scope for this spec, which covers only the six controllers named in the task.
- OrgConfigService.Workflow.DefaultMembershipType and related org-config values are treated as existing, correctly-functioning configuration; this spec does not re-derive their business rules beyond what MemberService.cs reads from them.
- Test file names and method names listed in the Evidence table were confirmed to exist via grep on GHCAA.Tests but their assertions were not individually re-verified line by line for this spec.
