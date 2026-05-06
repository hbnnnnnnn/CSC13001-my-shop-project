-- =============================================================================
-- Seed dữ liệu phục vụ test báo cáo / biểu đồ (Reports) trên desktop app.
-- Báo cáo chỉ tính đơn: status = 'Delivered' AND is_deleted = false.
--
-- Chạy (chọn một):
--   docker compose exec db psql -U postgres -d myshop -f /path/in/container/...
-- Hoặc từ máy host (port 5432 map sẵn):
--   psql -h localhost -U postgres -d myshop -f backend/database/seeds/02_report_charts_demo.sql
--
-- Script idempotent: mỗi lần chạy xóa bộ demo cũ (cùng marker) rồi nạp lại.
-- =============================================================================

BEGIN;

-- Xóa demo cũ (theo marker địa chỉ giao hàng + SKU + email khách)
DELETE FROM order_item
WHERE order_id IN (
    SELECT order_id FROM orders WHERE shipping_address = 'REPORT_CHARTS_DEMO_SEED'
);
DELETE FROM orders WHERE shipping_address = 'REPORT_CHARTS_DEMO_SEED';
DELETE FROM product WHERE sku LIKE 'RPTDEMO-%';
DELETE FROM category WHERE name IN ('RPTDEMO Phones', 'RPTDEMO Laptops', 'RPTDEMO Accessories');
DELETE FROM customer WHERE email = 'reportdemo.charts@local.test';

DO $$
DECLARE
    acc_id INTEGER;
    cat_phone INTEGER;
    cat_laptop INTEGER;
    cat_accessory INTEGER;
    cust_id INTEGER;
    p_phone INTEGER;
    p_laptop INTEGER;
    p_buds INTEGER;
    p_tab INTEGER;
    p_watch INTEGER;
    prod_ids INTEGER[];
    prod_prices INTEGER[];
    i INTEGER;
    idx INTEGER;
    qty INTEGER;
    price INTEGER;
    oid INTEGER;
    final INTEGER;
BEGIN
    SELECT account_id INTO acc_id FROM account ORDER BY account_id LIMIT 1;
    IF acc_id IS NULL THEN
        RAISE EXCEPTION 'Không có account. Chạy seeds/01_dummy_data.sql hoặc tạo ít nhất một ACCOUNT trước.';
    END IF;

    INSERT INTO category (name, description)
    VALUES
        ('RPTDEMO Phones', 'Seed category for report charts'),
        ('RPTDEMO Laptops', 'Seed category for report charts'),
        ('RPTDEMO Accessories', 'Seed category for report charts');

    SELECT category_id INTO cat_phone FROM category WHERE name = 'RPTDEMO Phones' ORDER BY category_id LIMIT 1;
    SELECT category_id INTO cat_laptop FROM category WHERE name = 'RPTDEMO Laptops' ORDER BY category_id LIMIT 1;
    SELECT category_id INTO cat_accessory FROM category WHERE name = 'RPTDEMO Accessories' ORDER BY category_id LIMIT 1;

    INSERT INTO customer (name, phone, email, address)
    VALUES (
        'Khách Demo Báo cáo',
        '0900000000',
        'reportdemo.charts@local.test',
        'Demo — không giao thật'
    )
    RETURNING customer_id INTO cust_id;

    INSERT INTO product (sku, name, price, stock, description, category_id)
    VALUES
        ('RPTDEMO-PHONE', N'Điện thoại demo R1', 8000000, 200, N'Seed cho biểu đồ', cat_phone),
        ('RPTDEMO-LAPTOP', N'Laptop demo R2', 22000000, 80, N'Seed cho biểu đồ', cat_laptop),
        ('RPTDEMO-BUDS', N'Tai nghe demo R3', 2000000, 300, N'Seed cho biểu đồ', cat_accessory),
        ('RPTDEMO-TAB', N'Máy tính bảng R4', 12000000, 120, N'Seed cho biểu đồ', cat_laptop),
        ('RPTDEMO-WATCH', N'Đồng hồ R5', 5000000, 150, N'Seed cho biểu đồ', cat_accessory);

    SELECT product_id INTO p_phone FROM product WHERE sku = 'RPTDEMO-PHONE';
    SELECT product_id INTO p_laptop FROM product WHERE sku = 'RPTDEMO-LAPTOP';
    SELECT product_id INTO p_buds FROM product WHERE sku = 'RPTDEMO-BUDS';
    SELECT product_id INTO p_tab FROM product WHERE sku = 'RPTDEMO-TAB';
    SELECT product_id INTO p_watch FROM product WHERE sku = 'RPTDEMO-WATCH';

    prod_ids := ARRAY[p_phone, p_laptop, p_buds, p_tab, p_watch];
    prod_prices := ARRAY[8000000, 22000000, 2000000, 12000000, 5000000];

    -- Helper: tạo một đơn Delivered + dòng hàng, chỉnh created_time
    -- (final_price = tổng total_price của các dòng)

    -- created_time = UTC now trừ lùi (luôn nằm trong khoảng ~70 ngày gần đây, khớp filter mặc định app)
    INSERT INTO orders (
        created_time, final_price, status, customer_id, account_id,
        shipping_address, recipient_name, recipient_phone, recipient_email, is_deleted
    ) VALUES (
        timezone('utc', now()) - interval '68 days' - interval '9 hours', 0, 'Delivered', cust_id, acc_id,
        'REPORT_CHARTS_DEMO_SEED', N'Khách Demo Báo cáo', '0900000000', 'reportdemo.charts@local.test', false
    ) RETURNING order_id INTO oid;
    final := 2 * 8000000;
    UPDATE orders SET final_price = final WHERE order_id = oid;
    INSERT INTO order_item (order_id, product_id, quantity, unit_sale_price, total_price)
    VALUES (oid, p_phone, 2, 8000000, final);

    INSERT INTO orders (
        created_time, final_price, status, customer_id, account_id,
        shipping_address, recipient_name, recipient_phone, recipient_email, is_deleted
    ) VALUES (
        timezone('utc', now()) - interval '61 days' - interval '14 hours' - interval '30 minutes', 0, 'Delivered', cust_id, acc_id,
        'REPORT_CHARTS_DEMO_SEED', N'Khách Demo Báo cáo', '0900000000', 'reportdemo.charts@local.test', false
    ) RETURNING order_id INTO oid;
    final := 22000000 + 2000000;
    UPDATE orders SET final_price = final WHERE order_id = oid;
    INSERT INTO order_item (order_id, product_id, quantity, unit_sale_price, total_price) VALUES
        (oid, p_laptop, 1, 22000000, 22000000),
        (oid, p_buds, 1, 2000000, 2000000);

    INSERT INTO orders (
        created_time, final_price, status, customer_id, account_id,
        shipping_address, recipient_name, recipient_phone, recipient_email, is_deleted
    ) VALUES (
        timezone('utc', now()) - interval '45 days' - interval '11 hours', 0, 'Delivered', cust_id, acc_id,
        'REPORT_CHARTS_DEMO_SEED', N'Khách Demo Báo cáo', '0900000000', 'reportdemo.charts@local.test', false
    ) RETURNING order_id INTO oid;
    final := 3 * 2000000;
    UPDATE orders SET final_price = final WHERE order_id = oid;
    INSERT INTO order_item (order_id, product_id, quantity, unit_sale_price, total_price)
    VALUES (oid, p_buds, 3, 2000000, final);

    INSERT INTO orders (
        created_time, final_price, status, customer_id, account_id,
        shipping_address, recipient_name, recipient_phone, recipient_email, is_deleted
    ) VALUES (
        timezone('utc', now()) - interval '38 days' - interval '8 hours' - interval '15 minutes', 0, 'Delivered', cust_id, acc_id,
        'REPORT_CHARTS_DEMO_SEED', N'Khách Demo Báo cáo', '0900000000', 'reportdemo.charts@local.test', false
    ) RETURNING order_id INTO oid;
    final := 12000000 + 5000000;
    UPDATE orders SET final_price = final WHERE order_id = oid;
    INSERT INTO order_item (order_id, product_id, quantity, unit_sale_price, total_price) VALUES
        (oid, p_tab, 1, 12000000, 12000000),
        (oid, p_watch, 1, 5000000, 5000000);

    INSERT INTO orders (
        created_time, final_price, status, customer_id, account_id,
        shipping_address, recipient_name, recipient_phone, recipient_email, is_deleted
    ) VALUES (
        timezone('utc', now()) - interval '37 days' - interval '16 hours', 0, 'Delivered', cust_id, acc_id,
        'REPORT_CHARTS_DEMO_SEED', N'Khách Demo Báo cáo', '0900000000', 'reportdemo.charts@local.test', false
    ) RETURNING order_id INTO oid;
    final := 1 * 8000000;
    UPDATE orders SET final_price = final WHERE order_id = oid;
    INSERT INTO order_item (order_id, product_id, quantity, unit_sale_price, total_price)
    VALUES (oid, p_phone, 1, 8000000, final);

    INSERT INTO orders (
        created_time, final_price, status, customer_id, account_id,
        shipping_address, recipient_name, recipient_phone, recipient_email, is_deleted
    ) VALUES (
        timezone('utc', now()) - interval '32 days' - interval '10 hours', 0, 'Delivered', cust_id, acc_id,
        'REPORT_CHARTS_DEMO_SEED', N'Khách Demo Báo cáo', '0900000000', 'reportdemo.charts@local.test', false
    ) RETURNING order_id INTO oid;
    final := 22000000;
    UPDATE orders SET final_price = final WHERE order_id = oid;
    INSERT INTO order_item (order_id, product_id, quantity, unit_sale_price, total_price)
    VALUES (oid, p_laptop, 1, 22000000, final);

    INSERT INTO orders (
        created_time, final_price, status, customer_id, account_id,
        shipping_address, recipient_name, recipient_phone, recipient_email, is_deleted
    ) VALUES (
        timezone('utc', now()) - interval '32 days' - interval '18 hours' - interval '30 minutes', 0, 'Delivered', cust_id, acc_id,
        'REPORT_CHARTS_DEMO_SEED', N'Khách Demo Báo cáo', '0900000000', 'reportdemo.charts@local.test', false
    ) RETURNING order_id INTO oid;
    final := 4 * 5000000;
    UPDATE orders SET final_price = final WHERE order_id = oid;
    INSERT INTO order_item (order_id, product_id, quantity, unit_sale_price, total_price)
    VALUES (oid, p_watch, 4, 5000000, final);

    INSERT INTO orders (
        created_time, final_price, status, customer_id, account_id,
        shipping_address, recipient_name, recipient_phone, recipient_email, is_deleted
    ) VALUES (
        timezone('utc', now()) - interval '27 days' - interval '9 hours' - interval '45 minutes', 0, 'Delivered', cust_id, acc_id,
        'REPORT_CHARTS_DEMO_SEED', N'Khách Demo Báo cáo', '0900000000', 'reportdemo.charts@local.test', false
    ) RETURNING order_id INTO oid;
    final := 2 * 12000000 + 5 * 2000000;
    UPDATE orders SET final_price = final WHERE order_id = oid;
    INSERT INTO order_item (order_id, product_id, quantity, unit_sale_price, total_price) VALUES
        (oid, p_tab, 2, 12000000, 24000000),
        (oid, p_buds, 5, 2000000, 10000000);

    INSERT INTO orders (
        created_time, final_price, status, customer_id, account_id,
        shipping_address, recipient_name, recipient_phone, recipient_email, is_deleted
    ) VALUES (
        timezone('utc', now()) - interval '20 days' - interval '13 hours' - interval '20 minutes', 0, 'Delivered', cust_id, acc_id,
        'REPORT_CHARTS_DEMO_SEED', N'Khách Demo Báo cáo', '0900000000', 'reportdemo.charts@local.test', false
    ) RETURNING order_id INTO oid;
    final := 8000000 + 22000000 + 2000000;
    UPDATE orders SET final_price = final WHERE order_id = oid;
    INSERT INTO order_item (order_id, product_id, quantity, unit_sale_price, total_price) VALUES
        (oid, p_phone, 1, 8000000, 8000000),
        (oid, p_laptop, 1, 22000000, 22000000),
        (oid, p_buds, 1, 2000000, 2000000);

    INSERT INTO orders (
        created_time, final_price, status, customer_id, account_id,
        shipping_address, recipient_name, recipient_phone, recipient_email, is_deleted
    ) VALUES (
        timezone('utc', now()) - interval '12 days' - interval '20 hours', 0, 'Delivered', cust_id, acc_id,
        'REPORT_CHARTS_DEMO_SEED', N'Khách Demo Báo cáo', '0900000000', 'reportdemo.charts@local.test', false
    ) RETURNING order_id INTO oid;
    final := 10 * 2000000;
    UPDATE orders SET final_price = final WHERE order_id = oid;
    INSERT INTO order_item (order_id, product_id, quantity, unit_sale_price, total_price)
    VALUES (oid, p_buds, 10, 2000000, final);

    INSERT INTO orders (
        created_time, final_price, status, customer_id, account_id,
        shipping_address, recipient_name, recipient_phone, recipient_email, is_deleted
    ) VALUES (
        timezone('utc', now()) - interval '3 days' - interval '12 hours', 0, 'Delivered', cust_id, acc_id,
        'REPORT_CHARTS_DEMO_SEED', N'Khách Demo Báo cáo', '0900000000', 'reportdemo.charts@local.test', false
    ) RETURNING order_id INTO oid;
    final := 6 * 8000000;
    UPDATE orders SET final_price = final WHERE order_id = oid;
    INSERT INTO order_item (order_id, product_id, quantity, unit_sale_price, total_price)
    VALUES (oid, p_phone, 6, 8000000, final);

    -- thêm dữ liệu đều theo ngày để biểu đồ mượt hơn
    FOR i IN 1..30 LOOP
        idx := ((i - 1) % 5) + 1;
        qty := ((i - 1) % 4) + 1;
        price := prod_prices[idx];

        INSERT INTO orders (
            created_time, final_price, status, customer_id, account_id,
            shipping_address, recipient_name, recipient_phone, recipient_email, is_deleted
        ) VALUES (
            timezone('utc', now()) - make_interval(days => i) - interval '3 hours', 0, 'Delivered', cust_id, acc_id,
            'REPORT_CHARTS_DEMO_SEED', N'Khách Demo Báo cáo', '0900000000', 'reportdemo.charts@local.test', false
        ) RETURNING order_id INTO oid;

        final := qty * price;
        UPDATE orders SET final_price = final WHERE order_id = oid;
        INSERT INTO order_item (order_id, product_id, quantity, unit_sale_price, total_price)
        VALUES (oid, prod_ids[idx], qty, price, final);
    END LOOP;

END $$;

COMMIT;

-- App Reports mặc định: ~120 ngày gần nhất (UTC). Sau khi chạy seed, bấm Áp dụng hoặc mở lại trang.
-- Nếu backend bật Redis cache báo cáo (~10 phút TTL), restart API hoặc đợi hết TTL sau khi seed.
