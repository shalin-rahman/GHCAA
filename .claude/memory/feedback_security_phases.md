---
name: Security hardening S1–S9 + Area 24 completion status
description: All security hardening phases complete; all 60 Area 24 items done; 296 tests pass
type: project
---

Security hardening phases S1–S9 and all Area 24 items are complete as of 2026-05-03. 296/296 tests pass.

**Why:** Full application security review requested by user; 60 tasks documented in TODO.md Area 24.

**What was implemented:**
- S1: Auth backdoors — VisualTestAuthMiddleware gated to dev+Visual profile; ephemeral JWT key; dev password cleared; ChatHub/NotificationHub [Authorize]
- S2: Sensitive data leaks — AdminController removes DefaultPassword/ResetUrl from responses; SocialAuthController masks ClientSecret; PaymentConfigController masks gateway keys via MaskSecrets()
- S3: OTP hardening — CSPRNG (RandomNumberGenerator.GetInt32), invalidate previous OTPs on new generation, 5-attempt lockout
- S4: Payment gateway security — BkashGateway uses per-request HttpRequestMessage (no DefaultRequestHeaders race); SSLCommerz uses QueryHelpers.ParseQuery; GatewaysController derives CallbackUrl from AppSettings:PublicApiBaseUrl; fee/admin fallbacks fail loudly
- S5: Auth hardening — User.FailedLoginAttempts + LockoutUntil fields; lockout after 5 failures (15 min); dummy BCrypt for timing normalization; SecurityStamp rotated on password change/reset; GenerateDefaultPassword uses CSPRNG
- S6: Path/file security — SecureFilesController canonicalizes path, rejects ".." and anything outside uploads root
- S7: Frontend security — auth.service.ts getUserFromStorage wrapped in try/catch + JWT exp check; interceptor only injects auth header for /api/ requests; authGuard redirects mustChangePassword users; gallery.ts uses noopener,noreferrer
- S8: DB indexes + validators — UserConfiguration adds indexes on MemberId, ResetToken (filtered), GoogleId (filtered), FacebookId (filtered); MemberRegistrationValidator adds MaximumLength on all text fields, age gate (13–120), phone regex for emergency contact, AdmissionYear < PassingYear rule
- S9: CSP/Headers — X-XSS-Protection removed; CSP script-src 'unsafe-inline' removed

**How to apply:** These are done — no re-implementation needed. A database migration is still needed for User.FailedLoginAttempts + LockoutUntil fields and the new indexes.
