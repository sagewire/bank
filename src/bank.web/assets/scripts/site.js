$(function () {
    var lastSize = findBootstrapEnvironment();
    console.log(lastSize);

    window.sr = ScrollReveal();
    sr.reveal('.reveal', { distance: "5px", origin: "right", duration: 500, delay: 0 });
    sr.reveal('.reveal-no-mobile', { distance: "5px", origin: "right", duration: 500, delay: 0, mobile: false });
    sr.reveal('.reveal-slow', { distance: "5px", origin: "right", duration: 2000, delay: 0 });

    $(".scroll-to").click(function (e) {
        e.preventDefault();
        var href = $(this).attr("href");

        $('html, body').animate({
            scrollTop: $(href).offset().top
        }, 1000);
    });

    var sidebar = $('.sidebar-fixed');
    console.log(sidebar);

    if (sidebar.innerWidth() !== undefined) {
        var top = sidebar.offset().top - parseFloat(sidebar.css('margin-top'));
        var sideBarWidth = 0;

        $(window).scroll(function (event) {
            sidebarPosition();
        });
    }

    if ($(window).scrollTop()) {
        sidebarPosition();
    }

    
    $(".card-menu .dropdown-item").click(function (e) {
        alert("Demo feature. Not ready yet.");
    });

    $("body").on("mouseenter", ".lineitem-cell", function (e) {
        $(this).find(".trending-percent").fadeIn(500);
    });

    $("body").on("mouseleave", ".lineitem-cell", function (e) {
        $(this).find(".trending-percent").fadeOut();
    });

    $("body").on("show.bs.popover", function (e) {
        //console.log('hi');
    });

    //$(".mdrm-cell").on("click", function (e) {

        //var row = $(e.currentTarget).parents("tr");
        //var cell = $(e.currentTarget);
        //var url = cell.data("url").toLowerCase();
        //var loaded = cell.data("loaded");
        //var organizationCells = $("td[data-organization]");

        //var organizations = [];

        //$.each(organizationCells, function (index, value) {
        //    organizations.push($(value).data("organization"));
        //});

        //console.log(organizations);

        //if (!loaded) {
        //    console.log(url);

        //    var jqxhr = $.ajax({
        //        url: url,
        //        method: "GET",
        //        data: {
        //            c: organizations.join(",")
        //        }
        //    })
        //      .done(function (data) {

        //          cell.data("loaded", true);
        //          $("<tr><td class='no-border' style='padding-left:" + cell.css("padding-left") + "' colspan='20'>" + data + "</td></tr>").insertAfter($(e.target).parents("tr"));
        //          //$(data).appendTo($(e.target));
        //      })
        //}
    //});

    $("body").on("click", ".inline-report .close", function (e) {
        var target = $(e.currentTarget).parents(".inline-report");
        target.remove();
    });

    $(".mdrm-row").on("click", function (e) {

        var row = $(e.currentTarget);

        var url = row.data("url").toLowerCase();
        var organizationCells = row.parents("table").find("th[data-organization]");

        var organizations = [];

        $.each(organizationCells, function (index, value) {
            organizations.push($(value).data("organization"));
        });

        if (!row.next().hasClass("inline-report")) {
            
            var newRow = $("<tr style='display:none;' class='inline-report'><td class='no-border' colspan='20'><div class='inside'><div class='p-1 text-xs-center'><i class='fa fa-circle-o-notch fa-spin fa-3x fa-fw'></i></div></div></td></tr>").insertAfter($(e.currentTarget));
            var inside = newRow.find(".inside");

            newRow.show("slow");

            var jqxhr = $.ajax({
                url: url,
                method: "GET",
                data: {
                    c: organizations.join(",")
                }
            })
              .done(function (data) {

                  inside.html(data);

              })
        }
        //else {
        //    console.log(row.next(".inline-report"));
        //    row.next().show(".inline-report");
        //}
    });

    //$('[data-toggle="popover"]').popover({

    //    trigger: 'click',
    //    placement: "top",
    //    html: true,
    //    content: '<div class="fact-options">' +
    //                    '<i class="fa fa-line-chart" aria-hidden="true"></i>' +
    //                    '<i class="fa fa-star-o" aria-hidden="true"></i>' +
    //                    //'<i class="fa fa-bell-o" aria-hidden="true"></i>' +
    //                '</div>'
    //})

    

    $("#modal").on("show.bs.modal", function (e) {
        var content = $(e.relatedTarget).data("content");
        var html = $(e.relatedTarget).data("html");
        var modalBody = $(".modal-body");

        if (content !== undefined) {

            var jqxhr = $.ajax(content)
                              .done(function (data) {
                                  modalBody.html(data);
                              })
                              .fail(function () {
                                  console.log("error");
                              })
                              .always(function () {

                              });
        }
        else if (html !== undefined) {
            modalBody.html(html);
        }

    });

    function sidebarPosition() {
        if (lastSize !== "lg") {
            return;
        }

        var y = $(window).scrollTop();
        if (y >= top) {
            sideBarWidth = sidebar.width();
            sidebar.width(sideBarWidth);
            sidebar.addClass('fixed');
            sidebar.find(".sidebar-fixed-hidden").show("slow");
            sidebar.find(".sidebar-fixed-displayed").hide("slow");
        } else {
            sidebar.removeClass('fixed');
            if ($(document).height() > $(window).height()) {
                //if (y === 0) {
                sidebar.find(".sidebar-fixed-hidden").hide("slow");
                sidebar.find(".sidebar-fixed-displayed").show("slow");

            }
        }
    }

    function findBootstrapEnvironment() {
        var envs = ["ExtraSmall", "Small", "Medium", "Large"];
        var envValues = ["xs", "sm", "md", "lg"];

        var $el = $('<div>');
        $el.appendTo($('body'));

        for (var i = envValues.length - 1; i >= 0; i--) {
            var envVal = envValues[i];

            $el.addClass('hidden-' + envVal + "-up");
            if ($el.is(':hidden')) {
                $el.remove();

                return envValues[i];
            }
        };
    }


});