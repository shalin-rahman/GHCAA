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


OTHER_SHAPES = (
    "## Work Package 34 - Site content\n"
    "> 28.21 [TODO] quoted item\n"
    "> Grep: quoted follow-on line\n"
    "34.D17 [IN PROGRESS 2026-10-01] [P0] lettered item\n"
    "47.13.1 [DONE 2026-09-04] three-part item\n"
    "49.3.C [TODO] **Priority: P2 | Depends on: none.** lettered sub-item\n"
    "- **62.56g** [DONE 2026-10-15] bold id only\n"
    "\n"
    "- **34.D7** - a summary line with no bracket is not an item\n"
)


class OtherIdShapes(unittest.TestCase):
    """Until 2026-10-20 these shapes were skipped, which left nine open items off the page."""

    def setUp(self):
        self._root = tracker_page.ROOT
        tracker_page.ROOT = Path(tempfile.mkdtemp())
        (tracker_page.ROOT / "docs").mkdir()
        (tracker_page.ROOT / "docs" / "TODO.md").write_text(OTHER_SHAPES, encoding="utf-8")
        self.items = {i["id"]: i for i in tracker_page.parse()[1]}

    def tearDown(self):
        tracker_page.ROOT = self._root

    def test_every_shape_is_read_once(self):
        self.assertEqual(list(self.items), ["28.21", "34.D17", "47.13.1", "49.3.C", "62.56g"])

    def test_quoted_item_drops_the_quote_marks(self):
        self.assertEqual(self.items["28.21"]["text"], "quoted item Grep: quoted follow-on line")

    def test_bracketed_priority_is_read(self):
        item = self.items["34.D17"]
        self.assertEqual((item["state"], item["pr"], item["status"]), ("IN PROGRESS", "P0", "open"))
        self.assertEqual(item["text"], "lettered item")

    def test_lettered_sub_item_keeps_its_priority(self):
        self.assertEqual(self.items["49.3.C"]["pr"], "P2")

    def test_bold_id_leaves_no_stray_bold(self):
        self.assertEqual(self.items["62.56g"]["text"], "bold id only")


class WontDo(unittest.TestCase):
    """Until 2026-10-31 a [WONTDO] item was not read at all, so it was neither open nor closed."""

    def setUp(self):
        self._root = tracker_page.ROOT
        tracker_page.ROOT = Path(tempfile.mkdtemp())
        (tracker_page.ROOT / "docs").mkdir()
        (tracker_page.ROOT / "docs" / "TODO.md").write_text(
            "## Work Package 62 - White label\n"
            "62.31 [WONTDO 2026-10-31] **Priority: P0 | Depends on: 62.3.** dropped by the owner\n",
            encoding="utf-8")
        self.items = {i["id"]: i for i in tracker_page.parse()[1]}

    def tearDown(self):
        tracker_page.ROOT = self._root

    def test_wontdo_is_read_as_closed(self):
        item = self.items["62.31"]
        self.assertEqual((item["state"], item["status"], item["pr"]), ("WONTDO", "closed", "P0"))


if __name__ == "__main__":
    unittest.main(verbosity=2)
