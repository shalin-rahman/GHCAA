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


GOOD_STEPS = "\n  1. Add the rule. -> check: the unit test.\n  2. Call it. -> check: the service test."


def card(sid="S01", items="8.5", drop=None, steps=GOOD_STEPS, change="`Real.cs`", order=None):
    labels = [label for label, _ in inv.CARD_PARTS if label != drop]
    if order:
        labels = order(labels)
    body = {"Steps": steps, "Change (only these files)": " " + change}
    lines = [f'<a id="{sid.lower()}"></a>', f"**{sid} · {items} · S**"]
    lines += [f"- {label}:{body.get(label, ' text.')}" for label in labels]
    return "\n".join(lines) + "\n\n"


def plan(cards, marker="A", rows=""):
    return ("## 3. Order\n\n| **A** | first phase |\n| S01 | 8.5 | rule | S | | 5 | |\n"
            "| S02 | 8.6 | caller | M | | | S01 |\n" + rows +
            "| **B** | later phase |\n| S09 | 8.9 | later | S | | | |\n\n"
            f"## 4. Cards\n\n<!-- card-template: {marker} -->\n\n### Phase A. First\n\n" + cards +
            "### Phase B. Later\n\n**S09 · 8.9 · S**\nShort form, `Gone.cs`.\n\n## 5. Gates\n")


class Cards(unittest.TestCase):

    def setUp(self):
        self.root = Path(tempfile.mkdtemp())
        (self.root / "src").mkdir()
        (self.root / "src" / "Real.cs").write_text("", encoding="utf-8")

    def problems(self, cards, **kw):
        return "\n".join(inv.card_problems(plan(cards, **kw), self.root))

    def test_full_cards_pass_and_short_phase_is_only_owed(self):
        md = plan(card() + card("S02", "8.6"))
        self.assertEqual(inv.card_problems(md, self.root), [])
        self.assertEqual(inv.template_phases(md), {"A"})
        self.assertEqual(inv.owed_phases(md), ["B"])

    def test_no_marker_checks_no_card(self):
        self.assertEqual(self.problems(card(drop="Goal"), marker=""), "")

    def test_split_cards_reads_ranges_and_phases(self):
        cards = inv.split_cards(plan(card("S01 to S02", "8.5, 8.6")))
        self.assertEqual([(c["ids"], c["phase"]) for c in cards], [(["S01", "S02"], "A"), (["S09"], "B")])

    def test_missing_part_is_named(self):
        got = self.problems(card(drop="Contract") + card("S02", "8.6"))
        self.assertIn('S01: part "Contract" is missing', got)
        self.assertNotIn("S02", got)

    def test_parts_out_of_order(self):
        got = self.problems(card(order=lambda labels: labels[::-1]) + card("S02", "8.6"))
        self.assertIn("S01: the parts are out of order", got)

    def test_step_without_check(self):
        steps = "\n  1. Add the rule. -> check: the unit test.\n  2. Call it."
        self.assertIn('S01: step 2 has no "-> check:"', self.problems(card(steps=steps) + card("S02", "8.6")))

    def test_steps_without_numbers(self):
        self.assertIn("S01: Steps has no numbered step", self.problems(card(steps=" do it.") + card("S02", "8.6")))

    def test_file_not_in_repo_is_reported(self):
        got = self.problems(card(change="`src/Real.cs`, `Gone.cs`, `api/x/{id}`") + card("S02", "8.6"))
        self.assertIn("S01: `Gone.cs` is not in the repo", got)
        self.assertNotIn("Real.cs", got)
        self.assertNotIn("api/x", got)

    def test_every_new_form_covers_the_file(self):
        change = "`A.cs` (new), `B.cs` (new: one test), `C.cs` (new, shared)"
        self.assertEqual(self.problems(card(change=change) + card("S02", "8.6")), "")

    def test_new_full_path_and_new_folder_cover_later_names(self):
        first = card(change="`src/Fresh.cs` (new), `tests/walk/` (new)")
        second = card("S02", "8.6", change="`Fresh.cs`, `tests/walk/steps.ts`")
        self.assertEqual(self.problems(first + second), "")

    def test_row_without_card_and_done_row(self):
        rows = "| S03 | done 2026-11-01 | old | S | | | |\n| S04 | 8.7 | owed | S | | | |\n"
        got = self.problems(card() + card("S02", "8.6"), rows=rows)
        self.assertIn("S04: in the section 3 table but has no card", got)
        self.assertNotIn("S03", got)
        self.assertNotIn("S09", got)

    def test_link_to_missing_anchor(self):
        got = self.problems(card() + card("S02", "8.6") + "See [S07](#s07) and [S01](#s01).\n\n")
        self.assertIn("S07: the plan links to #s07", got)
        self.assertNotIn("#s01", got)

    def test_unreadable_marker_and_phase_without_cards(self):
        two = card() + card("S02", "8.6")
        self.assertIn("marker is not readable", self.problems(two, marker="A, B"))
        self.assertIn("names phase C, which has no cards", self.problems(two, marker="A C"))

    def test_backwards_range_keeps_the_first_session(self):
        cards = inv.split_cards(plan(card("S02 to S01", "8.5, 8.6")))
        self.assertEqual(cards[0]["ids"], ["S02"])

    def test_check_with_nothing_after_it(self):
        steps = "\n  1. Add the rule. -> check:\n     the unit test.\n  2. Call it. -> check:"
        got = self.problems(card(steps=steps) + card("S02", "8.6"))
        self.assertIn('S01: step 2 has nothing after "-> check:"', got)
        self.assertNotIn("step 1", got)

    def test_skeleton_hint_left_in_a_part(self):
        got = self.problems(card(change="<files this card may change>") + card("S02", "8.6"))
        self.assertIn("S01: a part still holds the skeleton hint", got)
        self.assertEqual(self.problems(card(change="`run <id>`") + card("S02", "8.6")), "")

    def test_dot_folder_path_wrong_case_and_anchor_tail(self):
        change = "`.github/gone.yml`, `SRC/Real.cs`, `src/Real.cs#Section`, `.cs`"
        got = self.problems(card(change=change) + card("S02", "8.6"))
        self.assertIn("`.github/gone.yml` is not in the repo", got)
        self.assertIn("`SRC/Real.cs` is not in the repo", got)
        self.assertEqual(got.count("is not in the repo"), 2)

    def test_new_file_does_not_cover_an_earlier_card(self):
        got = self.problems(card(change="`Later.cs`") + card("S02", "8.6", change="`Later.cs` (new)"))
        self.assertIn("S01: `Later.cs` is not in the repo", got)
        self.assertNotIn("S02", got)

    def test_rows_outside_section_3_are_not_read(self):
        md = plan(card() + card("S02", "8.6")) + "\n## 12. Log\n\n| S44 | 8.8 | stray | S | | | |\n"
        self.assertEqual([r[0] for r in inv.sequence_rows(md)], ["S01", "S02", "S09"])

    def test_tracker_entry_stops_at_the_next_item(self):
        raw = "8.5 [TODO] rule\n  detail line\n8.6 [TODO] caller\n"
        self.assertEqual(inv.tracker_entry(raw, "8.5"), "8.5 [TODO] rule\n  detail line")
        self.assertIsNone(inv.tracker_entry(raw, "9.9"))

    def test_card_skeleton_by_session_and_by_item(self):
        md = plan(card())
        raw = "8.5 [TODO] rule\n8.6 [TODO] caller\n  detail line\n"
        text = inv.card_skeleton(md, "s02", raw)
        self.assertIn('<a id="s02"></a>\n**S02 · 8.6 · M**', text)
        self.assertIn("- Before you start: S01 closed. <", text)
        self.assertIn("Tracker entry 8.6:\n8.6 [TODO] caller\n  detail line", text)
        self.assertEqual([l.split(":")[0][2:] for l in text.splitlines() if l.startswith("- ")],
                         [label for label, _ in inv.CARD_PARTS])
        self.assertIn("**S01 · 8.5 · S · migration 5**", inv.card_skeleton(md, "8.5", raw))
        self.assertIsNone(inv.card_skeleton(md, "S77", raw))


if __name__ == "__main__":
    unittest.main()
