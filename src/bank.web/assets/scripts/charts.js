
$(function () {


    Highcharts.theme = {
        colors: ['#058DC7', '#50B432', '#ED561B', '#DDDF00', '#24CBE5', '#64E572', '#FF9655', '#FFF263', '#6AF9C4'],
        chart: {
            backgroundColor: null,
            borderWidth: 0,
            margin: [0, 0, 0, 0],

            style: {
                overflow: 'visible'
            },
            skipClone: true
        },
        title: {
            text: ''
        },
        credits: {
            enabled: false
        },
        xAxis: {
            lineColor: 'transparent',
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
            gridLineColor: 'transparent',
            endOnTick: false,
            startOnTick: false,
            labels: {
                enabled: false
            },
            title: {
                text: null
            },
            tickPositions: [0]
        },
        legend: {
            enabled: false
        },
        tooltip: {
            shared: true
        },
        plotOptions: {
            pie: {
                dataLabels: {
                    enabled: false
                }
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
                lineColor: "#FF3B3F",
                shadow: false,
                states: {
                    hover: {
                        lineWidth: 0
                    }
                },
                marker: {
                    radius: 0,
                    states: {
                        hover: {
                            fillColor: "#57A8ED",
                            lineColor: "#F6F67A",
                            radius: 20
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

$(function () {
    

    $("[data-chart-type='sankey']").each(function (index, element) {

        google.charts.load('current', { 'packages': ['sankey'] });
        google.charts.setOnLoadCallback(function () {
            drawSankeyChart(element);
        });

    });

    $("[data-chart-type='combo']").each(function (index, element) {
        
        $(this).highcharts('Combo', {

        });
         
    });


    $('[data-chart-type="primary"]').highcharts('Combo', {
        
        plotOptions: {
            areaspline: {
                lineWidth: 0,
                fillColor: {
                    linearGradient: { x1: .2, x2: 0, y1: 0, y2: .75 },

                    stops: [
                        [0, '#2E96EA'],
                        [1, '#30C8CA']
                    ]
                }
            }
        }
    });
    

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

});

jQuery.fn.exists = function () { return this.length > 0; }

Highcharts.Combo = function (elem, b, c) {
    
    var hasRenderToArg = typeof elem === 'string' || elem.nodeName,
    options = arguments[hasRenderToArg ? 1 : 0],
    defaultOptions = {

        chart: {
            renderTo: (options.chart && options.chart.renderTo) || this,
        },
        series: [],

    };
    
    options = Highcharts.merge(defaultOptions, options);

    var seriesData = $(elem).data("series");

    options.series = seriesData;
    

    return hasRenderToArg ?
        new Highcharts.Chart(elem, options, c) :
        new Highcharts.Chart(options, b);
};

Highcharts.SparkLine = Highcharts.Combo;