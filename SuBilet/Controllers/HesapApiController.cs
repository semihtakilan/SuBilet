using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SuBilet.DTOs;
using SuBilet.Models;
using SuBilet.Servisler;

namespace SuBilet.Controllers
{
    // Mobil uygulamanın kayıt ve giriş uç noktaları. Başarılı olunca JWT döner.
    [Route("api/hesap")]
    [ApiController]
    public class HesapApiController : ControllerBase
    {
        private readonly UserManager<Kullanici> _kullaniciYoneticisi;
        private readonly SignInManager<Kullanici> _girisYoneticisi;
        private readonly TokenServisi _tokenServisi;

        public HesapApiController(UserManager<Kullanici> kullaniciYoneticisi,
            SignInManager<Kullanici> girisYoneticisi, TokenServisi tokenServisi)
        {
            _kullaniciYoneticisi = kullaniciYoneticisi;
            _girisYoneticisi = girisYoneticisi;
            _tokenServisi = tokenServisi;
        }

        [HttpPost("kayit")]
        [ProducesResponseType<TokenYanitiDto>(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<TokenYanitiDto>> Kayit([FromBody] KayitDto dto)
        {
            var kullanici = new Kullanici
            {
                UserName = dto.Eposta.Trim(),
                Email = dto.Eposta.Trim(),
                PhoneNumber = dto.Telefon.Trim(),
                Ad = dto.Ad.Trim(),
                Soyad = dto.Soyad.Trim()
            };

            var sonuc = await _kullaniciYoneticisi.CreateAsync(kullanici, dto.Sifre);
            if (!sonuc.Succeeded)
            {
                var mesaj = string.Join(" ", sonuc.Errors.Select(h => h.Description));
                bool epostaKayitli = sonuc.Errors.Any(h =>
                    h.Code is nameof(IdentityErrorDescriber.DuplicateEmail)
                        or nameof(IdentityErrorDescriber.DuplicateUserName));

                return epostaKayitli ? Conflict(new { message = mesaj }) : BadRequest(new { message = mesaj });
            }

            // Kendi kendine kayıt olan herkes yolcudur (web ile aynı)
            await _kullaniciYoneticisi.AddToRoleAsync(kullanici, Roller.Yolcu);

            var yanit = await TokenYanitiOlusturAsync(kullanici);
            return CreatedAtAction(nameof(Ben), yanit);
        }

        [HttpPost("giris")]
        [ProducesResponseType<TokenYanitiDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status423Locked)]
        public async Task<ActionResult<TokenYanitiDto>> Giris([FromBody] GirisDto dto)
        {
            var kullanici = await _kullaniciYoneticisi.FindByEmailAsync(dto.Eposta.Trim());
            if (kullanici == null)
                return Unauthorized(new { message = "E-posta veya şifre hatalı." });

            // Web ile aynı kilit kuralı: 5 hatalı denemede 5 dakika (Program.cs). Çerez oluşturulmaz.
            var sonuc = await _girisYoneticisi.CheckPasswordSignInAsync(kullanici, dto.Sifre, lockoutOnFailure: true);

            if (sonuc.IsLockedOut)
                return StatusCode(StatusCodes.Status423Locked,
                    new { message = "Çok fazla hatalı deneme yapıldı. Birkaç dakika sonra tekrar deneyin." });

            if (!sonuc.Succeeded)
                return Unauthorized(new { message = "E-posta veya şifre hatalı." });

            return Ok(await TokenYanitiOlusturAsync(kullanici));
        }

        // Token'ın sahibini döner; mobil uygulama açılışta token'ın hâlâ geçerli olup olmadığını buradan anlar.
        [HttpGet("ben")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType<KullaniciDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<KullaniciDto>> Ben()
        {
            var kullaniciId = User.FindFirst(TokenServisi.IdClaim)?.Value;
            var kullanici = kullaniciId == null ? null : await _kullaniciYoneticisi.FindByIdAsync(kullaniciId);

            // Token geçerli ama hesap silinmişse oturum da geçersizdir
            if (kullanici == null)
                return Unauthorized(new { message = "Oturum geçersiz. Lütfen yeniden giriş yapın." });

            return Ok(await KullaniciDtoOlusturAsync(kullanici));
        }

        private async Task<TokenYanitiDto> TokenYanitiOlusturAsync(Kullanici kullanici)
        {
            var kullaniciDto = await KullaniciDtoOlusturAsync(kullanici);
            var (token, bitis) = _tokenServisi.Uret(kullanici, kullaniciDto.Roller);

            return new TokenYanitiDto
            {
                Token = token,
                GecerlilikBitisi = bitis,
                Kullanici = kullaniciDto
            };
        }

        private async Task<KullaniciDto> KullaniciDtoOlusturAsync(Kullanici kullanici) => new()
        {
            Id = kullanici.Id,
            Ad = kullanici.Ad,
            Soyad = kullanici.Soyad,
            Eposta = kullanici.Email ?? "",
            Telefon = kullanici.PhoneNumber ?? "",
            Roller = (await _kullaniciYoneticisi.GetRolesAsync(kullanici)).ToList()
        };
    }
}
