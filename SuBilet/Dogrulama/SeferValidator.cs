using Microsoft.AspNetCore.Mvc.ModelBinding;
using SuBilet.Models;

namespace SuBilet.Dogrulama;

public static class SeferValidator
{
    public static void Dogrula(Sefer sefer, ModelStateDictionary modelState, bool isNew = true)
    {
        if (sefer.KalkisSehirId.HasValue && sefer.VarisSehirId.HasValue && sefer.KalkisSehirId == sefer.VarisSehirId)
        {
            modelState.AddModelError("VarisSehirId", "Kalkış ve varış şehri aynı olamaz.");
        }

        if (isNew && sefer.KalkisZamani.HasValue && sefer.KalkisZamani.Value < DateTime.Now)
        {
            modelState.AddModelError("KalkisZamani", "Geçmiş tarihe sefer eklenemez.");
        }

        if (sefer.KalkisZamani.HasValue && sefer.TahminiVarisZamani.HasValue && sefer.TahminiVarisZamani <= sefer.KalkisZamani)
        {
            modelState.AddModelError("TahminiVarisZamani", "Tahmini varış zamanı kalkış zamanından sonra olmalıdır.");
        }
    }
}