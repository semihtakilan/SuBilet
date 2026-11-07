// Sefer sonuç sayfası: filtre ve sıralama seçimi değişince sonuçlar yenilenir.
// Geniş ekranda kutu işaretlenir işaretlenmez uygulanır. Mobilde filtre paneli açıkken
// birden fazla seçim yapılabilsin diye "Uygula" düğmesi beklenir.
document.addEventListener("DOMContentLoaded", function () {
    var form = document.getElementById("seferFiltreFormu");
    if (!form) return;

    var genisEkran = window.matchMedia("(min-width: 992px)");
    form.classList.add("sb-otomatik");

    function gonder() {
        form.classList.add("sb-yukleniyor");
        form.submit();
    }

    form.addEventListener("change", function (olay) {
        if (olay.target.type === "checkbox" && genisEkran.matches) gonder();
    });

    // Sıralama kutusu formun dışında durur (form="seferFiltreFormu"); her ekranda hemen uygulanır.
    document.querySelectorAll("select[form='seferFiltreFormu']").forEach(function (kutu) {
        kutu.addEventListener("change", gonder);
    });
});
