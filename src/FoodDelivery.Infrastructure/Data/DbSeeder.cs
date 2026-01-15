using FoodDelivery.Domain.Entities;
using FoodDelivery.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // Only seed if database is empty
        if (await context.Users.AnyAsync())
            return;

        // Create sample users
        var customerUser = new User
        {
            Id = Guid.NewGuid(),
            PhoneNumber = "0901234567",
            Email = "customer@example.com",
            FullName = "Nguyễn Văn Khách",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            Role = UserRole.Customer,
            AvatarUrl = "https://randomuser.me/api/portraits/men/1.jpg"
        };

        var merchantUser = new User
        {
            Id = Guid.NewGuid(),
            PhoneNumber = "0909876543",
            Email = "merchant@example.com",
            FullName = "Trần Thị Chủ Quán",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            Role = UserRole.Merchant,
            AvatarUrl = "https://randomuser.me/api/portraits/women/2.jpg"
        };

        var driverUser = new User
        {
            Id = Guid.NewGuid(),
            PhoneNumber = "0905555555",
            Email = "driver@example.com",
            FullName = "Lê Văn Tài Xế",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            Role = UserRole.Driver,
            AvatarUrl = "https://randomuser.me/api/portraits/men/3.jpg"
        };

        // Admin user for testing Admin screens
        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            PhoneNumber = "0900000000",
            Email = "admin@example.com",
            FullName = "Quản Trị Viên",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            Role = UserRole.Admin,
            AvatarUrl = "https://randomuser.me/api/portraits/men/4.jpg"
        };

        context.Users.AddRange(customerUser, merchantUser, driverUser, adminUser);

        // Create Customer profile
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            UserId = customerUser.Id
        };
        context.Customers.Add(customer);

        // Create Customer addresses
        context.Addresses.AddRange(
            new Address
            {
                CustomerId = customer.Id,
                Label = "Nhà",
                FullAddress = "123 Nguyễn Huệ, Phường Bến Nghé, Quận 1, TP. Hồ Chí Minh",
                Latitude = 10.7769,
                Longitude = 106.7009,
                IsDefault = true
            },
            new Address
            {
                CustomerId = customer.Id,
                Label = "Công ty",
                FullAddress = "456 Lê Lợi, Phường Bến Thành, Quận 1, TP. Hồ Chí Minh",
                Latitude = 10.7731,
                Longitude = 106.6989,
                IsDefault = false
            }
        );

        // Create Merchant profile
        var merchant = new Merchant
        {
            Id = Guid.NewGuid(),
            UserId = merchantUser.Id,
            BusinessName = "Golden Dragon Group"
        };
        context.Merchants.Add(merchant);

        // Create Driver profile
        var driver = new Driver
        {
            Id = Guid.NewGuid(),
            UserId = driverUser.Id,
            VehicleType = "Xe máy",
            VehicleBrand = "Honda PCX",
            VehiclePlate = "59C1-12345",
            Status = DriverStatus.Online,
            WalletBalance = 2450000,
            Rating = 4.9,
            TotalDeliveries = 150,
            CurrentLatitude = 10.7800,
            CurrentLongitude = 106.7000
        };
        context.Drivers.Add(driver);

        // Create Food Categories
        var categories = new[]
        {
            new FoodCategory { Name = "Cơm", NameSecondary = "Rice", IconUrl = "https://cdn-icons-png.flaticon.com/512/1046/1046857.png", BackgroundColor = "orange-100", DisplayOrder = 1 },
            new FoodCategory { Name = "Phở/Bún", NameSecondary = "Noodles", IconUrl = "https://cdn-icons-png.flaticon.com/512/1046/1046785.png", BackgroundColor = "red-100", DisplayOrder = 2 },
            new FoodCategory { Name = "Đồ uống", NameSecondary = "Drinks", IconUrl = "https://cdn-icons-png.flaticon.com/512/2405/2405479.png", BackgroundColor = "blue-100", DisplayOrder = 3 },
            new FoodCategory { Name = "Ăn nhanh", NameSecondary = "Fast Food", IconUrl = "https://cdn-icons-png.flaticon.com/512/3075/3075977.png", BackgroundColor = "yellow-100", DisplayOrder = 4 }
        };
        context.FoodCategories.AddRange(categories);

        // Create Restaurants
        var restaurant1 = new Restaurant
        {
            Id = Guid.NewGuid(),
            MerchantId = merchant.Id,
            Name = "Golden Dragon Bistro",
            NameSecondary = "金龍小館",
            Description = "Nhà hàng Trung Hoa với các món dim sum và mì truyền thống",
            ImageUrl = "https://images.unsplash.com/photo-1555396273-367ea4eb4db5?w=200",
            CoverImageUrl = "https://images.unsplash.com/photo-1517248135467-4c7edcad34c4?w=800",
            Address = "88 Đồng Khởi, Quận 1, TP.HCM",
            Latitude = 10.7756,
            Longitude = 106.7019,
            PhoneNumber = "028 1234 5678",
            Category = "Món Trung Hoa",
            IsOpen = true,
            OpenTime = TimeSpan.FromHours(7),
            CloseTime = TimeSpan.FromHours(22),
            EstimatedDeliveryMinutes = 25,
            DeliveryFee = 20000,
            MinOrderAmount = 50000,
            Rating = 4.8,
            TotalReviews = 520,
            HasPromotion = true
        };

        var restaurant2 = new Restaurant
        {
            Id = Guid.NewGuid(),
            MerchantId = merchant.Id,
            Name = "Bánh Mì 25 - Phố Cổ",
            Description = "Bánh mì truyền thống Việt Nam với nhiều loại nhân đa dạng",
            ImageUrl = "https://images.unsplash.com/photo-1600688640154-9619e002df30?w=200",
            CoverImageUrl = "https://images.unsplash.com/photo-1600688640154-9619e002df30?w=800",
            Address = "25 Hàng Cá, Hoàn Kiếm, Hà Nội",
            Latitude = 10.7790,
            Longitude = 106.7030,
            Category = "Món Việt",
            IsOpen = true,
            EstimatedDeliveryMinutes = 15,
            DeliveryFee = 15000,
            Rating = 4.8,
            TotalReviews = 200,
            HasPromotion = true
        };

        var restaurant3 = new Restaurant
        {
            Id = Guid.NewGuid(),
            MerchantId = merchant.Id,
            Name = "Pizza 4P's - Bến Thành",
            Description = "Pizza thủ công với nguyên liệu tươi nhập khẩu từ Nhật",
            ImageUrl = "https://images.unsplash.com/photo-1565299624946-b28f40a0ae38?w=200",
            CoverImageUrl = "https://images.unsplash.com/photo-1565299624946-b28f40a0ae38?w=800",
            Address = "08 Thủ Khoa Huân, Quận 1, TP.HCM",
            Latitude = 10.7720,
            Longitude = 106.6980,
            Category = "Món Ý",
            IsOpen = true,
            EstimatedDeliveryMinutes = 35,
            DeliveryFee = 0,
            Rating = 4.9,
            TotalReviews = 1200
        };

        var restaurant4 = new Restaurant
        {
            Id = Guid.NewGuid(),
            MerchantId = merchant.Id,
            Name = "Gà Rán Texas",
            Description = "Gà rán giòn rụm phong cách Texas",
            ImageUrl = "https://images.unsplash.com/photo-1626082927389-6cd097cdc6ec?w=200",
            CoverImageUrl = "https://images.unsplash.com/photo-1626082927389-6cd097cdc6ec?w=800",
            Address = "123 Nguyễn Trãi, Quận 5, TP.HCM",
            Latitude = 10.7600,
            Longitude = 106.6800,
            Category = "Đồ ăn nhanh",
            IsOpen = true,
            EstimatedDeliveryMinutes = 20,
            DeliveryFee = 25000,
            Rating = 4.5,
            TotalReviews = 500,
            IsNew = true
        };

        context.Restaurants.AddRange(restaurant1, restaurant2, restaurant3, restaurant4);

        // Create Menu Categories for Restaurant 1
        var menuCat1 = new MenuCategory { RestaurantId = restaurant1.Id, Name = "Món đặc trưng", NameSecondary = "人氣", IconName = "local_fire_department", DisplayOrder = 1 };
        var menuCat2 = new MenuCategory { RestaurantId = restaurant1.Id, Name = "Điểm sấm", NameSecondary = "點心", DisplayOrder = 2 };
        var menuCat3 = new MenuCategory { RestaurantId = restaurant1.Id, Name = "Mì & Phở", NameSecondary = "麵類", DisplayOrder = 3 };
        var menuCat4 = new MenuCategory { RestaurantId = restaurant1.Id, Name = "Đồ uống", NameSecondary = "飲料", DisplayOrder = 4 };

        context.MenuCategories.AddRange(menuCat1, menuCat2, menuCat3, menuCat4);

        // Create Menu Items
        context.MenuItems.AddRange(
            // Món đặc trưng
            new MenuItem
            {
                MenuCategoryId = menuCat1.Id, Name = "Há Cảo Tôm", NameSecondary = "鮮蝦餃",
                Description = "Bánh bao vỏ mỏng dai nhân tôm tươi nguyên con và măng tre giòn ngọt.",
                ImageUrl = "https://images.unsplash.com/photo-1563245372-f21724e3856d?w=400",
                Price = 65000, IsPopular = true
            },
            new MenuItem
            {
                MenuCategoryId = menuCat1.Id, Name = "Thịt Heo Quay Giòn", NameSecondary = "脆皮燒肉",
                Description = "Thịt ba chỉ quay chậm với lớp da giòn rụm và thịt mềm, ăn kèm sốt mù tạt vàng.",
                ImageUrl = "https://images.unsplash.com/photo-1544025162-d76978fc1a88?w=400",
                Price = 125000, IsPopular = true
            },
            // Điểm sấm
            new MenuItem
            {
                MenuCategoryId = menuCat2.Id, Name = "Xíu Mại", NameSecondary = "燒賣",
                Description = "Bánh bao truyền thống nhân thịt heo và tôm, bên trên phủ trứng cua.",
                ImageUrl = "https://images.unsplash.com/photo-1496116218417-1a781b1c416c?w=400",
                Price = 55000
            },
            new MenuItem
            {
                MenuCategoryId = menuCat2.Id, Name = "Bánh Bao Xá Xíu", NameSecondary = "叉燒包",
                Description = "Bánh bao hấp mềm xốp nhân thịt xá xíu đậm đà, thơm ngon.",
                ImageUrl = "https://images.unsplash.com/photo-1529692236671-f1f6cf9683ba?w=400",
                Price = 50000
            },
            // Mì & Phở
            new MenuItem
            {
                MenuCategoryId = menuCat3.Id, Name = "Mì Hoành Thánh", NameSecondary = "雲吞麵",
                Description = "Mì trứng tươi chan nước dùng đậm đà, ăn kèm hoành thánh tôm gói tay và cải thìa.",
                ImageUrl = "https://images.unsplash.com/photo-1569718212165-3a8278d5f624?w=400",
                Price = 110000
            },
            // Đồ uống
            new MenuItem
            {
                MenuCategoryId = menuCat4.Id, Name = "Trà Đào Cam Sả",
                Description = "Trà ướp hương đào, cam tươi và sả thơm mát.",
                ImageUrl = "https://images.unsplash.com/photo-1556679343-c7306c1976bc?w=400",
                Price = 45000
            },
            new MenuItem
            {
                MenuCategoryId = menuCat4.Id, Name = "Cà Phê Sữa Đá",
                Description = "Cà phê phin đậm đà pha với sữa đặc, thêm đá giòn.",
                ImageUrl = "https://images.unsplash.com/photo-1461023058943-07fcbe16d735?w=400",
                Price = 35000
            }
        );

        // Create Promotions
        context.Promotions.AddRange(
            new Promotion
            {
                Title = "Combo Trưa 59k",
                Subtitle = "Đặt ngay miễn phí giao hàng",
                ImageUrl = "https://images.unsplash.com/photo-1504674900247-0877df9cc836?w=800",
                Badge = "Freeship",
                BadgeColor = "primary",
                StartDate = DateTime.UtcNow.AddDays(-7),
                EndDate = DateTime.UtcNow.AddDays(30),
                DisplayOrder = 1
            },
            new Promotion
            {
                Title = "Tiệc Pizza",
                Subtitle = "HSD đến 30/10",
                ImageUrl = "https://images.unsplash.com/photo-1565299624946-b28f40a0ae38?w=800",
                Badge = "GIẢM 50%",
                BadgeColor = "orange-500",
                StartDate = DateTime.UtcNow.AddDays(-7),
                EndDate = DateTime.UtcNow.AddDays(60),
                DisplayOrder = 2
            }
        );

        // Create Vouchers
        context.Vouchers.AddRange(
            new Voucher
            {
                Code = "FREESHIP",
                Name = "Miễn phí vận chuyển",
                Description = "Giảm tối đa 15k cho đơn từ 0đ",
                Type = VoucherType.FreeShipping,
                DiscountValue = 15000,
                MaxDiscountAmount = 15000,
                MinOrderAmount = 0,
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow.AddDays(30),
                Category = "Vận chuyển"
            },
            new Voucher
            {
                Code = "GIAM50K",
                Name = "Giảm 50k",
                Description = "Đơn tối thiểu 200k • Quán Đặc Biệt",
                Type = VoucherType.FixedAmount,
                DiscountValue = 50000,
                MaxDiscountAmount = 50000,
                MinOrderAmount = 200000,
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow.AddDays(15),
                Category = "Đồ ăn"
            },
            new Voucher
            {
                Code = "VIMOMO20",
                Name = "Giảm 20k thanh toán ví",
                Description = "Áp dụng cho khách hàng mới liên kết",
                Type = VoucherType.FixedAmount,
                DiscountValue = 20000,
                MaxDiscountAmount = 20000,
                MinOrderAmount = 50000,
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow.AddDays(365),
                Category = "Đối tác"
            }
        );

        // Create Sample Orders
        var order1 = new Order
        {
            OrderNumber = "#260114001",
            CustomerId = customer.Id,
            RestaurantId = restaurant1.Id,
            DriverId = driver.Id,
            DeliveryAddress = "123 Nguyễn Huệ, Phường Bến Nghé, Quận 1",
            DeliveryLatitude = 10.7769,
            DeliveryLongitude = 106.7009,
            Subtotal = 115000,
            DeliveryFee = 20000,
            DiscountAmount = 0,
            TotalAmount = 135000,
            Status = OrderStatus.Delivering,
            PaymentMethod = PaymentMethod.Cash,
            PaymentStatus = PaymentStatus.Pending,
            EstimatedDeliveryMinutes = 15,
            ConfirmedAt = DateTime.UtcNow.AddMinutes(-20),
            PreparedAt = DateTime.UtcNow.AddMinutes(-10),
            PickedUpAt = DateTime.UtcNow.AddMinutes(-5)
        };

        context.Orders.Add(order1);

        // Order Items
        context.OrderItems.AddRange(
            new OrderItem
            {
                OrderId = order1.Id,
                MenuItemId = context.MenuItems.Local.First().Id,
                ItemName = "Há Cảo Tôm",
                UnitPrice = 65000,
                Quantity = 1,
                TotalPrice = 65000
            },
            new OrderItem
            {
                OrderId = order1.Id,
                MenuItemId = context.MenuItems.Local.Skip(2).First().Id,
                ItemName = "Xíu Mại",
                UnitPrice = 50000,
                Quantity = 1,
                TotalPrice = 50000
            }
        );

        // Order Tracking
        context.OrderTrackings.AddRange(
            new OrderTracking { OrderId = order1.Id, Status = OrderStatus.Pending, Description = "Đơn hàng đã được tạo", Timestamp = DateTime.UtcNow.AddMinutes(-25) },
            new OrderTracking { OrderId = order1.Id, Status = OrderStatus.Confirmed, Description = "Nhà hàng đã xác nhận", Timestamp = DateTime.UtcNow.AddMinutes(-20) },
            new OrderTracking { OrderId = order1.Id, Status = OrderStatus.Preparing, Description = "Đang chuẩn bị món", Timestamp = DateTime.UtcNow.AddMinutes(-15) },
            new OrderTracking { OrderId = order1.Id, Status = OrderStatus.ReadyForPickup, Description = "Sẵn sàng giao", Timestamp = DateTime.UtcNow.AddMinutes(-10) },
            new OrderTracking { OrderId = order1.Id, Status = OrderStatus.PickedUp, Description = "Tài xế đã lấy hàng", Timestamp = DateTime.UtcNow.AddMinutes(-5) },
            new OrderTracking { OrderId = order1.Id, Status = OrderStatus.Delivering, Description = "Đang giao đến bạn", Timestamp = DateTime.UtcNow.AddMinutes(-2) }
        );

        // Driver Earnings
        context.DriverEarnings.AddRange(
            new DriverEarning { DriverId = driver.Id, Type = EarningType.Delivery, Amount = 35000, Description = "Đơn hàng #260113001", EarnedAt = DateTime.UtcNow.AddHours(-2) },
            new DriverEarning { DriverId = driver.Id, Type = EarningType.Tip, Amount = 10000, Description = "Tip từ khách", EarnedAt = DateTime.UtcNow.AddHours(-2) },
            new DriverEarning { DriverId = driver.Id, Type = EarningType.Bonus, Amount = 100000, Description = "Tiền thưởng ngày", EarnedAt = DateTime.UtcNow.AddDays(-1) },
            new DriverEarning { DriverId = driver.Id, Type = EarningType.Delivery, Amount = 42000, Description = "Đơn hàng #260113002", EarnedAt = DateTime.UtcNow.AddHours(-5) }
        );

        await context.SaveChangesAsync();
    }
}
