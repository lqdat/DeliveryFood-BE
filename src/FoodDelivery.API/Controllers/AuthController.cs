using FoodDelivery.Application.Common;
using FoodDelivery.Application.DTOs.Auth;
using FoodDelivery.Domain.Entities;
using FoodDelivery.Domain.Enums;
using FoodDelivery.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FoodDelivery.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Register([FromBody] RegisterDto dto)
    {
        // Check if user exists
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == dto.PhoneNumber);
        if (existingUser != null)
        {
            return BadRequest(ApiResponse<AuthResponseDto>.ErrorResponse("Số điện thoại đã được đăng ký"));
        }

        // Parse role
        if (!Enum.TryParse<UserRole>(dto.Role, true, out var role))
        {
            role = UserRole.Customer;
        }

        // Create user
        var user = new User
        {
            PhoneNumber = dto.PhoneNumber,
            Email = dto.Email,
            FullName = dto.FullName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = role
        };

        _context.Users.Add(user);

        // Create role-specific profile
        switch (role)
        {
            case UserRole.Customer:
                _context.Customers.Add(new Customer { UserId = user.Id });
                break;
            case UserRole.Merchant:
                _context.Merchants.Add(new Merchant { UserId = user.Id });
                break;
            case UserRole.Driver:
                _context.Drivers.Add(new Driver { UserId = user.Id });
                break;
        }

        await _context.SaveChangesAsync();

        var response = GenerateAuthResponse(user);
        return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(response, "Đăng ký thành công"));
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login([FromBody] LoginDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == dto.PhoneNumber);
        
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            return Unauthorized(ApiResponse<AuthResponseDto>.ErrorResponse("Số điện thoại hoặc mật khẩu không đúng"));
        }

        if (!user.IsActive)
        {
            return Unauthorized(ApiResponse<AuthResponseDto>.ErrorResponse("Tài khoản đã bị khóa"));
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var response = GenerateAuthResponse(user);
        return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(response, "Đăng nhập thành công"));
    }

    [HttpPost("send-otp")]
    public async Task<ActionResult<ApiResponse<object>>> SendOtp([FromBody] SendOtpDto dto)
    {
        // In production, integrate with SMS provider
        // For demo, just return success
        return Ok(ApiResponse<object>.SuccessResponse(new { }, "OTP đã được gửi đến " + dto.PhoneNumber));
    }

    [HttpPost("verify-otp")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> VerifyOtp([FromBody] VerifyOtpDto dto)
    {
        // In production, verify actual OTP
        // For demo, accept any 6-digit code
        if (dto.OtpCode.Length != 6)
        {
            return BadRequest(ApiResponse<AuthResponseDto>.ErrorResponse("OTP không hợp lệ"));
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == dto.PhoneNumber);
        if (user == null)
        {
            return NotFound(ApiResponse<AuthResponseDto>.ErrorResponse("Không tìm thấy tài khoản"));
        }

        var response = GenerateAuthResponse(user);
        return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(response, "Xác thực thành công"));
    }

    private AuthResponseDto GenerateAuthResponse(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"] ?? "YourSuperSecretKeyHere12345678901234567890");
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.MobilePhone, user.PhoneNumber),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(ClaimTypes.Name, user.FullName)
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return new AuthResponseDto
        {
            Token = tokenHandler.WriteToken(token),
            RefreshToken = Guid.NewGuid().ToString(),
            ExpiresAt = tokenDescriptor.Expires.Value,
            User = new UserDto
            {
                Id = user.Id,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                FullName = user.FullName,
                AvatarUrl = user.AvatarUrl,
                Role = user.Role.ToString()
            }
        };
    }
}
