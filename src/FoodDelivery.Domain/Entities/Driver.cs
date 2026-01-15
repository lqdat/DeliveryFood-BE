using FoodDelivery.Domain.Enums;

namespace FoodDelivery.Domain.Entities;

public class Driver : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    public string VehicleType { get; set; } = string.Empty; // Xe máy, Ô tô
    public string VehiclePlate { get; set; } = string.Empty;
    public string VehicleBrand { get; set; } = string.Empty;
    
    public DriverStatus Status { get; set; } = DriverStatus.Offline;
    public decimal WalletBalance { get; set; } = 0;
    public double? CurrentLatitude { get; set; }
    public double? CurrentLongitude { get; set; }
    public double Rating { get; set; } = 5.0;
    public int TotalDeliveries { get; set; } = 0;
    
    // Approval workflow
    public bool IsApproved { get; set; } = false;
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }
    
    // Document URLs for verification
    public string? IdCardFrontUrl { get; set; }
    public string? IdCardBackUrl { get; set; }
    public string? DriverLicenseUrl { get; set; }
    public string? VehicleRegistrationUrl { get; set; }
    
    // Navigation properties
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<DriverEarning> Earnings { get; set; } = new List<DriverEarning>();
}

