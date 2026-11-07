using Microsoft.AspNetCore.Http.Extensions;

namespace SuBilet.Models.ViewModels;

// Sonuç sayfasındaki filtre paneli ve sıralama çubuğunun verisi.
// SeferFiltreleme.FiltrePaneliHazirlaAsync ile doldurulur.
public class SeferFiltrePaneli
{
    public SeferFiltresi Filtre { get; set; } = new();

    // Aramada çıkan firmalar ve her birinin sefer sayısı
    public List<FiltreSecenegi> Firmalar { get; set; } = new();

    // Dört saat aralığı ve her birindeki sefer sayısı
    public List<FiltreSecenegi> Saatler { get; set; } = new();

    // Filtresiz arama sonucundaki sefer sayısı
    public int ToplamSefer { get; set; }

    // Filtreler uygulandıktan sonra kalan sefer sayısı
    public int GosterilenSefer { get; set; }

    public IEnumerable<FiltreSecenegi> Siralamalar =>
        Enum.GetValues<SeferSiralama>()
            .Select(s => new FiltreSecenegi(s.ToString(), s.Ad(), null, 0, s == Filtre.Siralama));

    // Sorgu dizesindeki arama alanları (kalkış, varış, tarih...). Filtre formunda gizli alan olarak taşınır,
    // böylece filtre değişince arama kaybolmaz.
    public static IEnumerable<KeyValuePair<string, string>> AramaAlanlari(HttpRequest istek)
    {
        var filtreAlanlari = new HashSet<string>(SeferFiltresi.AlanAdlari, StringComparer.OrdinalIgnoreCase);
        foreach (var (ad, degerler) in istek.Query)
        {
            if (filtreAlanlari.Contains(ad)) continue;
            foreach (var deger in degerler)
            {
                yield return new(ad, deger ?? "");
            }
        }
    }

    // Aynı aramanın, verilen filtrelerle adresi. Sıralama korunur.
    public string Adres(HttpRequest istek, IEnumerable<int> firmaIdleri, IEnumerable<SaatAraligi> saatler)
    {
        var sorgu = new QueryBuilder(AramaAlanlari(istek));
        foreach (var id in firmaIdleri) sorgu.Add(nameof(SeferFiltresi.FirmaIdleri), id.ToString());
        foreach (var saat in saatler) sorgu.Add(nameof(SeferFiltresi.Saatler), saat.ToString());
        if (Filtre.Siralama != default) sorgu.Add(nameof(SeferFiltresi.Siralama), Filtre.Siralama.ToString());
        return istek.PathBase + istek.Path + sorgu.ToQueryString();
    }

    public string FirmaKaldirAdresi(HttpRequest istek, int firmaId) =>
        Adres(istek, Filtre.FirmaIdleri.Where(id => id != firmaId), Filtre.Saatler);

    public string SaatKaldirAdresi(HttpRequest istek, SaatAraligi saat) =>
        Adres(istek, Filtre.FirmaIdleri, Filtre.Saatler.Where(s => s != saat));

    public string TemizleAdresi(HttpRequest istek) => Adres(istek, [], []);
}

// Deger: formdan gönderilen değer (firma Id'si ya da "Sabah" gibi aralık adı)
public record FiltreSecenegi(string Deger, string Ad, string? Aciklama, int SeferSayisi, bool Secili);
