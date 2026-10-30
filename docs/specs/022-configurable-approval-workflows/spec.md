# Configurable approval workflows

**Feature Branch**: `022-configurable-approval-workflows`  
**Status**: Draft for implementation planning  
**Tracker**: Work Package 92 in `docs/TODO.md`  
**Depends on**: spec 002 for state transitions and authorization contracts; WP91 module boundaries are a design constraint, not a prerequisite.

## Purpose and scope

Approval routes currently use a fixed Admin policy. The existing `MemberApprovalMode` setting is saved but not read. This specification defines one configurable approval policy for each supported flow, while preserving current behavior by default.

In scope: member registration, event registration, gallery album/photo, job posting, news article, manual payment verification, member import, the pending-approvals queue, and election nomination only to the extent the shared policy mechanism serves it. Election-specific UI and workflows remain in the election plan.

Out of scope: family-link requests, constitutional amendments, and approval workflows that do not yet exist. A future scholarship approval uses this mechanism when that feature is built.

## Requirements

- Each flow has an enabled state and the roles permitted to approve or reject it.
- Defaults preserve current behavior: approval enabled and the existing Admin and SuperAdmin access.
- Member registration uses the existing `MemberApprovalMode`: `Auto` maps to approval disabled and `ManualReview` maps to approval enabled.
- Proposed: when approval is disabled, the create path sets the approved state and writes the audit entry required by the existing workflow. Confirm in TODO 92.1 before implementation.
- Proposed: SuperAdmin retains the protected override described in TODO 92.1. Confirm before implementation.
- Proposed: member import remains restricted to Admin and SuperAdmin. Confirm in TODO 92.1 before implementation.
- The pending-approvals queue applies the same flow-specific access rule as approve/reject actions.
- Settings changes are available only to SuperAdmin and are audited.
- Shared policy evaluation sits behind an Application interface so a later module boundary can own it without changing endpoint behavior.
- Web and Mobile hide actions the current user cannot perform; the API remains the authorization authority.
- Flow identifiers and defaults are centralized constants. API changes are recorded in `docs/API_CONTRACT_REGISTRY.md`.

## Decisions to confirm before implementation

TODO 92.1 explicitly leaves four product choices open. The proposed defaults below follow the implementation direction in 92.2; they are not recorded as owner-approved decisions.

| Question | Proposed default | Alternative to settle |
|---|---|---|
| Where do settings live? | Organization configuration under an approvals section. | A dedicated settings table. Inspect configuration ownership and migration constraints before choosing. |
| What happens when approval is off? | Create the item approved and write an audit entry. | Skip the queue while retaining a pending state or audit-only event; this changes how clients and downstream flows interpret the item. |
| Can SuperAdmin always approve? | Yes, preserve the protected SuperAdmin override. | Apply the configured role list to SuperAdmin too, which could remove the existing recovery path. |
| Who can approve member imports? | Keep Admin and SuperAdmin only. | Allow configured roles, increasing the number of roles able to mutate member records in bulk. |

Record the selected answers in this section and close TODO 92.1 before starting backend implementation. The defaults in the requirements are conditional on those choices being confirmed.

## Acceptance

1. Existing installations retain current approval behavior until an administrator changes a setting.
2. For each flow, tests cover enabled, disabled, allowed role, refused role, and default configuration.
3. Disabled approval creates an approved record and its audit evidence.
4. SuperAdmin-only settings routes reject other roles; every setting change is audited.
5. The pending queue and approve/reject actions agree on flow permissions.
6. Web and Mobile show only permitted actions, and the API still rejects unauthorized requests.
7. SQLite and PostgreSQL migrations apply, and authorization reflection tests match the new policy.
8. Spec 002, `business_flow` notes, `PROJECT_MAP.md`, and the role-permission matrix and figure in the book are synchronized.

## Failure and compatibility rules

- Missing settings use the documented default and cannot silently disable review.
- An unknown flow key is rejected and logged; it does not grant approval access.
- A failed audit write fails the settings mutation or approval transition as the current audit contract requires.
- No client-side role check may replace API authorization.
- Existing pending records remain actionable under the configured flow policy; changing a setting does not rewrite prior records.

## Task breakdown

See [tasks.md](./tasks.md). Tracker status and acceptance remain in `docs/TODO.md`.
