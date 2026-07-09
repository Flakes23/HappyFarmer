using HappyFarmer.MarketplaceService.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HappyFarmer.MarketplaceService.Api.Data;

public class MarketplaceDbContext(DbContextOptions<MarketplaceDbContext> options) : DbContext(options)
{
    public DbSet<Listing> Listings => Set<Listing>();
    public DbSet<ListingImage> ListingImages => Set<ListingImage>();
    public DbSet<BuyRequest> BuyRequests => Set<BuyRequest>();
    public DbSet<Interest> Interests => Set<Interest>();
    public DbSet<Message> Messages => Set<Message>();

    /// <summary>
    /// SQL Server "datetime2" không lưu Kind — mọi giá trị ghi vào là DateTime.UtcNow (Kind=Utc)
    /// nhưng đọc ra lại là Kind=Unspecified, khiến System.Text.Json serialize thiếu hậu tố "Z"
    /// và frontend hiểu nhầm thành giờ local (lệch múi giờ, vd sai 7 tiếng ở VN). Ép Kind=Utc lại
    /// mỗi khi đọc để JSON luôn trả đúng dạng UTC ISO-8601, áp dụng cho mọi property DateTime
    /// trong toàn bộ DbContext này thay vì phải cấu hình riêng từng field.
    /// </summary>
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
        configurationBuilder.Properties<DateTime?>().HaveConversion<NullableUtcDateTimeConverter>();
    }

    private class UtcDateTimeConverter() : ValueConverter<DateTime, DateTime>(
        v => v,
        v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

    private class NullableUtcDateTimeConverter() : ValueConverter<DateTime?, DateTime?>(
        v => v,
        v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Listing>(entity =>
        {
            entity.Property(l => l.Quantity).HasColumnType("decimal(12,2)");
            entity.Property(l => l.PricePerUnit).HasColumnType("decimal(12,2)");
            entity.Property(l => l.Unit).HasMaxLength(50);
            entity.Property(l => l.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(l => l.FarmerName).HasMaxLength(200);

            entity.HasIndex(l => new { l.Status, l.ProductId, l.RegionId });
            entity.HasIndex(l => l.FarmerId);

            entity.HasMany(l => l.Images).WithOne(i => i.Listing)
                .HasForeignKey(i => i.ListingId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ListingImage>(entity =>
        {
            entity.Property(i => i.ImageUrl).HasMaxLength(2048);
        });

        modelBuilder.Entity<BuyRequest>(entity =>
        {
            entity.Property(br => br.DesiredQuantity).HasColumnType("decimal(12,2)");
            entity.Property(br => br.Unit).HasMaxLength(50);
            entity.Property(br => br.MaxPricePerUnit).HasColumnType("decimal(12,2)");
            entity.Property(br => br.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(br => br.BuyerName).HasMaxLength(200);

            entity.HasIndex(br => new { br.Status, br.ProductId, br.RegionId });
            entity.HasIndex(br => br.BuyerId);
        });

        modelBuilder.Entity<Interest>(entity =>
        {
            entity.Property(i => i.Status).HasConversion<string>().HasMaxLength(20);

            entity.HasOne(i => i.Listing).WithMany()
                .HasForeignKey(i => i.ListingId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(i => i.BuyRequest).WithMany()
                .HasForeignKey(i => i.BuyRequestId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(i => i.InitiatorUserId);
            entity.HasIndex(i => i.TargetUserId);
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.Property(m => m.Body).HasMaxLength(4000).IsRequired();

            entity.HasOne(m => m.Interest).WithMany()
                .HasForeignKey(m => m.InterestId).OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(m => new { m.InterestId, m.Id });
        });
    }
}
