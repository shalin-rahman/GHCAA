"""List open tracker items and check a plan file against the tracker.

The parsing lives in docs/book/build/tracker_page.py, which already reads
TODO.md and TODO_ARCHIVE.md and every item shape. This script only filters
what it returns.

    python scripts/backlog_inventory.py                  # all open items
    python scripts/backlog_inventory.py --wp 84 --state PARTIAL
    python scripts/backlog_inventory.py --check docs/MASTER_EXECUTION_PLAN.md
    python scripts/backlog_inventory.py --card S34       # empty card for one session

--check exits 1 when the plan names a closed or unknown item, or leaves an
open item out. Text between <!-- backlog-check: off --> and
<!-- backlog-check: on --> is not checked. A range "88.1-88.5" names every
item in it.

parse() drops an item whose status it does not know, such as [IN-PROGRESS]
or [OPEN]. --check lists those lines so they are fixed, not lost.

--check also reads the session cards. The plan lists the rewritten phases in
<!-- card-template: A B -->. A card in a listed phase must hold the eleven
parts in order, end every step with "-> check:", and name only files that
exist; a file the card creates is followed by "(new)". Other phases are
printed as owed and do not fail. Every "(#s01)" link needs its
<a id="s01"></a> anchor.
"""

import argparse
import os
import re
import subprocess
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parents[1] / "docs" / "book" / "build"))

from tracker_page import parse  # noqa: E402

OPEN_STATES = ("TODO", "PARTIAL", "IN PROGRESS")
# BLOCKED items wait on something outside the code, so they sit with ONHOLD.
HELD_STATES = ("ONHOLD", "BLOCKED")

REPO_ROOT = Path(__file__).resolve().parents[1]
DEFAULT_PLAN = REPO_ROOT / "docs" / "MASTER_EXECUTION_PLAN.md"
TRACKER_FILES = [REPO_ROOT / "docs" / n for n in ("TODO.md", "TODO_ARCHIVE.md")]

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

# The eleven parts of a session card, in the order section 4 of the plan gives them.
CARD_PARTS = (
    ("Goal", "what is true after the session that is not true now"),
    ("Before you start", "cards that must be closed, and one command that shows the starting state"),
    ("Read (only these)", "the files and symbols to open"),
    ("Change (only these files)", "every file the session may edit or create; mark a created file (new)"),
    ("Do not touch", "nearby code that looks related and must stay as it is"),
    ("Steps", "numbered below, each ending with -> check: and how to see it worked"),
    ("Contract", "route, policy, request and response fields, status codes, or None"),
    ("Tests", "file, test name, and what the test proves"),
    ("Run", "the focused command first, then the layer gate"),
    ("Done when", "the facts that close the item, and the status each ID gets"),
    ("Stop and ask if", "the cases where the session stops and asks the owner, with the default answer"),
)
CARD_MARKER = re.compile(r"<!-- card-template:([A-Z ]*)-->")
SEQ_SECTION = re.compile(r"^## 3\. .*?(?=^## |\Z)", re.M | re.S)
CARD_SECTION = re.compile(r"^## 4\. .*?(?=^## |\Z)", re.M | re.S)
PHASE_HEAD = re.compile(r"^### (?:Phase|Track) ([A-Z])\.", re.M)
CARD_HEAD = re.compile(r"^\*\*([SH])(\d+)(?: to [SH]?(\d+))?(?= |\*)", re.M)
OTHER_HEAD = re.compile(r"^###? ", re.M)
SEQ_PHASE = re.compile(r"^\| \*\*([A-Z])\*\* \|")
SEQ_ROW = re.compile(r"^\| ([SH]\d+) \|(.*)\|\s*$")
ANCHOR = re.compile(r'<a id="([sh]\d+)"></a>')
ANCHOR_LINK = re.compile(r"\]\(#([sh]\d+)\)")
STEP_START = re.compile(r"^\s+\d+\.\s", re.M)
CODE_SPAN = re.compile(r"`([^`\n]+)`( \(new\b)?")
FILE_EXT = re.compile(r"\.(?:cs|csproj|ts|html|scss|css|dart|py|md|json|bat|ps1|sh|yml|yaml|sql)$")
LINE_SUFFIX = re.compile(r"(?::\d+(?:-\d+)?|#[\w.-]+)$")
EMPTY_CHECK = re.compile(r"-> check:\s*\Z")
HINT_LEFT = re.compile(r"^- [^`\n]*<[^<>`\n]+>\s*$", re.M)
# Characters that mark a code span as a command, a route or a pattern, not a file name.
NOT_A_PATH = set("<>*{}%:$()=,;\\")
_WALK_SKIP = {".git", "node_modules", "bin", "obj", ".dart_tool", "graphify-out", "dist", ".angular", ".venv"}


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


def template_phases(md_text):
    m = CARD_MARKER.search(md_text)
    return set(m.group(1).split()) if m else set()


def split_cards(md_text):
    """The cards in section 4, each as a dict with its ids, phase and text."""
    section = CARD_SECTION.search(md_text)
    if not section:
        return []
    body = section.group(0)
    marks = [(m.start(), "phase", m) for m in PHASE_HEAD.finditer(body)]
    marks += [(m.start(), "card", m) for m in CARD_HEAD.finditer(body)]
    marks += [(m.start(), "end", m) for m in OTHER_HEAD.finditer(body) if not PHASE_HEAD.match(body, m.start())]
    marks.sort(key=lambda t: t[0])
    cards, phase = [], None
    for n, (pos, kind, m) in enumerate(marks):
        if kind == "phase":
            phase = m.group(1)
        elif kind == "end":
            phase = None
        elif phase:
            end = marks[n + 1][0] if n + 1 < len(marks) else len(body)
            letter, lo, hi = m.group(1), int(m.group(2)), int(m.group(3) or m.group(2))
            # A range written backwards still names its first session.
            ids = [f"{letter}{k:02d}" for k in range(lo, max(lo, hi) + 1)]
            cards.append({"id": ids[0], "ids": ids, "phase": phase, "text": body[pos:end]})
    return cards


def owed_phases(md_text):
    """Phases whose cards are still in the short form."""
    return sorted({c["phase"] for c in split_cards(md_text)} - template_phases(md_text))


def sequence_rows(md_text):
    """Section 3 rows as (session id, phase, cells after the id)."""
    rows, phase = [], None
    section = SEQ_SECTION.search(md_text)
    for line in (section.group(0) if section else "").splitlines():
        p = SEQ_PHASE.match(line)
        if p:
            phase = p.group(1)
            continue
        r = SEQ_ROW.match(line)
        if r:
            rows.append((r.group(1), phase, [c.strip() for c in r.group(2).split("|")]))
    return rows


def _repo_files(root):
    root = Path(root)
    try:
        out = subprocess.run(["git", "-C", str(root), "ls-files", "-co", "--exclude-standard"],
                             capture_output=True, text=True, encoding="utf-8", check=True).stdout
        # ls-files still lists a file deleted in the working tree.
        paths = [p for p in out.splitlines() if p and (root / p).exists()]
    except (OSError, subprocess.CalledProcessError):
        paths = []
    if not paths:
        for folder, subfolders, names in os.walk(root):
            subfolders[:] = [d for d in subfolders if d not in _WALK_SKIP]
            paths += [os.path.relpath(os.path.join(folder, n), root).replace("\\", "/") for n in names]
    return paths


def _file_tokens(text):
    """Code spans that name a file or folder, as (name, marked new) pairs."""
    out = []
    for tok, new in CODE_SPAN.findall(text):
        t = LINE_SUFFIX.sub("", tok.strip())
        if not t or " " in t or t[0] in "*/-" or NOT_A_PATH & set(t):
            continue
        # A bare extension such as `.ts` names no file.
        if t[0] == "." and "/" not in t:
            continue
        if FILE_EXT.search(t) or t.endswith("/"):
            out.append((t, bool(new)))
    return out


def _in_repo(token, paths):
    want = token
    if want.endswith("/"):
        return any(p.startswith(want) or ("/" + want) in p for p in paths)
    return any(p == want or p.endswith("/" + want) for p in paths)


def _created(token, created):
    """True when a card marks this file, its full path or its folder as new."""
    want = token
    return any(want == c or c.endswith("/" + want) or (c.endswith("/") and (want.startswith(c) or ("/" + c) in want))
               for c in created)


def card_problems(md_text, root=REPO_ROOT):
    """Problems in the anchors and in the cards of the rewritten phases."""
    cards = split_cards(md_text)
    marked = template_phases(md_text)
    problems = []
    for link in sorted(set(ANCHOR_LINK.findall(md_text)) - set(ANCHOR.findall(md_text))):
        problems.append(f"{link.upper()}: the plan links to #{link} but no card has that anchor")
    if "card-template" in md_text and not CARD_MARKER.search(md_text):
        problems.append("the card-template marker is not readable; write it as <!-- card-template: A B -->")
    for letter in sorted(marked - {c["phase"] for c in cards}):
        problems.append(f"the card-template marker names phase {letter}, which has no cards in section 4")
    if not marked:
        return problems

    have = {i for c in cards for i in c["ids"]}
    for sid, phase, cells in sequence_rows(md_text):
        # A finished session keeps its row but needs no card.
        if phase in marked and sid not in have and not cells[0].lower().startswith("done"):
            problems.append(f"{sid}: in the section 3 table but has no card")

    paths = None
    created = set()
    for c in cards:
        if c["phase"] not in marked:
            continue
        cid, text = c["id"], c["text"]
        # A file counts as new from the card that creates it onward.
        created |= {t for t, new in _file_tokens(text) if new}
        at = []
        for label, _ in CARD_PARTS:
            m = re.search(r"^- " + re.escape(label) + ":", text, re.M)
            if m:
                at.append(m.start())
            else:
                problems.append(f'{cid}: part "{label}" is missing')
        if at != sorted(at):
            problems.append(f"{cid}: the parts are out of order")
        steps = re.search(r"^- Steps:(.*?)(?=^- |\Z)", text, re.M | re.S)
        if steps:
            chunks = STEP_START.split(steps.group(1))[1:]
            if not chunks:
                problems.append(f"{cid}: Steps has no numbered step")
            for n, chunk in enumerate(chunks, 1):
                if "-> check:" not in chunk:
                    problems.append(f'{cid}: step {n} has no "-> check:"')
                elif EMPTY_CHECK.search(chunk):
                    problems.append(f'{cid}: step {n} has nothing after "-> check:"')
        if HINT_LEFT.search(text):
            problems.append(f"{cid}: a part still holds the skeleton hint in angle brackets")
        for tok in sorted({t for t, _ in _file_tokens(text)}):
            if _created(tok, created):
                continue
            if paths is None:
                paths = _repo_files(root)
            if not _in_repo(tok, paths):
                problems.append(f"{cid}: `{tok}` is not in the repo; a file the card creates is marked (new)")
    return problems


def tracker_entry(raw_text, item_id, limit=3000):
    """The live tracker entry for one item: its status line and the lines under it."""
    starts = list(STATUS_LINE.finditer(raw_text))
    for n, m in enumerate(starts):
        if (m.group(1) or m.group(2)) != item_id:
            continue
        end = starts[n + 1].start() if n + 1 < len(starts) else len(raw_text)
        head = re.search(r"^#", raw_text[m.end():end], re.M)
        if head:
            end = m.end() + head.start()
        return raw_text[m.start():end].strip()[:limit]
    return None


def card_skeleton(md_text, wanted, raw_text):
    """An empty eleven-part card for a session ("S34") or for the sessions that hold an item ("84.19")."""
    rows = sequence_rows(md_text)
    hits = [r for r in rows if r[0] == wanted.upper()] or [r for r in rows if wanted in plan_ids(r[2][0])]
    if not hits:
        return None
    out = []
    for sid, _, cells in hits:
        cells = cells + [""] * 6
        items, size, mig, after = cells[0], cells[2], cells[4], cells[5]
        head = " · ".join(x for x in (sid, items, size, f"migration {mig}" if mig else "") if x)
        out += [f'<a id="{sid.lower()}"></a>', f"**{head}**"]
        for label, hint in CARD_PARTS:
            note = f"{after} closed. " if label == "Before you start" and after else ""
            out.append(f"- {label}: {note}<{hint}>")
        out.append("")
        for pid in sorted(plan_ids(items)):
            out += [f"Tracker entry {pid}:", tracker_entry(raw_text, pid) or "not found in the tracker", ""]
    return "\n".join(out)


def check(plan_path, items=None, raw_text=None, root=REPO_ROOT):
    if items is None:
        items = parse()[1]
    if raw_text is None:
        raw_text = _raw_tracker()
    plan_text = Path(plan_path).read_text(encoding="utf-8")
    named = plan_ids(HISTORY_BLOCK.sub("", plan_text))
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
    return problems + card_problems(plan_text, root)


def main(argv=None):
    ap = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    ap.add_argument("--wp", type=int, help="only this work package")
    ap.add_argument("--state", help="only this state, e.g. PARTIAL or ONHOLD")
    ap.add_argument("--check", metavar="PLAN", help="check a plan file against the tracker")
    ap.add_argument("--card", metavar="ID", help="print an empty card for a session (S34) or an item (84.19)")
    ap.add_argument("--plan", default=str(DEFAULT_PLAN), help="plan file that --card reads")
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
        owed = owed_phases(Path(args.check).read_text(encoding="utf-8"))
        if owed:
            print(f"cards still in the short form: phase {', '.join(owed)}")
        print(f"{len(problems)} problem(s)")
        return 1 if problems else 0

    if args.card:
        text = card_skeleton(Path(args.plan).read_text(encoding="utf-8"), args.card, _raw_tracker())
        print(text if text else f"{args.card}: no row in the section 3 table")
        return 0 if text else 1

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
