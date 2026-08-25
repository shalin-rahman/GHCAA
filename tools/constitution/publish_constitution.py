"""Publish a newly ratified constitution PDF as the active version.

The app must always serve the newest constitution with no hand-edited file paths left behind,
so this script is the single entry point: give it the PDF that was dropped into
`GHCAA.Web/public/assets/` and it

  1. extracts the document text in the exact plain-text shape `parseArticles` expects,
  2. rewrites `GHCAA.Infrastructure/Data/Seed/constitution.json` as a single active record, and
  3. repoints `CONSTITUTION_PDF_FALLBACK` in the Angular reader at the new asset.

Seeding only the new version is deliberate. `ConstitutionSeeder.SyncAsync` supersedes (never
deletes) a stored version that is missing from the seed, so the previous constitution stays in
the public Version History with its own `PdfUrl` -- which is why superseded PDFs must stay in
`public/assets/`.

Usage (from the repository root, with pymupdf installed):

    python tools/constitution/publish_constitution.py "GHCAA.Web/public/assets/GHCAA Constitution V4.2.pdf" \
        --summary-file docs/change-summary.txt

`--version` and `--effective` are read from the document's own closing line
("Document Version: 4.2 | Date of Ratification: 01.07.2026") unless given explicitly; the cover
page is NOT trusted, because exports of this document have shipped with a stale cover.

Run `dotnet test` and `npx vitest run` afterwards, then review the generated `Content` -- the
extractor is deterministic but the source documents are hand-authored and drift in layout.
"""
import argparse
import io
import json
import os
import re
import shutil
import sys

try:
    import fitz  # PyMuPDF
except ImportError:                                                  # pragma: no cover
    sys.exit("PyMuPDF is required: python -m pip install pymupdf")

REPO = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..'))
SEED = os.path.join(REPO, 'GHCAA.Infrastructure', 'Data', 'Seed', 'constitution.json')
READER = os.path.join(REPO, 'GHCAA.Web', 'src', 'app', 'public', 'constitution', 'constitution.ts')
ASSETS = os.path.join(REPO, 'GHCAA.Web', 'public', 'assets')

# Running heads, folios and the cover-page version stamp, all of which repeat on every page.
FURNITURE = re.compile(
    r'^(GOVT\. HARAGANGA COLLEGE ALUMNI ASSOCIATION|CONSTITUTION|HARAGANGIAN'
    r'|Page ?\d+|V ?\d+\.\d+)$', re.I)

# A block that opens one of these is structural: never fold it into the paragraph above it.
STRUCTURAL = re.compile(r'^(ARTICLE\s+[IVXL]+\s*:|Section\s+[0-9A-Z]+\s*:|●|\d{1,2}\.\s)')
CONT_STOP = re.compile(
    r'^(Article\s+[IVXL]+:|Section\s+[0-9A-Z]+:|Future Vision|- |\d{1,2}\. |Document Version)')
SECTION_RE = re.compile(r'^(Section\s+[0-9A-Z]+)\s*:\s*(.*)$', re.S)
COLOPHON = re.compile(
    r'Document Version:\s*([0-9.]+)\s*\|\s*Date of Ratification:\s*(\d{2})\.(\d{2})\.(\d{4})')

SMALL_WORDS = {'of', 'the', 'to', 'and', 'a', 'an', 'in', 'for', 'on'}
TERMINAL = '.:;!?_”'


def norm(text):
    """Undo the U+200B fencing a Google Docs PDF export puts around every styled run.

    A *doubled* fence is a swallowed space ("shall<zwsp><zwsp>have"); a single one is intra-word
    ("Signboard o<zwsp>f"). Dropping both fuses words, spacing both splits them, and span
    geometry cannot tell the two apart -- both measure a ~0.001 gap. Only the doubled form is
    a space.
    """
    text = text.replace('​​', ' ').replace('​', '').replace(' ', ' ')
    return re.sub(r'\s+', ' ', text)


def titlecase(value):
    words = value.split()
    out = []
    for i, word in enumerate(words):
        if word == '&':
            out.append('&')
        elif word.lower() in SMALL_WORDS and 0 < i < len(words) - 1:
            out.append(word.lower())
        else:
            out.append(word.capitalize())
    return ' '.join(out)


def read_blocks(pdf_path):
    """Text blocks with the leading bold run and the largest font size on each."""
    blocks = []
    with fitz.open(pdf_path) as doc:
        for page in doc:
            for block in page.get_text('dict')['blocks']:
                if 'lines' not in block:
                    continue
                lines = [norm(''.join(s['text'] for s in ln['spans'])).strip()
                         for ln in block['lines']]
                lines = [ln for ln in lines if ln and not FURNITURE.match(ln)]
                if not lines:
                    continue
                spans = [s for ln in block['lines'] for s in ln['spans']]
                lead = ''
                for span in spans:
                    if span['flags'] & 16:                            # bold
                        lead += span['text']
                    elif norm(span['text']).strip():
                        break
                blocks.append({
                    'text': ' '.join(lines).strip(),
                    'bold': norm(lead).strip(),
                    'size': round(max(s['size'] for s in spans), 1),
                })
    return blocks


def merge_page_breaks(blocks):
    """A paragraph split by a page break arrives as a second block starting mid-sentence."""
    merged = []
    for block in blocks:
        if merged and not block['bold'] and not STRUCTURAL.match(block['text']):
            prev = merged[-1]['text']
            if re.match(r'^[a-z(,]', block['text']) or (prev and prev[-1] not in '.:;!?_'):
                merged[-1]['text'] = prev + ' ' + block['text']
                continue
        merged.append(block)
    return merged


def split_items(body):
    """One list item per line; prose is left alone."""
    body = re.sub(r'\s*●\s*', '\n- ', body)
    body = re.sub(r'(?<!\d)\s(\d{1,2})\.\s+(?=[A-Z“"(])', r'\n\1. ', body)
    body = re.sub(r'^\s*(\d{1,2})\.\s+', r'\1. ', body)
    return body.strip()


def emit_section(head, bold, text):
    """`Section N: Title` on its own line, body beneath -- unless the source wrote a sentence."""
    title = re.sub(r'^Section\s+[0-9A-Z]+\s*:?\s*', '', bold).strip().rstrip(':').strip()
    body = re.sub(r'^\s*:\s*', '', text[len(head):])

    if title and body.startswith(title):
        tail = body[len(title):]
        if tail and tail[0].isalnum():
            # The bold run ended mid-word ("Signboard o|f"): the fence between the last bold
            # span and the first regular one carries no space of its own.
            cut = 0
            while cut < len(tail) and not tail[cut].isspace():
                cut += 1
            title, tail = title + tail[:cut], tail[cut:]
        body = tail
    body = re.sub(r'^\s*:\s*', '', body).strip()

    # Some sections are written as one sentence whose opening clause happens to be bold. A
    # lowercase continuation is the tell; keep those on a single line.
    if not title or (body and body[0].islower()):
        return ['', split_items(re.sub(r'\s+', ' ', head + ': ' + (title + ' ' + body).strip()))]

    out = ['', head + ': ' + title]
    if body:
        out.append(split_items(body))
    return out


def extract(pdf_path):
    out = []
    for block in merge_page_breaks(read_blocks(pdf_path)):
        text, bold = block['text'], block['bold']

        # Cover-page furniture. The preamble is stored unheaded, as the lead paragraph.
        if block['size'] > 14 or re.match(r'^V ?\d+\.\d+$', text) or text.upper() == 'PREAMBLE':
            continue

        article = re.match(r'^ARTICLE\s+([IVXL]+)\s*:\s*(.+)$', text, re.S)
        if article:
            out += ['', 'Article ' + article.group(1) + ': ' + titlecase(article.group(2)), '']
            continue

        # Letter-spaced closing heading, e.g. "F U T U R E  V I S I O N".
        if re.match(r'^F\s*U\s*T\s*U\s*R\s*E\s+V\s*I\s*S\s*I\s*O\s*N$', text):
            out += ['', 'Future Vision', '']
            continue

        section = SECTION_RE.match(text)
        if section and bold.startswith('Section'):
            out += emit_section(section.group(1), bold, text)
            continue

        out += ['', split_items(text)]

    text = '\n'.join(out)
    text = re.sub(r'\n{3,}', '\n\n', text)
    text = re.sub(r'[ ]{2,}', ' ', text)
    text = re.sub(r' +([,.;:)])', r'\1', text)
    text = re.sub(r'\( +', '(', text)
    text = re.sub(r'“ +', '“', text)
    text = re.sub(r' +”', '”', text)
    text = re.sub(r'^PREAMBLE\s+', '', text.strip())

    # Paragraphs the block pass could not rejoin, because the continuation carried a stray bold
    # run. At text level the tell is unambiguous: the line above stops without terminal
    # punctuation and the line below is neither a heading nor a list item.
    joined = []
    for line in text.split('\n'):
        if line.strip() and joined and not CONT_STOP.match(line):
            k = len(joined) - 1
            while k >= 0 and not joined[k].strip():
                k -= 1
            if k >= 0 and joined[k] and joined[k][-1] not in TERMINAL:
                joined[k] += ' ' + line.strip()
                del joined[k + 1:]
                continue
        joined.append(line)

    text = re.sub(r'\n{3,}', '\n\n', '\n'.join(joined))
    text = re.sub(r'([a-z]{2}\.)([A-Z][a-z])', r'\1 \2', text)       # "authorities.The"
    return text.strip()


def read_colophon(pdf_path):
    """Version and ratification date from the document's own closing line."""
    with fitz.open(pdf_path) as doc:
        tail = norm(doc[-1].get_text())
    match = COLOPHON.search(tail)
    if not match:
        return None, None
    version, day, month, year = match.groups()
    return version, '{}-{}-{}T00:00:00Z'.format(year, month, day)


def repoint_fallback(asset_url):
    source = io.open(READER, encoding='utf-8').read()
    updated, count = re.subn(
        r"(export const CONSTITUTION_PDF_FALLBACK = ')[^']*(';)",
        lambda m: m.group(1) + asset_url + m.group(2), source, count=1)
    if not count:
        sys.exit('CONSTITUTION_PDF_FALLBACK not found in ' + READER)
    if updated != source:
        io.open(READER, 'w', encoding='utf-8', newline='\n').write(updated)
        print('repointed CONSTITUTION_PDF_FALLBACK ->', asset_url)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('pdf', help='the ratified constitution PDF')
    parser.add_argument('--version', help='override the version on the document colophon')
    parser.add_argument('--effective', help='override the ratification date (ISO-8601 UTC)')
    parser.add_argument('--summary', help='ChangeSummary text shown on the public page')
    parser.add_argument('--summary-file', help='read ChangeSummary from a UTF-8 file')
    parser.add_argument('--dry-run', action='store_true',
                        help='print the extracted text instead of writing anything')
    args = parser.parse_args()

    pdf_path = os.path.abspath(args.pdf)
    if not os.path.isfile(pdf_path):
        sys.exit('no such file: ' + pdf_path)

    # The asset has to be served from public/assets, so copy it in if it was staged elsewhere.
    target = os.path.join(ASSETS, os.path.basename(pdf_path))
    if os.path.abspath(target) != pdf_path:
        shutil.copyfile(pdf_path, target)
        print('copied into assets:', os.path.basename(target))
    asset_url = '/assets/' + os.path.basename(target)

    doc_version, doc_effective = read_colophon(pdf_path)
    version = args.version or doc_version
    effective = args.effective or doc_effective
    if not version or not effective:
        sys.exit('could not read the document colophon; pass --version and --effective')

    content = extract(pdf_path)
    articles = len(re.findall(r'^Article ', content, re.M))
    sections = len(re.findall(r'^Section ', content, re.M))

    if args.dry_run:
        sys.stdout.reconfigure(encoding='utf-8')
        print(content)
        print('\n--- v{} | {} | {} chars | {} articles | {} sections'
              .format(version, effective, len(content), articles, sections), file=sys.stderr)
        return

    summary = args.summary
    if args.summary_file:
        summary = io.open(args.summary_file, encoding='utf-8').read().strip()
    if not summary:
        summary = ("Ratified version {} of the Association's constitution, superseding the "
                   "previous version.".format(version))
        print('WARNING: no --summary given; wrote a generic ChangeSummary. Edit it before shipping.')

    record = {
        'Id': 1,                       # only meaningful for HasData on a fresh database
        'Version': version,
        'Content': content,
        'PdfUrl': asset_url,
        'EffectiveDate': effective,
        'IsActive': True,
        'ChangeSummary': summary,
    }
    io.open(SEED, 'w', encoding='utf-8', newline='\n').write(
        json.dumps([record], ensure_ascii=False, indent=2) + '\n')
    print('wrote {} (v{}, {} chars, {} articles, {} sections)'
          .format(os.path.relpath(SEED, REPO), version, len(content), articles, sections))

    repoint_fallback(asset_url)
    print('next: review Content, then run `dotnet test` and `npx vitest run`.')


if __name__ == '__main__':
    main()
