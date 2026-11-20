# Implementation Plan: Public site, admin and member experience gaps

Design reference. When items 100.1 to 100.20 run is set in `docs/MASTER_EXECUTION_PLAN.md`, the only
open plan.

**Spec**: [spec.md](./spec.md)
**Tracker**: `docs/TODO.md` Work Package 100

## Shape

Backend, in the existing layers:

- Preferences (100.3): `MemberService_Profile` saves `NotifyRelevantUpdates` with the other four
  flags. A small `IMailPreferenceService` answers "may this member get this kind of mail" and is
  the only place that reads the flags; `CommunicationService` and the reminder work of 84.81 call it.
  Mail kinds go in `GHCAA.Domain/Constants.cs`; check for an existing constant first.
- Unsubscribe (100.3): a token signed with the data protection provider, holding member id and mail
  kind. `GET api/unsubscribe` and an undo `POST api/unsubscribe/undo`, both anonymous and rate limited.
  The blanket API rate-limit policy overrides named ones, so check which policy applies.
- Batch send (100.4): `POST api/admin/comm/send-batch/preview` returns the two counts. The send
  writes a batch row and one row per recipient, and a hosted worker sends them. The per-recipient
  row is what stops a second mail on retry. `GET api/admin/comm/batches/{id}` returns the status.
  One migration with its Designer file.
- Notification link (100.9): the link value becomes a constant. No schema change.
- Events (100.10): the member registration DTO gains `TicketCode` if it lacks it.
  `GET api/events/admin/{id}/attendance` returns the list. The check-in service refuses a cancelled
  registration. `AlumniEvent` has a global query filter, so admin reads need `IgnoreQueryFilters()`.
- Stats (100.11): `AdminStatsDto` in `GHCAA.Application/DTOs`, with the existing field names
  unchanged. Monthly figures come from grouped queries that work on both providers.
- Site content (100.6): three keys added by a migration data step, not `Seed/*.json`.
- Sitemap and previews (100.5): the owner chose prerendering (D1). The build prerenders the public
  routes and writes the sitemap from the public list routes, so the build needs the API address.
- Public articles (100.16): `NewsPost.MembersOnly` already exists and defaults to false, so no
  `IsPublic` column is added unless the owner says otherwise (card S123); anonymous list and read
  routes that return approved public articles only.
- Spotlights (100.17): a news category added the way categories are held today (read first); a
  category filter on the public news route.
- Newsletter (100.18): a subscriber table with state and consent times, one migration. Sign-up,
  confirm and unsubscribe routes, anonymous and rate limited, on the signed token of 100.3. The
  send reuses the batch rows and worker of 100.4 with subscribers as an audience. A cleanup step
  deletes unconfirmed addresses after the OrgConfig number of days.
- Refunds (100.19): a refund record against a payment, one migration. Approval goes through the
  approval mechanism the code already has (read first; 92.2 may later move the rule). The
  recorder cannot approve. Report queries net approved refunds.
- Always-sent mail (D8): the mail kind constants mark four kinds as always sent, and
  `IMailPreferenceService` returns true for them without reading the flags.
- No `DbContext` in controllers. Every route and DTO change goes in `docs/API_CONTRACT_REGISTRY.md`.

Web (`GHCAA.Web`):

- Public: fix the two preview links; a Join button in `layouts/public-layout`; a membership fees
  section under `public/landing/sections/`; `/privacy`, `/terms`, `/faq` routes on one content
  page component; an `/unsubscribe` page; a small SEO service that sets title, description,
  canonical and Open Graph tags per route; `index.html` loses the fixed host and keeps the author tag;
  public articles pages; a spotlights section on the home page; a newsletter field in the footer
  with confirm and unsubscribe pages; `public/.well-known/assetlinks.json`.
- Layouts: skip link and `aria-expanded` in public, portal and admin layouts; a bell component in
  portal and admin layouts, fed by the existing alert service.
- Member portal: "Email preferences" on the profile, extending `<app-notify-toggle>`;
  `/portal/notifications`; a QR code on the event pass; share buttons on news, events and jobs;
  the three portal tables on the shared table.
- Admin: batch send confirm dialog and progress; check-in page and attendance list; dashboard
  charts and the "needs attention" list; the shared table in `src/app/common/`; record results in
  the header search; the policy pages in the site content screen; the public switch and filter on
  article moderation; the subscriber list and audience; "Record refund", the approval queue entry
  and net totals in reports.
- A `canDeactivate` guard and a dirty-form contract in `src/app/common/`.
- Theme tokens and shared components only. Modal content sits inside `.modal-body` and
  `.modal-footer`.

Mobile (`GHCAA.Mobile`):

- Email preference toggles on `profile_edit_screen.dart`.
- QR code on the event pass with `qr_flutter`, which the app already has.
- Attendance list in the admin area, next to `gatekeeper_screen.dart`.
- Notification taps map the shared link constant to the notification screen.
- New stats figures as tiles on `/admin_dashboard`.
- Share action on news, events and jobs.
- Policy links on `register_screen.dart` and in the drawer, through the 84.69 screens.
- Back confirm on profile edit and the admin editors.
- The public switch on the article form; the spotlight label on news; the refund in payment history.
- Android app links: an `autoVerify` filter with the host from the build environment, and router
  mapping for news, event and job addresses. The signing fingerprint comes from the owner.
- Mobile follows its own patterns; it does not copy the web layout.

Tests and evidence:

- Backend tests per item, tagged with the FR number. Web specs for each new component and guard.
  Widget tests for the mobile screens that change.
- The event walkthrough in `GHCAA.Web/tests/e2e/event-walkthrough/` gains a check-in step and a
  refusal probe, and its README table is updated in the same change.
- A browser check for every visual change, at phone and desktop width, in light and dark themes.

## Order

The owner set the rule on 2026-11-17: each item runs where its dependencies are met, not last.
The session rows are in section 3 of the master plan.

1. 100.1 and 100.2: small, no API change, and 100.2 stops pointing search engines at preprod.
2. 100.3 backend, then its clients. 84.81 (reminders) should run after this so it uses the same check.
3. 100.4, which reads the opted-out count from step 2.
4. 100.6, 100.7 and 100.8: public pages with no new API.
5. 100.5, prerendered (D1 answered).
6. 100.9 and 100.10.
7. 100.11.
8. 100.12 after 88.5 (editable import grid), so one grid serves both.
9. 100.13, then 100.14 after 96.3 (shared header and search).
10. 100.15 closed on 2026-11-17 with the owner's answers.
11. 100.19 (refunds) after 84.92 (payment review on clients).
12. 100.16 (public articles) and 100.20 (Android app links) after 100.5; 100.17 (spotlights)
    after 100.1.
13. 100.18 (newsletter) after 100.3 and 100.4, whose token and worker it reuses.

Mobile release note: steps 2, 6 and 7 change routes the app reads. Old fields stay, so an older
app build keeps working.
