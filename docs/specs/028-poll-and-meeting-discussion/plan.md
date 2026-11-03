# Implementation Plan: Comments on polls and meetings

Design reference. When 84.71 runs is set in `docs/MASTER_EXECUTION_PLAN.md`, the only open plan.

**Spec**: [spec.md](./spec.md)
**Tracker**: `docs/TODO.md` item 84.71

## Shape

Backend, in the existing layers:

- `GHCAA.Domain/Models/ForumTopic.cs`: `CategoryId` becomes nullable; add `ContextType` (string,
  max 20) and `ContextId` (int?). Unique index on the pair where both are set.
- `GHCAA.Domain/Models/Poll.cs`: add `AllowComments` (default true).
- `GHCAA.Domain/Constants.cs`: a `Discussion` group with the context names, the 2000 character cap
  and the audit types. Check for an existing constant first.
- `GHCAA.Application/DTOs/Discussion/`: `CommentDto` (id, parent id, author id, name, photo, content,
  posted at, removed flag, replies) and `CreateCommentDto` (content, parent id).
- `GHCAA.Application/Interfaces/IDiscussionService.cs`: list, post, remove, lock.
- `GHCAA.Infrastructure/Services/DiscussionService.cs`: finds or creates the context topic, checks
  access through a small context check per type (poll now, meeting with spec 026), and reuses the
  forum sanitizer.
- `ForumService`: category queries already filter by category id, so context topics drop out. Check
  any query that reads topics without a category filter and add `ContextType == null`.
- `PollService`: `commentCount` on `PollDto`, `AllowComments` on create and a toggle, and archive
  locks the thread.
- Routes on `PollController`: `GET api/polls/{id}/comments`, `POST api/polls/{id}/comments`,
  `DELETE api/polls/{id}/comments/{commentId}`. On `AdminPollController`:
  `PUT api/admin/polls/{id}/comments/lock` with `{ locked }` and `PUT api/admin/polls/{id}/comments`
  with `{ allowComments }`. No `DbContext` in the controllers.
- Announcement (FR-13 to FR-15): `CreatePollDto` gets an optional `Announcement` block (post type,
  title, text, publish date). `PollService` calls `INewsService.CreateNewsAsync` inside the same
  transaction and stores `Poll.AnnouncementId`. Meetings use the same block in spec 026.
- Visibility (FR-16): `NewsPost.MembersOnly` (bool, default false, so existing rows stay public).
  `NewsService` read methods take whether the caller is signed in and filter on it; check every
  caller of `GetActiveNewsAsync` and `GetNewsByIdAsync`, including the landing page and any
  sitemap or feed. Web and mobile news models get the field and a badge; the admin news editor
  gets the switch.
- Categories (FR-17, FR-18): `Poll.Category` (string, max 50, default `Other`), the
  `DiscussionCategory` lookup group seeded in a migration data step (not `Seed/*.json`), a
  `category` query parameter on both poll list routes, and a filter on the web and mobile lists.
- One migration for both providers, with its Designer file.

Web:

- A shared `<app-comment-thread>` component in `GHCAA.Web/src/app/common/` that takes a load and a
  post function, so meetings reuse it. It uses the existing theme tokens and the date pipe.
- The poll detail view shows it under the options. The admin poll list gets the comments switch and
  a lock button.

Mobile:

- A `CommentThread` widget in `lib/core/widgets/`, shown on the poll detail screen. Errors through
  `ApiException.serverMessage`.

## Order

Each step ends with its tests green and `graphify update .`.

1. Model, constants, migration and Designer file. Check that existing forum lists are unchanged.
2. `DiscussionService` with poll access, sanitizing, one-level replies and removal. Unit tests.
3. Poll routes, `AllowComments`, lock, archive lock, `commentCount`, audit rows. Controller and
   service tests.
4. Web component and poll pages, with Vitest specs.
5. Mobile widget and poll screen, with widget tests at phone width.
6. `API_CONTRACT_REGISTRY.md`, `PROJECT_MAP.md`, swagger snapshot, then
   `python scripts/ci-local.py`.

## Risks

- Making `ForumTopic.CategoryId` nullable touches forum code that assumes a category. Step 1 greps
  every use of `CategoryId` and `Category` before the change.
- `IgnoreQueryFilters` and global filters on topics, if any, must not expose context topics in the
  forum.
