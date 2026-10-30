# Election ballot key procedure

This is the written procedure for the ballot key (TODO 37.13e). It covers who makes the key, where
the file lives, how it is replaced, how it is used at the count and how it is destroyed. The ballot
protocol section gives the formats, the failure reasons and the test vectors. The design is
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

Owner decision 2026-10-27: polling must stay blocked until the recorded copy count matches the
configured number and every configured holder has confirmed receipt. Until 37.14a is built, the
Commission must check the full custody list by hand before approving polling.

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

## Ballot protocol

The technical detail behind the steps above, taken from the code on 2026-10-23
(`GHCAA.Infrastructure/Services/BallotSeal.cs`, `GHCAA.Web/src/app/core/utils/ballot-key.util.ts` and
the cast and count methods of `ElectionService`). Limits are in `Constants.Elections`.

**Key pair.** RSA, 3072 bits, public exponent 65537, made in the browser with Web Crypto
(`RSA-OAEP`, SHA-256). The public key goes to the server as base64 SubjectPublicKeyInfo (SPKI). The
private key is saved as PKCS#8, either as bare base64 or inside `BEGIN PRIVATE KEY` armour. The server
accepts a public key of 3072 to 4096 bits with exponent 65537 only, and refuses anything else.

**Fingerprint.** Lower-case hex of SHA-256 over the SPKI bytes of the public key. At the count the
server works out the same value from the uploaded private key and refuses the key if the two differ.

**What is sealed.** The ballot's choices as JSON, one entry per seat: `[{"S":<seat id>,"N":[<nomination
ids>]}]`. The voter's id, name and the time are not in it.

**Padding.** The JSON is padded with spaces up to the next multiple of 512 bytes. A body that already
fills a block gets one more block, so the sealed length never shows how many choices a ballot holds
inside a block. JSON ignores the trailing spaces, and the count trims them.

**Sealing.** For each ballot the server makes a new 32-byte AES key and a 12-byte nonce.

1. It encrypts the padded body with AES-256-GCM (16-byte tag) under that key and nonce.
2. It wraps the AES key with RSA-OAEP, SHA-256 for both the hash and MGF1, under the election's public key.
3. It wipes the AES key and the plain body from memory.

**Sealed format, version 2.** `v2:` followed by base64 of the wrapped key (384 bytes for a 3072-bit
key), then the nonce, then the tag, then the ciphertext. A ballot with one 512-byte block is 1,235
characters long. The associated data is the UTF-8 text `ghcaa-ballot:v2:election:<election id>`, so a
ballot copied into another election fails to open. Version 1 had no prefix and no associated data. An
election keeps the version it was created with, and the count refuses a ballot whose prefix does not
match its election.

**Count.** After the integrity check (ballots, receipts and voters marked as voted agree) and the
10-ballot floor, the server opens every pending ballot in one transaction. It shuffles the opened
ballots once, then writes them as ballot rows with new random ids, so the stored order says nothing
about the order of voting. The private key is used for that one request and not stored.

**Failure handling.** The count stops and writes nothing on any failure. The reasons it returns:

| Reason | Meaning |
|---|---|
| `wrong-key` | The file is not a PKCS#8 RSA key, or its fingerprint does not match the election |
| `integrity` | Ballots, receipts and voters marked as voted do not agree |
| `below-threshold` | Fewer than 10 ballots, so the count is by hand |
| `unreadable` | A ballot would not open: changed, sealed for another election, in the wrong format, or not valid JSON |

Only `wrong-key` is fixed by trying another file. Every other reason is an incident under the security
document.

**Test vectors.** `GHCAA.Tests/Services/BallotSealVector.cs` holds a ballot sealed outside .NET with
Python `cryptography` 46, under a throwaway 3072-bit test key that seals nothing real. Election 7,
plaintext `[{"S":1,"N":[2]}]`, fingerprint
`c3679f5a7272d3974469168c0e8cc6932718933071921cd2be96c990eb6e9f2e`. `BallotSealTests` opens it with
the product code, checks that it fails for election 8, and checks the sealed length for bodies of 17,
511 and 512 bytes (1,235, 1,235 and 1,919 characters). A change to the layout, the padding or the
associated data breaks these tests.

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
confirmations (TODO 37.14a). Owner decision 2026-10-27: polling stays blocked until the configured
copy count matches the recorded copies and every configured holder confirms receipt. Until the app
enforces this, the Chief Commissioner records the holders in the log and checks the full custody
list by hand before approving polling.

The Commission sets the recount end date for each election and states it in the Election Calendar
(ER-02). The key is destroyed after that date.
