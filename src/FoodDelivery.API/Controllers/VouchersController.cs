using FoodDelivery.Application.Common;
using FoodDelivery.Application.DTOs.Customer;
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
public class VouchersController : ControllerBase
{
    private readonly AppDbContext _context;

    public VouchersController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<VoucherDto>>>> GetVouchers(
        [FromQuery] string? category,
        [FromQuery] decimal? orderAmount)
    {
        var now = DateTime.UtcNow;
        var query = _context.Vouchers
            .Where(v => v.IsActive && !v.IsDeleted && v.StartDate <= now && v.EndDate >= now);

        if (!string.IsNullOrEmpty(category))
            query = query.Where(v => v.Category == category);

        var vouchers = await query
            .OrderByDescending(v => v.EndDate)
            .Select(v => new VoucherDto
            {
                Id = v.Id,
                Code = v.Code,
                Name = v.Name,
                Description = v.Description,
                Type = v.Type.ToString(),
                DiscountValue = v.DiscountValue,
                MaxDiscountAmount = v.MaxDiscountAmount,
                MinOrderAmount = v.MinOrderAmount,
                EndDate = v.EndDate,
                IsEligible = !orderAmount.HasValue || orderAmount >= v.MinOrderAmount,
                Category = v.Category,
                IconUrl = v.IconUrl
            })
            .ToListAsync();

        return Ok(ApiResponse<List<VoucherDto>>.SuccessResponse(vouchers));
    }

    [HttpPost("validate")]
    public async Task<ActionResult<ApiResponse<VoucherDto>>> ValidateVoucher(
        [FromBody] ValidateVoucherDto dto)
    {
        var now = DateTime.UtcNow;
        var voucher = await _context.Vouchers
            .FirstOrDefaultAsync(v => v.Code == dto.Code && 
                                      v.IsActive && 
                                      !v.IsDeleted && 
                                      v.StartDate <= now && 
                                      v.EndDate >= now);

        if (voucher == null)
            return NotFound(ApiResponse<VoucherDto>.ErrorResponse("Mã không hợp lệ hoặc đã hết hạn"));

        if (dto.OrderAmount < voucher.MinOrderAmount)
            return BadRequest(ApiResponse<VoucherDto>.ErrorResponse(
                $"Đơn hàng tối thiểu {voucher.MinOrderAmount:N0}đ"));

        if (voucher.MaxUsage.HasValue && voucher.UsedCount >= voucher.MaxUsage)
            return BadRequest(ApiResponse<VoucherDto>.ErrorResponse("Mã đã hết lượt sử dụng"));

        decimal discountAmount = voucher.Type switch
        {
            VoucherType.Percentage => Math.Min(dto.OrderAmount * voucher.DiscountValue / 100, voucher.MaxDiscountAmount),
            VoucherType.FixedAmount => Math.Min(voucher.DiscountValue, voucher.MaxDiscountAmount),
            VoucherType.FreeShipping => voucher.MaxDiscountAmount,
            _ => 0
        };

        return Ok(ApiResponse<VoucherDto>.SuccessResponse(new VoucherDto
        {
            Id = voucher.Id,
            Code = voucher.Code,
            Name = voucher.Name,
            Description = $"Giảm {discountAmount:N0}đ",
            Type = voucher.Type.ToString(),
            DiscountValue = discountAmount,
            MaxDiscountAmount = voucher.MaxDiscountAmount,
            MinOrderAmount = voucher.MinOrderAmount,
            EndDate = voucher.EndDate,
            IsEligible = true
        }));
    }
}

public class ValidateVoucherDto
{
    public string Code { get; set; } = string.Empty;
    public decimal OrderAmount { get; set; }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly AppDbContext _context;

    public CustomersController(AppDbContext context)
    {
        _context = context;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("profile")]
    public async Task<ActionResult<ApiResponse<CustomerProfileDto>>> GetProfile()
    {
        var userId = GetUserId();
        var customer = await _context.Customers
            .Include(c => c.User)
            .Include(c => c.Addresses)
            .Include(c => c.Orders)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (customer == null)
            return NotFound(ApiResponse<CustomerProfileDto>.ErrorResponse("Customer not found"));

        return Ok(ApiResponse<CustomerProfileDto>.SuccessResponse(new CustomerProfileDto
        {
            Id = customer.Id,
            PhoneNumber = customer.User.PhoneNumber,
            Email = customer.User.Email,
            FullName = customer.User.FullName,
            AvatarUrl = customer.User.AvatarUrl,
            TotalOrders = customer.Orders.Count,
            Addresses = customer.Addresses.Select(a => new AddressDto
            {
                Id = a.Id,
                Label = a.Label,
                FullAddress = a.FullAddress,
                Note = a.Note,
                Latitude = a.Latitude,
                Longitude = a.Longitude,
                IsDefault = a.IsDefault
            }).ToList()
        }));
    }

    [HttpPut("profile")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        var userId = GetUserId();
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
            return NotFound(ApiResponse<object>.ErrorResponse("User not found"));

        if (dto.FullName != null) user.FullName = dto.FullName;
        if (dto.Email != null) user.Email = dto.Email;
        if (dto.AvatarUrl != null) user.AvatarUrl = dto.AvatarUrl;

        await _context.SaveChangesAsync();
        return Ok(ApiResponse<object>.SuccessResponse(new { }, "Cập nhật thành công"));
    }

    [HttpGet("addresses")]
    public async Task<ActionResult<ApiResponse<List<AddressDto>>>> GetAddresses()
    {
        var userId = GetUserId();
        var customer = await _context.Customers
            .Include(c => c.Addresses)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (customer == null)
            return NotFound(ApiResponse<List<AddressDto>>.ErrorResponse("Customer not found"));

        return Ok(ApiResponse<List<AddressDto>>.SuccessResponse(
            customer.Addresses.Select(a => new AddressDto
            {
                Id = a.Id,
                Label = a.Label,
                FullAddress = a.FullAddress,
                Note = a.Note,
                Latitude = a.Latitude,
                Longitude = a.Longitude,
                IsDefault = a.IsDefault
            }).ToList()));
    }

    [HttpPost("addresses")]
    public async Task<ActionResult<ApiResponse<AddressDto>>> CreateAddress([FromBody] CreateAddressDto dto)
    {
        var userId = GetUserId();
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);

        if (customer == null)
            return NotFound(ApiResponse<AddressDto>.ErrorResponse("Customer not found"));

        if (dto.IsDefault)
        {
            var existingAddresses = await _context.Addresses
                .Where(a => a.CustomerId == customer.Id)
                .ToListAsync();
            existingAddresses.ForEach(a => a.IsDefault = false);
        }

        var address = new Address
        {
            CustomerId = customer.Id,
            Label = dto.Label,
            FullAddress = dto.FullAddress,
            Note = dto.Note,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            IsDefault = dto.IsDefault
        };

        _context.Addresses.Add(address);
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<AddressDto>.SuccessResponse(new AddressDto
        {
            Id = address.Id,
            Label = address.Label,
            FullAddress = address.FullAddress,
            IsDefault = address.IsDefault
        }, "Đã thêm địa chỉ mới"));
    }
}
