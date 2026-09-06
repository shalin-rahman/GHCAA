# Configuration-Driven Framework — Implementation Guide

**Status:** Phase 1 (Backend + DB) COMPLETE | Phase 2 (Angular Consumer) COMPLETE | Phase 3 (Flutter Consumer) COMPLETE  
**Last Updated:** 2026-09-05  
**Branch:** `preprod`

**Part of this is now covered by Work Package 62 too.** This document covers *runtime* config for
one already-deployed organization: an admin edits branding at `/admin/org-config`, which calls
`PUT /api/config`. Work Package 62 adds an earlier layer: which defaults a fresh deployment starts
with, before any admin has saved anything. That comes from a profile pack
(`profiles/<name>/org-config.json`), picked by the `ORG_PROFILE` environment variable, read by
`IInstitutionProfileProvider`/`OrgConfigService.BuildDefaults()`. The two don't conflict: the
profile pack only supplies `BuildDefaults()`'s fallback values. An admin's saved row in
`OrganizationConfigs` still wins once one exists, same as before this was added. See
`docs/WHITE_LABEL_PLAN.md`, `docs/INSTITUTION_ONBOARDING.md`, and Work Package 62 in
`docs/TODO.md` for that half. This document is still accurate for the runtime-admin-edit half.

---

## 1. What This Framework Does

Transforms every hardcoded brand string, label, and feature toggle in the system into a runtime-configurable JSON document stored in the `OrganizationConfigs` database table and served via `GET /api/config`. Switching an entire organization's branding, terminology, or enabled features requires only a `PUT /api/config` call — no code change, no redeployment.

---

## 2. Architecture Overview

```
┌─────────────────────────────────────────────────────────────────────┐
│ Clients                                                              │
│  Angular Web ─────────────────┐                                     │
│  Flutter Mobile ──────────────┤──► GET /api/config ──► OrgConfigDto │
│  Admin (PUT /api/config) ─────┘                                     │
└───────────────────────────────┬─────────────────────────────────────┘
                                │
┌───────────────────────────────▼─────────────────────────────────────┐
│ GHCAA.API                                                            │
│  OrgConfigController (GET public, PUT SuperAdminOnly)               │
└───────────────────────────────┬─────────────────────────────────────┘
                                │ IOrgConfigService (Application)
┌───────────────────────────────▼─────────────────────────────────────┐
│ GHCAA.Infrastructure                                                 │
│  OrgConfigService                                                    │
│   ├─ IMemoryCache (10-min TTL, GetOrCreateAsync — no thundering herd)│
│   └─ ApplicationDbContext.OrganizationConfigs                        │
│       └─ ConfigJson (text/JSONB) — full OrgConfigDto serialized     │
│                                                                      │
│  Fallback chain: Cache → DB → BuildGhcaaDefaults()                  │
│  (app never crashes from missing config)                             │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 3. Files Delivered (Phase 1)

| File | Layer | Status |
|------|-------|--------|
| `GHCAA.Domain/Models/OrganizationConfig.cs` | Domain | Created |
| `GHCAA.Application/DTOs/OrgConfigDto.cs` | Application | Created |
| `GHCAA.Application/Interfaces/IOrgConfigService.cs` | Application | Created |
| `GHCAA.Infrastructure/Services/OrgConfigService.cs` | Infrastructure | Created |
| `GHCAA.Infrastructure/Data/Configurations/OrganizationConfigConfiguration.cs` | Infrastructure | Created |
| `GHCAA.Infrastructure/Data/ApplicationDbContext.cs` | Infrastructure | Modified (DbSet added) |
| `GHCAA.API/Controllers/OrgConfigController.cs` | API | Created |
| `GHCAA.API/Program.cs` | API | Modified (seed + DI) |
| `GHCAA.Domain/Enums.cs` | Domain | Modified (Guest added) |
| `GHCAA.Web/src/app/core/constants/app.constants.ts` | Angular | Modified (Guest added) |

---

## 4. Mandatory Migration (MUST DO BEFORE RUNNING)

```powershell
# Step 1: Apply the two pending migrations that were queued before this feature
dotnet ef database update --project GHCAA.Infrastructure --startup-project GHCAA.API --context ApplicationDbContext

# Step 2: Generate the OrganizationConfig migration
dotnet ef migrations add AddOrganizationConfig `
  --project GHCAA.Infrastructure `
  --startup-project GHCAA.API `
  --context ApplicationDbContext `
  --output-dir Data/Migrations/PgSql

# Step 3: Apply it
dotnet ef database update --project GHCAA.Infrastructure --startup-project GHCAA.API --context ApplicationDbContext
```

**What the migration creates:**
```sql
CREATE TABLE "OrganizationConfigs" (
    "Id"                 SERIAL PRIMARY KEY,
    "OrgId"              VARCHAR(50) NOT NULL,
    "SchemaVersion"      INT NOT NULL DEFAULT 1,
    "ConfigJson"         TEXT NOT NULL,
    "UpdatedAt"          TIMESTAMPTZ NOT NULL,
    "UpdatedByAdminId"   VARCHAR(50)
);
CREATE UNIQUE INDEX "IX_OrganizationConfigs_OrgId" ON "OrganizationConfigs" ("OrgId");
```

**On first app boot after migration:** Program.cs seeds the GHCAA defaults automatically. The PUT endpoint is immediately functional with real DB-backed data.

---

## 5. Config JSON Schema (v1)

```json
{
  "orgId": "ghcaa",
  "schemaVersion": 1,
  "branding": {
    "shortName": "GHCAA",
    "fullName": "Govt. Haraganga College Alumni Association",
    "memberNickname": "Haragangian",
    "institutionName": "Govt. Haraganga College",
    "institutionAcronym": "GHC",
    "membershipNumberPrefix": "GHC-",
    "approvalSeal": "GHC APPROVED",
    "establishedOn": "29 Nov 2025",
    "logoUrl": "/assets/logo.png",
    "primaryColor": "#1a237e",
    "accentColor": "#e53935"
  },
  "contact": {
    "supportEmail": "haragangian@gmail.com",
    "importEmailBase": "haragangian",
    "registeredOffice": "Govt. Haraganga College Campus, Munshiganj, Bangladesh.",
    "portalBaseUrl": "https://haragangian.com/portal",
    "socialLinks": { "facebook": "#", "whatsapp": "#", "youtube": "#" }
  },
  "currency": { "code": "BDT", "symbol": "৳", "name": "Bangladeshi Taka" },
  "features": {
    "enableEvents": true,
    "enableJobHub": true,
    "enableGallery": true,
    "enableForum": true,
    "enableMentorship": true,
    "enableFamilyLink": true,
    "enableMagazine": true,
    "enablePolls": true,
    "enableGamification": false,
    "enablePublicDirectory": true,
    "enableDigitalIdCard": true,
    "enableCertificates": true,
    "enableSocialAuth": false,
    "requirePaymentForMembership": true,
    "requireDocumentUpload": true,
    "allowSelfRegistration": true,
    "allowNonMemberEventRegistration": true
  },
  "workflow": {
    "memberApprovalMode": "ManualReview",
    "otpVerificationRequired": true,
    "defaultMembershipType": "General",
    "adminEmailOnNewRegistration": true,
    "membershipTypes": ["Founding","Executive","General","Associate","Honorary","Advisory","Guest"]
  },
  "localization": {
    "defaultLocale": "en",
    "supportedLocales": ["en", "bn"],
    "locales": {
      "en": { "orgName": "...", "tagline": "...", "memberLabel": "Member", ... },
      "bn": { "orgName": "...", "tagline": "...", "memberLabel": "সদস্য", ... }
    }
  }
}
```

---

## 6. API Endpoints

| Method | Route | Auth | Purpose |
|--------|-------|------|---------|
| `GET` | `/api/config` | Public | Returns full OrgConfigDto (10-min memory-cached) |
| `PUT` | `/api/config` | SuperAdminOnly | Updates config, invalidates cache, persists to DB |

**PUT payload:** full `OrgConfigDto` JSON (same shape as GET response).  
**PUT response:** `204 No Content`.  
**Cache invalidation on PUT:** immediate (IMemoryCache key removed). Next GET re-populates from DB.

---

## 7. DI Registration

`OrgConfigService` is auto-registered by `DependencyInjection.cs` (lines 44–57): any class in `.Services` namespace implementing an interface in `GHCAA.Application.Interfaces` is registered as Scoped. **No manual registration required.**

---

## 8. Guest Membership Type — Sync Checklist

A new `Guest` value was added to `MembershipType` enum. All three platforms must reflect this:

| Platform | File | Change | Status |
|----------|------|--------|--------|
| Backend | `GHCAA.Domain/Enums.cs:6` | `Guest` appended to end of enum | Done |
| Angular | `app.constants.ts:39` | `'Guest Member'` in `MEMBERSHIP_TYPES` | Done |
| Angular | `app.constants.ts:283` | `{ value: 'Guest', label: 'Guest Member' }` in `MEMBERSHIP_TYPE_OPTIONS` | Done |
| Config | `OrgConfigService.cs` | `["Guest"] = "Guest Member"` / `"অতিথি সদস্য"` in both locales | Done |
| Flutter | `lib/core/config/org_config.dart` | `'Guest'` in `membershipTypes` + `en`/`bn` label maps | Done |
| Flutter | `lib/features/lookups/dropdown_service.dart` | `'Guest'` in the type list + option map | Done |
| Flutter | `lib/screens/admin/fee_config_screen.dart` | `'Guest'` in the fee-tier `names` list | Done |
| Flutter | `lib/core/constants/registration_constants.dart` | `MembershipConstants.typeOptions` **deleted** — tiers are admin-assigned only, so no registration screen offers them | Done 2026-08-22 (TODO 35.5, closes 28.21) |
| Backend | `GHCAA.Application/DTOs/MemberRegistrationDto.cs` | `MembershipType` property **deleted** — the registration payload must not carry a tier | Done 2026-08-22 (TODO 35.5) |
| Backend | `MemberService.RegisterAsync` | Assigns `OrgConfig.Workflow.DefaultMembershipType` (fallback `General`) to both the member and the registration-fee lookup; ignores anything a client sends | Done 2026-08-22 (TODO 35.5) |
| Angular | `public/register/register.html` + `register.ts` | Tier `<select>` replaced by a read-only note; fee tier read from `orgConfig.config()?.workflow?.defaultMembershipType` | Done 2026-08-22 (TODO 35.5) |
| Flutter | `lib/screens/auth/register_screen.dart` | Step-3 tier dropdown replaced by explanatory text | Done 2026-08-22 (TODO 35.5) |
| Flutter | `lib/features/auth/register_wizard_provider.dart` | `membershipType` removed from model, ctor, `copyWith`, `data` map, `updateData` and submit payload | Done 2026-08-22 (TODO 35.5) |
| Flutter | `lib/screens/member/profile_edit_screen.dart` (~L435) | Non-admins now get a **read-only** `Member Tier & Category` block (previously they saw nothing; the dropdown was admin-gated) | Done 2026-08-22 (TODO 35.5) |
| Flutter | `lib/screens/member/directory_screen.dart` (~L199) | `'Guest'` in the `_buildFilterDropdown('TYPE', [...])` list | Done 2026-08-22 (TODO 28.21) |
| Angular | `common/directory/directory.html` (~L61) | Membership-type filter now loops `MEMBERSHIP_TYPE_OPTIONS` (was a hardcoded list ending at Advisory, hiding Guest members) | Done 2026-08-22 (TODO 35.3) |
| Angular | `member/digital-id/digital-id.ts:31` | Local label array deleted; delegates to `getMembershipTypeLabel` (index 6 read `'Life'`, so Guest printed "Life" on the ID card) | Done 2026-08-22 (TODO 35.1) |
| Angular | `member/dashboard/dashboard.ts:83` | Same local map deleted; delegates to `getMembershipTypeLabel` | Done 2026-08-22 (TODO 35.2) |
| Angular | `core/models/business.models.ts:2` | `'Guest'` added to the `MembershipType` TS union | Done 2026-08-22 (TODO 35.4) |
| DB | `MembershipFeeConfigs` table | Admin should add fee config row for Guest type via Admin portal | Manual step |
| Tests | `GHCAA.Tests/OrgConfig/OrgConfigSeedTests.cs` | Asserts `MembershipTypeLabels` has 7 keys incl. Guest | Done (TODO 28.22) |
| Tests | `core/constants/app.constants.spec.ts` (new) | Pins `MEMBERSHIP_TYPES` / `MEMBERSHIP_TYPE_OPTIONS` / `getMembershipTypeLabel` against the domain enum; asserts no ordinal is ever labelled `Life` | Done 2026-08-22 (TODO 35.6) |
| Tests | `digital-id.spec.ts`, `dashboard.spec.ts`, `directory.spec.ts` | Per-component guards: ordinal 6 → `Guest Member`; the directory filter offers and forwards `Guest` | Done 2026-08-22 (TODO 35.6) |

> **Status corrected 2026-08-22.** The single "Flutter: add Guest to any hardcoded type list" row
> above was expanded into five, because it was both **stale** (three of the five sites were already
> done) and **mis-referenced** — it pointed at "Work Package 28.12" and the Tests row at "Work Package 28.11", but
> those are the Angular model/service items. The Guest sync item is **28.21** and the test item is
> **28.22**.
>
> The two remaining sites were *not* the same kind of gap, and were deliberately not fixed in one
> sweep:
>
> - `directory_screen.dart` was a **plain defect** — a member-directory filter that cannot select
> Guest silently hides every Guest member from search results. **Fixed 2026-08-22.**
> - `registration_constants.dart` is a **product decision**, not a typo, and is still open. That list drives the
> self-service registration form, so adding `Guest` there lets an applicant *self-select* Guest
> membership. If Guest is meant to be admin-assign-only (the likely intent, since Guest sits
> outside the fee tiers), the correct action is to **leave it out and document that**, not to add
> it for symmetry. **Superseded 2026-08-22 (later) — now tracked as TODO 35.5**, and the
> "admin-assign-only" reading is *disproved*: the **web** registration form has been offering
> Guest all along (`public/register/register.html:137` loops `MEMBERSHIP_TYPE_OPTIONS`). So this is
> a client-to-client inconsistency, and one of the two forms is wrong whichever way the product
> decision lands. **Resolved 2026-08-22 by TODO 35.5.** The product decision (user, verbatim):
> *"Guest - membership will be updated by admin, infact any membershiptypes only can be updated by
> admin."* So the answer went further than Guest — **no** tier is applicant-selectable. Both
> registration forms lost their tier control and the DTO property was deleted, which also closed a
> real privilege-escalation hole (a self-registration could previously request `Founding`). The
> Dart list was therefore **deleted rather than completed with `Guest`**. Admin, directory, label
> and fee-config paths keep every tier, and members still see their own tier read-only.
>
> **Table extended 2026-08-22 (Work Package 35).** The checklist tracked Backend / Angular-constants /
> Config / Flutter / DB / Tests and had **no rows at all** for the Angular *consumers* — the
> directory template, the two component-local label maps, or the TS union. That blind spot is
> exactly why all four drifted unnoticed while the constants file was correct: the web app kept
> three copies of the type list, two of which labelled index 6 `'Life'`, a value the enum has never
> had. The consumer rows are now listed above and pinned by tests. **Rule going forward: a component
> must never inline a MembershipType list — call `getMembershipTypeLabel()` or bind
> `MEMBERSHIP_TYPE_OPTIONS`.** Mobile is already correct by construction here: `dashboard_screen.dart`
> renders the API's string straight through, so a new enum value needs no mobile change.

> **Note:** `MembershipType` is stored as an int in the DB (EF default). `Guest = 6` appended at the end — no migration needed for the enum itself; no existing rows are affected.

---

## 9. Known Tech Debt (from Opus Review — 2026-05-30)

| ID | Issue | Severity | Area | Status |
|----|-------|----------|------|--------|
| TD-1 | `Constants.cs` branding/email sections are now duplicate sources of truth | Medium | 28.16 | Resolved 2026-09-05 (Work Package 62.7) — both fields moved into the org-config pack |
| TD-2 | No optimistic concurrency token on `OrganizationConfig` entity | Low | 28.17 | Open |
| TD-3 | `MembershipTypeLabels` in locale packs still require manual sync when enum changes | Medium | 28.18 | Open — see Work Package 62.33 |
| TD-4 | `Constants.Branding.*` and `Constants.EmailSubjects.*` not yet deleted/deprecated | Medium | 28.16 | Resolved 2026-09-05 — grep for `Branding`/`EmailSubjects` in `Constants.cs` returns nothing |

---

## 10. Phase 2 — Angular Consumer (DONE)

`GHCAA.Web/src/app/core/services/org-config.service.ts`:
1. Loads `GET /api/config` and exposes `config()` as an Angular Signal
2. Falls back to `ORG_CONFIG_FALLBACK` (`core/config/org-config-fallback.generated.ts`, build-time
   generated from the active profile pack — Work Package 62.15) if the API call fails
3. `localePack()` exposes the locale-appropriate strings for lookup

---

## 11. Phase 3 — Flutter Consumer (DONE)

`GHCAA.Mobile/lib/core/services/org_config_service.dart` loads config on app init, and
`lib/core/config/org_config.dart` supplies `OrgConfig.offlineDefaults` (Work Package 62.27) as the
offline/failure fallback in place of the old hardcoded GHC defaults.

---

## 12. How to Add a New Organization

Two different operations, easy to conflate:

**Rebrand an already-deployed instance at runtime** (this framework, unchanged since Phase 1):
1. Login as SuperAdmin → `/admin/org-config` → edit branding/contact/locale labels → Save
   (`PUT /api/config`). Takes effect immediately, no redeploy.

**Stand up a brand-new institution** (Work Package 62, profile packs):
1. Build a `profiles/<name>/` folder and set `ORG_PROFILE=<name>` before first boot. This is what
   the runtime admin form's defaults come from, on a database with no saved `OrganizationConfig`
   row yet. `docs/INSTITUTION_ONBOARDING.md` has the full deployer-facing checklist.
   `scripts/new-institution.mjs` (Work Package 62.39) will scaffold this folder automatically, but
   it has not been built yet, so for now the folder is built by hand.
