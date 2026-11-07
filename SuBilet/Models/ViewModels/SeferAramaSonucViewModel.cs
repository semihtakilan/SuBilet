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
    public decimal Fiyat { get; set; }
    public KoltukDuzeni KoltukDuzeni { get; set; }
    public List<string> Ozellikler { get; set; } = new();
    public int BosKoltukSayisi { get; set; }
}