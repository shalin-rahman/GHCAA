# Chapter 8 — Security, Privacy and Trust

*[Chapter not written. The headings below are generated from `docs/DOCUMENTATION_BOOK_OUTLINE.md` and are kept in step with it: `build.py --strict` fails if a section exists in one and not the other.]*

**What this chapter owns.** The threat model, the controls that answer each threat, where each
control is enforced in the code, and the risk left over.

**What it must not repeat.** The security architecture's rationale, which is §6.7. The
implementation narrative of the security code, which is §7.10. The execution of security tests,
which is §9.10. Every control here names the threat it mitigates; a control that names no threat does
not belong in the chapter.

## 8.1 Security Objectives and Assumptions

*[Not written.]*

## 8.2 Threat Modelling (STRIDE)

*[Not written. Brief: assets, entry points, trust boundaries, enumerated threats]*

## 8.3 Authentication and Session Security

Three threats against the session model are named here; §7.10 has the mechanism and the code for each.

A JWT normally stays valid until it expires, so the threat is a token that should be dead — because
the account was disabled, the password changed, or a security response demanded it — but is not,
simply because it has not yet reached its expiry. The control is `SecurityStampMiddleware`
(§7.10), which checks a rotating stamp against the database on every request and rejects the token the
moment the two disagree, so revocation does not wait on token lifetime.

A refresh token is redeemed once and replaced, but rotation alone does not tell the legitimate holder
and a thief apart if the thief redeems a copy first. The threat is that theft going undetected while
the stolen token is still used. The control, in `TokenService.RotateRefreshTokenAsync` (§7.10, ticket
82.18), treats a revoked token being presented again as the signal that a copy was stolen and revokes
every refresh token belonging to that user, not only the one presented.

A session that authenticates a user for ordinary use is not the same guarantee that the person at the
keyboard right now is still them, and an admin session left open at a shared desk is the concrete case.
The threat is a destructive or financial action carried out on someone else's authority through a
session that was never re-confirmed. The control is `RequireStepUpAttribute` (§7.10), applied to 15
actions across six controllers, which requires a re-authentication claim no older than thirty minutes
before it lets the request through.

## 8.4 Authorisation Model and the Role–Permission Matrix

*[Not written.]*

## 8.5 Input Validation and Output Sanitisation

*[Not written.]*

## 8.6 File Upload Security

*[Not written.]*

## 8.7 Transport, Header and Browser-Policy Security

*[Not written.]*

## 8.8 Rate Limiting and Abuse Prevention

*[Not written.]*

## 8.9 Payment-Related Risk and the No-Gateway-Keys Posture

*[Not written. Brief: the security rationale for manual verification and its accepted operational cost]*

## 8.10 Governance Integrity

*[Not written. Brief: the scope of the voting features as sentiment and internal decision-making rather than as a secure-election system, with reference to §2.7]*

## 8.11 Personal Data: Lawful Basis, Minimisation, Consent, Retention and Subject Rights

*[Not written.]*

## 8.12 Audit Logging and Non-Repudiation

Two separate mechanisms answer this, not one. `AuditLogMiddleware` (`GHCAA.API/Middleware/AuditLogMiddleware.cs`,
lines 20-56) is a generic HTTP-level log: it records any non-GET request, and any request under
`/api/admin`, as an activity row through `IActivityService.LogActivityAsync` once the response
succeeds and the caller's user id can be read from the token. It does not know or care what kind of
data a request touched — that distinction is made separately, per entity.

`docs/ARCHITECTURE.md` (lines 118-149) sets that distinction as Class A and Class B. An entity is
Class A "if a row of it is evidence: money received or spent, a governance decision, a membership
status, or anything a member could later dispute" — currently `FinancialRecord`, `PaymentHistory`,
`MembershipDue`, `MembershipHistory`, `Member`, `User`, `ECMember`, `Constitution` and `Poll`. A Class
A row is never hard-deleted: it carries `HasQueryFilter(x => !x.IsArchived)` so an ordinary read
already excludes it once archived, a caller that genuinely needs the archived rows back asks for them
explicitly with `IgnoreQueryFilters()`, and a service method that archives a Class A row takes the
acting admin's id as a required argument — the controller returns `Unauthorized()` rather than record
the act as done by nobody when it cannot identify the caller. Everything else is Class B: content and
configuration that can be recreated if lost, which carries only a creation timestamp and can be hard-deleted
without the same argument for keeping it. The `IsArchived` name was chosen deliberately: three
Class A entities (`FinancialRecord`, `PaymentHistory`, `ECMember`) had used `IsDeleted` for the same
soft-delete behaviour before the naming was unified under ticket 82.30.

Non-repudiation for a Class A row therefore rests on the row itself — who archived it and when, kept
rather than erased — and audit logging rests on `AuditLogMiddleware`'s separate, undifferentiated
request log. Neither is aware of the other, and no test exercises `AuditLogMiddleware` directly; its
coverage today is incidental, through the controller tests that call the endpoints it wraps.

## 8.13 Conformance Assessment against OWASP ASVS

*[Not written.]*

## 8.14 Residual Risks and Recommendations

*[Not written.]*

## 8.15 Summary

*[Not written.]*

## Figures and Tables

*[Not drawn. The outline specifies 11 artefacts for this chapter. Each is added here with its caption as it is made, then `renumber.py --apply` is run.]*

