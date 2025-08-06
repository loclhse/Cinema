-- Database initialization script for team03_db
-- This script will be executed when PostgreSQL container starts

\echo 'Starting database initialization for team03_db...'

-- Create extensions if needed
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- Set timezone
SET timezone = 'UTC';

-- Create basic tables structure (example - adjust based on your actual schema)
-- You can add your actual database schema here

\echo 'Database initialization completed successfully!'
\echo 'Database team03_db is ready for use.'
