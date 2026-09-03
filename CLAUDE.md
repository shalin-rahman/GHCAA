# Graphify

This project has a knowledge graph at graphify-out/ with god nodes, community structure, and cross-file relationships.

Rules:

- For codebase questions, first run `graphify query "<question>"` when graphify-out/graph.json exists. Use `graphify path "<A>" "<B>"` for relationships and `graphify explain "<concept>"` for focused concepts. These return a scoped subgraph, usually much smaller than GRAPH_REPORT.md or raw grep output.
- If graphify-out/wiki/index.md exists, use it for broad navigation instead of raw source browsing.
- Read graphify-out/GRAPH_REPORT.md only for broad architecture review or when query/path/explain do not surface enough context.
- After modifying code, run `graphify update .` to keep the graph current (AST-only, no API cost).

# Comment, Doc & TODO Tone

All code comments, doc-comments, markdown docs, and TODO entries in this repo must read like a person
wrote them, not a model. This applies to every new or edited comment/doc/TODO, in every language
(C#, TypeScript, Dart, SQL, markdown).

- Plain, short sentences. Say what the code does or why, nothing more.
- No filler openers ("This function is responsible for...", "Note that...", "It's important to...",
  "This method serves to...").
- No restating the obvious (`// increment i`, `// constructor`). A comment earns its place by
  explaining something the code itself doesn't already say — a gotcha, a non-obvious reason, a
  workaround, a caller assumption.
- No em dashes, no bullet-lists-inside-comments, no heading-style comment banners
  (`// ===== SECTION =====`) unless the surrounding file already uses that convention.
- TODOs name the actual gap and, where known, why it's not done yet — not a vague "TODO: improve this".
- Don't add a comment just to prove a change was made ("// updated per review", "// fixed bug here").
- This applies retroactively too: when a file is touched for any reason, clean up any AI-sounding
  comments/docs/TODOs you pass over in it, not just the lines you're actively changing.
  Git history already carries that.
- When editing a file, match whatever comment style already exists there before applying this rule to
  new lines — don't rewrite untouched comments just to align tone.

# Documentation book (docs/book/)

The dissertation in `docs/book/` is held to a stricter standard than the rest of the docs, and the
standard is enforced by its build rather than by attention.

- Before editing a chapter, read `docs/book/README.md`. It carries the house style, the IEEE
  conventions, the A4 figure rules and the fix order for a diagram that will not print.
- After editing, run `python docs/book/build/build.py --pdf --strict`. It must end with
  `status : clean, ready to deliver`. Add `--no-placeholders` only for a copy being handed in. It fails on a numbering gap, a figure the body never names, a
  drifted front-matter list, banned vocabulary, a diagram that cannot print legibly on A4, and an open
  placeholder. Do not silence a check to get a green run; fix what it names.
- After adding, removing or moving a figure or table, run `python docs/book/build/renumber.py --apply`.
- `docs/DOCUMENTATION_BOOK_OUTLINE.md` and the chapter files in `docs/book/` are one structure seen
  twice. A section added, renamed, renumbered, reordered or dropped in either must be changed in the
  other in the same edit, and a chapter retitled in one must be retitled in the other. The build
  compares them and `--strict` fails on any disagreement, so do not put this off to a follow-up.
- Task numbers in `docs/TODO.md` move: implementation work and new instructions add areas and
  items, and areas get renumbered. Anything that quotes them has to be refreshed in the same
  change — the component-to-area map in `docs/book/build/wbs.py` (`wbs.py --check` fails on an
  unmapped area), and the tracker figures quoted in the outline's Chapter 11 block.
- Every diagram must print inside one A4 page with labels at 7pt or larger. Reshape the diagram
  (turn it, shorten labels, split it, or set it as a table) before reaching for `{landscape}`.
- Repository numbers quoted in the text must come from a command run against the tree, with the date
  recorded. Anything that cannot be sourced that way is a `*[` placeholder, never a plausible guess.
