"""Renumber figures and tables into bound order, and rebuild the front-matter lists.

The book numbers its artefacts per chapter in the order they are bound. When a
figure is added, dropped or moved, the numbers after it shift and every mention
in the body has to shift with them. Doing that by hand is how a book ends up
printing "Figure 6.3" followed by "Figure 6.9".

    python docs/book/build/renumber.py            # show what would change
    python docs/book/build/renumber.py --apply     # write it
    python docs/book/build/renumber.py --lists     # only rebuild List of Figures / Tables

Section numbers (§6.3, 6.11.9) are never touched: a number is only rewritten
where the word Figure, Fig., Table or their plurals stands in front of it.
"""

import argparse
import io
import os
import re
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

import build as builder
import lint

BOOK = builder.BOOK
FRONT = os.path.join(BOOK, builder.CHAPTERS[0])

CAPTION_LINE = re.compile(
    r"^(#{1,6}\s+)(Figure|Table)(\s+)(\d+)\.(\d+)(\s*[—–-]\s*)(.+)$")

# A reference phrase: the word, then one or more numbers joined by commas,
# "and", "to", an en dash or a hyphen.
NUMBER = r"\d+\.\d+"
JOIN = r"(?:\s*(?:,|and|to|–|-)\s*)"
PHRASE = re.compile(r"\b(Figures?|Tables?|Figs?\.)(\s+)(%s(?:%s%s)*)" % (NUMBER, JOIN, NUMBER))

LIST_HEADINGS = {"Figure": "List of Figures", "Table": "List of Tables"}


class Duplicate(Exception):
    pass


def plan(paths):
    """old label -> new label, per kind, in bound order.

    Two captions carrying the same label stop the run. There is no way to tell
    which of them a mention in the body meant, so rewriting references would
    silently point some of them at the wrong artefact.
    """
    caps = lint.captions(paths)
    counters = {}
    mapping = {"Figure": {}, "Table": {}}
    ordered = []
    seen = {}
    for path, line, kind, chapter, index, text, landscape in caps:
        key = (kind, chapter)
        counters[key] = counters.get(key, 0) + 1
        old = "%d.%d" % (chapter, index)
        new = "%d.%d" % (chapter, counters[key])
        if (kind, old) in seen:
            first = seen[(kind, old)]
            raise Duplicate(
                "%s %s is used by two captions, %s:%d and %s:%d. Give the new one a label no "
                "other caption holds, then run this again."
                % (kind, old, os.path.basename(first[0]), first[1],
                   os.path.basename(path), line))
        seen[(kind, old)] = (path, line)
        mapping[kind][old] = new
        ordered.append((path, line, kind, old, new, text, landscape))
    return mapping, ordered


def _rewrite_references(text, mapping):
    def one(match):
        word, gap, numbers = match.group(1), match.group(2), match.group(3)
        kind = "Table" if word.lower().startswith("table") else "Figure"
        table = mapping[kind]
        return word + gap + re.sub(NUMBER, lambda m: table.get(m.group(0), m.group(0)), numbers)
    return PHRASE.sub(one, text)


def rewrite(text, mapping):
    """One pass, line by line: a caption line is relabelled, prose is rewritten.

    Both in the same pass on purpose. Two passes would remap a caption's new
    number a second time, because a caption line also looks like a reference.
    """
    out = []
    for line in text.splitlines(True):
        match = CAPTION_LINE.match(line.rstrip("\n"))
        if match:
            kind = match.group(2)
            old = "%s.%s" % (match.group(4), match.group(5))
            new = mapping[kind].get(old, old)
            out.append("%s%s%s%s%s%s\n" % (match.group(1), kind, match.group(3), new,
                                           match.group(6), match.group(7)))
        else:
            out.append(_rewrite_references(line, mapping))
    return "".join(out)


PAGE_CELL = re.compile(r"\|\s*(\d+)\s*\|\s*$")
ROW_LABEL = re.compile(r"^\|\s*(\d+\.\d+)\s*\|")
ROW_HEADING = re.compile(r"^\|\s*(?:\*\*)?(?:§)?(.+?)(?:\*\*)?\s*\|")


def existing_pages(front_text):
    """Page numbers already in the front matter, so a rebuild keeps them.

    The folios come from the printed PDF and cost two prints to work out.
    Rebuilding a list must not throw them away; a number that has gone stale is
    corrected by the next --pdf run, but a number deleted here would leave the
    contents blank until someone noticed.
    """
    figures, tables, headings = {}, {}, {}
    section = None
    for line in front_text.splitlines():
        stripped = line.strip()
        if stripped.startswith("## "):
            lowered = stripped.lower()
            section = ("contents" if "table of contents" in lowered else
                       "figures" if "list of figures" in lowered else
                       "tables" if "list of tables" in lowered else None)
            continue
        if not section or not stripped.startswith("|"):
            continue
        page = PAGE_CELL.search(stripped)
        if not page:
            continue
        if section == "contents":
            heading = ROW_HEADING.match(stripped)
            if heading:
                headings[heading.group(1).strip()] = page.group(1)
            continue
        label = ROW_LABEL.match(stripped)
        if label:
            (figures if section == "figures" else tables)[label.group(1)] = page.group(1)
    return figures, tables, headings


def build_lists(ordered, figures=None, tables=None):
    """The List of Figures and List of Tables as Markdown tables."""
    figures, tables = figures or {}, tables or {}
    known = {"Figure": figures, "Table": tables}
    rows = {"Figure": [], "Table": []}
    for _path, _line, kind, _old, new, text, _landscape in ordered:
        if _path == FRONT:
            continue
        title = re.sub(r"\s*\{landscape\}\s*$", "", text).strip()
        rows[kind].append("| %s | %s | %s |" % (new, title, known[kind].get(new, "")))
    blocks = {}
    for kind, header in (("Figure", "Figure"), ("Table", "Table")):
        body = "\n".join(rows[kind])
        blocks[kind] = "| %s | Title | Page |\n| --- | --- | --- |\n%s\n" % (header, body)
    return blocks


TOC_HEADING = "Table of Contents"
# Parts and chapters (level 1) and their numbered sections (level 2). Deeper
# levels are left out: a contents page that lists §6.12.7 is a concordance.
TOC_LEVEL1 = re.compile(r"^#\s+(PART\s.+|Chapter\s.+|References\s*)$")
TOC_LEVEL2 = re.compile(r"^##\s+(\d+\.\d+\s+.+)$")
# Chapter rows are bold and section rows are not; that is the whole of the
# indentation a Markdown table cell allows, since leading spaces are stripped.
# The section sign belongs in a cross-reference in the prose, not in a contents
# list, where the number alone is the convention.
SECTION_MARK = ""


def build_contents(paths, headings=None):
    """The Table of Contents as Markdown, from the headings themselves."""
    headings = headings or {}
    lines = ["| Part, chapter and section | Page |", "| --- | --- |"]
    for path in paths:
        if os.path.abspath(path) == os.path.abspath(FRONT):
            continue
        in_fence = False
        for raw in io.open(path, encoding="utf-8"):
            text = raw.rstrip("\n").strip()
            if text.startswith("```"):
                in_fence = not in_fence
                continue
            if in_fence:
                continue
            top = TOC_LEVEL1.match(text)
            if top:
                title = top.group(1).strip()
                lines.append("| **%s** | %s |" % (title, headings.get(title, "")))
                continue
            section = TOC_LEVEL2.match(text)
            if section:
                title = section.group(1).strip()
                lines.append("| %s%s | %s |" % (SECTION_MARK, title, headings.get(title, "")))
    return "\n".join(lines) + "\n"


def replace_contents(front_text, table):
    """Swap whatever sits under the Table of Contents heading for the table."""
    heading = re.search(r"^##\s+.*%s\s*$" % re.escape(TOC_HEADING), front_text, re.M)
    if not heading:
        return front_text
    start = heading.end()
    end = front_text.find("\n---", start)
    end = end if end > 0 else front_text.find("\n## ", start)
    if end < 0:
        return front_text
    note = ("\n\nSection numbers are as printed in the body. Page numbers are the folios the PDF "
            "carries; a rebuild renumbers them, so they are filled in from the printed copy rather "
            "than kept by hand.\n\n")
    return front_text[:start] + note + table + front_text[end:]


def replace_list_sections(front_text, blocks):
    """Swap the table under each list heading, leaving the rest of the section."""
    for kind, marker in LIST_HEADINGS.items():
        heading = re.search(r"^##\s+.*%s\s*$" % re.escape(marker), front_text, re.M)
        if not heading:
            continue
        start = heading.end()
        end = front_text.find("\n## ", start)
        end = end if end > 0 else len(front_text)
        section = front_text[start:end]
        # The table runs from the first pipe line to the last one.
        table = re.search(r"(?:^\|.*\n)+", section, re.M)
        if not table:
            continue
        section = section[:table.start()] + blocks[kind] + section[table.end():]
        front_text = front_text[:start] + section + front_text[end:]
    return front_text


def main(argv=None):
    ap = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    ap.add_argument("--apply", action="store_true", help="write the changes")
    ap.add_argument("--lists", action="store_true",
                    help="only rebuild the front matter (contents, figures, tables), "
                         "leave numbering alone")
    args = ap.parse_args(argv)

    paths = [os.path.join(BOOK, name) for name in builder.CHAPTERS
             if os.path.exists(os.path.join(BOOK, name))]
    try:
        mapping, ordered = plan(paths)
    except Duplicate as exc:
        sys.stderr.write(str(exc) + "\n")
        return 2

    moved = {kind: {o: n for o, n in table.items() if o != n} for kind, table in mapping.items()}
    total = sum(len(t) for t in moved.values())
    if args.lists:
        print("leaving numbering as it stands; rebuilding the lists only")
    elif total == 0:
        print("numbering is already in bound order; nothing to renumber")
    else:
        for kind in ("Figure", "Table"):
            for old in sorted(moved[kind], key=lambda s: [int(p) for p in s.split(".")]):
                print("  %s %s -> %s" % (kind, old, moved[kind][old]))
        print("  %d label%s move" % (total, "" if total == 1 else "s"))

    if not args.apply:
        print("nothing written; pass --apply")
        return 0

    if not args.lists and total:
        for path in paths:
            original = io.open(path, encoding="utf-8").read()
            text = rewrite(original, mapping)
            if text != original:
                with io.open(path, "w", encoding="utf-8", newline="") as fh:
                    fh.write(text)
                print("  rewrote %s" % os.path.basename(path))

    # Rebuild the lists from the captions as they now stand.
    mapping, ordered = plan(paths)
    front = io.open(FRONT, encoding="utf-8").read()
    figures, tables, headings = existing_pages(front)
    updated = replace_list_sections(front, build_lists(ordered, figures, tables))
    updated = replace_contents(updated, build_contents(paths, headings))
    if updated != front:
        with io.open(FRONT, "w", encoding="utf-8", newline="") as fh:
            fh.write(updated)
        print("  rebuilt the contents, List of Figures and List of Tables in %s"
              % os.path.basename(FRONT))
    return 0


if __name__ == "__main__":
    sys.exit(main())
