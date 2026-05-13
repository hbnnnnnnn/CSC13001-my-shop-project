-- =============================================================================
-- 06_furniture_expand.sql
-- Mở rộng catalog nội thất: 3 danh mục × 22 sản phẩm + đơn hàng demo.
-- Idempotent: xoá toàn bộ đơn hàng / sản phẩm / danh mục rồi nạp lại.
-- =============================================================================

BEGIN;

-- ─── 1. CLEANUP ──────────────────────────────────────────────────────────────
DELETE FROM orders;
DELETE FROM product;
DELETE FROM category;
DELETE FROM customer WHERE email IN (
    'reportdemo.furniture@local.test',
    'reportexpand@local.test'
);
ALTER SEQUENCE category_category_id_seq     RESTART WITH 1;
ALTER SEQUENCE product_product_id_seq       RESTART WITH 1;
ALTER SEQUENCE orders_order_id_seq          RESTART WITH 1;
ALTER SEQUENCE order_item_order_item_id_seq RESTART WITH 1;

-- ─── 2. DANH MỤC (3) ────────────────────────────────────────────────────────
-- category_id: Ghế=1  Bàn=2  Giường=3
INSERT INTO CATEGORY (name, description) VALUES
('Ghế',    'Ghế ăn, ghế thư giãn, ghế bar, ghế bập bênh và các loại ghế nội thất gia đình.'),
('Bàn',    'Bàn ăn, bàn cà phê, bàn làm việc, bàn đầu giường và các loại bàn nội thất.'),
('Giường', 'Giường ngủ các kích cỡ king, queen, đơn; đầu bọc nệm, gỗ tự nhiên và sắt.');

-- ─── 3. SẢN PHẨM ─────────────────────────────────────────────────────────────
-- Ghế   product_id  1-22   category_id=1
-- Bàn   product_id 23-44   category_id=2
-- Giường product_id 45-66  category_id=3

INSERT INTO PRODUCT (sku, name, price, cost_price, stock, description, images, supplier, category_id) VALUES

-- ══ GHẾ (22) ══════════════════════════════════════════════════════════════════
('GHE-DINA-01','Ghế Ăn Gỗ Bắc Âu Chân Côn', 1290000, 900000, 50,
 'Ghế ăn thiết kế Scandinavian với chân côn gỗ sồi và lưng thoáng. Phù hợp bàn ăn 4–6 người.',
 ARRAY[
   'https://images.unsplash.com/photo-1560440021-33f9b867899d?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1598300042247-d088f8ab3a91?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1506439773649-6e0eb8cfb237?w=600&h=600&fit=crop&q=80'
 ], 'NordHome', 1),

('GHE-DINA-02','Ghế Ăn Bọc Nệm Hiện Đại', 1650000, 1155000, 40,
 'Ghế ăn đệm bọc vải linen mềm mại, chân gỗ sồi tự nhiên. Dễ phối màu đa phong cách.',
 ARRAY[
   'https://images.unsplash.com/photo-1567538096630-e0c55bd6374c?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1524758631624-e2822e304c36?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1586023492125-27b2c045efd7?w=600&h=600&fit=crop&q=80'
 ], 'LivingSpace', 1),

('GHE-DINA-03','Ghế Ăn Chân Sắt Mặt Gỗ', 980000, 686000, 60,
 'Kết hợp khung sắt công nghiệp và mặt ngồi gỗ thông. Bền bỉ, dễ vệ sinh hàng ngày.',
 ARRAY[
   'https://images.unsplash.com/photo-1586023492125-27b2c045efd7?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1592078615290-033ee584e267?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1503602642458-232111445657?w=600&h=600&fit=crop&q=80'
 ], 'IronWood', 1),

('GHE-DINA-04','Ghế Ăn Đan Mây Tự Nhiên', 1450000, 1015000, 35,
 'Lưng đan mây kết hợp chân gỗ tần bì. Phong cách boho tươi sáng cho phòng ăn.',
 ARRAY[
   'https://images.unsplash.com/photo-1617364852223-75f57e78dc96?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1586023492125-27b2c045efd7?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1598300042247-d088f8ab3a91?w=600&h=600&fit=crop&q=80'
 ], 'BohoLiving', 1),

('GHE-DINA-05','Ghế Ăn Nhựa Polypropylene', 590000, 413000, 100,
 'Ghế nhựa PP cao cấp, nhẹ và bền. Xếp chồng được, phù hợp nhà bếp nhỏ hoặc ban công.',
 ARRAY[
   'https://images.unsplash.com/photo-1506439773649-6e0eb8cfb237?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1567538096630-e0c55bd6374c?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1524758631624-e2822e304c36?w=600&h=600&fit=crop&q=80'
 ], 'StackPro', 1),

('GHE-LOUN-01','Ghế Thư Giãn Bọc Vải Cao Cấp', 4200000, 2940000, 25,
 'Ghế thư giãn góc nghiêng thoải mái, bọc vải chenille mềm. Kèm đệm tựa đầu rời.',
 ARRAY[
   'https://images.unsplash.com/photo-1592078615290-033ee584e267?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1503602642458-232111445657?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1586023492125-27b2c045efd7?w=600&h=600&fit=crop&q=80'
 ], 'ComfortZone', 1),

('GHE-LOUN-02','Ghế Thư Giãn Da Tổng Hợp', 5800000, 4060000, 20,
 'Bọc da PU cao cấp, khung gỗ kết hợp chân kim loại. Bề mặt dễ lau chùi.',
 ARRAY[
   'https://images.unsplash.com/photo-1605797063353-4cbcb6027f2c?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1579656381229-15bdb188da49?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1561997315-64748c1af8c8?w=600&h=600&fit=crop&q=80'
 ], 'LeatherCraft', 1),

('GHE-LOUN-03','Ghế Thư Giãn Khung Walnut', 6500000, 4550000, 15,
 'Khung walnut Mỹ nguyên khối, đệm bọc vải kem sang trọng. Thiết kế mid-century hiện đại.',
 ARRAY[
   'https://images.unsplash.com/photo-1567538096630-e0c55bd6374c?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1524758631624-e2822e304c36?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1592078615290-033ee584e267?w=600&h=600&fit=crop&q=80'
 ], 'WalnutStudio', 1),

('GHE-BAR-01','Ghế Bar Bọc Da Chân Cao', 2350000, 1645000, 30,
 'Độ cao điều chỉnh, bọc da PU đen, chân inox sáng bóng. Phù hợp quầy bar gia đình.',
 ARRAY[
   'https://images.unsplash.com/photo-1777618202827-409eaf6dcac4?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1639690326883-6f4ce9e362a9?w=800&auto=format',
   'https://images.unsplash.com/photo-1777739287700-c7d856df5537?w=800&auto=format'
 ], 'BarStudio', 1),

('GHE-BAR-02','Ghế Bar Gỗ Tự Nhiên', 1890000, 1323000, 25,
 'Gỗ thông tự nhiên, mặt ngồi tròn, chân cao vững chắc. Kết hợp tốt với bàn ăn kiểu cao.',
 ARRAY[
   'https://images.unsplash.com/photo-1598300042247-d088f8ab3a91?w=800&auto=format',
   'https://images.unsplash.com/photo-1506439773649-6e0eb8cfb237?w=800&auto=format',
   'https://images.unsplash.com/photo-1758685493098-d3a09d4044d0?w=600&h=600&fit=crop&q=80'
 ], 'NaturalBar', 1),

('GHE-BAR-03','Ghế Bar Công Nghiệp Chân Sắt', 1450000, 1015000, 35,
 'Khung sắt sơn tĩnh điện đen, đệm da mỏng. Phong cách công nghiệp cá tính.',
 ARRAY[
   'https://images.unsplash.com/photo-1776890579663-383de3a63d70?w=600&h=600&fit=crop&q=80',
   'https://plus.unsplash.com/premium_photo-1661878621378-e4d4f84b5f83?w=600&h=600&fit=crop&q=80',
   'https://plus.unsplash.com/premium_photo-1661963667668-f53a412a5922?w=600&h=600&fit=crop&q=80'
 ], 'IronBar', 1),

('GHE-ROCK-01','Ghế Bập Bênh Gỗ Walnut', 6800000, 4760000, 12,
 'Gỗ walnut Mỹ nguyên khối, đệm bọc vải tweed. Điểm nhấn tinh tế cho phòng khách.',
 ARRAY[
   'https://plus.unsplash.com/premium_photo-1669757697520-8aa68d8fad32?w=800&auto=format',
   'https://images.unsplash.com/photo-1729854193151-79127a0f5f32?w=600&h=600&fit=crop&q=80',
   'https://plus.unsplash.com/premium_photo-1682484703057-a197869384df?w=600&h=600&fit=crop&q=80'
 ], 'WalnutStudio', 1),

('GHE-ROCK-02','Ghế Bập Bênh Bọc Nệm Dày', 4500000, 3150000, 15,
 'Khung gỗ sồi, nệm ngồi và lưng bọc vải linen dày. Lý tưởng cho giờ thư giãn.',
 ARRAY[
   'https://plus.unsplash.com/premium_photo-1705423215849-0d632ca9b38f?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1729854193151-79127a0f5f32?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1641310548581-48c3cd11727e?w=800&auto=format'
 ], 'ComfortZone', 1),

('GHE-ACCE-01','Ghế Accent Nhung Đơn', 3200000, 2240000, 20,
 'Bọc nhung màu xanh rêu, chân gỗ tự nhiên. Điểm nhấn ấn tượng cho phòng ngủ.',
 ARRAY[
   'https://images.unsplash.com/photo-1567538096630-e0c55bd6374c?w=800&auto=format',
   'https://images.unsplash.com/photo-1586023492125-27b2c045efd7?w=800&auto=format',
   'https://plus.unsplash.com/premium_photo-1723874468810-3147a74bb3a7?w=600&h=600&fit=crop&q=80'
 ], 'VelvetHome', 1),

('GHE-ACCE-02','Ghế Accent Chân Vàng Bọc Vải', 4800000, 3360000, 18,
 'Khung kim loại mạ vàng, bọc vải gân caro. Phong cách glamour cho phòng khách.',
 ARRAY[
   'https://plus.unsplash.com/premium_photo-1705479742826-cb265b9d6999?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1619596658767-f3bbb82b0dee?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1734626317358-dde736aaca15?w=600&h=600&fit=crop&q=80'
 ], 'GoldHome', 1),

('GHE-WKCH-01','Ghế Làm Việc Lưng Thấp Gỗ', 2100000, 1470000, 30,
 'Gỗ cao su, lưng thấp thoáng mát, không gây đổ mồ hôi. Thích hợp home office.',
 ARRAY[
   'https://images.unsplash.com/photo-1598300042247-d088f8ab3a91?w=800&auto=format',
   'https://images.unsplash.com/photo-1524758631624-e2822e304c36?w=800&auto=format',
   'https://images.unsplash.com/photo-1567538096630-e0c55bd6374c?w=800&auto=format'
 ], 'WorkStyle', 1),

('GHE-WKCH-02','Ghế Làm Việc Ergonomic Gỗ', 3750000, 2625000, 22,
 'Đệm eo hỗ trợ cột sống, khung gỗ tần bì. Phù hợp ngồi làm việc nhiều giờ.',
 ARRAY[
   'https://images.unsplash.com/photo-1506439773649-6e0eb8cfb237?w=800&auto=format',
   'https://images.unsplash.com/photo-1586023492125-27b2c045efd7?w=800&auto=format',
   'https://images.unsplash.com/photo-1592078615290-033ee584e267?w=800&auto=format'
 ], 'ErgoWood', 1),

('GHE-OTTO-01','Ghế Ottoman Bọc Nhung Chân Gỗ', 1800000, 1260000, 35,
 'Ottoman nhỏ bọc nhung navy, chân gỗ tự nhiên. Dùng kê chân hoặc làm bàn cà phê mini.',
 ARRAY[
   'https://plus.unsplash.com/premium_photo-1705169612592-32610774a5d0?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1506898667547-42e22a46e125?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1598300042247-d088f8ab3a91?w=800&auto=format'
 ], 'OttomanCo', 1),

('GHE-OTTO-02','Ghế Ottoman Tròn Nắp Gập', 2200000, 1540000, 28,
 'Nắp gập lưu trữ đồ bên trong, bọc vải dệt thủ công. Vừa ngồi vừa chứa đồ.',
 ARRAY[
   'https://plus.unsplash.com/premium_photo-1664699106353-fdd2ddd86bd8?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1690618299438-cb453b14bab3?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1711049138957-792eec11dc88?w=600&h=600&fit=crop&q=80'
 ], 'StorageSeat', 1),

('GHE-BENC-01','Ghế Bench Cuối Giường Bọc Da', 3400000, 2380000, 20,
 'Dài 120cm, bọc da PU màu camel, chân gỗ sồi. Đặt cuối giường tạo điểm nhấn sang trọng.',
 ARRAY[
   'https://images.unsplash.com/photo-1772696860294-c40874ceb9b1?w=600&h=600&fit=crop&q=80',
   'https://plus.unsplash.com/premium_photo-1670274229154-a6d8ed9c5f88?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1573393772683-2db1c967eb3d?w=600&h=600&fit=crop&q=80'
 ], 'BenchMark', 1),

('GHE-SCAN-01','Ghế Gỗ Bắc Âu Đa Năng', 1850000, 1295000, 45,
 'Gỗ beech uốn cong tự nhiên phong cách Bắc Âu. Nhẹ, bền và dễ xếp chồng khi cần.',
 ARRAY[
   'https://plus.unsplash.com/premium_photo-1682410454254-809412bf04bd?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1647296168187-c5f8b5244c38?w=600&h=600&fit=crop&q=80',
   'https://plus.unsplash.com/premium_photo-1682582245151-aa44d698771f?w=600&h=600&fit=crop&q=80'
 ], 'NordHome', 1),

('GHE-KIDS-01','Ghế Học Trẻ Em Điều Chỉnh Cao', 1200000, 840000, 40,
 'Điều chỉnh chiều cao và góc nghiêng, thích hợp trẻ 4–12 tuổi. Thiết kế ergonomic nhỏ.',
 ARRAY[
   'https://images.unsplash.com/photo-1748887522207-3a5a9097bc43?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1750306957077-b74e45fe1819?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1748629760601-17a706aa35de?w=600&h=600&fit=crop&q=80'
 ], 'KidsRoom', 1),

-- ══ BÀN (22) ══════════════════════════════════════════════════════════════════
('BAN-DINE-01','Bàn Ăn Gỗ Sồi 4 Người 120cm', 8500000, 5950000, 20,
 'Mặt gỗ sồi Mỹ nguyên tấm, chân côn gỗ sồi. Kích thước 120×75cm, đủ cho 4 người.',
 ARRAY[
   'https://images.unsplash.com/photo-1694830470410-2339a679c942?w=800&auto=format',
   'https://plus.unsplash.com/premium_photo-1670869816874-5a22db823d6f?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1694830470387-2e0f234ecaf7?w=800&auto=format'
 ], 'OakMaster', 2),

('BAN-DINE-02','Bàn Ăn Gỗ Sồi 6 Người 160cm', 12500000, 8750000, 15,
 'Mặt sồi tự nhiên dài 160cm, chân chữ X vững chắc. Phù hợp phòng ăn rộng.',
 ARRAY[
   'https://images.unsplash.com/photo-1694830470410-2339a679c942?w=800&auto=format',
   'https://images.unsplash.com/photo-1694830470387-2e0f234ecaf7?w=800&auto=format',
   'https://images.unsplash.com/photo-1776219189131-ed08498b49a7?w=600&h=600&fit=crop&q=80'
 ], 'OakMaster', 2),

('BAN-DINE-03','Bàn Ăn Chân Sắt Mặt Đá Marble', 14800000, 10360000, 10,
 'Mặt đá marble trắng vân xám, chân sắt mạ đen. Sang trọng và dễ vệ sinh.',
 ARRAY[
   'https://plus.unsplash.com/premium_photo-1704812102666-c661be1fc614?w=800&auto=format',
   'https://images.unsplash.com/photo-1608635661411-b8de70247ea5?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1608635661512-52c656e0d4e5?w=600&h=600&fit=crop&q=80'
 ], 'MarbleHome', 2),

('BAN-DINE-04','Bàn Ăn 4 Người Gỗ Thông', 6200000, 4340000, 18,
 'Đường kính 110cm, chân trụ đơn vững chắc. Hình tròn tiết kiệm không gian góc phòng.',
 ARRAY[
   'https://images.unsplash.com/photo-1618221195710-dd6b41faaea6?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1694830470387-2e0f234ecaf7?w=800&auto=format',
   'https://images.unsplash.com/photo-1776219189131-ed08498b49a7?w=600&h=600&fit=crop&q=80'
 ], 'PineTable', 2),

('BAN-DINE-05','Bàn Ăn Gấp Tiết Kiệm Không Gian', 3200000, 2240000, 30,
 'Hai cánh gập mở rộng từ 50cm lên 120cm. Lý tưởng cho căn hộ nhỏ.',
 ARRAY[
   'https://plus.unsplash.com/premium_photo-1664392242935-f22e728a4667?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1562548726-fa37d0de9592?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1722858958428-f4764bb44157?w=600&h=600&fit=crop&q=80'
 ], 'SpaceSaver', 2),

('BAN-COFE-01','Bàn Cà Phê Gỗ Oval Tự Nhiên', 4500000, 3150000, 22,
 'Mặt oval gỗ acacia nguyên khối, chân chữ X gỗ sồi. Vân gỗ tự nhiên độc đáo.',
 ARRAY[
   'https://images.unsplash.com/photo-1758565811033-84d1365000c6?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1679521878363-6987b06a30f7?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1582505021096-297bd76dd9e5?w=600&h=600&fit=crop&q=80'
 ], 'AcaciaWood', 2),

('BAN-COFE-02','Bàn Cà Phê Tối Giản Chân Gỗ', 3800000, 2660000, 28,
 'Mặt MDF sơn mờ trắng, chân gỗ sồi vàng. Phong cách minimal hiện đại.',
 ARRAY[
   'https://images.unsplash.com/photo-1533090161767-e6ffed986c88?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1532372576444-dda954194ad0?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1618221195710-dd6b41faaea6?w=600&h=600&fit=crop&q=80'
 ], 'MinimalHome', 2),

('BAN-COFE-03','Bàn Cà Phê Kính Cường Lực', 5200000, 3640000, 15,
 'Khung kim loại mạ vàng, mặt kính cường lực 12mm. Tạo cảm giác rộng rãi cho phòng khách.',
 ARRAY[
   'https://images.unsplash.com/photo-1647967527216-adea2f078e07?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1563146413-d915a569d6b1?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1523841790171-6189ac051a5c?w=600&h=600&fit=crop&q=80'
 ], 'GlassHome', 2),

('BAN-COFE-04','Bàn Cà Phê Thấp Kiểu Nhật', 2800000, 1960000, 25,
 'Cao 35cm kiểu ngồi bệt Nhật Bản, gỗ tần bì walnut. Kết hợp tốt cùng chiếu tatami.',
 ARRAY[
   'https://images.unsplash.com/photo-1612372606404-0ab33e7187ee?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1449247709967-d4461a6a6103?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1533090161767-e6ffed986c88?w=600&h=600&fit=crop&q=80'
 ], 'ZenStyle', 2),

('BAN-SIDE-01','Bàn Phụ Tròn Chân Vàng', 1200000, 840000, 40,
 'Đường kính 45cm, mặt MDF trắng, chân kim loại mạ vàng. Đặt cạnh ghế sofa rất hợp.',
 ARRAY[
   'https://images.unsplash.com/photo-1611269154421-4e27233ac5c7?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1532372576444-dda954194ad0?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1618221195710-dd6b41faaea6?w=600&h=600&fit=crop&q=80'
 ], 'GoldHome', 2),

('BAN-SIDE-02','Bàn Phụ Mặt Đá Cẩm Thạch', 2500000, 1750000, 25,
 'Mặt đá cẩm thạch xanh rêu tự nhiên, chân trụ đồng. Điểm nhấn độc đáo bên ghế sofa.',
 ARRAY[
   'https://images.unsplash.com/photo-1600585152220-90363fe7e115?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1594026112284-02bb6f3352fe?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1612372606404-0ab33e7187ee?w=600&h=600&fit=crop&q=80'
 ], 'MarbleHome', 2),

('BAN-WORK-01','Bàn Làm Việc Gỗ Thông 120cm', 5200000, 3640000, 20,
 'Gỗ thông Bắc Mỹ 120cm, chân chữ A thép trắng. Bền và thân thiện với home office.',
 ARRAY[
   'https://images.unsplash.com/photo-1449247709967-d4461a6a6103?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1550226891-ef816aed4a98?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1533090161767-e6ffed986c88?w=600&h=600&fit=crop&q=80'
 ], 'WorkStyle', 2),

('BAN-WORK-02','Bàn Làm Việc Góc L 140cm', 7800000, 5460000, 15,
 'Góc chữ L 140×140cm, MDF phủ melamine, chân thép đen. Rộng rãi cho đa nhiệm.',
 ARRAY[
   'https://images.unsplash.com/photo-1532372576444-dda954194ad0?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1611269154421-4e27233ac5c7?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1594026112284-02bb6f3352fe?w=600&h=600&fit=crop&q=80'
 ], 'CornerDesk', 2),

('BAN-WORK-03','Bàn Làm Việc Đứng Điều Chỉnh', 9500000, 6650000, 12,
 'Điều chỉnh điện tử 70–120cm, mặt gỗ sồi, khung thép. Tốt cho sức khỏe cột sống.',
 ARRAY[
   'https://images.unsplash.com/photo-1618221195710-dd6b41faaea6?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1600585152220-90363fe7e115?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1449247709967-d4461a6a6103?w=600&h=600&fit=crop&q=80'
 ], 'StandDesk', 2),

('BAN-NITE-01','Bàn Đầu Giường Gỗ Thông', 1650000, 1155000, 35,
 'Gỗ thông tự nhiên với ngăn mở 45×40×55cm. Gọn nhẹ, đặt cạnh giường tiện lợi.',
 ARRAY[
   'https://images.unsplash.com/photo-1612372606404-0ab33e7187ee?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1550226891-ef816aed4a98?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1533090161767-e6ffed986c88?w=600&h=600&fit=crop&q=80'
 ], 'PineBed', 2),

('BAN-NITE-02','Bàn Đầu Giường 2 Ngăn Kéo', 2100000, 1470000, 30,
 'Hai ngăn kéo vân gỗ walnut, chân côn. Vừa đẹp vừa tiện lưu trữ bên giường ngủ.',
 ARRAY[
   'https://images.unsplash.com/photo-1611269154421-4e27233ac5c7?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1532372576444-dda954194ad0?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1618221195710-dd6b41faaea6?w=600&h=600&fit=crop&q=80'
 ], 'NightStand', 2),

('BAN-CONS-01','Bàn Console Vintage Gỗ Tái Chế', 4500000, 3150000, 15,
 'Hẹp 120×35cm từ gỗ tái chế, để hành lang hoặc phòng khách. Phong cách vintage.',
 ARRAY[
   'https://images.unsplash.com/photo-1600585152220-90363fe7e115?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1594026112284-02bb6f3352fe?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1449247709967-d4461a6a6103?w=600&h=600&fit=crop&q=80'
 ], 'VintageWood', 2),

('BAN-CONS-02','Bàn Console Chân Vàng Thanh Lịch', 3800000, 2660000, 18,
 'Mặt marble giả trắng, chân mạ vàng. Duyên dáng đặt tại lối vào hoặc phòng ăn.',
 ARRAY[
   'https://images.unsplash.com/photo-1612372606404-0ab33e7187ee?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1550226891-ef816aed4a98?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1611269154421-4e27233ac5c7?w=600&h=600&fit=crop&q=80'
 ], 'GoldHome', 2),

('BAN-TELE-01','Kệ TV Gỗ Đơn Giản 120cm', 3200000, 2240000, 22,
 'Dài 120cm, 2 ngăn mở gỗ thông, chân gỗ thấp. Phù hợp TV 40–55 inch.',
 ARRAY[
   'https://images.unsplash.com/photo-1449247709967-d4461a6a6103?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1533090161767-e6ffed986c88?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1532372576444-dda954194ad0?w=600&h=600&fit=crop&q=80'
 ], 'MediaHome', 2),

('BAN-TELE-02','Kệ TV Hiện Đại Có Cửa 150cm', 4900000, 3430000, 18,
 '150cm với 2 cánh cửa che đồ, mặt walnut, chân thép đen. Gọn gàng và hiện đại.',
 ARRAY[
   'https://images.unsplash.com/photo-1618221195710-dd6b41faaea6?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1600585152220-90363fe7e115?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1594026112284-02bb6f3352fe?w=600&h=600&fit=crop&q=80'
 ], 'MediaHome', 2),

('BAN-DRES-01','Bàn Trang Điểm Có Gương LED', 4200000, 2940000, 14,
 'Gương LED 3 khúc, 2 ngăn kéo, mặt kính cường lực. Ánh sáng đều cho trang điểm.',
 ARRAY[
   'https://images.unsplash.com/photo-1612372606404-0ab33e7187ee?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1550226891-ef816aed4a98?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1611269154421-4e27233ac5c7?w=600&h=600&fit=crop&q=80'
 ], 'BeautyDesk', 2),

('BAN-STUD-01','Bàn Học Học Sinh Có Ngăn Sách', 2400000, 1680000, 28,
 '100×55cm, ngăn sách dọc bên phải, mặt MDF chống xước. Phù hợp từ tiểu học đến THPT.',
 ARRAY[
   'https://images.unsplash.com/photo-1532372576444-dda954194ad0?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1449247709967-d4461a6a6103?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1533090161767-e6ffed986c88?w=600&h=600&fit=crop&q=80'
 ], 'StudyRoom', 2),

-- ══ GIƯỜNG (22) ═══════════════════════════════════════════════════════════════
('GIU-OAK-01','Giường Gỗ Sồi King Size 180×200', 14500000, 10150000, 10,
 'Gỗ sồi Mỹ nguyên khối king size 180×200cm, đầu giường nan ngang. Chắc chắn và sang trọng.',
 ARRAY[
   'https://images.unsplash.com/photo-1540518614846-7eded433c457?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1505693416388-ac5ce068fe85?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1616594039964-ae9021a400a0?w=600&h=600&fit=crop&q=80'
 ], 'OakMaster', 3),

('GIU-OAK-02','Giường Gỗ Sồi Queen Size 160×200', 11800000, 8260000, 12,
 'Queen 160×200cm gỗ sồi, đầu giường bo cong. Độ bền cao, dùng được nhiều thế hệ.',
 ARRAY[
   'https://images.unsplash.com/photo-1616627561950-9f746e330187?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1631049307264-da0ec9d70304?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1540518614846-7eded433c457?w=600&h=600&fit=crop&q=80'
 ], 'OakMaster', 3),

('GIU-OAK-03','Giường Gỗ Sồi Đơn 120×200', 7500000, 5250000, 15,
 'Gỗ sồi 120×200cm cho phòng ngủ đơn. Thiết kế gọn, chịu lực tốt.',
 ARRAY[
   'https://images.unsplash.com/photo-1617325247661-675ab4b64ae2?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1618220179428-22790b461013?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1505693416388-ac5ce068fe85?w=600&h=600&fit=crop&q=80'
 ], 'OakMaster', 3),

('GIU-UPHL-01','Giường Đầu Bọc Nhung Xám King', 16500000, 11550000, 8,
 'King 180×200cm đầu bọc nhung xám, khung gỗ sồi, chân kim loại. Sang trọng và ấm cúng.',
 ARRAY[
   'https://images.unsplash.com/photo-1588046130717-0eb0c9a3ba15?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1540518614846-7eded433c457?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1616627561950-9f746e330187?w=600&h=600&fit=crop&q=80'
 ], 'VelvetBed', 3),

('GIU-UPHL-02','Giường Đầu Bọc Nhung Xanh Queen', 13200000, 9240000, 10,
 'Queen size đầu bọc nhung xanh lam đậm. Điểm nhấn màu sắc độc đáo cho phòng ngủ.',
 ARRAY[
   'https://images.unsplash.com/photo-1560185893-a55cbc8c57e8?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1631049307264-da0ec9d70304?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1617325247661-675ab4b64ae2?w=600&h=600&fit=crop&q=80'
 ], 'VelvetBed', 3),

('GIU-UPHL-03','Giường Đầu Bọc Da Trắng', 15800000, 11060000, 8,
 'Đầu bọc da trắng sáng bóng, khung gỗ thấp. Phong cách minimal cao cấp.',
 ARRAY[
   'https://images.unsplash.com/photo-1618220179428-22790b461013?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1588046130717-0eb0c9a3ba15?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1616594039964-ae9021a400a0?w=600&h=600&fit=crop&q=80'
 ], 'LeatherBed', 3),

('GIU-PLAT-01','Giường Nền Thấp Kiểu Nhật 180×200', 9800000, 6860000, 14,
 'Platform thấp 18cm kiểu Nhật, gỗ walnut, không cần đầu giường. Zen và tối giản.',
 ARRAY[
   'https://images.unsplash.com/photo-1505693416388-ac5ce068fe85?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1560185893-a55cbc8c57e8?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1540518614846-7eded433c457?w=600&h=600&fit=crop&q=80'
 ], 'ZenBed', 3),

('GIU-PLAT-02','Giường Nền Thấp Gỗ Walnut 160×200', 11200000, 7840000, 12,
 'Queen size gỗ walnut, đầu giường tấm liền rộng 60cm. Phong cách Nhật–Bắc Âu.',
 ARRAY[
   'https://images.unsplash.com/photo-1616627561950-9f746e330187?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1631049307264-da0ec9d70304?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1617325247661-675ab4b64ae2?w=600&h=600&fit=crop&q=80'
 ], 'ZenBed', 3),

('GIU-PLAT-03','Giường Nền Thấp Tối Giản 120×200', 7200000, 5040000, 18,
 'Đơn nền thấp phong cách Nhật, gỗ thông, cao 15cm. Gọn nhẹ cho phòng nhỏ.',
 ARRAY[
   'https://images.unsplash.com/photo-1618220179428-22790b461013?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1588046130717-0eb0c9a3ba15?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1505693416388-ac5ce068fe85?w=600&h=600&fit=crop&q=80'
 ], 'ZenBed', 3),

('GIU-STOR-01','Giường Có Hộc Kéo King 180×200', 18500000, 12950000, 8,
 'King 180×200cm với 4 ngăn kéo hai bên, sàn nâng được. Tối ưu không gian lưu trữ.',
 ARRAY[
   'https://images.unsplash.com/photo-1616594039964-ae9021a400a0?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1560185893-a55cbc8c57e8?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1540518614846-7eded433c457?w=600&h=600&fit=crop&q=80'
 ], 'StorageBed', 3),

('GIU-STOR-02','Giường Có Hộc Kéo Queen 160×200', 15200000, 10640000, 10,
 'Queen 2 ngăn kéo ở chân giường, gỗ MDF phủ sồi, đầu giường bo cong.',
 ARRAY[
   'https://images.unsplash.com/photo-1631049307264-da0ec9d70304?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1616627561950-9f746e330187?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1618220179428-22790b461013?w=600&h=600&fit=crop&q=80'
 ], 'StorageBed', 3),

('GIU-BUNK-01','Giường Tầng Gỗ Thông Trẻ Em', 6500000, 4550000, 15,
 '90×200cm mỗi tầng, thang leo an toàn, thanh bảo vệ chống ngã. Phù hợp trẻ 6+ tuổi.',
 ARRAY[
   'https://images.unsplash.com/photo-1617325247661-675ab4b64ae2?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1588046130717-0eb0c9a3ba15?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1505693416388-ac5ce068fe85?w=600&h=600&fit=crop&q=80'
 ], 'KidsRoom', 3),

('GIU-BUNK-02','Giường Tầng Tích Hợp Bàn Học', 8900000, 6230000, 10,
 'Tầng trên 90×200cm, gầm dưới là bàn học kín. Tiết kiệm tối đa diện tích phòng trẻ.',
 ARRAY[
   'https://images.unsplash.com/photo-1560185893-a55cbc8c57e8?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1540518614846-7eded433c457?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1616594039964-ae9021a400a0?w=600&h=600&fit=crop&q=80'
 ], 'KidsRoom', 3),

('GIU-CANO-01','Giường Canopy Lãng Mạn King 180×200', 19800000, 13860000, 6,
 '4 trụ canopy king size, khung gỗ teak, rèm lụa trắng. Không gian phòng ngủ lãng mạn.',
 ARRAY[
   'https://images.unsplash.com/photo-1616627561950-9f746e330187?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1631049307264-da0ec9d70304?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1588046130717-0eb0c9a3ba15?w=600&h=600&fit=crop&q=80'
 ], 'DreamBed', 3),

('GIU-CANO-02','Giường Canopy Tối Giản Đen', 16900000, 11830000, 8,
 'Khung thép sơn tĩnh điện đen, hiện đại. Queen 160×200cm với màn che tùy chọn.',
 ARRAY[
   'https://images.unsplash.com/photo-1617325247661-675ab4b64ae2?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1618220179428-22790b461013?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1616627561950-9f746e330187?w=600&h=600&fit=crop&q=80'
 ], 'DreamBed', 3),

('GIU-IRON-01','Giường Sắt Kiểu Cổ Điển', 5800000, 4060000, 20,
 'Sắt rèn tay họa tiết cuộn tròn cổ điển, sơn trắng kem. Queen 160×200cm.',
 ARRAY[
   'https://images.unsplash.com/photo-1540518614846-7eded433c457?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1505693416388-ac5ce068fe85?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1631049307264-da0ec9d70304?w=600&h=600&fit=crop&q=80'
 ], 'IronBed', 3),

('GIU-IRON-02','Giường Sắt Hiện Đại Chân Cao', 7200000, 5040000, 15,
 'Chân cao 45cm, phong cách công nghiệp, sơn đen mờ. Queen 160×200cm.',
 ARRAY[
   'https://images.unsplash.com/photo-1616594039964-ae9021a400a0?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1560185893-a55cbc8c57e8?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1588046130717-0eb0c9a3ba15?w=600&h=600&fit=crop&q=80'
 ], 'IronBed', 3),

('GIU-PINE-01','Giường Gỗ Thông Tự Nhiên Queen', 9200000, 6440000, 14,
 'Gỗ thông Bắc Mỹ ít xử lý hóa chất. Queen 160×200cm, thân thiện môi trường.',
 ARRAY[
   'https://images.unsplash.com/photo-1540518614846-7eded433c457?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1616627561950-9f746e330187?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1617325247661-675ab4b64ae2?w=600&h=600&fit=crop&q=80'
 ], 'PineBed', 3),

('GIU-PINE-02','Giường Gỗ Thông Đầu Giường Cao', 8500000, 5950000, 16,
 'Đầu giường cao 120cm, 3 khe đọc sách trang trí. Queen 160×200cm ấm cúng.',
 ARRAY[
   'https://images.unsplash.com/photo-1631049307264-da0ec9d70304?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1618220179428-22790b461013?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1540518614846-7eded433c457?w=600&h=600&fit=crop&q=80'
 ], 'PineBed', 3),

('GIU-KIDS-01','Giường Trẻ Em Hình Xe Ô Tô', 5200000, 3640000, 18,
 'Mô phỏng xe ô tô màu đỏ/xanh, gỗ MDF an toàn. Kích thước 90×190cm.',
 ARRAY[
   'https://images.unsplash.com/photo-1588046130717-0eb0c9a3ba15?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1505693416388-ac5ce068fe85?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1616594039964-ae9021a400a0?w=600&h=600&fit=crop&q=80'
 ], 'KidsRoom', 3),

('GIU-KIDS-02','Giường Trẻ Em Có Cầu Trượt', 7800000, 5460000, 12,
 'Tầng trên ngủ, tầng dưới có cầu trượt và lều chơi. Gỗ thông an toàn 90×190cm.',
 ARRAY[
   'https://images.unsplash.com/photo-1560185893-a55cbc8c57e8?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1617325247661-675ab4b64ae2?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1631049307264-da0ec9d70304?w=600&h=600&fit=crop&q=80'
 ], 'KidsRoom', 3),

('GIU-WALL-01','Giường Gắn Tường Gấp Gọn Murphy', 12500000, 8750000, 8,
 'Gấp lên thành tủ trang trí khi không dùng. Queen 160×200cm, phù hợp studio nhỏ.',
 ARRAY[
   'https://images.unsplash.com/photo-1618220179428-22790b461013?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1540518614846-7eded433c457?w=600&h=600&fit=crop&q=80',
   'https://images.unsplash.com/photo-1616627561950-9f746e330187?w=600&h=600&fit=crop&q=80'
 ], 'SpaceBed', 3);


-- ─── 4. ĐƠN HÀNG DEMO ────────────────────────────────────────────────────────
DO $$
DECLARE
    acc_id   INTEGER;
    cust_id  INTEGER;

    -- Ghế
    p_dina01 INTEGER; p_dina02 INTEGER; p_dina03 INTEGER;
    p_loun01 INTEGER; p_loun02 INTEGER;
    p_bar01  INTEGER; p_rock01 INTEGER;
    p_acce01 INTEGER; p_otto01 INTEGER;
    p_scan01 INTEGER; p_benc01 INTEGER;
    -- Bàn
    p_dine01 INTEGER; p_dine02 INTEGER;
    p_cofe01 INTEGER; p_cofe02 INTEGER; p_cofe04 INTEGER;
    p_work01 INTEGER; p_work03 INTEGER;
    p_nite01 INTEGER; p_cons01 INTEGER;
    p_tele01 INTEGER; p_dres01 INTEGER;
    -- Giường
    p_oak01  INTEGER; p_oak02  INTEGER;
    p_uphl01 INTEGER; p_uphl02 INTEGER;
    p_plat01 INTEGER; p_plat03 INTEGER;
    p_stor01 INTEGER; p_bunk01 INTEGER;
    p_cano01 INTEGER; p_iron01 INTEGER; p_pine01 INTEGER;

    prod_arr  INTEGER[];
    price_arr INTEGER[];
    oid       INTEGER;
    qty       INTEGER;
    final     INTEGER;
    i         INTEGER;
    d         INTEGER;
    idx       INTEGER;
    day_start TIMESTAMP;
    cur_days  INTEGER;
BEGIN
    SELECT account_id INTO acc_id FROM account ORDER BY account_id LIMIT 1;
    IF acc_id IS NULL THEN
        RAISE EXCEPTION 'Không tìm thấy account — hãy chạy 04_furniture_seed.sql trước.';
    END IF;

    INSERT INTO customer (name, phone, email, address)
    VALUES ('Khách Demo Báo Cáo', '0900000098', 'reportexpand@local.test', 'Demo — không giao thật')
    RETURNING customer_id INTO cust_id;

    -- Lookup theo SKU để đảm bảo product_id luôn khớp
    SELECT product_id INTO p_dina01 FROM product WHERE sku = 'GHE-DINA-01';
    SELECT product_id INTO p_dina02 FROM product WHERE sku = 'GHE-DINA-02';
    SELECT product_id INTO p_dina03 FROM product WHERE sku = 'GHE-DINA-03';
    SELECT product_id INTO p_loun01 FROM product WHERE sku = 'GHE-LOUN-01';
    SELECT product_id INTO p_loun02 FROM product WHERE sku = 'GHE-LOUN-02';
    SELECT product_id INTO p_bar01  FROM product WHERE sku = 'GHE-BAR-01';
    SELECT product_id INTO p_rock01 FROM product WHERE sku = 'GHE-ROCK-01';
    SELECT product_id INTO p_acce01 FROM product WHERE sku = 'GHE-ACCE-01';
    SELECT product_id INTO p_otto01 FROM product WHERE sku = 'GHE-OTTO-01';
    SELECT product_id INTO p_scan01 FROM product WHERE sku = 'GHE-SCAN-01';
    SELECT product_id INTO p_benc01 FROM product WHERE sku = 'GHE-BENC-01';
    SELECT product_id INTO p_dine01 FROM product WHERE sku = 'BAN-DINE-01';
    SELECT product_id INTO p_dine02 FROM product WHERE sku = 'BAN-DINE-02';
    SELECT product_id INTO p_cofe01 FROM product WHERE sku = 'BAN-COFE-01';
    SELECT product_id INTO p_cofe02 FROM product WHERE sku = 'BAN-COFE-02';
    SELECT product_id INTO p_cofe04 FROM product WHERE sku = 'BAN-COFE-04';
    SELECT product_id INTO p_work01 FROM product WHERE sku = 'BAN-WORK-01';
    SELECT product_id INTO p_work03 FROM product WHERE sku = 'BAN-WORK-03';
    SELECT product_id INTO p_nite01 FROM product WHERE sku = 'BAN-NITE-01';
    SELECT product_id INTO p_cons01 FROM product WHERE sku = 'BAN-CONS-01';
    SELECT product_id INTO p_tele01 FROM product WHERE sku = 'BAN-TELE-01';
    SELECT product_id INTO p_dres01 FROM product WHERE sku = 'BAN-DRES-01';
    SELECT product_id INTO p_oak01  FROM product WHERE sku = 'GIU-OAK-01';
    SELECT product_id INTO p_oak02  FROM product WHERE sku = 'GIU-OAK-02';
    SELECT product_id INTO p_uphl01 FROM product WHERE sku = 'GIU-UPHL-01';
    SELECT product_id INTO p_uphl02 FROM product WHERE sku = 'GIU-UPHL-02';
    SELECT product_id INTO p_plat01 FROM product WHERE sku = 'GIU-PLAT-01';
    SELECT product_id INTO p_plat03 FROM product WHERE sku = 'GIU-PLAT-03';
    SELECT product_id INTO p_stor01 FROM product WHERE sku = 'GIU-STOR-01';
    SELECT product_id INTO p_bunk01 FROM product WHERE sku = 'GIU-BUNK-01';
    SELECT product_id INTO p_cano01 FROM product WHERE sku = 'GIU-CANO-01';
    SELECT product_id INTO p_iron01 FROM product WHERE sku = 'GIU-IRON-01';
    SELECT product_id INTO p_pine01 FROM product WHERE sku = 'GIU-PINE-01';

    -- ═══ A. ĐƠN LỊCH SỬ — 50 đơn trải 70 ngày gần đây ════════════════════════
    prod_arr  := ARRAY[p_dina01, p_scan01, p_loun01, p_cofe01, p_cofe02,
                        p_work01, p_nite01, p_dine01, p_oak01,  p_uphl01,
                        p_plat01, p_iron01, p_bar01,  p_acce01, p_pine01,
                        p_dina02, p_loun02, p_cofe04, p_dine02, p_oak02,
                        p_uphl02, p_plat03, p_stor01, p_bunk01, p_cano01];
    price_arr := ARRAY[1290000,  1850000,  4200000,  4500000,  3800000,
                        5200000,  1650000,  8500000,  14500000, 16500000,
                        9800000,  5800000,  2350000,  3200000,  9200000,
                        1650000,  5800000,  2800000,  12500000, 11800000,
                        13200000, 7200000,  18500000, 6500000,  19800000];

    FOR i IN 1..50 LOOP
        idx   := ((i - 1) % 25) + 1;
        qty   := ((i - 1) % 3) + 1;
        final := qty * price_arr[idx];

        INSERT INTO orders (created_time, final_price, status, customer_id, account_id,
            shipping_address, recipient_name, recipient_phone, recipient_email, is_deleted)
        VALUES (
            timezone('utc', now()) - make_interval(days => ((i - 1) * 70 / 50) + 1)
                                   - make_interval(hours => (i % 12) + 6),
            final, 'Delivered', cust_id, acc_id,
            'FURNITURE_EXPAND_SEED', 'Khách Demo Báo Cáo', '0900000098',
            'reportexpand@local.test', false
        ) RETURNING order_id INTO oid;

        INSERT INTO order_item (order_id, product_id, quantity, unit_sale_price, total_price)
        VALUES (oid, prod_arr[idx], qty, price_arr[idx], final);
    END LOOP;

    -- ═══ B. ĐƠN THÁNG HIỆN TẠI — 2 đơn/ngày cho biểu đồ doanh thu ════════════
    cur_days := EXTRACT(DAY FROM NOW())::INT;

    prod_arr  := ARRAY[p_dina01, p_dina02, p_scan01, p_bar01,  p_loun01,
                        p_cofe01, p_cofe02, p_work01, p_nite01, p_dine01,
                        p_oak01,  p_oak02,  p_uphl01, p_plat01, p_plat03,
                        p_iron01, p_pine01, p_bunk01, p_cano01, p_otto01];
    price_arr := ARRAY[1290000,  1650000,  1850000,  2350000,  4200000,
                        4500000,  3800000,  5200000,  1650000,  8500000,
                        14500000, 11800000, 16500000, 9800000,  7200000,
                        5800000,  9200000,  6500000,  19800000, 1800000];

    FOR d IN 0..(cur_days - 1) LOOP
        day_start := date_trunc('month', NOW()) + make_interval(days => d);

        -- Đơn buổi sáng
        idx   := (d % 20) + 1;
        qty   := (d % 3) + 1;
        final := qty * price_arr[idx];
        INSERT INTO orders (created_time, final_price, status, customer_id, account_id,
            shipping_address, recipient_name, recipient_phone, recipient_email, is_deleted)
        VALUES (
            day_start + make_interval(hours => 9, mins => (d * 17 % 30)),
            final, 'Delivered', cust_id, acc_id,
            'FURNITURE_EXPAND_SEED', 'Khách Demo Báo Cáo', '0900000098',
            'reportexpand@local.test', false
        ) RETURNING order_id INTO oid;
        INSERT INTO order_item (order_id, product_id, quantity, unit_sale_price, total_price)
        VALUES (oid, prod_arr[idx], qty, price_arr[idx], final);

        -- Đơn buổi chiều (sản phẩm khác)
        idx   := ((d + 10) % 20) + 1;
        qty   := (d % 2) + 1;
        final := qty * price_arr[idx];
        INSERT INTO orders (created_time, final_price, status, customer_id, account_id,
            shipping_address, recipient_name, recipient_phone, recipient_email, is_deleted)
        VALUES (
            day_start + make_interval(hours => 14, mins => (d * 23 % 45)),
            final, 'Delivered', cust_id, acc_id,
            'FURNITURE_EXPAND_SEED', 'Khách Demo Báo Cáo', '0900000098',
            'reportexpand@local.test', false
        ) RETURNING order_id INTO oid;
        INSERT INTO order_item (order_id, product_id, quantity, unit_sale_price, total_price)
        VALUES (oid, prod_arr[idx], qty, price_arr[idx], final);
    END LOOP;

END $$;

COMMIT;
