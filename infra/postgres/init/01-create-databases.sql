-- First-boot init for the shared PostgreSQL container.
-- The `postgres:17` image executes every file in /docker-entrypoint-initdb.d/
-- exactly once, the first time the data volume is empty.
--
-- The container already creates the default `sales_db` via $POSTGRES_DB; this
-- script adds the two extra logical databases for catalog and identity, so
-- every service owns its own (Database-per-Service, §2.3).

CREATE DATABASE catalog_db;
CREATE DATABASE identity_db;

GRANT ALL PRIVILEGES ON DATABASE catalog_db TO sales;
GRANT ALL PRIVILEGES ON DATABASE identity_db TO sales;
