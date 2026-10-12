"""Runs every check an election change needs and prints one line per step.

Full output of each step goes to a log file under the system temp folder, so a
failing step can be read without rerunning it. Exit code is 0 only if every step
that ran passed.

Run from the repo root:
    python scripts/verify-election.py              # election tests on all three clients, then tracker checks
    python scripts/verify-election.py --full       # whole backend suite instead of the election filter
    python scripts/verify-election.py --only web   # one of: backend, web, mobile, docs
"""
import argparse
import os
import re
import shutil
import subprocess
import sys
import tempfile
import time

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
WEB = os.path.join(ROOT, "GHCAA.Web")
MOBILE = os.path.join(ROOT, "GHCAA.Mobile")
LOGS = os.path.join(tempfile.gettempdir(), "ghcaa-verify")

BACKEND_FILTER = "FullyQualifiedName~Election|FullyQualifiedName~BallotSeal|FullyQualifiedName~Authorization"

# Specs that touch elections. Keep in step with new election screens.
WEB_SPECS = [
    "src/app/admin/election-personas/admin-election-personas.spec.ts",
    "src/app/admin/elections/admin-elections.spec.ts",
    "src/app/admin/elections/election-officials/election-officials.spec.ts",
    "src/app/admin/elections/election-rules-unlock/election-rules-unlock.spec.ts",
    "src/app/core/services/elections.service.spec.ts",
    "src/app/member/election/election.spec.ts",
    "src/app/officials/dashboard/officials-dashboard.spec.ts",
    "src/app/officials/my-appointments/my-appointments.spec.ts",
    "src/app/public/elections/election-docs.spec.ts",
    "src/app/public/elections/elections.spec.ts",
    "src/app/public/elections/election-results.spec.ts",
]

MOBILE_TESTS = [
    "test/election_service_test.dart",
    "test/election_screen_test.dart",
    "test/officials_dashboard_screen_test.dart",
    "test/officials_approvals_screen_test.dart",
]


def tool(name):
    # npx and flutter are .cmd/.bat shims on Windows; which() finds the real file.
    found = shutil.which(name)
    if not found:
        raise FileNotFoundError(f"{name} is not on PATH")
    return found


def summary(text, patterns):
    text = re.sub(r"\x1b\[[0-9;]*m", "", text)
    for line in reversed(text.splitlines()):
        if any(re.search(p, line) for p in patterns):
            return line.strip()
    return ""


def run(name, cmd, cwd, patterns):
    log = os.path.join(LOGS, f"{name}.log")
    start = time.time()
    try:
        p = subprocess.run(cmd, cwd=cwd, capture_output=True, text=True, encoding="utf-8",
                           errors="replace", stdin=subprocess.DEVNULL)
        out, code = p.stdout + p.stderr, p.returncode
    except FileNotFoundError as e:
        out, code = str(e), 127
    with open(log, "w", encoding="utf-8") as f:
        f.write(out)
    ok = code == 0
    if ok:
        note = summary(out, patterns)
    else:
        tail = [l for l in out.strip().splitlines() if l.strip()][-3:]
        note = summary(out, patterns) or " | ".join(tail)
    print(f"{'PASS' if ok else 'FAIL'}  {name:<16} {time.time() - start:5.0f}s  {note[:150]}")
    if not ok:
        print(f"      log: {log}")
    return ok


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--full", action="store_true", help="run the whole backend suite")
    ap.add_argument("--only", choices=["backend", "web", "mobile", "docs"])
    args = ap.parse_args()
    os.makedirs(LOGS, exist_ok=True)

    steps = []
    if args.only in (None, "backend"):
        cmd = ["dotnet", "test", "GHCAA.Tests", "--nologo", "-v", "q"]
        if not args.full:
            cmd += ["--filter", BACKEND_FILTER]
        steps.append(("backend-tests", cmd, ROOT, [r"Passed!|Failed!|Total tests|error"]))
    if args.only in (None, "web"):
        steps.append(("web-typecheck", [tool("npx"), "tsc", "-p", "tsconfig.app.json", "--noEmit"], WEB, [r"error TS"]))
        cmd = [tool("npx"), "ng", "test", "--watch=false"]
        for spec in WEB_SPECS:
            cmd += ["--include", spec]
        steps.append(("web-specs", cmd, WEB, [r"Tests\s+\d+", r"Test Files"]))
    if args.only in (None, "mobile"):
        steps.append(("mobile-analyze", [tool("flutter"), "analyze"], MOBILE, [r"No issues found|issues? found"]))
        steps.append(("mobile-tests", [tool("flutter"), "test", *MOBILE_TESTS], MOBILE, [r"All tests passed|Some tests failed|\+\d+"]))
    if args.only in (None, "docs"):
        py = sys.executable
        steps.append(("tracker-page", [py, "docs/book/build/tracker_page.py"], ROOT, [r"\S"]))
        steps.append(("wbs-check", [py, "docs/book/build/wbs.py", "--check"], ROOT, [r"\S"]))

    results = [run(*s) for s in steps]
    print(f"{sum(results)}/{len(results)} steps passed")
    sys.exit(0 if all(results) else 1)


if __name__ == "__main__":
    main()
