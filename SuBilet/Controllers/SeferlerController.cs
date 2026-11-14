using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
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

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> YolcuBilgileri(int id, List<SecilenKoltukInput> koltuklar)
    {
        var sefer = await SeferGetirAsync(id);
        if (sefer == null || sefer.Otobus == null)
        {
            return NotFound();
        }

        if (sefer.Durum != SeferDurumu.Aktif || sefer.KalkisZamani <= DateTime.Now)
        {
            TempData["Hata"] = "Bu sefer için bilet satışı yapılmıyor.";
            return RedirectToAction("Index", "Home");
        }

        var secimHatasi = await KoltukSecimleriniDogrulaAsync(sefer, koltuklar);
        if (secimHatasi != null)
        {
            TempData["Hata"] = secimHatasi;
            return RedirectToAction(nameof(Koltuklar), new { id });
        }

        var model = SeferOzetindenModelOlustur(sefer);
        model.Yolcular = koltuklar
            .OrderBy(k => k.KoltukNo)
            .Select(k => new YolcuBilgisiItemViewModel
            {
                KoltukNo = k.KoltukNo,
                Cinsiyet = k.Cinsiyet
            })
            .ToList();

        return View(model);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> YolcuBilgileri(YolcuBilgileriViewModel model)
    {
        var sefer = await SeferGetirAsync(model.SeferId);
        if (sefer == null || sefer.Otobus == null)
        {
            return NotFound();
        }

        if (sefer.Durum != SeferDurumu.Aktif || sefer.KalkisZamani <= DateTime.Now)
        {
            TempData["Hata"] = "Bu sefer için bilet satışı yapılmıyor.";
            return RedirectToAction("Index", "Home");
        }

        var secilenKoltuklar = model.Yolcular
            .Select(y => new SecilenKoltukInput { KoltukNo = y.KoltukNo, Cinsiyet = y.Cinsiyet })
            .ToList();

        var secimHatasi = await KoltukSecimleriniDogrulaAsync(sefer, secilenKoltuklar);
        if (secimHatasi != null)
        {
            TempData["Hata"] = secimHatasi;
            return RedirectToAction(nameof(Koltuklar), new { id = model.SeferId });
        }

        var kullanilanTcler = new HashSet<string>();
        for (int i = 0; i < model.Yolcular.Count; i++)
        {
            var yolcu = model.Yolcular[i];
            yolcu.YolcuAd = yolcu.YolcuAd?.Trim() ?? string.Empty;
            yolcu.YolcuSoyad = yolcu.YolcuSoyad?.Trim() ?? string.Empty;
            yolcu.TcKimlikNo = yolcu.TcKimlikNo?.Trim() ?? string.Empty;

            if (!string.IsNullOrEmpty(yolcu.TcKimlikNo))
            {
                if (!kullanilanTcler.Add(yolcu.TcKimlikNo))
                {
                    ModelState.AddModelError($"Yolcular[{i}].TcKimlikNo", "Aynı T.C. kimlik numarası birden fazla koltuk için kullanılamaz.");
                }
                else
                {
                    var seferdeBiletiVar = await _context.Biletler.AnyAsync(b =>
                        b.SeferId == model.SeferId &&
                        b.Durum == BiletDurumu.Aktif &&
                        b.TcKimlikNo == yolcu.TcKimlikNo);

                    if (seferdeBiletiVar)
                    {
                        ModelState.AddModelError($"Yolcular[{i}].TcKimlikNo", "Bu T.C. kimlik numarası ile bu seferde zaten aktif bir bilet bulunuyor.");
                    }
                }
            }
        }

        var guncelOzet = SeferOzetindenModelOlustur(sefer);
        model.FirmaAdi = guncelOzet.FirmaAdi;
        model.KalkisYeri = guncelOzet.KalkisYeri;
        model.VarisYeri = guncelOzet.VarisYeri;
        model.KalkisZamani = guncelOzet.KalkisZamani;
        model.VarisZamani = guncelOzet.VarisZamani;
        model.BirimFiyat = guncelOzet.BirimFiyat;
        model.KoltukDuzeni = guncelOzet.KoltukDuzeni;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        TempData["BekleyenRezervasyon"] = JsonSerializer.Serialize(model);
        TempData["Mesaj"] = "Yolcu bilgileri doğrulandı. Ödeme adımına geçmeye hazır.";
        return View(model);
    }

    private Task<Sefer?> SeferGetirAsync(int seferId) =>
        _context.Seferler
            .AsNoTracking()
            .Include(s => s.Firma)
            .Include(s => s.Otobus)
            .Include(s => s.KalkisSehir)
            .Include(s => s.VarisSehir)
            .FirstOrDefaultAsync(s => s.Id == seferId);

    private static YolcuBilgileriViewModel SeferOzetindenModelOlustur(Sefer sefer) =>
        new()
        {
            SeferId = sefer.Id,
            FirmaAdi = sefer.Firma?.Ad ?? string.Empty,
            KalkisYeri = sefer.KalkisSehir?.Ad ?? string.Empty,
            VarisYeri = sefer.VarisSehir?.Ad ?? string.Empty,
            KalkisZamani = sefer.KalkisZamani ?? DateTime.MinValue,
            VarisZamani = sefer.TahminiVarisZamani ?? (sefer.KalkisZamani ?? DateTime.MinValue),
            BirimFiyat = sefer.Fiyat ?? 0m,
            KoltukDuzeni = sefer.Otobus?.KoltukDuzeni ?? KoltukDuzeni.IkiArtiIki
        };

    private async Task<string?> KoltukSecimleriniDogrulaAsync(Sefer sefer, List<SecilenKoltukInput> koltuklar)
    {
        if (koltuklar == null || koltuklar.Count == 0)
        {
            return "Devam etmek için en az bir koltuk seçmelisiniz.";
        }

        if (koltuklar.Count > KoltukSecimViewModel.EnFazlaKoltuk)
        {
            return $"Tek seferde en fazla {KoltukSecimViewModel.EnFazlaKoltuk} koltuk seçebilirsiniz.";
        }

        if (koltuklar.Select(k => k.KoltukNo).Distinct().Count() != koltuklar.Count)
        {
            return "Aynı koltuk birden fazla kez seçilemez.";
        }

        var koltukSayisi = sefer.Otobus?.KoltukSayisi ?? 0;
        var koltukDuzeni = sefer.Otobus?.KoltukDuzeni ?? KoltukDuzeni.IkiArtiIki;
        var doluKoltuklar = await DoluKoltuklariGetirAsync(sefer.Id);

        foreach (var secim in koltuklar)
        {
            if (secim.KoltukNo < 1 || secim.KoltukNo > koltukSayisi)
            {
                return $"{secim.KoltukNo} numaralı koltuk bu otobüste bulunmuyor.";
            }

            if (secim.Cinsiyet != Cinsiyet.Erkek && secim.Cinsiyet != Cinsiyet.Kadin)
            {
                return $"{secim.KoltukNo} numaralı koltuk için geçerli bir cinsiyet seçilmelidir.";
            }

            if (doluKoltuklar.ContainsKey(secim.KoltukNo))
            {
                return $"{secim.KoltukNo} numaralı koltuk az önce doldu. Lütfen başka bir koltuk seçin.";
            }

            var yanKoltuk = KoltukPlani.YanKoltuk(secim.KoltukNo, koltukDuzeni, koltukSayisi);
            if (yanKoltuk.HasValue &&
                doluKoltuklar.TryGetValue(yanKoltuk.Value, out var yanCinsiyet) &&
                yanCinsiyet != secim.Cinsiyet)
            {
                return $"{secim.KoltukNo} numaralı koltuk, yan koltuk cinsiyet kuralına uygun değil.";
            }
        }

        return null;
    }

    private Task<Dictionary<int, Cinsiyet>> DoluKoltuklariGetirAsync(int seferId) =>
        _context.Biletler
            .AsNoTracking()
            .Where(b => b.SeferId == seferId && b.Durum == BiletDurumu.Aktif)
            .ToDictionaryAsync(b => b.KoltukNo, b => b.Cinsiyet);
}