---
name: Area 24 completion — new entities, endpoints, utilities added
description: All new code added in the Area 24 security hardening sessions; read before exploring auth, rate-limiting, file-upload, or news/magazine paths
type: project
---

## New Domain / Infrastructure

### RefreshToken entity
- **File**: `GHCAA.Domain/Models/RefreshToken.cs`
- **Fields**: Id, UserId (FK→Users), TokenHash varchar(64) SHA-256 hex, ExpiresAt, CreatedAt, IsRevoked
- **Config**: `GHCAA.Infrastructure/Data/Configurations/RefreshTokenConfiguration.cs`
- **DbSet**: `ApplicationDbContext.RefreshTokens`
- **EF migration**: NOT YET APPLIED — generate `AddRefreshTokens` migration first

### ITokenService extended (Application/Interfaces/ITokenService.cs)
New methods:
- `string GenerateRefreshToken()` — 32-byte cryptographically random base64
- `Task StoreRefreshTokenAsync(int userId, string plaintextToken, CT)` — hashes and persists
- `Task<(string NewToken, int UserId)?> RotateRefreshTokenAsync(string plaintextToken, CT)` — validate → revoke old → issue new
- `Task RevokeAllRefreshTokensAsync(int userId, CT)` — logout, bulk ExecuteUpdateAsync

### TokenService changes (Infrastructure/Services/TokenService.cs)
- JWT lifetime: **7 days → 60 minutes** (24.27)
- Constructor now requires `ApplicationDbContext db` (4th param)
- Null-safe role claims: `.Where(r => !string.IsNullOrEmpty(r.Name))`

### NewsService changes (Infrastructure/Services/NewsService.cs)
- `using Ganss.Xss;` + static `HtmlSanitizer _sanitizer`
- `CreateNewsAsync` and `UpdateNewsAsync` now sanitize `dto.Content` via `_sanitizer.Sanitize(dto.Content ?? "")` before storing (24.42)
- `HtmlSanitizer` NuGet added to both `GHCAA.Infrastructure` and `GHCAA.Tests` projects

## New API Endpoints (AuthController)

| Method | Route | Auth | Purpose |
|--------|-------|------|---------|
| POST | /api/auth/refresh | Anonymous | Rotate refresh token cookie → new access_token cookie |
| GET | /api/auth/me | [Authorize] | Return username/role/memberId from JWT claims |
| POST | /api/auth/logout | [Authorize] | Revoke all refresh tokens + clear cookies |

Login/Google/Facebook now also call `SetAuthCookiesAsync` which:
1. Sets `access_token` httpOnly cookie (65 min)
2. Generates + stores + sets `refresh_token` httpOnly cookie (7 days)

## Middleware (GHCAA.API/Middleware/)
- **LoginRateLimitMiddleware.cs** (NEW, 24.48): Intercepts POST /api/auth/login, buffers body, extracts `username` → `HttpContext.Items["LoginUsername"]`; must run BEFORE `UseRateLimiter()`
- Registration in Program.cs: `app.UseMiddleware<LoginRateLimitMiddleware>()` before `app.UseRateLimiter()`

## Rate Limiting Change (Program.cs)
- "auth" policy changed from `AddFixedWindowLimiter` (global) to `AddPolicy<string>` (PartitionedRateLimiter)
- Partition key: `$"{ip}:{username}"` (from LoginRateLimitMiddleware) — test env key: `"__test__"`

## JWT Cookie Auth (ServiceExtensions.cs)
`OnMessageReceived` extended: if no `Authorization` header present, reads `access_token` cookie as fallback. Bearer header still takes priority (mobile/API clients unaffected).

## Angular Changes

### auth.service.ts
- Token no longer stored in localStorage — only in-memory signal
- `sessionStorage.user_session` stores display fields only (no token)
- Legacy localStorage migrated to sessionStorage on first load
- Constructor calls `GET /api/auth/me` (withCredentials) if no session to restore from cookie
- New `refresh()` method → POST /api/auth/refresh
- `logout()` now calls POST /api/auth/logout (revokes server-side) then clears local state

### global-http.interceptor.ts  
- All /api/ requests get `withCredentials: true` (sends httpOnly cookies automatically)
- 401 queue: `BehaviorSubject<boolean> refreshDone$` + `isRefreshing` flag
- On 401: queue concurrent requests, call `authService.refresh()`, retry all on success, logout on failure
- Skips refresh for /api/auth/login and /api/auth/refresh to avoid infinite loops

### New API endpoint constants (app.constants.ts AUTH section)
`REFRESH: '/api/auth/refresh'`, `ME: '/api/auth/me'`, `LOGOUT: '/api/auth/logout'`

## New Angular Utility
- **`GHCAA.Web/src/app/core/utils/file-validation.util.ts`** (24.45)
- `validateUploadFile(file: File, kind: 'image' | 'pdf' | 'image-or-pdf'): string | null`
- Images ≤ 5 MB (JPEG/PNG/WebP), PDFs ≤ 10 MB
- Applied in: register.ts, gallery.ts, admin-members.ts, admin-events.ts, articles.ts

## Test Changes
- `GHCAA.Tests/Services/TokenServiceTests.cs`: 5 new tests; switched from InMemory to SQLite (required for ExecuteUpdateAsync support)
- `GHCAA.Tests/Controllers/AuthControllerTests.cs`: Updated constructor to 3-param (IAuthService, ITokenService, db); mocks GenerateRefreshToken + StoreRefreshTokenAsync
- `GHCAA.Tests/GHCAA.Tests.csproj`: Added HtmlSanitizer package reference

## Test count: 296/296 .NET pass; 229/229 Angular unit pass (57 files)

## Angular spec fixes applied (post-Area-24 session)
- `tsconfig.spec.json`: Added `"rootDir": "."` to fix TS6059 common-source-directory error
- `vitest.config.ts`: Added `exclude: ['tests/e2e/**', 'tests/visual/**']` so Playwright files don't pollute vitest
- `auth.spec.ts`: Added `RouterTestingModule`; flush `GET /api/auth/me` 401 in beforeEach
- `auth.service.spec.ts`: Flush `GET /api/auth/me` 401 in beforeEach; flush `POST /api/auth/logout` in logout test; changed localStorage assertions to sessionStorage
- `login.spec.ts`: Added `getSocialProviders: vi.fn().mockReturnValue(of([]))` to auth mock
- `register.spec.ts`: Added `GatewaysService` and `FinancialService` mocks (ngOnInit calls `finService.getApplicableFee`)
- `gallery.spec.ts`: Added `AuthService` and `NotificationService` mocks (Gallery injects both)
- `admin-events.spec.ts`: Added `NavService` mock with `isSuperAdmin: () => false` (template uses `nav.isSuperAdmin()`)
