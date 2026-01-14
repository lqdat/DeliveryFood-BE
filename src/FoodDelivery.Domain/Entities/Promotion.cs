namespace FoodDelivery.Domain.Entities;

public class Promotion : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string? Badge { get; set; } // "Freeship", "GIẢM 50%"
    public string? BadgeColor { get; set; } // "primary", "orange-500"
    
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; } = 0;
    
    public Guid? RestaurantId { get; set; }
    public Restaurant? Restaurant { get; set; }
    
    public Guid? VoucherId { get; set; }
    public Voucher? Voucher { get; set; }
}
