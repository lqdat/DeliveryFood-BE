using FoodDelivery.Application.Common;
using FoodDelivery.Application.DTOs.Notification;
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
public class NotificationsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationsController(AppDbContext context, IHubContext<NotificationHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    private Guid? GetUserId()
    {
        var userIdClaim = User.FindFirst("UserId")?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    /// <summary>
    /// Get notifications for the current user (paginated)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<NotificationDto>>>> GetNotifications(
        [FromQuery] bool? unreadOnly,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized(ApiResponse<PagedResult<NotificationDto>>.ErrorResponse("Unauthorized"));

        var query = _context.Notifications
            .Where(n => n.UserId == userId && !n.IsDeleted)
            .AsQueryable();

        if (unreadOnly == true)
        {
            query = query.Where(n => !n.IsRead);
        }

        var totalCount = await query.CountAsync();

        var notifications = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                Type = n.Type,
                ReferenceId = n.ReferenceId,
                Data = n.Data,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            })
            .ToListAsync();

        return Ok(ApiResponse<PagedResult<NotificationDto>>.SuccessResponse(new PagedResult<NotificationDto>
        {
            Items = notifications,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        }));
    }

    /// <summary>
    /// Get unread notification count
    /// </summary>
    [HttpGet("unread-count")]
    public async Task<ActionResult<ApiResponse<int>>> GetUnreadCount()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized(ApiResponse<int>.ErrorResponse("Unauthorized"));

        var count = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead && !n.IsDeleted)
            .CountAsync();

        return Ok(ApiResponse<int>.SuccessResponse(count));
    }

    /// <summary>
    /// Mark a notification as read
    /// </summary>
    [HttpPut("{id}/read")]
    public async Task<ActionResult<ApiResponse<object>>> MarkAsRead(Guid id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

        if (notification == null)
            return NotFound(ApiResponse<object>.ErrorResponse("Notification not found"));

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<object>.SuccessResponse(new { }, "Marked as read"));
    }

    /// <summary>
    /// Mark all notifications as read
    /// </summary>
    [HttpPut("read-all")]
    public async Task<ActionResult<ApiResponse<object>>> MarkAllAsRead()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var unreadNotifications = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead && !n.IsDeleted)
            .ToListAsync();

        foreach (var notification in unreadNotifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return Ok(ApiResponse<object>.SuccessResponse(new { count = unreadNotifications.Count }, "All notifications marked as read"));
    }

    /// <summary>
    /// Delete a notification
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteNotification(Guid id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

        if (notification == null)
            return NotFound(ApiResponse<object>.ErrorResponse("Notification not found"));

        notification.IsDeleted = true;
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<object>.SuccessResponse(new { }, "Notification deleted"));
    }

    /// <summary>
    /// Send a notification to a user (for internal use or admin)
    /// This also pushes the notification via SignalR in real-time
    /// </summary>
    [HttpPost("send")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<NotificationDto>>> SendNotification([FromBody] CreateNotificationDto dto)
    {
        var notification = new Notification
        {
            UserId = dto.UserId,
            Title = dto.Title,
            Message = dto.Message,
            Type = dto.Type,
            ReferenceId = dto.ReferenceId,
            Data = dto.Data
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        var notificationDto = new NotificationDto
        {
            Id = notification.Id,
            Title = notification.Title,
            Message = notification.Message,
            Type = notification.Type,
            ReferenceId = notification.ReferenceId,
            Data = notification.Data,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt
        };

        // Push notification via SignalR
        await _hubContext.Clients.Group($"user_{dto.UserId}").SendAsync("ReceiveNotification", notificationDto);

        return Ok(ApiResponse<NotificationDto>.SuccessResponse(notificationDto, "Notification sent"));
    }
}
