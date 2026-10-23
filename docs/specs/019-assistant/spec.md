# Feature Specification: Assistant

**Feature Branch**: `019-assistant`
**Created**: 25-09-2026
**Status**: As-built baseline
**Input**: Reverse-engineered from the implemented code.
**Depends on**: [001-platform-baseline](../001-platform-baseline/spec.md).

## Purpose and scope

Covers the in-app member assistant, an authenticated keyword-matching endpoint that answers
alumni-search questions against the Member table, as implemented by:

- `AssistantController` (`api/assistant/ask`)

Out of scope (see the related spec rather than repeating it here):
- [006-project-agentra-ai](../006-project-agentra-ai/spec.md), the reusable, provider-agnostic
  agent runtime design (host registration, capability adapters, model-provider abstraction,
  configuration-first policy, audit trail). That spec's own purpose statement names the current
  GHCAA AssistantController as "a small, authenticated, rule-based lookup", a compatibility
  adapter and evidence source, not proof the target platform is built. This spec documents that
  adapter as it exists today; it does not repeat the target-platform design.
- `MemberProfileDto`, the shape returned in FoundMembers: owned by the member-directory spec
  that defines it, not repeated here.

## User Scenarios & Testing

### User Story 1 - Ask the assistant to find alumni (Priority: P1)

Why this priority: this is the endpoint's only real capability; every other reply is a static
greeting or help message.

Independent Test: as an authenticated Member, POST api/assistant/ask with a query containing
"find" or "member", independent of any other feature.

Acceptance Scenarios:
1. Given an authenticated caller, When they POST api/assistant/ask with query containing
   "find", "search", "who", "alumni", or "member", Then the API returns up to 10 Member rows
   as FoundMembers with a count-prefixed Answer string.
2. Given a query that matches none of those keywords, When POST api/assistant/ask, Then the
   API returns a static greeting Answer with no FoundMembers.
3. Given a query containing "help" or "what can you do", When POST api/assistant/ask, Then the
   API returns a static capability-description Answer naming the org's institution name.
4. Given an empty or whitespace-only query, When POST api/assistant/ask, Then the API returns
   400 with detail "Query cannot be empty." before AskAsync is called.

### User Story 2 - Reach the assistant from Web and Mobile (Priority: P2)

Why this priority: the endpoint is only useful if both clients can reach it; there is no
independent value in the API alone.

Independent Test: submit a query from the Angular member assistant page and from the Flutter AI
Chat screen against the same endpoint, independent of each other.

Acceptance Scenarios:
1. Given the Angular assistant.ts component, When a member submits userInput, Then it POSTs
   { query } to API_ENDPOINTS.ASSISTANT and renders response.answer and response.foundMembers
   in the chat thread.
2. Given the Flutter AIChatScreen, When a member submits text, Then AssistantService.ask POSTs
   to /assistant/ask and renders the returned answer text; a request failure renders a fixed
   offline message instead of the raw error.

### Edge Cases

- The controller has no role restriction beyond `[Authorize]`: Member, Admin, and SuperAdmin
  callers all reach the same lookup with no scoping difference.
- A search covers only active, non-archived members, the same base set as the member
  directory. A four-digit year in the query keeps members whose passing year at this college
  matches. A sector word (for example "bank", "doctor", "govt") keeps members whose current job
  sector contains the mapped part of a PROFESSIONAL_SECTORS name. Words with no mapping, such as
  "corporate" or "business", do not narrow the search (TODO 84.12).
- `AssistantController` carries no `[EnableRateLimiting]` attribute, so it falls back to the
  application-wide per-IP limit of 100 requests a minute. Since the rate-limit fix this is a
  `GlobalLimiter` in `GHCAA.API/Extensions/RateLimitingExtensions.cs`, not a
  `RequireRateLimiting` call in `Program.cs`. There is no assistant-specific throttle.
- `GHCAA.Mobile/lib/features/assistant/assistant_service.dart` posts the body key `query`,
  which `AssistantQueryDto` binds as `Query`. It posted `question` before TODO 84.11, so every
  mobile request was rejected as empty.

## Requirements

### Functional Requirements

- FR-001: The system shall require an authenticated caller for POST api/assistant/ask, with no
  role restriction beyond membership in Member, Admin, or SuperAdmin. [code]
- FR-002: The system shall reject a request whose query is null, empty, or whitespace-only with
  400 and detail "Query cannot be empty.", never invoking AskAsync. [code]
- FR-003: The system shall classify a lower-cased query as an alumni-search request when it
  contains any of "find", "search", "who", "alumni", or "member", and shall query the Member
  table for that branch. [code]
- FR-004: The system shall return at most 10 active, non-archived Member rows, narrowed by a
  passing year or sector word found in the query, each mapped to MemberProfileDto with Id,
  FullName, PhotoPath, passing year, current sector and current designation. [code]
- FR-005: The system shall return a fixed help-text Answer naming the org config's
  Branding.InstitutionName when the query contains "help" or "what can you do". [code]
- FR-006: The system shall return a fixed greeting Answer with no FoundMembers when the query
  matches neither the search-intent nor the help-intent keyword sets. [code]
- FR-007: The system shall apply the application-wide Api rate-limit policy to
  api/assistant/ask, since the controller declares no endpoint-specific rate-limit policy.
  [code]

### Key Entities

- AssistantQueryDto: request body; single required field Query (string).
- AssistantResponseDto: response body; Answer (string) and an optional FoundMembers collection
  of MemberProfileDto.
- Member: existing entity queried for alumni-search matches. The feature reads Id, FullName,
  PhotoPath, Status and IsArchived, the college AcademicRecord's PassingYear, and the current
  ProfessionalRecord's Sector and Designation.

## Evidence

| FR | API (verb + route) | Service method | Web (file) | Mobile (file) | Test (file::test name) |
|---|---|---|---|---|---|
| FR-001 | POST api/assistant/ask | GHCAA.API/Controllers/AssistantController.cs | GHCAA.Web/src/app/member/assistant/assistant.ts | GHCAA.Mobile/lib/screens/member/ai_chat_screen.dart | none found |
| FR-002 | POST api/assistant/ask | AssistantController.cs::Ask | assistant.ts | ai_chat_screen.dart | none found |
| FR-003 | POST api/assistant/ask | GHCAA.Infrastructure/Services/AssistantService.cs::AskAsync | assistant.ts | GHCAA.Mobile/lib/features/assistant/assistant_service.dart | GHCAA.Mobile/test/assistant_service_test.dart::posts the question under query and reads the answer |
| FR-004 | POST api/assistant/ask | AssistantService.cs::AskAsync | assistant.ts | assistant_service.dart | GHCAA.Tests/Services/AssistantServiceTests.cs::AskAsync_YearInQuery_KeepsOnlyThatPassingYear, AskAsync_SectorWord_MatchesCurrentJobSector, AskAsync_LeavesOutInactiveAndArchivedMembers |
| FR-005 | POST api/assistant/ask | AssistantService.cs::AskAsync | assistant.ts | assistant_service.dart | none found |
| FR-006 | POST api/assistant/ask | AssistantService.cs::AskAsync | assistant.ts | assistant_service.dart | none found |
| FR-007 | POST api/assistant/ask | GHCAA.API/Extensions/RateLimitingExtensions.cs::AddAppRateLimiting (GlobalLimiter) | none found | none found | none found |

GHCAA.Web/src/app/core/services/assistant.service.spec.ts exists but tests only the Angular
HTTP wrapper (request shape and response passthrough). AssistantServiceTests covers the search
filters (TODO 84.12) and assistant_service_test.dart pins the mobile request body (TODO 84.11).
AssistantController still has no test.

## Gaps

- AssistantController has no test for the empty-query 400 (FR-002) or the authorization
  requirement (FR-001). [NEEDS CLARIFICATION: is controller-level test coverage planned for
  this endpoint, or is it deliberately excluded pending the Agentra AI migration described in
  006-project-agentra-ai?]
- No role scoping distinguishes Member, Admin, and SuperAdmin callers, and no per-caller or
  per-role rate limit exists separate from the shared Api policy; a single member account can
  consume up to the full 100-requests-per-minute application-wide budget against the Member
  table lookup. [NEEDS CLARIFICATION: is the shared Api policy an accepted risk for this
  endpoint, or does it need its own tighter, keyed rate limit given it runs a table scan on
  every call?]
- There is no provider selection, prompt template, or guardrail logic anywhere in
  AssistantService.cs; matching is five `string.Contains` keyword checks. This confirms
  006-project-agentra-ai's own framing of the current code as a rule-based adapter, not an
  LLM-backed assistant, but it means "prompts and guardrails" cannot be evidenced from this
  controller because none exist in the implementation.

## Enhancements: modularisation and reusability

### Reuse across layers

- ENH-001 (P2): AssistantController.Ask and AssistantService.AskAsync take a plain string;
  there is no shared request/response contract layer between this ad hoc DTO pair and the
  capability-adapter contract 006-project-agentra-ai specifies for host operations. Any future
  migration to that runtime would need this controller rewritten, not extended.

### Entity-based module shape

- ENH-002 (P3): AssistantService.cs lives in GHCAA.Infrastructure/Services alongside
  entity-specific services (MemberService, etc.) but has no entity of its own; it reads
  Members directly via ApplicationDbContext instead of going through an existing member-lookup
  service, duplicating the Members.AsQueryable() access pattern used elsewhere in the codebase
  instead of reusing a shared query.

### Existing reusable components

- ENH-003 (P3): GHCAA.Web/src/app/member/assistant/assistant.ts hand-rolls its own chat-message
  list, typing indicator, and timestamp formatting. No chat-bubble or message-list component
  exists under GHCAA.Web/src/app/common today (checked: breadcrumb, confirm-dialog, directory,
  export-buttons, footer, gallery, governance, health, icon, jobs, loading-panel, logo-spinner,
  modal-header, news, notify-toggle, page-header, pagination, payment-method-selector,
  payment-portal, payment-status, rich-text-editor, search-bar, step-up-dialog, theme-toggle,
  toast, user-menu; none is a chat surface). GHCAA.Web/src/app/core/services/chat.service.ts is
  a peer-to-peer messaging service, not a UI component, so it is not a candidate to reuse here.
  This is a genuine gap rather than a bypassed shared component.

### Hard-coded behaviour that should be configuration

- ENH-004 (P2): The five intent keywords ("find", "search", "who", "alumni", "member") and the
  two help keywords ("help", "what can you do") are literal strings inside AskAsync
  (AssistantService.cs lines 33 and 73), not sourced from OrgConfigService or any other
  configuration. Every other timing/behaviour knob audited in this codebase (rate-limit
  windows, step-up TTL) is at least a candidate for config; these keyword lists are English-only
  and cannot be tuned or localized without a code change.
- ENH-005 (P3): The result cap of 10 (AssistantService.cs line 53, `Take(10)`) is a literal, not
  a configuration value, unlike the page-size-5 pending-approvals pattern documented in
  012-membership-lifecycle-auth.

## Success Criteria

- SC-001: An authenticated Member, Admin, or SuperAdmin can submit a query containing an
  alumni-search keyword and receive up to 10 matching Member rows with name and photo (FR-003,
  FR-004), as shown by AssistantServiceTests.
- SC-002: An empty query is rejected with 400 before any database access (FR-002), as evidenced
  by AssistantController.cs::Ask; no automated test currently demonstrates this.

## Assumptions

- The sector words map to the web's PROFESSIONAL_SECTORS list
  (`GHCAA.Web/src/app/core/constants/app.constants.ts`). The old filter named sectors such as
  "Corporate" and "Govt. Service" that are not on that list, so it was rewritten rather than
  uncommented (TODO 84.12).
- Test file names referenced in the Evidence table ("none found") were confirmed absent via
  Glob against GHCAA.Tests, GHCAA.Web *.spec.ts, and GHCAA.Mobile/test at the time this spec
  was written.
