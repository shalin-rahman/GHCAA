"""Unit tests for scripts/ci-local.py. Run: python -m unittest scripts/test_ci_local.py"""
import importlib.util
import os
import shutil
import tempfile
import unittest
from unittest import mock

# The file name has a hyphen, so a plain import cannot load it.
_PATH = os.path.join(os.path.dirname(os.path.abspath(__file__)), "ci-local.py")
_spec = importlib.util.spec_from_file_location("ci_local", _PATH)
ci_local = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(ci_local)


class VitestFailedFilesTests(unittest.TestCase):
    def test_failed_files_counted(self):
        out = " Test Files  2 failed | 127 passed (129)\n      Tests  769 passed (769)\n"
        self.assertEqual(ci_local.vitest_failed_files(out), 2)

    def test_all_passed_gives_zero(self):
        out = " Test Files  130 passed (130)\n      Tests  1149 passed (1149)\n"
        self.assertEqual(ci_local.vitest_failed_files(out), 0)

    def test_missing_line_gives_minus_one(self):
        self.assertEqual(ci_local.vitest_failed_files("npm ERR! missing script\n"), -1)

    def test_colour_codes_are_ignored(self):
        out = "\x1b[2m Test Files \x1b[22m \x1b[1m\x1b[31m3 failed\x1b[39m\x1b[22m | 126 passed (129)\n"
        self.assertEqual(ci_local.vitest_failed_files(out), 3)


class RunUnitTestsStepTests(unittest.TestCase):
    def setUp(self):
        # Keep the real ci-local log of the last run intact.
        tmp = tempfile.mkdtemp()
        self.addCleanup(shutil.rmtree, tmp, ignore_errors=True)
        patcher = mock.patch.object(ci_local, "LOGS", tmp)
        patcher.start()
        self.addCleanup(patcher.stop)

    def _run(self, summary):
        step = ("web", "unit-tests", f'python -c "print(\'{summary}\')"', ci_local.ROOT, False)
        return ci_local.run(step)[0]

    def test_failed_file_fails_step_despite_exit_zero(self):
        self.assertEqual(self._run(" Test Files  1 failed | 129 passed (130)"), 1)

    def test_clean_summary_passes(self):
        self.assertEqual(self._run(" Test Files  130 passed (130)"), 0)


class FindToolManifestsTests(unittest.TestCase):
    def setUp(self):
        self.top = tempfile.mkdtemp()
        self.addCleanup(shutil.rmtree, self.top, ignore_errors=True)
        self.repo = os.path.join(self.top, "users", "repo")
        self.api = os.path.join(self.repo, "api")
        os.makedirs(os.path.join(self.api, ".config"))

    def _touch(self, *parts):
        path = os.path.join(*parts)
        open(path, "w").close()
        return path

    def test_finds_stray_manifest_above_repo(self):
        stray = self._touch(self.top, "dotnet-tools.json")
        self.assertIn(stray, ci_local.find_tool_manifests(self.api, stop=self.top))

    def test_nearest_first(self):
        own = self._touch(self.api, ".config", "dotnet-tools.json")
        stray = self._touch(self.top, "users", "dotnet-tools.json")
        self.assertEqual(ci_local.find_tool_manifests(self.api, stop=self.top), [own, stray])

    def test_none_found_gives_empty_list(self):
        self.assertEqual(ci_local.find_tool_manifests(self.api, stop=self.top), [])

    def test_walk_without_stop_ends_at_drive_root(self):
        self.assertIsInstance(ci_local.find_tool_manifests(self.api), list)


class SwaggerToolHintTests(unittest.TestCase):
    MISSING = "Cannot find a tool in the manifest file that has a command named 'swagger'."

    def test_stray_manifest_is_named(self):
        stray = os.path.join(os.path.dirname(ci_local.ROOT), "dotnet-tools.json")
        lines = ci_local.swagger_tool_hint(self.MISSING, [stray])
        self.assertTrue(any("outside the repo" in l and stray in l for l in lines))

    def test_sibling_folder_sharing_the_repo_name_is_outside(self):
        sibling = ci_local.ROOT + "-old" + os.sep + "dotnet-tools.json"
        lines = ci_local.swagger_tool_hint(self.MISSING, [sibling])
        self.assertTrue(any("outside the repo" in l for l in lines))

    def test_repo_manifest_gives_no_stray_note(self):
        own = os.path.join(ci_local.API, ".config", "dotnet-tools.json")
        lines = ci_local.swagger_tool_hint(self.MISSING, [own])
        self.assertEqual(len(lines), 1)
        self.assertIn(own, lines[0])

    def test_other_failure_gives_no_hint(self):
        self.assertEqual(ci_local.swagger_tool_hint("Startup type not found", ["x"]), [])


if __name__ == "__main__":
    unittest.main()
