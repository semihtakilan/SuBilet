using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuBilet.Data;
using SuBilet.Models;

namespace SuBilet.Areas.Admin.Controllers;

// Şehir CRUD: /Admin/Sehirler
[Area("Admin")]
[Authorize(Roles = Roller.Admin)]
public class SehirlerController : Controller
{
    private readonly AppDbContext _context;

    public SehirlerController(AppDbContext context)
    {
        _context = context;
    }

    // GET /Admin/Sehirler?arama=ank
    public async Task<IActionResult> Index(string? arama)
    {
        IQueryable<Sehir> sorgu = _context.Sehirler;

        if (!string.IsNullOrWhiteSpace(arama))
        {
            arama = arama.Trim();
            if (int.TryParse(arama, out var plaka))
            {
                sorgu = sorgu.Where(s => s.PlakaKodu == plaka);
            }
            else
            {
                // ILIKE: büyük/küçük harfe duyarsız arama (PostgreSQL)
                sorgu = sorgu.Where(s => EF.Functions.ILike(s.Ad, $"%{arama}%"));
            }
        }

        ViewData["Arama"] = arama;
        var sehirler = await sorgu.OrderBy(s => s.PlakaKodu).ToListAsync();
        return View(sehirler);
    }

    public IActionResult Ekle()
    {
        return View(new Sehir());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ekle([Bind("Ad,PlakaKodu")] Sehir sehir)
    {
        if (ModelState.IsValid)
        {
            sehir.Ad = sehir.Ad.Trim();
            await BenzersizlikKontrolEtAsync(sehir, 0);
        }
        if (!ModelState.IsValid)
        {
            return View(sehir);
        }

        _context.Sehirler.Add(sehir);
        await _context.SaveChangesAsync();
        TempData["Mesaj"] = $"\"{sehir.Ad}\" eklendi.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Duzenle(int id)
    {
        var sehir = await _context.Sehirler.FindAsync(id);
        if (sehir == null)
        {
            return NotFound();
        }
        return View(sehir);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Duzenle(int id, [Bind("Ad,PlakaKodu")] Sehir duzenlenen)
    {
        var sehir = await _context.Sehirler.FindAsync(id);
        if (sehir == null)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            duzenlenen.Ad = duzenlenen.Ad.Trim();
            await BenzersizlikKontrolEtAsync(duzenlenen, id);
        }
        if (!ModelState.IsValid)
        {
            duzenlenen.Id = id;
            return View(duzenlenen);
        }

        sehir.Ad = duzenlenen.Ad;
        sehir.PlakaKodu = duzenlenen.PlakaKodu;
        await _context.SaveChangesAsync();
        TempData["Mesaj"] = $"\"{sehir.Ad}\" güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Sil(int id)
    {
        var sehir = await _context.Sehirler.FindAsync(id);
        if (sehir == null)
        {
            return NotFound();
        }
        ViewData["SeferSayisi"] = await SeferSayisiAsync(id);
        return View(sehir);
    }

    [HttpPost, ActionName("Sil")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SilOnayla(int id)
    {
        var sehir = await _context.Sehirler.FindAsync(id);
        if (sehir == null)
        {
            return NotFound();
        }

        // Seferlerde kullanılan şehir silinemez (ilişki Restrict)
        if (await SeferSayisiAsync(id) > 0)
        {
            TempData["Hata"] = $"\"{sehir.Ad}\" seferlerde kullanıldığı için silinemez.";
            return RedirectToAction(nameof(Index));
        }

        _context.Sehirler.Remove(sehir);
        await _context.SaveChangesAsync();
        TempData["Mesaj"] = $"\"{sehir.Ad}\" silindi.";
        return RedirectToAction(nameof(Index));
    }

    // Aynı ad ya da plaka kodu başka şehirde var mı? (veritabanına bakan kural: AddModelError)
    private async Task BenzersizlikKontrolEtAsync(Sehir sehir, int id)
    {
        if (await _context.Sehirler.AnyAsync(s => s.Id != id && s.Ad == sehir.Ad))
        {
            ModelState.AddModelError(nameof(Sehir.Ad), "Bu şehir zaten kayıtlı.");
        }
        if (await _context.Sehirler.AnyAsync(s => s.Id != id && s.PlakaKodu == sehir.PlakaKodu))
        {
            ModelState.AddModelError(nameof(Sehir.PlakaKodu), "Bu plaka kodu başka bir şehre ait.");
        }
    }

    private Task<int> SeferSayisiAsync(int sehirId)
    {
        return _context.Seferler.CountAsync(s => s.KalkisSehirId == sehirId || s.VarisSehirId == sehirId);
    }
}
