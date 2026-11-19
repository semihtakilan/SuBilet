using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace SuBilet.Dogrulama;

// Kartın "AA/YY" biçimindeki son kullanma tarihi (mobil uygulamadaki KartDogrulama ile aynı kurallar).
// Kart, yazan ayın sonuna kadar geçerlidir; bu yüzden içinde bulunulan ay kabul edilir.
// En fazla 20 yıl sonrası kabul edilir.
// Sunucuda IsValid, tarayıcıda wwwroot/js/dogrulama.js içindeki aynı kural çalışır.
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public class SonKullanmaTarihiAttribute : ValidationAttribute, IClientModelValidator
{
    public const int EnFazlaYil = 20;

    private static readonly Regex Kalip = new(@"^(\d{2})/(\d{2})$");

    public SonKullanmaTarihiAttribute()
        : base("Son kullanma tarihi geçersiz.")
    {
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string metin || string.IsNullOrWhiteSpace(metin))
        {
            return ValidationResult.Success;
        }

        var hata = Hata(metin, DateTime.Today);
        return hata == null ? ValidationResult.Success : new ValidationResult(hata);
    }

    // Geçerliyse null, değilse kullanıcıya gösterilecek mesaj
    public static string? Hata(string metin, DateTime bugun)
    {
        var eslesme = Kalip.Match(metin.Trim());
        if (!eslesme.Success)
        {
            return "Son kullanma tarihini AA/YY biçiminde girin.";
        }

        var ay = int.Parse(eslesme.Groups[1].Value);
        var yil = 2000 + int.Parse(eslesme.Groups[2].Value);
        if (ay is < 1 or > 12)
        {
            return "Ay 01 ile 12 arasında olmalıdır.";
        }

        if (yil * 12 + ay < bugun.Year * 12 + bugun.Month)
        {
            return "Kartın son kullanma tarihi geçmiş.";
        }

        return yil > bugun.Year + EnFazlaYil ? "Son kullanma tarihi geçersiz." : null;
    }

    // Tarayıcı tarafı: input'a data-val-sonkullanma="mesaj" yazılır
    public void AddValidation(ClientModelValidationContext context)
    {
        context.Attributes.TryAdd("data-val", "true");
        context.Attributes.TryAdd("data-val-sonkullanma", ErrorMessageString);
        context.Attributes.TryAdd("maxlength", "5");
        context.Attributes.TryAdd("inputmode", "numeric");
    }
}
