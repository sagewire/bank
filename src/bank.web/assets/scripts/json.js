$(function () {

    $('[data-send=json]').click(function () {
        
        var source = $(this);
        var name = source.data("name");
        var value = source.data("value");
        var flash = $("#flash");
        var msg = $("#flash .message");
        var topNav = $(".top-nav");
        var originalHeight = topNav.height();

        var jqxhr = $.ajax({
            url: "/data/" + name,
            cache: false,
            data: { id: value }
        })
                      .done(function (data) {

                          console.log("success");
                          var result = JSON.parse(data);

                          flash.addClass("alert-success");
                          flash.animate({ height: "71px", speed: 500 })
                          console.log(result);
                          msg.html(result.html);

                          setTimeout(function () {
                              closeFlash();
                          }, 3000)
                      }) 
                      .fail(function () {
                          console.log("error");
                      })
                      .always(function () {
                          console.log("complete");
                      });

    });


    $("#flash .close").click(function () {
        closeFlash();
    })

    function closeFlash() {
        $("#flash").animate(
            {
                height: "0",
                speed: 500
            },{
                always: function () {
                    var msg = $("#flash .message");
                    msg.html("");
                }
            })

        
    }

})


//var dash = $("#sidenav-dashboard");

//var dest = dash.offset();
//var src = $("#plus").offset();

//console.log(source);
//$("#test").css(src);
//$("#test").show();

//$("#test").animate({
//    top: dest.top,
//    easing: "swing",
//    speed: 5000
//})

//$("#test").animate({
//    top: dest.top,
//    left: dest.left,
//    easing: "swing",
//    speed: 1000
//})