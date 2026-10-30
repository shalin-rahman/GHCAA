# Profile, family, and identity tasks

This breakdown covers active non-election work linked to spec 013. Tracker status, dependencies, and acceptance remain in `docs/TODO.md`.

## Sequence

- [ ] **84.21 — Remove duplicate family workflow.** Move name search to `FamilyLinkService` or name a real caller for the duplicate methods; preserve existing family-link contracts and extend tests only where coverage is missing.
- [ ] **84.23 — Close the Mobile profile/family gaps.** Trace the two gaps identified by the tracker through spec 013 and add parity using existing shared widgets and services.
- [ ] **97.1 — Add email verification to profile.** Clear verification when the address changes; reuse the registration OTP purpose and attempt lock; add profile state and verification action in Web and Mobile; ensure social login does not link an unverified address.
- [ ] **97.1 — Verify boundaries and evidence.** Cover wrong, expired, and replayed OTPs, attempt lock, changed-address checks, sign-in availability while unverified, and social-link prevention. Update API registry and feature documentation if contracts change.

## Scope notes

- Keep academic and organisation profile validation in spec 005.
- Do not include ballot-key or election-step-up behavior in this task file.
- Use the existing shared profile and confirmation patterns; do not introduce a second OTP service.
