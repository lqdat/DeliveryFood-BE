namespace FoodDelivery.Domain.Entities;

public class Merchant : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    public string BusinessName { get; set; } = string.Empty;
    public string? BusinessLicense { get; set; }
    
    // Navigation properties
    public ICollection<Restaurant> Restaurants { get; set; } = new List<Restaurant>();
}
