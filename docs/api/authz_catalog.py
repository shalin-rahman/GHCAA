#!/usr/bin/env python3
"""Rebuilds sections 2 and 3 of the spec 002 authorization catalog from the controller source.

Run from the repo root:
  python docs/api/authz_catalog.py            print the generated sections
  python docs/api/authz_catalog.py --write    replace them in the catalog
  python docs/api/authz_catalog.py --check    exit 1 if the catalog is stale

It reads attributes as text, one line at a time, so it assumes each attribute list opens and
closes on one line. It stops with an error if one does not. Line numbers are left out of the
--check comparison, so an edit that only shifts code does not count as stale.
"""
import os
import re
import sys

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
CONTROLLERS = os.path.join(ROOT, "GHCAA.API", "Controllers")
CATALOG = os.path.join(ROOT, "docs", "specs", "002-workflow-contracts-and-validation",
                       "evidence", "authorization-catalog.md")
START = "## 2. Combinations in use"
END = "## 4. Findings"

# Lower rank is looser. When a controller and an action name different policies both apply,
# the catalog shows the stricter one.
POLICY_RANK = ["Anonymous", "Authenticated", "MemberOnly", "ElectionStaff", "AdminOnly", "SuperAdminOnly"]
BASE_TEXT = {
    "Anonymous": "`[AllowAnonymous]`, no sign-in.",
    "Authenticated": "any signed-in user (`[Authorize]` or the fallback policy).",
    "MemberOnly": "`MemberOnly`: SuperAdmin, Admin or Member.",
    "ElectionStaff": "`ElectionStaff`: SuperAdmin, Admin or ElectionOfficial.",
    "AdminOnly": "`AdminOnly`: SuperAdmin or Admin.",
    "SuperAdminOnly": "`SuperAdminOnly`: SuperAdmin only.",
}
ROLE_ORDER = ["Member", "ElectionOfficial", "Admin", "SuperAdmin"]
VERBS = {"HttpGet": "GET", "HttpPost": "POST", "HttpPut": "PUT", "HttpPatch": "PATCH", "HttpDelete": "DELETE"}

CLASS_RE = re.compile(r"\bclass\s+(\w+Controller)\b")
METHOD_RE = re.compile(r"^\s*public\s+[^=;]*?\b(\w+)\s*\(")
IN_ROLE_RE = re.compile(r'IsInRole\(\s*(?:"(\w+)"|(?:[\w.]*\.)?(\w+))\s*\)')
STRING_RE = re.compile(r'"([^"]*)"')
WATCHED_RE = re.compile(r"\b(Http(Get|Post|Put|Patch|Delete)|Route|Authorize|AllowAnonymous|RequireStepUp|\w*RateLimiting)\b")


def split_attributes(line):
    """'[HttpGet("x"), AllowAnonymous]' -> [('HttpGet', '"x"'), ('AllowAnonymous', '')]."""
    body = line.strip()[1:-1]
    parts, depth, cur = [], 0, ""
    for ch in body:
        if ch == "(":
            depth += 1
        elif ch == ")":
            depth -= 1
        if ch == "," and depth == 0:
            parts.append(cur)
            cur = ""
        else:
            cur += ch
    parts.append(cur)
    result = []
    for p in parts:
        p = p.strip()
        m = re.match(r"([\w.]+)\s*(?:\((.*)\))?$", p)
        if m:
            result.append((m.group(1).split(".")[-1], m.group(2) or ""))
    return result


def last_name(arg):
    """'Policy = Constants.Policies.AdminOnly' or '"AdminOnly"' -> 'AdminOnly'."""
    s = STRING_RE.search(arg)
    if s:
        return s.group(1)
    return re.split(r"[\s.=]+", arg.strip())[-1]


class Attrs:
    def __init__(self, pairs):
        self.routes, self.policies, self.rate = [], [], []
        self.anonymous = self.authorize = self.step_up = self.rate_off = False
        self.http = []
        for name, arg in pairs:
            if name in VERBS:
                s = STRING_RE.search(arg)
                self.http.append((VERBS[name], s.group(1) if s else ""))
            elif name == "Route":
                self.routes.append(STRING_RE.search(arg).group(1))
            elif name == "AllowAnonymous":
                self.anonymous = True
            elif name == "Authorize":
                if "Policy" in arg:
                    self.policies.append(last_name(arg.split("=", 1)[1]))
                else:
                    self.authorize = True
            elif name == "RequireStepUp":
                self.step_up = True
            elif name == "EnableRateLimiting":
                self.rate.append(last_name(arg))
            elif name == "DisableRateLimiting":
                self.rate_off = True

    def policy(self):
        if self.policies:
            return max(self.policies, key=POLICY_RANK.index)
        return "Authenticated" if self.authorize else None


def method_body(lines, start):
    """Lines from the signature to the end of the body, for the IsInRole scan."""
    depth, opened, out = 0, False, []
    for i in range(start, len(lines)):
        # Braces inside strings and comments would throw the depth count off.
        text = re.sub(r'"(?:\\.|[^"\\])*"', '""', lines[i]).split("//")[0]
        out.append(lines[i])
        if not opened and "=>" in text and "{" not in text.split("=>")[0]:
            if text.rstrip().endswith(";"):
                return out
            continue
        depth += text.count("{") - text.count("}")
        opened = opened or "{" in text
        if opened and depth <= 0:
            return out
        if not opened and text.rstrip().endswith(";"):
            return out
    return out


def join_path(prefix, template):
    if template.startswith("~/") or template.startswith("/"):
        path = template.lstrip("~")
    else:
        path = "/" + "/".join(p.strip("/") for p in (prefix, template) if p)
    return path


def parse_file(path):
    with open(path, encoding="utf-8-sig") as f:
        lines = f.read().splitlines()
    rows, pending, controller = [], [], None
    for i, raw in enumerate(lines):
        line = re.sub(r"\]\s*//.*$", "]", raw.strip())
        # "[FromQuery] int page = 1," is a parameter on a signature line, not an attribute list.
        if line.startswith("[") and not re.match(r"\[[^]]*\]\s*\w", line):
            if not line.endswith("]"):
                if WATCHED_RE.search(line):
                    raise SystemExit(f"{path}:{i + 1}: attribute list does not close on its line")
                pending = []
                continue
            pending.extend(split_attributes(line))
            continue
        if not line or line.startswith("//"):
            continue
        cm = CLASS_RE.search(line)
        if cm:
            attrs = Attrs(pending)
            controller = (cm.group(1), attrs, attrs.routes[0] if attrs.routes else "")
            pending = []
            continue
        mm = METHOD_RE.match(raw)
        if mm and controller and pending:
            act = Attrs(pending)
            pending = []
            if not act.http:
                continue
            name, cls, prefix = controller
            prefix = prefix.replace("[controller]", name[:-len("Controller")])
            if act.anonymous or cls.anonymous:
                policy, where = "Anonymous", "action" if act.anonymous else "controller"
            else:
                a, c = act.policy(), cls.policy()
                if a and (not c or POLICY_RANK.index(a) >= POLICY_RANK.index(c)):
                    policy, where = a, "action"
                elif c:
                    policy, where = c, "controller"
                else:
                    policy, where = "Authenticated", "fallback"
            roles = set()
            for body_line in method_body(lines, i):
                for m in IN_ROLE_RE.finditer(body_line):
                    roles.add(m.group(1) or m.group(2))
            rate = [] if act.rate_off else cls.rate + [r for r in act.rate if r not in cls.rate]
            templates = act.http
            if act.routes:
                templates = [(v, t or act.routes[0]) for v, t in act.http]
            for verb, template in templates:
                rows.append({
                    "verb": verb, "path": join_path(prefix, template), "controller": name,
                    "action": mm.group(1), "line": i + 1, "policy": policy, "where": where,
                    "step_up": act.step_up or cls.step_up, "roles": roles, "rate": rate,
                })
            continue
        pending = []
    return rows


def extra_routes(path):
    """Second class-level [Route] on a controller, which the tables do not repeat."""
    with open(path, encoding="utf-8-sig") as f:
        lines = f.read().splitlines()
    out, routes = [], []
    for line in lines:
        s = line.strip()
        if s.startswith("[Route("):
            routes.append(STRING_RE.search(s).group(1))
        m = CLASS_RE.search(s)
        if m:
            if len(routes) > 1:
                out.append((m.group(1), routes[1:]))
            routes = []
        elif s and not s.startswith("["):
            routes = []
    return out


def role_label(roles):
    names = sorted(roles, key=lambda r: ROLE_ORDER.index(r) if r in ROLE_ORDER else len(ROLE_ORDER))
    return names[0] if len(names) == 1 else ", ".join(names[:-1]) + " or " + names[-1]


def combination(row):
    label = row["policy"]
    if row["step_up"]:
        label += " + step-up"
    if row["roles"]:
        label += " + in-body " + role_label(row["roles"])
    return label


def combo_key(row):
    return (POLICY_RANK.index(row["policy"]), row["step_up"], role_label(row["roles"]) if row["roles"] else "")


def collect(folder=CONTROLLERS):
    rows, extras, http_count = [], [], 0
    for name in sorted(os.listdir(folder)):
        if not name.endswith(".cs"):
            continue
        path = os.path.join(folder, name)
        rows.extend(parse_file(path))
        extras.extend(extra_routes(path))
        with open(path, encoding="utf-8-sig") as f:
            http_count += len(re.findall(r"\bHttp(?:Get|Post|Put|Patch|Delete)\b", f.read()))
    return rows, extras, http_count


def render(rows, extras, http_count):
    groups = {}
    for r in rows:
        groups.setdefault(combo_key(r), []).append(r)
    keys = sorted(groups)
    short = lambda c: c[:-len("Controller")]
    out = [START, ""]
    out.append("One row per route template, so an action with a legacy alias route counts once per alias. The")
    matched = "That matches" if len(rows) == http_count else "That does not match"
    out.append(f"script found {len(rows)} routes. {matched} the {http_count} `[Http*]` attributes in the folder.")
    out += ["", "| Combination | Routes | Controllers |", "|---|---|---|"]
    for k in keys:
        g = groups[k]
        ctrls = ", ".join(sorted({short(r["controller"]) for r in g}))
        out.append(f"| {combination(g[0])} | {len(g)} | {ctrls} |")
    in_body = sum(1 for r in rows if r["roles"])
    out += ["",
            "\"In-body\" means the policy lets the caller in and the method then branches on",
            f"`User.IsInRole(...)`, usually to widen what an Admin can see or do. {in_body} routes do this.",
            "", "Where the policy was set:", "",
            "| Policy | On the action | On the controller | Fallback only |", "|---|---|---|---|"]
    for p in POLICY_RANK:
        counts = [sum(1 for r in rows if r["policy"] == p and r["where"] == w) for w in ("action", "controller", "fallback")]
        if any(counts):
            out.append(f"| {p} | {counts[0]} | {counts[1]} | {counts[2]} |")
    fallback = sum(1 for r in rows if r["where"] == "fallback")
    out.append("")
    if fallback:
        out.append(f"{fallback} routes rely on the fallback policy alone.")
    else:
        out.append("No route relies on the fallback policy alone. Every action or its controller names its rule.")
    out += ["",
            "Class and action attributes both apply. Where a controller and an action name different policies,",
            "the stricter one is shown, and `[AllowAnonymous]` on either one wins over both.",
            "", "## 3. Endpoints by combination", "",
            "Paths are built from the controller's `[Route]` and the action's template."]
    if extras:
        parts = [f"`{c}` " + ("also answers under " if n == 0 else "under ") + " and ".join(f"`/{r}`" for r in rs)
                 for n, (c, rs) in enumerate(extras)]
        out.append(" and ".join(parts) + ", because each has a second class-level `[Route]`.")
        out.append("Those copies are not repeated below.")
    for n, k in enumerate(keys, 1):
        g = sorted(groups[k], key=lambda r: (r["path"], r["verb"]))
        out += ["", f"### 3.{n} {combination(g[0])}", "", f"Base policy: {BASE_TEXT[g[0]['policy']]}", "",
                "| Verb | Path | Action | Rate limit |", "|---|---|---|---|"]
        for r in g:
            out.append(f"| {r['verb']} | `{r['path']}` | `{r['controller']}.{r['action']}` (line {r['line']}) | {', '.join(r['rate'])} |")
    out.append("")
    return "\n".join(out) + "\n"


def splice(doc, generated):
    start, end = doc.index(START), doc.index(END)
    return doc[:start] + generated + doc[end:]


def without_lines(text):
    return re.sub(r" \(line \d+\)", "", text)


def main(argv):
    rows, extras, http_count = collect()
    generated = render(rows, extras, http_count)
    if not argv:
        sys.stdout.write(generated)
        return 0
    with open(CATALOG, encoding="utf-8", newline="") as f:
        doc = f.read()
    eol = "\r\n" if "\r\n" in doc else "\n"
    updated = splice(doc.replace("\r\n", "\n"), generated).replace("\n", eol)
    if argv[0] == "--check":
        if without_lines(updated) != without_lines(doc):
            print("authorization-catalog.md is stale. Run: python docs/api/authz_catalog.py --write")
            return 1
        print("authorization-catalog.md matches the controllers.")
        return 0
    if argv[0] == "--write":
        with open(CATALOG, "w", encoding="utf-8", newline="") as f:
            f.write(updated)
        print(f"wrote {len(rows)} routes to {os.path.relpath(CATALOG, ROOT)}")
        return 0
    print("usage: authz_catalog.py [--write | --check]")
    return 2


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
