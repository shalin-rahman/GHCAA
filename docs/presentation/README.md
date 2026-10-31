# Progress presentation

Material for the 12-slide progress review on the election module and the data model.

## Files

| File | What it is |
| --- | --- |
| [GHCAA-Mid.pptx](GHCAA-Mid.pptx) | The 12-slide deck. Every diagram is made of native PowerPoint shapes and can be edited |
| [GHCAA-FR-NFR-List.pdf](GHCAA-FR-NFR-List.pdf) | Requirements catalogue for printing: 57 functional, 37 non-functional, 11 quality-attribute scenarios |
| [GHCAA-SRS.pdf](GHCAA-SRS.pdf) | Software Requirements Specification for printing |
| [deck/build_deck.py](deck/build_deck.py) | Builds GHCAA-Mid.pptx |
| [deck/GHCAA-Mid.original.pptx](deck/GHCAA-Mid.original.pptx) | The first 5-slide draft, kept for reference |
| GHCAA-Mid_v1.pptx | The earlier version of the deck, kept for reference |

The deck and both PDFs are generated. Do not edit them by hand, because the next build will
overwrite the edits. Rebuild from the repo root:

    python docs/presentation/deck/build_deck.py
    python docs/book/build/export_presentation.py

The first command needs python-pptx. The second reads the requirements chapter of the book and
docs/SRS.md, removes references that only make sense inside the book, and stops if any are left.

## Numbers on the slides

Every count was taken from the repository on 2026-10-31:

| What | Count |
| --- | --- |
| API controllers | 49 |
| HTTP endpoints | 406 |
| Entity types | 83, of which 24 are for elections and polls |
| PostgreSQL migrations | 34 |
| Commits | 378 |
| Backend tests, `[Test]` and `[TestCase]` | 1,530 and 292 |
| Angular specs | 905 |
| Flutter tests | 372 |
| Election test attributes in the backend | 517, in 37 files |
| Tracker | 86 TODO, 28 PARTIAL |

These figures are written into build_deck.py. If code has landed since that date, recount them
and change the script before the talk. The test figures count declarations, not passing tests.
Run the suites and quote the pass result separately.

## Demo setup

From the repo root, on the local machine only, never preprod:

    scripts\run-election-walkthrough.bat setup        (before the talk: election up to Campaign)
    scripts\run-election-walkthrough.bat resume slow  (in the talk: polling to results, visible, 400 ms per step)
    scripts\run-election-walkthrough.bat full demo    (one go, polling dates moved to now)
    scripts\clean-election-test-data.bat              (afterwards: remove test data, restore accounts)

The script starts the API and web server if they are not running, and opens its report at the end.
Rehearse the full run the day before. Step-up codes are sent by real email, so the machine needs
network access.
