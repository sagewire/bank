;
(function () {

    jsPlumbToolkit.ready(function () {

        // Instantiate an instance of the toolkit. We provide a `beforeConnect` function, which filters
        // connections so that the only edges we can create are from a 'hello' type to a 'world' type. We also
        // provide a `beforeStartConnect` function, which creates some backing data for the edge that the user will
        // drag. In this case we return an object with the language from the `hello` node; it appears as an overlay
        // on the edge.
        var toolkit = jsPlumbToolkit.newInstance({
            beforeConnect:function(source, target) {
                return (source.data.type === "hello" && target.data.type === "world" && source.data.l === target.data.l );
            },
            beforeStartConnect:function(node, edgeType) {
                return { lang:node.data.l };
            }
        });

        var mainElement = document.querySelector("#jtk-demo-hello"),
            canvasElement = mainElement.querySelector(".jtk-demo-canvas"),
            miniviewElement = mainElement.querySelector(".miniview");


        // 2. render the data
        var renderer = toolkit.render({
            container: canvasElement,
            view: {
                nodes: {
                    hello: {
                        template: "tmplHello"
                    },
                    world: {
                        template: "tmplWorld"
                    }
                },
                ports:{
                    "default": {
                        maxConnections: 1,
                        uniqueEndpoint: true
                    }
                },
                edges:{
                    "default":{
                        events:{
                            click:function(params) {
                                toolkit.removeEdge(params.edge);
                            }
                        },
                        overlays:[
                            [ "Label", { label:"${lang}"}]
                        ]
                    }
                }
            },
            layout: {
                type: "Spring"
            },
            zoomToFit: true,
            jsPlumb: {
                Endpoint: "Blank",
                Connector: "StateMachine",
                PaintStyle: { strokeStyle: "green", lineWidth: 1 },
                Anchor: "Center"
            },
            miniview: {
                container:miniviewElement
            }
        });

        var solved = false,
            _testSolved = function() {
                // filter the data for nodes that are unconnected. If there are no unconnected
                // nodes, everything is connected and we have a winner.
                solved = (toolkit.filter(function(obj) {
                    return obj.objectType == "Node" && obj.getAllEdges().length == 0;
                }).getNodeCount() == 0);

                if(solved) {
                    alert("You win!");
                }
            };

        // listen for edgeAdded/edgeRemoved  events
        renderer.bind("edgeAdded", _testSolved);
        renderer.bind("edgeRemoved", _testSolved);

        var phrases = [
            { h:"Hello", l:"en", id:"1", type:"hello" },
            { h:"World", l:"en", id:"2", type:"world" },
            { h:"Bonjour", l:"fr", type:"hello" },
            { h:"Le Monde", l:"fr", type:"world"  },
            { h:"Hola", l:"es", id:"3", type:"hello" },
            { h:"Mundo", l:"es", id:"4", type:"world"  },
            { h:"Hallå", l:"sv", type:"hello" },
            { h:"Världen", l:"sv", type:"world" },
            { h:"Hallo", l:"de", type:"hello" },
            { h:"Welt", l:"de", type:"world"  },
            { h:"Hej", l:"dk", type:"hello" },
            { h:"Verden", l:"dk", type:"world"  },
            { h:"Ciao", l:"it", type:"hello" },
            { h:"Mondo", l:"it", type:"world" },
            { h:"Hei", l:"fi", type:"hello" },
            { h:"Maailma", l:"fi", type:"world"  },
            { h:"Zdravo", l:"si", type:"hello" },
            { h:"Svet", l:"si", type:"world"  },
            { h:"привет", l:"ru", type:"hello" },
            { h:"мир", l:"ru", type:"world" },
            { h:"здравей", l:"bg", type:"hello" },
            { h:"свят", l:"bg", type:"world" },
            { h:"Sawubona", l:"zu", type:"hello" },
            { h:"Mhlaba", l:"zu", type:"world" }
        ];

        // 3. load phrases and a couple of example edges This data is in the graphml-like
        // JSON syntax that the jsPlumb Toolkit uses as the default.
        toolkit.load({
            data: {
                nodes:phrases,
                edges: [
                    { source: "1", target: "2", data:{ lang:"en" } },
                    { source: "3", target: "4", data:{ lang:"it" } }
                ]
            }
        });

        // on home button tap, zoom content to fit.
        jsPlumb.on(mainElement, "tap", "[reset]", function () {
            toolkit.clearSelection();
            renderer.zoomToFit();
        });
    });
})();