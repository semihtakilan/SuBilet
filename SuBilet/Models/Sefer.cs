using System.ComponentModel.DataAnnotations;

namespace SuBilet.Models;

public class Sefer
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Firma seçin.")]
    [Display(Name = "Firma")]
    public int? FirmaId { get; set; }
    public Firma? Firma { get; set; }

    [Required(ErrorMessage = "Otobüs seçin.")]
    [Display(Name = "Otobüs")]
    public int? OtobusId { get; set; }
    public Otobus? Otobus { get; set; }

    [Required(ErrorMessage = "Kalkış şehrini seçin.")]
    [Display(Name = "Kalkış Şehri")]
    public int? KalkisSehirId { get; set; }
    public Sehir? KalkisSehir { get; set; }

    [Required(ErrorMessage = "Varış şehrini seçin.")]
    [Display(Name = "Varış Şehri")]
    public int? VarisSehirId { get; set; }
    public Sehir? VarisSehir { get; set; }

    [Required(ErrorMessage = "Kalkış zamanı zorunludur.")]
    [Display(Name = "Kalkış Zamanı")]
    public DateTime? KalkisZamani { get; set; }

    [Required(ErrorMessage = "Tahmini varış zamanı zorunludur.")]
    [Display(Name = "Tahmini Varış Zamanı")]
    public DateTime? TahminiVarisZamani { get; set; }

    [Required(ErrorMessage = "Fiyat zorunludur.")]
    [Range(typeof(decimal), "1", "100000", ErrorMessage = "Fiyat 1 ile 100.000 TL arasında olmalıdır.")]
    [Display(Name = "Fiyat (TL)")]
    public decimal? Fiyat { get; set; }

    [Display(Name = "Durum")]
    public SeferDurumu Durum { get; set; } = SeferDurumu.Aktif;

    public List<Bilet> Biletler { get; set; } = new();
}
