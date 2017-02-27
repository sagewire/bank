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
                        min: 20,
                        max: 45
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
                //zoomView: false
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
                    color: "#777777"
                },
                scaling: {
                    label: {
                        drawThreshold: 10
                    }
                },
                arrows: {
                    to: {
                        enabled: true
                    }
                }
            },
            physics: {
                adaptiveTimestep: true,
                barnesHut: {
                    //centralGravity: 0.1,
                    //springLength: 1,
                    //springConstant: .3,
                    //avoidOverlap: .1,
                    //damping: 1,
                    gravitationalConstant: -50000
                },
                stabilization: {
                    fit: true,
                    iterations: 1000,
                    updateInterval: 10
                }
            }
        };


        //var clusterIndex = 0;
        //var clusters = [];

        //var clusterOptions = {
        //    //joinCondition: function (nodeOptions) {
        //    //    if (nodeOptions.amountOfConnections === 1) {
        //    //        console.log(nodeOptions);
        //    //    }

        //    //    return nodeOptions.amountOfConnections === 1;
        //    //},
        //    processProperties: function (clusterOptions, childNodes) {
        //        console.log(childNodes);
        //        clusterIndex = clusterIndex + 1;
        //        var childrenCount = 0;
        //        for (var i = 0; i < childNodes.length; i++) {
        //            childrenCount += childNodes[i].childrenCount || 1;
        //        }
        //        clusterOptions.childrenCount = childrenCount;
        //        clusterOptions.label = "# " + childrenCount + "";
        //        clusterOptions.font = { size: childrenCount * 5 + 30 }
        //        clusterOptions.id = 'cluster:' + clusterIndex;
        //        clusters.push({ id: 'cluster:' + clusterIndex});
        //        return clusterOptions;
        //    },
        //    clusterNodeProperties: {
        //        allowSingleNodeCluster: true,
        //        shape: "square",
        //        color: {
        //            background: "#ff0000"
        //        }
        //    }
        //};

        var network = new vis.Network(container, data, options);
        //network.clusterOutliers(clusterOptions);

        network.on('click', function (params) {

            //options.interaction.zoomView = true;
            //network.setOptions(options);

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


        var timeout = null;
        var lastPosition = null;
        var zoomView = true;

        $(window).scroll(function () {
            //if (!timeout) {
            clearTimeout(timeout);
            //lastPosition = $(window).scrollTop();

            if (zoomView) {
                options.interaction.zoomView = false;
                network.setOptions(options);
                zoomView = false;
            }

            timeout = setTimeout(function () {
                //var currentPosition = $(window).scrollTop();
                console.log('scroll stop');
                
                options.interaction.zoomView = true;
                network.setOptions(options);
                zoomView = true;

                clearTimeout(timeout);
                timeout = null;
            }, 1000);
            //}
        });
    }


});