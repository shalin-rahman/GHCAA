"""Find the sentences worth a human look, so a sweep does not mean reading
every line.

Two checks, both stdlib only.

    refs   every section reference resolves, and carries a clause naming what
           is at the destination (SR-5). A wrong reference passes build.py
           --strict today, because that check only verifies figures and tables
           are named, not that a section number exists.

    prose  sentences that are likely to need a second reading (SR-1). This is a
           filter, not a judge: it narrows 1,100 lines to about 40 for a person
           to rule on, and it will flag sentences that turn out to be fine.

    python prose.py refs                  every chapter
    python prose.py prose 02 03           named chapters only
    python prose.py both 02
"""
import re
import sys
from pathlib import Path

BOOK = Path(__file__).resolve().parent.parent
CHAPTERS = sorted(p for p in BOOK.glob("*.md") if re.match(r"^\d\d-", p.name))

HEAD = re.compile(r"^#{2,4} (\d+(?:\.\d+)*) +(.+?)\s*$", re.M)
REF = re.compile(r"(?:§|Section |Sections |Sec\. )(\d+\.\d+(?:\.\d+)?)")
FENCE = re.compile(r"```.*?```", re.S)

STOP = set("the a an of and or to in on for with by as is are was were be been "
           "that this it its at from into which what who whom whose not no its "
           "their his her they them then than so if when where how why all any "
           "each both more most other some such only own same too very can will "
           "just should now here there".split())

# Constructions that reliably take two readings. Each is a hint, never a verdict.
SMELLS = [
    (re.compile(r"\bwhat (?:licen[sc]es|permits|warrants)\b", re.I), "abstract verb"),
    (re.compile(r"\bthe (?:inversion|corollary|upshot|yield|purchase) (?:is|of)\b", re.I),
     "abstract noun doing a verb's work"),
    (re.compile(r"\b(?:practical|real|net) (?:yield|purchase|upshot)\b", re.I), "abstract noun"),
    (re.compile(r"\bthe antecedent of\b", re.I), "abstract noun"),
    (re.compile(r"\bis (?:constitutive|dispositive|determinative) of\b", re.I), "abstract"),
    (re.compile(r"\b(?:that|which) is to say\b", re.I), "filler"),
    (re.compile(r"\bnot merely\b.*?\bbut\b", re.I), "rhetorical pairing"),
    (re.compile(r"\bis (?:precisely|exactly) (?:what|the)\b", re.I), "emphasis doing work"),
    (re.compile(r"\bin (?:the|this) sense that\b", re.I), "needs unpacking"),
    (re.compile(r"\b\w+ion of the \w+ion\b", re.I), "stacked nominalisation"),
    (re.compile(r"\bby way of\b", re.I), "wordy"),
    (re.compile(r"\bwith respect to\b|\bin terms of\b|\bin relation to\b", re.I), "wordy"),
    (re.compile(r"\bit (?:is|was) (?:the case|worth noting|notable) that\b", re.I), "filler"),
    (re.compile(r"\bthere (?:is|are|exists?) (?:a|an|no|the) \w+ (?:which|that)\b", re.I),
     "existential opener"),
]

LONG_WORDS = 45
VERY_LONG = 60


def sections():
    """number -> (chapter file, title)"""
    out = {}
    for p in CHAPTERS:
        for num, title in HEAD.findall(p.read_text(encoding="utf-8")):
            out[num] = (p.name, title)
    return out


def content_words(s):
    """Content words, crudely stemmed to six characters so that "architecture"
    in a sentence counts as naming a section titled "Architectural"."""
    return {w[:6] for w in re.findall(r"[a-z]+", s.lower())
            if w not in STOP and len(w) > 3}


def check_refs(files, index):
    broken, bare, tabular = [], [], []
    for p in files:
        text = p.read_text(encoding="utf-8")
        in_appendix = False
        for lineno, line in enumerate(text.split("\n"), 1):
            if line.startswith("## Figures and Tables"):
                in_appendix = True
            # A table cell and a traceability row are the right place for a bare
            # number, so SR-5 does not apply there. They are still listed, with
            # the destination title, because a reference can resolve and still
            # point at the wrong section and only a reader catches that.
            cell = in_appendix or line.lstrip().startswith("|")
            for m in REF.finditer(line):
                num = m.group(1)
                if num not in index:
                    broken.append((p.name, lineno, num, line.strip()[:80]))
                    continue
                title = index[num][1]
                if cell:
                    tabular.append((p.name, lineno, num, title))
                    continue
                # Named if the surrounding window shares a content word with the
                # destination's own heading.
                window = line[max(0, m.start() - 130):m.end() + 130]
                if not (content_words(title) & content_words(window)):
                    bare.append((p.name, lineno, num, title, window.strip()))
    return broken, bare, tabular


def sentences(text):
    text = FENCE.sub("", text)
    out = []
    for block in text.split("\n\n"):
        first = block.lstrip()[:2]
        if first[:1] in "#|>-*" or block.lstrip().startswith("<!--"):
            continue
        flat = " ".join(l.strip() for l in block.split("\n"))
        # A sentence may open with a code span, a quote or a bracket, so the
        # split cannot require a capital letter immediately after the space.
        for s in re.split(r"(?<=[.!?]) +(?=[`\"“(\[*]*[A-ZÀ-Ý])", flat):
            s = s.strip()
            if len(s.split()) > 4:
                out.append(s)
    return out


def check_prose(files):
    flags = []
    for p in files:
        text = p.read_text(encoding="utf-8")
        lines = text.split("\n")
        for s in sentences(text):
            reasons = []
            n = len(s.split())
            if n >= VERY_LONG:
                reasons.append(f"{n}w")
            elif n >= LONG_WORDS and s.count(",") >= 4:
                reasons.append(f"{n}w, {s.count(',')} commas")
            for pat, why in SMELLS:
                if pat.search(s):
                    reasons.append(why)
            if not reasons:
                continue
            head = s.split()[0:6]
            lineno = next((i for i, l in enumerate(lines, 1)
                           if " ".join(head) in " ".join(lines[i - 1:i + 2]).replace("  ", " ")), 0)
            flags.append((p.name, lineno, "; ".join(sorted(set(reasons))), s))
    return flags


def pick(args):
    if not args:
        return CHAPTERS
    return [p for p in CHAPTERS if any(p.name.startswith(a) for a in args)]


def main():
    try:
        sys.stdout.reconfigure(encoding="utf-8")
    except Exception:
        pass
    mode = sys.argv[1] if len(sys.argv) > 1 else "both"
    files = pick(sys.argv[2:])
    index = sections()

    if mode in ("refs", "both"):
        broken, bare, tabular = check_refs(files, index)
        print(f"== references: {len(broken)} broken, {len(bare)} bare in prose, "
              f"{len(tabular)} in tables (index holds {len(index)} sections)")
        for name, ln, num, ctx in broken:
            print(f"  BROKEN  {name}:{ln}  §{num} does not exist | {ctx}")
        for name, ln, num, title, ctx in bare:
            print(f"  bare    {name}:{ln}  §{num} -> {title}")
            print(f"          ...{ctx}...")
        if "--tables" in sys.argv:
            for name, ln, num, title in tabular:
                print(f"  cell    {name}:{ln}  §{num} -> {title}")

    if mode in ("prose", "both"):
        flags = check_prose(files)
        print(f"\n== prose candidates: {len(flags)}")
        for name, ln, why, s in flags:
            print(f"  {name}:{ln}  [{why}]")
            print(f"      {s[:150]}")


main()
