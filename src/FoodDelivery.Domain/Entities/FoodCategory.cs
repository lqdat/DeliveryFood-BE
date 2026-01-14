namespace FoodDelivery.Domain.Entities;

public class FoodCategory : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? NameSecondary { get; set; } // Rice, Noodles, etc.
    public string? IconUrl { get; set; }
    public string? BackgroundColor { get; set; } // "orange-100"
    public int DisplayOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
}
