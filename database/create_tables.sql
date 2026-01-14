-- =====================================================
-- Food Delivery Database Schema
-- PostgreSQL Script
-- =====================================================

-- Create extension for UUID generation
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- =====================================================
-- ENUM Types
-- =====================================================

DO $$ BEGIN
    CREATE TYPE user_role AS ENUM ('Customer', 'Merchant', 'Driver', 'Admin');
EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

DO $$ BEGIN
    CREATE TYPE order_status AS ENUM ('Pending', 'Confirmed', 'Preparing', 'ReadyForPickup', 'PickedUp', 'Delivering', 'Delivered', 'Cancelled');
EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

DO $$ BEGIN
    CREATE TYPE payment_method AS ENUM ('Cash', 'MoMo', 'ZaloPay', 'Card');
EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

DO $$ BEGIN
    CREATE TYPE payment_status AS ENUM ('Pending', 'Paid', 'Failed', 'Refunded');
EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

DO $$ BEGIN
    CREATE TYPE driver_status AS ENUM ('Offline', 'Online', 'Busy');
EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

DO $$ BEGIN
    CREATE TYPE voucher_type AS ENUM ('Percentage', 'FixedAmount', 'FreeShipping');
EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

DO $$ BEGIN
    CREATE TYPE earning_type AS ENUM ('Delivery', 'Tip', 'Bonus');
EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

-- =====================================================
-- TABLES
-- =====================================================

-- Users Table
CREATE TABLE IF NOT EXISTS "Users" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "PhoneNumber" VARCHAR(20) NOT NULL UNIQUE,
    "Email" VARCHAR(255) UNIQUE,
    "PasswordHash" VARCHAR(255) NOT NULL,
    "FullName" VARCHAR(100) NOT NULL,
    "AvatarUrl" VARCHAR(500),
    "Role" INTEGER NOT NULL DEFAULT 1,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "LastLoginAt" TIMESTAMP,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE
);

-- Customers Table
CREATE TABLE IF NOT EXISTS "Customers" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "UserId" UUID NOT NULL UNIQUE REFERENCES "Users"("Id") ON DELETE CASCADE,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE
);

-- Merchants Table
CREATE TABLE IF NOT EXISTS "Merchants" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "UserId" UUID NOT NULL UNIQUE REFERENCES "Users"("Id") ON DELETE CASCADE,
    "BusinessName" VARCHAR(255) NOT NULL DEFAULT '',
    "BusinessLicense" VARCHAR(255),
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE
);

-- Drivers Table
CREATE TABLE IF NOT EXISTS "Drivers" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "UserId" UUID NOT NULL UNIQUE REFERENCES "Users"("Id") ON DELETE CASCADE,
    "VehicleType" VARCHAR(50) NOT NULL DEFAULT '',
    "VehiclePlate" VARCHAR(20) NOT NULL DEFAULT '',
    "VehicleBrand" VARCHAR(50) NOT NULL DEFAULT '',
    "Status" INTEGER NOT NULL DEFAULT 1,
    "WalletBalance" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "CurrentLatitude" DOUBLE PRECISION,
    "CurrentLongitude" DOUBLE PRECISION,
    "Rating" DOUBLE PRECISION NOT NULL DEFAULT 5.0,
    "TotalDeliveries" INTEGER NOT NULL DEFAULT 0,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE
);

-- Addresses Table
CREATE TABLE IF NOT EXISTS "Addresses" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "CustomerId" UUID NOT NULL REFERENCES "Customers"("Id") ON DELETE CASCADE,
    "Label" VARCHAR(50) NOT NULL DEFAULT 'Nhà',
    "FullAddress" VARCHAR(500) NOT NULL,
    "Note" VARCHAR(255),
    "Latitude" DOUBLE PRECISION NOT NULL,
    "Longitude" DOUBLE PRECISION NOT NULL,
    "IsDefault" BOOLEAN NOT NULL DEFAULT FALSE,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE
);

-- Restaurants Table
CREATE TABLE IF NOT EXISTS "Restaurants" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "MerchantId" UUID NOT NULL REFERENCES "Merchants"("Id") ON DELETE CASCADE,
    "Name" VARCHAR(255) NOT NULL,
    "NameSecondary" VARCHAR(255),
    "Description" TEXT,
    "ImageUrl" VARCHAR(500),
    "CoverImageUrl" VARCHAR(500),
    "Address" VARCHAR(500) NOT NULL,
    "Latitude" DOUBLE PRECISION NOT NULL,
    "Longitude" DOUBLE PRECISION NOT NULL,
    "PhoneNumber" VARCHAR(20),
    "Category" VARCHAR(100),
    "IsOpen" BOOLEAN NOT NULL DEFAULT TRUE,
    "OpenTime" TIME,
    "CloseTime" TIME,
    "EstimatedDeliveryMinutes" INTEGER NOT NULL DEFAULT 30,
    "DeliveryFee" DECIMAL(18,2) NOT NULL DEFAULT 15000,
    "MinOrderAmount" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "Rating" DOUBLE PRECISION NOT NULL DEFAULT 5.0,
    "TotalReviews" INTEGER NOT NULL DEFAULT 0,
    "HasPromotion" BOOLEAN NOT NULL DEFAULT FALSE,
    "IsNew" BOOLEAN NOT NULL DEFAULT FALSE,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE
);

-- Food Categories Table (Homepage categories)
CREATE TABLE IF NOT EXISTS "FoodCategories" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "Name" VARCHAR(100) NOT NULL,
    "NameSecondary" VARCHAR(100),
    "IconUrl" VARCHAR(500),
    "BackgroundColor" VARCHAR(50),
    "DisplayOrder" INTEGER NOT NULL DEFAULT 0,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE
);

-- Menu Categories Table
CREATE TABLE IF NOT EXISTS "MenuCategories" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "RestaurantId" UUID NOT NULL REFERENCES "Restaurants"("Id") ON DELETE CASCADE,
    "Name" VARCHAR(100) NOT NULL,
    "NameSecondary" VARCHAR(100),
    "Description" TEXT,
    "IconName" VARCHAR(50),
    "DisplayOrder" INTEGER NOT NULL DEFAULT 0,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE
);

-- Menu Items Table
CREATE TABLE IF NOT EXISTS "MenuItems" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "MenuCategoryId" UUID NOT NULL REFERENCES "MenuCategories"("Id") ON DELETE CASCADE,
    "Name" VARCHAR(255) NOT NULL,
    "NameSecondary" VARCHAR(255),
    "Description" TEXT,
    "ImageUrl" VARCHAR(500),
    "Price" DECIMAL(18,2) NOT NULL,
    "OriginalPrice" DECIMAL(18,2),
    "IsAvailable" BOOLEAN NOT NULL DEFAULT TRUE,
    "IsPopular" BOOLEAN NOT NULL DEFAULT FALSE,
    "DisplayOrder" INTEGER NOT NULL DEFAULT 0,
    "Options" TEXT,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE
);

-- Vouchers Table
CREATE TABLE IF NOT EXISTS "Vouchers" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "Code" VARCHAR(50) NOT NULL UNIQUE,
    "Name" VARCHAR(255) NOT NULL,
    "Description" TEXT,
    "Type" INTEGER NOT NULL,
    "DiscountValue" DECIMAL(18,2) NOT NULL,
    "MaxDiscountAmount" DECIMAL(18,2) NOT NULL,
    "MinOrderAmount" DECIMAL(18,2) NOT NULL,
    "StartDate" TIMESTAMP NOT NULL,
    "EndDate" TIMESTAMP NOT NULL,
    "MaxUsage" INTEGER,
    "UsedCount" INTEGER NOT NULL DEFAULT 0,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "Category" VARCHAR(100),
    "IconUrl" VARCHAR(500),
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE
);

-- Promotions Table
CREATE TABLE IF NOT EXISTS "Promotions" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "Title" VARCHAR(255) NOT NULL,
    "Subtitle" VARCHAR(255),
    "Description" TEXT,
    "ImageUrl" VARCHAR(500),
    "Badge" VARCHAR(50),
    "BadgeColor" VARCHAR(50),
    "StartDate" TIMESTAMP NOT NULL,
    "EndDate" TIMESTAMP NOT NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "DisplayOrder" INTEGER NOT NULL DEFAULT 0,
    "RestaurantId" UUID REFERENCES "Restaurants"("Id") ON DELETE SET NULL,
    "VoucherId" UUID REFERENCES "Vouchers"("Id") ON DELETE SET NULL,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE
);

-- Orders Table
CREATE TABLE IF NOT EXISTS "Orders" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "OrderNumber" VARCHAR(50) NOT NULL UNIQUE,
    "CustomerId" UUID NOT NULL REFERENCES "Customers"("Id") ON DELETE RESTRICT,
    "RestaurantId" UUID NOT NULL REFERENCES "Restaurants"("Id") ON DELETE RESTRICT,
    "DriverId" UUID REFERENCES "Drivers"("Id") ON DELETE SET NULL,
    "VoucherId" UUID REFERENCES "Vouchers"("Id") ON DELETE SET NULL,
    "DeliveryAddress" VARCHAR(500) NOT NULL,
    "DeliveryLatitude" DOUBLE PRECISION NOT NULL,
    "DeliveryLongitude" DOUBLE PRECISION NOT NULL,
    "DeliveryNote" TEXT,
    "Subtotal" DECIMAL(18,2) NOT NULL,
    "DeliveryFee" DECIMAL(18,2) NOT NULL,
    "DiscountAmount" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "TotalAmount" DECIMAL(18,2) NOT NULL,
    "Status" INTEGER NOT NULL DEFAULT 1,
    "PaymentMethod" INTEGER NOT NULL DEFAULT 1,
    "PaymentStatus" INTEGER NOT NULL DEFAULT 1,
    "ConfirmedAt" TIMESTAMP,
    "PreparedAt" TIMESTAMP,
    "PickedUpAt" TIMESTAMP,
    "DeliveredAt" TIMESTAMP,
    "CancelledAt" TIMESTAMP,
    "CancellationReason" TEXT,
    "EstimatedDeliveryMinutes" INTEGER NOT NULL DEFAULT 30,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE
);

-- Order Items Table
CREATE TABLE IF NOT EXISTS "OrderItems" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "OrderId" UUID NOT NULL REFERENCES "Orders"("Id") ON DELETE CASCADE,
    "MenuItemId" UUID NOT NULL REFERENCES "MenuItems"("Id") ON DELETE RESTRICT,
    "ItemName" VARCHAR(255) NOT NULL,
    "UnitPrice" DECIMAL(18,2) NOT NULL,
    "Quantity" INTEGER NOT NULL DEFAULT 1,
    "TotalPrice" DECIMAL(18,2) NOT NULL,
    "Notes" TEXT,
    "SelectedOptions" TEXT,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE
);

-- Order Tracking Table
CREATE TABLE IF NOT EXISTS "OrderTrackings" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "OrderId" UUID NOT NULL REFERENCES "Orders"("Id") ON DELETE CASCADE,
    "Status" INTEGER NOT NULL,
    "Description" VARCHAR(500),
    "DescriptionSecondary" VARCHAR(500),
    "Timestamp" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "DriverLatitude" DOUBLE PRECISION,
    "DriverLongitude" DOUBLE PRECISION,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE
);

-- Reviews Table
CREATE TABLE IF NOT EXISTS "Reviews" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "OrderId" UUID NOT NULL UNIQUE REFERENCES "Orders"("Id") ON DELETE CASCADE,
    "CustomerId" UUID NOT NULL REFERENCES "Customers"("Id") ON DELETE RESTRICT,
    "DriverRating" INTEGER NOT NULL CHECK ("DriverRating" >= 1 AND "DriverRating" <= 5),
    "DriverComment" TEXT,
    "DriverTags" TEXT,
    "FoodRating" INTEGER NOT NULL CHECK ("FoodRating" >= 1 AND "FoodRating" <= 5),
    "FoodComment" TEXT,
    "ImageUrls" TEXT,
    "VideoUrl" VARCHAR(500),
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE
);

-- Chat Messages Table
CREATE TABLE IF NOT EXISTS "ChatMessages" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "OrderId" UUID NOT NULL REFERENCES "Orders"("Id") ON DELETE CASCADE,
    "SenderId" UUID NOT NULL,
    "IsFromCustomer" BOOLEAN NOT NULL,
    "Content" TEXT NOT NULL,
    "ImageUrl" VARCHAR(500),
    "IsRead" BOOLEAN NOT NULL DEFAULT FALSE,
    "ReadAt" TIMESTAMP,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE
);

-- Driver Earnings Table
CREATE TABLE IF NOT EXISTS "DriverEarnings" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "DriverId" UUID NOT NULL REFERENCES "Drivers"("Id") ON DELETE CASCADE,
    "OrderId" UUID REFERENCES "Orders"("Id") ON DELETE SET NULL,
    "Type" INTEGER NOT NULL,
    "Amount" DECIMAL(18,2) NOT NULL,
    "Description" TEXT,
    "EarnedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE
);

-- =====================================================
-- INDEXES
-- =====================================================

CREATE INDEX IF NOT EXISTS idx_users_phone ON "Users"("PhoneNumber");
CREATE INDEX IF NOT EXISTS idx_users_email ON "Users"("Email");
CREATE INDEX IF NOT EXISTS idx_restaurants_merchant ON "Restaurants"("MerchantId");
CREATE INDEX IF NOT EXISTS idx_restaurants_category ON "Restaurants"("Category");
CREATE INDEX IF NOT EXISTS idx_restaurants_location ON "Restaurants"("Latitude", "Longitude");
CREATE INDEX IF NOT EXISTS idx_menu_categories_restaurant ON "MenuCategories"("RestaurantId");
CREATE INDEX IF NOT EXISTS idx_menu_items_category ON "MenuItems"("MenuCategoryId");
CREATE INDEX IF NOT EXISTS idx_orders_customer ON "Orders"("CustomerId");
CREATE INDEX IF NOT EXISTS idx_orders_restaurant ON "Orders"("RestaurantId");
CREATE INDEX IF NOT EXISTS idx_orders_driver ON "Orders"("DriverId");
CREATE INDEX IF NOT EXISTS idx_orders_status ON "Orders"("Status");
CREATE INDEX IF NOT EXISTS idx_orders_created ON "Orders"("CreatedAt");
CREATE INDEX IF NOT EXISTS idx_order_items_order ON "OrderItems"("OrderId");
CREATE INDEX IF NOT EXISTS idx_order_tracking_order ON "OrderTrackings"("OrderId");
CREATE INDEX IF NOT EXISTS idx_chat_messages_order ON "ChatMessages"("OrderId");
CREATE INDEX IF NOT EXISTS idx_driver_earnings_driver ON "DriverEarnings"("DriverId");
CREATE INDEX IF NOT EXISTS idx_driver_earnings_date ON "DriverEarnings"("EarnedAt");
CREATE INDEX IF NOT EXISTS idx_vouchers_code ON "Vouchers"("Code");
CREATE INDEX IF NOT EXISTS idx_vouchers_dates ON "Vouchers"("StartDate", "EndDate");

-- =====================================================
-- SUCCESS MESSAGE
-- =====================================================
SELECT 'All tables created successfully!' AS result;
