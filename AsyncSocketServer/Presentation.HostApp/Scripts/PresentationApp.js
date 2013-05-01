var connection = null;
//obj = new obj();
window['userName'] = null;
window['isReconnect'] = false;

var setBlinkChat = null;
var prevPage = 0;
//var currPage = 1;

var nextPage = 2;
var pageIndx = 0;
var fileNames = null;

//CONNECT TO SERVER
function ConnectToServer() {
    var userState = getCookie("UserState");
    if (userState == "1" || userState == "2") {
        WebSocketConnection();
        var timeoutID1 = window.setTimeout(CheckClientStatus, 3000);
    }

    if (window['isReconnect'] == false) {
        GetHtmlFilesNames();
        var timeoutID2 = window.setTimeout(LoadInitialPage, 3000);
    }
    DisableNavBtnAndDropDown();
}

//TO CREATE WEBSOCKET CONNECTION
function WebSocketConnection() {
    //var cookiesValue = getCookie("Guid");
    //obj = splitCookie(cookiesValue);
    var guid = getCookie("Guid");
    var host = "ws://172.16.7.57:9090/" + guid;
    connection = new WebSocket(host);
    connection.onopen = function (evt) { onOpen(evt) };
    connection.onclose = function (evt) { onClose(evt) };
    connection.onmessage = function (evt) { onMessage(evt) };
    connection.onerror = function (evt) { onError(evt) };
}

// When the connection is open, send some data to the server
function onOpen(evt) {
    // alert("ready to send data..");
    UpdateOnlineUserSymbol();
}
// Log errors
function onError(evt) {
    //console.log('WebSocket Error ' + error);
    alert("error = " + evt.data);
}

// Log messages from the server
function onMessage(evt) {
    //console.log('Server: ' + evt.data);
    //alert("Data Came from Server");
    ReflectChange(evt.data);
}
// Log messages after diconnected from the server
function onClose(evt) {
    console.log('Disconnected before: ' + connection.readyState);
    window['isReconnect'] = true;
    if (connection.readyState == 3 || connection.readyState == 1) {
        //ConnectToServer();  //to reconnect
    }
    UpdateOnlineUserSymbol();
    UpdateOnlineUserStatus();
}

//LOADING INITIAL PAGE
function LoadInitialPage() {
    ShowLoadingImage("target2");
    var pName = "../Presentations/" + getCookie("PresenatationName") + "/";
    var url = pName + fileNames[pageIndx];
    GetPageContent(url, "target2");
    SetPageIndex();
    setTimeout(checkImgSrc, 200); //modi for synthroid
}

//Load page by AjaxCall
function GetPageContent(url, elemtId) {
    $.ajax({
        url: url,
        cache: false
    }).done(function (data) {
        findScriptTag(data);
        var body = data.replace(/^[\S\s]*<body[^>]*?>/i, "")
                    .replace(/<\/body[\S\s]*$/i, "");

        var regexpbody = /<body[^>]*?>/i;
        var matchbody = regexpbody.exec(data);

        if (matchbody != null) {
            matchbody = "" + matchbody;
            var clsName = matchbody.match(/class\=\"(.*)\"/);
            if (clsName != null) {
                // alert(clsName[1]);
                $("#target2").removeClass();
                $("#" + elemtId).addClass(clsName[1]);
            }
        }
        $("#" + elemtId).removeClass("loadImg");
        $("#" + elemtId).html(body);
    });
}
//FUNCTION TO GET THE COOKIE
function getCookie(NameOfCookie) {
    if (document.cookie.length > 0) {
        begin = document.cookie.indexOf(NameOfCookie + "=");
        if (begin != -1) {
            begin += NameOfCookie.length + 1;
            end = document.cookie.indexOf(";", begin);
            if (end == -1) end = document.cookie.length;
            //alert("cookie value : "+document.cookie.substring(begin, end));
            return unescape(document.cookie.substring(begin, end));
        }
    }
    return null;
}

////FUNCTION TO SPLIT THE COOKIE
//function splitCookie(cookiesValue) {
//    var obj1 = new Object();
//    var userInfo = cookiesValue.split('&');
//    obj1.guid = userInfo[0].split('=')[1];
//    obj1.isHost = userInfo[1].split('=')[1];
//    return obj1;
//}

//GET THE USER STATUS
function CheckClientStatus() {
    try {
        var JSONObject = { CommandName: "ClientStatus" };
        connection.send(JSON.stringify(JSONObject));
    } catch (ex) { alert(ex); }
}

//SHOW THE USER STATUS
function ShowClientStatus(JSONObject) {
    $('div.user_list > ul li').remove();
    //console.log("len = " + JSONObject.Users.length);
    for (var i = 0; i < JSONObject.Users.length; i++) {
        $('div.user_list > ul').append('<li>' + JSONObject.Users[i].Name + " - " + JSONObject.Users[i].Type + '</li>');
        var guid = getCookie('Guid');
        if (guid == JSONObject.Users[i].Guid)
            window['userName'] = JSONObject.Users[i].Name;
    }
    //$('div.panel > ul').listview('refresh');
}

$(document).ready(function () {
    //Function to sent chat message
    $("#chat_input").bind("keypress", function (event) {
        if (event.which == 13) {
            var message = $('#chat_input').val();
            if (message != "") {
                try {
                    var JSONObject = { CommandName: "Chat", Parameters: [{ UserName: window['userName'], Message: message}] };
                    connection.send(JSON.stringify(JSONObject));
                } catch (ex) { alert(ex); }
                $('#chat_input').val('');
                ShowChatMessage(JSONObject);
            }
        }
    });
});

$(document).on("click", "a.pLink", function (event) {
    var linkType, destLink;
    event.preventDefault();
    destLink = $(this).attr('href');
    target = $(this).attr('target');
    if (destLink != "" || destLink != " ")
        SendLinkAddress(destLink, target);
});

//REDIRECTION TO LINK ADDRESS
function SendLinkAddress(destLink, target) {
    try {
        // alert("link send to server");
        var JSONObject = { CommandName: "Redirect", Parameters: [{ LinkAddress: destLink, Target: target}] };
        connection.send(JSON.stringify(JSONObject));
    } catch (ex) { alert(ex); }
}

function RedirectToLink(JSONObject) {
    var url = JSONObject.Parameters[0].LinkAddress;
    var target = JSONObject.Parameters[0].Target;

    var pName = "../Presentations/" + getCookie("PresenatationName") + "/";
    url = pName + url;

    ShowLoadingImage("target2");
    GetPageContent(url, "target2");
    setTimeout(checkImgSrc, 200); //modi. for synthroid
}

//SWITCH TO DIFFERENT COMMANDS to APPLY
function ReflectChange(data) {
    //alert("data in reflect = " + data);
    var JSONObject = jQuery.parseJSON(data);
    //alert("command name in reflact :  = " + JSONObject.CommandName);
    switch (JSONObject.CommandName) {
        case "Redirect": RedirectToLink(JSONObject);
            break;
        case "ClientStatus": ShowClientStatus(JSONObject);
            break;
        case "PopUp": generatePopUp(JSONObject);
            break;
        case "Chat": ShowChatMessage(JSONObject);
            break;
        case "PageIndex": ShowPageWithIndex(JSONObject);
            break;
        case "RedirectToHome": RedirectToHome();
            break;
        case "ReAnimate": ReAnimate();
            break;

        default: alert("Wrong Command name !!");
    }
}

//SHOW LOADING .gif IMAGE WHILE LOADING DIV CONTENT(3target2)
function ShowLoadingImage(divId) {
    $("#" + divId).empty();
    var img = document.createElement("IMG");
    img.setAttribute("src", "../Content/images/ajax-loader2.gif");
    $("#" + divId).addClass("loadImg");
    document.getElementById(divId).appendChild(img);
}

//FUNCTION TO SEND POPUP MESSAGE(FUNCTION NAME AND PARAMETERS) FOR HUMIRA USING AJAX CALL
function makecall(functionName, params, e) {
    var bId = e.id;
    var shown = false;
    var popover = $("#" + bId).data('popover');
    shown = popover && popover.tip().is(':visible');
    if (typeof (shown) == 'undefined' || shown == null || shown == false) {
        shown = false;
    }
    else {
        shown = true;
    }

    try {
        var JSONObject = { CommandName: "PopUp", Parameters: [{ FunctionName: functionName, Params: params, Id: bId, IsShown: shown}] };
        //connection.send(JSONObject);
        SendChanges(JSON.stringify(JSONObject));
    } catch (ex) { alert(ex); }
    generatePopUp(JSONObject);
}

//FUNCTION TO GENERATE POPUP MESSAGE FOR HUMIRA BY AJAX CALL
function generatePopUp(JSONObject) {
    var params = JSONObject.Parameters[0].Params;
    var functionName = JSONObject.Parameters[0].FunctionName;
    var bId = JSONObject.Parameters[0].Id;
    var isShown = JSONObject.Parameters[0].IsShown;

    $("#" + bId).popover({ trigger: 'manual' });
    //$("#" + bId).popover({ placement: 'bottom' });
    $('.popover').hide();
    if (isShown == true) {
        $("#" + bId).popover('hide');
        return;
    }
    else if (isShown == false) {
        getPopUpContent(functionName, bId, params);
    }
    setTimeout(checkImgSrc, 200);
}

//GET PAGE CONTENT FOR POPUP BY AJAXCALL
function getPopUpContent(functionName, bId, params) {
    $.ajax({
        type: "POST",
        url: functionName,
        data: "{'Params':'" + escape(params) + "'}",
        contentType: "application/json; charset=utf-8",
        dataType: "html",
        success: function (data) {
            $("#" + bId).attr('data-placement', 'top');
            var body = data.replace(/^[\S\s]*<body[^>]*?>/i, "")
                    .replace(/<\/body[\S\s]*$/i, "");

            var regexpbody = /<body[^>]*?>/i;
            var matchbody = regexpbody.exec(data);
            var innerDiv = null;
            if (matchbody != null) {
                matchbody = "" + matchbody;
                var clsName = matchbody.match(/class\=\"(.*)\"/);
                if (clsName != null) {
                    //alert(clsName[1]);
                    innerDiv1 = document.createElement("DIV");
                    $(innerDiv1).addClass(clsName[1]);
                    $(innerDiv1).attr('id', 'innerPopupDiv');
                }
            }

            $(innerDiv1).html(body);
            innerDiv = document.createElement("DIV");
            $(innerDiv).append(innerDiv1);

            $("#" + bId).attr("data-content", $(innerDiv).html()).popover('show');
            if (functionName == "PopupWithHeader") {
                var pw = params.toString().split(',')[params.toString().split(',').length - 6];
                //var ph = params.toString().split(',')[params.toString().split(',').length - 5];
                $(".popover").width(pw);
                //$("#popup").height(ph);
            }
        },
        error: function () {
            alert(data);
        }
    });
}

//FUNCTION TO SHOW THE CHAT MESSAGE IN  A DIV
function ShowChatMessage(JSONObject) {
    var userName = JSONObject.Parameters[0].UserName;
    var message = JSONObject.Parameters[0].Message;
    if (userName == window["userName"])
        userName = "Me";
    $('div.chatters').append('<p class="user1"><strong>' + userName + ' : </strong>' + message + '</p>');
    $('div.chatters').scrollTop($('div.chatters').get(0).scrollHeight);
    if (setBlinkChat == null)
        setBlinkChat = setInterval(function () { highlightChatIcon() }, 2000); //highlight the chat icon
}

//FIRE on SWIPE ENEVT
$(document).ready(function () {
    $(document).on("swipeleft", "#target2", function () {
        gotoNextPage();
    });
    $(document).on("swiperight", "#target2", function () {
        gotoPrevPage()
    });
});

//NAVIGATE PAGE ON CLICK OF NEXT AND PREV BUTTONS
function navPageButton(e) {
    if (e.id == "nextBtn") {
        gotoNextPage();
    }
    else if (e.id == "prevBtn") {
        gotoPrevPage();
    }
}

function gotoNextPage() {
    if (pageIndx < (fileNames.length - 1)) {
        pageIndx++;
        SetPageIndex();
        sendPageIndex(pageIndx);
    }
    EnableNavButton();
}
function gotoPrevPage() {
    if (pageIndx > 0) {
        pageIndx--;
        SetPageIndex();
        sendPageIndex(pageIndx);
    }
    EnableNavButton();
}

//GET ALL HTML FILE NAMES INSIDE FOLDER
function GetHtmlFilesNames() {
    $.ajax({
        type: "POST",
        url: "GetHtmlfileNames",
        data: "{'Params':'hello'}",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (data) {
            fileNames = data;
            InitializeSelectSlide(fileNames);
        },
        error: function () {
            alert("erroe " + data);
        }
    });
}

// SET THE INDEX FOR NEW PAGE
function SetPageIndex() {
    if (pageIndx == -1)
        pageIndx = (fileNames.length - 1);

    pageIndx = (Math.abs(pageIndx)) % (fileNames.length);
    nextPage = pageIndx + 1;
    prevPage = pageIndx - 1;

    if (prevPage == -1)
        prevPage = (fileNames.length - 1);

    nextPage = (Math.abs(nextPage)) % (fileNames.length);
    prevPage = (Math.abs(prevPage)) % (fileNames.length);
}

//SEND THE INDEX OF PAGE TO LOAD
function sendPageIndex(indx) {
    try {
        var JSONObject = { CommandName: "PageIndex", Parameters: [{ PrevPage: prevPage, PageIndx: pageIndx, NextPage: nextPage}] };
        //connection.send(JSON.stringify(JSONObject));
        SendChanges(JSON.stringify(JSONObject));
    } catch (ex) { alert(ex); }
    ShowPageWithIndex(JSONObject);
}

//ON SWIPE SHOW THE RESPECTTIVE PAGE
function ShowPageWithIndex(JSONObject) {
    prevPage = JSONObject.Parameters[0].PrevPage;
    pageIndx = JSONObject.Parameters[0].PageIndx;
    nextPage = JSONObject.Parameters[0].NextPage;

    var pName = "../Presentations/" + getCookie("PresenatationName") + "/";

    ShowLoadingImage("target2");
    var url = pName + fileNames[pageIndx];
    GetPageContent(url, "target2");
    setTimeout(animateOnDivLoad, 300); //run animation
    setTimeout(checkImgSrc, 200); //modi for synthroid
}

//GET THE ALL SCRIPT TAG FROM PAGE AND EXECUTE SCRIPT BLOCK
function findScriptTag(objServerResponse) {
    deleteAnimationVar();
    var responseText = objServerResponse;
    var scripts, scriptsFinder = /<script\b[^>]*>([\s\S]*?)<\/script>/gm;
    while (scripts = scriptsFinder.exec(responseText)) {
        eval.call(window, scripts[1]);
    }
    //    if (typeof callbackOnSuccess != 'undefined') {
    //        callbackOnSuccess.call();
    //    }
}

//CHECK IMAGE SRC FOR VALIDATION
function checkImgSrc() {
    var im = document.getElementsByTagName('IMG');
    var pName = getCookie("PresenatationName");
    for (var i = 0; i < im.length; i++) {
        imgex(im[i].src, i, function (exists, indx) {
            if (exists == false) {
                im[indx].setAttribute('src', pName + "/" + im[indx].getAttribute('src'));
            }
        });
    }
}
function imgex(url, indx, callback) {
    var img = new Image();
    img.onload = function () { callback(true, indx); };
    img.onerror = function () { callback(false, indx); };
    img.src = url;
}

//SEND DATA TO SERVER ONLY USER IS HOST
function SendChanges(JSONObject) {
    var JSONObj = jQuery.parseJSON(JSONObject);
    var isHost = getCookie('IsHost');
    //if (obj.isHost == "True") {
    var userState = getCookie("UserState");
    if (isHost == "True" && userState == "2") {
        //alert(isHost+" = "+userState);
        connection.send(JSONObject);
    }
    else {
        //alert("isHost = " + obj.isHost);
    }
}

//ANIMATE CHAT ICON FOR NEW MESSAGES (IF CHAT BOX IS CLOSED)
function highlightChatIcon() {
    if (!($("footer#slidein").hasClass('active'))) {
        $("#chat").effect("bounce", "slow");
        $("#chat").effect("highlight", { color: "yellow" }, 1500);
    }
}

//ANIMATE CHAT AREA
function slideout() {
    document.getElementById('slidein').classList.toggle('active');

    $("#chat").stop(true, true);
    if (setBlinkChat != null) {
        clearInterval(setBlinkChat);
        setBlinkChat = null;
    }
}

//CHANGE ONLINE USER PIC
function UpdateOnlineUserSymbol() {
    if (connection.readyState == 1) {
        document.getElementById("onlineSymbol").setAttribute("src", "../../Content/images/user_online.png");
        document.getElementById("refreshButton").style.visibility = "hidden";
    }
    else {
        document.getElementById("onlineSymbol").setAttribute("src", "../../Content/images/user.png");
        var userState = getCookie("UserState");
        if (userState == "1" || userState == "2") {
            document.getElementById("refreshButton").style.visibility = "visible";
        }
    }
}
//CLEAR THE ONLINE USER LIST
function UpdateOnlineUserStatus() {
    $('div.user_list > ul li').remove();
    $('div.user_list > ul').append('<li>Disconneted...</li>');
}

function RefreshConnection() {
    if (connection.readyState != 1) {
        ConnectToServer();
        window.setTimeout(checkConnectionStatus, 1500);
    }
    else {
        alert("already connected...");
    }
}
function checkConnectionStatus() {
    if (connection.readyState != 1) {
        alert("Could not able to Connect to Server...");
    }
}

function InitializeSelectSlide(fileNames) {
    var _option, _select, _text;
    _select = document.getElementById("slideSelect");
    for (var i = 0; i < fileNames.length; i++) {
        _option = document.createElement('option');
        _text = fileNames[i].split('/');
        _text = _text[_text.length - 1];
        _option.text = _text;
        _select.add(_option, _select.options[null]);
    }
}
function ChangeSlide(e) {
    // alert(e.selectedIndex);
    if ((e.selectedIndex >= 0) && (e.selectedIndex <= (fileNames.length - 1))) {
        pageIndx = e.selectedIndex;
        SetPageIndex();
        sendPageIndex(pageIndx);
    }
    EnableNavButton();
}
function EnableNavButton() {
    if (pageIndx > 0 && pageIndx < (fileNames.length - 1)) {
        $('#prevBtn').show();
        $('#nextBtn').show();
    }
    else {
        if (pageIndx == 0) {
            $('#prevBtn').hide();
            $('#nextBtn').show();
        }
        else if (pageIndx == (fileNames.length - 1)) {
            $('#nextBtn').hide();
            $('#prevBtn').show();
        }
    }
}
function DisableNavBtnAndDropDown() {
    var userState = getCookie("UserState");
    if (userState == "1") {
        $('#nextBtn').hide();
        $('#prevBtn').hide();
        document.getElementById("slideSelect").disabled = true;
    }
    else if (userState == "3" || userState == "4") { //3=PARTICIPANT OFFLINE 4=PRESENTER OFFLINE
        $('#nextBtn').show();
        $('#prevBtn').show();
        document.getElementById("slideSelect").disabled = false;
        document.getElementById("chat").onclick = "return false;";
    }
}

function RedirectToHome() {
    //alert("RedirectToHome(JSONObject) " + JSONObject);
    console.log("RedirectToHome()");

    $.ajax({
        url: "Logout",
        cache: false
    }).done(function (data) {
        //console.log("RedirectToHome(JSONObject) DONE" + JSONObject+data);
        document.location.href = '/Account/Login/';
    });
}

function SendRedirectToHome() {
    console.log("SendRedirectToHome");
    try {
        var JSONObject = { CommandName: "RedirectToHome" };
        SendChanges(JSON.stringify(JSONObject));
    } catch (ex) { alert(ex); }
}

//RESTART THE ANIMATION
function SendReAnimate(e) {
    try {
        var JSONObject = { CommandName: "ReAnimate" };
        SendChanges(JSON.stringify(JSONObject));
    } catch (ex) { alert(ex); }
    ReAnimate();
}
function ReAnimate() {
    //restartAnimation();
    animateOnDivLoad();
}
//DETECT DEVICE - USER-AGENT
function Detectdevice() {
    var uagent = navigator.userAgent.toLowerCase();
    alert(uagent);
    if (uagent.search("iphone") > -1)
        alert('true');
    else
        alert('false');
}