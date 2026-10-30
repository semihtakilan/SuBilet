using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuBilet.Data;
using SuBilet.Models;
using SuBilet.Models.ViewModels;

namespace SuBilet.Areas.Admin.Controllers;

// Firma CRUD: /Admin/Firmalar
public class FirmalarController : AdminControllerBase
{
    // Türkçe kurallarla, büyük/küçük harf duyarsız karşılaştırma. Listede Ç, Ş, İ gibi harfler
    // alfabedeki yerine gelir; ad kontrolünde "metro turizm" ile "Metro Turizm" aynı sayılır.
    private static readonly StringComparer Turkce = StringComparer.Create(new CultureInfo("tr-TR"), ignoreCase: true);

    private readonly AppDbContext _context;

    public FirmalarController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var firmalar = (await _context.Firmalar
                .AsNoTracking()
                .Select(f => new FirmaSatiri(f, f.Otobusler.Count, f.Seferler.Count, f.Kullanicilar.Count))
                .ToListAsync())
            .OrderBy(s => s.Firma.Ad, Turkce)
            .ToList();
        return View(firmalar);
    }

    public async Task<IActionResult> Detay(int id)
    {
        var firma = await _context.Firmalar
            .AsNoTracking()
            .Include(f => f.Otobusler)
            .Include(f => f.Kullanicilar)
            .FirstOrDefaultAsync(f => f.Id == id);
        if (firma == null)
        {
            return NotFound();
        }

        var simdi = DateTime.Now;
        ViewData["SeferSayisi"] = await _context.Seferler.CountAsync(s => s.FirmaId == id);
        ViewData["YaklasanSeferSayisi"] = await _context.Seferler
            .CountAsync(s => s.FirmaId == id && s.Durum == SeferDurumu.Aktif && s.KalkisZamani >= simdi);
        return View(firma);
    }

    public IActionResult Ekle()
    {
        return View(new Firma());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ekle([Bind("Ad,LogoUrl,Telefon,Eposta,AktifMi")] Firma firma)
    {
        if (ModelState.IsValid)
        {
            BosluklariTemizle(firma);
            await AdKontrolEtAsync(firma.Ad, 0);
        }
        if (!ModelState.IsValid)
        {
            return View(firma);
        }

        _context.Firmalar.Add(firma);
        await _context.SaveChangesAsync();
        TempData["Mesaj"] = $"{firma.Ad} eklendi.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Duzenle(int id)
    {
        var firma = await _context.Firmalar.FindAsync(id);
        if (firma == null)
        {
            return NotFound();
        }
        return View(firma);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Duzenle(int id, [Bind("Ad,LogoUrl,Telefon,Eposta,AktifMi")] Firma duzenlenen)
    {
        var firma = await _context.Firmalar.FindAsync(id);
        if (firma == null)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            BosluklariTemizle(duzenlenen);
            await AdKontrolEtAsync(duzenlenen.Ad, id);
        }
        if (!ModelState.IsValid)
        {
            duzenlenen.Id = id;
            return View(duzenlenen);
        }

        // Formdan gelen nesne yerine veritabanındaki kayıt güncellenir; yalnızca izin verilen alanlar değişir
        firma.Ad = duzenlenen.Ad;
        firma.LogoUrl = duzenlenen.LogoUrl;
        firma.Telefon = duzenlenen.Telefon;
        firma.Eposta = duzenlenen.Eposta;
        firma.AktifMi = duzenlenen.AktifMi;
        await _context.SaveChangesAsync();
        TempData["Mesaj"] = $"{firma.Ad} güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    // Silme onay sayfası; bağlı kayıt varsa silme yerine pasif yapma önerilir
    public async Task<IActionResult> Sil(int id)
    {
        var satir = await _context.Firmalar
            .AsNoTracking()
            .Where(f => f.Id == id)
            .Select(f => new FirmaSatiri(f, f.Otobusler.Count, f.Seferler.Count, f.Kullanicilar.Count))
            .FirstOrDefaultAsync();
        if (satir == null)
        {
            return NotFound();
        }
        return View(satir);
    }

    [HttpPost, ActionName("Sil")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SilOnayla(int id)
    {
        var firma = await _context.Firmalar.FindAsync(id);
        if (firma == null)
        {
            return NotFound();
        }

        // Otobüsü ya da seferi olan firma silinemez (ilişki Restrict); geçmiş kayıtlar korunur
        bool bagliKayitVar = await _context.Otobusler.AnyAsync(o => o.FirmaId == id)
                          || await _context.Seferler.AnyAsync(s => s.FirmaId == id);
        if (bagliKayitVar)
        {
            TempData["Hata"] = $"{firma.Ad} firmasının otobüs ya da seferleri olduğu için silinemez. Firmayı pasif yapabilirsiniz.";
            return RedirectToAction(nameof(Index));
        }

        // Firma yetkililerinin FirmaId'si veritabanında kendiliğinden boşalır (SetNull)
        _context.Firmalar.Remove(firma);
        await _context.SaveChangesAsync();
        TempData["Mesaj"] = $"{firma.Ad} silindi.";
        return RedirectToAction(nameof(Index));
    }

    // Formdan gelen metinlerin başındaki/sonundaki boşluklar atılır
    private static void BosluklariTemizle(Firma firma)
    {
        firma.Ad = firma.Ad.Trim();
        firma.Telefon = firma.Telefon.Trim();
        firma.Eposta = firma.Eposta.Trim();
        firma.LogoUrl = firma.LogoUrl?.Trim();
    }

    // Aynı adla başka firma var mı? (id: düzenlenen firmanın kendisi hariç tutulsun diye; eklemede 0)
    private async Task AdKontrolEtAsync(string ad, int id)
    {
        var digerAdlar = await _context.Firmalar.Where(f => f.Id != id).Select(f => f.Ad).ToListAsync();
        if (digerAdlar.Contains(ad, Turkce))
        {
            ModelState.AddModelError(nameof(Firma.Ad), "Bu adla kayıtlı bir firma zaten var.");
        }
    }
}
