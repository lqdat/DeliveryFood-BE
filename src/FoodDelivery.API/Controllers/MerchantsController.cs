using FoodDelivery.Application.Common;
using FoodDelivery.Application.DTOs.Merchant;
using FoodDelivery.Domain.Entities;
using FoodDelivery.Domain.Enums;
using FoodDelivery.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FoodDelivery.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Merchant")]
public class MerchantsController : ControllerBase
{
    private readonly AppDbContext _context;

    public MerchantsController(AppDbContext context)
    {
        _context = context;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("dashboard")]
    public async Task<ActionResult<ApiResponse<MerchantDashboardDto>>> GetDashboard()
    {
        var userId = GetUserId();
        var merchant = await _context.Merchants
            .Include(m => m.Restaurants)
            .FirstOrDefaultAsync(m => m.UserId == userId);

        if (merchant == null)
            return NotFound(ApiResponse<MerchantDashboardDto>.ErrorResponse("Merchant not found"));

        var restaurant = merchant.Restaurants.FirstOrDefault();
        if (restaurant == null)
            return NotFound(ApiResponse<MerchantDashboardDto>.ErrorResponse("No restaurant found"));

        var today = DateTime.UtcNow.Date;
        var weekStart = today.AddDays(-(int)today.DayOfWeek + 1);

        var todayOrders = await _context.Orders
            .Where(o => o.RestaurantId == restaurant.Id && o.CreatedAt.Date == today)
            .ToListAsync();

        var weeklyOrders = await _context.Orders
            .Where(o => o.RestaurantId == restaurant.Id && o.CreatedAt >= weekStart)
            .ToListAsync();

        var currentOrders = await _context.Orders
            .Include(o => o.Customer).ThenInclude(c => c.User)
            .Include(o => o.OrderItems)
            .Where(o => o.RestaurantId == restaurant.Id && 
                        o.Status != OrderStatus.Delivered && 
                        o.Status != OrderStatus.Cancelled)
            .OrderByDescending(o => o.CreatedAt)
            .Take(10)
            .ToListAsync();

        return Ok(ApiResponse<MerchantDashboardDto>.SuccessResponse(new MerchantDashboardDto
        {
            Restaurant = new RestaurantInfoDto
            {
                Id = restaurant.Id,
                Name = restaurant.Name,
                ImageUrl = restaurant.ImageUrl
            },
            IsOpen = restaurant.IsOpen,
            TodayRevenue = todayOrders.Where(o => o.Status == OrderStatus.Delivered).Sum(o => o.TotalAmount),
            WeeklyRevenue = weeklyOrders.Where(o => o.Status == OrderStatus.Delivered).Sum(o => o.TotalAmount),
            TodayOrderCount = todayOrders.Count,
            PendingOrderCount = currentOrders.Count(o => o.Status == OrderStatus.Pending),
            RevenueChangePercentage = 12.5, // Simplified
            CurrentOrders = currentOrders.Select(o => new MerchantOrderDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                Status = o.Status,
                StatusText = GetStatusText(o.Status),
                TotalAmount = o.TotalAmount,
                ItemCount = o.OrderItems.Count,
                IsDelivery = true,
                Customer = new CustomerInfoDto
                {
                    Name = o.Customer.User.FullName,
                    AvatarUrl = o.Customer.User.AvatarUrl
                },
                Items = o.OrderItems.Select(oi => new OrderItemSummaryDto
                {
                    ItemName = oi.ItemName,
                    Quantity = oi.Quantity
                }).ToList(),
                CreatedAt = o.CreatedAt
            }).ToList(),
            WeeklyChart = GetWeeklyChart(weeklyOrders, weekStart)
        }));
    }

    [HttpPut("status")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateStatus([FromBody] UpdateRestaurantStatusDto dto)
    {
        var userId = GetUserId();
        var merchant = await _context.Merchants
            .Include(m => m.Restaurants)
            .FirstOrDefaultAsync(m => m.UserId == userId);

        var restaurant = merchant?.Restaurants.FirstOrDefault();
        if (restaurant == null)
            return NotFound(ApiResponse<object>.ErrorResponse("Restaurant not found"));

        restaurant.IsOpen = dto.IsOpen;
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<object>.SuccessResponse(new { }, 
            dto.IsOpen ? "Cửa hàng đang mở" : "Cửa hàng đã đóng"));
    }

    [HttpGet("orders")]
    public async Task<ActionResult<ApiResponse<List<MerchantOrderDto>>>> GetOrders(
        [FromQuery] OrderStatus? status,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();
        var merchant = await _context.Merchants
            .Include(m => m.Restaurants)
            .FirstOrDefaultAsync(m => m.UserId == userId);

        var restaurant = merchant?.Restaurants.FirstOrDefault();
        if (restaurant == null)
            return NotFound(ApiResponse<List<MerchantOrderDto>>.ErrorResponse("Restaurant not found"));

        var query = _context.Orders
            .Include(o => o.Customer).ThenInclude(c => c.User)
            .Include(o => o.OrderItems)
            .Where(o => o.RestaurantId == restaurant.Id);

        if (status.HasValue)
            query = query.Where(o => o.Status == status.Value);

        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(ApiResponse<List<MerchantOrderDto>>.SuccessResponse(
            orders.Select(o => new MerchantOrderDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                Status = o.Status,
                StatusText = GetStatusText(o.Status),
                TotalAmount = o.TotalAmount,
                ItemCount = o.OrderItems.Count,
                IsDelivery = true,
                Customer = new CustomerInfoDto
                {
                    Name = o.Customer.User.FullName,
                    AvatarUrl = o.Customer.User.AvatarUrl
                },
                CreatedAt = o.CreatedAt
            }).ToList()));
    }

    [HttpPut("orders/{id}/accept")]
    public async Task<ActionResult<ApiResponse<object>>> AcceptOrder(Guid id)
    {
        var order = await GetMerchantOrder(id);
        if (order == null) return NotFound(ApiResponse<object>.ErrorResponse("Order not found"));

        order.Status = OrderStatus.Confirmed;
        order.ConfirmedAt = DateTime.UtcNow;
        order.TrackingHistory.Add(new OrderTracking
        {
            OrderId = order.Id,
            Status = OrderStatus.Confirmed,
            Description = "Nhà hàng đã xác nhận đơn hàng"
        });

        await _context.SaveChangesAsync();
        return Ok(ApiResponse<object>.SuccessResponse(new { }, "Đã nhận đơn hàng"));
    }

    [HttpPut("orders/{id}/ready")]
    public async Task<ActionResult<ApiResponse<object>>> MarkOrderReady(Guid id)
    {
        var order = await GetMerchantOrder(id);
        if (order == null) return NotFound(ApiResponse<object>.ErrorResponse("Order not found"));

        order.Status = OrderStatus.ReadyForPickup;
        order.PreparedAt = DateTime.UtcNow;
        order.TrackingHistory.Add(new OrderTracking
        {
            OrderId = order.Id,
            Status = OrderStatus.ReadyForPickup,
            Description = "Đơn hàng đã sẵn sàng giao"
        });

        await _context.SaveChangesAsync();
        return Ok(ApiResponse<object>.SuccessResponse(new { }, "Đơn hàng sẵn sàng giao"));
    }

    [HttpGet("menu")]
    public async Task<ActionResult<ApiResponse<List<MenuItemManageDto>>>> GetMenu()
    {
        var userId = GetUserId();
        var merchant = await _context.Merchants
            .Include(m => m.Restaurants)
            .FirstOrDefaultAsync(m => m.UserId == userId);

        var restaurant = merchant?.Restaurants.FirstOrDefault();
        if (restaurant == null)
            return NotFound(ApiResponse<List<MenuItemManageDto>>.ErrorResponse("Restaurant not found"));

        var items = await _context.MenuItems
            .Include(mi => mi.MenuCategory)
            .Where(mi => mi.MenuCategory.RestaurantId == restaurant.Id && !mi.IsDeleted)
            .OrderBy(mi => mi.MenuCategory.DisplayOrder)
            .ThenBy(mi => mi.DisplayOrder)
            .Select(mi => new MenuItemManageDto
            {
                Id = mi.Id,
                CategoryId = mi.MenuCategoryId,
                CategoryName = mi.MenuCategory.Name,
                Name = mi.Name,
                NameSecondary = mi.NameSecondary,
                Description = mi.Description,
                ImageUrl = mi.ImageUrl,
                Price = mi.Price,
                OriginalPrice = mi.OriginalPrice,
                IsAvailable = mi.IsAvailable,
                IsPopular = mi.IsPopular,
                DisplayOrder = mi.DisplayOrder
            })
            .ToListAsync();

        return Ok(ApiResponse<List<MenuItemManageDto>>.SuccessResponse(items));
    }

    [HttpPost("menu/items")]
    public async Task<ActionResult<ApiResponse<MenuItemManageDto>>> CreateMenuItem([FromBody] CreateMenuItemDto dto)
    {
        var category = await _context.MenuCategories
            .Include(mc => mc.Restaurant)
            .FirstOrDefaultAsync(mc => mc.Id == dto.CategoryId);

        if (category == null)
            return NotFound(ApiResponse<MenuItemManageDto>.ErrorResponse("Category not found"));

        var item = new MenuItem
        {
            MenuCategoryId = dto.CategoryId,
            Name = dto.Name,
            NameSecondary = dto.NameSecondary,
            Description = dto.Description,
            ImageUrl = dto.ImageUrl,
            Price = dto.Price,
            OriginalPrice = dto.OriginalPrice,
            IsAvailable = dto.IsAvailable,
            IsPopular = dto.IsPopular
        };

        _context.MenuItems.Add(item);
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<MenuItemManageDto>.SuccessResponse(new MenuItemManageDto
        {
            Id = item.Id,
            Name = item.Name,
            Price = item.Price
        }, "Đã thêm món mới"));
    }

    [HttpPut("menu/items/{id}")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateMenuItem(Guid id, [FromBody] UpdateMenuItemDto dto)
    {
        var item = await _context.MenuItems.FindAsync(id);
        if (item == null)
            return NotFound(ApiResponse<object>.ErrorResponse("Item not found"));

        if (dto.Name != null) item.Name = dto.Name;
        if (dto.NameSecondary != null) item.NameSecondary = dto.NameSecondary;
        if (dto.Description != null) item.Description = dto.Description;
        if (dto.ImageUrl != null) item.ImageUrl = dto.ImageUrl;
        if (dto.Price.HasValue) item.Price = dto.Price.Value;
        if (dto.OriginalPrice.HasValue) item.OriginalPrice = dto.OriginalPrice;
        if (dto.IsAvailable.HasValue) item.IsAvailable = dto.IsAvailable.Value;
        if (dto.IsPopular.HasValue) item.IsPopular = dto.IsPopular.Value;
        if (dto.DisplayOrder.HasValue) item.DisplayOrder = dto.DisplayOrder.Value;

        await _context.SaveChangesAsync();
        return Ok(ApiResponse<object>.SuccessResponse(new { }, "Đã cập nhật món"));
    }

    [HttpDelete("menu/items/{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteMenuItem(Guid id)
    {
        var item = await _context.MenuItems.FindAsync(id);
        if (item == null)
            return NotFound(ApiResponse<object>.ErrorResponse("Item not found"));

        item.IsDeleted = true;
        await _context.SaveChangesAsync();
        return Ok(ApiResponse<object>.SuccessResponse(new { }, "Đã xóa món"));
    }

    private async Task<Order?> GetMerchantOrder(Guid orderId)
    {
        var userId = GetUserId();
        var merchant = await _context.Merchants
            .Include(m => m.Restaurants)
            .FirstOrDefaultAsync(m => m.UserId == userId);

        var restaurantId = merchant?.Restaurants.FirstOrDefault()?.Id;
        return await _context.Orders
            .Include(o => o.TrackingHistory)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.RestaurantId == restaurantId);
    }

    private static string GetStatusText(OrderStatus status) => status switch
    {
        OrderStatus.Pending => "Mới",
        OrderStatus.Confirmed => "Đã xác nhận",
        OrderStatus.Preparing => "Đang chuẩn bị",
        OrderStatus.ReadyForPickup => "Sẵn sàng",
        _ => status.ToString()
    };

    private static WeeklyRevenueChartDto GetWeeklyChart(List<Order> orders, DateTime weekStart)
    {
        var days = new[] { "T2", "T3", "T4", "T5", "T6", "T7", "CN" };
        var dailyTotals = new decimal[7];

        foreach (var o in orders.Where(o => o.Status == OrderStatus.Delivered))
        {
            var dayIndex = ((int)o.CreatedAt.DayOfWeek + 6) % 7;
            dailyTotals[dayIndex] += o.TotalAmount;
        }

        var maxAmount = dailyTotals.Max();

        return new WeeklyRevenueChartDto
        {
            Days = days.Select((d, i) => new DailyRevenueDto
            {
                DayName = d,
                Amount = dailyTotals[i],
                Percentage = maxAmount > 0 ? (double)(dailyTotals[i] / maxAmount * 100) : 0
            }).ToList()
        };
    }
}
