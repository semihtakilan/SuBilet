using System.ComponentModel.DataAnnotations;
using SuBilet.Dogrulama;

namespace SuBilet.Models.ViewModels;

public class SecilenKoltukInput
{
    public int KoltukNo { get; set; }
    public Cinsiyet Cinsiyet { get; set; }
}

public class YolcuBilgisiItemViewModel
{
    public int KoltukNo { get; set; }

    public Cinsiyet Cinsiyet { get; set; }

    [Required(ErrorMessage = "Yolcu adı zorunludur.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Ad 2 ile 50 karakter arasında olmalıdır.")]
    [RegularExpression(@"^[a-zA-ZçÇğĞıİöÖşŞüÜ\s]+$", ErrorMessage = "Ad yalnızca harflerden oluşmalıdır.")]
    [Display(Name = "Ad")]
    public string YolcuAd { get; set; } = string.Empty;

    [Required(ErrorMessage = "Yolcu soyadı zorunludur.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Soyad 2 ile 50 karakter arasında olmalıdır.")]
    [RegularExpression(@"^[a-zA-ZçÇğĞıİöÖşŞüÜ\s]+$", ErrorMessage = "Soyad yalnızca harflerden oluşmalıdır.")]
    [Display(Name = "Soyad")]
    public string YolcuSoyad { get; set; } = string.Empty;

    [Required(ErrorMessage = "T.C. kimlik no zorunludur.")]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "T.C. kimlik no 11 haneli olmalıdır.")]
    [TcKimlikNo]
    [Display(Name = "T.C. Kimlik No")]
    public string TcKimlikNo { get; set; } = string.Empty;
}

public class YolcuBilgileriViewModel
{
    public int SeferId { get; set; }
    public string FirmaAdi { get; set; } = string.Empty;
    public string KalkisYeri { get; set; } = string.Empty;
    public string VarisYeri { get; set; } = string.Empty;
    public DateTime KalkisZamani { get; set; }
    public DateTime VarisZamani { get; set; }
    public decimal BirimFiyat { get; set; }
    public KoltukDuzeni KoltukDuzeni { get; set; }

    public List<YolcuBilgisiItemViewModel> Yolcular { get; set; } = new();

    public decimal ToplamTutar => BirimFiyat * Yolcular.Count;
}