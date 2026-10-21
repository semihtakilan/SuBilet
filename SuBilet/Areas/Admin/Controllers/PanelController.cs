using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuBilet.Data;
using SuBilet.Models;
using SuBilet.Models.ViewModels;

namespace SuBilet.Areas.Admin.Controllers;

// Yönetim paneli ana sayfası: /Admin
[Area("Admin")]
[Authorize(Roles = Roller.Admin)]
public class PanelController : Controller
{
    private readonly AppDbContext _context;

    public PanelController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var simdi = DateTime.Now;
        var ozet = new PanelViewModel
        {
            SehirSayisi = await _context.Sehirler.CountAsync(),
            FirmaSayisi = await _context.Firmalar.CountAsync(),
            AktifFirmaSayisi = await _context.Firmalar.CountAsync(f => f.AktifMi),
            OtobusSayisi = await _context.Otobusler.CountAsync(),
            YaklasanSeferSayisi = await _context.Seferler
                .CountAsync(s => s.Durum == SeferDurumu.Aktif && s.KalkisZamani >= simdi),
            KullaniciSayisi = await _context.Users.CountAsync(),
            AktifBiletSayisi = await _context.Biletler.CountAsync(b => b.Durum == BiletDurumu.Aktif)
        };
        return View(ozet);
    }
}
