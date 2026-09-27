"""Pages per chapter in the printed book, against the outline's page budget.

    python docs/book/build/pages.py            pages per chapter, budget, total
    python docs/book/build/pages.py --words    also words per section, to find the long ones

The counted range runs from Chapter 1 to the end of the References and has to
stay under 100 pages (item 67.5 in docs/TODO.md). The per-chapter budgets are
read from the "Page budget" block in docs/DOCUMENTATION_BOOK_OUTLINE.md, so the
numbers live in one place. A chapter over its budget is reported; only the total
past the limit is a failure, because the outline lets one chapter borrow pages
from another as long as the total holds.

Reads the PDF the last `build.py --pdf` wrote. Exits 1 when the counted range is
over the limit or a chapter cannot be found in the PDF.
"""

import argparse
import io
import os
import re
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)

import build
import folios

PAGE_LIMIT = 99
REFERENCES = "References"
OUTLINE = os.path.join(build.BOOK, os.pardir, "DOCUMENTATION_BOOK_OUTLINE.md")
PDF = os.path.join(build.BOOK, "GHCAA-Documentation-Book.pdf")

BUDGET_START = "Per-chapter budget"
BUDGET_END = "**Counted range**"
BUDGET_ROW = re.compile(r"^\|\s*(\d+|References)\b[^|]*\|\s*\d+\s*\|\s*(\d+)\s*\|\s*$")
TITLE = re.compile(r"^#\s+((?:Chapter\s+(\d+)\b.*)|References)\s*$")
SECTION = re.compile(r"^##\s+(\S+)\s+(.*?)\s*$")
FENCE = re.compile(r"^\s*(```|~~~)")
WORD = re.compile(r"[A-Za-z0-9][A-Za-z0-9'’-]*")


def budgets(outline_text):
    """{"1": 4, ..., "References": 3} from the outline's per-chapter table."""
    start = outline_text.find(BUDGET_START)
    if start < 0:
        return {}
    end = outline_text.find(BUDGET_END, start)
    block = outline_text[start:end if end > 0 else None]
    found = {}
    for line in block.splitlines():
        match = BUDGET_ROW.match(line.strip())
        if match:
            found[match.group(1)] = int(match.group(2))
    return found


def chapter_titles(book=build.BOOK, files=None):
    """[(key, title)] in bound order: key is the chapter number or "References"."""
    titles = []
    for name in files or build.CHAPTERS[1:]:
        with io.open(os.path.join(book, name), encoding="utf-8") as handle:
            for line in handle:
                match = TITLE.match(line)
                if match:
                    titles.append((match.group(2) or REFERENCES, match.group(1)))
                    break
    return titles


def chapter_pages(pages, titles):
    """{key: printed pages}, plus the keys whose title the PDF never showed.

    Every chapter heading starts a new page (ieee-print.css), so each title is
    searched for from the page after the previous chapter's first page. A title
    quoted in the text of an earlier chapter is therefore not taken for its
    chapter. The last chapter runs to the end of the PDF, since the References
    close the printed book.
    """
    cursor = folios.body_start(pages)
    starts, missing = [], []
    for key, title in titles:
        page = folios.locate(pages, [title], start_page=cursor).get(title)
        if page is None:
            missing.append(key)
            continue
        starts.append((key, page))
        cursor = page + 1
    counts = {}
    for (key, page), following in zip(starts, starts[1:] + [(None, len(pages) + 1)]):
        counts[key] = following[1] - page
    return counts, missing


def section_words(path):
    """[(number, title, words)] for each ## section, fenced blocks left out."""
    sections, current, fenced = [], None, False
    with io.open(path, encoding="utf-8") as handle:
        for line in handle:
            if FENCE.match(line):
                fenced = not fenced
                continue
            if fenced:
                continue
            heading = SECTION.match(line)
            if heading:
                current = [heading.group(1), heading.group(2), 0]
                sections.append(current)
            elif current is not None:
                current[2] += len(WORD.findall(line))
    return [tuple(s) for s in sections]


def report(counts, missing, budget, out=sys.stdout):
    """Print the table. Returns the number of failures."""
    total = sum(counts.values())
    out.write("  %-12s %5s %6s\n" % ("chapter", "pages", "budget"))
    for key, pages in counts.items():
        allowed = budget.get(key)
        over = " over by %d" % (pages - allowed) if allowed is not None and pages > allowed else ""
        out.write("  %-12s %5d %6s%s\n" % (key, pages, "-" if allowed is None else allowed, over))
    out.write("  %-12s %5d %6d\n" % ("counted", total, PAGE_LIMIT))
    failures = 0
    if missing:
        out.write("  NOT FOUND in the PDF: %s\n" % ", ".join(missing))
        failures += len(missing)
    if total > PAGE_LIMIT:
        out.write("  OVER THE PAGE LIMIT: %d pages from Chapter 1 to the References, limit %d\n"
                  % (total, PAGE_LIMIT))
        failures += 1
    return failures


def check(pdf_path, out=sys.stdout):
    """For build.py --strict: print the table, return the failure count."""
    read = folios.reader()
    if read is None:
        out.write("  pages     : not counted (install pypdf: python -m pip install pypdf)\n")
        return 0
    with io.open(OUTLINE, encoding="utf-8") as handle:
        budget = budgets(handle.read())
    counts, missing = chapter_pages(read(pdf_path), chapter_titles())
    return report(counts, missing, budget, out)


def main(argv=None):
    ap = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    ap.add_argument("pdf", nargs="?", default=PDF, help="printed book (default: %(default)s)")
    ap.add_argument("--words", action="store_true", help="also list words per section")
    args = ap.parse_args(argv)
    failures = check(args.pdf)
    if args.words:
        for name in build.CHAPTERS[1:]:
            sys.stdout.write("\n%s\n" % name)
            for number, title, words in section_words(os.path.join(build.BOOK, name)):
                sys.stdout.write("  %5d  %s %s\n" % (words, number, title[:70]))
    return 1 if failures else 0


if __name__ == "__main__":
    sys.exit(main())
