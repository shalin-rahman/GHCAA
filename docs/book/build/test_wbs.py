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


if __name__ == "__main__":
    unittest.main(verbosity=2)
