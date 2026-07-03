namespace HappyFarmer.MarketPriceService.Api.Entities;

public class Product
{
    public int Id { get; set; }
    public required string NameVi { get; set; }
    public string? Category { get; set; }
    public required string Unit { get; set; }
    public string? ImageUrl { get; set; }
}
