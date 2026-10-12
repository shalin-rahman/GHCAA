# Ballot key reference implementation

A small stand-alone system that shows the ballot key flow from end to end: a key pair made in the
browser, ballots sealed in the browser, and a count that checks the uploaded key against the election
fingerprint before it opens anything. It is written for the officials handbook
(`../12-Election-Officials-Handbook.md`) and the dissertation. It shares no code with the product.

## How it differs from the product

The product seals each ballot on the server. The voter's choices reach the API over TLS, and
`GHCAA.Infrastructure/Services/BallotSeal.cs` seals them as the ballot is cast
(`ElectionService.CastBallotAsync`). This reference seals in the voter's browser, so the server never
sees a choice before the count. The key is made in the browser in both.

| Point | Product | Reference |
|---|---|---|
| Where a ballot is sealed | Server, at the moment of casting | Voter's browser |
| Key pair made | Admin browser | Admin browser |
| Private key | Uploaded once at the count | Uploaded once at the count |
| Associated data | `ghcaa-ballot:v2:election:{id}` | `ghcaa-ref-ballot:election:{id}` |
| Sealed layout | Same: wrapped key, nonce, tag, ciphertext | Same |
| Padding | JSON padded to 512-byte multiples | Same |
| Approvals, phases, audit chain, voter roll, OTP | Yes | No |
| Minimum ballots for an automated count | 10 | 10 |

Browser sealing removes the plaintext-on-the-wire step. It does not remove the key custodian's trust,
and it needs a trusted copy of the page code, since a tampered page could seal the choice to another
key. The fingerprint shown to the voter is the check for that.

## Design

| Item | Choice |
|---|---|
| Ballot cipher | AES-256-GCM, a new key for every ballot, 12-byte nonce, 16-byte tag |
| Key wrap | RSA-OAEP, 3072 bits, exponent 65537, SHA-256 |
| Fingerprint | Lowercase hex SHA-256 of the SPKI DER of the public key |
| Key file | PKCS#8 base64 in PEM armour, downloaded once, never sent until the count |
| Trust | The custodian holding the file can open every ballot. The server holds only the public key |

Sealed value, base64 of: wrapped key (384 bytes) | nonce (12) | tag (16) | ciphertext.

The ballot's associated data names the election, so a sealed ballot copied into another election fails
to open. Ballots are opened in random order. A ballot that will not open, or names an unknown choice,
counts as spoiled.

`ballots` holds no user id. `voters_voted` records who voted, in a separate table, so a stored ballot
cannot be tied to a voter by a join.

## Run it

Needs Node 20 or later and PostgreSQL.

```
createdb ballotref
psql ballotref -f schema.sql
npm install
DATABASE_URL=postgres://postgres:postgres@localhost:5432/ballotref npm start
```

Open `http://localhost:3000/admin.html`.

1. Set up: tick the three acknowledgements, give a title and choices, create. The `.pem` downloads once.
2. Vote: as `voter.one`, `voter.two` and so on. Each user can vote once. Add users in the `users` table.
3. Count: as `returning.officer`, drop the `.pem` file. A matching fingerprint shows a result chart.
   A wrong file is refused before any ballot is read. With fewer than 10 ballots the count is refused.

The demo sign-in reads the `x-user` header. It is not authentication. A real deployment replaces it.

## Endpoints

| Endpoint | Needs | Does |
|---|---|---|
| `POST /api/admin/initialize-election` | `set_ballot_key` | Saves the title, choices and public key. The server recomputes the fingerprint and rejects a key that is not RSA 3072 with exponent 65537 |
| `GET /api/election/:id` | none | Returns choices, public key and fingerprint |
| `POST /api/voter/cast-ballot` | signed in | Stores one sealed ballot, once per voter, while the election is open |
| `POST /api/admin/tally-votes` | `set_ballot_key` | Checks the key fingerprint, opens ballots in memory, stores the result, closes the election |

## Test

```
npm test
```

The tests need no database. They run the browser sealing module in Node and open the result with the
server code, and cover the fingerprint match, replay into another election, a changed ballot, equal
sealed lengths, the tally with spoiled ballots, and the key size check.

## Not covered

No login, no voter roll, no OTP, no audit chain, no two-person approval, no rate limit and no check of
the key file's age or location. The tally request carries the key in the body, so a real deployment
must keep that route on TLS and out of request logs.
