using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace SuBilet.Dogrulama;

// Simüle ödemedeki kart numarası doğrulaması (mobil uygulamadaki KartDogrulama ile aynı kurallar).
// Boşluk ve tireler yok sayılır; numara 16 haneli olmalı ve Luhn kontrolünden geçmelidir:
//   sağdan başlayarak her ikinci hane iki katına çıkarılır (9'dan büyükse 9 çıkarılır),
//   tüm haneler toplanır; toplam 10'a tam bölünüyorsa numara geçerlidir.
// Sunucuda IsValid, tarayıcıda wwwroot/js/dogrulama.js içindeki aynı kural çalışır.
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public class KartNumarasiAttribute : ValidationAttribute, IClientModelValidator
{
    public const int Uzunluk = 16;

    public KartNumarasiAttribute()
        : base("Kart numarası geçersiz.")
    {
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        // Boş değer [Required]'ın işidir
        if (value is not string metin || string.IsNullOrWhiteSpace(metin))
        {
            return ValidationResult.Success;
        }

        var hata = Hata(metin);
        return hata == null ? ValidationResult.Success : new ValidationResult(hata);
    }

    // Geçerliyse null, değilse kullanıcıya gösterilecek mesaj
    public static string? Hata(string metin)
    {
        var haneler = Rakamlar(metin);
        if (haneler.Length != Uzunluk)
        {
            return $"Kart numarası {Uzunluk} haneli olmalıdır.";
        }
        return LuhnGecerliMi(haneler) ? null : "Kart numarası geçersiz.";
    }

    public static bool LuhnGecerliMi(string haneler)
    {
        if (haneler.Length == 0 || !haneler.All(char.IsAsciiDigit))
        {
            return false;
        }

        var toplam = 0;
        var ikiKati = false;
        for (var i = haneler.Length - 1; i >= 0; i--)
        {
            var hane = haneler[i] - '0';
            if (ikiKati)
            {
                hane *= 2;
                if (hane > 9) hane -= 9;
            }
            toplam += hane;
            ikiKati = !ikiKati;
        }
        return toplam % 10 == 0;
    }

    public static string Rakamlar(string metin) => new(metin.Where(char.IsAsciiDigit).ToArray());

    // Veritabanına yazılan tek kart bilgisi
    public static string SonDortHane(string metin)
    {
        var haneler = Rakamlar(metin);
        return haneler[^4..];
    }

    // Tarayıcı tarafı: input'a data-val-kartnumarasi="mesaj" yazılır
    public void AddValidation(ClientModelValidationContext context)
    {
        context.Attributes.TryAdd("data-val", "true");
        context.Attributes.TryAdd("data-val-kartnumarasi", ErrorMessageString);
        context.Attributes.TryAdd("maxlength", "19");
        context.Attributes.TryAdd("inputmode", "numeric");
    }
}
