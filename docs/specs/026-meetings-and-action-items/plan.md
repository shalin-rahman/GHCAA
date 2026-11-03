# Implementation Plan: Meetings, discussions and action items

Design reference. When 84.19 and 84.20 run is set in `docs/MASTER_EXECUTION_PLAN.md`, the only open
plan.

**Spec**: [spec.md](./spec.md)
**Tracker**: `docs/TODO.md` items 84.19 and 84.20
**Runs after**: spec 028 (comments on polls and meetings), so the comment thread exists.

## Shape

Backend, in the existing layers:

- `GHCAA.Domain/Models/`: the six entities in the spec's data model, enums for mode, audience,
  status, RSVP, agenda item status and action item status, and `Poll.MeetingId`.
- `GHCAA.Domain/Constants.cs`: lookup group names `MeetingType` and `DiscussionCategory`, audit
  types and routes. Check for existing constants first. Spec 028 adds `DiscussionCategory` first,
  for polls; this spec only adds its meeting use.
- List routes take optional `category`, `type`, `seriesId` and `status` query parameters (FR-2a),
  filtered in the service after the audience check.
- Lookup seed values go through a migration data step, not `Seed/*.json`, because the JSON only
  seeds a new database.
- `GHCAA.Application/DTOs/Meetings/` and `IMeetingService`, `IMeetingSeriesService`.
- `GHCAA.Infrastructure/Services/MeetingService*.cs`, split by concern as the member service is
  (core, agenda, participants, decisions, to-do lists). To-do list routes take optional `status`,
  `ownerId`, `unassigned`, `due`, `priority` and `category` query parameters. The audience check
  lives in one method used by every read, by the poll service and by the spec 028 meeting context.
  An admins-only list is filtered out for members, except the items they own.
- `EC members` audience reads the current EC period from the governance data already used by
  `api/networking` committee routes.
- Notifications on reschedule and cancel go through `INotificationService`.
- Announcements reuse the spec 028 block and `INewsService.CreateNewsAsync`; the service refuses
  the block unless the audience is All members and builds the text without the online link.
- Controllers: `api/admin/meetings` and `api/admin/meeting-series` (AdminOnly), `api/meetings`
  (members). No `DbContext` in controllers.
- One migration per phase for both providers, each with its Designer file.

Web:

- `admin/meetings`: list with filters (type, category, series, status), editor with tabs for
  details, agenda, participants, decisions and minutes, to-do lists (checklist view with progress,
  reorder, move, filters). Shared admin components and theme tokens.
- `member/meetings`: upcoming and past lists with a category and type filter, detail with RSVP, agenda, polls, the
  `<app-comment-thread>` from spec 028, decisions and published minutes.
- Event page shows linked meetings. Dashboard shows "My to-dos" with the same filters.
- If no admin screen for lookup groups exists, add the two meeting groups to whatever admin page
  edits lookups today, or a small one under Settings.

Mobile:

- Member: meetings list and detail screens, RSVP, polls, comment thread, and "My to-dos".
- Admin (decision D1): series and meeting editor, agenda with reorder, attendance taking,
  decisions, minutes, to-do lists and the announcement block. Admin screens need a role guard,
  since mobile admin routes today have none (officials get 403 from the API).

## Order

Each phase ends with tests green, `python scripts/ci-local.py --quick` and `graphify update .`.

1. Model, lookups, migration. Tests that both providers apply it.
2. Meeting and series CRUD, states, audience check, notifications, audit. Service and controller
   tests per audience and state.
3. Agenda, participants, RSVP cut-off, attendance.
4. Decisions, outcome, minutes publish, to-do lists with filters, create next meeting with
   carry-over (84.20).
5. Poll link: `Poll.MeetingId`, poll reads and votes filtered by meeting audience.
6. Comments: add the `Meeting` context to the spec 028 service.
7. Web admin and member pages with Vitest specs.
8. Mobile member screens with widget tests at phone width.
9. Registry, project map, feature catalogue, swagger, full `python scripts/ci-local.py`.

## Risks

- Polls today are open to every member. Filtering polls by meeting audience must not hide the
  existing polls that have no meeting.
- The EC audience depends on how the current EC period is chosen. Use the same rule as the committee
  page so both agree.
