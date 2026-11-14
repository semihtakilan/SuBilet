using SuBilet.Models;

namespace SuBilet.Servisler;

// Otobüsün koltuk düzenine göre koltukların sıralara yerleşimi ve yan koltuk bilgisi.
// Koltuklar önden arkaya, her sırada soldan sağa numaralanır:
//
//   2+2:  1  2 | koridor |  3  4        2+1:  1 | koridor |  2  3
//         5  6 |         |  7  8              4 |         |  5  6
//
// Yan koltuk, koridorun aynı tarafında yan yana duran koltuktur (2+2'de 1–2, 3–4;
// 2+1'de 2–3). 2+1'deki tekli koltuğun yanı yoktur. Yan koltuk cinsiyet kuralı
// hem koltuk haritasında hem sunucu tarafındaki kontrolde bu bilgiyle uygulanır.
public static class KoltukPlani
{
    public static int SiradakiKoltukSayisi(KoltukDuzeni duzen) =>
        duzen == KoltukDuzeni.IkiArtiIki ? 4 : 3;

    // Koridorun solundaki koltuk sayısı
    public static int KoridorunSolundaki(KoltukDuzeni duzen) =>
        duzen == KoltukDuzeni.IkiArtiIki ? 2 : 1;

    // Koltuğun yanındaki koltuğun numarası; tekli koltukta ya da yan koltuk otobüste yoksa null
    public static int? YanKoltuk(int koltukNo, KoltukDuzeni duzen, int koltukSayisi)
    {
        if (koltukNo < 1 || koltukNo > koltukSayisi) return null;

        var siradaki = SiradakiKoltukSayisi(duzen);
        var konum = (koltukNo - 1) % siradaki;   // sıradaki yeri: 0 en solda
        var sol = KoridorunSolundaki(duzen);

        // Koridorun bir tarafında tek koltuk varsa yanı yoktur
        var taraftakiKoltuk = konum < sol ? sol : siradaki - sol;
        if (taraftakiKoltuk < 2) return null;

        // Taraftaki ikilinin ilk koltuğuysa yanı bir sonraki, değilse bir önceki
        var taraftakiYeri = konum < sol ? konum : konum - sol;
        var yan = taraftakiYeri == 0 ? koltukNo + 1 : koltukNo - 1;

        return yan <= koltukSayisi ? yan : null;
    }

    // Koltuk haritasının sıraları. Her sırada koridorun solundaki ve sağındaki koltuk
    // numaraları bulunur; son sıra eksikse olmayan koltukların yeri null kalır.
    public static List<KoltukSirasi> Siralar(KoltukDuzeni duzen, int koltukSayisi)
    {
        var siradaki = SiradakiKoltukSayisi(duzen);
        var sol = KoridorunSolundaki(duzen);
        var siralar = new List<KoltukSirasi>();

        for (var ilk = 1; ilk <= koltukSayisi; ilk += siradaki)
        {
            var koltuklar = Enumerable.Range(ilk, siradaki)
                .Select(no => no <= koltukSayisi ? no : (int?)null)
                .ToArray();
            siralar.Add(new KoltukSirasi(koltuklar[..sol], koltuklar[sol..]));
        }

        return siralar;
    }
}

public record KoltukSirasi(int?[] Sol, int?[] Sag);
