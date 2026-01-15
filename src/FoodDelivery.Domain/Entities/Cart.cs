using System.ComponentModel.DataAnnotations.Schema;

namespace FoodDelivery.Domain.Entities;

public class Cart : BaseEntity
{
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    
    public Guid RestaurantId { get; set; }
    public Restaurant Restaurant { get; set; } = null!;
    
    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    
    [NotMapped]
    public decimal Subtotal => Items.Sum(i => i.Quantity * i.UnitPrice);
}
