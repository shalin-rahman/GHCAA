# Feature Specification: Events, News, Gallery and Site Content

**Feature Branch**: `014-events-news-gallery-content`

**Created**: 25-09-2026

**Status**: As-built (reverse-documented from code)

**Input**: Domain covering `EventsController`, `NewsController`, `GalleryController`, `SiteContentController` and `ArchiveController` in `GHCAA.API/Controllers`.

## Purpose and scope

This spec documents what the code does today for alumni events (with RSVP and check-in), news and notices, the photo gallery, static site content blocks, and the archive of historical collections and items. It does not propose new work; gaps and enhancement candidates are called out separately below.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Member registers for an event and gets checked in (Priority: P1)

A member or guest browses active events, registers (with a receipt if the event charges a fee), waits for admin approval, receives an invitation once approved, and is checked in at the door by QR code.

**Why this priority**: This is the only end-to-end workflow in the domain with a defined state machine (pending, approved, rejected, waitlisted) and a physical-world outcome (check-in), so it carries the most operational risk if broken.

**Independent Test**: Register for an event as a member with `RequiresPayment = true`, upload a receipt, confirm the registration is `Pending`, approve it as admin, confirm `ApprovedAt`/`ApprovedByAdminId` are set and an approval email fires, then call the QR check-in endpoint with the issued `TicketCode` and confirm `IsCheckedIn` flips.

**Acceptance Scenarios**:

1. **Given** an active event with `RequiresRegistration = true` and no participant limit, **When** an authenticated member submits `POST api/events/register` with no guest fields, **Then** the service creates an `EventRegistration` with `Status = Pending`, `TicketCode` set to an 8-character uppercase hex code, and returns it to the caller.
2. **Given** an event with `ParticipantLimit` set and no free slots, **When** a member registers and `HasWaitlist = true`, **Then** the registration is created with `Status = Waitlisted` instead of `Pending`.
3. **Given** a `Pending` registration, **When** an admin calls `POST api/events/admin/approve-registration` with `Approve = true`, **Then** `Status` becomes `Approved`, `ApprovedAt`/`ApprovedByAdminId` are stamped, an `EVENT_PARTICIPATION_APPROVED` email is sent, and an in-app notification is created for the member.
4. **Given** an `Approved` registration owned by the caller, **When** the caller requests `GET api/events/registrations/{id}/invitation`, **Then** the invitation is returned; **when** the registration is not yet `Approved`, the endpoint returns 400 with "Invitation is only available for approved registrations."
5. **Given** a valid `TicketCode`, **When** an admin calls `POST api/events/admin/checkin/qr`, **Then** the matching registration is checked in via `CheckInByTicketCodeAsync`.

---

### User Story 2 - Member submits an article for editorial approval (Priority: P1)

A member drafts a news article or an admin authors a notice; non-admin submissions are always forced into the moderation queue before they go live.

**Why this priority**: This is the main content-integrity control in the domain: without it any authenticated user could publish directly.

**Independent Test**: Submit an article as a non-admin member, confirm `Status` is forced to `Pending` and `IsActive` to `false` regardless of what the request body sent, then approve it as admin and confirm it appears in `GET api/news`.

**Acceptance Scenarios**:

1. **Given** a non-admin member, **When** they call `POST api/news/submit` with `PostType = Notice`, **Then** the request is forbidden (notices are admin-authored only).
2. **Given** a non-admin member, **When** they call `POST api/news/submit` with `PostType = News` and `Status` set to anything other than `Draft` in the payload, **Then** the service ignores that value and stores `Status = Pending`, and forces `IsActive = false`.
3. **Given** a `Pending` article, **When** an admin calls `POST api/news/{id}/approve` (or its alias `admin/{id}/approve`), **Then** `ApproveArticleAsync` updates the status and the article becomes visible through `GET api/news`.

---

### User Story 3 - Member submits a photo to a moderated gallery album (Priority: P2)

A member creates or adds to their own photo album; admins review pending galleries and pending photos separately before either becomes publicly visible.

**Why this priority**: Public-facing media carries reputational risk, so it is gated behind two independent approval queues (album-level and photo-level), which is worth documenting precisely.

**Independent Test**: Submit a photo as a member via `POST api/gallery/member/submit`, confirm the resulting gallery and photo are both inactive/pending, approve the gallery as admin, and confirm `GetAllGalleriesAsync(onlyActive: true)` still filters out any photo inside it that is individually still pending.

**Acceptance Scenarios**:

1. **Given** a member with no existing album, **When** they call `POST api/gallery/member/submit`, **Then** a new `EventGallery` is created together with the first `EventPhoto` in one call, both starting in a pending state.
2. **Given** a member who owns a gallery, **When** they call `POST api/gallery/albums/{id}/photos`, **Then** the photo is added; **when** a different member (not the owner, not an admin) calls it, **Then** the request is forbidden.
3. **Given** a public request for `GET api/gallery` (`onlyActive = true`), **When** a gallery is approved but one of its photos is still pending, **Then** that photo is excluded from the response while the gallery and its approved photos remain visible.

---

### User Story 4 - Admin manages static site content blocks (Priority: P3)

An admin edits reusable content blocks (grouped by page/section) that render on public pages such as "about".

**Why this priority**: Lowest-risk of the five controllers: no moderation workflow, no file upload, admin-only end to end.

**Independent Test**: Create a `SiteContentDto` with `group = "about"`, confirm it appears in `GET api/site-content?group=about` once active, update it, then delete it and confirm a second `GET` for that id returns 404.

**Acceptance Scenarios**:

1. **Given** an anonymous visitor, **When** they call `GET api/site-content?group=about`, **Then** they receive only active blocks for that group, served through the `PublicContent` output-cache policy.
2. **Given** an admin, **When** they call `PUT api/site-content/{id}` for a nonexistent id, **Then** the service throws `KeyNotFoundException` and the controller returns 404.

---

### User Story 5 - Public reads an approved archive collection; a member submits an item for moderation (Priority: P3)

The archive holds historical collections and items (documents, media, transcripts). Anyone can browse published collections; any authenticated user (not just admin) can submit an item, but non-admin submissions are forced into draft/pending state.

**Why this priority**: Smallest surface, no controller-level test file exists for it, and `CreateItem` is the only endpoint in the whole domain open to any authenticated caller rather than gated by role, so it is worth documenting on its own.

**Independent Test**: Submit an archive item as a non-admin authenticated user and confirm `PublicationState` is forced to `Draft` and `ModerationState` to `Pending` regardless of what the request body sent; confirm `GET api/archive/public` never returns it until an admin moderates it.

**Acceptance Scenarios**:

1. **Given** an anonymous visitor, **When** they call `GET api/archive/public`, **Then** they receive only collections `GetPublicCollectionsAsync` returns as public.
2. **Given** a non-admin authenticated member, **When** they call `POST api/archive/items` with `PublicationState = Published`, **Then** the controller overwrites it to `Draft` and `ModerationState` to `Pending` before calling the service.
3. **Given** a pending item, **When** an admin calls `POST api/archive/admin/items/{id}/moderate` with `state` and `publication` query parameters, **Then** `ModerateItemAsync` updates both fields.

### Edge Cases

- What happens when two members register for the last slot of a capacity-limited event at the same time? `RegisterForEventAsync` (`GHCAA.Infrastructure/Services/EventService.cs:277-346`) inserts first, then re-counts slot-consuming registrations ordered by `Id`; if the insert pushed the event over `ParticipantLimit`, the registration is demoted to `Waitlisted` (if `HasWaitlist`) or removed and the call throws: this closes the race rather than allowing overfill.
- What happens when a member tries to register twice for the same event? The service checks for an existing non-`Rejected` registration by `MemberId` (or by `GuestEmail` for guests) and throws "You are already registered for this event." (`EventService.cs:248-262`).
- What happens when a guest tries to register for a members-only event? `RegisterForEventAsync` throws "This event is for members only." when `memberId` is null and `alumniEvent.AllowNonMembers` is false (`EventService.cs:264-267`).
- What happens when registration is attempted before `RegistrationStartDate`, after `RegistrationEndDate`, or after the event's own `EndDate`? All three are checked independently and each throws its own message; the event's `EndDate` is a hard backstop even if no `RegistrationEndDate` was ever configured (`EventService.cs:236-246`).
- What happens when an admin tries to send an invitation for a registration that was never approved? `GetRegistrationForInvitation` in `EventsController.cs:132-156` returns 400 before the service is called.
- What happens when a member submits a `Notice`-type article? `NewsController.SubmitArticle` (`NewsController.cs:111-132`) forbids it outright; only admins can author notices.
- What happens when a non-owner, non-admin member tries to add a photo to someone else's gallery? `GalleryController.AddPhotoToAlbum` (`GalleryController.cs:205-226`) returns `Forbid()`.
- What happens when `SiteContentController.Update` targets a missing id? The service throws `KeyNotFoundException`, caught by the controller and turned into 404 (`SiteContentController.cs:64-72`).
- What happens when a non-admin submits an archive item with `PublicationState = Published`? `ArchiveController.CreateItem` (`ArchiveController.cs:63-75`) overwrites it before calling the service: the request body's value is not trusted for non-admins.

## Requirements *(mandatory)*

### Functional Requirements

**Events**

- FR-001: The system shall let an anonymous caller list active events via `GET api/events` and view a single event via `GET api/events/{id}`. `[code+test]`: `EventsControllerTests.GetActiveEvents_ReturnsOk`, `GetEventById_ReturnsOk_IfFound`.
- FR-002: The system shall reject event registration when `RequiresRegistration` is false, the event is inactive, the event has ended, or the registration window has not opened or has closed, returning `InvalidOperationException` messages specific to each case. `[code]`: `EventService.cs:229-246`.
- FR-003: The system shall reject a second registration to the same event from the same member (or the same `GuestEmail` for a guest) while an existing registration is not `Rejected`. `[code]`: `EventService.cs:248-262`.
- FR-004: The system shall reject a guest registration when the event does not have `AllowNonMembers` set. `[code]`: `EventService.cs:264-267`.
- FR-005: The system shall generate a `TicketCode` as an 8-character uppercase hex string at registration time, not at approval time. `[code+test]`: `EventService.cs:312`; `EventServiceTests.RegisterForEventAsync_ShouldCreateRegistration`.
- FR-006: The system shall set a registration's initial `Status` to `Waitlisted` instead of rejecting it when the event has a `ParticipantLimit`, the limit is reached, and `HasWaitlist` is true; otherwise it shall throw when the limit is reached. `[code+test]`: `EventService.cs:277-296`; `EventServiceTests.RegisterForEventAsync_ShouldWaitlist_WhenCapacityExceeded`, `RegisterForEventAsync_ShouldThrow_WhenCapacityFull_AndNoWaitlist`.
- FR-007: The system shall re-check capacity after insert (ordered by registration `Id`) and demote or remove the registration if a concurrent insert pushed the event over its `ParticipantLimit`. `[code]`: `EventService.cs:320-346`.
- FR-008: The system shall accept an optional receipt file on registration, validated as `FileCategory.Document` up to 10 MB, and store it via `IFileStorageService` under `FileUploadType.PaymentProof`. `[code+test]`: `EventsController.cs:96`; `EventServiceTests.RegisterForEventAsync_ShouldIncludeReceiptPath_WhenFileProvided`.
- FR-009: The system shall let an authenticated member list their own registrations via `GET api/events/my-registrations`, and shall return an empty list rather than an error for a SuperAdmin who has no member record. `[code+test]`: `EventsController.cs:115-130`; `EventsControllerTests.GetMyRegistrations_ReturnsOk`, `GetMyRegistrations_SuperAdmin_ReturnsEmptyList`.
- FR-010: The system shall let an admin approve or reject a registration via `POST api/events/admin/approve-registration`, stamping `ApprovedAt` and `ApprovedByAdminId`, sending an `EVENT_PARTICIPATION_APPROVED` email and an in-app notification on approval. `[code+test]`: `EventService.cs:494-525`; `EventServiceTests.ApproveRegistrationAsync_ShouldUpdateStatusAndSendEmail`.
- FR-011: The system shall block an invitation request (`GET api/events/registrations/{id}/invitation`) with 400 unless the registration's `Status` is `Approved`, and shall let an admin bypass ownership while a non-admin must own the registration or receive 403. `[code]`: `EventsController.cs:132-156`.
- FR-012: The system shall check a participant in via `POST api/events/admin/checkin/qr` by matching a non-empty `TicketCode` to a registration and setting `IsCheckedIn`/`CheckedInAt`, and shall reject check-in for a registration that is not `Approved`. `[code+test]`: `EventsController.cs:246-253`; `EventServiceTests.CheckInParticipantAsync_ShouldSetCheckInTime`, `CheckInParticipantAsync_ShouldFail_WhenNotApproved`.
- FR-013: The system shall let an admin create, update, delete an event and upload its logo (`FileCategory.Image`, 5 MB limit) behind `Constants.Policies.AdminOnly`. `[code+test]`: `EventsController.cs:168-214`; `EventsControllerTests.CreateEvent_ReturnsCreatedAtAction`, `UpdateEvent_ReturnsOk`, `DeleteEvent_ReturnsOk_OnSuccess`, `UploadEventLogo_ReturnsOk`.
- FR-014: The system shall let an admin manage per-event operational tasks (create/toggle/delete) and a budget (view/update, add/delete expense) under `api/events/{id}/tasks` and `api/events/{id}/budget`. `[code]`: `EventsController.cs:257-319`.

**News**

- FR-015: The system shall let an anonymous caller list active news via `GET api/news`, filterable by `ArticleCategory` (`Event`, `Magazine`, `Regular`) and `PostType` (`News`, `Notice`), and view one article by id. `[code+test]`: `NewsController.cs:34-48`; `NewsControllerTests.GetActiveNews_ReturnsOk`, `GetActiveNews_PassesPostTypeFilterToService`.
- FR-016: The system shall forbid a non-admin, non-SuperAdmin caller from submitting a `PostType.Notice` article via `POST api/news/submit`. `[code+test]`: `NewsController.cs:111-132`; `NewsControllerTests.SubmitArticle_Forbids_WhenMemberSubmitsNotice`, `SubmitArticle_Allows_WhenAdminSubmitsNotice`.
- FR-017: The system shall force a non-admin submission's `Status` to `Pending` (unless `Draft` was requested) and `IsActive` to `false`, ignoring any other value sent in the request body. `[code]`: `NewsController.cs:120-131`.
- FR-018: The system shall let an admin approve or reject a pending article through `POST api/news/{id}/approve` (alias `admin/{id}/approve`) or `.../reject` (same alias pattern). `[code+test]`: `NewsController.cs:134-150`; `NewsControllerTests.ApproveArticle_ReturnsOk_OnSuccess`, `RejectArticle_ReturnsOk_OnSuccess`.
- FR-019: The system shall expose a mobile-aliased route `News/Pending` alongside `pending` and `admin/pending` for listing pending submissions, all admin-only. `[code]`: `NewsController.cs:58-66` (route comment: `// Mobile Alias`).
- FR-020: The system shall let any authenticated caller (not admin-only) upload a standalone image via `POST api/news/upload-image`, validated as `FileCategory.Image` up to 10 MB, stored under the caller's user id with filename pattern `news_{unixTimestamp}_{guid8chars}{ext}`, and return its URL. `[code]`: `NewsController.cs:168-196`. No test exercises this action; `NewsControllerTests.UploadDocument_ReturnsOk_WithStoredUrl` covers only the sibling document upload.
- FR-021: The system shall restrict notice document upload (`POST api/news/{id}/document`) to admins only, validated as `FileCategory.Document` up to 10 MB, filename pattern `notice_{unixTimestamp}_{guid8chars}{ext}`. `[code+test]`: `NewsController.cs:198-226`; `NewsControllerTests.UploadDocument_ReturnsOk_WithStoredUrl`.
- FR-022: The system shall let an admin add or remove a collaborator on a news post. `[code]`: `NewsController.cs:152-166`.

**Gallery**

- FR-023: The system shall let an admin upload a photo directly into a gallery via `POST api/gallery/upload`, validated as `FileCategory.Image` up to 10 MB. `[code+test]`: `GalleryController.cs:33-52`; `GalleryControllerTests.UploadPhoto_ReturnsOk_OnSuccess`, `UploadPhoto_ReturnsBadRequest_WhenValidationFails`.
- FR-024: The system shall let an anonymous caller list only active galleries (`GET api/gallery`) and view one active gallery, while excluding any individual photo inside an approved gallery that is itself still pending. `[code+test]`: `GalleryController.cs:54-62`, `97-105`; `GalleryServiceTests.GetAllGalleriesAsync_OnlyActive_FiltersOutUnapprovedPhotos_WithinAnApprovedGallery`, `GetGalleryByIdAsync_FiltersPendingPhotos_WhenGalleryIsApproved`.
- FR-025: The system shall let a member submit their first photo and create a new album in one call via `POST api/gallery/member/submit`, with the album and photo starting pending/inactive. `[code+test]`: `GalleryController.cs:165-181`; `GalleryServiceTests.CreateMemberAlbumAsync_CreatesPendingInactiveAlbum_AndNotifiesAdmins`.
- FR-026: The system shall let a member add a photo to an album they own via `POST api/gallery/albums/{id}/photos`, and shall return 403 when a non-owner, non-admin member attempts it. `[code+test]`: `GalleryController.cs:205-226`; `GalleryControllerTests.AddPhotoToAlbum_ReturnsForbid_WhenNotOwnerAndNotAdmin`.
- FR-027: The system shall let an admin approve or reject a gallery or an individual photo, each accepting an optional `notifyMember` flag (default `true`) controlling whether a notification is sent. `[code+test]`: `GalleryController.cs:239-269`; `GalleryServiceTests.ApproveGalleryAsync_SkipsNotification_WhenNotifyMemberIsFalse`, `RejectGalleryAsync_SkipsNotification_WhenNotifyMemberIsFalse`, `ApprovePhotoAsync_SkipsNotification_WhenNotifyMemberIsFalse`, `RejectPhotoAsync_SkipsNotification_WhenNotifyMemberIsFalse`.
- FR-028: The system shall not send an approval notification when the gallery has no owner member. `[code+test]`: `GalleryServiceTests.ApproveGalleryAsync_DoesNotNotify_WhenGalleryHasNoOwner`.
- FR-029: The system shall let an admin toggle a gallery's `IsActive` and `IsFeatured` flags independently via `PATCH admin/{id}/toggle-active` and `PATCH admin/{id}/toggle-featured`, returning 404 for a missing gallery. `[code+test]`: `GalleryController.cs:73-95`; `GalleryControllerTests.ToggleActive_ReturnsOk_WithFlippedFlag`, `ToggleActive_ReturnsNotFound_WhenGalleryMissing`, `ToggleFeatured_ReturnsOk_WithFlippedFlag`, `ToggleFeatured_ReturnsNotFound_WhenGalleryMissing`.
- FR-030: The system shall let an admin create a gallery by accepting the `EventGallery` domain model directly as the request body, rather than a dedicated create DTO. `[code+test]`: `GalleryController.cs:107-119`; `GalleryControllerTests.CreateGallery_ReturnsCreatedAtAction`.

**Site content**

- FR-031: The system shall serve only active site-content blocks for a given `group` (default `"about"`) to anonymous callers via `GET api/site-content`, behind the `Constants.OutputCachePolicies.PublicContent` cache policy. `[code+test]`: `SiteContentController.cs:23-30`; `SiteContentControllerTests.GetByGroup_ReturnsOk`; `SiteContentServiceTests.GetActiveByGroupAsync_FiltersByGroupAndActive_OrderedByDisplayOrder`.
- FR-032: The system shall let an admin create or update a site-content block, sanitizing the body HTML and stamping the admin id who made the change. `[code+test]`: `SiteContentController.cs:48-73`; `SiteContentServiceTests.CreateAsync_SanitizesBodyHtml`, `UpdateAsync_PersistsChangesAndStampsAdmin`.
- FR-033: The system shall return 404 (not a 500) when updating a site-content block whose id does not exist, by catching `KeyNotFoundException` from the service. `[code+test]`: `SiteContentController.cs:64-72`; `SiteContentControllerTests.Update_ReturnsNotFound_WhenServiceThrowsKeyNotFound`.
- FR-034: The system shall let an admin delete a site-content block and return 404 when it does not exist. `[code+test]`: `SiteContentController.cs:75-81`; `SiteContentServiceTests.DeleteAsync_ReturnsFalse_WhenMissing`.

**Archive**

- FR-035: The system shall serve only published archive collections and items to anonymous callers via `GET api/archive/public` and `GET api/archive/items/{id}`, hiding unapproved items. `[code+test]`: `ArchiveController.cs:19-30`; `ArchiveServiceTests.PublicItem_hides_unapproved_items`.
- FR-036: The system shall let any authenticated caller (not admin-restricted) submit an archive item via `POST api/archive/items`, but shall force `PublicationState = Draft` and `ModerationState = Pending` when the caller is not in the `Admin` or `SuperAdmin` role, overwriting whatever the request body sent. `[code]`: `ArchiveController.cs:63-75`.
- FR-037: The system shall reject an archive item that has neither media nor a transcript. `[code+test]`: `ArchiveServiceTests.CreateItem_rejects_records_without_media_or_transcript`.
- FR-038: The system shall let an admin list every archive item (including incomplete records) via `GET api/archive/admin/items`, unlike the public endpoint. `[code+test]`: `ArchiveController.cs:33-35`; `ArchiveServiceTests.Admin_items_include_incomplete_records`.
- FR-039: The system shall let admin search across archive items match transcript text. `[code+test]`: `ArchiveServiceTests.Admin_search_matches_transcript`.
- FR-040: The system shall let an admin moderate an item's `ModerationState` and `PublicationState` together in one call via `POST api/archive/admin/items/{id}/moderate`. `[code]`: `ArchiveController.cs:90-93`.

### Key Entities

- **AlumniEvent**: an event with `RequiresRegistration`, `RequiresPayment`, `AllowNonMembers`, `RegistrationStartDate`/`RegistrationEndDate`, `EndDate`, `ParticipantLimit`, `HasWaitlist`.
- **EventRegistration**: one registration against an event, either by `MemberId` or as a guest (`IsNonMember`, `GuestName`, `GuestEmail`, `GuestMobile`); carries `Status` (`Pending`, `Approved`, `Rejected`, `Waitlisted`), `TicketCode`, `IsCheckedIn`/`CheckedInAt`, `ApprovedAt`/`ApprovedByAdminId`, and payment fields (`PaymentReference`, `PaymentMethod`, `ReceiptPath`, `ContributionAmount`).
- **NewsPost**: an article or notice with `ArticleCategory` (`Event`, `Magazine`, `Regular`), `PostType` (`News`, `Notice`), `SubmissionStatus` (`Draft`, `Pending`, `Approved`, `Rejected`), `IsActive`, and optional collaborators.
- **EventGallery**: a photo album, either admin-created or member-owned (`OwnerMemberId`), with `IsActive`, `IsFeatured`, and an approval/rejection reason.
- **EventPhoto**: a photo inside a gallery with its own independent approval state, separate from the gallery's.
- **SiteContent**: a content block keyed by `group` and `DisplayOrder`, with sanitized HTML body and `IsActive`.
- **ArchiveCollection** / **ArchiveItem**: a historical collection and the items inside it, each with `ArchivePublicationState` (`Draft`, `Published`, `Archived`) and `ArchiveModerationState` (`Pending`, `Approved`, `Rejected`), and either media or a transcript.

## Evidence

| FR | API (verb + route) | Service method | Web (file) | Mobile (file) | Test (file::test name) |
|---|---|---|---|---|---|
| FR-001 | GET api/events, GET api/events/{id} | GetActiveEventsAsync, GetEventByIdAsync | GHCAA.Web/src/app/common/events/events.ts | GHCAA.Mobile/lib/screens/member/events_screen.dart | EventsControllerTests.cs::GetActiveEvents_ReturnsOk |
| FR-002 | POST api/events/register | RegisterForEventAsync | GHCAA.Web/src/app/common/events/events.ts | GHCAA.Mobile/lib/screens/member/event_details_screen.dart | EventServiceTests.cs::RegisterForEventAsync_ShouldThrowException_WhenDeadlinePassed |
| FR-003 | POST api/events/register | RegisterForEventAsync | GHCAA.Web/src/app/common/events/events.ts | GHCAA.Mobile/lib/screens/member/event_details_screen.dart | none found |
| FR-004 | POST api/events/register | RegisterForEventAsync | GHCAA.Web/src/app/common/events/events.ts | GHCAA.Mobile/lib/screens/member/event_details_screen.dart | EventServiceTests.cs::RegisterForEventAsync_NonMember_ShouldFail_WhenEventDoesNotAllow |
| FR-005 | POST api/events/register | RegisterForEventAsync | GHCAA.Web/src/app/common/events/events.ts | GHCAA.Mobile/lib/screens/member/event_details_screen.dart | EventServiceTests.cs::RegisterForEventAsync_ShouldCreateRegistration |
| FR-006 | POST api/events/register | RegisterForEventAsync | GHCAA.Web/src/app/common/events/events.ts | GHCAA.Mobile/lib/screens/member/event_details_screen.dart | EventServiceTests.cs::RegisterForEventAsync_ShouldWaitlist_WhenCapacityExceeded |
| FR-007 | POST api/events/register | RegisterForEventAsync | GHCAA.Web/src/app/common/events/events.ts | GHCAA.Mobile/lib/screens/member/event_details_screen.dart | none found |
| FR-008 | POST api/events/register | RegisterForEventAsync | GHCAA.Web/src/app/common/events/events.ts | GHCAA.Mobile/lib/screens/member/event_details_screen.dart | EventServiceTests.cs::RegisterForEventAsync_ShouldIncludeReceiptPath_WhenFileProvided |
| FR-009 | GET api/events/my-registrations | GetRegistrationsByMemberAsync | GHCAA.Web/src/app/common/events/events.ts | GHCAA.Mobile/lib/screens/member/events_screen.dart | EventsControllerTests.cs::GetMyRegistrations_SuperAdmin_ReturnsEmptyList |
| FR-010 | POST api/events/admin/approve-registration | ApproveRegistrationAsync | GHCAA.Web/src/app/admin/events/admin-events.ts | none found | EventServiceTests.cs::ApproveRegistrationAsync_ShouldUpdateStatusAndSendEmail |
| FR-011 | GET api/events/registrations/{id}/invitation | GetRegistrationByIdAsync | GHCAA.Web/src/app/common/events/events.ts | GHCAA.Mobile/lib/screens/member/event_details_screen.dart | none found |
| FR-012 | POST api/events/admin/checkin/qr | CheckInByTicketCodeAsync | GHCAA.Web/src/app/admin/events/admin-event-operations.ts | none found | EventServiceTests.cs::CheckInParticipantAsync_ShouldFail_WhenNotApproved |
| FR-013 | POST/PUT/DELETE api/events, POST api/events/admin/{id}/logo | CreateEventAsync, UpdateEventAsync, DeleteEventAsync, UpdateEventLogoAsync | GHCAA.Web/src/app/admin/events/admin-events.ts | none found | EventsControllerTests.cs::CreateEvent_ReturnsCreatedAtAction |
| FR-014 | api/events/{id}/tasks, api/events/{id}/budget | GetEventTasksAsync et al. (not opened in this pass) | GHCAA.Web/src/app/admin/events/admin-event-operations.ts | none found | not read in this pass |
| FR-015 | GET api/news | GetActiveNewsAsync | GHCAA.Web/src/app/common/news/news.ts | GHCAA.Mobile/lib/screens/member/news_screen.dart | NewsControllerTests.cs::GetActiveNews_PassesPostTypeFilterToService |
| FR-016 | POST api/news/submit | SubmitArticle (controller-level check) | GHCAA.Web/src/app/common/news/news.ts | GHCAA.Mobile/lib/screens/member/news_screen.dart | NewsControllerTests.cs::SubmitArticle_Forbids_WhenMemberSubmitsNotice |
| FR-017 | POST api/news/submit | SubmitArticle (controller-level force) | GHCAA.Web/src/app/common/news/news.ts | GHCAA.Mobile/lib/screens/member/news_screen.dart | NewsServiceTests.cs::UpdateNewsAsync_DoesNotChangeStatus_EvenWhenDtoCarriesTheDefault (covers the update path, not submit; see Gaps) |
| FR-018 | POST api/news/{id}/approve, .../reject | ApproveArticleAsync, RejectArticleAsync | GHCAA.Web/src/app/admin/news/admin-news.ts | none found | NewsControllerTests.cs::ApproveArticle_ReturnsOk_OnSuccess |
| FR-019 | GET api/news/pending, admin/pending, News/Pending | GetPendingSubmissionsAsync | GHCAA.Web/src/app/admin/news/admin-news.ts | none found | none found |
| FR-020 | POST api/news/upload-image | UploadImage (controller-level, no dedicated service method) | GHCAA.Web/src/app/admin/news/admin-news.ts | none found | none found |
| FR-021 | POST api/news/{id}/document | UploadDocument (controller-level) | GHCAA.Web/src/app/admin/news/admin-news.ts | none found | NewsControllerTests.cs::UploadDocument_ReturnsOk_WithStoredUrl |
| FR-022 | POST/DELETE api/news/{id}/collaborators | AddCollaboratorAsync, RemoveCollaboratorAsync | GHCAA.Web/src/app/admin/news/admin-news.ts | none found | not read in this pass |
| FR-023 | POST api/gallery/upload | AddPhotosToGalleryAsync (via controller save) | GHCAA.Web/src/app/admin/gallery/admin-gallery.ts | none found | GalleryControllerTests.cs::UploadPhoto_ReturnsOk_OnSuccess |
| FR-024 | GET api/gallery, GET api/gallery/{id} | GetAllGalleriesAsync, GetGalleryByIdAsync | GHCAA.Web/src/app/common/gallery/gallery.ts | GHCAA.Mobile/lib/screens/member/gallery_screen.dart | GalleryServiceTests.cs::GetAllGalleriesAsync_OnlyActive_FiltersOutUnapprovedPhotos_WithinAnApprovedGallery |
| FR-025 | POST api/gallery/member/submit | CreateMemberAlbumAsync, AddMemberPhotoToAlbumAsync | GHCAA.Web/src/app/common/gallery/gallery.ts | GHCAA.Mobile/lib/screens/member/gallery_screen.dart | GalleryServiceTests.cs::CreateMemberAlbumAsync_CreatesPendingInactiveAlbum_AndNotifiesAdmins |
| FR-026 | POST api/gallery/albums/{id}/photos | AddMemberPhotoToAlbumAsync | GHCAA.Web/src/app/common/gallery/gallery.ts | GHCAA.Mobile/lib/screens/member/gallery_screen.dart | GalleryControllerTests.cs::AddPhotoToAlbum_ReturnsForbid_WhenNotOwnerAndNotAdmin |
| FR-027 | POST admin/{id}/approve, admin/{id}/reject (gallery+photo) | ApproveGalleryAsync, RejectGalleryAsync, ApprovePhotoAsync, RejectPhotoAsync | GHCAA.Web/src/app/admin/gallery-approval/gallery-approval.ts | GHCAA.Mobile/lib/screens/admin/gallery_approval_screen.dart | GalleryServiceTests.cs::ApproveGalleryAsync_SkipsNotification_WhenNotifyMemberIsFalse |
| FR-028 | POST admin/{id}/approve | ApproveGalleryAsync | GHCAA.Web/src/app/admin/gallery-approval/gallery-approval.ts | GHCAA.Mobile/lib/screens/admin/gallery_approval_screen.dart | GalleryServiceTests.cs::ApproveGalleryAsync_DoesNotNotify_WhenGalleryHasNoOwner |
| FR-029 | PATCH admin/{id}/toggle-active, toggle-featured | ToggleActive/ToggleFeatured (controller-level, direct on entity) | GHCAA.Web/src/app/admin/gallery/admin-gallery.ts | none found | GalleryControllerTests.cs::ToggleActive_ReturnsOk_WithFlippedFlag |
| FR-030 | POST api/gallery | CreateEventGalleryAsync | GHCAA.Web/src/app/admin/gallery/admin-gallery.ts | none found | GalleryControllerTests.cs::CreateGallery_ReturnsCreatedAtAction |
| FR-031 | GET api/site-content | GetActiveByGroupAsync | GHCAA.Web/src/app/core/services/site-content.service.ts | none found | SiteContentServiceTests.cs::GetActiveByGroupAsync_FiltersByGroupAndActive_OrderedByDisplayOrder |
| FR-032 | POST/PUT api/site-content | CreateAsync, UpdateAsync | GHCAA.Web/src/app/admin/site-content/site-content.ts | none found | SiteContentServiceTests.cs::CreateAsync_SanitizesBodyHtml |
| FR-033 | PUT api/site-content/{id} | UpdateAsync | GHCAA.Web/src/app/admin/site-content/site-content.ts | none found | SiteContentControllerTests.cs::Update_ReturnsNotFound_WhenServiceThrowsKeyNotFound |
| FR-034 | DELETE api/site-content/{id} | DeleteAsync | GHCAA.Web/src/app/admin/site-content/site-content.ts | none found | SiteContentControllerTests.cs::Delete_ReturnsNotFound_WhenMissing |
| FR-035 | GET api/archive/public, GET api/archive/items/{id} | GetPublicCollectionsAsync, GetPublicItemAsync | GHCAA.Web/src/app/core/services/archive.service.ts | GHCAA.Mobile/lib/screens/member/legacy_archive_screen.dart | ArchiveServiceTests.cs::PublicItem_hides_unapproved_items |
| FR-036 | POST api/archive/items | CreateItemAsync (controller-level force) | GHCAA.Web/src/app/core/services/archive.service.ts | GHCAA.Mobile/lib/features/archive/archive_service.dart | none found |
| FR-037 | POST api/archive/items | CreateItemAsync | GHCAA.Web/src/app/core/services/archive.service.ts | GHCAA.Mobile/lib/features/archive/archive_service.dart | ArchiveServiceTests.cs::CreateItem_rejects_records_without_media_or_transcript |
| FR-038 | GET api/archive/admin/items | GetAdminItemsAsync | none found | none found | ArchiveServiceTests.cs::Admin_items_include_incomplete_records |
| FR-039 | GET api/archive/admin/items | GetAdminItemsAsync | none found | none found | ArchiveServiceTests.cs::Admin_search_matches_transcript |
| FR-040 | POST api/archive/admin/items/{id}/moderate | ModerateItemAsync | none found | none found | none found |

## Gaps

- No web admin UI component exists for archive collections/items (`GHCAA.Web/src/app` has `core/services/archive.service.ts` and `core/models/archive.models.ts` but no `admin/archive/*` component), and no controller-level test file exists for `ArchiveController` (only `GHCAA.Tests/Services/ArchiveServiceTests.cs` at the service layer): confirmed by directory search, not inferred. `docs/specs/014-events-news-gallery-content` evidence table, FR-038 through FR-040.
- `GalleryController.UploadPhoto` (`GHCAA.API/Controllers/GalleryController.cs:33-52`) is `[Authorize(Policy = AdminOnly)]` but resolves the acting user through `this.CurrentMemberIdRaw()` rather than `CurrentUserIdRaw()`, with an inline comment acknowledging it as a fallback ("Admin should have MemberId"). An admin without a linked `MemberId` (a system admin: see `gotcha_system_admin_no_email.md` in project memory) would fail this claim lookup. [NEEDS CLARIFICATION: is every admin account guaranteed to carry a MemberId, or can a system-admin-only account hit this path?]
- `NewsController.UploadImage` (`NewsController.cs:168-196`) is open to any authenticated member, per its comment "Allow members to upload images for their articles too". It takes no article id, so it cannot attach to another member's article. It does let any member store 10 MB images with no per-member quota or rate limit, and the stored file is not tied to an article, so an image from an abandoned draft is never cleaned up. No test covers the action.
- Archive is the only controller in this domain where a non-admin `[Authorize]` caller can call a create endpoint directly (`POST api/archive/items`); every other domain's member-facing create path goes through a domain-specific submit route (`api/news/submit`, `api/gallery/member/submit`). The forced-draft override protects publication state, but there is no rate limit or per-member cap visible in `ArchiveService` on how many items a member can submit. [NEEDS CLARIFICATION: is an unbounded per-member submission volume acceptable for archive items the way it would not be for, say, event registrations?]
- `EventsController.RegisterForEventForm`/`RegisterForEventJson` both call the same private `ProcessRegistration`, but `EventsControllerTests.cs` only has one test per entry point (`RegisterForEventForm_ReturnsOk`, `RegisterForEventJson_ReturnsOk`) that checks the happy path; the capacity/duplicate/deadline edge cases documented above (FR-002 through FR-004, FR-007) are covered at the `EventServiceTests.cs` level, not at the controller level.

## Enhancements: modularisation and reusability

### Reuse across layers

- ENH-001 (P2): `EventsController`, `NewsController` and `GalleryController` each implement their own claim-extraction fallback pattern for resolving the acting member/admin id (`CurrentMemberIdRaw()` vs `CurrentUserIdRaw()`, with ad hoc `int.TryParse` and a private `TryGetMemberId`/`TryGetAdminId`/`TryMemberId` helper duplicated in `GalleryController.cs:271-275`, `SiteContentController.cs:83-84` and `ArchiveController.cs:95-96`). The same three-line pattern appears once per controller instead of as a single shared extension method already living in `GHCAA.API.Extensions`.

### Entity-based module shape

- ENH-002 (P2): Events and Gallery are the two domains in this spec with the closest structural match: both have an admin-authored path and a member-submission path that lands in a moderation queue, both have approve/reject actions with an optional notify flag, and both attach files. `EventGallery`/`EventPhoto` (gallery) and `AlumniEvent`/`EventRegistration` (events) do not share a base type or a common moderation-state enum today: gallery moderation is boolean-ish (`IsActive` plus a rejection-reason string) while events use `EventRegistrationStatus` (`Pending, Approved, Rejected, Waitlisted`) and archive uses a distinct `ArchiveModerationState` (`Pending, Approved, Rejected`) (`GHCAA.Domain/Enums.cs:30, 38, 67`). Three separate pending/approved/rejected shapes exist for what is functionally the same moderation queue concept in News (`SubmissionStatus`), Gallery (boolean + reason), and Archive (`ArchiveModerationState`). Unifying them was out of scope for this reverse-documentation pass since it would touch the domain model, not just the API surface, and this spec's brief is to describe current behaviour, not redesign it.
- ENH-003 (P3): `GalleryController.CreateGallery` (`GalleryController.cs:107-119`) takes the raw `EventGallery` domain model as its request body instead of a DTO, which is inconsistent with every other create endpoint in this domain (`CreateEventDto`, `CreateNewsDto`, `ArchiveCollectionDto`, `UpsertSiteContentDto`) and couples the API contract directly to the EF entity shape.

### Existing reusable components

- Confirmed in use, not bypassed: `admin/events/admin-events.html`, `admin/news/admin-news.html`, `admin/gallery/admin-gallery.html` and `admin/site-content/site-content.html` all reference the shared `app-page-header`/`app-search-bar`/`app-breadcrumb` components rather than hand-rolled headers.
- ENH-004 (P3): `GHCAA.Mobile/lib` has no `SiteContent`-related screen or service file at all (confirmed by directory search across `lib/features`, `lib/screens`, `lib/core`): static content blocks (e.g. "about") appear to be web-only today. This is a scope gap, not a missing-widget problem, so it is filed here rather than under configuration.

### Hard-coded behaviour that should be configuration

- ENH-005 (P3): File size limits are repeated as literals at each call site rather than drawn from a shared constant: `10 * 1024 * 1024` appears five times (`EventsController.cs:96`, `NewsController.cs:172`, `NewsController.cs:202`, `GalleryController.cs:37`, `GalleryController.cs:171`, `GalleryController.cs:218`) and `5 * 1024 * 1024` once (`EventsController.cs:197`, event logo). No shared constant for these byte values was found under `GHCAA.Application`'s `Constants` classes in this pass.
- ENH-006 (P2): `GalleryController.ApproveGallery`/`RejectGallery`/`ApprovePhoto`/`RejectPhoto` and their News/Archive counterparts each hard-code the notify-by-default behaviour (`notifyMember = true`) as a C# default parameter rather than an org-configurable setting, even though the project already has an org-config mechanism used elsewhere (see `gotcha_orgconfig_feature_flag_default_drift.md` in project memory) that this could route through instead of a redeploy-only default.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- SC-001: A member can register for an event, get approved, and be checked in by QR code without any manual database intervention, exercising `RegisterForEventAsync`, `ApproveRegistrationAsync` and `CheckInByTicketCodeAsync` in sequence.
- SC-002: No article authored by a non-admin ever reaches `GET api/news` with `IsActive = true` without passing through `ApproveArticleAsync`.
- SC-003: No photo inside an approved gallery is publicly visible through `GET api/gallery/{id}` while its own `EventPhoto` moderation state is still pending.
- SC-004: Every `POST api/archive/items` call from a non-admin lands as `Draft`/`Pending`, never `Published`/`Approved`, regardless of the request body.

## Assumptions

- "Reverse-documented" FRs describe what the code does, not what was originally specified; where behaviour looks unintentional it is flagged under Gaps rather than corrected here.
- `not read in this pass` in the Evidence table means the file was not opened during this session, not that no implementation exists: a later pass should confirm before treating it as a gap.
- Angular and Flutter client behaviour is summarized from the file inventory and prior test/service reading; individual component bodies for News, Gallery and Site Content were not opened line-by-line in this pass, only their existence and shared-component usage were confirmed.
