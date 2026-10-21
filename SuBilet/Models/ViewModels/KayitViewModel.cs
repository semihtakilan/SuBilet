using System.ComponentModel.DataAnnotations;
using SuBilet.Dogrulama;

namespace SuBilet.Models.ViewModels;

public class KayitViewModel
{
    [Required(ErrorMessage = "Ad zorunludur.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Ad 2-50 karakter olmalıdır.")]
    [Display(Name = "Ad")]
    public string Ad { get; set; } = "";

    [Required(ErrorMessage = "Soyad zorunludur.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Soyad 2-50 karakter olmalıdır.")]
    [Display(Name = "Soyad")]
    public string Soyad { get; set; } = "";

    [Required(ErrorMessage = "E-posta zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin.")]
    [StringLength(100)]
    [Display(Name = "E-posta")]
    public string Eposta { get; set; } = "";

    [Required(ErrorMessage = "Cep telefonu zorunludur.")]
    [TelefonNo(SadeceCep = true, ErrorMessage = "Geçerli bir cep telefonu girin (05XX XXX XX XX).")]
    [Display(Name = "Cep Telefonu")]
    public string Telefon { get; set; } = "";

    [Required(ErrorMessage = "Şifre zorunludur.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Şifre en az 8 karakter olmalıdır.")]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre")]
    public string Sifre { get; set; } = "";

    [Required(ErrorMessage = "Şifreyi tekrar girin.")]
    [Compare(nameof(Sifre), ErrorMessage = "Şifreler aynı değil.")]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre (tekrar)")]
    public string SifreTekrar { get; set; } = "";
}
