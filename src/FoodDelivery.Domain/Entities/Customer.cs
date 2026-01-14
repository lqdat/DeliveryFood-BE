namespace FoodDelivery.Domain.Entities;

public class Customer : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    // Navigation properties
    public ICollection<Address> Addresses { get; set; } = new List<Address>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
