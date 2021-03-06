﻿
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
        xAxis: [{
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
        }],
        yAxis: [{
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
        }, {
            id: "annotations",
            reversed: true,
            visible: false
        }],
        legend: {
            enabled: true,
            layout: "horizontal",
            itemWidth: 200,
            itemStyle: {
                fontWeight: 'bold',
                width: '180%',
                textOverflow: "ellipsis",
                overflow: "hidden",
                whiteSpace: "nowrap"
            },
            //itemHoverStyle: {
            //    overflow: "auto",
            //    whiteSpace: "normal"

            //},
            useHTML: true
        },
        //tooltip: {
        //    shared: true
        //},
        //tooltip: {

        //    formatter: function () {

        //        var name = this.series.name;
        //        var key = this.key;

        //        if ($.isNumeric(key) || name === key) {
        //            key = "";
        //        }

        //        return '<b>' + name + '</b><br/><i>' + key + "</i><br/>" +
        //            Highcharts.dateFormat('%e-%b-%Y', new Date(this.x)) + '<br/> Value: ' + Highcharts.numberFormat(this.y, 0, "", ",") + ' ';
        //    }
        //},
        plotOptions: {

            pie: {
                dataLabels: {
                    enabled: false
                },
                showInLegend: true
            },
            areaspline: {
                pointPlacement: null,
                marker: {
                    radius: 0
                },
                tooltip: {
                    pointFormat: '<b>{point.y:,.0f}<br/>{point.x:%b %e %Y}'
                }
            },
            column: {
                pointPadding: 0
            },
            line: {

            },
            scatter: {
                marker: {
                    enabled: true,
                    //fillColor: "#cccccc",
                    symbol: "circle",
                    radius: 5
                },
                tooltip: {
                    pointFormat: '<b>{point.name}</b><br/>{point.x:%b %e %Y}'
                }
            },
            series: {
                stacking: "normal",
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
                    //radius: 0,
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
            //plotOptions: {
            //    series: {
            //        stacking: "normal"
            //    }
            //}
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

                formatter: function () {

                    var name = this.series.name;
                    var key = this.key;

                    if ($.isNumeric(key) || name === key) {
                        key = "";
                    }

                    return '<b>' + name + '</b><br/><i>' + key + "</i><br/>" +
                        Highcharts.dateFormat('%e-%b-%Y', new Date(this.x)) + '<br/> Value: ' + Highcharts.numberFormat(this.y, 2, ".", ",") + ' ';
                }
            },
            //tooltip: {
            //    //shared: true,
            //    split: true,
            //    useHTML: true,
            //    padding: 0,
            //    valueDecimals: 2,
            //    borderWidth: 0,
            //    headerFormat: "<table class='table'>",
            //    pointFormat: "<tr><td><b>{point.y}    </b></td><td> {point.x:%b %Y}</td></tr>",
            //    footerFormat: "</table>",
            //    //footerFormat: "<b>${point.y}</b> {point.x:%b %Y}",
            //    formatter: null,
            //    positioner: function () {
            //        return { x: -5, y: -100 };
            //    },
            //},
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


    $("[data-chart-type='primary']").each(function (index, element) {


        //$('[data-chart-type="primary"]').highcharts('Combo', {
        $(this).highcharts("Combo", {
            //chart: {
            //    marginBottom: 85,
            //    spacingBottom: 0
            //},
            //plotOptions: {
            //    areaspline: {
            //        lineWidth: 0,
            //    }
            //},
            //lang: {
            //    thousandsSep: ','
            //},
            //legend: {
            //    enabled: true,
            //    layout: "horizontal"
            //},
            //tooltip: {
            //    shared: true,
            //    //split: true,
            //    borderWidth: 0,
            //    shadow: false,
            //    useHTML: true,
            //    valueDecimals: 0,
            //    headerFormat: "<table class='primary-tooltip table table-sm table-striped'><tr><th colspan='2'>{point.x:%b %e %Y}</th></th>",
            //    pointFormat: "<tr><td style='border-left: 10px solid {point.series.color}'>{point.series.name}</td><td style='text-align: right;'>{point.y}</td></tr>",
            //    footerFormat: "</table>",
            //    formatter: null,
            //    positioner: function () {
            //        return { x: 0, y: 0 };
            //    },
            //},
            xAxis: [{
                //gridLineWidth: 1, 
                tickPositions: null,
                labels: {
                    enabled: true
                }
            }],
            yAxis: [{
                //gridLineWidth: 1, 
                tickPositions: null,
                //labels: {
                //    enabled: true,
                //    align: "left",
                //    formatter: function () {
                //        var ret,
                //            numericSymbols = ['', 'T', 'b', 'M', '', ''],
                //            i = numericSymbols.length;
                //        if (this.value >= 1000) {
                //            while (i-- && ret === undefined) {
                //                multi = Math.pow(1000, i + 1);
                //                if (this.value >= multi && numericSymbols[i] !== null) {

                //                    ret = (this.value / multi * .1).toFixed(1);// + numericSymbols[i];
                //                }
                //            }
                //        }
                //        return (ret ? ret : this.value);
                //    }
                //}
            }],

        });

    });
}

function drawSankeyChart(element) {
    var d = $(element).data("series")

    console.log(d);

    var data = google.visualization.arrayToDataTable(d[0].data);

    var view = new google.visualization.DataView(data);

    var max = 0;//google.visualization.data.max(view.getDistinctValues(2));

    var filteredRows = view.getFilteredRows([{ column: 2, minValue: 1 }]);

    view.setRows(filteredRows);
    console.log('hi.');
    console.log(filteredRows);

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

    var existing = $(elem).data("highchartsChart");
    var id = $(elem).attr("id");


    if (existing > -1) {
        return;
    }

    var hasRenderToArg = typeof elem === 'string' || elem.nodeName,
    options = arguments[hasRenderToArg ? 1 : 0],
    defaultOptions = {

        chart: {
            renderTo: (options.chart && options.chart.renderTo) || this,
        },
        series: [],
 
    };

    var newOptions = Highcharts.merge(defaultOptions, options);

    newOptions.xAxis = Highcharts.merge(defaultOptions.xAxis, options.xAxis);
    newOptions.yAxis = Highcharts.merge(defaultOptions.yAxis, options.yAxis);

    return DefaultChart(elem, b, c, newOptions);
};


function DefaultChart(elem, b, c, defaultOptions) {

    
    var hasRenderToArg = typeof elem === 'string' || elem.nodeName,
    options = arguments[hasRenderToArg ? 1 : 0];


    var newOptions = Highcharts.merge(options, defaultOptions);
    newOptions.xAxis = Highcharts.merge(defaultOptions.xAxis, options.xAxis);
    newOptions.yAxis = Highcharts.merge(defaultOptions.yAxis, options.yAxis);

    options = newOptions;
    
    var seriesData = $(elem).data("series");
    var annotations = $(elem).data("annotations");

    var counter = 0;
    $.each(seriesData, function (index, value) {
        var def = options.series[counter++];
        $.extend(true, value, def);
    });

    options.series = seriesData;


    if (annotations !== undefined && annotations.length > 0) {

        $.each(annotations, function (index, series) {

            var annotationSeries = {
                name: series.Name,
                type: "scatter",
                dashStyle: "Dot",
                lineWidth: 0,
                //color: "#cccccc",
                yAxis: "annotations",
                visible: series.Visible,
                data: []
            }

            $.each(series.Items, function (index, value) {
                var item = {
                    x: value.value,
                    y: 1,

                    name: value.text
                };

                annotationSeries.data.push(item);
            });

            options.series.push(annotationSeries);
            //console.log(annotationSeries);
            //console.log(JSON.stringify(options.series));

        });

    }

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


