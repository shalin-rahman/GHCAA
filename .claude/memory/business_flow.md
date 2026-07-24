---
name: GHCAA business flow and domain model
description: End-to-end business flows for membership lifecycle, payment, events, and governance — non-obvious domain knowledge not derivable from code alone
type: project
---

## What GHCAA Is
Government Housing Colony Alumni Association (GHCAA) — a Bangladeshi alumni association platform managing membership registration, dues collection, events, governance, careers, and digital ID cards. Members are alumni of a specific institution (GHC). The platform has three client surfaces: Angular web portal, Flutter mobile app, and an ASP.NET Core API.

---

## Member Lifecycle (critical flow spanning many services)

```
[Public] Fills registration form (MemberRegistrationDto)
    → OTP email verification (OtpService)
    → Member created with Status=Applied, no User account yet
    → Uploads documents (certificate + payment proof)
    → Selects payment method (manual receipt / bKash / Nagad / SSLCommerz)
    → If online gateway → InitiatePayment → callback → HandleSuccessfulPayment → auto-approve
    → If manual → Admin reviews in Admin Portal
        → ApproveMember → creates User account → emails credentials → Status=Active
        → OR RejectMember → currently HARD DELETE (bug 24.30 — should soft-delete)
[Active Member]
    → Logs in with MembershipNumber (username) or Email or NID
    → JWT issued (7-day, SecurityStamp validated on every request via SecurityStampMiddleware)
    → Can access portal: Profile, ID Card, Certificate, Events, Directory, Jobs, Chat, Polls
    → MustChangePassword=true on first login → forced change
[Renewal / Dues]
    → Admin configures MembershipFeeConfigs per MembershipType
    → Member pays via financial portal → PaymentHistory recorded
[Archival]
    → Admin can soft-archive (IsArchived=true, HasQueryFilter excludes them from all queries)
    → SuperAdmin can bulk-archive inactive members, restore individuals
```

**Key constraint:** User account is only created AFTER approval — a Member row exists (Status=Applied) long before a User row exists. Never assume Member.User is non-null.

---

## MembershipStatus Enum (lifecycle states)
- `Applied` — registered, awaiting admin review
- `Active` — approved, has User account
- `Inactive` — lapsed / manually set
- `Suspended` — temporarily blocked
- `Rejected` — currently hard-deleted (bug 24.30)

## MembershipType Enum (fee tiers)
- `General`, `Associate`, `Honorary`, `LifeMember`, `Student` — each has a MembershipFeeConfig

---

## Payment Flow

**Manual (bKash/Nagad/Rocket/BankTransfer/ManualReceipt):**
Member uploads receipt → Admin manually verifies and approves.

**Online gateway (SSLCommerz / bKash API):**
1. Frontend calls `POST /api/gateways/initiate` with Amount + Gateway + Reference
2. API creates pending `PaymentHistory` row with TrxId
3. Returns `GatewayUrl` → frontend redirects user there
4. Gateway POSTs/GETs callback to `/api/gateways/callback/{gateway}`
5. `HandleSuccessfulPayment` marks PaymentHistory=Completed
6. If `Reference` is `EVT-REG-*` → auto-approves EventRegistration
7. If Member.Status=Applied and amount >= configured fee → auto-approves member (calls ApproveMemberAsync)

**Config source:** `AppSettings:PublicApiBaseUrl` (server-side) must be set — callback URL is derived from this, never from client request.

**Outstanding:** HMAC webhook verification not yet implemented (24.10, 24.11). Idempotency not yet implemented (24.13).

---

## Admin Roles & Authorization

Two roles enforced via policy:
- `AdminOnly` — Admin + SuperAdmin can access
- `SuperAdminOnly` — only SuperAdmin

**Strict parity rule:** Backend `[Authorize(Policy = "SuperAdminOnly")]` must always match frontend `superAdminGuard`. Comment `// Strict role parity: Sync with frontend superAdminGuard` marks these endpoints.

Admin identity in requests: always read from `User.FindFirst("MemberId")` JWT claim — never trust DTO body fields like `ApprovedByAdminId` (24.51 still TODO).

---

## OTP Flow

1. Member submits email → `GenerateAndSendOtpAsync` → invalidates all previous unverified OTPs → CSPRNG 6-digit code → sends email
2. Member submits code → `VerifyOtpAsync` → finds latest active OTP → checks attempt count (max 5) → increments on miss → marks IsVerified=true on success
3. OTP entity has: Email, Code (plaintext — 24.20 still TODO to hash), ExpiryAt, IsVerified, Attempts, Purpose, CreatedAt

---

## EC (Executive Committee) Governance

- `ECTerm` defines terms with start/end dates
- `GovernanceAssignment` links Member to ECTerm with a role (President, Secretary, Treasurer, etc.)
- Constraint: one EC role per member per term period (enforced in service layer)
- Constitution: versioned legal documents stored in DB, publicly readable
- Amendment voting: verified alumni only

---

## Events Flow

- `AlumniEvent` has RegistrationFee (null=free), MaxParticipants, IsPublic
- `EventRegistration` tracks member registrations with Status (Pending/Approved/Cancelled) and PaymentReference
- Free events: registration auto-approved
- Paid events: payment via gateway → callback → auto-approve registration
- QR attendance scanning by gatekeeper role
- Past events auto-closed by a background/scheduled process

---

## Security Stamp Invalidation

Every API request passes through `SecurityStampMiddleware` which validates the JWT's SecurityStamp claim against the DB. When password is changed/reset or admin archives a user, the SecurityStamp rotates → all existing JWTs are immediately invalid → user must re-login. This is the primary session revocation mechanism.

---

## Key Configuration Keys (required in production)
- `Jwt:Key` — HS256 signing key (min 32 chars); dev falls back to ephemeral random key with warning log
- `Jwt:Issuer`, `Jwt:Audience` — both "GHCAA"
- `AppSettings:PublicApiBaseUrl` — payment gateway callback base URL
- `AppSettings:AllowedOrigins` — CORS origins array
- `GeneralSettings:SystemAdminId` — used for auto-approval attribution
- `GeneralSettings:AssociationNamePrefix` — e.g. "GHC-" for membership numbers
- `OtpSettings:ExpiryMinutes` — default 10
- `DataProtection:KeyRingPath` — optional; omit for in-memory (dev only)
