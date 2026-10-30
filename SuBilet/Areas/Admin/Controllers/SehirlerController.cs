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

    public async Task<IActionResult> Ekle()
    {
        await BosPlakaKodlariniHazirlaAsync();
        return View(new Sehir());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ekle([Bind("Ad,PlakaKodu")] Sehir sehir)
    {
        if (ModelState.IsValid)
        {
            sehir.Ad = sehir.Ad.Trim();
            await BenzersizlikKontrolEtAsync(sehir, 0);
        }
        if (!ModelState.IsValid)
        {
            await BosPlakaKodlariniHazirlaAsync();
            return View(sehir);
        }

        _context.Sehirler.Add(sehir);
        await _context.SaveChangesAsync();
        TempData["Mesaj"] = $"{sehir.Ad} eklendi.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Duzenle(int id)
    {
        var sehir = await _context.Sehirler.FindAsync(id);
        if (sehir == null)
        {
            return NotFound();
        }
        return View(sehir);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Duzenle(int id, [Bind("Ad,PlakaKodu")] Sehir duzenlenen)
    {
        var sehir = await _context.Sehirler.FindAsync(id);
        if (sehir == null)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            duzenlenen.Ad = duzenlenen.Ad.Trim();
            await BenzersizlikKontrolEtAsync(duzenlenen, id);
        }
        if (!ModelState.IsValid)
        {
            duzenlenen.Id = id;
            return View(duzenlenen);
        }

        // Formdan gelen nesne yerine veritabanındaki kayıt güncellenir; yalnızca izin verilen alanlar değişir
        sehir.Ad = duzenlenen.Ad;
        sehir.PlakaKodu = duzenlenen.PlakaKodu;
        await _context.SaveChangesAsync();
        TempData["Mesaj"] = $"{sehir.Ad} güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    // Silme onay sayfası
    public async Task<IActionResult> Sil(int id)
    {
        var sehir = await _context.Sehirler.FindAsync(id);
        if (sehir == null)
        {
            return NotFound();
        }
        ViewData["SeferSayisi"] = await SeferSayisiAsync(id);
        return View(sehir);
    }

    [HttpPost, ActionName("Sil")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SilOnayla(int id)
    {
        var sehir = await _context.Sehirler.FindAsync(id);
        if (sehir == null)
        {
            return NotFound();
        }

        // Seferlerde kalkış ya da varış olarak kullanılan şehir silinemez (ilişki Restrict)
        if (await SeferSayisiAsync(id) > 0)
        {
            TempData["Hata"] = $"{sehir.Ad} seferlerde kullanıldığı için silinemez.";
            return RedirectToAction(nameof(Index));
        }

        _context.Sehirler.Remove(sehir);
        await _context.SaveChangesAsync();
        TempData["Mesaj"] = $"{sehir.Ad} silindi.";
        return RedirectToAction(nameof(Index));
    }

    private Task<int> SeferSayisiAsync(int sehirId)
    {
        return _context.Seferler.CountAsync(s => s.KalkisSehirId == sehirId || s.VarisSehirId == sehirId);
    }

    // Aynı ad ya da plaka kodu başka bir şehirde var mı? Veritabanına bakmayı gerektiren
    // bu kurallar DataAnnotations ile yazılamadığı için hata ModelState'e elle eklenir.
    // id: düzenlenen şehrin kendisi karşılaştırmaya katılmasın diye (eklemede 0).
    private async Task BenzersizlikKontrolEtAsync(Sehir sehir, int id)
    {
        var digerSehirler = await _context.Sehirler.Where(s => s.Id != id).ToListAsync();

        var ad = Sadelestir(sehir.Ad);
        if (digerSehirler.Any(s => Sadelestir(s.Ad) == ad))
        {
            ModelState.AddModelError(nameof(Sehir.Ad), "Bu şehir zaten kayıtlı.");
        }

        var ayniPlakali = digerSehirler.FirstOrDefault(s => s.PlakaKodu == sehir.PlakaKodu);
        if (ayniPlakali != null)
        {
            ModelState.AddModelError(nameof(Sehir.PlakaKodu), $"Bu plaka kodu {ayniPlakali.Ad} şehrine ait.");
        }
    }

    // Ekle formunda kullanıcıya hangi plaka kodlarının boşta olduğunu göstermek için
    private async Task BosPlakaKodlariniHazirlaAsync()
    {
        var kullanilan = await _context.Sehirler.Select(s => s.PlakaKodu).ToListAsync();
        ViewData["BosPlakaKodlari"] = Enumerable.Range(1, 81).Where(p => !kullanilan.Contains(p)).ToList();
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
