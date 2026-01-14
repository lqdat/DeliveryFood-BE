using FoodDelivery.Domain.Enums;

namespace FoodDelivery.Domain.Entities;

public class User : BaseEntity
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
    
    // Navigation properties
    public Customer? Customer { get; set; }
    public Merchant? Merchant { get; set; }
    public Driver? Driver { get; set; }
}
