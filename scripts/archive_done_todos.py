"""Moves DONE items out of docs/TODO.md into docs/TODO_ARCHIVE.md.

Only items below the first Work Package heading move. The standing rules and the
Priority Index stay where they are. An item is a line the tracker page reads as an
item, plus the lines that continue it: lines up to the next item or heading, and
past a blank line only when the next line is indented deeper than the item. Each
block goes under the archive heading with the same text as its TODO.md heading,
or under a copy of that heading added at the end of the archive.

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


def collect(lines):
    """Return (blocks, keep) where blocks maps a heading to its moved lines."""
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
        if m and m.group(6) == "DONE":
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


def place(archive, blocks, order):
    """Insert each heading's blocks at the end of its archive section."""
    eol = "\r" if archive and archive[0].endswith("\r") else ""
    for heading in order:
        moved = []
        for block in blocks[heading]:
            # TODO.md is LF and the archive is CRLF, so moved lines take the archive's ending.
            moved.extend(l.rstrip("\r") + eol for l in block)
            moved.append(eol)
        hits = [n for n, l in enumerate(archive) if l.rstrip("\r") == heading]
        if not hits:
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

    todo = read(TODO)
    blocks, order, keep = collect(todo)
    total = sum(len(b) for b in blocks.values())
    for heading in order:
        ids = [ITEM.match(b[0]) for b in blocks[heading]]
        print(f"{len(ids):3}  {heading[:70]}")
        print("     " + ", ".join(f"{m.group(2)}.{m.group(3)}" for m in ids))
    print(f"{total} DONE items, {len(todo) - len(keep)} lines")
    if args.dry_run or not total:
        return
    write(ARCHIVE, place(read(ARCHIVE), blocks, order))
    write(TODO, keep)
    print("moved")


if __name__ == "__main__":
    main()
