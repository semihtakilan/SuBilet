using System.ComponentModel.DataAnnotations;

namespace SuBilet.Models;

public class Bilet
{
    public int Id { get; set; }

    [Required]
    [StringLength(8)]
    [Display(Name = "PNR Kodu")]
    public string PnrKodu { get; set; } = "";

    public int SeferId { get; set; }
    public Sefer? Sefer { get; set; }

    [Required]
    public string KullaniciId { get; set; } = "";
    public Kullanici? Kullanici { get; set; }

    public int OdemeId { get; set; }
    public Odeme? Odeme { get; set; }

    [Range(1, 60)]
    [Display(Name = "Koltuk No")]
    public int KoltukNo { get; set; }

    [Required(ErrorMessage = "Yolcu adı zorunludur.")]
    [StringLength(50)]
    [Display(Name = "Ad")]
    public string YolcuAd { get; set; } = "";

    [Required(ErrorMessage = "Yolcu soyadı zorunludur.")]
    [StringLength(50)]
    [Display(Name = "Soyad")]
    public string YolcuSoyad { get; set; } = "";

    [Required(ErrorMessage = "T.C. kimlik no zorunludur.")]
    [StringLength(11)]
    [Display(Name = "T.C. Kimlik No")]
    public string TcKimlikNo { get; set; } = "";

    [Display(Name = "Cinsiyet")]
    public Cinsiyet Cinsiyet { get; set; }

    [Display(Name = "Ücret")]
    public decimal Ucret { get; set; }

    [Display(Name = "Durum")]
    public BiletDurumu Durum { get; set; } = BiletDurumu.Aktif;

    [Display(Name = "Satış Tarihi")]
    public DateTime SatisTarihi { get; set; }
}
