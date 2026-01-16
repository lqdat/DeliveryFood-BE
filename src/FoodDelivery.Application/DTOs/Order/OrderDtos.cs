using System.ComponentModel.DataAnnotations;
using FoodDelivery.Domain.Enums;

namespace FoodDelivery.Application.DTOs.Order;

public class CreateOrderDto
{
    [Required]
    public Guid RestaurantId { get; set; }
    
    [Required]
    public string DeliveryAddress { get; set; } = string.Empty;
    
    public double DeliveryLatitude { get; set; }
    public double DeliveryLongitude { get; set; }
    public string? DeliveryNote { get; set; }
    
    [Required]
    public List<CreateOrderItemDto> Items { get; set; } = new();
    
    public Guid? VoucherId { get; set; }
    
    [Required]
    public PaymentMethod PaymentMethod { get; set; }
}

public class CreateOrderItemDto
{
    [Required]
    public Guid MenuItemId { get; set; }
    
    [Range(1, 100)]
    public int Quantity { get; set; } = 1;
    
    public string? Notes { get; set; }
    public string? SelectedOptions { get; set; }
}

public class OrderDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public string StatusText { get; set; } = string.Empty;
    
    // Restaurant
    public RestaurantSummaryDto Restaurant { get; set; } = null!;
    
    // Driver (null if not yet assigned)
    public DriverSummaryDto? Driver { get; set; }
    
    // Pricing
    public decimal Subtotal { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    
    // Payment
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    
    // Delivery
    public string DeliveryAddress { get; set; } = string.Empty;
    public double DeliveryLatitude { get; set; }
    public double DeliveryLongitude { get; set; }
    public string? DeliveryNote { get; set; }
    public int EstimatedDeliveryMinutes { get; set; }
    
    // Items
    public List<OrderItemDto> Items { get; set; } = new();
    
    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? PreparedAt { get; set; }
    public DateTime? PickedUpAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
}


public class OrderItemDto
{
    public Guid Id { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
    public string? Notes { get; set; }
}

public class RestaurantSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string Address { get; set; } = string.Empty;
}

public class DriverSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string VehicleType { get; set; } = string.Empty;
    public string VehiclePlate { get; set; } = string.Empty;
    public string VehicleBrand { get; set; } = string.Empty;
    public double Rating { get; set; }
    public string? PhoneNumber { get; set; }
}

public class OrderTrackingDto
{
    public OrderStatus Status { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public string? StatusTextSecondary { get; set; }
    public int EstimatedMinutes { get; set; }
    public List<TrackingStepDto> Steps { get; set; } = new();
    public DriverLocationDto? DriverLocation { get; set; }
}

public class TrackingStepDto
{
    public OrderStatus Status { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public bool IsActive { get; set; }
    public double Progress { get; set; } // 0-100
    public DateTime? Timestamp { get; set; }
}

public class DriverLocationDto
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ReviewOrderDto
{
    [Range(1, 5)]
    public int DriverRating { get; set; }
    public string? DriverComment { get; set; }
    public List<string>? DriverTags { get; set; }
    
    [Range(1, 5)]
    public int FoodRating { get; set; }
    public string? FoodComment { get; set; }
    
    public List<string>? ImageUrls { get; set; }
}
