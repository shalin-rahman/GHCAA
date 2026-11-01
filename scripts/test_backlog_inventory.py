"""Tests for backlog_inventory.py.

Run: python -m unittest discover -s scripts -p test_backlog_inventory.py
"""

import os
import sys
import tempfile
import unittest
from pathlib import Path

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

import backlog_inventory as inv


def fake_items(*pairs):
    return [dict(id=i, wp=int(i.split(".")[0]), state=s, pr="P2", text=f"item {i}")
            for i, s in pairs]


class OpenItems(unittest.TestCase):

    def test_open_excludes_done_wontdo_onhold(self):
        items = fake_items(("1.1", "DONE"), ("1.2", "WONTDO"), ("1.3", "ONHOLD"), ("1.4", "TODO"))
        self.assertEqual([i["id"] for i in inv.open_items(items)], ["1.4"])
        self.assertEqual([i["id"] for i in inv.held_items(items)], ["1.3"])

    def test_lettered_and_bold_ids_kept(self):
        items = fake_items(("34.D20", "TODO"), ("37.13m", "PARTIAL"), ("47.13.3", "IN PROGRESS"))
        self.assertEqual([i["id"] for i in inv.open_items(items)], ["34.D20", "37.13m", "47.13.3"])

    def test_duplicate_heading_open_wins(self):
        # parse() reads TODO.md before the archive, so the open copy comes first.
        items = fake_items(("88.4", "TODO"), ("88.4", "DONE"))
        self.assertEqual([i["id"] for i in inv.open_items(items)], ["88.4"])


class PlanIds(unittest.TestCase):

    def test_plan_ids_reads_ranges(self):
        self.assertEqual(inv.plan_ids("88.1–88.5 and 7.22"),
                         {"88.1", "88.2", "88.3", "88.4", "88.5", "7.22"})

    def test_check_finds_ids_the_plan_pattern_cannot_read(self):
        # The tracker accepts "84.0" and "37.13a.2"; the plan pattern does not.
        items = fake_items(("84.0", "TODO"), ("37.13a.2", "TODO"))
        plan = Path(tempfile.mkdtemp()) / "plan.md"
        plan.write_text("Waves: 84.0 and 37.13a.2", encoding="utf-8")
        self.assertEqual(inv.check(plan, items, raw_text=""), [])

    def test_unreadable_status_lines_are_reported(self):
        raw = ("7.16 [IN-PROGRESS] hardening\n98.43 [OPEN] config\n8.5 [TODO] fine\n"
               "- **37.13m** Title [PARTIAL] fine\n1.1 [DONE 2026-09-05] fine\n")
        self.assertEqual(inv.unreadable(raw), [("7.16", "IN-PROGRESS"), ("98.43", "OPEN")])

    def test_note_and_superseded_markers_are_not_reported(self):
        raw = "20.3 [SUPERSEDED] by 82.82\n44.1 [NOTE] context\n"
        self.assertEqual(inv.unreadable(raw), [])

    def test_plan_ids_reads_three_part_and_lettered_shapes(self):
        self.assertEqual(inv.plan_ids("28.0, 47.13.3, 49.3.C, 37.13a.2"),
                         {"28.0", "47.13.3", "49.3.C", "37.13a.2"})

    def test_check_fails_when_unreadable_line_is_not_in_plan(self):
        plan = Path(tempfile.mkdtemp()) / "plan.md"
        plan.write_text("Waves: 7.16", encoding="utf-8")
        problems = "\n".join(inv.check(plan, [], raw_text="7.16 [IN-PROGRESS] x\n98.43 [OPEN] y\n"))
        self.assertIn("98.43", problems)
        self.assertNotIn("7.16", problems)

    def test_main_runs_when_stdout_is_captured(self):
        import contextlib
        import io
        with contextlib.redirect_stdout(io.StringIO()):
            self.assertEqual(inv.main(["--state", "NOSUCHSTATE"]), 0)

    def test_plan_ids_skips_dates_versions_and_paths(self):
        text = "On 2026-11-01, net 9.0.1, see `docs/specs/004-x` and §12.8 in book"
        self.assertEqual(inv.plan_ids(text), set())

    def test_check_flags_closed_and_missing(self):
        items = fake_items(("62.31", "WONTDO"), ("7.22", "PARTIAL"), ("8.5", "TODO"))
        plan = Path(tempfile.mkdtemp()) / "plan.md"
        plan.write_text("Waves: 62.31, 8.5, 99.99", encoding="utf-8")
        problems = "\n".join(inv.check(plan, items, raw_text=""))
        self.assertIn("62.31", problems)
        self.assertIn("7.22", problems)
        self.assertIn("99.99", problems)
        self.assertNotIn("8.5 ", problems)

    def test_check_skips_history_blocks(self):
        # The gap report and progress log name closed items on purpose.
        items = fake_items(("7.22", "DONE"), ("8.5", "TODO"), ("8.7", "TODO"))
        plan = Path(tempfile.mkdtemp()) / "plan.md"
        plan.write_text("Waves: 8.5\n<!-- backlog-check: off -->\nDone: 7.22, 8.7\n"
                        "<!-- backlog-check: on -->\nTail\n", encoding="utf-8")
        problems = "\n".join(inv.check(plan, items, raw_text=""))
        self.assertNotIn("7.22", problems)
        self.assertIn("8.7", problems)


if __name__ == "__main__":
    unittest.main()
