namespace FoodDelivery.Domain.Entities;

public class ChatMessage : BaseEntity
{
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
    
    public Guid SenderId { get; set; }
    public bool IsFromCustomer { get; set; }
    
    public string Content { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }
}
