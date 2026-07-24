---
name: GHCAA Full App Review - TODO Insertion Status
description: Tracks which files have had TODO comments inserted from the comprehensive security/quality review
type: project
originSessionId: 4a9de309-e521-4b97-9ae5-abc21ce99db9
---
# GHCAA Review — TODO Insertion Progress

**Status: COMPLETE** — All TODOs inserted. Resume not needed.

**Goal:** Read every critical file found in review, insert `// TODO:` comments inline at each issue.

**Why:** User ran a full app review (3 parallel agents — API, Infrastructure, Angular). Findings are saved in conversation. Now inserting TODOs into source files.

## Files ALREADY READ (content in context — add TODOs now):
- `GHCAA.API/Middleware/VisualTestAuthMiddleware.cs`
- `GHCAA.Application/Security/JwtSigningKeyResolver.cs`
- `GHCAA.Infrastructure/Services/OtpService.cs`
- `GHCAA.API/Hubs/ChatHub.cs`
- `GHCAA.API/Hubs/NotificationHub.cs`
- `GHCAA.Infrastructure/Gateways/BkashGateway.cs`
- `GHCAA.Infrastructure/Gateways/SSLCommerzGateway.cs`
- `GHCAA.API/Controllers/SecureFilesController.cs`
- `GHCAA.Web/src/app/core/services/auth.service.ts`

## Files STILL TO READ + TODO:
- `GHCAA.Infrastructure/Services/AuthService.cs` — brute force, timing attack, social login gaps
- `GHCAA.Infrastructure/Services/TokenService.cs` — 7-day token, role null claim
- `GHCAA.Infrastructure/Services/MemberService.cs` — membership# race, hard delete of rejected, N+1
- `GHCAA.Infrastructure/Data/ApplicationDbContext.cs` — missing indexes
- `GHCAA.API/Controllers/AdminController.cs` — returns plaintext default password + reset URL
- `GHCAA.API/Controllers/GatewaysController.cs` — client callback URL, hardcoded fee=500, adminId="1"
- `GHCAA.API/Controllers/SecureFilesController.cs` — path traversal (ALREADY READ — needs edit)
- `GHCAA.Web/src/app/core/interceptors/global-http.interceptor.ts` — auth header to all hosts
- `GHCAA.Web/src/app/public/magazine/magazine.html` — [innerHTML] XSS on public route
- `GHCAA.Application/Validators/MemberRegistrationValidator.cs` — missing field length/regex caps

## Key findings summary (all critical):
1. VisualTestAuthMiddleware — hardcoded backdoor tokens, no env guard
2. JwtSigningKeyResolver — known fallback key "LOCAL_DEVELOPMENT_JWT_FALLBACK_32_CHARS_MIN"
3. OtpService — `new Random()` (not CSPRNG), cleartext OTP, no attempt lockout, prior OTPs not invalidated
4. ChatHub — `Dictionary` not thread-safe, missing `[Authorize]`
5. NotificationHub — missing `[Authorize]`, anonymous JoinBatch/JoinDepartment
6. BkashGateway — no HMAC/signature on webhook, amount never verified vs order, no idempotency, `DefaultRequestHeaders` race on shared HttpClient
7. SSLCommerzGateway — no verify_sign hash check, amount/tran_id never verified, form body parser splits on `=` incorrectly
8. SecureFilesController — no canonical path check (path traversal via `..`)
9. auth.service.ts — JWT in localStorage, no token expiry check, JSON.parse without try/catch
10. AuthService — no brute force lockout, username enumeration via timing/log, social login bypasses email verification
11. TokenService — 7-day token lifetime, no refresh strategy
12. MemberService — membership# lexicographic race, `Remove()` on rejected member loses audit trail, N+1 in GetAllMembers
13. AdminController — ApproveMember returns plaintext default password; ResetPasswordAdmin returns reset URL
14. GatewaysController — BaseUrl taken from client (open redirect/SSRF), fee fallback hardcoded 500, adminId fallback "1"
15. magazine.html — `[innerHTML]` binds untrusted Quill HTML on public route = stored XSS
16. global-http.interceptor.ts — Authorization header sent to ALL hosts including third-party
