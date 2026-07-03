using HappyFarmer.MarketPriceService.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace HappyFarmer.MarketPriceService.Api.Data;

public class MarketPriceDbContext(DbContextOptions<MarketPriceDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Region> Regions => Set<Region>();
    public DbSet<PriceEntry> PriceEntries => Set<PriceEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(p => p.NameVi).HasMaxLength(200);
            entity.Property(p => p.Category).HasMaxLength(100);
            entity.Property(p => p.Unit).HasMaxLength(50);
        });

        modelBuilder.Entity<Region>(entity =>
        {
            entity.Property(r => r.ProvinceName).HasMaxLength(200);
            entity.Property(r => r.MarketName).HasMaxLength(200);
        });

        modelBuilder.Entity<PriceEntry>(entity =>
        {
            entity.Property(pe => pe.Price).HasColumnType("decimal(12,2)");
            entity.Property(pe => pe.Source).HasConversion<string>().HasMaxLength(20);
            entity.Property(pe => pe.Status).HasConversion<string>().HasMaxLength(20);

            entity.HasOne(pe => pe.Product).WithMany().HasForeignKey(pe => pe.ProductId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(pe => pe.Region).WithMany().HasForeignKey(pe => pe.RegionId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(pe => new { pe.ProductId, pe.RegionId, pe.Status, pe.EffectiveDate });
        });
    }
}
