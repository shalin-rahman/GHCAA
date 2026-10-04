"""Tests for tracker_page.py's reading of docs/TODO.md.

Run: python docs/book/build/test_tracker_page.py

Standard library only, like test_wbs.py. Until 2026-10-04 the page only knew the
"7.16 [TODO] **Priority: P3.**" shape, so the 48 WP37 items written as
"- **37.13m Title [TODO] Priority: P1 | ...**" were missing, and their text was
folded into whichever plain item came before them.
"""

import os
import sys
import tempfile
import unittest
from pathlib import Path

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

import tracker_page

TODO = (
    "## Work Package 37 - Elections\n"
    "37.1 [DONE 2026-09-20] **Priority: P1 | Depends on: none.** plain done\n"
    "  - **37.13m Count executor recorded [TODO] Priority: P1 | Depends on: 37.13h.** bold open\n"
    "    more text for 37.13m\n"
    "  - **37.1f Navigation [DONE 2026-09-22].** bold done, no priority\n"
    "1.5 hours each [DONE] is prose, not an item\n"
)


class BothItemShapes(unittest.TestCase):

    def setUp(self):
        self._root = tracker_page.ROOT
        tracker_page.ROOT = Path(tempfile.mkdtemp())
        (tracker_page.ROOT / "docs").mkdir()
        (tracker_page.ROOT / "docs" / "TODO.md").write_text(TODO, encoding="utf-8")
        self.items = {i["id"]: i for i in tracker_page.parse()[1]}

    def tearDown(self):
        tracker_page.ROOT = self._root

    def test_bold_items_are_read_and_prose_is_not(self):
        self.assertEqual(list(self.items), ["37.1", "37.13m", "37.1f"])

    def test_bold_item_keeps_title_priority_and_dependency(self):
        item = self.items["37.13m"]
        self.assertEqual((item["state"], item["pr"], item["dep"]), ("TODO", "P1", "37.13h"))
        self.assertTrue(item["text"].startswith("Count executor recorded. bold open"))
        self.assertIn("more text for 37.13m", item["text"])
        self.assertNotIn("*", item["text"])

    def test_plain_item_stops_at_the_next_bold_item(self):
        self.assertNotIn("37.13m", self.items["37.1"]["text"])

    def test_bold_item_without_priority(self):
        item = self.items["37.1f"]
        self.assertEqual((item["state_label"], item["pr"]), ("DONE 2026-09-22", "none"))
        self.assertTrue(item["text"].startswith("Navigation. bold done"))


if __name__ == "__main__":
    unittest.main(verbosity=2)
