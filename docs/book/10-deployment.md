# Chapter 10 — Deployment and Operations

**What this chapter owns.** Whether the Association can run and keep running this platform after
the project ends: deployment, pipeline, migration, secrets, observability, backup, cost and handover.

**What it must not repeat.** Architectural rationale (Chapter 6), the build of the pipeline as
implementation narrative (§7.12), or measured operational results (Chapter 12). §10.6 is the
chapter's centre because it carries the RQ2 evidence; containerisation is support, not subject, and
no section here explains a tool to a reader who could read its manual.

## 10.1 Deployment Architecture, Environments and Containers

The platform runs as one container. The web client is built first and copied into the API's static
folder, so one process serves both the pages and the data. The container runs on Render. The
database is PostgreSQL hosted by Neon, and uploaded files go to an S3 bucket. The mobile client is
built separately and released through the app stores.

There are three places the system runs. On a developer machine it uses SQLite and starts with one
script. The shared test environment, called preprod, uses the hosted services just named. Each pull
request also gets its own copy of the database, made by branching the Neon database, and that copy
expires after fourteen days.

*[Fact needed: whether a production environment separate from preprod exists yet, and its host.]*

The image is built in stages, one for the web client, one for the API and a small one that runs the
result. Base images are pinned by digest, so a rebuild next year starts from the same base as
today. The deployment is limited to a single instance. The output cache and two configuration
caches live in the memory of that one process, and a second instance would serve stale answers. The
decision and its cost are recorded as an architecture decision in §6.10, the ADRs.

## 10.2 Continuous Integration and Continuous Deployment

A change reaches preprod only through the pipeline. Nothing is deployed by hand. This follows the
practice that every change takes the same tested path to release [61].

### 10.2.1 Pipeline stages and the quality gates actually enforced

A push to the preprod branch starts the run. Three lint jobs go first: the backend formatting check
with the authorisation catalogue check, the web client's type check, and the mobile client's static
analysis. Then the backend, web and mobile test suites run, and the web client is built. Last, the
full image is built. Only when all of these pass does the deploy job confirm the database can be
reached and ask Render to build and start the new container.

Pull requests run the same jobs and are never deployed. The workflow for the main branches adds
three gates: a dependency audit that fails on a high-severity advisory, a coverage report, and a
contract check that regenerates the API description and fails if it differs from the committed one.
Every third-party action is pinned to a commit, not to a tag that can be moved.

The end-to-end walkthroughs of §9.6, integration, system and end-to-end testing, are not a gate.
They run on my machine before a hand-back.

### 10.2.2 Security in the pipeline

One security task is automated. Dependabot raises grouped update requests against preprod for the
NuGet, npm, pub, Actions and Docker dependencies. A published advisory against a library this
project uses arrives as a pull request and does not wait to be noticed.

Nothing else is automated, and the reasons are separate. Static security scanning was scoped and
rejected on cost: CodeQL is free only on a public repository, and a private one needs paid GitHub
Advanced Security. Dynamic scanning has no environment to run against other than preprod or
production. The Flutter client would be out of CodeQL's reach in any case, because Dart is not one
of its languages.

The security work of Chapter 8 was therefore done by review and by test. That is a weaker guarantee
than a scan, and I report it as one. The remedy is listed in §13.4.2, technical debt and
reengineering priorities.

## 10.3 Database Provisioning, Configuration and Secrets

The database schema is not updated by a manual step. When the container starts, the application
applies any migration that has not yet run. The choice is recorded as an architecture decision. Its
benefit is that a deploy cannot forget the migration. Its risk is that a bad migration stops the
application from starting, and the recovery runbook has a scenario for that case.

Configuration comes from environment variables on the host, and the pipeline's own values are held
as repository secrets. No connection string or key is committed. Settings an administrator may
change, such as labels and fees, are kept in the database and edited in the application, so
changing them needs no deploy. The keys that protect cookies and tokens are also stored in the
database, because the container's disk is wiped at each deploy.

## 10.4 Observability

The application exposes two health addresses. One only says the process is alive, and the host
probes it. The other checks the database, the file store and the email service and reports each
separately. An operator opens it first when something looks wrong.

Logs go to the container's standard output and are read in the host's log view. Every email the
system tries to send is recorded in a table, which is how a missing one-time code is traced.
Governance actions are written to an audit log, described in §8.8, governance integrity.

There is no alerting and no error-reporting service. Nobody is told when the system fails. Someone
has to look. For a platform with one maintainer and no on-call rota this is a known gap, and
§12.10, limitations of the artefact, lists it.

## 10.5 Backup, Recovery, Release and Rollback

The repository documents no backup schedule and no restore procedure for the database. The recovery
runbook says so in plain words. If the database were lost, the schema, the constitution text and
the default settings would come back on the next start because they are created from code. Every
member record, payment and vote entered since would be gone unless the database host's own plan
keeps a copy. Uploaded files are in the S3 bucket and survive a redeploy.

*[Fact needed: what backup and point-in-time recovery the Neon plan in use provides, checked in the
Neon console, with the date checked. Until then the paragraph above stands as written.]*

A release is a push to the preprod branch that passes the pipeline. After each one the operator
follows a short smoke test: the health address, a sign-in, a member list with photographs and, if
an election is open, its monitor page. The runbook covers three failures: a migration that fails at
start, loss of data and loss of uploaded files.

## 10.6 Operating Cost, Maintenance and Handover

RQ2 asks for an architecture the Association can afford. The single container, the hosted database
and the file bucket were chosen partly to keep the monthly bill small. The alternatives considered
are in §6.2, architectural alternatives considered and the decision taken.

*[Numbers needed: the monthly cost of hosting, database, file storage, email and the app-store
accounts, each from the provider's price list with the date, and the total set against the
Association's stated means. To be filled when the amounts are calculated.]*

Maintenance has two parts. Dependency updates arrive as pull requests and need someone to read and
merge them. Everything else, including new features, fixes and the annual election, needs someone
who knows the code. Today that is one person. The handover material is in the repository: a
deployment checklist, the recovery runbook, a guide to the host set-up and seven architecture
decision records. These let a competent developer take over. They do not replace one.

*[Number needed: an estimate of maintenance hours per month, from the effort record of §11.4,
effort estimation.]*

## 10.7 Summary

The platform runs as one container with a hosted database and a file bucket, and every change
reaches it through the same tested pipeline. Migrations run at start, secrets stay outside the
repository, and the health address tells an operator what is broken. Three things are missing:
automated security scanning, alerting, and a documented database backup. The running cost and the
maintenance effort are still to be calculated.

## Figures and Tables

*[Not drawn. The outline specifies 10 artefacts for this chapter. Each is added here with its caption as it is made, then `renumber.py --apply` is run.]*
