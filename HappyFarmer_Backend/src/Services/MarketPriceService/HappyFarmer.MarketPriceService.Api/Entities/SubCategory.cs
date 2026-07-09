namespace HappyFarmer.MarketPriceService.Api.Entities;

public class SubCategory
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public required string Name { get; set; }
}
