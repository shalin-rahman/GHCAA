# Shared member-data sub-components — design for TODO 33.12

Status: **Design only — not implemented.** This document proposes an approach for a future
implementation task; no code changes are included in this pass.

## Problem

`member/profile/profile.html` (the member-facing profile page) and the admin member-detail
modal in `admin/members/admin-members.html` (opened via `openDetail()` in `admin-members.ts`)
both render the same underlying `Member` entity — EC/governance history, academic history,
professional history, emergency contact, address — but each view is a fully independent
template. Whenever one is edited (as in Work Package 33's 33.5–33.10 fixes), the other silently drifts
out of sync in section order, labels, and visual treatment (raised as findings in 33.1 and
33.2, triaged as 33.12).

## Why extraction is feasible

Both templates bind to the same field names on the underlying model:

- `academicHistory`, `professionalHistory`, `ecHistory` (arrays, same shape in both views)
- `emergencyContactName` / `emergencyContactRelation` / `emergencyContactPhone`
- `presentAddress` / `permanentAddress`

The member page binds these off a `profile` object (`profile.ts`); the admin modal binds them
off `selectedMember()` (`admin-members.ts`). Property names are otherwise identical, so a
shared sub-component driven by `@Input()`/`@Output()` on these same slices is structurally
sound — the two consumers differ in **presentation and permissions**, not in data shape.

## Differences to reconcile before extraction

| Aspect | Member profile (`profile.html`, post-33.10) | Admin modal (`admin-members.html`) |
|---|---|---|
| Section order | Membership status → Photo/Signature → Personal Details → Academic (detail) → Professional (detail) → Emergency → Address → Privacy → Notifications → **EC/Academic/Professional read-only recaps (bottom)** | Personal/Identity → Governance & Tier → Address → Emergency → Privacy → Notifications → Verification/Gamification → Academic → Professional → EC History → Payment History → Documents |
| EC History label | "Association Governance History" | "EC Position History" |
| Address heading | "Correspondence Identity" | (no heading; inline fields) |
| Emergency heading | "Emergency Contact Details" | "Emergency Contact Details" (matches) |
| Duplication | Academic/Professional shown twice: a read-only "snapshot" row plus a separate detailed editable timeline | Single instance each, no snapshot/detail split |
| Edit gating | Single `<form>` around the whole page, one submit action | Single `isEditing()` flag toggles nearly every field's read-vs-edit state (except Payment History, which is always read-only) |
| Anchors | Has `id="section-academic-history"` / `id="section-professional-history"` for in-page nav | No section anchors |

None of these differences are hard blockers, but they mean **extraction is not a pure
copy/paste** — each shared sub-component needs to accept enough inputs (heading text override,
read-only vs. edit-mode flag, whether to show the anchor `id`) to satisfy both call sites, or
the two pages need to first agree on canonical wording/order (independent of this doc's scope).

## Proposed component boundary

One presentational component per data section, each owning its own template/edit-state logic
and taking the relevant model slice as input:

- `<app-academic-history-editor [items]="academicHistory" [editable]="..." (itemsChange)="...">`
  — replaces both the member "Educational Timeline" snapshot + "Detailed Academic Milestones"
  edit block, and the admin "Academic Records" block.
- `<app-professional-history-editor [items]="professionalHistory" [primaryDesignation]="..." [editable]="..." (itemsChange)="...">`
  — replaces the member "Professional Experience" snapshot + "Detailed Career Timeline" edit
  block, and the admin "Professional Records" block.
- `<app-ec-history-view [items]="ecHistory" [editable]="..." (changeReasonSubmit)="...">`
  — replaces the member "Association Governance History" read-only card and the admin
  "EC Position History" block (which needs an edit-mode change-reason banner the member view
  does not).
- `<app-emergency-contact-form [(contact)]="emergencyContact">` — shared as-is; labels already
  match between the two views.
- `<app-address-form [(addresses)]="addresses">` — shared, with an optional `[showHeading]`
  input since the admin modal currently has no heading for this block.

Each component should be a dumb/presentational Angular component (no HTTP calls of its own),
receiving data and emitting change events, so both `profile.ts` and `admin-members.ts` keep
owning their own save/submit flow exactly as today.

## Follow-up (out of scope here)

1. Agree on canonical section order and label wording across both pages (a product/UX call,
   not a purely technical one) — resolve this before or during extraction, not as a
   prerequisite blocking this design doc.
2. Resolve the member-side snapshot vs. detail duplication for Academic/Professional as part of
   the extraction (the shared component should not carry forward the duplication).
3. File a new implementation TODO item once this design is reviewed and the ordering/labeling
   questions above are settled.
