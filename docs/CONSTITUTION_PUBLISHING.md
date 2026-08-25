# Publishing a New Constitution Version

**Rule: the application always serves the latest ratified constitution.** No page, no component
and no document link may pin a specific version. When a new PDF is ratified, one command
republishes everything.

---

## 1. The one command

Drop the ratified PDF into `GHCAA.Web/public/assets/`, then from the repository root:

```bash
python tools/constitution/publish_constitution.py \
    "GHCAA.Web/public/assets/GHCAA Constitution V4.2.pdf" \
    --summary-file docs/change-summary.txt
```

Requires `pymupdf` (`python -m pip install pymupdf`) — a documentation tool only, not an
application dependency.

The script:

1. **Extracts the document text** into the exact plain-text shape the Angular reader parses:
   an unheaded preamble paragraph, `Article <Roman>: <Title>` headings, `Section N: Title` on
   its own line with the body beneath, and one list item per line.
2. **Rewrites** `GHCAA.Infrastructure/Data/Seed/constitution.json` as a single active record —
   `Version`, `Content`, `PdfUrl`, `EffectiveDate`, `IsActive: true`, `ChangeSummary`.
3. **Repoints** `CONSTITUTION_PDF_FALLBACK` in
   `GHCAA.Web/src/app/public/constitution/constitution.ts` at the new asset.

`--version` and `--effective` are read from the document's own closing colophon
(`Document Version: 4.2 | Date of Ratification: 01.07.2026`); pass them explicitly to override.
Use `--dry-run` to print the extracted text without writing anything.

## 2. Review, then verify

The extractor is deterministic, but the source documents are hand-authored and their layout
drifts. Read the generated `Content` before committing — particularly the article count, the
section headings, and anything the source left as `____________` blanks.

```bash
dotnet test                     # 351 tests
cd GHCAA.Web && npx vitest run  # 64 files / 286 tests
npm run type-check
```

## 3. What follows the latest automatically

| Surface | How it stays current |
| --- | --- |
| `/constitution` reader | Renders `GET /api/governance/constitution` — whichever row is `IsActive`. |
| Landing hero "Read Constitution" | Routes to `/constitution`, not to a file. |
| ToC, article navigation | Rebuilt from the `Article N:` headings in the stored `Content`. |
| Version banner | `version` / `effectiveDate` / `changeSummary` off the active row. |
| Version History panel | `GET /api/governance/constitution/history`, each row with its own `PdfUrl`. |
| PDF buttons | `current().pdfUrl`, falling back to `CONSTITUTION_PDF_FALLBACK`. |

The only version-bearing string left in application code is `CONSTITUTION_PDF_FALLBACK`, and the
script owns it. **Never hardcode a constitution PDF path anywhere else.**

---

## Why it works this way

### `EnsureCreated()` cannot publish a new version

Startup runs `Database.EnsureCreated()`, which is a no-op once the tables exist. On preprod and
production the model's `HasData` seed never runs again, so editing the seed JSON alone can never
reach a live database. `GHCAA.Infrastructure/Data/ConstitutionSeeder.SyncAsync` — called from
`Program.cs` on every boot — closes that gap: it is idempotent, inserts a version it cannot
find, refreshes a stored version in place when the seed text changes, and supersedes rather than
deletes anything else.

### Superseded PDFs must stay on disk

Because the seeder supersedes rather than deletes, the previous version keeps its row, its
`AmendmentVote` records and its own `PdfUrl`. The Version History panel links to that URL, so
**deleting an old constitution PDF from `public/assets/` breaks the history**. Superseded PDFs
are permanent repository content.

### Traps in the source PDFs

These are properties of the exported documents, not bugs in the extractor. They cost a session
to diagnose once.

- **The cover page lies.** The v4.2 export still carries a "V 4.0" cover; only the closing
  colophon is authoritative. The script reads the last page for that reason.
- **U+200B fencing.** The Google Docs export wraps every styled run in zero-width spaces.
  A *doubled* fence marks a swallowed space (the words either side belong apart); a single
  fence is intra-word (the characters either side belong together). Replacing all of them
  with spaces splits words; dropping all of them fuses words. Span x-gap geometry cannot
  distinguish the two - both measure ~0.001.
- **Bold runs carry the structure.** `span['flags'] & 16` is what separates a `Section N: Title`
  heading from its body; `span['size'] > 14` is cover-page furniture.
- **Page breaks split paragraphs.** A continuation arrives as its own block and sometimes with a
  stray bold run; the script rejoins on "previous line ends without terminal punctuation and the
  next is not a heading or list item".

### Known defect in the v4.2 source document

Article V, Section C, item 6 reads *"6. TReplace the 21-day election notice rule with: …"* — a
leftover editing instruction in the ratified PDF. It is carried into the published text verbatim
and should be fixed in the source document, after which re-running the script republishes it.
