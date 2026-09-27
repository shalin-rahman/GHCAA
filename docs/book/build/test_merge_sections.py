"""Tests for merge_sections.py.

Run: python docs/book/build/test_merge_sections.py

Standard library only, like test_wbs.py. Each test writes a small chapter, a
second chapter that refers into it, and an outline, then merges.
"""

import io
import os
import shutil
import sys
import tempfile
import unittest

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

import merge_sections as ms

CHAPTER = """# Chapter 3 — Requirements

## 3.1 Sources

See §3.3 and Section 3.4.

## 3.2 Analysis

### 3.2.1 First part

### 3.2.2 Second part

## 3.3 Validation

Text that stays. Figure 3.3 and ISO/IEC 25010:2011 are not sections.

### 3.3.1 Reviews

## 3.4 Constraints

As §3.3.1 says, and §§3.2–3.3.
"""

OTHER = """# Chapter 9 — Verification

The checks of §3.4 close the ones in §3.3.1; Sections 3.2 and 3.3 set them.
"""

OUTLINE = """# Outline

## Chapter 3 — Requirements

- **3.1** Sources — where they came from
- **3.2** Analysis — negotiation
  - **3.2.1** First part
  - **3.2.2** Second part
- **3.3** Validation — reviews, see §3.4
  - **3.3.1** Reviews
- **3.4** Constraints — the domain

## Chapter 9 — Verification

- **9.1** Keeps §3.3 as a reference
"""


class Merge(unittest.TestCase):

    def setUp(self):
        self.dir = tempfile.mkdtemp()
        self.chapter = self.write("03.md", CHAPTER)
        self.other = self.write("09.md", OTHER)
        self.outline = self.write("outline.md", OUTLINE)

    def tearDown(self):
        shutil.rmtree(self.dir)

    def write(self, name, text):
        path = os.path.join(self.dir, name)
        with io.open(path, "w", encoding="utf-8", newline="") as handle:
            handle.write(text)
        return path

    def run_merge(self, spec, title):
        changes, dropped = ms.plan(spec, title, [self.chapter, self.other], self.outline, extra=[])
        return {path: new for path, _old, new in changes}, dropped

    def test_merge_in_the_middle(self):
        new, dropped = self.run_merge("3.2+3.3", "Analysis and Validation")
        chapter = new[self.chapter]
        self.assertEqual(dropped, ["## 3.3 Validation"])
        self.assertIn("## 3.2 Analysis and Validation\n", chapter)
        self.assertNotIn("## 3.3 Validation", chapter)
        self.assertIn("### 3.2.3 Reviews", chapter)
        self.assertIn("## 3.3 Constraints", chapter)
        self.assertIn("See §3.2 and Section 3.3.", chapter)
        self.assertIn("As §3.2.3 says, and §3.2.", chapter)
        self.assertIn("Text that stays.", chapter)
        self.assertIn("Figure 3.3 and ISO/IEC 25010:2011", chapter)

    def test_cross_reference_in_another_chapter(self):
        new, _ = self.run_merge("3.2+3.3", "Analysis and Validation")
        self.assertIn("The checks of §3.3 close the ones in §3.2.3; Section 3.2 set them.",
                      new[self.other])

    def test_merge_at_the_end(self):
        new, dropped = self.run_merge("3.3+3.4", "Validation and Constraints")
        chapter = new[self.chapter]
        self.assertEqual(dropped, ["## 3.4 Constraints"])
        self.assertIn("## 3.3 Validation and Constraints\n", chapter)
        self.assertIn("See §3.3 and Section 3.3.", chapter)
        self.assertIn("### 3.2.2 Second part", chapter)
        self.assertIn("The checks of §3.3 close", new[self.other])

    def test_outline_follows(self):
        new, _ = self.run_merge("3.2+3.3", "Analysis and Validation")
        outline = new[self.outline]
        self.assertIn("- **3.2** Analysis and Validation — negotiation\n", outline)
        self.assertIn("  - Validation — reviews, see §3.3\n", outline)
        self.assertIn("  - **3.2.3** Reviews\n", outline)
        self.assertIn("- **3.3** Constraints — the domain\n", outline)
        self.assertIn("- **9.1** Keeps §3.2 as a reference", outline)

    def test_bad_specs_are_refused(self):
        for spec in ("3.2", "3.2+3.4", "3.2+4.3", "3.3+3.2", "3.4+3.5"):
            with self.assertRaises(ms.MergeError, msg=spec):
                self.run_merge(spec, "x")


    def test_write_keeps_each_files_line_ending(self):
        crlf = self.write("crlf.md", OUTLINE.replace("\n", "\r\n"))
        new, _ = self.run_merge("3.2+3.3", "Analysis and Validation")
        ms._write(crlf, new[self.outline])
        ms._write(self.chapter, new[self.chapter])
        with io.open(crlf, "rb") as handle:
            data = handle.read()
        self.assertEqual(data.count(b"\n"), data.count(b"\r\n"))
        with io.open(self.chapter, "rb") as handle:
            self.assertNotIn(b"\r", handle.read())


class References(unittest.TestCase):
    MOVED = {"3.12": "3.11", "3.12.1": "3.11.3", "3.13": "3.12"}

    def test_range_collapses(self):
        self.assertEqual(ms.rewrite_references("§3.11–3.12.", self.MOVED), "§3.11.")

    def test_deeper_number_follows_its_parent(self):
        self.assertEqual(ms.rewrite_references("§3.12.1.4", self.MOVED), "§3.11.3.4")

    def test_untouched_chapter(self):
        self.assertEqual(ms.rewrite_references("§4.12 and Table 3.12", self.MOVED),
                         "§4.12 and Table 3.12")


if __name__ == "__main__":
    unittest.main()
