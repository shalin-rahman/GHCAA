# Platform and operations tasks

This breakdown maps non-election operations work to the tracker. Live service changes remain owner actions. Keep current status, evidence, and acceptance in `docs/TODO.md`.

## Owner and environment gates

- [ ] **7.21** — After DNS is live, configure and verify the canonical `www` redirect using the tracker request and expected Location header.
- [ ] **7.22** — Set preprod `ClientUrl` and allowed origin to the verified preprod host in Render and local preprod configuration; confirm the browser/API origin behavior.
- [ ] **7.26** — Configure the Brevo sender and key in the owner-managed environment, deploy, send a real preprod message, and confirm its EmailLogs row reads Sent.
- [ ] **7.28** — Set the owner-managed S3-compatible storage configuration, deploy, and verify an upload remains available after redeploy.
- [ ] **34.D17–34.D19** — After owner redeploys, inspect startup logs and member cards for schema repair, restored migration history, and academic record display.
- [ ] **84.48 / 96.15** — Reconcile owner-provided environment keys and startup settings, deploy, and record live health evidence without copying secrets into repository files.

## Local and documentation work

- [ ] **12.8–12.9** — Complete the docs/spec audit and obtain the recorded owner decision before moving or deleting files; update references in the same change.
- [ ] **34.D7–34.D8** — Refresh mobile visual baselines as a standalone change and complete the public/admin browser verification after the local role blocker is resolved.
- [ ] **62.6b / 62.41–62.42** — Follow the profile-onboarding and brand-lint prerequisites in TODO and spec 025; only enable blocking lint after preconditions pass.
- [ ] **72.4–72.5** — Observe the first Dependabot run; enable CodeQL only if the repository-publication trigger occurs.

## Evidence rules

- Do not mark a live owner action complete from local configuration or code inspection alone.
- Keep credentials in the owner-managed secret store; do not add values to docs or committed env files.
- Record the exact observable evidence required by each tracker item.
