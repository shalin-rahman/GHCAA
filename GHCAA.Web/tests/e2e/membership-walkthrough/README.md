# Membership walkthrough (local)

Runs the whole registration and approval workflow on the local stack and writes an HTML report
with a captioned screenshot for every step. Each person (two applicants, the admin, the new
member) works in their own browser session. It is built from the election walkthrough's helpers
(`../election-walkthrough/api.ts`, `local.ts`, `report.ts`, `web.ts`), so the same rules hold:
real API, real database, step-up codes read back from `EmailLogs`, and a refusal probe for each
rule. A step with no working screen goes through the API and the report lists it as a gap.

## Before the first run

The same as the election walkthrough (`../election-walkthrough/README.md`, "Before the first
run"): PostgreSQL with `GHCAADB_v2`, `GHCAA.API/.env`, the tools on PATH, and this machine's IP on
the Brevo list.

The applicants pay by cash. The run makes sure a "Cash (manual receipt)" method with no gateway is
enabled, creating it or switching it on through the SuperAdmin payment settings when needed
(`../election-walkthrough/payment-method.ts`), and the report says which it did. It stays enabled
afterwards like any manual method; switch it off under Admin, Payment settings if it is not wanted.

## Run it

Double-click `scripts\run-membership-walkthrough.bat` (headed, waits for a key at the end), or:

```bat
scripts\run-workflow-walkthrough.bat membership          rem headless
scripts\run-workflow-walkthrough.bat membership slow     rem headed, 400 ms per action
scripts\run-workflow-walkthrough.bat membership keep     rem leave the borrowed accounts on the test password
scripts\run-workflow-walkthrough.bat restore             rem put the borrowed accounts back
```

By hand, with the API and `npm run start` up: `cd GHCAA.Web; npm run test:e2e:membership`.

The report is `GHCAA.Web/walkthrough-report-membership/index.html` (git-ignored).

## Data

`scenario.json` holds the admin and the SuperAdmin (user ids; both are borrowed onto plus
addresses for the run and put back after), and the two applicants: names, dates, addresses,
academic and career record, whether they apply on the web or through the API, and the outcome.
NID, email and mobile are made fresh from the run time, so runs never clash. Applicant emails are
plus addresses of the sender in `GHCAA.API/.env`, so the email codes land in `EmailLogs` where the
run reads them.

Each applicant's name ends in "(local walkthrough)". The next run archives the earlier ones
through `DELETE api/admin/members/{id}` before it starts, so the approvals queue shows only the
current run.

## What the run covers

Keep this table and the run in step. A change to registration, payment verification, approval,
first login or the member's first pages adds or updates its step in `membership.walkthrough.ts`
and its row here in the same piece of work.

| Section | Steps and refusals checked |
|---|---|
| Clean slate | Earlier walkthrough applicants archived; the cash method found, switched on or created. |
| Application | Applicant one fills the three-step web form (identity, academic and career record, payment, photo, certificate, consent), gets the email code and lands on the login page. Applicant two applies through the API with photo and receipt; a wrong email code is refused; the right one verifies. The status route answers for both. The same person applying again (same NID, email, mobile) gets 409 with the reason. An applicant cannot log in before approval. |
| Payment | Approval is refused while the fee is unpaid. The ledger is shown; the fee is marked received through the API. |
| Approval | The application in the approvals queue and its review panel; approved on the web with the membership number; a second approval refused. Applicant two declined with a reason on the web; a second rejection refused; the rejected applicant cannot log in. |
| First login | The new member logs in with the NID, is made to change the password, sees the dashboard; the old password no longer works; the welcome notification is there; ID card, profile and payments pages shown. |

Not covered yet, each waiting on its build: a screen to verify a declared payment (84.92, the run
uses the API and reports the gap), email verification from the profile (97.1), the applied date and
waiting days on the approvals list (84.95), and a reason on termination (84.97). Not covered
by choice: online payment gateways, the mobile app's registration screens (their own tests), and
bulk import (88.x).
