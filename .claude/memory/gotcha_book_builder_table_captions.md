---
name: gotcha-book-builder-table-captions
description: docs/book/build/build.py only recognizes a table caption written as a "### Table N.N — ..." heading directly above the table; bold text ("**Table N.N — ...**") is silently ignored (not captioned, not numbered, not counted).
metadata:
  type: gotcha
---

**What happened:** While drafting Chapters 4-6, table captions were written as bold Markdown
(`**Table 4.1 — Evaluation plan**`) immediately above the table, matching how the caption *reads* in
chapters 1-3. The build ran with no errors and no warning specific to this — the builder's own
orphan-caption report only checks captions it already recognized, so a caption in the wrong form
doesn't show up as broken, it just silently never gets picked up as a table caption at all (no
number applied, not placed above the table per IEEE convention, not counted in the build report's
table total).

**Root cause:** per `docs/book/README.md`, the builder's caption rule is specifically "a heading of
the form `### Figure 3.1 — Caption text` or `### Table 3.1 — Caption text`" — a level-3 Markdown
heading, not bold inline text. Figures in the existing chapters already followed this
(`### Figure 1.1 — ...`); tables in chapters 1-3 mostly don't have a standalone numbered caption at
all (the FR/NFR/DC tables are plain inline tables with no `### Table` heading), so there was no
existing "bold table caption" example to copy from — the mistake was inventing a plausible-looking
convention instead of grepping for the real one first.

**How it was caught:** ran the build (`python3 docs/book/build/build.py`) before and after adding the
new chapters and compared the "tables: N captioned" line — it read 6 before the fix and only rose to
18 (6 existing + 12 new) after converting every `**Table N.N — ...**` line to `### Table N.N — ...`.
If that count doesn't rise by the number of new tables you captioned, something didn't get picked up.

**How to apply:** before writing a new chapter with numbered tables, run
`grep -n "^### Table" docs/book/*.md` to see the real pattern in use, and always write a numbered
table caption as its own `### Table N.N — Caption text` line directly above the table (no intervening
paragraph unless it's meant to be absorbed as a note, per the README). Then rebuild and check the
`tables: N captioned` count actually increased by the right amount — don't trust a clean exit code
alone.
