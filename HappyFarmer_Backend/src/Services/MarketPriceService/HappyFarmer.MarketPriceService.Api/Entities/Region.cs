namespace HappyFarmer.MarketPriceService.Api.Entities;

public class Region
{
    public int Id { get; set; }
    public required string ProvinceName { get; set; }
    public required string MarketName { get; set; }
    public double? Lat { get; set; }
    public double? Lon { get; set; }
}
