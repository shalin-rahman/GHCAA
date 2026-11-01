"""Tests for archive_done_todos.py.

Run: python -m unittest discover -s scripts -p test_archive_done_todos.py
"""

import os
import sys
import unittest

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

import archive_done_todos as arc


def ids(blocks, order):
    return [b[0].split(" ")[0] for h in order for b in blocks[h]]


class Collect(unittest.TestCase):

    TODO = ["# Tracker", "## STANDING RULES", "1.1 [DONE] a rule, not an item move",
            "## WORK PACKAGE 7: X", "7.1 [DONE 2026-10-01] done", "  more of 7.1", "",
            "7.2 [WONTDO 2026-10-31] dropped", "", "7.3 [TODO] open", "",
            "7.4 [PARTIAL] half", ""]

    def test_closed_states_move_and_open_stay(self):
        blocks, order, keep = arc.collect(self.TODO, arc.CLOSED)
        self.assertEqual(ids(blocks, order), ["7.1", "7.2"])
        self.assertIn("7.3 [TODO] open", keep)
        self.assertNotIn("  more of 7.1", keep)
        # Items above the first work package heading never move.
        self.assertIn("1.1 [DONE] a rule, not an item move", keep)

    def test_open_states_are_picked_from_the_archive(self):
        archive = ["# TODO Archive", "## WORK PACKAGE 51: Y", "51.3 [DONE] x", "",
                   "51.4 [IN PROGRESS 2026-09-11] tests", "", "51.6 [ONHOLD] held", ""]
        blocks, order, keep = arc.collect(archive, arc.OPEN)
        self.assertEqual(ids(blocks, order), ["51.4", "51.6"])
        self.assertIn("51.3 [DONE] x", keep)


class Place(unittest.TestCase):

    def test_new_heading_goes_before_the_next_higher_work_package(self):
        todo = ["# Tracker", "## WORK PACKAGE 37: A", "37.1 [TODO] a", "",
                "# Work Package 60 — B", "60.1 [TODO] b", ""]
        out = arc.place(todo, {"## WORK PACKAGE 51: Y": [["51.4 [IN PROGRESS] t"]]},
                        ["## WORK PACKAGE 51: Y"])
        self.assertLess(out.index("## WORK PACKAGE 51: Y"), out.index("# Work Package 60 — B"))
        self.assertGreater(out.index("51.4 [IN PROGRESS] t"), out.index("37.1 [TODO] a"))

    def test_existing_heading_gets_the_block_in_its_section(self):
        archive = ["# Archive\r", "## WORK PACKAGE 7: X\r", "7.0 [DONE] a\r", "\r",
                   "## WORK PACKAGE 8: Z\r", "8.1 [DONE] z\r"]
        out = arc.place(archive, {"## WORK PACKAGE 7: X": [["7.1 [DONE] b"]]},
                        ["## WORK PACKAGE 7: X"])
        self.assertLess(out.index("7.1 [DONE] b\r"), out.index("## WORK PACKAGE 8: Z\r"))

    def test_heading_with_higher_number_than_all_is_appended(self):
        out = arc.place(["## WORK PACKAGE 7: X", "7.1 [TODO] a"],
                        {"# Work Package 99 — W": [["99.1 [TODO] w"]]}, ["# Work Package 99 — W"])
        self.assertEqual(out[-2:], ["99.1 [TODO] w", ""])


if __name__ == "__main__":
    unittest.main()
