
(function () {

    jsPlumbToolkit.ready(function () {


        // get a jsPlumbToolkit instance.
        var toolkit = window.toolkit = jsPlumbToolkit.newInstance();

        var mainElement = document.querySelector("#viz-container");

        if (mainElement == null) {
            return;
        }

        //var data = {
        //    "nodes": [
        //        { "id": "window1", "name": "1", "type": "default" },
        //        { "id": "window2", "name": "2" },
        //        { "id": "window3", "name": "3" },
        //        { "id": "window4", "name": "4" },
        //        { "id": "window5", "name": "5" },
        //        { "id": "window6", "name": "6" },
        //        { "id": "window7", "name": "7" }
        //    ],
        //    "edges": [
        //        { "source": "window1", "target": "window3" },
        //        { "source": "window1", "target": "window4" },
        //        { "source": "window3", "target": "window5" },
        //        { "source": "window5", "target": "window2" },
        //        { "source": "window4", "target": "window6" },
        //        { "source": "window6", "target": "window2" }
        //    ]
        //};

        var canvasElement = mainElement.querySelector(".viz-canvas");
        //var miniviewElement = mainElement.querySelector(".miniview");

        var view = {
            nodes: {
                "Organization": {
                    template: "Organization",
                    events: {
                        mouseover: function (params) {
                            renderer.activateState("highlight", params.el);
                        },
                        mouseout: function (params) {
                            renderer.deactivateState("highlight", params.el);
                        }
                    }
                },
                "locked": {
                    template: "tmplLocked",
                    events: {
                        mouseover: function (params) {
                            renderer.activateState("highlight", params.el);
                        },
                        mouseout: function (params) {
                            renderer.deactivateState("highlight", params.el);
                        }
                    }
                },
                "focus": {
                    template: "tmplFocus",
                    events: {
                        mouseover: function (params) {
                            renderer.activateState("highlight", params.el);
                        },
                        mouseout: function (params) {
                            renderer.deactivateState("highlight", params.el);
                        }
                    }
                }
            },
            edges: {
                "default": {
                    paintStyle: { lineWidth: 2, strokeStyle: '#89bcde' },
                    hoverPaintStyle: { strokeStyle: "orange" },
                    overlays: [
                        ["Arrow", { fillStyle: "#89bcde", width: 10, length: 10, location: 1 }]
                    ],
                    events: {
                        click: function (params) {
                            //renderer.getJsPlumb().startEditing(params.connection);
                            renderer.startEditing(params.edge);
                        }
                    }
                }
            },
            states: {
                "highlight": {
                    "default": {
                        cssClass: "hover-node",
                        paintStyle: { lineWidth: 3, strokeStyle: "orange" },
                        endpointStyle: { fillStyle: "#FF6600" }

                    }
                }
            }
        };


        var options = window.jsoptions = {
            container: canvasElement,
            view: view,
            zoomToFit: true,
            enablePan: false,
            enableWheelZoom: false,
            elementsDraggable: false,
            consumeRightClick: false,
            layout: {
                type: "Spring",
                magnetize: false,
                padding: [60, 60],
                repulsion: 1000,
                absoluteBacked: false
            },
            //layout: {
            //    type: "AbstractHierarchicalLayout",
            //    parameters: {
            //        orientation: "horizontal",
            //        padding: [60, 60]
            //    },
            //    multipleRoots: false
            //},
            mode: "disabled",
            //miniview: {
            //    container: miniviewElement
            //},
            lassoFilter: ".controls, .viz-close, .controls *, .miniview, .miniview *",
            enablePanButtons: false,
            dragOptions: {
                filter: ".delete *, .add *"
            },
            events: {
                canvasClick: function (e) {
                    toolkit.clearSelection();
                },
                modeChanged: function (mode) {
                    jsPlumb.removeClass(jsPlumb.getSelector("[mode]"), "selected-mode");
                    jsPlumb.addClass(jsPlumb.getSelector("[mode='" + mode + "']"), "selected-mode");
                }
            },
            jsPlumb: {
                Anchor: "Continuous",
                Connector: ["StateMachine", { cssClass: "connectorClass", hoverClass: "connectorHoverClass" }],
                Endpoint: "Blank"
            }
        };


        var renderer = window.renderer = toolkit.load({ type: "json", data: data }).render(options);

        // pan mode/select mode
        jsPlumb.on(".controls", "tap", "[mode]", function () {
            renderer.setMode(this.getAttribute("mode"));
        });

        // on home button click, zoom content to fit.
        jsPlumb.on(".controls", "tap", "[reset]", function () {
            toolkit.clearSelection();
            renderer.zoomToFit();
        });


    });

})();
