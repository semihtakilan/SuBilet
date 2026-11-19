// İstemci tarafı doğrulama ayarları (jQuery Validation)

// Varsayılan İngilizce mesajların Türkçesi (attribute'ta ErrorMessage yazılmazsa bunlar görünür)
$.extend($.validator.messages, {
    required: "Bu alan zorunludur.",
    email: "Geçerli bir e-posta adresi girin.",
    url: "Geçerli bir adres girin.",
    number: "Geçerli bir sayı girin.",
    digits: "Sadece rakam girin.",
    maxlength: $.validator.format("En fazla {0} karakter girebilirsiniz."),
    minlength: $.validator.format("En az {0} karakter girin."),
    range: $.validator.format("{0} ile {1} arasında bir değer girin."),
    max: $.validator.format("{0} veya daha küçük bir değer girin."),
    min: $.validator.format("{0} veya daha büyük bir değer girin.")
});

// Türkçe ondalık ayırıcı: "1.250,50" gibi değerleri sayı kabul et
function sayiyaCevir(deger) {
    return parseFloat(String(deger).replace(/\./g, "").replace(",", "."));
}

$.validator.methods.number = function (value, element) {
    return this.optional(element) || /^-?(?:\d+|\d{1,3}(?:\.\d{3})+)(?:,\d+)?$/.test(value);
};

$.validator.methods.range = function (value, element, param) {
    if (this.optional(element)) return true;
    var sayi = sayiyaCevir(value);
    return sayi >= Number(param[0]) && sayi <= Number(param[1]);
};

// [TcKimlikNo] attribute'unun tarayıcı tarafı (kurallar Dogrulama/TcKimlikNoAttribute.cs ile aynı)
function tcKimlikNoGecerliMi(tc) {
    tc = String(tc).trim();
    if (!/^[1-9][0-9]{10}$/.test(tc)) return false;
    var h = tc.split("").map(Number);
    var tekler = h[0] + h[2] + h[4] + h[6] + h[8];
    var ciftler = h[1] + h[3] + h[5] + h[7];
    var onuncu = (((tekler * 7 - ciftler) % 10) + 10) % 10;
    var toplam = 0;
    for (var i = 0; i < 10; i++) toplam += h[i];
    return h[9] === onuncu && h[10] === toplam % 10;
}

$.validator.addMethod("tckimlikno", function (value, element) {
    return this.optional(element) || tcKimlikNoGecerliMi(value);
});
$.validator.unobtrusive.adapters.addBool("tckimlikno");

// [TelefonNo] attribute'unun tarayıcı tarafı (kurallar Dogrulama/TelefonNoAttribute.cs ile aynı)
function telefonGecerliMi(telefon, sadeceCep) {
    var sade = String(telefon).replace(/[\s\-().]/g, "");
    return sadeceCep
        ? /^(?:\+90|90|0)?5\d{9}$/.test(sade)
        : /^(?:(?:\+90|90|0)?[2-58]\d{9}|444\d{4})$/.test(sade);
}

$.validator.addMethod("telefonno", function (value, element, param) {
    return this.optional(element) || telefonGecerliMi(value, param.sadeceCep);
});
$.validator.unobtrusive.adapters.add("telefonno", ["sadececep"], function (options) {
    // Kural değeri nesne olmalı: false verilirse jQuery Validation kuralı tamamen kapatır
    options.rules["telefonno"] = { sadeceCep: options.params.sadececep === "true" };
    options.messages["telefonno"] = options.message;
});

// [KartNumarasi] ve [SonKullanmaTarihi] attribute'larının tarayıcı tarafı
// (kurallar Dogrulama/KartNumarasiAttribute.cs ve SonKullanmaTarihiAttribute.cs ile aynı).
// Geçerliyse null, değilse sunucudakiyle aynı mesajı döndürürler.
function kartNumarasiHatasi(deger) {
    var haneler = String(deger).replace(/\D/g, "");
    if (haneler.length !== 16) return "Kart numarası 16 haneli olmalıdır.";

    var toplam = 0;
    var ikiKati = false;
    for (var i = haneler.length - 1; i >= 0; i--) {
        var hane = Number(haneler[i]);
        if (ikiKati) {
            hane *= 2;
            if (hane > 9) hane -= 9;
        }
        toplam += hane;
        ikiKati = !ikiKati;
    }
    return toplam % 10 === 0 ? null : "Kart numarası geçersiz.";
}

function sonKullanmaHatasi(deger) {
    var eslesme = /^(\d{2})\/(\d{2})$/.exec(String(deger).trim());
    if (!eslesme) return "Son kullanma tarihini AA/YY biçiminde girin.";

    var ay = Number(eslesme[1]);
    var yil = 2000 + Number(eslesme[2]);
    if (ay < 1 || ay > 12) return "Ay 01 ile 12 arasında olmalıdır.";

    // Kart, yazan ayın sonuna kadar geçerlidir; içinde bulunulan ay kabul edilir
    var bugun = new Date();
    if (yil * 12 + ay < bugun.getFullYear() * 12 + bugun.getMonth() + 1) return "Kartın son kullanma tarihi geçmiş.";
    return yil > bugun.getFullYear() + 20 ? "Son kullanma tarihi geçersiz." : null;
}

// Hata mesajı, girilen değere göre kuraldan gelir
function mesajliKuralEkle(ad, hataBul) {
    $.validator.addMethod(ad, function (value, element) {
        return this.optional(element) || hataBul(value) === null;
    });
    $.validator.unobtrusive.adapters.add(ad, [], function (options) {
        options.rules[ad] = true;
        options.messages[ad] = function () {
            return hataBul(options.element.value) || options.message;
        };
    });
}

mesajliKuralEkle("kartnumarasi", kartNumarasiHatasi);
mesajliKuralEkle("sonkullanma", sonKullanmaHatasi);
