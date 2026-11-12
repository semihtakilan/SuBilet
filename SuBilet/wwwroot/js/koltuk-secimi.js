// Koltuk seçimi sayfası: boş koltuğa dokununca yolcunun cinsiyeti sorulur ve koltuk seçilir;
// seçili koltuğa yeniden dokununca seçim kaldırılır. Yan koltuk başka cinsiyetten bir yolcuya
// satılmışsa o cinsiyet seçilemez (kural sunucuda da ayrıca kontrol edilir).
document.addEventListener("DOMContentLoaded", function () {
    var harita = document.getElementById("koltukHaritasi");
    if (!harita) return;

    var CINSIYETLER = { 1: "Erkek", 2: "Kadın" };
    var HARFLER = { 1: "E", 2: "K" };

    var enFazla = parseInt(harita.dataset.enFazla, 10);
    var fiyat = parseFloat(harita.dataset.fiyat);
    var para = new Intl.NumberFormat("tr-TR", { style: "currency", currency: "TRY" });

    var liste = document.getElementById("secimListesi");
    var bosMesaj = document.getElementById("secimBos");
    var uyari = document.getElementById("secimUyari");
    var toplam = document.getElementById("secimToplam");
    var alanlar = document.getElementById("secimAlanlari");
    var devam = document.getElementById("secimDevam");

    var modalKutusu = document.getElementById("cinsiyetModal");
    var modal = new bootstrap.Modal(modalKutusu);
    var modalBaslik = document.getElementById("cinsiyetBaslik");
    var modalNotu = document.getElementById("cinsiyetNotu");
    var cinsiyetDugmeleri = modalKutusu.querySelectorAll("[data-cinsiyet]");

    var secimler = new Map();   // koltuk no → cinsiyet (1 erkek, 2 kadın)
    var sorulanKoltuk = null;   // cinsiyeti sorulan koltuğun düğmesi
    var uyariZamani = null;

    function koltuk(no) {
        return harita.querySelector('.sb-koltuk[data-no="' + no + '"]');
    }

    // Yan koltuk satılmışsa oradaki yolcunun cinsiyeti, değilse null
    function yanKoltuktakiCinsiyet(dugme) {
        var yan = dugme.dataset.yan ? koltuk(dugme.dataset.yan) : null;
        return yan && yan.dataset.cinsiyet ? parseInt(yan.dataset.cinsiyet, 10) : null;
    }

    function uyar(mesaj) {
        uyari.textContent = mesaj;
        clearTimeout(uyariZamani);
        uyariZamani = setTimeout(function () { uyari.textContent = ""; }, 4000);
    }

    function cinsiyetSor(dugme) {
        var no = dugme.dataset.no;
        var yandaki = yanKoltuktakiCinsiyet(dugme);

        sorulanKoltuk = dugme;
        modalBaslik.textContent = no + " numaralı koltuk";
        cinsiyetDugmeleri.forEach(function (secenek) {
            var cinsiyet = parseInt(secenek.dataset.cinsiyet, 10);
            secenek.disabled = yandaki !== null && yandaki !== cinsiyet;
        });

        if (yandaki !== null) {
            modalNotu.textContent = "Yanındaki " + dugme.dataset.yan + " numaralı koltuk " +
                CINSIYETLER[yandaki].toLocaleLowerCase("tr-TR") + " yolcuya satıldı; bu koltuğu yalnızca " +
                CINSIYETLER[yandaki].toLocaleLowerCase("tr-TR") + " yolcu alabilir.";
            modalNotu.hidden = false;
        } else {
            modalNotu.hidden = true;
        }

        modal.show();
    }

    function sec(no, cinsiyet) {
        secimler.set(no, cinsiyet);
        guncelle();
    }

    function kaldir(no) {
        secimler.delete(no);
        guncelle();
    }

    // Haritadaki seçili koltukları, seçim listesini, toplamı ve form alanlarını yeniler
    function guncelle() {
        var sirali = Array.from(secimler.keys()).sort(function (a, b) { return a - b; });

        harita.querySelectorAll(".sb-koltuk:not(:disabled)").forEach(function (dugme) {
            var no = parseInt(dugme.dataset.no, 10);
            var secili = secimler.has(no);
            dugme.classList.toggle("sb-koltuk-secili", secili);
            dugme.setAttribute("aria-pressed", secili);
            dugme.setAttribute("aria-label", no + " numaralı koltuk, " +
                (secili ? "seçildi (" + CINSIYETLER[secimler.get(no)].toLocaleLowerCase("tr-TR") + ")" : "boş"));
            if (secili) {
                dugme.dataset.harf = HARFLER[secimler.get(no)];
            } else {
                delete dugme.dataset.harf;
            }
        });

        liste.replaceChildren();
        alanlar.replaceChildren();
        sirali.forEach(function (no, sira) {
            var cinsiyet = secimler.get(no);

            var oge = document.createElement("li");
            oge.append("Koltuk " + no + " · " + CINSIYETLER[cinsiyet]);
            var kaldirDugmesi = document.createElement("button");
            kaldirDugmesi.type = "button";
            kaldirDugmesi.className = "btn-close";
            kaldirDugmesi.setAttribute("aria-label", no + " numaralı koltuğu kaldır");
            kaldirDugmesi.addEventListener("click", function () { kaldir(no); });
            oge.append(kaldirDugmesi);
            liste.append(oge);

            alanlar.append(gizliAlan("Koltuklar[" + sira + "].KoltukNo", no));
            alanlar.append(gizliAlan("Koltuklar[" + sira + "].Cinsiyet", cinsiyet));
        });

        bosMesaj.hidden = secimler.size > 0;
        toplam.textContent = para.format(secimler.size * fiyat);
        devam.disabled = secimler.size === 0;
    }

    function gizliAlan(ad, deger) {
        var alan = document.createElement("input");
        alan.type = "hidden";
        alan.name = ad;
        alan.value = deger;
        return alan;
    }

    harita.addEventListener("click", function (olay) {
        var dugme = olay.target.closest(".sb-koltuk");
        if (!dugme || dugme.disabled) return;

        var no = parseInt(dugme.dataset.no, 10);
        if (secimler.has(no)) {
            kaldir(no);
        } else if (secimler.size >= enFazla) {
            uyar("Bir seferde en fazla " + enFazla + " koltuk seçebilirsiniz.");
        } else {
            cinsiyetSor(dugme);
        }
    });

    cinsiyetDugmeleri.forEach(function (secenek) {
        secenek.addEventListener("click", function () {
            if (!sorulanKoltuk) return;
            sec(parseInt(sorulanKoltuk.dataset.no, 10), parseInt(secenek.dataset.cinsiyet, 10));
            modal.hide();
        });
    });

    // Pencere kapanınca odak, cinsiyeti sorulan koltuğa döner
    modalKutusu.addEventListener("hidden.bs.modal", function () {
        if (sorulanKoltuk) sorulanKoltuk.focus();
        sorulanKoltuk = null;
    });

    // Tarayıcı geri tuşuyla sayfa önbellekten gelirse (bfcache) seçim ekranla uyumlu kalsın
    window.addEventListener("pageshow", guncelle);
});
