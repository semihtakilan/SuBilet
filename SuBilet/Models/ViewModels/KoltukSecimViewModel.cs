using SuBilet.Servisler;

namespace SuBilet.Models.ViewModels;

// Koltuk seçimi sayfası: sefer özeti, koltuk haritası ve dolu koltuklar
public class KoltukSecimViewModel
{
    // Bir satın almada seçilebilecek en fazla koltuk
    public const int EnFazlaKoltuk = 5;

    public int SeferId { get; set; }
    public string FirmaAdi { get; set; } = string.Empty;

    // Sonuç listesine geri dönmek için arama bilgileri
    public int KalkisSehirId { get; set; }
    public int VarisSehirId { get; set; }

    public string KalkisYeri { get; set; } = string.Empty;
    public string VarisYeri { get; set; } = string.Empty;
    public DateTime KalkisZamani { get; set; }
    public DateTime VarisZamani { get; set; }
    public decimal Fiyat { get; set; }
    public KoltukDuzeni KoltukDuzeni { get; set; }
    public int KoltukSayisi { get; set; }

    // Satılmış (iptal edilmemiş) koltuklar: koltuk no → yolcunun cinsiyeti
    public Dictionary<int, Cinsiyet> DoluKoltuklar { get; set; } = new();

    public List<KoltukSirasi> Siralar => KoltukPlani.Siralar(KoltukDuzeni, KoltukSayisi);

    public int? YanKoltuk(int koltukNo) => KoltukPlani.YanKoltuk(koltukNo, KoltukDuzeni, KoltukSayisi);

    public int BosKoltukSayisi => KoltukSayisi - DoluKoltuklar.Count;
}
