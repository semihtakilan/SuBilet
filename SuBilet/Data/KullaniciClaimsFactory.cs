using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SuBilet.Models;

namespace SuBilet.Data;

// Giriş çerezine kullanıcının adını da ekler; menüde "Merhaba, Ayşe" yazabilmek için
// her istekte veritabanına gitmeye gerek kalmaz.
public class KullaniciClaimsFactory : UserClaimsPrincipalFactory<Kullanici, IdentityRole>
{
    public const string AdSoyadClaim = "AdSoyad";

    public KullaniciClaimsFactory(UserManager<Kullanici> userManager,
        RoleManager<IdentityRole> roleManager, IOptions<IdentityOptions> options)
        : base(userManager, roleManager, options)
    {
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(Kullanici user)
    {
        var kimlik = await base.GenerateClaimsAsync(user);
        kimlik.AddClaim(new Claim(AdSoyadClaim, user.AdSoyad));
        return kimlik;
    }
}
