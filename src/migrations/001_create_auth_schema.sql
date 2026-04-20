-- Migration: 001_create_auth_schema
-- Creates the auth schema and users table for identity/authentication data.
-- Follows ADR 004 (auth schema isolation) and ADR 005 (parameterized SQL only for DML).

CREATE SCHEMA IF NOT EXISTS auth;

CREATE TABLE IF NOT EXISTS auth.users (
    id            SERIAL PRIMARY KEY,
    email         VARCHAR(254)  NOT NULL,
    password_hash VARCHAR(255)  NOT NULL,
    created_at    TIMESTAMPTZ   NOT NULL DEFAULT now(),
    updated_at    TIMESTAMPTZ   NOT NULL DEFAULT now(),

    CONSTRAINT ck_users_email_not_empty CHECK (char_length(trim(email)) > 0),
    CONSTRAINT ck_users_password_hash_not_empty CHECK (char_length(password_hash) > 0)
);

-- Unique index on email to enforce one account per address (case-insensitive duplicates
-- are prevented at the application layer by normalising to lowercase before insert).
CREATE UNIQUE INDEX IF NOT EXISTS idx_users_email ON auth.users (email);
