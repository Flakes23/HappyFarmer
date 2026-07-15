using HappyFarmer.AiAdvisoryService.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HappyFarmer.AiAdvisoryService.Api.Data;

public class AiAdvisoryDbContext(DbContextOptions<AiAdvisoryDbContext> options) : DbContext(options)
{
    public DbSet<ChatSession> ChatSessions => Set<ChatSession>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<CropProfile> CropProfiles => Set<CropProfile>();
    public DbSet<HarvestPrediction> HarvestPredictions => Set<HarvestPrediction>();
    public DbSet<DiseaseDetection> DiseaseDetections => Set<DiseaseDetection>();

    /// <summary>
    /// SQL Server "datetime2" không lưu Kind — mọi giá trị ghi vào là DateTime.UtcNow (Kind=Utc)
    /// nhưng đọc ra lại là Kind=Unspecified, khiến System.Text.Json serialize thiếu hậu tố "Z"
    /// và frontend hiểu nhầm thành giờ local (lệch múi giờ). Ép Kind=Utc lại mỗi khi đọc, giống
    /// fix đã áp dụng ở MarketplaceDbContext.
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
        modelBuilder.Entity<ChatSession>(entity =>
        {
            entity.Property(s => s.Title).HasMaxLength(200);
            entity.Property(s => s.Status).HasConversion<string>().HasMaxLength(20);
            entity.HasIndex(s => s.FarmerId);
        });

        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.Property(m => m.Sender).HasConversion<string>().HasMaxLength(10);
            entity.Property(m => m.Content).HasMaxLength(4000).IsRequired();

            entity.HasOne<ChatSession>().WithMany()
                .HasForeignKey(m => m.SessionId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(m => new { m.SessionId, m.Id });
        });

        modelBuilder.Entity<CropProfile>(entity =>
        {
            entity.Property(c => c.CropTypeCode).HasMaxLength(50).IsRequired();
            entity.Property(c => c.CropNameVi).HasMaxLength(100).IsRequired();
            entity.HasIndex(c => c.CropNameVi).IsUnique();
        });

        modelBuilder.Entity<HarvestPrediction>(entity =>
        {
            entity.Property(h => h.CropType).HasMaxLength(100).IsRequired();
            entity.Property(h => h.Location).HasMaxLength(100).IsRequired();
            entity.Property(h => h.ConfidenceLevel).HasMaxLength(20).IsRequired();
            entity.Property(h => h.ReasoningText).HasMaxLength(2000).IsRequired();
            entity.HasIndex(h => h.FarmerId);
        });

        modelBuilder.Entity<DiseaseDetection>(entity =>
        {
            entity.Property(d => d.ImageUrl).HasMaxLength(500).IsRequired();
            entity.Property(d => d.CropTypeHint).HasMaxLength(100);
            entity.Property(d => d.Note).HasMaxLength(1000);
            entity.Property(d => d.IdentifiedCropType).HasMaxLength(100).IsRequired();
            entity.Property(d => d.DiseaseName).HasMaxLength(200);
            entity.Property(d => d.Severity).HasMaxLength(20);
            entity.Property(d => d.Description).HasMaxLength(2000).IsRequired();
            entity.HasIndex(d => d.FarmerId);
        });
    }
}
