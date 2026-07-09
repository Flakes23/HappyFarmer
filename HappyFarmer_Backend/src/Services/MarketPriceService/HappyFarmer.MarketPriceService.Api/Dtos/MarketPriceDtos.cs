using System.ComponentModel.DataAnnotations;
using HappyFarmer.MarketPriceService.Api.Entities;

namespace HappyFarmer.MarketPriceService.Api.Dtos;

public record PagedResult<T>(List<T> Items, int Page, int PageSize, int TotalCount);

public record CategoryResponse(int Id, string Name)
{
    public static CategoryResponse FromEntity(Category c) => new(c.Id, c.Name);
}

public record SubCategoryResponse(int Id, int CategoryId, string CategoryName, string Name)
{
    public static SubCategoryResponse FromEntity(SubCategory sc) => new(sc.Id, sc.CategoryId, sc.Category.Name, sc.Name);
}

public record ProductResponse(int Id, string NameVi, int SubCategoryId, string SubCategoryName, int CategoryId, string CategoryName, string Unit, string? ImageUrl)
{
    public static ProductResponse FromEntity(Product p) => new(
        p.Id, p.NameVi, p.SubCategoryId, p.SubCategory.Name, p.SubCategory.CategoryId, p.SubCategory.Category.Name, p.Unit, p.ImageUrl);
}

public record RegionResponse(int Id, string ProvinceName, string MarketName, double? Lat, double? Lon)
{
    public static RegionResponse FromEntity(Region r) => new(r.Id, r.ProvinceName, r.MarketName, r.Lat, r.Lon);
}

public record PriceResponse(
    int ProductId, string ProductName, int RegionId, string RegionName,
    decimal Price, string Source, DateOnly EffectiveDate, string? Unit);

public record PriceHistoryPoint(DateOnly EffectiveDate, decimal Price, string? Unit);

public record TrendingItem(
    int ProductId, string ProductName, int RegionId, string RegionName,
    decimal CurrentPrice, decimal? PreviousPrice, decimal? ChangePercent, string? Unit);

public record SubmitPriceRequest(
    [Required] int ProductId,
    [Required] int RegionId,
    [Required, Range(0.01, double.MaxValue)] decimal Price,
    [Required] DateOnly EffectiveDate);

/// <summary>
/// Crawler gửi tên thô (Category/SubCategory/Product/Region) thay vì Id có sẵn — server tự
/// find-or-create theo tên (xem InternalController.CrawlIngest), crawler không cần biết trước
/// catalog. <c>ProductUnit</c> (đơn vị mặc định của Product) khác <c>Unit</c> (đơn vị/khối lượng
/// thật của đúng bản ghi giá này, lưu vào PriceEntry.Unit) — 2 field khác nhau nên không trùng tên.
/// </summary>
public record CrawlIngestRequest(
    [Required, MaxLength(200)] string CategoryName,
    [Required, MaxLength(200)] string SubCategoryName,
    [Required, MaxLength(200)] string ProductName,
    [Required, MaxLength(50)] string ProductUnit,
    [Required, MaxLength(200)] string RegionProvinceName,
    [Required, MaxLength(200)] string RegionMarketName,
    [Required, Range(0.01, double.MaxValue)] decimal Price,
    [Required] DateOnly EffectiveDate,
    [MaxLength(300)] string? Unit,
    string? ImageUrl);

public record PriceEntryResponse(int Id, int ProductId, int RegionId, decimal Price, string Source, string Status, DateOnly EffectiveDate, string? Unit)
{
    public static PriceEntryResponse FromEntity(PriceEntry pe) => new(
        pe.Id, pe.ProductId, pe.RegionId, pe.Price, pe.Source.ToString(), pe.Status.ToString(), pe.EffectiveDate, pe.Unit);
}
