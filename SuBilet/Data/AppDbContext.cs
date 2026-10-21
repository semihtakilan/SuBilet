using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SuBilet.Models;

namespace SuBilet.Data;

// IdentityDbContext: AspNetUsers, AspNetRoles, AspNetUserRoles... tablolarını da içerir.
public class AppDbContext : IdentityDbContext<Kullanici>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Sehir> Sehirler { get; set; } = default!;
    public DbSet<Firma> Firmalar { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Identity tablolarının yapılandırması için önce üst sınıf çağrılır
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

        // Firma (1) - Kullanıcı (n): firma silinirse yetkilinin FirmaId'si boşalır
        modelBuilder.Entity<Kullanici>(e =>
        {
            e.HasOne(k => k.Firma)
             .WithMany(f => f.Kullanicilar)
             .HasForeignKey(k => k.FirmaId)
             .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
