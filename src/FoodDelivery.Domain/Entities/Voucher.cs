using FoodDelivery.Domain.Enums;

namespace FoodDelivery.Domain.Entities;

public class Voucher : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    public VoucherType Type { get; set; }
    public decimal DiscountValue { get; set; } // % hoặc số tiền
    public decimal MaxDiscountAmount { get; set; } // Giảm tối đa
    public decimal MinOrderAmount { get; set; } // Đơn tối thiểu
    
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    
    public int? MaxUsage { get; set; }
    public int UsedCount { get; set; } = 0;
    
    public bool IsActive { get; set; } = true;
    public string? Category { get; set; } // Vận chuyển, Đồ ăn, Đối tác
    public string? IconUrl { get; set; }
    
    // Navigation
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
