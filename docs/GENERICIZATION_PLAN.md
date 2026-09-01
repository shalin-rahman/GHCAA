# Genericization Plan: One Codebase, Any Institution

Status: PLAN (nothing implemented yet)
Raised: 2026-09-01, "make this application generic rather than GHC ... will work with GHC or any
other institution with minimal configuration changes"
Tracked as: docs/TODO.md Area 62

## 1. Goal and non-goals

Goal: a deployer stands up this platform for any alumni association by supplying one *institution
profile pack* (a folder of JSON + assets) and a handful of environment variables. No source edits, no
rebuild of business logic, no fork.

Non-goals for this plan:

- Multi-tenancy inside a single database. See ADR-1.
- Changing any existing GHC behaviour, URL, membership number, login, or stored data. Everything the
  GHC deployment does today must keep working byte for byte after this work. See section 8.
- Rewriting the design-token system, the payment flow, or the migration bootstrapper. Those already
  work and are only re-pointed at config.

## 2. Where the project already stands

This is not a from-scratch job. Area 28 already built most of the runtime plumbing:

- `OrganizationConfig` entity + `OrgConfigDto` (Branding, Contact, Currency, 17 FeatureToggles,
  Workflow, bilingual Localization) served from `GET /api/config`, edited at `PUT /api/config`
  (SuperAdmin only).
- Angular `OrgConfigService` loads it in `APP_INITIALIZER` and pushes `primaryColor`/`accentColor`
  into CSS custom properties. Flutter mirrors it via Riverpod `orgConfigProvider` with a
  SharedPreferences offline cache.
- `SiteContent` CMS (Area 34) already makes About/Contact prose admin-editable.
- Feature guards already hide whole modules per institution (`featureGuard` on 8 routes).
- Most seed data already lives in `GHCAA.Infrastructure/Data/Seed/*.json`, not in C#.

So roughly 60% of the mechanism exists. What is missing is that **the defaults are GHC**, in three
places at once, and a long tail of literals that never went through the config path.

The three hardcoded default sources (they must all become profile-driven):

1. `OrgConfigService.BuildGhcaaDefaults()` (Infrastructure, ~190 lines of C#).
2. `org-config.service.ts` `ghcaaDefaults` (Angular, ~90 lines, used when the API call fails).
3. `org_config.dart` `ghcaaDefaults` (Flutter, offline fallback).

## 3. Architecture decisions

### ADR-1: Deployment-per-institution, not row-level multi-tenancy

Each institution gets its own deployment and its own database. `OrganizationConfig.OrgId` stays as
the tenant key so a future multi-tenant mode is not blocked, but no `TenantId` column is added to any
other entity now.

Why: row-level multi-tenancy would touch every entity, every query, every EF global filter, the auth
pipeline, file storage paths, and the migration bootstrapper. That is a rewrite with a large
regression surface, for a benefit (shared hosting cost) nobody has asked for. Deployment-per-tenant
gives full data isolation for free and matches how the app is already hosted on Render.

Cost accepted: N deployments to upgrade instead of one. Mitigated by section 7 (one image, config
supplied at runtime, so an upgrade is a redeploy with the same tag).

### ADR-2: The institution profile pack is the single source of truth

```
profiles/
  default/                      neutral pack, ships in the repo, contains zero real institution data
    org-config.json             the OrgConfigDto shape (branding, contact, currency, features,
                                workflow, localization en/bn)
    site-content.json           About/Contact/legal CMS blocks
    email-templates.json        subjects and bodies, org name via placeholders only
    lookups.json                dropdown data (departments, batches, blood groups, districts)
    membership-tiers.json       tier labels, fees, order, self-selectable flags
    governance.json             EC role names and term rules
    documents.json              constitution/bylaws/forms registry (label, file, version)
    seo.json                    title, description, keywords, canonical base, JSON-LD org block
    demo-data/                  small synthetic member/event/gallery set for a fresh install
    assets/                     logo.png, favicon-*.png, seal.png, app-icon.png, splash.png
  ghc/                          the current GHC content, extracted verbatim from today's code+seeds
```

Selection: `ORG_PROFILE=ghc` (env var, default `default`). The API resolves
`profiles/$ORG_PROFILE/`, falling back file-by-file to `profiles/default/` so a partial pack is
valid. A profile is data, never code: no C# file may reference `ghc` by name.

### ADR-3: Two config planes, deliberately kept separate

| Plane | Changes | Mechanism | Examples |
|---|---|---|---|
| Runtime | Live, no redeploy | `OrganizationConfig` row, admin UI | org name, colors, contact, feature flags, tier labels |
| Boot | Redeploy required | Profile pack + env vars | favicon, app bundle id, seeded demo data, SEO JSON-LD, legal text baseline |

Rule: anything a SuperAdmin should be able to change goes in the runtime plane. Only things baked
into static files (favicon, app icon, index.html meta) stay in the boot plane. The profile pack seeds
the runtime plane on first boot and never overwrites it afterwards, except `Localization`, which
already self-heals from defaults on every read (keep that behaviour, just change the source).

### ADR-4: MembershipType stays an enum, gains a config-driven definition layer

`MembershipType` is a C# enum persisted as ints across Members, fees, seeds, web constants, and
Flutter constants. Renaming it to string keys is a data migration with real breakage risk for no
gain.

Instead, `membership-tiers.json` supplies per-institution presentation and policy for the existing
keys: display label (en/bn), display order, visibility, self-selectable at registration, fee config
link, and an `enabled` flag that hides tiers an institution does not use. An institution that needs a
genuinely new tier name maps it onto a spare enum slot via the same file. If a future institution
needs unlimited custom tiers, that is a separate v2 (a `MembershipTier` table), noted and not built
now.

### ADR-5: Brand literals are prevented, not just removed

A one-time cleanup rots. Add a `brand-lint` check to CI that greps the source trees (not `profiles/`,
not migrations, not test fixtures) for a banned literal list (`GHCAA`, `GHC-`, `Haraganga`,
`Haragangian`, the live Render hostname, `1938`). Any new occurrence fails the build. This is the
mechanism that makes the change stick; without it the plan is a one-off sweep.

Exception list lives in `brand-lint.config.json` with a reason per entry (data-protection key ring
name, existing bundle id, historical migration names).

## 4. What is actually hardcoded today (audit result)

### 4.1 API

- `OrgConfigService.BuildGhcaaDefaults()` (short/full name, institution name, `GHC-` membership
  prefix, `GHC APPROVED` seal, logo URL, registered office and campus addresses, both locale packs,
  EC role names, email subjects).
- `Constants.Defaults`: `MembershipPrefix = "GHC-"`, `ImportEmailBase = "haragangian"`.
- `IDCardService` (QuestPDF): `"GHC Alumni Association"`, `"GHC ALUMNI ASSOCIATION"`,
  `"Govt. Haraganga College Alumni Association"`, accent `#c5a059` baked into the layout.
- `email_templates.json`: org name written into subject and body text instead of a placeholder.
- `Program.cs`: `SetApplicationName("GHCAA")` (data-protection key ring, see section 8) and Swagger
  title `"GHCAA API V1"`.
- `appsettings.json`: `GeneralSettings.AssociationNamePrefix = "HARAGANGIAN-"`,
  `EmailDomain = "haragangian.com"`, `ProtectedSuperAdmins = ["shalin","superadmin"]`.
- `constitution.json`, `site_content.json`: GHC-specific prose (correctly in JSON already, just in
  the wrong folder).
- `members.json`: 631 real alumni records including names, emails, mobile numbers, NIDs, addresses.
- Payment gateway config assumes Bangladesh providers (SSLCommerz, bKash, Nagad, Rocket, DGePay).

### 4.2 Web

- `index.html`: title, description, keywords, canonical URL, full `AlumniOrganization` JSON-LD block
  with name, logo, and Munshiganj address. `public/sitemap.xml` and `robots.txt` hardcode the live
  Render hostname. Favicons are the GHC crest.
- `app.routes.ts`: every route `title` and `data.description` names GHCAA.
- Static institution text in `register.html` (terms and conditions, founding date, ownership clause),
  `about.html` (founding story, also inside the fallback branch so it shows when SiteContent is
  empty), `digital-id.html`, `assistant.html`, `directory.html`, `magazine.html`, `purpose.html`,
  `elections.ts`, `membership.ts`, `payment-status.ts`, plus admin placeholder text.
- `org-config.service.ts` `ghcaaDefaults` block, and `|| 'GHCAA'` style fallbacks scattered in
  templates.
- Direct `/assets/logo.png` references that bypass config in `reset-password.html`, `register.html`,
  `logo-spinner.html`, `about.html`.
- Hardcoded filenames: `GHCAA_ID_Card.png`, `GHCAA_Certificate.png`,
  `/assets/GHCAA Constitution V4.2.pdf`.
- `package.json` name, `styles.scss` header comment, seeded gold palette as the built-in default.

### 4.3 Mobile

- `AndroidManifest.xml` label `Haragangian`, `applicationId com.ghcaa.portal`, namespace still
  `com.example.ghcaa_mobile`; iOS `CFBundleDisplayName`/`CFBundleName`; `pubspec.yaml` name and
  description.
- Literal strings in `app_home_screen.dart`, `main.dart` (`HaragangianApp` class name),
  `register_screen.dart`, `about_screen.dart`, `ai_chat_screen.dart`, `app_config.dart` fallbacks,
  `chat_room_screen.dart`, `digital_id_screen.dart`, `magazine_screen.dart`,
  `profile_edit_screen.dart`, `submit_article_screen.dart`, `register_wizard_provider.dart`.
- `app_theme.dart` gold/obsidian constants (used as fallbacks only, which is acceptable once the
  fallback is neutral).
- App icon, splash, and `assets/logo.png` are GHC images.

### 4.4 Tests

Literal `"GHCAA"` assertions in `OrgConfigServiceTests`, `CommunicationServiceTests`,
`MemberServiceTests`, `TokenServiceTests` (6 occurrences). These must assert against the loaded
profile, not a constant.

## 5. Delivery phases

Each phase is independently shippable and leaves the GHC deployment working.

### Phase A: Profile pack foundation (no user-visible change)

Build `IInstitutionProfileProvider` in Infrastructure: resolves `ORG_PROFILE`, loads and caches the
pack, merges file-by-file over `profiles/default/`, validates against a schema on boot and fails
loudly with a readable error if a required key is missing. Extract today's GHC values verbatim into
`profiles/ghc/` and set `ORG_PROFILE=ghc` in the existing deployments. Write `profiles/default/` as a
neutral "Sample Alumni Association" pack. Add the `brand-lint` CI check in warn-only mode.

Exit criteria: `ORG_PROFILE=ghc` produces a byte-identical `GET /api/config` response to today.

### Phase B: API de-branding

`BuildGhcaaDefaults()` becomes `BuildDefaultsFromProfile()`. `Constants.Defaults` brand fields move
to config. `IDCardService` takes `IOrgConfigService` and draws name, seal text, and accent color from
it. `email_templates.json` gets `{{OrgName}}`/`{{OrgShortName}}`/`{{SupportEmail}}` placeholders and
the renderer supplies them. Swagger title and the data-protection application name become config with
the existing values preserved as the GHC profile's values. Seeders read profile-relative paths.
Update the 6 test assertions.

### Phase C: Web de-branding

Neutral fallback config in `org-config.service.ts` (or better, drop the inline block and serve the
boot config from the profile at build time so there is one source). Route titles move to a
`TitleStrategy` that composes a generic route label with `branding.shortName`. A prebuild script
(`scripts/apply-brand.mjs`) templatizes `index.html`, `sitemap.xml`, `robots.txt`, and the favicon set
from `seo.json` plus the profile assets. Register terms, About founding story, and the other static
prose move into `site-content.json` blocks with a genuinely generic empty state. Direct logo paths go
through the config-driven image component that already exists. Download filenames use the acronym
from config. The constitution PDF becomes an entry in `documents.json`, so any institution can publish
its own bylaws.

### Phase D: Mobile de-branding

Flutter flavors driven by the profile: app name, bundle id, icons, and splash generated from
`profiles/$ORG_PROFILE/assets` via `flutter_launcher_icons` and `flutter_native_splash` in the build
script. Literal strings move to `localePack` keys. `HaragangianApp` renames to `AlumniApp`.
`app_config.dart` fallbacks become neutral. The existing GHC bundle id
(`com.ghcaa.portal`) stays pinned in the GHC flavor, see section 8.

### Phase E: Data, tiers, and governance

Move `members.json` and the other real-data seeds into `profiles/ghc/demo-data/` (and out of the
default pack entirely, see section 9). Build `membership-tiers.json` and `governance.json` per ADR-4,
wire the label/order/visibility/self-selectable flags through API, web, and mobile. Make the payment
gateway set a registry keyed by profile so a non-Bangladesh institution can ship with none enabled
and the manual-payment path only. Currency and locale already flow from config; verify end to end.

### Phase F: Onboarding, ops, and proof

`docs/INSTITUTION_ONBOARDING.md` plus `scripts/new-institution.mjs` that scaffolds a pack from
`default` and prompts for the dozen values that matter. One Docker image, profile supplied at runtime,
so a second institution is an env change plus a mounted or baked profile folder. Flip `brand-lint` to
blocking. Add the proof tests in section 6.

## 6. Verification strategy

The plan is only real if it is provable. Four levels:

1. **Golden config test.** `ORG_PROFILE=ghc` must serialize `GET /api/config` identically to a
   snapshot taken before Phase A. Any drift is a regression, not a refactor.
2. **Default profile smoke.** Boot with `ORG_PROFILE=default` against an empty database and walk
   register, login, portal, admin, ID card, and PDF generation. Nothing may render "GHCAA" or the
   Bengali motto anywhere. This is the actual acceptance test for "generic".
3. **brand-lint in CI.** Blocking, with a documented exception list.
4. **Existing suites stay green.** `dotnet test` (330+ NUnit), vitest (59 files), `flutter analyze`,
   Playwright e2e including `config-regression.spec.ts`, which already asserts the org name comes from
   an intercepted config rather than markup. Extend it to run twice, once per profile.

## 7. Infrastructure

- One image per component, no per-institution build of the API or web bundle where avoidable. The web
  bundle does need a per-profile prebuild for `index.html`/favicon/SEO, so the web image is built per
  profile with `--build-arg ORG_PROFILE`. The API image is profile-agnostic and reads the env var.
- Environment matrix per institution: `ORG_PROFILE`, connection string, `FileStorage:BasePhysicalPath`
  (uploads are still ephemeral without a mounted disk, unchanged and already tracked), SMTP, gateway
  keys, `ProtectedSuperAdmins`, portal base URL.
- Uploads, backups, and log streams stay per-deployment, which falls out of ADR-1 for free.

## 8. Non-breaking guarantees for the existing GHC deployment

These are the traps that would silently break production if handled carelessly:

1. **`SetApplicationName("GHCAA")`** keys the data-protection ring. Changing it invalidates every
   issued token and cookie protected by it. It becomes config, and the GHC profile keeps the exact
   string `GHCAA`. It is on the brand-lint exception list for that reason.
2. **Membership numbers.** `GHC-0000000001` values are already issued and printed on ID cards. The
   prefix becomes config for *newly issued* numbers only. No backfill, no reformat, no migration that
   rewrites `MembershipNumber`.
3. **Mobile bundle id.** `com.ghcaa.portal` is what the published app uses. Changing it is a new app
   in the store, not an update. The GHC flavor pins it; only new institutions get a new id.
4. **MembershipType enum values.** Persisted as ints. ADR-4 changes labels only, never the numeric
   values or their order.
5. **Live URLs.** `sitemap.xml`/`robots.txt`/canonical must still emit the current hostname for the
   GHC profile.
6. **Localization self-heal.** `GetConfigAsync` currently overwrites stored Localization with code
   defaults on every read. If the source changes to the profile pack without care, every institution
   that customised localization through a future admin UI would lose it. Keep the self-heal, but heal
   from the *profile* pack, and revisit once localization becomes admin-editable.
7. **Seed extraction.** Moving `Seed/*.json` into a profile folder must not make
   `MigrationBootstrapper` re-seed or re-baseline. Path change only, same idempotency logic, verified
   against a copy of the preprod database before it goes near preprod.

## 9. Data protection call-out

`profiles/ghc/demo-data/members.json` would carry 631 real alumni records (names, emails, mobile
numbers, NIDs, addresses) inside a repository that is about to be handed to other institutions. That
is a personal-data disclosure, independent of this refactor. Two options, pick one before Phase E
ships:

- Keep real member data out of the repo entirely and load it from an operator-supplied file at deploy
  time (recommended), or
- Anonymise the committed copy and keep the real import as an operator artifact.

The `default` profile must never contain real personal data under any circumstance.

## 10. Area 61 is folded in, not run beside this

Area 61 (comment/doc tone, dead-code detection, refactor sweep) has three open items that would
otherwise read the same files this work already opens. Running them separately means touching the
whole repo twice, so they become standing obligations on every phase above. Tracked as 62.46-62.49.

- **Tone (retroactive).** The root CLAUDE.md rule already says a file touched for any reason gets its
  AI-sounding comments cleaned up, not just the changed lines. Area 62 touches most of the codebase,
  so this is where that rule actually gets paid off. It binds the new code too: the profile loader,
  brand-lint, the build scripts, and the onboarding doc all have to read like a person wrote them.
- **Dead code (61.1).** Deleting three large default blocks, the brand fields in `Constants.Defaults`,
  and the fixed seed paths will strand helpers, constants, and imports. Detect it per module with
  `graphify query`/`explain` at the moment that module is de-branded. That turns 61.1 from a blind
  full-repo sweep into a cheap by-product with real targets. Richest expected yield: Infrastructure
  (OrgConfigService, seeders), Angular `core/services` and `core/constants`, Flutter `core/config`.
- **Refactor (61.2).** Remove what the detection surfaces, in the same phase that stranded it, and
  nothing more. This is explicitly not a licence for a general refactor, and the project's
  no-abstraction-without-a-concrete-problem rule still applies.

If Area 62 is deferred or cancelled, 61.1 and 61.2 go back to being standalone items.

## 11. Sequencing and risk

```
A (foundation) ──► B (API) ──► C (Web) ──► F (onboarding/proof)
                     └──────► D (Mobile)
                     └──────► E (data/tiers)
```

A is the only hard prerequisite. B, C, D can then proceed in parallel if there are separate hands. E
depends on B for the tier config surface. F closes the area. Section 10's obligations ride along
inside every phase rather than forming a phase of their own.

Biggest risks, in order:

1. Silent divergence between the three default-config sources during the transition. Mitigated by the
   golden config test (6.1) running from Phase A onward.
2. Seed path change disturbing the migration bootstrapper. Mitigated by the throwaway-database dry
   run that is already the project's convention for migration work.
3. Scope creep into full multi-tenancy. ADR-1 is the line; anything asking for a `TenantId` column is
   out of this area.
