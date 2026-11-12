using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuBilet.Data;
using SuBilet.Models;
using SuBilet.Models.ViewModels;
using SuBilet.Servisler;

namespace SuBilet.Controllers;

public class SeferlerController : Controller
{
    private readonly AppDbContext _context;

    public SeferlerController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index(SeferAramaViewModel arama, SeferFiltresi filtre)
    {
        if (!arama.KalkisSehirId.HasValue || !arama.VarisSehirId.HasValue || !arama.Tarih.HasValue)
        {
            return RedirectToAction("Index", "Home");
        }

        var aramaTarihi = arama.Tarih.Value.Date;
        var ertesiGun = aramaTarihi.AddDays(1);

        // Bugün için aramada kalkmış seferler listelenmez; aralık şimdiki saatten başlar.
        var simdi = DateTime.Now;
        var baslangic = aramaTarihi > simdi ? aramaTarihi : simdi;

        var aramaSorgusu = _context.Seferler
            .AsNoTracking()
            .Include(s => s.Firma)
            .Include(s => s.Otobus)
            .Include(s => s.KalkisSehir)
            .Include(s => s.VarisSehir)
            .Include(s => s.Biletler)
            .Where(s => s.Durum == SeferDurumu.Aktif &&
                        s.KalkisSehirId == arama.KalkisSehirId.Value &&
                        s.VarisSehirId == arama.VarisSehirId.Value &&
                        s.KalkisZamani >= baslangic &&
                        s.KalkisZamani < ertesiGun);

        var filtrePaneli = await aramaSorgusu.FiltrePaneliHazirlaAsync(filtre);

        var seferler = await aramaSorgusu
            .Filtrele(filtre)
            .Sirala(filtre.Siralama)
            .Select(s => new SeferKartViewModel
            {
                Id = s.Id,
                FirmaAdi = s.Firma != null ? s.Firma.Ad : "",
                FirmaLogoUrl = s.Firma != null ? s.Firma.LogoUrl : null,
                KalkisZamani = s.KalkisZamani ?? DateTime.MinValue,
                VarisZamani = s.TahminiVarisZamani ?? (s.KalkisZamani ?? DateTime.MinValue),
                Fiyat = s.Fiyat ?? 0m,
                KoltukDuzeni = s.Otobus != null ? (s.Otobus.KoltukDuzeni ?? default) : default,
                Ozellikler = s.Otobus != null ? s.Otobus.Ozellikler : new List<string>(),
                BosKoltukSayisi = (s.Otobus != null ? (s.Otobus.KoltukSayisi ?? 0) : 0) - s.Biletler.Count
            })
            .ToListAsync();

        var kalkisSehir = await _context.Sehirler.FindAsync(arama.KalkisSehirId.Value);
        var varisSehir = await _context.Sehirler.FindAsync(arama.VarisSehirId.Value);

        var model = new SeferAramaSonucViewModel
        {
            KalkisSehirId = arama.KalkisSehirId,
            VarisSehirId = arama.VarisSehirId,
            KalkisYeri = kalkisSehir?.Ad ?? "",
            VarisYeri = varisSehir?.Ad ?? "",
            SeferTarihi = arama.Tarih,
            Filtre = filtre,
            FiltrePaneli = filtrePaneli,
            Seferler = seferler
        };

        return View(model);
    }

    // Seçilen seferin koltuk haritası: /Seferler/Koltuklar/5
    [HttpGet]
    public async Task<IActionResult> Koltuklar(int id)
    {
        var sefer = await _context.Seferler
            .AsNoTracking()
            .Include(s => s.Firma)
            .Include(s => s.Otobus)
            .Include(s => s.KalkisSehir)
            .Include(s => s.VarisSehir)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (sefer == null || sefer.Otobus == null)
        {
            return NotFound();
        }

        if (sefer.Durum != SeferDurumu.Aktif || sefer.KalkisZamani <= DateTime.Now)
        {
            TempData["Hata"] = "Bu sefer için bilet satışı yapılmıyor.";
            return RedirectToAction("Index", "Home");
        }

        var model = new KoltukSecimViewModel
        {
            SeferId = sefer.Id,
            FirmaAdi = sefer.Firma?.Ad ?? "",
            KalkisSehirId = sefer.KalkisSehirId ?? 0,
            VarisSehirId = sefer.VarisSehirId ?? 0,
            KalkisYeri = sefer.KalkisSehir?.Ad ?? "",
            VarisYeri = sefer.VarisSehir?.Ad ?? "",
            KalkisZamani = sefer.KalkisZamani ?? DateTime.MinValue,
            VarisZamani = sefer.TahminiVarisZamani ?? (sefer.KalkisZamani ?? DateTime.MinValue),
            Fiyat = sefer.Fiyat ?? 0m,
            KoltukDuzeni = sefer.Otobus.KoltukDuzeni ?? KoltukDuzeni.IkiArtiIki,
            KoltukSayisi = sefer.Otobus.KoltukSayisi ?? 0,
            DoluKoltuklar = await DoluKoltuklariGetirAsync(sefer.Id)
        };

        return View(model);
    }

    // GEÇİCİ: 5. haftada Furkan'ın koltuk durumu servisi gelince onunla değiştirilecek.
    // İptal edilen biletin koltuğu yeniden satılabildiği için boş sayılır.
    private Task<Dictionary<int, Cinsiyet>> DoluKoltuklariGetirAsync(int seferId) =>
        _context.Biletler
            .AsNoTracking()
            .Where(b => b.SeferId == seferId && b.Durum == BiletDurumu.Aktif)
            .ToDictionaryAsync(b => b.KoltukNo, b => b.Cinsiyet);
}