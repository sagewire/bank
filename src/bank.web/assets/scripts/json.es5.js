"use strict";

$(function () {

    $('[data-send=json]').click(function () {

        var source = $(this);
        var name = source.data("name");
        var value = source.data("value");
        var flash = $("#flash");

        var jqxhr = $.ajax({
            url: "/data/" + name,
            cache: false,
            data: { id: value }
        }).done(function (data) {

            console.log("success");
            var result = JSON.parse(data);

            flash.addClass("alert-success");
            flash.html(result.html);
            flash.animate({ height: "6vh", speed: 500 });

            setTimeout(function () {
                flash.fadeOut();
                setTimeout(function () {
                    flash.height(0);
                }, 1000);
            }, 3000);
        }).fail(function () {
            console.log("error");
        }).always(function () {
            console.log("complete");
        });
    });
});

//var dash = $("#sidenav-dashboard");

//var dest = dash.offset();
//var src = $("#plus").offset();

//console.log(source);
//$("#test").css(src);
//$("#test").show();

//$("#test").animate({
//    top: dest.top,
//    easing: "swing",
//    speed: 5000
//})

//$("#test").animate({
//    top: dest.top,
//    left: dest.left,
//    easing: "swing",
//    speed: 1000
//})

