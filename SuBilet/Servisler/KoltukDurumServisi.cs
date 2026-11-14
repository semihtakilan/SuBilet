using Microsoft.EntityFrameworkCore;
using SuBilet.Data;
using SuBilet.Models;

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

    public async Task<(bool Basarili, string HataMesaji)> SecimiDogrulaAsync(
        int seferId, IReadOnlyList<(int KoltukNo, Cinsiyet Cinsiyet)> secilenKoltuklar)
    {
        if (secilenKoltuklar.Count is < 1 or > 5)
            return (false, "1 ile 5 arasında koltuk seçebilirsiniz.");

        var sefer = await _context.Seferler
            .AsNoTracking()
            .Include(s => s.Otobus)
            .FirstOrDefaultAsync(s => s.Id == seferId);

        if (sefer?.Otobus?.KoltukDuzeni is not { } duzen || sefer.Otobus.KoltukSayisi is not { } koltukSayisi)
            return (false, "Sefer veya otobüs bilgisi bulunamadı.");

        if (secilenKoltuklar.Select(k => k.KoltukNo).Distinct().Count() != secilenKoltuklar.Count)
            return (false, "Aynı koltuk birden fazla kez seçilemez.");

        var doluKoltuklar = await DoluKoltuklariGetirAsync(seferId);
        var secilenSozluk = secilenKoltuklar.ToDictionary(k => k.KoltukNo, k => k.Cinsiyet);

        foreach (var (koltukNo, cinsiyet) in secilenKoltuklar)
        {
            if (koltukNo < 1 || koltukNo > koltukSayisi || doluKoltuklar.ContainsKey(koltukNo))
                return (false, $"{koltukNo} numaralı koltuk seçilemez veya dolu.");

            int? yanKoltuk = KoltukPlani.YanKoltuk(koltukNo, duzen, koltukSayisi);
            if (yanKoltuk.HasValue)
            {
                if (doluKoltuklar.TryGetValue(yanKoltuk.Value, out var doluCinsiyet) && doluCinsiyet != cinsiyet)
                    return (false, $"{koltukNo} numaralı koltuğun yanı farklı cinsiyete satılmış.");

                if (secilenSozluk.TryGetValue(yanKoltuk.Value, out var seciliYanCinsiyet) && seciliYanCinsiyet != cinsiyet)
                    return (false, $"Yan yana seçilen {koltukNo} ve {yanKoltuk.Value} koltukları farklı cinsiyet olamaz.");
            }
        }

        return (true, string.Empty);
    }
}