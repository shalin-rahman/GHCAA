# Documentation book — sources and build

The dissertation text lives here as plain Markdown, one file per chapter. Presentation is not baked
into the sources: IEEE conventions (caption above tables, caption below figures, chapter page breaks,
figure and table numbering labels) are applied at build time by `build/build.py`, so the Markdown
stays readable and diffable.

## Contents

| File | Part |
|---|---|
| `00-front-matter.md` | Title page, abstract, contents, list of figures, list of tables, abbreviations |
| `01-introduction.md` | Part I, Chapter 1 — Introduction |
| `02-literature-review.md` | Part I, Chapter 2 — Literature and systems review |
| `03-requirements.md` | Part I, Chapter 3 — Requirements engineering |
| `99-references.md` | IEEE numbered bibliography and the Association's governing documents |

The bound order is fixed by the `CHAPTERS` list at the top of `build/build.py`. Adding a chapter means
adding its filename there; the builder does not glob the directory, so a stray draft cannot wander
into the book by accident.

## Building

Requires Python 3 only — no pandoc, no Node packages, no `node_modules`.

```
python docs/book/build/build.py                    # single-column manuscript (default)
python docs/book/build/build.py --two-column       # IEEE Transactions two-column layout
python docs/book/build/build.py -o path/out.html   # choose the output path
```

Output defaults to `docs/book/GHCAA-Documentation-Book.html`, a single self-contained file with the
stylesheet embedded.

The builder prints a short report after each run:

```
  layout    : single-column
  figures   : 18 captioned, 18 diagrams
  tables    : 5 captioned
  captions with no artefact beneath them ...: Table 3.1, Table 3.2, Table 3.3, Table 3.5, Table 3.6
```

That last line is a sanity check, not an error. A caption is expected to sit directly above a table or
directly below a diagram; if it does not, the builder says so. Tables 3.1, 3.2, 3.3, 3.5 and 3.6 are
deliberate pointer entries — the List of Tables names them, but the material itself is set as prose or
lives in the section the entry points to. Any *new* name appearing on that line means a caption has
drifted away from its artefact and should be fixed in the Markdown.

## Printing to PDF

Open the generated HTML in Chrome or Edge and print. The settings that matter:

- Destination: **Save as PDF**
- Paper size: **A4**
- Margins: **Default** (the stylesheet sets its own via `@page`)
- Scale: **100%** — do not use "Fit to page width"
- **Background graphics: on** (table shading and the placeholder highlights depend on it)
- **Headers and footers: off** — the browser's own header would collide with the page margins

Wait for the diagrams to finish drawing before printing. When they are done the document root carries
`data-diagrams="rendered"`; if the renderer could not run, it carries `data-diagrams="source"` and
every diagram is replaced by its source in a dashed box, which is still legible but is not what should
be submitted.

Firefox and Safari will produce a readable document but honour `break-inside` and `column-span` less
faithfully, so figures and wide tables may split across pages. Use Chrome or Edge for the final copy.

## Diagrams

Diagrams are Mermaid, written as fenced ` ```mermaid ` blocks in the Markdown. The builder emits them
as `<pre class="mermaid">` and loads Mermaid 11 as an ES module from jsDelivr at view time. This keeps
the repository free of a rendering toolchain, at the cost of needing a network connection the first
time a build is opened. If the module fails to load, the page falls back to showing the diagram source
rather than a blank space.

## Markdown conventions the builder understands

- `# Heading` starts a chapter and forces a page break; the first one does not.
- A heading of the form `### Figure 3.1 — Caption text` or `### Table 3.1 — Caption text` becomes a
  caption. The builder attaches it to the Mermaid block or table that follows, placing it below a
  figure and above a table, and will absorb one intervening lead-in paragraph as a note.
- A figure caption ending `{landscape}` (e.g. `### Figure 6.4 — Design class diagram {landscape}`)
  is set on its own A4 landscape page instead of being shrunk to the 174mm portrait width. Every
  diagram scales to fit its column by default (`max-width: 100%` on the rendered SVG), which is
  enough for most figures; reserve `{landscape}` for the few that stay illegible even after that —
  the ERD, the design class diagram, the dependency structure matrix — consistent with Appendix H's
  "full-page fold-out" treatment for those three. The marker is stripped from the printed caption.
- A paragraph beginning `*[` is treated as an author placeholder and rendered highlighted, so that
  nothing provisional can reach a printed copy unnoticed.
- Tables longer than fourteen rows are allowed to break across pages; shorter ones are kept whole.
- Blockquotes are used for user stories and acceptance criteria.

## House style — binding on every remaining chapter

Chapters 4 onward must match Part I on all three counts below. This is not preference; it is what
keeps a 300-page document reading as one document.

**IEEE conventions.** Numeric citation markers in square brackets, resolved against the single
book-wide list in `99-references.md` — numbers are assigned in order of first appearance across the
whole book and are never renumbered to suit one chapter, so append new entries at the end. Standards
cited by designation and year of the edition consulted. Figures numbered per chapter with the caption
below; tables numbered per chapter with the caption above; every figure and table referred to by
number in the body before it appears, and listed in the front matter. Cross-references by section
number (§9.4), never by page or by "the section above".

**Tone.** Plain declarative English, British spelling, first person singular where the author is the
one who did the thing. Vary sentence length. State what was done and what happened; do not editorialise
about how significant it was. Prefer a concrete figure from the repository over a general claim — 260
endpoints, not "a comprehensive API". Name the limits explicitly: where something was not measured,
not tested, or not achieved, say so in the sentence rather than in a hedge. No bullet lists standing in
for argument, no three-adjective build-ups, no "leverage", "robust", "seamless", "comprehensive",
"delve", "landscape" or "it is important to note".

**Honesty rule.** Nothing enters the text that is not in the repository, the governing documents, or a
cited source. Anything else is a bracketed placeholder beginning `*[`, which the builder renders
highlighted so it cannot reach a printed copy unnoticed. Where a number is reported, name the artefact
it came from — a log file, a test run, a commit range — so an examiner can check it.

**Printing.** Every chapter must build clean through `build/build.py` and be checked in the browser
before it is considered done. New figures go in as Mermaid; new tables use the caption-heading form so
IEEE placement is applied automatically. Read the builder's orphan-caption line after each build: any
name on it other than Tables 3.1, 3.2, 3.3, 3.5 and 3.6 is a real defect.

## Before submission

Search the built document for the highlighted placeholders and resolve each one. Three remain in
Part I, and none can be answered from the repository:

- the supervisor's name and designation, and the month and year of submission, on the title page
- participant counts, sampling and session dates for the elicitation study (§3.1.2)
- the ethical approval reference and consent procedure (§3.1.3)

Two Part I placeholders have been closed: the author identity on the title page, and the review defect
counts in §3.12, which are now taken from the findings log at `docs/BUSINESS_FINDINGS.md` and set out
in Table 3.8.

References marked with a dagger in `99-references.md` need their edition, year, page range or DOI
checked against the copy actually consulted, and the commercial pricing bands in §2.9 carry an inline
note asking for the same.
