-- ==========================================
-- 02_add_indexes.sql
-- Thêm các chỉ mục (Index) để tối ưu hiệu suất truy vấn
-- ==========================================

-- 1. Bảng CUSTOMER
-- Phục vụ truy vấn: WHERE phone = $1
CREATE INDEX IF NOT EXISTS idx_customer_phone ON CUSTOMER(phone);

-- 2. Bảng PRODUCT
-- Phục vụ truy vấn: WHERE category_id = $1 (và khi JOIN với CATEGORY)
CREATE INDEX IF NOT EXISTS idx_product_category_id ON PRODUCT(category_id);

-- Phục vụ truy vấn: WHERE price >= $1 AND price <= $2
CREATE INDEX IF NOT EXISTS idx_product_price ON PRODUCT(price);

-- Phục vụ truy vấn: ORDER BY stock ASC (Báo cáo sắp hết hàng)
CREATE INDEX IF NOT EXISTS idx_product_stock ON PRODUCT(stock);

-- 3. Bảng ORDERS
-- Partial Index phục vụ truy vấn danh sách đơn hàng & báo cáo: 
-- Lọc WHERE is_deleted = false AND status = $1, sắp xếp/lọc theo created_time
CREATE INDEX IF NOT EXISTS idx_orders_status_created_partial 
    ON ORDERS(status, created_time DESC) 
    WHERE is_deleted = false;

-- Phục vụ tra cứu lịch sử mua hàng của khách (và tối ưu ON DELETE SET NULL)
CREATE INDEX IF NOT EXISTS idx_orders_customer_id ON ORDERS(customer_id);

-- Phục vụ tính KPI cho nhân viên (và tối ưu ON DELETE SET NULL)
CREATE INDEX IF NOT EXISTS idx_orders_account_id ON ORDERS(account_id);

-- 4. Bảng ORDER_ITEM
-- Phục vụ truy vấn: WHERE order_id = ANY($1) (Rất quan trọng cho DataLoader)
CREATE INDEX IF NOT EXISTS idx_order_item_order_id ON ORDER_ITEM(order_id);

-- Phục vụ truy vấn: JOIN order_item oi ON p.product_id = oi.product_id (Thống kê)
CREATE INDEX IF NOT EXISTS idx_order_item_product_id ON ORDER_ITEM(product_id);
