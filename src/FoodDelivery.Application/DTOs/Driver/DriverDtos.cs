using FoodDelivery.Domain.Enums;

namespace FoodDelivery.Application.DTOs.Driver;

public class DriverProfileDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string VehicleType { get; set; } = string.Empty;
    public string VehiclePlate { get; set; } = string.Empty;
    public string VehicleBrand { get; set; } = string.Empty;
    public DriverStatus Status { get; set; }
    public decimal WalletBalance { get; set; }
    public double Rating { get; set; }
    public int TotalDeliveries { get; set; }
}

public class UpdateDriverStatusDto
{
    public DriverStatus Status { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}

public class JobRequestDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    
    // Restaurant
    public string RestaurantName { get; set; } = string.Empty;
    public string? RestaurantImageUrl { get; set; }
    public string RestaurantAddress { get; set; } = string.Empty;
    public string? RestaurantCategory { get; set; }
    
    // Delivery
    public string DeliveryAddress { get; set; } = string.Empty;
    public double PickupDistanceKm { get; set; }
    public double TotalDistanceKm { get; set; }
    public int EstimatedMinutes { get; set; }
    
    // Earnings
    public decimal EstimatedEarnings { get; set; }
    public bool HasTip { get; set; }
    public bool IsHighDemand { get; set; }
    
    // Items
    public int ItemCount { get; set; }
    
    public int TimeoutSeconds { get; set; } = 30;
}

public class WalletDto
{
    public decimal Balance { get; set; }
    public decimal WeeklyEarnings { get; set; }
    public decimal TodayEarnings { get; set; }
    public List<EarningDto> RecentEarnings { get; set; } = new();
    public WeeklyChartDto WeeklyChart { get; set; } = null!;
}

public class EarningDto
{
    public Guid Id { get; set; }
    public EarningType Type { get; set; }
    public string TypeText { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public string? OrderNumber { get; set; }
    public DateTime EarnedAt { get; set; }
}

public class WeeklyChartDto
{
    public List<DailyEarningDto> Days { get; set; } = new();
}

public class DailyEarningDto
{
    public string DayName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public double Percentage { get; set; } // 0-100 for chart
}

public class WithdrawDto
{
    public decimal Amount { get; set; }
    public string BankAccount { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
}
