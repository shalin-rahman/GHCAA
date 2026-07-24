---
name: gotcha-seed-json-vs-live-db
description: "Editing GHCAA.Infrastructure/Data/Seed/*.json does NOT change the already-created SQLite DB's Members table on its own — how seeding actually reaches the live DB"
metadata: 
  node_type: memory
  type: project
  originSessionId: 80e0e324-13a0-4d9f-b036-6a6d47d35135
---

`GHCAA.Infrastructure/Data/Seed/members.json` (and other seed JSON files) are only consumed by EF Core's design-time `HasData()`/migration-snapshot mechanism — baked in once at migration-authoring time. Editing the JSON after that migration was generated has **no effect** on an already-created `GHCAA.API/GHCAADB.db` unless a new migration is authored, or the DB is dropped/recreated.

The only runtime re-seed path in `GHCAA.API/Program.cs` (`OverrideEFCoreMigratedData`) is gated by `ASP_SEED_PROFILE == "Visual"` and only overwrites the `Users` table (delete-all + re-insert from `Data/Seed/Visual/users.json`) — it never touches `Members`.

Confirmed empirically (2026-07-13): the live DB's `Members.PhotoPath` for 581/584 members already matched what's in `members.json` — meaning those were baked in at migration time, correctly matching each member's own NID/MembershipNumber to a seed image filename in `GHCAA.API/wwwroot/uploads/members/seed/`. Only a manual post-hoc JSON edit (not yet migrated) diverged from the DB.

**Why:** without knowing this, it looks like a seed-data edit "did nothing" when tested in-browser — easy to mistake for a code bug in the feature being tested, when it's actually a stale-DB issue.

**How to apply:** before debugging "my seed data change isn't showing up," always cross-check the live DB directly (no `sqlite3` CLI in this env — use `python -c "import sqlite3; ..."` against `GHCAA.API/GHCAADB.db`) rather than assuming the JSON is authoritative. If a genuine data fix must reach the live DB without a full migration, either (a) create a proper EF Core migration, or (b) directly `UPDATE` the SQLite row via Python as a dev-only stopgap — never treat editing the seed JSON alone as sufficient.

See [[session_portal_member_photo]].
