# Election ballot key procedure

This is the written procedure for the ballot key (TODO 37.13e). It covers who makes the key, where
the file lives, how it is replaced, how it is used at the count and how it is destroyed. The design is
in `docs/specs/023-election-ballot-secrecy-and-standards/spec.md` (FR-001 and FR-050). Incidents
(key lost, key leaked) are in `docs/Elections/09-Election-Security-and-Compliance.md`.

## What the key does

Every vote is sealed by the server as the ballot is cast, before it is stored. The choices reach the
server over TLS and are never written down unsealed. Each ballot gets a one-off AES-256-GCM key, and that key is sealed with the election's RSA public key (RSA-OAEP, 3072 bits, SHA-256). The
server holds only the public key. Nobody can open a ballot until the matching private key is uploaded
at the count.

Whoever holds the private key file can open every ballot of that election. The Returning Officer is a
trusted key custodian, and the key is a single point of trust. Splitting the key between several
officials was considered and not built (spec "Not adopted"). The committee accepts this trust
assumption. It is not an open gap.

## Who makes the key

The Returning Officer, or another official appointed with the `SetBallotKey` permission, makes the
key on the admin election screen before polling opens. The browser generates the pair. The public key
and its SHA-256 fingerprint are saved on the election. The private key downloads once to the
officer's device as a `.pem` file. The server never sees it until the count.

The admin screen warns, before the key is made, that a lost file cannot be recovered. Read the warning.

Check the result:

1. The election shows a fingerprint.
2. The file is on the officer's device and opens as text beginning with `-----BEGIN`.
3. The fingerprint is announced to the committee. Voters see the same fingerprint on the ballot
   screen, so anyone can compare.

Polling cannot open without a key, and the key cannot be changed once polling has opened.

## Where the file lives

- The original is on a device the Returning Officer controls, protected by a screen lock.
- The Association sets the number of sealed copies (default 2, at least 1). The Chief Commissioner
  names the holders from the appointed officials when the key is set. Each holder must be a different
  person and must not be a candidate. Each holder confirms receipt. The app does not yet hold the copy
  count or the confirmations (TODO 37.14a), so the Chief Commissioner records both in the log by hand.
- Each extra copy is another person who can open every ballot. Keep it in a sealed envelope, write down
  who holds it in the log, and destroy every copy together with the original once the recount window has
  closed.
- If the original and every copy are lost, that is a level 3 incident. The election is void and is run
  again.
- Never in email, chat, a shared drive, the repository or a ticket.
- The server does not store it and the app cannot recover it.

Backups and snapshots taken during polling keep the sealed ballots. Whoever holds one of those and the
key file can match every voter to their choices. Treat a polling-day backup as sensitive as the key
(procedure D in the security document).

## Replacing the key

Before polling opens only. A replacement needs a second person with the `Approve` permission, who is
not the person who asked. The second person approves from the officials dashboard or the approvals
screen on mobile. A person cannot approve their own request.

After replacement:

1. Announce the new fingerprint.
2. Destroy the old file and every copy of it.
3. Record the change in the Incident Register if it was caused by a loss or a leak.

Once polling has opened the app refuses a key change. A lost or leaked key at that point is handled
as an incident, not a replacement.

## Using the key at the count

At Counting, the officer uploads the file once. The count needs a second official's approval, and that
official never sees the key. The server uses the key for that one request and does not store it. If
fewer than 10 ballots were cast the app refuses, and the officers count by hand under the regulations.

The count refuses if ballots, receipts and voters marked as voted do not agree. Do not retry with a
different file unless the error says the key is wrong.

## Destroying the key after the declare

Destroy the file once the result is declared and the recount window has closed. The Commission sets the
recount end date in the Election Calendar (ER-02). Do not destroy the key before that date has passed or
while a recount request is open. The app does not yet check either: it holds no recount end date, shows
no warning on the dashboard and does not refuse the destruction record (TODO 37.12r). The Returning
Officer and the Chief Commissioner check the date by hand.

1. Delete every copy of the `.pem` file, including copies in a recycle bin, a downloads folder or a
   synced folder. Delete polling-day backups on the same day (procedure D).
2. Open the election on the officials dashboard. After the phase is Declared, an official with the
   `SetBallotKey` permission sees a "key file destroyed" box. Tick it and confirm with the
   step-up check.
3. The action is saved in the audit chain with who recorded it and when. It can be recorded once. A
   second attempt is refused.

The record is a statement by the official, not proof. The app cannot see the file. The audit entry
carries the key fingerprint so the record can be tied to the key it names.

Recording is on the web officials dashboard only. The mobile app has no such action.

The once-only check is a lock inside one server process. A unique index on the election and
the key-destroyed action in the audit table backs it, so a second instance cannot write a second
record either.

## Who can do what

| Step | Permission | Second person |
|---|---|---|
| Make the first key | `SetBallotKey` | No |
| Replace the key before polling | `SetBallotKey` | Yes, with `Approve` |
| Run the count with the key | `Count` | Yes, with `Approve` |
| Record that the key was destroyed | `SetBallotKey` | No |

## Open points

Decided 2026-10-11. The number of sealed copies is an Association setting (default 2, at least 1). This
replaces the earlier one-copy rule. The app does not yet enforce the copy count or the holder
confirmations (TODO 37.14a). Until it does, the Chief Commissioner records the holders in the log by
hand.

The Commission sets the recount end date for each election and states it in the Election Calendar
(ER-02). The key is destroyed after that date.
