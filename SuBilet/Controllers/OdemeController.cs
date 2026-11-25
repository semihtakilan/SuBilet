using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuBilet.Data;
using SuBilet.Dogrulama;
using SuBilet.Models;
using SuBilet.Models.ViewModels;
using SuBilet.Servisler;

namespace SuBilet.Controllers;

// Simüle ödeme sayfası. Yolcu bilgileri onaylanınca rezervasyon TempData ile buraya gelir.
// Gerçek ödeme alınmaz: kart sunucuda doğrulanır ve yalnızca son 4 hanesi saklanır.
[Authorize]
public class OdemeController : Controller
{
    private readonly AppDbContext _context;
    private readonly KoltukDurumServisi _koltukDurumServisi;
    private readonly BiletSatisServisi _biletSatisServisi;

    public OdemeController(
        AppDbContext context, KoltukDurumServisi koltukDurumServisi, BiletSatisServisi biletSatisServisi)
    {
        _context = context;
        _koltukDurumServisi = koltukDurumServisi;
        _biletSatisServisi = biletSatisServisi;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var hazirlik = await RezervasyonuHazirlaAsync();
        if (hazirlik.Yonlendirme != null)
        {
            return hazirlik.Yonlendirme;
        }

        return View(new OdemeViewModel { Rezervasyon = hazirlik.Rezervasyon! });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(OdemeViewModel model)
    {
        var hazirlik = await RezervasyonuHazirlaAsync();
        if (hazirlik.Yonlendirme != null)
        {
            return hazirlik.Yonlendirme;
        }

        model.Rezervasyon = hazirlik.Rezervasyon!;
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var odeme = await _biletSatisServisi.SatinAlAsync(
            KullaniciId()!, hazirlik.Sefer!, model.Rezervasyon.Yolcular,
            KartNumarasiAttribute.SonDortHane(model.KartNumarasi));

        TempData.Remove(BekleyenRezervasyon.TempDataAnahtari);

        // GEÇİCİ: Bilet onay sayfası Kerim'in işi; o gelince ödeme sonrası oraya yönlendirilecek.
        var pnrler = string.Join(", ", odeme.Biletler
            .OrderBy(b => b.KoltukNo)
            .Select(b => $"Koltuk {b.KoltukNo}: {b.PnrKodu}"));
        TempData["Mesaj"] = $"Ödemeniz alındı (kart **** {odeme.KartSonDortHane}). PNR kodlarınız: {pnrler}";
        return RedirectToAction("Index", "Home");
    }

    private string? KullaniciId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    private record Hazirlik(YolcuBilgileriViewModel? Rezervasyon, Sefer? Sefer, IActionResult? Yonlendirme);

    // TempData'daki rezervasyonu okur ve veritabanına göre yeniden kontrol eder: sefer hâlâ satışta mı,
    // koltuklar hâlâ boş mu. Sefer özeti ve fiyat her seferinde veritabanından yeniden alınır.
    // Sorun varsa rezervasyon silinir ve kullanıcı uygun sayfaya yönlendirilir.
    private async Task<Hazirlik> RezervasyonuHazirlaAsync()
    {
        var bekleyen = BekleyenRezervasyonuOku();
        if (bekleyen?.Rezervasyon?.Yolcular == null || bekleyen.KullaniciId != KullaniciId())
        {
            return Yonlendir("Ödeme için bekleyen bir rezervasyon bulunamadı. Lütfen sefer ve koltuk seçimiyle başlayın.",
                RedirectToAction("Index", "Home"));
        }

        var yolcular = bekleyen.Rezervasyon.Yolcular;
        var sefer = await _context.Seferler
            .AsNoTracking()
            .Include(s => s.Firma)
            .Include(s => s.Otobus)
            .Include(s => s.KalkisSehir)
            .Include(s => s.VarisSehir)
            .FirstOrDefaultAsync(s => s.Id == bekleyen.Rezervasyon.SeferId);

        if (sefer?.Otobus == null || sefer.Durum != SeferDurumu.Aktif || sefer.KalkisZamani <= DateTime.Now)
        {
            return Yonlendir("Bu sefer için bilet satışı yapılmıyor.", RedirectToAction("Index", "Home"));
        }

        var (basarili, hataMesaji) = await _koltukDurumServisi.SecimiDogrulaAsync(
            sefer.Id, yolcular.Select(y => (y.KoltukNo, y.Cinsiyet)).ToList());
        if (!basarili)
        {
            return Yonlendir(hataMesaji, RedirectToAction("Koltuklar", "Seferler", new { id = sefer.Id }));
        }

        var rezervasyon = new YolcuBilgileriViewModel
        {
            SeferId = sefer.Id,
            FirmaAdi = sefer.Firma?.Ad ?? string.Empty,
            KalkisYeri = sefer.KalkisSehir?.Ad ?? string.Empty,
            VarisYeri = sefer.VarisSehir?.Ad ?? string.Empty,
            KalkisZamani = sefer.KalkisZamani ?? DateTime.MinValue,
            VarisZamani = sefer.TahminiVarisZamani ?? (sefer.KalkisZamani ?? DateTime.MinValue),
            BirimFiyat = sefer.Fiyat ?? 0m,
            KoltukDuzeni = sefer.Otobus.KoltukDuzeni ?? KoltukDuzeni.IkiArtiIki,
            Yolcular = yolcular.OrderBy(y => y.KoltukNo).ToList()
        };

        return new Hazirlik(rezervasyon, sefer, null);
    }

    // Peek: sayfa yenilenince ya da form hatalı gönderilince rezervasyon kaybolmasın
    private BekleyenRezervasyon? BekleyenRezervasyonuOku()
    {
        if (TempData.Peek(BekleyenRezervasyon.TempDataAnahtari) is not string json)
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<BekleyenRezervasyon>(json);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private Hazirlik Yonlendir(string hata, IActionResult yonlendirme)
    {
        TempData.Remove(BekleyenRezervasyon.TempDataAnahtari);
        TempData["Hata"] = hata;
        return new Hazirlik(null, null, yonlendirme);
    }
}
