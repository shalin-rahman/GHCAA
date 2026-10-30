# Membership lifecycle and authentication tasks

This breakdown covers the general account-security work in TODO 84.51. Election-only device checks and election business rules remain in the election delivery plan. Keep item status and acceptance in `docs/TODO.md`.

## Sequence

- [ ] **84.51 — Lock password setup.** Count wrong setup passwords against the same account lockout as login; record successful setup activity; add the owner-approved admin unlock for any account, including member and system-admin accounts.
- [ ] **84.51 — Make authenticator failure locking atomic.** Replace the read-then-write attempt update with one relational update so parallel guesses cannot bypass the lock; use the existing SQLite-backed test fixture.
- [ ] **84.51 — Apply selected recovery rules.** An admin unlock clears both login and authenticator-app locks. Make `ResetByAdminAsync` one transaction that removes the authenticator and rotates the security stamp.
- [ ] **84.51 — Fix shared edge cases.** Remove the unused authenticator dependency from `RequestStepUp`; handle a null first academic-history entry; charge the news-upload quota only after a successful save; return a conflict from setup only for the already-configured case; narrow payment-config exception handling.
- [ ] **84.51 — Close missing generic tests.** Add only cases not already covered for authenticator reset/setup, public family admin access, upload-quota failure, and email-method rejection of recovery-code-shaped input. Leave the proposer-is-candidate test in the election plan.

## Completion evidence

- Relational tests cover lockout under concurrent failures and unlock clearing both locks.
- A reset failure cannot leave only one of authenticator deletion or security-stamp rotation committed.
- Existing tests are checked before adding new cases; all changes retain the tracker’s FR tags and acceptance evidence.
