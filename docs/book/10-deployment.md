# Chapter 10 — Deployment and Operations

*[Chapter not written. The headings below are generated from `docs/DOCUMENTATION_BOOK_OUTLINE.md` and are kept in step with it: `build.py --strict` fails if a section exists in one and not the other.]*

**What this chapter owns.** Whether the Association can run and keep running this platform after
the project ends: deployment, pipeline, migration, secrets, observability, backup, cost and handover.

**What it must not repeat.** Architectural rationale (Chapter 6), the build of the pipeline as
implementation narrative (§7.12), or measured operational results (Chapter 12). §10.6 is the
chapter's centre because it carries the RQ2 evidence; containerisation is support, not subject, and
no section here explains a tool to a reader who could read its manual.

## 10.1 Deployment Architecture, Environments and Containers

*[Not written.]*

*[Not written.]*

*[Not written.]*

## 10.2 Continuous Integration and Continuous Deployment

*[Not written.]*

### 10.2.1 Pipeline stages and the quality gates actually enforced

*[Not written. Brief: formatting, type checking, static analysis, build and test, per workflow]*

### 10.2.2 Security in the pipeline

*[Not written. Brief: what is automated and what is not. Dependabot raises grouped dependency updates against preprod for NuGet, npm, pub, Actions and Docker, so a published advisory against a library this project uses arrives as a pull request rather than waiting to be noticed. Nothing else is automated, and the reasons are separate rather than one excuse: static application security testing was scoped and rejected on cost, CodeQL being free only on a public repository and needing paid GitHub Advanced Security on a private one; dynamic testing has no environment to run against that is not either preprod or production; and the Flutter client would be out of reach of CodeQL in any case, Dart not being one of its languages. The security work of Chapter 8 was therefore done by review and by test, which is a weaker guarantee than a scan and is reported as one, with the remedy costed in §13.2]*

## 10.3 Database Provisioning, Configuration and Secrets

*[Not written.]*

*[Not written.]*

## 10.4 Observability

*[Not written. Brief: logging, monitoring, alerting, error reporting]*

## 10.5 Backup, Recovery, Release and Rollback

*[Not written.]*

*[Not written.]*

## 10.6 Operating Cost, Maintenance and Handover

*[Not written.]*

*[Not written.]*

## 10.7 Summary

*[Not written.]*

## Figures and Tables

*[Not drawn. The outline specifies 10 artefacts for this chapter. Each is added here with its caption as it is made, then `renumber.py --apply` is run.]*

