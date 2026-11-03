# Tasks: Comments on polls and meetings

**Input**: [spec.md](./spec.md) and [plan.md](./plan.md)
**Purpose**: The checklist for tracker item 84.71. Status and acceptance stay in `docs/TODO.md`.

## Phase 0: Approval

- [x] T000 Owner answered D1 to D3 and approved the spec on 2026-11-04.

## Phase 1: Model

- [ ] T001 Grep every use of `ForumTopic.CategoryId` and `Category` in all layers and list the queries
  that must skip context topics.
- [ ] T002 Make `ForumTopic.CategoryId` nullable, add `ContextType` and `ContextId` with a unique
  index, and add `Poll.AllowComments`. Add the `Discussion` constants.
- [ ] T003 One migration for SQLite and PostgreSQL with its Designer file. Check that it applies to
  an existing database and that forum counts are unchanged.

## Phase 2: Service

- [ ] T004 `IDiscussionService`, the DTOs and `DiscussionService`: find or create the topic, poll
  access check, sanitize, length check, one-level replies, removal by author or admin.
- [ ] T005 Tests: read and post as a member, 404 for a poll the member cannot open, blank and too
  long comment, reply to a reply, parent from another thread, removal by author, by admin and by
  another member, removed comment with and without replies.

## Phase 3: Poll routes

- [ ] T006 Comment routes on `PollController`, lock and allow-comments routes on
  `AdminPollController`, `commentCount` on `PollDto`, archive locks the thread, audit rows.
- [ ] T007 Tests: locked, archived and comments-off polls refuse posts with a reason; lock and unlock
  write audit rows; forum category lists do not show context topics.

- [ ] T006a Announcement on create: `Poll.AnnouncementId` in the T003 migration, optional
  announcement block on the create DTO, news post created in the same transaction through
  `INewsService`, hide-on-archive. Tests: created, rolled back when the news post fails, no poll
  options in the text.
- [ ] T006b News visibility (FR-16): `NewsPost.MembersOnly` in the T003 migration, filtered reads
  for anonymous callers, admin news editor switch, badge on web and mobile news lists. Tests:
  anonymous list and detail skip the post, a member sees it, existing posts stay public.

- [ ] T006c Poll category (FR-17, FR-18): `Poll.Category` and the `DiscussionCategory` seed in the
  T003 migration, existing polls set to Other, `category` filter on both list routes, picker on the
  create form, filter on web and mobile lists. Tests: filter, default for old polls, retired value.

## Phase 4: Web

- [ ] T008 `<app-comment-thread>` in `common/`, using theme tokens and the shared date pipe.
- [ ] T009 Poll detail shows the thread; the admin poll list gets the switch and the lock button.
  The poll create form gets the "Publish an announcement" block.
- [ ] T010 Vitest specs for the component and both pages.

## Phase 5: Mobile

- [ ] T011 `CommentThread` widget and its place on the poll detail screen.
- [ ] T012 Widget tests at phone width: list, post, reply, remove, locked message.

## Phase 6: Close

- [ ] T013 Record the routes and fields in `docs/API_CONTRACT_REGISTRY.md`; update `PROJECT_MAP.md`
  and the swagger snapshot; run `python scripts/ci-local.py` and `graphify update .`.
