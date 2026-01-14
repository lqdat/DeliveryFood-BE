namespace FoodDelivery.Domain.Entities;

public class MenuItem : BaseEntity
{
    public Guid MenuCategoryId { get; set; }
    public MenuCategory MenuCategory { get; set; } = null!;
    
    public string Name { get; set; } = string.Empty;
    public string? NameSecondary { get; set; } // Tên phụ (vd: 鮮蝦餃)
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    
    public decimal Price { get; set; }
    public decimal? OriginalPrice { get; set; } // Giá gốc (nếu đang giảm giá)
    
    public bool IsAvailable { get; set; } = true;
    public bool IsPopular { get; set; } = false;
    public int DisplayOrder { get; set; } = 0;
    
    // Tùy chọn món ăn (JSON: size, topping, etc.)
    public string? Options { get; set; }
}
