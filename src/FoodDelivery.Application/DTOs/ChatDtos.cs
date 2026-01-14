namespace FoodDelivery.Application.DTOs;

public class ChatMessageDto
{
    public Guid Id { get; set; }
    public Guid SenderId { get; set; }
    public bool IsFromCustomer { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SendMessageDto
{
    public string Content { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
}
