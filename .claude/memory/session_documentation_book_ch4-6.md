---
name: session-documentation-book-ch4-6
description: Chapters 4-6 of the dissertation/documentation book (docs/book) written and wired into the builder; house style and gotchas for continuing Chapters 7+
metadata:
  type: session
---

**What was done:** Wrote `docs/book/04-methodology.md` (Research Methodology), `docs/book/05-system-analysis.md`
(System Analysis and Behavioural Modelling) and `docs/book/06-architecture.md` (System Architecture and Design),
per the structure in `docs/DOCUMENTATION_BOOK_OUTLINE.md`. Added all three to `CHAPTERS` in
`docs/book/build/build.py` (in order, before `99-references.md`). Book builds clean:
`python3 docs/book/build/build.py` → 51 figures/51 diagrams, 18 tables captioned, no new orphan captions
(only the pre-existing Tables 3.1/3.2/3.3/3.5/3.6 pointer entries).

**Why these three chapters, this content:** Chapters 1-3 (front matter, introduction, literature review,
requirements) already existed. 4-6 are Part II — method, then the analysis/design models the method produces.
Every factual claim was checked against the actual repo rather than invented: e.g. 49 mapped `DbSet`s (grep-counted
in `ApplicationDbContext.cs`), 37 controllers (`ls GHCAA.API/Controllers`), 34 registered service interfaces (DI map
in `docs/project_map.md`), `styles.scss` at 3,354 lines, git log range 2026-02-09 to 2026-08-28 (203 commits),
JWT access token 60 min / refresh token 7 days (`appsettings.json` + `TokenService.cs`), real unique/composite
indexes from `GHCAA.Infrastructure/Data/Configurations/*.cs` for the data-dictionary and indexing-strategy sections.
The Ch.4 risk table (RMMM) is built from real closed/open items in `docs/TODO.md` (Areas 23, 24, 29), not invented
risks — e.g. the date-format drift (Area 23/29F.3) and the payment amount-verification bypass (29B.2).

**Honest gaps recorded on purpose (do not "fix" the prose without fixing the code, or vice versa):**
- Ch.5 §5.6/§5.8: `GovernanceService.VoteOnConstitutionAsync` checks `MembershipType` (Founding/Executive/General)
  and prior-vote, but does NOT check arrears/standing before admitting an amendment vote — so FR-36 ("voting
  member in good standing") is only partly enforced in code. Carried forward as an open item for Ch.8/Ch.9.
- Ch.6 §6.8.3: `docs/PROFILE_SHARED_COMPONENT_DESIGN.md` is a *design-only, not-implemented* proposal to de-duplicate
  the member profile page vs. the admin member-detail modal. Still two independent, drifting templates as of this
  session. Do not describe this as done in later chapters (Ch.7 in particular) unless it actually gets built.
- Ch.6 §6.11.4/§6.13 (ADR-06): `IFileStorageService` has exactly one implementation (`LocalFileStorageService`);
  the interface's substitutability is structural, not demonstrated.

**Gotcha while writing further chapters:** see [[gotcha_book_builder_table_captions.md]].

**How to apply / continue:** Chapters 7-13 remain (`docs/DOCUMENTATION_BOOK_OUTLINE.md` has the full structure,
figure/table inventory and per-chapter source-material map). Reuse existing IEEE reference numbers [1]-[68] from
`docs/book/99-references.md` where the topic already has one; only append a new numbered reference if the topic
genuinely isn't covered. Keep pulling facts from the repo (project_map.md, TODO.md, BUSINESS_FINDINGS.md,
architecture_data_flow.md, CONFIG_DRIVEN_FRAMEWORK.md, RENDER_DEPLOYMENT.md for Ch.10, git history for Ch.11) rather
than writing generic software-engineering prose — that's what keeps the book from reading as templated filler.
