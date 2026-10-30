using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SuBilet.Data;
using SuBilet.Dogrulama;
using SuBilet.Models;

namespace SuBilet.Controllers;

public class SeferController : Controller
{
    private readonly AppDbContext _context;

    public SeferController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var seferler = await _context.Seferler
            .Include(s => s.Firma)
            .Include(s => s.Otobus)
            .Include(s => s.KalkisSehir)
            .Include(s => s.VarisSehir)
            .ToListAsync();

        return View(seferler);
    }

    public IActionResult Create()
    {
        DropdownlariYukle();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Sefer sefer)
    {
        SeferValidator.Dogrula(sefer, ModelState);

        if (ModelState.IsValid)
        {
            _context.Add(sefer);
            await _context.SaveChangesAsync();
            TempData["Mesaj"] = "Sefer başarıyla oluşturuldu.";
            return RedirectToAction(nameof(Index));
        }

        DropdownlariYukle(sefer);
        return View(sefer);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var sefer = await _context.Seferler.FindAsync(id);
        if (sefer == null) return NotFound();

        DropdownlariYukle(sefer);
        return View(sefer);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Sefer sefer)
    {
        if (id != sefer.Id) return NotFound();

        SeferValidator.Dogrula(sefer, ModelState);

        if (ModelState.IsValid)
        {
            _context.Update(sefer);
            await _context.SaveChangesAsync();
            TempData["Mesaj"] = "Sefer bilgileri başarıyla güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        DropdownlariYukle(sefer);
        return View(sefer);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var sefer = await _context.Seferler
            .Include(s => s.Firma)
            .Include(s => s.Otobus)
            .Include(s => s.KalkisSehir)
            .Include(s => s.VarisSehir)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (sefer == null) return NotFound();

        return View(sefer);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var sefer = await _context.Seferler.FindAsync(id);
        if (sefer != null)
        {
            _context.Seferler.Remove(sefer);
            await _context.SaveChangesAsync();
            TempData["Mesaj"] = "Sefer silindi.";
        }

        return RedirectToAction(nameof(Index));
    }

    private void DropdownlariYukle(Sefer? sefer = null)
    {
        ViewBag.Firmalar = new SelectList(_context.Firmalar, "Id", "Ad", sefer?.FirmaId);
        ViewBag.Otobusler = new SelectList(_context.Otobusler, "Id", "Plaka", sefer?.OtobusId);
        ViewBag.Sehirler = new SelectList(_context.Sehirler, "Id", "Ad", sefer?.KalkisSehirId);
    }
}