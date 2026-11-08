using System.ComponentModel.DataAnnotations;
using SuBilet.Models;

namespace SuBilet.DTOs
{
    public class SeferCreateDto
    {
        [Required(ErrorMessage = "Firma seçimi zorunludur.")]
        public int FirmaId { get; set; }

        [Required(ErrorMessage = "Otobüs seçimi zorunludur.")]
        public int OtobusId { get; set; }

        [Required(ErrorMessage = "Kalkış şehri seçimi zorunludur.")]
        public int KalkisSehirId { get; set; }

        [Required(ErrorMessage = "Varış şehri seçimi zorunludur.")]
        public int VarisSehirId { get; set; }

        [Required(ErrorMessage = "Kalkış zamanı zorunludur.")]
        public DateTime KalkisZamani { get; set; }

        [Required(ErrorMessage = "Tahmini varış zamanı zorunludur.")]
        public DateTime TahminiVarisZamani { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Fiyat 0'dan büyük olmalıdır.")]
        public decimal Fiyat { get; set; }

        public SeferDurumu Durum { get; set; } = SeferDurumu.Aktif;
    }

    public class SeferUpdateDto : SeferCreateDto
    {
    }

    public class SeferResponseDto
    {
        public int Id { get; set; }
        public int FirmaId { get; set; }
        public string FirmaAd { get; set; } = string.Empty;
        public int OtobusId { get; set; }
        public string OtobusPlaka { get; set; } = string.Empty;
        public int KalkisSehirId { get; set; }
        public string KalkisSehirAd { get; set; } = string.Empty;
        public int VarisSehirId { get; set; }
        public string VarisSehirAd { get; set; } = string.Empty;
        public DateTime KalkisZamani { get; set; }
        public DateTime TahminiVarisZamani { get; set; }
        public decimal Fiyat { get; set; }
        public int DurumId { get; set; }
        public string Durum { get; set; } = string.Empty;
    }
}