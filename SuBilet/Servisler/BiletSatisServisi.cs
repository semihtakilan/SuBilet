using SuBilet.Data;
using SuBilet.Models;
using SuBilet.Models.ViewModels;

namespace SuBilet.Servisler;

// GEÇİCİ (6. hafta): Simüle ödeme sayfası uçtan uca çalışsın diye en küçük satış.
// Furkan'ın işi (ödeme ve bilet kayıtlarının tek transaction içinde oluşturulması,
// aynı koltuğun eşzamanlı satışının engellenmesi) bu metodun içini değiştirecek;
// ödeme sayfası yalnızca SatinAlAsync'i çağırır, imza aynı kalabilir.
public class BiletSatisServisi
{
    private readonly AppDbContext _context;

    public BiletSatisServisi(AppDbContext context)
    {
        _context = context;
    }

    // Kart bilgilerinden yalnızca son 4 hane gelir ve saklanır
    public async Task<Odeme> SatinAlAsync(
        string kullaniciId, Sefer sefer, IReadOnlyList<YolcuBilgisiItemViewModel> yolcular, string kartSonDortHane)
    {
        var simdi = DateTime.Now;
        var fiyat = sefer.Fiyat ?? 0m;

        var odeme = new Odeme
        {
            KullaniciId = kullaniciId,
            ToplamTutar = fiyat * yolcular.Count,
            KartSonDortHane = kartSonDortHane,
            OdemeTarihi = simdi,
            Durum = OdemeDurumu.Basarili,
            Biletler = yolcular
                .Select(y => new Bilet
                {
                    PnrKodu = PnrUretici.Uret(),
                    SeferId = sefer.Id,
                    KullaniciId = kullaniciId,
                    KoltukNo = y.KoltukNo,
                    YolcuAd = y.YolcuAd,
                    YolcuSoyad = y.YolcuSoyad,
                    TcKimlikNo = y.TcKimlikNo,
                    Cinsiyet = y.Cinsiyet,
                    Ucret = fiyat,
                    Durum = BiletDurumu.Aktif,
                    SatisTarihi = simdi
                })
                .ToList()
        };

        _context.Odemeler.Add(odeme);
        await _context.SaveChangesAsync();
        return odeme;
    }
}
