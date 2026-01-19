using FoodDelivery.Domain.Enums;

namespace FoodDelivery.Application.DTOs.Chat;

public class ChatMessageDto
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; } // Can be OrderId
    public Guid SenderId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
    public bool IsMine { get; set; }
}

public class ChatConversationDto
{
    public Guid Id { get; set; } // Conversation ID (usually OrderId)
    public Guid OrderId { get; set; }
    public string OrderStatus { get; set; } = string.Empty;
    
    public Guid ParticipantId { get; set; }
    public string ParticipantName { get; set; } = string.Empty;
    public string? ParticipantAvatar { get; set; }
    
    public string? LastMessage { get; set; }
    public DateTime? LastMessageTime { get; set; }
    public int UnreadCount { get; set; }
}

public class SendMessageRequest
{
    public Guid OrderId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
}
