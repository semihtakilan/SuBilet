using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace SuBilet.Dogrulama;

// Türkiye telefon numarası doğrulaması.
// Boşluk, tire, parantez ve nokta yok sayılır; başta +90, 90 ya da 0 olabilir.
//   SadeceCep = true  -> 5XX XXX XX XX (cep telefonu, kullanıcı kaydı)
//   SadeceCep = false -> sabit hat / cep / 0850 ya da 444 XX XX (firma iletişim)
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public class TelefonNoAttribute : ValidationAttribute, IClientModelValidator
{
    private static readonly Regex CepKalibi = new(@"^(?:\+90|90|0)?5\d{9}$");
    private static readonly Regex GenelKalip = new(@"^(?:(?:\+90|90|0)?[2-58]\d{9}|444\d{4})$");

    public bool SadeceCep { get; set; }

    public TelefonNoAttribute()
        : base("Geçerli bir telefon numarası girin.")
    {
    }

    public override bool IsValid(object? value)
    {
        if (value is not string metin || string.IsNullOrWhiteSpace(metin))
        {
            return true;
        }
        return GecerliMi(metin, SadeceCep);
    }

    public static bool GecerliMi(string telefon, bool sadeceCep)
    {
        var sade = Regex.Replace(telefon, @"[\s\-\(\)\.]", "");
        return sadeceCep ? CepKalibi.IsMatch(sade) : GenelKalip.IsMatch(sade);
    }

    // Tarayıcı tarafı: data-val-telefonno="mesaj" ve data-val-telefonno-sadececep="true/false"
    public void AddValidation(ClientModelValidationContext context)
    {
        context.Attributes.TryAdd("data-val", "true");
        context.Attributes.TryAdd("data-val-telefonno", FormatErrorMessage(context.ModelMetadata.GetDisplayName()));
        context.Attributes.TryAdd("data-val-telefonno-sadececep", SadeceCep ? "true" : "false");
        context.Attributes.TryAdd("inputmode", "tel");
    }
}
