-- Cập nhật giá vốn cho các sản phẩm mẫu để test báo cáo lợi nhuận
UPDATE PRODUCT SET cost_price = 22000000 WHERE sku = 'IP15PM';  -- iPhone: 29tr -> Vốn 22tr
UPDATE PRODUCT SET cost_price = 20000000 WHERE sku = 'SS24U';   -- Samsung: 27tr -> Vốn 20tr
UPDATE PRODUCT SET cost_price = 28000000 WHERE sku = 'MBP14';   -- Macbook: 35tr -> Vốn 28tr
UPDATE PRODUCT SET cost_price = 4000000 WHERE sku = 'AP2';      -- AirPods: 5.5tr -> Vốn 4tr

-- Với các sản phẩm khác nếu có, set mặc định bằng 75% giá bán
UPDATE PRODUCT SET cost_price = ROUND(price * 0.75) WHERE cost_price = 0;
