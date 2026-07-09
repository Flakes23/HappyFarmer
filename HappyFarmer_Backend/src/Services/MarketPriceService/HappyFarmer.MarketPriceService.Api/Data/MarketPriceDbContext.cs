using HappyFarmer.MarketPriceService.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace HappyFarmer.MarketPriceService.Api.Data;

public class MarketPriceDbContext(DbContextOptions<MarketPriceDbContext> options) : DbContext(options)
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<SubCategory> SubCategories => Set<SubCategory>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Region> Regions => Set<Region>();
    public DbSet<PriceEntry> PriceEntries => Set<PriceEntry>();

    /// <summary>
    /// Category/SubCategory/Product/Region không seed cố định — crawler (src/Tools/HappyFarmer.
    /// MarketPriceCrawler) tự tạo (find-or-create) qua endpoint crawl-ingest khi lần đầu gặp một
    /// tên mới, khớp theo Name/NameVi (xem InternalController.CrawlIngest).
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.Property(c => c.Name).HasMaxLength(200);
            entity.HasIndex(c => c.Name).IsUnique();
        });

        modelBuilder.Entity<SubCategory>(entity =>
        {
            entity.Property(sc => sc.Name).HasMaxLength(200);
            entity.HasOne(sc => sc.Category).WithMany().HasForeignKey(sc => sc.CategoryId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(sc => new { sc.CategoryId, sc.Name }).IsUnique();
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(p => p.NameVi).HasMaxLength(200);
            entity.Property(p => p.Unit).HasMaxLength(50);
            entity.HasOne(p => p.SubCategory).WithMany().HasForeignKey(p => p.SubCategoryId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(p => p.NameVi).IsUnique();
        });

        modelBuilder.Entity<Region>(entity =>
        {
            entity.Property(r => r.ProvinceName).HasMaxLength(200);
            entity.Property(r => r.MarketName).HasMaxLength(200);
            entity.HasIndex(r => new { r.ProvinceName, r.MarketName }).IsUnique();
        });

        modelBuilder.Entity<PriceEntry>(entity =>
        {
            entity.Property(pe => pe.Price).HasColumnType("decimal(12,2)");
            entity.Property(pe => pe.Unit).HasMaxLength(300);
            entity.Property(pe => pe.Source).HasConversion<string>().HasMaxLength(20);
            entity.Property(pe => pe.Status).HasConversion<string>().HasMaxLength(20);

            entity.HasOne(pe => pe.Product).WithMany().HasForeignKey(pe => pe.ProductId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(pe => pe.Region).WithMany().HasForeignKey(pe => pe.RegionId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(pe => new { pe.ProductId, pe.RegionId, pe.Status, pe.EffectiveDate });
        });
    }
}
