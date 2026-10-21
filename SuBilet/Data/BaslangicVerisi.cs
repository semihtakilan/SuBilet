using SuBilet.Models;

namespace SuBilet.Data;

// Migration ile veritabanına eklenen başlangıç kayıtları (HasData).
// Şehirlerin Id'si plaka koduyla aynıdır.
public static class BaslangicVerisi
{
    public static Sehir[] Sehirler()
    {
        string[] iller =
        {
            "Adana", "Adıyaman", "Afyonkarahisar", "Ağrı", "Amasya", "Ankara", "Antalya", "Artvin", "Aydın", "Balıkesir",
            "Bilecik", "Bingöl", "Bitlis", "Bolu", "Burdur", "Bursa", "Çanakkale", "Çankırı", "Çorum", "Denizli",
            "Diyarbakır", "Edirne", "Elazığ", "Erzincan", "Erzurum", "Eskişehir", "Gaziantep", "Giresun", "Gümüşhane", "Hakkari",
            "Hatay", "Isparta", "Mersin", "İstanbul", "İzmir", "Kars", "Kastamonu", "Kayseri", "Kırklareli", "Kırşehir",
            "Kocaeli", "Konya", "Kütahya", "Malatya", "Manisa", "Kahramanmaraş", "Mardin", "Muğla", "Muş", "Nevşehir",
            "Niğde", "Ordu", "Rize", "Sakarya", "Samsun", "Siirt", "Sinop", "Sivas", "Tekirdağ", "Tokat",
            "Trabzon", "Tunceli", "Şanlıurfa", "Uşak", "Van", "Yozgat", "Zonguldak", "Aksaray", "Bayburt", "Karaman",
            "Kırıkkale", "Batman", "Şırnak", "Bartın", "Ardahan", "Iğdır", "Yalova", "Karabük", "Kilis", "Osmaniye",
            "Düzce"
        };

        return iller
            .Select((ad, sira) => new Sehir { Id = sira + 1, Ad = ad, PlakaKodu = sira + 1 })
            .ToArray();
    }

    // Örnek (hayali) otobüs firmaları
    public static Firma[] Firmalar() => new[]
    {
        new Firma { Id = 1, Ad = "Anadolu Ekspres", Telefon = "0312 555 01 01", Eposta = "iletisim@anadoluekspres.example", AktifMi = true },
        new Firma { Id = 2, Ad = "Ege Yıldızı Turizm", Telefon = "0232 555 02 02", Eposta = "iletisim@egeyildizi.example", AktifMi = true },
        new Firma { Id = 3, Ad = "Boğaziçi Seyahat", Telefon = "0212 555 03 03", Eposta = "iletisim@bogazici.example", AktifMi = true },
        new Firma { Id = 4, Ad = "Karadeniz Hattı", Telefon = "0462 555 04 04", Eposta = "iletisim@karadenizhatti.example", AktifMi = true },
        new Firma { Id = 5, Ad = "Kapadokya Yolu", Telefon = "0384 555 05 05", Eposta = "iletisim@kapadokyayolu.example", AktifMi = true }
    };
}
