"""Regenerate Table 3.4 (the requirements traceability matrix extract) from the
[Category] tags in GHCAA.Tests.

    python docs/book/build/traceability.py              # the table, to stdout
    python docs/book/build/traceability.py --markdown   # the same, as a Markdown table
    python docs/book/build/traceability.py --check      # exit non-zero if the Test
                                                         # column this script computes
                                                         # disagrees with the one printed
                                                         # in 03-requirements.md

Table 3.4 has six columns: requirement, DC or clause, use case, design element,
implementation artefact and test. Only the last one is derivable from the tree — which
NUnit tests actually carry `[Category("FR-NN")]` or `[Category("DC-NN")]` is a fact
about GHCAA.Tests, not a judgement call. The other five are the analyst's reading of
the requirement (which use case it serves, which design section covers it, which
artefact implements it) and stay in ROWS, hand-authored, the way wbs.py keeps CODE and
DOCS hand-authored while deriving effort and dates from git.

A requirement with no tagged test is reported as a gap rather than skipped, because
that is the fact the table exists to surface: FR-19, FR-32, FR-33, FR-36, FR-37 and
FR-38 all had prose in Table 3.4 describing a named test before any such tag existed,
and --check exists so that overstatement cannot recur silently.

A row with more than MAX_TESTS_SHOWN tests prints the first one plus a count rather
than every name, so a requirement covered by six tests doesn't blow the printed table
past the page height on its own. The full name list for any requirement is always the
plain output of this script, not just the doc's rendering of it.
"""

import argparse
import io
import os
import re
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
REPO = os.path.dirname(os.path.dirname(os.path.dirname(HERE)))
TESTS_DIR = os.path.join(REPO, "GHCAA.Tests")
REQUIREMENTS_DOC = os.path.join(REPO, "docs", "book", "03-requirements.md")

# (requirement, DC/clause, use case, design element, artefact). Table 3.4 is a
# representative extract, not the full matrix (that is Appendix B, per the doc's own
# note), so this list matches the extract rather than every tagged requirement.
ROWS = [
    ("FR-01", "—", "UC-01", "Registration sequence, §5.5",
     "Registration endpoint and application service"),
    ("FR-02", "DC-08", "UC-01, UC-02", "Membership state machine, §5.3",
     "Membership status transitions"),
    ("FR-03", "—", "UC-04 (profile)", "Masking projection, §8.7",
     "Field-visibility model and directory projection"),
    ("FR-19", "DC-14", "UC-02", "Approval flow, §5.5",
     "Obligation raising on approval"),
    ("FR-21", "DC-14", "UC-04", "Ledger model, §5.4",
     "Payment and verification records"),
    ("FR-25", "DC-14", "UC-04", "Append-only rule, §5.6",
     "Receipt generation; amendment refusal"),
    ("FR-32", "DC-16", "UC-07", "Version resolution, §5.6",
     "Always-current reader"),
    ("FR-33", "DC-16", "UC-07", "Supersede-not-delete rule, §5.6",
     "Version synchronisation at start-up"),
    ("FR-36", "DC-03", "UC-09", "Eligibility rule, §5.6",
     "Voting eligibility guard"),
    ("FR-37", "DC-13", "UC-08, UC-09", "Threshold reporting, §5.6",
     "Count and threshold report"),
    ("FR-44", "DC-14", "UC-02, UC-04", "Archival deletion, §6.9",
     "Global filter on archived records"),
    ("NFR-S5", "DC-07", "UC-02", "Session invalidation, §8.5",
     "Security-stamp check per request"),
]

CATEGORY_ATTR = re.compile(r'^\s*\[Category\("((?:FR|DC|NFR)-[A-Za-z0-9]+)"\)\]\s*$')
TEST_ATTR = re.compile(r'^\s*\[(?:Test|TestCase\(.*\))\]\s*$')
METHOD_SIG = re.compile(
    r'^\s*(?:public|private|protected|internal)\s+(?:static\s+)?(?:async\s+)?'
    r'(?:Task(?:<[^>]*>)?|void|[A-Za-z0-9_<>\[\], ]+)\s+([A-Za-z_][A-Za-z0-9_]*)\s*\(')


def scan_tests():
    """Map each FR/DC/NFR tag to the test methods that carry it.

    A method can carry more than one Category attribute (an FR tag and a DC tag
    stacked together, as in FinancialServiceTests.cs), so the same test name can
    appear under several keys.
    """
    tagged = {}
    for root, _dirs, files in os.walk(TESTS_DIR):
        for name in files:
            if not name.endswith(".cs"):
                continue
            path = os.path.join(root, name)
            with io.open(path, encoding="utf-8") as fh:
                lines = fh.readlines()
            pending, seen_test = [], False
            for line in lines:
                cat = CATEGORY_ATTR.match(line)
                if cat:
                    pending.append(cat.group(1))
                    continue
                if TEST_ATTR.match(line):
                    seen_test = True
                    continue
                method = METHOD_SIG.match(line) if seen_test else None
                if method:
                    for tag in pending:
                        tagged.setdefault(tag, []).append(method.group(1))
                    pending, seen_test = [], False
                    continue
                if line.strip():
                    pending, seen_test = [], False
    return tagged


MAX_TESTS_SHOWN = 1


def test_column(req, tagged):
    tests = sorted(set(tagged.get(req, [])))
    if not tests:
        return "no tagged test (gap)"
    if len(tests) <= MAX_TESTS_SHOWN:
        return ", ".join(tests)
    shown = ", ".join(tests[:MAX_TESTS_SHOWN])
    return "%s (+%d more)" % (shown, len(tests) - MAX_TESTS_SHOWN)


def generate():
    tagged = scan_tests()
    return [(req, dc, uc, design, artefact, test_column(req, tagged))
            for req, dc, uc, design, artefact in ROWS]


def print_table(rows, markdown):
    headers = ("Req", "DC / clause", "Use case", "Design element (Ch. 5–6)",
               "Implementation artefact (Ch. 7)", "Test (Ch. 9)")
    if markdown:
        print("| " + " | ".join(headers) + " |")
        print("| " + " | ".join("---" for _ in headers) + " |")
        for row in rows:
            print("| " + " | ".join(row) + " |")
        return
    widths = [max(len(h), max((len(r[i]) for r in rows), default=0))
              for i, h in enumerate(headers)]
    fmt = "  ".join("%-" + str(w) + "s" for w in widths)
    print(fmt % headers)
    for row in rows:
        print(fmt % row)


TABLE_HEADING = "### Table 3.4"
TABLE_ROW = re.compile(r'^\|(.+)\|\s*$')


def doc_table_rows():
    """Read Table 3.4 as it currently stands in 03-requirements.md.

    Returns a list of tuples in the same shape as generate(), parsed from the
    Markdown table under the "### Table 3.4" heading, or None if the heading or
    table is missing.
    """
    text = io.open(REQUIREMENTS_DOC, encoding="utf-8").read()
    heading_pos = text.find(TABLE_HEADING)
    if heading_pos < 0:
        return None
    section = text[heading_pos:]
    lines = section.splitlines()
    rows = []
    in_table = False
    for line in lines:
        match = TABLE_ROW.match(line)
        if not match:
            if in_table:
                break
            continue
        cells = [c.strip() for c in match.group(1).split("|")]
        if cells[0] == "Req":
            in_table = True
            continue
        if set(c.strip("- ") for c in cells) == {""}:
            continue
        if in_table:
            rows.append(tuple(cells))
    return rows or None


def check():
    """Compare the generated table against the one printed in the document.

    Like wbs.py --check, this reports every disagreement rather than stopping at the
    first one, so a single run shows the whole gap.
    """
    generated = generate()
    current = doc_table_rows()
    problems = []
    if current is None:
        return ["could not find Table 3.4 in %s" % REQUIREMENTS_DOC]
    current_by_req = {row[0]: row for row in current}
    for row in generated:
        req = row[0]
        doc_row = current_by_req.get(req)
        if doc_row is None:
            problems.append("%s is in the generator's rows but not in the printed table" % req)
            continue
        if doc_row != row:
            for label, generated_cell, doc_cell in zip(
                    ("DC/clause", "use case", "design element", "artefact", "test"),
                    row[1:], doc_row[1:]):
                if generated_cell != doc_cell:
                    problems.append(
                        '%s %s: doc says "%s", tags say "%s"'
                        % (req, label, doc_cell, generated_cell))
    generated_reqs = {row[0] for row in generated}
    for doc_row in current:
        if doc_row[0] not in generated_reqs:
            problems.append("%s is in the printed table but not in the generator's rows"
                             % doc_row[0])
    return problems


def main(argv=None):
    # A Windows console defaults to cp1252 and cannot encode the section signs
    # this output quotes from the tracker. Same fix as build.py and wbs.py.
    for handle in (sys.stdout, sys.stderr):
        try:
            handle.reconfigure(encoding="utf-8", errors="replace")
        except (AttributeError, ValueError):
            pass
    ap = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    ap.add_argument("--markdown", action="store_true", help="emit a Markdown table")
    ap.add_argument("--check", action="store_true",
                    help="exit non-zero if the printed Table 3.4 disagrees with what "
                         "the [Category] tags in GHCAA.Tests actually support")
    args = ap.parse_args(argv)

    if args.check:
        problems = check()
        for line in problems:
            sys.stderr.write("  %s\n" % line)
        return 1 if problems else 0

    print_table(generate(), args.markdown)
    return 0


if __name__ == "__main__":
    sys.exit(main())
