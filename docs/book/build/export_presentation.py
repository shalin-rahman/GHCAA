#!/usr/bin/env python
"""Print the requirement catalogue and the SRS to A4 PDFs in docs/presentation.

The catalogue is built from the tables of Chapter 3, sections 3.3 and 3.4. The chapter prose
is left out, and references that only make sense inside the book (section signs, table and
figure numbers, citation brackets) are rewritten or removed. The run stops if any are left,
so a new reference in the chapter shows up here instead of reaching the printout.

    python docs/book/build/export_presentation.py
"""

import datetime
import os
import re
import sys
import tempfile

import markdown

import printer

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", ".."))
OUT = os.path.join(ROOT, "docs", "presentation")

CSS = """
@page { size: A4; margin: 16mm 15mm 18mm 15mm; }
body { font-family: "Segoe UI", "Helvetica Neue", Arial, sans-serif; font-size: 9.5pt;
       line-height: 1.4; color: #1B2430; background: #fff; }
h1 { font-family: "Segoe UI Semibold", "Segoe UI", sans-serif; font-weight: 600; font-size: 18pt;
     margin: 0 0 3pt; padding-bottom: 5pt; border-bottom: 1.5pt solid #0F6C73; }
h2 { font-family: "Segoe UI Semibold", "Segoe UI", sans-serif; font-weight: 600; font-size: 13pt;
     color: #0F6C73; margin: 18pt 0 6pt; break-after: avoid; }
h3 { font-family: "Segoe UI Semibold", "Segoe UI", sans-serif; font-weight: 600; font-size: 10.5pt;
     margin: 12pt 0 4pt; break-after: avoid; }
p { margin: 0 0 6pt; }
strong, b { font-weight: 600; }
p.meta { font-size: 9pt; color: #5B6675; margin: 0 0 12pt; }
ul { margin: 0 0 6pt; padding-left: 14pt; }
li { margin: 0 0 3pt; }
table { border-collapse: collapse; width: 100%; margin: 4pt 0 10pt; font-size: 9pt; }
thead { display: table-header-group; }
tr { break-inside: avoid; }
th, td { border-bottom: 0.6pt solid #C9CED6; padding: 3.5pt 6pt; vertical-align: top; text-align: left; }
th { background: #F3F4F6; border-bottom: 1pt solid #1B2430; font-weight: 600; }
td:first-child { white-space: nowrap; font-weight: 600; }
tr.total td { border-top: 1pt solid #1B2430; font-weight: 600; }
.qas table { font-size: 8pt; }
.qas td:first-child { white-space: nowrap; }
code { font-family: Consolas, monospace; font-size: 8.5pt; }
hr { display: none; }
"""

# Book-only references, rewritten in order. Exact strings first, then the general patterns.
EXACT = [
    ("Criterion: coverage demonstrated in Table 3.4 with no M-priority requirement unmapped.",
     "Criterion: the traceability matrix leaves no M-priority requirement unmapped."),
    ("without a corresponding DC entry, and no DC entry of priority M shall be unimplemented. "
     "Criterion: the DC column of Table 3.4 is complete in both directions.",
     "without a recorded domain constraint, and no must-have domain constraint shall be "
     "unimplemented. Criterion: traceability from constraint to code is complete in both directions."),
    ("A routine change of the kind catalogued in §9.12, being the addition",
     "A routine change, such as the addition"),
    (" to DC-03 to ", " to its domain constraint to "),
    (" (§8.4)", ""),
]
PATTERNS = [
    (r" \[\d+(, ?\d+)*\]", ""),
    (r" (recorded|reported) in §[\d.]+", r" \1"),
    (r" (of|in) §[\d.]+", ""),
]
# Constitution clauses such as "Art. III §B" are real sources and stay.
LEFTOVER = re.compile(r"§|\[\d+\]|Table \d|Figure \d|Chapter \d|docs/|\.md\b|DC-\d|FR-\d{3}")
CLAUSE = re.compile(r"Art\. [IVX]+ §[A-Z]+")

PRIORITY = ("M", "S", "C")
SOURCES = ("Source: D document analysis, with the constitution article cited where it applies; "
           "I interview; O observation; C competitor analysis; R dated stakeholder request.")


def read(*parts):
    with open(os.path.join(ROOT, *parts), encoding="utf-8") as f:
        return f.read()


def clean(text):
    for old, new in EXACT:
        text = text.replace(old, new)
    for pattern, repl in PATTERNS:
        text = re.sub(pattern, repl, text)
    return text


def check(name, text):
    left = [(n, line) for n, line in enumerate(text.splitlines(), 1)
            if LEFTOVER.search(CLAUSE.sub("", line))]
    if left:
        for n, line in left:
            print("%s line %d: %s" % (name, n, line[:160]), file=sys.stderr)
        sys.exit("%s still carries book references; add a rewrite rule above" % name)


def cells(row):
    return [c.strip() for c in row.strip().strip("|").split("|")]


def table(header, rows, total=None):
    out = ["| " + " | ".join(header) + " |", "|" + " --- |" * len(header)]
    out += ["| " + " | ".join(str(c) for c in r) + " |" for r in rows]
    html = markdown.markdown("\n".join(out), extensions=["tables"])
    if total:
        row = "<tr class='total'>" + "".join("<td>%s</td>" % c for c in total) + "</tr>"
        html = html.replace("</tbody>", row + "</tbody>")
    return html


def md(text):
    return markdown.markdown(text, extensions=["tables"])


def page(title, meta, body):
    # print_pdf waits for the book's Mermaid pass to mark the body; these pages
    # have no diagrams, so they are marked done from the start.
    return ("<!doctype html><html lang='en-GB'><head><meta charset='utf-8'><title>%s</title>"
            "<style>%s</style></head><body data-diagrams='rendered'><h1>%s</h1>"
            "<p class='meta'>%s</p>%s</body></html>" % (title, CSS, title, meta, body))


def requirement_groups(ch3):
    """Return the FR areas, NFR groups and QAS rows as lists of (title, header, rows)."""
    start = ch3.index("## 3.3 Requirements Specification")
    lines = ch3[start:ch3.index("## 3.5 ", start)].splitlines()
    fr, nfr, qas = [], [], []
    current = None
    for line in lines:
        heading = re.match(r"### 3\.3\.\d+ (.+)", line)
        group = re.match(r"\*\*(.+?)\*\* \*\(", line)
        if heading:
            current = (heading.group(1), [], [])
            fr.append(current)
        elif group:
            current = (group.group(1), [], [])
            nfr.append(current)
        elif line.startswith("| ID | NFR |"):
            current = ("Quality-attribute scenarios", [], [])
            qas.append(current)
        if current and line.startswith("|") and not line.startswith("| ---"):
            row = cells(clean(line))
            if row[0] == "ID":
                current[1].extend(row)
            else:
                current[2].append(row)
    return fr, nfr, qas


def catalogue(ch3, today):
    fr, nfr, qas = requirement_groups(ch3)
    n_fr = sum(len(rows) for _, _, rows in fr)
    n_nfr = sum(len(rows) for _, _, rows in nfr)
    n_qas = len(qas[0][2])

    summary, totals = [], [0, 0, 0]
    for title, _, rows in fr:
        counts = [sum(1 for r in rows if r[-1] == p) for p in PRIORITY]
        totals = [a + b for a, b in zip(totals, counts)]
        summary.append([title, len(rows)] + counts)
    if sum(totals) != n_fr:
        sys.exit("an FR row has a priority other than M, S or C")

    body = ["<h2>Summary</h2>",
            table(["Functional area", "Requirements", "Must", "Should", "Could"], summary,
                  ["Total", n_fr] + totals),
            table(["Quality characteristic (ISO/IEC 25010:2011)", "Requirements"],
                  [[t, len(r)] for t, _, r in nfr], ["Total", n_nfr]),
            md("Priority: M must, S should, C could. " + SOURCES),
            "<h2>Functional requirements</h2>"]
    for title, header, rows in fr:
        body += ["<h3>%s</h3>" % title, table(header, rows)]
    body += ["<h2>Non-functional requirements</h2>",
             md("Each requirement has a measurable acceptance criterion. A requirement that "
                "could not be given one was rejected rather than weakened.")]
    for title, header, rows in nfr:
        body += ["<h3>%s</h3>" % title, table(header, rows)]
    body += ["<h2>Quality-attribute scenarios</h2>",
             md("Each scenario names a source, stimulus, artefact, environment, response and "
                "response measure, so the requirement it tests can be checked."),
             "<div class='qas'>%s</div>" % table(qas[0][1], qas[0][2])]

    meta = ("%d functional requirements, %d non-functional requirements and %d quality-attribute "
            "scenarios. Printed %s." % (n_fr, n_nfr, n_qas, today))
    return body, meta


def srs(text, today):
    version = re.search(r"\*\*Version:\*\* *(\S+)", text).group(1)
    body = text[text.index("## 1. "):]
    body = body[:body.index("> [!IMPORTANT]")]
    body = re.sub(r" \(FR-\d{3}(, FR-\d{3})*\)", "", body)
    body = re.sub(r" \(`docs/TODO\.md`, Work Package\s+[\d./]+\)\. See `docs/[A-Z_]+\.md`\.",
                  ".", body)

    counts, section = [], None
    for line in body.splitlines():
        heading = re.match(r"### (\d+\.\d+) (.+)", line)
        if heading and heading.group(1)[0] in "34":
            section = [heading.group(2), 0]
            counts.append(section)
        elif line.startswith("## "):
            section = None
        elif section and line.startswith("* "):
            section[1] += 1
    summary = table(["Section", "Items"], counts, ["Total", sum(c for _, c in counts)])

    meta = "Version %s. Printed %s." % (version, today)
    return ["<h2>Contents at a glance</h2>", summary, md(body)], meta


def main():
    os.makedirs(OUT, exist_ok=True)
    today = datetime.date.today().isoformat()

    cat_body, cat_meta = catalogue(read("docs", "book", "03-requirements.md"), today)
    srs_body, srs_meta = srs(read("docs", "SRS.md"), today)
    docs = {
        "GHCAA-FR-NFR-List": ("GHCAA Requirements Catalogue", cat_meta, "".join(cat_body)),
        "GHCAA-SRS": ("GHCAA Software Requirements Specification", srs_meta, "".join(srs_body)),
    }
    for name, (_, _, body) in docs.items():
        check(name, re.sub(r"<[^>]+>", "", body))

    browser = printer.find_browser()
    with tempfile.TemporaryDirectory() as tmp:
        for name, (title, meta, body) in docs.items():
            html_path = os.path.join(tmp, name + ".html")
            with open(html_path, "w", encoding="utf-8") as f:
                f.write(page(title, meta, body))
            url = "file:///" + html_path.replace(os.sep, "/")
            path, numbered, reason = printer.print_pdf(browser, url,
                                                       os.path.join(OUT, name + ".pdf"))
            print("wrote %s.pdf, %d pages%s" % (name, printer.pdf_page_count(path),
                  "" if numbered else ", no page numbers: " + reason))


if __name__ == "__main__":
    main()
