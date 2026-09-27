#!/usr/bin/env python
"""Build the GHCAA documentation book into a Word (.docx) file.

Reuses build.py's markdown parser rather than re-reading the chapters, and
reuses printer.py/devtools.py's headless-Chrome plumbing rather than adding a
new one. The one new dependency is python-docx, already on this machine.

Diagrams are Mermaid, drawn by JavaScript only once a browser opens the page
(build.py's docstring covers why: the saved HTML holds no <svg>, only
<pre class="mermaid"> source). A markup-to-docx pass can carry over headings,
paragraphs, tables and captions, but not a diagram that only exists once a
browser has rendered it — so this opens the same built HTML in the same
headless Chrome/Edge the PDF path uses, waits for Mermaid the same way
print_pdf() does, and screenshots each rendered diagram as a PNG to embed.

    python docs/book/build/export_docx.py
    python docs/book/build/export_docx.py -o out.docx
"""

import argparse
import base64
import io
import json
import os
import re
import shutil
import sys
import tempfile

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

import build
import devtools
import printer

from docx import Document
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.oxml import OxmlElement
from docx.oxml.ns import qn
from docx.shared import Inches, Mm, Pt, RGBColor

HERE = os.path.dirname(os.path.abspath(__file__))
BOOK = os.path.dirname(HERE)

# These three front-matter sections carry PDF folios that a Word file cannot
# reproduce (its page count is different, and depends on the reader's zoom).
# The table of contents is replaced by a native Word TOC field instead, which
# stays correct on any page count; the figure/table lists point the reader at
# Word's own navigation instead of shipping numbers already known to be wrong.
SKIP_FOLIO_HEADINGS = {
    "vii. table of contents",
    "viii. list of figures",
    "ix. list of tables",
}

# Same four spans as build.inline(), renumbered for one combined pattern:
# 1 code, 2 link label, 3 link target (dropped — a docx run carries no href
# without extra XML this one-off export does not need), 4 bold, 5 italic.
_INLINE = re.compile(
    r"`([^`]+)`"
    r"|\[([^\]\[]+)\]\(([^)\s]+)\)"
    r"|\*\*([^*]+)\*\*"
    r"|(?<![*\w])\*([^*\n]+)\*(?!\*)"
)


def add_inline(paragraph, text):
    """Append text to a paragraph as styled runs, using the book's inline markup."""
    pos = 0
    for m in _INLINE.finditer(text):
        if m.start() > pos:
            paragraph.add_run(text[pos:m.start()])
        if m.group(1) is not None:
            run = paragraph.add_run(m.group(1))
            run.font.name = "Consolas"
            run.font.size = Pt(9)
        elif m.group(2) is not None:
            paragraph.add_run(m.group(2))
        elif m.group(4) is not None:
            paragraph.add_run(m.group(4)).bold = True
        elif m.group(5) is not None:
            paragraph.add_run(m.group(5)).italic = True
        pos = m.end()
    if pos < len(text):
        paragraph.add_run(text[pos:])


def set_letter_spacing(run, points):
    """python-docx has no letter-spacing API; ieee-print.css tracks a few
    labels (h1, .part-title, table captions), so add the raw run property."""
    rPr = run._element.get_or_add_rPr()
    spacing = OxmlElement("w:spacing")
    spacing.set(qn("w:val"), str(int(round(points * 20))))
    rPr.append(spacing)


def set_left_border(paragraph, color="999999", size_pt=1.5):
    """Mirrors blockquote's CSS border-left; python-docx has no paragraph-border API."""
    pPr = paragraph._p.get_or_add_pPr()
    borders = OxmlElement("w:pBdr")
    left = OxmlElement("w:left")
    left.set(qn("w:val"), "single")
    left.set(qn("w:sz"), str(int(size_pt * 8)))
    left.set(qn("w:space"), "4")
    left.set(qn("w:color"), color)
    borders.append(left)
    pPr.append(borders)


def add_toc_field(document):
    """A native Word table of contents, updated by the reader (right-click, Update Field).

    Word does not paginate until it opens the file, so no tool running here
    can know the page numbers; a field is the only way to get real ones.
    """
    paragraph = document.add_paragraph()
    run = paragraph.add_run()
    fld_begin = OxmlElement("w:fldChar")
    fld_begin.set(qn("w:fldCharType"), "begin")
    instr = OxmlElement("w:instrText")
    instr.set(qn("xml:space"), "preserve")
    instr.text = 'TOC \\o "1-3" \\h \\z \\u'
    fld_sep = OxmlElement("w:fldChar")
    fld_sep.set(qn("w:fldCharType"), "separate")
    fld_placeholder = OxmlElement("w:t")
    fld_placeholder.text = "Right-click and choose Update Field to build the table of contents."
    fld_end = OxmlElement("w:fldChar")
    fld_end.set(qn("w:fldCharType"), "end")
    r = run._r
    r.append(fld_begin)
    r.append(instr)
    r.append(fld_sep)
    r.append(fld_placeholder)
    r.append(fld_end)


def parse_table_rows(rows):
    cells = [[c.strip() for c in r.strip().strip("|").split("|")] for r in rows]
    head, body = None, cells
    if len(cells) >= 2 and all(re.match(r"^:?-{2,}:?$", c.replace(" ", "")) for c in cells[1]):
        head, body = cells[0], cells[2:]
    return head, body


def add_table(document, rows):
    head, body = parse_table_rows(rows)
    ncols = len(head) if head else (len(body[0]) if body else 0)
    if ncols == 0:
        return
    table = document.add_table(rows=0, cols=ncols)
    table.style = "Table Grid"
    if head:
        row = table.add_row().cells
        for cell, text in zip(row, head):
            add_inline(cell.paragraphs[0], text)
            for run in cell.paragraphs[0].runs:
                run.bold = True
    for source_row in body:
        row = table.add_row().cells
        for cell, text in zip(row, source_row):
            add_inline(cell.paragraphs[0], text)


class DocxBuilder(object):
    """Walks the same blocks build.Renderer does, emitting docx elements instead of HTML."""

    def __init__(self, document, diagrams_dir):
        self.doc = document
        self.diagrams_dir = diagrams_dir
        self.diagram_index = 0
        self.figures = 0
        self.tables = 0
        self.title_done = False
        self.missing_captions = []
        self.skip_until_level = None

    def diagram_picture(self, landscape):
        self.diagram_index += 1
        path = os.path.join(self.diagrams_dir, "diagram-%03d.png" % self.diagram_index)
        if os.path.exists(path):
            self.doc.add_picture(path, width=Inches(9.0 if landscape else 6.3))
            self.doc.paragraphs[-1].alignment = WD_ALIGN_PARAGRAPH.CENTER
        else:
            p = self.doc.add_paragraph()
            p.add_run("[diagram %d could not be captured]" % self.diagram_index).italic = True

    def note(self, text):
        if not text:
            return
        p = self.doc.add_paragraph()
        p.alignment = WD_ALIGN_PARAGRAPH.CENTER
        add_inline(p, text)
        for run in p.runs:
            run.italic = True
            run.font.size = Pt(9)
            run.font.color.rgb = RGBColor(0x44, 0x44, 0x44)

    def figure(self, num, caption, note, landscape):
        self.diagram_picture(landscape)
        self.note(note)
        cap = self.doc.add_paragraph()
        cap.alignment = WD_ALIGN_PARAGRAPH.CENTER
        label = cap.add_run("Fig. %s. " % num)
        label.bold = True
        label.font.size = Pt(9)
        add_inline(cap, caption)
        for run in cap.runs:
            if run.font.size is None:
                run.font.size = Pt(9)
        self.figures += 1

    def table_figure(self, num, caption, rows, note):
        # ieee-print.css renders the label in small caps rather than
        # upper-casing the text (figure.tbl > figcaption .label), so the
        # run stays "Table %s" and only the font-variant changes.
        cap = self.doc.add_paragraph()
        cap.alignment = WD_ALIGN_PARAGRAPH.CENTER
        label = cap.add_run("Table %s " % num)
        label.bold = True
        label.font.size = Pt(9)
        label.font.small_caps = True
        set_letter_spacing(label, 0.54)   # 0.06em at 9pt
        add_inline(cap, caption)
        for run in cap.runs:
            if run.font.size is None:
                run.font.size = Pt(9)
        self.note(note)
        add_table(self.doc, rows)
        self.tables += 1

    def render(self, blocks):
        i, n = 0, len(blocks)
        while i < n:
            kind, payload = blocks[i]

            if self.skip_until_level is not None:
                if kind == "h" and payload[0] <= self.skip_until_level:
                    self.skip_until_level = None
                else:
                    i += 1
                    continue

            if kind == "caption":
                what, num, caption, landscape = payload
                nxt = blocks[i + 1] if i + 1 < n else (None, None)
                note, step = None, 1
                if nxt[0] == "p" and i + 2 < n and blocks[i + 2][0] in ("fence", "table"):
                    note, nxt, step = nxt[1], blocks[i + 2], 2

                if what == "Figure" and nxt[0] == "fence":
                    self.figure(num, caption, note, landscape)
                    i += step + 1
                    continue
                if what == "Table" and nxt[0] == "table":
                    self.table_figure(num, caption, nxt[1], note)
                    i += step + 1
                    continue

                self.missing_captions.append("%s %s" % (what, num))
                h = self.doc.add_heading(level=3)
                h.alignment = WD_ALIGN_PARAGRAPH.CENTER
                label = "Fig. %s." % num if what == "Figure" else "Table %s" % num
                h.add_run(label + " ")
                add_inline(h, caption)
                # h3.orphan-caption overrides h3's italic to normal and sets the
                # whole line, not just the label, in small caps.
                for run in h.runs:
                    run.italic = False
                    run.font.size = Pt(10)
                    run.font.small_caps = True
                    set_letter_spacing(run, 0.5)   # 0.05em at 10pt
                i += 1
                continue

            if kind == "h":
                level, text = payload
                key = re.sub(r"\s+", " ", text).strip().lower()

                if not self.title_done and level == 1:
                    h = self.doc.add_heading(level=0)
                    add_inline(h, text)
                    if i + 1 < n and blocks[i + 1][0] == "h" and blocks[i + 1][1][0] == 2:
                        sub = self.doc.add_paragraph()
                        sub.alignment = WD_ALIGN_PARAGRAPH.CENTER
                        add_inline(sub, blocks[i + 1][1][1])
                        for run in sub.runs:
                            run.italic = True
                        i += 1
                    self.title_done = True
                    self.doc.add_page_break()
                    add_toc_field(self.doc)
                    self.doc.add_page_break()
                    i += 1
                    continue

                if re.match(r"^PART\s", text):
                    self.doc.add_page_break()
                    h = self.doc.add_heading(level=min(level, 9))
                    h.alignment = WD_ALIGN_PARAGRAPH.CENTER
                    add_inline(h, text)
                    for run in h.runs:
                        set_letter_spacing(run, 1.82)   # 0.14em at 13pt
                    i += 1
                    continue

                if level == 1:
                    # h1 { break-before: page }, except right after a PART
                    # divider, which already put the chapter on a fresh page
                    # (.part-title + h1 { break-before: avoid }).
                    prev = blocks[i - 1] if i > 0 else (None, None)
                    after_part = prev[0] == "h" and re.match(r"^PART\s", prev[1][1])
                    if not after_part:
                        self.doc.add_page_break()

                h = self.doc.add_heading(level=min(level, 9))
                if level == 1:
                    h.alignment = WD_ALIGN_PARAGRAPH.CENTER
                add_inline(h, text)
                if level == 1:
                    for run in h.runs:
                        set_letter_spacing(run, 0.15)   # 0.01em at 15pt
                if key in SKIP_FOLIO_HEADINGS:
                    if key != "vii. table of contents":
                        note = self.doc.add_paragraph()
                        note.add_run(
                            "Captions appear in place throughout the text. Word's Navigation "
                            "Pane, or References ▸ Insert Table of Figures, gives an "
                            "interactive list; the page numbers in the printed PDF do not carry "
                            "over here, since this file paginates differently."
                        ).italic = True
                    self.skip_until_level = level
                i += 1
                continue

            if kind == "p":
                add_inline(self.doc.add_paragraph(), payload)
            elif kind == "table":
                add_table(self.doc, payload)
            elif kind == "fence":
                lang, src = payload
                if lang == "mermaid":
                    self.diagram_picture(False)
                else:
                    p = self.doc.add_paragraph(src)
                    for run in p.runs:
                        run.font.name = "Consolas"
                        run.font.size = Pt(9)
            elif kind == "quote":
                p = self.doc.add_paragraph(style="Intense Quote")
                set_left_border(p)
                add_inline(p, " ".join(payload))
            elif kind == "ul":
                for item in payload:
                    add_inline(self.doc.add_paragraph(style="List Bullet"), item)
            elif kind == "ol":
                for item in payload:
                    add_inline(self.doc.add_paragraph(style="List Number"), item)
            # "hr": no docx equivalent worth the XML; the following heading
            # already carries the section break visually.
            i += 1


def capture_diagrams(html_path, out_dir, timeout=300):
    """Screenshot every rendered Mermaid diagram, in document order, as diagram-NNN.png.

    Numbering has to match DocxBuilder.diagram_picture()'s counter exactly: both
    walk the diagrams in source order, one via the DOM, one via parse_blocks.
    """
    executable = printer.find_browser()
    directory = os.path.dirname(html_path)
    with printer.LocalServer(directory) as server:
        url = server.url(os.path.basename(html_path))
        with devtools.Browser(executable, timeout=timeout) as page:
            # Twice the CSS-pixel resolution the print path uses, since a
            # screen-resolution screenshot looks soft next to Times body text.
            page.call("Emulation.setDeviceMetricsOverride",
                      width=1240, height=1754, deviceScaleFactor=2, mobile=False)
            page.open_page(url)
            if not page.wait_for("document.body && document.body.dataset.diagrams",
                                  seconds=timeout):
                raise RuntimeError("the diagrams never finished drawing")
            count = int(page.evaluate("document.querySelectorAll('.diagram').length") or 0)
            for idx in range(count):
                box_json = page.evaluate(
                    "(function(){"
                    "var el=document.querySelectorAll('.diagram')[%d];"
                    "var r=el.getBoundingClientRect();"
                    "return JSON.stringify({x:r.x,y:r.y,width:r.width,height:r.height});"
                    "})()" % idx)
                box = json.loads(box_json)
                if box["width"] <= 0 or box["height"] <= 0:
                    continue
                result = page.call(
                    "Page.captureScreenshot", format="png", captureBeyondViewport=True,
                    clip={"x": box["x"], "y": box["y"], "width": box["width"],
                          "height": box["height"], "scale": 1})
                data = result.get("data")
                if not data:
                    continue
                out_path = os.path.join(out_dir, "diagram-%03d.png" % (idx + 1))
                with open(out_path, "wb") as fh:
                    fh.write(base64.b64decode(data))
    return count


def new_document():
    document = Document()
    section = document.sections[0]
    section.page_width = Mm(210)
    section.page_height = Mm(297)
    section.top_margin = Mm(20)
    section.bottom_margin = Mm(22)
    section.left_margin = Mm(18)
    section.right_margin = Mm(18)

    normal = document.styles["Normal"]
    normal.font.name = "Times New Roman"
    normal.font.size = Pt(11)
    normal.paragraph_format.alignment = WD_ALIGN_PARAGRAPH.JUSTIFY
    normal.paragraph_format.line_spacing = 1.42
    normal.paragraph_format.space_after = Pt(6.8)

    # (size, bold, italic, space_before, space_after) per ieee-print.css:
    # h1..h4 share font-weight: bold, margin: 0 0 0.5em, overridden per level below.
    heading_rules = {
        0: (20, True, False, None, None),
        1: (15, True, False, None, Pt(21)),
        2: (11.5, True, False, Pt(18.4), Pt(5.75)),
        3: (11, True, True, Pt(13.2), Pt(5.5)),
        4: (11, False, True, Pt(11), Pt(5.5)),
    }
    for level, (size, bold, italic, space_before, space_after) in heading_rules.items():
        style_name = "Title" if level == 0 else "Heading %d" % level
        if style_name in document.styles:
            style = document.styles[style_name]
            style.font.name = "Times New Roman"
            style.font.size = Pt(size)
            style.font.bold = bold
            style.font.italic = italic
            if space_before is not None:
                style.paragraph_format.space_before = space_before
            if space_after is not None:
                style.paragraph_format.space_after = space_after

    if "Intense Quote" in document.styles:
        quote = document.styles["Intense Quote"]
        quote.font.size = Pt(10.5)

    return document


def write_docx(output, keep_diagrams=False):
    diagrams_dir = tempfile.mkdtemp(prefix="book-docx-diagrams-")
    html_dir = tempfile.mkdtemp(prefix="book-docx-html-")
    try:
        html_path = os.path.join(html_dir, "book.html")
        renderer, _ = build.write_html(html_path)
        print("captured %d diagram(s) into %s" % (
            capture_diagrams(html_path, diagrams_dir), diagrams_dir))

        document = new_document()
        builder = DocxBuilder(document, diagrams_dir)

        for name in build.CHAPTERS:
            path = os.path.join(BOOK, name)
            with io.open(path, encoding="utf-8") as fh:
                lines = [build.substitute(line) for line in fh.readlines()]
            builder.render(build.parse_blocks(lines))

        document.save(output)
        return builder
    finally:
        shutil.rmtree(html_dir, ignore_errors=True)
        if keep_diagrams:
            print("diagram images kept at %s" % diagrams_dir)
        else:
            shutil.rmtree(diagrams_dir, ignore_errors=True)


def main(argv=None):
    for handle in (sys.stdout, sys.stderr):
        try:
            handle.reconfigure(encoding="utf-8", errors="replace")
        except (AttributeError, ValueError):
            pass
    ap = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    ap.add_argument("-o", "--output",
                    default=os.path.join(BOOK, "GHCAA-Documentation-Book.docx"))
    ap.add_argument("--keep-diagrams", action="store_true",
                    help="do not delete the captured diagram PNGs afterwards (for debugging)")
    args = ap.parse_args(argv)

    builder = write_docx(args.output, args.keep_diagrams)

    print("wrote %s (%.0f KB)" % (args.output, os.path.getsize(args.output) / 1024.0))
    print("  figures : %d captioned, %d diagrams" % (builder.figures, builder.diagram_index))
    print("  tables  : %d captioned" % builder.tables)
    if builder.missing_captions:
        print("  captions with no artefact beneath them: %s"
              % ", ".join(builder.missing_captions))
    return 0


if __name__ == "__main__":
    sys.exit(main())
