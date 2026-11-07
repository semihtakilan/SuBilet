using System.ComponentModel.DataAnnotations;

namespace SuBilet.Models.ViewModels;

// Sefer sonuç sayfasındaki filtre ve sıralama seçimleri.
// Sorgu dizesinden okunur: ?FirmaIdleri=1&FirmaIdleri=3&Saatler=Sabah&Siralama=FiyatArtan
public class SeferFiltresi
{
    // Sorgu dizesindeki alan adları; adres kurarken arama alanlarından ayırmak için kullanılır.
    public static readonly string[] AlanAdlari = { nameof(FirmaIdleri), nameof(Saatler), nameof(Siralama) };

    [Display(Name = "Firma")]
    public List<int> FirmaIdleri { get; set; } = new();

    [Display(Name = "Kalkış saati")]
    public List<SaatAraligi> Saatler { get; set; } = new();

    [Display(Name = "Sırala")]
    public SeferSiralama Siralama { get; set; } = SeferSiralama.KalkisErken;

    public int SeciliFiltreSayisi => FirmaIdleri.Count + Saatler.Count;
}

// Kalkış saatine göre gün dört parçaya bölünür. Başlangıç saati dahil, bitiş saati hariçtir.
public enum SaatAraligi
{
    [Display(Name = "Gece", Description = "00:00 – 06:00")]
    Gece = 0,

    [Display(Name = "Sabah", Description = "06:00 – 12:00")]
    Sabah = 1,

    [Display(Name = "Öğle", Description = "12:00 – 18:00")]
    Ogle = 2,

    [Display(Name = "Akşam", Description = "18:00 – 24:00")]
    Aksam = 3
}

public enum SeferSiralama
{
    [Display(Name = "Erken kalkış")]
    KalkisErken = 0,

    [Display(Name = "Geç kalkış")]
    KalkisGec = 1,

    [Display(Name = "En düşük fiyat")]
    FiyatArtan = 2,

    [Display(Name = "En yüksek fiyat")]
    FiyatAzalan = 3
}

public static class SaatAraligiUzantilari
{
    // Aralığın kapsadığı saatler: Sabah → 6, 7, 8, 9, 10, 11
    public static IEnumerable<int> Saatleri(this SaatAraligi aralik) => Enumerable.Range((int)aralik * 6, 6);

    public static SaatAraligi Bul(int saat) => (SaatAraligi)(saat / 6);
}
