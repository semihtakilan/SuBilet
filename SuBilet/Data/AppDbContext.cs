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
    public DbSet<Otobus> Otobusler { get; set; } = default!;
    public DbSet<Sefer> Seferler { get; set; } = default!;
    public DbSet<Odeme> Odemeler { get; set; } = default!;
    public DbSet<Bilet> Biletler { get; set; } = default!;

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Sefer saatleri Türkiye saatiyle tutulur; saat dilimi bilgisi gerekmez.
        // (PostgreSQL'in "timestamp with time zone" tipi yalnızca UTC kabul eder.)
        configurationBuilder.Properties<DateTime>().HaveColumnType("timestamp without time zone");
        configurationBuilder.Properties<DateTime?>().HaveColumnType("timestamp without time zone");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Identity tablolarının yapılandırması için önce üst sınıf çağrılır
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Sehir>(e =>
        {
            e.HasIndex(s => s.PlakaKodu).IsUnique();
            e.HasIndex(s => s.Ad).IsUnique();
            e.HasData(BaslangicVerisi.Sehirler());
        });

        modelBuilder.Entity<Firma>(e =>
        {
            e.HasIndex(f => f.Ad).IsUnique();
            e.HasData(BaslangicVerisi.Firmalar());
        });

        // Firma (1) - Kullanıcı (n): firma silinirse yetkilinin FirmaId'si boşalır
        modelBuilder.Entity<Kullanici>(e =>
        {
            e.HasOne(k => k.Firma)
             .WithMany(f => f.Kullanicilar)
             .HasForeignKey(k => k.FirmaId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // Firma (1) - Otobüs (n). Restrict: otobüsü olan firma silinemez.
        modelBuilder.Entity<Otobus>(e =>
        {
            e.HasIndex(o => o.Plaka).IsUnique();

            e.HasOne(o => o.Firma)
             .WithMany(f => f.Otobusler)
             .HasForeignKey(o => o.FirmaId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Sefer>(e =>
        {
            e.Property(s => s.Fiyat).HasPrecision(10, 2);

            // Arama sorgusu (kalkış + varış + tarih) için indeks
            e.HasIndex(s => new { s.KalkisSehirId, s.VarisSehirId, s.KalkisZamani });

            e.HasOne(s => s.Firma)
             .WithMany(f => f.Seferler)
             .HasForeignKey(s => s.FirmaId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(s => s.Otobus)
             .WithMany(o => o.Seferler)
             .HasForeignKey(s => s.OtobusId)
             .OnDelete(DeleteBehavior.Restrict);

            // Şehir ile iki ayrı ilişki: kalkış ve varış
            e.HasOne(s => s.KalkisSehir)
             .WithMany(sh => sh.KalkisSeferleri)
             .HasForeignKey(s => s.KalkisSehirId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(s => s.VarisSehir)
             .WithMany(sh => sh.VarisSeferleri)
             .HasForeignKey(s => s.VarisSehirId)
             .OnDelete(DeleteBehavior.Restrict);

            // Formdaki kurallar veritabanında da korunsun
            e.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Seferler_FarkliSehir", "\"KalkisSehirId\" <> \"VarisSehirId\"");
                t.HasCheckConstraint("CK_Seferler_VarisKalkistanSonra", "\"TahminiVarisZamani\" > \"KalkisZamani\"");
            });
        });

        modelBuilder.Entity<Odeme>(e =>
        {
            e.Property(o => o.ToplamTutar).HasPrecision(10, 2);

            e.HasOne(o => o.Kullanici)
             .WithMany(k => k.Odemeler)
             .HasForeignKey(o => o.KullaniciId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Bilet>(e =>
        {
            e.Property(b => b.Ucret).HasPrecision(10, 2);
            e.HasIndex(b => b.PnrKodu).IsUnique();

            // Aynı seferde aynı koltuk iki kez satılamaz. İptal edilen biletler sayılmaz;
            // böylece iptal edilen koltuk yeniden satılabilir (PostgreSQL kısmi indeks).
            e.HasIndex(b => new { b.SeferId, b.KoltukNo })
             .IsUnique()
             .HasFilter("\"Durum\" = 0");

            e.HasOne(b => b.Sefer)
             .WithMany(s => s.Biletler)
             .HasForeignKey(b => b.SeferId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(b => b.Kullanici)
             .WithMany(k => k.Biletler)
             .HasForeignKey(b => b.KullaniciId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(b => b.Odeme)
             .WithMany(o => o.Biletler)
             .HasForeignKey(b => b.OdemeId)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
