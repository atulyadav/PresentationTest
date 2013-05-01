/*

Author: Gautam G Gosavi
Description: Will have code to interact objective-c

*/

var sAppConstant = "CCI";
var sHTTPSep = ":";
var sParamSep = "|";

/*
This func will take your objective c code function and params to it.
e.g : document.location = "myapp:" + "myfunction:" + param1 + ":" + param2;
*/

//function makecall(functionname , params)
//{
//    /* Added to keep local testing working */
//    if (window.navigator.platform == "MacIntel")
//        return

//	if(params != null)
//	{
//		if(params.length > 0)
//		{
//			var sParams = "";
//			//alert(params.length);
//			for(i = 0;i < params.length; i++)
//			{
//				sParams += (params[i]) + sParamSep;
//			}
//			if(sParams.length > 0)
//		    {
//				sParams = sParams.substring(0, sParams.length-1);
//			}
//			//alert(sAppConstant + sHTTPSep + functionname + sParamSep + sParams);
//			window.location =  sAppConstant + sHTTPSep + functionname + sParamSep + sParams;
//		}else
//		{
//			window.location = sAppConstant + sHTTPSep + functionname;
//		}
//	}else
//	{
//	   window.location = sAppConstant + sHTTPSep + functionname;
//	}
//}

function htmlEncode(value) {
    return $('<div/>').text(value).html();
}

function htmlDecode(value) {
    return $('<div/>').html(value).text();
}

function savetodb() {
    var arr = Array();
    arr[0] = document.frmtest.txtname.value;
    arr[1] = document.frmtest.txtdtpicker.value;
    makecall("savedata", arr);
}