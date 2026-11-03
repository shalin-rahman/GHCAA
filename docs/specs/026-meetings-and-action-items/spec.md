# Feature Specification: Meetings, discussions and action items

**Feature Branch**: 026-meetings-and-action-items
**Created**: draft before 2026-11-04; rewritten 2026-11-04 from the owner's requirements
**Status**: Approved by the owner on 2026-11-04. Not started
**Input**: Owner requests of 2026-11-04: "check for discussion/meeting (with series, can be several
times, may have poll associate, need dynamic type, category, date, venue, agenda, decisions, member
comments, outcomes etc)" and "discussions/meetings can be event relevant, AGM, general etc, can be
online or offline".
**Depends on**: [001-platform-baseline](../001-platform-baseline/spec.md);
[028-poll-and-meeting-discussion](../028-poll-and-meeting-discussion/spec.md) for member comments;
spec 014 (events, news, gallery and content) for the event a meeting can belong to.
**Tracker**: `docs/TODO.md` items 84.19 (meetings) and 84.20 (action items)

## Purpose and scope

The association runs AGMs, EC meetings, event planning meetings and open discussion sessions. None
of them can be recorded in the system. `governance.html` already tells members that minutes are
archived, but no meeting or minutes entity exists in any layer (checked 2026-11-04: no `Meeting`
type in Domain or Application).

This spec covers one meeting record used for every kind of meeting and discussion session. A
meeting can belong to a series that meets many times, can be tied to an event, can be held in
person, online or both, and can have polls attached. It carries an agenda, attendance, decisions,
outcomes, minutes, action items and a member comment thread.

Out of scope: automatic repeat rules (each occurrence is created, usually by copying the last one),
calendar sync and `.ics` files, video calling inside the app, reminders before a meeting, quorum
rules, and custom form fields per meeting type.

## Terms

- **Meeting**: one sitting on one date. A discussion session is a meeting whose type says so.
- **Series**: a named group of meetings of the same kind, such as "EC monthly meeting" or "AGM".
- **Type** and **category**: admin-managed lists. Type says what kind of sitting it is (AGM, EGM, EC
  meeting, general meeting, event planning, discussion session). Category says what it is about
  (governance, election, membership, finance, events and so on). Both are `LookupItem` groups, so an admin adds or
  retires values without a release. Polls use the same category list.
- **Audience**: who may see the meeting: all members, EC members, or named invitees only.

## User scenarios

1. An admin creates the series "EC monthly meeting" with type EC meeting, category governance and
   audience EC members.
2. The admin creates the March meeting in that series: date and time, in person at the college
   auditorium, with a meeting link for those joining online. They add five agenda items and
   publish it. EC members see it and answer Yes, No or Maybe.
3. The admin attaches an existing poll, "Venue for the reunion", to the meeting. EC members vote on
   it from the meeting page.
4. After the meeting the admin marks who attended, records two decisions, adds three action items
   with owners and due dates, writes the outcome summary and publishes the minutes.
5. In April the admin creates the next meeting from the March one. The series, type, category,
   venue and audience are copied, open action items are listed for review, and deferred agenda
   items are carried over.
6. An admin creates the AGM for all members, linked to the annual reunion event, held at the venue
   of that event. Members read the agenda, comment before the day, and read the minutes after.
7. An admin creates an online discussion session on the alumni scholarship scheme, open to all
   members, with a poll attached and comments on.

## Functional requirements

Meeting record:

- **FR-1** An Admin can create, edit and cancel a meeting. Fields: title, description, type,
  category, series (optional), event (optional), start date and time, end time (optional), mode
  (in person, online, hybrid), venue (required for in person and hybrid), online link (required for
  online and hybrid), audience, and whether comments are on.
- **FR-2** Type comes from the `MeetingType` lookup group and category from the
  `DiscussionCategory` group, which polls share (spec 028 FR-17). Seed values: types AGM, EGM, EC
  meeting, General meeting, Event planning, Discussion session. Categories, the first five as
  first drafted and the rest added on owner request of 2026-11-04: Governance, Finance, Events,
  Alumni affairs, Other, Election, Membership, Fundraising, Mentorship and careers, Scholarship and
  welfare, Communication. An admin adds more without a release. A retired value stays on old
  meetings and polls and is not offered for new ones.
- **FR-2a** Every meeting, series and poll carries one category. The admin and member meeting lists
  filter by category, type, series and status, on web and mobile, through query parameters on the
  list routes (`?category=Election`). A meeting page shows its category as a link to the list
  filtered by it, and lists its polls, which can be filtered the same way.
- **FR-3** A meeting linked to an event shows on that event's page, and the event's venue is offered
  as the default venue.
- **FR-4** States: Draft, Scheduled, Held, Closed, Cancelled. Draft is seen by admins only.
  Scheduled is seen by the audience. Held is set by the admin after the sitting. Closed means minutes
  are published and the meeting is read-only except action items. A cancelled meeting stays visible
  with its reason.
- **FR-5** A change of date, time, venue, link or mode on a scheduled meeting, and a cancellation,
  send a notification to the audience through the existing notification service.

Series:

- **FR-6** An Admin can create a series with a title, type, category, default venue, default mode and
  default audience. A meeting in a series shows its number in the series ("EC monthly meeting #12").
- **FR-7** "Create next meeting" copies a meeting's series, type, category, mode, venue, link,
  audience and comment setting, carries over agenda items marked Deferred, and lists the open
  action items from earlier meetings in the series.

Audience and attendance:

- **FR-8** Audience is All members, EC members (the members holding a position in the current EC
  period) or Invitees only. The API applies it to every meeting route, its polls and its comments.
  A member outside the audience gets 404.
- **FR-9** For Invitees only, the admin picks members. For the other audiences the invite list is
  the audience itself.
- **FR-10** A member in the audience can answer Yes, No or Maybe until the meeting starts, and change
  the answer until then.
- **FR-11** After the meeting the admin marks attendance per member. The meeting shows counts of
  answers and of attendance.

Agenda, decisions, outcomes and minutes:

- **FR-12** An agenda is an ordered list of items with a title, notes, presenter (optional member)
  and time in minutes (optional). The admin can reorder items. After the meeting each item is marked
  Discussed, Deferred or Dropped.
- **FR-13** A decision has its text, an optional agenda item, an optional linked poll, and an
  optional vote count (for, against, abstain). Decisions are listed on the meeting and in the
  series.
- **FR-14** The outcome is a short summary written by the admin after the meeting.
- **FR-15** Minutes are text written by the admin. They are seen by the audience only after the
  admin publishes them, which moves the meeting to Closed.

Polls and comments:

- **FR-16** An admin can attach one or more polls to a meeting, new or existing. A poll belongs to
  one meeting at most. A poll on a meeting follows the meeting's audience for viewing and voting.
- **FR-17** A meeting has a member comment thread as defined in spec 028 (comments on polls and
  meetings), with `ContextType = Meeting`. Comments open when the meeting is scheduled and stay open
  until an admin locks the thread or closes the meeting.

Action items (84.20):

- **FR-18** A meeting can have one or more to-do lists (owner request, 2026-11-04), such as
  "Reunion preparation" or "Follow-ups". A list has a title, an order on the meeting and a
  category from `DiscussionCategory` (defaults to the meeting's). Each list holds ordered action
  items.
- **FR-18a** An action item has a title, notes (optional), owner (a member, optional so a list can
  be drafted before people are named), due date (optional), priority (Low, Normal, High), status
  (Open, In progress, Done, Dropped), the list and meeting it came from, and optionally a decision.
  The admin can reorder items and move one to another list on the same meeting. The list shows
  progress as done out of total.
- **FR-19** The owner and admins can change the status. Open items appear on the owner's dashboard
  as "My to-dos" and on the next meeting in the series until done or dropped. "Create next meeting"
  carries open items across in their lists.
- **FR-19a** To-do lists and items are filtered by status, owner, due (overdue, this week, later),
  priority and category, on the meeting page, in the series view and in "My to-dos", on web and
  mobile. Items with no owner show as "Unassigned" and can be filtered that way.
- **FR-19b** Everyone in the meeting's audience can read its to-do lists. A list can be marked
  "Admins only" for internal work; its items still show to their owners.
- **FR-20** Every to-do list belongs to a meeting. Standalone lists, tasks and notes outside a
  meeting are not built (decision for 84.20).

Clients and records:

- **FR-21** Web: an admin area for series, meetings, agenda, attendance, decisions, minutes and
  action items; a member page listing upcoming and past meetings with detail, RSVP, polls and
  comments.
- **FR-22** Mobile: the member page, RSVP, polls, comments and the owner's own action items, plus
  the same admin management as web: series, meetings, agenda, attendance, decisions, minutes,
  action items and announcements (decision D1).
- **FR-23** Dates show as `dd-MM-yyyy` and times as `HH:mm` in local time on every client. The wire
  format is UTC.
- **FR-24** Create, edit, cancel, publish minutes, attendance and action item changes are written to
  the audit log.

Announcement (owner request, 2026-11-04):

- **FR-25** When creating or scheduling a meeting, an admin can tick "Publish an announcement", the
  same block as polls in spec 028 FR-13: News or Notice, title and text prefilled from the meeting
  (title, type, date, time, mode, venue), publish date. The news post is created in the same request
  through `INewsService`, links to the meeting page, and its id is kept on `Meeting.AnnouncementId`.
- **FR-26** The option is offered only when the audience is All members. The admin picks public or
  members only, as in spec 028 FR-16. The online link is never copied into the text, since a public
  post can be read by anyone.
- **FR-27** Rescheduling or cancelling a meeting that has an announcement offers to update the news
  post with the new details or a "Cancelled" line. Publishing minutes offers a second announcement
  with the outcome summary, never the minutes text.

## Non-functional requirements

- **NFR-1** Migrations run on SQLite and PostgreSQL and ship with Designer files.
- **NFR-2** Audience checks are done in the service, with tests for each audience.
- **NFR-3** Every new route and DTO is recorded in `docs/API_CONTRACT_REGISTRY.md`.

## Data model

- `MeetingSeries`: title, type, category, default mode, venue, link, audience, active flag.
- `Meeting`: series id, number in series, event id, title, description, type, category, starts at,
  ends at, mode, venue, link, audience, status, cancel reason, outcome, minutes, minutes published
  at, comments on, announcement id, created by and at.
- `MeetingAgendaItem`: meeting id, order, title, notes, presenter id, minutes, status, carried from
  item id.
- `MeetingParticipant`: meeting id, member id, RSVP, responded at, attended.
- `MeetingDecision`: meeting id, agenda item id, text, poll id, for, against, abstain, recorded by.
- `MeetingTodoList`: meeting id, title, order, category, admins only, created by and at.
- `MeetingActionItem`: list id, meeting id, decision id, order, title, notes, owner id (nullable),
  due date (nullable), priority, status, carried from item id, updated at.
- `Poll.MeetingId`: nullable.

## Decisions for the owner

Answered by the owner on 2026-11-04.

- **D1** Admins manage meetings on web and on mobile (FR-22).
- **D2** Admins only (Admin and SuperAdmin) create and manage meetings. EC members take part as an
  audience, not as managers.
- **D3** Minutes, including AGM minutes, are for signed-in members in the audience only. The public
  sees only an outcome announcement the admin chooses to publish (FR-27).
- **D4** An invitee-only meeting is hidden from members who were not invited; the API returns 404
  (FR-8).

## Acceptance

1. An admin can run the scenarios above on web and on mobile; a member can see, answer, vote and
   comment on both.
2. Each audience rule is enforced on meetings, their polls and their comments.
3. "Create next meeting" copies the fields and carries deferred items and open action items.
4. Minutes are hidden until published.
5. A meeting can hold several to-do lists; items can be reordered, moved, assigned later, filtered
   by status, owner, due, priority and category, and open items carry to the next meeting.
6. An all-member meeting created with "Publish an announcement" shows on the news page without its
   online link; EC and invitee meetings never offer the option.
7. Backend tests cover to-do lists and their filters, states, audiences, RSVP cut-off, agenda
   order, carry-over, poll link, announcements and action item changes; Vitest and Flutter widget
   tests cover the pages.

## Traceability

Build plan: [plan.md](./plan.md). Tasks: [tasks.md](./tasks.md). Tracker: 84.19 and 84.20.
