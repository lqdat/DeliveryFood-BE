using FoodDelivery.Application.Common;
using FoodDelivery.Application.DTOs.Search;
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
public class SearchController : ControllerBase
{
    private readonly AppDbContext _context;

    public SearchController(AppDbContext context)
    {
        _context = context;
    }

    private Guid? GetUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim)) return null;
        return Guid.Parse(userIdClaim);
    }

    [HttpGet("history")]
    public async Task<ActionResult<ApiResponse<List<string>>>> GetHistory()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized(ApiResponse<List<string>>.ErrorResponse("Unauthorized"));

        var history = await _context.SearchHistories
            .Where(sh => sh.UserId == userId)
            .GroupBy(sh => sh.Keyword)
            .Select(g => new { Keyword = g.Key, CreatedAt = g.Max(sh => sh.CreatedAt) })
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => x.Keyword)
            .Take(10)
            .ToListAsync();

        return Ok(ApiResponse<List<string>>.SuccessResponse(history));
    }

    [HttpPost("history")]
    public async Task<ActionResult<ApiResponse<object>>> SaveKeyword([FromBody] SaveSearchKeywordDto dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Keyword))
            return BadRequest(ApiResponse<object>.ErrorResponse("Keyword cannot be empty"));

        var userId = GetUserId();
        if (userId == null) return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var keyword = dto.Keyword.Trim();
        
        // Remove existing if any to move to top (optional because of GroupBy in GetHistory, 
        // but keeps DB clean)
        var existing = await _context.SearchHistories
            .Where(sh => sh.UserId == userId && sh.Keyword.ToLower() == keyword.ToLower())
            .ToListAsync();
        
        if (existing.Any())
        {
            _context.SearchHistories.RemoveRange(existing);
        }

        var history = new SearchHistory
        {
            UserId = userId.Value,
            Keyword = keyword
        };

        _context.SearchHistories.Add(history);
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<object>.SuccessResponse(new { }));
    }

    [HttpDelete("history")]
    public async Task<ActionResult<ApiResponse<object>>> ClearHistory()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var history = await _context.SearchHistories
            .Where(sh => sh.UserId == userId)
            .ToListAsync();

        _context.SearchHistories.RemoveRange(history);
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<object>.SuccessResponse(new { }));
    }
}
