using FoodDelivery.Application.Common;
using FoodDelivery.Application.DTOs.Order;
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
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _context;

    public OrdersController(AppDbContext context)
    {
        _context = context;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<OrderDto>>>> GetOrders(
        [FromQuery] OrderStatus? status,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
        
        if (customer == null)
            return NotFound(ApiResponse<PagedResult<OrderDto>>.ErrorResponse("Customer not found"));

        var query = _context.Orders
            .Include(o => o.Restaurant)
            .Include(o => o.Driver).ThenInclude(d => d!.User)
            .Include(o => o.OrderItems)
            .Where(o => o.CustomerId == customer.Id && !o.IsDeleted);

        if (status.HasValue)
        {
            query = query.Where(o => o.Status == status.Value);
        }

        var totalCount = await query.CountAsync();
        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var orderDtos = orders.Select(MapToOrderDto).ToList();

        return Ok(ApiResponse<PagedResult<OrderDto>>.SuccessResponse(new PagedResult<OrderDto>
        {
            Items = orderDtos,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        }));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> GetOrder(Guid id)
    {
        var userId = GetUserId();
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);

        var order = await _context.Orders
            .Include(o => o.Restaurant)
            .Include(o => o.Driver).ThenInclude(d => d!.User)
            .Include(o => o.OrderItems).ThenInclude(oi => oi.MenuItem)
            .FirstOrDefaultAsync(o => o.Id == id && o.CustomerId == customer!.Id);

        if (order == null)
            return NotFound(ApiResponse<OrderDto>.ErrorResponse("Không tìm thấy đơn hàng"));

        return Ok(ApiResponse<OrderDto>.SuccessResponse(MapToOrderDto(order)));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<OrderDto>>> CreateOrder([FromBody] CreateOrderDto dto)
    {
        var userId = GetUserId();
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
        
        if (customer == null)
            return NotFound(ApiResponse<OrderDto>.ErrorResponse("Customer not found"));

        var restaurant = await _context.Restaurants.FindAsync(dto.RestaurantId);
        if (restaurant == null || !restaurant.IsOpen)
            return BadRequest(ApiResponse<OrderDto>.ErrorResponse("Nhà hàng không khả dụng"));

        // Get menu items
        var menuItemIds = dto.Items.Select(i => i.MenuItemId).ToList();
        var menuItems = await _context.MenuItems
            .Where(mi => menuItemIds.Contains(mi.Id) && mi.IsAvailable)
            .ToDictionaryAsync(mi => mi.Id);

        if (menuItems.Count != menuItemIds.Count)
            return BadRequest(ApiResponse<OrderDto>.ErrorResponse("Một số món không khả dụng"));

        // Calculate totals
        decimal subtotal = 0;
        var orderItems = new List<OrderItem>();

        foreach (var item in dto.Items)
        {
            var menuItem = menuItems[item.MenuItemId];
            var totalPrice = menuItem.Price * item.Quantity;
            subtotal += totalPrice;

            orderItems.Add(new OrderItem
            {
                MenuItemId = menuItem.Id,
                ItemName = menuItem.Name,
                UnitPrice = menuItem.Price,
                Quantity = item.Quantity,
                TotalPrice = totalPrice,
                Notes = item.Notes,
                SelectedOptions = item.SelectedOptions
            });
        }

        decimal discountAmount = 0;
        Voucher? voucher = null;
        if (dto.VoucherId.HasValue)
        {
            voucher = await _context.Vouchers.FindAsync(dto.VoucherId.Value);
            if (voucher != null && voucher.IsActive && subtotal >= voucher.MinOrderAmount)
            {
                discountAmount = voucher.Type switch
                {
                    VoucherType.Percentage => Math.Min(subtotal * voucher.DiscountValue / 100, voucher.MaxDiscountAmount),
                    VoucherType.FixedAmount => Math.Min(voucher.DiscountValue, voucher.MaxDiscountAmount),
                    VoucherType.FreeShipping => restaurant.DeliveryFee,
                    _ => 0
                };
            }
        }

        var deliveryFee = voucher?.Type == VoucherType.FreeShipping ? 0 : restaurant.DeliveryFee;
        var totalAmount = subtotal + deliveryFee - discountAmount;

        // Generate order number
        var orderNumber = $"#{DateTime.UtcNow:yyMMdd}{new Random().Next(1000, 9999)}";

        var order = new Order
        {
            OrderNumber = orderNumber,
            CustomerId = customer.Id,
            RestaurantId = restaurant.Id,
            VoucherId = dto.VoucherId,
            DeliveryAddress = dto.DeliveryAddress,
            DeliveryLatitude = dto.DeliveryLatitude,
            DeliveryLongitude = dto.DeliveryLongitude,
            DeliveryNote = dto.DeliveryNote,
            Subtotal = subtotal,
            DeliveryFee = deliveryFee,
            DiscountAmount = discountAmount,
            TotalAmount = totalAmount,
            PaymentMethod = dto.PaymentMethod,
            EstimatedDeliveryMinutes = restaurant.EstimatedDeliveryMinutes,
            OrderItems = orderItems
        };

        // Add initial tracking
        order.TrackingHistory.Add(new OrderTracking
        {
            OrderId = order.Id,
            Status = OrderStatus.Pending,
            Description = "Đơn hàng đã được tạo",
            DescriptionSecondary = "Order created"
        });

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Reload with includes
        order = await _context.Orders
            .Include(o => o.Restaurant)
            .Include(o => o.OrderItems)
            .FirstAsync(o => o.Id == order.Id);

        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, 
            ApiResponse<OrderDto>.SuccessResponse(MapToOrderDto(order), "Đặt hàng thành công"));
    }

    [HttpGet("{id}/tracking")]
    public async Task<ActionResult<ApiResponse<OrderTrackingDto>>> GetOrderTracking(Guid id)
    {
        var userId = GetUserId();
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);

        var order = await _context.Orders
            .Include(o => o.TrackingHistory)
            .Include(o => o.Driver)
            .FirstOrDefaultAsync(o => o.Id == id && o.CustomerId == customer!.Id);

        if (order == null)
            return NotFound(ApiResponse<OrderTrackingDto>.ErrorResponse("Không tìm thấy đơn hàng"));

        var allStatuses = new[] 
        { 
            OrderStatus.Confirmed, 
            OrderStatus.Preparing, 
            OrderStatus.Delivering, 
            OrderStatus.Delivered 
        };

        var currentIndex = Array.IndexOf(allStatuses, order.Status);

        var tracking = new OrderTrackingDto
        {
            Status = order.Status,
            StatusText = GetStatusText(order.Status),
            StatusTextSecondary = GetStatusTextSecondary(order.Status),
            EstimatedMinutes = order.EstimatedDeliveryMinutes,
            Steps = allStatuses.Select((s, i) => new TrackingStepDto
            {
                Status = s,
                StatusText = GetStatusText(s),
                IsCompleted = i < currentIndex,
                IsActive = i == currentIndex,
                Progress = i < currentIndex ? 100 : (i == currentIndex ? 66 : 0),
                Timestamp = order.TrackingHistory.FirstOrDefault(t => t.Status == s)?.Timestamp
            }).ToList(),
            DriverLocation = order.Driver != null ? new DriverLocationDto
            {
                Latitude = order.Driver.CurrentLatitude ?? 0,
                Longitude = order.Driver.CurrentLongitude ?? 0,
                UpdatedAt = DateTime.UtcNow
            } : null
        };

        return Ok(ApiResponse<OrderTrackingDto>.SuccessResponse(tracking));
    }

    [HttpPut("{id}/cancel")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> CancelOrder(Guid id, [FromBody] string? reason)
    {
        var userId = GetUserId();
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);

        var order = await _context.Orders
            .Include(o => o.Restaurant)
            .FirstOrDefaultAsync(o => o.Id == id && o.CustomerId == customer!.Id);

        if (order == null)
            return NotFound(ApiResponse<OrderDto>.ErrorResponse("Không tìm thấy đơn hàng"));

        if (order.Status >= OrderStatus.Preparing)
            return BadRequest(ApiResponse<OrderDto>.ErrorResponse("Không thể hủy đơn hàng đang được chuẩn bị"));

        order.Status = OrderStatus.Cancelled;
        order.CancelledAt = DateTime.UtcNow;
        order.CancellationReason = reason;

        await _context.SaveChangesAsync();

        return Ok(ApiResponse<OrderDto>.SuccessResponse(MapToOrderDto(order), "Đã hủy đơn hàng"));
    }

    [HttpPost("{id}/review")]
    public async Task<ActionResult<ApiResponse<object>>> ReviewOrder(Guid id, [FromBody] ReviewOrderDto dto)
    {
        var userId = GetUserId();
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);

        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id && o.CustomerId == customer!.Id);

        if (order == null)
            return NotFound(ApiResponse<object>.ErrorResponse("Không tìm thấy đơn hàng"));

        if (order.Status != OrderStatus.Delivered)
            return BadRequest(ApiResponse<object>.ErrorResponse("Chỉ có thể đánh giá đơn hàng đã giao"));

        var existingReview = await _context.Reviews.AnyAsync(r => r.OrderId == id);
        if (existingReview)
            return BadRequest(ApiResponse<object>.ErrorResponse("Đơn hàng đã được đánh giá"));

        var review = new Review
        {
            OrderId = id,
            CustomerId = customer!.Id,
            DriverRating = dto.DriverRating,
            DriverComment = dto.DriverComment,
            DriverTags = dto.DriverTags != null ? string.Join(",", dto.DriverTags) : null,
            FoodRating = dto.FoodRating,
            FoodComment = dto.FoodComment,
            ImageUrls = dto.ImageUrls != null ? string.Join(",", dto.ImageUrls) : null
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<object>.SuccessResponse(new { }, "Cảm ơn bạn đã đánh giá!"));
    }

    private OrderDto MapToOrderDto(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            Status = order.Status,
            StatusText = GetStatusText(order.Status),
            Restaurant = new RestaurantSummaryDto
            {
                Id = order.Restaurant.Id,
                Name = order.Restaurant.Name,
                ImageUrl = order.Restaurant.ImageUrl,
                Address = order.Restaurant.Address
            },
            Driver = order.Driver != null ? new DriverSummaryDto
            {
                Id = order.Driver.Id,
                Name = order.Driver.User.FullName,
                AvatarUrl = order.Driver.User.AvatarUrl,
                VehicleType = order.Driver.VehicleType,
                VehiclePlate = order.Driver.VehiclePlate,
                VehicleBrand = order.Driver.VehicleBrand,
                Rating = order.Driver.Rating,
                PhoneNumber = order.Driver.User.PhoneNumber
            } : null,
            Subtotal = order.Subtotal,
            DeliveryFee = order.DeliveryFee,
            DiscountAmount = order.DiscountAmount,
            TotalAmount = order.TotalAmount,
            PaymentMethod = order.PaymentMethod,
            PaymentStatus = order.PaymentStatus,
            DeliveryAddress = order.DeliveryAddress,
            EstimatedDeliveryMinutes = order.EstimatedDeliveryMinutes,
            Items = order.OrderItems.Select(oi => new OrderItemDto
            {
                Id = oi.Id,
                ItemName = oi.ItemName,
                ImageUrl = oi.MenuItem?.ImageUrl,
                UnitPrice = oi.UnitPrice,
                Quantity = oi.Quantity,
                TotalPrice = oi.TotalPrice,
                Notes = oi.Notes
            }).ToList(),
            CreatedAt = order.CreatedAt,
            DeliveredAt = order.DeliveredAt
        };
    }

    private static string GetStatusText(OrderStatus status) => status switch
    {
        OrderStatus.Pending => "Chờ xác nhận",
        OrderStatus.Confirmed => "Đã xác nhận",
        OrderStatus.Preparing => "Đang chuẩn bị",
        OrderStatus.ReadyForPickup => "Sẵn sàng giao",
        OrderStatus.PickedUp => "Đã lấy hàng",
        OrderStatus.Delivering => "Đang giao",
        OrderStatus.Delivered => "Đã giao",
        OrderStatus.Cancelled => "Đã hủy",
        _ => "Unknown"
    };

    private static string GetStatusTextSecondary(OrderStatus status) => status switch
    {
        OrderStatus.Pending => "Pending confirmation",
        OrderStatus.Confirmed => "Confirmed",
        OrderStatus.Preparing => "Preparing",
        OrderStatus.ReadyForPickup => "Ready for pickup",
        OrderStatus.PickedUp => "Picked up",
        OrderStatus.Delivering => "On the way",
        OrderStatus.Delivered => "Delivered",
        OrderStatus.Cancelled => "Cancelled",
        _ => "Unknown"
    };
}
