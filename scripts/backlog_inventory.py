"""List open tracker items and check a plan file against the tracker.

The parsing lives in docs/book/build/tracker_page.py, which already reads
TODO.md and TODO_ARCHIVE.md and every item shape. This script only filters
what it returns.

    python scripts/backlog_inventory.py                  # all open items
    python scripts/backlog_inventory.py --wp 84 --state PARTIAL
    python scripts/backlog_inventory.py --check docs/MASTER_EXECUTION_PLAN.md

--check exits 1 when the plan names a closed or unknown item, or leaves an
open item out. Text between <!-- backlog-check: off --> and
<!-- backlog-check: on --> is not checked. A range "88.1-88.5" names every
item in it.

parse() drops an item whose status it does not know, such as [IN-PROGRESS]
or [OPEN]. --check lists those lines so they are fixed, not lost.
"""

import argparse
import re
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parents[1] / "docs" / "book" / "build"))

from tracker_page import parse  # noqa: E402

OPEN_STATES = ("TODO", "PARTIAL", "IN PROGRESS")
# BLOCKED items wait on something outside the code, so they sit with ONHOLD.
HELD_STATES = ("ONHOLD", "BLOCKED")

TRACKER_FILES = [Path(__file__).resolve().parents[1] / "docs" / n for n in ("TODO.md", "TODO_ARCHIVE.md")]

# The same shapes tracker_page.ITEM accepts: "84.0", "34.D17", "37.13m", "37.13a.2", "47.13.3", "49.3.C".
# Work packages start at 1 and the edges reject a leading "§" or digit, so "§12.8" does not read as an item.
# A three-part ID never has 0 in the middle, which keeps "9.0.1" out.
_ID = r"[1-9]\d{0,2}\.(?:[A-Z]\d+|[1-9]\d*[a-z]?\.(?:[1-9]\d*|[A-Z])|\d+[a-z]?)"
_EDGE_L, _EDGE_R = r"(?<![\w.§/-])", r"(?![\w]|\.?\d)"
PLAN_ID = re.compile(_EDGE_L + "(" + _ID + ")" + _EDGE_R)
PLAN_RANGE = re.compile(_EDGE_L + r"([1-9]\d{0,2})\.(\d+)\s*[–-]\s*\1\.(\d+)" + _EDGE_R)
READABLE = ("TODO", "IN PROGRESS", "BLOCKED", "ONHOLD", "PARTIAL", "DONE", "WONTDO")
# Lines that record a decision or a pointer, not work. parse() skips them and that is right.
NOT_WORK = ("NOTE", "SUPERSEDED", "MOVED", "VERIFIED-STALE")
# An item line's first status bracket, plain ("7.16 [X]") or bold ("- **37.13m** Title [X]").
STATUS_LINE = re.compile(
    r"^(?:> )?(?:(" + _ID + r")\s+|\s*- \*\*(" + _ID + r")\*\*[^\[\n]*)"
    r"\[([A-Z][A-Z-]*(?: [A-Z][A-Z-]*)?)(?=[\] ])", re.M)

# The gap report and progress log name closed items on purpose, so --check skips them.
# An "off" marker with no "on" after it runs to the end of the file.
HISTORY_BLOCK = re.compile(r"<!-- backlog-check: off -->.*?(?:<!-- backlog-check: on -->|\Z)", re.S)


def _first_per_id(items):
    # parse() reads TODO.md before the archive, so the first copy is the live one.
    seen, out = set(), []
    for item in items:
        if item["id"] not in seen:
            seen.add(item["id"])
            out.append(item)
    return out


def open_items(items):
    return [i for i in _first_per_id(items) if i["state"] in OPEN_STATES]


def held_items(items):
    return [i for i in _first_per_id(items) if i["state"] in HELD_STATES]


def plan_ids(md_text):
    ids = set(PLAN_ID.findall(md_text))
    for wp, lo, hi in PLAN_RANGE.findall(md_text):
        ids.update(f"{wp}.{n}" for n in range(int(lo), int(hi) + 1))
    return ids


def unreadable(raw_text):
    """Item lines whose status parse() cannot read, as (id, status) pairs."""
    out = []
    for m in STATUS_LINE.finditer(raw_text):
        if m.group(3) not in READABLE + NOT_WORK:
            out.append((m.group(1) or m.group(2), m.group(3)))
    return out


def _raw_tracker():
    return "\n".join(f.read_text(encoding="utf-8") for f in TRACKER_FILES if f.exists())


def check(plan_path, items=None, raw_text=None):
    if items is None:
        items = parse()[1]
    if raw_text is None:
        raw_text = _raw_tracker()
    named = plan_ids(HISTORY_BLOCK.sub("", Path(plan_path).read_text(encoding="utf-8")))
    state = {i["id"]: i["state"] for i in _first_per_id(items)}
    lost = {pid: st for pid, st in unreadable(raw_text) if pid not in state}
    problems = []
    for pid, st in sorted(lost.items()):
        if pid not in named:
            problems.append(f"{pid}: status [{st}] cannot be read by the tracker and the plan does not name it")
    for pid in sorted(named):
        if pid in lost:
            continue
        if pid not in state:
            problems.append(f"{pid}: named in the plan but not in the tracker")
        elif state[pid] in ("DONE", "WONTDO"):
            problems.append(f"{pid}: named in the plan but {state[pid]} in the tracker")
    for item in open_items(items):
        if item["id"] not in named:
            problems.append(f"{item['id']}: {item['state']} in the tracker but missing from the plan")
    return problems


def main(argv=None):
    ap = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    ap.add_argument("--wp", type=int, help="only this work package")
    ap.add_argument("--state", help="only this state, e.g. PARTIAL or ONHOLD")
    ap.add_argument("--check", metavar="PLAN", help="check a plan file against the tracker")
    args = ap.parse_args(argv)
    # The Windows console defaults to cp1252, which cannot print the em dashes in item text.
    # A captured stream (tests, pipes in some hosts) has no reconfigure.
    if hasattr(sys.stdout, "reconfigure"):
        sys.stdout.reconfigure(encoding="utf-8")

    items = parse()[1]
    if args.check:
        raw = _raw_tracker()
        for pid, st in unreadable(raw):
            print(f"warning: {pid} has status [{st}], which the tracker parser skips")
        problems = check(args.check, items, raw)
        for p in problems:
            print(p)
        print(f"{len(problems)} problem(s)")
        return 1 if problems else 0

    if args.state:
        rows = [i for i in _first_per_id(items) if i["state"] == args.state.upper()]
    else:
        rows = open_items(items)
    if args.wp is not None:
        rows = [i for i in rows if i["wp"] == args.wp]
    for i in rows:
        print(f"{i['id']:<10} | {i['state']:<11} | {i['pr']:<4} | {i['wp']:<3} | {i['text'][:70]}")
    print(f"{len(rows)} item(s)")
    return 0


if __name__ == "__main__":
    sys.exit(main())
