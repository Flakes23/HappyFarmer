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

        // Danh mục khởi điểm cho MarketPriceCrawler (src/Tools/HappyFarmer.MarketPriceCrawler) —
        // vùng thật ứng với từng nguồn crawl (không gộp "toàn quốc"). Dùng dải Id 101+ để không
        // đụng dữ liệu test thủ công đã có sẵn trong DB (Products/Regions Id 1-3/1-2). "Cà chua"
        // đã tồn tại sẵn (Id=1) nên không seed trùng — crawler tự khớp theo NameVi qua API, không
        // phụ thuộc Id cố định.
        modelBuilder.Entity<Region>().HasData(
            new Region { Id = 101, ProvinceName = "Vĩnh Long", MarketName = "Chợ Vĩnh Long" },
            new Region { Id = 102, ProvinceName = "TP. Hồ Chí Minh", MarketName = "Chợ đầu mối (tổng hợp)" },
            new Region { Id = 103, ProvinceName = "Gia Lai", MarketName = "Giá tham khảo tỉnh Gia Lai" },
            new Region { Id = 104, ProvinceName = "Đồng Nai", MarketName = "Giá tham khảo tỉnh Đồng Nai" },
            new Region { Id = 105, ProvinceName = "Đắk Nông", MarketName = "Giá tham khảo tỉnh Đắk Nông" },
            new Region { Id = 106, ProvinceName = "Đắk Lắk", MarketName = "Giá tham khảo tỉnh Đắk Lắk" },
            new Region { Id = 107, ProvinceName = "Lâm Đồng", MarketName = "Giá tham khảo tỉnh Lâm Đồng" }
        );

        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 101, NameVi = "Xà lách", Category = "Rau củ quả", Unit = "kg" },
            new Product { Id = 102, NameVi = "Rau diếp cá", Category = "Rau củ quả", Unit = "kg" },
            new Product { Id = 103, NameVi = "Cải ngọt", Category = "Rau củ quả", Unit = "kg" },
            new Product { Id = 104, NameVi = "Cải bẹ xanh", Category = "Rau củ quả", Unit = "kg" },
            new Product { Id = 105, NameVi = "Rau muống", Category = "Rau củ quả", Unit = "kg" },
            new Product { Id = 106, NameVi = "Bí đao", Category = "Rau củ quả", Unit = "kg" },
            new Product { Id = 107, NameVi = "Dưa leo", Category = "Rau củ quả", Unit = "kg" },
            new Product { Id = 108, NameVi = "Hành lá", Category = "Rau củ quả", Unit = "kg" },
            new Product { Id = 109, NameVi = "Nấm rơm", Category = "Rau củ quả", Unit = "kg" },
            new Product { Id = 110, NameVi = "Đậu bắp", Category = "Rau củ quả", Unit = "kg" },
            new Product { Id = 111, NameVi = "Hồ tiêu", Category = "Nông sản công nghiệp", Unit = "kg" },
            new Product { Id = 112, NameVi = "Cà phê", Category = "Nông sản công nghiệp", Unit = "kg" }
        );
    }
}
