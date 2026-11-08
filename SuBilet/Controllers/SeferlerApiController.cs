using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuBilet.Data;
using SuBilet.DTOs;
using SuBilet.Models;

namespace SuBilet.Controllers
{
    [Route("api/seferler")]
    [ApiController]
    public class SeferlerApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SeferlerApiController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SeferResponseDto>>> GetSeferler()
        {
            var seferler = await _context.Seferler
                .Include(s => s.Firma)
                .Include(s => s.Otobus)
                .Include(s => s.KalkisSehir)
                .Include(s => s.VarisSehir)
                .Select(s => MapToResponseDto(s))
                .ToListAsync();

            return Ok(seferler);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SeferResponseDto>> GetSefer(int id)
        {
            var sefer = await _context.Seferler
                .Include(s => s.Firma)
                .Include(s => s.Otobus)
                .Include(s => s.KalkisSehir)
                .Include(s => s.VarisSehir)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sefer == null)
                return NotFound(new { message = $"{id} numaralı sefer bulunamadı." });

            return Ok(MapToResponseDto(sefer));
        }

        [HttpPost]
        public async Task<ActionResult<SeferResponseDto>> CreateSefer([FromBody] SeferCreateDto dto)
        {
            var hata = await SeferDogrulaAsync(dto);
            if (hata != null) return BadRequest(new { message = hata });

            var sefer = new Sefer
            {
                FirmaId = dto.FirmaId,
                OtobusId = dto.OtobusId,
                KalkisSehirId = dto.KalkisSehirId,
                VarisSehirId = dto.VarisSehirId,
                KalkisZamani = dto.KalkisZamani,
                TahminiVarisZamani = dto.TahminiVarisZamani,
                Fiyat = dto.Fiyat,
                Durum = dto.Durum
            };

            _context.Seferler.Add(sefer);
            await _context.SaveChangesAsync();

            await _context.Entry(sefer).Reference(s => s.Firma).LoadAsync();
            await _context.Entry(sefer).Reference(s => s.Otobus).LoadAsync();
            await _context.Entry(sefer).Reference(s => s.KalkisSehir).LoadAsync();
            await _context.Entry(sefer).Reference(s => s.VarisSehir).LoadAsync();

            return CreatedAtAction(nameof(GetSefer), new { id = sefer.Id }, MapToResponseDto(sefer));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSefer(int id, [FromBody] SeferUpdateDto dto)
        {
            var sefer = await _context.Seferler.FindAsync(id);
            if (sefer == null)
                return NotFound(new { message = $"{id} numaralı sefer bulunamadı." });

            var hata = await SeferDogrulaAsync(dto);
            if (hata != null) return BadRequest(new { message = hata });

            sefer.FirmaId = dto.FirmaId;
            sefer.OtobusId = dto.OtobusId;
            sefer.KalkisSehirId = dto.KalkisSehirId;
            sefer.VarisSehirId = dto.VarisSehirId;
            sefer.KalkisZamani = dto.KalkisZamani;
            sefer.TahminiVarisZamani = dto.TahminiVarisZamani;
            sefer.Fiyat = dto.Fiyat;
            sefer.Durum = dto.Durum;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSefer(int id)
        {
            var sefer = await _context.Seferler.FindAsync(id);
            if (sefer == null)
                return NotFound(new { message = $"{id} numaralı sefer bulunamadı." });

            bool biletVarMi = await _context.Biletler.AnyAsync(b => b.SeferId == id);
            if (biletVarMi)
            {
                return Conflict(new { message = "Bu sefere ait satılmış veya rezerve bilet bulunduğu için sefer silinemez." });
            }

            _context.Seferler.Remove(sefer);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private async Task<string?> SeferDogrulaAsync(SeferCreateDto dto)
        {
            if (dto.KalkisSehirId == dto.VarisSehirId)
                return "Kalkış ve varış şehirleri aynı olamaz.";

            if (dto.TahminiVarisZamani <= dto.KalkisZamani)
                return "Tahmini varış zamanı kalkış zamanından sonra olmalıdır.";

            if (dto.KalkisZamani < DateTime.Now)
                return "Geçmiş bir tarihe sefer tanımlanamaz.";

            bool firmaVarMi = await _context.Firmalar.AnyAsync(f => f.Id == dto.FirmaId);
            if (!firmaVarMi) return "Belirtilen firma bulunamadı.";

            var otobus = await _context.Otobusler.FindAsync(dto.OtobusId);
            if (otobus == null) return "Belirtilen otobüs bulunamadı.";

            if (otobus.FirmaId != dto.FirmaId)
                return "Seçilen otobüs belirtilen firmaya ait değil.";

            if (!Enum.IsDefined(typeof(SeferDurumu), dto.Durum))
                return "Geçersiz sefer durumu değeri.";

            return null;
        }

        private static SeferResponseDto MapToResponseDto(Sefer sefer)
        {
            return new SeferResponseDto
            {
                Id = sefer.Id,
                FirmaId = sefer.FirmaId ?? 0,
                FirmaAd = sefer.Firma?.Ad ?? string.Empty,
                OtobusId = sefer.OtobusId ?? 0,
                OtobusPlaka = sefer.Otobus?.Plaka ?? string.Empty,
                KalkisSehirId = sefer.KalkisSehirId ?? 0,
                KalkisSehirAd = sefer.KalkisSehir?.Ad ?? string.Empty,
                VarisSehirId = sefer.VarisSehirId ?? 0,
                VarisSehirAd = sefer.VarisSehir?.Ad ?? string.Empty,
                KalkisZamani = sefer.KalkisZamani ?? DateTime.MinValue,
                TahminiVarisZamani = sefer.TahminiVarisZamani ?? DateTime.MinValue,
                Fiyat = sefer.Fiyat ?? 0,
                DurumId = (int)sefer.Durum,
                Durum = sefer.Durum.ToString()
            };
        }
    }
}
