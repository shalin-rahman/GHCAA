-- Fix local test account passwords per TODO.md (Phase 3 ENV-005)
-- Run via: scripts/fix-local-test-passwords.ps1
-- Hashes generated with BCrypt work factor 11

UPDATE "Users"
SET "PasswordHash" = '$2a$11$BwV.B0dnfPt4sSFuf/7HNul5eCTYPBPmK2MSDgE0bYVFlHDU78KAS',
    "FailedLoginAttempts" = 0,
    "LockoutUntil" = NULL
WHERE "Username" = 'shalin';

-- demo_user: create if missing (Member role, linked to Demo Member id=1)
INSERT INTO "Users" ("Id", "Username", "PasswordHash", "MemberId", "CreatedAt", "IsActive", "IsArchived", "MustChangePassword", "SecurityStamp", "FailedLoginAttempts")
SELECT 9999, 'demo_user', '$2a$11$UB8WCuiDh0vge6h./.WxdeLXC9rwRyPHCWU7N5j2pBZZmxpjBJh3u', 1, NOW() AT TIME ZONE 'UTC', true, false, false, gen_random_uuid()::text, 0
WHERE NOT EXISTS (SELECT 1 FROM "Users" WHERE "Username" = 'demo_user');

UPDATE "Users"
SET "PasswordHash" = '$2a$11$UB8WCuiDh0vge6h./.WxdeLXC9rwRyPHCWU7N5j2pBZZmxpjBJh3u',
    "MemberId" = COALESCE("MemberId", 1),
    "FailedLoginAttempts" = 0,
    "LockoutUntil" = NULL,
    "IsActive" = true
WHERE "Username" = 'demo_user';

-- Assign Member role (RolesId=3) if not present
INSERT INTO "UserRoles" ("RolesId", "UsersId")
SELECT 3, u."Id"
FROM "Users" u
WHERE u."Username" = 'demo_user'
  AND NOT EXISTS (
    SELECT 1 FROM "UserRoles" ru WHERE ru."UsersId" = u."Id" AND ru."RolesId" = 3
  );
