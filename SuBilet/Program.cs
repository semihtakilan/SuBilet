using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using SuBilet.Data;
using SuBilet.Models;

var builder = WebApplication.CreateBuilder(args);

// Bağlantı dizesi depoda değil, User Secrets'ta tutulur (README > Kurulum).
var baglantiDizesi = builder.Configuration.GetConnectionString("SuBiletDb")
    ?? throw new InvalidOperationException(
        "'SuBiletDb' bağlantı dizesi bulunamadı. README'deki User Secrets adımını uygulayın.");

// Servisler
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(baglantiDizesi));

builder.Services.AddIdentity<Kullanici, IdentityRole>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = false;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddErrorDescriber<TurkceIdentityHatalari>()
    .AddClaimsPrincipalFactory<KullaniciClaimsFactory>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Hesap/Giris";
    options.LogoutPath = "/Hesap/Cikis";
    options.AccessDeniedPath = "/Hesap/ErisimEngellendi";
});

builder.Services.AddControllersWithViews(options =>
{
    // Model binding'in İngilizce hata mesajları (ör. sayı alanına harf girilince) Türkçe olsun
    var mesajlar = options.ModelBindingMessageProvider;
    mesajlar.SetValueIsInvalidAccessor(deger => $"'{deger}' geçerli bir değer değil.");
    mesajlar.SetAttemptedValueIsInvalidAccessor((deger, alan) => $"'{deger}' değeri {alan} için geçerli değil.");
    mesajlar.SetNonPropertyAttemptedValueIsInvalidAccessor(deger => $"'{deger}' geçerli bir değer değil.");
    mesajlar.SetUnknownValueIsInvalidAccessor(alan => $"{alan} için girilen değer geçerli değil.");
    mesajlar.SetNonPropertyUnknownValueIsInvalidAccessor(() => "Girilen değer geçerli değil.");
    mesajlar.SetValueMustBeANumberAccessor(alan => $"{alan} alanına sayı girin.");
    mesajlar.SetNonPropertyValueMustBeANumberAccessor(() => "Bu alana sayı girin.");
    mesajlar.SetValueMustNotBeNullAccessor(alan => $"{alan} boş bırakılamaz.");
    mesajlar.SetMissingBindRequiredValueAccessor(alan => $"{alan} için değer girilmedi.");
    mesajlar.SetMissingKeyOrValueAccessor(() => "Değer zorunludur.");
    mesajlar.SetMissingRequestBodyRequiredValueAccessor(() => "İstek gövdesi boş olamaz.");
});

var app = builder.Build();

// Roller ve varsayılan yönetici (yoksa oluşturulur)
await KimlikVerisi.TohumlaAsync(app.Services);

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

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

// Yönetim paneli: /Admin/{controller}/{action}/{id?}; /Admin tek başına panel ana sayfasını açar.
// Area rotası varsayılan rotadan önce tanımlanmalı.
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Panel}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
