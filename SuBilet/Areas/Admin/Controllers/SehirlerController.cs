using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuBilet.Data;
using SuBilet.Models;

namespace SuBilet.Areas.Admin.Controllers;

// Şehir CRUD: /Admin/Sehirler
public class SehirlerController : AdminControllerBase
{
    private readonly AppDbContext _context;

    public SehirlerController(AppDbContext context)
    {
        _context = context;
    }

    // GET /Admin/Sehirler?arama=ank  (ad ya da plaka koduyla arama)
    public async Task<IActionResult> Index(string? arama)
    {
        var sehirler = await _context.Sehirler.OrderBy(s => s.PlakaKodu).ToListAsync();

        // En fazla 81 kayıt olduğu için süzme bellekte yapılır; böylece sonuç
        // veritabanının dil ayarına bağlı kalmaz.
        if (!string.IsNullOrWhiteSpace(arama))
        {
            arama = arama.Trim();
            if (int.TryParse(arama, out var plaka))
            {
                sehirler = sehirler.Where(s => s.PlakaKodu == plaka).ToList();
            }
            else
            {
                var aranan = Sadelestir(arama);
                sehirler = sehirler.Where(s => Sadelestir(s.Ad).Contains(aranan)).ToList();
            }
        }

        ViewData["Arama"] = arama;
        return View(sehirler);
    }

    // Aramada büyük/küçük harf ve Türkçe karakter farkı gözetilmez:
    // "IZMIR", "izmir" ve "İzmir" aynı; "usak" Uşak'ı, "cankiri" Çankırı'yı bulur.
    private static string Sadelestir(string metin)
    {
        return string.Concat(metin.Select(harf => harf switch
        {
            'ç' or 'Ç' => 'c',
            'ğ' or 'Ğ' => 'g',
            'ı' or 'I' or 'İ' => 'i',
            'ö' or 'Ö' => 'o',
            'ş' or 'Ş' => 's',
            'ü' or 'Ü' => 'u',
            _ => char.ToLowerInvariant(harf)
        }));
    }
}
