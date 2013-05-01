  function getState(currentstate){
					
					
					 if (!localStorage.stateName){
						localStorage.stateName = currentstate;
						/*document.getElementById("result").innerHTML="State name: " + currentstate;*/
						document.getElementById(currentstate).style.display = "block";
					}
					else if(localStorage.stateName != 'undefined' ){
						localStorage.stateName = localStorage.stateName;
						/*document.getElementById("result").innerHTML="State name: " + localStorage.stateName;*/
						document.getElementById(localStorage.stateName).style.display = "block";
						}
					else{
						
						document.getElementById(localStorage.stateName).innerHTML="Sorry, your browser does not support web storage...";
					}
				}
 
 
 function toggleSpin(state) {
	 							var SN = localStorage.stateName;
                                var toggleBtn =  document.getElementById('toggleBtn');
								
                                var slide = document.getElementById('slide1'+ SN);
								var slide_2 = document.getElementById('slide2' + SN);
								
								 
                                
                                if (state == "on") {
                                    toggleBtn.setAttribute('onclick', 'toggleSpin("off")');
                                    slide.style.display = "none";
									slide_2.style.display = "block";
                                    toggleBtn.style.webkitTransform = "rotate(1080deg)";
									
									/*makecall('OmnitureTracking', Array('Animation','NotProtected','Toggle')); makecall('OmnitureTracking', Array('TabClick','RxProtection','Tab'))  makecall('OmnitureTracking', Array('TabClick','ProtectionDecline','Tab'))"*/
                                }
                                else {
                                    toggleBtn.setAttribute('onclick', 'toggleSpin("on")');
                                    slide.style.display = "block";
									slide_2.style.display = "none";
                                    toggleBtn.style.webkitTransform = "rotate(0deg)";
									/*makecall('OmnitureTracking', Array('Animation','Protected','Toggle'));*/
                                }
                            }
							

  
 
 
 
 function toggleSpin2(nation) {
                               
								var toggleBtn2 = document.getElementById('toggleBtn2');
								 var slide = document.getElementById('slide1a');
								var slide_2 = document.getElementById('slide2b');
                                var slideTitle2 = document.getElementById('slideTitle2');
                                
                                
                                if (nation == "on") {
                                  
									toggleBtn2.setAttribute('onclick', 'toggleSpin2("off")');
                                    slide.style.display = "none";
									slide_2.style.display = "block";
                                    
									toggleBtn2.style.webkitTransform = "rotate(1080deg)";
									/*makecall('OmnitureTracking', Array('Animation','NotProtected','Toggle')); makecall('OmnitureTracking', Array('TabClick','RxProtection','Tab'))  makecall('OmnitureTracking', Array('TabClick','ProtectionDecline','Tab'))"*/
                                }
                                else {
                                   
									toggleBtn2.setAttribute('onclick', 'toggleSpin2("on")');
                                    slide.style.display = "block";
									slide_2.style.display = "none";
                                    toggleBtn2.style.webkitTransform = "rotate(0deg)";
									/*makecall('OmnitureTracking', Array('Animation','Protected','Toggle'));*/
                                }
                            }
							

							
                          