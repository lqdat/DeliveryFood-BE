using FoodDelivery.Domain.Enums;

namespace FoodDelivery.Domain.Entities;

public class Order : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty; // #2491
    
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    
    public Guid RestaurantId { get; set; }
    public Restaurant Restaurant { get; set; } = null!;
    
    public Guid? DriverId { get; set; }
    public Driver? Driver { get; set; }
    
    public Guid? VoucherId { get; set; }
    public Voucher? Voucher { get; set; }
    
    // Delivery address
    public string DeliveryAddress { get; set; } = string.Empty;
    public double DeliveryLatitude { get; set; }
    public double DeliveryLongitude { get; set; }
    public string? DeliveryNote { get; set; }
    
    // Pricing
    public decimal Subtotal { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal DiscountAmount { get; set; } = 0;
    public decimal TotalAmount { get; set; }
    
    // Status
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    
    // Timestamps
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? PreparedAt { get; set; }
    public DateTime? PickedUpAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
    
    public int EstimatedDeliveryMinutes { get; set; }
    
    // Navigation properties
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<OrderTracking> TrackingHistory { get; set; } = new List<OrderTracking>();
    public ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
    public Review? Review { get; set; }
}
