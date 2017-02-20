
$(function () {


    Highcharts.theme = {
        colors: ['#058DC7', '#64E572', '#FF9655', '#FFF263', '#6AF9C4', '#50B432', '#ED561B', '#DDDF00'],
        chart: {
            backgroundColor: null,
            borderWidth: 0,
            //margin: [0, 0, 0, 0],
            //marginLeft: 0,
            //marginRight: 0,
            //marginTop: 0,
            //marginBottom: 0,
            //spacingBottom: 0,

            style: {
                overflow: 'visible'
            },
            skipClone: true
        },
        lang: {
            thousandsSep: ","
        },
        title: {
            text: ''
        },
        credits: {
            enabled: false
        },
        xAxis: {
            lineWidth: 0,
            gridLineWidth: 0,
            gridLineColor: '#eeeeee',
            type: "datetime",
            labels: {
                enabled: false
            },
            title: {
                text: null
            },
            startOnTick: false,
            endOnTick: false,
            maxPadding: 0,
            minPadding: 0,
            tickPositions: []
        },
        yAxis: {
            gridLineWidth: 0,
            gridLineColor: '#eeeeee',
            endOnTick: false,
            startOnTick: false,
            labels: {
                enabled: false
            },
            title: {
                text: null
            },
            tickPositions: []
        },
        legend: {
            enabled: true,
            layout: "vertical",
            itemWidth: 200,
            itemStyle: {
                fontWeight: 'bold',
                width: '180%',
                textOverflow: "ellipsis",
                overflow: "hidden",
                whiteSpace: "nowrap"
            },
            itemHoverStyle: {
                overflow: "auto",
                whiteSpace: "normal"

            },
            useHTML: true
        },
        //tooltip: {
        //    shared: true
        //},
        tooltip: {

            formatter: function () {

                var name = this.series.name;
                var key = this.key;

                if ($.isNumeric(key)) {
                    key = "";
                }

                return '<b>' + name + '</b><br/><i>' + key + "</i><br/>" +
                    Highcharts.dateFormat('%e-%b-%Y', new Date(this.x)) + '<br/> Value: ' + Highcharts.numberFormat(this.y, 0, "", ",") + ' ';
            }
        },
        plotOptions: {
            pie: {
                dataLabels: {
                    enabled: false
                },
                showInLegend: true
            },
            areaspline: {
                pointPlacement: null

            },
            column: {
                pointPadding: 0
            },
            line: {

            },
            series: {
                enabledMouseTracking: false,
                animation: {
                    duration: 1750
                },
                lineWidth: 2,
                //lineColor: "transparent",
                shadow: false,
                states: {
                    hover: {
                        lineWidth: 2
                    }
                },
                marker: {
                    radius: 0,
                    symbol: "circle",
                    states: {
                        hover: {
                            radius: 5
                        }
                    }
                },
                fillOpacity: 0.25
            }
        }
    };

    Highcharts.setOptions(Highcharts.theme);

    var lastHover = null;
    var pending = false;

    $("body").on("mouseenter", ".lineitem-cell,.lineitem-sparkline", function (e) {

        if (pending) {
            return;
        }

        lastHover = this;
        var thisHover = this;

        pending = true;

        setTimeout(function () {


            if (lastHover === thisHover) {

                //console.log('starting timer');
                //console.log(thisHover);
                //console.log(lastHover);

                if ($(thisHover).find(".lineitem-sparkline").exists()) {
                    $(thisHover).find(".lineitem-sparkline").fadeIn();
                    return;
                }

                var series = $(thisHover).data('series');

                var chartDiv = document.createElement("div");
                chartDiv.className = "lineitem-sparkline";
                $(chartDiv).data("series", series);
                $(thisHover).prepend(chartDiv);


                $(chartDiv).highcharts('SparkLine', {
                    plotOptions: {
                        series: {
                            animation: {
                                duration: 750
                            }
                        }
                    }
                });

            }
        }, 200);



    });

    $("body").on("mouseleave", ".lineitem-cell", function (e) {
        pending = false;
        var that = this;
        setTimeout(function () {
            $(that).find(".lineitem-sparkline").fadeOut();
        }, 500);
    });

});


function renderCharts() {
    console.log('rendering charts...');

    $("[data-chart-type='sankey']").each(function (index, element) {

        google.charts.load('current', { 'packages': ['sankey'] });
        google.charts.setOnLoadCallback(function () {
            drawSankeyChart(element);
        });

    });


    $("[data-chart-type='combo']").each(function (index, element) {

        $(this).highcharts('Combo', {
            plotOptions: {
                series: {
                    stacking: "normal"
                }
            }
        });

    });

    $("[data-chart-type='key-ratio']").each(function (index, element) {

        $(this).highcharts('Combo', {
            chart: {
                marginBottom: 75,
                spacingBottom: 0
            },
            plotOptions: {
                areaspline: {
                    lineWidth: 0
                }
            },
            legend: {
                enabled: true,
                layout: "vertical"
            },
            tooltip: {
                //shared: true,
                split: true,
                useHTML: true,
                padding: 0,
                valueDecimals: 2,
                borderWidth: 0,
                headerFormat: "<table class='table'>",
                pointFormat: "<tr><td><b>{point.y}    </b></td><td> {point.x:%b %Y}</td></tr>",
                footerFormat: "</table>",
                //footerFormat: "<b>${point.y}</b> {point.x:%b %Y}",
                formatter: null,
                positioner: function () {
                    return { x: -5, y: -100 };
                },
            },
            series: [
                {},
                { lineWidth: 0 }
            ]
        });

    });


    $("[data-chart-type='fixed-placement']").each(function (index, element) {

        $(this).highcharts('Combo', {
            plotOptions: {
                bar: {
                    grouping: true,
                    borderWidth: 0,
                    groupPadding: 0,
                    pointPadding: 0
                }
            },
            yAxis: {
                max: 100
            },
            series: [
                {
                    color: 'rgba(165,170,217,.5)',
                    pointPadding: 0.1,
                    zIndex: 100
                },
                {
                    color: 'rgba(126,86,134,.9)',
                    pointPadding: 0.2,
                },
                {
                    color: 'rgba(186,60,61,.9)',
                    pointPadding: 0.3,
                }
            ]
        });

    });


    $('[data-chart-type="primary"]').highcharts('Combo', {
        chart: {
            marginBottom: 85,
            spacingBottom: 0
        },
        plotOptions: {
            areaspline: {
                lineWidth: 0,
            }
        },
        lang: {
            thousandsSep: ','
        },
        legend: {
            enabled: true,
            layout: "horizontal"
        },
        tooltip: {
            shared: true,
            //split: true,
            borderWidth: 0,
            shadow: false,
            useHTML: true,
            valueDecimals: 0,
            headerFormat: "<table class='primary-tooltip table table-sm table-striped'><tr><th colspan='2'>{point.x:%b %e %Y}</th></th>",
            pointFormat: "<tr><td style='border-left: 10px solid {point.series.color}'>{point.series.name}</td><td style='text-align: right;'>{point.y}</td></tr>",
            footerFormat: "</table>",
            formatter: null,
            positioner: function () {
                return { x: 0, y: 0 };
            },
        },
        xAxis: {
            gridLineWidth: 1,
            tickPositions: null,
            labels: {
                enabled: true
            },

        },
        yAxis: {
            gridLineWidth: 0,
            tickPositions: null
        },
        series: [
            {
                fillColor: {
                    linearGradient: { x1: .2, x2: 0, y1: 0, y2: .75 },

                    stops: [
                        [0, Highcharts.Color('#2E96EA').setOpacity(.70).get('rgba')],
                        [1, Highcharts.Color('#30C8CA').setOpacity(.70).get('rgba')]
                    ]
                }
            }
        ]
    });
}

function drawSankeyChart(element) {
    var d = $(element).data("series")

    console.log(d);

    var data = google.visualization.arrayToDataTable(d[0].data);

    var view = new google.visualization.DataView(data);

    var max = google.visualization.data.max(view.getDistinctValues(2));

    var filteredRows = view.getFilteredRows([{ column: 2, minValue: max * 0.008 }]);

    view.setRows(filteredRows);

    var colors = ['#a6cee3', '#b2df8a', '#fb9a99', '#fdbf6f',
          '#cab2d6', '#ffff99', '#1f78b4', '#33a02c'];

    // Sets chart options.
    var options = {
        //width: 600,
        tooltip: {
            isHtml: true
        },
        sankey: {
            iterations: 128,
            node: {
                interactivity: true,
                nodePadding: 15,
                width: 15,
                colors: colors,
                label: {
                    fontSize: 12
                }
            },
            link: {
                colorMode: 'source',
                colors: colors
            }
        }
    };

    // Instantiates and draws our chart, passing in some options.
    var chart = new google.visualization.Sankey(element);
    chart.draw(view, options);
}

function tooltip(from, to, value) {
    return "<div style='background-color:red'>" + value + "</div>";
}

$(function () {
    renderCharts();
});

jQuery.fn.exists = function () { return this.length > 0; }

Highcharts.SparkLine = function (elem, b, c) {

    var hasRenderToArg = typeof elem === 'string' || elem.nodeName,
    options = arguments[hasRenderToArg ? 1 : 0],
    defaultOptions = {

        chart: {
            renderTo: (options.chart && options.chart.renderTo) || this,
            margin: [0, 0, 0, 0],
        },

        tooltip: {
            enabled: false
        },
        legend: {
            enabled: false
        },
        series: [],

    };

    return DefaultChart(elem, b, c, defaultOptions);
};

Highcharts.Combo = function (elem, b, c) {
    
    var hasRenderToArg = typeof elem === 'string' || elem.nodeName,
    options = arguments[hasRenderToArg ? 1 : 0],
    defaultOptions = {

        chart: {
            renderTo: (options.chart && options.chart.renderTo) || this,
        },
        series: [],
        xAxis: {
            plotLines: []
        }
    };



    return DefaultChart(elem, b, c, defaultOptions);
};


function DefaultChart(elem, b, c, defaultOptions) {

    var hasRenderToArg = typeof elem === 'string' || elem.nodeName,
    options = arguments[hasRenderToArg ? 1 : 0],


    options = Highcharts.merge(defaultOptions, options);

    var seriesData = $(elem).data("series");
    var annotations = $(elem).data("annotations");

    var counter = 0;
    $.each(seriesData, function (index, value) {
        var def = options.series[counter++];
        $.extend(true, value, def);
    });

    var plotlines = options.xAxis.plotLines;

    $.each(annotations, function (index, value) {
        var item = {
            color: '#FF0000',
            width: 2,
            zIndex: 5,
            value: value.value,
            label: {
                text: value.text
            }
        };

        plotlines.push(item);
    });

    options.series = seriesData;

    var chart = hasRenderToArg ?
        new Highcharts.Chart(elem, options, c) :
        new Highcharts.Chart(options, b);

    //chart.series[0].data[40].select();

    var pointsToSelect = [];
    $.each(chart.series, function (index, s) {
        var focus = s.options.focus;

        if (focus === "last") {
            var points = s.points;
            var point = points[points.length - 1];
            pointsToSelect.push(point);
        }
    });


    if (pointsToSelect.length > 0) {
        chart.tooltip.refresh(pointsToSelect);
    }
    chart.reflow();

    return chart;
};

//Highcharts.SparkLine = Highcharts.Combo;