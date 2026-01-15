using FoodDelivery.Application.Common;
using FoodDelivery.Application.DTOs.Restaurant;
using FoodDelivery.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RestaurantsController : ControllerBase
{
    private readonly AppDbContext _context;

    public RestaurantsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<RestaurantDto>>>> GetRestaurants(
        [FromQuery] string? search,
        [FromQuery] string? category,
        [FromQuery] string? sortBy, // nearMe, bestSeller, fastest, rating
        [FromQuery] bool? hasPromotion,
        [FromQuery] double? latitude,
        [FromQuery] double? longitude,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = _context.Restaurants
            .Where(r => !r.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(r => r.Name.Contains(search) || 
                                     (r.NameSecondary != null && r.NameSecondary.Contains(search)));
        }

        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(r => r.Category == category);
        }

        if (hasPromotion == true)
        {
            query = query.Where(r => r.HasPromotion);
        }

        var totalCount = await query.CountAsync();

        // Handle sorting
        var restaurantsQuery = query.AsEnumerable();

        if (sortBy == "nearMe" && latitude.HasValue && longitude.HasValue)
        {
            restaurantsQuery = restaurantsQuery.OrderBy(r => CalculateDistance(latitude.Value, longitude.Value, r.Latitude, r.Longitude));
        }
        else if (sortBy == "bestSeller")
        {
            restaurantsQuery = restaurantsQuery.OrderByDescending(r => r.TotalOrders);
        }
        else if (sortBy == "fastest")
        {
            restaurantsQuery = restaurantsQuery.OrderBy(r => r.EstimatedDeliveryMinutes);
        }
        else // default or rating
        {
            restaurantsQuery = restaurantsQuery.OrderByDescending(r => r.Rating);
        }

        var restaurants = restaurantsQuery
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new RestaurantDto
            {
                Id = r.Id,
                Name = r.Name,
                NameSecondary = r.NameSecondary,
                Description = r.Description,
                ImageUrl = r.ImageUrl,
                CoverImageUrl = r.CoverImageUrl,
                Address = r.Address,
                Latitude = r.Latitude,
                Longitude = r.Longitude,
                Category = r.Category,
                IsOpen = r.IsOpen,
                EstimatedDeliveryMinutes = r.EstimatedDeliveryMinutes,
                DeliveryFee = r.DeliveryFee,
                Rating = r.Rating,
                TotalReviews = r.TotalReviews,
                HasPromotion = r.HasPromotion,
                IsNew = r.IsNew,
                IsTrending = r.IsTrending,
                TotalOrders = r.TotalOrders,
                Distance = (latitude.HasValue && longitude.HasValue) 
                    ? CalculateDistance(latitude.Value, longitude.Value, r.Latitude, r.Longitude) 
                    : 0
            })
            .ToList();

        var result = new PagedResult<RestaurantDto>
        {
            Items = restaurants,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        return Ok(ApiResponse<PagedResult<RestaurantDto>>.SuccessResponse(result));
    }

    [HttpGet("nearby")]
    public async Task<ActionResult<ApiResponse<List<RestaurantDto>>>> GetNearbyRestaurants([FromQuery] NearbyRestaurantsQuery query)
    {
        // Simple distance calculation (in production, use PostGIS or similar)
        var restaurants = await _context.Restaurants
            .Where(r => !r.IsDeleted && r.IsOpen)
            .ToListAsync();

        var nearbyRestaurants = restaurants
            .Select(r => new RestaurantDto
            {
                Id = r.Id,
                Name = r.Name,
                NameSecondary = r.NameSecondary,
                Description = r.Description,
                ImageUrl = r.ImageUrl,
                CoverImageUrl = r.CoverImageUrl,
                Address = r.Address,
                Latitude = r.Latitude,
                Longitude = r.Longitude,
                Category = r.Category,
                IsOpen = r.IsOpen,
                EstimatedDeliveryMinutes = r.EstimatedDeliveryMinutes,
                DeliveryFee = r.DeliveryFee,
                Rating = r.Rating,
                TotalReviews = r.TotalReviews,
                HasPromotion = r.HasPromotion,
                IsNew = r.IsNew,
                Distance = CalculateDistance(query.Latitude, query.Longitude, r.Latitude, r.Longitude)
            })
            .Where(r => r.Distance <= query.RadiusKm)
            .OrderBy(r => query.SortBy switch
            {
                "rating" => -r.Rating,
                "deliveryTime" => r.EstimatedDeliveryMinutes,
                _ => r.Distance
            })
            .Take(query.PageSize)
            .ToList();

        return Ok(ApiResponse<List<RestaurantDto>>.SuccessResponse(nearbyRestaurants));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<RestaurantDetailDto>>> GetRestaurant(Guid id)
    {
        var restaurant = await _context.Restaurants
            .Include(r => r.MenuCategories.Where(mc => mc.IsActive && !mc.IsDeleted))
                .ThenInclude(mc => mc.MenuItems.Where(mi => !mi.IsDeleted))
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

        if (restaurant == null)
        {
            return NotFound(ApiResponse<RestaurantDetailDto>.ErrorResponse("Không tìm thấy nhà hàng"));
        }

        var dto = new RestaurantDetailDto
        {
            Id = restaurant.Id,
            Name = restaurant.Name,
            NameSecondary = restaurant.NameSecondary,
            Description = restaurant.Description,
            ImageUrl = restaurant.ImageUrl,
            CoverImageUrl = restaurant.CoverImageUrl,
            Address = restaurant.Address,
            PhoneNumber = restaurant.PhoneNumber,
            Latitude = restaurant.Latitude,
            Longitude = restaurant.Longitude,
            Category = restaurant.Category,
            IsOpen = restaurant.IsOpen,
            OpenTime = restaurant.OpenTime,
            CloseTime = restaurant.CloseTime,
            EstimatedDeliveryMinutes = restaurant.EstimatedDeliveryMinutes,
            DeliveryFee = restaurant.DeliveryFee,
            MinOrderAmount = restaurant.MinOrderAmount,
            Rating = restaurant.Rating,
            TotalReviews = restaurant.TotalReviews,
            HasPromotion = restaurant.HasPromotion,
            IsNew = restaurant.IsNew,
            MenuCategories = restaurant.MenuCategories
                .OrderBy(mc => mc.DisplayOrder)
                .Select(mc => new MenuCategoryDto
                {
                    Id = mc.Id,
                    Name = mc.Name,
                    NameSecondary = mc.NameSecondary,
                    Description = mc.Description,
                    IconName = mc.IconName,
                    Items = mc.MenuItems
                        .OrderBy(mi => mi.DisplayOrder)
                        .Select(mi => new MenuItemDto
                        {
                            Id = mi.Id,
                            Name = mi.Name,
                            NameSecondary = mi.NameSecondary,
                            Description = mi.Description,
                            ImageUrl = mi.ImageUrl,
                            Price = mi.Price,
                            OriginalPrice = mi.OriginalPrice,
                            IsAvailable = mi.IsAvailable,
                            IsPopular = mi.IsPopular
                        })
                        .ToList()
                })
                .ToList()
        };

        return Ok(ApiResponse<RestaurantDetailDto>.SuccessResponse(dto));
    }

    [HttpGet("categories")]
    public async Task<ActionResult<ApiResponse<List<FoodCategoryDto>>>> GetFoodCategories()
    {
        var categories = await _context.FoodCategories
            .Where(c => c.IsActive && !c.IsDeleted)
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new FoodCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                NameSecondary = c.NameSecondary,
                IconUrl = c.IconUrl,
                BackgroundColor = c.BackgroundColor
            })
            .ToListAsync();

        return Ok(ApiResponse<List<FoodCategoryDto>>.SuccessResponse(categories));
    }

    [HttpGet("promotions")]
    public async Task<ActionResult<ApiResponse<List<PromotionDto>>>> GetPromotions()
    {
        var now = DateTime.UtcNow;
        var promotions = await _context.Promotions
            .Where(p => p.IsActive && !p.IsDeleted && p.StartDate <= now && p.EndDate >= now)
            .OrderBy(p => p.DisplayOrder)
            .Select(p => new PromotionDto
            {
                Id = p.Id,
                Title = p.Title,
                Subtitle = p.Subtitle,
                ImageUrl = p.ImageUrl,
                Badge = p.Badge,
                BadgeColor = p.BadgeColor,
                EndDate = p.EndDate
            })
            .ToListAsync();

        return Ok(ApiResponse<List<PromotionDto>>.SuccessResponse(promotions));
    }

    [HttpGet("trending")]
    public async Task<ActionResult<ApiResponse<List<RestaurantDto>>>> GetTrendingRestaurants([FromQuery] int limit = 10)
    {
        var restaurants = await _context.Restaurants
            .Where(r => !r.IsDeleted && r.IsApproved)
            .OrderByDescending(r => r.IsTrending)
            .ThenByDescending(r => r.TotalOrders)
            .ThenByDescending(r => r.Rating)
            .Take(limit)
            .Select(r => new RestaurantDto
            {
                Id = r.Id,
                Name = r.Name,
                NameSecondary = r.NameSecondary,
                Description = r.Description,
                ImageUrl = r.ImageUrl,
                CoverImageUrl = r.CoverImageUrl,
                Address = r.Address,
                Latitude = r.Latitude,
                Longitude = r.Longitude,
                Category = r.Category,
                IsOpen = r.IsOpen,
                EstimatedDeliveryMinutes = r.EstimatedDeliveryMinutes,
                DeliveryFee = r.DeliveryFee,
                Rating = r.Rating,
                TotalReviews = r.TotalReviews,
                HasPromotion = r.HasPromotion,
                IsNew = r.IsNew,
                IsTrending = r.IsTrending,
                TotalOrders = r.TotalOrders
            })
            .ToListAsync();

        return Ok(ApiResponse<List<RestaurantDto>>.SuccessResponse(restaurants));
    }

    [HttpGet("popular-categories")]
    public async Task<ActionResult<ApiResponse<List<FoodCategoryDto>>>> GetPopularCategories([FromQuery] int limit = 8)
    {
        var categories = await _context.FoodCategories
            .Where(c => c.IsActive && !c.IsDeleted)
            .OrderBy(c => c.DisplayOrder)
            .Take(limit)
            .Select(c => new FoodCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                NameSecondary = c.NameSecondary,
                IconUrl = c.IconUrl,
                BackgroundColor = c.BackgroundColor
            })
            .ToListAsync();

        return Ok(ApiResponse<List<FoodCategoryDto>>.SuccessResponse(categories));
    }

    private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371; // Earth's radius in km
        var dLat = ToRad(lat2 - lat1);
        var dLon = ToRad(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private static double ToRad(double deg) => deg * Math.PI / 180;
}
