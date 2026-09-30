"""Tests for authz_catalog.py.

Run: python docs/api/test_authz_catalog.py

Standard library only, like the book build tests. The cases are the ones that would put a route in
the wrong section of the catalog without any error: the stricter of two policies, AllowAnonymous
winning, step-up on the class, parameter attributes on signature lines, and a second class route.
"""

import os
import sys
import tempfile
import unittest

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

import authz_catalog as ac

SAMPLE = '''
namespace X;

[ApiController]
[Route("api/things")]
[Route("api/thing")]
[Authorize(Policy = Constants.Policies.AdminOnly)]
[EnableRateLimiting(Constants.RateLimitPolicies.Auth)]
public class ThingsController : ControllerBase
{
    [HttpGet("{id}"), AllowAnonymous]
    public IActionResult Get(int id) => Ok();

    [HttpPost]
    [Authorize(Policy = Policies.SuperAdminOnly)] // stricter than the class
    [GHCAA.API.Filters.RequireStepUp]
    public IActionResult Create(
        [FromQuery] int page = 1,
        [FromQuery] string q = "")
    {
        if (User.IsInRole(Constants.Roles.SuperAdmin)) { return Ok("}"); }
        return Ok();
    }

    [HttpDelete("{id}")]
    [HttpDelete("/api/legacy/things/{id}")]
    [Authorize(Policy = Policies.MemberOnly)]
    [DisableRateLimiting]
    public IActionResult Remove(int id)
    {
        return Ok();
    }

    public class Body
    {
        [RegularExpression(@"^x$",
            ErrorMessage = "no")]
        public string Name { get; set; } = "";
    }
}

[Route("api/[controller]")]
[Authorize]
[RequireStepUp]
public sealed class OtherController : ControllerBase
{
    [HttpGet]
    public IActionResult List() => Ok(User.IsInRole("Admin"));
}
'''


class ParseSample(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.dir = tempfile.TemporaryDirectory()
        with open(os.path.join(cls.dir.name, "Things.cs"), "w", encoding="utf-8") as f:
            f.write(SAMPLE)
        cls.rows, cls.extras, cls.http = ac.collect(cls.dir.name)
        cls.by = {(r["verb"], r["path"]): r for r in cls.rows}

    @classmethod
    def tearDownClass(cls):
        cls.dir.cleanup()

    def test_every_http_attribute_gives_one_row(self):
        self.assertEqual(len(self.rows), 5)
        self.assertEqual(self.http, 5)

    def test_allow_anonymous_on_the_action_wins_over_the_class_policy(self):
        row = self.by[("GET", "/api/things/{id}")]
        self.assertEqual((row["policy"], row["where"]), ("Anonymous", "action"))

    def test_stricter_action_policy_is_shown_with_step_up_and_in_body_role(self):
        row = self.by[("POST", "/api/things")]
        self.assertEqual((row["policy"], row["where"]), ("SuperAdminOnly", "action"))
        self.assertTrue(row["step_up"])
        self.assertEqual(row["roles"], {"SuperAdmin"})
        self.assertEqual(ac.combination(row), "SuperAdminOnly + step-up + in-body SuperAdmin")

    def test_looser_action_policy_keeps_the_class_policy(self):
        row = self.by[("DELETE", "/api/things/{id}")]
        self.assertEqual((row["policy"], row["where"]), ("AdminOnly", "controller"))
        self.assertEqual(row["rate"], [])

    def test_absolute_template_ignores_the_class_route(self):
        self.assertIn(("DELETE", "/api/legacy/things/{id}"), self.by)

    def test_class_step_up_controller_token_and_literal_role(self):
        row = self.by[("GET", "/api/Other")]
        self.assertEqual((row["policy"], row["where"]), ("Authenticated", "controller"))
        self.assertTrue(row["step_up"])
        self.assertEqual(row["roles"], {"Admin"})

    def test_rate_limit_comes_from_the_class(self):
        self.assertEqual(self.by[("GET", "/api/things/{id}")]["rate"], ["Auth"])

    def test_second_class_route_is_reported_not_repeated(self):
        self.assertEqual(self.extras, [("ThingsController", ["api/thing"])])
        self.assertFalse(any(p.startswith("/api/thing/") for _, p in self.by))

    def test_line_is_the_method_signature(self):
        self.assertEqual(self.by[("POST", "/api/things")]["line"], SAMPLE.splitlines().index("    public IActionResult Create(") + 1)


class RenderAndSplice(unittest.TestCase):
    def test_splice_keeps_text_outside_the_generated_sections(self):
        doc = "# T\n\n## 1. Policies\n\nkeep\n\n" + ac.START + "\nold\n" + ac.END + "\n\nkeep too\n"
        out = ac.splice(doc, ac.START + "\nnew\n")
        self.assertIn("keep\n", out)
        self.assertIn("new", out)
        self.assertNotIn("old", out)
        self.assertTrue(out.endswith(ac.END + "\n\nkeep too\n"))

    def test_check_ignores_line_number_shifts(self):
        a = "| GET | `/x` | `A.B` (line 10) |  |"
        self.assertEqual(ac.without_lines(a), ac.without_lines(a.replace("10", "12")))

    def test_unclosed_watched_attribute_stops_the_run(self):
        with tempfile.TemporaryDirectory() as d:
            with open(os.path.join(d, "Bad.cs"), "w", encoding="utf-8") as f:
                f.write('public class BadController\n{\n    [HttpGet("x",\n        Name = "y")]\n    public IActionResult X() => Ok();\n}\n')
            with self.assertRaises(SystemExit):
                ac.collect(d)


if __name__ == "__main__":
    unittest.main()
