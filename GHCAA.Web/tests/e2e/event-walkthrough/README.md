# Event walkthrough (local)

Runs event participation with approval on the local stack and writes an HTML report with a
captioned screenshot for every step. The admin, each member and a public visitor work in their own
browser sessions. It is built from the election walkthrough's helpers
(`../election-walkthrough/api.ts`, `local.ts`, `report.ts`, `web.ts`): real API, real database,
step-up codes read back from `EmailLogs`, and a refusal probe for each rule. A step with no working
screen goes through the API and the report lists it as a gap.

## Before the first run

The same as the election walkthrough (`../election-walkthrough/README.md`, "Before the first
run"). Registrations pay by cash: the run makes sure a "Cash (manual receipt)" method with no
gateway is enabled, creating it or switching it on as the SuperAdmin in `scenario.json` when needed
(`../election-walkthrough/payment-method.ts`). It stays enabled afterwards like any manual method.

## Run it

Double-click `scripts\run-event-walkthrough.bat` (headed, waits for a key at the end), or:

```bat
scripts\run-workflow-walkthrough.bat event               rem headless
scripts\run-workflow-walkthrough.bat event slow          rem headed, 400 ms per action
scripts\run-workflow-walkthrough.bat event keep          rem leave the borrowed accounts on the test password
scripts\run-workflow-walkthrough.bat restore             rem put the borrowed accounts back
```

By hand, with the API and `npm run start` up: `cd GHCAA.Web; npm run test:e2e:event`.

The report is `GHCAA.Web/walkthrough-report-event/index.html` (git-ignored).

## Data

`scenario.json` holds the admin, the SuperAdmin (for the payment method) and the members (user ids; borrowed onto plus addresses for the
run and put back after), the event (title, place, fee, seat limit, waitlist, guests allowed, how
many days ahead it starts), each member's channel (web or API) and outcome (approve, reject,
waitlist), and the guest. The event title ends in "(local walkthrough)"; the next run deletes the
earlier walkthrough events through `DELETE api/events/admin/{id}` before it starts.

With the default two seats, the first two members take the seats and the third member and the
guest are waitlisted.

## What the run covers

Keep this table and the run in step. A change to events, registration, approval, passes, check-in
or the waitlist adds or updates its step in `event.walkthrough.ts` and its row here in the same
piece of work.

| Section | Steps and refusals checked |
|---|---|
| Clean slate | Earlier walkthrough events deleted; the cash method found, switched on or created. |
| Creating the event | An event that ends before it starts is refused. The admin fills the event form (dates, fee, seat limit, waitlist, payment, guests) and saves; the stored rules are checked. |
| Registrations | One member registers on the web with the payment declared and sees it under My Participations; the others register through the API; a guest registers without an account. Registrations past the seat limit are waitlisted. A second registration by the same member gets 409 with the reason; a registration with no account and no guest details is refused; an unknown event is not found. |
| Approval | On the Participation Approvals tab one registration is approved and one rejected. The list after the decisions is recorded, a waitlist that did not move is reported, and the approved member's notification is checked. |
| Event pass | The invitation is emailed for the approved registration and refused for the rejected one; the rejected member cannot open the pass, and nobody can open someone else's. The approved member's pass is shown. |
| Check-in | The ticket code checks in once; a second check-in, an unknown code and a check-in by a member are refused. The public events page and participant list are shown. |

Not covered yet, each waiting on its build: a member cancelling a registration and the waitlist
moving up (84.80; the run reports the waitlist that does not move), reminders before the event
(84.81), and feedback after it (84.82). Not covered by choice: event tasks and budget, online
payment gateways, and the mobile event screens (their own tests).
