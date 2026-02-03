using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PocMfa.Domain;

namespace PocMfa.Infrastructure;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(rt => rt.Id);
            entity.HasIndex(rt => rt.TokenHash).IsUnique();
            entity.HasIndex(rt => new { rt.UserId, rt.ExpiresAt });
            entity.Property(rt => rt.TokenHash).HasMaxLength(512);
        });

        builder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(al => al.Id);
            entity.HasIndex(al => new { al.UserId, al.CreatedAt });
            entity.Property(al => al.Action).HasMaxLength(128);
        });
    }
}
