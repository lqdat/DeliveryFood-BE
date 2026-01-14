namespace FoodDelivery.Domain.Entities;

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
    
    public Guid MenuItemId { get; set; }
    public MenuItem MenuItem { get; set; } = null!;
    
    public string ItemName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal TotalPrice { get; set; }
    
    public string? Notes { get; set; } // Không ớt, nhiều đá, etc.
    public string? SelectedOptions { get; set; } // JSON options
}
