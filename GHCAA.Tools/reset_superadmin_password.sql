-- Run this in PostgreSQL to reset superadmin password to: SuperAdminPassword123!
UPDATE "Users"
SET "PasswordHash" = '$2a$11$CQ4KnTDZ7qUQMNre86iruOpgOx8fEoMe2G3RF/1U4cCLa5ltYtE1O'
WHERE "Username" = 'superadmin';

-- Verify
SELECT "Id", "Username", "IsActive", "IsArchived" FROM "Users" WHERE "Username" = 'superadmin';
