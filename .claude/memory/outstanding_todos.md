---
name: ghcaa-outstanding-todos
description: Area 24 + UI/security/usability review plan (API+Web+Mobile) both complete. Only pending DB migration remains.
metadata: 
  node_type: memory
  type: project
  originSessionId: 80e0e324-13a0-4d9f-b036-6a6d47d35135
---

## Status: ALL AREA 24 ITEMS COMPLETE + UI/SECURITY/USABILITY REVIEW PLAN COMPLETE

All 24.xx security items have been implemented across 5 sessions. 296/296 tests pass.

A follow-up cross-cutting review plan (`glowing-skipping-sparkle.md`) covering UI layout/design/theme/security/usability across API, Web, and Mobile is now fully implemented — see [[session_ui_security_usability_review.md]] for what changed.

Mobile JWT refresh flow is DONE (resolves the old concern below): `/auth/refresh` + `/auth/refresh-mobile` exist with dedicated rate-limit policies, and the Flutter app has a working refresh flow (`api_client.dart` interceptor) — no longer forces re-login every 60 min.

---

## Pending Migration

Two migrations generated but NOT yet applied to the PostgreSQL dev database (password auth required). Apply when DB is accessible:

```
dotnet ef database update --project GHCAA.Infrastructure --startup-project GHCAA.API --context ApplicationDbContext
```

### Migration 1: `PhaseB_S5S8_OtpHmac_PaymentIdempotency_Indexes`
Adds:
- `User.FailedLoginAttempts` (int default 0), `User.LockoutUntil` (DateTime?)
- `PaymentHistory.GatewayPaymentId` (varchar 255, UNIQUE partial index)
- `Otp.Code` widened to varchar(64) for HMAC-SHA256 hex
- Composite index on `Member(Status, IsArchived)`
- Indexes on User(MemberId), User(ResetToken IS NOT NULL), User(GoogleId), User(FacebookId)
- Composite index on Otp(Email, ExpiryAt)

### Migration 2: RefreshTokens table (generate before applying)
The `RefreshToken` entity and `RefreshTokenConfiguration` exist in code but migration not yet generated.
Generate with:
```
dotnet ef migrations add AddRefreshTokens --project GHCAA.Infrastructure --startup-project GHCAA.API --context ApplicationDbContext
```
Table: `RefreshTokens` (Id, UserId FK→Users, TokenHash varchar(64) UNIQUE, ExpiresAt, CreatedAt, IsRevoked)
Indexes: TokenHash (unique), (UserId, IsRevoked) composite
