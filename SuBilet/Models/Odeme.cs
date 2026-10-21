using System.ComponentModel.DataAnnotations;

namespace SuBilet.Models;

// Simüle ödeme kaydı. Tam kart numarası saklanmaz, yalnızca son 4 hane.
public class Odeme
{
    public int Id { get; set; }

    [Required]
    public string KullaniciId { get; set; } = "";
    public Kullanici? Kullanici { get; set; }

    [Display(Name = "Toplam Tutar")]
    public decimal ToplamTutar { get; set; }

    [Required]
    [StringLength(4, MinimumLength = 4)]
    [RegularExpression(@"^\d{4}$")]
    [Display(Name = "Kartın Son 4 Hanesi")]
    public string KartSonDortHane { get; set; } = "";

    [Display(Name = "Ödeme Tarihi")]
    public DateTime OdemeTarihi { get; set; }

    [Display(Name = "Durum")]
    public OdemeDurumu Durum { get; set; } = OdemeDurumu.Basarili;

    public List<Bilet> Biletler { get; set; } = new();
}
