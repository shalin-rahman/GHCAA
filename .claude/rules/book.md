---
paths:
  - "docs/book/**"
  - "docs/DOCUMENTATION_BOOK_OUTLINE.md"
  - "docs/TODO.md"
---

# Documentation book (docs/book/)

The dissertation in `docs/book/` is held to a stricter standard than the rest of the docs, and the
standard is enforced by its build rather than by attention.

- Before editing a chapter, read `docs/book/README.md`. It carries the house style, the IEEE
  conventions, the A4 figure rules and the fix order for a diagram that will not print.
- After editing, run `python docs/book/build/build.py --pdf --strict`. It must end with
  `status : clean, ready to deliver`. Add `--no-placeholders` only for a copy being handed in. It
  fails on a numbering gap, a figure the body never names, a drifted front-matter list, banned
  vocabulary, a diagram that cannot print legibly on A4, and an open placeholder. Do not silence a
  check to get a green run; fix what it names.
- After adding, removing or moving a figure or table, run `python docs/book/build/renumber.py --apply`.
- `docs/DOCUMENTATION_BOOK_OUTLINE.md` and the chapter files in `docs/book/` are one structure seen
  twice. A section added, renamed, renumbered, reordered or dropped in either must be changed in the
  other in the same edit, and a chapter retitled in one must be retitled in the other. The build
  compares them and `--strict` fails on any disagreement, so do not put this off to a follow-up.
- Task numbers in `docs/TODO.md` move: implementation work and new instructions add areas and
  items, and areas get renumbered. Anything that quotes them has to be refreshed in the same
  change: the component-to-area map in `docs/book/build/wbs.py` (`wbs.py --check` fails on an
  unmapped area), and the tracker figures quoted in the outline's Chapter 11 block.
- Every diagram must print inside one A4 page with labels at 7pt or larger. Reshape the diagram
  (turn it, shorten labels, split it, or set it as a table) before reaching for `{landscape}`.
- Repository numbers quoted in the text must come from a command run against the tree, with the date
  recorded. Anything that cannot be sourced that way is a `*[` placeholder, never a plausible guess.
