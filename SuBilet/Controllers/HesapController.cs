using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SuBilet.Models;
using SuBilet.Models.ViewModels;

namespace SuBilet.Controllers;

// Kayıt, giriş ve çıkış işlemleri (ASP.NET Core Identity)
public class HesapController : Controller
{
    private readonly UserManager<Kullanici> _kullaniciYoneticisi;
    private readonly SignInManager<Kullanici> _girisYoneticisi;

    public HesapController(UserManager<Kullanici> kullaniciYoneticisi, SignInManager<Kullanici> girisYoneticisi)
    {
        _kullaniciYoneticisi = kullaniciYoneticisi;
        _girisYoneticisi = girisYoneticisi;
    }

    [HttpGet]
    public IActionResult Kayit()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }
        return View(new KayitViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Kayit(KayitViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var kullanici = new Kullanici
        {
            UserName = model.Eposta.Trim(),
            Email = model.Eposta.Trim(),
            PhoneNumber = model.Telefon.Trim(),
            Ad = model.Ad.Trim(),
            Soyad = model.Soyad.Trim()
        };

        var sonuc = await _kullaniciYoneticisi.CreateAsync(kullanici, model.Sifre);
        if (!sonuc.Succeeded)
        {
            // Identity hataları (aynı e-posta, zayıf şifre...) formun üstünde gösterilir
            foreach (var hata in sonuc.Errors)
            {
                ModelState.AddModelError("", hata.Description);
            }
            return View(model);
        }

        // Kendi kendine kayıt olan herkes yolcudur
        await _kullaniciYoneticisi.AddToRoleAsync(kullanici, Roller.Yolcu);
        await _girisYoneticisi.SignInAsync(kullanici, isPersistent: false);

        TempData["Mesaj"] = $"Hoş geldin {kullanici.Ad}! Hesabın oluşturuldu.";
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Giris(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new GirisViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Giris(GirisViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // 5 hatalı denemede hesap 5 dakika kilitlenir (Program.cs)
        var sonuc = await _girisYoneticisi.PasswordSignInAsync(
            model.Eposta.Trim(), model.Sifre, model.BeniHatirla, lockoutOnFailure: true);

        if (sonuc.Succeeded)
        {
            // Sadece kendi sitemizdeki adreslere dön (açık yönlendirme saldırısına karşı)
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        if (sonuc.IsLockedOut)
        {
            ModelState.AddModelError("", "Çok fazla hatalı deneme yapıldı. Birkaç dakika sonra tekrar deneyin.");
        }
        else
        {
            ModelState.AddModelError("", "E-posta veya şifre hatalı.");
        }
        return View(model);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cikis()
    {
        await _girisYoneticisi.SignOutAsync();
        TempData["Mesaj"] = "Çıkış yapıldı.";
        return RedirectToAction("Index", "Home");
    }

    public IActionResult ErisimEngellendi()
    {
        return View();
    }
}
