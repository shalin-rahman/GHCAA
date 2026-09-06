"""Build a filterable view of every item in docs/TODO.md, open and closed.

Reads the tracker, never a hand-kept copy of it, so the page cannot drift from
the file. Run it again after any tracker change and republish.
"""
import html
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[3]
OUT = ROOT / "docs" / "book" / "build" / "tracker.html"

HEAD = re.compile(r"^#+ *Work Package (\d+)[ \u2014:-]*(.*)$")
# The bracket sometimes carries a trailing date or note, e.g. "[DONE 2026-09-05]"
# or "[DONE \u2014 see 40.11]", so the state word and that trailing text are two groups.
ITEM = re.compile(r"^(\d+)\.(\d+[a-z]?) *\[(TODO|IN PROGRESS|BLOCKED|ONHOLD|PARTIAL|DONE)([^\]]*)\]\s*(.*)$")

CATEGORY = {
    62: ("White-label", "Making one codebase serve any institution"),
    60: ("Mobile", "Flutter client parity with the web portal"),
    42: ("Feature", "Product work requested by the Association"),
    45: ("Feature", "Product work requested by the Association"),
    46: ("Feature", "Product work requested by the Association"),
    47: ("Feature", "Product work requested by the Association"),
    49: ("Feature", "Product work requested by the Association"),
    51: ("Feature", "Product work requested by the Association"),
    52: ("Feature", "Product work requested by the Association"),
    43: ("Code", "Backend and web engineering"),
    44: ("Code", "Backend and web engineering"),
    57: ("Tests", "Coverage, test design and test evidence"),
    48: ("Security", "Audit, scanning and hardening"),
    72: ("Security", "Audit, scanning and hardening"),
}
CATEGORY[80] = ("Code", "Backend and web engineering")
CATEGORY[81] = ("Feature", "Product work requested by the Association")
CATEGORY[82] = ("Architecture", "Audit findings and the refactoring they name")
for n in range(63, 80):
    CATEGORY[n] = ("Dissertation", "The book, its build and its evidence")

PRIORITIES = ["P0", "P1", "P2", "P3", "P4", "none"]
PRIORITY_MEANING = {
    "P0": "Ships nothing until this is done",
    "P1": "Blocks the next milestone",
    "P2": "Wanted, scheduled",
    "P3": "Wanted, unscheduled",
    "P4": "Nice to have",
    "none": "Never triaged",
}


def classify(wp, text):
    if wp in CATEGORY:
        return CATEGORY[wp]
    low = text.lower()
    if "test" in low:
        return ("Tests", "Coverage, test design and test evidence")
    if "mobile" in low or "flutter" in low:
        return ("Mobile", "Flutter client parity with the web portal")
    if "security" in low or "vulner" in low:
        return ("Security", "Audit, scanning and hardening")
    return ("Code", "Backend and web engineering")


def inline(s):
    """Markdown inline to HTML, on already-escaped text."""
    s = html.escape(s)
    s = re.sub(r"`([^`]+)`", r"<code>\1</code>", s)
    s = re.sub(r"\*\*([^*]+)\*\*", r"<strong>\1</strong>", s)
    return s


def parse():
    lines = (ROOT / "docs" / "TODO.md").read_text(encoding="utf-8").split("\n")
    titles, items = {}, []
    wp, wp_title = None, ""
    for i, line in enumerate(lines):
        m = HEAD.match(line)
        if m:
            wp = int(m.group(1))
            wp_title = re.sub(r"\s*\(raised by user.*", "", m.group(2)).strip(" \u2014-:")
            titles[wp] = wp_title
            continue
        m = ITEM.match(line)
        if not m:
            continue
        num = int(m.group(1))
        body = [m.group(5)]
        for nxt in lines[i + 1:]:
            if not nxt.strip() or ITEM.match(nxt) or nxt.startswith("#"):
                break
            body.append(nxt.strip())
        blob = " ".join(body)
        # An item states its priority and dependencies in one bold run:
        # "**Priority: P2 | Depends on: 82.1.**". Older items split them or
        # omit the bold, so both shapes have to parse.
        meta = re.search(r"\*\*([^*]*(?:Priority|Depends on):[^*]*)\*\*", blob)
        meta_text = meta.group(1) if meta else blob
        pr = re.search(r"Priority:\s*(P\d)", meta_text)
        dep_m = re.search(r"Depends on:?\s*([^|*]+)", meta_text)
        clean = re.sub(r"\*\*[^*]*(?:Priority|Depends on):[^*]*\*\*", "", blob)
        clean = re.sub(r"Priority:\s*P\d\s*[.|]?", "", clean)
        clean = re.sub(r"Depends on:?\s*(?:none|[\d.,a-z\s]+?)\.", "", clean, count=1)
        clean = re.sub(r"^[\s|*.]+", "", clean).strip()
        cat, cat_note = classify(num, blob)
        state = m.group(3)
        note = m.group(4).strip(" —-:")
        items.append(dict(
            id=f"{m.group(1)}.{m.group(2)}", wp=num, state=state,
            state_label=(f"{state} {note}".strip() if note else state),
            status=("closed" if state == "DONE" else "open"),
            pr=pr.group(1) if pr else "none",
            dep=dep_m.group(1).strip().rstrip(".").strip() if dep_m else "",
            cat=cat, cat_note=cat_note, text=clean))
    return titles, items


def main():
    titles, items = parse()
    open_items = [it for it in items if it["status"] == "open"]
    closed_total = len(items) - len(open_items)
    cats = {}
    for it in items:
        cats.setdefault(it["cat"], {"note": it["cat_note"], "items": []})["items"].append(it)
    # The priority matrix is about triaging open work, so it counts open items
    # only; a category with nothing but closed items still gets a (all-zero) row.
    order = sorted(cats, key=lambda c: -sum(1 for it in cats[c]["items"] if it["status"] == "open"))

    counts = {c: {p: 0 for p in PRIORITIES} for c in order}
    for it in open_items:
        counts[it["cat"]][it["pr"]] += 1
    totals = {p: sum(counts[c][p] for c in order) for p in PRIORITIES}

    rows = []
    for c in order:
        cells = "".join(
            f'<td class="n{" z" if counts[c][p] == 0 else ""}">{counts[c][p]}</td>'
            for p in PRIORITIES)
        cat_open = sum(1 for it in cats[c]["items"] if it["status"] == "open")
        rows.append(f'<tr><th scope="row">{html.escape(c)}</th>{cells}'
                    f'<td class="n tot">{cat_open}</td></tr>')
    total_cells = "".join(f'<td class="n">{totals[p]}</td>' for p in PRIORITIES)
    rows.append(f'<tr class="sum"><th scope="row">All</th>{total_cells}'
                f'<td class="n tot">{len(open_items)}</td></tr>')

    blocks = []
    for c in order:
        group = cats[c]
        by_wp = {}
        for it in group["items"]:
            by_wp.setdefault(it["wp"], []).append(it)
        packs = []
        for wp in sorted(by_wp):
            cards = []
            for it in sorted(by_wp[wp], key=lambda x: (PRIORITIES.index(x["pr"]), x["id"])):
                dep = (f'<span class="dep">after {html.escape(it["dep"])}</span>'
                       if it["dep"] and it["dep"].lower() != "none" else "")
                state = ("" if it["state"] == "TODO"
                         else f'<span class="state">{it["state_label"].lower()}</span>')
                cards.append(
                    f'<article class="item" data-pr="{it["pr"]}" data-cat="{html.escape(c)}" '
                    f'data-status="{it["status"]}" data-state="{it["state"].lower().replace(" ", "-")}" '
                    f'data-find="{html.escape((it["id"] + " " + it["text"]).lower())}">'
                    f'<div class="meta"><span class="pill {it["pr"]}">{it["pr"]}</span>'
                    f'<span class="id">{it["id"]}</span>{state}{dep}</div>'
                    f'<p>{inline(it["text"])}</p></article>')
            packs.append(
                f'<section class="pack"><h3><span class="wp">WP{wp}</span>'
                f'{html.escape(titles.get(wp, ""))}</h3>{"".join(cards)}</section>')
        cat_open = sum(1 for it in group["items"] if it["status"] == "open")
        cat_closed = len(group["items"]) - cat_open
        count_label = f"{cat_open} open" + (f" · {cat_closed} closed" if cat_closed else "")
        blocks.append(
            f'<section class="cat" data-cat="{html.escape(c)}">'
            f'<header class="cathead"><h2>{html.escape(c)}</h2>'
            f'<p>{html.escape(group["note"])}</p>'
            f'<span class="count">{count_label}</span></header>'
            f'{"".join(packs)}</section>')

    filters = "".join(
        f'<button class="chip {p}" data-filter-pr="{p}">'
        f'<span>{p}</span><em>{totals[p]}</em></button>' for p in PRIORITIES if totals[p])
    cat_filters = "".join(
        f'<button class="chip" data-filter-cat="{html.escape(c)}">'
        f'<span>{html.escape(c)}</span>'
        f'<em>{sum(1 for it in cats[c]["items"] if it["status"] == "open")}</em></button>'
        for c in order)
    legend = "".join(f'<div><dt class="pill {p}">{p}</dt><dd>{PRIORITY_MEANING[p]}</dd></div>'
                     for p in PRIORITIES if totals[p])

    OUT.write_text(TEMPLATE.format(
        matrix="".join(rows), blocks="".join(blocks), filters=filters,
        cat_filters=cat_filters, legend=legend, total=len(open_items),
        closed=closed_total, p0=totals["P0"], p1=totals["P1"], untriaged=totals["none"],
        packs=len({it["wp"] for it in open_items})), encoding="utf-8")
    print(f"wrote {OUT} ({OUT.stat().st_size // 1024} KB, "
          f"{len(open_items)} open, {closed_total} closed)")


TEMPLATE = """<title>GHCAA Open Work</title>
<link rel="stylesheet" href="https://fonts.googleapis.com/css2?family=IBM+Plex+Sans:ital,wght@0,400;0,500;0,600;1,400&family=IBM+Plex+Mono:wght@400;500&display=swap">
<style>
:root {{
  --paper:#f6f7f9; --surface:#fff; --sunk:#eef1f5; --line:#d9e0e8; --line-soft:#e8edf3;
  --ink:#111820; --ink-2:#3d4956; --ink-3:#6b7887;
  --accent:#2f5fa8; --accent-soft:#e7eefa;
  --p0:#b3261e; --p1:#a8541b; --p2:#7d661a; --p3:#3d6b53; --p4:#4e6377; --pn:#7b8794;
  --p0-bg:#fbe9e7; --p1-bg:#fbeee2; --p2-bg:#f8f2dd; --p3-bg:#e6f1ea; --p4-bg:#eaeff4; --pn-bg:#eef1f4;
  --radius:7px;
}}
@media (prefers-color-scheme: dark) {{
  :root:not([data-theme="light"]) {{
    --paper:#0e1319; --surface:#161d25; --sunk:#111820; --line:#2a343f; --line-soft:#212a34;
    --ink:#e7edf4; --ink-2:#b3c0cd; --ink-3:#7f8d9c;
    --accent:#7aa8e8; --accent-soft:#1b2836;
    --p0:#f28b82; --p1:#e8a76b; --p2:#d9c165; --p3:#84c9a3; --p4:#9db3c7; --pn:#8b98a6;
    --p0-bg:#2c1a19; --p1-bg:#2a2118; --p2-bg:#282516; --p3-bg:#17271f; --p4-bg:#1a222b; --pn-bg:#1a212a;
  }}
}}
:root[data-theme="dark"] {{
  --paper:#0e1319; --surface:#161d25; --sunk:#111820; --line:#2a343f; --line-soft:#212a34;
  --ink:#e7edf4; --ink-2:#b3c0cd; --ink-3:#7f8d9c;
  --accent:#7aa8e8; --accent-soft:#1b2836;
  --p0:#f28b82; --p1:#e8a76b; --p2:#d9c165; --p3:#84c9a3; --p4:#9db3c7; --pn:#8b98a6;
  --p0-bg:#2c1a19; --p1-bg:#2a2118; --p2-bg:#282516; --p3-bg:#17271f; --p4-bg:#1a222b; --pn-bg:#1a212a;
}}
* {{ box-sizing:border-box; }}
body {{ margin:0; background:var(--paper); color:var(--ink);
  font:400 15px/1.55 "IBM Plex Sans","Segoe UI",system-ui,sans-serif;
  -webkit-font-smoothing:antialiased; }}
.wrap {{ max-width:1120px; margin:0 auto; padding:40px 24px 96px; }}

header.top {{ border-bottom:2px solid var(--ink); padding-bottom:20px; margin-bottom:28px; }}
header.top h1 {{ margin:0 0 6px; font-size:30px; font-weight:600; letter-spacing:-.02em;
  text-wrap:balance; }}
header.top .sub {{ margin:0; color:var(--ink-3); max-width:62ch; }}
.stats {{ display:flex; flex-wrap:wrap; gap:28px; margin-top:20px; }}
.stats div {{ display:flex; flex-direction:column; gap:2px; }}
.stats b {{ font:500 26px/1 "IBM Plex Mono",ui-monospace,monospace; font-variant-numeric:tabular-nums; }}
.stats span {{ font-size:11px; letter-spacing:.08em; text-transform:uppercase; color:var(--ink-3); }}

.matrix {{ overflow-x:auto; margin:0 0 28px; }}
table {{ border-collapse:collapse; width:100%; font-variant-numeric:tabular-nums; }}
th,td {{ padding:7px 10px; text-align:left; border-bottom:1px solid var(--line-soft); }}
thead th {{ font-size:11px; letter-spacing:.08em; text-transform:uppercase; color:var(--ink-3);
  font-weight:500; border-bottom:1px solid var(--line); }}
td.n, thead th.n {{ text-align:right; font-family:"IBM Plex Mono",ui-monospace,monospace; }}
td.z {{ color:var(--ink-3); opacity:.4; }}
td.tot {{ font-weight:500; }}
tr.sum th, tr.sum td {{ border-top:1px solid var(--ink); border-bottom:none; font-weight:600; }}
tbody th {{ font-weight:400; }}

.controls {{ position:sticky; top:0; z-index:5; background:var(--paper);
  padding:12px 0; margin-bottom:24px; border-bottom:1px solid var(--line);
  display:flex; flex-wrap:wrap; gap:8px; align-items:center; }}
.chip {{ display:inline-flex; align-items:baseline; gap:6px; padding:4px 10px; cursor:pointer;
  border:1px solid var(--line); border-radius:99px; background:var(--surface); color:var(--ink-2);
  font:inherit; font-size:13px; }}
.chip em {{ font:500 11px/1 "IBM Plex Mono",monospace; font-style:normal; color:var(--ink-3); }}
.chip:hover {{ border-color:var(--ink-3); }}
.chip[aria-pressed="true"] {{ background:var(--accent-soft); border-color:var(--accent); color:var(--ink); }}
.chip[aria-pressed="true"] em {{ color:var(--accent); }}
.chip.P0[aria-pressed="true"] {{ background:var(--p0-bg); border-color:var(--p0); }}
.chip.P1[aria-pressed="true"] {{ background:var(--p1-bg); border-color:var(--p1); }}
.chip.P2[aria-pressed="true"] {{ background:var(--p2-bg); border-color:var(--p2); }}
.chip.P3[aria-pressed="true"] {{ background:var(--p3-bg); border-color:var(--p3); }}
.chip.P4[aria-pressed="true"] {{ background:var(--p4-bg); border-color:var(--p4); }}
.chip.none[aria-pressed="true"] {{ background:var(--pn-bg); border-color:var(--pn); }}
.sep {{ width:1px; align-self:stretch; background:var(--line); margin:0 4px; }}
.seg {{ display:inline-flex; border:1px solid var(--line); border-radius:99px; overflow:hidden; }}
.seg button {{ border:none; background:var(--surface); color:var(--ink-2); font:inherit; font-size:13px;
  padding:4px 12px; cursor:pointer; }}
.seg button + button {{ border-left:1px solid var(--line); }}
.seg button[aria-pressed="true"] {{ background:var(--accent-soft); color:var(--ink); font-weight:500; }}
input[type=search] {{ flex:1; min-width:180px; padding:5px 11px; border:1px solid var(--line);
  border-radius:99px; background:var(--surface); color:var(--ink); font:inherit; font-size:13px; }}
input[type=search]:focus-visible, .chip:focus-visible {{ outline:2px solid var(--accent); outline-offset:1px; }}
.clear {{ border:none; background:none; color:var(--accent); font:inherit; font-size:13px;
  cursor:pointer; padding:4px 6px; }}

dl.legend {{ display:flex; flex-wrap:wrap; gap:6px 20px; margin:0 0 32px; padding:14px 16px;
  background:var(--sunk); border-radius:var(--radius); }}
dl.legend > div {{ display:flex; align-items:center; gap:8px; }}
dl.legend dd {{ margin:0; font-size:13px; color:var(--ink-2); }}

.cathead {{ display:flex; align-items:baseline; gap:12px; margin:44px 0 4px;
  border-bottom:1px solid var(--line); padding-bottom:8px; }}
.cathead h2 {{ margin:0; font-size:20px; font-weight:600; letter-spacing:-.01em; }}
.cathead p {{ margin:0; color:var(--ink-3); font-size:13px; flex:1; }}
.cathead .count {{ font:500 13px/1 "IBM Plex Mono",monospace; color:var(--ink-3); }}

.pack {{ margin:22px 0 0; }}
.pack h3 {{ margin:0 0 8px; font-size:14px; font-weight:500; color:var(--ink-2);
  display:flex; gap:9px; align-items:baseline; }}
.wp {{ font:500 11px/1.4 "IBM Plex Mono",monospace; letter-spacing:.03em;
  background:var(--sunk); color:var(--ink-3); padding:2px 6px; border-radius:4px; }}

.item {{ display:grid; grid-template-columns:150px 1fr; gap:16px; padding:11px 0;
  border-top:1px solid var(--line-soft); }}
.item:first-of-type {{ border-top:1px solid var(--line); }}
.item p {{ margin:0; color:var(--ink-2); max-width:74ch; }}
.item code {{ font:400 .87em/1 "IBM Plex Mono",monospace; background:var(--sunk);
  padding:1px 4px; border-radius:3px; word-break:break-word; }}
.item strong {{ color:var(--ink); font-weight:600; }}
.meta {{ display:flex; flex-wrap:wrap; gap:6px; align-items:baseline; align-content:flex-start; }}
.pill {{ font:500 11px/1.5 "IBM Plex Mono",monospace; padding:1px 7px; border-radius:4px; }}
.pill.P0 {{ color:var(--p0); background:var(--p0-bg); }}
.pill.P1 {{ color:var(--p1); background:var(--p1-bg); }}
.pill.P2 {{ color:var(--p2); background:var(--p2-bg); }}
.pill.P3 {{ color:var(--p3); background:var(--p3-bg); }}
.pill.P4 {{ color:var(--p4); background:var(--p4-bg); }}
.pill.none {{ color:var(--pn); background:var(--pn-bg); }}
.id {{ font:500 12px/1.6 "IBM Plex Mono",monospace; color:var(--ink); }}
.dep, .state {{ font-size:11px; color:var(--ink-3); }}
.state {{ text-transform:uppercase; letter-spacing:.06em; }}
.empty {{ padding:40px 0; color:var(--ink-3); }}
[hidden] {{ display:none !important; }}
@media (max-width:640px) {{
  .item {{ grid-template-columns:1fr; gap:4px; }}
  .cathead {{ flex-wrap:wrap; }}
}}
@media (prefers-reduced-motion:reduce) {{ * {{ transition:none !important; }} }}
</style>

<div class="wrap">
<header class="top">
  <h1>GHCAA Open Work</h1>
  <p class="sub">Every item in <code>docs/TODO.md</code>, grouped by the work package that owns it.
  Shows open items by default; use the Open/Closed/All control to see what's done.
  Generated from the tracker file, so it cannot drift from it.</p>
  <div class="stats">
    <div><b>{total}</b><span>open items</span></div>
    <div><b>{closed}</b><span>closed items</span></div>
    <div><b>{packs}</b><span>work packages</span></div>
    <div><b>{p0}</b><span>P0 blocking</span></div>
    <div><b>{p1}</b><span>P1 next</span></div>
    <div><b>{untriaged}</b><span>never triaged</span></div>
  </div>
</header>

<div class="matrix">
<table>
<thead><tr><th scope="col">Category</th><th scope="col" class="n">P0</th><th scope="col" class="n">P1</th>
<th scope="col" class="n">P2</th><th scope="col" class="n">P3</th><th scope="col" class="n">P4</th>
<th scope="col" class="n">none</th><th scope="col" class="n">Total</th></tr></thead>
<tbody>{matrix}</tbody>
</table>
</div>

<dl class="legend">{legend}</dl>

<div class="controls">
  <div class="seg" id="status" role="group" aria-label="Status">
    <button data-status="open" aria-pressed="true" type="button">Open</button>
    <button data-status="todo" aria-pressed="false" type="button">Todo</button>
    <button data-status="partial" aria-pressed="false" type="button">Partial</button>
    <button data-status="onhold" aria-pressed="false" type="button">On Hold</button>
    <button data-status="blocked" aria-pressed="false" type="button">Blocked</button>
    <button data-status="closed" aria-pressed="false" type="button">Closed</button>
    <button data-status="all" aria-pressed="false" type="button">All</button>
  </div>
  <span class="sep"></span>
  {filters}<span class="sep"></span>{cat_filters}
  <input type="search" id="q" placeholder="Search item text or number" aria-label="Search items">
  <button class="clear" id="clear" type="button">Reset</button>
</div>

<div id="list">{blocks}</div>
<p class="empty" id="none" hidden>No item matches those filters.</p>
</div>

<script>
(function () {{
  var pr = new Set(), cat = new Set(), q = "", status = "open";
  var items = Array.prototype.slice.call(document.querySelectorAll(".item"));
  var chips = Array.prototype.slice.call(document.querySelectorAll(".chip"));
  var statusButtons = Array.prototype.slice.call(document.querySelectorAll("#status button"));

  // "open"/"closed"/"all" match the coarse open-vs-done split; any other value
  // (todo/partial/onhold/blocked) matches the item's exact state word instead,
  // so a PARTIAL or ONHOLD item can be found without wading through every open item.
  function statusOk(el) {{
    if (status === "all") return true;
    if (status === "open" || status === "closed") return el.dataset.status === status;
    return el.dataset.state === status;
  }}

  function apply() {{
    var shown = 0;
    items.forEach(function (el) {{
      var ok = statusOk(el)
        && (!pr.size || pr.has(el.dataset.pr))
        && (!cat.size || cat.has(el.dataset.cat))
        && (!q || el.dataset.find.indexOf(q) > -1);
      el.hidden = !ok;
      if (ok) shown++;
    }});
    document.querySelectorAll(".pack").forEach(function (p) {{
      p.hidden = !p.querySelector(".item:not([hidden])");
    }});
    document.querySelectorAll(".cat").forEach(function (c) {{
      c.hidden = !c.querySelector(".pack:not([hidden])");
    }});
    document.getElementById("none").hidden = shown > 0;
  }}

  statusButtons.forEach(function (b) {{
    b.addEventListener("click", function () {{
      status = b.dataset.status;
      statusButtons.forEach(function (o) {{ o.setAttribute("aria-pressed", String(o === b)); }});
      apply();
    }});
  }});
  chips.forEach(function (c) {{
    c.setAttribute("aria-pressed", "false");
    c.addEventListener("click", function () {{
      var set = c.dataset.filterPr ? pr : cat;
      var key = c.dataset.filterPr || c.dataset.filterCat;
      if (set.has(key)) {{ set.delete(key); c.setAttribute("aria-pressed", "false"); }}
      else {{ set.add(key); c.setAttribute("aria-pressed", "true"); }}
      apply();
    }});
  }});
  document.getElementById("q").addEventListener("input", function (e) {{
    q = e.target.value.toLowerCase().trim(); apply();
  }});
  document.getElementById("clear").addEventListener("click", function () {{
    pr.clear(); cat.clear(); q = ""; status = "open";
    document.getElementById("q").value = "";
    chips.forEach(function (c) {{ c.setAttribute("aria-pressed", "false"); }});
    statusButtons.forEach(function (o) {{ o.setAttribute("aria-pressed", String(o.dataset.status === "open")); }});
    apply();
  }});
  apply();
}}());
</script>
"""

main()
