---
name: reference-docs-folder
description: What lives in the repo /docs folder — prior review artifacts, config framework, payment/gateway notes; read before re-reviewing
metadata:
  type: reference
---

The repo `/docs` folder holds prior analysis and reference material. Read the relevant file before re-doing a review — much may already be documented.

- `TODO.md` (~38KB) — THE master task tracker. Areas 1–28 = shipped work (mostly [DONE]); AREA 29 = full-stack review findings (2026-07-24), all [TODO]. Credentials block near the middle. Check off Area 29 items as fixed.
- `PLAN.md` — execution plans. Top = Forum mobile UI (3.7); appended = "Full-Stack Review Remediation" 6-phase plan mapping to TODO Area 29.
- `SRS.md` — software requirements spec.
- `BUSINESS_FINDINGS.md` (~33KB) — the large existing business/functionality findings log.
- `BUSINESS_FUNCTIONALITY_REVIEW_PLAN.md` — the plan the findings were produced against.
- `BUSINESS_TEST_CHECKLIST.md` — manual test checklist.
- `CONFIG_DRIVEN_FRAMEWORK.md` — how the config-driven (multi-org/branding) system works (Area 28).
- `PAYMENT_GATEWAY_WORKFLOW.md` + `dgepay_info.txt` — payment gateway flow and DGePay integration notes.
- `PHASE4_FINAL_REVIEW.md` — phase 4 review notes.
- `architecture_data_flow.md` — data flow diagram/notes.
- `db_connection.txt` — DB connection reference.

- `book/` — the dissertation/documentation book source (Markdown, one file per chapter) and its
  builder; see `docs/book/README.md` for build instructions and house style, and
  `docs/DOCUMENTATION_BOOK_OUTLINE.md` for the full 13-chapter structure. Chapters 1-6 exist as of
  this note; see [[session_documentation_book_ch4-6.md]] before continuing it.

Related: [[project_map.md]] (class/route map at repo root), [[gotcha_date_format_converter.md]],
[[session_documentation_book_ch4-6.md]], [[gotcha_book_builder_table_captions.md]].
