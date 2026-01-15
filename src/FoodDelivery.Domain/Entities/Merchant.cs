namespace FoodDelivery.Domain.Entities;

public class Merchant : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    public string BusinessName { get; set; } = string.Empty;
    public string? BusinessLicense { get; set; }
    
    // Approval workflow
    public bool IsApproved { get; set; } = false;
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }
    
    // Document URLs for verification
    public string? BusinessLicenseUrl { get; set; }
    public string? FoodSafetyCertUrl { get; set; }
    public string? IdCardFrontUrl { get; set; }
    
    // Navigation properties
    public ICollection<Restaurant> Restaurants { get; set; } = new List<Restaurant>();
}

