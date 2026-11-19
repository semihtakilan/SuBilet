using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using SuBilet.Dogrulama;

namespace SuBilet.Models.ViewModels;

// Yolcu bilgileri onaylanınca TempData'da ödeme adımına taşınan rezervasyon.
// Kullanıcı kimliği, aynı tarayıcıda başka bir hesapla giriş yapılırsa
// önceki kullanıcının yolcu bilgilerinin görünmemesi içindir.
public record BekleyenRezervasyon(string KullaniciId, YolcuBilgileriViewModel Rezervasyon)
{
    public const string TempDataAnahtari = "BekleyenRezervasyon";
}

// Simüle ödeme sayfası. Gerçek ödeme alınmaz; kart bilgilerinden yalnızca
// son 4 hane saklanır. Kart numarası ve CVV hiçbir yere yazılmaz.
public class OdemeViewModel
{
    // Sayfadaki sefer ve yolcu özeti; formdan değil, TempData'daki rezervasyondan gelir
    [BindNever]
    [ValidateNever]
    public YolcuBilgileriViewModel Rezervasyon { get; set; } = new();

    [Required(ErrorMessage = "Kart üzerindeki adı girin.")]
    [StringLength(60, ErrorMessage = "Ad en fazla 60 karakter olabilir.")]
    [RegularExpression(@"^\s*[a-zA-ZçÇğĞıİöÖşŞüÜ]+(\s+[a-zA-ZçÇğĞıİöÖşŞüÜ]+)+\s*$",
        ErrorMessage = "Kart üzerindeki adı ve soyadı harflerle girin.")]
    [Display(Name = "Kart Üzerindeki Ad Soyad")]
    public string KartSahibi { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kart numarasını girin.")]
    [KartNumarasi]
    [Display(Name = "Kart Numarası")]
    public string KartNumarasi { get; set; } = string.Empty;

    [Required(ErrorMessage = "Son kullanma tarihini girin.")]
    [SonKullanmaTarihi]
    [Display(Name = "Son Kullanma Tarihi")]
    public string SonKullanmaTarihi { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kartın arkasındaki 3 haneli kodu girin.")]
    [RegularExpression(@"^\d{3}$", ErrorMessage = "Kartın arkasındaki 3 haneli kodu girin.")]
    [Display(Name = "CVV")]
    public string Cvv { get; set; } = string.Empty;
}
