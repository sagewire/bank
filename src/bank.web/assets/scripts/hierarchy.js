
$(function () {

    var hierarchy = null;

    function init() {
        var peopleElement = document.getElementById("org-chart");



        var json = $(peopleElement).data("data");

        hierarchy = new getOrgChart(peopleElement, {
            //enableSearch: false,
            expandToLevel: 5,
            //enableZoom: false,
            //theme: "monica",
            siblingSeparation: 20,
            orientation: getOrgChart.RO_LEFT,
            primaryFields: ["label", "value"],
            linkType: "M",
            enableEdit: false,
            enableDetailsView: false,
            dataSource: json,
            renderNodeEvent: renderNodeEventHandler,
            //customize: {
            //    "RIAD4079": { color: "green", height: "50px" }
            //}
        });

    }

    function renderNodeEvent(sender, args) {
        console.log(args);
    }

    init();

    var timeout = null;
    var lastPosition = null;
    var zoomView = true;

    $("#mouse-overlay").click(function (e) {
        $(this).removeClass("fullscreen");
    });

    $(window).scroll(function () {
        //if (!timeout) {
        clearTimeout(timeout);
        //lastPosition = $(window).scrollTop();

        $("#mouse-overlay").addClass("fullscreen");


        timeout = setTimeout(function () {
            //var currentPosition = $(window).scrollTop();
            console.log('scroll stop');

            //console.log(hierarchy);
            //hierarchy.config.enableZoom = true;
            $("#mouse-overlay").removeClass("fullscreen");
            clearTimeout(timeout);
            timeout = null;
        }, 600);
        //}
    });


});

function isNumeric(n) {
    return !isNaN(parseFloat(n)) && isFinite(n);
}

var hex2rgb = function (hex) {
    var result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
    return result ? [
        parseInt(result[1], 16),
        parseInt(result[2], 16),
        parseInt(result[3], 16)
    ] : null;
};

var rgb2hex = function (rgb) {
    return "#" + ((1 << 24) + (rgb[0] << 16) + (rgb[1] << 8) + rgb[2]).toString(16).slice(1);
};

function interpolateColor(color1, color2, factor) {
    if (arguments.length < 3) { factor = 0.5; }
    var result = color1.slice();
    for (var i = 0; i < 3; i++) {
        result[i] = Math.max(0, Math.round(result[i] + factor * (color2[i] - color1[i])));
    }
    console.log(result);
    return result;
};

var start = hex2rgb("#FFFF00");
var end = hex2rgb("#FF0000");
var max = null;
var min = null;
var factor = null;

//function setFactor(chart) {
//    max = null;
//    min = null;
//    for (var id in chart.nodes) {
//        var node = chart.nodes[id];
//        if (node.data["value"]) {
//            var salary = node.data["value"].replace(/,/g, "");
//            if (isNumeric(salary)) {
//                if (max == null && min == null) {
//                    max = salary;
//                    min = salary;
//                }
//                else {
//                    max = Math.max(salary, max);
//                    min = Math.min(salary, min);
//                }
//            }
//        }
//    }

//    factor = (max - min) / 100;
//}

function renderNodeEventHandler(sender, args) {
    var salary = 0;
    if (args.node.data["value"] !== null) {
        salary = args.node.data["value"].replace(/,/g, "");
    }
    var hex = "#fff";

    if (salary !== 0) {

        var relativeTo = args.node.data["relativeTo"] * 40;

        var rgb = interpolateColor(start, end, relativeTo);
        hex = rgb2hex(rgb);
    }
    

    
    args.content[1] = args.content[1].replace("rect", "rect style='fill: " + hex + "; stroke: " + hex + ";'")
    //args.content[1] = args.content[1].replace("rect", "rect style='fill: rgba(255, 0, 0, .5); stroke:  rgba(255, 0, 0, .2);'")
}
