using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SuBilet.Models;

namespace SuBilet.Servisler;

// appsettings.json'daki "Jwt" bölümü. Anahtar depoda değil, User Secrets'ta tutulur.
public class JwtAyarlari
{
    public const string Bolum = "Jwt";

    public string Yayinci { get; set; } = "SuBilet";
    public string Hedef { get; set; } = "SuBilet.Mobil";

    // HMAC-SHA256 için en az 32 karakter
    public string Anahtar { get; set; } = "";

    public int GecerlilikSaati { get; set; } = 24;

    public SymmetricSecurityKey ImzaAnahtari() => new(Encoding.UTF8.GetBytes(Anahtar));
}

// Mobil uygulama için giriş belgesi (JWT) üretir. Web sitesi çerezle çalışmaya devam eder.
public class TokenServisi
{
    // Token içindeki kısa claim adları; Program.cs'te doğrulama da bunlara göre ayarlıdır.
    public const string IdClaim = JwtRegisteredClaimNames.Sub;
    public const string AdClaim = "name";
    public const string RolClaim = "role";

    private readonly JwtAyarlari _ayarlar;

    public TokenServisi(IOptions<JwtAyarlari> ayarlar)
    {
        _ayarlar = ayarlar.Value;
    }

    public (string Token, DateTime GecerlilikBitisi) Uret(Kullanici kullanici, IEnumerable<string> roller)
    {
        var bitis = DateTime.UtcNow.AddHours(_ayarlar.GecerlilikSaati);

        var claimler = new List<Claim>
        {
            new(IdClaim, kullanici.Id),
            new(JwtRegisteredClaimNames.Email, kullanici.Email ?? ""),
            new(AdClaim, kullanici.AdSoyad),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        claimler.AddRange(roller.Select(rol => new Claim(RolClaim, rol)));

        var token = new JwtSecurityToken(
            issuer: _ayarlar.Yayinci,
            audience: _ayarlar.Hedef,
            claims: claimler,
            expires: bitis,
            signingCredentials: new SigningCredentials(_ayarlar.ImzaAnahtari(), SecurityAlgorithms.HmacSha256));

        return (new JwtSecurityTokenHandler().WriteToken(token), bitis);
    }
}
