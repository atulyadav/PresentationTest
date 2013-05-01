/*****
 Dropdown JS
 Synthroid
 Dev: Chad Adams
 *****/

function getState() {
	// Select State Dropdown.
	var stateValue = document.getElementById('stateDropDown').value.toLowerCase();
	//uppercase for omniture
	var statUpper = document.getElementById('stateDropDown').value;
    makecall('PlainPopupWithParams',Array('state-popups',statUpper,'510','400','0','0','926','489','4','0', stateValue, 'StateDropDown'));	    
    document.getElementById('stateDropDown').selectedIndex = 0;
};

function getState2() {
	// Select State Dropdown.
	var stateValue = document.getElementById('stateDropDown2').value.toLowerCase();
	//for omniture
	var statUpper = document.getElementById('stateDropDown2').value;
	makecall('PlainPopupWithParams',Array('state-popups',statUpper,'510','400','0','0','926','489','4','0', stateValue, 'StateDropDown'));	
};

function popoutState(abbrv)
{
	//For image map links.
	//var statUpper = document.getElementById('stateDropDown').value;
	makecall('PlainPopupWithParams',Array('state-popups',abbrv.toUpperCase() ,'510','400','0','0','926','489','4','0', abbrv, 'ButtonClicked'));	
}