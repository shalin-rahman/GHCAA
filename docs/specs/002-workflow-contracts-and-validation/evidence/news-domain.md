# News/Article Approval Domain: State Transitions, Validation, and Endpoint Contracts

**Reviewed**: 2026-09-21. Covers `NewsController.cs`, `NewsService.cs`,
`NewsDto.cs`, `Enums.cs`. Part of
[../tasks.md](../tasks.md) T002/T008/T011, following the
[events-domain.md](./events-domain.md) template.

## 1. Submission state transitions

`Enums.SubmissionStatus` (`GHCAA.Domain/Enums.cs:38`, nested in a static
`Enums` class, referenced as `Enums.SubmissionStatus`): `Draft`, `Pending`,
`Approved`, `Rejected`.

| From | To | Trigger | Who | Where | Side effects |
|---|---|---|---|---|---|
| (none) | `Draft` | Submit article, explicitly requesting Draft | Member (non-admin) | `POST /api/news/submit` → `NewsController.cs:113-128`, `NewsService.CreateNewsAsync` | `IsActive` forced `false`; row created |
| (none) | `Pending` | Submit article (any status other than `Draft`, non-admin caller) | Member (non-admin) | Same endpoint (`NewsController.cs:124-125`) | `dto.Status` overwritten to `Pending` server-side regardless of what caller sent; `IsActive` forced `false` |
| (none) | `Approved` (default) | `CreateNewsDto.Status` defaults to `Approved` (`NewsDto.cs:53`) | Admin only — `POST /api/news` is `AdminOnly`; non-admins go through `/submit`, which overrides the default | `POST /api/news` → `NewsController.cs:70`, `NewsService.CreateNewsAsync` | Post created directly `Approved`, no approval step for admin-authored posts |
| `Pending` (or any) | `Approved` | Approve | Admin (`AdminOnly`) | `POST /api/news/{id}/approve` or `/admin/{id}/approve` → `NewsService.ApproveArticleAsync` (`NewsService.cs:161`) | Sets `Status = Approved`, `IsActive = true`, `PublishDate = UtcNow` — no check that prior status was `Pending`; callable on an already-`Approved` or even `Rejected`/`Draft` post |
| `Pending` (or any) | `Rejected` | Reject | Admin (`AdminOnly`) | `POST /api/news/{id}/reject` or `/admin/{id}/reject` → `NewsService.RejectArticleAsync` (`NewsService.cs:174`) | Sets `Status = Rejected`, `IsActive = false` — same lack of prior-state guard |
| `Rejected`/`Draft`/`Approved` | (no re-submit path) | — | — | — | No endpoint moves a post back to `Pending` after rejection; the author would need `UpdateNews` (admin-only) or a fresh submission |

**Gaps carried forward, not resolved here:**
- `Draft` is reachable only via `/submit` when the member explicitly sets
  `Status = Draft`; no listing endpoint filters on `Draft`
  (`GetPendingSubmissionsAsync` filters only `Pending`;
  `GetMySubmissionsAsync` returns all statuses for that author). Draft posts
  are visible to their author but never surfaced for admin action.
- `ApproveArticleAsync`/`RejectArticleAsync` have no guard on the current
  `Status` — both are callable on a post in any state. This is a real gap
  compared to the Events domain, where check-in/invitation actions gate on
  `Status == Approved`.
- `UpdateNewsAsync` never sets `Status` from the DTO (comment at
  `NewsService.cs:134-137`, tagged WP 57.4) specifically so an update can't
  accidentally re-approve a pending post — status changes are meant to go
  only through approve/reject.
- Non-admin `POST /api/news` (the non-`/submit` route) is blocked by
  `[Authorize(Policy = AdminOnly)]`, so a member can't reach the
  `Approved`-by-default path directly.
- Not read in this pass: whether the `NewsPost` model has an `ApprovedBy`
  audit column that `ApproveArticleAsync` simply isn't populating, or
  whether no such column exists at all.

## 2. Validation matrix

| DTO | Field | Rule | Enforced by |
|---|---|---|---|
| `CreateNewsDto` | `Title` | Required, 5-300 chars | `[Required]`/`[MinLength]`/`[MaxLength]` data annotations |
| `CreateNewsDto` | `Content` | Required, min 20 chars | `[Required]`/`[MinLength]` data annotations |
| `CreateNewsDto` | `ImageUrl` | Must be relative path or absolute http(s) URL | Custom `RelativeOrAbsoluteUrlAttribute` (`NewsDto.cs:12-21`) |
| `CreateNewsDto` | `AttachmentFileName` | Max 260 chars | `[MaxLength(260)]` |
| `CreateNewsDto` | `AttachmentUrl` | No format rule found | **Flagged: no enforced rule** |
| `CreateNewsDto` | `ArticleCategory`, `PostType` | No enum-value validation beyond model binding | **Flagged: no enforced rule** (binder rejects out-of-range ints, but no explicit check) |
| `CreateNewsDto`/submit | `Status` | On `/submit`, controller inline logic overrides the caller's value for non-admins (`NewsController.cs:119-127`) rather than validating it; `POST /api/news` (admin) has no equivalent check, so an admin can set any `Status` directly | Controller inline check on `/submit` only |
| `CreateNewsDto`/submit | `PostType == Notice` for non-admin | Rejected with 403 | Controller inline check (`NewsController.cs:121-122`), not a validator class |
| `UpdateNewsDto` | all `CreateNewsDto` fields | Same as above (inherits; no `IValidatableObject` override, unlike `CreateEventDto`) | Inherited DataAnnotations only |
| `UpdateNewsDto` | `Status` | Accepted on the DTO but deliberately ignored by the service (`NewsService.cs:134-137`) | Service-level no-op, not a validator |
| Image/document upload | `IFormFile` | Category/size validated (`FileCategory.Image`/`Document`, 10 MB max) | `FileValidationService.ValidateFormFile` (`NewsController.cs:172,202`) |
| Approve/Reject | `id` existence | `FindAsync` returns null → controller returns `NotFound()` | Service-level null check, not a validator |
| `AddCollaboratorAsync` | duplicate collaborator | No-op (returns true) if already present | Service-level check (`NewsService.cs:188-189`), not a validator |

No FluentValidation validator found anywhere in this domain — `CreateNewsDto`
and `UpdateNewsDto` rely on DataAnnotations plus one custom
`ValidationAttribute`.

## 3. Endpoint contract table

| Route | Method | Request DTO | Response | Success | Documented failures | Auth |
|---|---|---|---|---|---|---|
| `/api/news` | GET | — (query: `articleCategory?`, `postType?`) | `NewsPostDto[]` (only `IsActive && Approved`) | 200 | — | Anonymous |
| `/api/news/{id:int}` | GET | — | `NewsPostDto` | 200 | 404 | Anonymous |
| `/api/news/admin` | GET | — | `NewsPostDto[]` (all statuses) | 200 | — | AdminOnly |
| `/api/news/pending`, `/api/news/admin/pending`, `/api/news/News/Pending` (mobile alias) | GET | — | `NewsPostDto[]` (Pending only) | 200 | — | AdminOnly |
| `/api/news` | POST | `CreateNewsDto` | `NewsPostDto` (201, `CreatedAtAction`) | 201 | 401 (bad author claim) | AdminOnly |
| `/api/news/{id:int}` | PUT | `UpdateNewsDto` (Id overwritten from route) | `NewsPostDto` | 200 | not read in this pass — no explicit 404; `UpdateNewsAsync` throws `KeyNotFoundException` unhandled by the controller | AdminOnly |
| `/api/news/{id:int}` | DELETE | — | — | 200 | 404 | AdminOnly |
| `/api/news/my-submissions` | GET | — | `NewsPostDto[]` (by author) | 200 | 401 | Authenticated |
| `/api/news/submit` | POST | `CreateNewsDto` | `NewsPostDto` (201) | 201 | 401 (bad claim), 403 (`PostType == Notice` for non-admin) | Authenticated |
| `/api/news/{id:int}/approve`, `/api/news/admin/{id:int}/approve` | POST | — | `{ Message }` | 200 | 404 | AdminOnly |
| `/api/news/{id:int}/reject`, `/api/news/admin/{id:int}/reject` | POST | — | `{ Message }` | 200 | 404 | AdminOnly |
| `/api/news/{id:int}/collaborators/{userId:int}` | POST | — | `{ Message }` | 200 | 400 (via `Problem()`) | AdminOnly |
| `/api/news/{id:int}/collaborators/{userId:int}` | DELETE | — | `{ Message }` | 200 | 404 | AdminOnly |
| `/api/news/upload-image` | POST (multipart) | `IFormFile file` | `{ url, relativePath }` | 200 | 400 (bad file), 401 | Authenticated |
| `/api/news/upload-document` | POST (multipart) | `IFormFile file` | `{ url, relativePath, fileName }` | 200 | 400 (bad file), 401 | AdminOnly |

Two GET routes (`/api/news`, `/api/news/{id}`) carry
`[OutputCache(PolicyName = PublicContent)]`. Both are anonymous/public-content
only, so this is unrelated to the per-user leak documented in
`session_outputcache_crossuser_leak_fix.md`.

Pagination: none of these routes accept `cursor`, `page`, or `pageSize` —
every list endpoint returns a full array, same as Events. News is not on the
offset/cursor split described in
`001-platform-baseline/contracts/api-cross-layer.md`.
