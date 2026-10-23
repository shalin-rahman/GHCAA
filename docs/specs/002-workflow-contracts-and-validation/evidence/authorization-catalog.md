# Authorization Policy Catalog

**Reviewed**: 2026-09-26, sections 2 and 3 regenerated 2026-10-21. Built for
[../tasks.md](../tasks.md) T014 and tracker item 84.5 from the files in `GHCAA.API/Controllers`, `GHCAA.API/Extensions/ServiceExtensions.cs`
(`AddAppAuthorization`), `GHCAA.API/Filters/RequireStepUpAttribute.cs`,
`GHCAA.API/Extensions/RateLimitingExtensions.cs` and the tests named in section 5. Sections 2 and
3 come from `docs/api/authz_catalog.py`, which reads the route, policy, step-up and rate-limit
attributes on each action and its controller. Run it with `--write` after changing a controller,
and with `--check` to see whether this file is stale. The other sections are written by hand. It is not a runtime dump, so an attribute added through a convention or filter
registered in `Program.cs` would not show here. None was found.

## 1. Policies defined

| Policy | Rule | Defined at |
|---|---|---|
| `SuperAdminOnly` | `RequireRole(SuperAdmin)` | `ServiceExtensions.cs` `AddAppAuthorization` |
| `AdminOnly` | `RequireRole(SuperAdmin, Admin)` | same |
| `MemberOnly` | `RequireRole(SuperAdmin, Admin, Member)` | same |
| `ElectionStaff` | `RequireRole(SuperAdmin, Admin, ElectionOfficial)` | same. Spec 023 (37.12d). The service then checks the caller's appointment for that election |
| Fallback | `RequireAuthenticatedUser()` | same. Any action with no attribute needs a signed-in user |

No `[Authorize(Roles = ...)]` string is used anywhere. Every role gate is either one of the three
policies or an `IsInRole` check inside the method body, and every `IsInRole` call uses
`Constants.Roles`.

Two further layers sit on top of the policy:

- **Step-up** (`[RequireStepUp]`): the caller must hold a recent `step_up` claim from
  `POST /api/auth/step-up/verify`. Without it the filter answers 403 with a ProblemDetails body whose `code` is `STEP_UP_REQUIRED`.
  See [error-catalog.md](./error-catalog.md), section 2, for the body.
- **Rate limits** (`[EnableRateLimiting]`): per-endpoint policies on top of the global `Api` limit
  that `Program.cs` applies to every controller with `MapControllers().RequireRateLimiting(...)`.

SignalR hubs: `ChatHub` and `NotificationHub` both carry a bare `[Authorize]`. `NotificationHub`
then puts Admin and SuperAdmin callers into the admin group with `IsInRole("Admin")` and
`IsInRole("SuperAdmin")` (`GHCAA.API/Hubs/NotificationHub.cs:21` and `:41`).

## 2. Combinations in use

One row per route template, so an action with a legacy alias route counts once per alias. The
script found 403 routes. That matches the 403 `[Http*]` attributes in the folder.

| Combination | Routes | Controllers |
|---|---|---|
| Anonymous | 72 | Archive, Auth, Campaigns, Contact, CredentialVerification, ElectionContent, Elections, Events, Financials, Gallery, Gateways, Governance, Health, JobHub, Lookups, Networking, News, OrgConfig, PaymentConfig, Registration, Scholarships, SiteContent, Theme |
| Authenticated | 74 | Activity, Assistant, Auth, Campaigns, ElectionAppointments, ElectionPersonasRead, Elections, FamilyLink, Financials, Forum, Governance, MemberCommunications, Mentorship, Messaging, Networking, News, Notification, PendingApprovals, Poll, Profile |
| Authenticated + in-body Admin | 1 | Events |
| Authenticated + in-body Admin or SuperAdmin | 10 | Archive, FamilyLink, Financials, JobHub, News, SecureFiles |
| Authenticated + in-body SuperAdmin | 5 | Events, Financials, Forum |
| Authenticated + step-up | 5 | Auth, ElectionAppointments, Elections |
| MemberOnly | 6 | Gallery, Scholarships |
| MemberOnly + in-body Admin or SuperAdmin | 1 | Gallery |
| ElectionStaff | 4 | ElectionAppointments, Elections |
| ElectionStaff + step-up | 29 | AdminElections, ElectionAppointments, Elections |
| AdminOnly | 145 | Activity, Admin, AdminGovernance, AdminPoll, AdminSocialAuth, Archive, Campaigns, Communication, CredentialVerification, ElectionContent, Elections, Events, Financials, Gallery, JobHub, Lookups, MemberImport, Mentorship, News, PendingApprovals, Scholarships, SiteContent, Theme |
| AdminOnly + in-body SuperAdmin | 4 | Admin |
| AdminOnly + step-up | 4 | Admin, AdminElections, AdminGovernance, Elections |
| AdminOnly + step-up + in-body SuperAdmin | 1 | Admin |
| SuperAdminOnly | 17 | Activity, Admin, AdminDevTracker, AdminErrorLogs, ElectionPersonas, FinancialLedger, Financials, OrgConfig, PaymentConfig, Roles |
| SuperAdminOnly + in-body SuperAdmin | 2 | PaymentConfig |
| SuperAdminOnly + step-up | 23 | Admin, ElectionPersonas, ElectionRulesUnlock, Elections, FinancialLedger, PaymentConfig, Roles |

"In-body" means the policy lets the caller in and the method then branches on
`User.IsInRole(...)`, usually to widen what an Admin can see or do. 24 routes do this.

Where the policy was set:

| Policy | On the action | On the controller | Fallback only |
|---|---|---|---|
| Anonymous | 72 | 0 | 0 |
| Authenticated | 23 | 72 | 0 |
| MemberOnly | 7 | 0 | 0 |
| ElectionStaff | 18 | 15 | 0 |
| AdminOnly | 96 | 58 | 0 |
| SuperAdminOnly | 15 | 27 | 0 |

No route relies on the fallback policy alone. Every action or its controller names its rule.

Class and action attributes both apply. Where a controller and an action name different policies,
the stricter one is shown, and `[AllowAnonymous]` on either one wins over both.

## 3. Endpoints by combination

Paths are built from the controller's `[Route]` and the action's template.
`MessagingController` also answers under `/api/chat` and `NotificationController` under `/api/notification`, because each has a second class-level `[Route]`.
Those copies are not repeated below.

### 3.1 Anonymous

Base policy: `[AllowAnonymous]`, no sign-in.

| Verb | Path | Action | Rate limit |
|---|---|---|---|
| GET | `/api/Theme/active` | `ThemeController.GetActiveTheme` (line 23) |  |
| GET | `/api/archive/items/{id:int}` | `ArchiveController.PublicItem` (line 24) |  |
| GET | `/api/archive/public` | `ArchiveController.PublicCollections` (line 19) |  |
| POST | `/api/auth/facebook` | `AuthController.FacebookLogin` (line 65) | Auth |
| POST | `/api/auth/forgot-password` | `AuthController.ForgotPassword` (line 368) | Auth, PasswordReset |
| POST | `/api/auth/google` | `AuthController.GoogleLogin` (line 55) | Auth |
| POST | `/api/auth/login` | `AuthController.Login` (line 43) | Auth |
| GET | `/api/auth/providers` | `AuthController.GetProviders` (line 35) | Auth |
| POST | `/api/auth/refresh` | `AuthController.Refresh` (line 77) | Auth, Refresh |
| POST | `/api/auth/refresh-mobile` | `AuthController.RefreshMobile` (line 111) | Auth, Refresh |
| POST | `/api/auth/register` | `RegistrationController.Register` (line 28) | Registration |
| POST | `/api/auth/resend-otp` | `RegistrationController.ResendOtp` (line 107) | Registration, Auth |
| POST | `/api/auth/reset-password` | `AuthController.ResetPassword` (line 378) | Auth |
| GET | `/api/auth/status/{id:int}` | `RegistrationController.GetStatus` (line 74) | Registration |
| POST | `/api/auth/verify-email` | `RegistrationController.VerifyEmail` (line 92) | Registration, Auth |
| GET | `/api/campaigns/public` | `CampaignsController.GetPublicCampaigns` (line 23) |  |
| GET | `/api/campaigns/{slug}` | `CampaignsController.GetBySlug` (line 30) |  |
| GET | `/api/campaigns/{slug}/honour-roll` | `CampaignsController.GetHonourRoll` (line 38) |  |
| POST | `/api/campaigns/{slug}/pledges` | `CampaignsController.CreatePledge` (line 49) |  |
| GET | `/api/config` | `OrgConfigController.GetConfig` (line 18) |  |
| POST | `/api/contact` | `ContactController.Submit` (line 21) |  |
| GET | `/api/election-content` | `ElectionContentController.GetPublished` (line 28) |  |
| GET | `/api/election-content/{kind}/{key}` | `ElectionContentController.GetPublishedByKey` (line 36) |  |
| GET | `/api/elections/current` | `ElectionsController.GetCurrent` (line 127) |  |
| GET | `/api/elections/latest-declared` | `ElectionsController.GetLatestDeclared` (line 134) |  |
| GET | `/api/elections/{id:int}` | `ElectionsController.Get` (line 50) |  |
| GET | `/api/elections/{id:int}/audit/verify` | `ElectionsController.VerifyAudit` (line 99) |  |
| GET | `/api/elections/{id:int}/board` | `ElectionsController.Board` (line 61) |  |
| GET | `/api/elections/{id:int}/documents/{formCode}/verification` | `ElectionsController.VerifyDocument` (line 450) | CredentialVerification |
| GET | `/api/elections/{id:int}/nominations` | `ElectionsController.Nominations` (line 235) |  |
| GET | `/api/elections/{id:int}/opened-ballots` | `ElectionsController.OpenedBallots` (line 89) |  |
| GET | `/api/elections/{id:int}/results` | `ElectionsController.Results` (line 78) |  |
| GET | `/api/elections/{id:int}/tracking-codes` | `ElectionsController.TrackingCodes` (line 117) |  |
| GET | `/api/events` | `EventsController.GetActiveEvents` (line 29) |  |
| POST | `/api/events/register` | `EventsController.RegisterForEventForm` (line 60) |  |
| POST | `/api/events/register` | `EventsController.RegisterForEventJson` (line 68) |  |
| GET | `/api/events/{id}` | `EventsController.GetEventById` (line 51) |  |
| GET | `/api/events/{id}/participants` | `EventsController.GetPublicParticipants` (line 42) |  |
| GET | `/api/financials/fees/applicable` | `FinancialsController.GetApplicableFee` (line 26) |  |
| GET | `/api/gallery` | `GalleryController.GetGalleries` (line 50) |  |
| GET | `/api/gallery/{id}` | `GalleryController.GetGallery` (line 97) |  |
| GET | `/api/gateways/callback/bkashgateway` | `GatewaysController.BkashCallbackGet` (line 215) |  |
| GET | `/api/gateways/callback/dgepay` | `GatewaysController.DGePayCallback` (line 243) |  |
| POST | `/api/gateways/callback/sslcommerz` | `GatewaysController.SSLCommerzCallback` (line 188) |  |
| POST | `/api/gateways/initiate` | `GatewaysController.InitiatePayment` (line 50) |  |
| POST | `/api/gateways/webhook/{gateway}` | `GatewaysController.GatewayWebhook` (line 272) |  |
| GET | `/api/governance/constitution` | `GovernanceController.GetCurrentConstitution` (line 53) |  |
| GET | `/api/governance/constitution/history` | `GovernanceController.GetConstitutionHistory` (line 62) |  |
| GET | `/api/governance/current` | `GovernanceController.GetCurrentEC` (line 26) |  |
| GET | `/api/governance/ec/current` | `GovernanceController.GetCurrentEC` (line 26) |  |
| GET | `/api/governance/ec/history` | `GovernanceController.GetECHistory` (line 44) |  |
| GET | `/api/governance/positions` | `GovernanceController.GetPositions` (line 38) |  |
| GET | `/api/jobs` | `JobHubController.GetActiveJobs` (line 26) |  |
| GET | `/api/jobs/{id}` | `JobHubController.GetJob` (line 69) |  |
| GET | `/api/lookups` | `LookupsController.GetAllLookups` (line 35) |  |
| GET | `/api/lookups/stats` | `LookupsController.GetPublicStats` (line 26) |  |
| GET | `/api/lookups/{group}` | `LookupsController.GetByGroup` (line 44) |  |
| GET | `/api/networking/committee` | `NetworkingController.GetExecutiveCommittee` (line 42) |  |
| GET | `/api/networking/directory` | `NetworkingController.Search` (line 23) |  |
| GET | `/api/networking/periods` | `NetworkingController.GetPeriods` (line 50) |  |
| GET | `/api/networking/search` | `NetworkingController.Search` (line 23) |  |
| GET | `/api/networking/updates` | `NetworkingController.GetLatestUpdates` (line 58) |  |
| GET | `/api/news` | `NewsController.GetActiveNews` (line 29) |  |
| GET | `/api/news/{id:int}` | `NewsController.GetNewsById` (line 38) |  |
| GET | `/api/payment-config/active` | `PaymentConfigController.GetActivePaymentMethods` (line 23) |  |
| POST | `/api/scholarships/calls/{callId:int}/applications` | `ScholarshipsController.Apply` (line 25) |  |
| GET | `/api/scholarships/public/calls` | `ScholarshipsController.PublicCalls` (line 22) |  |
| GET | `/api/scholarships/public/funds` | `ScholarshipsController.PublicFunds` (line 19) |  |
| GET | `/api/scholarships/status/{referenceCode}` | `ScholarshipsController.Status` (line 33) | ScholarshipStatus |
| GET | `/api/site-content` | `SiteContentController.GetByGroup` (line 25) |  |
| GET | `/api/verify/{shortCode}` | `CredentialVerificationController.Verify` (line 19) | CredentialVerification |
| GET | `/health` | `HealthController.GetHealth` (line 32) |  |

### 3.2 Authenticated

Base policy: any signed-in user (`[Authorize]` or the fallback policy).

| Verb | Path | Action | Rate limit |
|---|---|---|---|
| GET | `/api/Family/links` | `FamilyLinkController.GetFamily` (line 98) |  |
| GET | `/api/Family/search` | `FamilyLinkController.Search` (line 119) |  |
| GET | `/api/Forum/categories` | `ForumController.GetCategories` (line 23) |  |
| GET | `/api/Forum/categories/{categoryId}/topics` | `ForumController.GetTopics` (line 29) |  |
| POST | `/api/Forum/topics` | `ForumController.CreateTopic` (line 50) |  |
| GET | `/api/Forum/topics/{topicId}` | `ForumController.GetTopic` (line 35) |  |
| GET | `/api/Forum/topics/{topicId}/posts` | `ForumController.GetPosts` (line 44) |  |
| POST | `/api/Forum/topics/{topicId}/posts` | `ForumController.CreatePost` (line 70) |  |
| GET | `/api/activity/me` | `ActivityController.GetMyActivity` (line 22) |  |
| POST | `/api/assistant/ask` | `AssistantController.Ask` (line 20) |  |
| POST | `/api/auth/admin/step-up/request` | `AuthController.RequestStepUp` (line 172) | Auth |
| POST | `/api/auth/admin/step-up/verify` | `AuthController.VerifyStepUp` (line 195) | Auth |
| GET | `/api/auth/authenticator` | `AuthController.GetAuthenticator` (line 235) | Auth |
| POST | `/api/auth/authenticator/confirm` | `AuthController.ConfirmAuthenticator` (line 282) | Auth |
| POST | `/api/auth/authenticator/setup-with-password` | `AuthController.SetupAuthenticatorWithPassword` (line 260) | Auth |
| POST | `/api/auth/logout` | `AuthController.Logout` (line 155) |  |
| GET | `/api/auth/me` | `AuthController.Me` (line 132) |  |
| POST | `/api/auth/step-up/request` | `AuthController.RequestStepUp` (line 172) | Auth |
| POST | `/api/auth/step-up/verify` | `AuthController.VerifyStepUp` (line 195) | Auth |
| GET | `/api/campaigns/my-pledges` | `CampaignsController.GetMyPledges` (line 72) |  |
| GET | `/api/communications/me` | `MemberCommunicationsController.GetMine` (line 14) |  |
| GET | `/api/election-personas` | `ElectionPersonasReadController.List` (line 76) |  |
| POST | `/api/elections/nominations/{nominationId:int}/withdraw` | `ElectionsController.Withdraw` (line 261) |  |
| GET | `/api/elections/{id:int}/ballot` | `ElectionsController.Ballot` (line 67) |  |
| POST | `/api/elections/{id:int}/nominations` | `ElectionsController.Nominate` (line 240) |  |
| GET | `/api/family-links/my-family` | `FamilyLinkController.GetFamily` (line 98) |  |
| GET | `/api/family-links/received` | `FamilyLinkController.GetReceived` (line 87) |  |
| DELETE | `/api/family-links/remove/{requestId}` | `FamilyLinkController.Remove` (line 58) |  |
| POST | `/api/family-links/respond` | `FamilyLinkController.Respond` (line 49) |  |
| GET | `/api/family-links/search` | `FamilyLinkController.Search` (line 119) |  |
| POST | `/api/family-links/send` | `FamilyLinkController.Send` (line 34) |  |
| GET | `/api/family-links/sent` | `FamilyLinkController.GetSent` (line 78) |  |
| POST | `/api/family-links/{requestId}/cancel` | `FamilyLinkController.Cancel` (line 68) |  |
| POST | `/api/financials/record-payment` | `FinancialsController.RecordPayment` (line 50) |  |
| GET | `/api/financials/saved-methods` | `FinancialsController.GetSavedPaymentMethods` (line 228) |  |
| POST | `/api/financials/saved-methods` | `FinancialsController.AddSavedPaymentMethod` (line 238) |  |
| DELETE | `/api/financials/saved-methods/{id}` | `FinancialsController.DeleteSavedPaymentMethod` (line 248) |  |
| POST | `/api/governance/constitution/{id:int}/vote` | `GovernanceController.VoteOnAmendment` (line 69) |  |
| GET | `/api/me/election-appointments` | `ElectionAppointmentsController.Mine` (line 52) |  |
| POST | `/api/me/election-appointments/{id:int}/decline` | `ElectionAppointmentsController.Decline` (line 73) |  |
| GET | `/api/members/family` | `FamilyLinkController.GetFamily` (line 98) |  |
| POST | `/api/members/family` | `FamilyLinkController.Send` (line 34) |  |
| POST | `/api/mentorship` | `MentorshipController.SendRequest` (line 30) |  |
| GET | `/api/mentorship/received` | `MentorshipController.GetReceived` (line 58) |  |
| GET | `/api/mentorship/sent` | `MentorshipController.GetSent` (line 49) |  |
| POST | `/api/mentorship/{id}/complete` | `MentorshipController.MarkComplete` (line 77) |  |
| POST | `/api/mentorship/{id}/respond` | `MentorshipController.Respond` (line 67) |  |
| GET | `/api/messaging/conversations` | `MessagingController.GetConversations` (line 23) |  |
| GET | `/api/messaging/history/{otherUserId}` | `MessagingController.GetChatHistory` (line 33) |  |
| POST | `/api/messaging/mark-read/{messageId}` | `MessagingController.MarkAsRead` (line 55) |  |
| PATCH | `/api/messaging/read/{messageId}` | `MessagingController.MarkAsRead` (line 55) |  |
| GET | `/api/messaging/recent` | `MessagingController.GetConversations` (line 23) |  |
| POST | `/api/messaging/send` | `MessagingController.SendMessage` (line 67) |  |
| GET | `/api/messaging/unread` | `MessagingController.GetUnreadCount` (line 44) |  |
| GET | `/api/networking/member/{id}` | `NetworkingController.GetPublicProfile` (line 33) |  |
| GET | `/api/news/my-submissions` | `NewsController.GetMySubmissions` (line 95) |  |
| POST | `/api/news/upload-image` | `NewsController.UploadImage` (line 164) |  |
| GET | `/api/notifications` | `NotificationController.GetMyNotifications` (line 45) |  |
| POST | `/api/notifications/device-token` | `NotificationController.RegisterDeviceToken` (line 30) |  |
| POST | `/api/notifications/read-all` | `NotificationController.MarkAllAsRead` (line 75) |  |
| POST | `/api/notifications/{id}/read` | `NotificationController.MarkAsRead` (line 63) |  |
| GET | `/api/pending/me/summary` | `PendingApprovalsController.GetMySummary` (line 69) |  |
| GET | `/api/polls/active` | `PollController.GetActivePolls` (line 29) |  |
| GET | `/api/polls/{id}` | `PollController.GetPoll` (line 38) |  |
| POST | `/api/polls/{id}/vote` | `PollController.Vote` (line 48) |  |
| GET | `/api/profile` | `ProfileController.GetProfile` (line 41) |  |
| PUT | `/api/profile` | `ProfileController.UpdateProfile` (line 53) |  |
| GET | `/api/profile/certificate` | `ProfileController.GetCertificate` (line 118) |  |
| GET | `/api/profile/certificate/pdf` | `ProfileController.GetCertificatePdf` (line 128) |  |
| POST | `/api/profile/change-password` | `ProfileController.ChangePassword` (line 65) |  |
| GET | `/api/profile/id-card` | `ProfileController.GetIDCard` (line 98) |  |
| GET | `/api/profile/id-card/pdf` | `ProfileController.GetIDCardPdf` (line 108) |  |
| POST | `/api/profile/photo` | `ProfileController.UploadPhoto` (line 138) |  |
| POST | `/api/profile/signature` | `ProfileController.UploadSignature` (line 157) |  |

### 3.3 Authenticated + in-body Admin

Base policy: any signed-in user (`[Authorize]` or the fallback policy).

| Verb | Path | Action | Rate limit |
|---|---|---|---|
| GET | `/api/events/registration/{id}` | `EventsController.GetRegistrationForInvitation` (line 134) |  |

### 3.4 Authenticated + in-body Admin or SuperAdmin

Base policy: any signed-in user (`[Authorize]` or the fallback policy).

| Verb | Path | Action | Rate limit |
|---|---|---|---|
| POST | `/api/archive/items` | `ArchiveController.CreateItem` (line 63) |  |
| GET | `/api/family-links/{memberId}/family` | `FamilyLinkController.GetPublicFamily` (line 107) |  |
| GET | `/api/financials/membership-history/{memberId}` | `FinancialsController.GetMembershipHistory` (line 193) |  |
| GET | `/api/financials/receipt/{paymentId}` | `FinancialsController.DownloadReceipt` (line 86) |  |
| POST | `/api/jobs` | `JobHubController.PostJob` (line 37) |  |
| PATCH | `/api/jobs/deactivate/{id}` | `JobHubController.DeactivateJob` (line 78) |  |
| DELETE | `/api/jobs/{id}` | `JobHubController.DeactivateJob` (line 78) |  |
| PUT | `/api/jobs/{id}` | `JobHubController.UpdateJob` (line 51) |  |
| POST | `/api/news/submit` | `NewsController.SubmitArticle` (line 107) |  |
| GET | `/api/secure-files/{*filePath}` | `SecureFilesController.GetSecureFile` (line 27) |  |

### 3.5 Authenticated + in-body SuperAdmin

Base policy: any signed-in user (`[Authorize]` or the fallback policy).

| Verb | Path | Action | Rate limit |
|---|---|---|---|
| DELETE | `/api/Forum/posts/{postId}` | `ForumController.DeletePost` (line 106) |  |
| DELETE | `/api/Forum/topics/{topicId}` | `ForumController.DeleteTopic` (line 94) |  |
| GET | `/api/events/my-registrations` | `EventsController.GetMyRegistrations` (line 117) |  |
| GET | `/api/financials/my-dues` | `FinancialsController.GetMyDues` (line 109) |  |
| GET | `/api/financials/my-history` | `FinancialsController.GetMyPaymentHistory` (line 33) |  |

### 3.6 Authenticated + step-up

Base policy: any signed-in user (`[Authorize]` or the fallback policy).

| Verb | Path | Action | Rate limit |
|---|---|---|---|
| DELETE | `/api/auth/authenticator` | `AuthController.RemoveAuthenticator` (line 310) | Auth |
| POST | `/api/auth/authenticator/recovery-codes` | `AuthController.RegenerateRecoveryCodes` (line 296) | Auth |
| POST | `/api/auth/authenticator/setup` | `AuthController.SetupAuthenticator` (line 247) | Auth |
| POST | `/api/elections/{id:int}/vote` | `ElectionsController.Vote` (line 273) |  |
| POST | `/api/me/election-appointments/{id:int}/accept` | `ElectionAppointmentsController.Accept` (line 62) |  |

### 3.7 MemberOnly

Base policy: `MemberOnly`: SuperAdmin, Admin or Member.

| Verb | Path | Action | Rate limit |
|---|---|---|---|
| POST | `/api/gallery` | `GalleryController.SubmitMemberPhoto` (line 164) |  |
| POST | `/api/gallery/albums` | `GalleryController.CreateAlbum` (line 184) |  |
| GET | `/api/gallery/albums/mine` | `GalleryController.GetMyAlbums` (line 194) |  |
| GET | `/api/scholarships/applications/{applicationId:int}/review` | `ScholarshipsController.ReviewApplication` (line 43) |  |
| POST | `/api/scholarships/applications/{applicationId:int}/review` | `ScholarshipsController.SubmitReview` (line 50) |  |
| GET | `/api/scholarships/review-queue` | `ScholarshipsController.ReviewQueue` (line 40) |  |

### 3.8 MemberOnly + in-body Admin or SuperAdmin

Base policy: `MemberOnly`: SuperAdmin, Admin or Member.

| Verb | Path | Action | Rate limit |
|---|---|---|---|
| POST | `/api/gallery/albums/{id}/photos` | `GalleryController.AddPhotoToAlbum` (line 204) |  |

### 3.9 ElectionStaff

Base policy: `ElectionStaff`: SuperAdmin, Admin or ElectionOfficial.

| Verb | Path | Action | Rate limit |
|---|---|---|---|
| GET | `/api/elections/{id:int}/appointments` | `ElectionAppointmentsController.List` (line 18) |  |
| GET | `/api/elections/{id:int}/audit` | `ElectionsController.Audit` (line 111) |  |
| GET | `/api/elections/{id:int}/documents/{formCode}` | `ElectionsController.Document` (line 422) |  |
| GET | `/api/elections/{id:int}/readiness` | `ElectionsController.Readiness` (line 163) |  |

### 3.10 ElectionStaff + step-up

Base policy: `ElectionStaff`: SuperAdmin, Admin or ElectionOfficial.

| Verb | Path | Action | Rate limit |
|---|---|---|---|
| GET | `/api/admin/elections` | `AdminElectionsController.List` (line 22) |  |
| POST | `/api/admin/elections/{id:int}/ballot-key` | `AdminElectionsController.SetBallotKey` (line 102) |  |
| POST | `/api/admin/elections/{id:int}/candidates` | `AdminElectionsController.AddCandidate` (line 136) |  |
| DELETE | `/api/admin/elections/{id:int}/candidates/{candidateId:int}` | `AdminElectionsController.RemoveCandidate` (line 154) |  |
| POST | `/api/admin/elections/{id:int}/close` | `AdminElectionsController.Close` (line 119) |  |
| PUT | `/api/admin/elections/{id:int}/form-details` | `AdminElectionsController.UpdateFormDetails` (line 171) |  |
| GET | `/api/admin/elections/{id:int}/nominations` | `AdminElectionsController.Nominations` (line 183) |  |
| PUT | `/api/admin/elections/{id:int}/nominations/{nominationId:int}/consent` | `AdminElectionsController.RecordConsent` (line 189) |  |
| GET | `/api/admin/elections/{id:int}/nominations/{nominationId:int}/scrutiny-checks` | `AdminElectionsController.GetScrutinyChecks` (line 202) |  |
| PUT | `/api/admin/elections/{id:int}/nominations/{nominationId:int}/scrutiny-checks` | `AdminElectionsController.SetScrutinyCheck` (line 207) |  |
| POST | `/api/admin/elections/{id:int}/publish` | `AdminElectionsController.Publish` (line 95) |  |
| GET | `/api/admin/elections/{id:int}/recounts` | `AdminElectionsController.ListRecounts` (line 220) |  |
| POST | `/api/admin/elections/{id:int}/recounts` | `AdminElectionsController.RecordRecount` (line 225) |  |
| PUT | `/api/admin/elections/{id:int}/recounts/{recountId:int}/decision` | `AdminElectionsController.DecideRecount` (line 235) |  |
| PUT | `/api/admin/elections/{id:int}/recounts/{recountId:int}/figures` | `AdminElectionsController.RecordRecountFigures` (line 245) |  |
| POST | `/api/elections/appointments/{id:int}/revoke` | `ElectionAppointmentsController.Revoke` (line 42) |  |
| POST | `/api/elections/approvals/{approvalId:int}/approve` | `ElectionsController.Approve` (line 380) |  |
| POST | `/api/elections/approvals/{approvalId:int}/reject` | `ElectionsController.Reject` (line 410) |  |
| POST | `/api/elections/nominations/{nominationId:int}/scrutiny` | `ElectionsController.Scrutinise` (line 252) |  |
| POST | `/api/elections/{id:int}/appointments` | `ElectionAppointmentsController.Appoint` (line 30) |  |
| GET | `/api/elections/{id:int}/approvals` | `ElectionsController.Approvals` (line 347) |  |
| POST | `/api/elections/{id:int}/count` | `ElectionsController.Count` (line 306) |  |
| POST | `/api/elections/{id:int}/declare` | `ElectionsController.Declare` (line 333) |  |
| POST | `/api/elections/{id:int}/key-destroyed` | `ElectionsController.KeyDestroyed` (line 361) |  |
| POST | `/api/elections/{id:int}/phase` | `ElectionsController.SetPhase` (line 143) |  |
| POST | `/api/elections/{id:int}/readiness/attest` | `ElectionsController.AttestReadiness` (line 171) |  |
| POST | `/api/elections/{id:int}/seats` | `ElectionsController.AddSeat` (line 190) |  |
| POST | `/api/elections/{id:int}/voter-roll/freeze` | `ElectionsController.FreezeRoll` (line 207) |  |
| POST | `/api/elections/{id:int}/voter-roll/unfreeze` | `ElectionsController.UnfreezeRoll` (line 215) |  |

### 3.11 AdminOnly

Base policy: `AdminOnly`: SuperAdmin or Admin.

| Verb | Path | Action | Rate limit |
|---|---|---|---|
| POST | `/api/Theme` | `ThemeController.CreateTheme` (line 39) |  |
| GET | `/api/Theme/all` | `ThemeController.GetAllThemes` (line 32) |  |
| DELETE | `/api/Theme/{id}` | `ThemeController.DeleteTheme` (line 69) |  |
| PUT | `/api/Theme/{id}` | `ThemeController.UpdateTheme` (line 54) |  |
| GET | `/api/activity/admin/{memberId}` | `ActivityController.GetMemberActivity` (line 34) |  |
| GET | `/api/admin/comm/logs` | `CommunicationController.GetLogs` (line 32) |  |
| GET | `/api/admin/comm/member/{memberId:int}` | `CommunicationController.GetMemberLogs` (line 39) |  |
| POST | `/api/admin/comm/send-batch` | `CommunicationController.SendBatch` (line 68) |  |
| POST | `/api/admin/comm/send-custom` | `CommunicationController.SendCustom` (line 88) |  |
| POST | `/api/admin/comm/send-type` | `CommunicationController.SendType` (line 78) |  |
| GET | `/api/admin/comm/templates` | `CommunicationController.GetTemplates` (line 25) |  |
| POST | `/api/admin/comm/templates` | `CommunicationController.CreateTemplate` (line 54) |  |
| DELETE | `/api/admin/comm/templates/{id}` | `CommunicationController.DeleteTemplate` (line 61) |  |
| PUT | `/api/admin/comm/templates/{id}` | `CommunicationController.UpdateTemplate` (line 46) |  |
| GET | `/api/admin/contact-messages` | `AdminController.GetContactMessages` (line 321) |  |
| DELETE | `/api/admin/contact-messages/{id}` | `AdminController.DeleteContactMessage` (line 336) |  |
| POST | `/api/admin/contact-messages/{id}/read` | `AdminController.MarkMessageAsRead` (line 328) |  |
| POST | `/api/admin/governance/members/{ecMemberId}/end-term` | `AdminGovernanceController.EndTerm` (line 86) |  |
| GET | `/api/admin/governance/periods` | `AdminGovernanceController.GetPeriods` (line 25) |  |
| POST | `/api/admin/governance/periods` | `AdminGovernanceController.CreatePeriod` (line 32) |  |
| GET | `/api/admin/governance/periods/{id:int}/handover-checks` | `AdminGovernanceController.GetHandoverChecks` (line 182) |  |
| PUT | `/api/admin/governance/periods/{id:int}/handover-checks` | `AdminGovernanceController.SetHandoverCheck` (line 186) |  |
| PUT | `/api/admin/governance/periods/{id}` | `AdminGovernanceController.UpdatePeriod` (line 39) |  |
| POST | `/api/admin/governance/periods/{id}/activate` | `AdminGovernanceController.ActivatePeriod` (line 47) |  |
| GET | `/api/admin/governance/periods/{id}/members` | `AdminGovernanceController.GetCommitteeMembers` (line 55) |  |
| POST | `/api/admin/governance/periods/{id}/members` | `AdminGovernanceController.AssignMember` (line 69) |  |
| GET | `/api/admin/governance/periods/{id}/seats` | `AdminGovernanceController.GetCommitteeSeats` (line 62) |  |
| GET | `/api/admin/governance/positions` | `AdminGovernanceController.GetPositions` (line 102) |  |
| POST | `/api/admin/governance/positions` | `AdminGovernanceController.CreatePosition` (line 106) |  |
| GET | `/api/admin/governance/positions/invalid-rows` | `AdminGovernanceController.GetInvalidPositionRows` (line 152) |  |
| POST | `/api/admin/governance/positions/repair` | `AdminGovernanceController.RepairPosition` (line 156) |  |
| DELETE | `/api/admin/governance/positions/{id:int}` | `AdminGovernanceController.DeletePosition` (line 139) |  |
| PUT | `/api/admin/governance/positions/{id:int}` | `AdminGovernanceController.UpdatePosition` (line 119) |  |
| POST | `/api/admin/governance/positions/{id:int}/restore` | `AdminGovernanceController.RestorePosition` (line 136) |  |
| POST | `/api/admin/governance/positions/{id:int}/retire` | `AdminGovernanceController.RetirePosition` (line 133) |  |
| POST | `/api/admin/members/import` | `MemberImportController.Import` (line 26) |  |
| GET | `/api/admin/members/import/export` | `MemberImportController.Export` (line 42) |  |
| POST | `/api/admin/members/{id}/approve` | `AdminController.ApproveMember` (line 84) |  |
| GET | `/api/admin/members/{id}/certificate` | `AdminController.GetMemberCertificate` (line 307) |  |
| GET | `/api/admin/members/{id}/certificate/pdf` | `AdminController.GetMemberCertificatePdf` (line 314) |  |
| GET | `/api/admin/members/{id}/documents` | `AdminController.GetDocuments` (line 76) |  |
| PATCH | `/api/admin/members/{id}/documents` | `AdminController.UpdateMemberDocuments` (line 232) |  |
| GET | `/api/admin/members/{id}/id-card` | `AdminController.GetMemberIDCard` (line 268) |  |
| GET | `/api/admin/members/{id}/id-card/pdf` | `AdminController.GetMemberIDCardPdf` (line 275) |  |
| POST | `/api/admin/members/{id}/photo` | `AdminController.UpdateMemberPhoto` (line 212) |  |
| POST | `/api/admin/members/{id}/reactivate` | `AdminController.ReactivateMember` (line 175) |  |
| POST | `/api/admin/members/{id}/reject` | `AdminController.RejectMember` (line 128) |  |
| POST | `/api/admin/members/{id}/revert-approval` | `AdminController.RevertMemberApproval` (line 111) |  |
| POST | `/api/admin/members/{id}/signature` | `AdminController.UpdateMemberSignature` (line 222) |  |
| GET | `/api/admin/polls` | `AdminPollController.GetAllPolls` (line 29) |  |
| POST | `/api/admin/polls` | `AdminPollController.CreatePoll` (line 36) |  |
| DELETE | `/api/admin/polls/{id}` | `AdminPollController.DeletePoll` (line 52) |  |
| PUT | `/api/admin/polls/{id}/toggle` | `AdminPollController.ToggleStatus` (line 44) |  |
| GET | `/api/admin/social-auth` | `AdminSocialAuthController.GetConfigs` (line 23) |  |
| PUT | `/api/admin/social-auth/{provider}` | `AdminSocialAuthController.UpdateConfig` (line 38) |  |
| POST | `/api/admin/social-auth/{provider}/toggle` | `AdminSocialAuthController.Toggle` (line 53) |  |
| GET | `/api/archive/admin/collections` | `ArchiveController.AdminCollections` (line 37) |  |
| POST | `/api/archive/admin/collections` | `ArchiveController.CreateCollection` (line 42) |  |
| DELETE | `/api/archive/admin/collections/{id:int}` | `ArchiveController.DeleteCollection` (line 58) |  |
| PUT | `/api/archive/admin/collections/{id:int}` | `ArchiveController.UpdateCollection` (line 50) |  |
| GET | `/api/archive/admin/items` | `ArchiveController.AdminItems` (line 32) |  |
| DELETE | `/api/archive/admin/items/{id:int}` | `ArchiveController.DeleteItem` (line 85) |  |
| PUT | `/api/archive/admin/items/{id:int}` | `ArchiveController.UpdateItem` (line 77) |  |
| POST | `/api/archive/admin/items/{id:int}/moderate` | `ArchiveController.ModerateItem` (line 90) |  |
| POST | `/api/campaigns/admin` | `CampaignsController.Create` (line 89) |  |
| PUT | `/api/campaigns/admin` | `CampaignsController.Update` (line 107) |  |
| GET | `/api/campaigns/admin/all` | `CampaignsController.GetAllForAdmin` (line 82) |  |
| POST | `/api/campaigns/admin/pledges/confirm-receipt` | `CampaignsController.ConfirmReceipt` (line 129) |  |
| GET | `/api/campaigns/admin/tiers` | `CampaignsController.GetTiers` (line 140) |  |
| POST | `/api/campaigns/admin/tiers` | `CampaignsController.CreateTier` (line 147) |  |
| GET | `/api/campaigns/admin/{campaignId}/pledges` | `CampaignsController.GetPledgesForAdmin` (line 122) |  |
| POST | `/api/election-content` | `ElectionContentController.Create` (line 59) |  |
| GET | `/api/election-content/admin` | `ElectionContentController.GetAll` (line 44) |  |
| GET | `/api/election-content/admin/{id:int}` | `ElectionContentController.GetById` (line 51) |  |
| PUT | `/api/election-content/{id:int}` | `ElectionContentController.Update` (line 70) |  |
| GET | `/api/elections/forms/{code}/blank` | `ElectionsController.BlankForm` (line 437) |  |
| POST | `/api/events/admin` | `EventsController.CreateEvent` (line 170) |  |
| GET | `/api/events/admin/all` | `EventsController.GetAllEventsForAdmin` (line 162) |  |
| POST | `/api/events/admin/approve-registration` | `EventsController.ApproveRegistration` (line 226) |  |
| POST | `/api/events/admin/budget` | `EventsController.UpdateBudget` (line 299) |  |
| POST | `/api/events/admin/checkin/qr` | `EventsController.QRCodeCheckIn` (line 248) |  |
| POST | `/api/events/admin/expenses` | `EventsController.AddExpense` (line 307) |  |
| DELETE | `/api/events/admin/expenses/{id}` | `EventsController.DeleteExpense` (line 315) |  |
| GET | `/api/events/admin/registrations` | `EventsController.GetAllRegistrations` (line 218) |  |
| POST | `/api/events/admin/registrations/{id}/send-invitation` | `EventsController.SendInvitation` (line 240) |  |
| POST | `/api/events/admin/tasks` | `EventsController.CreateTask` (line 267) |  |
| DELETE | `/api/events/admin/tasks/{id}` | `EventsController.DeleteTask` (line 283) |  |
| POST | `/api/events/admin/tasks/{id}/toggle` | `EventsController.ToggleTask` (line 275) |  |
| GET | `/api/events/admin/{eventId}/budget` | `EventsController.GetEventBudget` (line 291) |  |
| GET | `/api/events/admin/{eventId}/tasks` | `EventsController.GetEventTasks` (line 259) |  |
| DELETE | `/api/events/admin/{id}` | `EventsController.DeleteEvent` (line 187) |  |
| PUT | `/api/events/admin/{id}` | `EventsController.UpdateEvent` (line 178) |  |
| POST | `/api/events/admin/{id}/logo` | `EventsController.UploadEventLogo` (line 195) |  |
| POST | `/api/financials/dues/generate` | `FinancialsController.GenerateAnnualDues` (line 136) |  |
| GET | `/api/financials/member/{memberId}/history` | `FinancialsController.GetMemberPaymentHistory` (line 221) |  |
| DELETE | `/api/financials/payment/{id}` | `FinancialsController.DeletePayment` (line 208) |  |
| PATCH | `/api/financials/update-status/{id}` | `FinancialsController.UpdateStatus` (line 79) |  |
| POST | `/api/gallery/admin` | `GalleryController.CreateGallery` (line 106) |  |
| GET | `/api/gallery/admin/pending` | `GalleryController.GetPendingApprovals` (line 229) |  |
| DELETE | `/api/gallery/admin/photos/{photoId}` | `GalleryController.RemovePhoto` (line 154) |  |
| DELETE | `/api/gallery/admin/{id}` | `GalleryController.DeleteGallery` (line 146) |  |
| PUT | `/api/gallery/admin/{id}` | `GalleryController.UpdateGallery` (line 120) |  |
| POST | `/api/gallery/admin/{id}/approve` | `GalleryController.ApproveGallery` (line 238) |  |
| POST | `/api/gallery/admin/{id}/photos` | `GalleryController.AddPhotos` (line 138) |  |
| POST | `/api/gallery/admin/{id}/reject` | `GalleryController.RejectGallery` (line 246) |  |
| PATCH | `/api/gallery/admin/{id}/toggle-active` | `GalleryController.ToggleActive` (line 72) |  |
| PATCH | `/api/gallery/admin/{id}/toggle-featured` | `GalleryController.ToggleFeatured` (line 84) |  |
| GET | `/api/gallery/all` | `GalleryController.GetAllGalleries` (line 63) |  |
| POST | `/api/gallery/photos/{photoId}/approve` | `GalleryController.ApprovePhoto` (line 254) |  |
| POST | `/api/gallery/photos/{photoId}/reject` | `GalleryController.RejectPhoto` (line 262) |  |
| POST | `/api/gallery/upload-photo` | `GalleryController.UploadPhoto` (line 29) |  |
| GET | `/api/jobs/admin/pending` | `JobHubController.GetPendingJobs` (line 97) |  |
| POST | `/api/jobs/admin/{id}/approve` | `JobHubController.ApproveJob` (line 105) |  |
| POST | `/api/jobs/admin/{id}/reject` | `JobHubController.RejectJob` (line 113) |  |
| POST | `/api/lookups` | `LookupsController.CreateLookup` (line 53) |  |
| DELETE | `/api/lookups/{id}` | `LookupsController.DeleteLookup` (line 70) |  |
| PUT | `/api/lookups/{id}` | `LookupsController.UpdateLookup` (line 61) |  |
| GET | `/api/mentorship/admin/all` | `MentorshipController.GetAllForAdmin` (line 88) |  |
| POST | `/api/mentorship/admin/{id}/close` | `MentorshipController.AdminClose` (line 96) |  |
| POST | `/api/news` | `NewsController.CreateNews` (line 64) |  |
| GET | `/api/news/News/Pending` | `NewsController.GetPendingSubmissions` (line 56) |  |
| GET | `/api/news/admin` | `NewsController.GetAllNewsForAdmin` (line 46) |  |
| GET | `/api/news/admin/pending` | `NewsController.GetPendingSubmissions` (line 56) |  |
| POST | `/api/news/admin/{id:int}/approve` | `NewsController.ApproveArticle` (line 131) |  |
| POST | `/api/news/admin/{id:int}/reject` | `NewsController.RejectArticle` (line 140) |  |
| GET | `/api/news/pending` | `NewsController.GetPendingSubmissions` (line 56) |  |
| POST | `/api/news/upload-document` | `NewsController.UploadDocument` (line 197) |  |
| DELETE | `/api/news/{id:int}` | `NewsController.DeleteNews` (line 87) |  |
| PUT | `/api/news/{id:int}` | `NewsController.UpdateNews` (line 78) |  |
| POST | `/api/news/{id:int}/approve` | `NewsController.ApproveArticle` (line 131) |  |
| DELETE | `/api/news/{id:int}/collaborators/{userId:int}` | `NewsController.RemoveCollaborator` (line 156) |  |
| POST | `/api/news/{id:int}/collaborators/{userId:int}` | `NewsController.AddCollaborator` (line 148) |  |
| POST | `/api/news/{id:int}/reject` | `NewsController.RejectArticle` (line 140) |  |
| GET | `/api/pending/admin/summary` | `PendingApprovalsController.GetAdminSummary` (line 45) |  |
| GET | `/api/scholarships/admin/applications` | `ScholarshipsController.AdminApplications` (line 58) |  |
| POST | `/api/scholarships/admin/awards` | `ScholarshipsController.CreateAward` (line 72) |  |
| POST | `/api/scholarships/admin/awards/{awardId:int}/disburse` | `ScholarshipsController.Disburse` (line 79) |  |
| POST | `/api/scholarships/admin/calls` | `ScholarshipsController.CreateCall` (line 64) |  |
| POST | `/api/scholarships/admin/funds` | `ScholarshipsController.CreateFund` (line 61) |  |
| POST | `/api/site-content` | `SiteContentController.Create` (line 49) |  |
| GET | `/api/site-content/admin` | `SiteContentController.GetAll` (line 33) |  |
| DELETE | `/api/site-content/{id:int}` | `SiteContentController.Delete` (line 76) |  |
| GET | `/api/site-content/{id:int}` | `SiteContentController.GetById` (line 41) |  |
| PUT | `/api/site-content/{id:int}` | `SiteContentController.Update` (line 59) |  |
| POST | `/api/verify/{shortCode}/revoke` | `CredentialVerificationController.Revoke` (line 29) |  |

### 3.12 AdminOnly + in-body SuperAdmin

Base policy: `AdminOnly`: SuperAdmin or Admin.

| Verb | Path | Action | Rate limit |
|---|---|---|---|
| GET | `/api/admin/members` | `AdminController.GetAllMembers` (line 45) |  |
| GET | `/api/admin/members/{id}` | `AdminController.GetMemberById` (line 67) |  |
| PUT | `/api/admin/members/{id}` | `AdminController.UpdateMemberAdmin` (line 183) |  |
| GET | `/api/admin/stats` | `AdminController.GetStats` (line 29) |  |

### 3.13 AdminOnly + step-up

Base policy: `AdminOnly`: SuperAdmin or Admin.

| Verb | Path | Action | Rate limit |
|---|---|---|---|
| POST | `/api/admin/elections` | `AdminElectionsController.Create` (line 43) |  |
| DELETE | `/api/admin/governance/members/{ecMemberId}/hard-delete` | `AdminGovernanceController.DeleteECMember` (line 207) |  |
| POST | `/api/admin/sync-members` | `AdminController.SyncMembers` (line 38) |  |
| POST | `/api/elections` | `ElectionsController.Create` (line 27) |  |

### 3.14 AdminOnly + step-up + in-body SuperAdmin

Base policy: `AdminOnly`: SuperAdmin or Admin.

| Verb | Path | Action | Rate limit |
|---|---|---|---|
| POST | `/api/admin/members/{id}/reset-password-admin` | `AdminController.ResetPasswordAdmin` (line 283) |  |

### 3.15 SuperAdminOnly

Base policy: `SuperAdminOnly`: SuperAdmin only.

| Verb | Path | Action | Rate limit |
|---|---|---|---|
| GET | `/api/activity/admin/global` | `ActivityController.GetGlobalActivity` (line 42) |  |
| GET | `/api/admin/dev-tracker` | `AdminDevTrackerController.GetOpenItems` (line 24) |  |
| GET | `/api/admin/election-personas` | `ElectionPersonasController.List` (line 16) |  |
| GET | `/api/admin/error-logs` | `AdminErrorLogsController.GetErrorLogs` (line 22) |  |
| POST | `/api/admin/members/{id}/restore` | `AdminController.RestoreMember` (line 167) |  |
| PUT | `/api/config` | `OrgConfigController.UpdateConfig` (line 24) |  |
| GET | `/api/financials/fees/config` | `FinancialsController.GetFeeConfigs` (line 144) |  |
| POST | `/api/financials/fees/config` | `FinancialsController.AddFeeConfig` (line 152) |  |
| PUT | `/api/financials/fees/config` | `FinancialsController.UpdateFeeConfig` (line 173) |  |
| GET | `/api/ledger` | `FinancialLedgerController.GetRecords` (line 23) |  |
| GET | `/api/ledger/export/csv` | `FinancialLedgerController.ExportCsv` (line 79) |  |
| GET | `/api/ledger/summary` | `FinancialLedgerController.GetSummary` (line 30) |  |
| POST | `/api/payment-config/admin` | `PaymentConfigController.CreateConfig` (line 75) |  |
| POST | `/api/payment-config/admin/seed-defaults` | `PaymentConfigController.SeedDefaults` (line 157) |  |
| POST | `/api/payment-config/admin/{id}/toggle` | `PaymentConfigController.ToggleConfig` (line 109) |  |
| GET | `/api/roles` | `RolesController.GetRoles` (line 75) |  |
| GET | `/api/roles/users` | `RolesController.GetUsers` (line 30) |  |

### 3.16 SuperAdminOnly + in-body SuperAdmin

Base policy: `SuperAdminOnly`: SuperAdmin only.

| Verb | Path | Action | Rate limit |
|---|---|---|---|
| GET | `/api/payment-config/admin/all` | `PaymentConfigController.GetAllConfigs` (line 53) |  |
| PUT | `/api/payment-config/admin/{id}` | `PaymentConfigController.UpdateConfig` (line 91) |  |

### 3.17 SuperAdminOnly + step-up

Base policy: `SuperAdminOnly`: SuperAdmin only.

| Verb | Path | Action | Rate limit |
|---|---|---|---|
| POST | `/api/admin/election-personas` | `ElectionPersonasController.Create` (line 21) |  |
| DELETE | `/api/admin/election-personas/{id:int}` | `ElectionPersonasController.Delete` (line 49) |  |
| PUT | `/api/admin/election-personas/{id:int}` | `ElectionPersonasController.Update` (line 30) |  |
| POST | `/api/admin/election-personas/{id:int}/active` | `ElectionPersonasController.SetActive` (line 44) |  |
| GET | `/api/admin/elections/rules-unlock` | `ElectionRulesUnlockController.Get` (line 23) |  |
| POST | `/api/admin/elections/rules-unlock` | `ElectionRulesUnlockController.Open` (line 30) |  |
| POST | `/api/admin/elections/rules-unlock/close` | `ElectionRulesUnlockController.Close` (line 50) |  |
| POST | `/api/admin/members/bulk-archive-inactive` | `AdminController.BulkArchiveInactive` (line 159) |  |
| DELETE | `/api/admin/members/{id}` | `AdminController.ArchiveMember` (line 149) |  |
| POST | `/api/elections/appointments/{appointmentId:int}/emergency-revoke` | `ElectionsController.EmergencyRevoke` (line 397) |  |
| POST | `/api/ledger` | `FinancialLedgerController.AddRecord` (line 38) |  |
| DELETE | `/api/ledger/{id}` | `FinancialLedgerController.DeleteRecord` (line 69) |  |
| PUT | `/api/ledger/{id}` | `FinancialLedgerController.UpdateRecord` (line 54) |  |
| DELETE | `/api/payment-config/admin/{id}` | `PaymentConfigController.DeleteConfig` (line 120) |  |
| POST | `/api/roles` | `RolesController.CreateRole` (line 83) |  |
| POST | `/api/roles/assign` | `RolesController.AssignRole` (line 91) |  |
| POST | `/api/roles/remove` | `RolesController.RemoveRole` (line 100) |  |
| POST | `/api/roles/users` | `RolesController.CreateAdmin` (line 47) |  |
| DELETE | `/api/roles/users/{id}` | `RolesController.DeleteUser` (line 109) |  |
| DELETE | `/api/roles/users/{id}/authenticator` | `RolesController.ResetAuthenticator` (line 151) |  |
| POST | `/api/roles/users/{id}/disable` | `RolesController.DisableUser` (line 120) |  |
| POST | `/api/roles/users/{id}/enable` | `RolesController.EnableUser` (line 129) |  |
| POST | `/api/roles/users/{id}/reset-password-admin` | `RolesController.ResetPasswordAdmin` (line 138) |  |

## 4. Findings

- `AdminOnly` is written two ways: `Constants.Policies.AdminOnly` and `Policies.AdminOnly` (with a
  `using static`). Both resolve to the same constant, so this is a style gap, not a bug.
- Fixed since the 2026-09-26 review: in-body role checks used the literal strings `"Admin"` and
  `"SuperAdmin"` in 24 routes and in `NotificationHub`; they now use `Constants.Roles`.
  `FinancialsController.DownloadReceipt` called `Forbid(string)`, which takes a scheme name and so
  gave a 500; it now calls `Forbid()` (`FinancialsController.cs:102` explains why). No test read
  the attributes across all controllers; `AuthorizationPolicyReflectionTests` now does (84.44).

## 5. Tests that pin authorization

| Test file | What it pins |
|---|---|
| `GHCAA.Tests/Controllers/AuthorizationPolicyReflectionTests.cs` | every action's effective policy, `[AllowAnonymous]` and `[RequireStepUp]`, read by reflection and compared with a list kept in step with section 3 (84.44) |
| `GHCAA.Tests/Filters/RequireStepUpAttributeTests.cs` | step-up denied when the claim is missing, not a number or expired; allowed when recent; TTL from config; body carries `STEP_UP_REQUIRED` |
| `GHCAA.Tests/Controllers/DestructiveStepUpActionsTests.cs` | results of two step-up actions (`RolesController.DeleteUser`, `AdminGovernanceController.DeleteECMember`), including 401 with no caller id. It does not check that the attribute is present |
| `GHCAA.Tests/Controllers/AuthControllerMutationTests.cs` | 401 on failed Google and Facebook login and on missing, invalid, inactive or archived refresh; step-up request and verify |
| `GHCAA.Tests/Controllers/FinancialsControllerTests.cs` | receipt ownership check (see the `Forbid(string)` finding above) |
| `GHCAA.Tests/Controllers/GalleryControllerTests.cs` | 403 when a non-owner, non-admin adds a photo; 401 with no user id claim |
| `GHCAA.Tests/Controllers/NewsControllerTests.cs` | 403 when a member submits a notice |
| `GHCAA.Tests/Controllers/SecureFilesControllerTests.cs` | 403 for a non-owner, non-admin file read |
| `GHCAA.Tests/Controllers/PendingApprovalsControllerTests.cs` | 401 with no member claim |
| `GHCAA.Tests/Controllers/PollControllerTests.cs` | 401 on member actions with no member claim |
| `GHCAA.Web/src/app/core/interceptors/step-up-interceptor.spec.ts`, `step-up.service.spec.ts` | the web client prompts on `STEP_UP_REQUIRED` and retries |

Apart from the reflection test, these are unit tests that call the controller directly. The
reflection test proves each attribute is present. That the framework then enforces it is left to
ASP.NET.
