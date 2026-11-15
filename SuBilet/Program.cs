using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using SuBilet.Data;
using SuBilet.Models;
using SuBilet.Servisler;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<KoltukDurumServisi>();

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

// Mobil uygulama API'ye JWT ile girer; web sitesi çerezle çalışmaya devam eder.
// Anahtar depoda değil, User Secrets'ta tutulur (README > Kurulum).
var jwtBolumu = builder.Configuration.GetSection(JwtAyarlari.Bolum);
var jwtAyarlari = jwtBolumu.Get<JwtAyarlari>() ?? new JwtAyarlari();
var jwtAnahtariUretildi = false;
if (jwtAyarlari.Anahtar.Length < 32)
{
    if (!builder.Environment.IsDevelopment())
    {
        throw new InvalidOperationException(
            "'Jwt:Anahtar' en az 32 karakter olmalı. README'deki User Secrets adımını uygulayın.");
    }

    // Geliştirmede anahtar verilmemişse her açılışta yenisi üretilir;
    // uygulama yeniden başlayınca eski tokenlar geçersiz olur.
    jwtAyarlari.Anahtar = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
    jwtAnahtariUretildi = true;
}
builder.Services.Configure<JwtAyarlari>(jwtBolumu);
builder.Services.PostConfigure<JwtAyarlari>(a => a.Anahtar = jwtAyarlari.Anahtar);
builder.Services.AddScoped<TokenServisi>();

builder.Services.AddAuthentication()
    .AddJwtBearer(options =>
    {
        // Claim adları token'daki gibi kalsın (sub, name, role)
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new()
        {
            ValidIssuer = jwtAyarlari.Yayinci,
            ValidAudience = jwtAyarlari.Hedef,
            IssuerSigningKey = jwtAyarlari.ImzaAnahtari(),
            NameClaimType = TokenServisi.AdClaim,
            RoleClaimType = TokenServisi.RolClaim,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

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
if (jwtAnahtariUretildi)
{
    app.Logger.LogWarning("'Jwt:Anahtar' verilmediği için geçici bir anahtar üretildi; uygulama yeniden başlayınca tokenlar geçersiz olur.");
}
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}
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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();
app.MapControllers();

app.Run();
