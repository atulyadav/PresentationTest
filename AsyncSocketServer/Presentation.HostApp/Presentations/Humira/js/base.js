/* EXTERNAL DEVICE PROPERTIES */
var bExternalDevice;
var bExternalDeviceAnimateTo = false;

/* CALL IDs FOR USE ON A PAGE */
var $ = function (id) {
    return document.getElementById(id);
}
/* CALL SELECTORS (CLASSES AND TAGS) FOR USE ON A PAGE */
var $$ = function (selector) {
    return $A(document.querySelectorAll(selector));
}
/* RETURNS AND STRIP EXCESS HTML FROM A CLASS OR ID */
var $A = function (a) {
    return Array.prototype.slice.apply(a, []);
}

var strip = function (string) {
    return string.replace(/^\s+|\s+$/g, '')
}
/* SEARCH TO SEE IF ELEMENT HAS A CLASS */
var hasClass = function (el, klass) {
    return el.className.indexOf(klass) != -1;
}
/* SEARCH FOR A ELEMENT AND ADD A CLASS */
var addClass = function (el, klass) {
    var classes = [klass];
    el.className.split(/\s+/gi).forEach(function (c) {
        if (c != klass)
            classes.push(strip(c));
    });
    el.className = classes.join(' ');
}
/* SEARCH FOR A ELEMENT AND REMOVE A CLASS */
var removeClass = function (el, klass) {
    var classes = [];
    el.className.split(/\s+/gi).forEach(function (c) {
        if (c != klass)
            classes.push(strip(c));
    });
    el.className = classes.join(' ');
}
/* SEARCH FOR A ELEMENT AND ADD CUSTOM CSS STYLES A CLASS */
var addStyles = function (css) {
    var style = document.createElement('style');
    style.setAttribute('type', 'text/css');
    style.innerText = css;
    $$('head')[0].appendChild(style);
}

/* DEFINES DEFAULT PROPERTIES FOR BASE.JS (DO NOT CHANGE) */
var index = 0, delay = 800, cache = {}, _start = false, _loaded = false, frame = 0;

/* DEFINES THE AMOUNT OF FRAMES USED IN BASE.js (SET TO 7 FOR MAX) */
var frameSelector = '.frame-1, .frame-2, .frame-3, .frame-4, .frame-5, .frame-6, .frame-7';

/*
* For testing on Safari for windows Windows ** change 'touchend' to 'click' on
* Windows ** Please remember to flip back from click when submitting to SVN.
*/
var touchEvent = 'click';

/* CALLS THE TOUCH EVENT FOR THE SELECTOR/ID SPECIFIED IN THE ARRAY */
var touch = function () {
    var items = $A(arguments);
    items.__ontouch__ = true;
    return items;
}

/* STAGGERS LIKE ELEMENTS (Like lis) IN DOM ORDER */
var stagger = function () {
    var items = $A(arguments);
    items.__stagger__ = true;
    return items;
}

/* MASK ELEMENT AND SHOW (FOR LINE CHARTS) */
var mask = function () {
    var items = $A(arguments);
    items.__usemask__ = true;
    return items;
}

/* MASK ELEMENT USING THE Y AXIS AND SHOW (FOR LINE CHARTS) */
var maskY = function () {
    var items = $A(arguments);
    items.__usemasky__ = items.__usemask__ = true;
    return items;
}

/* ADDS A PAUSE DELAY BETWEEN ANIMATIONS IN THE ARRAY (set in miliseconds) */
var pause = function (amount) {
    var items = [];
    items.__pause__ = amount;
    return items;
}
/* FLIPS AN ELEMENT FROM HIDDEN USING THE X AXIS */
var flip = function (selector, rotation) {
    var rotation = rotation || "rotateX";
    var css = selector + ".initialize { -webkit-transform: " + rotation
	+ "(90deg); } "
    css += selector + ".animate { -webkit-transform: " + rotation
	+ "(0deg); -webkit-transition-duration:1s; }"
    css += selector + "{ -webkit-perspective: perspective(1000px); }"
    addStyles(css);
    return selector
}
/* FLIPS AN ELEMENT FROM HIDDEN USING THE Y AXIS */
var flipY = function (selector, rotation) {
    var rotation = rotation || "rotateY";
    var css = selector + ".initialize { -webkit-transform: " + rotation
	+ "(90deg); } "
    css += selector + ".animate { -webkit-transform: " + rotation
	+ "(0deg); -webkit-transition-duration:1s; }"
    css += selector + "{ -webkit-perspective: perspective(1000px); }"
    addStyles(css);
    return selector
}
/* FADES IN THE OBJECT ( REMOVES DEFAULT SCALING ) */
var fade = function (selector) {
    var css = selector + ".initialize { -webkit-transform: none; } "
    css += selector + ".animate { -webkit-transform:none; }"
    addStyles(css);
    return selector
}
/* SETS ELEMENT'S OPACITY TO ZERO */
var hide = function (selector) {
    return function () {
        $$(selector).forEach(function (el) {
            el.style.webkitTransition = 'opacity 1s';
            el.style.opacity = 0;
        })
    }
}
/*
* TRIGGERS AN EXTERNAL PAGE FUNCTION - ( MUST BE INCLUDED on the page the array
* is triggered )
*/
var func = function (selector, f) {
    return function () {
        f.apply(null, selector ? $$(selector) : [])
    }
}
/* SEARCHES FOR SELECTOR AND SETS CUSTOM CSS STYLES */
var custom = function (selector, css, before) {
    var before = before || '';
    var styles = selector
	+ ".initialize { -webkit-transform: none; opacity: 1; " + before
	+ " } "
    styles += selector + ".animate { -webkit-transform: none; opacity: 1; "
	+ css + " }";
    addStyles(styles);
    return selector
} /*
* INITIALIZE function: Initializes the elements called in the page, adds a
* class 'initialize' to any DOM Elements specified, for animation. Also,
* triggers hiding of tabbed frames on a page.
*/
var initialize = function () {
    //alert("init");
    index = 0;
    ANIMATION.forEach(function (section, i) {
        if (typeof (section) != 'object')
            section = ANIMATION[i] = [section]
        section.forEach(function (selector) {
            if (selector.apply)
                return;

            $$(selector).forEach(function (el) {
                if (section.__usemask__) {
                    if (!cache[el]) {
                        cache[el] = {
                            'width': el.style.width,
                            'height': el.style.height
                        };
                    }
                    el.style[section.__usemasky__ ? 'height' : 'width'] = '0';
                    //addClass(el, 'animate');            // Commented by Hilario
                    el.style.webkitTransition = 'none';  // Added by Hilario
                    //el.style.width = '0';
                } else {
                    addClass(el, 'initialize');
                    removeClass(el, 'animate');
                }
                el.style.webkitTransitionDuration = 'auto';
            });
        });
    })
    initializeFrames();
}

var initializeFrames = function () {
    var dots = $$('.dot');
    if (!dots.length)
        return;

    dots.forEach(function (dot, i) {
        dot.addEventListener(touchEvent, function (dot) {
            gotoFrame(i + 1);
        }, false);
        dot.addEventListener('webkitTransitionEnd', function () {
            /* Keep the dots from doing shady stuff */
            dot.style.webkitTransition = 'none';
        }, false)
    });

    gotoFrame(1);
}

var gotoFrame = function (num) {
    var i = 1;
    $$('.dot').forEach(function (el) {
        if (hasClass(el, 'dot' + num) || i == num)
            addClass(el, 'active')
        else
            removeClass(el, 'active')
        i++;
    });

    $$(frameSelector).forEach(function (el) {
        if (hasClass(el, 'frame-' + num))
            el.style.display = 'block', addClass(el, 'active');
        else
            el.style.display = 'none', removeClass(el, 'active');
    })
}

var gotoFrameExternal = function (num) {
    var i = 1;
    $$('.dot').forEach(function (el) {
        if (hasClass(el, 'dot' + num) || i == num)
            addClass(el, 'active')
        else
            removeClass(el, 'active')
        i++;
    });

    $$(frameSelector).forEach(function (el) {
        if (hasClass(el, 'frame-' + num))
            el.style.display = 'block';
        else
            el.style.display = 'none';
    })
}

var progress = function () {
    var section = ANIMATION[index];
    /* console.log(ANIMATION, index) */
    var i = 0;
    if (!section)
        return;
    section.forEach(function (selector) {
        if (selector.apply)
            return selector()

        $$(selector).forEach(function (el) {
            if (section.__stagger__) {
                removeClass(el, 'initialize');
                addClass(el, 'animate');
                el.style.webkitTransitionDelay = (0.2 * i) + 's';
            }
            if (section.__usemask__) {
                el.style.webkitTransition = 'all 1s linear'; 	 // Added by Hilario
                // addClass(el, 'animate-with-mask');					 // Commented by Hilario
                // el.style.webkitTransitionDuration = '1s';
                var prop = section.__usemasky__ ? 'height' : 'width'
                el.style[prop] = cache[el][prop]
            }
            if (!section.__usemask__ && !section.__stagger__) {
                addClass(el, 'animate');
                removeClass(el, 'initialize');
            }
            i++;
        })
    });
    index++;
}

var progressTouch = function () {
    if (ANIMATION[index])
        progress()
}
/*
* START ANIMATION: Function called when the pages is loaded into the WebView,
* triggering the framework. See the top comments to trigger start animation
* manually.
*/
var startAnimation = function (anim) {
    //alert("start");
    /*
    * if (!_loaded) { _start = true; return; }
    */

    var del = 0;
    (anim || ANIMATION).forEach(function (section) {
        if (section.__pause__)
            del += section.__pause__

        if (!section.__ontouch__) {
            setTimeout(progress, del);
            del += delay;
        }
    });
}

var expandChart = function (id) {
    removeClass($(id), "hide");
}

var hideChart = function (id) {
    addClass($(id), "hide");
}

var toggleChart = function (el) {
    if (el == 'sustained' && !toggled)
        return

    var el = $(el);
    if (hasClass(el, 'inactive'))
        removeClass(el, 'inactive')
    else
        addClass(el, 'inactive')
    toggled = true
}

function animateTo(dex) {
    index = dex || 0;
    delay = 500;
    initialize();
    startAnimation(ANIMATION.slice(dex, -1));
}
///* ADDS "loaded" CLASS TO <BODY> TO CONFIRM PAGE HAS LOADED BASE.js */
window.addEventListener('load', function () {
    var body = $$('body')[0];
    body.setAttribute('class', 'loaded ' + body.getAttribute('class'));
    document.body = body;

    /* Set their properties */
    if (window.navigator.platform != "MacIntel")
        initialize();
    else
        touchEvent = 'click';

    body.addEventListener(touchEvent, function (e) {
        progressTouch();
    });

    /* FOR TESTING */
    if (window.location.search.length > 1) {
        if (touchEvent == "click")
            initialize()
        setTimeout(startAnimation, 500);
    }

    _loaded = true;
    if (_start) {
        setTimeout(startAnimation, 500);
    }
}, false);

window.addEventListener('unload', function () {
    delete ANIMATION;
})

/* SETS ANIMATION BASED OF iPAD CONNECTED TO EXTERNAL DEVICE */
function checkAnimateTo() {
    animateTo(0);
    if (bExternalDevice) {
        /* makecall('AnimateTo',Array('0')); */
    }
}