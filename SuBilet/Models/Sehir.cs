using System.ComponentModel.DataAnnotations;

namespace SuBilet.Models;

public class Sehir
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Şehir adı zorunludur.")]
    [StringLength(50, ErrorMessage = "Şehir adı en fazla 50 karakter olabilir.")]
    [Display(Name = "Şehir")]
    public string Ad { get; set; } = "";

    [Required(ErrorMessage = "Plaka kodu zorunludur.")]
    [Range(1, 81, ErrorMessage = "Plaka kodu 1 ile 81 arasında olmalıdır.")]
    [Display(Name = "Plaka Kodu")]
    public int? PlakaKodu { get; set; }
}
