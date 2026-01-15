using FoodDelivery.Application.Common;
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
public class CartController : ControllerBase
{
    private readonly AppDbContext _context;

    public CartController(AppDbContext context)
    {
        _context = context;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<ApiResponse<object>>> GetCart()
    {
        var userId = GetUserId();
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
        if (customer == null) return NotFound(ApiResponse<object>.ErrorResponse("Customer not found"));

        var cart = await _context.Carts
            .Include(c => c.Restaurant)
            .Include(c => c.Items).ThenInclude(i => i.MenuItem)
            .FirstOrDefaultAsync(c => c.CustomerId == customer.Id);

        if (cart == null)
        {
            return Ok(ApiResponse<object>.SuccessResponse(null));
        }

        var cartDto = new
        {
            RestaurantId = cart.RestaurantId,
            RestaurantName = cart.Restaurant.Name,
            Items = cart.Items.Select(i => new
            {
                MenuItemId = i.MenuItemId,
                ItemName = i.MenuItem.Name,
                ImageUrl = i.MenuItem.ImageUrl,
                Price = i.UnitPrice,
                Quantity = i.Quantity,
                Notes = i.Notes
            }),
            Subtotal = cart.Subtotal,
            DeliveryFee = cart.Restaurant.DeliveryFee,
            Total = cart.Subtotal + cart.Restaurant.DeliveryFee
        };

        return Ok(ApiResponse<object>.SuccessResponse(cartDto));
    }

    [HttpPost("items")]
    public async Task<ActionResult<ApiResponse<object>>> AddToCart([FromBody] AddToCartRequest request)
    {
        var userId = GetUserId();
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
        if (customer == null) return NotFound(ApiResponse<object>.ErrorResponse("Customer not found"));

        var menuItem = await _context.MenuItems
            .Include(m => m.MenuCategory)
            .FirstOrDefaultAsync(m => m.Id == request.MenuItemId);

        if (menuItem == null) return NotFound(ApiResponse<object>.ErrorResponse("Món ăn không tồn tại"));

        var restaurantId = menuItem.MenuCategory.RestaurantId;

        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.CustomerId == customer.Id);

        if (cart == null)
        {
            cart = new Cart
            {
                CustomerId = customer.Id,
                RestaurantId = restaurantId
            };
            _context.Carts.Add(cart);
        }
        else if (cart.RestaurantId != restaurantId)
        {
            // Clear cart if adding from a different restaurant
            _context.CartItems.RemoveRange(cart.Items);
            cart.RestaurantId = restaurantId;
        }

        var existingItem = cart.Items.FirstOrDefault(i => i.MenuItemId == request.MenuItemId);
        if (existingItem != null)
        {
            existingItem.Quantity += request.Quantity;
            existingItem.Notes = request.Notes;
            existingItem.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            var cartItem = new CartItem
            {
                CartId = cart.Id,
                MenuItemId = request.MenuItemId,
                Quantity = request.Quantity,
                UnitPrice = menuItem.Price,
                Notes = request.Notes
            };
            _context.CartItems.Add(cartItem);
        }

        await _context.SaveChangesAsync();

        return Ok(ApiResponse<object>.SuccessResponse(new { }, "Đã thêm vào giỏ hàng"));
    }

    [HttpPut("items/{menuItemId}")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateQuantity(Guid menuItemId, [FromBody] int quantity)
    {
        var userId = GetUserId();
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
        if (customer == null) return NotFound(ApiResponse<object>.ErrorResponse("Customer not found"));

        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.CustomerId == customer.Id);

        if (cart == null) return NotFound(ApiResponse<object>.ErrorResponse("Giỏ hàng trống"));

        var item = cart.Items.FirstOrDefault(i => i.MenuItemId == menuItemId);
        if (item == null) return NotFound(ApiResponse<object>.ErrorResponse("Món ăn không có trong giỏ hàng"));

        if (quantity <= 0)
        {
            _context.CartItems.Remove(item);
        }
        else
        {
            item.Quantity = quantity;
            item.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        // If no items left, remove cart
        if (!cart.Items.Any(i => i.Quantity > 0))
        {
            _context.Carts.Remove(cart);
            await _context.SaveChangesAsync();
        }

        return Ok(ApiResponse<object>.SuccessResponse(new { }, "Đã cập nhật số lượng"));
    }

    [HttpDelete("items/{menuItemId}")]
    public async Task<ActionResult<ApiResponse<object>>> RemoveItem(Guid menuItemId)
    {
        return await UpdateQuantity(menuItemId, 0);
    }

    [HttpDelete]
    public async Task<ActionResult<ApiResponse<object>>> ClearCart()
    {
        var userId = GetUserId();
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
        if (customer == null) return NotFound(ApiResponse<object>.ErrorResponse("Customer not found"));

        var cart = await _context.Carts.FirstOrDefaultAsync(c => c.CustomerId == customer.Id);
        if (cart != null)
        {
            _context.Carts.Remove(cart);
            await _context.SaveChangesAsync();
        }

        return Ok(ApiResponse<object>.SuccessResponse(new { }, "Đã xóa giỏ hàng"));
    }
}

public class AddToCartRequest
{
    public Guid MenuItemId { get; set; }
    public int Quantity { get; set; }
    public string? Notes { get; set; }
}
