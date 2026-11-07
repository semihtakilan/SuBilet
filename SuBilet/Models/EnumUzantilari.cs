using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace SuBilet.Models;

// Enum değerinin [Display] adını ve açıklamasını verir: SeferDurumu.Iptal.Ad() → "İptal"
public static class EnumUzantilari
{
    public static string Ad(this Enum deger) => DisplayBilgisi(deger)?.GetName() ?? deger.ToString();

    public static string? Aciklama(this Enum deger) => DisplayBilgisi(deger)?.GetDescription();

    private static DisplayAttribute? DisplayBilgisi(Enum deger) =>
        deger.GetType().GetField(deger.ToString())?.GetCustomAttribute<DisplayAttribute>();
}
