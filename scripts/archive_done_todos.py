"""Moves DONE and WONTDO items from docs/TODO.md to docs/TODO_ARCHIVE.md, and open items back.

The archive holds closed items only. An item still TODO, PARTIAL, IN PROGRESS,
ONHOLD or BLOCKED in the archive goes back to TODO.md.

Only items below the first Work Package heading move. The standing rules and the
Priority Index stay where they are. An item is a line the tracker page reads as an
item, plus the lines that continue it: lines up to the next item or heading, and
past a blank line only when the next line is indented deeper than the item. Each
block goes under the heading with the same text in the other file. When that file
has no such heading, a copy is added before the next higher Work Package.

tracker_page.py and wbs.py read both files, so the tracker counts do not change.
Run with --dry-run first.
"""
import argparse
import io
import os
import re
import sys

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), ".."))
sys.path.insert(0, os.path.join(ROOT, "docs", "book", "build"))
from tracker_page import ITEM  # noqa: E402

TODO = os.path.join(ROOT, "docs", "TODO.md")
ARCHIVE = os.path.join(ROOT, "docs", "TODO_ARCHIVE.md")
# Level 1 and 2 headings open a Work Package; ### headings are sections inside one.
WP_HEAD = re.compile(r"^#{1,2} ")
WP_NUM = re.compile(r"work package (\d+)", re.I)
CLOSED = ("DONE", "WONTDO")
OPEN = ("TODO", "PARTIAL", "IN PROGRESS", "ONHOLD", "BLOCKED")


def read(path):
    with io.open(path, encoding="utf-8", newline="") as fh:
        return fh.read().split("\n")


def write(path, lines):
    with io.open(path, "w", encoding="utf-8", newline="") as fh:
        fh.write("\n".join(lines))


def indent(line):
    return len(line) - len(line.lstrip(" "))


def block_end(lines, start):
    """Index one past the last line of the item that starts at `start`."""
    own = indent(lines[start])
    i = start + 1
    while i < len(lines):
        line = lines[i]
        if line.startswith("#") or ITEM.match(line):
            return i
        if not line.strip():
            j = i
            while j < len(lines) and not lines[j].strip():
                j += 1
            nxt = lines[j] if j < len(lines) else ""
            if nxt and indent(nxt) > own and not ITEM.match(nxt):
                i = j
                continue
            return i
        i += 1
    return i


def collect(lines, states):
    """Return (blocks, order, keep) where blocks maps a heading to the item blocks in `states`."""
    first_wp = next(i for i, l in enumerate(lines) if re.match(r"^#{1,2} *WORK PACKAGE|^# Work Package", l, re.I))
    blocks, order, drop = {}, [], set()
    heading = None
    i = first_wp
    while i < len(lines):
        line = lines[i]
        if WP_HEAD.match(line):
            heading = line.rstrip("\r")
            i += 1
            continue
        m = ITEM.match(line)
        if m and m.group(6) in states:
            end = block_end(lines, i)
            if heading not in blocks:
                blocks[heading] = []
                order.append(heading)
            blocks[heading].append(lines[i:end])
            drop.update(range(i, end))
            # Take one blank line with the block, so no double gap is left behind.
            if end < len(lines) and not lines[end].strip() and i > 0 and not lines[i - 1].strip():
                drop.add(end)
                end += 1
            i = end
            continue
        i += 1
    keep = [l for n, l in enumerate(lines) if n not in drop]
    return blocks, order, keep


def _next_higher_wp(lines, heading):
    """Index of the first Work Package heading numbered above `heading`, or None."""
    own = WP_NUM.search(heading)
    if not own:
        return None
    for n, l in enumerate(lines):
        m = WP_HEAD.match(l) and WP_NUM.search(l)
        if m and int(m.group(1)) > int(own.group(1)):
            return n
    return None


def place(archive, blocks, order):
    """Insert each heading's blocks at the end of its section in `archive` (either file)."""
    eol = "\r" if archive and archive[0].endswith("\r") else ""
    for heading in order:
        moved = []
        for block in blocks[heading]:
            # TODO.md is LF and the archive is CRLF, so moved lines take the target's ending.
            moved.extend(l.rstrip("\r") + eol for l in block)
            moved.append(eol)
        hits = [n for n, l in enumerate(archive) if l.rstrip("\r") == heading]
        if not hits:
            at = _next_higher_wp(archive, heading)
            if at is not None:
                archive[at:at] = [heading + eol, eol] + moved
                continue
            if archive and archive[-1].strip():
                archive.append(eol)
            archive.extend([heading + eol, eol] + moved)
            continue
        at = hits[-1] + 1
        while at < len(archive) and not WP_HEAD.match(archive[at]):
            at += 1
        # Back up over trailing blanks and a `---` rule so the block sits inside the section.
        while at > hits[-1] + 1 and (not archive[at - 1].strip() or archive[at - 1].strip() == "---"):
            at -= 1
        archive[at:at] = [eol] + moved[:-1]
    return archive


def main():
    ap = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    ap.add_argument("--dry-run", action="store_true", help="list what would move, write nothing")
    args = ap.parse_args()

    todo, archive = read(TODO), read(ARCHIVE)
    closed, closed_order, todo_keep = collect(todo, CLOSED)
    reopened, open_order, archive_keep = collect(archive, OPEN)
    for label, blocks, order in (("to the archive", closed, closed_order),
                                 ("back to TODO.md", reopened, open_order)):
        total = sum(len(b) for b in blocks.values())
        print(f"{total} item(s) {label}")
        for heading in order:
            found = [ITEM.match(b[0]) for b in blocks[heading]]
            print(f"{len(found):3}  {heading[:70]}")
            print("     " + ", ".join(f"{m.group(2)}.{m.group(3)} [{m.group(6)}]" for m in found))
    if args.dry_run or not (closed or reopened):
        return
    write(ARCHIVE, place(archive_keep, closed, closed_order))
    write(TODO, place(todo_keep, reopened, open_order))
    print("moved")


if __name__ == "__main__":
    main()
