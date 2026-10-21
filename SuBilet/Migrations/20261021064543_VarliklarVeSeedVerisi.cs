using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SuBilet.Migrations
{
    /// <inheritdoc />
    public partial class VarliklarVeSeedVerisi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Odemeler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    KullaniciId = table.Column<string>(type: "text", nullable: false),
                    ToplamTutar = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    KartSonDortHane = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    OdemeTarihi = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Durum = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Odemeler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Odemeler_AspNetUsers_KullaniciId",
                        column: x => x.KullaniciId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Otobusler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FirmaId = table.Column<int>(type: "integer", nullable: false),
                    Plaka = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    KoltukDuzeni = table.Column<int>(type: "integer", nullable: false),
                    KoltukSayisi = table.Column<int>(type: "integer", nullable: false),
                    Ozellikler = table.Column<List<string>>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Otobusler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Otobusler_Firmalar_FirmaId",
                        column: x => x.FirmaId,
                        principalTable: "Firmalar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Seferler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FirmaId = table.Column<int>(type: "integer", nullable: false),
                    OtobusId = table.Column<int>(type: "integer", nullable: false),
                    KalkisSehirId = table.Column<int>(type: "integer", nullable: false),
                    VarisSehirId = table.Column<int>(type: "integer", nullable: false),
                    KalkisZamani = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    TahminiVarisZamani = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Fiyat = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Durum = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seferler", x => x.Id);
                    table.CheckConstraint("CK_Seferler_FarkliSehir", "\"KalkisSehirId\" <> \"VarisSehirId\"");
                    table.CheckConstraint("CK_Seferler_VarisKalkistanSonra", "\"TahminiVarisZamani\" > \"KalkisZamani\"");
                    table.ForeignKey(
                        name: "FK_Seferler_Firmalar_FirmaId",
                        column: x => x.FirmaId,
                        principalTable: "Firmalar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Seferler_Otobusler_OtobusId",
                        column: x => x.OtobusId,
                        principalTable: "Otobusler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Seferler_Sehirler_KalkisSehirId",
                        column: x => x.KalkisSehirId,
                        principalTable: "Sehirler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Seferler_Sehirler_VarisSehirId",
                        column: x => x.VarisSehirId,
                        principalTable: "Sehirler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Biletler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PnrKodu = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    SeferId = table.Column<int>(type: "integer", nullable: false),
                    KullaniciId = table.Column<string>(type: "text", nullable: false),
                    OdemeId = table.Column<int>(type: "integer", nullable: false),
                    KoltukNo = table.Column<int>(type: "integer", nullable: false),
                    YolcuAd = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    YolcuSoyad = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TcKimlikNo = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    Cinsiyet = table.Column<int>(type: "integer", nullable: false),
                    Ucret = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Durum = table.Column<int>(type: "integer", nullable: false),
                    SatisTarihi = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Biletler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Biletler_AspNetUsers_KullaniciId",
                        column: x => x.KullaniciId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Biletler_Odemeler_OdemeId",
                        column: x => x.OdemeId,
                        principalTable: "Odemeler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Biletler_Seferler_SeferId",
                        column: x => x.SeferId,
                        principalTable: "Seferler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Firmalar",
                columns: new[] { "Id", "Ad", "AktifMi", "Eposta", "LogoUrl", "Telefon" },
                values: new object[,]
                {
                    { 1, "Anadolu Ekspres", true, "iletisim@anadoluekspres.example", null, "0312 555 01 01" },
                    { 2, "Ege Yıldızı Turizm", true, "iletisim@egeyildizi.example", null, "0232 555 02 02" },
                    { 3, "Boğaziçi Seyahat", true, "iletisim@bogazici.example", null, "0212 555 03 03" },
                    { 4, "Karadeniz Hattı", true, "iletisim@karadenizhatti.example", null, "0462 555 04 04" },
                    { 5, "Kapadokya Yolu", true, "iletisim@kapadokyayolu.example", null, "0384 555 05 05" }
                });

            migrationBuilder.InsertData(
                table: "Sehirler",
                columns: new[] { "Id", "Ad", "PlakaKodu" },
                values: new object[,]
                {
                    { 1, "Adana", 1 },
                    { 2, "Adıyaman", 2 },
                    { 3, "Afyonkarahisar", 3 },
                    { 4, "Ağrı", 4 },
                    { 5, "Amasya", 5 },
                    { 6, "Ankara", 6 },
                    { 7, "Antalya", 7 },
                    { 8, "Artvin", 8 },
                    { 9, "Aydın", 9 },
                    { 10, "Balıkesir", 10 },
                    { 11, "Bilecik", 11 },
                    { 12, "Bingöl", 12 },
                    { 13, "Bitlis", 13 },
                    { 14, "Bolu", 14 },
                    { 15, "Burdur", 15 },
                    { 16, "Bursa", 16 },
                    { 17, "Çanakkale", 17 },
                    { 18, "Çankırı", 18 },
                    { 19, "Çorum", 19 },
                    { 20, "Denizli", 20 },
                    { 21, "Diyarbakır", 21 },
                    { 22, "Edirne", 22 },
                    { 23, "Elazığ", 23 },
                    { 24, "Erzincan", 24 },
                    { 25, "Erzurum", 25 },
                    { 26, "Eskişehir", 26 },
                    { 27, "Gaziantep", 27 },
                    { 28, "Giresun", 28 },
                    { 29, "Gümüşhane", 29 },
                    { 30, "Hakkari", 30 },
                    { 31, "Hatay", 31 },
                    { 32, "Isparta", 32 },
                    { 33, "Mersin", 33 },
                    { 34, "İstanbul", 34 },
                    { 35, "İzmir", 35 },
                    { 36, "Kars", 36 },
                    { 37, "Kastamonu", 37 },
                    { 38, "Kayseri", 38 },
                    { 39, "Kırklareli", 39 },
                    { 40, "Kırşehir", 40 },
                    { 41, "Kocaeli", 41 },
                    { 42, "Konya", 42 },
                    { 43, "Kütahya", 43 },
                    { 44, "Malatya", 44 },
                    { 45, "Manisa", 45 },
                    { 46, "Kahramanmaraş", 46 },
                    { 47, "Mardin", 47 },
                    { 48, "Muğla", 48 },
                    { 49, "Muş", 49 },
                    { 50, "Nevşehir", 50 },
                    { 51, "Niğde", 51 },
                    { 52, "Ordu", 52 },
                    { 53, "Rize", 53 },
                    { 54, "Sakarya", 54 },
                    { 55, "Samsun", 55 },
                    { 56, "Siirt", 56 },
                    { 57, "Sinop", 57 },
                    { 58, "Sivas", 58 },
                    { 59, "Tekirdağ", 59 },
                    { 60, "Tokat", 60 },
                    { 61, "Trabzon", 61 },
                    { 62, "Tunceli", 62 },
                    { 63, "Şanlıurfa", 63 },
                    { 64, "Uşak", 64 },
                    { 65, "Van", 65 },
                    { 66, "Yozgat", 66 },
                    { 67, "Zonguldak", 67 },
                    { 68, "Aksaray", 68 },
                    { 69, "Bayburt", 69 },
                    { 70, "Karaman", 70 },
                    { 71, "Kırıkkale", 71 },
                    { 72, "Batman", 72 },
                    { 73, "Şırnak", 73 },
                    { 74, "Bartın", 74 },
                    { 75, "Ardahan", 75 },
                    { 76, "Iğdır", 76 },
                    { 77, "Yalova", 77 },
                    { 78, "Karabük", 78 },
                    { 79, "Kilis", 79 },
                    { 80, "Osmaniye", 80 },
                    { 81, "Düzce", 81 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Biletler_KullaniciId",
                table: "Biletler",
                column: "KullaniciId");

            migrationBuilder.CreateIndex(
                name: "IX_Biletler_OdemeId",
                table: "Biletler",
                column: "OdemeId");

            migrationBuilder.CreateIndex(
                name: "IX_Biletler_PnrKodu",
                table: "Biletler",
                column: "PnrKodu",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Biletler_SeferId_KoltukNo",
                table: "Biletler",
                columns: new[] { "SeferId", "KoltukNo" },
                unique: true,
                filter: "\"Durum\" = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Odemeler_KullaniciId",
                table: "Odemeler",
                column: "KullaniciId");

            migrationBuilder.CreateIndex(
                name: "IX_Otobusler_FirmaId",
                table: "Otobusler",
                column: "FirmaId");

            migrationBuilder.CreateIndex(
                name: "IX_Otobusler_Plaka",
                table: "Otobusler",
                column: "Plaka",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Seferler_FirmaId",
                table: "Seferler",
                column: "FirmaId");

            migrationBuilder.CreateIndex(
                name: "IX_Seferler_KalkisSehirId_VarisSehirId_KalkisZamani",
                table: "Seferler",
                columns: new[] { "KalkisSehirId", "VarisSehirId", "KalkisZamani" });

            migrationBuilder.CreateIndex(
                name: "IX_Seferler_OtobusId",
                table: "Seferler",
                column: "OtobusId");

            migrationBuilder.CreateIndex(
                name: "IX_Seferler_VarisSehirId",
                table: "Seferler",
                column: "VarisSehirId");

            // HasData kayıtları Id'yi elle verdiği için PostgreSQL'in Id sayacı ilerlemez.
            // Sayaç en büyük Id'ye taşınır; panelden eklenen ilk kayıt Id çakışması yaşamaz.
            migrationBuilder.Sql("SELECT setval(pg_get_serial_sequence('\"Sehirler\"', 'Id'), (SELECT MAX(\"Id\") FROM \"Sehirler\"));");
            migrationBuilder.Sql("SELECT setval(pg_get_serial_sequence('\"Firmalar\"', 'Id'), (SELECT MAX(\"Id\") FROM \"Firmalar\"));");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Biletler");

            migrationBuilder.DropTable(
                name: "Odemeler");

            migrationBuilder.DropTable(
                name: "Seferler");

            migrationBuilder.DropTable(
                name: "Otobusler");

            migrationBuilder.DeleteData(
                table: "Firmalar",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Firmalar",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Firmalar",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Firmalar",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Firmalar",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 38);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 39);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 40);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 41);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 42);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 43);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 44);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 45);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 46);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 47);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 48);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 49);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 50);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 51);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 52);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 53);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 54);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 55);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 56);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 57);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 58);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 59);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 60);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 61);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 62);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 63);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 64);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 65);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 66);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 67);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 68);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 69);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 70);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 71);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 72);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 73);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 74);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 75);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 76);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 77);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 78);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 79);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 80);

            migrationBuilder.DeleteData(
                table: "Sehirler",
                keyColumn: "Id",
                keyValue: 81);
        }
    }
}
