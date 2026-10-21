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
