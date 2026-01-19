using FoodDelivery.Application.DTOs.Chat;
using FoodDelivery.Domain.Entities;
using FoodDelivery.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FoodDelivery.API.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly AppDbContext _context;

    public ChatHub(AppDbContext context)
    {
        _context = context;
    }

    private Guid? GetUserId()
    {
        var userId = Context.User?.FindFirst("UserId")?.Value;
        return Guid.TryParse(userId, out var id) ? id : null;
    }

    /// <summary>
    /// Join the chat group for a specific order
    /// </summary>
    public async Task JoinOrderGroup(string orderId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"order_{orderId}");
    }

    /// <summary>
    /// Leave the chat group for a specific order
    /// </summary>
    public async Task LeaveOrderGroup(string orderId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"order_{orderId}");
    }

    /// <summary>
    /// Send a message to the order chat group
    /// </summary>
    public async Task SendMessage(string orderIdStr, string content)
    {
        var userId = GetUserId();
        if (userId == null) throw new HubException("Unauthorized");

        if (!Guid.TryParse(orderIdStr, out var orderId))
            throw new HubException("Invalid Order ID");

        // Validate order participation
        var order = await _context.Orders
            .Include(o => o.Driver)
            .Include(o => o.Customer)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null) throw new HubException("Order not found");

        // Determine if sender is customer or driver
        // Note: In real app, check user role vs sender. Assuming customer for now if matches customer ID
        // Or get from User table directly
        var user = await _context.Users.FindAsync(userId);
        if (user == null) throw new HubException("User not found");

        // Simple validation: Ensure user is part of the order
        // This logic might need strict role checking
        bool isCustomer = order.Customer != null && order.Customer.UserId == userId;
        bool isDriver = order.Driver != null && order.Driver.UserId == userId;

        if (!isCustomer && !isDriver)
            throw new HubException("You are not part of this order");

        var message = new ChatMessage
        {
            OrderId = orderId,
            SenderId = userId.Value,
            IsFromCustomer = isCustomer,
            Content = content,
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
            IsMine = false // Will be determined by client
        };

        // Broadcast to group (including sender, client handles deduplication if needed, or exclude sender)
        await Clients.Group($"order_{orderIdStr}").SendAsync("ReceiveMessage", messageDto);
    }
}
