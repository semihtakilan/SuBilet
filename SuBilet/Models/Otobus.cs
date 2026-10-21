using System.ComponentModel.DataAnnotations;

namespace SuBilet.Models;

public class Otobus
{
    // Formda işaretlenebilecek özellikler
    public static readonly string[] OzellikSecenekleri = { "Wi-Fi", "Priz", "USB", "Koltuk ekranı", "İkram", "Klima" };

    public int Id { get; set; }

    [Required(ErrorMessage = "Firma seçin.")]
    [Display(Name = "Firma")]
    public int? FirmaId { get; set; }
    public Firma? Firma { get; set; }

    // Örnek: 34 ABC 123. Kayıttan önce büyük harfe çevrilir.
    [Required(ErrorMessage = "Plaka zorunludur.")]
    [StringLength(12)]
    [RegularExpression(@"^(0[1-9]|[1-7][0-9]|8[01]) ?[A-Za-z]{1,3} ?[0-9]{2,4}$",
        ErrorMessage = "Plakayı 34 ABC 123 biçiminde girin.")]
    [Display(Name = "Plaka")]
    public string Plaka { get; set; } = "";

    [Required(ErrorMessage = "Koltuk düzenini seçin.")]
    [Display(Name = "Koltuk Düzeni")]
    public KoltukDuzeni? KoltukDuzeni { get; set; }

    [Required(ErrorMessage = "Koltuk sayısı zorunludur.")]
    [Range(10, 60, ErrorMessage = "Koltuk sayısı 10 ile 60 arasında olmalıdır.")]
    [Display(Name = "Koltuk Sayısı")]
    public int? KoltukSayisi { get; set; }

    // PostgreSQL'de text[] (dizi) sütunu olarak tutulur
    [Display(Name = "Özellikler")]
    public List<string> Ozellikler { get; set; } = new();

    public List<Sefer> Seferler { get; set; } = new();
}
