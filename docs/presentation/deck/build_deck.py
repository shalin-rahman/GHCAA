"""Build the 12-slide progress deck as docs/presentation/GHCAA-Mid.pptx.

Every diagram is drawn with native shapes and connectors, so it stays editable in PowerPoint.
The requirement counts are read from Chapter 3 through export_presentation.py, so the slide and
the printed catalogue cannot disagree. The other numbers were counted from the repository on
2026-10-31; recount them before the talk if code has landed since.

From the repo root:

    python docs/presentation/deck/build_deck.py

The output overwrites GHCAA-Mid.pptx, so hand edits made there since are lost.
"""
import os
import sys

from pptx import Presentation
from pptx.chart.data import CategoryChartData
from pptx.dml.color import RGBColor
from pptx.enum.chart import XL_CHART_TYPE, XL_LABEL_POSITION, XL_LEGEND_POSITION
from pptx.enum.dml import MSO_LINE_DASH_STYLE
from pptx.enum.shapes import MSO_CONNECTOR, MSO_SHAPE
from pptx.enum.text import MSO_ANCHOR, PP_ALIGN
from pptx.oxml.ns import qn
from pptx.oxml.xmlchemy import OxmlElement
from pptx.util import Inches, Pt

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", ".."))
OUT = os.path.join(ROOT, "docs", "presentation", "GHCAA-Mid.pptx")
sys.path.insert(0, os.path.join(ROOT, "docs", "book", "build"))
import export_presentation as catalogue  # noqa: E402

INK = RGBColor(0x1B, 0x24, 0x30)
MUTED = RGBColor(0x5B, 0x66, 0x75)
RULE = RGBColor(0xC9, 0xCE, 0xD6)
PANEL = RGBColor(0xF3, 0xF4, 0xF6)
ACCENT = RGBColor(0x0F, 0x6C, 0x73)
TINT = RGBColor(0xE4, 0xEF, 0xF0)
WHITE = RGBColor(0xFF, 0xFF, 0xFF)
FONT, SEMI = "Segoe UI", "Segoe UI Semibold"

LEFT, RIGHT = 0.6, 12.733
WIDTH = RIGHT - LEFT
TOTAL = 12
FOOTER = "GHCAA progress review  ·  31 October 2026"

prs = Presentation()
prs.slide_width, prs.slide_height = Inches(13.333), Inches(7.5)
BLANK = prs.slide_layouts[6]


def strip_style(shape):
    # The default shape style pulls theme fills and effects; every colour here is set explicitly.
    style = shape._element.find(qn("p:style"))
    if style is not None:
        shape._element.remove(style)


def runs(paragraph, parts, size, color, semi=False):
    if isinstance(parts, str):
        parts = [(parts, semi)]
    for value, bold in parts:
        r = paragraph.add_run()
        r.text = value
        r.font.name = SEMI if bold else FONT
        r.font.size = Pt(size)
        r.font.color.rgb = color


def fill_text(frame, paras, anchor=MSO_ANCHOR.TOP, align=PP_ALIGN.LEFT, margin=0.0):
    """paras: tuples of (parts, size, color[, semi[, space_after_pt]])."""
    frame.word_wrap = True
    frame.auto_size = None
    frame.vertical_anchor = anchor
    frame.margin_left = frame.margin_right = Inches(margin)
    frame.margin_top = frame.margin_bottom = Inches(min(margin, 0.05))
    for i, para in enumerate(paras):
        parts, size, color, semi, after = tuple(para) + (False, 0)[len(para) - 3:]
        p = frame.paragraphs[0] if i == 0 else frame.add_paragraph()
        p.alignment = align
        p.space_after = Pt(after)
        runs(p, parts, size, color, semi)


def text(slide, x, y, w, h, paras, **kw):
    box = slide.shapes.add_textbox(Inches(x), Inches(y), Inches(w), Inches(h))
    fill_text(box.text_frame, paras, **kw)
    return box


def rect(slide, x, y, w, h, fill=None, line=None, paras=(), dash=False, weight=0.75,
         anchor=MSO_ANCHOR.MIDDLE, align=PP_ALIGN.CENTER, margin=0.08):
    shape = slide.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(x), Inches(y), Inches(w), Inches(h))
    strip_style(shape)
    if fill is None:
        shape.fill.background()
    else:
        shape.fill.solid()
        shape.fill.fore_color.rgb = fill
    if line is None:
        shape.line.fill.background()
    else:
        shape.line.color.rgb = line
        shape.line.width = Pt(weight)
        if dash:
            shape.line.dash_style = MSO_LINE_DASH_STYLE.DASH
    fill_text(shape.text_frame, list(paras), anchor=anchor, align=align, margin=margin)
    return shape


def arrow_end(connector):
    ln = connector.line._get_or_add_ln()
    tail = OxmlElement("a:tailEnd")
    tail.set("type", "triangle")
    tail.set("w", "med")
    tail.set("len", "med")
    ln.append(tail)


def link(slide, a, b, a_side=3, b_side=1, color=INK, weight=1.0, arrow=True):
    """Straight connector glued to two shapes. Sides: 0 top, 1 left, 2 bottom, 3 right."""
    c = slide.shapes.add_connector(MSO_CONNECTOR.STRAIGHT, 0, 0, 0, 0)
    strip_style(c)
    c.begin_connect(a, a_side)
    c.end_connect(b, b_side)
    c.line.color.rgb = color
    c.line.width = Pt(weight)
    if arrow:
        arrow_end(c)
    return c


def line(slide, x1, y1, x2, y2, color=RULE, weight=0.75, arrow=False):
    c = slide.shapes.add_connector(MSO_CONNECTOR.STRAIGHT, Inches(x1), Inches(y1), Inches(x2), Inches(y2))
    strip_style(c)
    c.line.color.rgb = color
    c.line.width = Pt(weight)
    if arrow:
        arrow_end(c)
    return c


def hline(slide, x, y, w, color=RULE, weight=0.75):
    return line(slide, x, y, x + w, y, color, weight)


def label(slide, x, y, w, caption, color=MUTED):
    return text(slide, x, y, w, 0.3, [(caption.upper(), 10.5, color, True)])


def new_slide(number, title, notes):
    slide = prs.slides.add_slide(BLANK)
    text(slide, LEFT, 0.38, WIDTH, 0.62, [(title, 26, INK, True)], anchor=MSO_ANCHOR.BOTTOM)
    hline(slide, LEFT, 1.12, WIDTH)
    hline(slide, LEFT, 1.12, 0.9, ACCENT, 2.5)
    text(slide, LEFT, 7.0, 8, 0.3, [(FOOTER, 10, MUTED)])
    text(slide, RIGHT - 1.5, 7.0, 1.5, 0.3, [("%d / %d" % (number, TOTAL), 10, MUTED)],
         align=PP_ALIGN.RIGHT)
    slide.notes_slide.notes_text_frame.text = notes
    return slide


def set_border(cell, color, width_pt, sides):
    tc = cell._tc.get_or_add_tcPr()
    # The schema puts borders before the cell fill; PowerPoint ignores them if they come after.
    for i, side in enumerate(("L", "R", "T", "B")):
        ln = OxmlElement("a:ln%s" % side)
        if side in sides:
            ln.set("w", str(int(Pt(width_pt))))
            solid = OxmlElement("a:solidFill")
            clr = OxmlElement("a:srgbClr")
            clr.set("val", str(color))
            solid.append(clr)
            ln.append(solid)
        else:
            ln.set("w", "0")
            ln.append(OxmlElement("a:noFill"))
        tc.insert(i, ln)


def table(slide, x, y, widths, rows, size=11, row_h=0.34, header_h=None, bold_first=True,
          total=False):
    shape = slide.shapes.add_table(len(rows), len(widths), Inches(x), Inches(y),
                                   Inches(sum(widths)), Inches(row_h * len(rows)))
    tbl = shape.table
    # "No Style, No Grid": every fill and border below is set by hand.
    tbl._tbl.tblPr.find(qn("a:tableStyleId")).text = "{2D5ABB26-0587-4C30-8999-92F81FD0307C}"
    tbl.first_row = False
    tbl.horz_banding = False
    for i, w in enumerate(widths):
        tbl.columns[i].width = Inches(w)
    for r, row in enumerate(rows):
        tbl.rows[r].height = Inches(header_h if (r == 0 and header_h) else row_h)
        head = r == 0
        last = total and r == len(rows) - 1
        for c, value in enumerate(row):
            cell = tbl.cell(r, c)
            cell.margin_left = cell.margin_right = Inches(0.08)
            cell.margin_top = cell.margin_bottom = Inches(0.04)
            cell.vertical_anchor = MSO_ANCHOR.MIDDLE if head else MSO_ANCHOR.TOP
            cell.fill.solid()
            cell.fill.fore_color.rgb = PANEL if head else WHITE
            cell.text_frame.word_wrap = True
            runs(cell.text_frame.paragraphs[0], str(value), size, INK,
                 head or last or (bold_first and c == 0))
            set_border(cell, INK if (head or last) else RULE, 1.0 if (head or last) else 0.75,
                       ("B", "T") if last else ("B",))
    return shape


def chart_style(chart, size=11):
    chart.font.name = FONT
    chart.font.size = Pt(size)
    chart.font.color.rgb = INK
    chart.value_axis.visible = False
    chart.value_axis.has_major_gridlines = False
    ca = chart.category_axis
    ca.format.line.color.rgb = RULE
    ca.has_major_gridlines = False
    ca.tick_labels.font.size = Pt(size)
    ca.tick_labels.font.color.rgb = INK
    # Bar charts list categories bottom-up; reversing keeps the source order top-down.
    ca.reverse_order = True


ch3 = catalogue.read("docs", "book", "03-requirements.md")
FR, NFR, QAS = catalogue.requirement_groups(ch3)
N_FR = sum(len(r) for _, _, r in FR)
N_NFR = sum(len(r) for _, _, r in NFR)
N_QAS = len(QAS[0][2])
TESTS = [("Backend tests  [Test]", 1530), ("Backend cases  [TestCase]", 292),
         ("Angular specs", 905), ("Flutter tests", 372)]
N_TESTS = sum(n for _, n in TESTS)


def short_area(title):
    return title.split(",")[0].replace(" Application Requirements", "")


# 1. Title

s = prs.slides.add_slide(BLANK)
text(s, LEFT, 0.6, 9, 0.35, [("PROGRESS REVIEW  ·  31 OCTOBER 2026", 11, ACCENT, True)])
text(s, LEFT, 1.25, 11.2, 2.3,
     [("Design, Development and Evaluation of a Specification-Driven, Reusable Alumni "
       "Management Platform for Resource-Constrained Institutions", 32, INK, True)],
     anchor=MSO_ANCHOR.BOTTOM)
hline(s, LEFT, 3.85, 1.2, ACCENT, 3)
text(s, LEFT, 4.05, 11, 0.5,
     [("Platform progress. Focus: the election module and the data model.", 18, MUTED)])
hline(s, LEFT, 5.35, WIDTH)
text(s, LEFT, 5.5, 8, 1.3, [
    ("Md Habibur Rahman  ·  EMIT, Batch 7  ·  2512220", 15, INK, True, 4),
    ("Supervised by Professor Dr. Kazi Muheymin-Us-Sakib", 14, MUTED, False, 4),
])
s.notes_slide.notes_text_frame.text = (
    "Progress since the last review, focused on two parts: the election module and the data "
    "model behind it. Every number in the deck was counted from the repository on 31 October.")

# 2. At a glance

s = new_slide(2, "Where the project stands",
              "These are counts of what exists in the code, not of features used in production. "
              "Test declarations are not a pass rate; quote the latest run separately.")
tiles = [
    ("406", "HTTP endpoints across 49 API controllers"),
    ("83", "entity types in the database; 24 serve elections and polls"),
    ("34", "PostgreSQL schema migrations"),
    ("{:,}".format(N_TESTS), "test declarations across backend, web and mobile"),
    ("%d / %d / %d" % (N_FR, N_NFR, N_QAS), "functional requirements / quality requirements / scenarios"),
    ("378", "commits in the repository"),
]
col_w = 3.75
gap = (WIDTH - 3 * col_w) / 2
for i, (big, small) in enumerate(tiles):
    x = LEFT + (i % 3) * (col_w + gap)
    y = 1.55 + (i // 3) * 2.0
    hline(s, x, y, col_w, INK, 1)
    text(s, x, y + 0.15, col_w, 0.85, [(big, 40, INK, True)])
    text(s, x, y + 1.0, col_w, 0.7, [(small, 13, MUTED)])
hline(s, LEFT, 5.75, WIDTH)
text(s, LEFT, 5.88, WIDTH, 0.4, [([("Stack   ", True), (
    "ASP.NET Core 9  ·  EF Core 9  ·  PostgreSQL 16  ·  Angular 21  ·  Flutter  ·  SignalR  ·  "
    "S3 object storage", False)], 13, INK)])
text(s, LEFT, 6.3, WIDTH, 0.4, [([("Clients   ", True), (
    "one API serves the Angular web app and the Flutter mobile app", False)], 13, INK)])

# 3. Architecture: context and layers

s = new_slide(3, "Architecture: one API, four layers, dependencies point inward",
              "Clean Architecture. Domain has no outward references; Application defines the "
              "interfaces; Infrastructure implements them with EF Core, storage, email and payment "
              "adapters; the API layer hosts controllers, SignalR hubs and the middleware pipeline.")
label(s, LEFT, 1.4, 2.4, "Clients")
clients = [
    ("Web app", "Angular 21, Signals", "member, admin and officials areas"),
    ("Mobile app", "Flutter, Riverpod", "member and officials areas"),
    ("Public pages", "no sign-in", "officials board, results, audit check"),
]
cboxes = []
for i, (name, tech, use) in enumerate(clients):
    cboxes.append(rect(s, LEFT, 1.8 + i * 1.3, 2.25, 1.1, PANEL, None, [
        (name, 13, INK, True, 2), (tech, 11, ACCENT, False, 2), (use, 10.5, MUTED)],
        align=PP_ALIGN.LEFT, margin=0.12))

api_x, api_w = 3.75, 5.8
host = rect(s, api_x, 1.45, api_w, 4.3, None, RULE, weight=1.0)
text(s, api_x + 0.2, 1.52, 4, 0.32, [("ASP.NET Core 9 API host", 12, INK, True)])
layers = [
    ("API", "49 controllers · SignalR hubs for chat and notifications · middleware · health checks"),
    ("Infrastructure", "EF Core 9 data access · S3 storage · email · payment adapters"),
    ("Application", "services and use cases · DTOs · validation · interfaces the outer layers implement"),
    ("Domain", "83 entity types · enums · business rules · no references to other layers"),
]
bands = []
for i, (name, body) in enumerate(layers):
    core = i == 3
    bands.append(rect(s, api_x + 0.45, 1.95 + i * 0.92, api_w - 0.65, 0.8, TINT if core else PANEL,
                      ACCENT if core else None, [([(name + "   ", True), (body, False)], 11, INK)],
                      align=PP_ALIGN.LEFT, margin=0.12))
for i in range(3):
    link(s, bands[i], bands[i + 1], 2, 0, MUTED, 1.0)
text(s, api_x, 5.85, api_w, 0.5, [(
    "Arrows show compile-time references. Each layer references only the layers below it.",
    10.5, MUTED)])
# Straight arrows from each client to the host edge, so none of them cross a label.
for i, wire in enumerate(["HTTPS\nJSON, JWT", "HTTPS\n+ SignalR", "HTTPS\nread only"]):
    mid = 1.8 + i * 1.3 + 0.55
    line(s, LEFT + 2.25, mid, api_x, mid, INK, 1.0, arrow=True)
    text(s, LEFT + 2.27, mid - 0.5, api_x - LEFT - 2.29, 0.46, [(wire, 9.5, MUTED)],
         align=PP_ALIGN.CENTER)

label(s, 10.3, 1.4, 2.4, "Services")
services = [
    ("PostgreSQL 16", "Neon in pre-production"),
    ("Object storage", "S3-compatible, member uploads"),
    ("Email delivery", "one-time codes and notices"),
    ("Payment gateway", "SSLCommerz, off by default"),
]
for i, (name, use) in enumerate(services):
    box = rect(s, 10.3, 1.8 + i * 1.0, RIGHT - 10.3, 0.82, None, RULE, [
        (name, 12, INK, True, 2), (use, 10.5, MUTED)], align=PP_ALIGN.LEFT, margin=0.12)
    link(s, bands[1], box, 3, 1, MUTED, 1.0)
text(s, 10.3, 5.85, RIGHT - 10.3, 0.5, [("Pre-production runs on Render.", 10.5, MUTED)])

# 4. Pipeline and decisions

s = new_slide(4, "Every request takes the same path",
              "The order of the middleware matters. Exceptions are caught before anything else can "
              "fail; rate limits apply before any identity work; the security stamp is checked "
              "after the token is validated, so a revoked session stops on its next request.")
label(s, LEFT, 1.4, 6, "Request pipeline, in order")
stages = [
    ("Transport", ["CORS", "compression, output cache", "HTTPS redirect"]),
    ("Safety net", ["exception handler", "security headers", "audit log"]),
    ("Throttle", ["sign-in 5 per min", "registration 10 per 5 min", "API 100 per min"]),
    ("Connection", ["WebSockets", "static files", "hub token from query"]),
    ("Identity", ["JWT validation", "security stamp check", "authorisation policies"]),
    ("Endpoints", ["controllers", "SignalR hubs", "health checks"]),
]
sw = 1.83
sg = (WIDTH - 6 * sw) / 5
prev = None
for i, (name, items) in enumerate(stages):
    last = i == len(stages) - 1
    paras = [(name, 12, WHITE if last else ACCENT, True, 4)] + \
            [(t, 11, WHITE if last else INK, False, 2) for t in items]
    box = rect(s, LEFT + i * (sw + sg), 1.75, sw, 1.65, ACCENT if last else PANEL, None, paras,
               anchor=MSO_ANCHOR.TOP, align=PP_ALIGN.LEFT, margin=0.1)
    if prev is not None:
        link(s, prev, box, 3, 1, INK, 1.0)
    prev = box

label(s, LEFT, 3.7, 6, "Design decisions")
table(s, LEFT, 4.05, [3.55, 4.1], [
    ["Decision", "Reason"],
    ["JWT plus a security stamp checked per request", "A revoked session or changed role stops at once"],
    ["Settings stored in the database, edited by admins", "A limit or flag changes without a redeploy"],
    ["Payments recorded manually; gateway off by default", "Matches bKash, Nagad, Rocket and bank use"],
    ["Rule-based assistant, no language model", "Answers come only from site data"],
    ["One API for web and mobile", "One set of rules and one contract for both"],
], size=11, row_h=0.46, header_h=0.36)

label(s, 8.95, 3.7, 3.8, "CI pipeline, run in order")
prev = None
for i, step in enumerate(["Static analysis", "API tests", "Web UI tests", "Mobile tests", "Build"]):
    box = rect(s, 8.95, 4.08 + i * 0.55, RIGHT - 8.95, 0.4, PANEL, None, [(step, 11, INK)],
               align=PP_ALIGN.LEFT, margin=0.12)
    if prev is not None:
        link(s, prev, box, 2, 0, MUTED, 1.0)
    prev = box

# 5. Lifecycle

s = new_slide(5, "Election lifecycle: ten statuses, five two-person gates",
              "Dates move the election through nomination and withdrawal on their own. The five "
              "gated steps need one official to request and a different official to approve. "
              "From the open-polling request until the result is declared, the election rules are "
              "frozen; a change needs a time-limited unlock with a written reason.")
statuses = ["Announced", "Nomination", "Scrutiny", "Withdrawal", "Candidate list", "Campaign",
            "Polling", "Counting", "Declared", "Archived"]
bw = 1.04
bg = (WIDTH - 10 * bw) / 9
xs = [LEFT + i * (bw + bg) for i in range(10)]
for caption, a, b in (("Setup", 0, 5), ("Live", 6, 7), ("Closed", 8, 9)):
    label(s, xs[a], 1.42, xs[b] + bw - xs[a], caption)
    hline(s, xs[a], 1.75, xs[b] + bw - xs[a], INK, 0.75)
boxes = []
for i, name in enumerate(statuses):
    live, closed = i in (6, 7), i >= 8
    boxes.append(rect(s, xs[i], 1.9, bw, 0.72, ACCENT if live else (WHITE if closed else PANEL),
                      INK if closed else None, [(name, 11, WHITE if live else INK, live)],
                      margin=0.04))
gates = {0, 5, 6, 7, 8}
for i in range(9):
    link(s, boxes[i], boxes[i + 1], 3, 1, ACCENT if i in gates else MUTED,
         2.0 if i in gates else 1.0)
fx = xs[5] + bw / 2
hline(s, fx, 2.8, xs[8] - fx, ACCENT, 3)
text(s, fx, 2.86, 5.5, 0.3, [("Rules frozen from the open-polling request until declaration",
                                10.5, ACCENT)])
table(s, LEFT, 3.3, [0.45, 2.85, 7.15, 1.68], [
    ["", "Move", "What has to happen", "Two people"],
    ["1", "Announced → Nomination", "Setup published", "Yes"],
    ["2", "Nomination → Scrutiny", "Nomination closing date passes", ""],
    ["3", "Scrutiny → Withdrawal", "Every nomination ruled on", ""],
    ["4", "Withdrawal → Candidate list", "Withdrawal closing date passes", ""],
    ["5", "Candidate list → Campaign", "List published; order fixed for every voter", ""],
    ["6", "Campaign → Polling", "Ballot key in place; approval opens polling", "Yes"],
    ["7", "Polling → Counting", "Approval closes polling", "Yes"],
    ["8", "Counting → Declared", "Ballots opened together in random order; approval declares", "Yes"],
    ["9", "Declared → Archived", "Appeal window closed; approval archives", "Yes"],
], size=10.5, row_h=0.32, bold_first=False)

# 6. Roles and the two-person rule

s = new_slide(6, "Who acts where: 14 posts, 3 portals, no single-person control",
              "Posts are filled per election and never carry over. A post is active only after "
              "the appointee accepts and signs a neutrality declaration. A candidate cannot hold "
              "a post, and there is one live Returning Officer at a time.")
label(s, LEFT, 1.4, 5, "Portals")
portals = [
    ("Admin web", "set up the election, appoint officials, generate the ballot key"),
    ("Officials area, web and mobile", "accept a post, sign the declaration, rule on nominations, "
                                       "approve as second signer, run the count"),
    ("Member web and mobile", "nominate, second, vote, keep the receipt, read the board and results"),
]
for i, (name, does) in enumerate(portals):
    rect(s, LEFT, 1.78 + i * 0.86, 5.9, 0.76, PANEL, None, [(name, 12, INK, True, 1), (does, 11, MUTED)],
         align=PP_ALIGN.LEFT, margin=0.12)
label(s, LEFT, 4.5, 5, "Posts in an election")
posts = ["Chief Commissioner", "Commissioner", "Returning Officer", "Assistant Returning Officer",
         "Presiding Officer", "Polling Officer", "Scrutineer", "Counting Supervisor",
         "Technical Administrator", "Cybersecurity Auditor", "Security Officer", "Observer",
         "Appeal Tribunal Member", "Search Committee Member"]
for col in range(2):
    text(s, LEFT + col * 3.0, 4.85, 2.95, 2.0,
         [(p, 11, INK, False, 1) for p in posts[col * 7:(col + 1) * 7]])

rx = 7.0
label(s, rx, 1.4, 5.7, "The two-person rule")
steps = [("Request", "official A"), ("Approve", "official B, never A"), ("Run", "a separate step")]
sw2 = 1.6
sg2 = (RIGHT - rx - 3 * sw2) / 2
prev = None
for i, (name, who) in enumerate(steps):
    box = rect(s, rx + i * (sw2 + sg2), 1.78, sw2, 0.76, TINT if i == 1 else PANEL,
               ACCENT if i == 1 else None, [(name, 12, INK, True, 1), (who, 10.5, MUTED)])
    if prev is not None:
        link(s, prev, box, 3, 1, INK, 1.0)
    prev = box
text(s, rx, 2.75, 5.7, 0.3, [("Gated actions, configurable per organisation", 11, INK, True)])
actions = ["Publish", "Open polling", "Replace key", "Close polling", "Count", "Declare", "Archive"]
cw = 1.3
cg = (RIGHT - rx - 4 * cw) / 3
for i, a in enumerate(actions):
    rect(s, rx + (i % 4) * (cw + cg), 3.1 + (i // 4) * 0.5, cw, 0.38, None, ACCENT,
         [(a, 11, ACCENT)], margin=0.03)
rect(s, rx + 3 * (cw + cg), 3.6, cw, 0.38, ACCENT, None, [("Emergency revoke", 10.5, WHITE, True)],
     margin=0.03)
text(s, rx, 4.2, RIGHT - rx, 2.6, [
    ([("Emergency revoke ", True), ("always needs a second person; there is no bypass.", False)], 11, INK, False, 5),
    ([("Bypass for the other seven ", True), ("is a SuperAdmin emergency setting, off by default.", False)], 11, INK, False, 5),
    ([("Duplicates refused: ", True), ("a second open request for the same step is rejected; an approved, unrun count still counts as open.", False)], 11, INK, False, 5),
    ([("Access ends ", True), ("21 days after declaration by default, without anyone acting.", False)], 11, INK, False, 0),
])

# 7. Election records

s = new_slide(7, "Election records: the missing link is the secrecy",
              "Read the slide by what is not drawn. Member links to five records. SeatVote records "
              "only that a member voted for a seat. No foreign key from Member reaches the four "
              "ballot tables, and the audit chain logs a vote without its seat or time of day. "
              "The database holds 83 entity types in all; 24 belong to elections and polls.")
member = rect(s, LEFT, 3.55, 1.45, 0.9, INK, None, [("Member", 13, WHITE, True)])
c1x, c2x, c3x = 2.75, 6.3, 9.75
label(s, c1x, 1.4, 3.2, "Linked to a member")
label(s, c2x, 1.4, 3.2, "Never linked to a member", ACCENT)
label(s, c3x, 1.4, 3.0, "Owned by the election")
linked = [("ElectionAppointment", "post, acceptance, declaration"),
          ("VoterRoll", "eligibility frozen, voted at"),
          ("Nomination", "candidate, proposer, seconder, ballot order"),
          ("SeatVote", "member, seat, time; nothing about choices"),
          ("ECMember", "winners in post")]
for i, (name, fields) in enumerate(linked):
    box = rect(s, c1x, 1.8 + i * 0.93, 3.1, 0.78, PANEL, None,
               [(name, 11.5, INK, True, 1), (fields, 10.5, MUTED)], align=PP_ALIGN.LEFT, margin=0.12)
    link(s, member, box, 3, 1, INK, 1.0, arrow=False)
rect(s, c2x - 0.15, 1.72, 3.4, 4.0, None, ACCENT, dash=True, weight=1.25)
sealed = [("PendingBallot", "sealed choices; no member, no time"),
          ("BallotReceipt", "tracking code; no link to any ballot"),
          ("Ballot", "spoiled flag; no member, no time"),
          ("BallotVote", "one row per chosen candidate")]
sboxes = []
for i, (name, fields) in enumerate(sealed):
    sboxes.append(rect(s, c2x, 1.85 + i * 0.95, 3.1, 0.78, TINT, None,
                       [(name, 11.5, INK, True, 1), (fields, 10.5, MUTED)],
                       align=PP_ALIGN.LEFT, margin=0.12))
link(s, sboxes[2], sboxes[3], 2, 0, MUTED, 1.0)
text(s, c2x - 0.15, 5.85, 3.4, 0.95, [(
    "No foreign key from Member reaches these four tables. At the count, sealed rows are opened "
    "together, shuffled and stored under random keys.", 10.5, ACCENT)])
owned = [("Election", "status, dates, tie rule, public key"),
         ("ElectionSeat", "position, seat count"),
         ("ElectionPersona", "post, headcount, permissions"),
         ("ElectionApproval", "action, requester, approver"),
         ("ScrutinyDecision", "officer, accepted, reason"),
         ("ElectionResult", "votes, elected, tie"),
         ("ElectionAuditEntry", "hash chain; no seat or time on votes")]
for i, (name, fields) in enumerate(owned):
    rect(s, c3x, 1.8 + i * 0.7, RIGHT - c3x, 0.6, None, RULE,
         [(name, 11, INK, True), (fields, 10, MUTED)], align=PP_ALIGN.LEFT, margin=0.1)

# 8. Secrecy flow

s = new_slide(8, "Casting and counting: sealed at cast, opened together",
              "Trust model, stated plainly: this is a trusted-server secret ballot, not an end-to-end "
              "verifiable one. Whoever holds the private key can open every sealed ballot. The "
              "Returning Officer keeps the key off the server, a second person must approve the "
              "count, and the key is destroyed afterwards.")
flows = [
    ("While polling is open  ·  one database transaction", [
        ("Confirm identity", "member signs in and enters a one-time code"),
        ("Record that they voted", "frozen roll checked; SeatVote stores member, seat and time"),
        ("Seal the choices", "padded and sealed with the election's public key into PendingBallot"),
        ("Hand back a receipt", "tracking code in BallotReceipt, linked to no ballot"),
    ]),
    ("At the count  ·  two-person approval", [
        ("Supply the key", "Returning Officer brings the private key, held off the server"),
        ("Open together", "every sealed row is opened in one pass and shuffled"),
        ("Store anonymously", "Ballot and BallotVote under random keys; sealed rows deleted"),
        ("Publish", "results, plus shuffled ballots that anyone can re-add"),
    ]),
]
fw = 2.85
fg = (WIDTH - 4 * fw) / 3
n = 0
for r, (caption, items) in enumerate(flows):
    y = 1.42 + r * 1.85
    label(s, LEFT, y, WIDTH, caption, ACCENT)
    prev = None
    for i, (head, body) in enumerate(items):
        n += 1
        box = rect(s, LEFT + i * (fw + fg), y + 0.38, fw, 1.1, PANEL, None, [
            ("%d  %s" % (n, head), 12, INK, True, 4), (body, 11, MUTED)],
            anchor=MSO_ANCHOR.TOP, align=PP_ALIGN.LEFT, margin=0.12)
        if prev is not None:
            link(s, prev, box, 3, 1, INK, 1.0)
        prev = box
pw = (WIDTH - 0.3) / 2
rect(s, LEFT, 5.3, pw, 1.0, None, RULE, [
    ("Tamper evidence", 12, INK, True, 3),
    ("Each significant action is appended to a hash chain. Anyone can check it through a public "
     "verify endpoint; a changed, removed or reordered entry breaks the chain.", 11, MUTED)],
    anchor=MSO_ANCHOR.TOP, align=PP_ALIGN.LEFT, margin=0.12)
rect(s, LEFT + pw + 0.3, 5.3, pw, 1.0, None, ACCENT, [
    ("Disclosed limit", 12, ACCENT, True, 3),
    ("A backup taken during polling, together with the private key, could show roughly when "
     "someone voted. The key is destroyed after the election.", 11, MUTED)],
    anchor=MSO_ANCHOR.TOP, align=PP_ALIGN.LEFT, margin=0.12)

# 9. Requirements

s = new_slide(9, "Requirements: %d functional, %d quality, %d scenarios" % (N_FR, N_NFR, N_QAS),
              "Priorities use MoSCoW. Every quality requirement has a measurable acceptance "
              "criterion; ones that could not be given one were rejected. The full catalogue is "
              "the printed handout.")
label(s, LEFT, 1.4, 7, "Functional requirements by area and priority")
cd = CategoryChartData()
cd.categories = [short_area(t) for t, _, _ in FR]
for name, code in (("Must", "M"), ("Should", "S"), ("Could", "C")):
    cd.add_series(name, [sum(1 for r in rows if r[-1] == code) for _, _, rows in FR])
chart = s.shapes.add_chart(XL_CHART_TYPE.BAR_STACKED, Inches(LEFT), Inches(1.75), Inches(7.2),
                           Inches(4.75), cd).chart
chart_style(chart)
chart.has_legend = True
chart.legend.position = XL_LEGEND_POSITION.TOP
chart.legend.include_in_layout = False
chart.legend.font.size = Pt(11)
plot = chart.plots[0]
plot.gap_width = 55
plot.overlap = 100
for series, color, label_color in zip(plot.series, (ACCENT, MUTED, RULE), (WHITE, WHITE, INK)):
    series.format.fill.solid()
    series.format.fill.fore_color.rgb = color
    series.format.line.fill.background()
    dl = series.data_labels
    dl.show_value = True
    dl.font.size = Pt(10.5)
    dl.font.color.rgb = label_color
    dl.position = XL_LABEL_POSITION.CENTER
    # A zero-length segment would still print "0"; this format leaves zeros blank.
    dl.number_format = "0;;;"
    dl.number_format_is_linked = False
by_priority = [sum(1 for _, _, rows in FR for r in rows if r[-1] == c) for c in "MSC"]
text(s, LEFT, 6.5, 7.2, 0.35, [("Must %d  ·  Should %d  ·  Could %d" % tuple(by_priority), 11, MUTED)])

label(s, 8.3, 1.4, 4.4, "Quality requirements, ISO/IEC 25010")
table(s, 8.3, 1.75, [3.25, 1.18],
      [["Characteristic", "Count"]] + [[t, len(r)] for t, _, r in NFR] + [["Total", N_NFR]],
      size=11, row_h=0.32, total=True)

# 10. Verification and demo

s = new_slide(10, "Verification: three test suites and a scripted election",
              "Counts are test declarations found in source on 31 October, not a pass rate; run "
              "the suites before the talk and quote the result separately. The design has been "
              "checked by tests and the scripted run; it has not yet been used in a live election. "
              "Demo: scripts\\run-election-walkthrough.bat setup the day before, then resume slow "
              "in the room; clean-election-test-data.bat afterwards.")
label(s, LEFT, 1.4, 7, "Test declarations in source")
cd = CategoryChartData()
cd.categories = [name for name, _ in TESTS]
cd.add_series("Tests", [v for _, v in TESTS])
chart = s.shapes.add_chart(XL_CHART_TYPE.BAR_CLUSTERED, Inches(LEFT), Inches(1.75), Inches(7.0),
                           Inches(3.4), cd).chart
chart_style(chart, 11.5)
chart.has_legend = False
chart.has_title = False
plot = chart.plots[0]
plot.gap_width = 60
series = plot.series[0]
series.format.fill.solid()
series.format.fill.fore_color.rgb = ACCENT
dl = series.data_labels
dl.show_value = True
dl.number_format = "#,##0"
dl.number_format_is_linked = False
dl.font.size = Pt(11.5)
dl.font.color.rgb = INK
dl.position = XL_LABEL_POSITION.OUTSIDE_END
hline(s, LEFT, 5.35, 7.0)
text(s, LEFT, 5.45, 3.4, 1.3, [("517", 32, INK, True), ("election test attributes in 37 backend files", 11, MUTED)])
text(s, LEFT + 3.6, 5.45, 3.4, 1.3, [("{:,}".format(N_TESTS), 32, INK, True),
                                     ("declarations in all, counted from source", 11, MUTED)])

label(s, 8.3, 1.4, 4.4, "Scripted election run")
demo = [("Set up", "election announced through campaign"),
        ("Open polling", "two officials approve"),
        ("Vote", "ten scripted members cast ballots"),
        ("Close and count", "two officials approve each step"),
        ("Declare", "results and shuffled ballots published"),
        ("Report", "the run writes a report of every step")]
prev = None
for i, (head, body) in enumerate(demo):
    box = rect(s, 8.3, 1.78 + i * 0.84, RIGHT - 8.3, 0.66, PANEL, None,
               [([(head + "   ", True), (body, False)], 11, INK)], align=PP_ALIGN.LEFT, margin=0.12)
    if prev is not None:
        link(s, prev, box, 2, 0, MUTED, 1.0)
    prev = box

# 11. Standards

s = new_slide(11, "Standards as design baselines, not certification",
              "None of these is law for the association. They are used to check the design, and "
              "each row says where the design deliberately stops short. Certification would need "
              "an independent assessor, and no such claim is made.")
table(s, LEFT, 1.45, [1.95, 2.6, 4.25, 3.33], [
    ["Standard", "Expects", "How the design answers", "Where it stops short"],
    ["CoE CM/Rec(2017)5", "secret, equal and verifiable e-voting",
     "sealed ballots, frozen roll, random candidate order, tracking codes, published shuffled ballots",
     "no end-to-end cryptographic proof"],
    ["VVSG 2.0", "secrecy, separation of duties, error handling",
     "padded sealed ballots; posts per election; a failed cast writes nothing",
     "no paper or hardware trail; no certification or load testing"],
    ["ISO/IEC 27001", "access control, cryptography, logging",
     "access tied to an accepted post and ended automatically; two-person rule; hash-chained log",
     "the management system is the organisation's job"],
    ["ISO/IEC 27018", "data minimisation and retention",
     "sealed ballot holds no member and no time; key destroyed after the election",
     "sub-processor and contract matters"],
    ["WCAG 2.2 AA", "accessible voting screens",
     "keyboard, screen reader, contrast, focus and a review-then-cast step",
     "assisted voting goes through the paper procedure"],
], size=11, row_h=0.72, header_h=0.36)
text(s, LEFT, 5.62, WIDTH, 1.1, [
    ([("Left out on purpose.  ", True), (
        "A split ballot key, sealing on the voter's device, a roll objection window, load testing "
        "and certification. Each is named as a limit rather than claimed.", False)], 12, INK)])

# 12. What is left, and questions

s = new_slide(12, "What is left, and questions",
              "The tracker counts understate progress, because finished items move to an archive. "
              "Phone checks on a real device and two follow-up fixes wait for the election "
              "calendar.")
label(s, LEFT, 1.4, 6.5, "Open work")
items = [
    ("86 to do, 28 partial", "in the tracker; finished work moves to an archive, so these counts understate progress"),
    ("Waiting on the election calendar", "phone checks on a real device and two follow-up fixes"),
    ("First live election", "then a report on how it ran, set against the quality scenarios"),
]
for i, (head, body) in enumerate(items):
    y = 1.8 + i * 1.3
    hline(s, LEFT, y, 6.6, RULE)
    text(s, LEFT, y + 0.12, 6.6, 1.05, [(head, 15, INK, True, 3), (body, 12.5, MUTED)])
rect(s, 8.0, 1.45, RIGHT - 8.0, 5.3, PANEL, None, [
    ("Questions", 40, INK, True, 10),
    ("Thank you", 20, MUTED, False, 26),
    ("SOURCE CODE", 10.5, ACCENT, True, 3),
    ("github.com/shalin-rahman/GHCAA", 14, INK)],
    anchor=MSO_ANCHOR.MIDDLE, align=PP_ALIGN.LEFT, margin=0.45)

prs.save(OUT)
print("wrote", os.path.relpath(OUT, ROOT), "with", len(prs.slides), "slides")
