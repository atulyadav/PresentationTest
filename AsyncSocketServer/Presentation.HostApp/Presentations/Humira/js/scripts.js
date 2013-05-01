
$(document).ready(function(){
	

// slider
var slider = new Swipe(document.getElementById('slider'), {
      callback: function(e, pos) {
        
        var i = bullets.length;
        while (i--) {
          bullets[i].className = ' ';
        }
        bullets[pos].className = 'on';

      }
    });
    if($("#slider em").length > 0)bullets = document.getElementById('position').getElementsByTagName('em');
	
	var $viewportMeta = $('meta[name="viewport"]');
	$('input, select, textarea').bind('focus blur', function(event) {
	$viewportMeta.attr('content', 'width=device-width,initial-scale=1,maximum-scale=' + (event.type == 'blur' ? 10 : 1));
	});
	
	$(".extURL").bind("click", function(e){
		e.preventDefault();
		tempURL = $(this).attr("href");
		var r=confirm("You are currently leaving RA.com and going to a web site\nthat is not sponsored by Abbott.\n\nwould you like to continue?");
		if (r==true){
			window.location.href = tempURL;
		}
	});
	
	$("#slider .arrow").bind("click", function(){
		($(this).hasClass("right"))?direction = 1:direction = -1;
		newPos = slider.getPos() + direction;
		slider.slide(newPos, 500);
		(newPos == 0 || newPos == $("#slides li").length - 1)?$(this).addClass("hide"):$("#slider .arrow").removeClass("hide");
	});
});