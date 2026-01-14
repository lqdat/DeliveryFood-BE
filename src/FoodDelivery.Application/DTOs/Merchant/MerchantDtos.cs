using System.ComponentModel.DataAnnotations;
using FoodDelivery.Domain.Enums;

namespace FoodDelivery.Application.DTOs.Merchant;

public class MerchantDashboardDto
{
    public RestaurantInfoDto Restaurant { get; set; } = null!;
    public bool IsOpen { get; set; }
    public decimal TodayRevenue { get; set; }
    public decimal WeeklyRevenue { get; set; }
    public int TodayOrderCount { get; set; }
    public int PendingOrderCount { get; set; }
    public double RevenueChangePercentage { get; set; }
    public List<MerchantOrderDto> CurrentOrders { get; set; } = new();
    public WeeklyRevenueChartDto WeeklyChart { get; set; } = null!;
}

public class RestaurantInfoDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
}

public class MerchantOrderDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int ItemCount { get; set; }
    public bool IsDelivery { get; set; }
    public CustomerInfoDto Customer { get; set; } = null!;
    public List<OrderItemSummaryDto> Items { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class CustomerInfoDto
{
    public string Name { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
}

public class OrderItemSummaryDto
{
    public string ItemName { get; set; } = string.Empty;
    public int Quantity { get; set; }
}

public class WeeklyRevenueChartDto
{
    public List<DailyRevenueDto> Days { get; set; } = new();
}

public class DailyRevenueDto
{
    public string DayName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public double Percentage { get; set; }
}

// Menu Management
public class MenuItemManageDto
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? NameSecondary { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public decimal Price { get; set; }
    public decimal? OriginalPrice { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsPopular { get; set; }
    public int DisplayOrder { get; set; }
}

public class CreateMenuItemDto
{
    [Required]
    public Guid CategoryId { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public string? NameSecondary { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    
    [Required]
    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }
    
    public decimal? OriginalPrice { get; set; }
    public bool IsAvailable { get; set; } = true;
    public bool IsPopular { get; set; } = false;
}

public class UpdateMenuItemDto
{
    public string? Name { get; set; }
    public string? NameSecondary { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public decimal? Price { get; set; }
    public decimal? OriginalPrice { get; set; }
    public bool? IsAvailable { get; set; }
    public bool? IsPopular { get; set; }
    public int? DisplayOrder { get; set; }
}

public class UpdateRestaurantStatusDto
{
    public bool IsOpen { get; set; }
}
