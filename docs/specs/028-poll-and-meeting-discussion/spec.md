# Feature Specification: Comments on polls and meetings

**Feature Branch**: 028-poll-and-meeting-discussion
**Created**: 2026-11-04
**Status**: Approved by the owner on 2026-11-04. Not started
**Input**: Owner request of 2026-11-04: "check we have any existing discussion and poll management,
register member can choose poll options set by admins, able to discuss/comments - if not create
todo, add plan and tasks, first write specs".
**Depends on**: [001-platform-baseline](../001-platform-baseline/spec.md) (polls and forum as built).
**Used by**: [026-meetings-and-action-items](../026-meetings-and-action-items/spec.md), which gives
each meeting the same comment thread.
**Tracker**: `docs/TODO.md` item 84.71

## Purpose and scope

Members can vote on polls but cannot say why, ask a question about an option or answer each other.
The forum lets members talk, but a forum topic is not tied to a poll, so the talk sits in a
different place from the vote. This spec adds a comment thread to each poll, and the same thread
to each meeting once spec 026 (meetings, agenda, decisions and action items) is built.

In scope: reading and posting comments, one level of replies, removal by the author or an admin,
an admin lock, a per-poll switch to turn comments off, comment counts, audit of admin actions, and
the web and mobile screens for members.

Out of scope: editing a posted comment, reactions, mentions, notifications about new comments,
attachments, and comments on anything other than polls and meetings.

## What exists today (checked 2026-11-04)

- Polls: `Poll`, `PollOption`, `PollVote`. Admins create, toggle and delete polls through
  `api/admin/polls` (`AdminPollController`, AdminOnly). Members list active polls, open one and vote
  through `api/polls` (`PollController`). Web has `GHCAA.Web/src/app/*/polls`, mobile has a polls
  screen. A deleted poll is archived (`IsArchived`), not removed.
- Forum: `ForumCategory`, `ForumTopic`, `ForumPost`. A post can reply to another post through
  `ParentPostId`. Content passes through `HtmlSanitizer` before it is saved. A topic can be locked.
  Only the author or a SuperAdmin can delete a topic or post.
- No comment, reply or discussion is attached to a poll anywhere in the code.

## Design choice

Comments are stored as forum posts under a forum topic that belongs to the poll or meeting. The
topic is created on the first comment. This reuses the post table, the sanitizer, the reply link
and the lock flag instead of adding a second comment model.

A context topic has no forum category, so it never shows in the forum's category lists. It carries
two new columns, `ContextType` (`Poll` or `Meeting`) and `ContextId`, unique together. Who may read
or post is decided by the poll or meeting, not by the forum.

The other option was a separate `Comment` table keyed by context. It would avoid touching the forum
model but would duplicate sanitizing, replies, removal and counts. It is recorded here in case the
forum change proves awkward during build.

## User scenarios

1. A member opens an active poll, reads the comments under it, votes, and posts a comment saying
   why they chose an option.
2. Another member replies to that comment. The reply shows under it.
3. A member removes their own comment. It shows as "Comment removed" so replies still make sense.
4. An admin removes a rude comment and locks the thread. Members can still read it but cannot post.
5. An admin creates a poll with comments turned off. Members see the poll with no comment box.
6. A poll closes. Members can still read the comments. Posting stays open until an admin locks the
   thread or archives the poll.

## Functional requirements

- **FR-1** A signed-in member who can open a poll can read its comments, newest thread last, with
  paging. A member who cannot open the poll gets 404 for its comments too.
- **FR-2** That member can post a comment while the thread is open: the poll allows comments, the
  thread is not locked and the poll is not archived. Otherwise the API answers 400 with the reason
  in `detail`.
- **FR-3** A comment is 1 to 2000 characters after trimming, and is sanitized the same way as forum
  posts. A blank comment answers 400.
- **FR-4** A reply names a parent comment in the same thread. A reply to a reply is attached to the
  top-level comment, so threads are one level deep. A parent from another thread answers 400.
- **FR-5** The author can remove their own comment. An Admin or SuperAdmin can remove any comment.
  Removal sets the post inactive. The list shows "Comment removed" in its place when it has active
  replies, and leaves it out when it has none.
- **FR-6** An Admin can lock and unlock a thread. Archiving a poll locks its thread.
- **FR-7** A poll has an `AllowComments` flag, set when the admin creates it and changeable later.
  It defaults to on.
- **FR-8** The poll list and poll detail carry `commentCount`, the number of active comments.
- **FR-9** Each comment shows the author's name, photo and posted time (`dd-MM-yyyy HH:mm` on every
  client). It does not show how the author voted.
- **FR-10** Admin removal, lock and unlock are written to the audit log with the admin, the poll and
  the comment.
- **FR-11** Posting uses the existing API rate-limit policy. No extra limiter is added.
- **FR-12** When spec 026 is built, a meeting gets the same thread with `ContextType = Meeting`. Who
  may read and post follows the meeting's audience rule in spec 026, not the poll rules here.

Announcement (owner request, 2026-11-04):

- **FR-13** When creating a poll, an admin can tick "Publish an announcement". The form then offers
  News or Notice (the existing `PostType`), a title and text prefilled from the poll, and a publish
  date that defaults to now. Saving the poll creates the `NewsPost` through the existing
  `INewsService.CreateNewsAsync` in the same request, with a link to the poll page. If the news post
  fails, the poll is not saved and the admin sees why.
- **FR-14** The announcement can be public (`GET api/news` allows anonymous reads), so the text is
  editable before saving and the poll's options and results are never copied into it. The member
  must still sign in to vote.
- **FR-15** The poll keeps the id of its announcement (`Poll.AnnouncementId`, nullable). The admin
  poll list shows the link, and archiving the poll offers to hide the announcement too. An
  announcement can also be added later from the poll list.
- **FR-16** Each news post gets a visibility: Public (the default, so every existing post stays as
  it is) or Members only. The announcement block has this choice and defaults to Members only. A
  members-only post is left out of anonymous reads of `GET api/news` and `GET api/news/{id}` (404),
  the public landing page and the public news pages, and shows to signed-in members on web and
  mobile with a "Members only" badge. The admin news editor gets the same choice for any post.

Categories (owner request, 2026-11-04):

- **FR-17** A poll has a category from the `DiscussionCategory` lookup group, shared with meetings
  in spec 026 FR-2: Governance, Finance, Events, Alumni affairs, Other, Election, Membership,
  Fundraising, Mentorship and careers, Scholarship and welfare, Communication. Existing polls get
  Other. An admin can add or retire values without a release.
- **FR-18** The admin and member poll lists filter by category and by state (active, closed,
  archived) on web and mobile, through query parameters on the list routes
  (`GET api/polls/active?category=Election`, `GET api/admin/polls?category=Election`). The category
  shows on each poll as a link to the list filtered by it.

## Non-functional requirements

- **NFR-1** The migration runs on SQLite and PostgreSQL and ships with its `.Designer.cs`.
- **NFR-2** Forum category lists, topic counts and post counts do not change for existing data.
- **NFR-3** Every new route and DTO field is recorded in `docs/API_CONTRACT_REGISTRY.md`.

## Decisions for the owner

Answered by the owner on 2026-11-04.

- **D1** A closed poll keeps taking comments until an admin locks the thread. Archiving still locks
  it (FR-7).
- **D2** Admin removal asks for a reason that goes to the audit log only. The author sees
  "Comment removed" like everyone else and gets no message.
- **D3** The admin picks per announcement whether it is public or members only. This adds a
  members-only flag to news posts (FR-16).

## Acceptance

1. A member can read, post, reply to and remove their own comments on a poll from web and mobile.
2. An admin can remove any comment, lock and unlock a thread, and turn comments off for a poll.
3. Locked, archived and comments-off polls refuse new comments with a clear message on both clients.
4. The forum's own lists are unchanged.
5. Backend tests cover FR-1 to FR-10 and FR-13 to FR-18, including an anonymous read that must
   not see a members-only post. Vitest and Flutter widget tests cover the
   comment section.
6. Creating a poll with "Publish an announcement" ticked shows the news or notice on the news page
   with a link to the poll; a failed news post leaves no poll behind.

## Traceability

Tasks: [tasks.md](./tasks.md). Build plan: [plan.md](./plan.md). Tracker: 84.71.
