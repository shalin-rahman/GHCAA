"""Turn the built HTML into a PDF, and measure whether anything overflows A4.

Standard library only. The work is done by a headless Chrome or Edge, which is
the same engine the manual "open it and print" route uses, so the automated PDF
and a hand-printed one come out the same.

Two things need a browser rather than a parser:

  * Mermaid draws the diagrams at view time, so their real size is not known
    until a layout engine has run.
  * Whether a figure or table fits A4 is a layout question, not a markup one.

The page is served over http://127.0.0.1 on a random port instead of being
opened as file://, because Chrome refuses ES-module imports from file:// and
Mermaid is an ES module. The server is local, serves one directory, and is shut
down when the run ends.
"""

import http.server
import json
import os
import re
import shutil
import socketserver
import subprocess
import sys
import tempfile
import threading

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

import devtools

# A4 print area in CSS pixels at 96 dpi, matching the @page margins in
# ieee-print.css: portrait 20mm/18mm/22mm/18mm, landscape 18mm/20mm.
MM = 96.0 / 25.4
PORTRAIT_W = 174 * MM          # 210 - 18 - 18
PORTRAIT_H = 255 * MM          # 297 - 20 - 22
LANDSCAPE_W = 257 * MM         # 297 - 20 - 20
LANDSCAPE_H = 174 * MM         # 210 - 18 - 18

# Half a millimetre at 96 dpi. Below this a difference is layout rounding rather
# than overflow, and chasing it produces false failures on every reflow.
SLACK = 2.0

BROWSER_ENV = "BOOK_BROWSER"

_CANDIDATES = [
    r"C:\Program Files\Google\Chrome\Application\chrome.exe",
    r"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe",
    r"C:\Program Files\Microsoft\Edge\Application\msedge.exe",
    r"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe",
    "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome",
    "/Applications/Microsoft Edge.app/Contents/MacOS/Microsoft Edge",
    "/usr/bin/google-chrome",
    "/usr/bin/chromium",
    "/usr/bin/chromium-browser",
    "/usr/bin/microsoft-edge",
]

_ON_PATH = ["chrome", "google-chrome", "chromium", "msedge", "microsoft-edge"]


class BrowserMissing(Exception):
    pass


def find_browser():
    """Chrome or Edge, in that order. Override with the BOOK_BROWSER env var."""
    override = os.environ.get(BROWSER_ENV)
    if override:
        if not os.path.exists(override) and not shutil.which(override):
            raise BrowserMissing("%s is set to %s, which does not exist" % (BROWSER_ENV, override))
        return override
    for path in _CANDIDATES:
        if os.path.exists(path):
            return path
    for name in _ON_PATH:
        found = shutil.which(name)
        if found:
            return found
    raise BrowserMissing(
        "no Chrome or Edge found. Install one, or point %s at the executable. "
        "Firefox and Safari are not used here: they honour break-inside and "
        "column-span less faithfully and would print figures split across pages."
        % BROWSER_ENV)


class _Quiet(http.server.SimpleHTTPRequestHandler):
    def log_message(self, fmt, *args):
        pass


class _Server(socketserver.ThreadingTCPServer):
    daemon_threads = True
    allow_reuse_address = True


class LocalServer(object):
    """Serves one directory on 127.0.0.1, on a port the OS picks."""

    def __init__(self, directory):
        self.directory = directory
        handler = lambda *a, **kw: _Quiet(*a, directory=directory, **kw)
        self._httpd = _Server(("127.0.0.1", 0), handler)
        self.port = self._httpd.server_address[1]
        self._thread = threading.Thread(target=self._httpd.serve_forever, daemon=True)

    def __enter__(self):
        self._thread.start()
        return self

    def __exit__(self, *exc):
        self._httpd.shutdown()
        self._httpd.server_close()

    def url(self, filename, query=""):
        return "http://127.0.0.1:%d/%s%s" % (self.port, filename, query)


def _base_args(profile):
    return [
        "--headless=new",
        "--disable-gpu",
        "--no-sandbox",
        "--no-first-run",
        "--no-default-browser-check",
        "--disable-extensions",
        "--disable-dev-shm-usage",
        "--user-data-dir=%s" % profile,
        "--window-size=1240,1754",          # A4 at roughly 150 dpi
        "--run-all-compositor-stages-before-draw",
        "--virtual-time-budget=60000",       # Mermaid needs the CDN fetch plus layout
    ]
    # Used only by the command-line `--print-to-pdf` fallback in _run below,
    # which is one-shot and has no other way to wait for Mermaid. The audit
    # measurement and the protocol print both go through devtools.Browser
    # instead, which waits on document.body.dataset.diagrams explicitly —
    # a virtual-time budget would end a long-lived session's page target
    # rather than just run out.


def _run(browser, args, timeout):
    profile = tempfile.mkdtemp(prefix="book-print-")
    try:
        proc = subprocess.run([browser] + _base_args(profile) + args,
                              stdout=subprocess.PIPE, stderr=subprocess.PIPE,
                              timeout=timeout)
        return proc
    finally:
        shutil.rmtree(profile, ignore_errors=True)


def dump_dom(browser, url, timeout=180):
    """The page as the browser sees it after Mermaid has drawn.

    Rendered through the same devtools.Browser session and the same explicit
    wait that print_pdf uses, rather than a one-shot `--dump-dom` process
    carrying --run-all-compositor-stages-before-draw and a virtual-time
    budget. Two different sets of browser flags measuring one page and
    printing another meant the A4 audit could pass or fail on a rendering
    the printed PDF never went through.
    """
    with devtools.Browser(browser, timeout=timeout) as page:
        page.open_page(url)
        page.wait_for("document.body && document.body.dataset.diagrams", seconds=timeout)
        dom = page.evaluate("document.documentElement.outerHTML")
    if not dom or not dom.strip():
        raise RuntimeError("the browser returned an empty document")
    return dom


_AUDIT_RE = re.compile(r'<pre id="a4-audit"[^>]*>(.*?)</pre>', re.S)
_STATE_RE = re.compile(r'data-diagrams="([a-z]+)"')
_ERR_RE = re.compile(r'data-diagram-errors="([^"]*)"')


def diagram_errors(dom):
    """Mermaid blocks that failed to draw, named by their caption."""
    match = _ERR_RE.search(dom)
    if not match or not match.group(1).strip():
        return []
    text = match.group(1).replace("&amp;", "&").replace("&lt;", "<").replace("&gt;", ">")
    return [part.strip() for part in text.split(" | ") if part.strip()]


def read_audit(dom):
    """(diagram state, measurements) out of a DOM dumped with ?audit=1."""
    state_match = _STATE_RE.search(dom)
    state = state_match.group(1) if state_match else "unknown"
    body = _AUDIT_RE.search(dom)
    if not body:
        return state, []
    text = body.group(1)
    text = text.replace("&amp;", "&").replace("&lt;", "<").replace("&gt;", ">").replace("&quot;", '"')
    try:
        return state, json.loads(text)
    except ValueError as exc:
        raise RuntimeError("could not read the audit block: %s" % exc)


# A diagram shrunk to fit the column carries its labels down with it. Below
# this printed size the text stops being readable on paper, so the diagram has
# to be split, simplified, or set on a landscape page instead of shrunk. Set at
# 7pt rather than 6.5 after looking at printed proofs: at 6.5 a dense class
# diagram is technically legible and still unpleasant to read.
MIN_LABEL_PT = 7.0


def overflows(items):
    """Figures and tables that will not print correctly on A4.

    Five separate failures, which need different fixes:

      too wide      the artefact runs past the text block, so the edge is cut
      too tall      it cannot fit a page and is not marked as breakable, so the
                    browser will either shrink the page or split it badly
      too small     it fits, but only by shrinking its labels below legibility
      clipped       a title or label is wider than the drawing, so the browser
                    cuts it off at the frame
      overprinted   it fits and is legible, but two labels sit on top of each
                    other, so neither can be read

    A landscape figure is measured against the landscape page; everything else
    against the portrait text block. A table marked long is allowed to run over
    several pages by design, so its height is not a fault.
    """
    bad = []
    for item in items:
        landscape = item.get("landscape")
        limit_w = LANDSCAPE_W if landscape else PORTRAIT_W
        limit_h = LANDSCAPE_H if landscape else PORTRAIT_H
        reasons = []

        if item["width"] > limit_w + SLACK:
            reasons.append("%.0fmm wide, the page holds %.0fmm"
                           % (item["width"] / MM, limit_w / MM))
        if not item.get("breakable") and item["height"] > limit_h + SLACK:
            reasons.append("%.0fmm tall, the page holds %.0fmm — mark it {landscape}, "
                           "split it, or let it break"
                           % (item["height"] / MM, limit_h / MM))
        pt = item.get("pt")
        if pt is not None and pt < MIN_LABEL_PT:
            aspect = item.get("aspect")
            want = 0.78 if not landscape else 1.74
            shape = ""
            if aspect:
                shape = (", it is %.2f wide-to-tall against the %.2f the %s page wants"
                         % (aspect, want, "landscape" if landscape else "portrait"))
            reasons.append("labels print at %.1fpt, under the %.1fpt floor%s"
                           % (pt, MIN_LABEL_PT, shape))
        if item.get("overflowX", 0) > SLACK:
            reasons.append("table content is %.0fmm wider than its column"
                           % (item["overflowX"] / MM))
        cut = item.get("clippedCount", 0)
        if cut:
            reasons.append("%d label%s drawn outside the frame and cut off: %s — shorten the "
                           "title or the label so the drawing is the widest thing in the figure"
                           % (cut, "" if cut == 1 else "s", "; ".join(item.get("clipped", [])[:3])))
        hits = item.get("collisionCount", 0)
        if hits:
            sample = "; ".join(item.get("collisions", [])[:3])
            reasons.append("%d label%s printed over another: %s — separate the "
                           "points, shorten the labels, or set it as a table"
                           % (hits, "" if hits == 1 else "s", sample))

        if reasons:
            bad.append((item, reasons))
    return bad


# The running foot: a centred folio in the body face, sized to sit inside the
# 22mm bottom margin without touching the text block. Chrome substitutes the
# pageNumber and totalPages spans.
FOOTER = (
    '<div style="width:100%; font-family:\'Times New Roman\',Times,serif; font-size:9pt; '
    'color:#000; padding:0 18mm; margin-top:6mm;">'
    '<div style="text-align:center;">'
    '<span class="pageNumber"></span> of <span class="totalPages"></span>'
    '</div></div>'
)

# No running head. Chrome applies one header template to every page, so it
# cannot carry a chapter name, and a constant one would print the title across
# the title page itself. A folio alone is the cleaner choice; the deviation is
# recorded in README.md rather than worked around.
HEADER = ""



def print_pdf(browser, url, out_path, timeout=300, folio=True, running_head=False):
    """Print over the DevTools protocol, so the PDF carries page numbers.

    The `--print-to-pdf` switch cannot add a folio and drops background
    graphics. `Page.printToPDF` takes both, plus header and footer templates.
    If the protocol cannot be reached the switch is used instead, and the
    caller is told the PDF has no page numbers rather than left to find out.

    Returns (path, numbered, reason), where reason says why the protocol route
    was not used, so a fallback can be diagnosed rather than merely noticed.
    """
    out_path = os.path.abspath(out_path)
    if os.path.exists(out_path):
        try:
            open(out_path, "r+b").close()
        except OSError:
            raise RuntimeError(
                "cannot write %s: it is open in another program. Close the PDF viewer and run "
                "again. Without this check the build quietly falls back to the command-line "
                "switch and ships a copy with no page numbers." % out_path)
    try:
        with devtools.Browser(browser, timeout=timeout) as page:
            page.open_page(url)
            if not page.wait_for("document.body && document.body.dataset.diagrams", seconds=180):
                raise devtools.ProtocolError("the diagrams never finished drawing")
            state = page.evaluate("document.body.dataset.diagrams")
            if state != "rendered":
                raise devtools.ProtocolError(
                    "diagram state is %r, so the PDF would show source" % state)
            page.print_pdf(out_path,
                           header=HEADER if running_head else "",
                           footer=FOOTER if folio else "")
        return out_path, True, ""
    except (devtools.ProtocolError, OSError) as exc:
        # Yesterday's PDF would otherwise satisfy the size test below and be
        # returned as today's, page count and all, if the fallback writes
        # nothing.
        if os.path.exists(out_path):
            try:
                os.remove(out_path)
            except OSError:
                pass
        proc = _run(browser, ["--print-to-pdf=%s" % out_path, "--no-pdf-header-footer", url],
                    timeout)
        if not os.path.exists(out_path) or os.path.getsize(out_path) < 20000:
            raise RuntimeError("the browser produced no usable PDF: %s; the protocol route "
                               "failed first with: %s"
                               % (proc.stderr.decode("utf-8", "replace")[-400:], exc))
        return out_path, False, str(exc)


def pdf_page_count(path):
    """Page count read off the PDF itself, so the report is not a guess."""
    with open(path, "rb") as fh:
        blob = fh.read()
    counts = [int(m.group(1)) for m in re.finditer(rb"/Type\s*/Pages?[^>]*?/Count\s+(\d+)", blob)]
    if counts:
        return max(counts)
    return len(re.findall(rb"/Type\s*/Page[^s]", blob)) or None


def to_pdf(html_path, pdf_path=None, audit=True, stream=sys.stdout):
    """Build the PDF next to the HTML, checking A4 fit on the way through.

    Returns (pdf path, list of A4 problems). Problems are reported, not fatal:
    a caller that wants them to fail the run checks the list.
    """
    html_path = os.path.abspath(html_path)
    directory, filename = os.path.split(html_path)
    pdf_path = pdf_path or os.path.splitext(html_path)[0] + ".pdf"
    browser = find_browser()
    problems = []

    with LocalServer(directory) as server:
        if audit:
            dom = dump_dom(browser, server.url(filename, "?audit=1"))
            state, items = read_audit(dom)
            if state != "rendered":
                broken = diagram_errors(dom)
                detail = ("; ".join(broken) if broken
                          else "check the network connection to the Mermaid CDN")
                raise RuntimeError(
                    "the diagrams did not all render (state: %s), so the PDF would show source "
                    "for at least one figure: %s" % (state, detail))
            problems = overflows(items)
            stream.write("  measured  : %d figures and tables laid out\n" % len(items))
            for item, reasons in problems:
                stream.write("  OVERFLOWS A4: %s — %s\n" % (item["label"], "; ".join(reasons)))
        _, numbered, why = print_pdf(browser, server.url(filename), pdf_path)

    pages = pdf_page_count(pdf_path)
    stream.write("  pdf       : %s (%.1f MB%s)\n" % (
        pdf_path, os.path.getsize(pdf_path) / 1048576.0,
        ", %d pages" % pages if pages else ""))
    if not numbered:
        stream.write("  WARNING   : printed through the command-line switch, so this PDF carries "
                     "no page numbers and no background graphics\n")
        stream.write("  cause     : %s\n" % why)
        # Counted as a defect rather than only announced. The reprint that fills
        # the folios sends this stream to a discarded buffer, so a warning alone
        # let a PDF with no page numbers ship under "clean, ready to deliver".
        problems = list(problems) + [
            ({"label": "the PDF itself"},
             ["printed through the command-line switch, so it has no page numbers and no "
              "background graphics: %s" % why])]
    stream.write("  engine    : %s\n" % os.path.basename(browser))
    return pdf_path, problems
