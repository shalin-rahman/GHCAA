"""Tests for pages.py and the folios.body_start it depends on.

Run: python docs/book/build/test_pages.py

Standard library only, like test_wbs.py. The body_start cases matter most: when
the Part divider pages were dropped (67.5), the old short-page test stopped
matching, body_start fell back to page 1, and every contents row and every
chapter count was taken from the contents page instead of the body.
"""

import io
import os
import sys
import tempfile
import unittest

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

import folios
import pages

CONTENTS = ("Part, chapter and section Page PART I — PROBLEM AND CONTEXT 12 "
            "Chapter 1 — Introduction 12 Chapter 2 — Review 17 References 30")
OUTLINE = """Some text | 1 Other table | 9 | 9 |

Per-chapter budget, in printed pages:

| Chapter | 27 Sep | Budget |
|---|---|---|
| 1 Introduction | 5 | 4 |
| 2 Literature and systems review | 9 | 8 |
| References | 3 | 3 |
| **Counted range** | **17** | **15** |

| 3 After the block | 1 | 1 |
"""


class BodyStart(unittest.TestCase):

    def test_part_heading_on_the_first_chapter_page(self):
        book = ["Title", CONTENTS, "PART I — PROBLEM AND CONTEXT Chapter 1 — Introduction " + "x " * 400]
        self.assertEqual(folios.body_start(book), 3)

    def test_part_divider_page_still_found(self):
        book = ["Title", CONTENTS, "PART I — PROBLEM AND CONTEXT", "Chapter 1 — Introduction"]
        self.assertEqual(folios.body_start(book), 3)

    def test_contents_page_is_never_the_body(self):
        self.assertEqual(folios.body_start(["Title", CONTENTS]), 1)


class Budgets(unittest.TestCase):

    def test_reads_only_the_budget_block(self):
        self.assertEqual(pages.budgets(OUTLINE), {"1": 4, "2": 8, "References": 3})

    def test_no_block_gives_no_budget(self):
        self.assertEqual(pages.budgets("nothing here"), {})


class ChapterPages(unittest.TestCase):
    TITLES = [("1", "Chapter 1 — Introduction"), ("2", "Chapter 2 — Review"),
              ("References", "References")]

    def book(self):
        return ["Title", CONTENTS,
                "PART I — PROBLEM AND CONTEXT Chapter 1 — Introduction", "more",
                "Chapter 2 — Review, which cites References in passing", "more", "more",
                "References", "end"]

    def test_counts_from_the_body_not_the_contents(self):
        counts, missing = pages.chapter_pages(self.book(), self.TITLES)
        self.assertEqual(missing, [])
        self.assertEqual(counts, {"1": 2, "2": 3, "References": 2})

    def test_title_quoted_earlier_is_not_the_chapter(self):
        # "References" is quoted on Chapter 2's first page, three pages before
        # the real heading. Searching from the page after Chapter 2 opens skips it.
        counts, _ = pages.chapter_pages(self.book(), self.TITLES)
        self.assertEqual(counts["References"], 2)

    def test_missing_chapter_is_reported(self):
        titles = self.TITLES[:1] + [("9", "Chapter 9 — Absent")] + self.TITLES[2:]
        _, missing = pages.chapter_pages(self.book(), titles)
        self.assertEqual(missing, ["9"])


class Report(unittest.TestCase):

    def test_under_the_limit_passes(self):
        out = io.StringIO()
        self.assertEqual(pages.report({"1": 50, "2": 49}, [], {"1": 60}, out), 0)

    def test_over_the_limit_fails(self):
        out = io.StringIO()
        self.assertEqual(pages.report({"1": 50, "2": 50}, [], {}, out), 1)
        self.assertIn("OVER THE PAGE LIMIT", out.getvalue())

    def test_one_chapter_over_budget_is_not_a_failure(self):
        out = io.StringIO()
        self.assertEqual(pages.report({"1": 10}, [], {"1": 4}, out), 0)
        self.assertIn("over by 6", out.getvalue())

    def test_missing_chapter_fails(self):
        self.assertEqual(pages.report({"1": 10}, ["2"], {}, io.StringIO()), 1)


class Words(unittest.TestCase):

    def test_counts_prose_and_skips_fenced_blocks(self):
        text = ("# Chapter 1 — X\n\nlead-in words\n\n## 1.1 First\n\nOne two three.\n\n"
                "```mermaid\nflowchart LR\n  a --> b\n```\n\n## 1.2 Second\n\nfour five\n")
        with tempfile.NamedTemporaryFile("w", suffix=".md", delete=False, encoding="utf-8") as f:
            f.write(text)
        try:
            self.assertEqual(pages.section_words(f.name),
                             [("1.1", "First", 3), ("1.2", "Second", 2)])
        finally:
            os.remove(f.name)

    def test_chapter_titles_skip_the_part_heading(self):
        with tempfile.TemporaryDirectory() as book:
            with io.open(os.path.join(book, "01.md"), "w", encoding="utf-8") as f:
                f.write("# PART I — CONTEXT\n\n# Chapter 1 — Introduction\n")
            with io.open(os.path.join(book, "99.md"), "w", encoding="utf-8") as f:
                f.write("# References\n")
            self.assertEqual(pages.chapter_titles(book, ["01.md", "99.md"]),
                             [("1", "Chapter 1 — Introduction"), ("References", "References")])


if __name__ == "__main__":
    unittest.main()
