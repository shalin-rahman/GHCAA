"""Regenerate the work-breakdown evidence for Chapter 11 from git and the tracker.

Chapter 11 quotes durations, task counts and an arrival profile. None of it is
typed in by hand: this script derives all of it from the repository, so an
examiner can re-run it and a later reader can see whether it still holds.

    python docs/book/build/wbs.py                 # the tables, to stdout
    python docs/book/build/wbs.py --markdown      # the same, as Markdown tables
    python docs/book/build/wbs.py --check         # exit non-zero if a component
                                                  # has no evidence at all

Three things are measured, and they are not interchangeable:

  duration     apportioned commit-days. For each day with a commit, that day is
               divided equally between the components it touched, so the parts
               sum to the days actually worked instead of counting one day
               several times over.
  tasks        numbered items in docs/TODO.md, by the area they belong to.
  arrival      whether an area was planned, or arrived as stakeholder feedback,
               a defect, or a review finding.

The limits are as important as the figures, and Chapter 11 states them:

  * A commit-day is a lower bound on effort. Reading, debugging and thinking
    leave no commit.
  * Git dates a document when it was committed, not when the work happened.
    For code that gap is small. For the research stream it inverts the
    schedule: the requirements documents were committed months after the
    elicitation they record.
  * Elicitation, interviews, review sessions and incident response produced no
    commits at all. They are listed here with no duration, and the figure has
    to come from the author.
"""

import argparse
import collections
import io
import math
import os
import re
import subprocess
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
REPO = os.path.dirname(os.path.dirname(os.path.dirname(HERE)))
TODO = os.path.join(REPO, "docs", "TODO.md")

# (id, activity, tracker areas, paths, predecessors). Paths decide the
# duration; areas tie the activity to the tracker; predecessors are the
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
    ("C3", "Registry and membership", ["2", "32", "35", "46", "49"],
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
    ("C17", "Standards, docs and dissertation", ["12", "23", "61", "63", "64", "65"],
     ["docs"], ["C15", "C16", "C14"]),
]

# The documentation deliverables, measured the same way.
DOCS = [
    ("D1", "Requirements specification",
     ["docs/SRS.md", "docs/FEATURES.md", "docs/BUSINESS_FINDINGS.md",
      "docs/BUSINESS_FUNCTIONALITY_REVIEW_PLAN.md"]),
    ("D2", "Governing-document set",
     ["docs/Elections", "docs/CONSTITUTION_PUBLISHING.md"]),
    ("D3", "Design description",
     ["docs/architecture_data_flow.md", "docs/project_map.md",
      "docs/PROFILE_SHARED_COMPONENT_DESIGN.md", "docs/CONFIG_DRIVEN_FRAMEWORK.md",
      "docs/PAYMENT_GATEWAY_WORKFLOW.md", "docs/UI_UX_REMEDIATION_PLAN.md"]),
    ("D4", "Project plan and change log", ["docs/TODO.md", "docs/PLAN.md"]),
    ("D5", "Test and validation plan",
     ["docs/BUSINESS_TEST_CHECKLIST.md", "docs/low_coverage_report.md",
      "docs/PHASE4_FINAL_REVIEW.md"]),
    ("D6", "Deployment runbook",
     ["docs/RENDER_DEPLOYMENT.md", "docs/appsettings.txt", "docs/deploy_connection.txt"]),
    ("D7", "Dissertation", ["docs/book", "docs/DOCUMENTATION_BOOK_OUTLINE.md"]),
    ("D8", "White-label plan", ["docs/GENERICIZATION_PLAN.md"]),
]

# Work that left no commit of its own. The durations here are calculated
# assumptions, not measurements, and each carries the arithmetic that produced
# it so a reader can reject the rate and redo the sum. The additive flag is the
# honest part: incident response is already inside the measured commit-days,
# because the fixes were commits, so only its diagnosis time is new.
#
# (id, activity, basis, hours, additive)
ASSUMED = [
    ("U1", "Requirements elicitation: interviews",
     "10 participants x 35 min contact (5.8h) + 3 role guides at 1h (3h) + write-up at 1x contact (5.8h)",
     14.6, True),
    ("U2", "Governing-document analysis",
     "43,000 words (constitution 5,239 + 8 election documents about 38,000) at 1,500 words/h for "
     "clause-by-clause classification (28.7h), + 16 domain-constraint entries at 15 min (4h)",
     32.7, True),
    ("U3", "Formal technical review sessions",
     "2 sessions (3 and 29 July 2026) x (2h preparation + 2h session + 1h logging); the output was "
     "the 5 specification defects of Table 3.8",
     10.0, True),
    ("U4", "Stakeholder discussion",
     "18 feedback areas x 30 min of discussion; triage into tracker items is excluded, being "
     "already counted under D4",
     9.0, True),
    ("U5", "Deployment incident response",
     "4 dated incidents x 2h diagnosis before the first fix commit. Not additive: those four dates "
     "carry 9, 7, 10 and 2 commits, among the busiest in the project, so the fix work already sits "
     "inside the measured commit-days",
     8.0, False),
]

HOURS_PER_DAY = 8.0

ITEM = re.compile(r"^\s*(\d+)\.(\d+[a-zA-Z]?)\s*\[([^\]]*)\]", re.M)
AREA_HEAD = re.compile(r"^#+ *(?:AREA|Area) (\d+)[^\n]*", re.M)

DEFECT = re.compile(r"\b(bug|broken|500|error|fix before|live-site|issue)", re.I)
FEEDBACK = re.compile(r"raised by user|raised by \"", re.I)
REVIEW = re.compile(r"\b(review|audit)\b", re.I)


def commit_days(paths):
    """The set of dates carrying a commit that touched any of these paths."""
    out = subprocess.run(["git", "log", "--pretty=%ad", "--date=short", "--"] + list(paths),
                         capture_output=True, text=True, cwd=REPO)
    return set(out.stdout.split())


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
    """(tasks, done, arrival) keyed by area number, read from docs/TODO.md."""
    text = io.open(TODO, encoding="utf-8").read()
    heads = [(m.start(), m.group(1)) for m in AREA_HEAD.finditer(text)]
    tasks, done, arrival = collections.Counter(), collections.Counter(), {}
    for index, (pos, area) in enumerate(heads):
        end = heads[index + 1][0] if index + 1 < len(heads) else len(text)
        body = text[pos:end]
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
    return tasks, done, arrival


def critical_path(duration, predecessors):
    """Forward and backward pass. Returns per-activity ES, EF, LS, LF, float."""
    order = []

    def visit(node):
        if node in order:
            return
        for parent in predecessors[node]:
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
    duration = {key: max(1, int(math.ceil(share[key]))) for key, _n, _a, _p, _pr in CODE}
    predecessors = {c[0]: c[4] for c in CODE}
    schedule, finish = critical_path(duration, predecessors)
    tasks, done, arrival = tracker()

    name = {c[0]: c[1] for c in CODE}
    areas = {c[0]: c[2] for c in CODE}

    bar = "| " if markdown else ""
    sep = " | " if markdown else "  "
    end = " |" if markdown else ""

    print("## Code components: duration, schedule and tracker link\n")
    if markdown:
        print("| ID | Activity | Dur | ES | EF | LS | LF | Float | Pred | Tasks | Done | Areas |")
        print("|---|---|---|---|---|---|---|---|---|---|---|---|")
    for key, _n, _a, _p, _pr in sorted(CODE, key=lambda c: schedule[c[0]][0]):
        es, ef, ls, lf, float_ = schedule[key]
        t = sum(tasks[a] for a in areas[key])
        d = sum(done[a] for a in areas[key])
        row = [key, name[key], duration[key], es, ef, ls, lf, float_,
               ",".join(predecessors[key]) or "-", t, d, " ".join(areas[key])]
        print(bar + sep.join(str(x) for x in row) + end)

    print("\ncritical path: %s = %d working days"
          % (" -> ".join(k for k in duration if schedule[k][4] == 0), finish))
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
        print("| Arrival | Areas | Share | Tasks | Share |")
        print("|---|---|---|---|---|")
    for kind in ("planned", "feedback", "defect", "review"):
        print(bar + sep.join([kind, str(counts[kind]), "%.0f%%" % (100.0 * counts[kind] / total_a),
                              str(task_counts[kind]), "%.0f%%" % (100.0 * task_counts[kind] / total_t)]) + end)
    reactive = total_a - counts["planned"]
    reactive_t = total_t - task_counts["planned"]
    print("\nreactive: %d of %d areas (%.0f%%), %d of %d tasks (%.0f%%)"
          % (reactive, total_a, 100.0 * reactive / total_a,
             reactive_t, total_t, 100.0 * reactive_t / total_t))

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

    total = len(every) + len(devery) + additive
    print("\nmeasured code commit-days     : %d" % len(every))
    print("measured document commit-days : %d" % len(devery))
    print("assumed, additive             : %d" % additive)
    print("project effort, all streams   : %d days, about %.1f person-months at 22 days"
          % (total, total / 22.0))

    return duration, tasks, areas


def main(argv=None):
    ap = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    ap.add_argument("--markdown", action="store_true", help="emit Markdown tables")
    ap.add_argument("--check", action="store_true",
                    help="exit non-zero if a component has no commits or no tracker areas")
    args = ap.parse_args(argv)

    duration, tasks, areas = report(args.markdown)

    if args.check:
        problems = []
        for key, label, area_list, paths, _pred in CODE:
            if not commit_days(paths):
                problems.append("%s (%s) matched no commits; check its paths" % (key, label))
            if not sum(tasks[a] for a in area_list):
                problems.append("%s (%s) has no tracker items; check its areas" % (key, label))
        mapped = {a for _k, _l, al, _p, _pr in CODE for a in al}
        loose = sorted(set(tasks) - mapped, key=int)
        if loose:
            problems.append("tracker areas not assigned to any component: %s" % ", ".join(loose))
        for line in problems:
            sys.stderr.write("  %s\n" % line)
        return 1 if problems else 0
    return 0


if __name__ == "__main__":
    sys.exit(main())
