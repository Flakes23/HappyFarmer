namespace HappyFarmer.MarketPriceService.Api.Entities;

public class Product
{
    public int Id { get; set; }
    public required string NameVi { get; set; }
    public int SubCategoryId { get; set; }
    public SubCategory SubCategory { get; set; } = null!;
    public required string Unit { get; set; }
    public string? ImageUrl { get; set; }
}
