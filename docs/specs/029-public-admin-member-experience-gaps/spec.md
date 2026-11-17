# Feature Specification: Public site, admin and member experience gaps

**Feature Branch**: 029-public-admin-member-experience-gaps
**Created**: 2026-11-17
**Status**: Decisions D1 to D8 answered by the owner on 2026-11-17. Not started
**Input**: Owner requests of 2026-11-17: reviews of the public site and the admin site against
professional alumni and club systems, then "can you add gap details plans, todo, tasks specs
considering changes on member and mobile app too".
**Depends on**: [001-platform-baseline](../001-platform-baseline/spec.md) (the platform as built).
**Used by**: none yet.
**Tracker**: `docs/TODO.md` Work Package 100, items 100.1 to 100.20 (100.15 closed with the owner's answers)

## Purpose and scope

The two reviews found gaps that a visitor, a member or an administrator meets on the first day:
links that lead to a sign-in wall, mail that cannot be turned off, a batch send with no safety step,
an event pass that cannot be scanned, and lists that cannot be sorted or exported. This spec states
what each gap needs on every client, so that no fix lands on one client and leaves the others behind.

In scope: the nineteen build items of Work Package 100 (100.1 to 100.14 and 100.16 to 100.20), on the public site, the admin site, the member
portal, the mobile app and the API.

Out of scope, because the tracker already holds them: dues and event reminders (84.81), the arrears
report (84.94), saved reports (90.1 to 90.4), cohorts (37.4), the memorial register (37.5),
localization (37.7 and 8.8), chapters (37.9), the impact report (37.10), vacant governance seats
(95.4), sending push (84.61) and the white-label work (Work Package 62). Scheduled email is also
out of scope.

## What exists today (checked 2026-11-17)

The code was read, not run.

- Public site: the home page previews for news and jobs link to `/portal/news` and `/portal/jobs`.
  `index.html` holds an `author` meta tag with a personal name and email, and a canonical link and
  JSON-LD fixed to the preprod host. There are no Open Graph tags. `public/sitemap.xml` is a static
  file with nine addresses. There is no privacy, terms or FAQ page. No layout has a skip link or
  `aria-expanded` on its menu button. `GET api/financials/fees/applicable` is anonymous and the
  home page does not use it.
- Communication: `Member` has five `Notify*` flags. `MemberService_Profile` saves four and leaves
  out `NotifyRelevantUpdates`. Neither the web profile nor the mobile profile edit screen shows
  them. No unsubscribe route exists. The batch send routes on `CommunicationController` send inside
  the request with no preview, while the response says "queued". Push links name
  `/portal/notifications`, which is not a web route. The web has no bell; an alert panel shows on
  the member dashboard only. Mobile has a notification screen.
- Events: `EventRegistration` has `TicketCode`, `IsCheckedIn` and `CheckedInAt`, and
  `POST api/events/admin/checkin/qr` exists. The mobile admin has a scanner (`gatekeeper_screen.dart`).
  The web pass draws decorative bars, not a code. The web has no check-in page and no attendance list.
- Admin: `GET api/admin/stats` returns an untyped object used by the web dashboard and the mobile
  admin dashboard. The dashboard badge "Live System Status" is static text. 27 admin files and 3
  portal files build their own `data-table`. No route uses `canDeactivate`. The compact search bar
  filters menu entries only.
- Not confirmed: whether the send path reads the `Notify*` flags, whether the member registration
  DTO returns `TicketCode`, and whether the mobile admin has a batch send screen.

## Design choice

Each gap is closed with what the platform already has before anything new is added: the site
content store for the policy pages, the existing ticket code for the pass, the existing stats route
for the dashboard, the existing search routes for the header search. New routes are added only for
unsubscribe, the send preview and status, the attendance list and the sitemap. The owner's
answers of 2026-11-17 add three more: public articles, the newsletter and refunds.

One rule holds across the items: a change to a shared route keeps the old fields, because the mobile
app ships on its own schedule.

## User scenarios

1. A visitor opens the home page, sees what membership costs, clicks a news card and reads the post
   without signing in, then clicks Join.
2. A visitor pastes a news link into a chat app and the preview shows the title and image.
3. A member clicks "unsubscribe" in an event email, lands on a page that confirms it, and still
   receives a sign-in code the next day.
4. A member changes email preferences in the mobile app and sees the same values on the web profile.
5. An administrator writes a batch email, sees "This goes to 412 members; 37 opted out", confirms,
   and watches the progress.
6. A member shows the pass QR code at the door. A volunteer scans it on a laptop. A second scan
   says the pass was used.
7. An administrator sorts the member list by join date, selects twenty rows and exports them.
8. An administrator edits an event, clicks another menu entry and is asked before the change is lost.

## Functional requirements

Public site (100.1, 100.2, 100.5 to 100.8)

- **FR-1** No template under the public area links to a `/portal` route other than sign-in.
- **FR-2** The public header shows a Join button to a signed-out visitor only.
- **FR-3** `index.html` holds no fixed host. The `author` meta tag stays (owner, 2026-11-17). Canonical and JSON-LD values come
  from the page address and OrgConfig at run time.
- **FR-4** Every public route sets its own title and description. News, event and job pages also set
  Open Graph title, description and image. The public routes are prerendered at build time
  (decision D1), so the tags are in the served HTML. A post published after the last build has no
  preview until the next build.
- **FR-5** The sitemap lists published public news, events and jobs. Members-only content is left out.
- **FR-6** The member portal and the mobile app offer a share action on news, events and jobs. The
  shared address is the public one.
- **FR-7** `/privacy`, `/terms` and `/faq` render from site content keys, are edited in the existing
  admin site content screen, and are linked from the footer and from the consent line on both
  registration screens. Mobile opens them through the site content screens of 84.69.
- **FR-8** Each web layout (public, portal, admin) has a skip link and a menu button that reports
  its state.
- **FR-9** The home page lists membership types and fees from the anonymous fees route and hides the
  section when the route returns none.

Communication (100.3, 100.4, 100.9)

- **FR-10** The profile save writes all five `Notify*` flags.
- **FR-11** The member portal profile and the mobile profile edit screen show and save the five flags.
- **FR-12** Every batch and reminder email that a member can turn off carries an unsubscribe link with a signed token that
  names the member and the kind of mail. The route needs no sign-in, turns off that one flag, and
  rejects a changed or unknown token with the same response.
- **FR-13** The send path skips a member who opted out of that kind. Four kinds are never skipped
  and carry no unsubscribe link (decision D8): account and security mail (sign-in codes, password
  reset, email verification, account-change notices), payment receipts, election notices the rules
  require, and membership status mail (application approved or rejected, expiry and dues-owed
  notices). Dues reminders therefore always send; event reminders follow the preference.
- **FR-14** A preview route returns the recipient count and the opted-out count for a batch before
  it is sent. The admin screen shows both in a confirm dialog.
- **FR-15** A batch send runs on a background worker. The admin can read its status (waiting,
  sending, done, failed, with counts). A failed recipient does not stop the batch. A retry does not
  mail a member twice.
- **FR-16** The portal and admin layouts show a bell with the unread count. `/portal/notifications`
  lists notifications with mark read and mark all read.
- **FR-17** The notification link value is a shared constant. Web maps it to the page; mobile maps
  it to the notification screen.

Events (100.10)

- **FR-18** A member's own registration returns the ticket code. No other member's code is returned.
- **FR-19** The pass on the portal and in the app shows a QR code of the ticket code.
- **FR-20** The admin web has a check-in page (typed code or camera) and an attendance list per
  event with the checked-in time and CSV export. The mobile admin shows the same list.
- **FR-21** Check-in refuses a cancelled registration and reports a pass already used.

Admin (100.11 to 100.14)

- **FR-22** `api/admin/stats` returns a typed DTO that keeps the existing field names and adds new
  members per month, dues collected per month, pending approvals by kind and upcoming events.
- **FR-23** The web dashboard shows the new figures as charts and a "needs attention" list. The
  mobile admin dashboard shows them as tiles. The status badge reads the health endpoint or is removed.
- **FR-24** One shared table component gives sort, paging, row selection with bulk actions and CSV
  export. Admin lists and the three portal tables move to it.
- **FR-25** A changed and unsaved form asks before the user leaves it, on web (route guard) and on
  mobile (back confirm), for the admin editors, the member profile and registration.
- **FR-26** The admin header search returns members, payments and events as well as menu entries,
  limited to what the admin's role may open.

Built from the owner's answers (100.16 to 100.20)

- **FR-27** An article is members only unless its author or an admin marks it public. Anonymous
  routes return approved public articles only. Public articles have a page, are in the sitemap and
  carry the tags of FR-4. The switch is on the portal and mobile article forms and on the admin
  moderation screen (D2, 100.16).
- **FR-28** "Spotlight" is a news category, added the way existing categories are held. The home
  page shows the latest three and hides the section when there are none. The portal and the app
  show the category (D3, 100.17).
- **FR-29** A visitor can sign up for the newsletter. The address gets mail only after it confirms
  through a mailed link. Each consent step is stored with its time. Every newsletter carries an
  unsubscribe link. The sign-up answer does not reveal whether an address is known. An address
  left unconfirmed is deleted after a set number of days (D3, 100.18).
- **FR-30** An admin can send a batch to confirmed subscribers through the preview and worker of
  FR-14 and FR-15, and can list, export and delete subscribers. An address that is both a member
  and a subscriber is mailed once (D3, 100.18).
- **FR-31** An admin records a refund against a payment with amount, reason and date. A second
  person, the treasurer, approves it through the existing approval workflow; the recorder cannot
  approve. The amount cannot exceed what is left of the payment. Only an approved refund changes
  the payment status and the report totals. No gateway is called (D5, 100.19).
- **FR-32** The member sees an approved refund in the payment history on the portal and in the
  app (D5, 100.19).
- **FR-33** A public news, event or job address opens the Android app when it is installed. The
  site serves `assetlinks.json` for the preprod host, with the fingerprint read from the release
  signing key. The step is repeated for the production domain (D6, 100.20).
- No installable web app is built (D4).

## Non-functional requirements

- **NFR-1** Privacy: no personal data in static assets other than the `author` meta tag the owner kept. An unsubscribe token reveals nothing about
  the member and cannot be used to sign in.
- **NFR-2** Compatibility: a shared route keeps its existing fields and status codes.
- **NFR-3** Accessibility: new controls are reachable by keyboard and named for screen readers.
  Colors and spacing come from the theme tokens.
- **NFR-4** Portability: migrations work on PostgreSQL, and the SQLite model build stays valid.
  The background send does not depend on one host.
- **NFR-5** Labels come from OrgConfig and lookups. No college name or host is written into a
  template.
- **NFR-6** Each item lands with unit tests in the same change, tagged with its FR number.

## Owner decisions (answered 2026-11-17)

The owner answered all eight in the question dialog on 2026-11-17.

- **D1** Link previews (FR-4): prerender the public routes at build time.
- **D2** Public articles: members only by default, with a "public" switch per article (FR-27).
- **D3** Newsletter and spotlights: both. Spotlights as a news category (FR-28) and a newsletter
  sign-up for non-members with double opt-in, an unsubscribe link and an admin send (FR-29, FR-30).
- **D4** Installable web app: no.
- **D5** Refunds: a ledger entry that a second person, the treasurer, approves through the
  existing approval workflow. No gateway call (FR-31, FR-32).
- **D6** Android app links: now, on the preprod host, with a second pass when the production
  domain arrives (FR-33).
- **D7** Policy wording: first answer, the owner supplies it. Second answer the same day, which
  stands: the pages ship with drafted standard text that matches what the system stores and
  does, editable in the admin site content screen. The draft is not legal advice.
- **D8** Always-sent mail: membership status, election notices, payment receipts, and account and
  security mail (FR-13).

Two points stay open and are checked first by their items, not decided here: which existing
approval mechanism holds a refund (100.19), and how news categories are stored (100.17).

## Acceptance

Each tracker item carries its own acceptance line in `docs/TODO.md`. The spec is met when all
nineteen build items are closed and:

- a signed-out visitor can reach every page the home page links to;
- an opted-out member receives no batch or event reminder mail of that kind and still receives the
  four always-sent kinds;
- no newsletter reaches an address that did not confirm;
- no refund counts before a second person approves it;
- no batch leaves without a confirmed count;
- a pass can be issued, shown, scanned and refused on both clients;
- the event walkthrough covers check-in.

## Traceability

| Requirement | Tracker item |
|---|---|
| FR-1, FR-2 | 100.1 |
| FR-3 | 100.2 |
| FR-4 to FR-6, D1 | 100.5 |
| FR-7, D7 | 100.6 |
| FR-8 | 100.7 |
| FR-9 | 100.8 |
| FR-10 to FR-13, D8 | 100.3 |
| FR-14, FR-15 | 100.4 |
| FR-16, FR-17 | 100.9 |
| FR-18 to FR-21 | 100.10 |
| FR-22, FR-23 | 100.11 |
| FR-24 | 100.12 |
| FR-25 | 100.13 |
| FR-26 | 100.14 |
| D2 to D6 (answers) | 100.15, closed 2026-11-17 |
| FR-27, D2 | 100.16 |
| FR-28, D3 | 100.17 |
| FR-29, FR-30, D3 | 100.18 |
| FR-31, FR-32, D5 | 100.19 |
| FR-33, D6 | 100.20 |
