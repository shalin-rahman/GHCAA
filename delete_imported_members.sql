-- delete_imported_members.sql
-- Description: Removes members inserted by the auto-import process few minutes ago.
-- Criteria: NID starting with 'IMPORT-', Email starting with 'haragangian+', or FatherName starting with 'IMPORT-'

BEGIN;

-- 1. Identify targeted Member IDs into a temporary table to avoid re-evaluating
CREATE TEMP TABLE "TempImportedMembers" AS
SELECT "Id" FROM "Members"
WHERE "NID" LIKE 'IMPORT-%'
   OR "Email" LIKE 'haragangian+%'
   OR "FatherName" LIKE 'IMPORT-Father-%';

-- 2. Clear out any UserRoles associated with these Members' Users
DELETE FROM "UserRoles"
WHERE "UsersId" IN (
    SELECT "Id" FROM "Users"
    WHERE "MemberId" IN (SELECT "Id" FROM "TempImportedMembers")
);

-- 3. Delete from associated log/transaction tables if any exist for these users
-- (e.g. ActivityLogs or FileUploads mapped during import)
DELETE FROM "ActivityLogs"
WHERE "MemberId" IN (SELECT "Id" FROM "TempImportedMembers");

DELETE FROM "FileUploads"
WHERE "MemberId" IN (SELECT "Id" FROM "TempImportedMembers");

-- 4. Delete Users
DELETE FROM "Users"
WHERE "MemberId" IN (SELECT "Id" FROM "TempImportedMembers");

-- 5. Finally, delete the Members
DELETE FROM "Members"
WHERE "Id" IN (SELECT "Id" FROM "TempImportedMembers");

-- Cleanup
DROP TABLE "TempImportedMembers";

COMMIT;
