using FoodDelivery.Domain.Enums;

namespace FoodDelivery.Domain.Entities;

public class DriverEarning : BaseEntity
{
    public Guid DriverId { get; set; }
    public Driver Driver { get; set; } = null!;
    
    public Guid? OrderId { get; set; }
    public Order? Order { get; set; }
    
    public EarningType Type { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    
    public DateTime EarnedAt { get; set; } = DateTime.UtcNow;
}
