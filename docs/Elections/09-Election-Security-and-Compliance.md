# Election threat model, incident procedure and compliance table

**Feature Branch**: 023-election-ballot-secrecy-and-standards
**Tracker**: `docs/TODO.md` item 37.13f
**Status**: Draft, not yet reviewed or signed off
**Standards**: `docs/Elections/00-election-standards-requirements.md` sections 16, 17, 19 and the
section 21 matrix
**Key procedure**: `docs/Elections/11-Election-Key-Procedure.md`
**Review**: `docs/Elections/00-Election-Online-Voting-Review.md`
**Checked against code**: 2026-10-10

Every statement about the code below was read from the files named beside it. A statement that
could not be confirmed from the repository is marked *[to confirm: ...]*.

## Threat model

### What the design is

The system is a trusted-server secret ballot. The voter's browser or app sends the plain choices
over HTTPS. The server checks them, seals them with the Returning Officer's public key and stores
the sealed text (`ElectionService.CastBallotAsync`, `BallotSeal.Seal`). Only the private key, which
the server does not keep, opens them (`ElectionService.CountAsync`). The system is not
end-to-end verifiable. Nobody outside the server can prove from the data alone that the count
matches what voters chose.

### What it protects

- After the count, the stored ballots hold no voter identifier. The count shuffles the opened
  ballots and the tracking codes inside one transaction, and removes the pending rows
  (`CountAsync`). The count then vacuums the two live tables (`VacuumPendingBallotsAsync`).
- The vote rows (`SeatVote`, `VoterRoll.VotedAt`) keep the date only, so they carry no order that
  could be matched to the ballot batch.
- A ballot sealed for one election does not open in another, for elections created after 37.13i
  (`BallotSeal`, `Election.BallotSealVersion`).
- The count stops if ballots, receipts and voters marked as voted do not agree (`integrity`), or
  if fewer than `Elections.MinimumBallotsToCount` ballots exist.
- A second person must approve the count and key replacement, and the approver cannot count
  (`ElectionApprovalService`, TODO 37.13h).
- The private key is not written to disk or the log. The vote and count bodies never reach a log
  line or an activity row (test `AuditLogMiddlewareTests`, TODO 37.13l).

### What it does not protect

- It does not protect against a compromised server while polling is open. The server sees the
  plain choices during the vote request.
- It does not give the voter proof that their own choice was counted as cast. The tracking code
  shows only that some ballot was counted.
- It does not protect against a voter's own compromised device.
- It does not hide a voter's choices from someone holding the private key and the sealed rows
  together with the link described in the next section.
- It does not use threshold custody. One key file is a single point of trust (spec "Known limits",
  "Not adopted").

### Who can link a voter to a ballot

| Who | When | How |
|---|---|---|
| The server operator, or anyone running code on the server | During polling | Reads the plain choices in the vote request, or reads `xmin` on the sealed rows and the vote rows written in the same transaction. |
| Someone holding a backup, snapshot or WAL archive taken during polling, plus the key file | Any time later, with no limit | The same `xmin` match, then the key file opens the sealed rows. |
| Someone holding the key file and live access to the database before the count | Between close and count | Opens the pending rows and uses `xmin`. |
| Someone with database access after the count | After the vacuum | Cannot match voters to ballots. Rows are in shuffled order under one transaction id. *[to confirm: that Neon keeps no copy of the dead rows after VACUUM, for example in point-in-time history]* |
| The Returning Officer alone | Always | Holds the key, but has no voter link without database or backup access. |

The count does not touch backups. The key file must be destroyed after the result is declared and
the recount window has closed. `11-Election-Key-Procedure.md` has the steps.

### What the client is trusted with

- The browser or app builds the ballot request and the printed receipt from the choices the voter
  just confirmed. The server never returns the choices for a tracking code (spec "Receipt and
  print standard").
- The server does not rely on the client for secrecy. It does rely on the client to show the
  candidate list as the server sent it and to send what the voter picked. A tampered client could
  send other choices. Nothing in the app detects this.
- The admin web screen generates the key pair in the browser with Web Crypto (`generateBallotKeyPair` in
  `ballot-key.util.ts`). The officer's own machine is trusted for key generation. The public half is
  sent in `SetBallotKey` and the private half only in the count body. The mobile app does not generate
  keys. It only shows the key fingerprint (checked in the mobile code on 2026-10-11).
- A Content Security Policy limits where page script can send data (`SecurityHeadersMiddleware`,
  TODO 37.13k).

### Where the Render platform boundary sits

- Render runs the API and serves the web app from one service (`docs/RENDER_DEPLOYMENT.md`).
  Render sees all traffic after TLS ends at its edge, and holds the process memory, including the
  private key while a count runs. Render is trusted infrastructure.
- The database is Neon Postgres (`docs/RENDER_DEPLOYMENT.md`). Neon holds the sealed rows, the
  vote rows and any backup it makes. Neon is trusted infrastructure.
- Render's access log shows the time and IP of each vote call, but not the body (spec "Known
  limits"). *[to confirm: Render log retention on the current plan]*
- Uploaded files sit on the container disk and are lost on redeploy
  (`docs/RECOVERY_RUNBOOK.md`). A key file must never be stored there.
- No backup schedule for Neon is recorded in the repository (`docs/RECOVERY_RUNBOOK.md`, Scenario
  2). *[to confirm: Neon plan, backup and point-in-time recovery settings, and who can open them]*
- Deploys run on pushes to `preprod`. The deploy checklist holds changes to environment variables while
  an election is in polling or counting (`docs/DEPLOYMENT_CHECKLIST.md`).

### Circumstances where online voting should not be used

Decided 2026-10-11. The Commission does not use online voting, and records that in its minutes, when:

- the hosting is not secure enough for a secret ballot;
- the key ceremony failed;
- the voter roll cannot be frozen; or
- too many voters lack digital access.

The last is the Commission's judgement. There is no fixed threshold. The handbook gives examples. The
manual ballot (`04-Election-Manual-Ballot.md`) applies instead.

## Incident procedure

The Returning Officer leads. The second approver and the committee chair are told at step 2. Record
each step in the Incident Register (`docs/Elections/02-Election-Operational-Manual.md`).

### A. Key lost

1. Confirm the loss. Ask every official who held a copy. Do not assume it is gone.
2. Tell the chair and the second approver. Write the time and the facts in the Incident Register.
3. If polling has not opened: replace the key from the admin screen. This needs a second person
   (`ReplaceBallotKey`). The new fingerprint is shown to voters. Announce it.
4. If polling has opened: the app refuses a key change (`SetBallotKeyAsync`, `phase-closed`) and
   nothing can open the sealed ballots. The committee decides under the regulations whether to void
   the poll and run a new election. *[to confirm: the regulations do not name this case]*
5. Keep the sealed rows. Do not delete them until the committee decides.
6. Record the decision and the new election id in the register.

### B. Key leaked or copied

1. Treat any copy outside the Returning Officer's control as a leak. Write down who held it and
   when.
2. If polling has not opened: replace the key (step 3 above) and destroy every older copy.
3. If polling is open or closed and no count has run: the leak alone does not give the voter link.
   It does with a backup or database access. List every backup and snapshot taken since polling
   opened (see D). Ask the platform owner for a list of who had database access.
4. If the committee decides the link may exist, the poll cannot be made secret again. Record that
   the secrecy of this election is compromised and let the committee decide whether to void it.
5. After declare, destroy the key file and record the destruction in the register.
6. Revoking an official in the app does not revoke a key copy they hold outside it (spec "Known
   limits"). Replace the key where step 2 allows.

### C. Suspected link between a voter and a ballot

1. Write down what was seen and who reported it. Do not copy the evidence to chat or email.
2. Tell the chair. Do not run the count while the report is open.
3. List the routes to a link from the table above. Check each: backups taken since polling
   opened, who held database access, who held the key file.
4. Check the election activity rows and the audit rows for the count (`ElectionCountRun`) for an
   executor who should not have run it.
5. Keep the log evidence. Render logs and `ErrorLogs` age out (see Logging and retention). Save
   the relevant lines.
6. The committee decides whether the link is real and whether to void the poll. Record the
   decision.

### D. Backup or snapshot taken during polling

1. List every copy: Neon branches, Neon backups, exports and any file made by hand.
2. Treat each copy as equal to the key in sensitivity. Restrict who can open it.
3. Delete it once the result is declared and the recount window has closed, and record the
   deletion.

### E. Poll stopped, or the service is down

1. Note the time and how long the outage lasted.
2. Do not restore an old database over a live election. Restoring loses votes cast since the copy
   and can bring back voters marked as not voted.
3. When the service returns, read the polling window and the turnout. Ballots, receipts and voters
   marked as voted must still agree. The count refuses to run if they do not.
4. If polling closed during the outage, the Chief Commissioner can extend it with the extend-polling
   action. It is decided as follows: up to 24 hours each time, at most twice, only while polling is open,
   with a written reason, a second approver and the step-up check. Voters are told each time. A third
   extension is level 3: the election is void and is run again. The app does not have this action yet
   (TODO 37.1p). Until it does, the polling close time set at setup stands and the committee decides on
   a hand ballot.
5. Record the outage, the decision and any voters who say they could not vote.

### Escalation levels

Decided 2026-10-11. There are three levels.

| Level | Who acts | Cases |
|---|---|---|
| 1 | The official on duty handles it and writes it in the Incident Register. | Any fault that does not touch the key, the votes or the poll. |
| 2 | The Chief Commissioner is told at once. | D (backup or snapshot during polling) and E (poll stopped or service down). |
| 3 | The Commission and the Executive Committee are told. Polling may pause. | A (key lost), B (key leaked) and C (suspected link between voters and choices). |

## Logging and retention

| Source | What it records for the vote and count routes | Retention |
|---|---|---|
| `CorrelationIdMiddleware` request line | Method, path, status, time taken, member id and correlation id for every route except the vote route. The count route is logged this way, with no body or query string. The vote route gets no line. | *[to confirm: Render log retention]* |
| `AuditLogMiddleware`, vote route | One `ElectionVote` activity row: user name, election id, user id, date only (`LogVoteAsync`). No log line. | *[to confirm: no retention rule found for the activity log]* |
| `AuditLogMiddleware`, other non-GET routes | A log line (method, path, user) and an `ApiAction` activity row for a 2xx reply, which covers the count route. | *[to confirm: as above]* |
| `ElectionApprovalService` | Log lines for a request, approval or rejection (election id, user id, action, approval id). A line when a count runs under an approval. An `ElectionCountRun` activity row naming requester, approver and executor. Errors if the key was not cleared or the audit row was not written. | *[to confirm: as above]* |
| `ElectionService.VacuumPendingBallotsAsync` | A warning if a vacuum fails. | *[to confirm: Render log retention]* |
| `ExceptionMiddleware` | An error line with method and path. A row in `ErrorLogs` with request path and method. No body. | `ErrorLogs` rows older than 90 days are deleted on write (`Constants.ErrorLogs.RetentionDays`). |
| Render access log | Time and IP of each call, no body (spec "Known limits"). | *[to confirm: Render log retention]* |
| `SeatVote.VotedAt`, `VoterRoll.VotedAt` | Date only. | Kept with the election, for ten years from the declare. No code path purges election rows (checked 2026-10-11). Purge by request with a second approver is decided and not built (TODO 37.12s). |

Not logged: the vote body, the count body, the private key, the query string
(`AuditLogMiddlewareTests`). `CountAsync` writes no log line of its own for a failed count (wrong key,
integrity, unreadable). The caller sees the error code, and nothing records the attempt apart from
the request line.

## Compliance table

Meets means the code or document named does it. Planned means a TODO item is open. Gap means
nothing covers it.

### Section 21, one row per area of the compliance matrix

The standards document lists fifteen areas in its section 21 matrix. Three have their own tables above
(incident response, disaster recovery, internet voting risk). The rest are here. Each row names a
tracker item or a file. Nothing is marked as met unless the code or a document named shows it.

| Area | Met by | Status |
|---|---|---|
| E-voting principles (CM/Rec(2017)5) | Spec 023 requirements and "Standards applied". No traceability matrix against the Recommendation exists | Partial |
| Voting-system security (VVSG 2.0) | Sealed ballots (`BallotSeal`), count integrity check, test elections (TODO 37.13d), audit chain (TODO 37.13a). No security or usability test report | Partial |
| Information security (ISO/IEC 27001) | No ISMS documentation found in the repository | Gap |
| Cloud PII protection (ISO/IEC 27018) | Voter data sits with Neon and Render. See "Where the Render platform boundary sits". Neon and Render terms not confirmed | Gap, to confirm |
| Accessibility (WCAG 2.2 AA) | `docs/Elections/10-Election-Accessibility-Checklist.md`. The manual run is TODO 37.13g and is not done | Planned |
| Authentication | OTP sign-in and a step-up check on officer actions (`RequireStepUp`). No separate authentication test report | Partial |
| Access control | Election permission flags and `ElectionAccessService`, appointments that end by themselves (TODO 37.12e, 37.12g), reflection test `AuthorizationPolicyReflectionTests` | Meets |
| Ballot secrecy | `BallotSeal`, `PendingBallot`, `BallotReceipt` with no link to a ballot (spec FR-001). The key custodian trust assumption is accepted and recorded in `docs/Elections/11-Election-Key-Procedure.md` | Meets, with the limits in spec "Known limits" |
| Software integrity | Not documented. Deployment goes through the Render hook (`docs/DEPLOYMENT_CHECKLIST.md`). No release hash or signing step exists in `.github/workflows` (checked 2026-10-11) | Gap |
| Auditability | Hash-chained `ElectionAuditEntries` and `GET api/elections/{id}/audit/verify` (TODO 37.13a). Destroying the key is recorded in the chain (spec FR-050) | Meets |
| Verifiability | Voter receipt with a tracking code, and the opened ballots after declare (TODO 37.13c). End-to-end verifiable cryptography is not adopted (spec "Not adopted") | Partial, by decision |
| Availability | Not documented. *[to confirm: Render and Neon plan limits, any DDoS cover]* | Gap, to confirm |

### Section 17, incident response

| Requirement | Met by | Status |
|---|---|---|
| 1. Detection | `ErrorLogs`, request log lines, count integrity check (`CountAsync`). Hash-chained audit trail: `ElectionAuditWriter` and `GET api/elections/{id}/audit/verify` (TODO 37.13a) | Partial |
| 2. Classification | Escalation levels above | Defined 2026-10-11 |
| 3. Containment | Phase change to Counting (`ClosePolling`), key replacement before polling, emergency revoke (`ElectionApprovalService`) | Partial |
| 4. Investigation | `ElectionCountRun` row, approval rows, `ActivityLog`. Audit chain in `ElectionAuditEntries` (TODO 37.13a) | Partial |
| 5. Evidence preservation | Procedure D and step C5 above. No tool preserves logs | Partial |
| 6. Stakeholder notification | Incident Register in the operational manual. No in-app notice | Gap |
| 7. Election-impact assessment | Procedures A to E above | Meets, once signed off |
| 8. Recovery | No restore rule for a live election. Procedure E | Partial |
| 9. Post-incident analysis | Not defined | Gap |
| 10. Corrective action | Tracked in `docs/TODO.md` | Meets |
| Severity levels and escalation | Not defined | Gap |

### Section 19, internet voting risk

| Requirement | Met by | Status |
|---|---|---|
| Assumed threat model | Threat model above | Meets, once signed off |
| Trusted and untrusted components | Trusted server, Returning Officer, Render, Neon, above | Meets |
| Voter-device assumptions | "What the client is trusted with" | Meets |
| Network assumptions | HTTPS, HSTS and CSP (`SecurityHeadersMiddleware`, `UseHsts` in `Program.cs`) | Meets |
| Administrator trust assumptions | Key custodian, two-person actions (`ElectionApprovalService`, TODO 37.13e, 37.13h) | Partial, key procedure open |
| Cloud-provider trust assumptions | "Where the Render platform boundary sits" | Meets, with open confirmations |
| Cryptographic assumptions | RSA public key, AES-GCM with election id as associated data (`BallotSeal`). Full ballot protocol is TODO 37.13e | Partial |
| Residual risks | Spec "Known limits" | Meets |
| Mitigations | Spec "Requirements", this document | Meets |
| Circumstances where online voting should not be used | Placeholder above | Gap, committee decision |
| No claim that encryption, MFA or cloud security alone makes it safe | Spec purpose section and this threat model | Meets |

### Section 16, backup and disaster recovery

| Requirement | Met by | Status |
|---|---|---|
| RTO, RPO, frequency, retention, encryption, integrity checks | Not recorded (`docs/RECOVERY_RUNBOOK.md`, Scenario 2) | Gap |
| Election-state recovery that keeps integrity | Procedure E | Partial |
