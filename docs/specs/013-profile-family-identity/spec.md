# Profile, family links, secure files, and credential verification

**Feature Branch**: `013-profile-family-identity`
**Created**: 25-09-2026
**Status**: As-built baseline
**Input**: Reverse-engineered from the implemented code.
**Depends on**: spec 003, the alumni programs and verification spec that first covers digital credentials; spec 005, the academic and organisation profile-flag spec that shapes `BaseMemberDto`'s academic fields; and the "Member profiles, directory, and identity documents" and "Family links" sections of `docs/specs/001-platform-baseline/evidence/implementation-feature-catalog.md`, the platform-baseline feature catalog.

## Purpose and scope

This spec covers the member-facing identity surface: a member's own profile read/update, password change, digital ID-card and certificate issuance (image and PDF), photo and signature upload, family-link requests between members, the secure-file download gate that protects uploaded documents, and the public short-code credential verification page.

In scope:
- `ProfileController` (`api/profile`): profile read/update, change-password, id-card/certificate (data URI and PDF), photo/signature upload.
- `FamilyLinkController` (`api/family-links`, with legacy aliases under `api/members/family` and `api/Family`): send/respond/cancel/remove, sent/received lists, my-family, a target member's family, and name search.
- `SecureFilesController` (`api/secure-files`): the single file-download gate for member-owned documents stored outside `wwwroot`.
- `CredentialVerificationController` (`api/verify`): anonymous short-code lookup and admin revocation.

Out of scope:
- Admin-side member management (list, approve, archive, admin password reset, admin-triggered photo/signature/document updates): covered by `AdminController`, not read in this pass.
- The academic/professional/EC history fields carried on `BaseMemberDto`: spec 005, the academic and organisation profile-flag spec, owns their validation rules.
- The directory, search, and committee-period surfaces on `NetworkingController`: spec 003, the alumni programs and verification spec, and the platform-baseline catalog's "Member profiles" section own those.
- Registration and initial member creation: a different DTO path (`CreateMemberDto`), not `BaseMemberDto` via `UpdateProfileDto`.

## User Scenarios & Testing

### User Story 1 - A member views and edits their own profile (Priority: P1)

A logged-in member opens their profile page, sees their stored personal, address, and academic details, edits a field, and saves.

**Why this priority**: Every other feature in this spec (photo, signature, id-card, family) hangs off this page; without it a member cannot see their own record.

**Independent Test**: Log in as a member, call `GET api/profile`, change a field and call `PUT api/profile`, then re-fetch and confirm the change persisted.

**Acceptance Scenarios**:
1. **Given** a member with a valid session, **When** they call `GET api/profile`, **Then** they receive their own `MemberProfileDto`, with `NID` masked to its first 3 and last 2 characters when `IsNIDPublic` is false and the caller is privileged.
2. **Given** a member submits an `UpdateProfileDto` with a changed `PresentAddress`, **When** `PUT api/profile` runs, **Then** the stored member record reflects the new address and the response is `{Message: "Profile updated successfully"}`.
3. **Given** a caller whose member id claim is absent or malformed, **When** they call `GET api/profile` or `PUT api/profile`, **Then** the API returns 401.

---

### User Story 2 - A member changes their password (Priority: P1)

A member enters their old and new password on the profile page. The system verifies the old password, rotates credentials, and keeps the member signed in under the new password.

**Why this priority**: A password-change flow that leaves stale tokens valid is a real account-takeover risk if the old password was compromised.

**Independent Test**: Call `POST api/profile/change-password` with the wrong old password (expect 400), then with the correct old password (expect 200 and fresh cookies), then confirm the previous refresh token no longer works.

**Acceptance Scenarios**:
1. **Given** a member submits the wrong `OldPassword`, **When** `POST api/profile/change-password` runs, **Then** the response is 400 with detail "Password change failed. Verify your old password." and no state changes.
2. **Given** a member submits a `NewPassword` that is at least 8 characters with an upper-case letter, a lower-case letter, and a digit, **When** the old password matches, **Then** the password hash is replaced, `MustChangePassword` is cleared, `SecurityStamp` is rotated, all refresh tokens for that user are revoked, and new `access_token`/`refresh_token`/`XSRF-TOKEN` cookies are issued (access token 65 minutes, refresh token 7 days).

---

### User Story 3 - A member downloads their digital ID card and certificate (Priority: P2)

A member views a QR-coded digital ID card and a membership certificate, and can download either as a PDF.

**Why this priority**: A visible, verifiable credential is a core membership benefit, but it is not required to use the rest of the portal.

**Independent Test**: Call `GET api/profile/id-card` and confirm a `DataUri` with an embedded QR code pointing at a 10-character short code; call `GET api/verify/{shortCode}` and confirm it resolves.

**Acceptance Scenarios**:
1. **Given** a member with an active profile, **When** they call `GET api/profile/id-card`, **Then** the response is `{DataUri}`, an SVG data URI containing a QR code that encodes `{PortalBaseUrl}/verify/{shortCode}`.
2. **Given** the same member calls `GET api/profile/id-card/pdf`, **When** the request succeeds, **Then** the response is a `application/pdf` file named `ID_Card_{memberId}.pdf`.
3. **Given** a member views their id-card twice, **When** each view runs, **Then** a new `IssuedCredential` row with a distinct short code is minted each time; no existing valid credential is reused.

---

### User Story 4 - Two members link as family (Priority: P2)

One member sends a family-link request to another by membership number; the receiving member accepts or rejects it; either party can later remove the link.

**Why this priority**: Family visibility drives some directory and notification behaviour, but a member can use the rest of the portal without ever linking family.

**Independent Test**: As member A, `POST api/family-links/send` to member B's membership number; as member B, `GET api/family-links/received`, then `POST api/family-links/respond` with `Approve: true`; as either, confirm `GET api/family-links/my-family` lists the link.

**Acceptance Scenarios**:
1. **Given** member A sends a request to an unknown membership number, **When** `POST api/family-links/send` runs, **Then** the response is 404.
2. **Given** member A sends a request to member A's own membership number, **When** `POST api/family-links/send` runs, **Then** the response is 400 (self-link rejected).
3. **Given** member A already has a pending request to member B, **When** member A sends another request to member B, **Then** the response is 400 (duplicate request rejected).
4. **Given** member B has a `Requested` link from member A, **When** member B calls `POST api/family-links/respond` with `Approve: true`, **Then** the link's status becomes accepted and both members see each other under `GET api/family-links/my-family`.
5. **Given** a third member C who is not family-linked to member B, **When** member C calls `GET api/family-links/{memberB}/family` and member B's `IsFamilyPublic` is false, **Then** member C does not see member B's family list.

---

### User Story 5 - A member's uploaded document is downloaded through the secure-file gate (Priority: P1)

A member or an admin requests a stored certificate, payment proof, or signature file by its path; the system serves it only to the owner or an admin, and only from the two configured storage roots.

**Why this priority**: This is the only backstop between a guessable file path and another member's private document; a defect here is a direct privacy leak.

**Independent Test**: As a non-owning, non-admin member, request another member's file path and confirm 403; as the owner, confirm 200; as an admin, confirm 200 for any member's file; request a path containing `..` and confirm 404.

**Acceptance Scenarios**:
1. **Given** a `FilePath` with no matching `FileUpload` row, **When** `GET api/secure-files/{*filePath}` runs, **Then** the response is 404 and a warning is logged.
2. **Given** a non-owner, non-admin caller, **When** they request another member's file, **Then** the response is 403 (`Forbid`) and a warning is logged.
3. **Given** a path containing `..`, **When** the request runs, **Then** the response is 404 before any disk access.
4. **Given** the owning member or an Admin/SuperAdmin caller, **When** they request a file that resolves inside `wwwroot`'s public uploads root or the secure uploads root, **Then** the file bytes are returned with a content type derived from its extension (`.pdf`, `.jpg`/`.jpeg`, `.png`, else `application/octet-stream`).

---

### User Story 6 - Anyone verifies a printed or scanned credential (Priority: P2)

Someone scans the QR code on a printed ID card or certificate, or types the short code into the public verify page, and sees whether the credential is currently valid.

**Why this priority**: This is the public trust surface for the credential system; it must work without login, but a member cannot be blocked from using their own profile if it is briefly unavailable.

**Independent Test**: Call `GET api/verify/{shortCode}` anonymously for a valid code (expect a result with `Valid: true`), then for a revoked code (expect `Valid: false`), then for a malformed code (expect 404).

**Acceptance Scenarios**:
1. **Given** an anonymous caller and a `shortCode` that is blank or not exactly 10 characters, **When** `GET api/verify/{shortCode}` runs, **Then** the response is 404 and `VerifyCredentialAsync` is never called.
2. **Given** a valid 10-character code, **When** the request runs, **Then** the response is 200 with the verification result (`Valid`, `MemberName`, `Status`).
3. **Given** an Admin caller and a non-blank `Reason`, **When** `POST api/verify/{shortCode}/revoke` runs, **Then** the credential's `IsRevoked`, `RevokedReason`, and `RevokedOn` are set and the response is 204; a blank `Reason` returns 400.

## Edge Cases

- `UpdateProfileAsync` sets `NotifyEventCreation`, `NotifyParticipationApproval`, `NotifyRegistrationUpdate`, and `NotifyCommitteeChanges` from the DTO but the code path for `NotifyRelevantUpdates` was not confirmed in this pass against `MemberService_Profile.cs`; see Gaps.
- `IDCardService.VerifyCredentialAsync` treats a credential as valid when `!IsRevoked && (ExpiresOn is null || ExpiresOn > UtcNow)`, but no code path in `IDCardService.cs` sets `ExpiresOn`, so in practice a credential only stops being valid through explicit revocation.
- `FamilyLinkController.RemoveLinkAsync` lets either party cancel a link in any status, including an already-accepted one, not only a pending request.
- `SecureFilesController` resolves `FileStorage:BasePhysicalPath` and falls back to `wwwroot` or `AppDomain.CurrentDomain.BaseDirectory` when that config key is absent, then requires the resolved path to land inside one of the two computed roots before serving it (`SecureFilesController.cs:60-90`).
- `GET api/family-links/{memberId}/family` is reachable by any authenticated member for any `memberId`, gated only by the target's `IsFamilyPublic` flag inside `FamilyLinkService.GetFamilyAsync`, not by a route-level policy.

## Requirements

### Functional Requirements

- FR-001: `ProfileController` shall return the caller's own profile on `GET api/profile`, returning 401 when the member id claim is absent or unparsable and 404 when no member record exists for that id. [code+test]
- FR-002: `ProfileController` shall update the caller's own profile on `PUT api/profile` from an `UpdateProfileDto`, returning 404 if the update target does not exist and `{Message: "Profile updated successfully"}` on success. [code+test]
- FR-003: `ProfileController` shall change the caller's password on `POST api/profile/change-password` only after `UserService.ChangePasswordAsync` verifies `OldPassword` against the stored BCrypt hash, and on success shall rotate `SecurityStamp`, revoke all of that user's refresh tokens, and re-issue `access_token` (65 minute TTL), `refresh_token` (7 day TTL), and `XSRF-TOKEN` cookies; a failed verification shall return 400 with detail "Password change failed. Verify your old password." [code+test]
- FR-004: `ProfileController` shall return the caller's digital ID card as a data URI on `GET api/profile/id-card`. [code+test]
- FR-005: `ProfileController` shall return the caller's digital ID card as a PDF named `ID_Card_{memberId}.pdf` on `GET api/profile/id-card/pdf`. [code]
- FR-006: `ProfileController` shall return the caller's membership certificate as a data URI on `GET api/profile/certificate`. [code+test]
- FR-007: `ProfileController` shall return the caller's membership certificate as a PDF named `Certificate_{memberId}.pdf` on `GET api/profile/certificate/pdf`. [code]
- FR-008: `ProfileController` shall accept a photo upload of up to 5 MB on `POST api/profile/photo`, validated by `FileValidationService` against the image content-type allowlist, extension allowlist, and file-signature check, returning 400 on any validation failure and `{Message, PhotoPath}` on success. [code]
- FR-009: `ProfileController` shall accept a signature upload of up to 2 MB on `POST api/profile/signature`, validated the same way as the photo upload, returning 400 on any validation failure and `{Message, SignaturePath}` on success. [code]
- FR-010: `IDCardService` shall mint a new `IssuedCredential` row with a fresh 10-character short code on every id-card or certificate view, rather than reusing an existing unexpired, unrevoked credential for the same member. [code]
- FR-011: `CredentialVerificationController` shall allow anonymous, rate-limited (`RateLimitPolicies.CredentialVerification`) access to `GET api/verify/{shortCode}`, returning 404 when `shortCode` is blank or not exactly 10 characters and otherwise the result of `IDCardService.VerifyCredentialAsync`. [code+test]
- FR-012: `CredentialVerificationController` shall require the `AdminOnly` policy and a non-blank `Reason` for `POST api/verify/{shortCode}/revoke`, returning 400 for a blank reason, 204 on a successful revocation, and 404 if the short code does not exist. [code]
- FR-013: `FamilyLinkController` shall create a pending family-link request on `POST api/family-links/send` (aliased at `POST api/members/family` for mobile), returning 404 for an unknown target membership number and 400 for a self-link or a duplicate pending request to the same target. [code+test]
- FR-014: `FamilyLinkController` shall record a member's acceptance or rejection of a pending request on `POST api/family-links/respond`, returning 404 if the request does not exist. [code+test]
- FR-015: `FamilyLinkController` shall let either party of a family link remove it, in any status, on `DELETE api/family-links/remove/{requestId}`, returning 404 if the request does not exist. [code+test]
- FR-016: `FamilyLinkController` shall let either party cancel a link on `POST api/family-links/{requestId}/cancel`, returning 404 if the request does not exist. [code+test]
- FR-017: `FamilyLinkController` shall list the caller's own outgoing requests on `GET api/family-links/sent` and incoming requests still in `Requested` status on `GET api/family-links/received`. [code]
- FR-018: `FamilyLinkController` shall return a member's linked family on `GET api/family-links/my-family` (aliased at `GET api/members/family` and `GET api/Family/links`) for the caller's own record, and on `GET api/family-links/{memberId}/family` for any member, gated by that member's `IsFamilyPublic` flag when the caller is not the same member. [code]
- FR-019: `FamilyLinkController` shall require a non-blank `name` query parameter on `GET api/family-links/search` (aliased at `GET api/Family/search`), returning 400 for a blank value and otherwise the result of `FamilyService.SearchByNameAsync`. [code]
- FR-020: `SecureFilesController` shall serve a file on `GET api/secure-files/{*filePath}` only to the file's owning member or a caller in the Admin or SuperAdmin role, returning 403 for any other authenticated caller and 404 when no `FileUpload` row matches the path. [code+test]
- FR-021: `SecureFilesController` shall reject any `filePath` containing `..` with a 404 before any file-system access. [code+test]
- FR-022: `SecureFilesController` shall resolve `filePath` only against the public uploads root and the secure uploads root computed from `FileStorage:BasePhysicalPath`, returning 404 when the resolved path falls outside both roots or does not exist on disk. [code]

## Key Entities

- **Member** (subset relevant here): `NID`, `MobileNo`, `Email`, `PresentAddress`, `PermanentAddress`, `IsNIDPublic`/`IsMobilePublic`/`IsEmailPublic`/`IsAddressPublic`/`IsFamilyPublic`, `PhotoPath`, `SignaturePath`, notification flags (`NotifyEventCreation`, `NotifyParticipationApproval`, `NotifyRegistrationUpdate`, `NotifyRelevantUpdates`, `NotifyCommitteeChanges`).
- **FamilyLinkRequest**: requester member id, target member id, `Relationship` (`RelationshipType` enum), `Note`, `Status` (Requested/Accepted/Rejected/Cancelled/Removed).
- **FileUpload**: `MemberId`, `FileName`, `FilePath`, `UploadType`, `SizeBytes`: the row `SecureFilesController` looks up before serving a file.
- **IssuedCredential**: `ShortCode` (10 characters), owning member, `IsRevoked`, `RevokedReason`, `RevokedOn`, `ExpiresOn` (never observed set in this pass).

## Evidence

| FR | Route | Service | Angular file | Flutter file | Test |
|---|---|---|---|---|---|
| FR-001 | GET api/profile | `MemberService.GetProfileAsync` (MemberService_Profile.cs) | GHCAA.Web/src/app/core/services/profile.service.ts:53 | GHCAA.Mobile/lib/screens/member/profile_screen.dart | GHCAA.Tests/Controllers/ProfileControllerTests.cs:58 |
| FR-002 | PUT api/profile | `MemberService.UpdateProfileAsync` (MemberService_Profile.cs) | GHCAA.Web/src/app/core/services/profile.service.ts:56 | GHCAA.Mobile/lib/screens/member/profile_edit_screen.dart | GHCAA.Tests/Controllers/ProfileControllerTests.cs:70 |
| FR-003 | POST api/profile/change-password | `UserService.ChangePasswordAsync` (UserService.cs:110) | GHCAA.Web/src/app/member/change-password/change-password.ts:52 | none found | GHCAA.Tests/Controllers/ProfileControllerTests.cs:82 |
| FR-004 | GET api/profile/id-card | `IDCardService.GenerateIDCardDataUriAsync` (IDCardService.cs) | GHCAA.Web/src/app/member/digital-id/digital-id.ts:43 | GHCAA.Mobile/lib/screens/member/digital_id_screen.dart | GHCAA.Tests/Controllers/ProfileControllerTests.cs:100 |
| FR-005 | GET api/profile/id-card/pdf | `IDCardService.GenerateIDCardPdfAsync` (IDCardService.cs) | none found | none found | none found |
| FR-006 | GET api/profile/certificate | `IDCardService.GenerateCertificateDataUriAsync` (IDCardService.cs) | GHCAA.Web/src/app/member/digital-id/digital-id.ts:60 | none found | GHCAA.Tests/Controllers/ProfileControllerTests.cs:111 |
| FR-007 | GET api/profile/certificate/pdf | `IDCardService.GenerateCertificatePdfAsync` (IDCardService.cs) | none found | none found | none found |
| FR-008 | POST api/profile/photo | `FileValidationService.Validate` + `MemberService.UpdateMemberPhotoAsync` | GHCAA.Web/src/app/member/profile/profile.ts:283 | GHCAA.Mobile/lib/screens/member/profile_edit_screen.dart | none found |
| FR-009 | POST api/profile/signature | `FileValidationService.Validate` + `MemberService.UpdateMemberSignatureAsync` | GHCAA.Web/src/app/member/profile/profile.ts:304 | none found | none found |
| FR-010 | GET api/profile/id-card, /certificate, /id-card/pdf, /certificate/pdf | `IDCardService.IssueCredentialAsync` (private, IDCardService.cs) | n/a | n/a | none found |
| FR-011 | GET api/verify/{shortCode} | `IDCardService.VerifyCredentialAsync` (IDCardService.cs) | GHCAA.Web/src/app/public/verify/verify.ts | GHCAA.Mobile/lib/features/credentials/credential_verification_service.dart | GHCAA.Tests/Controllers/CredentialVerificationControllerTests.cs:16,36 |
| FR-012 | POST api/verify/{shortCode}/revoke | `IDCardService.RevokeCredentialAsync` (IDCardService.cs) | none found | none found | none found |
| FR-013 | POST api/family-links/send | `FamilyLinkService.SendRequestAsync` (FamilyLinkService.cs) | GHCAA.Web/src/app/core/services/family-link.service.ts:19 | GHCAA.Mobile/lib/features/networking/family_service.dart:44 | GHCAA.Tests/Controllers/FamilyLinkControllerTests.cs:78,91 |
| FR-014 | POST api/family-links/respond | `FamilyLinkService.RespondAsync` (FamilyLinkService.cs) | GHCAA.Web/src/app/core/services/family-link.service.ts:23 | GHCAA.Mobile/lib/features/networking/family_service.dart:58 | GHCAA.Tests/Controllers/FamilyLinkControllerTests.cs:104,115 |
| FR-015 | DELETE api/family-links/remove/{requestId} | `FamilyLinkService.RemoveLinkAsync` (FamilyLinkService.cs) | GHCAA.Web/src/app/core/services/family-link.service.ts:31 | GHCAA.Mobile/lib/features/networking/family_service.dart:81 | GHCAA.Tests/Controllers/FamilyLinkControllerTests.cs:38,48 |
| FR-016 | POST api/family-links/{requestId}/cancel | `FamilyLinkService.CancelAsync` (FamilyLinkService.cs) | GHCAA.Web/src/app/core/services/family-link.service.ts:27 | GHCAA.Mobile/lib/features/networking/family_service.dart:71 | GHCAA.Tests/Controllers/FamilyLinkControllerTests.cs:58,68 |
| FR-017 | GET api/family-links/sent, /received | `FamilyLinkService.GetSentRequestsAsync`/`GetReceivedRequestsAsync` (FamilyLinkService.cs) | GHCAA.Web/src/app/core/services/family-link.service.ts:11,15 | GHCAA.Mobile/lib/features/networking/family_service.dart:24,34 | none found |
| FR-018 | GET api/family-links/my-family, /{memberId}/family | `FamilyLinkService.GetFamilyAsync` (FamilyLinkService.cs) | GHCAA.Web/src/app/core/services/family-link.service.ts:35 | GHCAA.Mobile/lib/features/networking/family_service.dart:14 | none found |
| FR-019 | GET api/family-links/search | `FamilyService.SearchByNameAsync` (FamilyService.cs) | GHCAA.Web/src/app/core/services/family-link.service.ts:39 | GHCAA.Mobile/lib/features/networking/family_service.dart:91 | none found |
| FR-020 | GET api/secure-files/{*filePath} | `FileUploadRepository.GetByFilePathAsync` (FileUploadRepository.cs:22) | none found | none found | GHCAA.Tests/Controllers/SecureFilesControllerTests.cs:63,75,104 |
| FR-021 | GET api/secure-files/{*filePath} | `SecureFilesController.GetSecureFile` (path-traversal check) | none found | none found | GHCAA.Tests/Controllers/SecureFilesControllerTests.cs:92 |
| FR-022 | GET api/secure-files/{*filePath} | `SecureFilesController.IsInside` (local static helper) | none found | none found | none found |

## Gaps

- Angular calls only `getIDCard` and `getCertificate` (data URI); no Angular file calls the PDF routes (`id-card/pdf`, `certificate/pdf`). `GHCAA.Mobile/lib/screens/member/digital_id_screen.dart:278-346` builds its own PDF client-side with the `pdf` package instead of calling either backend PDF route, so the two PDF endpoints (FR-005, FR-007) have no confirmed caller in either client. [NEEDS CLARIFICATION: are the backend PDF routes still needed, or is mobile's client-side PDF the intended path and the two GET .../pdf routes dead code?]
- `IDCardService.VerifyCredentialAsync`'s `ExpiresOn` check has no writer anywhere in `IDCardService.cs`; a credential's only path to becoming invalid is explicit admin revocation, not time expiry, even though the DTO carries an expiry field (IDCardService.cs, `VerifyCredentialAsync`).
- `FamilyService.cs` implements a second, parallel family-link workflow (`SendRequestAsync`, `RespondAsync`, `CancelRequestAsync`, `GetRequestsAsync`, `GetLinkedMembersAsync`, `UnlinkAsync`) using a different DTO (`CreateFamilyRequestDto`). Only `FamilyService.SearchByNameAsync` is called from `FamilyLinkController`; no other controller call site was found for the rest of that class in this pass. See ENH-001.
- `GET api/family-links/{memberId}/family` has no route-level policy. `FamilyLinkService.GetFamilyAsync` (FamilyLinkService.cs:173-178) returns an empty list when the target's `IsFamilyPublic` is off, which looks the same as a member with no links, so the flag itself does not leak. The same check also blocks admins: the comment at line 176 says the admin override was never added. [NEEDS CLARIFICATION: should an admin see a private family?]
- No client (Angular or Flutter) was found calling `POST api/verify/{shortCode}/revoke`; the only consumer of admin revocation traced in this pass is the controller/test pair itself. [NEEDS CLARIFICATION: is credential revocation performed from an admin screen not covered by this pass, or only via direct API access today?]
- No test exercises `POST api/profile/photo`, `POST api/profile/signature`, `GET api/profile/id-card/pdf`, `GET api/profile/certificate/pdf`, or `POST api/verify/{shortCode}/revoke`; these five endpoints are `[code]`-only in the Evidence table above.
- `GHCAA.Mobile` has no signature-upload call site (`GHCAA.Mobile/lib` has no file referencing "signature"); mobile members cannot set a signature through the app even though the backend accepts one (FR-009). This is a missing capability, not a layout difference.
- `GHCAA.Mobile` has no change-password call site tied to `POST api/profile/change-password`; the mobile `forgot_password_screen.dart` found in this pass is a separate reset flow, not the in-session change-password endpoint covered by FR-003. [NEEDS CLARIFICATION: does mobile intentionally omit in-app password change in favour of forgot-password, or is this an unbuilt screen?]

## Enhancements: modularisation and reusability

### Reuse across layers

- ENH-001 (P2, real duplication): `FamilyService.cs`'s `SendRequestAsync`/`RespondAsync`/`CancelRequestAsync`/`GetRequestsAsync`/`GetLinkedMembersAsync`/`UnlinkAsync` duplicate `FamilyLinkService.cs`'s job under a different DTO shape, with only `SearchByNameAsync` actually wired into `FamilyLinkController`. Either remove the unused methods or fold `SearchByNameAsync` into `FamilyLinkService` and retire `IFamilyService`'s overlapping surface. `GHCAA.Infrastructure/Services/FamilyService.cs`.
- ENH-002 (P2, real duplication): `GHCAA.Mobile/lib/screens/member/digital_id_screen.dart:278-346` re-implements ID-card PDF layout client-side with the `pdf` package, duplicating what `IDCardService.GenerateIDCardPdfAsync` already produces server-side. A single source of truth for the printed layout would avoid the two drifting apart.

### Entity-based module shape

- The four controllers in this spec sit across three concerns (member self-service, secure-file access control, public credential verification) that could be split into an `Identity` module (profile, id-card, credential verification) and keep family links under the existing networking/member module, mirroring the events/campaigns/mentorship/gallery entity-module pattern already used elsewhere. This is a shape observation, not a defect; no duplication was found to justify moving code now.

### Existing reusable components

- Web: `GHCAA.Web/src/app/member/requests/requests.ts` (the family-link consumer) and `GHCAA.Web/src/app/member/profile/profile.ts` both inject `ConfirmDialogService` from `GHCAA.Web/src/app/core/services/confirm-dialog.service.ts`, reusing the shared `common/confirm-dialog` component correctly.
- Mobile: `GHCAA.Mobile/lib/screens/member/family_link_screen.dart:11,496,515` reuses `core/widgets/confirm_dialog.dart` for cancel/remove confirmations, matching the pattern used by `dashboard_screen.dart`, `gallery_screen.dart`, and other member screens.
- ENH-003 (P3, tidy-up): `GHCAA.Mobile/lib/screens/member/profile_edit_screen.dart`'s photo picker builds its own inline image widget rather than reusing `core/widgets/upload_surface.dart`, the widget `financial_portal_screen.dart:340` and `submit_article_screen.dart:185` already use for file uploads. Reusing it would keep the upload affordance consistent across the app.

### Hard-coded behaviour that should be configuration

- ENH-004 (P1, security risk): the 5 MB photo limit and 2 MB signature limit are literal `5*1024*1024`/`2*1024*1024` arguments passed at the `ProfileController.UploadPhoto`/`UploadSignature` call sites (`GHCAA.API/Controllers/ProfileController.cs`), not sourced from a named constant or org-configurable setting. A value this security-relevant (denial-of-service via oversized uploads) is worth centralizing so it can be tuned without a code change.
- ENH-005 (P3, tidy-up): the content-type-to-extension map in `SecureFilesController.GetSecureFile` (`.pdf`, `.jpg`/`.jpeg`, `.png`, else `application/octet-stream`) is a local literal switch rather than reuse of `FileValidationService`'s existing extension/content-type tables (`GHCAA.Infrastructure/Services/FileValidationService.cs`), which already enumerate the same four cases plus `.webp`. A shared lookup would keep the two in sync; `.webp` files currently fall through to `application/octet-stream` on download even though upload accepts them.

## Success Criteria

- SC-001: A member can read and update their own profile end to end (FR-001, FR-002), confirmed by `ProfileControllerTests.cs:58,70`.
- SC-002: A password change cannot succeed without the correct old password, and a successful change invalidates prior sessions (FR-003), confirmed by `ProfileControllerTests.cs:82` and the `SecurityStamp`/refresh-token-revocation code path in `UserService.cs:110-128`.
- SC-003: A member's uploaded document cannot be fetched by another non-admin member, and a path-traversal attempt is rejected before disk access (FR-020, FR-021), confirmed by `SecureFilesControllerTests.cs:63,92`.
- SC-004: An anonymous visitor can verify a credential by short code without authentication, and a malformed code never reaches the verification service (FR-011), confirmed by `CredentialVerificationControllerTests.cs:16,36`.
- SC-005: A family-link request cannot be created against an unknown member, cannot self-target, and cannot duplicate an existing pending request (FR-013), confirmed by `FamilyLinkControllerTests.cs:78,91` and the guard logic in `FamilyLinkService.cs`.

## Assumptions

- "Code wins over docs": where the platform-baseline feature catalog's "Family links" section already names the legacy alias routes as "concrete implementation surface," this spec treats those aliases the same way rather than re-deriving them from a fresh route scan.
- The evidence table's Angular/Flutter/test cells reflect only what a grep or read in this pass actually surfaced; a cell marked "none found" means no matching file was located during this trace, not that no such file exists anywhere in the repo.
- `BaseMemberDto`'s DataAnnotations (`[Required]`, `[MaxLength]`, the Bangladeshi mobile-number regex, `AcademicHistory` requiring at least one entry) are assumed to be the full validation surface for `UpdateProfileDto`, since no FluentValidation validator class was found for it in `GHCAA.Application/Validators`.
- Dates in this spec are shown `dd-MM-yyyy`; token TTLs and other durations are stated in their native unit (minutes/days) as found in code.
