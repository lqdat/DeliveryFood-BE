namespace FoodDelivery.Domain.Entities;

public class CartItem : BaseEntity
{
    public Guid CartId { get; set; }
    public Cart Cart { get; set; } = null!;
    
    public Guid MenuItemId { get; set; }
    public MenuItem MenuItem { get; set; } = null!;
    
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    
    public string? Notes { get; set; }
    public string? SelectedOptions { get; set; }
}
