using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace SuBilet.Models;

// Identity'nin kullanıcı sınıfı genişletildi. Tablo adı AspNetUsers olarak kalır.
// E-posta, telefon (PhoneNumber) ve şifre özeti (PasswordHash) IdentityUser'dan gelir.
public class Kullanici : IdentityUser
{
    [Required]
    [StringLength(50)]
    public string Ad { get; set; } = "";

    [Required]
    [StringLength(50)]
    public string Soyad { get; set; } = "";

    // Yalnızca firma yetkilileri için dolu olur
    public int? FirmaId { get; set; }
    public Firma? Firma { get; set; }

    public string AdSoyad => $"{Ad} {Soyad}";
}
