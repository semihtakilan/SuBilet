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

## Kurulum (sıfırdan)

### 1. Gerekenleri kurun (bir kez)

- **.NET 10 SDK** – [dotnet.microsoft.com/download](https://dotnet.microsoft.com/download). Kontrol: `dotnet --version` 10 ile başlamalı.
- **PostgreSQL**
  - Mac: `brew install postgresql@17`
  - Windows: [postgresql.org/download](https://www.postgresql.org/download/) kurulum programı. Kurulumda `postgres` kullanıcısına bir şifre verin, portu 5432 bırakın.
- **EF Core aracı:** `dotnet tool install --global dotnet-ef`

### 2. PostgreSQL'i başlatın

- Mac: `brew services run postgresql@17` (bilgisayar her açıldığında; hep açık kalsın isterseniz `brew services start postgresql@17`)
- Windows: Servis olarak kendiliğinden çalışır.

### 3. Projeyi indirin

```bash
git clone https://github.com/semihtakilan/SuBilet.git
cd SuBilet/SuBilet
```

### 4. Gizli ayarları girin (User Secrets – depoya girmez)

Bağlantı dizesi:

```bash
# Mac (Homebrew): kullanıcı adı Mac kullanıcı adınızdır, şifre yoktur
dotnet user-secrets set "ConnectionStrings:SuBiletDb" "Host=localhost;Port=5432;Database=subilet;Username=MAC_KULLANICI_ADINIZ"

# Windows: kurulumda verdiğiniz postgres şifresi
dotnet user-secrets set "ConnectionStrings:SuBiletDb" "Host=localhost;Port=5432;Database=subilet;Username=postgres;Password=SIFRENIZ"
```

Varsayılan yönetici (`admin@subilet.com`) şifresi. En az 8 karakter; büyük harf, küçük harf ve rakam içermeli:

```bash
dotnet user-secrets set "VarsayilanYonetici:Sifre" "SuBilet.2026"
```

### 5. Veritabanını oluşturun

```bash
dotnet ef database update
```

`subilet` veritabanı, tablolar, 81 il ve 5 örnek firma oluşturulur.

### 6. Çalıştırın

```bash
dotnet run        # ya da dosya kaydettikçe yenilenmesi için: dotnet watch
```

Tarayıcıda **http://localhost:5214** adresini açın. İlk açılışta roller (Admin, FirmaYetkilisi, Yolcu) ve yönetici hesabı kendiliğinden oluşur.

### 7. Giriş yapın

`admin@subilet.com` ve 4. adımda verdiğiniz şifreyle giriş yapın. Yönetici hesabı Admin rolündedir; sağ üstteki menüden **Yönetim Paneli**'ne (ya da doğrudan http://localhost:5214/Admin adresine) girebilirsiniz. Kendi kendine üye olan kullanıcılar Yolcu rolündedir; yönetim paneline giremez.

### Visual Studio ile (Windows)

1. `SuBilet.slnx` dosyasını açın.
2. Projeye sağ tık → **Manage User Secrets** → aşağıdakini yapıştırın:
   ```json
   {
     "ConnectionStrings": { "SuBiletDb": "Host=localhost;Port=5432;Database=subilet;Username=postgres;Password=SIFRENIZ" },
     "VarsayilanYonetici": { "Sifre": "SuBilet.2026" }
   }
   ```
3. Package Manager Console'da `Update-Database` çalıştırın.
4. Ctrl+F5 ile başlatın.

### Sık karşılaşılan hatalar

| Hata | Sebep / çözüm |
|---|---|
| `'SuBiletDb' bağlantı dizesi bulunamadı` | 4. adım yapılmamış. |
| `Connection refused` | PostgreSQL kapalı; 2. adım. |
| `password authentication failed` | Bağlantı dizesindeki kullanıcı adı ya da şifre yanlış. |
| `relation "AspNetRoles" does not exist` | 5. adım (`dotnet ef database update`) yapılmamış. |
| Yönetici girişi olmuyor | Yönetici şifresi ilk çalıştırmadan önce verilmemiş ya da kurallara uymuyor. Şifreyi verip uygulamayı yeniden başlatın. |

Veritabanını sıfırlamak için: `dotnet ef database drop` ardından `dotnet ef database update`.

## Proje yapısı

```
ŞuBilet/
├── SuBilet.slnx                 # çözüm dosyası
└── SuBilet/                     # ASP.NET Core MVC projesi
    ├── Areas/Admin/             # yönetim paneli (/Admin, yalnızca Admin rolü)
    ├── Controllers/             # Home (ana sayfa, arama formu), Hesap (kayıt/giriş)
    ├── Data/                    # AppDbContext, seed verisi, Identity ayarları
    ├── Dogrulama/               # T.C. kimlik no ve telefon doğrulama attribute'ları
    ├── Migrations/              # EF Core migration'ları
    ├── Models/                  # varlıklar ve ViewModel'ler
    ├── Views/
    └── wwwroot/                 # css, js, kütüphaneler
```

### Yönetim paneline yeni sayfa eklemek

1. Controller'ı `Areas/Admin/Controllers` altına koyun ve `AdminControllerBase`'den türetin. `[Area("Admin")]` ve `[Authorize(Roles = "Admin")]` bu sınıftan gelir; ayrıca yazmanız gerekmez.
2. View'ları `Areas/Admin/Views/<ControllerAdı>/` altına koyun; panel menüsü olan layout kendiliğinden kullanılır.
3. Menü bağlantısını `Areas/Admin/Views/Shared/_AdminLayout.cshtml` içindeki listeye bir satır olarak ekleyin.

## Çalışma düzeni (dal ve Pull Request)

Çakışmaları önlemek için kimse `main` dalına doğrudan göndermez. Her iş kendi dalında yapılır ve Pull Request (PR) ile birleştirilir:

```bash
git switch main && git pull                 # main'in son hâlini alın
git switch -c hafta3-otobus-crud            # işe uygun kısa bir dal adı
# ... kod yazın, küçük adımlarla commit atın ...
git push -u origin hafta3-otobus-crud
gh pr create --base main                     # ya da GitHub'da "Compare & pull request"
```

PR'ı başka bir üye gözden geçirip birleştirir. Birleştirmeden önce `main`'e giren yeni değişiklikleri dalınıza alın (`git pull origin main`); çakışma çıkarsa kendi dalınızda çözün.

## Haftalık ilerleme

| Hafta | İş | Durum |
|---|---|---|
| 1 | Depo, MVC projesi, README; PostgreSQL + EF Core, Şehir/Firma ve ilk migration; ortak layout ve sefer arama formu | Tamamlandı |
| 2 | Tüm varlıklar, Fluent API ilişkileri, 81 il + firma seed verisi; Identity, roller, varsayılan yönetici; kayıt/giriş, T.C. kimlik no ve telefon doğrulaması | Tamamlandı |
| 3 | Yönetim paneli; Şehir, Firma, Otobüs, Sefer CRUD; kullanıcı rol atama | Devam ediyor (panel, Şehir ve Firma CRUD tamamlandı) |
| 4 | Sefer arama ve sonuç listesi; Web API (/api/seferler) + Swagger | – |
| 5 | Koltuk durumu servisi, koltuk haritası, yolcu bilgileri formu | – |
| 6 | Ödeme + bilet transaction'ı, simüle ödeme, PNR ve Biletlerim | – |
| 7 | Bilet iptali, PNR ile sorgulama, profil, firma paneli | – |
| 8 | Raporlar (Chart.js), PDF/QR bilet, API genişletme | – |
| 9 | Testler, hata sayfaları, mobil kontrol, README tamamlama | – |
