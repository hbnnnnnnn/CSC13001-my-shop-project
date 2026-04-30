-- Migration: Add cost_price column to products table for internal cost tracking / profit analytics
ALTER TABLE PRODUCT
ADD COLUMN IF NOT EXISTS cost_price INTEGER NOT NULL DEFAULT 0 CHECK (cost_price >= 0);
