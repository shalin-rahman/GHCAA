"""Fill the Page columns of the contents and the lists from the printed PDF.

A page number cannot be known before the document is paginated, and the only
thing that paginates it is the browser. So the order is: print, read back which
page each heading and caption landed on, write those numbers into the front
matter, print again.

This is the one part of the build that needs a package outside the standard
library, because reading text out of a PDF is not something the standard
library does. It is therefore optional: `pypdf` or `PyMuPDF`, whichever is
installed. Without either, the build still produces the whole book and says the
Page columns were left empty, rather than failing or filling them with guesses.

    python -m pip install pypdf
"""

import io
import os
import re


def reader():
    """A callable pdf_path -> list of page texts, or None if no library is present."""
    try:
        import pypdf

        def read(path):
            document = pypdf.PdfReader(path)
            return [(page.extract_text() or "") for page in document.pages]
        return read
    except ImportError:
        pass
    try:
        import fitz

        def read(path):
            with fitz.open(path) as document:
                return [page.get_text() for page in document]
        return read
    except ImportError:
        return None


SPACE = re.compile(r"\s+")
MARKUP = re.compile(r"[`*_]")


def _flat(text):
    """Whitespace-insensitive, so a heading broken across lines still matches."""
    return SPACE.sub(" ", text).strip()


def _caption_head(title, words=5):
    """The first few words of a caption title, as the printed page shows them."""
    clean = MARKUP.sub("", title)
    clean = clean.split(":")[0].split("(")[0]
    return " ".join(_flat(clean).split()[:words])


def _tight(text):
    """No whitespace at all.

    A table caption prints its label and its text as two blocks, and the
    extractor runs them together as "Table 2.1Review protocol summary". Rather
    than guess where a space survived, both sides are compared with every space
    removed.
    """
    return SPACE.sub("", text)


def locate(pages, anchors, start_page=1):
    """anchor -> 1-based page it first appears on, searching from start_page."""
    flattened = [_flat(page) for page in pages]
    tightened = [_tight(page) for page in pages]
    found = {}
    for anchor in anchors:
        needle = _flat(anchor)
        if not needle:
            continue
        tight_needle = _tight(anchor)
        for index in range(start_page - 1, len(flattened)):
            if needle in flattened[index] or tight_needle in tightened[index]:
                found[anchor] = index + 1
                break
    return found


PART_TITLE = re.compile(r"PART\s+[IVX]+", re.I)


def body_start(pages):
    """The first page of the body, so front-matter echoes are not matched.

    The contents page lists every chapter title, so a search beginning at page
    one reports the contents page for all of them. The body opens on a part
    title page, which carries almost nothing else — that shortness is what
    distinguishes it from the contents page, which also names every part.
    """
    for index, page in enumerate(pages, 1):
        text = _flat(page)
        if PART_TITLE.search(text) and len(text) < 250:
            return index
    return 1


CHAPTER_ROW = re.compile(r"^\|\s*\*\*(.+?)\*\*\s*\|")
SECTION_ROW = re.compile(r"^\|\s*§(\d+\.\d+)\s+(.+?)\s*\|")
# The title cell only: stopping at the next pipe keeps a page number that is
# already in the row out of the anchor.
LABEL_ROW = re.compile(r"^\|\s*(\d+\.\d+)\s*\|\s*([^|]+?)\s*\|")


def anchors_from_front(front_text):
    """What to look for in the PDF, per row of the three front-matter tables.

    Returns a list of (row text, anchor) pairs. The anchor for a contents row
    is the heading itself; for a figure or table row it is the caption label,
    which is the shortest string unique to the page the artefact sits on.
    """
    pairs = []
    section = None
    for line in front_text.splitlines():
        stripped = line.strip()
        if stripped.startswith("## "):
            lowered = stripped.lower()
            if "table of contents" in lowered:
                section = "contents"
            elif "list of figures" in lowered:
                section = "figures"
            elif "list of tables" in lowered:
                section = "tables"
            else:
                section = None
            continue
        if not section or not stripped.startswith("|"):
            continue
        if section == "contents":
            chapter = CHAPTER_ROW.match(stripped)
            if chapter:
                pairs.append((line, chapter.group(1)))
                continue
            heading = SECTION_ROW.match(stripped)
            if heading:
                pairs.append((line, "%s %s" % (heading.group(1), heading.group(2))))
            continue
        row = LABEL_ROW.match(stripped)
        if row:
            # The label alone is not enough for a table: "Table 3.4" appears in
            # every sentence that refers to it, and the first of those comes
            # before the table itself. Anchor on the label plus the opening of
            # the caption, which only the artefact carries.
            label = ("Fig. %s." % row.group(1)) if section == "figures" else ("Table %s" % row.group(1))
            pairs.append((line, "%s %s" % (label, _caption_head(row.group(2)))))
    return pairs


TRAILING_CELL = re.compile(r"\|\s*\d*\s*\|\s*$")


def write_pages(front_path, numbers):
    """Put a page number in the last cell of each row we found a page for.

    The cell is overwritten whether it is empty or already holds a number: the
    page a figure sits on moves whenever the text above it changes, so a run
    that only filled blanks would leave yesterday's folios in place.
    """
    lines = io.open(front_path, encoding="utf-8").read().splitlines(True)
    filled = 0
    for index, line in enumerate(lines):
        page = numbers.get(line.rstrip("\n"))
        if not page:
            continue
        stripped = line.rstrip("\n").rstrip()
        if not TRAILING_CELL.search(stripped):
            continue
        lines[index] = TRAILING_CELL.sub("| %d |" % page, stripped) + "\n"
        filled += 1
    if filled:
        io.open(front_path, "w", encoding="utf-8").write("".join(lines))
    return filled


def fill(front_path, pdf_path, read=None):
    """Read the PDF, write the folios into the front matter. Returns (filled, total)."""
    read = read or reader()
    if read is None:
        return None, None
    pages = read(pdf_path)
    if not pages:
        return 0, 0
    start = body_start(pages)
    pairs = anchors_from_front(io.open(front_path, encoding="utf-8").read())
    located = locate(pages, [anchor for _row, anchor in pairs], start_page=start)
    numbers = {}
    for row, anchor in pairs:
        page = located.get(anchor)
        if page:
            numbers[row.rstrip("\n")] = page
    return write_pages(front_path, numbers), len(pairs)
