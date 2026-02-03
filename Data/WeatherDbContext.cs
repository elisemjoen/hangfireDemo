using Microsoft.EntityFrameworkCore;
using Weather.API.Entities;

namespace Weather.API.Data;

public class WeatherDbContext(DbContextOptions<WeatherDbContext> options)
    : DbContext(options)
{
    public DbSet<WeatherEntity> WeatherResults { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<WeatherEntity>(entity =>
        {
            entity.Property(e => e.Time).HasColumnType("datetime2");
            entity.Property(e => e.CreatedAtUtc).HasColumnType("datetime2");
            entity.HasIndex(e => e.Time);
        });
    }
}