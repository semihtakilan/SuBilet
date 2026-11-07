using SuBilet.Models;
using SuBilet.Models.ViewModels;

namespace SuBilet.Servisler;

// Sefer arama sonuçlarına firma / kalkış saati filtresi ve sıralama uygular.
// Sorgu veritabanında çalışır (IQueryable); arama sorgusundan sonra, Select'ten önce çağrılır:
//
//   var seferler = await aramaSorgusu
//       .Filtrele(filtre)
//       .Sirala(filtre.Siralama)
//       .Select(...)
//       .ToListAsync();
public static class SeferFiltreleme
{
    public static IQueryable<Sefer> Filtrele(this IQueryable<Sefer> sorgu, SeferFiltresi filtre)
    {
        if (filtre.FirmaIdleri.Count > 0)
        {
            var firmaIdleri = filtre.FirmaIdleri.Distinct().ToList();
            sorgu = sorgu.Where(s => firmaIdleri.Contains(s.FirmaId!.Value));
        }

        // Seçilen aralıklar saat listesine çevrilir: Sabah + Akşam → 6..11 ve 18..23.
        // Tanımsız değerler (adrese elle yazılmış ?Saatler=9 gibi) yok sayılır.
        var saatler = filtre.Saatler
            .Where(a => Enum.IsDefined(a))
            .Distinct()
            .SelectMany(a => a.Saatleri())
            .ToList();
        if (saatler.Count > 0)
        {
            sorgu = sorgu.Where(s => saatler.Contains(s.KalkisZamani!.Value.Hour));
        }

        return sorgu;
    }

    // Eşitlikte ikinci ölçüt devreye girer; aynı fiyattakiler kalkış saatine göre dizilir.
    public static IQueryable<Sefer> Sirala(this IQueryable<Sefer> sorgu, SeferSiralama siralama)
    {
        return siralama switch
        {
            SeferSiralama.KalkisGec => sorgu.OrderByDescending(s => s.KalkisZamani).ThenBy(s => s.Fiyat).ThenBy(s => s.Id),
            SeferSiralama.FiyatArtan => sorgu.OrderBy(s => s.Fiyat).ThenBy(s => s.KalkisZamani).ThenBy(s => s.Id),
            SeferSiralama.FiyatAzalan => sorgu.OrderByDescending(s => s.Fiyat).ThenBy(s => s.KalkisZamani).ThenBy(s => s.Id),
            _ => sorgu.OrderBy(s => s.KalkisZamani).ThenBy(s => s.Fiyat).ThenBy(s => s.Id)
        };
    }
}
