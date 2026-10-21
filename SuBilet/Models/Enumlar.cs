using System.ComponentModel.DataAnnotations;

namespace SuBilet.Models;

// Veritabanında sayı olarak tutulur; ekranda [Display] adı görünür.

public enum KoltukDuzeni
{
    [Display(Name = "2+1")]
    IkiArtiBir = 1,

    [Display(Name = "2+2")]
    IkiArtiIki = 2
}

public enum SeferDurumu
{
    [Display(Name = "Aktif")]
    Aktif = 0,

    [Display(Name = "İptal")]
    Iptal = 1,

    [Display(Name = "Tamamlandı")]
    Tamamlandi = 2
}

public enum OdemeDurumu
{
    [Display(Name = "Başarılı")]
    Basarili = 0,

    [Display(Name = "İade")]
    Iade = 1
}

public enum BiletDurumu
{
    [Display(Name = "Aktif")]
    Aktif = 0,

    [Display(Name = "İptal")]
    Iptal = 1
}

public enum Cinsiyet
{
    [Display(Name = "Erkek")]
    Erkek = 1,

    [Display(Name = "Kadın")]
    Kadin = 2
}
