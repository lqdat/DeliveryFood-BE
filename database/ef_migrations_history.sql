-- =====================================================
-- EF Core Migrations History Table
-- Run this AFTER create_tables.sql if using EF Core
-- =====================================================

CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" VARCHAR(150) NOT NULL PRIMARY KEY,
    "ProductVersion" VARCHAR(32) NOT NULL
);

-- Mark InitialCreate migration as applied (so EF won't try to re-create tables)
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260114000000_InitialCreate', '10.0.0')
ON CONFLICT ("MigrationId") DO NOTHING;

SELECT 'EF Migrations History table created!' AS result;
