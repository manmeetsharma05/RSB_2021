using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Rajya_Sanik_Board.Models
{
  
    public class Generalform
    {
        [Required]
        [Display(Name = "How much is the sum")]
        public string Captcha { get; set; }
        public List<Generalform> pdf   { get; set; }
        public List<Generalform> pdf1 { get; set; }
        public List<Generalform> pdf2 { get; set; }
        public int No_FamilyMemb { get; set; }
        //newchanges 03-03-2018
        public bool Wife_recorded_in_PPO_no { get; set; }
        public string Wife_othername { get; set; }
        public string Number_Of_children { get; set; }
        public bool Recorded_in_DO_Part2 { get; set; }
        public string Date_of_Enrolment { get; set; }
        public DateTime Date_of_Enrolment1 { get; set; }
        public DateTime Date_of_Retirement { get; set; }
        public bool Disable1 { get; set; }
        public decimal? Disability_Percentage { get; set; }
        public string PPO_number { get; set; }
        public string Pancard_number { get; set; }
        public string Recorded_in_DO_Part2text { get; set; }
        //end
        //martyr
        public string msg { get; set; }
        //for pwd change
        public string oldpswd { get; set; }
        public string newpswd { get; set; }
        public string reenternewpswd { get; set; }
        public string UserChangepwd { get; set; }
        public string hidennewpwd { get; set; }
        public string hidenre { get; set; }
        //end
        public string status { get; set; }
        //public string Wifename { get; set; }
        //public string Othrthnwifenme { get; set; }
        //public string Authority_Letter_no { get; set; }
        //public string noofchildren { get; set; }
        //public string ChildName { get; set; }
        //public string Reson_for_Discharge { get; set; }
        //public string Name_of_Party { get; set; }
        //public string DisabilityPercentage { get; set; }
        //public string Post { get; set; }
        //public string PPO_Number { get; set; }
        //public string No_yr_prsnt_Posting { get; set; }
        //public string PayScale { get; set; }
        //public string Date_Of_joining { get; set; } 
        ////end

        public byte[] imagedept { get; set; }
        public string dobsanik { get; set; }
        public string dobdept { get; set; }
        public string domdept { get; set; }
        public string domaward { get; set; }
        public string dobcmp { get; set; }
        public string domretiremnt { get; set; }
        public string domloan { get; set; }
        public string ImageUrl { get; set; }
        public string pathsnk { get; set; }
        public string pathdep { get; set; }
        public string defaultimage { get; set; }
        public int id { get; set; }
        [Required(ErrorMessage = "Date is required")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]        
        public DateTime Date_of_Published { get; set; }
        public string ESMIdentitycardnumber { get; set; }
        public bool Active { get; set; }
        //for error pages
        public string Error { get; set; }
        public string PageError { get; set; }
        public DateTime DateError { get; set; }
        public string DetailError { get; set; }
        //end

        public string Army_No { get; set; }
        //login
        public string userid { get; set; }
        
        public string pswd { get; set; }
        public string UserType { get; set; }
        //end
        public string Sanik_Name_eng { get; set; }
        public string Sanik_Name_hindi { get; set; }
        public string Father_Name_eng { get; set; }
        public string Father_Name_hindi { get; set; }
        public string Mother_Name_eng { get; set; }
        public string Mother_Name_hindi { get; set; }
        public string CategoryCode { get; set; }
        public string CategoryDesc { get; set; }
        //[DisplayName("DOB")]
        //[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MM-yyyy}")]
        public string DOB { get; set; }
        public DateTime DOBnew { get; set; }
        public string spousename { get; set; }
        public string spousenamehindi { get; set; }
        public string Gender_code { get; set; }
        public string dname { get; set; }
        public string Gender { get; set; }
        public string MaritalStatusCode { get; set; }
        public string MaritalStatusDesc { get; set; }
       
        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }
        [Required(ErrorMessage = "Character Description is required")]
        [RegularExpression(@"^[a-zA-Z0-9 ]+$", ErrorMessage = "Use letter,number only ")]
        public string Character_Description { get; set; }
        public string Citizen_ID { get; set; }
        public string UID { get; set; }
        
        public string Per_cors { get; set; }
        public bool? perchk { get; set; }
        [Required(ErrorMessage = "Regement/corps Name is required")]
        [RegularExpression(@"^[a-zA-Z ]+$", ErrorMessage = "Use letters only ")]
            
        public string regt_CorpsType { get; set; }
        public string Pancard_No { get; set; }
        public string mobileno { get; set; }
        public string landline { get; set; }
       
        public string DOM { get; set; }
        public DateTime? DOM1 { get; set; }
        public string emialid { get; set; }
        public string Per_address_eng { get; set; }
        public string Per_address_hindi { get; set; }
        public string Per_Landmark_english { get; set; }
        public string Dependent_DOB { get; set; }
        public DateTime? Dependent_DOB1 { get; set; }
        public string RelationCode { get; set; } 
        public string dcode { get; set; }
        public string VCODE { get; set; }
        public string VNAME { get; set; }
        public string tcode { get; set; }
        public string tname { get; set; }
        public string towncode { get; set; }
        public string townname { get; set; }
        public string Urban_rural { get; set; }
        public string statecode { get; set; }
        public string StateName { get; set; }
        public string Per_Landmark_Hindi { get; set; }
        public string Pin_code { get; set; }
        public string Cors_address { get; set; }
        public int Dependent_Id { get; set; }
        public int Pensionstatus_ID { get; set; }
        [Required(ErrorMessage = "Pension status is required")]
        [RegularExpression(@"^[a-zA-Z0-9 ]+$", ErrorMessage = "Use alphanumeric or letter only please")]
        public string Pensionstatus_Type { get; set; }
        public string BankID { get; set; }
        public int Bank_Code { get; set; }
         [Required(ErrorMessage = "award Name is required")]
         [RegularExpression(@"^[a-zA-Z0-9 ]+$", ErrorMessage = "Use alphanumeric or letter only please")]
        public string awardName { get; set; }
        public int? Rank_ID { get; set; }
        [Required(ErrorMessage = "Rank type  is required")]
        [RegularExpression(@"^[a-zA-Z0-9 ]+$", ErrorMessage = "Use alphanumeric or letter only please")]
        public string Rank_Type { get; set; }
        public int? medical_ID { get; set; }
        [Required(ErrorMessage = "Medical Type is required")]
        [RegularExpression(@"^[a-zA-Z ]+$", ErrorMessage = "Use letters only please")]
        public string MedicalCat_Type { get; set; }
        public int? Character_Id { get; set; }
        [Required(ErrorMessage = "Character type  is required")]
        [RegularExpression(@"^[a-zA-Z ]+$", ErrorMessage = "Use character letters only please")]
        public string Character_Type { get; set; }
        public int Court_Case_Id { get; set; }
        public string Case_No { get; set; }
        public string Case_Year { get; set; }
        public string Court_Name { get; set; }
        public string Decision { get; set; }
        public int Complain_Id { get; set; }
        public string Name_of_Complain { get; set; }
        public string Level_of_decision { get; set; }
        public string Date_of_Complain { get; set; }
        public DateTime? Date_of_Complain1 { get; set; }
        public string Pending_With { get; set; }
        public string Decision_Given { get; set; }
        public int Force_Dept_Id { get; set; }
        public string Force_Name { get; set; }
        public int? Force_Cat_ID { get; set; }
        //public string Force_Name { get; set; }
        public string Force_Cat_Name { get; set; }
        public int? Retirement_Id { get; set; }
        public string Name_of_Retiree { get; set; }
        public string RetirementDate { get; set; }
        public DateTime? RetirementDate1 { get; set; }
        public bool Advance_Doc_Submit { get; set; }
        public string Remarks { get; set; }
        public int Rent_House_Id { get; set; }
        public string Address { get; set; }
        public decimal? Amount_OF_Rent { get; set; }
        public  decimal? Annual_income { get; set; }
        public decimal? Outstanding_Amount { get; set; }
        public decimal? Annual_Budget_for_Maintenance { get; set; }
        public int Hired_Id { get; set; }
        public string Location { get; set; }
        public DateTime Date_of_Hire { get; set; }
        public string Period_of_lease { get; set; }
        public int Scheme_ID { get; set; }
        public string Property { get; set; }
       
         [Required(ErrorMessage = "Scheme Name is required")]
         [RegularExpression(@"^[a-zA-Z0-9 ]+$", ErrorMessage = "Use alphanumeric or letters only ")]
        public string Scheme_Name { get; set; }

         public byte[] Photo { get; set; }
        public string imgg { get; set; }
        
        public int? Regement_Corps_id { get; set; }
        public int? awardID { get; set; } 
        public int? Sanikawrdid { get; set; }
        public DateTime? award_date1 { get; set; }
        public string award_date { get; set; }
        public string PerposeSanikawrdid { get; set; }
        public string Perpose { get; set; }
        public int? Rest_House_Id { get; set; }
        public string BankName { get; set; }
        public string Bank_Acc_no { get; set; }
        public string Bank_IFSC { get; set; }
        public string Dependent_Name { get; set; }
        public int Loan_id { get; set; }
        public decimal? Loan_Amount { get; set; }
        public string Purpose { get; set; }
        public DateTime? Date_loan1 { get; set; }
        public string Date_loan { get; set; }
       

        public string RelationDesc { get; set; }

        public string ImgByte { get; set; }

        public int? regt_CorpsID { get; set; }
    }
    
}