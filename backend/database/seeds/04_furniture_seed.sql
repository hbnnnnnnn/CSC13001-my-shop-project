-- =============================================================================
-- 04_furniture_seed.sql
-- Xóa toàn bộ dữ liệu cũ và nạp dữ liệu mẫu cửa hàng nội thất.
--
-- Product ID map (tham khảo khi viết order_item):
--   1  GHE-SCAN-01  Ghế Gỗ Bắc Âu            1,850,000
--   2  GHE-LOUN-02  Ghế Thư Giãn Bọc Vải      4,200,000
--   3  GHE-DINA-03  Ghế Ăn Hiện Đại            1,290,000
--   4  GHE-ROCK-04  Ghế Bập Bênh Walnut         6,800,000
--   5  GHE-BAR-05   Ghế Bar Cao Cấp             2,350,000
--   6  BAN-DINE-01  Bàn Ăn Gỗ Sồi 6 Người     12,500,000
--   7  BAN-COFE-02  Bàn Cà Phê Tối Giản         3,800,000
--   8  BAN-WORK-03  Bàn Làm Việc Gỗ Thông       5,200,000
--   9  BAN-NITE-04  Bàn Đầu Giường              1,650,000
--  10  BAN-CONS-05  Bàn Console Vintage         4,500,000
--  11  DEN-PEND-01  Đèn Thả Trần Edison           980,000
--  12  DEN-FLOO-02  Đèn Sàn Bắc Âu              2,100,000
--  13  DEN-TABL-03  Đèn Bàn Cổ Điển               750,000
--  14  DEN-CHAN-04  Đèn Chùm Sputnik             5,500,000
--  15  DEN-WALL-05  Đèn Tường Đọc Sách            620,000
--  16  SOF-LSEC-01  Sofa Góc Chữ L Bouclé       18,500,000
--  17  SOF-3SEA-02  Sofa Văng 3 Chỗ              9,800,000
--  18  SOF-LEAT-03  Sofa Da Thật Ý              28,000,000
--  19  SOF-BED-04   Sofa Bed Đa Năng             7,200,000
--  20  SOF-OTTO-05  Ghế Ottoman Nhung            1,800,000
--  21  TU-WARD-01   Tủ Quần Áo 4 Cánh           8,900,000
--  22  KE-BOOK-02   Kệ Sách Gỗ 5 Tầng           3,200,000
--  23  KE-WALL-03   Kệ Trang Trí Treo Tường        890,000
--  24  TU-SHOE-04   Tủ Giày 12 Ngăn             2,600,000
--  25  TU-DISP-05   Tủ Trưng Bày Kính            5,800,000
--  26  GIU-OAK-01   Giường Gỗ Sồi King Size     14,500,000
--  27  GIU-UPHL-02  Giường Đầu Bọc Nhung        11,200,000
--  28  GIU-PLAT-03  Giường Nền Thấp Nhật Bản     8,800,000
--  29  GIU-BUNK-04  Giường Tầng Trẻ Em           6,500,000
--  30  GIU-CANO-05  Giường Canopy Lãng Mạn      16,900,000
-- =============================================================================

-- Xóa toàn bộ dữ liệu cũ theo thứ tự phụ thuộc khoá ngoại
TRUNCATE TABLE ORDER_ITEM CASCADE;
TRUNCATE TABLE ORDERS CASCADE;
TRUNCATE TABLE PRODUCT CASCADE;
TRUNCATE TABLE CUSTOMER CASCADE;
TRUNCATE TABLE CATEGORY CASCADE;
TRUNCATE TABLE ACCOUNT CASCADE;

-- Reset sequences về 1
ALTER SEQUENCE account_account_id_seq RESTART WITH 1;
ALTER SEQUENCE category_category_id_seq RESTART WITH 1;
ALTER SEQUENCE product_product_id_seq RESTART WITH 1;
ALTER SEQUENCE customer_customer_id_seq RESTART WITH 1;
ALTER SEQUENCE orders_order_id_seq RESTART WITH 1;
ALTER SEQUENCE order_item_order_item_id_seq RESTART WITH 1;

-- ============================================================
-- 1. TÀI KHOẢN NHÂN VIÊN (mật khẩu mặc định: 123456)
-- ============================================================
-- password_hash = bcrypt('123456', 10)
INSERT INTO ACCOUNT (username, password_hash, full_name, account_role) VALUES
('admin',  '$2b$10$OmJgr4A/HfALCgow8A2ks.zmBgODaxSjbMrqVZW776zRHqJW.J8Pq', 'Nguyễn Văn An',  'Admin'),
('sale1',  '$2b$10$OmJgr4A/HfALCgow8A2ks.zmBgODaxSjbMrqVZW776zRHqJW.J8Pq', 'Trần Thị Bích',  'Sale'),
('sale2',  '$2b$10$OmJgr4A/HfALCgow8A2ks.zmBgODaxSjbMrqVZW776zRHqJW.J8Pq', 'Lê Minh Tuấn',   'Sale');

-- ============================================================
-- 2. DANH MỤC SẢN PHẨM
-- ============================================================
INSERT INTO CATEGORY (name, description) VALUES
('Ghế',       'Ghế ăn, ghế thư giãn, ghế bar và các loại ghế trang trí'),
('Bàn',       'Bàn ăn, bàn cà phê, bàn làm việc và bàn trang trí'),
('Đèn',       'Đèn thả trần, đèn sàn, đèn bàn và đèn tường trang trí'),
('Sofa',      'Sofa góc, sofa văng, sofa bed và ghế đơn bọc nệm'),
('Tủ & Kệ',  'Tủ quần áo, kệ sách, tủ giày và kệ trang trí'),
('Giường',    'Giường đôi, giường đơn, giường tầng và đầu giường bọc vải');

-- ============================================================
-- 3. KHÁCH HÀNG (tên tiếng Việt)
-- ============================================================
INSERT INTO CUSTOMER (name, phone, email, address) VALUES
('Phạm Thị Diễm',   '0901111111', 'diem.pham@gmail.com',    '12 Nguyễn Huệ, Quận 1, TP.HCM'),
('Hoàng Văn Đức',   '0902222222', 'duc.hoang@gmail.com',    '45 Lê Lợi, Quận 3, TP.HCM'),
('Vũ Thị Hương',    '0903333333', 'huong.vu@gmail.com',     '78 Trần Hưng Đạo, Quận 5, TP.HCM'),
('Đặng Văn Khoa',   '0904444444', 'khoa.dang@gmail.com',    '23 Đinh Tiên Hoàng, Quận Bình Thạnh, TP.HCM'),
('Ngô Thị Lan',     '0905555555', 'lan.ngo@gmail.com',      '56 Pasteur, Quận 1, TP.HCM'),
('Bùi Văn Minh',    '0906666666', 'minh.bui@gmail.com',     '90 Cách Mạng Tháng 8, Quận 10, TP.HCM'),
('Đỗ Thị Ngọc',     '0907777777', 'ngoc.do@gmail.com',      '34 Võ Văn Tần, Quận 3, TP.HCM'),
('Lý Văn Phúc',     '0908888888', 'phuc.ly@gmail.com',      '67 Hai Bà Trưng, Quận 1, TP.HCM');

-- ============================================================
-- 4. SẢN PHẨM NỘI THẤT (30 sản phẩm, 5 mỗi danh mục)
-- ============================================================

-- ── GHẾ – category_id = 1 ─────────────────────────────────
INSERT INTO PRODUCT (sku, name, price, cost_price, stock, description, images, supplier, category_id) VALUES
(
  'GHE-SCAN-01',
  'Ghế Gỗ Phong Cách Bắc Âu',
  1850000, 1100000, 80,
  'Ghế ăn tối giản phong cách Scandinavian. Khung gỗ sồi tự nhiên, chân thon côn cong nhẹ, mặt ngồi bọc đệm vải lanh. Kết hợp hài hoà giữa thẩm mỹ và độ bền.',
  ARRAY[
    'https://images.unsplash.com/photo-1503602642458-232111445657?w=600&h=600&fit=crop&q=80',
    'https://images.unsplash.com/photo-1617364852223-75f57e78dc96?w=600&h=600&fit=crop&q=80',
    'https://images.unsplash.com/photo-1586023492125-27b2c045efd7?w=600&h=600&fit=crop&q=80'
  ],
  'Nordic Home', 1
),
(
  'GHE-LOUN-02',
  'Ghế Thư Giãn Bọc Vải Cao Cấp',
  4200000, 2600000, 35,
  'Ghế thư giãn bọc vải nhung mềm mại. Kiểu dáng cánh rộng ôm ấp cơ thể, chân kim loại mạ vàng sương. Điểm nhấn sang trọng cho góc đọc sách hoặc phòng khách.',
  ARRAY[
    'https://images.unsplash.com/photo-1567538096630-e0c55bd6374c?w=600&h=600&fit=crop&q=80',
    'https://images.unsplash.com/photo-1503602642458-232111445657?w=600&h=600&fit=crop&q=80',
    'https://images.unsplash.com/photo-1617364852223-75f57e78dc96?w=600&h=600&fit=crop&q=80'
  ],
  'Luxe Living', 1
),
(
  'GHE-DINA-03',
  'Ghế Ăn Hiện Đại Chân Kim Loại',
  1290000, 750000, 120,
  'Ghế ăn đơn giản, hiện đại với mặt ngồi gỗ ép cong ergonomic và chân thép sơn đen. Phù hợp phòng bếp và bàn ăn phong cách industrial hoặc minimalist.',
  ARRAY[
    'https://images.unsplash.com/photo-1586023492125-27b2c045efd7?w=600&h=600&fit=crop&q=80',
    'https://images.unsplash.com/photo-1617364852223-75f57e78dc96?w=600&h=600&fit=crop&q=80',
    'https://images.unsplash.com/photo-1503602642458-232111445657?w=600&h=600&fit=crop&q=80'
  ],
  'UrbanForm', 1
),
(
  'GHE-ROCK-04',
  'Ghế Bập Bênh Gỗ Walnut',
  6800000, 4200000, 20,
  'Ghế bập bênh thủ công từ gỗ walnut nguyên tấm. Đường cong tự nhiên giúp thư giãn hoàn toàn. Lý tưởng cho phòng khách, ban công hoặc hiên nhà.',
  ARRAY[
    'https://images.unsplash.com/photo-1533090481720-856c6e3c1fdc?w=600&h=600&fit=crop&q=80',
    'https://images.unsplash.com/photo-1503602642458-232111445657?w=600&h=600&fit=crop&q=80',
    'https://images.unsplash.com/photo-1617364852223-75f57e78dc96?w=600&h=600&fit=crop&q=80'
  ],
  'WoodCraft VN', 1
),
(
  'GHE-BAR-05',
  'Ghế Bar Chân Cao Phong Cách',
  2350000, 1400000, 50,
  'Ghế bar chân cao với mặt ngồi tròn bọc da tổng hợp, chân inox đánh bóng. Có tay vịn và vòng đỡ chân tiện lợi. Thích hợp cho quầy bar, bếp đảo hoặc không gian café.',
  ARRAY[
    'https://images.unsplash.com/photo-1583227061267-8428fb76fbfd?w=600&h=600&fit=crop&q=80',
    'https://images.unsplash.com/photo-1567016526105-22da7c13161a?w=600&h=600&fit=crop&q=80',
    'https://images.unsplash.com/photo-1567538096630-e0c55bd6374c?w=600&h=600&fit=crop&q=80'
  ],
  'UrbanForm', 1
);

-- ── BÀN – category_id = 2 ──────────────────────────────────
INSERT INTO PRODUCT (sku, name, price, cost_price, stock, description, images, supplier, category_id) VALUES
(
  'BAN-DINE-01',
  'Bàn Ăn Gỗ Sồi 6 Người',
  12500000, 8000000, 15,
  'Bàn ăn gia đình rộng rãi cho 6 người, mặt bàn gỗ sồi nguyên tấm dày 4 cm. Chân gỗ chữ A vững chắc. Hoàn thiện bằng dầu gỗ tự nhiên bảo vệ vân gỗ.',
  ARRAY[
    'https://images.unsplash.com/photo-1550226891-ef816aed4a98?w=600&h=600&fit=crop&q=80',
    'https://images.unsplash.com/photo-1611269154421-4e27233ac5c7?w=600&h=600&fit=crop&q=80',
    'https://images.unsplash.com/photo-1598300042247-d088f8ab3a91?w=600&h=600&fit=crop&q=80'
  ],
  'WoodCraft VN', 2
),
(
  'BAN-COFE-02',
  'Bàn Cà Phê Tối Giản Khung Sắt',
  3800000, 2300000, 40,
  'Bàn cà phê phòng khách với mặt kính cường lực 8 mm và khung sắt sơn tĩnh điện đen. Thiết kế trong suốt giúp không gian thoáng đãng và hiện đại.',
  ARRAY[
    'https://images.unsplash.com/photo-1611269154421-4e27233ac5c7?w=600&h=600&fit=crop&q=80',
    'https://images.unsplash.com/photo-1499933374294-4584851497cc?w=600&h=600&fit=crop&q=80',
    'https://images.unsplash.com/photo-1550226891-ef816aed4a98?w=600&h=600&fit=crop&q=80'
  ],
  'UrbanForm', 2
),
(
  'BAN-WORK-03',
  'Bàn Làm Việc Gỗ Thông Lưu Trữ',
  5200000, 3200000, 25,
  'Bàn làm việc home-office với mặt bàn gỗ thông rộng 140 cm. Có ngăn kéo kép bên trái và kệ sách phía sau. Thiết kế tiện ích cho không gian văn phòng tại nhà.',
  ARRAY[
    'https://images.unsplash.com/photo-1598300042247-d088f8ab3a91?w=600&h=600&fit=crop&q=80',
    'https://images.unsplash.com/photo-1550226891-ef816aed4a98?w=600&h=600&fit=crop&q=80',
    'https://images.unsplash.com/photo-1611269154421-4e27233ac5c7?w=600&h=600&fit=crop&q=80'
  ],
  'Nordic Home', 2
),
(
  'BAN-NITE-04',
  'Bàn Đầu Giường Ngăn Kéo Gỗ',
  1650000, 950000, 60,
  'Bàn đầu giường nhỏ gọn với 1 ngăn kéo và kệ mở phía dưới. Gỗ MDF phủ veneer sồi, chân côn gỗ tự nhiên. Phù hợp phòng ngủ phong cách Bắc Âu hay Japandi.',
  ARRAY[
    'https://images.unsplash.com/photo-1499933374294-4584851497cc?w=600&h=600&fit=crop&q=80',
    'https://images.unsplash.com/photo-1598300042247-d088f8ab3a91?w=600&h=600&fit=crop&q=80',
    'https://images.unsplash.com/photo-1550226891-ef816aed4a98?w=600&h=600&fit=crop&q=80'
  ],
  'Nordic Home', 2
),
(
  'BAN-CONS-05',
  'Bàn Console Hành Lang Vintage',
  4500000, 2800000, 18,
  'Bàn console trang trí hành lang hay phòng khách theo phong cách vintage. Khung gỗ tếch cũ tái chế, mặt đá marble trắng. Có ngăn kéo tiện lợi và kệ dưới.',
  ARRAY[
    'https://images.unsplash.com/photo-1550226891-ef816aed4a98?w=600&h=600&fit=crop&q=80',
    'https://images.unsplash.com/photo-1499933374294-4584851497cc?w=600&h=600&fit=crop&q=80',
    'https://images.unsplash.com/photo-1611269154421-4e27233ac5c7?w=600&h=600&fit=crop&q=80'
  ],
  'Luxe Living', 2
);

-- ── ĐÈN – category_id = 3 ─────────────────────────────────
INSERT INTO PRODUCT (sku, name, price, cost_price, stock, description, images, supplier, category_id) VALUES
(
  'DEN-PEND-01',
  'Đèn Thả Trần Công Nghiệp Edison',
  980000, 560000, 100,
  'Đèn thả trần phong cách industrial với bóng Edison vintage. Chao đèn gang đúc mạ đồng, dây điện bện vải nâu. Ánh sáng vàng ấm tạo không khí cozy cho nhà bếp hay phòng ăn.',
  ARRAY[
    'https://images.unsplash.com/photo-1636368208791-17b81ed832d2?w=600&h=600&fit=crop&q=80',
    'https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=600&h=600&fit=crop&q=80',
    'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=600&h=600&fit=crop&q=80'
  ],
  'LightArt', 3
),
(
  'DEN-FLOO-02',
  'Đèn Sàn Bắc Âu Cần Gập',
  2100000, 1250000, 45,
  'Đèn sàn với cần gập linh hoạt điều chỉnh góc chiếu sáng. Chao đèn vải trắng ngà, thân đèn sắt sơn đen mờ. Phù hợp góc đọc sách, phòng ngủ hoặc phòng khách.',
  ARRAY[
    'https://images.unsplash.com/photo-1524484485831-a92ffc0de03f?w=600&h=600&fit=crop&q=80',
    'https://images.unsplash.com/photo-1636368208791-17b81ed832d2?w=600&h=600&fit=crop&q=80',
    'https://images.unsplash.com/photo-1555041469-a586c61ea9bc?w=600&h=600&fit=crop&q=80'
  ],
  'Nordic Home', 3
),
(
  'DEN-TABL-03',
  'Đèn Bàn Cổ Điển Chao Vải Lanh',
  750000, 430000, 80,
  'Đèn bàn thiết kế cổ điển với đế gốm thủ công màu be và chao đèn vải lanh tự nhiên. Ánh sáng khuếch tán nhẹ nhàng, tạo không gian ấm cúng và thư giãn.',
  ARRAY[
    'https://images.unsplash.com/photo-1565814329452-e1efa11c5b89?w=600&h=600&fit=crop&q=80',
    'https://images.unsplash.com/photo-1524484485831-a92ffc0de03f?w=600&h=600&fit=crop&q=80',
    'https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=600&h=600&fit=crop&q=80'
  ],
  'LightArt', 3
),
(
  'DEN-CHAN-04',
  'Đèn Chùm Sputnik Mạ Đồng',
  5500000, 3400000, 12,
  'Đèn chùm Sputnik thiết kế độc đáo với 12 nhánh tỏa ra từ tâm, mỗi nhánh gắn bóng cầu thủy tinh trong. Khung kim loại mạ vàng đồng. Điểm nhấn sang trọng cho phòng khách hay phòng ăn.',
  ARRAY[
    'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=600&h=600&fit=crop&q=80',
    'https://images.unsplash.com/photo-1636368208791-17b81ed832d2?w=600&h=600&fit=crop&q=80',
    'https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=600&h=600&fit=crop&q=80'
  ],
  'Luxe Living', 3
),
(
  'DEN-WALL-05',
  'Đèn Tường Đọc Sách Xoay 360°',
  620000, 350000, 90,
  'Đèn tường đọc sách với khớp xoay 360 độ, điều chỉnh góc chiếu tùy ý. Thiết kế gọn nhẹ, lắp đặt dễ dàng. LED tiết kiệm điện 5W, ánh sáng trắng trung tính 4000K.',
  ARRAY[
    'https://images.unsplash.com/photo-1555041469-a586c61ea9bc?w=600&h=600&fit=crop&q=80',
    'https://images.unsplash.com/photo-1565814329452-e1efa11c5b89?w=600&h=600&fit=crop&q=80',
    'https://images.unsplash.com/photo-1636368208791-17b81ed832d2?w=600&h=600&fit=crop&q=80'
  ],
  'LightArt', 3
);

-- ── SOFA – category_id = 4 ────────────────────────────────
INSERT INTO PRODUCT (sku, name, price, cost_price, stock, description, images, supplier, category_id) VALUES
(
  'SOF-LSEC-01',
  'Sofa Góc Chữ L Vải Bouclé',
  18500000, 12000000, 8,
  'Sofa góc chữ L phủ vải bouclé cao cấp màu kem. Đệm ngồi mút lò xo túi cho độ êm ái tối đa. Chân gỗ sồi thấp. Phù hợp phòng khách rộng, tạo không gian ngồi thoải mái cho cả gia đình.',
  ARRAY[
    'https://images.unsplash.com/photo-1631510390389-c1e4fb20ff31?w=600&h=600&fit=crop&q=80',
    'https://images.unsplash.com/photo-1493663284031-b7e3aefcae8e?w=600&h=600&fit=crop&q=80',
    'https://images.unsplash.com/photo-1540574163026-643ea20ade25?w=600&h=600&fit=crop&q=80'
  ],
  'Luxe Living', 4
),
(
  'SOF-3SEA-02',
  'Sofa Văng 3 Chỗ Phong Cách Hiện Đại',
  9800000, 6200000, 15,
  'Sofa 3 chỗ ngồi với khung gỗ thông chắc chắn, đệm ngồi mút cao 20 cm. Bọc vải polyester chống bụi bẩn. Màu xanh rêu trầm mang đến vẻ tinh tế và hiện đại cho phòng khách.',
  ARRAY[
    'https://images.unsplash.com/photo-1493663284031-b7e3aefcae8e?w=600&h=600&fit=crop&q=80',
    'https://images.unsplash.com/photo-1540574163026-643ea20ade25?w=600&h=600&fit=crop&q=80',
    'https://images.unsplash.com/photo-1631510390389-c1e4fb20ff31?w=600&h=600&fit=crop&q=80'
  ],
  'UrbanForm', 4
),
(
  'SOF-LEAT-03',
  'Sofa Da Thật Ý Nhập Khẩu',
  28000000, 18000000, 5,
  'Sofa da thật full-grain nhập khẩu Italy. Khung sắt gia cường kết hợp gỗ cao su. Đường may tay tỉ mỉ, màu cognac nâu ấm cổ điển. Cam kết chất lượng bền đẹp theo thời gian.',
  ARRAY[
    'https://images.unsplash.com/photo-1634712282287-14ed57b9cc89?w=600&h=600&fit=crop&q=80',
    'https://images.unsplash.com/photo-1493663284031-b7e3aefcae8e?w=600&h=600&fit=crop&q=80',
    'https://images.unsplash.com/photo-1556909114-f6e7ad7d3136?w=600&h=600&fit=crop&q=80'
  ],
  'Luxe Living', 4
),
(
  'SOF-BED-04',
  'Sofa Bed Đa Năng Gấp Mở',
  7200000, 4500000, 20,
  'Sofa bed đa năng: ban ngày làm sofa 2 chỗ ngồi, ban đêm gấp phẳng thành giường đơn rộng 90 cm. Lý tưởng cho phòng nhỏ, căn hộ studio. Bọc vải chống thấm dễ lau chùi.',
  ARRAY[
    'https://images.unsplash.com/photo-1558997519-83ea9252edf8?w=600&h=600&fit=crop&q=80',
    'https://images.unsplash.com/photo-1634712282287-14ed57b9cc89?w=600&h=600&fit=crop&q=80',
    'https://images.unsplash.com/photo-1631510390389-c1e4fb20ff31?w=600&h=600&fit=crop&q=80'
  ],
  'UrbanForm', 4
),
(
  'SOF-OTTO-05',
  'Ghế Ottoman Tròn Bọc Nhung',
  1800000, 1050000, 55,
  'Ghế đôn tròn bọc nhung mềm mại, có thể dùng làm bàn đặt ly, ghế ngồi hoặc kê chân. Ruột mút đặc chắc. Đường kính 50 cm, chân gỗ sồi. Có nhiều màu để lựa chọn.',
  ARRAY[
    'https://images.unsplash.com/photo-1540574163026-643ea20ade25?w=600&h=600&fit=crop&q=80',
    'https://images.unsplash.com/photo-1556909114-f6e7ad7d3136?w=600&h=600&fit=crop&q=80',
    'https://images.unsplash.com/photo-1493663284031-b7e3aefcae8e?w=600&h=600&fit=crop&q=80'
  ],
  'Nordic Home', 4
);

-- ── TỦ & KỆ – category_id = 5 ─────────────────────────────
INSERT INTO PRODUCT (sku, name, price, cost_price, stock, description, images, supplier, category_id) VALUES
(
  'TU-WARD-01',
  'Tủ Quần Áo 4 Cánh Gỗ Công Nghiệp',
  8900000, 5800000, 10,
  'Tủ quần áo 4 cánh trượt với gương full-length ở 2 cánh giữa. Ngăn treo áo dài, ngăn kéo 4 tầng và kệ đựng phụ kiện. Gỗ MDF chống ẩm, bề mặt phủ melamine trắng ngà.',
  ARRAY[
    'https://images.unsplash.com/photo-1506439773649-6e0eb8cfb237?w=600&h=600&fit=crop&q=80',
    'https://images.unsplash.com/photo-1521587760476-6c12a4b040da?w=600&h=600&fit=crop&q=80',
    'https://images.unsplash.com/photo-1522771739844-6a9f6d5f14af?w=600&h=600&fit=crop&q=80'
  ],
  'WoodCraft VN', 5
),
(
  'KE-BOOK-02',
  'Kệ Sách Gỗ Tự Nhiên 5 Tầng',
  3200000, 2000000, 30,
  'Kệ sách đứng 5 tầng làm từ gỗ thông tự nhiên. Dùng trưng bày sách, cây cảnh, đồ trang trí. Thiết kế open-shelf thông thoáng, dễ lau chùi. Kích thước: W80 x D30 x H180 cm.',
  ARRAY[
    'https://images.unsplash.com/photo-1521587760476-6c12a4b040da?w=600&h=600&fit=crop&q=80',
    'https://images.unsplash.com/photo-1595428774223-ef52624120d2?w=600&h=600&fit=crop&q=80',
    'https://images.unsplash.com/photo-1505693314120-0d443867891c?w=600&h=600&fit=crop&q=80'
  ],
  'WoodCraft VN', 5
),
(
  'KE-WALL-03',
  'Kệ Trang Trí Treo Tường Bộ 3',
  890000, 520000, 70,
  'Bộ 3 kệ gỗ treo tường với kích thước khác nhau (S, M, L). Dễ lắp đặt với thanh treo ẩn chắc chắn. Bề mặt gỗ sồi hoặc óc chó. Phù hợp trưng bày ảnh, cây nhỏ, tiểu cảnh.',
  ARRAY[
    'https://images.unsplash.com/photo-1505693314120-0d443867891c?w=600&h=600&fit=crop&q=80',
    'https://images.unsplash.com/photo-1595428774223-ef52624120d2?w=600&h=600&fit=crop&q=80',
    'https://images.unsplash.com/photo-1521587760476-6c12a4b040da?w=600&h=600&fit=crop&q=80'
  ],
  'Nordic Home', 5
),
(
  'TU-SHOE-04',
  'Tủ Giày 12 Ngăn Cửa Lật',
  2600000, 1600000, 25,
  'Tủ giày 12 ngăn cửa lật tiết kiệm diện tích. Mỗi ngăn chứa 1 đôi giày hoặc dép. Bề mặt sơn trắng bóng dễ lau. Tay nắm nhôm tối giản. Kích thước: W60 x D25 x H120 cm.',
  ARRAY[
    'https://images.unsplash.com/photo-1695552839440-c7e7a9e4eac7?w=600&h=600&fit=crop&q=80',
    'https://images.unsplash.com/photo-1506439773649-6e0eb8cfb237?w=600&h=600&fit=crop&q=80',
    'https://images.unsplash.com/photo-1522771739844-6a9f6d5f14af?w=600&h=600&fit=crop&q=80'
  ],
  'UrbanForm', 5
),
(
  'TU-DISP-05',
  'Tủ Trưng Bày Kính Tempered LED',
  5800000, 3700000, 12,
  'Tủ trưng bày 4 tầng với cửa kính cường lực trong suốt. Đèn LED nội thất tích hợp. Lý tưởng trưng bày đồ sưu tầm, mỹ phẩm, đồ trang sức. Khung nhôm màu đen mờ sang trọng.',
  ARRAY[
    'https://images.unsplash.com/photo-1522771739844-6a9f6d5f14af?w=600&h=600&fit=crop&q=80',
    'https://images.unsplash.com/photo-1695552839440-c7e7a9e4eac7?w=600&h=600&fit=crop&q=80',
    'https://images.unsplash.com/photo-1506439773649-6e0eb8cfb237?w=600&h=600&fit=crop&q=80'
  ],
  'Luxe Living', 5
);

-- ── GIƯỜNG – category_id = 6 ──────────────────────────────
INSERT INTO PRODUCT (sku, name, price, cost_price, stock, description, images, supplier, category_id) VALUES
(
  'GIU-OAK-01',
  'Giường Gỗ Sồi King Size Tối Giản',
  14500000, 9500000, 10,
  'Giường đôi King Size (180 x 200 cm) làm từ gỗ sồi nguyên tấm. Đầu giường cao 90 cm với thanh ngang nổi. Khung giường hỗ trợ nệm dày đến 30 cm.',
  ARRAY[
    'https://images.unsplash.com/photo-1631049307264-da0ec9d70304?w=600&h=600&fit=crop&q=80',
    'https://images.unsplash.com/photo-1590490360182-c33d57733427?w=600&h=600&fit=crop&q=80',
    'https://images.unsplash.com/photo-1699208313215-110ab750fa4e?w=600&h=600&fit=crop&q=80'
  ],
  'WoodCraft VN', 6
),
(
  'GIU-UPHL-02',
  'Giường Đầu Bọc Nhung Xanh Đậm',
  11200000, 7000000, 12,
  'Giường Queen Size (160 x 200 cm) với đầu giường bọc nhung xanh đậm ôm cong sang trọng. Khung gỗ cao su bền chắc. Tông màu jewel tone tạo điểm nhấn ấn tượng cho phòng ngủ.',
  ARRAY[
    'https://images.unsplash.com/photo-1590490360182-c33d57733427?w=600&h=600&fit=crop&q=80',
    'https://images.unsplash.com/photo-1699208313215-110ab750fa4e?w=600&h=600&fit=crop&q=80',
    'https://images.unsplash.com/photo-1631049307264-da0ec9d70304?w=600&h=600&fit=crop&q=80'
  ],
  'Luxe Living', 6
),
(
  'GIU-PLAT-03',
  'Giường Nền Thấp Phong Cách Nhật Bản',
  8800000, 5600000, 18,
  'Giường platform thấp sàn theo phong cách Japandi. Khung gỗ sồi thấp 20 cm, mặt sàn nệm phẳng rộng. Tạo cảm giác gần gũi thiên nhiên, không gian phòng ngủ thanh tịnh và thoáng đãng.',
  ARRAY[
    'https://images.unsplash.com/photo-1699208313215-110ab750fa4e?w=600&h=600&fit=crop&q=80',
    'https://images.unsplash.com/photo-1631049307264-da0ec9d70304?w=600&h=600&fit=crop&q=80',
    'https://images.unsplash.com/photo-1590490360182-c33d57733427?w=600&h=600&fit=crop&q=80'
  ],
  'Nordic Home', 6
),
(
  'GIU-BUNK-04',
  'Giường Tầng Trẻ Em Gỗ Thông',
  6500000, 4200000, 15,
  'Giường tầng 2 tầng dành cho trẻ em từ 6 tuổi. Khung gỗ thông nhập khẩu chắc chắn, cầu thang bên với ngăn kéo lưu trữ. Thanh chắn an toàn tầng trên. Có thể tháo rời thành 2 giường đơn.',
  ARRAY[
    'https://images.unsplash.com/photo-1555854877-bab0e564b8d5?w=600&h=600&fit=crop&q=80',
    'https://images.unsplash.com/photo-1590490360182-c33d57733427?w=600&h=600&fit=crop&q=80',
    'https://images.unsplash.com/photo-1631049307264-da0ec9d70304?w=600&h=600&fit=crop&q=80'
  ],
  'WoodCraft VN', 6
),
(
  'GIU-CANO-05',
  'Giường Canopy Lãng Mạn Khung Sắt',
  16900000, 11000000, 6,
  'Giường canopy 4 cột khung sắt đánh bóng màu đồng vàng. Kèm màn voan trắng bồng bềnh tạo không gian phòng ngủ như resort nghỉ dưỡng. Kích thước Queen (160 x 200 cm).',
  ARRAY[
    'https://images.unsplash.com/photo-1629230324981-0293a8ae17e9?w=600&h=600&fit=crop&q=80',
    'https://images.unsplash.com/photo-1631049307264-da0ec9d70304?w=600&h=600&fit=crop&q=80',
    'https://images.unsplash.com/photo-1699208313215-110ab750fa4e?w=600&h=600&fit=crop&q=80'
  ],
  'Luxe Living', 6
);

-- ============================================================
-- 5. ĐƠN HÀNG & CHI TIẾT ĐƠN HÀNG
--    final_price sẽ được tính lại chính xác ở cuối file.
-- ============================================================

-- ── Đơn 1: Phạm Thị Diễm – Delivered (25 ngày trước) ─────
INSERT INTO ORDERS (created_time, final_price, status, customer_id, account_id,
  shipping_address, recipient_name, recipient_phone, recipient_email, is_deleted)
VALUES (NOW() - INTERVAL '25 days', 0, 'Delivered', 1, 2,
  '12 Nguyễn Huệ, Quận 1, TP.HCM',
  'Phạm Thị Diễm', '0901111111', 'diem.pham@gmail.com', false);
INSERT INTO ORDER_ITEM (order_id, product_id, quantity, unit_sale_price, total_price) VALUES
  (1, 1,  2, 1850000,  3700000),   -- 2× Ghế Gỗ Bắc Âu
  (1, 6,  1, 12500000, 12500000),  -- Bàn Ăn Gỗ Sồi
  (1, 12, 1, 2100000,  2100000);   -- Đèn Sàn Bắc Âu

-- ── Đơn 2: Hoàng Văn Đức – Delivered (20 ngày trước) ─────
INSERT INTO ORDERS (created_time, final_price, status, customer_id, account_id,
  shipping_address, recipient_name, recipient_phone, recipient_email, is_deleted)
VALUES (NOW() - INTERVAL '20 days', 0, 'Delivered', 2, 1,
  '45 Lê Lợi, Quận 3, TP.HCM',
  'Hoàng Văn Đức', '0902222222', 'duc.hoang@gmail.com', false);
INSERT INTO ORDER_ITEM (order_id, product_id, quantity, unit_sale_price, total_price) VALUES
  (2, 17, 1, 9800000, 9800000);   -- Sofa Văng 3 Chỗ

-- ── Đơn 3: Vũ Thị Hương – Shipped (5 ngày trước) ─────────
INSERT INTO ORDERS (created_time, final_price, status, customer_id, account_id,
  shipping_address, recipient_name, recipient_phone, recipient_email, is_deleted)
VALUES (NOW() - INTERVAL '5 days', 0, 'Shipped', 3, 2,
  '78 Trần Hưng Đạo, Quận 5, TP.HCM',
  'Vũ Thị Hương', '0903333333', 'huong.vu@gmail.com', false);
INSERT INTO ORDER_ITEM (order_id, product_id, quantity, unit_sale_price, total_price) VALUES
  (3, 11, 1, 980000,  980000),   -- Đèn Thả Trần Edison
  (3, 13, 2, 750000,  1500000),  -- 2× Đèn Bàn Cổ Điển
  (3, 22, 1, 3200000, 3200000),  -- Kệ Sách Gỗ 5 Tầng
  (3, 23, 1, 890000,  890000);   -- Kệ Treo Tường Bộ 3

-- ── Đơn 4: Đặng Văn Khoa – Processing (2 ngày trước) ─────
INSERT INTO ORDERS (created_time, final_price, status, customer_id, account_id,
  shipping_address, recipient_name, recipient_phone, recipient_email, is_deleted)
VALUES (NOW() - INTERVAL '2 days', 0, 'Processing', 4, 3,
  '23 Đinh Tiên Hoàng, Bình Thạnh, TP.HCM',
  'Đặng Văn Khoa', '0904444444', 'khoa.dang@gmail.com', false);
INSERT INTO ORDER_ITEM (order_id, product_id, quantity, unit_sale_price, total_price) VALUES
  (4, 18, 1, 28000000, 28000000); -- Sofa Da Thật Ý

-- ── Đơn 5: Ngô Thị Lan – Created (hôm qua) ───────────────
INSERT INTO ORDERS (created_time, final_price, status, customer_id, account_id,
  shipping_address, recipient_name, recipient_phone, recipient_email, is_deleted)
VALUES (NOW() - INTERVAL '1 day', 0, 'Created', 5, 2,
  '56 Pasteur, Quận 1, TP.HCM',
  'Ngô Thị Lan', '0905555555', 'lan.ngo@gmail.com', false);
INSERT INTO ORDER_ITEM (order_id, product_id, quantity, unit_sale_price, total_price) VALUES
  (5, 26, 1, 14500000, 14500000), -- Giường Gỗ Sồi King
  (5, 24, 1, 2600000,  2600000);  -- Tủ Giày 12 Ngăn

-- ── Đơn 6: Bùi Văn Minh – Delivered (40 ngày trước) ──────
INSERT INTO ORDERS (created_time, final_price, status, customer_id, account_id,
  shipping_address, recipient_name, recipient_phone, recipient_email, is_deleted)
VALUES (NOW() - INTERVAL '40 days', 0, 'Delivered', 6, 1,
  '90 Cách Mạng Tháng 8, Quận 10, TP.HCM',
  'Bùi Văn Minh', '0906666666', 'minh.bui@gmail.com', false);
INSERT INTO ORDER_ITEM (order_id, product_id, quantity, unit_sale_price, total_price) VALUES
  (6, 4,  1, 6800000, 6800000),  -- Ghế Bập Bênh Walnut
  (6, 15, 2, 620000,  1240000);  -- 2× Đèn Tường Đọc Sách

-- ── Đơn 7: Đỗ Thị Ngọc – Cancelled (15 ngày trước) ──────
INSERT INTO ORDERS (created_time, final_price, status, customer_id, account_id,
  shipping_address, recipient_name, recipient_phone, recipient_email, is_deleted)
VALUES (NOW() - INTERVAL '15 days', 0, 'Cancelled', 7, 3,
  '34 Võ Văn Tần, Quận 3, TP.HCM',
  'Đỗ Thị Ngọc', '0907777777', 'ngoc.do@gmail.com', false);
INSERT INTO ORDER_ITEM (order_id, product_id, quantity, unit_sale_price, total_price) VALUES
  (7, 14, 1, 5500000, 5500000),  -- Đèn Chùm Sputnik
  (7, 20, 2, 1800000, 3600000);  -- 2× Ghế Ottoman

-- ── Đơn 8: Lý Văn Phúc – Delivered (60 ngày trước) ───────
INSERT INTO ORDERS (created_time, final_price, status, customer_id, account_id,
  shipping_address, recipient_name, recipient_phone, recipient_email, is_deleted)
VALUES (NOW() - INTERVAL '60 days', 0, 'Delivered', 8, 2,
  '67 Hai Bà Trưng, Quận 1, TP.HCM',
  'Lý Văn Phúc', '0908888888', 'phuc.ly@gmail.com', false);
INSERT INTO ORDER_ITEM (order_id, product_id, quantity, unit_sale_price, total_price) VALUES
  (8, 27, 1, 11200000, 11200000), -- Giường Đầu Bọc Nhung
  (8, 28, 1, 8800000,  8800000),  -- Giường Nền Thấp Nhật Bản
  (8, 12, 2, 2100000,  4200000);  -- 2× Đèn Sàn Bắc Âu

-- ── Đơn 9: Phạm Thị Diễm – Delivered (55 ngày trước) ─────
INSERT INTO ORDERS (created_time, final_price, status, customer_id, account_id,
  shipping_address, recipient_name, recipient_phone, recipient_email, is_deleted)
VALUES (NOW() - INTERVAL '55 days', 0, 'Delivered', 1, 1,
  '12 Nguyễn Huệ, Quận 1, TP.HCM',
  'Phạm Thị Diễm', '0901111111', 'diem.pham@gmail.com', false);
INSERT INTO ORDER_ITEM (order_id, product_id, quantity, unit_sale_price, total_price) VALUES
  (9, 16, 1, 18500000, 18500000); -- Sofa Góc Chữ L Bouclé

-- ── Đơn 10: Hoàng Văn Đức – Delivered (45 ngày trước) ────
INSERT INTO ORDERS (created_time, final_price, status, customer_id, account_id,
  shipping_address, recipient_name, recipient_phone, recipient_email, is_deleted)
VALUES (NOW() - INTERVAL '45 days', 0, 'Delivered', 2, 3,
  '45 Lê Lợi, Quận 3, TP.HCM',
  'Hoàng Văn Đức', '0902222222', 'duc.hoang@gmail.com', false);
INSERT INTO ORDER_ITEM (order_id, product_id, quantity, unit_sale_price, total_price) VALUES
  (10, 21, 1, 8900000, 8900000),  -- Tủ Quần Áo 4 Cánh
  (10, 2,  1, 4200000, 4200000),  -- Ghế Thư Giãn Bọc Vải
  (10, 15, 4, 620000,  2480000);  -- 4× Đèn Tường Đọc Sách

-- ── Tính lại final_price chính xác dựa trên order_item ───
UPDATE ORDERS
SET final_price = (
  SELECT COALESCE(SUM(total_price), 0)
  FROM ORDER_ITEM
  WHERE ORDER_ITEM.order_id = ORDERS.order_id
);
