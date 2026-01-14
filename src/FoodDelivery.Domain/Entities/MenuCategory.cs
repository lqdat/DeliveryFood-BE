namespace FoodDelivery.Domain.Entities;

public class MenuCategory : BaseEntity
{
    public Guid RestaurantId { get; set; }
    public Restaurant Restaurant { get; set; } = null!;
    
    public string Name { get; set; } = string.Empty;
    public string? NameSecondary { get; set; } // Tên phụ (vd: 人氣, 點心)
    public string? Description { get; set; }
    public string? IconName { get; set; }
    public int DisplayOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
}
