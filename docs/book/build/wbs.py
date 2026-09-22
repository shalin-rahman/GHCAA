"""Regenerate the work-breakdown evidence for Chapter 11 from git and the tracker.

Chapter 11 quotes durations, task counts and an arrival profile. None of it is
typed in by hand: this script derives all of it from the repository, so an
examiner can re-run it and a later reader can see whether it still holds.

    python docs/book/build/wbs.py                 # the tables, to stdout
    python docs/book/build/wbs.py --markdown      # the same, as Markdown tables
    python docs/book/build/wbs.py --check         # exit non-zero if a component
                                                  # has no evidence at all

Four things are reported, and they answer different questions:

  effort       apportioned commit-days. For each day with a commit, that day is
               divided equally between the components it touched, so the parts
               sum to the days actually worked instead of counting one day
               several times over. The precedence network runs on this.
  span         first commit to last, per component: how long it stayed open.
               This is Gantt material, not network material — the components
               overlapped almost completely, and a serial pass over their spans
               sums to five times the calendar the project actually ran in.
  tasks        numbered items in docs/TODO.md, by the work package they belong to.
  arrival      whether a work package was planned, or arrived as stakeholder feedback,
               a defect, or a review finding.

Work that came before the first commit is in PRE and is stated, not measured:
the governing documents were studied and the requirements elicited, analysed
and specified before there was a repository to commit them to.

The limits are as important as the figures, and Chapter 11 states them:

  * A commit-day is a lower bound on effort. Reading, debugging and thinking
    leave no commit.
  * Git dates a document when it was committed, not when the work happened.
    For code that gap is small. For the research stream it inverts the
    schedule: the requirements documents were committed months after the
    elicitation they record, which is why PRE exists.
  * The PRE dates are back-scheduled from the first commit in the order the
    work had to happen. They are a reconstruction and the report says so.
"""

import argparse
import collections
import datetime
import io
import math
import os
import re
import subprocess
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
REPO = os.path.dirname(os.path.dirname(os.path.dirname(HERE)))
TODO = os.path.join(REPO, "docs", "TODO.md")
TODO_ARCHIVE = os.path.join(REPO, "docs", "TODO_ARCHIVE.md")


def _tracked_text():
    """TODO.md plus the archive, for functions that count areas/items.

    Closed work packages move to TODO_ARCHIVE.md to keep TODO.md lean
    (see docs/TODO_ARCHIVE.md header). Tracker and remaining-hours counts
    have to see both files, or an archived area's DONE items silently
    drop out of the Chapter 11 tables. sync() stays TODO.md-only: it
    writes marker comments back by byte offset, and those offsets only
    make sense against the real file on disk.
    """
    text = io.open(TODO, encoding="utf-8").read()
    if os.path.exists(TODO_ARCHIVE):
        text += "\n" + io.open(TODO_ARCHIVE, encoding="utf-8").read()
    return text

# (id, activity, tracker work packages, paths, predecessors). Paths decide
# the duration; the packages tie the activity to the tracker; predecessors are the
# author's reading of what had to exist first, not derived from git.
CODE = [
    ("C1", "Persistence and migrations", ["31", "47"],
     ["GHCAA.Infrastructure/Data"], []),
    ("C2", "Authentication and access", ["15", "24"],
     ["GHCAA.API/Controllers/AuthController.cs",
      "GHCAA.API/Controllers/RegistrationController.cs",
      "GHCAA.Infrastructure/Services/AuthService.cs",
      "GHCAA.Infrastructure/Services/OtpService.cs",
      "GHCAA.Infrastructure/Services/TokenService.cs"], ["C1"]),
    ("C3", "Registry and membership", ["2", "32", "35", "46", "49", "83"],
     ["GHCAA.Infrastructure/Services/MemberService.cs",
      "GHCAA.API/Controllers/ProfileController.cs",
      "GHCAA.API/Controllers/AdminController.cs",
      "GHCAA.API/Controllers/MemberImportController.cs",
      "GHCAA.Domain/Models/Member.cs"], ["C1", "C2"]),
    ("C4", "Payments and finance", ["5", "19", "25"],
     ["GHCAA.API/Controllers/FinancialsController.cs",
      "GHCAA.API/Controllers/FinancialLedgerController.cs",
      "GHCAA.API/Controllers/PaymentConfigController.cs",
      "GHCAA.Infrastructure/Services/FinancialService.cs",
      "GHCAA.Infrastructure/Services/FinancialLedgerService.cs",
      "GHCAA.Infrastructure/Gateways"], ["C3"]),
    ("C5", "Events and attendance", ["4"],
     ["GHCAA.API/Controllers/EventsController.cs",
      "GHCAA.Infrastructure/Services/EventService.cs",
      "GHCAA.Infrastructure/Services/IDCardService.cs"], ["C3"]),
    ("C6", "Governance and elections", ["9", "16", "36", "37", "38", "39", "42"],
     ["GHCAA.API/Controllers/GovernanceController.cs",
      "GHCAA.API/Controllers/AdminGovernanceController.cs",
      "GHCAA.API/Controllers/PollController.cs",
      "GHCAA.Infrastructure/Services/GovernanceService.cs",
      "GHCAA.Infrastructure/Services/PollService.cs",
      "GHCAA.Infrastructure/Data/ConstitutionSeeder.cs"], ["C3"]),
    ("C7", "Content and communication", ["3", "34", "50", "59"],
     ["GHCAA.API/Controllers/NewsController.cs",
      "GHCAA.API/Controllers/CommunicationController.cs",
      "GHCAA.API/Controllers/SiteContentController.cs",
      "GHCAA.Infrastructure/Services/NewsService.cs",
      "GHCAA.Infrastructure/Services/CommunicationService.cs",
      "GHCAA.Infrastructure/Services/SiteContentService.cs"], ["C2"]),
    ("C8", "Gallery and albums", ["40", "51", "52", "55"],
     ["GHCAA.API/Controllers/GalleryController.cs",
      "GHCAA.Infrastructure/Services/GalleryService.cs"], ["C5"]),
    ("C9", "Careers and job board", ["6"],
     ["GHCAA.API/Controllers/JobHubController.cs",
      "GHCAA.Infrastructure/Services/JobHubService.cs"], ["C3"]),
    ("C10", "Networking, forum and messaging", ["10", "18"],
     ["GHCAA.API/Controllers/ForumController.cs",
      "GHCAA.API/Controllers/MessagingController.cs",
      "GHCAA.API/Controllers/NetworkingController.cs",
      "GHCAA.Infrastructure/Services/ForumService.cs",
      "GHCAA.Infrastructure/Services/ChatService.cs"], ["C3"]),
    ("C11", "Configuration and white-label", ["28", "62"],
     ["GHCAA.API/Controllers/OrgConfigController.cs",
      "GHCAA.API/Controllers/ThemeController.cs",
      "GHCAA.Infrastructure/Services/OrgConfigService.cs",
      "GHCAA.Infrastructure/Services/ThemeService.cs"], ["C1"]),
    ("C12", "Security and hardening", ["7", "43", "45", "48"],
     ["GHCAA.API/Middleware", "GHCAA.Application/Security",
      "GHCAA.API/Controllers/SecureFilesController.cs",
      "GHCAA.API/Controllers/RolesController.cs"], ["C2"]),
    ("C13", "Web client", ["11", "26", "30", "33", "53", "54", "56", "58"],
     ["GHCAA.Web/src"],
     ["C3", "C4", "C5", "C6", "C7", "C8", "C9", "C10", "C11"]),
    ("C14", "Mobile client", ["1", "8", "60"], ["GHCAA.Mobile/lib"],
     ["C3", "C4", "C5"]),
    ("C15", "Quality and testing", ["13", "14", "17", "20", "21", "22", "27", "29", "44", "57"],
     ["GHCAA.Tests", "GHCAA.Mobile/test", "GHCAA.Web/tests"], ["C12", "C13"]),
    ("C16", "Deployment and operations", ["41"],
     ["Dockerfile", ".github", "docs/RENDER_DEPLOYMENT.md"], ["C13"]),
    ("C17", "Standards, docs and dissertation", ["12", "23", "61", "63", "64", "65", "66", "67", "68", "69", "70", "71", "72", "73", "74", "75", "76", "77", "84", "85", "86"],
     ["docs"], ["C15", "C16", "C14"]),
]

# The documentation deliverables, measured the same way.
DOCS = [
    ("D1", "Requirements specification",
     ["docs/SRS.md", "docs/FEATURES.md", "docs/BUSINESS_FINDINGS.md",
      "docs/BUSINESS_REVIEW_PLAN.md"]),
    ("D2", "Governing-document set",
     ["docs/Elections", "docs/CONSTITUTION_PUBLISHING.md"]),
    ("D3", "Design description",
     ["docs/ARCHITECTURE.md", "docs/PROJECT_MAP.md",
      "docs/SHARED_PROFILE_COMPONENTS.md", "docs/CONFIG_DRIVEN_FRAMEWORK.md",
      "docs/PAYMENT_GATEWAY_WORKFLOW.md", "docs/UI_FIX_PLAN.md"]),
    ("D4", "Project plan and change log", ["docs/TODO.md", "docs/FORUM_PLAN_2026-05.md"]),
    ("D5", "Test and validation plan",
     ["docs/BUSINESS_TEST_CHECKLIST.md", "docs/COVERAGE_SNAPSHOT_2026-05-26.md",
      "docs/BACKEND_REVIEW_2026-07-03.md", "docs/ARCHITECTURE_AUDIT_2026-09.md",
      "docs/materials/57.1-field-coverage-audit.md"]),
    ("D6", "Deployment runbook",
     ["docs/RENDER_DEPLOYMENT.md", "docs/appsettings.txt", "docs/deploy_connection.txt"]),
    ("D7", "Dissertation", ["docs/book", "docs/DOCUMENTATION_BOOK_OUTLINE.md"]),
    ("D8", "White-label plan", ["docs/WHITE_LABEL_PLAN.md"]),
]

# Work that left no commit of its own and runs alongside development rather
# than before it. Elicitation and document analysis used to sit here as U1 and
# U2; they are now P1 and P2 in PRE, where they can carry a calendar placement
# as well as an effort figure, and counting them in both places would inflate
# the total. The durations here are calculated
# assumptions, not measurements, and each carries the arithmetic that produced
# it so a reader can reject the rate and redo the sum. The additive flag is the
# honest part: incident response is already inside the measured commit-days,
# because the fixes were commits, so only its diagnosis time is new.
#
# (id, activity, basis, hours, additive)
ASSUMED = [
    ("U3", "Formal technical review sessions",
     "2 sessions, corrected 2026-09-06 from an earlier claim of 3. docs/BUSINESS_FINDINGS.md's own "
     "row count is unchanged (132) in every commit since 3 July 2026, which is the evidence that the "
     "specification review was one session that day, not two on 3 and 29 July as previously stated — "
     "the 29 July commit that touches the file edits an unrelated mobile-CI row, not a specification "
     "finding. One session (2h preparation + 2h session + 1h logging = 5h), whose output was the 5 "
     "specification defects of Table 3.8; plus the architecture and engineering audit of 4 September "
     "2026 against the brief in docs/materials/REVIEW.md, recorded in "
     "docs/ARCHITECTURE_AUDIT_2026-09.md, at 5h. That second session is a design review rather than a "
     "specification review, and its output was 15 tracker items (82.14-82.28) rather than "
     "specification defects",
     10.0, True),
    ("U4", "Stakeholder discussion",
     "18 feedback work packages x 30 min of discussion; triage into tracker items is excluded, being "
     "already counted under D4",
     9.0, True),
    ("U5", "Deployment incident response",
     "2 confirmed dated incidents, corrected 2026-09-06 from a previously claimed 4. Commit-message "
     "content on 2026-08-27 (9 commits) matches the MigrationBootstrapper legacy-database incident; "
     "2026-08-28 (7 commits) matches two incidents diagnosed and fixed the same day (stale-chunk "
     "caching after deploy, and the AuthService NG0200 bug). No commit-message evidence was found "
     "tying any other date to a deployment incident, so the other two originally-claimed incidents "
     "are dropped rather than carried on an unverified count. 2h diagnosis each before the first fix "
     "commit. Not additive: those two dates carry 9 and 7 commits, among the busiest in the project, "
     "so the fix work already sits inside the measured commit-days",
     4.0, False),
]

HOURS_PER_DAY = 8.0

ITEM = re.compile(r"^\s*(\d+)\.(\d+[a-zA-Z]?)\s*\[([^\]]*)\]", re.M)
# Headings were "Area N" until 2026-09-03 and are being renamed to "Work Package N".
# Both forms parse while the rename runs.
AREA_HEAD = re.compile(r"^#+ *(?:AREA|Area|WORK PACKAGE|Work Package) (\d+)[^\n]*", re.M)

# SR-2 in docs/TODO.md: a work package may state its own schedule facts, which is the
# only evidence available for work that left no commit for git to date.
#     <!-- wbs: component=C17 start=2026-09-02 end=2026-09-03 after=64,65 -->
MARKER = re.compile(r"<!--\s*wbs:\s*([^>]*?)-->")
FIELD = re.compile(r"(\w+)\s*=\s*(\S+)")

DEFECT = re.compile(r"\b(bug|broken|500|error|fix before|live-site|issue)", re.I)
FEEDBACK = re.compile(r"raised by user|raised by \"", re.I)
REVIEW = re.compile(r"\b(review|audit)\b", re.I)


def commit_days(paths):
    """The set of dates carrying a commit that touched any of these paths.

    Git's exit status is checked rather than assumed. Without that, a missing
    git or a path that is not a repository returns nothing, every component
    floors to a one-day duration, and the script prints a complete schedule and
    a critical path that are entirely invented — numbers that go into Chapter 11.
    """
    try:
        out = subprocess.run(["git", "log", "--pretty=%ad", "--date=short", "--"] + list(paths),
                             capture_output=True, text=True, cwd=REPO)
    except OSError as exc:
        raise SystemExit("cannot run git, so no duration here would be evidence: %s" % exc)
    if out.returncode != 0:
        raise SystemExit("git log failed in %s: %s"
                         % (REPO, (out.stderr or "").strip()[:300]))
    return set(out.stdout.split())


# Work that came before the first commit, so git cannot date it. Requirements
# were elicited and the governing documents analysed before there was code to
# commit them against, and the documents recording that work were written up
# months later, which is why git places the research stream after the
# development it governed rather than before it.
#
# The dates are not a diary. They are back-scheduled from the first commit,
# 2026-02-09, in the order the work had to happen, using the effort figures of
# ASSUMED spread over part-time evenings and weekends at the rate the rest of
# the project ran at. Chapter 11 says so where it prints them: a reader may
# challenge the placement, and can check the ordering against the constitution
# version the requirements cite.
PRE = [
    ("P1", "Governing-document study: constitution and election code", 10, [],
     "13,800 words (the constitution plus the 8 election documents, measured directly from "
     "GHCAA.Infrastructure/Data/Seed/constitution.json and docs/Elections/*.md, corrected 2026-09-06 "
     "from an earlier, unsourced figure of 43,000) classified clause by clause into 16 constraints. "
     "The day count below was not independently re-derived from the corrected word count and may be "
     "high for it — confirm or revise"),
    ("P2", "Requirements elicitation: interviews and observation", 5, ["P1"],
     "10 participants at 30-40 min, plus guides and write-up"),
    ("P3", "Requirements analysis and specification", 8, ["P1", "P2"],
     "54 functional and 34 quality requirements, with the conflicts negotiated"),
    ("P4", "Architecture and technology selection", 4, ["P3"],
     "four candidate architectures scored, stack and hosting chosen"),
    ("P5", "Test strategy and initial test-case design", 3, ["P3"],
     "test levels, the definition of done, and the first equivalence classes"),
]

# The first commit in the repository. Everything in PRE ends before it.
DEVELOPMENT_START = datetime.date(2026, 2, 9)


def back_schedule(activities, finish_before):
    """Place PRE activities so the last one ends the working day before the code starts."""
    duration = {a[0]: a[2] for a in activities}
    predecessors = {a[0]: a[3] for a in activities}
    schedule, total = critical_path(duration, predecessors)
    dates = {}
    for key, (es, ef, _ls, _lf, _fl) in schedule.items():
        start = finish_before - datetime.timedelta(days=int((total - es) * 7 / 5.0))
        end = finish_before - datetime.timedelta(days=int((total - ef) * 7 / 5.0))
        dates[key] = (start, end)
    return duration, predecessors, schedule, total, dates


# Reuse-adjusted effort model.
#
# Commit-days are a lower bound on recorded effort and say nothing about how
# much system was delivered. This model answers the other question: what the
# delivered code represents as effort, and why that is not what the calendar
# shows. It is built from measured source size, a stated production rate per
# kind of source, and stated reduction factors for the reuse and tooling used.
#
# The rates differ by kind because the kinds are not comparable. Markup and
# stylesheet lines are produced several times faster than business logic, and
# test code faster still, being repetitive by design. Generated code is excluded
# from the count entirely: EF Core migration designers alone run to 1.7 million
# lines and nobody wrote them.
SOURCE = [
    ("Backend C#, excluding generated migrations", 12,
     [("GHCAA.API", ".cs"), ("GHCAA.Application", ".cs"), ("GHCAA.Domain", ".cs"),
      ("GHCAA.Infrastructure", ".cs"), ("GHCAA.Tools", ".cs"), ("GHCAA.Export", ".cs")]),
    ("Web client: TypeScript, templates, stylesheets", 25,
     [("GHCAA.Web/src", ".ts"), ("GHCAA.Web/src", ".html"), ("GHCAA.Web/src", ".scss")]),
    ("Mobile client: Dart", 18, [("GHCAA.Mobile/lib", ".dart")]),
    ("Automated tests", 20, [("GHCAA.Tests", ".cs")]),
]

# The build scripts are hand-written and their commit days are already measured,
# C17's path filter being `docs`, but their lines are not part of the delivered
# system: nobody in the Association runs them. They are reported on their own so
# the size figure and the commit-day figure cover the same scope, which they did
# not until 2026-09-03. The reduction factors are not applied to them; framework
# scaffolding and prior reuse have no meaning for a stdlib-only script.
INSTRUMENT = [
    ("Dissertation build scripts, Python", [("docs/book/build", ".py")]),
]

# Multiplicative reductions against the nominal figure. Each is a judgement and
# is stated as one; a reader who disagrees can change the factor and redo the
# sum. They multiply rather than add because they compound: a screen built from
# an existing shared control, scaffolded by the framework and finished with an
# assistant is cheaper than any one of those alone would make it.
REUSE = [
    ("Framework scaffolding and code generation", 0.80,
     "generated, not written",
     "EF Core migrations, Angular CLI component and routing scaffolds, Flutter project "
     "structure, OpenAPI plumbing. None of it was written by hand and all of it is in the count"),
    ("Reuse of shared components within the project", 0.85,
     "written once, used on dozens of screens",
     "the shared control set, the token layer, base services and the common admin table and "
     "form patterns, each written once and used across dozens of screens"),
    ("Reuse from the author's own earlier projects", None,
     "measured footprint x the share that came over intact",
     "authentication, the front-end data grid, backend structure and base services, file handling, payment-gateway adapters and deployment configuration, taken from the author's own 2024 projects rather than "
     "designed again. The recency is what makes the factor credible: those projects are .NET Core, "
     "Angular and Flutter, the same technology generation as this platform, so the material "
     "carried over as working code rather than as a design to be reimplemented. This is an accumulated asset, not a windfall: it is the reason the platform "
     "was affordable, and the reason §12.11 records that the affordability does not transfer to an "
     "association without such a person"),
    ("AI-assisted and rapid development tooling", 0.70,
     "drafts generated, then reviewed and corrected",
     "assistant-generated first drafts, refactors and test scaffolds, reviewed and corrected "
     "rather than accepted; the review time is inside the remaining 70 per cent. One block can be "
     "named: the 2,453 lines of service interfaces and DTOs in GHCAA.Application were generated, "
     "not carried over, which is why they are absent from the carried-over table above. The factor "
     "stays a judgement rather than a measurement for the reason the footprint cannot settle it: "
     "assistance was diffuse across the whole codebase, not confined to identifiable modules"),
]

# Work still to do, so the chapter can state a figure for the finished project
# rather than only for the part already delivered. Both lines are counted from
# the tree, not estimated in prose: the sections come from the placeholders the
# book build reports, and the tracker items from the open entries in TODO.md.
# The rates are judgements and are printed with the arithmetic.
REMAINING_RATES = {"P0": 4.0, "P1": 4.0, "P2": 2.0, "P3": 1.0, "P4": 1.0, "none": 2.0}
SECTION_HOURS = 1.5      # writing one unwritten section of a chapter
ARTEFACT_HOURS = 0.5     # drawing one figure or building one table

# The modules carried over from the author's own 2024 projects, and the share of
# them that came across intact. The share is the author's (75 to 80 per cent, so
# 77.5 taken as the midpoint); the footprint is measured from the tree. Together
# they replace what used to be a bare judgement: the prior-reuse factor below is
# derived from these two numbers rather than asserted.
CARRIED = [
    ("Authentication, OTP, tokens and the middleware pipeline",
     [("GHCAA.API/Controllers/AuthController.cs", ""),
      ("GHCAA.API/Controllers/RegistrationController.cs", ""),
      ("GHCAA.Infrastructure/Services/AuthService.cs", ""),
      ("GHCAA.Infrastructure/Services/OtpService.cs", ""),
      ("GHCAA.Infrastructure/Services/TokenService.cs", ""),
      ("GHCAA.API/Middleware", ".cs")]),
    ("Payment-gateway adapters", [("GHCAA.Infrastructure/Gateways", ".cs")]),
    ("File handling, validation and storage",
     [("GHCAA.Infrastructure/Services/LocalFileStorageService.cs", ""),
      ("GHCAA.Infrastructure/Services/FileValidationService.cs", "")]),
    ("Front-end grid, shared controls, core services and layouts",
     [("GHCAA.Web/src/app/common", ".ts"), ("GHCAA.Web/src/app/common", ".html"),
      ("GHCAA.Web/src/app/common", ".scss"),
      ("GHCAA.Web/src/app/core", ".ts"), ("GHCAA.Web/src/app/core", ".html"),
      ("GHCAA.Web/src/app/core", ".scss"),
      ("GHCAA.Web/src/app/layouts", ".ts"), ("GHCAA.Web/src/app/layouts", ".html"),
      ("GHCAA.Web/src/app/layouts", ".scss")]),
    ("Design-token stylesheet", [("GHCAA.Web/src/styles.scss", "")]),
]

# What share of a carried-over module came across without being rewritten.
CARRIED_RATE = 0.775

HOURS_PER_WORKDAY = 8


PLACEHOLDER = re.compile(r"^\s*\*\[", re.M)
OPEN_ITEM = re.compile(
    r"^\s*\d+\.\d+[a-z]?\s*\[TODO\][^\n]*(?:\n(?!\s*\d+\.\d+[a-z]?\s*\[)[^\n]*)*", re.M)
# ONHOLD items are not counted as remaining engineering effort: they're blocked on something
# outside this session (user action, a business decision) rather than schedulable work.
PRIORITY = re.compile(r"\*\*Priority: (P[0-4])")


def remaining():
    """(unwritten sections, planned artefacts, open items by priority)."""
    book = os.path.join(REPO, "docs", "book")
    sections = 0
    for name in sorted(os.listdir(book)):
        if not name.endswith(".md") or not name[0].isdigit():
            continue
        sections += len(PLACEHOLDER.findall(
            io.open(os.path.join(book, name), encoding="utf-8").read()))
    outline = io.open(os.path.join(REPO, "docs", "DOCUMENTATION_BOOK_OUTLINE.md"),
                      encoding="utf-8").read()
    artefacts = 0
    for chapter in range(7, 14):
        head = "## Chapter %d " % chapter
        if head not in outline:
            continue
        start = outline.index(head)
        rest = outline[start + len(head):]
        marker = "\n## "
        stop = rest.index(marker) if marker in rest else len(rest)
        artefacts += len([line for line in rest[:stop].splitlines()
                          if line.startswith(("- Figure ", "- Table ", "- Listings"))])
    items = collections.Counter()
    text = _tracked_text()
    for entry in OPEN_ITEM.findall(text):
        found = PRIORITY.search(entry)
        items[found.group(1) if found else "none"] += 1
    return sections, artefacts, items


def _count(folder, suffix):
    """Lines in one file, or in every file with that suffix under one folder."""
    base = os.path.join(REPO, folder)
    if os.path.isfile(base):
        with io.open(base, encoding="utf-8", errors="replace") as fh:
            return sum(1 for _ in fh)
    skip = {"bin", "obj", "node_modules", "Migrations"}
    lines = 0
    for here, dirs, files in os.walk(base):
        dirs[:] = [d for d in dirs if d not in skip]
        for name in files:
            if suffix and not name.endswith(suffix):
                continue
            with io.open(os.path.join(here, name), encoding="utf-8", errors="replace") as fh:
                lines += sum(1 for _ in fh)
    return lines


def carried():
    """(label, lines) for each carried-over module, and the total."""
    rows = []
    for label, paths in CARRIED:
        rows.append((label, sum(_count(folder, suffix) for folder, suffix in paths)))
    return rows, sum(count for _label, count in rows)


def instrument_size():
    """(label, lines) for research instrumentation, counted the same way."""
    out = []
    for label, roots in INSTRUMENT:
        lines = 0
        for folder, suffix in roots:
            base = os.path.join(REPO, folder)
            for here, dirs, files in os.walk(base):
                # `_to_delete` holds superseded drafts, kept for reference only.
                dirs[:] = [d for d in dirs if not d.startswith("_")]
                for name in files:
                    if not name.endswith(suffix):
                        continue
                    with io.open(os.path.join(here, name), encoding="utf-8",
                                 errors="replace") as fh:
                        lines += sum(1 for _ in fh)
        out.append((label, lines))
    return out


def source_size():
    """(label, rate, lines) per kind of source, counted from the tree now."""
    skip = {"bin", "obj", "node_modules", "Migrations"}
    out = []
    for label, rate, roots in SOURCE:
        lines = 0
        for folder, suffix in roots:
            base = os.path.join(REPO, folder)
            for here, dirs, files in os.walk(base):
                dirs[:] = [d for d in dirs if d not in skip]
                for name in files:
                    if not name.endswith(suffix):
                        continue
                    path = os.path.join(here, name)
                    try:
                        with io.open(path, encoding="utf-8", errors="replace") as fh:
                            lines += sum(1 for _ in fh)
                    except OSError:
                        pass
        out.append((label, rate, lines))
    return out


WORKDAYS_PER_WEEK = 5


def span(days):
    """(first date, last date, working days between) for a set of commit dates.

    Effort and duration are different quantities and the schedule needs both.
    Apportioned commit-days measure effort: how much work a component took. The
    span measures duration: how long it was open, from the first commit that
    touched it to the last. A component worked on across three months by one
    part-time maintainer has a small effort and a long duration, and a network
    built on effort alone produces a critical path that no calendar recognises.
    """
    if not days:
        return None, None, 0
    dates = sorted(datetime.date(*map(int, d.split("-"))) for d in days)
    first, last = dates[0], dates[-1]
    elapsed = (last - first).days + 1
    working = int(round(elapsed * WORKDAYS_PER_WEEK / 7.0))
    return first, last, max(working, len(days))


def apportion(groups):
    """id -> effort-days, splitting each working day between what it touched."""
    days = {key: commit_days(paths) for key, paths in groups.items()}
    every = sorted(set().union(*days.values())) if days else []
    share = collections.Counter()
    for day in every:
        touched = [key for key in days if day in days[key]]
        for key in touched:
            share[key] += 1.0 / len(touched)
    return share, days, every


def tracker():
    """(tasks, done, arrival, stated) keyed by work package number, read from docs/TODO.md.

    `stated` holds whatever a work package declared in its own wbs marker: the
    component it belongs to, and for work that produced no commit, the dates and the packages
    it waited on.
    """
    text = _tracked_text()
    heads = [(m.start(), m.group(1)) for m in AREA_HEAD.finditer(text)]
    tasks, done, arrival, stated = collections.Counter(), collections.Counter(), {}, {}
    for index, (pos, area) in enumerate(heads):
        end = heads[index + 1][0] if index + 1 < len(heads) else len(text)
        body = text[pos:end]
        marker = MARKER.search(body)
        if marker:
            stated[area] = dict(FIELD.findall(marker.group(1)))
        opening = body[:400]
        if DEFECT.search(opening):
            arrival[area] = "defect"
        elif FEEDBACK.search(opening):
            arrival[area] = "feedback"
        elif REVIEW.search(opening):
            arrival[area] = "review"
        else:
            arrival[area] = "planned"
    for match in ITEM.finditer(text):
        area = match.group(1)
        tasks[area] += 1
        if match.group(3).strip().upper().startswith("DONE"):
            done[area] += 1
    # An item numbered for an area whose heading the pattern missed would be
    # counted per component and left out of the arrival totals, understating the
    # denominator of every share reported below. Give it the neutral class
    # rather than dropping it.
    for area in tasks:
        arrival.setdefault(area, "planned")
    if not tasks:
        raise SystemExit("no tracker items matched in %s; the item or heading "
                         "pattern no longer fits the file" % TODO)
    return tasks, done, arrival, stated


def critical_path(duration, predecessors):
    """Forward and backward pass. Returns per-activity ES, EF, LS, LF, float."""
    order = []

    def visit(node):
        if node in order:
            return
        for parent in predecessors[node]:
            if parent not in duration:
                raise SystemExit("%s lists %s as a predecessor and there is no such component; "
                                 "check the CODE table" % (node, parent))
            visit(parent)
        order.append(node)

    for node in duration:
        visit(node)

    es, ef = {}, {}
    for node in order:
        es[node] = max([ef[p] for p in predecessors[node]] or [0])
        ef[node] = es[node] + duration[node]
    finish = max(ef.values())
    lf, ls = {}, {}
    for node in reversed(order):
        successors = [s for s in predecessors if node in predecessors[s]]
        lf[node] = min([ls[s] for s in successors] or [finish])
        ls[node] = lf[node] - duration[node]
    return {n: (es[n], ef[n], ls[n], lf[n], ls[n] - es[n]) for n in duration}, finish


def report(markdown=False):
    share, days, every = apportion({c[0]: c[3] for c in CODE})
    spans = {key: span(days[key]) for key, _n, _a, _p, _pr in CODE}
    # Three quantities, and the network may only be built on one of them.
    #
    #   effort    apportioned commit-days: how much work the component took.
    #   span      first commit to last: how long it stayed open.
    #   observed  the actual calendar dates, printed beside the network.
    #
    # The network runs on effort. Running it on the span looks tempting and is
    # wrong: the components overlapped almost completely, so a serial precedence
    # pass over their spans sums to 1,008 working days across a project that ran
    # for 207 calendar days. The span belongs on a Gantt chart, where bars may
    # overlap, not in a critical-path calculation, where they may not. Chapter 11
    # prints both and says which question each answers.
    duration = {key: max(1, int(math.ceil(share[key]))) for key, _n, _a, _p, _pr in CODE}
    effort = duration
    predecessors = {c[0]: c[4] for c in CODE}
    schedule, finish = critical_path(duration, predecessors)
    tasks, done, arrival, stated = tracker()

    name = {c[0]: c[1] for c in CODE}
    areas = {c[0]: c[2] for c in CODE}

    bar = "| " if markdown else ""
    sep = " | " if markdown else "  "
    end = " |" if markdown else ""

    pre_dur, pre_pred, _ps, pre_total, pre_dates = back_schedule(PRE, DEVELOPMENT_START)
    print("## Before the first commit: research, specification and design\n")
    if markdown:
        print("| ID | Activity | Days | Start | End | Follows | Why that long |")
        print("|---|---|---|---|---|---|---|")
    for key, label, _d, _p, why in PRE:
        started, ended = pre_dates[key]
        print(bar + sep.join([key, label, str(pre_dur[key]), str(started), str(ended),
                              ",".join(pre_pred[key]) or "-", why]) + end)
    print("\n%d working days, back-scheduled to end as the first commit lands on %s. "
          "Git cannot date work that produced no commit, and the documents recording it were "
          "written up months later." % (pre_total, DEVELOPMENT_START))

    print("\n## Code components: effort, observed span, and the effort network\n")
    if markdown:
        print("| ID | Activity | Effort | First | Last | Span | ES | EF | LS | LF | "
              "Float | Pred | Tasks | Done | Packages |")
        print("|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|")
    for key, _n, _a, _p, _pr in sorted(CODE, key=lambda c: schedule[c[0]][0]):
        es, ef, ls, lf, float_ = schedule[key]
        first, last, open_days = spans[key]
        t = sum(tasks[a] for a in areas[key])
        d = sum(done[a] for a in areas[key])
        row = [key, name[key], effort[key], first or "-", last or "-", open_days,
               es, ef, ls, lf, float_,
               ",".join(predecessors[key]) or "-", t, d, " ".join(areas[key])]
        print(bar + sep.join(str(x) for x in row) + end)

    # Ordered by early start, not by the order the components happen to be
    # declared in: the declaration order is only the schedule order while the
    # CODE table stays topologically sorted, which nothing enforces.
    on_path = sorted((k for k in duration if schedule[k][4] == 0),
                     key=lambda k: (schedule[k][0], k))
    print("\ncritical path: %s = %d working days" % (" -> ".join(on_path), finish))
    print("working days evidenced by a code commit: %d" % len(every))

    dshare, ddays, devery = apportion({d[0]: d[2] for d in DOCS})
    print("\n## Documentation deliverables\n")
    if markdown:
        print("| ID | Deliverable | Days active | Effort-days |")
        print("|---|---|---|---|")
    for key, label, paths in DOCS:
        print(bar + sep.join([key, label, str(len(ddays[key])),
                              str(max(1, int(math.ceil(dshare[key]))))]) + end)
    print("\nworking days evidenced by a document commit: %d" % len(devery))

    print("\n## How the work arrived\n")
    counts, task_counts = collections.Counter(), collections.Counter()
    for area, kind in arrival.items():
        counts[kind] += 1
        task_counts[kind] += tasks[area]
    total_a, total_t = sum(counts.values()), sum(task_counts.values())
    if markdown:
        print("| Arrival | Packages | Share | Tasks | Share |")
        print("|---|---|---|---|---|")
    for kind in ("planned", "feedback", "defect", "review"):
        print(bar + sep.join([kind, str(counts[kind]), "%.0f%%" % (100.0 * counts[kind] / total_a),
                              str(task_counts[kind]), "%.0f%%" % (100.0 * task_counts[kind] / total_t)]) + end)
    reactive = total_a - counts["planned"]
    reactive_t = total_t - task_counts["planned"]
    print("\nreactive: %d of %d work packages (%.0f%%), %d of %d tasks (%.0f%%)"
          % (reactive, total_a, 100.0 * reactive / total_a,
             reactive_t, total_t, 100.0 * reactive_t / total_t))

    if stated:
        print("\n## Schedule facts stated by the work package itself (SR-2 markers)\n")
        if markdown:
            print("| Package | Component | Start | End | Follows |")
            print("|---|---|---|---|---|")
        for area, fields in sorted(stated.items(), key=lambda kv: int(kv[0])):
            print(bar + sep.join([area, fields.get("component", "-"),
                                  fields.get("start", "-"), fields.get("end", "-"),
                                  fields.get("after", "-")]) + end)

    print("\n## Work with no commit of its own: calculated assumptions\n")
    if markdown:
        print("| ID | Activity | Basis | Hours | Days | Adds to total |")
        print("|---|---|---|---|---|---|")
    additive = 0
    for key, label, basis, hours, adds in ASSUMED:
        days = int(math.ceil(hours / HOURS_PER_DAY))
        if adds:
            additive += days
        print(bar + sep.join([key, label, basis, "%.1f" % hours, str(days),
                              "yes" if adds else "no, overlaps measured days"]) + end)

    pre_days = sum(a[2] for a in PRE)
    total = len(every) + len(devery) + additive + pre_days
    print("\nmeasured code commit-days     : %d" % len(every))
    print("measured document commit-days : %d" % len(devery))
    print("pre-development, stated       : %d" % pre_days)
    print("assumed, additive             : %d" % additive)
    print("project effort, all streams   : %d days, about %.1f person-months at 22 days"
          % (total, total / 22.0))

    print("\n## What the delivered system represents, before and after reuse\n")
    if markdown:
        print("| Source | Lines | Lines/hour | Hours |")
        print("|---|---|---|---|")
    sizes = source_size()
    hours = 0.0
    total_lines = 0
    for label, rate, lines in sizes:
        hours += lines / float(rate)
        total_lines += lines
        print(bar + sep.join([label, str(lines), str(rate), "%.0f" % (lines / float(rate))]) + end)
    nominal = hours / HOURS_PER_WORKDAY
    print("\nnominal, built conventionally : %.0f hours, %.0f working days" % (hours, nominal))

    print("\nWritten for the research, counted apart from the system:\n")
    if markdown:
        print("| Instrumentation | Lines |")
        print("|---|---|")
    for label, lines in instrument_size():
        print(bar + sep.join([label, str(lines)]) + end)
    print("\nThese lines are not in the figure above and no reduction factor is applied to\n"
          "them. Their commit days are already inside the measured figure, C17's path\n"
          "filter being `docs`, so counting the lines again would count the same work twice.")

    print("\nCarried over from the author's own 2024 projects, measured from the tree:\n")
    if markdown:
        print("| Module | Lines |")
        print("|---|---|")
    rows, carried_lines = carried()
    for label, count in rows:
        print(bar + sep.join([label, str(count)]) + end)
    share = carried_lines / float(total_lines)
    derived = 1.0 - share * CARRIED_RATE
    print("\n%d of %d lines, %.1f%% of the codebase. At the %.0f%% of each module that came over\n"
          "intact, that is %.1f%% of the total saved, so the prior-reuse factor below is %.2f rather\n"
          "than a judgement."
          % (carried_lines, total_lines, 100 * share, 100 * CARRIED_RATE,
             100 * share * CARRIED_RATE, derived))

    print("\nReductions applied to that nominal figure, multiplicatively:\n")
    if markdown:
        print("| Reduction | Factor | Why |")
        print("|---|---|---|")
    adjusted = nominal
    for label, factor, short, _basis in REUSE:
        if factor is None:
            factor = derived
        adjusted *= factor
        print(bar + sep.join([label, "x%.2f" % factor, short]) + end)
    print("\nreuse-adjusted effort         : %.0f working days" % adjusted)
    print("effort evidenced by the record: %d days" % total)
    print("ratio                         : %.1fx" % (adjusted / max(1, total)))
    print("\nThe two figures answer different questions and neither replaces the other. The adjusted\n"
          "figure is what the delivered system represents as work; the evidenced figure is what the\n"
          "record can prove, and a commit-day is a lower bound because reading, debugging and design\n"
          "leave no commit. Chapter 11 reports both and says which part of the gap it cannot separate.")

    sections, artefacts, open_items = remaining()
    print("\n## Still to do, so the figure is for the finished project\n")
    if markdown:
        print("| Work | Count | Rate | Hours |")
        print("|---|---|---|---|")
    left = [("Unwritten chapter sections", sections, SECTION_HOURS,
             "one placeholder per section in chapters 7 to 13"),
            ("Figures and tables not yet made", artefacts, ARTEFACT_HOURS,
             "the artefact lists of chapters 7 to 13 in the outline")]
    hours_left = 0.0
    for label, count, rate, basis in left:
        hours_left += count * rate
        print(bar + sep.join([label, str(count), "%.1f h each" % rate,
                              "%.0f" % (count * rate)]) + end)
    for priority in ("P0", "P1", "P2", "P3", "P4", "none"):
        count = open_items.get(priority, 0)
        if not count:
            continue
        rate = REMAINING_RATES[priority]
        hours_left += count * rate
        print(bar + sep.join(["Open tracker items, %s" % priority, str(count),
                              "%.1f h each" % rate, "%.0f" % (count * rate)]) + end)
    days_left = hours_left / HOURS_PER_WORKDAY
    print("\nThe item rates cover the fix and the test that pins it together, because §3.7's "
          "definition of\ndone requires both: a defect closes against a test that would fail if it "
          "came back. Writing the\ntest is not a separate line here for that reason.")
    print("\nstill to do                   : %.0f hours, %.0f working days" % (hours_left, days_left))
    print("at completion, evidenced      : %.0f days" % (total + days_left))
    print("at completion, reuse-adjusted : %.0f days" % (adjusted + days_left))

    return duration, tasks, areas, stated


DONE_DATE = re.compile(r"\[DONE (\d{4}-\d{2}-\d{2})")


def sync(write=False):
    """Give every unmarked area a wbs marker, from evidence already in the file.

    An area added to the tracker is invisible to the work breakdown until
    something maps it to a component, and until now that meant editing the CODE
    table by hand. This fills the gap from what the area already states: the
    dates come from its own [DONE yyyy-mm-dd] stamps, and the component from the
    paths the area names.

    Nothing is guessed. An area that names no path known to CODE is left without
    a component and reported, so `--check` still fails and a person decides.
    Existing markers are never overwritten.
    """
    text = io.open(TODO, encoding="utf-8").read()
    heads = [(m.start(), m.end(), m.group(1)) for m in AREA_HEAD.finditer(text)]
    listed = {a for _k, _l, al, _p, _pr in CODE for a in al}
    additions, unresolved = [], []
    for index, (pos, head_end, area) in enumerate(heads):
        end = heads[index + 1][0] if index + 1 < len(heads) else len(text)
        body = text[pos:end]
        if MARKER.search(body) or area in listed:
            # Already mapped, by its own marker or by the CODE table. A second
            # statement of the same fact is one more thing to keep in step.
            continue
        votes = collections.Counter()
        for key, _label, _areas, paths, _pred in CODE:
            for path in paths:
                if path in body:
                    votes[key] += body.count(path)
        component = votes.most_common(1)[0][0] if votes else None
        dates = sorted(set(DONE_DATE.findall(body)))
        fields = []
        if component:
            fields.append("component=%s" % component)
        else:
            unresolved.append(area)
        if dates:
            fields.append("start=%s" % dates[0])
            fields.append("end=%s" % dates[-1])
        if not fields:
            unresolved.append(area)
            continue
        additions.append((head_end, area, " ".join(fields)))

    if write and additions:
        for head_end, _area, fields in sorted(additions, reverse=True):
            text = text[:head_end] + "\n\n<!-- wbs: %s -->" % fields + text[head_end:]
        with io.open(TODO, "w", encoding="utf-8", newline="") as fh:
            fh.write(text)
    return additions, sorted(set(unresolved), key=int)


def main(argv=None):
    # A Windows console defaults to cp1252 and cannot encode the section signs
    # and en dashes this output quotes from the tracker. Same fix as build.py.
    for handle in (sys.stdout, sys.stderr):
        try:
            handle.reconfigure(encoding="utf-8", errors="replace")
        except (AttributeError, ValueError):
            pass
    ap = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    ap.add_argument("--markdown", action="store_true", help="emit Markdown tables")
    ap.add_argument("--check", action="store_true",
                    help="exit non-zero if a component has no commits or no tracker work packages")
    ap.add_argument("--sync", action="store_true",
                    help="write a wbs marker into every area that has none, from its own "
                         "[DONE] dates and the paths it names")
    ap.add_argument("--dry-run", action="store_true",
                    help="with --sync, print what would be written and change nothing")
    args = ap.parse_args(argv)

    if args.sync:
        additions, unresolved = sync(write=not args.dry_run)
        for _pos, area, fields in additions:
            print("  area %-3s %s%s" % (area, fields, "" if not args.dry_run else "   (dry run)"))
        if not additions:
            print("  every area already carries a marker")
        for area in unresolved:
            sys.stderr.write("  area %s names no path this script knows; give it a component "
                             "by hand\n" % area)
        return 1 if unresolved else 0

    duration, tasks, areas, stated = report(args.markdown)

    if args.check:
        problems = []
        known = {c[0] for c in CODE}
        for area, fields in sorted(stated.items(), key=lambda kv: int(kv[0])):
            component = fields.get("component")
            if component and component not in known:
                problems.append("area %s names component %s, which is not in CODE"
                                % (area, component))
            for other in [a for a in fields.get("after", "").split(",") if a]:
                if other not in tasks:
                    problems.append("area %s says it followed area %s, which has no items"
                                    % (area, other))
        for key, label, area_list, paths, _pred in CODE:
            if not commit_days(paths):
                problems.append("%s (%s) matched no commits; check its paths" % (key, label))
            if not sum(tasks[a] for a in area_list):
                problems.append("%s (%s) has no tracker items; check its work packages" % (key, label))
        mapped = {a for _k, _l, al, _p, _pr in CODE for a in al}
        # A work package may name its own component in a wbs marker instead of
        # being listed in CODE, which is how packages written from now on are mapped.
        mapped |= {a for a, fields in stated.items() if fields.get("component")}
        loose = sorted(set(tasks) - mapped, key=int)
        if loose:
            problems.append("tracker work packages not assigned to any component: %s" % ", ".join(loose))
        for line in problems:
            sys.stderr.write("  %s\n" % line)
        return 1 if problems else 0
    return 0


if __name__ == "__main__":
    sys.exit(main())
