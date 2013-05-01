/* [ BASE.JS ]
* TO USE BASE.js:
*
* On the iPad/Android slide's page <head> add this:
* <script type='text/javascript' charset='utf-8'>
* 	var ANIMATE = [
*
* ]
* </script>
*
* TO CALL ANY BASE.JS VARIABLES ON THE PAGE:
<script type='text/javascript' charset='utf-8'>
var startAnimation = function () {
}
</script>
* FOR EXAMPLE ADDING A CLASS TO AN ID ONLOAD:
var startAnimation = function () {
addClass($("graph-1"), "animate");
};

* For more information read the HTML5 Developer docs on Sharepoint here:
* http://office.intouchsol.com/Development/mobileappdev/Mobil%20App%20Dev%20Document%20Library/HTML5Resources.docx
*
*/

/* EXTERNAL DEVICE PROPERTIES */
var bExternalDevice;
var bExternalDeviceAnimateTo = false;

/* CALL IDs FOR USE ON A PAGE */
var $e = function (id) {
    return document.getElementById(id);
}
/* CALL SELECTORS (CLASSES AND TAGS) FOR USE ON A PAGE */
var $$s = function (selector) {
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
    $$s('head')[0].appendChild(style);
}

/* DEFINES DEFAULT PROPERTIES FOR BASE.JS (DO NOT CHANGE) */
var index = 0, delay = 800, cache = {}, _start = false, _loaded = false, frame = 0;

/* DEFINES THE AMOUNT OF FRAMES USED IN BASE.js (SET TO 7 FOR MAX) */
var frameSelector = '.frame-1, .frame-2, .frame-3, .frame-4, .frame-5, .frame-6, .frame-7, .frame-8';

/*
* For testing on Safari for windows Windows ** change 'touchend' to 'click' on
* Windows ** Please remember to flip back from click when submitting to SVN.
*/
var touchEvent = 'touchend';

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
        $$s(selector).forEach(function (el) {
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
        f.apply(null, selector ? $$s(selector) : [])
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
}
/*
* INITIALIZE function: Initializes the elements called in the page, adds a
* class 'initialize' to any DOM Elements specified, for animation. Also,
* triggers hiding of tabbed frames on a page.
*/
var initialize = function () {
    index = 0;
    ANIMATION.forEach(function (section, i) {
        if (typeof (section) != 'object')
            section = ANIMATION[i] = [section]
        section.forEach(function (selector) {
            if (selector.apply)
                return;

            $$s(selector).forEach(function (el) {
                if (section.__usemask__) {
                    if (!cache[el]) {
                        cache[el] = {
                            'width': el.style.width,
                            'height': el.style.height
                        };
                    }
                    el.style[section.__usemasky__ ? 'height' : 'width'] = '0';
                    addClass(el, 'animate');
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
    var dots = $$s('.dot');
    if (!dots.length)
        return;

    dots.forEach(function (dot, i) {
        dot.addEventListener(touchEvent, function (dot) {
            savetabt(i + 1);
            gotoFrame(i + 1);
            // BOC GGOSAVI
            if (bExternalDevice) {
                var frameno = i + 1;
                //var param  = "gotoFrameExternal,"+frameno;
                makecall('ExternalAnimationIntParam', Array("gotoFrameExternal", "" + frameno));
            }
            // EOC GGOSAVI
        }, false);
        dot.addEventListener('webkitTransitionEnd', function () {
            /* Keep the dots from doing shady stuff */
            dot.style.webkitTransition = 'none';
        }, false)
    });
    var activeFrame = gettabt();
    gotoFrameOnLoad(activeFrame);
}
var gotoFrameOnLoad = function (num) {
    var i = 1;

    $$s('.dot').forEach(function (el) {
        if (hasClass(el, 'dot' + num) || i == num)
            addClass(el, 'active')
        else
            removeClass(el, 'active')
        i++;
    });

    $$s(frameSelector).forEach(function (el) {
        if (hasClass(el, 'frame-' + num))
            el.style.display = 'block', addClass(el, 'active');
        else
            el.style.display = 'none', removeClass(el, 'active');
    })
}

var gotoFrame = function (num) {
    var i = 1;
    $$s('.dot').forEach(function (el) {
        if (hasClass(el, 'dot' + num) || i == num)
            addClass(el, 'active')
        else
            removeClass(el, 'active')
        i++;
    });

    $$s(frameSelector).forEach(function (el) {
        if (hasClass(el, 'frame-' + num))
            el.style.display = 'block', addClass(el, 'active');
        else
            el.style.display = 'none', removeClass(el, 'active');
    })
    gotoFrameInitialize(num);

    setTimeout(gotoFrameAnimate, 500); ;
}

function gotoFrameInitialize(num) {
    try {
        if (ANIMATE_TAB_SETTINGS) {
            ANIMATION = ANIMATE_TAB_SETTINGS[num - 1];

            ANIMATION.forEach(function (section, i) {
                if (typeof (section) != 'object')
                    section = ANIMATION[i] = [section]

                section.forEach(function (selector) {
                    if (selector.apply)
                        return;

                    $$s(selector).forEach(function (el) {
                        if (section.__usemask__) {
                            if (!cache[el]) {
                                cache[el] = {
                                    'width': el.style.width,
                                    'height': el.style.height
                                };
                            }

                            el.style[section.__usemasky__ ? 'height' : 'width'] = '0';
                            addClass(el, 'animate');
                        }
                        else {
                            addClass(el, 'initialize');
                            removeClass(el, 'animate');
                        }

                        el.style.webkitTransitionDuration = 'auto';
                    }); //end $$s(selector).forEach(function(el)
                }); //end section.forEach(function(selector)
            });  //end ANIMATION.forEach(function(section, i)
        }
    }
    catch (e) { return };
}

function gotoFrameAnimate() {
    var del = 500;
    index = 0;
    if (ANIMATE_TAB_SETTINGS) {
        ANIMATION.forEach(function (section) {
            if (section.__pause__)
                del += section.__pause__

            if (!section.__ontouch__) {
                setTimeout(progress, del);
                del += delay;
            }
        });
    }
}

var gotoFrameExternal = function (num) {
    var i = 1;
    $$s('.dot').forEach(function (el) {
        if (hasClass(el, 'dot' + num) || i == num)
            addClass(el, 'active')
        else
            removeClass(el, 'active')
        i++;
    });

    $$s(frameSelector).forEach(function (el) {
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

        $$s(selector).forEach(function (el) {
            if (section.__stagger__) {
                removeClass(el, 'initialize');
                addClass(el, 'animate');
                el.style.webkitTransitionDelay = (0.2 * i) + 's';
            }
            if (section.__usemask__) {
                addClass(el, 'animate-with-mask');
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
    removeClass($e(id), "hide");
}

var hideChart = function (id) {
    addClass($e(id), "hide");
}

var toggleChart = function (el) {
    if (el == 'sustained' && !toggled)
        return

    var el = $e(el);
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
/* ADDS "loaded" CLASS TO <BODY> TO CONFIRM PAGE HAS LOADED BASE.js */
window.addEventListener('load', function () {
    var body = $$s('body')[0];
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
//function Added by Atul---start----
function animateOnDivLoad() {
    //alert("onLoadOfDivContent");
    $("#target2").addClass(" loaded");
    //var divv = $("#target2");
    /* Set their properties */
    if (window.navigator.platform != "MacIntel")
        initialize();
    else
        touchEvent = 'click';

    document.getElementById("target2").addEventListener(touchEvent, function (e) {
        progressTouch();
    });
    //    alert(window.location.search.length);
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
}
function deleteAnimationVar() {
    delete ANIMATION;
}
//function Added by Atul---end----
/* SETS ANIMATION BASED OF iPAD CONNECTED TO EXTERNAL DEVICE */
function checkAnimateTo() {
    animateTo(0);
    if (bExternalDevice) {
        /* makecall('AnimateTo',Array('0')); */
    }
}
function savetabt(value) {
    localStorage.setItem(HTML_NAME + "tabValueStorage", value);
}
function gettabt() {
    try {
        var testvalue = localStorage.getItem(HTML_NAME + "tabValueStorage");
        if (testvalue != null) {
            var value = parseInt(testvalue);

            return value;
        }
        else
            return 1;
    }
    catch (e) {
        return 1;
    }
}
var HTML_NAME = "";
function loadtabanimation() {
    var source = document.location.href.split("/");
    HTML_NAME = source[source.length - 1];

    try {
        var testvalue = localStorage.getItem(HTML_NAME + "tabValueStorage");
        if (testvalue != null) {
            var value = parseInt(testvalue);

            if (ANIMATE_TAB_SETTINGS) {
                ANIMATION = ANIMATE_TAB_SETTINGS[value - 1];
            }
            else if (value > 1) {
                ANIMATION = [];
            }
        }
    }
    catch (e) {
        //alert('no storage');
    }

    return;
}