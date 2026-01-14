namespace FoodDelivery.Application.DTOs.Customer;

public class CustomerProfileDto
{
    public Guid Id { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public List<AddressDto> Addresses { get; set; } = new();
    public int TotalOrders { get; set; }
}

public class UpdateProfileDto
{
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? AvatarUrl { get; set; }
}

public class AddressDto
{
    public Guid Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public string FullAddress { get; set; } = string.Empty;
    public string? Note { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public bool IsDefault { get; set; }
}

public class CreateAddressDto
{
    public string Label { get; set; } = "Nhà";
    public string FullAddress { get; set; } = string.Empty;
    public string? Note { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public bool IsDefault { get; set; } = false;
}

public class VoucherDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Type { get; set; } = string.Empty;
    public decimal DiscountValue { get; set; }
    public decimal MaxDiscountAmount { get; set; }
    public decimal MinOrderAmount { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsEligible { get; set; }
    public string? Category { get; set; }
    public string? IconUrl { get; set; }
}

public class CartDto
{
    public Guid RestaurantId { get; set; }
    public string RestaurantName { get; set; } = string.Empty;
    public List<CartItemDto> Items { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal Discount { get; set; }
    public decimal Total { get; set; }
    public VoucherDto? AppliedVoucher { get; set; }
}

public class CartItemDto
{
    public Guid MenuItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string? Notes { get; set; }
}

public class AddToCartDto
{
    public Guid RestaurantId { get; set; }
    public Guid MenuItemId { get; set; }
    public int Quantity { get; set; } = 1;
    public string? Notes { get; set; }
}
