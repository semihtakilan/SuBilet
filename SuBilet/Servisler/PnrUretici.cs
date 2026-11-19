using System.Security.Cryptography;

namespace SuBilet.Servisler;

// GEÇİCİ (6. hafta): PNR kodu üretimi Kerim'in işi; o gelince bu sınıf onunla değiştirilecek.
// Mobil uygulamadaki geçici PnrUretici ile aynı kurallar: her biletin kendi PNR'ı vardır
// (veritabanında benzersiz, en fazla 8 karakter). Karışmasın diye 0/O ve 1/I gibi benzeyen
// karakterler ile Türk alfabesinde olmayan Q, W, X kullanılmaz.
public static class PnrUretici
{
    public const int Uzunluk = 6;
    private const string Karakterler = "ABCDEFGHJKLMNPRSTUVYZ23456789";

    public static string Uret() =>
        new(RandomNumberGenerator.GetItems<char>(Karakterler, Uzunluk));
}
