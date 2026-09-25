# Feature Specification: Communication

**Feature Branch**: `017-communication`
**Created**: 25-09-2026
**Status**: As-built baseline
**Input**: Reverse-engineered from the implemented code.
**Depends on**: spec 012, the membership lifecycle and auth baseline (member identity, roles, claims)

## Purpose and scope

This spec covers direct messaging, admin broadcast communication, a member's own communication history, in-app notifications, the discussion forum, and the public contact form, as built across six controllers in `GHCAA.API/Controllers`:

- `MessagingController` - routes `api/messaging` and its alias `api/chat`. Direct member-to-member chat.
- `CommunicationController` - route `api/admin/comm`. Admin-only email/SMS template management and batch or custom broadcast sending.
- `MemberCommunicationsController` - route `api/communications`. A member's self-service view of their own communication log.
- `NotificationController` - routes `api/notifications` and its alias `api/notification`. In-app notifications and FCM device token registration.
- `ForumController` - route `api/Forum`. Categories, topics, and posts.
- `ContactController` - route `api/contact`. The public enquiry form.

### Out of scope

- Reviewing, marking read, and deleting contact enquiries is exposed on `AdminController` (`GET/POST/DELETE api/admin/contact-messages`, `GHCAA.API/Controllers/AdminController.cs:326-347`), not on `ContactController`. `ContactController` only accepts new submissions. `AdminController` is not one of the six controllers in scope for this spec and is not traced further here.
- Real-time delivery (`IRealTimeService`, the SignalR hubs at `api/hubs/chat` and `api/hubs/notifications`) is a transport detail called by `ChatService` and `NotificationService`; it has no controller of its own and is noted only where it affects an FR.
- Sending a push notification to a registered FCM device token is not built. `NotificationController.RegisterDeviceToken` only persists the token (`GHCAA.Infrastructure/Services/DeviceTokenService.cs:1-10`, comment references work package 82.53a).
- `GHCAA.Mobile/lib/screens/member/ai_chat_screen.dart` was not traced as part of this domain; it appears to serve the AI assistant feature (`api/assistant/ask`, see the untracked spec 019, the assistant workflow) rather than `MessagingController`. Not read in this pass to confirm.

## User Scenarios & Testing

### User Story 1 - Member exchanges direct messages (Priority: P1)

**Why this priority**: Direct messaging is the core interaction `MessagingController` and `ChatService` exist for; every other action on the controller depends on a working send/receive path.

**Independent Test**: As an authenticated member, send a message to another member, then fetch the chat history between the two and confirm the message appears.

**Acceptance Scenarios**:

1. **Given** an authenticated member with no prior history with another member, **When** they POST `api/messaging/send` with `{ReceiverId, Content}`, **Then** the message is persisted and a new conversation thread exists for both users.
2. **Given** a member who is the receiver of an unread message, **When** they POST `api/messaging/mark-read/{messageId}` (or PATCH `api/messaging/read/{messageId}`), **Then** the message is marked read.
3. **Given** a member who is not the receiver of a message, **When** they attempt to mark it read, **Then** the message stays unread and the call reports failure.

### User Story 2 - Admin manages templates and sends broadcast communication (Priority: P1)

**Why this priority**: This is the only path to reach members in bulk (fee reminders, event updates); a broken template or send path affects every member at once.

**Independent Test**: As an admin, create a template, send a batch email to a graduating year, and confirm the batch is rejected when no year is supplied.

**Acceptance Scenarios**:

1. **Given** an admin, **When** they POST `api/admin/comm/templates` with a new template, **Then** the template is created and returned.
2. **Given** an admin, **When** they POST `api/admin/comm/send-batch` with no `PassingYear` or `PassingYears`, **Then** the API returns 400 with detail "At least one PassingYear is required".
3. **Given** an admin, **When** they POST `api/admin/comm/send-custom` with `Channel: "email"` and a list of `Emails`, **Then** the emails are queued and the response names the channel used.
4. **Given** an admin, **When** they POST `api/admin/comm/send-custom` with `TargetMethod: "batch"` and no `TargetValue`/`TargetValues`, **Then** the API returns 400.

### User Story 3 - Member views their own communication history (Priority: P2)

**Why this priority**: Lets a member self-serve "did I get that email" without contacting the secretariat; shares its query logic with the admin view in Story 2.

**Independent Test**: As a member who has received at least one communication, GET `api/communications/me` and confirm only that member's rows come back.

**Acceptance Scenarios**:

1. **Given** an authenticated member, **When** they GET `api/communications/me`, **Then** they receive a paged list containing only their own communication log rows.
2. **Given** a request whose member id claim is missing or unparseable, **When** it reaches `GetMine`, **Then** the API returns 401 with detail "Member profile is required."

### User Story 4 - Member manages in-app notifications (Priority: P1)

**Why this priority**: In-app notifications are the one delivery channel every member sees on login, gated by their own preference flags, and shared by several other domains (events, elections, payments) that call into `NotificationService`.

**Independent Test**: As a member with `NotifyEventCreation` enabled, trigger an event-creation notification and confirm it appears in their list; with the flag disabled, confirm it does not.

**Acceptance Scenarios**:

1. **Given** a member with a relevant preference flag enabled, **When** a matching event occurs, **Then** a notification is created and delivered in real time.
2. **Given** a member with the flag disabled, **When** the same event occurs, **Then** no notification is created for that member.
3. **Given** a member with an unread notification belonging to someone else, **When** they POST `api/notifications/{id}/read` for that id, **Then** the row is not updated.
4. **Given** a member with several unread notifications, **When** they POST `api/notifications/read-all`, **Then** all of their unread notifications are marked read in one call.
5. **Given** a mobile client on app start, **When** it POSTs `api/notifications/device-token` with a non-empty token, **Then** the token is saved to the member's profile, overwriting any previous token.

### User Story 5 - Member uses the discussion forum (Priority: P2)

**Why this priority**: A secondary community feature; broken forum moderation is a content-quality and privacy risk (unsanitised HTML, orphaned topics) rather than a blocker to core membership operations.

**Independent Test**: As a member, create a topic under an active category, reply to it, then delete the topic as its author and confirm it disappears from the topic list.

**Acceptance Scenarios**:

1. **Given** a member and an active category, **When** they POST `api/Forum/topics` with `{CategoryId, Title, Content}`, **Then** the topic is created with its content HTML-sanitised.
2. **Given** an inactive or missing category, **When** a member tries to create a topic under it, **Then** the create fails.
3. **Given** a member replying to a topic, **When** the post body's `TopicId` differs from the route `{topicId}`, **Then** the API returns 400 "Topic ID mismatch."
4. **Given** a topic under a category that has since been deactivated, **When** its author or a SuperAdmin deletes it, **Then** the delete still succeeds (the lookup bypasses the active-category filter).
5. **Given** a member who is neither the topic's author nor a SuperAdmin, **When** they try to delete it, **Then** the delete is rejected.

### User Story 6 - Visitor submits a contact enquiry (Priority: P3)

**Why this priority**: Public-facing but low-frequency and non-blocking; failure to notify the secretariat by email does not stop the enquiry from being recorded.

**Independent Test**: As an anonymous visitor, POST a valid enquiry and confirm a fixed confirmation message comes back.

**Acceptance Scenarios**:

1. **Given** an anonymous visitor, **When** they POST `api/contact` with `{FullName, Email, Subject, Message}` all within their length limits, **Then** the enquiry is saved and the response reads "Your enquiry has been filed in the secretariat records."

### Edge Cases

- Reading a forum topic (`GET api/Forum/topics/{topicId}`) increments its view count on every call, including repeat reads by the same member in the same session; there is no dedupe or rate limit (`GHCAA.Infrastructure/Services/ForumService.cs`, `GetTopicByIdAsync`).
- `ForumService.CreateTopicAsync`/`CreatePostAsync` throw an untyped `Exception` for "Invalid or inactive category.", "Topic not found or is inactive.", and "Topic is locked and cannot receive new posts."; `ForumController` does not catch these, so they surface as unhandled 500s rather than 400s.
- `CommunicationService`'s `SendEmailByCodeAsync` and `SendTemplatedEmailAsync` log a warning and return silently when the template code is not found; the HTTP caller gets no error.
- `ContactService.SubmitMessageAsync` wraps its notification-email loop in a try/catch whose comment says "Log failure but don't block submission", but the catch block is empty; a failed recipient email is neither logged nor surfaced (`GHCAA.Infrastructure/Services/ContactService.cs`).
- `NotificationService.GetUserNotificationsAsync` hardcodes `Take(50)` with no pagination parameter, unlike the communication log endpoints which accept `page`/`pageSize`.
- `MessagingController.GetChatHistory` defaults to the 50 most recent messages when no count is supplied (`GHCAA.API/Controllers/MessagingController.cs`).

## Requirements

### Functional Requirements

- FR-001: An authenticated member shall retrieve their recent conversation list via GET `api/messaging/recent` (alias `api/chat/recent`). [code]
- FR-002: An authenticated member shall retrieve chat history with another member via GET `api/messaging/history/{otherUserId}`, returning the 50 most recent messages by default. [code]
- FR-003: An authenticated member shall retrieve their unread message count via GET `api/messaging/unread`. [code]
- FR-004: The receiver of a message shall mark it read via POST `api/messaging/mark-read/{messageId}` or PATCH `api/messaging/read/{messageId}`; the system shall leave the message unread and report failure when the caller is not the receiver. [code+test]
- FR-005: An authenticated member shall send a direct message via POST `api/messaging/send` with body `{ReceiverId, Content}`. [code+test]
- FR-006: An admin shall retrieve all email and SMS templates via GET `api/admin/comm/templates`, receiving database templates merged with the system-default fallback set for any code not overridden. [code]
- FR-007: An admin shall retrieve the most recent communication log entries via GET `api/admin/comm/logs`, capped at a `count` query parameter defaulting to 100. [code]
- FR-008: An admin shall retrieve a specific member's communication log, paged, via GET `api/admin/comm/member/{memberId}` with `page` defaulting to 1 and `pageSize` to 25. [code]
- FR-009: An admin shall update an existing template via PUT `api/admin/comm/templates/{id}`. [code+test]
- FR-010: An admin shall create a new template via POST `api/admin/comm/templates`. [code+test]
- FR-011: An admin shall delete a template via DELETE `api/admin/comm/templates/{id}`, receiving 204 on success. [code+test]
- FR-012: An admin shall send a templated batch email to one or more graduating-year batches via POST `api/admin/comm/send-batch` with `BulkEmailDto{PassingYears or PassingYear, TemplateCode, CustomVars}`; the system shall return 400 with detail "At least one PassingYear is required" when neither is supplied. [code+test]
- FR-013: An admin shall send a templated email to one or more membership types via POST `api/admin/comm/send-type` with `BulkEmailDto{MembershipTypes or MembershipType, TemplateCode, CustomVars}`; the system shall return 400 with detail "At least one MembershipType is required" when neither is supplied. [code+test]
- FR-014: An admin shall send a custom, non-templated broadcast via POST `api/admin/comm/send-custom` with `CustomEmailDto{Channel: "email"|"push"|"both", TargetMethod: "batch"|"type"|omitted}`; the system shall return 400 when `TargetMethod` is "batch" or "type" and no target year or membership-type values are supplied. [code+test]
- FR-015: An authenticated member shall retrieve their own communication log, paged, via GET `api/communications/me` with `page` defaulting to 1 and `pageSize` to 25; the system shall return 401 with detail "Member profile is required." when the member id claim is missing or unparseable. [code+test]
- FR-016: A mobile client shall register or refresh its FCM device token via POST `api/notifications/device-token` with body `{Token, Platform}`; the system shall return 400 when `Token` is empty or whitespace, and shall overwrite any previously stored token for that member on repeat calls. [code+test]
- FR-017: An authenticated member shall retrieve their in-app notifications, most recent first and capped at 50 rows, via GET `api/notifications`; the system shall return a 500 problem response on an unhandled service exception. [code+test]
- FR-018: An authenticated member shall mark a single notification read via POST `api/notifications/{id}/read`; the system shall not update a notification belonging to a different member. [code+test]
- FR-019: An authenticated member shall mark all of their unread notifications read in one call via POST `api/notifications/read-all`. [code+test]
- FR-020: The system shall create an in-app notification for a member only when that member's matching preference flag (`NotifyEventCreation`, `NotifyParticipationApproval`, `NotifyRegistrationUpdate`, `NotifyRelevantUpdates`, or `NotifyCommitteeChanges`) is enabled, except a direct-message notification, which the system shall always deliver regardless of preference. [code+test]
- FR-021: The system shall broadcast a notification to every non-archived member whose matching preference flag is enabled when an admin sends a custom broadcast with a push channel via POST `api/admin/comm/send-custom`. [code+test]
- FR-022: When creating a notification from a template, the system shall source the notification's title and body from the resolved email template with HTML tags stripped for plain-text display, and shall fall back to the caller-supplied literal title and message when the template cannot be resolved. [code+test]
- FR-023: An authenticated member shall list active forum categories via GET `api/Forum/categories`. [code+test]
- FR-024: An authenticated member shall list topics for a category, paged, via GET `api/Forum/categories/{categoryId}/topics` with `page` defaulting to 1 and `pageSize` to 20. [code]
- FR-025: An authenticated member shall retrieve a single topic via GET `api/Forum/topics/{topicId}`, receiving 404 if it does not exist; the system shall increment the topic's view count on every successful read. [code]
- FR-026: An authenticated member shall list posts for a topic, paged, via GET `api/Forum/topics/{topicId}/posts` with `page` defaulting to 1 and `pageSize` to 20. [code]
- FR-027: An authenticated member shall create a topic under an active category via POST `api/Forum/topics` with body `{CategoryId, Title, Content}`; the system shall HTML-sanitise `Content` before storage and shall reject the create when the category is missing or inactive. [code+test]
- FR-028: An authenticated member shall reply to a topic via POST `api/Forum/topics/{topicId}/posts` with body `{TopicId, Content, ParentPostId}`; the system shall return 400 "Topic ID mismatch." when the body's `TopicId` differs from the route `{topicId}`, and shall reject the reply when the topic is not found, is inactive, or is locked. [code+test]
- FR-029: The topic's author or a SuperAdmin shall soft-delete a topic via DELETE `api/Forum/topics/{topicId}` (setting `IsActive = false`); the system shall still locate a topic whose category has since been deactivated, and shall reject the delete for any other member. [code+test]
- FR-030: The post's author or a SuperAdmin shall soft-delete a post via DELETE `api/Forum/posts/{postId}` (setting `IsActive = false`), using the same category-bypass lookup as topic deletion. [code+test]
- FR-031: An anonymous visitor shall submit a contact enquiry via POST `api/contact` with body `{FullName (required, max 150 chars), Email (required, valid email, max 200 chars), Subject (required, max 250 chars), Message (required, min 10, max 2000 chars)}`; the system shall persist the enquiry and return the fixed confirmation "Your enquiry has been filed in the secretariat records." regardless of whether the configured recipients' notification email succeeds. [code+test]

### Key Entities

- **ChatMessage** - sender, receiver, content, sent timestamp, read flag. Persisted and queried by `ChatService`.
- **EmailTemplate** - code, subject, body, description, channel (email/SMS). Merged view of database rows and the static `DefaultTemplates` fallback list in `CommunicationService`.
- **EmailLog** - one row per send attempt, with a `Status` of "Sent" or "Failed"; never deleted on a failed send.
- **Notification** - member id, type (`EventCreation`, `ParticipationApproval`, `RegistrationUpdate`, `GeneralSystem`, `CommitteeAssignment`, direct-message), title, message, target URL, read flag, created timestamp.
- **ForumCategory / ForumTopic / ForumPost** - category has an active flag; topic has category, author, lock flag, view count, active flag; post has topic, author, parent post (for threaded replies), active flag.
- **ContactMessage** - full name, email, subject, message, read flag, created timestamp.
- **Member.FcmToken / FcmTokenPlatform / FcmTokenUpdatedAt** - the device-token fields `DeviceTokenService` writes to; no separate device-token entity.
- **Member notification preference flags** - `NotifyEventCreation`, `NotifyParticipationApproval`, `NotifyRegistrationUpdate`, `NotifyRelevantUpdates`, `NotifyCommitteeChanges` on Member; not traced to the Angular/Flutter screen that edits them in this pass.

## Evidence

| FR | Route | Service | Angular | Flutter | Test |
|----|-------|---------|---------|---------|------|
| FR-001 | GET api/messaging/recent | ChatService.GetRecentChatsAsync (GHCAA.Infrastructure/Services/ChatService.cs) | GHCAA.Web/src/app/core/services/chat.service.ts | GHCAA.Mobile/lib/features/messaging/chat_service.dart (`/chat/conversations`) | none found |
| FR-002 | GET api/messaging/history/{otherUserId} | ChatService.GetChatHistoryAsync | GHCAA.Web/src/app/core/services/chat.service.ts | GHCAA.Mobile/lib/features/messaging/chat_service.dart (`/chat/history/{id}`) | none found |
| FR-003 | GET api/messaging/unread | ChatService.GetUnreadMessagesAsync | GHCAA.Web/src/app/core/services/chat.service.ts | not read in this pass | none found |
| FR-004 | POST api/messaging/mark-read/{id}; PATCH api/messaging/read/{id} | ChatService.MarkAsReadAsync | GHCAA.Web/src/app/core/services/chat.service.ts | not read in this pass | GHCAA.Tests/Services/ChatServiceTests.cs::MarkAsReadAsync_WhenCallerIsRecipient_MarksReadAndReturnsTrue, ::MarkAsReadAsync_WhenCallerIsNotRecipient_ReturnsFalseAndLeavesUnread |
| FR-005 | POST api/messaging/send | ChatService.SendMessageAsync | GHCAA.Web/src/app/core/services/chat.service.ts | not read in this pass | GHCAA.Tests/Services/ChatServiceTests.cs::SendMessageAsync_WithNoPriorHistory_ShouldStartNewConversation |
| FR-006 | GET api/admin/comm/templates | CommunicationService.GetAllTemplatesAsync (GHCAA.Infrastructure/Services/CommunicationService.cs) | GHCAA.Web/src/app/admin/comm/admin-comm.ts | none found | none found |
| FR-007 | GET api/admin/comm/logs | CommunicationService.GetRecentLogsAsync | GHCAA.Web/src/app/admin/comm/admin-comm.ts | none found | none found |
| FR-008 | GET api/admin/comm/member/{memberId} | CommunicationService.GetAdminMemberLogsAsync | GHCAA.Web/src/app/admin/comm/admin-comm.ts | none found | none found |
| FR-009 | PUT api/admin/comm/templates/{id} | CommunicationService.UpdateTemplateAsync | GHCAA.Web/src/app/admin/comm/admin-comm.ts | none found | GHCAA.Tests/Controllers/CommunicationControllerTests.cs::UpdateTemplate_ReturnsOk_OnSuccess, ::UpdateTemplate_PropagatesException_WhenTemplateMissing |
| FR-010 | POST api/admin/comm/templates | CommunicationService.CreateTemplateAsync | GHCAA.Web/src/app/admin/comm/admin-comm.ts | none found | GHCAA.Tests/Controllers/CommunicationControllerTests.cs::CreateTemplate_ReturnsOk_OnSuccess, ::CreateTemplate_PropagatesException_WhenCodeAlreadyExists |
| FR-011 | DELETE api/admin/comm/templates/{id} | CommunicationService.DeleteTemplateAsync | GHCAA.Web/src/app/admin/comm/admin-comm.ts | none found | GHCAA.Tests/Controllers/CommunicationControllerTests.cs::DeleteTemplate_ReturnsNoContent_OnSuccess, ::DeleteTemplate_PropagatesException_WhenTemplateMissing |
| FR-012 | POST api/admin/comm/send-batch | CommunicationService.SendBatchEmailAsync | GHCAA.Web/src/app/admin/comm/admin-comm.ts | none found | GHCAA.Tests/Controllers/CommunicationControllerTests.cs::SendBatch_ReturnsOk_OnSuccess, ::SendBatch_ReturnsBadRequest_WhenNoPassingYearGiven |
| FR-013 | POST api/admin/comm/send-type | CommunicationService.SendTypeEmailAsync | GHCAA.Web/src/app/admin/comm/admin-comm.ts | none found | GHCAA.Tests/Controllers/CommunicationControllerTests.cs::SendType_ReturnsOk_OnSuccess, ::SendType_ReturnsBadRequest_WhenNoMembershipTypeGiven |
| FR-014 | POST api/admin/comm/send-custom | CommunicationService.SendBatchCustomEmailAsync / SendTypeCustomEmailAsync / SendCustomEmailAsync; NotificationService.BroadcastNotificationAsync | GHCAA.Web/src/app/admin/comm/admin-comm.ts | GHCAA.Mobile/lib/screens/admin/admin_modules.dart (`/admin/comm/send-custom`) | GHCAA.Tests/Controllers/CommunicationControllerTests.cs::SendCustom_ReturnsOk_OnSuccess_ForEmailList, ::SendCustom_ReturnsBadRequest_WhenBatchTargetHasNoValues |
| FR-015 | GET api/communications/me | CommunicationService.GetMemberLogsAsync (private GetLogsForMemberAsync) | GHCAA.Web/src/app/core/services/member-communications.service.ts; GHCAA.Web/src/app/member/communications/communications.ts | GHCAA.Mobile/lib/features/notifications/notification_service.dart (`/communications/me`) | GHCAA.Tests/Services/CommunicationServiceTests.cs::GetMemberLogsAsync_ReturnsOnlyMemberRowsWithPagingAndRedactsSuccessfulErrors |
| FR-016 | POST api/notifications/device-token | DeviceTokenService.RegisterTokenAsync (GHCAA.Infrastructure/Services/DeviceTokenService.cs) | none found | GHCAA.Mobile/lib/features/notifications/push_notification_service.dart | GHCAA.Tests/Services/DeviceTokenServiceTests.cs::RegisterTokenAsync_ForExistingMember_SavesTokenAndReturnsTrue, ::RegisterTokenAsync_ForUnknownMember_ReturnsFalse, ::RegisterTokenAsync_CalledTwice_OverwritesThePreviousToken |
| FR-017 | GET api/notifications | NotificationService.GetUserNotificationsAsync (GHCAA.Infrastructure/Services/NotificationService.cs) | GHCAA.Web/src/app/core/services/alert.service.ts (consumed by GHCAA.Web/src/app/member/dashboard/dashboard.ts) | GHCAA.Mobile/lib/features/notifications/notification_service.dart (`/notifications`) | GHCAA.Tests/Services/NotificationServiceTests.cs::GetUserNotifications_ShouldReturnOnlyMembersOwnNotifications, ::GetUserNotifications_ShouldReturnMostRecentFirst |
| FR-018 | POST api/notifications/{id}/read | NotificationService.MarkAsReadAsync | GHCAA.Web/src/app/core/services/alert.service.ts | GHCAA.Mobile/lib/features/notifications/notification_service.dart (`/notifications/{id}/read`) | GHCAA.Tests/Services/NotificationServiceTests.cs::MarkAsReadAsync_ShouldMarkSingleNotificationAsRead, ::MarkAsReadAsync_WhenWrongMember_DoesNotUpdateRow |
| FR-019 | POST api/notifications/read-all | NotificationService.MarkAllAsReadAsync | GHCAA.Web/src/app/core/services/alert.service.ts | none found | GHCAA.Tests/Services/NotificationServiceTests.cs::MarkAllAsReadAsync_ShouldMarkAllUnreadForMember |
| FR-020 | internal (not directly routed; invoked from event, election, and committee workflows) | NotificationService.CreateNotificationAsync | none found | none found | GHCAA.Tests/Services/NotificationServiceTests.cs::CreateNotification_WhenEventOptedIn_ShouldPersistNotification, ::CreateNotification_WhenEventOptedOut_ShouldSkipNotification, ::CreateNotification_ParticipationApproval_RespectsOptOut, ::CreateNotification_RegistrationUpdate_RespectsOptOut, ::CreateNotification_DirectMessage_AlwaysDelivered_RegardlessOfPreferences |
| FR-021 | POST api/admin/comm/send-custom (push/both channel) | NotificationService.BroadcastNotificationAsync | GHCAA.Web/src/app/admin/comm/admin-comm.ts | GHCAA.Mobile/lib/screens/admin/admin_modules.dart | GHCAA.Tests/Services/NotificationServiceTests.cs::BroadcastNotification_ShouldOnlyReachOptedInMembers, ::BroadcastNotification_ShouldExcludeArchivedMembers |
| FR-022 | internal (invoked wherever a template-backed notification is raised) | NotificationService.CreateNotificationFromTemplateAsync | none found | none found | GHCAA.Tests/Services/NotificationServiceTests.cs::CreateNotificationFromTemplate_WhenTemplateExists_UsesTemplateTextAndStripsHtml, ::CreateNotificationFromTemplate_WhenTemplateMissing_FallsBackToLiteralText |
| FR-023 | GET api/Forum/categories | ForumService.GetCategoriesAsync (GHCAA.Infrastructure/Services/ForumService.cs) | GHCAA.Web/src/app/core/services/forum.service.ts; GHCAA.Web/src/app/member/forum/forum.ts | GHCAA.Mobile/lib/features/forum/forum_service.dart (`/forum/categories`) | GHCAA.Tests/Services/ForumServiceTests.cs::GetCategoriesAsync_ReturnsSeededCategory |
| FR-024 | GET api/Forum/categories/{categoryId}/topics | ForumService.GetTopicsAsync | GHCAA.Web/src/app/core/services/forum.service.ts | GHCAA.Mobile/lib/features/forum/forum_service.dart | none found |
| FR-025 | GET api/Forum/topics/{topicId} | ForumService.GetTopicByIdAsync | GHCAA.Web/src/app/core/services/forum.service.ts | GHCAA.Mobile/lib/features/forum/forum_service.dart (`/forum/topics/{id}`) | none found |
| FR-026 | GET api/Forum/topics/{topicId}/posts | ForumService.GetPostsAsync | GHCAA.Web/src/app/core/services/forum.service.ts | GHCAA.Mobile/lib/features/forum/forum_service.dart | none found |
| FR-027 | POST api/Forum/topics | ForumService.CreateTopicAsync | GHCAA.Web/src/app/core/services/forum.service.ts | GHCAA.Mobile/lib/features/forum/forum_service.dart | GHCAA.Tests/Services/ForumServiceTests.cs::CreateTopicAsync_AndCreatePostAsync_ShouldPersist |
| FR-028 | POST api/Forum/topics/{topicId}/posts | ForumService.CreatePostAsync | GHCAA.Web/src/app/core/services/forum.service.ts | GHCAA.Mobile/lib/features/forum/forum_service.dart | GHCAA.Tests/Services/ForumServiceTests.cs::CreateTopicAsync_AndCreatePostAsync_ShouldPersist |
| FR-029 | DELETE api/Forum/topics/{topicId} | ForumService.DeleteTopicAsync | GHCAA.Web/src/app/core/services/forum.service.ts | GHCAA.Mobile/lib/features/forum/forum_service.dart (`/forum/topics/{id}` delete) | GHCAA.Tests/Services/ForumServiceTests.cs::DeleteTopicAsync_ShouldSoftDelete_TopicHiddenByQueryFilter_ViaIgnoreQueryFilters, ::DeleteTopicAsync_ShouldThrow_WhenNotAuthorAndNotSuperAdmin |
| FR-030 | DELETE api/Forum/posts/{postId} | ForumService.DeletePostAsync | GHCAA.Web/src/app/core/services/forum.service.ts | GHCAA.Mobile/lib/features/forum/forum_service.dart (`/forum/posts/{id}` delete) | GHCAA.Tests/Services/ForumServiceTests.cs::DeletePostAsync_ShouldSoftDelete_PostHiddenByQueryFilter_ViaIgnoreQueryFilters |
| FR-031 | POST api/contact | ContactService.SubmitMessageAsync (GHCAA.Infrastructure/Services/ContactService.cs) | GHCAA.Web/src/app/core/services/contact.service.ts; GHCAA.Web/src/app/public/contact/contact.ts | not read in this pass | GHCAA.Tests/Controllers/ContactControllerTests.cs::Submit_ReturnsOk |

## Gaps

- `ForumController.CreateTopic` and `CreatePost` check `ModelState.IsValid`, but `CreateForumTopicDto` and `CreateForumPostDto` (`GHCAA.Application/DTOs/ForumDtos.cs`) carry no `[Required]`/length validation attributes, so the check never trips. Topic and post length limits exist only as the HTML sanitiser's implicit behaviour, not a stated rule. [NEEDS CLARIFICATION: what are the intended title and content length limits for a forum topic/post?]
- `ForumService.CreateTopicAsync`/`CreatePostAsync` throw an untyped `System.Exception` for business-rule violations, and `ForumController` does not catch it, so an inactive category or a locked topic surfaces to the client as an unhandled 500 rather than a 400 (`GHCAA.Infrastructure/Services/ForumService.cs`).
- `ContactService.SubmitMessageAsync`'s catch block around the recipient-notification email is empty despite its comment reading "Log failure but don't block submission" - a failed notification is neither logged nor visible anywhere (`GHCAA.Infrastructure/Services/ContactService.cs`).
- `CommunicationService.SendEmailByCodeAsync`/`SendTemplatedEmailAsync` silently no-op (log a warning, return) when a template code does not resolve; the calling admin or system gets no error and no indication the send did not happen (`GHCAA.Infrastructure/Services/CommunicationService.cs`).
- Angular's `AlertService.markAllAsRead()` (`GHCAA.Web/src/app/core/services/alert.service.ts`) calls `api/notifications/read-all`; the Flutter `notification_service.dart` has no equivalent call, so a mobile member cannot mark all notifications read in one action. [NEEDS CLARIFICATION: is mark-all-read intentionally web-only, or a missing mobile feature?]
- Member notification preference flags (`NotifyEventCreation`, `NotifyParticipationApproval`, `NotifyRegistrationUpdate`, `NotifyRelevantUpdates`, `NotifyCommitteeChanges`) gate FR-020 and FR-021, but no controller in this domain's six exposes a route to read or set them, and the shared `app-notify-toggle` component (`GHCAA.Web/src/app/common/notify-toggle/notify-toggle.component.ts`) is used on gallery approval, governance, and job-approval screens, not on any member notification-preference screen found in this pass. [NEEDS CLARIFICATION: which controller and screen own editing these flags?]

## Enhancements: modularisation and reusability

### Reuse across layers

- ENH-001 (P2): `CommunicationController.GetMemberLogs` and `MemberCommunicationsController.GetMine` both call `CommunicationService`'s private `GetLogsForMemberAsync`, differing only in whether the member id comes from an admin-supplied route parameter or the caller's own claim. This is already correct reuse at the service layer; no change needed, noted here as the pattern other duplication in this domain should follow.

### Entity-based module shape

- ENH-002 (P3): Communication (messaging, notifications, forum, contact) sits alongside events, campaigns, mentorship, and gallery as a self-contained entity domain with its own controllers, services, and DTOs. It already follows that shape; no coupling to another domain's tables was found in the services read in this pass.

### Existing reusable components

- ENH-003 (P2): `NotificationService.CreateNotificationAsync` and `BroadcastNotificationAsync` both implement the same switch over `NotificationType` to check a member's preference flag (`GHCAA.Infrastructure/Services/NotificationService.cs`). The individual-send path and the broadcast path duplicate this mapping; a single private helper (`IsOptedIn(Member, NotificationType)`) used by both would remove the duplication.
- ENH-004 (P3): The web admin contact-messages screen (`GHCAA.Web/src/app/admin/contact-messages`) and its Flutter counterpart (`GHCAA.Mobile/lib/screens/admin/contact_messages_screen.dart`) both call `AdminController`'s contact-message routes, not `ContactController`. Not itself a reuse problem, but worth flagging next to ENH-001 since it means the six controllers in this spec do not cover the full contact-message lifecycle a reader would expect from the Angular/Flutter file names alone.

### Hard-coded behaviour that should be configuration

- ENH-005 (P3): `MessagingController.GetChatHistory`'s default history length (50 messages) and `NotificationService.GetUserNotificationsAsync`'s hardcoded `Take(50)` are both literal `50` values with no shared constant and no override in `GHCAA.Application` Constants classes. A single named constant (for example `Constants.Paging.DefaultChatHistory`) would make the two easier to keep in sync if either changes.
- ENH-006 (P2): `CommunicationController.SendCustom`'s channel string (`"email"`, `"push"`, `"both"`) and target-method string (`"batch"`, `"type"`) are compared as raw string literals in the controller (`GHCAA.API/Controllers/CommunicationController.cs:94-113`), with no corresponding constant or enum in `GHCAA.Application`. A typo in either string from the Angular or Flutter admin client would silently fall through to the `else` branch rather than error.

## Success Criteria

- SC-001: Every direct-message send, read, and unread-count action (FR-001 to FR-005) round-trips correctly for two distinct members, verified by GHCAA.Tests/Services/ChatServiceTests.cs.
- SC-002: Admin template CRUD and all three broadcast-send paths (FR-006 to FR-014) reject an incomplete request with 400 and accept a complete one with 200, verified by GHCAA.Tests/Controllers/CommunicationControllerTests.cs.
- SC-003: A member's self-service communication log (FR-015) never returns another member's rows, verified by GHCAA.Tests/Services/CommunicationServiceTests.cs::GetMemberLogsAsync_ReturnsOnlyMemberRowsWithPagingAndRedactsSuccessfulErrors.
- SC-004: In-app notification creation, broadcast, and read-state actions (FR-016 to FR-022) respect each member's preference flags and ownership boundary, verified by GHCAA.Tests/Services/NotificationServiceTests.cs and GHCAA.Tests/Services/DeviceTokenServiceTests.cs.
- SC-005: Forum topic and post creation, listing, and soft-delete (FR-023 to FR-030) enforce the author-or-SuperAdmin rule and the deactivated-category bypass, verified by GHCAA.Tests/Services/ForumServiceTests.cs.
- SC-006: A contact enquiry (FR-031) is always saved and always returns the fixed confirmation message, independent of whether the recipient notification email succeeds, verified by GHCAA.Tests/Controllers/ContactControllerTests.cs::Submit_ReturnsOk.

## Assumptions

- "Admin" throughout this spec means a caller authorized under `Constants.Policies.AdminOnly`, except forum deletion, which checks `User.IsInRole("SuperAdmin")` specifically and does not accept the broader Admin role.
- The `api/chat` and `api/notification` route aliases on `MessagingController` and `NotificationController` are assumed to exist for backward compatibility with an earlier route naming; both aliases were read directly on the controller attributes, not inferred.
- Golden/screenshot tests under `GHCAA.Mobile/test/goldens` (`member_chats_list.png`, `member_chat_room.png`, `member_notifications.png`, `admin_contact_messages.png`, `notifications_center.png`) confirm those mobile screens render, but were not counted as behavioural test evidence for any FR, since they check pixels, not the API contract.
