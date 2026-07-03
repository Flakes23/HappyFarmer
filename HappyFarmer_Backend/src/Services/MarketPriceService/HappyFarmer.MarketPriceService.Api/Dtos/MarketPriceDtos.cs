using System.ComponentModel.DataAnnotations;
using HappyFarmer.MarketPriceService.Api.Entities;

namespace HappyFarmer.MarketPriceService.Api.Dtos;

public record ProductResponse(int Id, string NameVi, string? Category, string Unit, string? ImageUrl)
{
    public static ProductResponse FromEntity(Product p) => new(p.Id, p.NameVi, p.Category, p.Unit, p.ImageUrl);
}

public record RegionResponse(int Id, string ProvinceName, string MarketName, double? Lat, double? Lon)
{
    public static RegionResponse FromEntity(Region r) => new(r.Id, r.ProvinceName, r.MarketName, r.Lat, r.Lon);
}

public record PriceResponse(
    int ProductId, string ProductName, int RegionId, string RegionName,
    decimal Price, string Source, DateOnly EffectiveDate);

public record PriceHistoryPoint(DateOnly EffectiveDate, decimal Price);

public record TrendingItem(
    int ProductId, string ProductName, int RegionId, string RegionName,
    decimal CurrentPrice, decimal? PreviousPrice, decimal? ChangePercent);

public record SubmitPriceRequest(
    [Required] int ProductId,
    [Required] int RegionId,
    [Required, Range(0.01, double.MaxValue)] decimal Price,
    [Required] DateOnly EffectiveDate);

public record CrawlIngestRequest(
    [Required] int ProductId,
    [Required] int RegionId,
    [Required, Range(0.01, double.MaxValue)] decimal Price,
    [Required] DateOnly EffectiveDate);

public record PriceEntryResponse(int Id, int ProductId, int RegionId, decimal Price, string Source, string Status, DateOnly EffectiveDate)
{
    public static PriceEntryResponse FromEntity(PriceEntry pe) => new(
        pe.Id, pe.ProductId, pe.RegionId, pe.Price, pe.Source.ToString(), pe.Status.ToString(), pe.EffectiveDate);
}
