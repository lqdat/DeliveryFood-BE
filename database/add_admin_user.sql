-- Add Admin user to existing database
-- Run this script to add Admin account for testing

INSERT INTO "Users" ("Id", "PhoneNumber", "Email", "FullName", "PasswordHash", "Role", "AvatarUrl", "IsPhoneVerified", "CreatedAt", "UpdatedAt", "IsDeleted")
VALUES (
    'a0000000-0000-0000-0000-000000000000',
    '0900000000',
    'admin@example.com',
    'Quản Trị Viên',
    '$2a$11$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/X5.AQxeC/O/fwoMwy', -- Password: 123456
    4, -- Admin role
    'https://randomuser.me/api/portraits/men/4.jpg',
    true,
    NOW(),
    NOW(),
    false
)
ON CONFLICT ("PhoneNumber") DO NOTHING;

-- Verify the Admin user was created
SELECT "Id", "PhoneNumber", "FullName", "Role" FROM "Users" WHERE "PhoneNumber" = '0900000000';
