using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SuBilet.Models.ViewModels;

// Ana sayfadaki sefer arama formu (nereden, nereye, tarih).
public class SeferAramaViewModel
{
    [Required(ErrorMessage = "Kalkış şehrini seçin.")]
    [Display(Name = "Nereden")]
    public int? KalkisSehirId { get; set; }

    [Required(ErrorMessage = "Varış şehrini seçin.")]
    [Display(Name = "Nereye")]
    public int? VarisSehirId { get; set; }

    [Required(ErrorMessage = "Gidiş tarihini seçin.")]
    [DataType(DataType.Date)]
    [Display(Name = "Gidiş Tarihi")]
    public DateTime? Tarih { get; set; }

    // Açılır listelerin seçenekleri; formdan gelmez.
    [ValidateNever]
    public SelectList Sehirler { get; set; } = default!;
}
