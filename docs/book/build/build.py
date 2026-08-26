#!/usr/bin/env python
"""Build the GHCAA documentation book into one print-ready, IEEE-styled HTML file.

Standard library only. No pandoc, no npm packages, nothing to install.

    python docs/book/build/build.py                 # single-column manuscript
    python docs/book/build/build.py --two-column     # IEEE Transactions layout
    python docs/book/build/build.py -o out.html      # choose the output path

Then open the output in Chrome or Edge and print to PDF: A4, margins "Default",
"Background graphics" on, browser headers and footers off.

The markdown sources stay readable and editable; the IEEE presentation rules
(caption above tables, caption below figures, "Fig. 3.1." / "TABLE 3.1" labels,
chapter page breaks, Times body text) are applied here at build time.
"""

import argparse
import html
import io
import os
import re
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
BOOK = os.path.dirname(HERE)

# Order matters: this is the bound order of the book.
CHAPTERS = [
    "00-front-matter.md",
    "01-introduction.md",
    "02-literature-review.md",
    "03-requirements.md",
    "99-references.md",
]

DOC_TITLE = "GHCAA Alumni Association Platform"

# Mermaid is fetched from a CDN at view time. If the machine is offline the
# diagram source is shown in a bordered box instead, so the document still
# prints and nothing is silently lost.
MERMAID_CDN = "https://cdn.jsdelivr.net/npm/mermaid@11/dist/mermaid.esm.min.mjs"


# --------------------------------------------------------------------------
# inline markdown
# --------------------------------------------------------------------------

_CODE = re.compile(r"`([^`]+)`")
_LINK = re.compile(r"\[([^\]\[]+)\]\(([^)\s]+)\)")
_BOLD = re.compile(r"\*\*([^*]+)\*\*")
_EM = re.compile(r"(?<![*\w])\*([^*\n]+)\*(?!\*)")


def inline(text):
    """Render the inline subset used in these documents."""
    holds = []

    def hold(markup):
        holds.append(markup)
        return "\x00%d\x00" % (len(holds) - 1)

    # code spans first: nothing inside them is markup
    text = _CODE.sub(lambda m: hold("<code>%s</code>" % html.escape(m.group(1))), text)
    text = html.escape(text, quote=False)
    text = _LINK.sub(
        lambda m: hold('<a href="%s">%s</a>' % (html.escape(m.group(2), quote=True), m.group(1))),
        text,
    )
    text = _BOLD.sub(lambda m: "<strong>%s</strong>" % m.group(1), text)
    text = _EM.sub(lambda m: "<em>%s</em>" % m.group(1), text)
    for i, markup in enumerate(holds):
        text = text.replace("\x00%d\x00" % i, markup)
    return text


def slug(text):
    s = re.sub(r"[^a-z0-9]+", "-", text.lower()).strip("-")
    return s[:64] or "s"


# --------------------------------------------------------------------------
# block parsing
# --------------------------------------------------------------------------

CAPTION_RE = re.compile(r"^(Figure|Table)\s+([0-9]+\.[0-9]+)\s*[—–-]\s*(.+)$")
# A caption may end with "{landscape}" to mark a diagram too wide for the
# portrait column width — it is set on its own A4 landscape page instead of
# being shrunk further. Stripped from the text that becomes the caption.
LANDSCAPE_RE = re.compile(r"\s*\{landscape\}\s*$")


def parse_blocks(lines):
    """Turn markdown lines into a list of (kind, payload) blocks."""
    blocks = []
    i, n = 0, len(lines)
    while i < n:
        raw = lines[i]
        line = raw.rstrip("\n")
        stripped = line.strip()

        if not stripped:
            i += 1
            continue

        # fenced code
        if stripped.startswith("```"):
            lang = stripped[3:].strip()
            i += 1
            buf = []
            while i < n and not lines[i].strip().startswith("```"):
                buf.append(lines[i].rstrip("\n"))
                i += 1
            i += 1  # closing fence
            blocks.append(("fence", (lang, "\n".join(buf))))
            continue

        # heading
        m = re.match(r"^(#{1,6})\s+(.*)$", stripped)
        if m:
            level, text = len(m.group(1)), m.group(2).strip()
            cap = CAPTION_RE.match(text)
            if cap:
                caption_text = cap.group(3)
                landscape = bool(LANDSCAPE_RE.search(caption_text))
                if landscape:
                    caption_text = LANDSCAPE_RE.sub("", caption_text)
                blocks.append(("caption", (cap.group(1), cap.group(2), caption_text, landscape)))
            else:
                blocks.append(("h", (level, text)))
            i += 1
            continue

        # horizontal rule
        if re.match(r"^(-{3,}|\*{3,}|_{3,})$", stripped):
            blocks.append(("hr", None))
            i += 1
            continue

        # table
        if stripped.startswith("|"):
            rows = []
            while i < n and lines[i].strip().startswith("|"):
                rows.append(lines[i].strip())
                i += 1
            blocks.append(("table", rows))
            continue

        # blockquote
        if stripped.startswith(">"):
            buf = []
            while i < n and lines[i].strip().startswith(">"):
                buf.append(re.sub(r"^\s*>\s?", "", lines[i].rstrip("\n")))
                i += 1
            blocks.append(("quote", buf))
            continue

        # lists
        if re.match(r"^[-*+]\s+", stripped):
            items, buf = [], None
            while i < n:
                cur = lines[i].rstrip("\n")
                if re.match(r"^\s*[-*+]\s+", cur):
                    if buf is not None:
                        items.append(" ".join(buf))
                    buf = [re.sub(r"^\s*[-*+]\s+", "", cur)]
                    i += 1
                elif cur.strip() and cur.startswith(("  ", "\t")) and buf is not None:
                    buf.append(cur.strip())
                    i += 1
                else:
                    break
            if buf is not None:
                items.append(" ".join(buf))
            blocks.append(("ul", items))
            continue

        if re.match(r"^\d+\.\s+", stripped):
            items, buf = [], None
            while i < n:
                cur = lines[i].rstrip("\n")
                if re.match(r"^\s*\d+\.\s+", cur):
                    if buf is not None:
                        items.append(" ".join(buf))
                    buf = [re.sub(r"^\s*\d+\.\s+", "", cur)]
                    i += 1
                elif cur.strip() and cur.startswith(("  ", "\t")) and buf is not None:
                    buf.append(cur.strip())
                    i += 1
                else:
                    break
            if buf is not None:
                items.append(" ".join(buf))
            blocks.append(("ol", items))
            continue

        # paragraph: join the wrapped lines
        buf = []
        while i < n:
            cur = lines[i].rstrip("\n")
            s = cur.strip()
            if (not s or s.startswith(("#", ">", "|", "```"))
                    or re.match(r"^([-*+]\s+|\d+\.\s+)", s)
                    or re.match(r"^(-{3,}|\*{3,}|_{3,})$", s)):
                break
            buf.append(s)
            i += 1
        blocks.append(("p", " ".join(buf)))
    return blocks


# --------------------------------------------------------------------------
# rendering
# --------------------------------------------------------------------------

class Renderer(object):
    def __init__(self):
        self.out = []
        self.diagrams = 0
        self.figures = 0
        self.tables = 0
        self.title_done = False
        self.missing_captions = []

    def w(self, s):
        self.out.append(s)

    # -- pieces ------------------------------------------------------------

    def table_html(self, rows):
        cells = [[c.strip() for c in r.strip().strip("|").split("|")] for r in rows]
        head, body = None, cells
        if len(cells) >= 2 and all(re.match(r"^:?-{2,}:?$", c.replace(" ", "")) for c in cells[1]):
            head, body = cells[0], cells[2:]
        parts = ["<table>"]
        if head:
            parts.append("<thead><tr>%s</tr></thead>"
                         % "".join("<th>%s</th>" % inline(c) for c in head))
        parts.append("<tbody>")
        for row in body:
            parts.append("<tr>%s</tr>" % "".join("<td>%s</td>" % inline(c) for c in row))
        parts.append("</tbody></table>")
        return "".join(parts), len(body)

    def diagram_html(self, source):
        self.diagrams += 1
        esc = html.escape(source)
        return ('<div class="diagram"><pre class="mermaid">%s</pre>'
                '<noscript><div class="diagram-fallback">%s</div></noscript></div>'
                % (esc, esc))

    def figure(self, num, caption, body_html, note=None, landscape=False):
        self.figures += 1
        self.w('<figure class="fig%s" id="fig-%s">%s'
               '<figcaption><span class="label">Fig. %s.</span> %s</figcaption>%s'
               '</figure>' % (" landscape" if landscape else "", num.replace(".", "-"),
                              body_html, num, inline(caption), self.note(note)))

    def table_figure(self, num, caption, table_html, long_table, note=None):
        self.tables += 1
        self.w('<figure class="tbl%s" id="tbl-%s">'
               '<figcaption><span class="label">Table %s</span>'
               '<span class="cap">%s</span></figcaption>%s%s</figure>'
               % (" long" if long_table else "", num.replace(".", "-"),
                  num, inline(caption), self.note(note), table_html))

    @staticmethod
    def note(text):
        """A lead-in line between a caption heading and its artefact."""
        return '' if not text else '<div class="note">%s</div>' % inline(text)

    # -- main --------------------------------------------------------------

    def render(self, blocks):
        i, n = 0, len(blocks)
        while i < n:
            kind, payload = blocks[i]

            if kind == "caption":
                what, num, caption, landscape = payload
                nxt = blocks[i + 1] if i + 1 < n else (None, None)
                # A caption may be followed by one lead-in line (a legend or a
                # source note) before the artefact itself.
                note, step = None, 1
                if nxt[0] == "p" and i + 2 < n and blocks[i + 2][0] in ("fence", "table"):
                    note, nxt, step = nxt[1], blocks[i + 2], 2

                if what == "Figure" and nxt[0] == "fence":
                    self.figure(num, caption, self.diagram_html(nxt[1][1]), note, landscape)
                    i += step + 1
                    continue
                if what == "Table" and nxt[0] == "table":
                    thtml, rowcount = self.table_html(nxt[1])
                    self.table_figure(num, caption, thtml, rowcount > 14, note)
                    i += step + 1
                    continue

                # Caption with no artefact directly beneath it: the source
                # points elsewhere. Keep the label, record it for the report.
                self.missing_captions.append("%s %s" % (what, num))
                label = "Fig. %s." % num if what == "Figure" else "Table %s" % num
                self.w('<h3 class="orphan-caption">%s %s</h3>'
                       % (html.escape(label), inline(caption)))
                i += 1
                continue

            if kind == "h":
                level, text = payload
                if not self.title_done and level == 1:
                    sub = ""
                    if i + 1 < n and blocks[i + 1][0] == "h" and blocks[i + 1][1][0] == 2:
                        sub = '<div class="subtitle">%s</div>' % inline(blocks[i + 1][1][1])
                        i += 1
                    self.w('<section class="title-page"><h1 class="title">%s</h1>%s</section>'
                           % (inline(text), sub))
                    self.title_done = True
                    i += 1
                    continue
                cls = ' class="part-title"' if re.match(r"^PART\s", text) else ""
                self.w('<h%d id="%s"%s>%s</h%d>' % (level, slug(text), cls, inline(text), level))
                i += 1
                continue

            if kind == "p":
                cls = ' class="todo"' if payload.lstrip().startswith("*[") else ""
                self.w("<p%s>%s</p>" % (cls, inline(payload)))
            elif kind == "table":
                thtml, rowcount = self.table_html(payload)
                self.w('<figure class="tbl%s">%s</figure>'
                       % (" long" if rowcount > 14 else "", thtml))
            elif kind == "fence":
                lang, src = payload
                if lang == "mermaid":
                    self.w('<figure class="fig">%s</figure>' % self.diagram_html(src))
                else:
                    self.w("<pre>%s</pre>" % html.escape(src))
            elif kind == "quote":
                inner = Renderer()
                inner.render(parse_blocks([l + "\n" for l in payload]))
                self.w("<blockquote>%s</blockquote>" % "".join(inner.out))
            elif kind == "ul":
                self.w("<ul>%s</ul>" % "".join("<li>%s</li>" % inline(x) for x in payload))
            elif kind == "ol":
                self.w("<ol>%s</ol>" % "".join("<li>%s</li>" % inline(x) for x in payload))
            elif kind == "hr":
                self.w("<hr />")
            i += 1


PAGE = """<!DOCTYPE html>
<html lang="en">
<head>
<meta charset="utf-8" />
<meta name="viewport" content="width=device-width, initial-scale=1" />
<title>%(title)s</title>
<style>
%(css)s
</style>
</head>
<body class="%(bodyclass)s">
%(content)s
<script type="module">
try {
  const mermaid = (await import("%(cdn)s")).default;
  mermaid.initialize({
    startOnLoad: false,
    theme: "neutral",
    fontFamily: "Times New Roman, Times, serif",
    flowchart: { useMaxWidth: true, htmlLabels: true },
    class: { useMaxWidth: true }
  });
  await mermaid.run({ querySelector: "pre.mermaid" });
  document.body.dataset.diagrams = "rendered";
} catch (e) {
  for (const el of document.querySelectorAll("pre.mermaid")) {
    el.classList.remove("mermaid");
    el.classList.add("diagram-fallback");
  }
  document.body.dataset.diagrams = "source";
}
</script>
</body>
</html>
"""


def main(argv=None):
    ap = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    ap.add_argument("-o", "--output",
                    default=os.path.join(BOOK, "GHCAA-Documentation-Book.html"))
    ap.add_argument("--two-column", action="store_true",
                    help="IEEE Transactions two-column layout instead of single-column")
    args = ap.parse_args(argv)

    css_path = os.path.join(HERE, "ieee-print.css")
    with io.open(css_path, encoding="utf-8") as fh:
        css = fh.read()

    r = Renderer()
    for name in CHAPTERS:
        path = os.path.join(BOOK, name)
        if not os.path.exists(path):
            sys.stderr.write("skipping missing %s\n" % name)
            continue
        with io.open(path, encoding="utf-8") as fh:
            r.render(parse_blocks(fh.readlines()))

    page = PAGE % {
        "title": html.escape(DOC_TITLE),
        "css": css,
        "bodyclass": "two-col" if args.two_column else "one-col",
        "content": "\n".join(r.out),
        "cdn": MERMAID_CDN,
    }
    with io.open(args.output, "w", encoding="utf-8") as fh:
        fh.write(page)

    print("wrote %s (%.0f KB)" % (args.output, os.path.getsize(args.output) / 1024.0))
    print("  layout    : %s" % ("two-column" if args.two_column else "single-column"))
    print("  figures   : %d captioned, %d diagrams" % (r.figures, r.diagrams))
    print("  tables    : %d captioned" % r.tables)
    if r.missing_captions:
        print("  captions with no artefact beneath them (by design where the source "
              "points elsewhere): %s" % ", ".join(r.missing_captions))
    return 0


if __name__ == "__main__":
    sys.exit(main())
