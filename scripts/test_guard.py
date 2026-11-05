import unittest

from guard import check


class GuardTests(unittest.TestCase):
    def test_bare_conflict_in_a_migration_is_caught(self):
        hits = check([("GHCAA.Infrastructure/Data/Migrations/PgSql/X.cs", "ON CONFLICT DO NOTHING;")])
        self.assertEqual(len(hits), 1)

    def test_named_conflict_target_passes(self):
        line = 'ON CONFLICT ("LookupGroup", "Value") DO NOTHING;'
        self.assertEqual(check([("GHCAA.Infrastructure/Data/Migrations/PgSql/X.cs", line)]), [])

    def test_rule_applies_only_to_its_paths(self):
        self.assertEqual(check([("docs/notes.md", "ON CONFLICT DO NOTHING")]), [])

    def test_em_dash_only_in_comments(self):
        self.assertEqual(len(check([("a/b.ts", "  // one — two")])), 1)
        self.assertEqual(check([("a/b.ts", "const s = 'one — two';")]), [])

    def test_literal_colour_in_component_scss(self):
        self.assertEqual(len(check([("GHCAA.Web/src/app/x/x.scss", "color: #fff;")])), 1)
        self.assertEqual(check([("GHCAA.Web/src/app/x/x.scss", "color: var(--text);")]), [])


if __name__ == "__main__":
    unittest.main()
