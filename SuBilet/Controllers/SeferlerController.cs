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

        if (arama.KalkisSehirId == arama.VarisSehirId)
        {
            TempData["Hata"] = "Kalkış ve varış şehri aynı olamaz.";
            return RedirectToAction("Index", "Home");
        }

        if (arama.Tarih.Value.Date < DateTime.Today)
        {
            TempData["Hata"] = "Geçmiş bir tarih için sefer araması yapılamaz.";
            return RedirectToAction("Index", "Home");
        }

        var aramaTarihi = arama.Tarih.Value.Date;
        var ertesiGun = aramaTarihi.AddDays(1);

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
                        s.KalkisZamani >= aramaTarihi &&
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
                BosKoltukSayisi = (s.Otobus != null ? (s.Otobus.KoltukSayisi ?? 0) : 0)
                                   - s.Biletler.Count(b => b.Durum != BiletDurumu.Iptal)
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
}