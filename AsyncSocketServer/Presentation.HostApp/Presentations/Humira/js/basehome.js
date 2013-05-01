
var $ = function(id) {
	return document.getElementById(id);
}

var $$ = function(selector) {
	return $A(document.querySelectorAll(selector));
}

var $A = function(a) {
	return Array.prototype.slice.apply(a, []);
}

var strip = function(string) {
	return string.replace(/^\s+|\s+$/g, '')
}

var addClass = function(el, klass) {
	var classes = [klass];
	el.className.split(/\s+/gi).forEach(function(c) {
		if (c != klass) classes.push(strip(c));
	});
	el.className = classes.join(' ');
}

var removeClass = function(el, klass) {
	var classes = [];
	el.className.split(/\s+/gi).forEach(function(c) {
		if (c != klass) classes.push(strip(c));
	});
	el.className = classes.join(' ');
}

var addStyles = function(css) {
	var style = document.createElement('style');
	style.setAttribute('type', 'text/css');
	style.innerText = css;
	$$('head')[0].appendChild(style);
}

var index = 0,
	delay = 500,
	cache = {};
	
var touch = function() {
	var items = $A(arguments);
	items.__ontouch__ = true;
	return items;
}

var stagger = function() {
	var items = $A(arguments);
	items.__stagger__ = true;
	return items;
}

var mask = function() {
	var items = $A(arguments);
	items.__usemask__ = true;
	return items;
}

var pause = function(amount) {
	var items = [];
	items.__pause__ = amount;
	return items;
}

var flip = function(selector, rotation) {
	var rotation = rotation || "rotateX";
	var css = selector + ".initialize { -webkit-transform: "+ rotation +"(90deg); } "
	css += selector + ".animate { -webkit-transform: "+ rotation +"(0deg); -webkit-transition-duration:1s; }"
	css += selector + "{ -webkit-perspective: perspective(1000px); }"
	addStyles(css);
	return selector
}

var fade = function(selector) {
	var css = selector + ".initialize { -webkit-transform: none; } "
	css += selector + ".animate { -webkit-transform:none; }"
	addStyles(css);
	return selector
}

var hide = function(selector) {
	return function() {
		$$(selector).forEach(function(el) {
			el.style.opacity = 0;
		})
	}
}

var custom = function(selector, css) {
	var styles = selector + ".initialize { -webkit-transform: none; opacity: 1; } "
	styles += selector + ".animate { -webkit-transform: none; opacity: 1; "+ css +" }";
	addStyles(styles);
	return selector
}

var initialize = function() {
	ANIMATION.forEach(function(section) {
		section.forEach(function(selector) {
			if (selector.apply)
				return;
				
			$$(selector).forEach(function(el) {
				if (section.__usemask__) {
					cache[el] = {'width':el.style.width, height:el.style.height};
					el.style.width = '0';
					addClass(el, 'animate');
				} else {
					addClass(el, 'initialize');
				}
				el.style.webkitTransitionDuration = 'auto';
			});
		});
	})
}

var progress = function() {
	var section = ANIMATION[index];
	var i = 0;
	section.forEach(function(selector) {
		
		if (selector.apply)
			return selector()
		
		$$(selector).forEach(function(el) {
			
			if (section.__stagger__) {
				removeClass(el, 'initialize');
				addClass(el, 'animate');
				el.style.webkitTransitionDelay = '0.'+i+'s';
			} else if (section.__usemask__) {
				addClass(el, 'animate');
				el.style.webkitTransitionDuration = '1s';
				el.style.width = cache[el].width
			} else {
				addClass(el, 'animate');
				removeClass(el, 'initialize');
			}
			i++;
		})
	});
	index++;
}

var progressTouch = function() {
	if (ANIMATION[index])
		progress()
}

var startAnimation = function() {
     window.location.reload();
	var del = 0;
	ANIMATION.forEach(function(section) {
		
		if (section.__pause__) 
			del += section.__pause__
		
		if (!section.__ontouch__) {
			setTimeout(progress, del);
			del += delay;
		}
		
	});
   
}


window.addEventListener('load', function() {
	
	var body = $$('body')[0];
	body.setAttribute('class', 'loaded ' + body.getAttribute('class'));
	document.body = body;
	
	// Set their properties
	initialize();
	
	//body.addEventListener('click', function() {
	//	progressTouch();
	//});
	
	body.addEventListener('touchend', function(e) {
		progressTouch();
	});
	
	// FOR TESTING
	//setTimeout(startAnimation, 2000);
	
}, false);

