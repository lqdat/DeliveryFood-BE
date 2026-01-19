using FoodDelivery.Application.Common;
using FoodDelivery.Application.DTOs.Chat;
using FoodDelivery.Domain.Entities;
using FoodDelivery.Domain.Enums;
using FoodDelivery.Infrastructure.Data;
using FoodDelivery.API.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FoodDelivery.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IHubContext<ChatHub> _hubContext;

    public ChatController(AppDbContext context, IHubContext<ChatHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    private Guid? GetUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    /// <summary>
    /// Get list of active conversations (orders in progress)
    /// </summary>
    [HttpGet("conversations")]
    public async Task<ActionResult<ApiResponse<PagedResult<ChatConversationDto>>>> GetConversations(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized(ApiResponse<PagedResult<ChatConversationDto>>.ErrorResponse("Unauthorized"));

        // Find active orders involving the user
        // Order status: Confirmed -> Delivering (exclude Pending, Cancelled, Delivered for now, unless history is needed)
        // Adjust status filter based on requirements
        var activeStatuses = new[] 
        { 
            OrderStatus.Confirmed, 
            OrderStatus.Preparing, 
            OrderStatus.ReadyForPickup, 
            OrderStatus.PickedUp, 
            OrderStatus.Delivering 
        };

        var query = _context.Orders
            .Include(o => o.Customer).ThenInclude(c => c.User)
            .Include(o => o.Driver).ThenInclude(d => d!.User)
            .Include(o => o.Restaurant)
            .Where(o => !o.IsDeleted && activeStatuses.Contains(o.Status))
            .Where(o => o.Customer.UserId == userId || (o.Driver != null && o.Driver.UserId == userId));

        var totalCount = await query.CountAsync();

        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var conversations = new List<ChatConversationDto>();

        foreach (var order in orders)
        {
            // Determine participant (the other party)
            bool isCurrentUserCustomer = order.Customer.UserId == userId;
            
            var participantName = "Unknown";
            var participantAvatar = "";
            Guid participantId = Guid.Empty;

            if (isCurrentUserCustomer)
            {
                if (order.Driver != null)
                {
                    participantId = order.Driver.UserId;
                    participantName = order.Driver.User.FullName;
                    participantAvatar = order.Driver.User.AvatarUrl ?? "";
                }
                else
                {
                    // No driver yet
                    participantName = "Đang tìm tài xế...";
                }
            }
            else
            {
                // Current user is driver
                participantId = order.Customer.UserId;
                participantName = order.Customer.User.FullName;
                participantAvatar = order.Customer.User.AvatarUrl ?? "";
            }

            // Get last message info
            var lastMessage = await _context.ChatMessages
                .Where(m => m.OrderId == order.Id)
                .OrderByDescending(m => m.CreatedAt)
                .FirstOrDefaultAsync();

            var unreadCount = await _context.ChatMessages
                .Where(m => m.OrderId == order.Id && m.SenderId != userId && !m.IsRead)
                .CountAsync();

            conversations.Add(new ChatConversationDto
            {
                Id = order.Id,
                OrderId = order.Id,
                OrderStatus = order.Status.ToString(),
                ParticipantId = participantId,
                ParticipantName = participantName,
                ParticipantAvatar = participantAvatar,
                LastMessage = lastMessage?.Content ?? (lastMessage?.ImageUrl != null ? "[Image]" : null),
                LastMessageTime = lastMessage?.CreatedAt,
                UnreadCount = unreadCount
            });
        }

        return Ok(ApiResponse<PagedResult<ChatConversationDto>>.SuccessResponse(new PagedResult<ChatConversationDto>
        {
            Items = conversations,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        }));
    }

    /// <summary>
    /// Get messages for a specific order
    /// </summary>
    [HttpGet("{orderId}/messages")]
    public async Task<ActionResult<ApiResponse<PagedResult<ChatMessageDto>>>> GetMessages(
        Guid orderId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized(ApiResponse<PagedResult<ChatMessageDto>>.ErrorResponse("Unauthorized"));

        // Validate access
        var hasAccess = await _context.Orders
            .Include(o => o.Driver)
            .AnyAsync(o => o.Id == orderId && 
                          (o.Customer.UserId == userId || (o.Driver != null && o.Driver.UserId == userId)));

        if (!hasAccess)
            return Unauthorized(ApiResponse<PagedResult<ChatMessageDto>>.ErrorResponse("You do not have access to this chat"));

        var query = _context.ChatMessages
            .Where(m => m.OrderId == orderId)
            .OrderByDescending(m => m.CreatedAt); // Newest first for chat UI often

        var totalCount = await query.CountAsync();

        var messages = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Mark messages as read (if they are not from me)
        var unreadMessages = messages.Where(m => m.SenderId != userId && !m.IsRead).ToList();
        if (unreadMessages.Any())
        {
            foreach (var msg in unreadMessages)
            {
                msg.IsRead = true;
                msg.ReadAt = DateTime.UtcNow;
            }
            await _context.SaveChangesAsync();
        }

        var dtos = messages.Select(m => new ChatMessageDto
        {
            Id = m.Id,
            ConversationId = m.OrderId,
            SenderId = m.SenderId,
            Content = m.Content,
            ImageUrl = m.ImageUrl,
            CreatedAt = m.CreatedAt,
            IsRead = m.IsRead,
            IsMine = m.SenderId == userId
        }).ToList(); 
        
        return Ok(ApiResponse<PagedResult<ChatMessageDto>>.SuccessResponse(new PagedResult<ChatMessageDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        }));
    }

    /// <summary>
    /// Send a message (HTTP Fallback)
    /// </summary>
    [HttpPost("messages")]
    public async Task<ActionResult<ApiResponse<ChatMessageDto>>> SendMessage([FromBody] SendMessageRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized(ApiResponse<ChatMessageDto>.ErrorResponse("Unauthorized"));

        var order = await _context.Orders
            .Include(o => o.Driver)
            .Include(o => o.Customer)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId);

        if (order == null)
            return NotFound(ApiResponse<ChatMessageDto>.ErrorResponse("Order not found"));

        bool isCustomer = order.Customer.UserId == userId;
        bool isDriver = order.Driver != null && order.Driver.UserId == userId;

        if (!isCustomer && !isDriver)
            return Unauthorized(ApiResponse<ChatMessageDto>.ErrorResponse("You are not part of this order"));

        var message = new ChatMessage
        {
            OrderId = request.OrderId,
            SenderId = userId.Value,
            IsFromCustomer = isCustomer,
            Content = request.Content,
            ImageUrl = request.ImageUrl,
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };

        _context.ChatMessages.Add(message);
        await _context.SaveChangesAsync();

        var messageDto = new ChatMessageDto
        {
            Id = message.Id,
            ConversationId = message.OrderId,
            SenderId = message.SenderId,
            Content = message.Content,
            ImageUrl = message.ImageUrl,
            CreatedAt = message.CreatedAt,
            IsRead = message.IsRead,
            IsMine = true
        };

        // Push to SignalR group
        await _hubContext.Clients.Group($"order_{request.OrderId}").SendAsync("ReceiveMessage", new ChatMessageDto 
        {
            Id = message.Id,
            ConversationId = message.OrderId,
            SenderId = message.SenderId,
            Content = message.Content,
            ImageUrl = message.ImageUrl,
            CreatedAt = message.CreatedAt,
            IsRead = message.IsRead,
            IsMine = false // For receivers
        });

        return Ok(ApiResponse<ChatMessageDto>.SuccessResponse(messageDto));
    }
}
