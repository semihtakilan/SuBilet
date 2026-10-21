using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using SuBilet.Data;

var builder = WebApplication.CreateBuilder(args);

// Bağlantı dizesi depoda değil, User Secrets'ta tutulur (README > Kurulum).
var baglantiDizesi = builder.Configuration.GetConnectionString("SuBiletDb")
    ?? throw new InvalidOperationException(
        "'SuBiletDb' bağlantı dizesi bulunamadı. README'deki User Secrets adımını uygulayın.");

// Servisler
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(baglantiDizesi));
builder.Services.AddControllersWithViews();

var app = builder.Build();

// İstek hattı (middleware sırası önemlidir)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Tarih, para ve ondalık sayılar her bilgisayarda Türkçe biçimde okunup yazılsın
var turkce = new CultureInfo("tr-TR");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(turkce),
    SupportedCultures = new List<CultureInfo> { turkce },
    SupportedUICultures = new List<CultureInfo> { turkce }
});

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
