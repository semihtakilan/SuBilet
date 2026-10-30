using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SuBilet.Models;

namespace SuBilet.Areas.Admin.Controllers;

// Yönetim panelindeki tüm controller'lar bu sınıftan türer.
// Area ve yetki kuralı burada bir kez yazılır; böylece yeni eklenen bir sayfa
// yanlışlıkla korumasız kalmaz. Giriş yapmamış kullanıcı giriş sayfasına,
// Admin rolü olmayan kullanıcı "Erişim Engellendi" sayfasına yönlendirilir.
[Area("Admin")]
[Authorize(Roles = Roller.Admin)]
public abstract class AdminControllerBase : Controller
{
}
