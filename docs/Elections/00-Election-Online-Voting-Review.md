 # Online Voting Security Review

 ## Purpose

 This document records the security and architecture review of the GHCAA online-voting design described in:

 `docs/Elections/00-Election-Master-Overview.md`

 The current design provides a strong foundation for election administration, voter eligibility, election-scoped permissions, ballot/member separation, auditability, and operational controls.

 However, the current design should **not yet be described as providing fully end-to-end verifiable secret online voting**.

 The principal limitations concern:

 - client-side trust;
- server-side ballot sealing;
- cryptographic protocol specification;
- private-key custody;
- end-to-end verification;
- infrastructure and logging correlation;
- backups and snapshots;
- coercion resistance.

 This review recommends strengthening those areas without unnecessarily changing the existing election workflow.

---

 # 1\. Current strengths

 The existing design contains several good security properties.

---

 # 2\. Primary security concern: server-side ballot sealing

 The current design seals the ballot at the server boundary.

 Conceptually:

```
Voter
  |
  v
Browser / client
  |
  | plaintext ballot exists here
  v
Application server
  |
  | encryption/sealing
  v
Ballot database
```

 This means the system protects the ballot after server-side sealing, but it does not provide protection against a malicious or compromised voting client or application server observing the voter's selections before sealing.

 Therefore the following statement should NOT be made without qualification:

 > Nobody can determine how a particular voter voted.

 The more accurate security property is:

 > The system is designed to prevent ordinary database access, election administration access, and post-election access from associating a voter with their ballot after the ballot has been accepted and sealed.

 A compromised client, malicious application deployment, or compromised server operating before encryption may still observe the plaintext ballot.

 This limitation should be part of the formal threat model.

---

 # 3\. Recommended security-property terminology

 The election documentation should distinguish three different properties.

 ## 3.1 Database ballot secrecy

 Protection against:

 - database administrators;
- database dumps;
- ordinary election officials;
- post-election application administrators;
- accidental database relationships.

 The current architecture can provide this property if the database separation invariants are correctly implemented.

 ## 3.2 Server/application confidentiality

 Protection against:

 - malicious application code;
- compromised application server;
- compromised deployment artifact;
- malicious infrastructure administrator.

 The current architecture does **not** provide this property for the period before server-side ballot encryption.

 This should be stated explicitly.

 ## 3.3 End-to-end verifiability

 A voter should ideally be able to establish:

```
intended vote
     |
     v
encrypted ballot
     |
     v
published election record
     |
     v
included in tally
     |
     v
published result
```

 The current system does not fully provide this property.

 The documentation should therefore avoid describing the system as "end-to-end verifiable" unless such a mechanism is subsequently implemented.

---

 # 4\. Cryptographic protocol must be specified separately

 "Sealed ballot" is not sufficient as a cryptographic specification.

 The implementation needs a dedicated protocol document defining at least:

 - cryptographic algorithm;
- key generation;
- key size;
- encryption mode;
- authentication/integrity protection;
- nonce generation;
- canonical ballot serialization;
- padding, if applicable;
- associated authenticated data;
- ciphertext format;
- key fingerprint;
- key storage;
- key backup policy;
- key rotation policy;
- key destruction;
- decryption procedure;
- failure handling;
- cryptographic test vectors.

 The protocol must be deterministic at the serialization layer even if the encryption itself is randomized.

 For example:

```
Ballot selections
       |
       v
Canonical serialization
       |
       v
Validation
       |
       v
Encryption
       |
       v
Authenticated ciphertext
       |
       v
Ballot record
```

 The exact cryptographic algorithms should be selected and reviewed separately rather than being implied by application terminology.

---

 # 5\. Private-key custody is currently a concentration of trust

 The current design gives the Returning Officer custody of the election's private key.

 Conceptually:

```
Election public key
       |
       +---- published

Election private key
       |
       +---- Returning Officer
```

 This creates a single-person cryptographic trust boundary.

 Two-person approval of a count operation does not completely eliminate this issue because the complete decryption capability remains with one person.

 For higher assurance, the preferred architecture is threshold custody.

 For example:

```
Key Share A ---- Official A
Key Share B ---- Official B
Key Share C ---- Official C

             2-of-3
                |
                v
          decrypt/count
```

 No individual official should possess the complete private key.

 A 2-of-3 or 3-of-5 model is sufficient conceptually; the exact threshold should be selected based on the association's governance requirements.

 If threshold cryptography is intentionally not used, the documentation should explicitly record:

 > The Returning Officer is a trusted cryptographic custodian and possession of the private key constitutes a single-point-of-trust assumption.

---

 # 6\. Anonymous ballot authorization

 Authentication, eligibility, authorization, and ballot secrecy should be treated as separate concepts.

 The desired flow is:

```
1. Authenticate member
          |
          v
2. Verify election eligibility
          |
          v
3. Issue one-time voting authorization
          |
          v
4. Break the identity relationship
          |
          v
5. Cast anonymous ballot
          |
          v
6. Record that voting entitlement was consumed
```

 The authorization used to cast a ballot must not subsequently provide a path back to the ballot.

 The implementation should specifically test that:

```
voting authorization
        X
        |
        v
ballot identifier
```

 cannot be reconstructed.

---

 # 7\. Database anonymity invariant

 The following should become a formal security invariant:

 > No production database relation, index, audit record, application log, or persistent event may provide a recoverable path from a member identity to a ballot identity or ballot contents.

 This should be tested automatically.

 Tests should attempt to reconstruct:

```
Member ID
   |
   +-- direct foreign key
   +-- indirect relation
   +-- event ID
   +-- request ID
   +-- session ID
   +-- audit ID
   +-- timestamp
   +-- IP address
   +-- ballot ID
```

 and fail if a usable correlation path exists.

---

 # 8\. Logging is part of the ballot-secrecy boundary

 Database design alone is insufficient.

 The following systems can accidentally reconstruct voter-to-ballot relationships:

 - reverse-proxy logs;
- application logs;
- authentication logs;
- request IDs;
- tracing systems;
- APM systems;
- crash reporting;
- analytics;
- database query logs;
- provider logs;
- backups;
- snapshots.

 During voting, logs must not contain:

 - ballot contents;
- ballot selections;
- request bodies containing selections;
- member identifiers alongside ballot identifiers;
- authentication identifiers alongside ballot identifiers;
- unnecessary precise timestamps associated with ballot identifiers;
- voting-page analytics that expose voter identity.

 Error handling must also ensure that ballot payloads cannot appear in exception messages or crash reports.

---

 # 9\. Minimize infrastructure correlation

 The system should assume that infrastructure metadata can be more revealing than the application database.

 For example, this is dangerous:

```
22:01:03
member=123
POST /vote
request_id=ABC

22:01:03
request_id=ABC
ballot_id=XYZ
```

 Even if the database contains no MemberID on ballot XYZ, the infrastructure has reconstructed the relationship.

 The production architecture should therefore minimize or eliminate persistent request-level correlation across the identity and ballot stages.

---

 # 10\. Receipt / tracking code

 The tracking code is useful as a submission confirmation, but the documentation should carefully distinguish:

```
"I successfully submitted a ballot."
```

 from:

```
"I can prove to another person how I voted."
```

 The second property can create coercion and vote-buying problems.

 The tracking code should therefore:

 - confirm submission;
- not reveal ballot selections;
- not retrieve the plaintext ballot;
- not provide transferable proof of the voter's choices;
- not expose the voter's identity when publicly checking it.

 If a future design provides stronger voter verification, it should be reviewed specifically for coercion implications.

---

 # 11\. End-to-end verification is currently incomplete

 The current design does not allow a voter or independent observer to cryptographically verify the entire chain:

```
voter intention
      |
      v
encrypted ballot
      |
      v
published ballot set
      |
      v
decryption
      |
      v
tally
      |
      v
result
```

 This is acceptable if the system is explicitly described as a trusted-server voting system.

 It should not, however, be described as a fully end-to-end verifiable voting system.

 A future enhancement could introduce cryptographic proofs for:

 - ballot inclusion;
- ballot validity;
- correct decryption;
- correct tally;
- published result consistency.

 These should be treated as a separate security project rather than added informally to the existing implementation.

---

 # 12\. Coercion resistance

 Remote voting cannot guarantee a coercion-free environment.

 The organisation cannot control whether a voter is:

 - observed while voting;
- instructed how to vote;
- forced to vote in a particular way;
- using a compromised device;
- sharing a screen;
- voting under pressure.

 The system should therefore explicitly state:

 > Online voting cannot guarantee the absence of voter coercion because the organisation does not control the voter's physical environment or client device.

 The non-transferable/non-provable receipt design helps reduce some coercion risks but does not eliminate them.

---

 # 13\. Client compromise

 The security model should explicitly consider:

 - malicious browser extensions;
- malware;
- compromised device;
- modified JavaScript;
- malicious browser;
- phishing page;
- compromised frontend deployment.

 A compromised client may observe the voter's selection before encryption.

 Therefore:

 > Client compromise is outside the secrecy guarantees of the current server-side sealing model.

 This should be documented as a security limitation.

---

 # 14\. Frontend integrity

 Because the voting interface executes code on the voter's device, frontend integrity is security-sensitive.

 Production voting deployments should have controls around:

 - immutable deployment artifacts;
- reviewed releases;
- CI verification;
- deployment commit identification;
- restricted deployment permissions;
- dependency locking;
- dependency auditing;
- content security policy;
- Subresource Integrity where applicable;
- no unnecessary third-party JavaScript;
- no advertising/analytics scripts on voting pages.

 The voting page should contain as little third-party code as possible.

---

 # 15\. Candidate ordering and randomization

 Randomization should use a cryptographically secure random source.

 Randomness must not be derived from:

 - member ID;
- ballot ID;
- database insertion order;
- timestamp;
- predictable PRNG seeds.

 Where randomization affects the published election record, the election should retain sufficient audit information to demonstrate that the process was performed according to the documented procedure without exposing voter identity.

---

 # 16\. Ballot identifiers must not leak identity

 Ballot IDs should be:

 - random;
- non-sequential;
- unrelated to MemberID;
- unrelated to authentication session IDs;
- unrelated to email addresses;
- unrelated to timestamps;
- unrelated to database insertion order where possible.

 Avoid:

```
ballot_id = 10452
```

 if database sequence information can expose voting order.

 Prefer an opaque, cryptographically random identifier.

---

 # 17\. Backup and snapshot threat

 During polling, assume an attacker may obtain:

```
database backup
+
application logs
+
database WAL
+
server snapshot
+
cloud-provider snapshot
+
private key
```

 The combination may reveal more than any individual artifact.

 Therefore the election security model should explicitly cover:

 - automated backups;
- database snapshots;
- provider snapshots;
- disaster-recovery copies;
- application logs;
- private-key backups;
- temporary files.

 The system should avoid creating unnecessary copies of election data during polling.

---

 # 18\. Render/free hosting consideration

 If the application is deployed using a managed platform such as Render, the infrastructure trust boundary must be documented.

 The organisation may not control:

 - underlying host;
- platform administrators;
- infrastructure logs;
- backups;
- snapshots;
- networking infrastructure;
- deployment infrastructure.

 Therefore the threat model should explicitly distinguish:

```
Application-level security
        |
        v
Platform-level trust
        |
        v
Infrastructure-level trust
```

 A managed hosting provider should be considered part of the trusted infrastructure unless the cryptographic design specifically protects against a compromised host.

 A free hosting tier should not be assumed to provide the same operational guarantees as dedicated election infrastructure.

---

 # 19\. Election-day logging policy

 A dedicated election logging policy should define what is retained.

 During polling:

 ### Allowed

 - election lifecycle events;
- configuration changes;
- official approvals;
- system health;
- aggregate counters that cannot identify voters;
- security events that do not expose ballot information.

 ### Prohibited

 - ballot contents;
- candidate selections;
- raw voting request bodies;
- MemberID + BallotID pairs;
- authentication session + BallotID pairs;
- unnecessary voter IP retention;
- third-party analytics on voting pages.

 The exact retention period should be documented.

---

 # 20\. Security tests required before production

 The election system should have automated tests for at least the following.

 ## 20.1 Identity separation

```
Given a cast ballot,
the ballot record cannot identify the voter.
```

 ## 20.2 Reverse lookup

```
Given a MemberID,
the application cannot retrieve their ballot.
```

 ## 20.3 Ballot lookup

```
Given a BallotID,
the application cannot retrieve the voter's identity.
```

 ## 20.4 Log inspection

```
A complete production-like voting flow
does not produce a recoverable voter-to-ballot correlation
in application or proxy logs.
```

 ## 20.5 Backup inspection

```
A polling-period backup does not contain
a direct voter-to-ballot relationship.
```

 ## 20.6 Authorization replay

```
A consumed voting authorization cannot cast another ballot.
```

 ## 20.7 Cross-election isolation

```
An authorization or ballot from Election A
cannot be used in Election B.
```

 ## 20.8 Duplicate voting

```
A member cannot consume two voting entitlements
for the same seat/election.
```

 ## 20.9 Ballot tampering

```
Modification of encrypted ballot data
is detected and rejected.
```

 ## 20.10 Decryption integrity

```
Invalid/tampered ciphertext cannot produce
a valid counted vote.
```

---

 # 21\. Security review artifacts

 Before a production election, retain:

```
evidence/election-security/
  threat-model.md
  cryptographic-protocol.md
  key-custody.md
  database-anonymity-test.txt
  log-anonymity-test.txt
  backup-anonymity-test.txt
  crypto-test-vectors.txt
  dependency-audit.txt
  frontend-integrity.txt
  deployment-commit.txt
  incident-response.md
```

 The evidence should identify the exact application commit used for the election.

---

 # 22\. Recommended architecture

 The preferred security architecture is:

```
                  Member
                    |
                    v
              Authentication
                    |
                    v
             Eligibility check
                    |
                    v
          One-time voting authorization
                    |
                    v
              Voting client
                    |
              ballot selection
                    |
                    v
             ballot encryption
                    |
                    | ciphertext only
                    v
               Ballot store
                    |
                    |
             polling closes
                    |
                    v
          threshold key custody
                    |
                    v
              Decryption
                    |
                    v
            Independent tally
                    |
                    v
             Published result
```

 Separately:

```
Member
  |
  +---- voting entitlement consumed
```

 There must be no persistent path connecting the two branches.

---

 # 23\. Recommended security classification

 The current architecture can reasonably be described as:

 > **Secret-ballot, trusted-server online voting**

 provided the database, application, logging, and key-custody controls are implemented as specified.

 It should not currently be described as:

 > End-to-end verifiable online voting

 because the voter cannot independently verify the complete cryptographic path from their intended vote through inclusion and tally.

 It should also not claim protection against:

 > compromised voting clients or malicious application code before server-side sealing.

---

 # 24\. Priority actions

 ## P0 — Required before production

 1. Specify the complete cryptographic ballot protocol.
2. Document the exact secrecy guarantees.
3. Implement and test strict Member ↔ Ballot separation.
4. Audit application/proxy/infrastructure logging for correlation.
5. Define private-key custody and access controls.
6. Define backup/snapshot security during polling.
7. Add cryptographic integrity protection for ballots.
8. Add automated anonymity and correlation tests.
9. Document client/server trust limitations.
10. Perform a security review of the actual production deployment.

 ## P1 — Strongly recommended

 1. Use threshold private-key custody.
2. Reduce third-party JavaScript on voting pages.
3. Add frontend deployment-integrity controls.
4. Add cryptographic test vectors.
5. Add an election-specific incident-response procedure.
6. Make ballot IDs cryptographically random and opaque.
7. Establish a formal election-day logging policy.

 ## P2 — Future enhancement

 1. End-to-end verifiable ballots.
2. Cryptographic proof of ballot inclusion.
3. Verifiable decryption.
4. Verifiable tallying.
5. Independent election-observer tooling.

---

 # 25\. Final review conclusion

 The existing election design is a good **election administration and workflow foundation**.

 Its strongest architectural idea is the separation between:

```
"this member has voted"
```

 and:

```
"this ballot contains these selections"
```

 That separation should remain the central invariant.

 The main issue is that the current architecture should not overstate what "sealed ballot" provides. Because encryption occurs at the server boundary, the system inherently trusts the voting client and the application server during the pre-encryption portion of the vote.

 For a production deployment, the project should therefore explicitly position the system as a **trusted-server secret-ballot system**, unless and until an end-to-end cryptographic voting protocol is implemented.

 The highest-value next step is not to redesign the election workflow. It is to produce and implement a dedicated cryptographic/security specification covering:

```
Threat model
     +
Anonymous authorization
     +
Ballot encryption
     +
Key custody
     +
Database separation
     +
Logging privacy
     +
Backup privacy
     +
Integrity verification
     +
Auditability
     +
Incident response
```
