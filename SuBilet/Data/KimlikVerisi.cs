using Microsoft.AspNetCore.Identity;
using SuBilet.Models;

namespace SuBilet.Data;

// Uygulama açılırken rolleri ve varsayılan yönetici hesabını oluşturur.
// Zaten varsa hiçbir şey yapmaz; her açılışta güvenle çalışabilir.
public static class KimlikVerisi
{
    public static async Task TohumlaAsync(IServiceProvider servisler)
    {
        using var kapsam = servisler.CreateScope();
        var rolYoneticisi = kapsam.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var kullaniciYoneticisi = kapsam.ServiceProvider.GetRequiredService<UserManager<Kullanici>>();
        var ayarlar = kapsam.ServiceProvider.GetRequiredService<IConfiguration>();
        var gunluk = kapsam.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("KimlikVerisi");

        foreach (var rol in Roller.Tumu)
        {
            if (!await rolYoneticisi.RoleExistsAsync(rol))
            {
                await rolYoneticisi.CreateAsync(new IdentityRole(rol));
            }
        }

        // E-posta appsettings.json'da, şifre User Secrets'ta (depoya girmez)
        var eposta = ayarlar["VarsayilanYonetici:Eposta"];
        var sifre = ayarlar["VarsayilanYonetici:Sifre"];
        if (string.IsNullOrWhiteSpace(eposta) || string.IsNullOrWhiteSpace(sifre))
        {
            gunluk.LogWarning("Varsayılan yönetici oluşturulmadı: VarsayilanYonetici:Sifre User Secrets'ta tanımlı değil.");
            return;
        }

        var yonetici = await kullaniciYoneticisi.FindByEmailAsync(eposta);
        if (yonetici == null)
        {
            yonetici = new Kullanici
            {
                UserName = eposta,
                Email = eposta,
                EmailConfirmed = true,
                Ad = "Site",
                Soyad = "Yöneticisi"
            };
            var sonuc = await kullaniciYoneticisi.CreateAsync(yonetici, sifre);
            if (!sonuc.Succeeded)
            {
                gunluk.LogError("Varsayılan yönetici oluşturulamadı: {Hatalar}",
                    string.Join(" ", sonuc.Errors.Select(h => h.Description)));
                return;
            }
        }

        if (!await kullaniciYoneticisi.IsInRoleAsync(yonetici, Roller.Admin))
        {
            await kullaniciYoneticisi.AddToRoleAsync(yonetici, Roller.Admin);
        }
    }
}
