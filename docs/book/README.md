# Documentation book — sources and build

The dissertation text lives here as plain Markdown, one file per chapter. Presentation is not baked
into the sources: IEEE conventions (caption above tables, caption below figures, chapter page breaks,
figure and table numbering labels) are applied at build time by `build/build.py`, so the Markdown
stays readable and diffable.

The rule this directory is maintained under is that **the book is deliverable at every commit**. A
build either reports `status : clean, ready to deliver` or names what is wrong. Nothing is left in a
state that only the author knows how to finish.

## Contents

| File | Part |
|---|---|
| `00-front-matter.md` | Title page, abstract, contents, list of figures, list of tables, abbreviations |
| `01-introduction.md` | Part I, Chapter 1 — Introduction |
| `02-literature-review.md` | Part I, Chapter 2 — Literature and systems review |
| `03-requirements.md` | Part I, Chapter 3 — Requirements engineering |
| `04-methodology.md` | Part II, Chapter 4 — Research methodology |
| `05-system-analysis.md` | Part II, Chapter 5 — System analysis and behavioural modelling |
| `06-architecture.md` | Part II, Chapter 6 — System architecture and design |
| `99-references.md` | IEEE numbered bibliography and the Association's governing documents |

Chapters 7-13 (Part III construction/validation and Part IV evaluation/closure) are not yet written;
see `docs/DOCUMENTATION_BOOK_OUTLINE.md` for their planned structure. Nothing in the written chapters
points at a figure, table or section number inside an unwritten chapter: forward pointers are to the
chapter, never to a numbered artefact that does not exist yet.

The bound order is fixed by the `CHAPTERS` list at the top of `build/build.py`. Adding a chapter means
adding its filename there; the builder does not glob the directory, so a stray draft cannot wander
into the book by accident.

## The three tools

| Command | What it does |
|---|---|
| `python docs/book/build/build.py` | builds the HTML and runs every source check |
| `python docs/book/build/build.py --pdf` | the same, then measures A4 fit, prints the PDF with page numbers, and fills the Page columns from it |
| `python docs/book/build/renumber.py --apply` | renumbers figures and tables into bound order and rebuilds the contents and the front-matter lists |
| `python docs/book/build/prose.py refs` | checks every section reference: none broken, and each names what is at the destination. Prints the destination title beside every reference, which is how a reference that resolves to the wrong section is caught |
| `python docs/book/build/prose.py prose 03` | lists the sentences in a chapter likely to need a second reading. A filter for a human pass, not a gate |
| `python docs/book/build/tracker_page.py` | writes `tracker.html`, a filterable view of every open item in `docs/TODO.md`. Generated, git-ignored, republish after a tracker change |
| `python docs/book/build/wbs.py` | regenerates Chapter 11 evidence from git and `docs/TODO.md`: component durations, the critical path, task counts, and how the work arrived. `--check` fails if a component has no commits or no tracker areas; `--sync` writes a `<!-- wbs: ... -->` marker into any tracker area that has none, so a newly added area maps itself |

Python 3 only — no pandoc, no Node packages, no `node_modules`. The PDF step additionally needs
Chrome or Edge, which it finds by itself (override with the `BOOK_BROWSER` environment variable).

One optional package: `pypdf` (or `PyMuPDF`) lets the build read page numbers back out of the printed
PDF and fill the Page columns of the contents and the two lists. Without it everything else still
works and the report says the columns were left empty — it never guesses a page number.

### What the PDF carries

- a folio on every page, "N of M", printed over the DevTools protocol because Chrome's
  `--print-to-pdf` switch cannot add one and quietly drops background graphics
- background graphics, so table rules and shaded cells print as designed
- one A4 landscape page for the figure that needs it, the rest portrait
- page numbers in the Table of Contents, List of Figures and List of Tables, read back from the
  printed copy and re-verified after the reprint

Two deviations from the outline's front-matter convention, both forced by the print engine and both
recorded rather than hidden: the front matter is numbered in Arabic with the body rather than in
lower-case Roman, and the title page carries a folio. Chrome applies one footer template to every
page, so neither can vary. There is no running head for the same reason — a constant one would print
the title across the title page. TODO 63.18 records what closing this would take.

## Building

```
python docs/book/build/build.py                    # single-column manuscript (default)
python docs/book/build/build.py --two-column       # IEEE Transactions two-column layout
python docs/book/build/build.py -o path/out.html   # choose the output path
python docs/book/build/build.py --audit            # measure A4 fit, do not print
python docs/book/build/build.py --pdf              # HTML, A4 audit, then the PDF
python docs/book/build/build.py --pdf --strict     # the same, non-zero exit on any defect
python docs/book/build/build.py --pdf --strict --no-placeholders   # the submission gate
```

Output defaults to `docs/book/GHCAA-Documentation-Book.html` (self-contained, stylesheet embedded)
and `docs/book/GHCAA-Documentation-Book.pdf`.

A clean run reports:

```
  layout    : single-column
  figures   : 56 captioned, 56 diagrams
  tables    : 19 captioned
  captions with no artefact beneath them: Table 3.1, Table 3.2, Table 3.3, Table 3.5, Table 3.6
  placeholders still open: 5
  measured  : 101 figures and tables laid out
  pdf       : ...GHCAA-Documentation-Book.pdf (2.4 MB, 83 pages)
  status    : clean, ready to deliver
```

The orphan-caption line is a sanity check, not an error. Tables 3.1, 3.2, 3.3, 3.5 and 3.6 are
deliberate pointer entries: the List of Tables names them, but the material is set as prose or lives
in the section the entry points to. Any *other* name on that line is a caption that has drifted away
from its artefact, and the build reports it as a defect. The allowed set lives in
`build/lint.py:ALLOWED_ORPHANS`.

`--strict` is the gate to run after every edit: it fails on any lint finding, any drifted caption
and any figure that will not print on A4. Open placeholders and not-yet-cited references are always
listed but do not fail it, because a chapter in progress legitimately carries both. Add
`--no-placeholders` for the copy being handed in, which must carry neither.

## Making the PDF

`--pdf` does what the manual route did, without the manual steps. It serves the built HTML from
`http://127.0.0.1` on a spare port, drives headless Chrome or Edge over it, and prints A4 with the
browser's own headers and footers off. The local server exists because Chrome refuses ES-module
imports from `file://` and Mermaid is an ES module.

Before printing, the same browser measures the page and refuses to print if the diagrams did not all
draw, naming the ones that failed. That check exists because a Mermaid syntax error used to leave a
figure printed as a box of source, which is easy to miss in an eighty-page document.

To print by hand instead — open the HTML in Chrome or Edge and use Save as PDF, paper size A4,
margins Default, scale 100%, headers and footers off. Wait for the diagrams to draw first: the body
carries `data-diagrams="rendered"` when they are done, `partial` if some failed, `source` if Mermaid
could not load at all. Firefox and Safari honour `break-inside` and `column-span` less faithfully, so
figures and wide tables may split across pages; use Chrome or Edge for a copy that will be submitted.

## Fitting A4

Every diagram is scaled at view time to fit inside one page, which is why no figure splits across a
page break. The fit is computed in JavaScript rather than left to CSS because the landscape page is
wider than anything the screen preview can show, so its scale has to be calculated:

- portrait figures fit 174 x 224 mm (the A4 text block, less room for the caption)
- landscape figures fit 257 x 148 mm
- a diagram smaller than its box is enlarged, but never by more than half again

`--audit` then reports any figure or table that still does not work on paper, in five kinds:

- **too wide** — it runs past the text block, so the edge would be cut
- **too tall** — it cannot fit a page and is not marked breakable
- **labels too small** — it fits, but only by shrinking its labels below 7pt
- **labels overprinted** — two labels sit on top of each other, so neither reads
- **labels clipped** — a title or label is wider than the drawing and is cut off at the frame

The last two are about the words rather than the box, and neither is visible in the page count or the
figure size. Mermaid overprints whenever two chart points share a coordinate or two edges join the
same pair of nodes: give the points distinct positions, or replace the pair of arrows with one
double-headed arrow whose label says which flow goes which way. It clips whenever a `title` or a
point label is wider than the chart, because the viewBox is sized from the drawing and everything
outside it is simply cut: shorten the title, or drop it and let the figure caption carry the name.

The last one is the common one, and marking it `{landscape}` is usually the wrong fix. A diagram is
too small because its shape does not match the page: a fan-out tree drawn `TB` grows sideways, a
process chain drawn `LR` grows sideways, and either way the page has to shrink it to fit the width
while most of the page height goes unused. The fixes, in the order to try them:

1. **Turn it.** A fan-out tree wants `flowchart LR`; a step chain or pipeline wants `flowchart TB`.
   The audit prints the diagram's wide-to-tall ratio against the ~0.78 the portrait page wants and
   the ~1.74 the landscape page wants.
2. **Shorten the labels.** Diagram width is set by the longest label, not by the node count. Break
   labels with `<br/>` and cut words that carry nothing.
3. **Stack what sits side by side.** `direction TB` inside a subgraph, or invisible links (`~~~`)
   between nodes, turn a row into a column. Mermaid ignores `direction` on a subgraph that has edges
   crossing its boundary, so check the result rather than assuming.
4. **Split it.** Four entity-relationship sub-models print better than one diagram of forty-nine
   tables. Splitting is not a compromise; a diagram nobody can read conveys nothing.
5. **Set it as a table.** Some artefacts are tabular in the first place — the CRC card set is now
   Table 5.2 for exactly this reason.
6. **Only then, `{landscape}`.** It buys 257mm of width and costs a page of its own. It is the right
   answer for a genuinely wide diagram, such as the high-level architecture of Figure 6.1.

Close the PDF before building. A viewer holding `GHCAA-Documentation-Book.pdf` open locks the file;
the build now stops and says so, because the older behaviour was worse — it fell back to the
command-line switch and produced a copy with no page numbers and no background graphics.

After a figure is added, dropped, moved or turned into a table, run
`python docs/book/build/renumber.py --apply`. It relabels captions in bound order, rewrites every
mention in the body, and rebuilds the List of Figures and List of Tables from the captions, so the
front matter cannot drift from the book. It refuses to run while two captions share a label, because
a mention of that label would be ambiguous.

## What the build checks

`build/lint.py` runs on every build. All of it reports a file and a line.

| Check | What fails it |
|---|---|
| outline drift | a section in a chapter and not in the outline, or the reverse, or the two in a different order, or a chapter retitled in one alone |
| tone | a word or opener the house style rules out (the list is `BANNED` in `lint.py`) |
| numbering | a figure or table number used twice, or a chapter's numbering with a gap in it |
| forward references | an artefact printed before the body has named it, which IEEE does not allow |
| front-matter lists | a figure or table missing from the List of Figures or List of Tables, or a row in either list with no caption behind it |
| abstract length | the stated word count no longer matching the abstract |
| citations | a `[n]` marker with no entry in `99-references.md` |
| uncited references | an entry nothing cites yet; expected while Part III and IV are unwritten, a defect for a finished copy |
| placeholders | every `*[` paragraph still open, counted per file; listed line by line under `--no-placeholders` |
| drifted captions | a caption with no artefact under it that is not in the allowed set |

The reference list is exempt from the tone check: it is titles and journal names, not the author's
prose. Headings, tables and code fences are exempt too.

### The outline and the chapters are one structure

`docs/DOCUMENTATION_BOOK_OUTLINE.md` is the approved structure and `docs/book/*.md` is what gets
bound. They are two views of the same thing, so **a change to either is a change to both**: add a
section to a chapter and add its bullet to the outline; renumber, retitle, reorder or drop a section
in the outline and do the same in the chapter. This is not a convention to remember — `lint.py`
compares them on every build and `--strict` fails on any disagreement, in either direction,
including a difference in order alone.

Chapters 7 to 13 exist as stubs generated from the outline: the headings are real and checked, and
each section carries a `*[Not written]*` placeholder with the outline's brief, so `--no-placeholders`
counts exactly what is left to write. Fill a section by replacing its placeholder; do not delete the
heading.

## Diagrams

Diagrams are Mermaid, written as fenced ` ```mermaid ` blocks. The builder emits them as
`<pre class="mermaid">` and loads Mermaid 11 as an ES module from jsDelivr at view time, so the
repository carries no rendering toolchain and needs a network connection the first time a build is
opened.

Each diagram is rendered on its own rather than in one batch call, so a syntax error in one costs one
figure and is reported by caption, instead of silently dropping every diagram in the book back to
source. Diagram type settings (font size, spacing, ER layout direction) are set once in `build.py`
so every figure comes out at one scale.

Two Mermaid traps worth knowing, both of which have already cost a figure here:

- a semicolon ends a statement, so a note or label containing one is cut in half
- an unquoted `/` or `:` in a label is read as syntax; quote any label with punctuation in it

## Markdown conventions the builder understands

- `# Heading` starts a chapter and forces a page break; the first one does not.
- A heading of the form `### Figure 3.1 — Caption text` or `### Table 3.1 — Caption text` becomes a
  caption. The builder attaches it to the Mermaid block or table that follows, placing it below a
  figure and above a table, and will absorb one intervening lead-in paragraph as a note.
- A figure caption ending `{landscape}` (e.g. `### Figure 6.1 — High-level architecture {landscape}`)
  is set on its own A4 landscape page. See "Fitting A4" above for when that is the right call.
- A paragraph beginning `*[` is treated as an author placeholder and rendered highlighted, so that
  nothing provisional can reach a printed copy unnoticed. `*[` anywhere else in a line, including
  inside a table cell, is reported by the build even though it is not highlighted.
- `{{build-month-year}}` and `{{build-date}}` are filled in at build time, so the title page carries
  the date the copy was printed rather than one nobody remembered to update.
- Tables longer than fourteen rows are allowed to break across pages; shorter ones are kept whole.
- Blockquotes are used for user stories and acceptance criteria.

## House style — binding on every remaining chapter

Chapters 4 onward must match Part I on all three counts below. This is not preference; it is what
keeps a 300-page document reading as one document.

**IEEE conventions.** Numeric citation markers in square brackets, resolved against the single
book-wide list in `99-references.md` — numbers are assigned in order of first appearance across the
whole book and are never renumbered to suit one chapter, so append new entries at the end. Standards
cited by designation and year of the edition consulted. Figures numbered per chapter with the caption
below; tables numbered per chapter with the caption above; every figure and table named in the body
before it appears, and listed in the front matter. Both of those last two are enforced by the build,
not left to attention. Cross-references by section number (§9.4), never by page or by "the section
above". The section sign belongs in a cross-reference and nowhere else: the contents list
carries the bare number, which is the convention a reader expects there.

**Tone.** Plain declarative English, British spelling, first person singular where the author is the
one who did the thing. Vary sentence length. State what was done and what happened; do not editorialise
about how significant it was. Prefer a concrete figure from the repository over a general claim — 276
endpoints, not "a comprehensive API". Name the limits explicitly: where something was not measured,
not tested, or not achieved, say so in the sentence rather than in a hedge. No bullet lists standing in
for argument, no three-adjective build-ups, and none of the vocabulary in `lint.py:BANNED`. Nothing in
this book should read as though it were generated: no filler openers, no restating the obvious, no
sentence whose only work is to announce that the next sentence is important.

**Honesty rule.** Nothing enters the text that is not in the repository, the governing documents, or a
cited source. Anything else is a bracketed placeholder beginning `*[`, which the builder renders
highlighted and lists after every build. Where a number is reported, name the artefact it came from —
a log file, a test run, a commit range — so an examiner can check it. Figures included: a diagram
states what the code does, and where the two disagree the code is right.

**Keeping the numbers true.** Repository figures quoted in the text go stale as the code moves. The
ones in the book now were re-taken on 14 September 2026: 285 endpoint attributes across 38 controllers,
54 `DbSet` properties (53 domain models plus `DataProtectionKeys`, a framework table added per
`docs/adr/0007-data-protection-keys-in-database.md` — §6.3.2's "fifty-three entity sets" still means
the domain models alone, since that count is about §5.7's analysis model, not every table the schema
has), 30 enumerations, 46 service interfaces with 43
implementations, 1 migration (`GHCAA.Infrastructure/Data/Migrations/PgSql/20260907193705_InitialBaseline.cs`
— the prior 31-file history was squashed to this single baseline on 8 September 2026), 745 backend
tests enumerated by `dotnet test --list-tests` (not re-run as a pass/fail suite for this count), 430
passing web tests (not re-verified on this date), 3,468 lines of `styles.scss`. Re-take them before
submission with the commands in `docs/PROJECT_MAP.md` and the two test suites, and correct the
sentences that carry them.

## Before submission

Every author placeholder is closed. The build reports none, and
`python docs/book/build/build.py --pdf --strict --no-placeholders` passes.

Two gates, because they are reached at different times:

| Gate | Adds |
|---|---|
| `--strict` | fails on any defect: lint finding, drifted caption, A4 problem. Run after every edit |
| `--strict --no-placeholders` | also fails while any `*[` placeholder is open. Passes now |
| `--strict --final` | also fails while any reference in the list is uncited. Not reachable until Part III and IV are written |

Seventeen references are numbered but not yet cited. That is by design: numbering is book-wide and
assigned in order of first appearance, so entries first cited in Part III already sit in the list.

Two standards in use have been revised since the work was done: ISO/IEC 25010:2011 by the 2023
edition, which renames usability and portability and adds safety, and OWASP ASVS 4.0.3 by 5.0.0. Both
are cited by the edition the work was carried out against, with the successor named and the
consequence stated in §3.4 and §4.5. Do not restate a conformance claim under a newer edition, and do
not reclassify the NFR taxonomy to the 2023 model — the identifiers run through the whole book.

Participants and officers are identified by office, never by name. They are identifiable members of a
small association who agreed verbally; §4.9 gives the reasoning.

`docs/materials/` holds the Pressman slide sets and a precedence-diagram exercise used while writing.
It is git-ignored, being third-party copyrighted teaching material. Cite Pressman and Maxim's 8th
edition, reference [54]; nothing is quoted from the slides.

Then run `python docs/book/build/build.py --pdf --strict --no-placeholders` and hand over the PDF it
writes.
