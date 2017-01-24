﻿"use strict";
var page = require('webpage').create(),
    system = require('system'),
    address, output, size, width, height;

if (system.args.length < 3 || system.args.length > 5) {
    console.log('Usage: rasterize.js URL filename [paperwidth*paperheight|paperformat] [zoom]');
    console.log('  paper (pdf output) examples: "5in*7.5in", "10cm*20cm", "A4", "Letter"');
    console.log('  image (png/jpg output) examples: "1920px" entire page, window width 1920px');
    console.log('                                   "800px*600px" window, clipped to 800x600');
    phantom.exit(1);
} else {
    address = system.args[1];
    output = system.args[2];
    width = system.args[3];
    height = system.args[4];

    page.viewportSize = { width: width, height: height };
    page.clipRect = { top: 0, left: 0, width: width, height: height };

    page.open(address, function (status) {
        if (status !== 'success') {
            console.log('Unable to load the address!');
            phantom.exit(1);
        } else {
            window.setTimeout(function () {
                //page.render(output, { format: "png" });

                var base64 = page.renderBase64('PNG');
                console.log(base64);
                phantom.exit();
            }, 200);
        }
    });
}
