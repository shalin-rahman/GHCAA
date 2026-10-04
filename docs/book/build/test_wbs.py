"""Regression tests for wbs.py's evidence-gathering.

Run: python docs/book/build/test_wbs.py

Standard library only, matching the rest of the book build — these scripts have
no third-party dependencies and adding a test runner for one file would be a
worse trade than a `unittest` main.

The case that matters is `commit_days` refusing to return an empty set when git
fails. Every component's duration is derived from the dates it returns, so a
silent empty set floors each one to a single day and the script goes on to print
a full schedule and critical path with no sign anything went wrong. Those
numbers are quoted in Chapter 11, which is the reason this is worth a test at
all: the failure produces confident, wrong, publishable output rather than an
error. Tracked as 69.20.
"""

import os
import subprocess
import sys
import tempfile
import unittest

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

import wbs


class CommitDaysChecksGit(unittest.TestCase):
    """69.20: git's exit status is checked rather than assumed."""

    def setUp(self):
        self._repo = wbs.REPO
        self._run = subprocess.run

    def tearDown(self):
        wbs.REPO = self._repo
        subprocess.run = self._run

    def test_returns_real_dates_from_this_repository(self):
        """The happy path still works — a guard that rejects everything is no better."""
        days = wbs.commit_days(["GHCAA.API"])
        self.assertTrue(days, "expected commit dates for GHCAA.API in this repository")
        for day in days:
            self.assertRegex(day, r"^\d{4}-\d{2}-\d{2}$")

    def test_non_repository_raises_instead_of_returning_empty(self):
        """A directory that is not a repository must stop the run, not floor durations to 1."""
        wbs.REPO = tempfile.mkdtemp()
        with self.assertRaises(SystemExit) as caught:
            wbs.commit_days(["anything"])
        self.assertIn("git log failed", str(caught.exception))

    def test_non_zero_exit_raises_and_reports_stderr(self):
        """Any non-zero status, not only 'not a repository', has to surface."""
        class Result:
            returncode = 128
            stdout = ""
            stderr = "fatal: your current branch does not have any commits yet"

        subprocess.run = lambda *a, **k: Result()
        with self.assertRaises(SystemExit) as caught:
            wbs.commit_days(["anything"])
        self.assertIn("does not have any commits", str(caught.exception))

    def test_missing_git_binary_raises(self):
        """git absent from PATH is the other way this silently produced invented numbers."""
        def boom(*a, **k):
            raise OSError(2, "No such file or directory: 'git'")

        subprocess.run = boom
        with self.assertRaises(SystemExit) as caught:
            wbs.commit_days(["anything"])
        self.assertIn("cannot run git", str(caught.exception))


class NoCommitCheck(unittest.TestCase):
    """--check flags a component with no commits only when its paths look wrong."""

    def test_planned_component_with_no_files_and_nothing_done_is_not_flagged(self):
        self.assertFalse(wbs.expects_commits(["GHCAA.API/NoSuchFile.cs"], ["90"], {"90": 0}))

    def test_area_missing_from_the_done_counts_is_not_flagged(self):
        self.assertFalse(wbs.expects_commits(["GHCAA.API/NoSuchFile.cs"], ["90"], {}))

    def test_done_items_with_a_wrong_path_are_flagged(self):
        self.assertTrue(wbs.expects_commits(["GHCAA.API/NoSuchFile.cs"], ["90"], {"90": 1}))

    def test_a_path_that_exists_is_flagged(self):
        self.assertTrue(wbs.expects_commits(["GHCAA.API"], ["90"], {}))


class TrackerItemShapes(unittest.TestCase):
    """WP37 writes items as "- **37.13m Title [TODO] Priority: P1 ...**". The patterns only
    knew "7.16 [TODO] **Priority: P3.**" until 2026-10-04, so 48 items were left out of the
    task totals and 27 open ones out of the remaining hours quoted in Chapter 11."""

    TEXT = (
        "7.16 [IN-PROGRESS] **Priority: P3.** plain shape\n"
        "7.17 [TODO] **Priority: P2 | Depends on: none.** plain open\n"
        "  more text for 7.17\n"
        "  - **37.13m Count executor [TODO] Priority: P1 | Depends on: 37.13h.** bold open\n"
        "  - **37.13h Count approval [DONE 2026-10-03] Priority: P1 | Depends on: none.** bold done\n"
        "1.5 hours each [see table] is prose, not an item\n"
    )

    def test_both_shapes_are_counted_as_items(self):
        found = [(m.group(2), m.group(3), m.group(4)) for m in wbs.ITEM.finditer(self.TEXT)]
        self.assertEqual(found, [("7", "16", "IN-PROGRESS"), ("7", "17", "TODO"),
                                 ("37", "13m", "TODO"), ("37", "13h", "DONE 2026-10-03")])

    def test_open_items_carry_their_priority_in_either_shape(self):
        entries = [m.group(0) for m in wbs.OPEN_ITEM.finditer(self.TEXT)]
        self.assertEqual([wbs.PRIORITY.search(e).group(1) for e in entries], ["P2", "P1"])
        # An open item runs to the next item of either shape, not into it.
        self.assertIn("more text for 7.17", entries[0])
        self.assertNotIn("bold open", entries[0])
        self.assertNotIn("bold done", entries[1])


if __name__ == "__main__":
    unittest.main(verbosity=2)
