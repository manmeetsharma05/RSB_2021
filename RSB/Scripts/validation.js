/// <reference path="jsapi.js" />
var agetooltip = null;
$("#UID").on("keyup", function () {
    
    if ($("#UID").val() == "") {
        if (agetooltip != null) {
            $("#UID").tooltip("disable");
        }
    }
    else {
       
        var age = $("#UID").val();
        var patt = new RegExp("^[0-9]+$");
        if (age.length <= 12 && patt.test(age) == true) {
            if (agetooltip != null) {
               
                $("#UID").tooltip("disable");
                $("#UID").css("border", "1px solid #777777");
            }


        }
        else {
           
            agetooltip = $("#UID").tooltip({
                items: "#UID",
                content: "number only and length must be 12",
                regexp: "^[0-9]+$"
            });
            
            $("#UID").tooltip("open");
            $("#UID").css("border", "1px solid red");
        }
    }
});
var agetool=null;
$("#uidseacr").on("keyup", function () {
    var check = $('input[name=RadioUID]:checked').val();
    //alert(check);
    if (check=="uid") {
        
        if ($("#uidseacr").val() == "") {
            if (agetool != null)
                $("#uidseacr").tooltip("disable");
        }
        else {
            //   var age = parseInt($("#Sanik_Name_eng").val(), 10);
            var agee = $("#uidseacr").val();
            var pat = new RegExp("^[0-9]{12,12}$");
            var pattn = new RegExp("\W+");
            if ((agee.length <= 13) && pat.test(agee) == true && pattn.test(agee) != true) {
                
                if (agetool == null) {
                   
                    $("#uidseacr").tooltip("disable");
                    $("#uidseacr").css("border", "1px solid #777777");
                }


            }
            else {
                agetooltip = $("#uidseacr").tooltip({
                    items: "#uidseacr",
                    content: " Use  numeric and length must be 12",
                    regexp: "^[0-9]{12,12}$"
                });
                
                $("#uidseacr").tooltip("open");
                $("#uidseacr").css("border", "1px solid red");
            }
        }
    }
    else if (check == "cid")
    {
                
        if ($("#uidseacr").val() == "") {
            if (agetool != null)
                $("#uidseacr").tooltip("disable");
        }
        else {
            //   var age = parseInt($("#Sanik_Name_eng").val(), 10);
            var agee = $("#uidseacr").val();
            var pat = new RegExp("^[A-Z0-9]{12}$");
            var pattn = new RegExp("\W+");
            if ((agee.length <= 12)  && pat.test(agee) == true&& pattn.test(agee)!=true) {
                if (agetool != null) {
                                
                    $("#uidseacr").tooltip("disable");
                    $("#uidseacr").css("border", "1px solid #777777");
                }


            }
            else {
                agetooltip = $("#uidseacr").tooltip({
                    items: "#uidseacr",
                    content: " Use aphanumeric Or numeric and length must be 12",
                    regexp: "^[A-Z0-9]{12}$"
                });
                            
                $("#uidseacr").tooltip("open");
                $("#uidseacr").css("border", "1px solid red");
            }
        }
                
       
    }
    else if (check == "mobileno")
        
    {
        var mobiletool = null;
       if ($("#uidseacr").val() == "") {
                       if (mobiletool != null)
                           $("#uidseacr").tooltip("disable");
                   }
                   else {

                       var mobile = $("#uidseacr").val();
                       var patmb = new RegExp("^[0-9]{10,10}$");
                       var pattn = new RegExp("\W+");
                       if (mobile.length < 11 && patmb.test(mobile) == true&& pattn.test(mobile)!=true) {
                           if (mobiletool != null) {
                               
                               $("#uidseacr").tooltip("disable");
                               $("#uidseacr").css("border", "1px solid #777777");
                           }


                       }
                       else {
                           //alert("wrong");
                           mobiletool = $("#uidseacr").tooltip({
                               items: "#uidseacr",
                               content: "number only and length must be 10",
                               regexp: "^[0-9]{10,10}$"
                           });
                          
                           $("#uidseacr").tooltip("open");
                           $("#uidseacr").css("border", "1px solid red");
                       }
                   }
   }
   else if (check == "armyno") {
       if ($("#uidseacr").val() == "") {
           if (agetool != null)
               $("#uidseacr").tooltip("disable");
       }
       else {
           //   var age = parseInt($("#Sanik_Name_eng").val(), 10);
           var agee = $("#uidseacr").val();
           var pat = new RegExp("^[A-Z0-9]{12}$");
           var pattn = new RegExp("\W+");
           if ((agee.length <= 13) || (agee.length <= 10) && pat.test(agee) == true && pattn.test(agee) != true) {
               if (agetool != null) {
                   
                   $("#uidseacr").tooltip("disable");
                   $("#uidseacr").css("border", "1px solid #777777");
               }


           }
           else {
               agetooltip = $("#uidseacr").tooltip({
                   items: "#uidseacr",
                   content: " Use aphanumeric Or numeric and length must be 12",
                   regexp: "^[A-Z0-9]{12}$"
               });
              
               $("#uidseacr").tooltip("open");
               $("#uidseacr").css("border", "1px solid red");
           }
       }
   }
  else 
   {
       if ($("#uidseacr").val() == "") {
                       if (agetool != null)
                           $("#uidseacr").tooltip("disable");
                   }
                   else {
                     
                       var agee = $("#uidseacr").val();
                       var pat = new RegExp("^[A-Z0-9]{12}$");
                       var pattn = new RegExp("\W+");
                       if ((agee.length <= 13) || (agee.length <= 10) && pat.test(agee) == true&& pattn.test(agee)!=true) {
                           if (agetool != null) {
                              
                               $("#uidseacr").tooltip("disable");
                               $("#uidseacr").css("border", "1px solid #777777");
                           }


                       }
                       else {
                           agetooltip = $("#uidseacr").tooltip({
                               items: "#uidseacr",
                               content: " Use aphanumeric Or numeric and length must be 12",
                               regexp: "^[A-Z0-9]{12}$"
                           });
                           
                           $("#uidseacr").tooltip("open");
                           $("#uidseacr").css("border", "1px solid red");
                       }
                   }
   }
   
});



var mobiletool = null;

$("#mobileno").on("keyup", function () {

    if ($("#mobileno").val() == "") {
        if (mobiletool != null)
            $("#mobileno").tooltip("disable");
    }
    else {
        
        var mobile = $("#mobileno").val();
        var patmb = new RegExp("^[0-9]{10,10}$");
        if (mobile.length < 11 && patmb.test(mobile) == true) {
            if (mobiletool != null) {
               
                $("#mobileno").tooltip("disable");
                $("#mobileno").css("border", "1px solid #777777");
            }


        }
        else {
            //alert("wrong");
            mobiletool = $("#mobileno").tooltip({
                items: "#mobileno",
                content: "number only and length must be 10",
                regexp: "^[0-9]{10,10}$"
            });
            
            $("#mobileno").tooltip("open");
            $("#mobileno").css("border", "1px solid red");
        }
    }
});

var landtool = null;

$("#landline").on("keyup", function () {

    if ($("#landline").val() == "") {
        if (landtool != null)
            $("#landline").tooltip("disable");
    } 
    else {

        var landline = $("#landline").val();
        var patld = new RegExp("^[0-9]{11,11}$");
        if (landline.length < 12 && patld.test(landline) == true) {
            if (landtool != null) {

                $("#landline").tooltip("disable");
                $("#landline").css("border", "1px solid #777777");
            }


        }
        else {
            //alert("wrong");
            landtool = $("#landline").tooltip({
                items: "#landline",
                content: "number only and length must be 11",
                regexp: "^[0-9]{11,11}$"
            });
           
            $("#landline").tooltip("open");
            $("#landline").css("border", "1px solid red");
        }
    }
});


var emailtool = null;

$("#emialid").on("keyup", function () {

    if ($("#emialid").val() == "") {
        if (emailtool != null)
            $("#emialid").tooltip("disable");
    }
    else {

        var email = $("#emialid").val();
        var patem = new RegExp(/^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,3})+$/);
        if (patem.test(email) == true) {
            if (emailtool != null) {
               
                $("#emialid").tooltip("disable");
                $("#emialid").css("border", "1px solid #777777");
            }


        }
        else {
            emailtool = $("#emialid").tooltip({
                items: "#emialid",
                content: "ex : abc@gmail.com",
                regexp: /^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,3})+$/
            });
            
            $("#emialid").tooltip("open");
            $("#emialid").css("border", "1px solid red");
        }
    }
});
var nametool = null;


$("#Sanik_Name_eng").on("keyup", function () {
   
    
    if ($("#Sanik_Name_eng").val() == "") {
        if (nametool != null )
            $("#Sanik_Name_eng").tooltip("disable");
    }
    else {

        var name = $("#Sanik_Name_eng").val();
        
        var patemm = new RegExp("^[A-Za-z ]{1,99}$");
        if (patemm.test(name) == true ) {
            if (nametool != null ) {
               
                $("#Sanik_Name_eng").tooltip("disable");
                $("#Sanik_Name_eng").css("border", "1px solid #777777");
            }


        }
        else {
            
            nametool = $("#Sanik_Name_eng").tooltip({
                items: "#Sanik_Name_eng",
                content: "enter valid name special characters and numbers are not allowed",
                regexp: "^[A-Za-z ]{1,99}$"
            });
           
            $("#Sanik_Name_eng").tooltip("open");
            $("#Sanik_Name_eng").css("border", "1px solid red");
        }
    }
});
var ddopart = null;


$("#Recorded_in_DO_Part2text").on("keyup", function () {


    if ($("#Recorded_in_DO_Part2text").val() == "") {
        if (ddopart != null)
            $("#Recorded_in_DO_Part2text").tooltip("disable");
    }
    else {

        var name = $("#Recorded_in_DO_Part2text").val();

        var patemm = new RegExp("^[A-Za-z ]{1,99}$");
        if (patemm.test(name) == true) {
            if (ddopart != null) {

                $("#Recorded_in_DO_Part2text").tooltip("disable");
                $("#Recorded_in_DO_Part2text").css("border", "1px solid #777777");
            }


        }
        else {

            ddopart = $("#Recorded_in_DO_Part2text").tooltip({
                items: "#Recorded_in_DO_Part2text",
                content: "enter valid name special characters and numbers are not allowed",
                regexp: "^[A-Za-z ]{1,99}$"
            });

            $("#Recorded_in_DO_Part2text").tooltip("open");
            $("#Recorded_in_DO_Part2text").css("border", "1px solid red");
        }
    }
});


var fthrnametool = null;

$("#Father_Name_eng").on("keyup", function () {

    if ($("#Father_Name_eng").val() == "") {
        if (fthrnametool != null)
            $("#Father_Name_eng").tooltip("disable");
    }
    else {

        var name = $("#Father_Name_eng").val();
        var regpattrn = new RegExp("^[A-Za-z ]{1,99}$");
        if (regpattrn.test(name) == true) {
            if (fthrnametool != null) {
                
                $("#Father_Name_eng").tooltip("disable");
                $("#Father_Name_eng").css("border", "1px solid #777777");
            }


        }
        else {
            fthrnametool = $("#Father_Name_eng").tooltip({
                items: "#Father_Name_eng",
                content: "enter valid name special characters and numbers are not allowed",
                regexp: "^[A-Za-z ]{1,99}$"
            });
            
            $("#Father_Name_eng").tooltip("open");
            $("#Father_Name_eng").css("border", "1px solid red");
        }
    }
});


var mthrnametool = null;

$("#Mother_Name_eng").on("keyup", function () {

    if ($("#Mother_Name_eng").val() == "") {
        if (mthrnametool != null)
            $("#Mother_Name_eng").tooltip("disable");
    }
    else {

        var mthrname = $("#Mother_Name_eng").val();
        var regpatt = new RegExp("^[A-Za-z ]{1,99}$");
        if (regpatt.test(mthrname) == true) {
            if (mthrnametool != null) {
                
                $("#Mother_Name_eng").tooltip("disable");
                $("#Father_Name_eng").css("border", "1px solid red");
            }


        }
        else {
            mthrnametool = $("#Mother_Name_eng").tooltip({
                items: "#Mother_Name_eng",
                content: "enter valid name special characters  and numbers are not allowed",
                regexp: "^[A-Za-z ]{1,99}$"
            });
            
            $("#Mother_Name_eng").tooltip("open");
            $("#Mother_Name_eng").css("border", "1px solid red");
        }
    }
});

var spnametool = null;

$("#spousename").on("keyup", function () {

    if ($("#spousename").val() == "") {
        if (spnametool != null)
            $("#spousename").tooltip("disable");
    }
    else {

        var spname = $("#spousename").val();
        var regpat = new RegExp("^[A-Za-z ]{1,99}$");
        if (regpat.test(spname) == true) {
            if (spnametool != null) {
               
                $("#spousename").tooltip("disable");
                $("#spousename").css("border", "1px solid #777777");
            }


        }
        else {
            spnametool = $("#spousename").tooltip({
                items: "#spousename",
                content: "enter valid name special characters  and numbers are not allowed",
                regexp: "^[A-Za-z ]{1,99}$"
            });
            
            $("#spousename").tooltip("open");
            $("#spousename").css("border", "1px solid red");
        }
    }
});


var citzntool = null;
$("#Citizen_ID").on("keyup", function () {

    if ($("#Citizen_ID").val() == "") {
        if (citzntool != null)
            $("#Citizen_ID").tooltip("disable");
    }
    else {
        //   var age = parseInt($("#Sanik_Name_eng").val(), 10);
        var ctzn = $("#Citizen_ID").val();
        var patrn = new RegExp("^[A-Z0-9]{12}$");
        if ((ctzn.length < 13) && patrn.test(ctzn) == true) {
            if (citzntool != null) {
               
                $("#Citizen_ID").tooltip("disable");
                $("#Citizen_ID").css("border", "1px solid #777777");
            }


        }
        else {
            citzntool = $("#Citizen_ID").tooltip({
                items: "#Citizen_ID",
                content: " Use aphanumeric Or numeric and length must be 12",
                regexp: "^[A-Z0-9]{12}$"
            });
            
            $("#Citizen_ID").tooltip("open");
            $("#Citizen_ID").css("border", "1px solid red");
        }
    }
});


var agetooltip1 = null;
$("#cid1").on("keyup", function () {

    if ($("#cid1").val() == "") {
        if (agetooltip1 != null) {
            $("#cid1").tooltip("disable");
        }
    }
    else {

        var age = $("#cid1").val();
        var patt = new RegExp("^[0-9]+$");
        if (age.length <= 12 && patt.test(age) == true) {
            if (agetooltip1 != null) {

                $("#cid1").tooltip("disable");
                $("#cid1").css("border", "1px solid #777777");
            }


        }
        else {

            agetooltip1 = $("#cid1").tooltip({
                items: "#cid1",
                content: "number only and length must be 12",
                regexp: "^[0-9]+$"
            });

            $("#cid1").tooltip("open");
            $("#cid1").css("border", "1px solid red");
        }
    }
});


var renttool = null;
$("#Amount_OF_Rent").on("keyup", function () {

    if ($("#Amount_OF_Rent").val() == "") {
        
        if (renttool != null)
            $("#Amount_OF_Rent").tooltip("disable");
    }
    else {
        //   var age = parseInt($("#Sanik_Name_eng").val(), 10);
        var rnt = $("#Amount_OF_Rent").val();
        var patrnt = new RegExp("^[0-9]+\.?[0-9]*$");
        if (patrnt.test(rnt) == true) {
            if (renttool != null) {
               
                $("#Amount_OF_Rent").tooltip("disable");
                $("#Amount_OF_Rent").css("border", "1px solid #777777");
            }


        }
        else {
            renttool = $("#Amount_OF_Rent").tooltip({
                items: "#Amount_OF_Rent",
                content: " Not valid Amount",
                regexp: "^[0-9]+\.?[0-9]*$"
            });
            
            $("#Amount_OF_Rent").tooltip("open");
            $("#Amount_OF_Rent").css("border", "1px solid red");
        }
    }
});

var annltool = null;
$("#Annual_income").on("keyup", function () {

    if ($("#Annual_income").val() == "") {
        
        if (annltool != null)
            $("#Annual_income").tooltip("disable");
    }
    else {
        //   var age = parseInt($("#Sanik_Name_eng").val(), 10);
        var annl = $("#Annual_income").val();
        var patanl = new RegExp("^[0-9]+\.?[0-9]*$");
        if (patanl.test(annl) == true) {
            if (annltool != null) {
                
                $("#Annual_income").tooltip("disable");
                $("#Annual_income").css("border", "1px solid #777777");
            }


        }
        else {
            annltool = $("#Annual_income").tooltip({
                items: "#Annual_income",
                content: " Not valid Amount",
                regexp: "^[0-9]+\.?[0-9]*$"
            });
           
            $("#Annual_income").tooltip("open");
            $("#Annual_income").css("border", "1px solid red");
        }
    }
});

var budgttool = null;
$("#Annual_Budget_for_Maintenance").on("keyup", function () {

    if ($("#Annual_Budget_for_Maintenance").val() == "") {
        
        if (budgttool != null)
            $("#Annual_Budget_for_Maintenance").tooltip("disable");
    }
    else {
        //   var age = parseInt($("#Sanik_Name_eng").val(), 10);
        var budget = $("#Annual_Budget_for_Maintenance").val();
        var patbdgt = new RegExp("^[0-9]+\.?[0-9]*$");
        if (patbdgt.test(budget) == true) {
            if (budgttool != null) {
               
                $("#Annual_Budget_for_Maintenance").tooltip("disable");
                $("#Annual_Budget_for_Maintenance").css("border", "1px solid #777777");
            }


        }
        else {
            budgttool = $("#Annual_Budget_for_Maintenance").tooltip({
                items: "#Annual_Budget_for_Maintenance",
                content: " Not valid Amount",
                regexp: "^[0-9]+\.?[0-9]*$"
            });
           
            $("#Annual_Budget_for_Maintenance").tooltip("open");
            $("#Annual_Budget_for_Maintenance").css("border", "1px solid red");
        }
    }
});

//second tab validation

$(document).keydown(function (event) {
    if (event.ctrlKey && event.keyCode == 86) {
        var citztool = null;
        if ($("#cid1").val() == "") {
            if (citztool == null)
                $("#cid1").tooltip("disable");
        }
        else {

            var ctz = $("#cid1").val();
            var pattrn = new RegExp("^[A-Z0-9]{12}$");
            if ((ctz.length < 13) && pattrn.test(ctz) == true) {
                if (citzntool != null) {

                    $("#cid1").tooltip("disable");
                    $("#cid1").css("border", "1px solid #777777");

                }


            }
            else {
                citztool = $("#cid1").tooltip({
                    items: "#cid1",
                    content: " Use aphanumeric Or numeric and length must be 12",
                    regexp: "^[A-Z0-9]{12}$"
                });

                $("#cid1").tooltip("open");
                $("#cid1").css("border", "1px solid red");
            }
        }

    }
});



var citztool = null;
//$("#cid1").on("keyup", function () {

//    if ($("#cid1").val() == "") {
//        if (citztool == null)
//            $("#cid1").tooltip("disable");
//    }
//    else {
//        //   var age = parseInt($("#Sanik_Name_eng").val(), 10);
//        var ctz = $("#cid1").val();
//        var pattrn = new RegExp("^[A-Z0-9]{12}$");
//        if ((ctz.length < 13) && pattrn.test(ctz) == true) {
//            if (citzntool == null) {
               
//                $("#cid1").tooltip("disable");
//                $("#cid1").css("border", "1px solid #777777");
//            }


//        }
//        else {
//            citztool = $("#cid1").tooltip({
//                items: "#cid1",
//                content: " Use aphanumeric Or numeric and length must be 13",
//                regexp: "^[A-Z0-9]{12}$"
//            });
            
//            $("#cid1").tooltip("open");
//            $("#cid1").css("border", "1px solid red");
//        }
//    }
//});
   
var dnametool = null;

$("#Dependent_Name").on("keyup", function ()
{

    if ($("#Dependent_Name").val() == "")
    {
        if (dnametool != null)
            $("#Dependent_Name").tooltip("disable");
    }
    else
    {

        var dname = $("#Dependent_Name").val();
        var patem = new RegExp("^[A-Za-z ]{1,99}$");
        if (patem.test(dname) == true)
        {
            if (dnametool != null)
            {
                
                $("#Dependent_Name").tooltip("disable");
                $("#Dependent_Name").css("border", "1px solid #777777");
            }


        }
        else
        {
            dnametool = $("#Dependent_Name").tooltip(
                {
                items: "#Dependent_Name",
                content: "enter valid name special characters and numbers are not allowed",
                regexp: "^[A-Za-z ]{1,99}$"
            });
            
            $("#Dependent_Name").tooltip("open");
            $("#Dependent_Name").css("border", "1px solid red");
        }
    }
});

//third tab validation

   
var casetooltip = null;
$("#Case_Year").on("keyup", function ()
{

    if ($("#Case_Year").val() == "")
    {
        if (casetooltip != null)
            $("#Case_Year").tooltip("disable");
    }
    else
    {

        var caseyr = $("#Case_Year").val();
        var patten = new RegExp("^[0-9]+$");
        if (caseyr.length !=4  && patten.test(caseyr) == true)
        {
            
            if (casetooltip != null)
            {
                
                $("#Case_Year").tooltip("disable");
                $("#Case_Year").css("border", "1px solid #777777");
            }


        }
        else
        {

            casetooltip = $("#Case_Year").tooltip(
                {
                items: "#Case_Year",
                content: "number only ",
                regexp: "^[0-9]+$"
            });
           
            $("#Case_Year").tooltip("open");
            $("#Case_Year").css("border", "1px solid red");
        }
    }
});


var crtnametool = null;

$("#Court_Name").on("keyup", function () {

    if ($("#Court_Name").val() == "")
    {
        if (crtnametool != null)
            $("#Court_Name").tooltip("disable");
    }
    else {

        var cname = $("#Court_Name").val();
        var regpattr = new RegExp("^[A-Za-z ]{1,99}$");
        if (regpattr.test(cname) == true)
        {
            if (crtnametool != null)
            {
                
                $("#Court_Name").tooltip("disable");
                $("#FCourt_Name").css("border", "1px solid #777777");
            }


        }
        else
        {
            crtnametool = $("#Court_Name").tooltip(
                {
                items: "#Court_Name",
                content: "enter valid name",
                regexp: "^[A-Za-z ]{1,99}$"
            });
            
            $("#Court_Name").tooltip("open");
            $("#Court_Name").css("border", "1px solid red");
        }
    }
});

//Loan page
var LoanAmounttool = null;

$("#Loan_Amount").on("keyup", function () {

    if ($("#Loan_Amount").val() == "") {
        if (LoanAmounttool != null)
            $("#Loan_Amount").tooltip("disable");
    }
    else {

        var mobile = $("#Loan_Amount").val();
        var patmb = new RegExp("^[0-9]+\.?[0-9]*$");
        var patem = new RegExp(/^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,3})+$/);
        var patemm = new RegExp("^[A-Za-z\s]{1,99}$");
        if (  patmb.test(mobile) == true && patem.test(mobile)!=true && patemm.test(mobile)!=true) {
            if (LoanAmounttool != null) {
                
                $("#Loan_Amount").tooltip("disable");
                $("#Loan_Amount").css("border", "1px solid #777777");
            }


        }
        else {
            //alert("wrong");
            LoanAmounttool = $("#Loan_Amount").tooltip({
                items: "#Loan_Amount",
                content: "number only ",
                regexp: "^[0-9]+\.?[0-9]*$"
            });
           
            $("#Loan_Amount").tooltip("open");
            $("#Loan_Amount").css("border", "1px solid red");
        }
    }
});



var OutstandingAmounttool = null;

$("#Outstanding_Amount").on("keyup", function () {

    if ($("#Outstanding_Amount").val() == "") {
        if (OutstandingAmounttool != null)
            $("#Outstanding_Amount").tooltip("disable");
    }
    else {

        var mobile = $("#Outstanding_Amount").val();
        var patmb = new RegExp("^[0-9]+\.?[0-9]*$");
        var patem = new RegExp(/^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,3})+$/);
        var patemm = new RegExp("^[A-Za-z\s]{1,99}$");
        if (patmb.test(mobile) == true && patem.test(mobile) != true && patemm.test(mobile) != true) {
            if (OutstandingAmounttool != null) {
                
                $("#Outstanding_Amount").tooltip("disable");
                $("#Outstanding_Amount").css("border", "1px solid #777777");
            }


        }
        else {
            //alert("wrong");
            OutstandingAmounttool = $("#Outstanding_Amount").tooltip({
                items: "#Outstanding_Amount",
                content: "number only ",
                regexp: "^[0-9]+\.?[0-9]*$"
            });
            
            $("#Outstanding_Amount").tooltip("open");
            $("#Outstanding_Amount").css("border", "1px solid red");
        }
    }
});
var banktooltip = null;
$("#Bank_Acc_no").on("keyup", function () {

    if ($("#Bank_Acc_no").val() == "") {
        if (banktooltip != null)
            $("#Bank_Acc_no").tooltip("disable");
    }
    else {

        var bnk = $("#Bank_Acc_no").val();
        var patten = new RegExp("^[0-9]+$");
        if ( patten.test(bnk) == true) {

            if (banktooltip != null) {

                $("#Bank_Acc_no").tooltip("disable");
                $("#Bank_Acc_no").css("border", "1px solid #777777");
            }


        }
        else {

            banktooltip = $("#Bank_Acc_no").tooltip(
                {
                    items: "#Bank_Acc_no",
                    content: "number only ",
                    regexp: "^[0-9]+$"
                });

            $("#Bank_Acc_no").tooltip("open");
            $("#Bank_Acc_no").css("border", "1px solid red");
        }
    }
});

var ifsctool = null;
$("#Bank_IFSC").on("keyup", function () {

    if ($("#Bank_IFSC").val() == "") {
        if (ifsctool != null)
            $("#Bank_IFSC").tooltip("disable");
    }
    else {
        //   var age = parseInt($("#Sanik_Name_eng").val(), 10);
        var ifsc = $("#Bank_IFSC").val();
        var patrn = new RegExp("^[A-Z0-9]{12}$");
        if ((ctzn.length < 11) && patrn.test(ifsc) == true) {
            if (ifsctool != null) {

                $("#Bank_IFSC").tooltip("disable");
                $("#Bank_IFSC").css("border", "1px solid #777777");
            }


        }
        else {
            ifsctool = $("#Bank_IFSC").tooltip({
                items: "#Bank_IFSC",
                content: " Use aphanumeric Or numeric and length must be 11",
                regexp: "^[A-Z0-9]{12}$"
            });

            $("#Bank_IFSC").tooltip("open");
            $("#Bank_IFSC").css("border", "1px solid red");
        }
    }
});

var lvldecsn = null;
$("#Level_of_decision").on("keyup", function () {

    if ($("#Level_of_decision").val() == "") {
        if (lvldecsn != null)
            $("#Level_of_decision").tooltip("disable");
    }
    else {
        //   var age = parseInt($("#Sanik_Name_eng").val(), 10);
        var ctzn = $("#Level_of_decision").val();
        var patrn = new RegExp("^[A-Za-z0-9, ]$");
        if (patrn.test(ctzn) == true) {
            if (lvldecsn != null) {

                $("#Level_of_decision").tooltip("disable");
                $("#Level_of_decision").css("border", "1px solid #777777");
            }


        }
        else {
            lvldecsn = $("#Level_of_decision").tooltip({
                items: "#Level_of_decision",
                content: " Use aphanumeric",
                regexp: "^[A-Za-z0-9, ]$"
            });

            $("#Level_of_decision").tooltip("open");
            $("#Level_of_decision").css("border", "1px solid red");
        }
    }
});

//script type="text/javascript">

// Load the Google Transliterate API

//</script>



   


   

  

    

    



    

   


   

   









