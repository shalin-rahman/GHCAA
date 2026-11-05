"""Checks lines added since HEAD against rules taken from past review findings.

Only new lines are checked, so old code never blocks a run. A review finding that a regex can
detect gets one line in RULES.

    python scripts/guard.py
"""
import fnmatch
import re
import subprocess
import sys

# (path glob, regex, message)
RULES = [
    ("GHCAA.Infrastructure/Data/Migrations/*.cs", r"ON CONFLICT\s+DO",
     "name the conflict target, so an unrelated Id clash fails instead of being skipped"),
    ("GHCAA.Web/src/*.html", r"date:\s*'dd-MM-yyyy'",
     "use the appDate pipe, which follows the configured date format"),
    ("GHCAA.Web/src/*.html", r'placeholder="dd-mm-yyyy"',
     "take the placeholder from the configured date format"),
    ("GHCAA.Web/src/app/*.scss", r"#[0-9a-fA-F]{3,8}\b",
     "use a theme token instead of a literal colour"),
    ("*.cs", r"^\s*//.*—", "no em dash in comments"),
    ("*.ts", r"^\s*(//|\*).*—", "no em dash in comments"),
    ("*.dart", r"^\s*//.*—", "no em dash in comments"),
    (".claude/settings.json", r"AppData\\\\Local\\\\Temp|scratchpad",
     "session-only permission; remove it before handback"),
]


def git(*args):
    return subprocess.run(["git", *args], capture_output=True, text=True, encoding="utf-8").stdout


def added_lines():
    """Yields (path, line) for every line added since HEAD, including untracked files."""
    path = None
    for line in git("diff", "HEAD", "-U0", "--no-color").splitlines():
        if line.startswith("+++ "):
            path = line[6:] if line.startswith("+++ b/") else None
        elif path and line.startswith("+"):
            yield path, line[1:]
    for path in git("ls-files", "--others", "--exclude-standard").splitlines():
        try:
            with open(path, encoding="utf-8") as f:
                for line in f:
                    yield path, line.rstrip("\n")
        except (UnicodeDecodeError, OSError):
            continue


def check(lines):
    hits = []
    for path, line in lines:
        for glob, pattern, message in RULES:
            if fnmatch.fnmatch(path, glob) and re.search(pattern, line):
                hits.append(f"{path}: {message}\n    {line.strip()[:120]}")
    return hits


if __name__ == "__main__":
    hits = check(added_lines())
    print("\n".join(hits) or "guard: no hits")
    sys.exit(1 if hits else 0)
