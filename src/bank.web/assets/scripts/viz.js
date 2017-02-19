$(function () {

    // create a network
    var container = document.getElementById('viz');
    
    if (container != null) {

        var options = {
            nodes: {
                shape: 'dot',
                scaling: {
                    min: 20,
                    max: 90,
                    label: {
                        min: 12,
                        max: 20
                    }
                },
                //widthConstraint: {
                //    maximum: 100
                //},
                //size: 60,
                font: {
                    align: "left"
                },
                borderWidth: 2,
                shadow: true
            },
            interaction: {
                zoomView: false
            },
            layout: {
                improvedLayout: true
            },
            edges: {
                smooth: {
                    enabled: true,
                    type: "dynamic"
                },
                font: {
                    size: 8,
                    color: "#c0c0c0"
                },
                scaling: {
                    label: {
                        drawThreshold: 16
                    }
                },
                arrows: {
                    from: {
                        enabled: true
                    }
                }
            },
            physics: {
                adaptiveTimestep: true,
                barnesHut: {
                    springLength: 200,
                    avoidOverlap: .1,
                    damping: 1
                },
                stabilization: {
                    fit: true,
                    iterations: 1000,
                    updateInterval: 10
                }
            }
        };

        var network = new vis.Network(container, data, options);

        network.on('click', function (params) {

            options.interaction.zoomView = true;
            network.setOptions(options);

        });


        network.on("doubleClick", function (params) {

            console.log(params);

            if (params.nodes.length == 0) {
                return;
            }

            var id = params.nodes[0];
            
            var result = $.grep(data.nodes, function (e) { return e.id == id; });
            
            if (result.length >= 0) {
                var href = result[0].url;
            }

            window.location = href;
        });



        network.once("startStabilizing", function () {

            $("#spinner").show();
        });

        network.on("stabilizationProgress", function (params) {

            var percent = (params.iterations / 300 * 100) % 100;
            $("#spinner .progress-bar").css("width", percent + "%");
            if (params.iterations > params.total - 50) {
                $("#spinner").hide();
            }
        });

        network.on("stabilized", function () {
            $("#spinner").hide();
        });

    }


});