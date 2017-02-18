jsPlumbToolkit.ready(function() {

    var images = ["img/amsterdam.jpg", "img/tramlineparis.jpg", "img/ladefense.jpg"],
        image = images[Math.floor(Math.random() * images.length)];

    var mainElement = document.querySelector("#jtk-demo-image"),
        canvasElement = mainElement.querySelector(".jtk-demo-canvas");

    // get a new Toolkit instance and render to 'canvas' element, supplying URL to background, and indicating we want to clamp the panner to always
    // keep the background partly in view.
    var renderer = jsPlumbToolkit.newInstance().render({
        container:canvasElement,
        background:{
            url:image,
            onBackgroundReady:function() {
                renderer.zoomToBackground();
            }
        },
        clampToBackground:true
    });

    // on home button tap, zoom to fit background.
    jsPlumb.on(mainElement, "tap", "[reset]", function () {
        renderer.zoomToBackground();
    });
});