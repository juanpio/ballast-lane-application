-- Migration: 002_create_domain_orders
-- Creates the business-domain orders table with soft-delete support.
-- Uses bounded columns and explicit constraints in the domain schema.

CREATE SCHEMA IF NOT EXISTS domain;

CREATE TABLE IF NOT EXISTS domain.orders (
    id               VARCHAR(120)   PRIMARY KEY,
    customer_id      INTEGER        NOT NULL,
    order_date       TIMESTAMPTZ    NOT NULL,
    shipping_address VARCHAR(300)   NOT NULL,
    total_amount     NUMERIC(18,4)  NOT NULL,
    currency         VARCHAR(3)     NOT NULL,
    created_at       TIMESTAMPTZ    NOT NULL DEFAULT now(),
    updated_at       TIMESTAMPTZ    NOT NULL DEFAULT now(),
    deleted_at       TIMESTAMPTZ    NULL,

    CONSTRAINT fk_orders_customer
        FOREIGN KEY (customer_id)
        REFERENCES auth.users(id),
    CONSTRAINT ck_orders_shipping_address_not_empty
        CHECK (char_length(trim(shipping_address)) > 0),
    CONSTRAINT ck_orders_total_amount_positive
        CHECK (total_amount > 0),
    CONSTRAINT ck_orders_currency_allowed
        CHECK (currency IN ('USD', 'EUR'))
);

CREATE INDEX IF NOT EXISTS idx_orders_customer_active
    ON domain.orders (customer_id, updated_at DESC)
    WHERE deleted_at IS NULL;
