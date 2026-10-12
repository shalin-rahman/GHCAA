"""Runs the same gates the GitHub CI workflows run, in the same order, and prints one line per step.

Full output of each step goes to a log file under the system temp folder.
Exit code is 0 only if every step that ran passed. Nothing here rewrites a file:
the format step only verifies, because `dotnet format` on Windows writes CRLF into
LF files and fails the Linux runner.

Run from the repo root:
    python scripts/ci-local.py                # every gate except the slow ones
    python scripts/ci-local.py --quick        # format, catalog, swagger, type-check, analyze only
    python scripts/ci-local.py --only backend # one of: backend, web, mobile, contract
    python scripts/ci-local.py --e2e          # also the Playwright suite
Format errors that only ask for CRLF are Windows noise and are reported as a note, not a failure.
"""
import argparse
import os
import subprocess
import sys
import tempfile
import time

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
WEB = os.path.join(ROOT, "GHCAA.Web")
MOBILE = os.path.join(ROOT, "GHCAA.Mobile")
API = os.path.join(ROOT, "GHCAA.API")
LOGS = os.path.join(tempfile.gettempdir(), "ghcaa-ci-local")
SWAGGER_OUT = os.path.join(LOGS, "swagger.generated.json")

# (group, name, command, cwd, quick). Same commands as .github/workflows/ghcaa-ci-standard.yml.
STEPS = [
    ("backend", "format-verify", "dotnet format --verify-no-changes", ROOT, True),
    ("backend", "authz-catalog", "python docs/api/authz_catalog.py --check", ROOT, True),
    ("backend", "authz-catalog-tests", "python -m unittest discover -s docs/api -p test_authz_catalog.py", ROOT, True),
    ("backend", "restore-locked", "dotnet restore --locked-mode", ROOT, False),
    ("backend", "vulnerable-packages", "dotnet list package --vulnerable --include-transitive", ROOT, False),
    ("backend", "tests-release", "dotnet test GHCAA.Tests/GHCAA.Tests.csproj --configuration Release", ROOT, False),
    ("contract", "api-build-release", "dotnet build GHCAA.API/GHCAA.API.csproj -c Release", ROOT, True),
    ("contract", "swagger-generate",
     f'dotnet tool run swagger tofile --output "{SWAGGER_OUT}" bin/Release/net9/GHCAA.API.dll v1', API, True),
    ("contract", "swagger-diff",
     f'python docs/api/diff_swagger.py docs/api/swagger.json "{SWAGGER_OUT}"', ROOT, True),
    ("web", "type-check", "npm run type-check", WEB, True),
    ("web", "unit-tests", "npm run test:unit -- --run", WEB, False),
    ("web", "build", "npm run build", WEB, False),
    ("web", "audit", "npm audit --audit-level=high", WEB, False),
    ("mobile", "analyze", "flutter analyze", MOBILE, True),
    ("mobile", "tests", "flutter test --exclude-tags golden --reporter expanded", MOBILE, False),
]

E2E = ("web", "e2e", "npm run test:e2e", WEB, False)


def run(step):
    group, name, cmd, cwd, _ = step
    log = os.path.join(LOGS, f"{group}-{name}.log")
    start = time.time()
    with open(log, "wb") as out:
        code = subprocess.run(cmd, cwd=cwd, shell=True, stdin=subprocess.DEVNULL,
                              stdout=out, stderr=subprocess.STDOUT).returncode
    text = open(log, "rb").read().decode("utf-8", "replace")
    # `dotnet list package --vulnerable` exits 0 on a finding, so the CI greps its text.
    if name == "vulnerable-packages" and "has the following vulnerable packages" in text:
        code = 1
    if name == "format-verify" and code != 0:
        code = format_code_ignoring_windows_eol(text)
    return code, time.time() - start, log


def format_code_ignoring_windows_eol(text):
    """Windows `dotnet format` wants CRLF where the Linux runner wants LF. An error whose only
    fix is a CRLF newline is that difference, not a real finding, so it does not fail the run."""
    errors = [l for l in text.splitlines() if ": error " in l]
    real = [l for l in errors if "with '\\r\\n" not in l]
    if errors and not real:
        print(f"      note: {len(errors)} format errors are Windows CRLF only; the Linux runner is the judge")
        return 0
    return 1


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--quick", action="store_true")
    ap.add_argument("--only", choices=["backend", "web", "mobile", "contract"])
    ap.add_argument("--e2e", action="store_true")
    args = ap.parse_args()

    os.makedirs(LOGS, exist_ok=True)
    steps = STEPS + ([E2E] if args.e2e else [])
    if args.quick:
        steps = [s for s in steps if s[4]]
    if args.only:
        steps = [s for s in steps if s[0] == args.only]

    failed = []
    for step in steps:
        code, secs, log = run(step)
        print(f"{'PASS' if code == 0 else 'FAIL'}  {step[0]}/{step[1]:<22} {secs:5.0f}s", flush=True)
        if code != 0:
            failed.append((step, log))
            print(f"      log: {log}", flush=True)

    print(f"{len(steps) - len(failed)}/{len(steps)} steps passed")
    return 1 if failed else 0


if __name__ == "__main__":
    sys.exit(main())
