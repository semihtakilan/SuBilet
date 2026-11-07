using SuBilet.Models;

namespace SuBilet.Models.ViewModels;

public class SeferAramaSonucViewModel
{
    public int? KalkisSehirId { get; set; }
    public int? VarisSehirId { get; set; }
    public string KalkisYeri { get; set; } = string.Empty;
    public string VarisYeri { get; set; } = string.Empty;
    public DateTime? SeferTarihi { get; set; }

    public SeferFiltresi Filtre { get; set; } = new();
    public SeferFiltrePaneli? FiltrePaneli { get; set; }

    public List<SeferKartViewModel> Seferler { get; set; } = new();
}

public class SeferKartViewModel
{
    public int Id { get; set; }
    public string FirmaAdi { get; set; } = string.Empty;
    public string? FirmaLogoUrl { get; set; }
    public DateTime KalkisZamani { get; set; }
    public DateTime VarisZamani { get; set; }
    public TimeOnly KalkisSaati => TimeOnly.FromDateTime(KalkisZamani);
    public TimeOnly VarisSaati => TimeOnly.FromDateTime(VarisZamani);
    public TimeSpan Sure => VarisZamani >= KalkisZamani
        ? VarisZamani - KalkisZamani
        : TimeSpan.Zero;

    // 26 saatlik yolculuk "26 sa 0 dk" yazılır; TimeSpan.Hours gün kısmını atardı.
    public string SureMetni => Sure.Minutes == 0
        ? $"{(int)Sure.TotalHours} sa"
        : $"{(int)Sure.TotalHours} sa {Sure.Minutes} dk";

    // Varış kalkıştan kaç gün sonra (gece yarısını geçen seferlerde +1)
    public int VarisGunFarki => (VarisZamani.Date - KalkisZamani.Date).Days;
    public decimal Fiyat { get; set; }
    public KoltukDuzeni KoltukDuzeni { get; set; }
    public List<string> Ozellikler { get; set; } = new();
    public int BosKoltukSayisi { get; set; }
}