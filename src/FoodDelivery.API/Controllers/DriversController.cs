using FoodDelivery.Application.Common;
using FoodDelivery.Application.DTOs.Driver;
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
[Authorize(Roles = "Driver")]
public class DriversController : ControllerBase
{
    private readonly AppDbContext _context;

    public DriversController(AppDbContext context)
    {
        _context = context;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("profile")]
    public async Task<ActionResult<ApiResponse<DriverProfileDto>>> GetProfile()
    {
        var userId = GetUserId();
        var driver = await _context.Drivers
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.UserId == userId);

        if (driver == null)
            return NotFound(ApiResponse<DriverProfileDto>.ErrorResponse("Driver not found"));

        return Ok(ApiResponse<DriverProfileDto>.SuccessResponse(new DriverProfileDto
        {
            Id = driver.Id,
            FullName = driver.User.FullName,
            PhoneNumber = driver.User.PhoneNumber,
            AvatarUrl = driver.User.AvatarUrl,
            VehicleType = driver.VehicleType,
            VehiclePlate = driver.VehiclePlate,
            VehicleBrand = driver.VehicleBrand,
            Status = driver.Status,
            WalletBalance = driver.WalletBalance,
            Rating = driver.Rating,
            TotalDeliveries = driver.TotalDeliveries
        }));
    }

    [HttpPut("status")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateStatus([FromBody] UpdateDriverStatusDto dto)
    {
        var userId = GetUserId();
        var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.UserId == userId);

        if (driver == null)
            return NotFound(ApiResponse<object>.ErrorResponse("Driver not found"));

        driver.Status = dto.Status;
        if (dto.Latitude.HasValue) driver.CurrentLatitude = dto.Latitude;
        if (dto.Longitude.HasValue) driver.CurrentLongitude = dto.Longitude;

        await _context.SaveChangesAsync();

        return Ok(ApiResponse<object>.SuccessResponse(new { }, 
            dto.Status == DriverStatus.Online ? "Bạn đang online" : "Bạn đã offline"));
    }

    [HttpGet("jobs/pending")]
    public async Task<ActionResult<ApiResponse<List<JobRequestDto>>>> GetPendingJobs()
    {
        var userId = GetUserId();
        var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.UserId == userId);

        if (driver == null || driver.Status != DriverStatus.Online)
            return Ok(ApiResponse<List<JobRequestDto>>.SuccessResponse(new List<JobRequestDto>()));

        // Get orders pending driver assignment
        var pendingOrders = await _context.Orders
            .Include(o => o.Restaurant)
            .Include(o => o.OrderItems)
            .Where(o => o.Status == OrderStatus.ReadyForPickup && o.DriverId == null)
            .Take(5)
            .ToListAsync();

        var jobs = pendingOrders.Select(o => new JobRequestDto
        {
            Id = o.Id,
            OrderNumber = o.OrderNumber,
            RestaurantName = o.Restaurant.Name,
            RestaurantImageUrl = o.Restaurant.ImageUrl,
            RestaurantAddress = o.Restaurant.Address,
            RestaurantCategory = o.Restaurant.Category,
            DeliveryAddress = o.DeliveryAddress,
            PickupDistanceKm = CalculateDistance(
                driver.CurrentLatitude ?? 0, driver.CurrentLongitude ?? 0,
                o.Restaurant.Latitude, o.Restaurant.Longitude),
            TotalDistanceKm = CalculateDistance(
                o.Restaurant.Latitude, o.Restaurant.Longitude,
                o.DeliveryLatitude, o.DeliveryLongitude),
            EstimatedMinutes = o.EstimatedDeliveryMinutes,
            EstimatedEarnings = CalculateEarnings(o.TotalAmount),
            HasTip = true,
            IsHighDemand = o.DeliveryFee > 20000,
            ItemCount = o.OrderItems.Count,
            TimeoutSeconds = 30
        }).ToList();

        return Ok(ApiResponse<List<JobRequestDto>>.SuccessResponse(jobs));
    }

    [HttpPost("jobs/{orderId}/accept")]
    public async Task<ActionResult<ApiResponse<object>>> AcceptJob(Guid orderId)
    {
        var userId = GetUserId();
        var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.UserId == userId);

        if (driver == null)
            return NotFound(ApiResponse<object>.ErrorResponse("Driver not found"));

        var order = await _context.Orders.FindAsync(orderId);
        if (order == null)
            return NotFound(ApiResponse<object>.ErrorResponse("Đơn hàng không tồn tại"));

        if (order.DriverId != null)
            return BadRequest(ApiResponse<object>.ErrorResponse("Đơn hàng đã được nhận"));

        order.DriverId = driver.Id;
        order.Status = OrderStatus.PickedUp;
        driver.Status = DriverStatus.Busy;

        order.TrackingHistory.Add(new OrderTracking
        {
            OrderId = order.Id,
            Status = OrderStatus.PickedUp,
            Description = "Tài xế đã lấy hàng",
            DescriptionSecondary = "Driver picked up order"
        });

        await _context.SaveChangesAsync();

        return Ok(ApiResponse<object>.SuccessResponse(new { }, "Đã nhận đơn hàng!"));
    }

    [HttpPost("jobs/{orderId}/decline")]
    public async Task<ActionResult<ApiResponse<object>>> DeclineJob(Guid orderId)
    {
        // Just return success - order remains available for other drivers
        return Ok(ApiResponse<object>.SuccessResponse(new { }, "Đã từ chối đơn hàng"));
    }

    [HttpGet("wallet")]
    public async Task<ActionResult<ApiResponse<WalletDto>>> GetWallet()
    {
        var userId = GetUserId();
        var driver = await _context.Drivers
            .Include(d => d.Earnings)
            .FirstOrDefaultAsync(d => d.UserId == userId);

        if (driver == null)
            return NotFound(ApiResponse<WalletDto>.ErrorResponse("Driver not found"));

        var today = DateTime.UtcNow.Date;
        var weekStart = today.AddDays(-(int)today.DayOfWeek + 1);

        var todayEarnings = driver.Earnings
            .Where(e => e.EarnedAt.Date == today)
            .Sum(e => e.Amount);

        var weeklyEarnings = driver.Earnings
            .Where(e => e.EarnedAt >= weekStart)
            .Sum(e => e.Amount);

        var recentEarnings = driver.Earnings
            .OrderByDescending(e => e.EarnedAt)
            .Take(10)
            .Select(e => new EarningDto
            {
                Id = e.Id,
                Type = e.Type,
                TypeText = e.Type switch
                {
                    EarningType.Delivery => "Giao hàng",
                    EarningType.Tip => "Tip từ khách",
                    EarningType.Bonus => "Tiền thưởng",
                    _ => "Khác"
                },
                Amount = e.Amount,
                Description = e.Description,
                OrderNumber = e.Order?.OrderNumber,
                EarnedAt = e.EarnedAt
            }).ToList();

        var weeklyChart = GetWeeklyChart(driver.Earnings, weekStart);

        return Ok(ApiResponse<WalletDto>.SuccessResponse(new WalletDto
        {
            Balance = driver.WalletBalance,
            TodayEarnings = todayEarnings,
            WeeklyEarnings = weeklyEarnings,
            RecentEarnings = recentEarnings,
            WeeklyChart = weeklyChart
        }));
    }

    [HttpPost("withdraw")]
    public async Task<ActionResult<ApiResponse<object>>> Withdraw([FromBody] WithdrawDto dto)
    {
        var userId = GetUserId();
        var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.UserId == userId);

        if (driver == null)
            return NotFound(ApiResponse<object>.ErrorResponse("Driver not found"));

        if (dto.Amount > driver.WalletBalance)
            return BadRequest(ApiResponse<object>.ErrorResponse("Số dư không đủ"));

        driver.WalletBalance -= dto.Amount;
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<object>.SuccessResponse(new { }, "Yêu cầu rút tiền đã được gửi"));
    }

    private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371;
        var dLat = (lat2 - lat1) * Math.PI / 180;
        var dLon = (lon2 - lon1) * Math.PI / 180;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return Math.Round(R * c, 1);
    }

    private static decimal CalculateEarnings(decimal orderTotal)
    {
        return Math.Round(orderTotal * 0.15m + 10000, 0); // 15% + base 10k
    }

    private static WeeklyChartDto GetWeeklyChart(ICollection<DriverEarning> earnings, DateTime weekStart)
    {
        var days = new[] { "T2", "T3", "T4", "T5", "T6", "T7", "CN" };
        var dailyTotals = new decimal[7];

        foreach (var e in earnings.Where(e => e.EarnedAt >= weekStart))
        {
            var dayIndex = ((int)e.EarnedAt.DayOfWeek + 6) % 7;
            dailyTotals[dayIndex] += e.Amount;
        }

        var maxAmount = dailyTotals.Max();

        return new WeeklyChartDto
        {
            Days = days.Select((d, i) => new DailyEarningDto
            {
                DayName = d,
                Amount = dailyTotals[i],
                Percentage = maxAmount > 0 ? (double)(dailyTotals[i] / maxAmount * 100) : 0
            }).ToList()
        };
    }
}
