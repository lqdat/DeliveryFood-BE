using FoodDelivery.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Users
    public DbSet<User> Users => Set<User>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Merchant> Merchants => Set<Merchant>();
    public DbSet<Driver> Drivers => Set<Driver>();
    public DbSet<Address> Addresses => Set<Address>();

    // Restaurant & Menu
    public DbSet<Restaurant> Restaurants => Set<Restaurant>();
    public DbSet<MenuCategory> MenuCategories => Set<MenuCategory>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<FoodCategory> FoodCategories => Set<FoodCategory>();
    public DbSet<Promotion> Promotions => Set<Promotion>();

    // Orders
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderTracking> OrderTrackings => Set<OrderTracking>();
    
    // Cart
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    
    // Vouchers
    public DbSet<Voucher> Vouchers => Set<Voucher>();

    // Reviews & Chat
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

    // Driver
    public DbSet<DriverEarning> DriverEarnings => Set<DriverEarning>();
    
    // Search
    public DbSet<SearchHistory> SearchHistories => Set<SearchHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User Configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.PhoneNumber).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique().HasFilter("\"Email\" IS NOT NULL");
        });

        // User -> Customer (1:1)
        modelBuilder.Entity<Customer>()
            .HasOne(c => c.User)
            .WithOne(u => u.Customer)
            .HasForeignKey<Customer>(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // User -> Merchant (1:1)
        modelBuilder.Entity<Merchant>()
            .HasOne(m => m.User)
            .WithOne(u => u.Merchant)
            .HasForeignKey<Merchant>(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // User -> Driver (1:1)
        modelBuilder.Entity<Driver>()
            .HasOne(d => d.User)
            .WithOne(u => u.Driver)
            .HasForeignKey<Driver>(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Merchant -> Restaurant (1:N)
        modelBuilder.Entity<Restaurant>()
            .HasOne(r => r.Merchant)
            .WithMany(m => m.Restaurants)
            .HasForeignKey(r => r.MerchantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restaurant -> MenuCategory (1:N)
        modelBuilder.Entity<MenuCategory>()
            .HasOne(mc => mc.Restaurant)
            .WithMany(r => r.MenuCategories)
            .HasForeignKey(mc => mc.RestaurantId)
            .OnDelete(DeleteBehavior.Cascade);

        // MenuCategory -> MenuItem (1:N)
        modelBuilder.Entity<MenuItem>()
            .HasOne(mi => mi.MenuCategory)
            .WithMany(mc => mc.MenuItems)
            .HasForeignKey(mi => mi.MenuCategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        // Customer -> Address (1:N)
        modelBuilder.Entity<Address>()
            .HasOne(a => a.Customer)
            .WithMany(c => c.Addresses)
            .HasForeignKey(a => a.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        // Order relationships
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasIndex(e => e.OrderNumber).IsUnique();

            entity.HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(o => o.Restaurant)
                .WithMany(r => r.Orders)
                .HasForeignKey(o => o.RestaurantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(o => o.Driver)
                .WithMany(d => d.Orders)
                .HasForeignKey(o => o.DriverId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(o => o.Voucher)
                .WithMany(v => v.Orders)
                .HasForeignKey(o => o.VoucherId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Order -> OrderItem (1:N)
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.OrderItems)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Order -> OrderTracking (1:N)
        modelBuilder.Entity<OrderTracking>()
            .HasOne(ot => ot.Order)
            .WithMany(o => o.TrackingHistory)
            .HasForeignKey(ot => ot.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Order -> ChatMessage (1:1)
        modelBuilder.Entity<ChatMessage>()
            .HasOne(cm => cm.Order)
            .WithMany(o => o.ChatMessages)
            .HasForeignKey(cm => cm.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Cart Configuration
        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasOne(c => c.Customer)
                .WithOne()
                .HasForeignKey<Cart>(c => c.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(c => c.Restaurant)
                .WithMany()
                .HasForeignKey(c => c.RestaurantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CartItem>()
            .HasOne(ci => ci.Cart)
            .WithMany(c => c.Items)
            .HasForeignKey(ci => ci.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        // Order -> Review (1:1)
        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasOne(r => r.Order)
                .WithOne(o => o.Review)
                .HasForeignKey<Review>(r => r.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.Customer)
                .WithMany(c => c.Reviews)
                .HasForeignKey(r => r.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Driver -> DriverEarning (1:N)
        modelBuilder.Entity<DriverEarning>()
            .HasOne(de => de.Driver)
            .WithMany(d => d.Earnings)
            .HasForeignKey(de => de.DriverId)
            .OnDelete(DeleteBehavior.Cascade);

        // Voucher
        modelBuilder.Entity<Voucher>()
            .HasIndex(v => v.Code)
            .IsUnique();

        // Decimal precision
        modelBuilder.Entity<MenuItem>()
            .Property(m => m.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Order>()
            .Property(o => o.Subtotal)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Order>()
            .Property(o => o.DeliveryFee)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Order>()
            .Property(o => o.DiscountAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Order>()
            .Property(o => o.TotalAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Driver>()
            .Property(d => d.WalletBalance)
            .HasPrecision(18, 2);

        modelBuilder.Entity<DriverEarning>()
            .Property(de => de.Amount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Voucher>()
            .Property(v => v.DiscountValue)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Voucher>()
            .Property(v => v.MaxDiscountAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Voucher>()
            .Property(v => v.MinOrderAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<CartItem>()
            .Property(ci => ci.UnitPrice)
            .HasPrecision(18, 2);

        // Search History
        modelBuilder.Entity<SearchHistory>(entity =>
        {
            entity.HasOne(sh => sh.User)
                .WithMany()
                .HasForeignKey(sh => sh.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
