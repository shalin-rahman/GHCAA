# Constants and Magic-Value Classification (docs/TODO.md 82.12)

`docs/materials/REVIEW.md` §25.8 asks for every constant across the three clients to be classified
as a technical constant, an environment value, an organisation value, an administrator-managed
value, or a business policy. The standing rule that a repeated literal gets a named constant
already applies to every change in this repo — the value here is the classification itself: which
of the constants below are generic (stay in code) and which are GHC-specific (belong in the Work
Package 62 profile pack instead). This does not rename or move anything; it raises the
organisation-specific rows as Work Package 62 expanded items.

Classification key (matches REVIEW.md §25.8):

| Value Type | Recommended Location |
| --- | --- |
| Technical | Code (as today) |
| Environment | Environment configuration |
| Organisation | Organisation configuration (WP62 profile pack) |
| Admin | Database/admin configuration |
| Policy | Controlled configuration/domain model |

## GHCAA.Domain/Constants.cs

| Group | Classification | Reason |
| --- | --- | --- |
| `Roles` (SuperAdmin/Admin/Member) | Technical | Referenced by `[Authorize(Roles=...)]` and seeded rows; changing these requires a migration anyway. Generic across any institution using this codebase. |
| `Policies` | Technical | Registered once in `AddAppAuthorization`; same reasoning as `Roles`. |
| `RateLimitPolicies` | Technical | Rate-limit *policy names* are wiring, not organisation data — the numeric limits behind each policy (in `Program.cs`'s `AddRateLimiter`) are the actual tunable and are already read from config there, not from this file. |
| `OutputCachePolicies` | Technical | Same as above — cache policy identifiers, not values. |
| `ConfigKeys` | Technical | These are the *names* of config keys (correctly centralized so no service repeats a string literal — see 82.17), not the values themselves. Generic. |
| `TemplateCodes` (OTP_EMAIL, WELCOME_EMAIL, ...) | Technical | Codes the notification pipeline switches on; the *content* behind each code is already admin-managed (`EmailTemplate` table). Generic. |
| `Headers.CorrelationId` | Technical | Protocol-level header name, not organisation data. |
| `Defaults.MaxFileSizeBytes` / image compression defaults | Policy | These are fallback values for `FileStorageOptions` (82.17) — a real tunable an operator might reasonably want to raise/lower per deployment. Environment-configurable already (`appsettings.json`); the constant is just the fallback when the key is absent. Correctly policy, not organisation. |
| `Defaults.UnknownValue`, `Defaults.ImportPrefix` | Technical | Internal sentinel/prefix values with no organisation-specific meaning. |
| `ErrorLogs.LevelError/LevelWarning`, `RetentionDays`, `MaxPageSize` | Policy | Retention/paging are operational policy an admin might reasonably want to change; today hardcoded. Not organisation-specific (no institution needs a different value), so this is a "make it configurable" candidate for a future item, not a WP62 item. |

No GHC-specific literal exists in `Constants.cs` — the file is already generic. This matches its
`namespace GHCAA.Domain` position: nothing here needed a WP62 profile-pack entry.

## GHCAA.Web/src/app/core/constants/app.constants.ts (533 lines) — read for classification only, not edited (Angular is out of scope for this change)

| Group | Classification | Reason |
| --- | --- | --- |
| `SEARCH_DEBOUNCE_MS` | Technical | UI timing constant, generic. |
| `EC_ROLES` / `EC_ROLES_OPTIONS` | Organisation | Executive Committee role names/order are GHCAA's own governance structure (President, Secretary, ...); another institution's committee shape differs. **Raise against WP62** as a profile-pack field, alongside the existing `workflow.membershipTypes` pattern already in `OrganizationConfig`. |
| `LOOKUP_GROUPS` | Technical | Names of server-backed lookup groups (`MembershipStatus`, `Gender`, ...), not the option values themselves — the values already come from the DB-backed lookup service. Generic. |
| `DATE_FORMAT`, `DATE_REGEX` | Technical | This is the two-format date contract (ADR-0001), deliberately fixed across the whole platform, not per-institution. |
| `DEVELOPER_INFO` (name/email of a specific person) | Organisation | Hardcoded personal attribution (a real name and email), not a technical constant. **Raise against WP62** — a white-labeled deployment should not ship someone else's developer contact by default; belongs in the profile pack or is dropped from client-facing code entirely. |
| `MEMBERSHIP_STATUS_MAP`, `PAYMENT_STATUS_MAP`, `PLEDGE_STATUS_MAP`, `SUBMISSION_STATUS(_MAP)` | Technical | UI label/CSS-class mappings for enum values shared with the backend; the enums themselves are code (see `Enums.MembershipStatus` row above), so the display mapping is the same kind of technical constant. |
| `MEMBERSHIP_TYPES`, `MEMBERSHIP_TYPE_OPTIONS` | Organisation | Falls back to hardcoded values when `OrganizationConfig.Workflow.MembershipTypes` isn't loaded yet; the source of truth is already WP28/62 admin config — this is a fallback duplicate, not a second real source. No new WP62 item; flag as a fallback that should read only from `OrganizationConfig` once it's guaranteed loaded before this file's consumers run. |
| `MEMBER_CATEGORIES`, `BLOOD_GROUPS`, `GENDERS`, `TSHIRT_SIZES`, `ARTICLE_CATEGORIES` | Technical | Same fallback-for-DB-lookup pattern as `LOOKUP_GROUPS` — generic reference data, not GHC-specific (blood groups and genders don't vary by institution). |
| `POST_TYPE_TABS` | Technical | UI tab labels for the News/Notice merge (ADR-05); generic. |
| `FINANCIAL_CATEGORY_OPTIONS` | Technical | Mirrors `Enums.FinancialCategory`; generic. |
| `ACADEMIC_CERTIFICATES`, `ACADEMIC_SUBJECTS`, `ACADEMIC_DATA`, `IS_HSC`, `ensureValidAcademicData` | Organisation | HSC/SSC certificate levels and subject lists are specific to the Bangladeshi education system GHCAA's members come from. **Raise against WP62** — another institution (especially outside Bangladesh) needs a different certificate/subject taxonomy; today it's hardcoded assuming every deployment shares GHCAA's alumni background. |
| `PROFESSIONAL_SECTORS` | Organisation | Same reasoning as academic data — a fixed, GHCAA-context sector list. Lower priority than academic data (sectors generalize better across institutions) but still worth a WP62 profile-pack field if this codebase is reused elsewhere. |
| `ROUTES`, `API_ENDPOINTS` | Technical | Application routing/API contract, not organisation data. |

## GHCAA.Mobile/lib/core/constants/app_constants.dart — read for classification only, not edited (Flutter is out of scope for this change)

| Group | Classification | Reason |
| --- | --- | --- |
| `paddingSmall/Medium/Large/ExtraLarge`, `radiusSmall/Medium/Large/ExtraLarge` | Technical | Aliases onto `AppTheme.space*`/`radius*` design tokens — generic design-system values, not organisation data. |
| `durationFast/Medium/Slow` | Technical | Animation timing, generic. |
| `idCardAspectRatio` | Technical | A fixed physical card ratio, not institution-specific. |

## GHCAA.Mobile/lib/core/constants/registration_constants.dart

| Group | Classification | Reason |
| --- | --- | --- |
| `LookupGroups` (membershipStatus, userStatus, ...) | Technical | Names of server-backed lookup groups, mirrors the web `LOOKUP_GROUPS` row above — generic. |
| `AcademicConstants` (certificates, hscSubjects, generalSubjects) | Organisation | Same Bangladeshi-education-system dependency as the web `ACADEMIC_*` constants. **Raise against WP62** together with the web-side entry — this is the same data duplicated per client, which is itself a cross-platform duplication REVIEW.md §25.6 would also flag, so the WP62 fix (move to `OrganizationConfig`/profile pack) closes both gaps at once. |
| `MembershipConstants.bloodGroupOptions` | Technical | Same reasoning as the web `BLOOD_GROUPS` row — generic reference data. |

## Summary: items to raise against Work Package 62

1. `EC_ROLES` / `EC_ROLES_OPTIONS` (web) — Executive Committee role names.
2. `DEVELOPER_INFO` (web) — hardcoded personal attribution.
3. `ACADEMIC_CERTIFICATES` / `ACADEMIC_SUBJECTS` / `ACADEMIC_DATA` (web) and `AcademicConstants`
   (mobile) — Bangladeshi education-system taxonomy, duplicated across both clients.
4. `PROFESSIONAL_SECTORS` (web) — GHCAA-context sector list, lower priority.

Everything else in scope classifies as technical, policy, or an already-correct WP28 fallback and
needs no new configuration mechanism, consistent with REVIEW.md §25.8's instruction not to move
every constant into configuration.
