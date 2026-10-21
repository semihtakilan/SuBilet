namespace SuBilet.Models;

// Rol adları tek yerde; [Authorize(Roles = Roller.Admin)] gibi kullanılır.
public static class Roller
{
    public const string Admin = "Admin";
    public const string FirmaYetkilisi = "FirmaYetkilisi";
    public const string Yolcu = "Yolcu";

    public static readonly string[] Tumu = { Admin, FirmaYetkilisi, Yolcu };

    public static string GorunenAd(string rol) => rol switch
    {
        Admin => "Yönetici",
        FirmaYetkilisi => "Firma Yetkilisi",
        Yolcu => "Yolcu",
        _ => rol
    };
}
