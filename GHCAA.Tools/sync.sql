SELECT "FullName", "MembershipNumber", "IsArchived", "IsVerified", "Status" FROM "Members" LIMIT 10;
SELECT COUNT(*) FROM "Members" WHERE "IsArchived" = false AND "Status" = 1;
