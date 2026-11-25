// Ödeme sayfası: kart numarası dörderli gruplanır, son kullanma tarihine "/" kendiliğinden
// eklenir, CVV'ye yalnızca rakam girilir. Form geçerliyse düğme kilitlenir; iki kez
// tıklanınca ödeme iki kez gönderilmez. Doğrulama kuralları dogrulama.js'tedir.
document.addEventListener("DOMContentLoaded", function () {
    var form = document.getElementById("odemeFormu");
    if (!form) return;

    var kartNo = document.getElementById("KartNumarasi");
    var sonKullanma = document.getElementById("SonKullanmaTarihi");
    var cvv = document.getElementById("Cvv");
    var dugme = document.getElementById("odemeDugmesi");
    var dugmeMetni = dugme.textContent;

    // Alanı biçimlendirir; imleç, önündeki rakam sayısına göre yerinde kalır
    function bicimle(alan, bicimleyici) {
        var oncekiRakam = alan.value.slice(0, alan.selectionStart).replace(/\D/g, "").length;
        var yeni = bicimleyici(alan.value.replace(/\D/g, ""));
        alan.value = yeni;

        var konum = 0;
        for (var sayac = 0; konum < yeni.length && sayac < oncekiRakam; konum++) {
            if (/\d/.test(yeni[konum])) sayac++;
        }
        alan.setSelectionRange(konum, konum);
    }

    kartNo.addEventListener("input", function () {
        bicimle(kartNo, function (rakamlar) {
            return rakamlar.slice(0, 16).replace(/(\d{4})(?=\d)/g, "$1 ");
        });
    });

    sonKullanma.addEventListener("input", function () {
        bicimle(sonKullanma, function (rakamlar) {
            rakamlar = rakamlar.slice(0, 4);
            return rakamlar.length > 2 ? rakamlar.slice(0, 2) + "/" + rakamlar.slice(2) : rakamlar;
        });
    });

    cvv.addEventListener("input", function () {
        cvv.value = cvv.value.replace(/\D/g, "").slice(0, 3);
    });

    $(form).on("submit", function () {
        if (!$(form).valid()) return;
        dugme.disabled = true;
        dugme.textContent = "Ödeme yapılıyor…";
    });

    // Tarayıcı geri tuşuyla sayfa önbellekten gelirse (bfcache) düğme yeniden açılsın
    window.addEventListener("pageshow", function () {
        dugme.disabled = false;
        dugme.textContent = dugmeMetni;
    });
});
