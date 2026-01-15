namespace FoodDelivery.Domain.Entities;

public class SearchHistory : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    public string Keyword { get; set; } = string.Empty;
}
