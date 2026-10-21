// Ana sayfa: kalkış/varış şehirlerini yer değiştir
$(function () {
    $("#sehirDegistir").on("click", function () {
        var kalkis = $("#KalkisSehirId");
        var varis = $("#VarisSehirId");
        var gecici = kalkis.val();
        kalkis.val(varis.val());
        varis.val(gecici);
    });

    // "Bugün" / "Yarın" düğmeleri tarih kutusunu doldurur
    $("[data-gun]").on("click", function () {
        var tarih = new Date();
        tarih.setDate(tarih.getDate() + parseInt($(this).data("gun"), 10));
        var ay = String(tarih.getMonth() + 1).padStart(2, "0");
        var gun = String(tarih.getDate()).padStart(2, "0");
        $("#Tarih").val(tarih.getFullYear() + "-" + ay + "-" + gun).trigger("change");
    });
});
