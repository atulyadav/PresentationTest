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

var hasClass = function(el, klass) {
	return el.className.indexOf(klass) != -1;
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

var frame, 
    nav, 
    index = 0,
    length,
    currentSlide,
    totalSlides;

function setupPreview() {
    frame = $('preview');
    nav = $('nav');
    currentSlide = $('currentSlide');
    totalSlides = $('totalSlides');
    length = SLIDES.length;
    
    totalSlides.innerHTML = length;
    currentSlide.innerHTML = index + 1;
    
    if (window.location.hash)
        gotoSlide(parseInt(window.location.hash.substr(1)))
    else
        gotoSlide(0);
}

function previous() {
    gotoSlide(index-1)
}

function next() {
    gotoSlide(index+1);
}

function gotoSlide(slideNumber) {
    if (slideNumber > length-1 || slideNumber < 0)
        return;
    index = slideNumber;
    currentSlide.innerHTML = index + 1;
    var url = SLIDES[index] + "?t";
    frame.src = url;
    window.location.hash = "#" + index;
}

window.addEventListener('load', function() {
	
	var body = $$('body')[0];
	document.body = body;	
	setupPreview()
	
}, false);