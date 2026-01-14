-- =====================================================
-- Food Delivery Database Seed Data
-- PostgreSQL Script
-- Run this AFTER create_tables.sql
-- =====================================================

-- =====================================================
-- INSERT USERS
-- Password: 123456 (BCrypt hash)
-- =====================================================

INSERT INTO "Users" ("Id", "PhoneNumber", "Email", "PasswordHash", "FullName", "AvatarUrl", "Role", "IsActive")
VALUES 
    ('a1111111-1111-1111-1111-111111111111', '0901234567', 'customer@example.com', 
     '$2a$11$rBNvKK.yKEjgGqgwRJ7XPu3JYF5LO.G8Jn0hCq/HBwMZsVqVYqGXy', 
     'Nguyễn Văn Khách', 'https://randomuser.me/api/portraits/men/1.jpg', 1, TRUE),
    
    ('b2222222-2222-2222-2222-222222222222', '0909876543', 'merchant@example.com', 
     '$2a$11$rBNvKK.yKEjgGqgwRJ7XPu3JYF5LO.G8Jn0hCq/HBwMZsVqVYqGXy', 
     'Trần Thị Chủ Quán', 'https://randomuser.me/api/portraits/women/2.jpg', 2, TRUE),
    
    ('c3333333-3333-3333-3333-333333333333', '0905555555', 'driver@example.com', 
     '$2a$11$rBNvKK.yKEjgGqgwRJ7XPu3JYF5LO.G8Jn0hCq/HBwMZsVqVYqGXy', 
     'Lê Văn Tài Xế', 'https://randomuser.me/api/portraits/men/3.jpg', 3, TRUE);

-- =====================================================
-- INSERT CUSTOMER PROFILE
-- =====================================================

INSERT INTO "Customers" ("Id", "UserId")
VALUES ('d4444444-4444-4444-4444-444444444444', 'a1111111-1111-1111-1111-111111111111');

-- =====================================================
-- INSERT CUSTOMER ADDRESSES
-- =====================================================

INSERT INTO "Addresses" ("CustomerId", "Label", "FullAddress", "Latitude", "Longitude", "IsDefault")
VALUES 
    ('d4444444-4444-4444-4444-444444444444', 'Nhà', '123 Nguyễn Huệ, Phường Bến Nghé, Quận 1, TP. Hồ Chí Minh', 10.7769, 106.7009, TRUE),
    ('d4444444-4444-4444-4444-444444444444', 'Công ty', '456 Lê Lợi, Phường Bến Thành, Quận 1, TP. Hồ Chí Minh', 10.7731, 106.6989, FALSE);

-- =====================================================
-- INSERT MERCHANT PROFILE
-- =====================================================

INSERT INTO "Merchants" ("Id", "UserId", "BusinessName")
VALUES ('e5555555-5555-5555-5555-555555555555', 'b2222222-2222-2222-2222-222222222222', 'Golden Dragon Group');

-- =====================================================
-- INSERT DRIVER PROFILE
-- =====================================================

INSERT INTO "Drivers" ("Id", "UserId", "VehicleType", "VehiclePlate", "VehicleBrand", "Status", "WalletBalance", "CurrentLatitude", "CurrentLongitude", "Rating", "TotalDeliveries")
VALUES ('f6666666-6666-6666-6666-666666666666', 'c3333333-3333-3333-3333-333333333333', 'Xe máy', '59C1-12345', 'Honda PCX', 2, 2450000, 10.7800, 106.7000, 4.9, 150);

-- =====================================================
-- INSERT FOOD CATEGORIES
-- =====================================================

INSERT INTO "FoodCategories" ("Name", "NameSecondary", "IconUrl", "BackgroundColor", "DisplayOrder")
VALUES 
    ('Cơm', 'Rice', 'https://cdn-icons-png.flaticon.com/512/1046/1046857.png', 'orange-100', 1),
    ('Phở/Bún', 'Noodles', 'https://cdn-icons-png.flaticon.com/512/1046/1046785.png', 'red-100', 2),
    ('Đồ uống', 'Drinks', 'https://cdn-icons-png.flaticon.com/512/2405/2405479.png', 'blue-100', 3),
    ('Ăn nhanh', 'Fast Food', 'https://cdn-icons-png.flaticon.com/512/3075/3075977.png', 'yellow-100', 4),
    ('Trà sữa', 'Bubble Tea', 'https://cdn-icons-png.flaticon.com/512/2684/2684574.png', 'purple-100', 5),
    ('Bánh mì', 'Bread', 'https://cdn-icons-png.flaticon.com/512/3014/3014990.png', 'amber-100', 6);

-- =====================================================
-- INSERT RESTAURANTS
-- =====================================================

INSERT INTO "Restaurants" ("Id", "MerchantId", "Name", "NameSecondary", "Description", "ImageUrl", "CoverImageUrl", "Address", "Latitude", "Longitude", "PhoneNumber", "Category", "IsOpen", "OpenTime", "CloseTime", "EstimatedDeliveryMinutes", "DeliveryFee", "MinOrderAmount", "Rating", "TotalReviews", "HasPromotion")
VALUES 
    ('11111111-aaaa-1111-aaaa-111111111111', 'e5555555-5555-5555-5555-555555555555', 
     'Golden Dragon Bistro', '金龍小館', 
     'Nhà hàng Trung Hoa với các món dim sum và mì truyền thống',
     'https://images.unsplash.com/photo-1555396273-367ea4eb4db5?w=200',
     'https://images.unsplash.com/photo-1517248135467-4c7edcad34c4?w=800',
     '88 Đồng Khởi, Quận 1, TP.HCM', 10.7756, 106.7019, '028 1234 5678',
     'Món Trung Hoa', TRUE, '07:00:00', '22:00:00', 25, 20000, 50000, 4.8, 520, TRUE),
    
    ('22222222-bbbb-2222-bbbb-222222222222', 'e5555555-5555-5555-5555-555555555555', 
     'Bánh Mì 25 - Phố Cổ', NULL, 
     'Bánh mì truyền thống Việt Nam với nhiều loại nhân đa dạng',
     'https://images.unsplash.com/photo-1600688640154-9619e002df30?w=200',
     'https://images.unsplash.com/photo-1600688640154-9619e002df30?w=800',
     '25 Hàng Cá, Hoàn Kiếm, Hà Nội', 10.7790, 106.7030, NULL,
     'Món Việt', TRUE, NULL, NULL, 15, 15000, 0, 4.8, 200, TRUE),
    
    ('33333333-cccc-3333-cccc-333333333333', 'e5555555-5555-5555-5555-555555555555', 
     'Pizza 4Ps - Bến Thành', NULL, 
     'Pizza thủ công với nguyên liệu tươi nhập khẩu từ Nhật',
     'https://images.unsplash.com/photo-1565299624946-b28f40a0ae38?w=200',
     'https://images.unsplash.com/photo-1565299624946-b28f40a0ae38?w=800',
     '08 Thủ Khoa Huân, Quận 1, TP.HCM', 10.7720, 106.6980, NULL,
     'Món Ý', TRUE, NULL, NULL, 35, 0, 100000, 4.9, 1200, FALSE),
    
    ('44444444-dddd-4444-dddd-444444444444', 'e5555555-5555-5555-5555-555555555555', 
     'Gà Rán Texas', NULL, 
     'Gà rán giòn rụm phong cách Texas',
     'https://images.unsplash.com/photo-1626082927389-6cd097cdc6ec?w=200',
     'https://images.unsplash.com/photo-1626082927389-6cd097cdc6ec?w=800',
     '123 Nguyễn Trãi, Quận 5, TP.HCM', 10.7600, 106.6800, NULL,
     'Đồ ăn nhanh', TRUE, NULL, NULL, 20, 25000, 0, 4.5, 500, FALSE);

-- Update IsNew for last restaurant
UPDATE "Restaurants" SET "IsNew" = TRUE WHERE "Id" = '44444444-dddd-4444-dddd-444444444444';

-- =====================================================
-- INSERT MENU CATEGORIES
-- =====================================================

INSERT INTO "MenuCategories" ("Id", "RestaurantId", "Name", "NameSecondary", "IconName", "DisplayOrder")
VALUES 
    ('aa111111-1111-1111-1111-111111111111', '11111111-aaaa-1111-aaaa-111111111111', 'Món đặc trưng', '人氣', 'local_fire_department', 1),
    ('bb222222-2222-2222-2222-222222222222', '11111111-aaaa-1111-aaaa-111111111111', 'Điểm sấm', '點心', NULL, 2),
    ('cc333333-3333-3333-3333-333333333333', '11111111-aaaa-1111-aaaa-111111111111', 'Mì & Phở', '麵類', NULL, 3),
    ('dd444444-4444-4444-4444-444444444444', '11111111-aaaa-1111-aaaa-111111111111', 'Đồ uống', '飲料', NULL, 4);

-- =====================================================
-- INSERT MENU ITEMS
-- =====================================================

INSERT INTO "MenuItems" ("Id", "MenuCategoryId", "Name", "NameSecondary", "Description", "ImageUrl", "Price", "IsPopular", "DisplayOrder")
VALUES 
    -- Món đặc trưng
    ('item1111-1111-1111-1111-111111111111', 'aa111111-1111-1111-1111-111111111111', 
     'Há Cảo Tôm', '鮮蝦餃', 
     'Bánh bao vỏ mỏng dai nhân tôm tươi nguyên con và măng tre giòn ngọt.',
     'https://images.unsplash.com/photo-1563245372-f21724e3856d?w=400', 65000, TRUE, 1),
    
    ('item2222-2222-2222-2222-222222222222', 'aa111111-1111-1111-1111-111111111111', 
     'Thịt Heo Quay Giòn', '脆皮燒肉', 
     'Thịt ba chỉ quay chậm với lớp da giòn rụm và thịt mềm, ăn kèm sốt mù tạt vàng.',
     'https://images.unsplash.com/photo-1544025162-d76978fc1a88?w=400', 125000, TRUE, 2),
    
    -- Điểm sấm
    ('item3333-3333-3333-3333-333333333333', 'bb222222-2222-2222-2222-222222222222', 
     'Xíu Mại', '燒賣', 
     'Bánh bao truyền thống nhân thịt heo và tôm, bên trên phủ trứng cua.',
     'https://images.unsplash.com/photo-1496116218417-1a781b1c416c?w=400', 55000, FALSE, 1),
    
    ('item4444-4444-4444-4444-444444444444', 'bb222222-2222-2222-2222-222222222222', 
     'Bánh Bao Xá Xíu', '叉燒包', 
     'Bánh bao hấp mềm xốp nhân thịt xá xíu đậm đà, thơm ngon.',
     'https://images.unsplash.com/photo-1529692236671-f1f6cf9683ba?w=400', 50000, FALSE, 2),
    
    -- Mì & Phở
    ('item5555-5555-5555-5555-555555555555', 'cc333333-3333-3333-3333-333333333333', 
     'Mì Hoành Thánh', '雲吞麵', 
     'Mì trứng tươi chan nước dùng đậm đà, ăn kèm hoành thánh tôm gói tay và cải thìa.',
     'https://images.unsplash.com/photo-1569718212165-3a8278d5f624?w=400', 110000, FALSE, 1),
    
    -- Đồ uống
    ('item6666-6666-6666-6666-666666666666', 'dd444444-4444-4444-4444-444444444444', 
     'Trà Đào Cam Sả', NULL, 
     'Trà ướp hương đào, cam tươi và sả thơm mát.',
     'https://images.unsplash.com/photo-1556679343-c7306c1976bc?w=400', 45000, FALSE, 1),
    
    ('item7777-7777-7777-7777-777777777777', 'dd444444-4444-4444-4444-444444444444', 
     'Cà Phê Sữa Đá', NULL, 
     'Cà phê phin đậm đà pha với sữa đặc, thêm đá giòn.',
     'https://images.unsplash.com/photo-1461023058943-07fcbe16d735?w=400', 35000, FALSE, 2);

-- =====================================================
-- INSERT VOUCHERS
-- =====================================================

INSERT INTO "Vouchers" ("Id", "Code", "Name", "Description", "Type", "DiscountValue", "MaxDiscountAmount", "MinOrderAmount", "StartDate", "EndDate", "Category")
VALUES 
    ('v1111111-1111-1111-1111-111111111111', 'FREESHIP', 'Miễn phí vận chuyển', 'Giảm tối đa 15k cho đơn từ 0đ', 3, 15000, 15000, 0, NOW() - INTERVAL '30 days', NOW() + INTERVAL '30 days', 'Vận chuyển'),
    ('v2222222-2222-2222-2222-222222222222', 'GIAM50K', 'Giảm 50k', 'Đơn tối thiểu 200k • Quán Đặc Biệt', 2, 50000, 50000, 200000, NOW() - INTERVAL '30 days', NOW() + INTERVAL '15 days', 'Đồ ăn'),
    ('v3333333-3333-3333-3333-333333333333', 'VIMOMO20', 'Giảm 20k thanh toán ví', 'Áp dụng cho khách hàng mới liên kết', 2, 20000, 20000, 50000, NOW() - INTERVAL '30 days', NOW() + INTERVAL '365 days', 'Đối tác'),
    ('v4444444-4444-4444-4444-444444444444', 'GIAM20', 'Giảm 20%', 'Giảm 20% tối đa 30k cho đơn từ 100k', 1, 20, 30000, 100000, NOW() - INTERVAL '10 days', NOW() + INTERVAL '20 days', 'Đồ ăn');

-- =====================================================
-- INSERT PROMOTIONS
-- =====================================================

INSERT INTO "Promotions" ("Title", "Subtitle", "ImageUrl", "Badge", "BadgeColor", "StartDate", "EndDate", "DisplayOrder")
VALUES 
    ('Combo Trưa 59k', 'Đặt ngay miễn phí giao hàng', 'https://images.unsplash.com/photo-1504674900247-0877df9cc836?w=800', 'Freeship', 'primary', NOW() - INTERVAL '7 days', NOW() + INTERVAL '30 days', 1),
    ('Tiệc Pizza', 'HSD đến 30/10', 'https://images.unsplash.com/photo-1565299624946-b28f40a0ae38?w=800', 'GIẢM 50%', 'orange-500', NOW() - INTERVAL '7 days', NOW() + INTERVAL '60 days', 2),
    ('Trà Sữa Sale', 'Mua 1 tặng 1', 'https://images.unsplash.com/photo-1558857563-b371033873b8?w=800', 'HOT', 'red-500', NOW() - INTERVAL '3 days', NOW() + INTERVAL '14 days', 3);

-- =====================================================
-- INSERT SAMPLE ORDER
-- =====================================================

INSERT INTO "Orders" ("Id", "OrderNumber", "CustomerId", "RestaurantId", "DriverId", "DeliveryAddress", "DeliveryLatitude", "DeliveryLongitude", "Subtotal", "DeliveryFee", "DiscountAmount", "TotalAmount", "Status", "PaymentMethod", "PaymentStatus", "EstimatedDeliveryMinutes", "ConfirmedAt", "PreparedAt", "PickedUpAt")
VALUES 
    ('o1111111-1111-1111-1111-111111111111', '#260114001', 'd4444444-4444-4444-4444-444444444444', '11111111-aaaa-1111-aaaa-111111111111', 'f6666666-6666-6666-6666-666666666666', 
     '123 Nguyễn Huệ, Phường Bến Nghé, Quận 1', 10.7769, 106.7009, 
     115000, 20000, 0, 135000, 6, 1, 1, 15, 
     NOW() - INTERVAL '20 minutes', NOW() - INTERVAL '10 minutes', NOW() - INTERVAL '5 minutes');

-- Insert Order Items
INSERT INTO "OrderItems" ("OrderId", "MenuItemId", "ItemName", "UnitPrice", "Quantity", "TotalPrice")
VALUES 
    ('o1111111-1111-1111-1111-111111111111', 'item1111-1111-1111-1111-111111111111', 'Há Cảo Tôm', 65000, 1, 65000),
    ('o1111111-1111-1111-1111-111111111111', 'item3333-3333-3333-3333-333333333333', 'Xíu Mại', 50000, 1, 50000);

-- Insert Order Tracking
INSERT INTO "OrderTrackings" ("OrderId", "Status", "Description", "Timestamp")
VALUES 
    ('o1111111-1111-1111-1111-111111111111', 1, 'Đơn hàng đã được tạo', NOW() - INTERVAL '25 minutes'),
    ('o1111111-1111-1111-1111-111111111111', 2, 'Nhà hàng đã xác nhận', NOW() - INTERVAL '20 minutes'),
    ('o1111111-1111-1111-1111-111111111111', 3, 'Đang chuẩn bị món', NOW() - INTERVAL '15 minutes'),
    ('o1111111-1111-1111-1111-111111111111', 4, 'Sẵn sàng giao', NOW() - INTERVAL '10 minutes'),
    ('o1111111-1111-1111-1111-111111111111', 5, 'Tài xế đã lấy hàng', NOW() - INTERVAL '5 minutes'),
    ('o1111111-1111-1111-1111-111111111111', 6, 'Đang giao đến bạn', NOW() - INTERVAL '2 minutes');

-- =====================================================
-- INSERT DRIVER EARNINGS
-- =====================================================

INSERT INTO "DriverEarnings" ("DriverId", "Type", "Amount", "Description", "EarnedAt")
VALUES 
    ('f6666666-6666-6666-6666-666666666666', 1, 35000, 'Đơn hàng #260113001', NOW() - INTERVAL '2 hours'),
    ('f6666666-6666-6666-6666-666666666666', 2, 10000, 'Tip từ khách', NOW() - INTERVAL '2 hours'),
    ('f6666666-6666-6666-6666-666666666666', 3, 100000, 'Tiền thưởng ngày', NOW() - INTERVAL '1 day'),
    ('f6666666-6666-6666-6666-666666666666', 1, 42000, 'Đơn hàng #260113002', NOW() - INTERVAL '5 hours'),
    ('f6666666-6666-6666-6666-666666666666', 1, 28000, 'Đơn hàng #260112003', NOW() - INTERVAL '1 day'),
    ('f6666666-6666-6666-6666-666666666666', 1, 55000, 'Đơn hàng #260112004', NOW() - INTERVAL '2 days');

-- =====================================================
-- SUCCESS MESSAGE
-- =====================================================
SELECT 'Seed data inserted successfully!' AS result;
SELECT COUNT(*) AS total_users FROM "Users";
SELECT COUNT(*) AS total_restaurants FROM "Restaurants";
SELECT COUNT(*) AS total_menu_items FROM "MenuItems";
SELECT COUNT(*) AS total_vouchers FROM "Vouchers";
