using HappyFarmer.AuthService.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace HappyFarmer.AuthService.Api.Data;

public class AuthDbContext(DbContextOptions<AuthDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(u => u.FullName).HasMaxLength(200);
            entity.Property(u => u.PhoneNumber).HasMaxLength(20);
            entity.Property(u => u.Email).HasMaxLength(256);
            entity.Property(u => u.Role).HasConversion<string>().HasMaxLength(20);
            entity.HasIndex(u => u.PhoneNumber).IsUnique().HasFilter("[PhoneNumber] IS NOT NULL");
            entity.HasIndex(u => u.Email).IsUnique().HasFilter("[Email] IS NOT NULL");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.Property(rt => rt.TokenHash).HasMaxLength(512);
            entity.HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}