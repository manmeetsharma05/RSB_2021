
    //Required field validator
    
    function validate(req)
    {
        var mbno = $('#mobileno').val();
        
        var armyno = $('#Army_No').val();
        var esmino = $('#ESMIdentitycardnumber').val();
        var peraddno = $('#Per_address_eng').val();
        var gndr = $("#GenderStatus option:selected").val();
        var cast = $("#CastStatus option:selected").val();
        var mat = $("#MaritalStatus option:selected").val();
        var reg = $("#RegementCorpsList option:selected").val();
        var state = $("#StateStatus option:selected").val();
        var dist = $("#DistrictStatus option:selected").val();
        var tnme = $("#tname option:selected").val();
        var twnme = $("#town-name option:selected").val();
        var name = $('#Sanik_Name_eng').val();
        var fname = $('#Father_Name_eng').val();
        var mname = $('#Mother_Name_eng').val();
        var dob = $('#DOB').val();
        if (name.length == 0 && fname.length == 0 && dob.length == 0 && mbno.length == 0  && armyno.length == 0 && esmino.length == 0
            && peraddno.length == 0 && gndr == 0 && cast == 0 && mat == 0 && reg == 0 && state == 0 && dist == 0 && tnme == 0 && twnme == 0) {
           
            return true;
        }
        else {
            if (name.length == 0) {
                // $('#Sanik_Name_eng').after('<div class="red">Name is Required</div>');
               
                return true;
            }
            else {
                // $('#Sanik_Name_eng').next(".red").remove();
                if (fname.length == 0) {
                    // $('#Father_Name_eng').after('<div class="red">Father Name is Required</div>');

                    
                    return true;
                }
                else {
                    // $('#Father_Name_eng').next(".red").remove();
                    if (mname.length == 0) {
                        // $('#Mother_Name_eng').after('<div class="red">Mother Name is Required</div>');

                        return true;
                    }
                    else {
                        // $('#Mother_Name_eng').next(".red").remove(); // *** this line have been added ***
                        if (dob.length == 0) {
                           
                            return true; // $('#DOB').after('<div class="red">DOB is Required</div>');
                        }
                        else {
                            // $('#DOB').next(".red").remove(); // *** this line have been added ***
                            if (mbno.length == 0) {
                               
                                return true;//$('#mobileno').after('<div class="red">Mobile No. is Required</div>');
                            }
                            else {
                                
                                    if (armyno.length == 0) {
                                       
                                        return true;//$('#Army_No').after('<div class="red">Army  No. is Required</div>');
                                    }
                                    else {
                                        //$('#Army_No').next(".red").remove(); // *** this line have been added ***
                                        if (esmino.length == 0) {
                                           
                                            return true;// $('#ESMIdentitycardnumber').after('<div class="red">ESMI Identity  No. is Required</div>');
                                        }
                                        else {
                                            // $('#ESMIdentitycardnumber').next(".red").remove(); // *** this line have been added ***
                                            if (peraddno.length == 0) {
                                               
                                                return true;//  $('#Per_address_eng').after('<div class="red">Permanent Address is Required</div>');
                                            }
                                            else {
                                                // $('#Per_address_eng').next(".red").remove(); // *** this line have been added ***
                                                if (gndr == 0) {
                                                   
                                                    return true;// $('#GenderStatus').after('<div class="red">Gender is Required</div>');
                                                }
                                                else {
                                                    // $('#GenderStatus').next(".red").remove(); // *** this line have been added ***
                                                    if (cast == '10') {
                                                       
                                                        return true;// $('#CastStatus').after('<div class="red">Cast is Required</div>');
                                                    }
                                                    else {
                                                        //$('#CastStatus').next(".red").remove(); // *** this line have been added ***
                                                        if (mat == 0) {
                                                           
                                                            return true;//  $('#MaritalStatus').after('<div class="red">Marital Status is Required</div>');
                                                        }
                                                        else {
                                                            //$('#MaritalStatus').next(".red").remove(); // *** this line have been added ***
                                                            if (reg == 0) {
                                                               
                                                                return true;//$('#RegementCorpsList').after('<div class="red">Regement/Corps ID is Required</div>');
                                                            }
                                                            else {
                                                                //$('#RegementCorpsList').next(".red").remove(); // *** this line have been added ***
                                                                if (state == 0) {
                                                                   
                                                                    return true;//  $('#StateStatus').after('<div class="red">State is Required</div>');
                                                                }
                                                                else {
                                                                    // $('#StateStatus').next(".red").remove(); // *** this line have been added ***
                                                                    if (dist == 0) {
                                                                       
                                                                        return true;// $('#DistrictStatus').after('<div class="red">District is Required</div>');
                                                                    }
                                                                    else {
                                                                        //$('#DistrictStatus').next(".red").remove(); // *** this line have been added ***
                                                                        if (tnme == 0) {
                                                                            
                                                                            return true;// $('#tname').after('<div class="red">Tehsil Name is Required</div>');
                                                                        }
                                                                        else {
                                                                            // $('#tname').next(".red").remove(); // *** this line have been added ***
                                                                            if (twnme == 0) {
                                                                               
                                                                                return true;// $('#town-name').after('<div class="red">Town/Village Name is Required</div>');
                                                                            }
                                                                            else {
                                                                                // $('#town-name').next(".red").remove(); // *** this line have been added ***
                                                                                if ($('input[name=RadioRuralUrban]:checked').length <= 0) {
                                                                                    
                                                                                    return true;// $('#RadioRuralUrban').after('<div class="red">Selection required Name is Required</div>')
                                                                                }
                                                                                else {
                                                                                    // $('#RadioRuralUrban').next(".red").remove();
                                                                                    
                                                                                    return false;
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                
                            }
                        }
                    }
                }

            }

        }
  
    }

    $(document).ready(function () {

        $("#Cors_address").val('');
        $("#StateStatus").val("06");
        //$("#StateStatus").text("Haryana");
        //alert($("#StateStatus").text());
        if ($("#StateStatus").val() == "06")
        {
            dbind();
        }
        $('input[type = "radio"]').click(function () {
            if (this.value == "uid") {
                var searchkey = this.value;
                $('#hdsearchkeywordtype').val(searchkey);

               
               
                // search();
            }


        });
        $('input[type = "radio"]').click(function () {
            if (this.value == "cid") {
                var searchkey = this.value;
                $('#hdsearchkeywordtype').val(searchkey);
                // search();
            }


        });
        $('input[type = "radio"]').click(function () {
            if (this.value == "mobileno") {
                var searchkey = this.value;
                $('#hdsearchkeywordtype').val(searchkey);
               
                // search();
            }


        });
        $('input[type = "radio"]').click(function () {
            if (this.value == "armyno") {
                var searchkey = this.value;
                $('#hdsearchkeywordtype').val(searchkey);
                // search();
            }


        });
        $('input[type = "radio"]').click(function () {
            if (this.value == "esmino") {
                var searchkey = this.value;
                $('#hdsearchkeywordtype').val(searchkey);
                // search();
            }


        });

        $('.nav-tabs > li a[title]').tooltip();
        $('a[data-toggle="tab"]').on('show.bs.tab', function (e) {

            var $target = $(e.target);

            if ($target.parent().hasClass('disabled')) {
                return false;
            }
        });
        $('#example-dropUp').multiselect({
            enableFiltering: true,
            includeSelectAllOption: true,
            maxHeight: 400,

        });
        $("#stepp2").addClass("disabled");
        $("#stepp3").addClass("disabled");
        $("#stepp4").addClass("disabled");
       
        
        
    });

//for DOB

    $(document).ready(function () {
        var GenderStatus;
        

    //alert('page load');
    //$('#DOB').datepicker();
        $("#GenderStatus").prepend('<option selected="selected" value="0">Select Gender</option>');
        $("#CastStatus").prepend('<option selected="selected" value="10">Select cast</option>');
        $("#MaritalStatus").prepend('<option selected="selected" value="0">Select marital status</option>');
        $("#RegementCorpsList").prepend('<option selected="selected" value="0">Select regement/corps ID</option>');
    //$("#StateStatus").prepend('<option selected="selected" value="06">Harayana</option>');
        $("#RelationshipList").prepend('<option selected="selected" value="0">Select relation</option>');
        $("#MaritalStatus1").prepend('<option selected="selected" value="0">Select marital</option>');
        $("#ForceNameStatus").prepend('<option selected="selected" value="0">Select force name</option>');
        $("#RankStatus").prepend('<option selected="selected" value="0">Select rank</option>');
        $("#MedicalStatus").prepend('<option selected="selected" value="0">Select medical status</option>');
        $("#CharStatus").prepend('<option selected="selected" value="0">Select character</option>');
        $("#BankStatus").prepend('<option selected="selected" value="0">Select bank</option>');
        $("#AwardStatus").prepend('<option selected="selected" value="0">Select award</option>');
        
        var date_input = $('input[name="DOB"]'); //our date input has the name "date"
        var container = $('.bootstrap-iso form').length > 0 ? $('.bootstrap-iso form').parent() : "body";
        var options = {           
        dateFormat: 'dd/mm/yy',
        changeMonth: true,
        changeYear: true,
        container: container,
        todayHighlight: true,
        autoclose: true,
    };
        date_input.datepicker(options);
    })

   
            $(document).ready(function () {
       
                var date_input = $('input[name="Date_loan"]'); //our date input has the name "date"
                var container = $('.bootstrap-iso form').length > 0 ? $('.bootstrap-iso form').parent() : "body";
                var options = {
                    dateFormat: 'dd/mm/yy',
                    changeMonth: true,
                    changeYear: true,
                    container: container,
                    todayHighlight: true,
                    autoclose: true,
                };
                date_input.datepicker(options);
            })
        
            $(document).ready(function () {

                var date_input = $('input[name="award_date"]'); //our date input has the name "date"
                var container = $('.bootstrap-iso form').length > 0 ? $('.bootstrap-iso form').parent() : "body";
                var options = {
                    dateFormat: 'dd/mm/yy',
                    changeMonth: true,
                    changeYear: true,
                    container: container,
                    todayHighlight: true,
                    autoclose: true,
                };
                date_input.datepicker(options);
            })
       
            $(document).ready(function () {
                var date_input = $('input[name="Dependent_DOB"]'); //our date input has the name "date"
                var container = $('.bootstrap-iso form').length > 0 ? $('.bootstrap-iso form').parent() : "body";
                var options = {
                    dateFormat: 'dd/mm/yy',
                    changeMonth: true,
                    changeYear: true,
                    container: container,
                    todayHighlight: true,
                    autoclose: true,
                };
                date_input.datepicker(options);
            })
       
            $(document).ready(function () {
                var date_input = $('input[name="RetirementDate"]'); //our date input has the name "date"
                var container = $('.bootstrap-iso form').length > 0 ? $('.bootstrap-iso form').parent() : "body";
                var options = {
                    dateFormat: 'dd/mm/yy',
                    changeMonth: true,
                    changeYear: true,
                    container: container,
                    todayHighlight: true,
                    autoclose: true,
                };
                date_input.datepicker(options);
            })
        
            $(document).ready(function () {
                var date_input = $('input[name="Date_of_Complain"]'); //our date input has the name "date"
                var container = $('.bootstrap-iso form').length > 0 ? $('.bootstrap-iso form').parent() : "body";
                var options = {
                    dateFormat: 'dd/mm/yy',
                    changeMonth: true,
                    changeYear: true,
                    container: container,
                    todayHighlight: true,
                    autoclose: true,
                };
                date_input.datepicker(options);
            })
       
            $(document).ready(function () {
                var date_input = $('input[name="DOM"]'); //our date input has the name "date"
                var container = $('.bootstrap-iso form').length > 0 ? $('.bootstrap-iso form').parent() : "body";
                var options = {
                    dateFormat: 'dd/mm/yy',
                    changeMonth: true,
                    changeYear: true,
                    container: container,
                    todayHighlight: true,
                    autoclose: true,
                };
                date_input.datepicker(options);
            })
        
