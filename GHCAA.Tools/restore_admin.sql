INSERT INTO "Users" ("Id", "Username", "PasswordHash", "IsActive", "IsArchived", "CreatedAt") 
VALUES (1, 'superadmin', '$2a$11$yUryc8gFlef8/.jJugVivORnhn76z3IW1HsiAiRjrIvZfPltqSlaC', true, false, NOW())
ON CONFLICT ("Id") DO UPDATE SET "Username" = 'superadmin', "PasswordHash" = '$2a$11$yUryc8gFlef8/.jJugVivORnhn76z3IW1HsiAiRjrIvZfPltqSlaC';

INSERT INTO "UserRoles" ("RolesId", "UsersId") 
VALUES (1, 1)
ON CONFLICT ("RolesId", "UsersId") DO NOTHING;

INSERT INTO "Users" ("Id", "Username", "PasswordHash", "IsActive", "IsArchived", "CreatedAt")
VALUES (2, 'shalin', '$2a$11$CQ4KnTDZ7qUQMNre86iruOpgOx8fEoMe2G3RF/1U4cCLa5ltYtE1O', true, false, NOW())
ON CONFLICT ("Id") DO UPDATE SET "Username" = 'shalin', "PasswordHash" = '$2a$11$CQ4KnTDZ7qUQMNre86iruOpgOx8fEoMe2G3RF/1U4cCLa5ltYtE1O';

INSERT INTO "UserRoles" ("RolesId", "UsersId") 
VALUES (1, 2)
ON CONFLICT ("RolesId", "UsersId") DO NOTHING;
