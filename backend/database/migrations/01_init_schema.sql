CREATE TABLE IF NOT EXISTS ACCOUNT (
    account_id SERIAL PRIMARY KEY,
    username VARCHAR(50) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    full_name VARCHAR(255) NOT NULL,
    account_role VARCHAR(20) NOT NULL DEFAULT 'Sale'
);

CREATE TABLE IF NOT EXISTS CATEGORY (
    category_id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    description TEXT
);

CREATE TABLE IF NOT EXISTS PRODUCT (
    product_id SERIAL PRIMARY KEY,
    sku VARCHAR(100) UNIQUE NOT NULL,
    name VARCHAR(255) NOT NULL,
    price INTEGER NOT NULL CHECK (price >= 0),
    stock INTEGER NOT NULL CHECK (stock >= 0),
    description TEXT,
    images TEXT[], -- Array of image URLs
    category_id INTEGER REFERENCES CATEGORY(category_id),
    created_time TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_time TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS CUSTOMER (
    customer_id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    phone VARCHAR(20),
    address TEXT
);

CREATE TABLE IF NOT EXISTS ORDERS (
    order_id SERIAL PRIMARY KEY,
    created_time TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_time TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    final_price INTEGER NOT NULL,
    status VARCHAR(50) NOT NULL, -- 'Created', 'Paid', 'Cancelled'
    customer_id INTEGER REFERENCES CUSTOMER(customer_id),
    account_id INTEGER REFERENCES ACCOUNT(account_id),
    shipping_address TEXT
);

CREATE TABLE IF NOT EXISTS ORDER_ITEM (
    order_item_id SERIAL PRIMARY KEY,
    order_id INTEGER REFERENCES ORDERS(order_id) ON DELETE CASCADE,
    product_id INTEGER REFERENCES PRODUCT(product_id),
    quantity INTEGER NOT NULL,
    unit_sale_price INTEGER NOT NULL,
    total_price INTEGER NOT NULL
);

-- ==========================================
-- Triggers for auto-updating updated_time
-- ==========================================

-- 1. Create a generic function to update the updated_time column
CREATE OR REPLACE FUNCTION update_modified_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_time = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- 2. Attach the trigger to the PRODUCT table
DROP TRIGGER IF EXISTS trigger_product_updated_time ON PRODUCT;
CREATE TRIGGER trigger_product_updated_time
BEFORE UPDATE ON PRODUCT
FOR EACH ROW
EXECUTE FUNCTION update_modified_column();

-- 3. Attach the trigger to the ORDERS table
DROP TRIGGER IF EXISTS trigger_orders_updated_time ON ORDERS;
CREATE TRIGGER trigger_orders_updated_time
BEFORE UPDATE ON ORDERS
FOR EACH ROW
EXECUTE FUNCTION update_modified_column();
