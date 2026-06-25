# Configuration-Driven Framework — Implementation Guide

**Status:** Phase 1 (Backend + DB) COMPLETE | Phase 2 (Angular Consumer) TODO | Phase 3 (Flutter Consumer) TODO  
**Last Updated:** 2026-05-30  
**Branch:** `preprod`

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
| `GHCAA.Domain/Models/OrganizationConfig.cs` | Domain | ✅ Created |
| `GHCAA.Application/DTOs/OrgConfigDto.cs` | Application | ✅ Created |
| `GHCAA.Application/Interfaces/IOrgConfigService.cs` | Application | ✅ Created |
| `GHCAA.Infrastructure/Services/OrgConfigService.cs` | Infrastructure | ✅ Created |
| `GHCAA.Infrastructure/Data/Configurations/OrganizationConfigConfiguration.cs` | Infrastructure | ✅ Created |
| `GHCAA.Infrastructure/Data/ApplicationDbContext.cs` | Infrastructure | ✅ Modified (DbSet added) |
| `GHCAA.API/Controllers/OrgConfigController.cs` | API | ✅ Created |
| `GHCAA.API/Program.cs` | API | ✅ Modified (seed + DI) |
| `GHCAA.Domain/Enums.cs` | Domain | ✅ Modified (Guest added) |
| `GHCAA.Web/src/app/core/constants/app.constants.ts` | Angular | ✅ Modified (Guest added) |

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
| Backend | `GHCAA.Domain/Enums.cs:6` | `Guest` appended to end of enum | ✅ Done |
| Angular | `app.constants.ts:39` | `'Guest Member'` in `MEMBERSHIP_TYPES` | ✅ Done |
| Angular | `app.constants.ts:283` | `{ value: 'Guest', label: 'Guest Member' }` in `MEMBERSHIP_TYPE_OPTIONS` | ✅ Done |
| Config | `OrgConfigService.cs` | `["Guest"] = "Guest Member"` / `"অতিথি সদস্য"` in both locales | ✅ Done |
| Flutter | `GHCAA.Mobile/lib/core/...` | Add `'Guest'` to any hardcoded type list | ⏳ TODO (Area 28.12) |
| DB | `MembershipFeeConfigs` table | Admin should add fee config row for Guest type via Admin portal | Manual step |
| Tests | `GHCAA.Tests/` | Update any test asserting exact membership type count | ⏳ TODO (Area 28.11) |

> **Note:** `MembershipType` is stored as an int in the DB (EF default). `Guest = 6` appended at the end — no migration needed for the enum itself; no existing rows are affected.

---

## 9. Known Tech Debt (from Opus Review — 2026-05-30)

| ID | Issue | Severity | Area |
|----|-------|----------|------|
| TD-1 | `Constants.cs` branding/email sections are now duplicate sources of truth | Medium | 28.16 |
| TD-2 | No optimistic concurrency token on `OrganizationConfig` entity | Low | 28.17 |
| TD-3 | `MembershipTypeLabels` in locale packs still require manual sync when enum changes | Medium | 28.18 |
| TD-4 | `Constants.Branding.*` and `Constants.EmailSubjects.*` not yet deleted/deprecated | Medium | 28.16 |

---

## 10. Phase 2 — Angular Consumer (TODO)

Create `OrgConfigService` Angular service that:
1. Loads `GET /api/config` via `APP_INITIALIZER` before any component renders
2. Exposes `config()` as an Angular Signal
3. Exposes `t(path, locale?)` for locale string lookup
4. Falls back to `GHCAA_DEFAULT_CONFIG` if API call fails
5. Exposes `isEnabled(feature)` for feature-gate guards

See `Area 28` tasks 28.1–28.9 in TODO.md.

---

## 11. Phase 3 — Flutter Consumer (TODO)

Create `OrgConfigService` Dart service that:
1. Loads `GET /api/config` on app init via `OrgConfigService.instance.load(apiClient)`
2. Caches in `SharedPreferences` for offline resilience
3. Exposes `pack` accessor returning locale-appropriate `LocalePack`
4. Updates `app_drawer.dart` hardcoded strings to read from `pack.nav.*`

See `Area 28` tasks 28.10–28.14 in TODO.md.

---

## 12. How to Add a New Organization (Once All 3 Phases Complete)

1. Login as SuperAdmin
2. Navigate to `/admin/org-config`
3. Fill in branding, contact, locale labels
4. Click Save → `PUT /api/config`
5. All three platforms reflect new organization instantly
6. No code change. No redeployment.
