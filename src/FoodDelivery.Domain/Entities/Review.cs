namespace FoodDelivery.Domain.Entities;

public class Review : BaseEntity
{
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
    
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    
    // Driver rating
    public int DriverRating { get; set; } // 1-5
    public string? DriverComment { get; set; }
    public string? DriverTags { get; set; } // JSON: ["Thân thiện", "Đúng giờ"]
    
    // Food rating
    public int FoodRating { get; set; } // 1-5
    public string? FoodComment { get; set; }
    
    // Media
    public string? ImageUrls { get; set; } // JSON array of image URLs
    public string? VideoUrl { get; set; }
}
