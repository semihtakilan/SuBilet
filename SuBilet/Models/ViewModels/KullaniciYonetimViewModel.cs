namespace SuBilet.Models.ViewModels;

public class KullaniciYonetimViewModel
{
    public string Id { get; set; } = string.Empty;
    public string AdSoyad { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefon { get; set; }
    public IList<string> Roller { get; set; } = new List<string>();
    public bool AdminMi => Roller.Contains("Admin");
}