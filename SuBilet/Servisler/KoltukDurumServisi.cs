using Microsoft.EntityFrameworkCore;
using SuBilet.Data;
using SuBilet.Models;
using SuBilet.Models.ViewModels;

namespace SuBilet.Servisler;

public class KoltukDurumServisi
{
    private readonly AppDbContext _context;

    public KoltukDurumServisi(AppDbContext context)
    {
        _context = context;
    }

    public Task<Dictionary<int, Cinsiyet>> DoluKoltuklariGetirAsync(int seferId) =>
            _context.Biletler
                .AsNoTracking()
                .Where(b => b.SeferId == seferId && b.Durum == BiletDurumu.Aktif)
                .ToDictionaryAsync(b => b.KoltukNo, b => b.Cinsiyet);

    // Seçilen koltukları yolcu bilgileri ve ödeme adımlarından önce sunucuda yeniden kontrol eder.
    // Yan koltuk kuralı yalnızca başka bir yolcuya satılmış koltuklar için uygulanır: aynı satın
    // almada yan yana seçilen koltuklar birlikte seyahat eden yolculara aittir, cinsiyetleri
    // farklı olabilir (koltuk haritası ve mobil uygulama da böyle çalışır).
    public async Task<(bool Basarili, string HataMesaji)> SecimiDogrulaAsync(
        int seferId, IReadOnlyList<(int KoltukNo, Cinsiyet Cinsiyet)> secilenKoltuklar)
    {
        if (secilenKoltuklar.Count == 0)
            return (false, "Devam etmek için en az bir koltuk seçmelisiniz.");

        if (secilenKoltuklar.Count > KoltukSecimViewModel.EnFazlaKoltuk)
            return (false, $"Tek seferde en fazla {KoltukSecimViewModel.EnFazlaKoltuk} koltuk seçebilirsiniz.");

        var sefer = await _context.Seferler
            .AsNoTracking()
            .Include(s => s.Otobus)
            .FirstOrDefaultAsync(s => s.Id == seferId);

        if (sefer?.Otobus?.KoltukDuzeni is not { } duzen || sefer.Otobus.KoltukSayisi is not { } koltukSayisi)
            return (false, "Sefer veya otobüs bilgisi bulunamadı.");

        if (secilenKoltuklar.Select(k => k.KoltukNo).Distinct().Count() != secilenKoltuklar.Count)
            return (false, "Aynı koltuk birden fazla kez seçilemez.");

        var doluKoltuklar = await DoluKoltuklariGetirAsync(seferId);

        foreach (var (koltukNo, cinsiyet) in secilenKoltuklar)
        {
            if (koltukNo < 1 || koltukNo > koltukSayisi)
                return (false, $"{koltukNo} numaralı koltuk bu otobüste bulunmuyor.");

            if (cinsiyet is not (Cinsiyet.Erkek or Cinsiyet.Kadin))
                return (false, $"{koltukNo} numaralı koltuk için geçerli bir cinsiyet seçilmelidir.");

            if (doluKoltuklar.ContainsKey(koltukNo))
                return (false, $"{koltukNo} numaralı koltuk az önce doldu. Lütfen başka bir koltuk seçin.");

            var yanKoltuk = KoltukPlani.YanKoltuk(koltukNo, duzen, koltukSayisi);
            if (yanKoltuk.HasValue &&
                doluKoltuklar.TryGetValue(yanKoltuk.Value, out var yanCinsiyet) &&
                yanCinsiyet != cinsiyet)
            {
                return (false, $"{koltukNo} numaralı koltuğun yanı farklı cinsiyetten bir yolcuya satılmış.");
            }
        }

        return (true, string.Empty);
    }
}