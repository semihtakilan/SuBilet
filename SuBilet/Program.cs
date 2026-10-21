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
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
