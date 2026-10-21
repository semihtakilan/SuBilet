# ŞuBilet – Çevrim İçi Otobüs Bileti Satış Portalı

BLM4581 Ağ Tabanlı Teknolojiler ve Uygulamaları dersi dönem projesi (2026–2027 Güz).

ŞuBilet, Türkiye genelindeki otobüs firmalarının seferlerini tek bir platformda toplayan bir bilet satış portalıdır. Kullanıcılar sefer arayıp karşılaştırabilir, koltuk seçip çevrim içi bilet satın alabilir. Yöneticiler şehir, firma, otobüs, sefer ve kullanıcıları yönetir; firma yetkilileri kendi firmalarının otobüs ve seferlerini yönetir.

## Ekip

| Ad Soyad | Lider olduğu haftalar |
|---|---|
| Semih TAKILAN | 1, 2, 3 |
| Furkan ERDEM | 4, 5, 6 |
| Kerim Mert Demir | 7, 8, 9 |

## Teknolojiler

- ASP.NET Core MVC (.NET 10 LTS)
- PostgreSQL + Entity Framework Core (Npgsql), Code-First migration
- ASP.NET Core Identity (giriş, roller)
- Bootstrap 5, jQuery Validation Unobtrusive
- Swashbuckle (Swagger UI), Chart.js, QuestPDF + QRCoder (ilerleyen haftalarda)

## Kurulum

Gerekenler: [.NET 10 SDK](https://dotnet.microsoft.com/download), PostgreSQL (14 ve üstü), `dotnet-ef` aracı.

```bash
# 1) dotnet-ef aracını kurun (bir kez)
dotnet tool install --global dotnet-ef

# 2) Bağlantı dizesini User Secrets'a yazın (depoya girmez)
cd SuBilet
dotnet user-secrets set "ConnectionStrings:SuBiletDb" "Host=localhost;Port=5432;Database=subilet;Username=postgres;Password=SIFRENIZ"

# 3) Veritabanını migration'larla oluşturun
dotnet ef database update

# 4) Uygulamayı çalıştırın
dotnet watch
```

Visual Studio kullananlar 2. adım için projeye sağ tık → **Manage User Secrets**, 3. adım için Package Manager Console'da `Update-Database` kullanabilir.

## Proje yapısı

```
ŞuBilet/
├── SuBilet.slnx          # çözüm dosyası
└── SuBilet/              # ASP.NET Core MVC projesi
    ├── Controllers/
    ├── Models/
    ├── Views/
    └── wwwroot/
```

## Haftalık ilerleme

| Hafta | İş | Durum |
|---|---|---|
| 1 | Depo, MVC projesi, README; PostgreSQL + EF Core, Şehir/Firma ve ilk migration; ortak layout ve sefer arama formu | Devam ediyor |
| 2 | Tüm varlıklar, Fluent API ilişkileri, seed verisi; Identity, roller, kayıt/giriş, özel doğrulamalar | – |
| 3 | Yönetici paneli; Şehir, Firma, Otobüs, Sefer CRUD; kullanıcı rol atama | – |
| 4 | Sefer arama ve sonuç listesi; Web API (/api/seferler) + Swagger | – |
| 5 | Koltuk durumu servisi, koltuk haritası, yolcu bilgileri formu | – |
| 6 | Ödeme + bilet transaction'ı, simüle ödeme, PNR ve Biletlerim | – |
| 7 | Bilet iptali, PNR ile sorgulama, profil, firma paneli | – |
| 8 | Raporlar (Chart.js), PDF/QR bilet, API genişletme | – |
| 9 | Testler, hata sayfaları, mobil kontrol, README tamamlama | – |
