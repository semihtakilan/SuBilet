using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SuBilet.Data;
using SuBilet.Models;

namespace SuBilet.Areas.Admin.Controllers;

public class OtobuslerController : AdminControllerBase
{
    private readonly AppDbContext _context;

    public OtobuslerController(AppDbContext context)
    {
        _context = context;
    }

    // GET: Admin/Otobusler
    public async Task<IActionResult> Index()
    {
        var otobusler = await _context.Otobusler
            .Include(o => o.Firma)
            .OrderBy(o => o.Firma.Ad)
            .ThenBy(o => o.Plaka)
            .ToListAsync();

        return View(otobusler);
    }

    // GET: Admin/Otobusler/Ekle
    public async Task<IActionResult> Ekle()
    {
        await FirmalariYukleAsync();
        return View(new Otobus { KoltukSayisi = 40 });
    }

    // POST: Admin/Otobusler/Ekle
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ekle(Otobus otobus, List<string>? secilenOzellikler)
    {
        otobus.Plaka = PlakaDuzenle(otobus.Plaka);
        otobus.Ozellikler = OzellikleriAyikla(secilenOzellikler);
        await PlakaKontrolAsync(otobus);

        if (ModelState.IsValid)
        {
            _context.Add(otobus);
            await _context.SaveChangesAsync();
            TempData["Mesaj"] = $"{otobus.Plaka} plakalı otobüs başarıyla eklendi.";
            return RedirectToAction(nameof(Index));
        }

        await FirmalariYukleAsync(otobus.FirmaId);
        return View(otobus);
    }

    // GET: Admin/Otobusler/Duzenle/5
    public async Task<IActionResult> Duzenle(int? id)
    {
        if (id == null) return NotFound();

        var otobus = await _context.Otobusler.FindAsync(id);
        if (otobus == null) return NotFound();

        await FirmalariYukleAsync(otobus.FirmaId);
        return View(otobus);
    }

    // POST: Admin/Otobusler/Duzenle/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Duzenle(int id, Otobus otobus, List<string>? secilenOzellikler)
    {
        if (id != otobus.Id) return NotFound();

        otobus.Plaka = PlakaDuzenle(otobus.Plaka);
        otobus.Ozellikler = OzellikleriAyikla(secilenOzellikler);
        await PlakaKontrolAsync(otobus);

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(otobus);
                await _context.SaveChangesAsync();
                TempData["Mesaj"] = $"{otobus.Plaka} plakalı otobüs güncellendi.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Otobusler.Any(e => e.Id == otobus.Id)) return NotFound();
                throw;
            }
        }

        await FirmalariYukleAsync(otobus.FirmaId);
        return View(otobus);
    }

    // GET: Admin/Otobusler/Sil/5
    public async Task<IActionResult> Sil(int? id)
    {
        if (id == null) return NotFound();

        var otobus = await _context.Otobusler
            .Include(o => o.Firma)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (otobus == null) return NotFound();

        return View(otobus);
    }

    // POST: Admin/Otobusler/Sil/5
    [HttpPost, ActionName("SilOnay")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SilOnay(int id)
    {
        var otobus = await _context.Otobusler
            .Include(o => o.Seferler)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (otobus == null) return NotFound();

        if (otobus.Seferler.Any())
        {
            TempData["Hata"] = "Bu otobüse atanmış seferler bulunduğu için silinemez.";
            return RedirectToAction(nameof(Index));
        }

        _context.Otobusler.Remove(otobus);
        await _context.SaveChangesAsync();
        TempData["Mesaj"] = $"{otobus.Plaka} plakalı otobüs silindi.";
        return RedirectToAction(nameof(Index));
    }

    // Plaka hep aynı biçimde saklansın diye kırpılıp büyük harfe çevrilir.
    // ToUpperInvariant: uygulama Türkçe çalıştığı için ToUpper "i" harfini "İ" yapardı.
    private static string PlakaDuzenle(string? plaka) => (plaka ?? "").Trim().ToUpperInvariant();

    // Formdan yalnızca tanımlı özellikler kabul edilir
    private static List<string> OzellikleriAyikla(List<string>? secilenler) =>
        (secilenler ?? new List<string>())
            .Where(o => Otobus.OzellikSecenekleri.Contains(o))
            .Distinct()
            .ToList();

    // Plaka veritabanında benzersiz; aynı plaka başka otobüste varsa forma hata yazılır.
    // Yeni kayıtta Id 0 olduğu için tüm otobüslerle karşılaştırılır.
    private async Task PlakaKontrolAsync(Otobus otobus)
    {
        if (await _context.Otobusler.AnyAsync(o => o.Id != otobus.Id && o.Plaka == otobus.Plaka))
        {
            ModelState.AddModelError(nameof(otobus.Plaka), "Bu plakaya sahip başka bir otobüs zaten kayıtlı.");
        }
    }

    private async Task FirmalariYukleAsync(int? seciliFirmaId = null)
    {
        var firmalar = await _context.Firmalar
            .OrderBy(f => f.Ad)
            .Select(f => new { f.Id, f.Ad })
            .ToListAsync();

        ViewBag.Firmalar = new SelectList(firmalar, "Id", "Ad", seciliFirmaId);
    }
}