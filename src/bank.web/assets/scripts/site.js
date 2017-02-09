var sidenavTimer = null;
var sidenavState = null;

function openSidenav(delay) {

    
    if (isSidenavOpen()) {
        return;
    }
    
    sidenavTimer = setTimeout(function () {
        var nav = $("#global-sidenav");
        nav.addClass("open");
        console.log('open');
        $(".darken").fadeIn();
        $(".top-nav").addClass("sidenav-open");
        //$("#search-box").focus();
    }, delay);
}

function closeSidenav() {

    var nav = $("#global-sidenav");
    if (!isSidenavOpen()) {
        return;
    }
    
    console.log('close');

    clearTimeout(sidenavTimer);

    var nav = $("#global-sidenav");
    nav.removeClass("open");

    $(".darken").fadeOut();
    
    $(".account").hide();
    $(".top-nav").removeClass("sidenav-open");

}

function isSidenavOpen() {
    return $("#global-sidenav").width() > 75;
}

function toggleSidenav(delay) {
    
    if (isSidenavOpen()) {
        closeSidenav();
    }
    else {
        openSidenav(delay);
    }
}

$(function () {

    $(document).keyup(function (e) {
        if (e.keyCode == 27) { // escape key maps to keycode `27`
            closeSidenav();

            $('.typeahead').typeahead('setQuery', '');

            $(".fullscreen-handle").each(function (index, value) {

                toggleFullscreen($(value));
            })

        }
    });


    $("#global-sidenav").on("click", function () {
        if (!isSidenavOpen()) {
            return false;
        }
    })

    $("#global-sidenav").on("mouseenter", function () {
        openSidenav(225);
    })

    $("#global-sidenav").on("mouseleave", function () {
        closeSidenav();
    })

    $("#sidenav-toggler").click(function (e) {
        e.preventDefault();
        toggleSidenav(0);
        return false;
    })


    var preloaded = [];
    var preloading = false;
    var leavingPage = false;

    $("a").on("click", "body", function () {
        leavingPage = true;
    });

    $('a').hover(function () {
        var href = $(this).attr("href");
        preload(href);
    });

    function preload(href) {

        if (preloading) {
//            console.log('cancel preload ' + href);
            return;
        }
  //      console.log('preloading ' + href);
        //var href = $(this).attr("href");

        if (href === undefined) {
            return;
        }

        if (href.indexOf("http") > -1 || href.indexOf("tel") > -1) {
            return;
        }

        //var url = $(this).attr("href");

        if (href == window.location.pathname) {
            return;
        }

        if ($.inArray(href, preloaded) === -1) {
            preloading = true;
            console.log('preloading ' + href);
            $.get(href)
                .done(function (html) {
                    if (!leavingPage) {
                        preloaded.push(href);
                    }
                })
            .always(function () {
                preloading = false;
                console.log('done');
            });
        }
    }


    $(".darken").click(function () {
        closeSidenav();
    });

    $("[data-toggle=fullscreen]").click(function () {

        toggleFullscreen($(this));
    });

    function toggleFullscreen(target) {
        
        var card = target.closest(".card");
        var charts = card.find(".chart-responsive");

        if (target.data("fullscreen")) {

            card.removeClass("fullscreen");
            $("body").removeClass("noscroll");
            target.addClass("fa-arrows-alt");
            target.removeClass("fa-times");
            target.data("fullscreen", false);
            charts.removeClass("fullscreen-chart");
            target.removeClass("fullscreen-handle");

        }
        else {
            card.addClass("fullscreen");
            $("body").addClass("noscroll");

            target.removeClass("fa-arrows-alt");
            target.addClass("fa-times");
            target.data("fullscreen", true);
            charts.addClass("fullscreen-chart");
            target.addClass("fullscreen-handle");

        }

        if (charts.length > 0) {
            charts.highcharts().reflow();
        }

    }

    $(".favorite, .not-favorite").on("mouseenter", function () {
        var star = $(this);
        toggleStar(star);
    });

    $(".favorite, .not-favorite").on("mouseleave", function () {
        var star = $(this);
        toggleStar(star);
    });

    $(".favorite, .not-favorite").on("click", function () {
        var star = $(this);
        toggleStar(star);
    });



    function toggleStar(star) {
        
        if (star.hasClass("fa-star")) {
            console.log('star');
            star.addClass("fa-star-o");
            star.removeClass("fa-star");
        }
        else {
            console.log('star-o');

            star.addClass("fa-star");
            star.removeClass("fa-star-o");
        }
    }

    //$(".favorite").on("mouseleave", function () {
    //    var f = $(this);
    //    if (f.hasClass("fa-star")) {
    //        $(this).addClass("fa-star-o");
    //        $(this).removeClass("fa-star");
    //    }
    //    else {
    //        $(this).addClass("fa-star");
    //        $(this).removeClass("fa-star-o");
    //    }
    //});

    //$(".not-favorite").on("mouseenter", function () {
    //    $(this).removeClass("fa-star-o");
    //    $(this).addClass("fa-star");
    //});

    //$(".not-favorite").on("mouseleave", function () {
    //    $(this).addClass("fa-star-o");
    //    $(this).removeClass("fa-star");
    //});

    $(".toggle").on("click", function (e) {
        e.preventDefault();

        var target = $(this).attr("href");
        var group = $(this).data("group");
        var isVisible = $(target).is(':visible');

        if (isVisible) {
            $(target).hide();
        }
        else {
            $("." + group).hide();
            $(target).show();
        }
        return false;
        
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



    //$("#modal").on("show.bs.modal", function (e) {
    //    var content = $(e.relatedTarget).data("content");
    //    var html = $(e.relatedTarget).data("html");
    //    var modalBody = $(".modal-body");

    //    if (content !== undefined) {

    //        var jqxhr = $.ajax(content)
    //                          .done(function (data) {
    //                              modalBody.html(data);
    //                          })
    //                          .fail(function () {
    //                              console.log("error");
    //                          })
    //                          .always(function () {

    //                          });
    //    }
    //    else if (html !== undefined) {
    //        modalBody.html(html);
    //    }

    //});

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

    //var target = $("thead").offset().top,
    //timeout = null;

    //var targets = $("table");

    
    //$(window).scroll(function () {
    //    if (!timeout) {
    //        timeout = setTimeout(function () {
    //            console.log('scroll');
    //            clearTimeout(timeout);
    //            timeout = null;

    //            targets.each(function (index, elem) {

    //                var table = $(elem);
    //                var thead = table.find("thead");
    //                var row = table.find("tr").first();

    //                var columns = thead.find("td");

    //                //console.log(table);
    //                //console.log(thead);

    //                var position = table.offset();
    //                var visibleWidth = table.outerWidth();;
    //                var actualWidth = thead.width();
                    

    //                if ($(window).scrollTop() >= position.top) {
    //                    console.log('made it');
    //                    var runningTotal = 0;

    //                    //columns.each(function (index, value) {
    //                    //    var column = $(value);
    //                    //    var w = column.css("width");
    //                    //    var h = column.css("height");
    //                    //    var pos = column.offset();

    //                    //    console.log(w);

    //                    //    column.css("width", w);
    //                    //    column.css("height", h);

    //                    //    column.css({ left: runningTotal });
    //                    //    runningTotal = runningTotal + new Number(w.replace("px", ""));
    //                    //    console.log('running ' + runningTotal);

    //                    //    if (runningTotal > visibleWidth) {
    //                    //        console.log('fixing');
                                
    //                    //        //column.css("position", "relative");
    //                    //        column.css("min-width", "0");
    //                    //        column.css("overflow", "hidden");
    //                    //        //column.css("width", "50px");
    //                    //    }
    //                    //});

    //                    var row = table.find(".header-row");

    //                    thead.css({ width: thead.css("width") });

    //                    table.addClass("fixed");
    //                    //row.width(actualWidth);
    //                    //thead.width(visibleWidth);
    //                    //thead.css({height: "200px" });
    //                    //thead.width(visibleWidth);


    //                    //row.width(visibleWidth);

    //                    table.scroll(function (e) {
    //                        console.log('side scroll');
    //                        //var table = $(table);
    //                        var thead = table.find("thead");

    //                        var scrollLeft = table.scrollLeft();

    //                        thead.css({"left": table.offset().left - scrollLeft});
    //                        //console.log(thead.offset());
    //                    });
    //                }
    //            });

                
    //        }, 250);
    //    }
    //});

    //$(window).scroll(function () {
    //    if (!timeout) {
    //        timeout = setTimeout(function () {
    //            console.log('scroll');
    //            clearTimeout(timeout);
    //            timeout = null;
    var lastPosition = 0;

    $("table").scroll(function (e) {
    
        var table = $(this);
        console.log('side scroll');

        var thead = table.find("thead.tableFloatingHeaderOriginal");
        var clone = table.find("thead.tableFloatingHeader");

        //thead.scrollLeft($(this).scrollLeft());

        thead.animate({
            scrollLeft: $(this).scrollLeft()
        }, 10);

        clone.animate({
            scrollLeft: $(this).scrollLeft()
        }, 10);
    });

    $('table').stickyTableHeaders({ fixedOffset: $('.top-nav'), cacheHeaderHeight: true });

    $.typeahead({
        input: '.js-typeahead-name',
        filter: false,
        dynamic: true,
        minLength: 0,
        searchOnFocus: true,
        delay: 250,
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
                        url: "/data/search/data",
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

                //setTimeout(function () {
                //    $("#search-box").focus();
                //}, 750);
            },
            onNavigateAfter: function(node, lis, a, item, query, event) {
                var href = item.url;
                preload(href);
            },
            onMouseEnter: function (node, a, item, event) {
                var href = item.url;
                preload(href);

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
