"""Merge neighbouring sections of a chapter under one heading, and renumber.

    python docs/book/build/merge_sections.py 3.11+3.12 "Feasibility and Requirements Validation"
    python docs/book/build/merge_sections.py 3.11+3.12 "..." --apply

One merge per call. The first section's heading takes the new title; the later
headings are dropped and their text stays where it is, so it now sits under the
first. Their subsections are renumbered to follow the first section's own, the
later sections of the chapter move up, and every "§n.m" or "Section n.m" in the
book, the outline and wbs.py is rewritten to match. The outline's bullets for the
dropped sections lose their number and become sub-bullets of the merged one.

Joining the prose is left to a manual edit. Apply merges from the end of a
chapter backwards, so the numbers still to be merged do not move (item 67.7 in
docs/TODO.md). Afterwards run renumber.py --apply to rebuild the contents.
"""

import argparse
import difflib
import io
import os
import re
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)

import build

OUTLINE = os.path.join(build.BOOK, os.pardir, "DOCUMENTATION_BOOK_OUTLINE.md")
# wbs.py prints sentences that quote section numbers into the book.
EXTRA = [os.path.join(HERE, "wbs.py")]

NUMBER = r"\d+\.\d+(?:\.\d+)*"
CHAPTER_HEAD = re.compile(r"^# Chapter (\d+) — ")
SECTION_HEAD = re.compile(r"^(#{2,4}) (%s) (.*)$" % NUMBER)
OUTLINE_CHAPTER = re.compile(r"^## Chapter (\d+) — ")
OUTLINE_BULLET = re.compile(r"^(\s*)- \*\*(%s)\*\*(\s*)(.*)$" % NUMBER)
# A reference: the section sign or the word Section, then a number, then any
# further numbers joined by a dash, a comma, "and", "or" or "to".
REFERENCE = re.compile(r"(§§?\s?|\b[Ss]ections?\s+)(%s(?:\s*(?:–|-|,|and|or|to)\s*§?%s)*)"
                       % (NUMBER, NUMBER))
TOKEN = re.compile(r"(%s)" % NUMBER)
TITLE_SPLIT = " — "


class MergeError(ValueError):
    pass


def parse(spec):
    """"3.11+3.12" -> (3, 11, 12). The sections must be neighbours in one chapter."""
    parts = [p.strip().split(".") for p in spec.split("+")]
    if len(parts) < 2 or any(len(p) != 2 or not all(x.isdigit() for x in p) for p in parts):
        raise MergeError("expected two or more sections like 3.11+3.12, got %r" % spec)
    chapters = {int(p[0]) for p in parts}
    numbers = [int(p[1]) for p in parts]
    if len(chapters) != 1:
        raise MergeError("%s spans more than one chapter" % spec)
    if numbers != list(range(numbers[0], numbers[0] + len(numbers))):
        raise MergeError("%s: the sections must be consecutive and in order" % spec)
    return chapters.pop(), numbers[0], numbers[-1]


def mapping(lines, chapter, first, last):
    """old number -> new number, for the sections and subsections that move."""
    subsections, top = {}, 0
    for line in lines:
        head = SECTION_HEAD.match(line)
        if not head:
            continue
        parts = [int(x) for x in head.group(2).split(".")]
        if parts[0] != chapter:
            continue
        top = max(top, parts[1])
        if len(parts) == 3:
            subsections.setdefault(parts[1], []).append(parts[2])
    for n in range(first, last + 1):
        if n > top:
            raise MergeError("Chapter %d has no section %d.%d" % (chapter, chapter, n))
    moved, shift, offset = {}, last - first, len(subsections.get(first, []))
    for n in range(first + 1, last + 1):
        moved["%d.%d" % (chapter, n)] = "%d.%d" % (chapter, first)
        for index, sub in enumerate(subsections.get(n, []), 1):
            moved["%d.%d.%d" % (chapter, n, sub)] = "%d.%d.%d" % (chapter, first, offset + index)
        offset += len(subsections.get(n, []))
    for n in range(last + 1, top + 1):
        moved["%d.%d" % (chapter, n)] = "%d.%d" % (chapter, n - shift)
    return moved


def renumber(number, moved):
    """Longest known prefix wins, so 3.12.2.1 follows wherever 3.12.2 went."""
    parts = number.split(".")
    for size in range(len(parts), 1, -1):
        key = ".".join(parts[:size])
        if key in moved:
            return ".".join([moved[key]] + parts[size:])
    return number


def rewrite_references(text, moved):
    """Rewrite every section reference; "§3.11–3.12" collapses to "§3.11"."""
    def one(match):
        prefix, body = match.group(1), match.group(2)
        pieces = TOKEN.split(body)
        kept, last = [], None
        for index in range(1, len(pieces), 2):
            new = renumber(pieces[index], moved)
            if new == last:
                continue
            kept.append(pieces[index - 1] + new if kept else new)
            last = new
        if len(kept) == 1:
            prefix = prefix.replace("§§", "§").replace("Sections", "Section").replace("sections", "section")
        return prefix + "".join(kept) + pieces[-1]
    return REFERENCE.sub(one, text)


def merge_chapter(text, chapter, first, last, title, moved):
    out, dropped = [], []
    for line in text.splitlines(True):
        head = SECTION_HEAD.match(line.rstrip("\n"))
        if head and head.group(2).count(".") == 1:
            n = int(head.group(2).split(".")[1])
            if int(head.group(2).split(".")[0]) == chapter:
                if n == first:
                    out.append("%s %s %s\n" % (head.group(1), head.group(2), title))
                    continue
                if first < n <= last:
                    dropped.append(line.strip())
                    # The blank line under the dropped heading goes too.
                    if out and not out[-1].strip():
                        out.pop()
                    continue
        if head:
            out.append("%s %s %s\n" % (head.group(1), renumber(head.group(2), moved), head.group(3)))
            continue
        out.append(rewrite_references(line, moved))
    return "".join(out), dropped


def merge_outline(text, chapter, first, last, title, moved):
    out, inside = [], False
    for line in text.splitlines(True):
        body = line.rstrip("\n")
        head = OUTLINE_CHAPTER.match(body)
        if head:
            inside = int(head.group(1)) == chapter
        elif body.startswith("# ") or body.startswith("## "):
            inside = False
        bullet = OUTLINE_BULLET.match(body) if inside else None
        if bullet:
            indent, number, gap, rest = bullet.groups()
            parts = [int(x) for x in number.split(".")]
            if len(parts) == 2 and parts[1] == first:
                tail = rest.split(TITLE_SPLIT, 1)
                rest = title + (TITLE_SPLIT + tail[1] if len(tail) == 2 else "")
                line = "%s- **%s**%s%s\n" % (indent, number, gap, rewrite_references(rest, moved))
            elif len(parts) == 2 and first < parts[1] <= last:
                line = "%s  - %s\n" % (indent, rewrite_references(rest, moved))
            else:
                line = "%s- **%s**%s%s\n" % (indent, renumber(number, moved), gap,
                                             rewrite_references(rest, moved))
            out.append(line)
            continue
        out.append(rewrite_references(line, moved))
    return "".join(out)


def _read(path):
    # Universal newlines: the text comes back with "\n" whatever the file uses.
    with io.open(path, encoding="utf-8") as handle:
        return handle.read()


def _write(path, text):
    """Write text back with the line ending the file already has.

    The chapters are LF and the outline and wbs.py are CRLF, and the repo marks
    them -text, so git would show every line as changed if one flipped.
    """
    with io.open(path, "rb") as handle:
        crlf = b"\r\n" in handle.read()
    with io.open(path, "w", encoding="utf-8", newline="\r\n" if crlf else "\n") as handle:
        handle.write(text)


def plan(spec, title, chapters, outline=OUTLINE, extra=EXTRA):
    """[(path, old text, new text)] for every file the merge changes, plus the dropped headings."""
    chapter, first, last = parse(spec)
    texts = {path: _read(path) for path in list(chapters) + [outline] + list(extra)}
    home = [p for p in chapters if any(CHAPTER_HEAD.match(l) and int(CHAPTER_HEAD.match(l).group(1)) == chapter
                                       for l in texts[p].splitlines())]
    if len(home) != 1:
        raise MergeError("found Chapter %d in %d files, expected one" % (chapter, len(home)))
    moved = mapping(texts[home[0]].splitlines(), chapter, first, last)
    changes, dropped = [], []
    for path, old in texts.items():
        if path == home[0]:
            new, dropped = merge_chapter(old, chapter, first, last, title, moved)
        elif path == outline:
            new = merge_outline(old, chapter, first, last, title, moved)
        else:
            new = rewrite_references(old, moved)
        if new != old:
            changes.append((path, old, new))
    return changes, dropped


def _diff(old, new, name):
    return difflib.unified_diff(old.splitlines(), new.splitlines(), name, name, n=0, lineterm="")


def main(argv=None):
    ap = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    ap.add_argument("spec", help="sections to merge, e.g. 3.11+3.12")
    ap.add_argument("title", help="the merged heading, naming both topics")
    ap.add_argument("--apply", action="store_true", help="write the changes (default: show them)")
    args = ap.parse_args(argv)
    chapters = [os.path.join(build.BOOK, name) for name in build.CHAPTERS]
    try:
        changes, dropped = plan(args.spec, args.title, chapters)
    except MergeError as exc:
        print("merge_sections: %s" % exc)
        return 2
    for heading in dropped:
        print("  dropped   : %s" % heading)
    for path, old, new in changes:
        name = os.path.relpath(path, os.path.join(build.BOOK, os.pardir))
        for line in _diff(old, new, name):
            print("  " + line[:130])
    if not args.apply:
        print("dry run: %d file%s would change; add --apply to write"
              % (len(changes), "" if len(changes) == 1 else "s"))
        return 0
    for path, _old, new in changes:
        _write(path, new)
    print("wrote %d file%s; now run renumber.py --apply and build.py --strict"
          % (len(changes), "" if len(changes) == 1 else "s"))
    return 0


if __name__ == "__main__":
    sys.exit(main())
