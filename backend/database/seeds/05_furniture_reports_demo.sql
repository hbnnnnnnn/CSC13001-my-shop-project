-- =============================================================================
-- 05_furniture_reports_demo.sql
-- Seed dữ liệu phục vụ test báo cáo / biểu đồ với sản phẩm nội thất.
-- Dựa trên các sản phẩm đã có từ 04_furniture_seed.sql.
--
-- Script idempotent: mỗi lần chạy xóa demo cũ (marker RPTDEMO) rồi nạp lại.
-- =============================================================================

BEGIN;

-- Xóa demo cũ theo marker
DELETE FROM order_item
WHERE order_id IN (
    SELECT order_id FROM orders WHERE shipping_address = 'REPORT_FURNITURE_DEMO_SEED'
);
DELETE FROM orders WHERE shipping_address = 'REPORT_FURNITURE_DEMO_SEED';
DELETE FROM customer WHERE email = 'reportdemo.furniture@local.test';

-- Xóa RPTDEMO điện tử cũ (từ seed 02 cũ nếu còn sót)
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
    acc_id      INTEGER;
    cust_id     INTEGER;

    -- SKU nội thất từ 04_furniture_seed (lấy theo sku cho an toàn)
    p_ghe       INTEGER;   -- Ghế Gỗ Bắc Âu          1,850,000
    p_sofa      INTEGER;   -- Sofa Văng 3 Chỗ         9,800,000
    p_ban       INTEGER;   -- Bàn Cà Phê Tối Giản     3,800,000
    p_den       INTEGER;   -- Đèn Thả Trần Edison       980,000
    p_giuong    INTEGER;   -- Giường Nền Thấp Nhật     8,800,000

    prod_ids    INTEGER[];
    prod_prices INTEGER[];
    i           INTEGER;
    idx         INTEGER;
    qty         INTEGER;
    price       INTEGER;
    oid         INTEGER;
    final       INTEGER;
BEGIN
    SELECT account_id INTO acc_id FROM account ORDER BY account_id LIMIT 1;
    IF acc_id IS NULL THEN
        RAISE EXCEPTION 'Không có account. Chạy seed 04_furniture_seed.sql trước.';
    END IF;

    SELECT product_id INTO p_ghe    FROM product WHERE sku = 'GHE-SCAN-01';
    SELECT product_id INTO p_sofa   FROM product WHERE sku = 'SOF-3SEA-02';
    SELECT product_id INTO p_ban    FROM product WHERE sku = 'BAN-COFE-02';
    SELECT product_id INTO p_den    FROM product WHERE sku = 'DEN-PEND-01';
    SELECT product_id INTO p_giuong FROM product WHERE sku = 'GIU-PLAT-03';

    IF p_ghe IS NULL OR p_sofa IS NULL OR p_ban IS NULL OR p_den IS NULL OR p_giuong IS NULL THEN
        RAISE EXCEPTION 'Thiếu sản phẩm nội thất. Hãy chắc chắn seed 04_furniture_seed.sql đã chạy.';
    END IF;

    INSERT INTO customer (name, phone, email, address)
    VALUES (
        'Khách Demo Báo Cáo',
        '0900000099',
        'reportdemo.furniture@local.test',
        'Demo — không giao thật'
    )
    RETURNING customer_id INTO cust_id;

    prod_ids    := ARRAY[p_ghe, p_sofa, p_ban, p_den, p_giuong];
    prod_prices := ARRAY[1850000, 9800000, 3800000, 980000, 8800000];

    -- ── Đơn cố định trải đều 70 ngày để biểu đồ có điểm dữ liệu ──

    INSERT INTO orders (created_time, final_price, status, customer_id, account_id,
        shipping_address, recipient_name, recipient_phone, recipient_email, is_deleted)
    VALUES (timezone('utc', now()) - interval '68 days' - interval '9 hours',
        0, 'Delivered', cust_id, acc_id,
        'REPORT_FURNITURE_DEMO_SEED', 'Khách Demo Báo Cáo', '0900000099', 'reportdemo.furniture@local.test', false)
    RETURNING order_id INTO oid;
    final := 2 * 1850000;
    UPDATE orders SET final_price = final WHERE order_id = oid;
    INSERT INTO order_item (order_id, product_id, quantity, unit_sale_price, total_price)
    VALUES (oid, p_ghe, 2, 1850000, final);

    INSERT INTO orders (created_time, final_price, status, customer_id, account_id,
        shipping_address, recipient_name, recipient_phone, recipient_email, is_deleted)
    VALUES (timezone('utc', now()) - interval '61 days' - interval '14 hours' - interval '30 minutes',
        0, 'Delivered', cust_id, acc_id,
        'REPORT_FURNITURE_DEMO_SEED', 'Khách Demo Báo Cáo', '0900000099', 'reportdemo.furniture@local.test', false)
    RETURNING order_id INTO oid;
    final := 9800000 + 3800000;
    UPDATE orders SET final_price = final WHERE order_id = oid;
    INSERT INTO order_item (order_id, product_id, quantity, unit_sale_price, total_price) VALUES
        (oid, p_sofa, 1, 9800000, 9800000),
        (oid, p_ban,  1, 3800000, 3800000);

    INSERT INTO orders (created_time, final_price, status, customer_id, account_id,
        shipping_address, recipient_name, recipient_phone, recipient_email, is_deleted)
    VALUES (timezone('utc', now()) - interval '45 days' - interval '11 hours',
        0, 'Delivered', cust_id, acc_id,
        'REPORT_FURNITURE_DEMO_SEED', 'Khách Demo Báo Cáo', '0900000099', 'reportdemo.furniture@local.test', false)
    RETURNING order_id INTO oid;
    final := 4 * 980000;
    UPDATE orders SET final_price = final WHERE order_id = oid;
    INSERT INTO order_item (order_id, product_id, quantity, unit_sale_price, total_price)
    VALUES (oid, p_den, 4, 980000, final);

    INSERT INTO orders (created_time, final_price, status, customer_id, account_id,
        shipping_address, recipient_name, recipient_phone, recipient_email, is_deleted)
    VALUES (timezone('utc', now()) - interval '38 days' - interval '8 hours' - interval '15 minutes',
        0, 'Delivered', cust_id, acc_id,
        'REPORT_FURNITURE_DEMO_SEED', 'Khách Demo Báo Cáo', '0900000099', 'reportdemo.furniture@local.test', false)
    RETURNING order_id INTO oid;
    final := 8800000 + 3800000;
    UPDATE orders SET final_price = final WHERE order_id = oid;
    INSERT INTO order_item (order_id, product_id, quantity, unit_sale_price, total_price) VALUES
        (oid, p_giuong, 1, 8800000, 8800000),
        (oid, p_ban,    1, 3800000, 3800000);

    INSERT INTO orders (created_time, final_price, status, customer_id, account_id,
        shipping_address, recipient_name, recipient_phone, recipient_email, is_deleted)
    VALUES (timezone('utc', now()) - interval '37 days' - interval '16 hours',
        0, 'Delivered', cust_id, acc_id,
        'REPORT_FURNITURE_DEMO_SEED', 'Khách Demo Báo Cáo', '0900000099', 'reportdemo.furniture@local.test', false)
    RETURNING order_id INTO oid;
    final := 3 * 1850000;
    UPDATE orders SET final_price = final WHERE order_id = oid;
    INSERT INTO order_item (order_id, product_id, quantity, unit_sale_price, total_price)
    VALUES (oid, p_ghe, 3, 1850000, final);

    INSERT INTO orders (created_time, final_price, status, customer_id, account_id,
        shipping_address, recipient_name, recipient_phone, recipient_email, is_deleted)
    VALUES (timezone('utc', now()) - interval '32 days' - interval '10 hours',
        0, 'Delivered', cust_id, acc_id,
        'REPORT_FURNITURE_DEMO_SEED', 'Khách Demo Báo Cáo', '0900000099', 'reportdemo.furniture@local.test', false)
    RETURNING order_id INTO oid;
    final := 9800000;
    UPDATE orders SET final_price = final WHERE order_id = oid;
    INSERT INTO order_item (order_id, product_id, quantity, unit_sale_price, total_price)
    VALUES (oid, p_sofa, 1, 9800000, final);

    INSERT INTO orders (created_time, final_price, status, customer_id, account_id,
        shipping_address, recipient_name, recipient_phone, recipient_email, is_deleted)
    VALUES (timezone('utc', now()) - interval '27 days' - interval '9 hours' - interval '45 minutes',
        0, 'Delivered', cust_id, acc_id,
        'REPORT_FURNITURE_DEMO_SEED', 'Khách Demo Báo Cáo', '0900000099', 'reportdemo.furniture@local.test', false)
    RETURNING order_id INTO oid;
    final := 2 * 8800000 + 5 * 980000;
    UPDATE orders SET final_price = final WHERE order_id = oid;
    INSERT INTO order_item (order_id, product_id, quantity, unit_sale_price, total_price) VALUES
        (oid, p_giuong, 2, 8800000,  17600000),
        (oid, p_den,    5,  980000,   4900000);

    INSERT INTO orders (created_time, final_price, status, customer_id, account_id,
        shipping_address, recipient_name, recipient_phone, recipient_email, is_deleted)
    VALUES (timezone('utc', now()) - interval '20 days' - interval '13 hours' - interval '20 minutes',
        0, 'Delivered', cust_id, acc_id,
        'REPORT_FURNITURE_DEMO_SEED', 'Khách Demo Báo Cáo', '0900000099', 'reportdemo.furniture@local.test', false)
    RETURNING order_id INTO oid;
    final := 1850000 + 9800000 + 3800000;
    UPDATE orders SET final_price = final WHERE order_id = oid;
    INSERT INTO order_item (order_id, product_id, quantity, unit_sale_price, total_price) VALUES
        (oid, p_ghe,  1, 1850000, 1850000),
        (oid, p_sofa, 1, 9800000, 9800000),
        (oid, p_ban,  1, 3800000, 3800000);

    INSERT INTO orders (created_time, final_price, status, customer_id, account_id,
        shipping_address, recipient_name, recipient_phone, recipient_email, is_deleted)
    VALUES (timezone('utc', now()) - interval '12 days' - interval '20 hours',
        0, 'Delivered', cust_id, acc_id,
        'REPORT_FURNITURE_DEMO_SEED', 'Khách Demo Báo Cáo', '0900000099', 'reportdemo.furniture@local.test', false)
    RETURNING order_id INTO oid;
    final := 6 * 980000;
    UPDATE orders SET final_price = final WHERE order_id = oid;
    INSERT INTO order_item (order_id, product_id, quantity, unit_sale_price, total_price)
    VALUES (oid, p_den, 6, 980000, final);

    INSERT INTO orders (created_time, final_price, status, customer_id, account_id,
        shipping_address, recipient_name, recipient_phone, recipient_email, is_deleted)
    VALUES (timezone('utc', now()) - interval '3 days' - interval '12 hours',
        0, 'Delivered', cust_id, acc_id,
        'REPORT_FURNITURE_DEMO_SEED', 'Khách Demo Báo Cáo', '0900000099', 'reportdemo.furniture@local.test', false)
    RETURNING order_id INTO oid;
    final := 4 * 1850000;
    UPDATE orders SET final_price = final WHERE order_id = oid;
    INSERT INTO order_item (order_id, product_id, quantity, unit_sale_price, total_price)
    VALUES (oid, p_ghe, 4, 1850000, final);

    -- ── Vòng lặp 30 đơn trải đều cho biểu đồ mượt ────────────────────
    FOR i IN 1..30 LOOP
        idx   := ((i - 1) % 5) + 1;
        qty   := ((i - 1) % 4) + 1;
        price := prod_prices[idx];

        INSERT INTO orders (created_time, final_price, status, customer_id, account_id,
            shipping_address, recipient_name, recipient_phone, recipient_email, is_deleted)
        VALUES (
            timezone('utc', now()) - make_interval(days => i) - interval '3 hours',
            0, 'Delivered', cust_id, acc_id,
            'REPORT_FURNITURE_DEMO_SEED', 'Khách Demo Báo Cáo', '0900000099', 'reportdemo.furniture@local.test', false
        ) RETURNING order_id INTO oid;

        final := qty * price;
        UPDATE orders SET final_price = final WHERE order_id = oid;
        INSERT INTO order_item (order_id, product_id, quantity, unit_sale_price, total_price)
        VALUES (oid, prod_ids[idx], qty, price, final);
    END LOOP;

END $$;

COMMIT;

-- Sau khi chạy seed, bấm "Áp dụng" hoặc mở lại trang Báo cáo trong app.
-- Nếu Redis cache báo cáo đang bật (~10 phút TTL), restart API hoặc đợi hết TTL.
