-- Clear old data (to avoid duplicate errors when re-running this script)
TRUNCATE TABLE ORDER_ITEM CASCADE;
TRUNCATE TABLE ORDERS CASCADE;
TRUNCATE TABLE PRODUCT CASCADE;
TRUNCATE TABLE CUSTOMER CASCADE;
TRUNCATE TABLE CATEGORY CASCADE;
TRUNCATE TABLE ACCOUNT CASCADE;

-- Reset identity sequences (IDs back to 1)
ALTER SEQUENCE account_account_id_seq RESTART WITH 1;
ALTER SEQUENCE category_category_id_seq RESTART WITH 1;
ALTER SEQUENCE product_product_id_seq RESTART WITH 1;
ALTER SEQUENCE customer_customer_id_seq RESTART WITH 1;
ALTER SEQUENCE orders_order_id_seq RESTART WITH 1;
ALTER SEQUENCE order_item_order_item_id_seq RESTART WITH 1;

-- 1. Insert Accounts
-- Default password is '123456' (in production this will be a real bcrypt hash)
INSERT INTO ACCOUNT (username, password_hash, full_name, account_role) VALUES
('admin', 'hashed_123456', 'Administrator', 'Admin'),
('sale1', 'hashed_123456', 'Sales Staff 1', 'Sale');

-- 2. Insert Categories
INSERT INTO CATEGORY (name, description) VALUES
('Smartphones', 'Mobile phones and smartphones from all brands'),
('Laptops', 'Laptops and desktop computers'),
('Accessories', 'Earphones, charging cables, adapters, phone cases');

-- 3. Insert Customers
INSERT INTO CUSTOMER (name, phone, email, address) VALUES
('John Smith', '0901234567', 'john@example.com', '123 Main Street, District 1, HCMC'),
('Jane Doe', '0987654321', 'jane@example.com', '456 Second Street, District 2, HCMC');

-- 4. Insert Products
-- Smartphones (category_id = 1)
INSERT INTO PRODUCT (sku, name, price, stock, description, images, supplier, category_id) VALUES
('IP15PM', 'iPhone 15 Pro Max 256GB', 29000000, 50, 'Apple iPhone 15 Pro Max 2023. Super-light titanium build.', ARRAY['https://picsum.photos/id/1/600/600', 'https://picsum.photos/id/2/600/600', 'https://picsum.photos/id/3/600/600'], 'Apple', 1),
('SS24U', 'Samsung Galaxy S24 Ultra', 27000000, 30, 'Samsung 2024 AI flagship with built-in S-Pen.', ARRAY['https://picsum.photos/id/4/600/600', 'https://picsum.photos/id/5/600/600', 'https://picsum.photos/id/6/600/600'], 'Samsung', 1);

-- Laptops (category_id = 2)
INSERT INTO PRODUCT (sku, name, price, stock, description, images, supplier, category_id) VALUES
('MBP14', 'MacBook Pro 14 M3', 35000000, 20, 'Apple M3 chip, latest 2023 model.', ARRAY['https://picsum.photos/id/7/600/600', 'https://picsum.photos/id/8/600/600', 'https://picsum.photos/id/9/600/600'], 'Apple', 2);

-- Accessories (category_id = 3)
INSERT INTO PRODUCT (sku, name, price, stock, description, images, supplier, category_id) VALUES
('AP2', 'AirPods Pro 2', 5500000, 100, 'Wireless earbuds with active noise cancellation.', ARRAY['https://picsum.photos/id/10/600/600', 'https://picsum.photos/id/11/600/600', 'https://picsum.photos/id/12/600/600'], 'Apple', 3);

-- NOTE: Add more products to reach the minimum of 22 products per category required by the project spec.

-- 5. Insert Orders
INSERT INTO ORDERS (final_price, status, customer_id, account_id, shipping_address, recipient_name, recipient_phone, recipient_email, is_deleted) VALUES
(29000000, 'Delivered', 1, 2, '123 Main Street, District 1, HCMC', 'John Smith', '0901234567', 'john@example.com', false),       -- Order created by sale1
(35000000, 'Created', 2, 1, 'Company X, Building Y, District 3, HCMC', 'Jane Doe', '0987654321', 'jane@example.com', false); -- Order created by admin

-- 6. Insert Order Items
INSERT INTO ORDER_ITEM (order_id, product_id, quantity, unit_sale_price, total_price) VALUES
(1, 1, 1, 29000000, 29000000), -- Order 1: iPhone 15 Pro Max
(2, 3, 1, 35000000, 35000000); -- Order 2: MacBook Pro 14
