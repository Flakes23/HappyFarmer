namespace HappyFarmer.MarketplaceService.Api.Entities;

public class ListingImage
{
    public int Id { get; set; }
    public int ListingId { get; set; }
    public Listing Listing { get; set; } = null!;
    public required string ImageUrl { get; set; }
    public int SortOrder { get; set; }
}
