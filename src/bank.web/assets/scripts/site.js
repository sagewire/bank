var sidenavTimer = null;
var sidenavState = null;

function openSidenav(delay) {
    
    sidenavTimer = setTimeout(function () {
        var nav = $("#global-sidenav");
        nav.addClass("open");
        $(".top-nav").addClass("sidenav-open");
        $("#search-box").focus();
    }, delay);
}

function closeSidenav() {
    clearTimeout(sidenavTimer);

    var nav = $("#global-sidenav");
    nav.removeClass("open");
    $(".top-nav").removeClass("sidenav-open");

}

function toggleSidenav(delay) {
    var nav = $("#global-sidenav");
    if (nav.hasClass("open")) {
        closeSidenav();
    }
    else {
        openSidenav(delay);
    }
}

$(function () {

    $("#global-sidenav").on("mouseenter", function () {
        openSidenav(225);
    })

    $("#global-sidenav").on("mouseleave", function () {
        closeSidenav();
    })

    $("#sidenav-toggler").click(function () {
        toggleSidenav(0);
    })

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


    /* off-canvas sidebar toggle */
    $('[data-toggle=offcanvas]').click(function () {
        $('.row-offcanvas').toggleClass('active');
        $('.collapse').toggleClass('in').toggleClass('hidden-xs').toggleClass('visible-xs');
    });


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
        
    });



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


$(function () {


    $.typeahead({
        input: '.js-typeahead-name',
        filter: false,
        dynamic: true,
        delay: 90,
        maxItem: 20,
        //order: "desc",
        template: function (query, item) {
         //   <a href="@Model.Organization.ProfileUrl"
         //   class="preload avatar avatar-md media-object img-responsive"
         //   style="background-image: url('@Model.Organization.Avatar')">

         //</a>
            return '<div class="search-result"><div class="media"><i class="media-left"><a href="{{url}}" class="media-object avatar avatar-sm img-responsive" style="background-image: url({{avatar}})"></a></i><div class="name">{{name}}<br/><small>{{city}}, {{state}} <div class="float-right"><b>{{assets}} Assets</b></div></small></div></div>';
        },
        emptyTemplate: "No results for {{query}}",
        href: "{{url}}",
        source: {
            lenders: {
                display: "name",
                ajax: function (query) {
                    return {
                        url: "/modals/search/data",
                        path: "data.snippets",
                        data: {
                            q: "{{query}}"
                        }
                    }
                }
            }
        },
        callback: {
            onReady: function () {

                setTimeout(function () {
                    $("#search-box").focus();
                }, 750);
            },
            onClick: function (node, a, item, event) {
                window.location = item.url;
            },
            onSubmit: function (node, form, item, event) {
                return false;
            }
        }
    });
});
