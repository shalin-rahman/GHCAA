DO $$
BEGIN
    -- 1. Clears out Roles associated with target users
    BEGIN
        DELETE FROM "UserRoles" WHERE "UsersId" IN (
            SELECT "Id" FROM "Users" WHERE "Username" NOT IN ('superadmin', 'shalin')
        );
    EXCEPTION WHEN OTHERS THEN
        NULL;
    END;

    -- 2. Clear out other records tied directly to Users
    BEGIN
        DELETE FROM "Otps" WHERE "UserId" IN (
            SELECT "Id" FROM "Users" WHERE "Username" NOT IN ('superadmin', 'shalin')
        );
    EXCEPTION WHEN OTHERS THEN
        NULL;
    END;

    BEGIN
        DELETE FROM "ChatMessages" 
        WHERE "SenderId" IN (SELECT "Id" FROM "Users" WHERE "Username" NOT IN ('superadmin', 'shalin'))
           OR "ReceiverId" IN (SELECT "Id" FROM "Users" WHERE "Username" NOT IN ('superadmin', 'shalin'));
    EXCEPTION WHEN OTHERS THEN
        NULL;
    END;

    BEGIN
        DELETE FROM "NewsPosts" WHERE "AuthorId" IN (
            SELECT "Id" FROM "Users" WHERE "Username" NOT IN ('superadmin', 'shalin')
        );
    EXCEPTION WHEN OTHERS THEN
        NULL;
    END;

    BEGIN
        DELETE FROM "Notifications" WHERE "UserId" IN (
            SELECT "Id" FROM "Users" WHERE "Username" NOT IN ('superadmin', 'shalin')
        );
    EXCEPTION WHEN OTHERS THEN
        NULL;
    END;

    BEGIN
        DELETE FROM "EmailLogs" WHERE "UserId" IN (
            SELECT "Id" FROM "Users" WHERE "Username" NOT IN ('superadmin', 'shalin')
        );
    EXCEPTION WHEN OTHERS THEN
        NULL;
    END;

    -- 3. Clear out records tied directly to Members we are trying to delete.
    -- (We keep members whose Id matches the MemberId of 'superadmin' and 'shalin')
    BEGIN
        DELETE FROM "ActivityLogs" WHERE "MemberId" NOT IN (
            SELECT "MemberId" FROM "Users" WHERE "Username" IN ('superadmin', 'shalin') AND "MemberId" IS NOT NULL
        );
    EXCEPTION WHEN OTHERS THEN
        NULL;
    END;

    BEGIN
        DELETE FROM "FileUploads" WHERE "MemberId" NOT IN (
            SELECT "MemberId" FROM "Users" WHERE "Username" IN ('superadmin', 'shalin') AND "MemberId" IS NOT NULL
        );
    EXCEPTION WHEN OTHERS THEN
        NULL;
    END;

    BEGIN
        DELETE FROM "PaymentHistories" WHERE "MemberId" NOT IN (
            SELECT "MemberId" FROM "Users" WHERE "Username" IN ('superadmin', 'shalin') AND "MemberId" IS NOT NULL
        );
    EXCEPTION WHEN OTHERS THEN
        NULL;
    END;

    BEGIN
        DELETE FROM "MembershipHistories" WHERE "MemberId" NOT IN (
            SELECT "MemberId" FROM "Users" WHERE "Username" IN ('superadmin', 'shalin') AND "MemberId" IS NOT NULL
        );
    EXCEPTION WHEN OTHERS THEN
        NULL;
    END;

    BEGIN
        DELETE FROM "MembershipDues" WHERE "MemberId" NOT IN (
            SELECT "MemberId" FROM "Users" WHERE "Username" IN ('superadmin', 'shalin') AND "MemberId" IS NOT NULL
        );
    EXCEPTION WHEN OTHERS THEN
        NULL;
    END;

    BEGIN
        DELETE FROM "JobOpportunities" WHERE "PostedByMemberId" NOT IN (
            SELECT "MemberId" FROM "Users" WHERE "Username" IN ('superadmin', 'shalin') AND "MemberId" IS NOT NULL
        );
    EXCEPTION WHEN OTHERS THEN
        NULL;
    END;

    BEGIN
        DELETE FROM "EventRegistrations" WHERE "MemberId" NOT IN (
            SELECT "MemberId" FROM "Users" WHERE "Username" IN ('superadmin', 'shalin') AND "MemberId" IS NOT NULL
        );
    EXCEPTION WHEN OTHERS THEN
        NULL;
    END;

    BEGIN
        DELETE FROM "ECMembers" WHERE "MemberId" NOT IN (
            SELECT "MemberId" FROM "Users" WHERE "Username" IN ('superadmin', 'shalin') AND "MemberId" IS NOT NULL
        );
    EXCEPTION WHEN OTHERS THEN
        NULL;
    END;

    BEGIN
        DELETE FROM "FinancialRecords" WHERE "MemberId" NOT IN (
            SELECT "MemberId" FROM "Users" WHERE "Username" IN ('superadmin', 'shalin') AND "MemberId" IS NOT NULL
        );
    EXCEPTION WHEN OTHERS THEN
        NULL;
    END;

    -- 4. Finally, safely delete the target Users
    BEGIN
        DELETE FROM "Users" WHERE "Username" NOT IN ('superadmin', 'shalin');
    EXCEPTION WHEN OTHERS THEN
        NULL;
    END;

    -- 5. Delete the target Members
    BEGIN
        DELETE FROM "Members" WHERE "Id" NOT IN (
            SELECT "MemberId" FROM "Users" WHERE "Username" IN ('superadmin', 'shalin') AND "MemberId" IS NOT NULL
        );
    EXCEPTION WHEN OTHERS THEN
        NULL;
    END;

END $$;
