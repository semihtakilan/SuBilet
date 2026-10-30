using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuBilet.Models;
using SuBilet.Models.ViewModels;

namespace SuBilet.Areas.Admin.Controllers;

public class KullanicilarController : AdminControllerBase
{
    private readonly UserManager<Kullanici> _userManager;

    public KullanicilarController(UserManager<Kullanici> userManager)
    {
        _userManager = userManager;
    }

    // GET: Admin/Kullanicilar
    public async Task<IActionResult> Index()
    {
        var kullanicilar = await _userManager.Users
            .OrderBy(u => u.Ad)
            .ThenBy(u => u.Soyad)
            .ToListAsync();

        var modelList = new List<KullaniciYonetimViewModel>();

        foreach (var kullanici in kullanicilar)
        {
            var roller = await _userManager.GetRolesAsync(kullanici);
            modelList.Add(new KullaniciYonetimViewModel
            {
                Id = kullanici.Id,
                AdSoyad = $"{kullanici.Ad} {kullanici.Soyad}",
                Email = kullanici.Email ?? string.Empty,
                Telefon = kullanici.PhoneNumber,
                Roller = roller
            });
        }

        return View(modelList);
    }

    // POST: Admin/Kullanicilar/RolDegistir
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RolDegistir(string id)
    {
        if (string.IsNullOrEmpty(id)) return NotFound();

        var kullanici = await _userManager.FindByIdAsync(id);
        if (kullanici == null) return NotFound();

        var girisYapanKullanici = await _userManager.GetUserAsync(User);
        if (girisYapanKullanici != null && girisYapanKullanici.Id == kullanici.Id)
        {
            TempData["Hata"] = "Kendi yöneticilik rolünüzü kaldıramazsınız.";
            return RedirectToAction(nameof(Index));
        }

        var adminMi = await _userManager.IsInRoleAsync(kullanici, "Admin");

        if (adminMi)
        {
            await _userManager.RemoveFromRoleAsync(kullanici, "Admin");
            TempData["Basari"] = $"{kullanici.Ad} {kullanici.Soyad} kullanıcısından Admin rolü alındı.";
        }
        else
        {
            await _userManager.AddToRoleAsync(kullanici, "Admin");
            TempData["Basari"] = $"{kullanici.Ad} {kullanici.Soyad} kullanıcısına Admin rolü verildi.";
        }

        return RedirectToAction(nameof(Index));
    }
}