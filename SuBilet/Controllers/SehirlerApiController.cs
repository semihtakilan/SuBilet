using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuBilet.Data;
using SuBilet.DTOs;

namespace SuBilet.Controllers
{
    // Mobil uygulamanın arama ekranındaki şehir listesi. Giriş gerektirmez.
    [Route("api/sehirler")]
    [ApiController]
    public class SehirlerApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SehirlerApiController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [ProducesResponseType<IEnumerable<SehirDto>>(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<SehirDto>>> GetSehirler()
        {
            var sehirler = await _context.Sehirler
                .AsNoTracking()
                .Select(s => new SehirDto { Id = s.Id, Ad = s.Ad, PlakaKodu = s.PlakaKodu ?? 0 })
                .ToListAsync();

            // Web'deki arama formuyla aynı: Türkçe alfabe sırası (Ç, Ş, İ doğru yere gelsin)
            var turkce = StringComparer.Create(new CultureInfo("tr-TR"), false);
            return Ok(sehirler.OrderBy(s => s.Ad, turkce));
        }
    }
}
