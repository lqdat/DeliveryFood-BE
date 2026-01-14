using FoodDelivery.Domain.Enums;

namespace FoodDelivery.Domain.Entities;

public class OrderTracking : BaseEntity
{
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
    
    public OrderStatus Status { get; set; }
    public string? Description { get; set; }
    public string? DescriptionSecondary { get; set; } // English
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    public double? DriverLatitude { get; set; }
    public double? DriverLongitude { get; set; }
}
