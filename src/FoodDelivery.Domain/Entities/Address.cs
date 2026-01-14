namespace FoodDelivery.Domain.Entities;

public class Address : BaseEntity
{
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    
    public string Label { get; set; } = "Nhà"; // Nhà, Công ty, etc.
    public string FullAddress { get; set; } = string.Empty;
    public string? Note { get; set; }
    
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    
    public bool IsDefault { get; set; } = false;
}
