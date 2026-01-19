using FoodDelivery.Domain.Enums;

namespace FoodDelivery.Domain.Entities;

public class Notification : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    
    public NotificationType Type { get; set; } = NotificationType.General;
    
    // Optional reference to related entity (e.g., OrderId)
    public Guid? ReferenceId { get; set; }
    
    // Additional data as JSON string
    public string? Data { get; set; }
    
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }
}
