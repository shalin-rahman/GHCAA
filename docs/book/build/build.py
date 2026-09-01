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
import datetime
import html
import io
import os
import re
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

import folios
import lint
import printer

HERE = os.path.dirname(os.path.abspath(__file__))
BOOK = os.path.dirname(HERE)

# Order matters: this is the bound order of the book.
CHAPTERS = [
    "00-front-matter.md",
    "01-introduction.md",
    "02-literature-review.md",
    "03-requirements.md",
    "04-methodology.md",
    "05-system-analysis.md",
    "06-architecture.md",
    "99-references.md",
]

DOC_TITLE = "GHCAA Alumni Association Platform"

# Mermaid is fetched from a CDN at view time. If the machine is offline the
# diagram source is shown in a bordered box instead, so the document still
# prints and nothing is silently lost.
MERMAID_CDN = "https://cdn.jsdelivr.net/npm/mermaid@11/dist/mermaid.esm.min.mjs"

# Diagram label size. Mermaid measures in px on a 96 dpi canvas, so 14px is
# 10.5pt at scale 1; a diagram shrunk to fit the column prints at that size
# times the shrink factor, which is what the A4 audit checks.
MERMAID_FONT_PX = "14px"
MERMAID_BASE_PT = 10.5

# Tokens filled in at build time, so a copy printed today carries today's date
# rather than a date someone forgot to update. Pin the submission date in the
# source once it is fixed; until then the title page follows the build.
TOKENS = {
    "{{build-month-year}}": lambda: datetime.date.today().strftime("%B %Y"),
    "{{build-date}}": lambda: datetime.date.today().isoformat(),
}


def substitute(line):
    for token, value in TOKENS.items():
        if token in line:
            line = line.replace(token, value())
    return line


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
// Mermaid draws at view time. Diagram type settings are set here rather than
// per diagram so every figure in the book comes out at one scale and one font.
const SETTINGS = {
  startOnLoad: false,
  theme: "neutral",
  fontFamily: "Times New Roman, Times, serif",
  themeVariables: { fontSize: "%(mermaidfont)s" },
  flowchart: { useMaxWidth: true, htmlLabels: true, nodeSpacing: 26, rankSpacing: 34,
               padding: 6, diagramPadding: 4 },
  sequence: { useMaxWidth: true, diagramMarginX: 8, diagramMarginY: 8, boxMargin: 6,
              width: 130, height: 34, actorFontSize: 13, noteFontSize: 12,
              messageFontSize: 12 },
  class: { useMaxWidth: true, padding: 6, diagramPadding: 4 },
  state: { useMaxWidth: true, padding: 6, diagramPadding: 4 },
  // ER diagrams fan out from one central entity, which lays out very wide by
  // default; LR turns that fan into a column that fits the portrait page.
  er: { useMaxWidth: true, entityPadding: 8, minEntityWidth: 90, fontSize: 12,
        layoutDirection: "LR" },
  gantt: { useMaxWidth: true },
  journey: { useMaxWidth: true },
  quadrantChart: { useMaxWidth: true, chartWidth: 460, chartHeight: 460 },
  xyChart: { useMaxWidth: true },
  mindmap: { useMaxWidth: true },
  pie: { useMaxWidth: true }
};

// Diagrams are drawn one at a time rather than in one mermaid.run() call. One
// bad diagram would otherwise abort the whole batch, leaving the rest of the
// book showing source with no indication of which block was at fault.
const failures = [];
try {
  const mermaid = (await import("%(cdn)s")).default;
  mermaid.initialize(SETTINGS);

  const blocks = [...document.querySelectorAll("pre.mermaid")];
  for (let i = 0; i < blocks.length; i++) {
    const pre = blocks[i];
    const fig = pre.closest("figure");
    const cap = fig && fig.querySelector("figcaption");
    const name = cap ? cap.textContent.trim().replace(/\\s+/g, " ").slice(0, 60)
                     : "diagram " + (i + 1);
    try {
      const { svg } = await mermaid.render("mmd-" + i, pre.textContent);
      const holder = document.createElement("div");
      holder.innerHTML = svg;
      pre.replaceWith(holder.firstElementChild);
    } catch (err) {
      failures.push(name + ": " + (err && err.message ? err.message.split(String.fromCharCode(10))[0] : err));
      pre.classList.remove("mermaid");
      pre.classList.add("diagram-fallback");
    }
  }

  // Fit each diagram inside one A4 page. Mermaid sizes an SVG to its own
  // drawing, which on a dense or tall diagram runs off the page or, worse,
  // splits across two. The fit is computed here rather than left to CSS
  // because the landscape page is wider than anything the screen preview can
  // show, so its scale cannot be measured, only calculated.
  const MMPX = 96 / 25.4;
  const PORTRAIT = { w: 174 * MMPX, h: 224 * MMPX };   // 255mm page less the caption
  const LANDSCAPE = { w: 257 * MMPX, h: 148 * MMPX };  // 174mm page less the caption
  const MAX_ENLARGE = 1.5;                             // small diagrams grow, but not absurdly

  for (const fig of document.querySelectorAll("figure.fig")) {
    const svg = fig.querySelector(".diagram svg");
    if (!svg) continue;
    const box = svg.viewBox && svg.viewBox.baseVal;
    if (!box || !box.width || !box.height) continue;
    const page = fig.classList.contains("landscape") ? LANDSCAPE : PORTRAIT;
    const scale = Math.min(page.w / box.width, page.h / box.height, MAX_ENLARGE);
    svg.setAttribute("width", Math.round(box.width * scale));
    svg.setAttribute("height", Math.round(box.height * scale));
    svg.setAttribute("preserveAspectRatio", "xMidYMid meet");
    svg.style.width = Math.round(box.width * scale) + "px";
    svg.style.height = "auto";
    svg.style.maxWidth = "100%%";
    svg.dataset.vbw = box.width;
    svg.dataset.vbh = box.height;
    svg.dataset.scale = scale;
  }
  document.body.dataset.diagrams = failures.length ? "partial" : "rendered";
  if (failures.length) {
    document.body.dataset.diagramErrors = failures.join(" | ");
  }
} catch (e) {
  for (const el of document.querySelectorAll("pre.mermaid")) {
    el.classList.remove("mermaid");
    el.classList.add("diagram-fallback");
  }
  document.body.dataset.diagrams = "source";
  document.body.dataset.diagramErrors = String(e && e.message ? e.message : e);
}

// Measurement pass for build.py --pdf / --audit. Only runs when asked for, so
// a document opened for reading carries no extra markup. The browser is doing
// the layout here, which is the only way to know what actually fits A4.
if (location.search.includes("audit")) {
  const BASE_PT = %(basept)s;   // Mermaid label size in pt at scale 1
  const out = [];
  for (const fig of document.querySelectorAll("figure.fig, figure.tbl")) {
    const cap = fig.querySelector("figcaption");
    const label = cap ? cap.textContent.trim().replace(/\\s+/g, " ").slice(0, 70)
                      : "(uncaptioned " + fig.className + ")";
    const rect = fig.getBoundingClientRect();
    const svg = fig.querySelector(".diagram svg");
    const table = fig.querySelector("table");
    const item = {
      label: label,
      kind: svg ? "diagram" : (table ? "table" : "other"),
      landscape: fig.classList.contains("landscape"),
      breakable: fig.classList.contains("long"),
      width: rect.width,
      height: rect.height
    };
    if (svg) {
      const vbw = parseFloat(svg.dataset.vbw || 0);
      const vbh = parseFloat(svg.dataset.vbh || 0);
      const scale = parseFloat(svg.dataset.scale || 1);
      item.scale = scale;
      item.width = vbw * scale;
      item.height = vbh * scale + (cap ? cap.getBoundingClientRect().height : 0);
      item.pt = +(BASE_PT * scale).toFixed(2);
      item.aspect = vbw && vbh ? +(vbw / vbh).toFixed(2) : null;
      item.nodes = svg.querySelectorAll("g.node, g.classGroup, g.entityBox, .actor, g.stateGroup").length;
    }
    if (table) {
      item.overflowX = table.scrollWidth - table.clientWidth;
      item.columns = table.querySelectorAll("thead th").length
                     || (table.querySelector("tr") || { children: [] }).children.length;
      item.rows = table.querySelectorAll("tbody tr").length;
    }
    out.push(item);
  }
  const pre = document.createElement("pre");
  pre.id = "a4-audit";
  pre.textContent = JSON.stringify(out);
  pre.style.display = "none";
  document.body.appendChild(pre);
  document.body.dataset.audit = "done";
}
</script>
</body>
</html>
"""


def write_html(output, two_column=False):
    """Render the whole book to one HTML file. Returns (renderer, source paths).

    Separate from main() because the folio pass renders a second time, once the
    page numbers read back from the printed PDF are in the front matter.
    """
    with io.open(os.path.join(HERE, "ieee-print.css"), encoding="utf-8") as fh:
        css = fh.read()

    r = Renderer()
    sources = []
    for name in CHAPTERS:
        path = os.path.join(BOOK, name)
        if not os.path.exists(path):
            sys.stderr.write("skipping missing %s\n" % name)
            continue
        sources.append(path)
        with io.open(path, encoding="utf-8") as fh:
            lines = [substitute(line) for line in fh.readlines()]
        r.render(parse_blocks(lines))

    page = PAGE % {
        "title": html.escape(DOC_TITLE),
        "css": css,
        "bodyclass": "two-col" if two_column else "one-col",
        "content": "\n".join(r.out),
        "cdn": MERMAID_CDN,
        "mermaidfont": MERMAID_FONT_PX,
        "basept": MERMAID_BASE_PT,
    }
    with io.open(output, "w", encoding="utf-8") as fh:
        fh.write(page)
    return r, sources


def main(argv=None):
    ap = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    ap.add_argument("-o", "--output",
                    default=os.path.join(BOOK, "GHCAA-Documentation-Book.html"))
    ap.add_argument("--two-column", action="store_true",
                    help="IEEE Transactions two-column layout instead of single-column")
    ap.add_argument("--pdf", nargs="?", const=True, default=False, metavar="PATH",
                    help="also print to PDF through headless Chrome or Edge "
                         "(A4, no browser headers); defaults to the HTML name with .pdf")
    ap.add_argument("--audit", action="store_true",
                    help="measure every figure and table against the A4 text block "
                         "and report what does not fit, without printing")
    ap.add_argument("--strict", action="store_true",
                    help="exit non-zero on any defect: a lint finding, a drifted caption "
                         "or a figure that will not print on A4. This is the gate to run "
                         "after every edit")
    ap.add_argument("--no-placeholders", action="store_true",
                    help="also fail while any author placeholder is still open; the extra "
                         "condition a copy being handed in has to meet")
    ap.add_argument("--final", action="store_true",
                    help="the finished-book gate: --no-placeholders, and every reference in "
                         "the list cited somewhere in the text. Not reachable until Part III "
                         "and IV are written")
    ap.add_argument("--no-lint", action="store_true", help="skip the source checks")
    ap.add_argument("--no-folios", action="store_true",
                    help="do not read page numbers back into the contents and lists, "
                         "which skips the second print")
    args = ap.parse_args(argv)

    r, sources = write_html(args.output, args.two_column)

    print("wrote %s (%.0f KB)" % (args.output, os.path.getsize(args.output) / 1024.0))
    print("  layout    : %s" % ("two-column" if args.two_column else "single-column"))
    print("  figures   : %d captioned, %d diagrams" % (r.figures, r.diagrams))
    print("  tables    : %d captioned" % r.tables)

    failures = 0
    drifted = [c for c in r.missing_captions if c not in lint.ALLOWED_ORPHANS]
    if r.missing_captions:
        print("  captions with no artefact beneath them: %s" % ", ".join(r.missing_captions))
    if drifted:
        failures += len(drifted)
        print("  DRIFTED captions (a caption has parted company with its artefact): %s"
              % ", ".join(drifted))

    if not args.no_lint:
        results = lint.run(sources, os.path.join(BOOK, CHAPTERS[0]))
        failures += lint.report(results, sys.stdout,
                                no_placeholders=args.no_placeholders or args.final,
                                final=args.final)

    if args.pdf or args.audit:
        try:
            if args.audit and not args.pdf:
                failures += _audit_only(args.output)
            else:
                target = None if args.pdf is True else args.pdf
                pdf_path, problems = printer.to_pdf(args.output, target)
                failures += len(problems)
                if not args.no_folios:
                    _fill_folios(args.output, pdf_path, args.two_column)
        except (printer.BrowserMissing, RuntimeError) as exc:
            print("  pdf       : not produced — %s" % exc)
            failures += 1

    if failures:
        print("  status    : %d thing%s to fix before this is a deliverable copy"
              % (failures, "" if failures == 1 else "s"))
        return 1 if args.strict else 0
    print("  status    : clean, ready to deliver")
    return 0


def _fill_folios(html_path, pdf_path, two_column):
    """Read page numbers off the printed PDF into the front matter, then reprint.

    Two prints are needed and there is no way round it: the page a heading
    lands on is not known until the document has been paginated, and the
    contents page has to state it. The second print is checked against the
    first, so a number that moved is reported rather than left wrong.
    """
    front = os.path.join(BOOK, CHAPTERS[0])
    read = folios.reader()
    if read is None:
        print("  folios    : Page columns left empty (install pypdf to fill them: "
              "python -m pip install pypdf)")
        return

    filled, total = folios.fill(front, pdf_path, read)
    if not filled:
        print("  folios    : nothing to fill")
        return

    write_html(html_path, two_column)
    printer.to_pdf(html_path, pdf_path, audit=False, stream=io.StringIO())

    # Adding the numbers changed the front matter, so confirm nothing moved.
    pages = read(pdf_path)
    start = folios.body_start(pages)
    pairs = folios.anchors_from_front(io.open(front, encoding="utf-8").read())
    again = folios.locate(pages, [a for _row, a in pairs], start_page=start)
    stated = {}
    for row, anchor in pairs:
        match = re.search(r"\|\s*(\d+)\s*\|\s*$", row.rstrip())
        if match:
            stated[anchor] = int(match.group(1))
    moved = [a for a, page in stated.items() if again.get(a) not in (None, page)]
    print("  folios    : %d of %d page numbers written from the printed copy"
          % (filled, total))
    if moved:
        print("  FOLIOS MOVED on the reprint: %s. Run --pdf again to settle them."
              % ", ".join(sorted(moved)[:6]))


def _audit_only(html_path):
    """Measure the built page against A4 without printing it."""
    browser = printer.find_browser()
    directory, filename = os.path.split(os.path.abspath(html_path))
    with printer.LocalServer(directory) as server:
        dom = printer.dump_dom(browser, server.url(filename, "?audit=1"))
    state, items = printer.read_audit(dom)
    if state != "rendered":
        print("  audit     : not every diagram drew (state: %s)" % state)
        for line in printer.diagram_errors(dom):
            print("  DIAGRAM FAILED: %s" % line)
        return 1 + len(printer.diagram_errors(dom))
    problems = printer.overflows(items)
    print("  measured  : %d figures and tables laid out by %s"
          % (len(items), os.path.basename(browser)))
    for item, reasons in problems:
        print("  DOES NOT FIT A4: %s — %s" % (item["label"], "; ".join(reasons)))
    return len(problems)


if __name__ == "__main__":
    sys.exit(main())
