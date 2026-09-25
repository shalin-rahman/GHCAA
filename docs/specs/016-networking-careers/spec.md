# Feature Specification: Networking and Careers

**Feature Branch**: `016-networking-careers`

**Created**: 25-09-2026

**Status**: As-built baseline

**Input**: Reverse-engineered from the implemented code.

**Depends on**: spec 012, the membership lifecycle and auth workflow, for the member
identity, privacy-flag and role fields this domain reads and displays.

## Purpose and scope

Covers the alumni directory, job board and mentorship request workflow:

- `NetworkingController` (`api/networking`): public alumni directory search, public member
  profile, executive committee roster and periods, latest alumni updates.
- `JobHubController` (`api/jobs`): job posting, editing, deactivation, admin approval queue.
- `MentorshipController` (`api/mentorship`): mentorship request, accept/decline, completion,
  admin oversight list.

Out of scope:

- Spec 012, the membership lifecycle and auth workflow, covers member registration, roles,
  claims and the authenticated `MemberService.GetProfileAsync` full-profile view.
- Spec 014, events and content, covers alumni events separately from job postings.
- `PendingApprovalsController` is out of scope; it consumes `IJobHubService.GetMemberJobsAsync`
  but is a different controller (see Gaps).

## User Scenarios & Testing

### User Story 1 - Public directory search (Priority: P1)

A visitor or member searches the alumni directory without needing to sign in, and results
mask any field the profile owner has not marked public.

**Why this priority**: The directory is the primary public-facing feature of this domain;
every other screen (committee, mentorship, jobs) links back into a member's directory entry.

**Independent Test**: Call `GET api/networking/search` with a text query and confirm the
response contains masked `Email`/`MobileNo`/`PresentAddress`/`PermanentAddress` fields for
members who have not opted in, and real values for members who have.

**Acceptance Scenarios**:

1. **Given** a member has `IsEmailPublic = false`, **When** a caller searches by the member's
   real email address, **Then** the member does not appear in the results (the query filters
   on `IsEmailPublic`, not just the display mask).
2. **Given** a member has `IsEmailPublic = true`, **When** a caller searches by that email,
   **Then** the member appears and the response shows the real address.
3. **Given** a member is `IsArchived` or not `MembershipStatus.Active`, **When** any search is
   run, **Then** the member is excluded from results.

---

### User Story 2 - Members post and manage job listings (Priority: P1)

A member posts a job opening. A non-admin poster's job enters `Pending` status and waits for
admin approval; an admin's own posting is auto-approved.

**Why this priority**: Job posting is the domain's main write path and the one with an
approval workflow, ownership checks and notification branching worth protecting.

**Independent Test**: Post a job as a non-admin member, confirm it is `Pending` and does not
appear in `GET api/jobs` (active jobs), then approve it as an admin and confirm it does.

**Acceptance Scenarios**:

1. **Given** a non-admin member posts a job, **When** the post completes, **Then** `Status` is
   `Pending` and `IsActive` stays `true`, but the job is excluded from `GetActiveJobsAsync`
   because that query also requires `Status == Approved`.
2. **Given** an admin posts a job, **When** the post completes, **Then** `Status` is
   `Approved` immediately, and the poster is notified only if `CreateJobDto.NotifyMembers` is
   `true`.
3. **Given** a member who did not post a job, **When** that member calls `PUT api/jobs/{id}`
   or `DELETE api/jobs/{id}`, **Then** the controller returns 403 Forbid unless the caller is
   an admin.
4. **Given** an admin rejects a job with a reason, **When** the rejection completes, **Then**
   `Status` becomes `Rejected`, `RejectionReason` is set, and `IsActive` is also set to
   `false`.

---

### User Story 3 - Members request and manage mentorship (Priority: P2)

A member sends a mentorship request to another member. The target member accepts or declines;
either party can later mark an accepted mentorship complete.

**Why this priority**: Mentorship is a smaller-volume workflow than search or jobs, but it is
the one entity-shaped request/response/notify cycle in this domain worth comparing against the
brief's other named shared-pattern candidates.

**Independent Test**: Send a request from member A to member B, confirm a second request from
A to B while the first is still `Pending` returns 409, then have B respond and confirm the
status transition.

**Acceptance Scenarios**:

1. **Given** member A sends a request to member A (self), **When** `SendRequest` runs,
   **Then** the controller returns 400 with the message "You cannot send a mentorship request
   to yourself."
2. **Given** a `Pending` request already exists for the same requester/mentor pair, **When** a
   second request is sent, **Then** `SendRequestAsync` throws `InvalidOperationException` and
   the controller returns 409 Conflict.
3. **Given** member B is not the mentor on request X, **When** B calls
   `POST api/mentorship/{X}/respond`, **Then** the query `r.MentorId == mentorId` finds no
   match and the controller returns 404 (the same code as "request does not exist").
4. **Given** a request is `Accepted`, **When** either the requester or the mentor calls
   `POST api/mentorship/{id}/complete`, **Then** the status becomes `Completed`.

---

### Edge Cases

- `NetworkingController` carries a class-level `[Authorize]` but every action is individually
  marked `[AllowAnonymous]` (`GHCAA.API/Controllers/NetworkingController.cs:15-20`), so the
  whole directory, profile, committee and updates surface is public despite the class
  attribute suggesting otherwise.
- A missing or corrupt search cursor does not error; `TryDecodeCursor` falls back to the first
  page (`GHCAA.Infrastructure/Services/NetworkingService.cs`, cursor-decode block).
- `DeactivateJob` is mapped to both `[HttpDelete("{id}")]` and `[HttpPatch("deactivate/{id}")]`
  (`GHCAA.API/Controllers/JobHubController.cs`), so a DELETE call never removes a job row, it
  only flips `IsActive` to `false`.
- `RespondAsync`'s authorization check is baked into the lookup query (`MentorId == mentorId`),
  so "request not found" and "you are not the mentor" both return the same 404 with no way for
  a caller to distinguish them.
- `RejectJobAsync` sets `IsActive = false` in addition to `Status = Rejected`, so a rejected job
  is both non-approved and inactive at once, unlike a merely deactivated job which keeps
  whatever status it had.

## Requirements

### Functional Requirements

- FR-001: `NetworkingController.Search` shall accept `GET api/networking/search` (also mapped
  as `api/networking/directory`) with a `MemberSearchFilterDto` and return a paged result of
  `MemberSummaryDto` via `INetworkingService.SearchMembersAsync`. [code+test]
- FR-002: `NetworkingController.GetPublicProfile` shall accept `GET api/networking/member/{id}`
  and return 404 when `GetMemberProfileAsync` finds no active, non-archived member with that
  id. [code+test]
- FR-003: `NetworkingController.GetExecutiveCommittee` shall accept `GET api/networking/committee`
  with an optional `periodId` query parameter and return the members holding an EC role in that
  period, or the active period when `periodId` is omitted. [code+test]
- FR-004: `NetworkingController.GetPeriods` shall accept `GET api/networking/periods` and return
  every EC period's `Id`, `Title`, `IsActive`, `StartDate` and `EndDate`. [code+test]
- FR-005: `NetworkingController.GetLatestUpdates` shall accept `GET api/networking/updates` with
  a `count` query parameter defaulting to 10, and return the most recently updated members
  ordered by `LastUpdateDate` descending. [code+test]
- FR-006: `NetworkingService.SearchMembersAsync` shall filter email matches on
  `m.IsEmailPublic == true` before comparing the search text, so a non-public email cannot be
  confirmed to exist through a non-empty search result. [code+test]
- FR-007: `NetworkingService.MapToDto` shall omit identity-verification fields (NID, date of
  birth, parents' names, emergency contact, certificate file paths) from the public profile
  response regardless of any privacy flag, and shall mask `Email`, `MobileNo`,
  `PresentAddress` and `PermanentAddress` as "Confidential" unless the matching `IsXPublic`
  flag is true. [code]
- FR-008: `NetworkingService.SearchMembersAsync` shall paginate by keyset cursor, fetching
  `PageSize + 1` rows to compute `HasNextPage`/`NextCursor` without relying on `TotalItems`,
  and shall fall back to the first page when the supplied cursor is missing or fails to
  decode. [code+test]
- FR-009: `NetworkingService.SearchMembersAsync` shall exclude members whose `Status` is not
  `Active` or whose `IsArchived` is `true`. [code+test]
- FR-010: `NetworkingService.GetExecutiveCommitteeAsync` shall default to the `ECPeriod` with
  `IsActive == true` when no `periodId` is supplied, and shall only include EC members whose
  `EndDate` is null (current role holders). [code+test]
- FR-011: `NetworkingService.MapToSummary` shall populate the flattened `PassingYear`, `Degree`
  and `Subject` directory fields for each returned member summary. [code+test]
- FR-012: `NetworkingService.MapToDto` shall include a member's family links only when the
  owning member's `IsFamilyPublic` is true and the linked member's own `IsFamilyPublic` is also
  true (a mutual privacy gate on both sides of the link). [code]
- FR-013: `JobHubController.GetActiveJobs` shall accept `GET api/jobs` as `[AllowAnonymous]`
  with the `Constants.OutputCachePolicies.PublicContent` output-cache policy, filtered by an
  optional `Enums.JobCategory` and text query. [code+test]
- FR-014: `JobHubController.PostJob` shall accept `POST api/jobs`, return 400 Problem when the
  caller has no valid member-id claim, and derive `isAdmin` from `User.IsInRole("Admin")` or
  `User.IsInRole("SuperAdmin")`. [code+test]
- FR-015: `JobHubService.PostJobAsync` shall set `Status = Pending` for a non-admin poster and
  `Status = Approved` for an admin poster. [code+test]
- FR-016: `JobHubService.PostJobAsync` shall notify the poster only when the posting
  auto-approves and `CreateJobDto.NotifyMembers` is true, and shall always notify admins via
  `IAdminNotificationService.NotifyPendingApprovalAsync` when the posting is `Pending`,
  regardless of `NotifyMembers`. [code+test]
- FR-017: `JobHubController.UpdateJob` shall accept `PUT api/jobs/{id}` and return 403 Forbid
  when `UpdateJobAsync` reports the caller is neither the job's `PostedByMemberId` nor an
  admin. [code+test]
- FR-018: `JobHubController.GetJob` shall accept `GET api/jobs/{id}` as `[AllowAnonymous]` with
  the `PublicContent` output-cache policy, returning 404 when no job matches. [code+test]
- FR-019: `JobHubController.DeactivateJob` shall accept both `DELETE api/jobs/{id}` and
  `PATCH api/jobs/deactivate/{id}`, set `IsActive = false` (the row is not removed), and return
  403 Forbid when the caller's member id does not match `PostedByMemberId` and the caller is
  not an admin. [code+test]
- FR-020: `JobHubController.GetPendingJobs` shall accept `GET api/jobs/admin/pending` under
  `[Authorize(Policy = Constants.Policies.AdminOnly)]` and return only jobs with
  `Status == Pending`. [code+test]
- FR-021: `JobHubController.ApproveJob` shall accept `POST api/jobs/admin/{id}/approve` under
  `AdminOnly`, set `Status = Approved`, clear `RejectionReason`, and notify the poster only
  when the `notifyMember` query parameter (default `true`) is true. [code+test]
- FR-022: `JobHubController.RejectJob` shall accept `POST api/jobs/admin/{id}/reject` under
  `AdminOnly` with a `RejectJobRequest { Reason, NotifyMember = true }` body, set
  `Status = Rejected`, set `RejectionReason`, set `IsActive = false`, and notify the poster
  with the reason only when `NotifyMember` is true. [code+test]
- FR-023: `MentorshipController.SendRequest` shall accept `POST api/mentorship`, return 401
  when the caller has no member-id claim, return 400 when `MentorId` equals the caller's own
  id, and return 409 when `SendRequestAsync` throws `InvalidOperationException`. [code]
- FR-024: `MentorshipService.SendRequestAsync` shall create a request with `Status = Pending`
  and shall throw `InvalidOperationException` when a `Pending` request already exists for the
  same requester/mentor pair. [code+test]
- FR-025: `MentorshipController.GetSent` and `GetReceived` shall accept
  `GET api/mentorship/sent` and `GET api/mentorship/received`, return 401 without a member-id
  claim, and otherwise return the caller's sent or received requests. [code]
- FR-026: `MentorshipService.RespondAsync` shall update `Status` to `Accepted` or `Declined`
  only when the row matches both the request id and `MentorId == mentorId`, returning `false`
  (mapped to 404 by the controller) otherwise. [code+test]
- FR-027: `MentorshipService.MarkCompleteAsync` shall set `Status = Completed` when the caller
  is either the `RequesterId` or the `MentorId` on a request whose current `Status` is
  `Accepted`. [code+test]
- FR-028: `MentorshipController.GetAllForAdmin` shall accept `GET api/mentorship/admin/all`
  under `[Authorize(Policy = Constants.Policies.AdminOnly)]` and return every mentorship
  request. [code]

### Key Entities

- **Member (directory projection)**: the existing `Member` entity from spec 012, the
  membership lifecycle and auth workflow, projected here into `MemberSummaryDto` (search
  results, EC roster) and `MemberProfileDto` (single public profile), both privacy-masked by
  the `IsEmailPublic`/`IsMobilePublic`/`IsAddressPublic`/`IsFamilyPublic` flags.
- **ECPeriod / EC membership**: an executive-committee term (`Title`, `IsActive`, `StartDate`,
  `EndDate`) and the members holding a role within it; a member's EC role ends when their
  `EndDate` is set.
- **JobOpportunity**: `Title`, `Company`, `Location`, `Description`, `Requirements`,
  `ContactEmail`, `ApplicationLink`, `PostedByMemberId`, `PostedDate`, `ExpiryDate`,
  `IsActive`, `JobCategory`, `Status` (`Enums.SubmissionStatus`: Pending, Approved, Rejected),
  `RejectionReason`.
- **MentorshipRequest**: `RequesterId`, `MentorId`, `Message` (max 500 chars), `Domain` (max
  200 chars, the mentee's area of interest), `Status` (`MentorshipStatus`: Pending, Accepted,
  Declined, Completed), `ResponseNote` (max 500 chars), `RequestedAt`, `RespondedAt`.

## Evidence

| FR | API (verb + route) | Service method | Web (file) | Mobile (file) | Test (file::test name) |
|----|---------------------|-----------------|------------|----------------|--------------------------|
| FR-001 | GET api/networking/search | NetworkingService.SearchMembersAsync | GHCAA.Web/src/app/core/services/networking.service.ts | GHCAA.Mobile/lib/features/networking/networking_service.dart | GHCAA.Tests/Controllers/NetworkingControllerTests.cs::Search_ReturnsOk |
| FR-002 | GET api/networking/member/{id} | NetworkingService.GetMemberProfileAsync | GHCAA.Web/src/app/core/services/networking.service.ts | none found | GHCAA.Tests/Controllers/NetworkingControllerTests.cs::GetPublicProfile_ReturnsOk_IfFound |
| FR-003 | GET api/networking/committee | NetworkingService.GetExecutiveCommitteeAsync | GHCAA.Web/src/app/core/services/networking.service.ts | GHCAA.Mobile/lib/screens/member/committee_screen.dart | GHCAA.Tests/Controllers/NetworkingControllerTests.cs::GetCommittee_ReturnsOk |
| FR-004 | GET api/networking/periods | NetworkingService.GetECPeriodsAsync | GHCAA.Web/src/app/core/services/networking.service.ts | GHCAA.Mobile/lib/screens/member/committee_screen.dart | GHCAA.Tests/Controllers/NetworkingControllerTests.cs::GetPeriods_ReturnsOk |
| FR-005 | GET api/networking/updates | NetworkingService.GetLatestAlumniUpdatesAsync | GHCAA.Web/src/app/core/services/networking.service.ts | none found | GHCAA.Tests/Controllers/NetworkingControllerTests.cs::GetLatestUpdates_ReturnsOk |
| FR-006 | GET api/networking/search | NetworkingService.SearchMembersAsync | GHCAA.Web/src/app/core/services/networking.service.ts | GHCAA.Mobile/lib/features/networking/networking_service.dart | GHCAA.Tests/Services/NetworkingServiceTests.cs::SearchMembersAsync_ShouldHonorPrivacyFlags |
| FR-007 | GET api/networking/member/{id} | NetworkingService.MapToDto | none found | none found | none found |
| FR-008 | GET api/networking/search | NetworkingService.SearchMembersAsync | GHCAA.Web/src/app/core/services/networking.service.ts | GHCAA.Mobile/lib/features/networking/networking_service.dart | GHCAA.Tests/Services/NetworkingServiceTests.cs::SearchMembersAsync_CursorPagination_ShouldPageThroughAllMembersOnce |
| FR-009 | GET api/networking/search | NetworkingService.SearchMembersAsync | GHCAA.Web/src/app/core/services/networking.service.ts | GHCAA.Mobile/lib/features/networking/networking_service.dart | GHCAA.Tests/Services/NetworkingServiceTests.cs::SearchMembersAsync_ShouldHideInactiveAndArchivedMembers |
| FR-010 | GET api/networking/committee | NetworkingService.GetExecutiveCommitteeAsync | GHCAA.Web/src/app/core/services/networking.service.ts | GHCAA.Mobile/lib/screens/member/committee_screen.dart | GHCAA.Tests/Services/NetworkingServiceTests.cs::GetExecutiveCommitteeAsync_ShouldReturnMembersWithECPosition |
| FR-011 | GET api/networking/committee | NetworkingService.GetExecutiveCommitteeAsync | GHCAA.Web/src/app/core/services/networking.service.ts | GHCAA.Mobile/lib/screens/member/committee_screen.dart | GHCAA.Tests/Services/NetworkingServiceTests.cs::GetExecutiveCommitteeAsync_ShouldPopulateBatchInformation |
| FR-012 | GET api/networking/member/{id} | NetworkingService.MapToDto | none found | none found | none found |
| FR-013 | GET api/jobs | JobHubService.GetActiveJobsAsync | GHCAA.Web/src/app/core/services/job.service.ts | GHCAA.Mobile/lib/features/jobs/job_service.dart | GHCAA.Tests/Services/JobHubServiceTests.cs::GetActiveJobsAsync_ShouldReturnOnlyActiveAndUnexpiredJobs |
| FR-014 | POST api/jobs | JobHubService.PostJobAsync | GHCAA.Web/src/app/core/services/job.service.ts | GHCAA.Mobile/lib/features/jobs/job_service.dart | GHCAA.Tests/Services/JobHubServiceTests.cs::PostJobAsync_ShouldAddJobAndReturnDto |
| FR-015 | POST api/jobs | JobHubService.PostJobAsync | GHCAA.Web/src/app/core/services/job.service.ts | GHCAA.Mobile/lib/features/jobs/job_service.dart | GHCAA.Tests/Services/JobHubServiceTests.cs::PostJobAsync_NonAdmin_SetsStatusPending_AndNotifiesAdmins |
| FR-016 | POST api/jobs | JobHubService.PostJobAsync | GHCAA.Web/src/app/core/services/job.service.ts | GHCAA.Mobile/lib/features/jobs/job_service.dart | GHCAA.Tests/Services/JobHubServiceTests.cs::PostJobAsync_Admin_SkipsNotification_WhenDtoOptsOut |
| FR-017 | PUT api/jobs/{id} | JobHubService.UpdateJobAsync | GHCAA.Web/src/app/core/services/job.service.ts | GHCAA.Mobile/lib/screens/member/job_details_screen.dart (raw dio call, see ENH-001) | GHCAA.Tests/Services/JobHubServiceTests.cs::UpdateJobAsync_ReturnsFalse_WhenNeitherPosterNorAdmin |
| FR-018 | GET api/jobs/{id} | JobHubService.GetJobByIdAsync | GHCAA.Web/src/app/core/services/job.service.ts | GHCAA.Mobile/lib/screens/member/job_details_screen.dart (raw dio call, see ENH-001) | GHCAA.Tests/Controllers/JobHubControllerTests.cs::GetJob_ReturnsOk_IfFound |
| FR-019 | DELETE api/jobs/{id}, PATCH api/jobs/deactivate/{id} | JobHubService.DeactivateJobAsync | GHCAA.Web/src/app/core/services/job.service.ts | GHCAA.Mobile/lib/features/jobs/job_service.dart | GHCAA.Tests/Services/JobHubServiceTests.cs::DeactivateJobAsync_ShouldSetIsActiveToFalse |
| FR-020 | GET api/jobs/admin/pending | JobHubService.GetPendingJobsAsync | GHCAA.Web/src/app/admin/job-approval/job-approval.ts | GHCAA.Mobile/lib/screens/admin/job_approval_screen.dart | GHCAA.Tests/Services/JobHubServiceTests.cs::GetPendingJobsAsync_ReturnsOnlyPendingJobs |
| FR-021 | POST api/jobs/admin/{id}/approve | JobHubService.ApproveJobAsync | GHCAA.Web/src/app/admin/job-approval/job-approval.ts | GHCAA.Mobile/lib/screens/admin/job_approval_screen.dart | GHCAA.Tests/Services/JobHubServiceTests.cs::ApproveJobAsync_SkipsNotification_WhenNotifyMemberIsFalse |
| FR-022 | POST api/jobs/admin/{id}/reject | JobHubService.RejectJobAsync | GHCAA.Web/src/app/admin/job-approval/job-approval.ts | GHCAA.Mobile/lib/screens/admin/job_approval_screen.dart | GHCAA.Tests/Services/JobHubServiceTests.cs::RejectJobAsync_SetsStatusRejected_AndDeactivates |
| FR-023 | POST api/mentorship | MentorshipService.SendRequestAsync | GHCAA.Web/src/app/core/services/mentorship.service.ts | GHCAA.Mobile/lib/screens/member/member_details_screen.dart | none found |
| FR-024 | POST api/mentorship | MentorshipService.SendRequestAsync | GHCAA.Web/src/app/core/services/mentorship.service.ts | GHCAA.Mobile/lib/features/networking/mentorship_service.dart | GHCAA.Tests/Services/MentorshipServiceTests.cs::SendRequestAsync_DuplicatePending_ShouldThrow |
| FR-025 | GET api/mentorship/sent, GET api/mentorship/received | MentorshipService.GetSentRequestsAsync / GetReceivedRequestsAsync | GHCAA.Web/src/app/core/services/mentorship.service.ts | GHCAA.Mobile/lib/screens/member/mentorship_hub_screen.dart | none found |
| FR-026 | POST api/mentorship/{id}/respond | MentorshipService.RespondAsync | GHCAA.Web/src/app/core/services/mentorship.service.ts | GHCAA.Mobile/lib/screens/member/mentorship_hub_screen.dart | GHCAA.Tests/Services/MentorshipServiceTests.cs::RespondAsync_WrongMentor_ShouldReturnFalse |
| FR-027 | POST api/mentorship/{id}/complete | MentorshipService.MarkCompleteAsync | GHCAA.Web/src/app/core/services/mentorship.service.ts | GHCAA.Mobile/lib/screens/member/mentorship_hub_screen.dart | GHCAA.Tests/Services/MentorshipServiceTests.cs::MarkCompleteAsync_ShouldUpdateToCompleted |
| FR-028 | GET api/mentorship/admin/all | MentorshipService.GetAllForAdminAsync | GHCAA.Web/src/app/admin/mentorship/admin-mentorship.ts | none found | none found |

## Gaps

- No `GHCAA.Tests/Controllers/MentorshipControllerTests.cs` exists (confirmed absent; only
  `GHCAA.Tests/Services/MentorshipServiceTests.cs` covers this domain's mentorship code). The
  controller's 401/400/409 branches on `SendRequest`, and the `AdminOnly` gate on
  `GetAllForAdmin`, are untested at the HTTP layer.
- No FluentValidation validator exists for `CreateJobDto` (confirmed by grep across the
  Application project; only the DTO file, `IJobHubService.cs` and `JobHubController.cs`
  reference the type). A job post gets no server-side length or format check beyond ASP.NET
  model binding, unlike registration's `MemberRegistrationValidator`.
  [NEEDS CLARIFICATION: is job-post validation intentionally left to client-side checks only,
  or is a validator missing?]
- `GHCAA.Mobile/lib/screens/member/job_details_screen.dart:150` sends a PUT body of only
  `{title, description}` to `PUT api/jobs/{id}`, while
  `GHCAA.Infrastructure/Services/JobHubService.cs`'s `UpdateJobAsync` overwrites every
  `CreateJobDto` field (`Company`, `Location`, `Requirements`, `ContactEmail`,
  `ApplicationLink`, `JobCategory`, `ExpiryDate`) from the incoming body unconditionally.
  The fields are unlikely to be blanked. `GHCAA.Application` builds with `<Nullable>enable</Nullable>`
  and nothing sets `SuppressImplicitRequiredAttributeForNonNullableReferenceTypes`, so the
  non-nullable `CompanyName`, `Location` and `Requirements` are implicitly required and the
  request should be rejected with 400 by model validation. The expected result is that a
  mobile job edit always fails. This has not been run to confirm.
- `GHCAA.Application/Interfaces/IJobHubService.cs`'s `GetMemberJobsAsync` is not called from
  `JobHubController` (confirmed by grep of non-binary matches); its only caller is
  `PendingApprovalsController.cs:86`, a controller outside this spec's scope. `JobHubController`
  itself has no "my postings" route.
- `INetworkingService.GetECPeriodsAsync` and three `IMentorshipService` methods
  (`GetSentRequestsAsync`, `GetReceivedRequestsAsync`, `GetAllForAdminAsync`) return
  `IEnumerable<object>` rather than a typed DTO. The only documented shape for these responses
  is the Angular-side `MentorshipRequestDto`/`MentorshipAdminRow` interfaces in
  `GHCAA.Web/src/app/core/models/business.models.ts` (lines 791 and 805), which can drift from
  what the backend actually serializes without either side failing a build.
- `GHCAA.Web/src/app/core/services/networking.service.ts`'s `getMemberProfile(id)` builds its
  route by string concatenation off `API_ENDPOINTS.NETWORKING.BASE` instead of a dedicated
  constant like the other `NETWORKING` sub-keys (`COMMITTEE`, `SEARCH`, `UPDATES`).
- `GHCAA.Mobile/lib/features/networking/networking_service.dart` has no wrapper for
  `GET api/networking/member/{id}` (the public profile detail route); no mobile screen read in
  this pass calls it either directly or through the service.
- `GHCAA.Web/src/app/admin/mentorship/admin-mentorship.ts` calls `getAllForAdmin()` with no
  search parameter and filters the full unfiltered list client-side by requester/mentor name
  and domain, the same unpaged-admin-list shape spec 012, the membership lifecycle and auth
  workflow, already documents as a gap for its own admin lists.

## Enhancements: modularisation and reusability

### Reuse across layers

- ENH-001 (P2): `GHCAA.Mobile/lib/screens/member/job_details_screen.dart` calls
  `dio.get('/jobs/$jobId')` (line 16), `dio.put('/jobs/$jobId', ...)` (line 150) and
  `dio.delete('/jobs/$jobId')` (line 183) directly instead of through
  `GHCAA.Mobile/lib/features/jobs/job_service.dart`, which has no `getJobById`/`updateJob`
  method to call. `jobs_screen.dart` and `mentorship_hub_screen.dart` both go through their
  service layer correctly, so this is a real, isolated inconsistency rather than the mobile
  app's general pattern.
- ENH-002 (P2): `INetworkingService.GetECPeriodsAsync`, `IMentorshipService.GetSentRequestsAsync`,
  `GetReceivedRequestsAsync` and `GetAllForAdminAsync` all return `IEnumerable<object>`
  (`GHCAA.Application/Interfaces/INetworkingService.cs`,
  `GHCAA.Application/Interfaces/IMentorshipService.cs`). Giving these four methods typed DTOs
  would remove the risk noted in Gaps, that the Angular models are the only real contract for
  what the endpoints serialize.

### Entity-based module shape

- ENH-003 (P3): `MentorshipRequest`/`MentorshipService`/`MentorshipController` follow a
  request, respond, notify shape (`Status`: Pending, Accepted, Declined, Completed) that is
  close to `JobOpportunity`'s own Pending, Approved, Rejected admin-approval shape in
  `JobHubService`. Both independently implement their own notify-on-transition logic
  (`INotificationService` calls in `MentorshipService.cs` and `JobHubService.cs`). Mentorship
  is named in the brief alongside events, campaigns and gallery as a shared-pattern candidate;
  extracting a common "request/response workflow" shape (status enum, actor ids, notify hook)
  would remove this duplication between Mentorship and JobHub's approval path, not just within
  Mentorship alone. This was not cross-checked against the actual Events or campaigns code in
  this pass, so the comparison is limited to Mentorship against JobHub, both read in full here.

### Existing reusable components

- `GHCAA.Web/src/app/admin/mentorship/admin-mentorship.ts` correctly reuses
  `LoadingPanelComponent`, `PageHeaderComponent` and `SearchBarComponent`.
- `GHCAA.Web/src/app/admin/job-approval/job-approval.ts` correctly reuses
  `PageHeaderComponent`, `SearchBarComponent`, `LoadingPanelComponent`, `NotifyToggleComponent`
  and `ModalHeaderComponent`.
- `GHCAA.Mobile/lib/screens/member/mentorship_hub_screen.dart` correctly reuses
  `app_scaffold.dart`, `glass_container.dart`, `empty_state_widget.dart` and
  `logo_spinner.dart`.
- No missing shared component was found bypassed in this domain; the one confirmed bypass
  (ENH-001) is a raw HTTP call, not a UI-component gap.

### Hard-coded behaviour that should be configuration

- ENH-004 (P3): `NetworkingController.GetLatestUpdates`'s default `count = 10`
  (`GHCAA.API/Controllers/NetworkingController.cs`) is a literal in the action signature rather
  than a named constant, unlike `GHCAA.Web/src/app/common/jobs/jobs.ts`'s job categories, which
  are correctly sourced from `LookupService.getOptions(LOOKUP_GROUPS.JobCategory)` rather than
  hard-coded.

## Success Criteria

### Measurable Outcomes

- SC-001: `GET api/networking/search` never returns a member whose `IsEmailPublic` is false in
  response to a search on that member's real email, verified by
  `NetworkingServiceTests.cs::SearchMembersAsync_ShouldHonorPrivacyFlags` (FR-006).
- SC-002: A non-admin member's job post is `Pending` and absent from `GET api/jobs` until an
  admin approves it, verified by `JobHubServiceTests.cs::PostJobAsync_NonAdmin_SetsStatusPending_AndNotifiesAdmins`
  and `GetActiveJobsAsync_ShouldReturnOnlyActiveAndUnexpiredJobs` (FR-015, FR-013).
- SC-003: A member cannot update or deactivate another member's job posting, verified by
  `JobHubServiceTests.cs::UpdateJobAsync_ReturnsFalse_WhenNeitherPosterNorAdmin` (FR-017).
- SC-004: A duplicate `Pending` mentorship request for the same requester/mentor pair is
  rejected with 409, verified by
  `MentorshipServiceTests.cs::SendRequestAsync_DuplicatePending_ShouldThrow` (FR-024).
- SC-005: An invalid or missing search cursor never returns an error response; it falls back to
  page one, verified by
  `NetworkingServiceTests.cs::SearchMembersAsync_InvalidCursor_ShouldFallBackToFirstPageInsteadOfThrowing`
  (FR-008).

## Assumptions

- Mobile is not expected to mirror every web admin screen; the absence of a mobile
  `GetAllForAdmin` mentorship screen is treated as an established web-only convention, not a
  gap, consistent with other admin-only back-office screens elsewhere in the app.
- "Card and list" mobile layouts for directory, committee and jobs screens are the intended
  mobile design, not an incomplete port of the web table layout.
- Where a test exercises a service method that a controller action calls directly with no
  branching logic of its own (for example `GetCommittee` calling
  `GetExecutiveCommitteeAsync`), the controller-level FR is tagged `[code+test]` on the
  strength of that service test plus the matching controller test found in
  `NetworkingControllerTests.cs`; where no controller test exists (Mentorship), the FR is
  tagged `[code]` even if a service-level test exists for the method it calls.
