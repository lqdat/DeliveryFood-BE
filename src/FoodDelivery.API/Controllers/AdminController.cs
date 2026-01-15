using FoodDelivery.Application.Common;
using FoodDelivery.Application.Interfaces;
using FoodDelivery.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAppDbContext _context;

    public AdminController(IAppDbContext context)
    {
        _context = context;
    }

    // ==================== DASHBOARD ====================

    /// <summary>
    /// Get admin dashboard overview
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var today = DateTime.UtcNow.Date;
        var weekStart = today.AddDays(-(int)today.DayOfWeek);
        var monthStart = new DateTime(today.Year, today.Month, 1);

        // Revenue calculations
        var todayRevenue = await _context.Orders
            .Where(o => o.CreatedAt.Date == today && o.Status == OrderStatus.Delivered)
            .SumAsync(o => o.TotalAmount);

        var weeklyRevenue = await _context.Orders
            .Where(o => o.CreatedAt >= weekStart && o.Status == OrderStatus.Delivered)
            .SumAsync(o => o.TotalAmount);

        var monthlyRevenue = await _context.Orders
            .Where(o => o.CreatedAt >= monthStart && o.Status == OrderStatus.Delivered)
            .SumAsync(o => o.TotalAmount);

        // Order statistics
        var activeOrders = await _context.Orders
            .Where(o => o.Status != OrderStatus.Delivered && o.Status != OrderStatus.Cancelled)
            .CountAsync();

        var completedToday = await _context.Orders
            .Where(o => o.CreatedAt.Date == today && o.Status == OrderStatus.Delivered)
            .CountAsync();

        // Pending approvals
        var pendingDrivers = await _context.Drivers
            .Where(d => !d.IsApproved && !d.IsDeleted)
            .CountAsync();

        var pendingMerchants = await _context.Merchants
            .Where(m => !m.IsApproved && !m.IsDeleted)
            .CountAsync();

        // Totals
        var totalDrivers = await _context.Drivers.Where(d => !d.IsDeleted).CountAsync();
        var totalMerchants = await _context.Merchants.Where(m => !m.IsDeleted).CountAsync();
        var totalCustomers = await _context.Customers.Where(c => !c.IsDeleted).CountAsync();

        // Recent orders
        var recentOrders = await _context.Orders
            .Include(o => o.Restaurant)
            .OrderByDescending(o => o.CreatedAt)
            .Take(10)
            .Select(o => new
            {
                o.Id,
                o.OrderNumber,
                Restaurant = o.Restaurant!.Name,
                o.TotalAmount,
                Status = o.Status.ToString(),
                o.CreatedAt
            })
            .ToListAsync();

        // Weekly chart data (last 7 days)
        var weeklyChart = new List<object>();
        for (int i = 6; i >= 0; i--)
        {
            var date = today.AddDays(-i);
            var dayRevenue = await _context.Orders
                .Where(o => o.CreatedAt.Date == date && o.Status == OrderStatus.Delivered)
                .SumAsync(o => o.TotalAmount);

            weeklyChart.Add(new
            {
                DayName = date.ToString("ddd"),
                Date = date.ToString("yyyy-MM-dd"),
                Revenue = dayRevenue
            });
        }

        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            TodayRevenue = todayRevenue,
            WeeklyRevenue = weeklyRevenue,
            MonthlyRevenue = monthlyRevenue,
            ActiveOrders = activeOrders,
            CompletedToday = completedToday,
            PendingDrivers = pendingDrivers,
            PendingMerchants = pendingMerchants,
            TotalDrivers = totalDrivers,
            TotalMerchants = totalMerchants,
            TotalCustomers = totalCustomers,
            RecentOrders = recentOrders,
            WeeklyChart = weeklyChart
        }));
    }

    // ==================== DRIVER APPROVAL ====================

    /// <summary>
    /// Get list of pending driver registrations
    /// </summary>
    [HttpGet("drivers/pending")]
    public async Task<IActionResult> GetPendingDrivers()
    {
        var pendingDrivers = await _context.Drivers
            .Include(d => d.User)
            .Where(d => !d.IsApproved && !d.IsDeleted)
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => new
            {
                d.Id,
                d.User!.FullName,
                d.User.PhoneNumber,
                d.User.Email,
                d.User.AvatarUrl,
                d.VehicleType,
                d.VehicleBrand,
                d.VehiclePlate,
                d.IdCardFrontUrl,
                d.IdCardBackUrl,
                d.DriverLicenseUrl,
                d.VehicleRegistrationUrl,
                SubmittedAt = d.CreatedAt
            })
            .ToListAsync();

        return Ok(ApiResponse<object>.SuccessResponse(pendingDrivers));
    }

    /// <summary>
    /// Approve a driver registration
    /// </summary>
    [HttpPost("drivers/{id}/approve")]
    public async Task<IActionResult> ApproveDriver(Guid id)
    {
        var driver = await _context.Drivers.FindAsync(id);
        if (driver == null)
            return NotFound(ApiResponse<object>.ErrorResponse("Không tìm thấy tài xế"));

        driver.IsApproved = true;
        driver.ApprovedAt = DateTime.UtcNow;
        driver.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(ApiResponse<object>.SuccessResponse(new { }, "Đã duyệt tài xế thành công"));
    }

    /// <summary>
    /// Reject a driver registration
    /// </summary>
    [HttpPost("drivers/{id}/reject")]
    public async Task<IActionResult> RejectDriver(Guid id, [FromBody] RejectRequest? request)
    {
        var driver = await _context.Drivers.FindAsync(id);
        if (driver == null)
            return NotFound(ApiResponse<object>.ErrorResponse("Không tìm thấy tài xế"));

        driver.IsDeleted = true;
        driver.RejectionReason = request?.Reason;
        driver.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(ApiResponse<object>.SuccessResponse(new { }, "Đã từ chối yêu cầu đăng ký"));
    }

    // ==================== MERCHANT APPROVAL ====================

    /// <summary>
    /// Get list of pending merchant registrations
    /// </summary>
    [HttpGet("merchants/pending")]
    public async Task<IActionResult> GetPendingMerchants()
    {
        var pendingMerchants = await _context.Merchants
            .Include(m => m.User)
            .Include(m => m.Restaurants)
            .Where(m => !m.IsApproved && !m.IsDeleted)
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new
            {
                m.Id,
                m.BusinessName,
                m.User!.FullName,
                m.User.PhoneNumber,
                m.User.Email,
                Restaurant = m.Restaurants.Select(r => new
                {
                    r.Id,
                    r.Name,
                    r.ImageUrl,
                    r.Address,
                    r.Category
                }).FirstOrDefault(),
                m.BusinessLicenseUrl,
                m.FoodSafetyCertUrl,
                m.IdCardFrontUrl,
                SubmittedAt = m.CreatedAt
            })
            .ToListAsync();

        return Ok(ApiResponse<object>.SuccessResponse(pendingMerchants));
    }

    /// <summary>
    /// Approve a merchant registration
    /// </summary>
    [HttpPost("merchants/{id}/approve")]
    public async Task<IActionResult> ApproveMerchant(Guid id)
    {
        var merchant = await _context.Merchants
            .Include(m => m.Restaurants)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (merchant == null)
            return NotFound(ApiResponse<object>.ErrorResponse("Không tìm thấy nhà hàng"));

        merchant.IsApproved = true;
        merchant.ApprovedAt = DateTime.UtcNow;
        merchant.UpdatedAt = DateTime.UtcNow;

        // Also approve associated restaurants
        foreach (var restaurant in merchant.Restaurants)
        {
            restaurant.IsApproved = true;
            restaurant.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return Ok(ApiResponse<object>.SuccessResponse(new { }, "Đã duyệt nhà hàng thành công"));
    }

    /// <summary>
    /// Reject a merchant registration
    /// </summary>
    [HttpPost("merchants/{id}/reject")]
    public async Task<IActionResult> RejectMerchant(Guid id, [FromBody] RejectRequest? request)
    {
        var merchant = await _context.Merchants.FindAsync(id);
        if (merchant == null)
            return NotFound(ApiResponse<object>.ErrorResponse("Không tìm thấy nhà hàng"));

        merchant.IsDeleted = true;
        merchant.RejectionReason = request?.Reason;
        merchant.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(ApiResponse<object>.SuccessResponse(new { }, "Đã từ chối yêu cầu đăng ký"));
    }

    // ==================== ORDER MONITORING ====================

    /// <summary>
    /// Get all orders for monitoring
    /// </summary>
    [HttpGet("orders")]
    public async Task<IActionResult> GetAllOrders(
        [FromQuery] string? status = null,
        [FromQuery] string? search = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = _context.Orders
            .Include(o => o.Restaurant)
            .Include(o => o.Customer)
                .ThenInclude(c => c!.User)
            .Include(o => o.Driver)
                .ThenInclude(d => d!.User)
            .AsQueryable();

        // Filter by status
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<OrderStatus>(status, out var orderStatus))
        {
            query = query.Where(o => o.Status == orderStatus);
        }

        // Search by order number, restaurant, or customer
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(o =>
                o.OrderNumber.Contains(search) ||
                o.Restaurant!.Name.Contains(search) ||
                o.Customer!.User!.FullName.Contains(search));
        }

        var totalCount = await query.CountAsync();

        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new
            {
                o.Id,
                o.OrderNumber,
                Restaurant = o.Restaurant!.Name,
                Customer = o.Customer!.User!.FullName,
                Driver = o.Driver != null ? o.Driver.User!.FullName : null,
                Status = o.Status.ToString(),
                o.TotalAmount,
                o.CreatedAt
            })
            .ToListAsync();

        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            Items = orders,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        }));
    }

    /// <summary>
    /// Get order details for admin
    /// </summary>
    [HttpGet("orders/{id}")]
    public async Task<IActionResult> GetOrderDetail(Guid id)
    {
        var order = await _context.Orders
            .Include(o => o.Restaurant)
            .Include(o => o.Customer)
                .ThenInclude(c => c!.User)
            .Include(o => o.Driver)
                .ThenInclude(d => d!.User)
            .Include(o => o.OrderItems)
            .Include(o => o.TrackingHistory)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
            return NotFound(ApiResponse<object>.ErrorResponse("Không tìm thấy đơn hàng"));

        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            order.Id,
            order.OrderNumber,
            Restaurant = new
            {
                order.Restaurant!.Id,
                order.Restaurant.Name,
                order.Restaurant.ImageUrl,
                order.Restaurant.Address
            },
            Customer = new
            {
                order.Customer!.User!.FullName,
                order.Customer.User.PhoneNumber,
                order.Customer.User.AvatarUrl
            },
            Driver = order.Driver != null ? new
            {
                order.Driver.User!.FullName,
                order.Driver.User.PhoneNumber,
                order.Driver.VehicleType,
                order.Driver.VehiclePlate
            } : null,
            Status = order.Status.ToString(),
            order.DeliveryAddress,
            order.Subtotal,
            order.DeliveryFee,
            order.DiscountAmount,
            order.TotalAmount,
            PaymentMethod = order.PaymentMethod.ToString(),
            PaymentStatus = order.PaymentStatus.ToString(),
            order.CreatedAt,
            order.DeliveredAt,
            Items = order.OrderItems.Select(i => new
            {
                i.ItemName,
                i.Quantity,
                i.UnitPrice,
                i.TotalPrice
            }),
            Trackings = order.TrackingHistory.OrderBy(t => t.Timestamp).Select(t => new
            {
                Status = t.Status.ToString(),
                t.Description,
                t.Timestamp
            })
        }));
    }

    // ==================== REPORTS ====================

    /// <summary>
    /// Get reports and statistics
    /// </summary>
    [HttpGet("reports")]
    public async Task<IActionResult> GetReports([FromQuery] string period = "week")
    {
        var today = DateTime.UtcNow.Date;
        DateTime startDate;

        switch (period.ToLower())
        {
            case "today":
                startDate = today;
                break;
            case "week":
                startDate = today.AddDays(-7);
                break;
            case "month":
                startDate = today.AddDays(-30);
                break;
            default:
                startDate = today.AddDays(-7);
                break;
        }

        // Revenue stats
        var totalRevenue = await _context.Orders
            .Where(o => o.CreatedAt >= startDate && o.Status == OrderStatus.Delivered)
            .SumAsync(o => o.TotalAmount);

        var previousPeriodStart = startDate.AddDays(-(today - startDate).Days);
        var previousRevenue = await _context.Orders
            .Where(o => o.CreatedAt >= previousPeriodStart && o.CreatedAt < startDate && o.Status == OrderStatus.Delivered)
            .SumAsync(o => o.TotalAmount);

        var revenueGrowth = previousRevenue > 0
            ? Math.Round((double)(totalRevenue - previousRevenue) / (double)previousRevenue * 100, 1)
            : 0;

        // Order stats
        var totalOrders = await _context.Orders
            .Where(o => o.CreatedAt >= startDate)
            .CountAsync();

        var completedOrders = await _context.Orders
            .Where(o => o.CreatedAt >= startDate && o.Status == OrderStatus.Delivered)
            .CountAsync();

        var cancelledOrders = await _context.Orders
            .Where(o => o.CreatedAt >= startDate && o.Status == OrderStatus.Cancelled)
            .CountAsync();

        var completionRate = totalOrders > 0
            ? Math.Round((double)completedOrders / totalOrders * 100, 1)
            : 0;

        var cancellationRate = totalOrders > 0
            ? Math.Round((double)cancelledOrders / totalOrders * 100, 1)
            : 0;

        // User stats
        var newCustomers = await _context.Customers
            .Where(c => c.CreatedAt >= startDate)
            .CountAsync();

        var activeDrivers = await _context.Drivers
            .Where(d => d.Status == DriverStatus.Online || d.Status == DriverStatus.Busy)
            .CountAsync();

        var activeMerchants = await _context.Restaurants
            .Where(r => r.IsOpen && r.IsApproved)
            .CountAsync();

        // Top restaurants
        var topRestaurants = await _context.Orders
            .Where(o => o.CreatedAt >= startDate && o.Status == OrderStatus.Delivered)
            .GroupBy(o => o.RestaurantId)
            .Select(g => new
            {
                RestaurantId = g.Key,
                OrderCount = g.Count(),
                Revenue = g.Sum(o => o.TotalAmount)
            })
            .OrderByDescending(x => x.Revenue)
            .Take(5)
            .ToListAsync();

        var restaurantIds = topRestaurants.Select(x => x.RestaurantId).ToList();
        var restaurants = await _context.Restaurants
            .Where(r => restaurantIds.Contains(r.Id))
            .ToDictionaryAsync(r => r.Id, r => r.Name);

        var topRestaurantsResult = topRestaurants.Select(x => new
        {
            Name = restaurants.GetValueOrDefault(x.RestaurantId, "Unknown"),
            x.OrderCount,
            x.Revenue
        });

        // Top drivers
        var topDrivers = await _context.Orders
            .Where(o => o.CreatedAt >= startDate && o.DriverId != null && o.Status == OrderStatus.Delivered)
            .GroupBy(o => o.DriverId)
            .Select(g => new
            {
                DriverId = g.Key,
                DeliveryCount = g.Count()
            })
            .OrderByDescending(x => x.DeliveryCount)
            .Take(5)
            .ToListAsync();

        var driverIds = topDrivers.Select(x => x.DriverId).ToList();
        var drivers = await _context.Drivers
            .Include(d => d.User)
            .Where(d => driverIds.Contains(d.Id))
            .ToDictionaryAsync(d => d.Id, d => new { d.User!.FullName, d.Rating });

        var topDriversResult = topDrivers.Select(x => new
        {
            Name = drivers.GetValueOrDefault(x.DriverId!.Value)?.FullName ?? "Unknown",
            Rating = drivers.GetValueOrDefault(x.DriverId!.Value)?.Rating ?? 0,
            x.DeliveryCount
        });

        // Daily chart
        var dailyChart = new List<object>();
        var days = (today - startDate).Days + 1;
        for (int i = days - 1; i >= 0; i--)
        {
            var date = today.AddDays(-i);
            var dayRevenue = await _context.Orders
                .Where(o => o.CreatedAt.Date == date && o.Status == OrderStatus.Delivered)
                .SumAsync(o => o.TotalAmount);

            var dayOrders = await _context.Orders
                .Where(o => o.CreatedAt.Date == date)
                .CountAsync();

            dailyChart.Add(new
            {
                Date = date.ToString("yyyy-MM-dd"),
                DayName = date.ToString("ddd"),
                Revenue = dayRevenue,
                Orders = dayOrders
            });
        }

        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            Period = period,
            Revenue = new
            {
                Total = totalRevenue,
                Growth = revenueGrowth
            },
            Orders = new
            {
                Total = totalOrders,
                Completed = completedOrders,
                Cancelled = cancelledOrders,
                CompletionRate = completionRate,
                CancellationRate = cancellationRate
            },
            Users = new
            {
                NewCustomers = newCustomers,
                ActiveDrivers = activeDrivers,
                ActiveMerchants = activeMerchants
            },
            TopRestaurants = topRestaurantsResult,
            TopDrivers = topDriversResult,
            DailyChart = dailyChart
        }));
    }

    // ==================== USER MANAGEMENT ====================

    /// <summary>
    /// Get all users
    /// </summary>
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(
        [FromQuery] string? role = null,
        [FromQuery] string? search = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = _context.Users.AsQueryable();

        if (!string.IsNullOrEmpty(role) && Enum.TryParse<UserRole>(role, out var userRole))
        {
            query = query.Where(u => u.Role == userRole);
        }

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(u =>
                u.FullName.Contains(search) ||
                u.PhoneNumber.Contains(search) ||
                (u.Email != null && u.Email.Contains(search)));
        }

        var totalCount = await query.CountAsync();

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new
            {
                u.Id,
                u.PhoneNumber,
                u.Email,
                u.FullName,
                u.AvatarUrl,
                Role = u.Role.ToString(),
                u.IsActive,
                u.CreatedAt
            })
            .ToListAsync();

        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            Items = users,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        }));
    }
}

// Request DTOs
public class RejectRequest
{
    public string? Reason { get; set; }
}
