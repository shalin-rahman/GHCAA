---
name: session-portal-member-photo
description: "Member profile photo now shows in the top-right avatar across all /portal/* pages, sourced from ProfileService.getProfile().photoPath"
metadata: 
  node_type: memory
  type: project
  originSessionId: 80e0e324-13a0-4d9f-b036-6a6d47d35135
---

Added a real profile photo to the top-right header avatar in `GHCAA.Web/src/app/layouts/portal-layout/` (`portal-layout.ts`/`.html`/`.scss`) — this shared layout wraps every `/portal/*` route (including `/portal/id-card`), so the fix applies app-wide from one place. Falls back to the initial-letter avatar when `photoPath` is null. `AuthService.currentUser()`'s `User` model does not carry `photoPath` — it's fetched via `ProfileService.getProfile()`.

The sidebar-footer avatar (separate from the top-right header one) still only shows the initial letter — not changed, since the request was specifically "top right".

Seed data: `GHCAA.Infrastructure/Data/Seed/members.json` already has 581/584 members with `PhotoPath` correctly matched to their own NID/MembershipNumber against files in `GHCAA.API/wwwroot/uploads/members/seed/`. Three members have no genuine matching image and are intentionally left with null/empty `PhotoPath` (not force-assigned a borrowed photo): Id 1 "Demo Member" (NID 0000000001), Id 2 "Shalin Rahman" (NID 0000000002 — the usual test login), Id 374 "Md Jamal Hossain" (NID 2512245). To visually verify the photo feature in-browser, log in as a member that does have a matched photo, e.g. Id 201 "Md Habibur Rahman (Shalin)" (NID 2512005 → `uploads/members/seed/2512005.jpg`).

See [[gotcha_seed_json_vs_live_db]] for why editing seed JSON alone doesn't change the live SQLite DB.
