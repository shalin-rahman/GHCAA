# Tasks: Public site, admin and member experience gaps

**Input**: [spec.md](./spec.md) and [plan.md](./plan.md)
**Purpose**: The checklist for tracker items 100.1 to 100.20. Status and acceptance stay in `docs/TODO.md`.

## Phase 0: Approval

- [x] T000 Owner answered D1 to D8 on 2026-11-17. The answers are in the spec.

## Phase 1: Public links and metadata (100.1, 100.2)

- [ ] T001 Grep `app/public/` and `layouts/public-layout/` for `/portal` links and point each at
  its public route.
- [ ] T002 Join button in the public header, hidden when signed in. Spec for both states.
- [ ] T003 Guard spec: fails on a `/portal` link in a public template other than sign-in.
- [ ] T004 Set canonical and JSON-LD at run time. The `author` meta tag in `index.html` stays.
- [ ] T005 Extend the white-label guard test to `index.html`. Browser check at phone and desktop width.

## Phase 2: Email preferences and unsubscribe (100.3)

- [ ] T006 Read the send path and record on 100.3 whether it checks the `Notify*` flags today.
- [ ] T007 Save `NotifyRelevantUpdates` in the profile save. Test all five flags round trip.
- [ ] T008 `IMailPreferenceService` and the mail kind constants. Tests: each kind skipped when off;
  the four always-sent kinds of D8 sent with every flag off.
- [ ] T009 Signed unsubscribe token, the anonymous route and its undo. Tests: valid, changed,
  unknown and reused token; rate limit applies.
- [ ] T010 Unsubscribe link in the batch and reminder templates.
- [ ] T011 Web: `/unsubscribe` page and "Email preferences" on the member profile. Specs.
- [ ] T012 Mobile: the five toggles on profile edit. Widget test.
- [ ] T013 Record the routes in `docs/API_CONTRACT_REGISTRY.md`. Note on 84.81 that reminders use
  the same check.

## Phase 3: Batch send safety (100.4)

- [ ] T014 Check whether the mobile admin has a batch send screen and record it on 100.4.
- [ ] T015 Preview route with recipient and opted-out counts. Tests.
- [ ] T016 Batch and recipient rows, one migration with its Designer file, and the hosted worker.
  Tests: a failed recipient, a retry with no duplicate, a restart mid-batch.
- [ ] T017 Status route. The send response says what the server does.
- [ ] T018 Admin web: confirm dialog with the counts, then progress. Spec. Browser check.
- [ ] T019 Run `code-reviewer` on the migration. Record the routes in the registry.

## Phase 4: Public content (100.6, 100.7, 100.8)

- [x] T020 Three site content keys with drafted standard text (D7, second answer).
- [x] T021 Web: `/privacy`, `/terms`, `/faq` on one content component; footer and consent links;
  "Not published yet" for an empty page. Specs.
- [ ] T022 Mobile: consent line and drawer links, after 84.69 lands. Widget test.
- [ ] T023 Skip link and `aria-expanded` in the three web layouts. Specs for focus and state.
- [ ] T024 Membership fees section on the home page from the anonymous fees route. Spec for a list
  and for none. Check the first step of mobile registration shows the same fees.

## Phase 5: Previews, titles, sitemap, share (100.5)

- [ ] T025 Prerender the public routes at build time (D1). SEO service sets title, description, canonical and Open Graph tags.
- [ ] T026 Sitemap from published content. Backend tests: public post listed, members-only post not.
- [ ] T027 Share action on news, events and jobs in the portal and the app. Spec and widget test.
- [ ] T028 Check a news link in a preview checker and record the result on 100.5, with the limit
  that a post published after the last build has no preview until the next build.

## Phase 6: Notifications (100.9)

- [ ] T029 Notification link constant; web and mobile map it to their own route.
- [ ] T030 Bell component in portal and admin layouts; `/portal/notifications` page with mark read
  and mark all read. Specs. Browser check in light and dark themes.

## Phase 7: Event pass and check-in (100.10)

- [ ] T031 Check whether the member registration DTO returns `TicketCode` and record it on 100.10.
- [ ] T032 Ticket code on the member's own registration only. Attendance list route. Check-in
  refuses a cancelled registration. Tests for each and for a second scan.
- [ ] T033 QR code on the pass in the portal and the app.
- [ ] T034 Admin web check-in page and attendance list with CSV. Mobile admin attendance list.
- [ ] T035 Event walkthrough: a check-in step and a refusal probe; README table updated. Registry updated.

## Phase 8: Admin dashboard (100.11)

- [ ] T036 `AdminStatsDto` with the old field names. Contract test pins them. Tests for an empty month.
- [ ] T037 Web charts and "needs attention" list with theme tokens. Status badge reads health or goes.
- [ ] T038 Mobile tiles for the new figures. Widget test.

## Phase 9: Shared table, form guard, search (100.12, 100.13, 100.14)

- [ ] T039 After 88.5: decide on its entry whether the import grid can serve as the shared table.
- [ ] T040 Shared table component with specs for sort, paging, select-all on the page and export.
- [ ] T041 Move members and payments admin lists and the three portal tables. Guard spec on the
  count of hand-built tables. Later admin lists move a few per session.
- [ ] T042 `canDeactivate` guard and dirty-form contract; apply to admin editors, profile and
  registration. Specs.
- [ ] T043 Mobile back confirm on profile edit and admin editors. Widget test.
- [ ] T044 After 96.3: header search returns members, payments and events by role. Spec.

## Phase 10: Owner decisions (100.15)

- [x] T045 Owner's dated answers to D2 to D6 recorded on 100.15 (2026-11-17); items 100.16 to
  100.20 opened.

## Phase 11: Refunds (100.19)

- [ ] T046 Read which approval mechanism can hold a refund and record it on 100.19. Ask the owner
  if none fits.
- [ ] T047 Refund record, one migration with its Designer file. Tests: full and part refund, amount
  over the remainder, self-approval refused, rejected request leaves the payment unchanged, audit rows.
- [ ] T048 Report queries net approved refunds. Test on the dues report.
- [ ] T049 Admin web: "Record refund", the approval queue entry, report totals. Specs.
- [ ] T050 Portal and mobile payment history show the refund. Mobile admin approval if its list
  exists. Spec and widget test. `code-reviewer` on the migration. Registry updated.

## Phase 12: Public articles and spotlights (100.16, 100.17)

- [ ] T051 Use the existing `NewsPost.MembersOnly` flag (card S123), with a data step only if the owner says yes. Anonymous list and read routes. Tests: public
  approved returned; members-only, pending and rejected refused; existing rows stay members only.
- [ ] T052 Web public articles pages, in the sitemap with the FR-4 tags. Switch on the portal form
  and the admin moderation screen. Specs.
- [ ] T053 Mobile: the switch on the article form; share uses the public address. Widget test.
- [ ] T054 Read how news categories are held and record it on 100.17. Add "Spotlight" the same way.
  Category filter on the public news route. Test.
- [ ] T055 Home page spotlights section, hidden when empty. Category label in the portal and the
  app. Spec and widget test.

## Phase 13: Newsletter (100.18)

- [ ] T056 Subscriber table with consent times, one migration. Sign-up, confirm and unsubscribe
  routes. Tests: unconfirmed gets no mail, confirmed does, unsubscribed does not, changed token
  refused, the sign-up answer is the same for a known address, rate limit applies.
- [ ] T057 Subscribers as a batch audience, confirmed only, a shared address mailed once. Cleanup
  of unconfirmed addresses after the OrgConfig number of days. Tests.
- [ ] T058 Web: footer field, confirm and unsubscribe pages, admin subscriber list with CSV and
  delete, the audience on the batch screen. Specs. The privacy page names the list.
- [ ] T059 `code-reviewer` and `security-reviewer` on the routes and the migration. Registry updated.

## Phase 14: Android app links (100.20)

- [ ] T060 Ask the owner for the SHA-256 fingerprint of the release signing key. Do not guess it.
- [ ] T061 `autoVerify` filter with the host from the build environment; router mapping; an unknown
  path goes to the browser. Widget or router test.
- [ ] T062 `public/.well-known/assetlinks.json`. Owner checks on a phone with the preprod build.
  Record the steps to repeat for the production domain on 100.20.

## Phase 15: Close

- [ ] T063 `python scripts/ci-local.py` per layer touched. `code-reviewer` on each diff.
- [ ] T064 Sync every status spot: tracker entry, Priority Index, master plan progress log.
