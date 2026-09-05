"""Checks on the book sources: house tone, IEEE numbering, front-matter lists.

Run by build.py on every build. Failures print a file and line so they can be
fixed at source. Nothing here rewrites the Markdown; it only reports.

Three groups of check:

  tone       words and openers the house style bans, mostly the vocabulary that
             makes a paragraph read as generated rather than written
  numbering  figures and tables numbered in sequence per chapter, and named in
             the body before the artefact appears (IEEE)
  lists      every figure and table present in the List of Figures / Tables
"""

import io
import os
import re

# Words and phrases the house style rules out. The pattern is matched against
# prose only: headings, tables, code fences and the reference list are skipped,
# so a cited title containing one of these words does not trip the check.
BANNED = [
    (r"\bleverag(e|es|ed|ing)\b", "use, or name the thing being used"),
    (r"\brobust(ly|ness)?\b", "say what it withstands"),
    (r"\bseamless(ly)?\b", "say what the user does not have to do"),
    (r"\bcomprehensive(ly)?\b", "give the count"),
    (r"\bdelv(e|es|ed|ing)\b", "examine, or just do it"),
    (r"\bit is important to note\b", "delete the opener, keep the sentence"),
    (r"\bit is worth noting\b", "delete the opener, keep the sentence"),
    (r"\bit should be noted\b", "delete the opener, keep the sentence"),
    (r"\bplays? an? (crucial|vital|pivotal|key|important) role\b", "say what it does"),
    (r"\bcutting[- ]edge\b", "name the version or the technique"),
    (r"\bstate[- ]of[- ]the[- ]art\b", "name the baseline being compared against"),
    (r"\bholistic(ally)?\b", "say which parts"),
    (r"\bstreamlin(e|es|ed|ing)\b", "say what step was removed"),
    (r"\bempower(s|ed|ing)?\b", "say what the user can now do"),
    (r"\bunlock(s|ed|ing)?\b", "say what became possible"),
    (r"\bgame[- ]chang(er|ing)\b", "state the measured difference"),
    (r"\ba testament to\b", "state the evidence"),
    (r"\bever[- ]evolving\b", "give the dates"),
    (r"\bin the realm of\b", "in"),
    (r"\btapestry\b", "drop it"),
    (r"\bunderscor(e|es|ed|ing)\b", "shows, or drop the sentence"),
    (r"\bshowcas(e|es|ed|ing)\b", "shows"),
    (r"\bmyriad\b", "give the number"),
    (r"\bplethora\b", "give the number"),
    (r"\bmeticulous(ly)?\b", "say what was checked"),
    (r"\bharness(ing)? the power\b", "use"),
    (r"\bdiv(e|es|ed|ing) into\b", "examine"),
    (r"\bnavigat(e|es|ed|ing) the (complexit|challeng|landscap)", "say what was difficult"),
    (r"\bin (today|todays|today's) (world|landscape|environment)\b", "drop the opener"),
    (r"\bas we can see\b", "drop it, the reader can see"),
    (r"\bfirst and foremost\b", "first"),
    (r"\bwhen it comes to\b", "for"),
    (r"\bthe (software|technology|digital) landscape\b", "name the systems"),
    (r"^\s*(Moreover|Furthermore|Additionally)\b", "start with the substance"),
    (r"\bin conclusion\b", "the section's last paragraph needs no announcement"),
    (r"\bthis ensures\b", "say what it prevents, and what still gets through"),
    (r"\ba wide range of\b", "give the range"),
    (r"\butilis(e|es|ed|ing)\b", "use"),
    (r"\butiliz(e|es|ed|ing)\b", "use"),
    (r"\bparadigm shift\b", "say what changed"),
    (r"\bsynerg(y|ies|istic)\b", "say which two things and what they do together"),
    (r"\bdeep dive\b", "examine"),
    (r"\bmultifaceted\b", "name the facets"),
    (r"\bit is clear that\b", "if it is clear, drop the clause"),
    (r"\bsignificantly\b(?!\s+different)", "give the figure, or say 'statistically significant'"),
]

BANNED_RE = [(re.compile(pat, re.I), pat, hint) for pat, hint in BANNED]

# Captions the List of Figures / Tables names on purpose, where the material is
# set as prose or lives in the section the entry points to rather than as a
# table directly beneath the caption. Anything else on the orphan line is a
# caption that has drifted away from its artefact.
ALLOWED_ORPHANS = {"Table 3.1", "Table 3.2", "Table 3.3", "Table 3.5", "Table 3.6"}

CAPTION = re.compile(r"^#{1,6}\s+(Figure|Table)\s+(\d+)\.(\d+)\s*[—–-]\s*(.+?)\s*(\{landscape\})?\s*$")
# A mention names one artefact, a list of them, or a range: "Figure 6.3",
# "Tables 3.1, 3.2 and 3.5", "Figures 6.3 to 6.6".
REFERENCE = re.compile(
    r"\b(Figures?|Tables?|Figs?\.)\s+(\d+\.\d+(?:\s*(?:,|and|to|–|-)\s*\d+\.\d+)*)")
LABEL = re.compile(r"\d+\.\d+")
RANGE = re.compile(r"(\d+)\.(\d+)\s*(?:to|–)\s*(\d+)\.(\d+)")
LOF_ROW = re.compile(r"^\|\s*(\d+\.\d+)\s*\|", re.M)

# The reference list is titles and journal names, not the author's prose.
SKIP_TONE_FILES = {"99-references.md"}


def _prose_lines(path):
    """Yield (line number, text) for prose only: no fences, tables, headings."""
    with io.open(path, encoding="utf-8") as fh:
        lines = fh.readlines()
    in_fence = False
    for number, raw in enumerate(lines, 1):
        text = raw.rstrip("\n")
        stripped = text.strip()
        if stripped.startswith("```"):
            in_fence = not in_fence
            continue
        if in_fence or not stripped:
            continue
        if stripped.startswith(("#", "|")):
            continue
        if stripped.startswith(">"):
            # Block quotes here are user stories and acceptance criteria, which
            # the author wrote, so the tone rules apply to them.
            text = stripped.lstrip("> ")
        yield number, text


def tone(paths):
    problems = []
    for path in paths:
        if os.path.basename(path) in SKIP_TONE_FILES:
            continue
        for number, text in _prose_lines(path):
            for pattern, source, hint in BANNED_RE:
                found = pattern.search(text)
                if found:
                    problems.append((path, number, found.group(0), hint))
    return problems


def captions(paths):
    """Every caption in bound order: (path, line, kind, chapter, index, text, landscape)."""
    found = []
    for path in paths:
        with io.open(path, encoding="utf-8") as fh:
            in_fence = False
            for number, raw in enumerate(fh, 1):
                stripped = raw.strip()
                if stripped.startswith("```"):
                    in_fence = not in_fence
                    continue
                if in_fence:
                    continue
                match = CAPTION.match(stripped)
                if match:
                    found.append((path, number, match.group(1), int(match.group(2)),
                                  int(match.group(3)), match.group(4), bool(match.group(5))))
    return found


def numbering(caps):
    """Gaps and repeats in per-chapter figure and table numbering."""
    problems = []
    seen = {}
    for path, number, kind, chapter, index, _text, _ls in caps:
        key = (kind, chapter)
        seen.setdefault(key, []).append((index, path, number))
    for (kind, chapter), entries in sorted(seen.items()):
        indices = [e[0] for e in entries]
        duplicates = {i for i in indices if indices.count(i) > 1}
        for index in sorted(duplicates):
            path, number = next((p, n) for i, p, n in entries if i == index)
            problems.append((path, number, "%s %d.%d is used twice" % (kind, chapter, index)))
        expected = set(range(1, max(indices) + 1))
        missing = sorted(expected - set(indices))
        if missing:
            path = entries[0][1]
            problems.append((path, entries[0][2],
                             "%s numbering in chapter %d skips %s"
                             % (kind, chapter, ", ".join("%d.%d" % (chapter, m) for m in missing))))
    return problems


def _mentions(lines):
    """(kind, label) -> line number of the first mention in the body.

    Caption headings are skipped: a caption is the artefact, not a mention of
    it. A range such as "Figures 6.3 to 6.6" counts as naming each figure in
    the span, which is how IEEE numbering is normally cited.
    """
    first = {}
    in_fence = False
    for number, raw in enumerate(lines, 1):
        stripped = raw.strip()
        # A figure named inside another figure's Mermaid source is not the body
        # naming it, and counting it would satisfy the forward-reference rule
        # with no prose anywhere.
        if stripped.startswith("```"):
            in_fence = not in_fence
            continue
        if in_fence:
            continue
        if CAPTION.match(stripped):
            continue
        for match in REFERENCE.finditer(raw):
            kind = "Table" if match.group(1).lower().startswith("table") else "Figure"
            body = match.group(2)
            labels = set(LABEL.findall(body))
            for span in RANGE.finditer(body):
                chapter, start, chapter2, end = (int(span.group(1)), int(span.group(2)),
                                                 int(span.group(3)), int(span.group(4)))
                if chapter == chapter2 and end >= start:
                    labels.update("%d.%d" % (chapter, i) for i in range(start, end + 1))
            for label in labels:
                first.setdefault((kind, label), number)
    return first


def forward_references(paths, caps):
    """IEEE: an artefact is named in the body before the reader reaches it."""
    problems = []
    by_path = {}
    for path, number, kind, chapter, index, _t, _l in caps:
        by_path.setdefault(path, []).append((number, kind, "%d.%d" % (chapter, index)))
    for path, entries in by_path.items():
        with io.open(path, encoding="utf-8") as fh:
            lines = fh.readlines()
        mentioned_before = _mentions(lines)
        for caption_line, kind, label in entries:
            word = "Figure" if kind == "Figure" else "Table"
            first = mentioned_before.get((word, label))
            mentioned = first is not None and first < caption_line
            if not mentioned:
                problems.append((path, caption_line,
                                 "%s %s appears before the body refers to it" % (word, label)))
    return problems


def front_matter_lists(front_path, caps):
    """The lists and the captions must name the same artefacts, both ways round.

    Checking only that a caption is listed leaves the reverse open: a figure
    that is deleted or renumbered leaves a row behind in the List of Figures,
    with a page number that now points at something else.
    """
    if not os.path.exists(front_path):
        return []
    with io.open(front_path, encoding="utf-8") as fh:
        text = fh.read()
    sections = {}
    for name, marker in (("Figure", "List of Figures"), ("Table", "List of Tables")):
        # The heading itself, not the Table of Contents entry that names it first.
        heading = re.search(r"^##\s+.*%s\s*$" % re.escape(marker), text, re.M)
        if not heading:
            continue
        start = heading.end()
        end = text.find("\n## ", start)
        block = text[start:end if end > 0 else len(text)]
        sections[name] = {m.group(1) for m in LOF_ROW.finditer(block)}
    problems = []
    captioned = {"Figure": set(), "Table": set()}
    for path, number, kind, chapter, index, _t, _l in caps:
        if os.path.abspath(path) == os.path.abspath(front_path):
            continue
        label = "%d.%d" % (chapter, index)
        captioned.setdefault(kind, set()).add(label)
        listed = sections.get(kind, set())
        if label not in listed:
            problems.append((path, number,
                             "%s %s is not in the List of %ss" % (kind, label, kind)))
    for kind, listed in sections.items():
        for label in sorted(listed - captioned.get(kind, set())):
            problems.append((front_path, 1,
                             "the List of %ss has a row for %s %s and no such caption exists; "
                             "run renumber.py --lists --apply" % (kind, kind, label)))
    return problems


ABSTRACT = re.compile(r"##\s+[iv]+\.\s+Abstract\s*\n(.*?)\n\*\*Word count:\*\*\s*([\d,]+)", re.S)
WORD = re.compile(r"[A-Za-z0-9]")


def abstract_word_count(front_path):
    """The abstract states its own length; check the statement against the text.

    Counted the way an examiner would: hyphenated compounds are one word, code
    ticks and markdown emphasis do not count, and a tolerance of five words
    covers the difference between one counting convention and another.
    """
    if not os.path.exists(front_path):
        return []
    text = io.open(front_path, encoding="utf-8").read()
    match = ABSTRACT.search(text)
    if not match:
        return [(front_path, 1, "the Abstract heading or its Word count line no longer matches "
                                "the pattern, so the length was not checked")]
    body = re.sub(r"[`*]", "", match.group(1))
    words = len([w for w in body.split() if WORD.search(w)])
    stated = int(match.group(2).replace(",", ""))
    if abs(words - stated) > 5:
        line = text[:match.end()].count("\n") + 1
        return [(front_path, line,
                 "the abstract states %d words and runs to %d" % (stated, words))]
    return []


_ONES = {"zero": 0, "one": 1, "two": 2, "three": 3, "four": 4, "five": 5, "six": 6, "seven": 7,
         "eight": 8, "nine": 9, "ten": 10, "eleven": 11, "twelve": 12, "thirteen": 13,
         "fourteen": 14, "fifteen": 15, "sixteen": 16, "seventeen": 17, "eighteen": 18,
         "nineteen": 19}
_TENS = {"twenty": 20, "thirty": 30, "forty": 40, "fifty": 50, "sixty": 60, "seventy": 70,
         "eighty": 80, "ninety": 90}


def _words_to_int(phrase):
    """Parse a spelled-out English integer up to a few thousand, or return None.

    Only needs to cover how this book actually phrases a repository count:
    "eighty-two", "two hundred and thirty-five". Not a general parser.
    """
    words = re.split(r"[\s-]+", phrase.lower().strip())
    words = [w for w in words if w and w != "and"]
    if not words:
        return None
    total = 0
    current = 0
    for w in words:
        if w in _ONES:
            current += _ONES[w]
        elif w in _TENS:
            current += _TENS[w]
        elif w == "hundred":
            current = (current or 1) * 100
        elif w == "thousand":
            total += (current or 1) * 1000
            current = 0
        else:
            return None
    return total + current


_NUMBER_WORD = r"[A-Za-z]+(?:[\s-]+[A-Za-z]+){0,4}"
REPO_COUNT_PATTERNS = [
    (re.compile(r"(?:\b(\d[\d,]*)\b|\b(" + _NUMBER_WORD + r")\b)\s+commits\b", re.I),
     "commit", lambda: _git_commit_count()),
    (re.compile(r"(?:\b(\d[\d,]*)\b|\b(" + _NUMBER_WORD + r")\b)\s+work packages\b", re.I),
     "work package", lambda: _todo_work_package_count()),
]


def _git_commit_count():
    import subprocess
    try:
        out = subprocess.run(["git", "rev-list", "--count", "HEAD"], capture_output=True,
                              text=True, check=True, cwd=os.path.dirname(os.path.abspath(__file__)))
        return int(out.stdout.strip())
    except Exception:
        return None


def _todo_work_package_count():
    """The highest top-level number `docs/TODO.md` has reached — matching how the

    chapters phrase it ("eighty-two work packages" = numbering reached 82), not
    a count of distinct numbers in use, since renumbering can leave gaps.
    """
    todo_path = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..", "TODO.md")
    if not os.path.exists(todo_path):
        return None
    text = io.open(todo_path, encoding="utf-8").read()
    numbers = [int(n) for n in re.findall(r"(?m)^(\d+)\.\d+[a-zA-Z]?\s*\[", text)]
    return max(numbers) if numbers else None


def repository_counts(paths):
    """A quoted repository count (commits, work packages), digits or words,

    checked against what the tree gives right now. Repository numbers drift
    every time work is added; this is what makes the drift a build failure
    instead of a defect an external reader has to find, per the repository-
    numbers rule in CLAUDE.md.
    """
    problems = []
    live = {}
    for path in paths:
        for number, text in _prose_lines(path):
            for pattern, kind, get_live in REPO_COUNT_PATTERNS:
                for match in pattern.finditer(text):
                    digit_group, word_group = match.group(1), match.group(2)
                    if digit_group:
                        quoted = int(digit_group.replace(",", ""))
                    else:
                        quoted = _words_to_int(word_group)
                        if quoted is None:
                            continue
                    if kind not in live:
                        live[kind] = get_live()
                    actual = live[kind]
                    if actual is None:
                        continue
                    if quoted != actual:
                        problems.append((path, number,
                                          "quotes %d %ss; the tree gives %d" % (quoted, kind, actual)))
    return problems


REF_ENTRY = re.compile(r"^\[(\d+)\]", re.M)
CITATION = re.compile(r"\[(\d+)\]")
FENCE = re.compile(r"```.*?```", re.S)


def references(paths, refs_path):
    """Citations with no entry, and entries nothing cites yet.

    A marker with no entry is a defect at any time. An entry nothing cites is
    expected while the book is part-written — the numbering is book-wide and
    assigned in order of first appearance, so entries first cited in Part III
    are already in the list — and becomes a defect only for a finished copy.
    """
    if not os.path.exists(refs_path):
        # Silence here would turn citation checking off for good the day the
        # reference file is renamed.
        return [(refs_path or "(no 99- file)", 1,
                 "no reference list found, so no citation was checked")], []
    defined = set(int(m.group(1)) for m in REF_ENTRY.finditer(
        io.open(refs_path, encoding="utf-8").read()))
    cited, where = set(), {}
    for path in paths:
        if os.path.abspath(path) == os.path.abspath(refs_path):
            continue
        # Replacing the fence with its own newlines keeps every later line at
        # the number it has in the file.
        text = FENCE.sub(lambda m: "\n" * m.group(0).count("\n"),
                         io.open(path, encoding="utf-8").read())
        for number, line in enumerate(text.splitlines(), 1):
            for match in CITATION.finditer(line):
                value = int(match.group(1))
                cited.add(value)
                where.setdefault(value, (path, number))
    dangling = [(where[n][0], where[n][1], "citation [%d] has no entry in the reference list" % n)
                for n in sorted(cited - defined)]
    uncited = sorted(defined - cited)
    return dangling, uncited


PLACEHOLDER = re.compile(r"\*\[")


def placeholders(paths):
    """Bracketed author placeholders still open in the sources.

    Matched anywhere in a line, not only at the start of a paragraph: the
    submission date sat inside a sentence and a requirement's source sat inside
    a table cell, and both went unreported while the check only looked at the
    first two characters of a paragraph.
    """
    open_items = []
    for path in paths:
        in_fence = False
        with io.open(path, encoding="utf-8") as fh:
            for number, raw in enumerate(fh, 1):
                if raw.strip().startswith("```"):
                    in_fence = not in_fence
                    continue
                if in_fence:
                    continue
                found = PLACEHOLDER.search(raw)
                if found:
                    text = raw.strip()
                    excerpt = text[max(0, found.start() - 20):][:100]
                    open_items.append((path, number, excerpt))
    return open_items


# docs/book/build/lint.py -> docs/DOCUMENTATION_BOOK_OUTLINE.md
OUTLINE = os.path.join(os.path.dirname(os.path.dirname(os.path.dirname(
    os.path.abspath(__file__)))), "DOCUMENTATION_BOOK_OUTLINE.md")

OUTLINE_CHAPTER = re.compile(r"^## Chapter (\d+) — (.+)$")
OUTLINE_SECTION = re.compile(r"^\s*- \*\*(\d+\.\d+(?:\.\d+)?)\*\*")
BODY_CHAPTER = re.compile(r"^# Chapter (\d+) — (.+)$")
BODY_SECTION = re.compile(r"^#{2,3} (\d+\.\d+(?:\.\d+)?) ")


def _outline(outline_path):
    """chapter -> (title, [section numbers]) as the outline promises them."""
    spec, chapter = {}, None
    for raw in io.open(outline_path, encoding="utf-8"):
        line = raw.rstrip("\n")
        head = OUTLINE_CHAPTER.match(line)
        if head:
            chapter = int(head.group(1))
            spec[chapter] = (head.group(2).strip(), [])
            continue
        if chapter and (line.startswith("# ") or line.startswith("## ")):
            chapter = None
            continue
        section = OUTLINE_SECTION.match(line)
        if chapter and section:
            spec[chapter][1].append(section.group(1))
    return spec


def outline_drift(paths, outline_path=None):
    """Sections the outline promises and the chapter does not carry, or the reverse.

    The outline is reviewed and approved before writing; the chapters are what
    gets bound. They are two views of one structure, so a section may not appear
    in one without the other, and a chapter may not be retitled in one alone.
    Both directions are checked, because drift has happened in both: a chapter
    swap that reached the prose but not the outline's own numbering, and a
    written section that the outline never learned about.
    """
    outline_path = outline_path or OUTLINE
    if not os.path.exists(outline_path):
        return [(outline_path, 0, "the outline is missing, so the structure cannot be checked")]
    spec = _outline(outline_path)
    findings = []
    for path in paths:
        chapter, present = None, []
        for number, raw in enumerate(io.open(path, encoding="utf-8"), 1):
            line = raw.rstrip("\n")
            head = BODY_CHAPTER.match(line)
            if head:
                chapter = int(head.group(1))
                promised = spec.get(chapter)
                if promised and promised[0] != head.group(2).strip():
                    findings.append((path, number, "chapter title is %r, the outline says %r"
                                     % (head.group(2).strip(), promised[0])))
                continue
            section = BODY_SECTION.match(line)
            if section:
                present.append((section.group(1), number))
        if chapter is None:
            continue
        if chapter not in spec:
            findings.append((path, 1, "Chapter %d is not in the outline" % chapter))
            continue
        promised = spec[chapter][1]
        seen = [n for n, _ in present]
        for number, line_no in present:
            if number not in promised:
                findings.append((path, line_no,
                                 "§%s is written but the outline does not list it" % number))
        for number in promised:
            if number not in seen:
                findings.append((path, 1,
                                 "the outline lists §%s and this chapter does not carry it" % number))
        # Same sections, different sequence: the outline is the approved reading
        # order, so a chapter that reorders its topics has to say so there too.
        if sorted(seen) == sorted(promised) and seen != promised:
            first = next(i for i in range(len(seen)) if seen[i] != promised[i])
            findings.append((path, 1, "sections are in a different order from the outline, "
                                      "first at §%s where the outline has §%s"
                                      % (seen[first], promised[first])))
    return findings


def run(paths, front_path):
    """All checks. Returns a dict of category -> list of findings."""
    caps = captions(paths)
    refs_path = next((p for p in paths if os.path.basename(p).startswith("99-")), "")
    dangling, uncited = references(paths, refs_path)
    return {
        "outline drift": outline_drift(paths),
        "citations": dangling,
        "uncited references": uncited,
        "tone": tone(paths),
        "abstract length": abstract_word_count(front_path),
        "numbering": numbering(caps),
        "forward references": forward_references(paths, caps),
        "front-matter lists": front_matter_lists(front_path, caps),
        "placeholders": placeholders(paths),
        "repository counts": repository_counts(paths),
        "captions": caps,
    }


def report(results, stream, no_placeholders=False, final=False):
    """Print the findings. Returns the number that count as failures.

    Two conditions sit outside the ordinary defect list, because a
    part-written book legitimately carries both and a finished one must not:

      no_placeholders  every author placeholder resolved
      final            every reference in the list cited somewhere in the text

    They are separate flags because they are reached at different times. The
    placeholders can all be closed while Part III is still unwritten, and the
    references numbered for Part III cannot be cited until it exists.
    """
    failures = 0
    for name in ("outline drift", "tone", "numbering", "forward references", "front-matter lists",
                 "abstract length", "citations", "repository counts"):
        items = results[name]
        if not items:
            continue
        failures += len(items)
        stream.write("  %s: %d\n" % (name, len(items)))
        for item in items:
            path, number = item[0], item[1]
            detail = ("%r — use %s" % (item[2], item[3])) if name == "tone" else item[2]
            stream.write("    %s:%d  %s\n" % (os.path.basename(path), number, detail))
    uncited = results["uncited references"]
    if uncited:
        stream.write("  references not yet cited (expected while Part III and IV are unwritten): "
                     + ", ".join("[%d]" % n for n in uncited) + "\n")
        if final:
            failures += len(uncited)

    open_items = results["placeholders"]
    if open_items:
        by_file = {}
        for path, number, text in open_items:
            by_file.setdefault(os.path.basename(path), []).append(number)
        stream.write("  placeholders still open: %d across %d files (%s)\n"
                     % (len(open_items), len(by_file),
                        ", ".join("%s %d" % (name, len(rows))
                                  for name, rows in sorted(by_file.items()))))
        # Listing every one only helps when they are what is being closed. A stub
        # chapter carries one per section, and 130 of them bury the real findings
        # printed above.
        if no_placeholders:
            for path, number, text in open_items:
                stream.write("    %s:%d  %s\n" % (os.path.basename(path), number, text))
            failures += len(open_items)
    return failures
