# Repo reference

`.github/copilot-instructions.md` is the canonical source for the full commands table, architecture,
backend/Angular/Flutter conventions, API contract logging and the shared change workflow. Read it
before cross-layer work. `AGENTS.md` and `GEMINI.md` add model-specific guidance; keep shared rules
aligned across all four files. This file repeats only the commands and rules needed every session.

Open work follows `docs/MASTER_EXECUTION_PLAN.md`, the only open plan (owner, 2026-11-03). Its
first section is the handover: the current wave, the next items and the owner gates. Check it against
the tracker with `python scripts/backlog_inventory.py --check docs/MASTER_EXECUTION_PLAN.md`. Other
plan files are design references: they say how to build an item, never when.

Layers: `GHCAA.Domain` → `GHCAA.Application` (DTOs, interfaces) → `GHCAA.Infrastructure` / `GHCAA.API`
(thin host). Clients: `GHCAA.Web` (Angular 21), `GHCAA.Mobile` (Flutter). PostgreSQL in prod, SQLite locally.

## Commands (full table in copilot-instructions)

- Backend: `dotnet test GHCAA.Tests/GHCAA.Tests.csproj --filter "FullyQualifiedName~X"`
- Web: `cd GHCAA.Web; npm run type-check; npm run test:unit -- <spec path>`
- Mobile: `cd GHCAA.Mobile; flutter analyze; flutter test <file> --exclude-tags golden`
- Local stack: `.\GHCAA.Tools\run-app.ps1` / `stop-app.ps1`
- Before handing work back: `python scripts/ci-local.py` (`--quick`, or `--only backend|web|mobile|contract`
  for one layer; `--e2e` adds end-to-end).

## Traps

- FluentValidation validators never run; use DataAnnotations.
- A migration without its `.Designer.cs` is silently skipped. `ef migrations add` then `remove` truncates
  the model snapshot.
- `Seed/*.json` only seeds a new DB; editing it does not change an existing one.
- Displayed dates are `dd-MM-yyyy` on every client. The API's `DateFormatConverter` handles the wire
  format; do not format dates ad hoc.

## Graphify

- `graphify query "<question>"` (or `path`, `explain`) before raw grep; `graphify-out/wiki/index.md`
  for broad navigation.
- After modifying code or docs, run `graphify update .` (AST-only, no API cost).

## Cross-layer changes and evidence

- Check `docs/TODO.md` and `docs/PROJECT_MAP.md` before cross-layer work. Order comes from the master
  plan; a linked plan only says how to build the item.
- Treat API and business-rule changes as cross-layer work. Check both web and mobile clients, update
  relevant tests, and record route or DTO changes in `docs/API_CONTRACT_REGISTRY.md`.
- Keep implemented mechanisms, executable test evidence, and empirical research claims distinct. A
  passing test does not by itself establish real-world effectiveness or performance.
- The user owns Git and deployment operations. Do not commit, push, merge, or deploy.
- Never commit `.env` files, connection strings or diagnostic logs (`api_error.log`, `api_stdout.txt`,
  `build_errors.txt`).

## Comment, doc and TODO tone

Every new or edited comment, doc-comment, markdown doc and TODO entry, in any language, must read like
a person wrote it.

- Plain, short sentences that say what the code does or why.
- No filler openers ("This function is responsible for...", "Note that..."), no restating the obvious
  (`// increment i`), no proof-of-change notes ("// updated per review").
- A comment earns its place by naming a gotcha, a non-obvious reason, a workaround or a caller assumption.
- No em dashes, no bullet lists inside comments, no `// ===== SECTION =====` banners unless the file
  already uses them.
- TODOs name the actual gap and, where known, why it is not done yet.
- When a file is touched, clean up machine-written comments and docs in the part being edited. Leave
  unrelated comments alone unless they are wrong or out of date. Match the file's existing style.

## Documentation book

Rules for `docs/book/` live in `.claude/rules/book.md` and load when a book file, the outline or
`docs/TODO.md` is opened.
