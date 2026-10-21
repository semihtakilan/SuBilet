using Microsoft.EntityFrameworkCore;
using SuBilet.Models;

namespace SuBilet.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Sehir> Sehirler { get; set; } = default!;
    public DbSet<Firma> Firmalar { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Sehir>(e =>
        {
            e.HasIndex(s => s.PlakaKodu).IsUnique();
            e.HasIndex(s => s.Ad).IsUnique();
        });

        modelBuilder.Entity<Firma>(e =>
        {
            e.HasIndex(f => f.Ad).IsUnique();
        });
    }
}
