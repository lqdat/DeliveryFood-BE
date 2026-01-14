using FoodDelivery.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Application.Interfaces;

public interface IAppDbContext
{
    DbSet<User> Users { get; }
    DbSet<Customer> Customers { get; }
    DbSet<Merchant> Merchants { get; }
    DbSet<Driver> Drivers { get; }
    DbSet<Address> Addresses { get; }
    DbSet<Restaurant> Restaurants { get; }
    DbSet<MenuCategory> MenuCategories { get; }
    DbSet<MenuItem> MenuItems { get; }
    DbSet<FoodCategory> FoodCategories { get; }
    DbSet<Promotion> Promotions { get; }
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    DbSet<OrderTracking> OrderTrackings { get; }
    DbSet<Voucher> Vouchers { get; }
    DbSet<Review> Reviews { get; }
    DbSet<ChatMessage> ChatMessages { get; }
    DbSet<DriverEarning> DriverEarnings { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
