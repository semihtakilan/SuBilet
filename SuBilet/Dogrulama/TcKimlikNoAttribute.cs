using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace SuBilet.Dogrulama;

// T.C. kimlik numarası doğrulaması.
// Kurallar: 11 rakam, ilk hane 0 olamaz,
//   10. hane = ((1+3+5+7+9. haneler) * 7 - (2+4+6+8. haneler)) mod 10
//   11. hane = (ilk 10 hanenin toplamı) mod 10
// Sunucuda IsValid, tarayıcıda wwwroot/js/dogrulama.js içindeki aynı kural çalışır.
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public class TcKimlikNoAttribute : ValidationAttribute, IClientModelValidator
{
    public TcKimlikNoAttribute()
        : base("Geçerli bir T.C. kimlik numarası girin.")
    {
    }

    public override bool IsValid(object? value)
    {
        // Boş değer [Required]'ın işidir
        if (value is not string metin || string.IsNullOrWhiteSpace(metin))
        {
            return true;
        }
        return GecerliMi(metin.Trim());
    }

    public static bool GecerliMi(string tc)
    {
        if (tc.Length != 11 || !tc.All(char.IsAsciiDigit) || tc[0] == '0')
        {
            return false;
        }

        int[] h = tc.Select(c => c - '0').ToArray();
        int tekler = h[0] + h[2] + h[4] + h[6] + h[8];
        int ciftler = h[1] + h[3] + h[5] + h[7];

        int onuncu = ((tekler * 7 - ciftler) % 10 + 10) % 10;
        int onBirinci = h.Take(10).Sum() % 10;

        return h[9] == onuncu && h[10] == onBirinci;
    }

    // Tarayıcı tarafı: input'a data-val-tckimlikno="mesaj" yazılır
    public void AddValidation(ClientModelValidationContext context)
    {
        context.Attributes.TryAdd("data-val", "true");
        context.Attributes.TryAdd("data-val-tckimlikno", FormatErrorMessage(context.ModelMetadata.GetDisplayName()));
        context.Attributes.TryAdd("maxlength", "11");
        context.Attributes.TryAdd("inputmode", "numeric");
    }
}
