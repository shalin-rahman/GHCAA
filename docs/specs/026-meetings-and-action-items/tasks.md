# Tasks: Meetings, discussions and action items

**Input**: [spec.md](./spec.md) and [plan.md](./plan.md)
**Purpose**: The checklist for tracker items 84.19 and 84.20. Status and acceptance stay in
`docs/TODO.md`.

## Phase 0: Approval

- [x] T000 84.8 platform baseline. Done 2026-09-25: as-built specs 012 to 020.
- [x] T001a [84.19] Owner answered D1 to D4 on 2026-11-04 (recorded in the spec).
- [x] T001 [84.19] Owner approved the spec on 2026-11-04.

## Phase 1: Model

- [ ] T002 [84.19] Entities, enums, `Poll.MeetingId`, constants, the `MeetingType` lookup group
  with seed values in a migration step (`DiscussionCategory` comes from spec 028).
- [ ] T002a [84.19] Category, type, series and status filters on the meeting list routes, web and
  mobile lists (FR-2a). Tests: each filter, a retired category still filters old meetings.
- [ ] T003 [84.19] Migration for SQLite and PostgreSQL with Designer files; apply to an existing
  database.

## Phase 2: Meetings and series

- [ ] T004 [84.19] Services and DTOs for series and meetings: create, edit, cancel, states, audience
  check, notification on reschedule and cancel, audit rows.
- [ ] T005 [84.19] Admin and member controllers.
- [ ] T005a [84.19] Announcement on create and schedule (FR-25 to FR-27), reusing the poll
  announcement block from spec 028; offered for All-members meetings only; update on reschedule
  and cancel; outcome announcement on minutes publish.
- [ ] T006 [84.19] Tests, including announcement created, link left out, refused for EC and
  invitee audiences, and each state change, each audience (all, EC, invitees), Draft hidden from
  members, 404 outside the audience, venue and link rules per mode, retired lookup values.

## Phase 3: Agenda and attendance

- [ ] T007 [84.19] Agenda items with ordering and status; participants, RSVP until start, attendance.
- [ ] T008 [84.19] Tests: reorder, RSVP after start refused, attendance counts.

## Phase 4: Decisions, minutes and action items

- [ ] T009 [84.19] Decisions, outcome, minutes publish to Closed.
- [ ] T010 [84.20] To-do lists per meeting (FR-18 to FR-19b): lists with order, category and
  admins-only flag; items with notes, optional owner, due date, priority, status, reorder and move;
  owner and admin status changes; "My to-dos" on the dashboard; filters by status, owner, due,
  priority and category.
- [ ] T010a Tests: several lists on one meeting, reorder and move, unassigned items, each filter,
  admins-only list hidden from members but its items shown to their owners, carry-over.
- [ ] T011 [84.19] Create next meeting: copy fields, carry deferred items, list open action items.
- [ ] T012 Tests: minutes hidden before publish, carry-over, action item rights.

## Phase 5: Polls and comments

- [ ] T013 [84.19] Attach polls; poll reads and votes follow the meeting audience; polls with no
  meeting behave as before.
- [ ] T014 [84.19] Add the `Meeting` context to the spec 028 discussion service.
- [ ] T015 Tests for both.

## Phase 6: Web

- [ ] T016 [84.19] Admin meetings list and editor; lookup values editable by an admin.
- [ ] T017 [84.19] Member meetings list and detail; event page and dashboard links.
- [ ] T018 Vitest specs for the pages and services.

## Phase 7: Mobile

- [ ] T019 [84.19] Meetings list and detail, RSVP, polls, comments, "My to-dos" with filters.
- [ ] T019a [84.19] Admin screens (D1): series and meeting editor, agenda reorder, attendance,
  decisions, minutes, to-do lists, announcement block, with a role guard.
- [ ] T020 Widget tests at phone width, member and admin screens.

## Phase 8: Close

- [ ] T021 Registry, `PROJECT_MAP.md`, feature catalogue, swagger snapshot, full
  `python scripts/ci-local.py`, `graphify update .`.
