using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuBilet.Data;
using SuBilet.Models;
using SuBilet.Models.ViewModels;

namespace SuBilet.Areas.Admin.Controllers;

// Firma CRUD: /Admin/Firmalar
[Area("Admin")]
[Authorize(Roles = Roller.Admin)]
public class FirmalarController : Controller
{
    private readonly AppDbContext _context;

    public FirmalarController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var firmalar = await _context.Firmalar
            .OrderBy(f => f.Ad)
            .Select(f => new FirmaSatiri(f, f.Otobusler.Count, f.Seferler.Count))
            .ToListAsync();
        return View(firmalar);
    }

    public async Task<IActionResult> Detay(int id)
    {
        var firma = await _context.Firmalar
            .Include(f => f.Otobusler)
            .Include(f => f.Kullanicilar)
            .FirstOrDefaultAsync(f => f.Id == id);
        if (firma == null)
        {
            return NotFound();
        }
        ViewData["SeferSayisi"] = await _context.Seferler.CountAsync(s => s.FirmaId == id);
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
            firma.Ad = firma.Ad.Trim();
            await AdKontrolEtAsync(firma, 0);
        }
        if (!ModelState.IsValid)
        {
            return View(firma);
        }

        _context.Firmalar.Add(firma);
        await _context.SaveChangesAsync();
        TempData["Mesaj"] = $"\"{firma.Ad}\" eklendi.";
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
            duzenlenen.Ad = duzenlenen.Ad.Trim();
            await AdKontrolEtAsync(duzenlenen, id);
        }
        if (!ModelState.IsValid)
        {
            duzenlenen.Id = id;
            return View(duzenlenen);
        }

        firma.Ad = duzenlenen.Ad;
        firma.LogoUrl = duzenlenen.LogoUrl;
        firma.Telefon = duzenlenen.Telefon;
        firma.Eposta = duzenlenen.Eposta;
        firma.AktifMi = duzenlenen.AktifMi;
        await _context.SaveChangesAsync();
        TempData["Mesaj"] = $"\"{firma.Ad}\" güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Sil(int id)
    {
        var firma = await _context.Firmalar
            .Where(f => f.Id == id)
            .Select(f => new FirmaSatiri(f, f.Otobusler.Count, f.Seferler.Count))
            .FirstOrDefaultAsync();
        if (firma == null)
        {
            return NotFound();
        }
        return View(firma);
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

        // Otobüsü ya da seferi olan firma silinemez (ilişki Restrict); pasif yapılabilir
        bool bagliKayitVar = await _context.Otobusler.AnyAsync(o => o.FirmaId == id)
                          || await _context.Seferler.AnyAsync(s => s.FirmaId == id);
        if (bagliKayitVar)
        {
            TempData["Hata"] = $"\"{firma.Ad}\" firmasının otobüs ya da seferleri olduğu için silinemez. Firmayı pasif yapabilirsiniz.";
            return RedirectToAction(nameof(Index));
        }

        _context.Firmalar.Remove(firma);
        await _context.SaveChangesAsync();
        TempData["Mesaj"] = $"\"{firma.Ad}\" silindi.";
        return RedirectToAction(nameof(Index));
    }

    private async Task AdKontrolEtAsync(Firma firma, int id)
    {
        if (await _context.Firmalar.AnyAsync(f => f.Id != id && f.Ad == firma.Ad))
        {
            ModelState.AddModelError(nameof(Firma.Ad), "Bu adla kayıtlı bir firma zaten var.");
        }
    }
}
