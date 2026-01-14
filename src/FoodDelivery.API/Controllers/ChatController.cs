using FoodDelivery.Application.Common;
using FoodDelivery.Application.DTOs;
using FoodDelivery.Domain.Entities;
using FoodDelivery.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FoodDelivery.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly AppDbContext _context;

    public ChatController(AppDbContext context)
    {
        _context = context;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("orders/{orderId}/messages")]
    public async Task<ActionResult<ApiResponse<List<ChatMessageDto>>>> GetMessages(Guid orderId)
    {
        var userId = GetUserId();
        
        // Verify user has access to this order
        var order = await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Driver)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
            return NotFound(ApiResponse<List<ChatMessageDto>>.ErrorResponse("Order not found"));

        var isCustomer = order.Customer.UserId == userId;
        var isDriver = order.Driver?.UserId == userId;

        if (!isCustomer && !isDriver)
            return Forbid();

        var messages = await _context.ChatMessages
            .Where(m => m.OrderId == orderId)
            .OrderBy(m => m.CreatedAt)
            .Select(m => new ChatMessageDto
            {
                Id = m.Id,
                SenderId = m.SenderId,
                IsFromCustomer = m.IsFromCustomer,
                Content = m.Content,
                ImageUrl = m.ImageUrl,
                IsRead = m.IsRead,
                CreatedAt = m.CreatedAt
            })
            .ToListAsync();

        // Mark messages as read
        var unreadMessages = await _context.ChatMessages
            .Where(m => m.OrderId == orderId && !m.IsRead && m.IsFromCustomer != isCustomer)
            .ToListAsync();

        unreadMessages.ForEach(m =>
        {
            m.IsRead = true;
            m.ReadAt = DateTime.UtcNow;
        });

        await _context.SaveChangesAsync();

        return Ok(ApiResponse<List<ChatMessageDto>>.SuccessResponse(messages));
    }

    [HttpPost("orders/{orderId}/messages")]
    public async Task<ActionResult<ApiResponse<ChatMessageDto>>> SendMessage(
        Guid orderId, 
        [FromBody] SendMessageDto dto)
    {
        var userId = GetUserId();
        var user = await _context.Users.FindAsync(userId);
        
        var order = await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Driver)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
            return NotFound(ApiResponse<ChatMessageDto>.ErrorResponse("Order not found"));

        var isCustomer = order.Customer.UserId == userId;
        var isDriver = order.Driver?.UserId == userId;

        if (!isCustomer && !isDriver)
            return Forbid();

        var message = new ChatMessage
        {
            OrderId = orderId,
            SenderId = userId,
            IsFromCustomer = isCustomer,
            Content = dto.Content,
            ImageUrl = dto.ImageUrl
        };

        _context.ChatMessages.Add(message);
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<ChatMessageDto>.SuccessResponse(new ChatMessageDto
        {
            Id = message.Id,
            SenderId = message.SenderId,
            IsFromCustomer = message.IsFromCustomer,
            Content = message.Content,
            ImageUrl = message.ImageUrl,
            IsRead = false,
            CreatedAt = message.CreatedAt
        }));
    }
}
