
$(function () {

    var hierarchy = null;

    function init() {
        var peopleElement = document.getElementById("org-chart");
        
        getOrgChart.themes.bankjitsu = {
            "size": [
               500,
               220
            ],
            "toolbarHeight": 46,

            "textPointsNoImage": [
               {
                   "x": 10,
                   "y": 200,
                   "width": 490,
                   "anchor": "end"
               },
               {
                   "x": 475,
                   "y": 55,
                   "width": 490
               },
               {
                   "x": 25,
                   "y": 125,
                   "width": 490
               },
               {
                   "x": 10,
                   "y": 90,
                   "width": 490
               },
               {
                   "x": 10,
                   "y": 115,
                   "width": 490
               },
               {
                   "x": 10,
                   "y": 140,
                   "width": 490
               }
            ],
            "box": "<path class=\"get-box\" d=\"M0 0 L500 0 L500 220 L0 220 Z\"/>",
            "text": "<text width=\"[width]\" text-anchor=\"[anchor]\" class=\"get-text get-text-[index]\" x=\"[x]\" y=\"[y]\">[text]</text>",
            //"image": "<clipPath id=\"getMonicaClip\"><circle cx=\"105\" cy=\"65\" r=\"85\" /></clipPath><image preserveAspectRatio=\"xMidYMid slice\" clip-path=\"url(#getMonicaClip)\" xlink:href=\"[href]\" x=\"20\" y=\"-20\" height=\"170\" width=\"170\"/>"
        }



        var json = $(peopleElement).data("data");
        var expandToLevel = $(peopleElement).data("expand-to-level");

        $.each(json, function (index, value) {
            value.percent = (value.relativeTo * 100).toFixed(2) + "%";
        });

        hierarchy = new getOrgChart(peopleElement, {
            //enableSearch: false,
            expandToLevel: expandToLevel,
            //enableZoom: false,
            theme: "bankjitsu",
            siblingSeparation: 15,
            
            orientation: getOrgChart.RO_LEFT,
            primaryFields: ["label", "value", "percent"],
            linkType: "M",
            enableEdit: false,
            enableDetailsView: false,
            dataSource: json,
            renderNodeEvent: renderNodeEventHandler,

        });

        console.log(hierarchy.move);
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

    return result;
};

var start = hex2rgb("#FFFF00");
var end = hex2rgb("#FF0000");

var debitStart = hex2rgb("#0000FF");
var debitEnd = hex2rgb("#00FF00");

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
    var relativeTo = 1;

    if (salary !== 0) {

        relativeTo = args.node.data["relativeTo"];// * 200;
        var balance = args.node.data["balance"];

        var s = start;
        var e = end;
        hex = "#FF0000";

        console.log(args.node.data["label"] + " " + balance);

        if (balance === null) {
            var val = args.node.data["value"].replace(/,/g, "");
            if (val > 0) {
                balance = "credit";
            }
            else {
                balance = "debit";
            }
        }

        if (balance == "credit") {
            //console.log('credit');
            s = debitStart;
            e = debitEnd;
            hex = "#00FF00";
        }

        //var rgb = interpolateColor(s, e, relativeTo);
        //hex = rgb2hex(rgb);
    }

    var rgb = hex2rgb(hex);

    relativeTo = Math.min(relativeTo + .2, 1);

    //console.log(args);

    args.node.data

    //args.content[1] = args.content[1].replace("rect", "rect style='fill: " + hex + "; stroke: " + hex + ";'")
    args.content[1] = args.content[1].replace("path", "path style='fill: rgba(" + rgb + ', ' + relativeTo + "); stroke:  rgba(" + rgb + ", " + relativeTo + ");'")
    args.content[2] = args.content[2].replace("[anchor]", "start");
    args.content[3] = args.content[3].replace("[anchor]", "end");
    args.content[4] = args.content[4].replace("[anchor]", "start");
}
