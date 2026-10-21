using System.Diagnostics;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SuBilet.Data;
using SuBilet.Models;
using SuBilet.Models.ViewModels;

namespace SuBilet.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _context;

    public HomeController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var arama = new SeferAramaViewModel { Tarih = DateTime.Today };
        await SehirleriDoldurAsync(arama);
        return View(arama);
    }

    // GET /Home/Ara?KalkisSehirId=34&VarisSehirId=6&Tarih=2026-10-25
    // Sonuç listesi plana göre 4. haftada eklenecek; şimdilik form doğrulanıp seçim özetleniyor.
    [HttpGet]
    public async Task<IActionResult> Ara(SeferAramaViewModel arama)
    {
        if (ModelState.IsValid && arama.KalkisSehirId == arama.VarisSehirId)
        {
            ModelState.AddModelError(nameof(arama.VarisSehirId), "Kalkış ve varış şehri aynı olamaz.");
        }
        if (ModelState.IsValid && arama.Tarih < DateTime.Today)
        {
            ModelState.AddModelError(nameof(arama.Tarih), "Geçmiş bir tarih seçilemez.");
        }

        await SehirleriDoldurAsync(arama);

        if (ModelState.IsValid)
        {
            var kalkis = await _context.Sehirler.FindAsync(arama.KalkisSehirId);
            var varis = await _context.Sehirler.FindAsync(arama.VarisSehirId);
            ViewData["AramaOzeti"] = $"{kalkis?.Ad} → {varis?.Ad}, {arama.Tarih:d MMMM yyyy dddd}";
        }

        return View("Index", arama);
    }

    public IActionResult Gizlilik()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private async Task SehirleriDoldurAsync(SeferAramaViewModel arama)
    {
        // Türkçe alfabeye göre sıralamak için sıralama bellekte yapılır (Ç, Ş, İ doğru yere gelsin).
        var sehirler = (await _context.Sehirler.ToListAsync())
            .OrderBy(s => s.Ad, StringComparer.Create(new CultureInfo("tr-TR"), false));
        arama.Sehirler = new SelectList(sehirler, "Id", "Ad");
    }
}
