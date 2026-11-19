using System.ComponentModel.DataAnnotations;
using SuBilet.Dogrulama;

namespace SuBilet.DTOs
{
    // Kurallar web'deki kayıt formuyla (KayitViewModel) aynı; şifre tekrarını mobil uygulama kendisi kontrol eder.
    public class KayitDto
    {
        [Required(ErrorMessage = "Ad zorunludur.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Ad 2-50 karakter olmalıdır.")]
        public string Ad { get; set; } = "";

        [Required(ErrorMessage = "Soyad zorunludur.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Soyad 2-50 karakter olmalıdır.")]
        public string Soyad { get; set; } = "";

        [Required(ErrorMessage = "E-posta zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin.")]
        [StringLength(100)]
        public string Eposta { get; set; } = "";

        [Required(ErrorMessage = "Cep telefonu zorunludur.")]
        [TelefonNo(SadeceCep = true, ErrorMessage = "Geçerli bir cep telefonu girin (05XX XXX XX XX).")]
        public string Telefon { get; set; } = "";

        [Required(ErrorMessage = "Şifre zorunludur.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Şifre en az 8 karakter olmalıdır.")]
        public string Sifre { get; set; } = "";
    }

    public class GirisDto
    {
        [Required(ErrorMessage = "E-posta zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin.")]
        public string Eposta { get; set; } = "";

        [Required(ErrorMessage = "Şifre zorunludur.")]
        public string Sifre { get; set; } = "";
    }

    public class KullaniciDto
    {
        public string Id { get; set; } = "";
        public string Ad { get; set; } = "";
        public string Soyad { get; set; } = "";
        public string Eposta { get; set; } = "";
        public string Telefon { get; set; } = "";
        public List<string> Roller { get; set; } = new();
    }

    // Kayıt ve giriş yanıtı. Mobil uygulama token'ı her istekte "Authorization: Bearer ..." başlığıyla gönderir.
    public class TokenYanitiDto
    {
        public string Token { get; set; } = "";

        // UTC
        public DateTime GecerlilikBitisi { get; set; }

        public KullaniciDto Kullanici { get; set; } = new();
    }
}
