using System.ComponentModel.DataAnnotations;

namespace SuBilet.Models;

public class Firma
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Firma adı zorunludur.")]
    [StringLength(100, ErrorMessage = "Firma adı en fazla 100 karakter olabilir.")]
    [Display(Name = "Firma Adı")]
    public string Ad { get; set; } = "";

    [StringLength(300)]
    [Url(ErrorMessage = "Geçerli bir adres girin (https://...).")]
    [Display(Name = "Logo Adresi")]
    public string? LogoUrl { get; set; }

    [Required(ErrorMessage = "Telefon zorunludur.")]
    [StringLength(20)]
    [Phone(ErrorMessage = "Geçerli bir telefon numarası girin.")]
    [Display(Name = "Telefon")]
    public string Telefon { get; set; } = "";

    [Required(ErrorMessage = "E-posta zorunludur.")]
    [StringLength(100)]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin.")]
    [Display(Name = "E-posta")]
    public string Eposta { get; set; } = "";

    [Display(Name = "Aktif")]
    public bool AktifMi { get; set; } = true;
}
