# PART III — CONSTRUCTION AND VALIDATION

# Chapter 7 — Implementation

*[Chapter mostly not written; §7.16 is. The headings below are generated from `docs/DOCUMENTATION_BOOK_OUTLINE.md` and are kept in step with it: `build.py --strict` fails if a section exists in one and not the other.]*

**What this chapter owns.** How the artefact was built: the mechanisms that were difficult, the
alternatives weighed while writing them, and what the code does that the design did not anticipate.

**What it must not repeat.** Why the architecture is shaped this way, which is Chapter 6. Threats and
controls, which are Chapter 8. How anything was tested, which is Chapter 9. Any measured result,
which is Chapter 12. No section here is a walk through the source tree, and no listing appears unless
the surrounding argument fails without it.

## 7.1 Development Environment, Toolchain and Reproducibility

*[Not written.]*

## 7.2 Solution and Module Structure

*[Not written.]*

## 7.3 Coding Standards, Conventions and Static Enforcement

*[Not written.]*

## 7.4 Implementation of the Domain and Persistence Layers

*[Not written.]*

## 7.5 Implementation of the Application and Business Services

*[Not written.]*

## 7.6 Implementation of the API Layer

*[Not written.]*

## 7.7 Implementation of the Web Client

*[Not written.]*

## 7.8 Implementation of the Mobile Client

*[Not written.]*

## 7.9 Real-Time Features

*[Not written.]*

## 7.10 Security Implementation

*[Not written.]*

## 7.11 Document Generation

*[Not written. Brief: identity cards, certificates, credential PDFs]*

## 7.12 Constitution Publication Pipeline

*[Not written. Brief: the always-latest invariant, the extraction tool, and why the naive approach fails on a live database]*

## 7.13 Third-Party Libraries: selection criteria

*[Not written. Brief: licence review and justification]*

## 7.14 Software Configuration Management

*[Not written. Brief: version control strategy, branching model, change control and release identification]*

## 7.15 Notable Implementation Challenges and Their Resolution

*[Not written. Brief: presented as symptom, hypothesis, evidence and resolution]*

## 7.16 Institution Profile Packs and White-Label Configuration

Every string, seed row and asset described elsewhere in this chapter names Govt. Haraganga College
by default, because the system was built for one association and had no reason to be otherwise until
Work Package 62 asked what it would take to run the same codebase for a second one. The constraint
that shaped the answer was not technical but organisational: the live deployment could not be
touched. `ORG_PROFILE` is unset on every environment this system runs in today, and any change whose
correctness depended on someone remembering to set it first was rejected on that basis alone, not
because it was hard to build.

The mechanism is a profile pack: a folder under `profiles/<name>/` (`profiles/ghc/`,
`profiles/default/`) holding `org-config.json`, an institution's own seed data, static page text and
image assets. `IInstitutionProfileProvider` reads the `ORG_PROFILE` environment variable at boot,
resolves it to a folder, and falls back file by file to `profiles/default/` for anything the named
profile does not supply — the same defaulting rule `Path.Combine`-based seed resolution elsewhere in
the codebase already used, extended rather than replaced. `OrgConfigService.BuildDefaults()` is the
one place that decides whether the pack is trusted: it drives configuration only when
`ProfileExplicitlySelected` is true, and falls back to the hardcoded values the service always had
otherwise. An unset `ORG_PROFILE` therefore behaves exactly as it did before this work package,
which is the property the constraint above demanded, and `OrgConfigGoldenSnapshotTests` freezes a
snapshot of the pre-Work-Package-62 output specifically so a future change cannot alter that
behaviour silently. The same guard shape recurs at every layer this work package touched: the ASP.NET
Core data-protection application name, the Angular boot-time branding fallback, and the Flutter
mobile application identifier are all read from the pack only once a profile is explicitly named,
because each of them protects something that breaks if it changes under a live deployment — session
cookies, first paint before the API responds, and the published app's store listing, respectively.

Seed data required a second decision, independent of the first: `docs/SEED_CLASSIFICATION.md`
grades every seed file by whether its content is structural (every institution needs the same rows),
shared-shape (every institution needs a row here, but not this one's), or historical (an empty table
is the only correct starting state for a new institution). Moving the third class out of
`Data/Seed/` and into `profiles/ghc/demo-data/` exposed two latent defects that had never been
visible before, because the class of data being moved had always been present in the fallback path
that both defects shared. `ApplicationDbContext.LoadSeed`'s test for whether it was safe to read a
profile pack checked whether the EF Core design-time tooling assembly was loaded into the process,
which is true for an ordinary test run as well as for an actual `dotnet ef` invocation, since the API
project references that package for its own migration tooling; and the project file that copies seed
fixtures into the test build output had never listed the `Data/Seed/Visual/` subfolder at all. Between
them, every affected integration test had been silently reading the pre-migration seed file instead
of the intended fixture set for as long as either defect existed, undetected because the fixture and
the real data it stood in for happened to satisfy the same foreign keys. Replacing the assembly-scan
check with the framework's own `EF.IsDesignTime` flag and adding the missing copy rule fixed both;
the full backend suite (590 tests) is the evidence that nothing else depended on the accidental
behaviour.

Two build-time generators keep the web and mobile clients honest to the same pack rather than a
second, hand-copied one: `GHCAA.Web/scripts/apply-brand.mjs` rewrites `index.html`, the sitemap and
the favicon set from the active profile before `ng build` runs, and
`GHCAA.Mobile/tool/apply_profile.dart` does the equivalent for the Android manifest label, the iOS
display name, and the launcher-icon/splash-screen generation inputs. Both are idempotent against the
GHC profile specifically so that running them changes nothing observable in the current build — verified
by diffing their output against the committed files rather than assumed. `scripts/brand-lint.mjs`
scans the three client codebases for the literal strings this work package removed and reports any
that return, currently warn-only until Work Package 62.41's acceptance test exists to prove a
`default`-profile build is actually clean before that check is allowed to fail a build.

Work Package 82.58 added the second layer the framework needed before it was useful to a
development environment other than the GHC deployment: `profiles/default/demo-data/` now holds a
minimal but fully-relational sample dataset — members, users, an EC history, news items, gallery
entries, financials and membership dues — sized to exercise every foreign-key path in the schema
without importing any real alumni record. `DatabaseBootstrapperExtensions` seeds this pack when
`ORG_PROFILE` is unset and the environment is non-production, so a clean `dotnet run` in development
reaches a usable state without any manual SQL. A companion `profiles/default/constitution.json`
provides a placeholder constitution for the same reason: the `ConstitutionSeeder` would abort
otherwise, because it enforces the always-latest invariant regardless of profile.

One path-resolution defect surfaced during this work: `ResolveProfilePackPath` had been matching
file names by exact string, so a file stored as `ec_history.json` would not be found when the pack
referenced it as `ec-history.json` (or vice versa). The fix normalises both the stored name and the
lookup key to canonical low-dash delimiters before comparing, with a secondary hyphen-normalised pass, so the resolver
is now format-agnostic. The change is covered by the `SeedDataIntegrityTests` suite, which was
extended in the same work package to validate both the `ghc` and `default` profile packs: every
referenced file must exist, every JSON document must parse, and every foreign-key reference within
the pack must resolve against the schema — run for both profiles on every CI execution.

What this work package does not close is recorded rather than hidden: 631 real alumni records,
their password hashes and their payment history are written as literal values into eight already-committed
EF Core migrations, not only into the seed files this section describes moving, so a second
institution's database would still not start empty until that separate problem (Work Package
62.31/82.31) is resolved. `docs/INSTITUTION_ONBOARDING.md` states this plainly to whoever deploys
the system next, rather than letting a profile pack's existence imply a safety the migration chain
does not yet provide.

## 7.17 Summary

*[Not written.]*

## Figures and Tables

*[Not drawn. The outline specifies 12 artefacts for this chapter. Each is added here with its caption as it is made, then `renumber.py --apply` is run.]*

