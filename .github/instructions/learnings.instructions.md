---
description: Evidence boundaries for GHCAA requirements, NFRs, and research claims
---

# GHCAA evidence boundaries

## Learnings

- Treat Work Package 85 as documentation currency, not as a completed concurrency or observability research study. `NFR-R5` proves a serializable transaction and conditional update for vote integrity, while the current test proves sequential replay rejection, not simultaneous high-contention requests.
- Do not describe election voting as “proven concurrency-safe under contention” unless the repository contains a concurrent load or race test with measured results. Use the narrower claim “database-enforced vote-integrity mechanism” until that evidence exists.
- Treat `NFR-R6` as evidence of a bounded 256 KB rotating mobile diagnostic log and report attachment. Do not claim that it establishes diagnostic usefulness, privacy safety, retention-policy optimality, or operational impact without a separate evaluation.
- When proposing GHCAA research topics, separate three layers: implemented mechanism, executable verification, and empirical research evidence. A passed unit test supports the second layer; it does not automatically support the third.
- The proposal describes GHCAA as a reusable platform foundation, not a proven plug-and-play multi-tenant product. Authentication, authorisation, accounting, payment records, audit, file handling, notifications, organisation configuration, shared client controls, testing and deployment patterns are reusable capabilities; membership, constitutional, election, branding and content rules remain organisation-specific and require adaptation and testing.
- Proposal claims about testing must distinguish requirement coverage, suite results and code coverage. The repository does not maintain one authoritative combined coverage percentage, so report Coverlet backend statement/branch coverage and requirement-to-test traceability separately rather than inventing a headline number.
- The optional AI-agent extension is future work unless time and evidence permit it. It must remain advisory and tool-restricted, use existing authentication, authorisation and privacy boundaries, and never approve memberships, verify payments, modify accounting records or decide election eligibility.
