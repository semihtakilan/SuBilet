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
    // Ç, Ş, İ gibi harfler alfabedeki yerine gelsin diye sıralama Türkçe kurallarla bellekte yapılır
    private static readonly StringComparer TurkceSiralama = StringComparer.Create(new CultureInfo("tr-TR"), ignoreCase: true);

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
            .OrderBy(s => s.Firma.Ad, TurkceSiralama)
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
}
