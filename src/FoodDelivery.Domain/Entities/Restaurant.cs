namespace FoodDelivery.Domain.Entities;

public class Restaurant : BaseEntity
{
    public Guid MerchantId { get; set; }
    public Merchant Merchant { get; set; } = null!;
    
    public string Name { get; set; } = string.Empty;
    public string? NameSecondary { get; set; } // Tên phụ (vd: 金龍小館)
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string? CoverImageUrl { get; set; }
    
    public string Address { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    
    public string? PhoneNumber { get; set; }
    public string? Category { get; set; } // Món Việt, Món Ý, etc.
    
    public bool IsOpen { get; set; } = true;
    public TimeSpan? OpenTime { get; set; }
    public TimeSpan? CloseTime { get; set; }
    
    public int EstimatedDeliveryMinutes { get; set; } = 30;
    public decimal DeliveryFee { get; set; } = 15000;
    public decimal MinOrderAmount { get; set; } = 0;
    
    public double Rating { get; set; } = 5.0;
    public int TotalReviews { get; set; } = 0;
    
    public bool HasPromotion { get; set; } = false;
    public bool IsNew { get; set; } = false;
    public bool IsApproved { get; set; } = true; // Default true for existing restaurants
    
    // Navigation properties
    public ICollection<MenuCategory> MenuCategories { get; set; } = new List<MenuCategory>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
