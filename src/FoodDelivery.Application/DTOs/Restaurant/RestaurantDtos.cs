namespace FoodDelivery.Application.DTOs.Restaurant;

public class RestaurantDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NameSecondary { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string? CoverImageUrl { get; set; }
    public string Address { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? Category { get; set; }
    public bool IsOpen { get; set; }
    public int EstimatedDeliveryMinutes { get; set; }
    public decimal DeliveryFee { get; set; }
    public double Rating { get; set; }
    public int TotalReviews { get; set; }
    public bool HasPromotion { get; set; }
    public bool IsNew { get; set; }
    public double Distance { get; set; } // Calculated field
}

public class RestaurantDetailDto : RestaurantDto
{
    public string? PhoneNumber { get; set; }
    public TimeSpan? OpenTime { get; set; }
    public TimeSpan? CloseTime { get; set; }
    public decimal MinOrderAmount { get; set; }
    public List<MenuCategoryDto> MenuCategories { get; set; } = new();
}

public class MenuCategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NameSecondary { get; set; }
    public string? Description { get; set; }
    public string? IconName { get; set; }
    public List<MenuItemDto> Items { get; set; } = new();
}

public class MenuItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NameSecondary { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public decimal Price { get; set; }
    public decimal? OriginalPrice { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsPopular { get; set; }
}

public class FoodCategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NameSecondary { get; set; }
    public string? IconUrl { get; set; }
    public string? BackgroundColor { get; set; }
}

public class PromotionDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? ImageUrl { get; set; }
    public string? Badge { get; set; }
    public string? BadgeColor { get; set; }
    public DateTime EndDate { get; set; }
}

public class NearbyRestaurantsQuery
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double RadiusKm { get; set; } = 5;
    public string? Category { get; set; }
    public string? SortBy { get; set; } // distance, rating, deliveryTime
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
